# 実装計画書 Ver.prt.0.16 — バグ修正・システム拡充パッチ

**ステータス**: ✅ 全タスク完了
**目的**: ゲームプレイで発見された6つのバグ修正・機能改善・システム拡充
**テスト結果**: 5,570テスト全合格（Core: 5,422 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | ショップ購入アイテムがインベントリに反映されない | バグ修正 | ✅ 完了 |
| 2 | 再訪フロアでトラップが再生成される | バグ修正 | ✅ 完了 |
| 3 | レベルアップ時にスキルポイントが付与されない | バグ修正 | ✅ 完了 |
| 4 | スキルツリーを下から上に広がるデザインに変更＋行レベル制限 | UI改善 | ✅ 完了 |
| 5 | 渇き回復手段（水アイテム）の追加 | 機能追加 | ✅ 完了 |
| 6 | スキル・魔法の習得手段（訓練師・図書館司書NPC）の追加 | 機能追加 | ✅ 完了 |

---

## 詳細

### タスク1: ショップ購入アイテムがインベントリに反映されない
- **問題**: ショップで購入するとゴールドは消費されるが、アイテムがインベントリに追加されない
- **根本原因**: `ShopSystem.GenerateXxxShopItems()`で使用されるアイテムID（例: `"healing_potion"`, `"iron_sword"`）が`ItemDefinitions._items`辞書のキー（例: `"potion_healing"`, `"weapon_iron_sword"`）と不一致。`TryBuyItem()`が`ItemDefinitions.Create(result.ItemId)`を呼ぶとnullが返される
- **修正**: 全4ショップタイプ（GeneralShop/WeaponShop/ArmorShop/MagicShop）の約28個のアイテムIDを`ItemDefinitions`のキーに統一
- **変更ファイル**: `WorldMapSystem.cs`（L543-629）

### タスク2: 再訪フロアでトラップが再生成される
- **問題**: 一度訪れた階層に戻ると、解除済みのトラップが再配置される
- **根本原因**: (1) `SaveFloorItemsToCache()`がGroundItemsのみ更新しMap参照を更新していなかった (2) 1階からのダンジョン脱出時にキャッシュ保存が呼ばれていなかった
- **修正**:
  - `SaveFloorItemsToCache()`→`SaveFloorToCache()`にリネームし、Map参照も含めた完全なキャッシュ更新を実装
  - 1階脱出パス（`TryAscendStairs` L2402）にもキャッシュ保存呼び出しを追加
  - 全3箇所のフロア遷移（上昇・下降・脱出）でキャッシュ保存が確実に行われるように
- **変更ファイル**: `GameController.cs`（L580-587, L2353, L2404, L2430）

### タスク3: レベルアップ時にスキルポイントが付与されない
- **問題**: レベルアップしてもスキルポイントが0のまま、スキルツリーでノードを解放できない
- **根本原因**: `SkillTreeSystem.AddPoints()`メソッドは実装済みだが、`Player.OnLevelUp`イベントハンドラから呼び出されていなかった
- **修正**: `SubscribePlayerEvents()`内の`Player.OnLevelUp`ハンドラに`_skillTreeSystem.AddPoints(1)`を追加。レベルアップ毎にスキルポイント1を獲得
- **変更ファイル**: `GameController.cs`（L506）

### タスク4: スキルツリーを下から上に広がるデザインに変更＋行レベル制限
- **問題**: スキルツリーが上から下に展開されており、木が成長するような下→上の直感的なデザインになっていない。また各行のレベル制限が不十分
- **修正**:
  - **描画反転**: `SkillTreeWindow.xaml.cs`の`RenderCurrentTab()`で`maxTreeY`を計算し、全Y座標を`maxTreeY - node.TreeY`で反転描画。Tier1（基礎）が下、Tier3（上級）が上に表示
  - **レベル制限強化**: 全Tier2ノードの`RequiredLevel`を1→5に変更（21箇所）。最終的なレベル制限: Tier1=Lv1、Tier2=Lv5、Tier3=Lv10
  - `DrawConnection()`と`DrawNode()`に`maxTreeY`パラメータを追加
- **変更ファイル**: `SkillTreeWindow.xaml.cs`, `SkillTreeSystem.cs`

### タスク5: 渇き回復手段（水アイテム）の追加
- **問題**: `ThirstLevel`の渇きステータスが存在するが、回復手段となるアイテム（水など）が一切存在しない
- **修正**:
  - **Foodクラス拡張**: `HydrationValue`プロパティを追加（0=回復なし、1=1段階回復、2=完全回復）
  - **FoodType拡張**: `Water`と`CleanWater`を追加
  - **アイテム追加**: `ItemFactory.CreateWater()`（8G、渇き1段階回復）と`CreateCleanWater()`（25G、渇き2段階回復＋HP10回復）
  - **ItemDefinitions登録**: `"food_water"`, `"food_clean_water"`
  - **ショップ追加**: GeneralShopに水（常時）、清水（Lv10以上で追加）
  - **消費ロジック**: `UseItem()`で`HydrationValue > 0`のFoodを使用時に`PlayerThirstLevel`を回復
- **変更ファイル**: `Consumables.cs`, `ItemFactory.cs`, `WorldMapSystem.cs`, `GameController.cs`

### タスク6: スキル・魔法の習得手段（訓練師・図書館司書NPC）の追加
- **問題**: 多くのシステム（スキルツリー、魔法等）が存在するが、ゲームプレイ内での習得・取得手段がない
- **修正**:
  - **TileType拡張**: `NpcTrainer`（訓練師）と`NpcLibrarian`（図書館司書）を追加
  - **NPC対話**: `HandleNpcTile()`に訓練師・図書館司書の対話を追加
  - **アクション**: `DispatchNpcAction()`に`open_skill_tree`、`train_combat`、`learn_magic`を追加
  - **訓練システム**: `TryTrainCombat()` — 50G消費でスキルポイント+1
  - **学習システム**: `TryLearnMagic()` — 100G消費でスキルポイント+2
  - **NPC判定**: `IsNpcTile()`に新タイプを追加
  - **町マップ配置**: `LocationMapGenerator.GenerateTownMap()`に訓練師と図書館司書を配置
- **変更ファイル**: `Tile.cs`, `GameController.cs`, `LocationMapGenerator.cs`

---

## テスト追加

| テストファイル | テスト数 | 内容 |
|--------------|---------|------|
| `VersionPrt016SystemTests.cs` | 13 | ショップID整合性×4、スキルポイント付与、Tier2/3レベル制限×3、水アイテム生成・使用×3、NPCタイル・町マップ配置×2 |

### 既存テスト修正
- `WorldMapSystemTests.ShopSystem_MagicShop_WandHasSize1x2`: MagicShopのアイテムID変更に対応（火の杖→火炎の巻物）
- `EnemyItemExpansionTests.ItemDefinitions_TotalItemCount`: アイテム合計数79→81に更新（水アイテム2個追加）

---

## 変更ファイル一覧

| ファイル | 変更種別 | 概要 |
|---------|---------|------|
| `src/RougelikeGame.Core/Systems/WorldMapSystem.cs` | 修正 | ShopSystemの全アイテムIDをItemDefinitionsに統一、水アイテムをショップに追加 |
| `src/RougelikeGame.Gui/GameController.cs` | 修正 | フロアキャッシュ改善、スキルポイント付与、渇き回復ロジック、訓練師/図書館NPC対応 |
| `src/RougelikeGame.Core/Systems/SkillTreeSystem.cs` | 修正 | Tier2ノードのRequiredLevelを1→5に変更 |
| `src/RougelikeGame.Core/Items/Consumables.cs` | 修正 | FoodにHydrationValueプロパティ追加、FoodTypeにWater/CleanWater追加 |
| `src/RougelikeGame.Core/Items/ItemFactory.cs` | 修正 | CreateWater/CreateCleanWater追加、ItemDefinitionsに登録 |
| `src/RougelikeGame.Core/Map/Tile.cs` | 修正 | NpcTrainer/NpcLibrarianタイルタイプ追加 |
| `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs` | 修正 | 町マップに訓練師/図書館司書NPC配置 |
| `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs` | 修正 | スキルツリー描画を下→上に反転 |
| `tests/RougelikeGame.Core.Tests/VersionPrt016SystemTests.cs` | 新規 | Phase16テスト13件 |
| `tests/RougelikeGame.Core.Tests/WorldMapSystemTests.cs` | 修正 | MagicShopテスト更新 |
| `tests/RougelikeGame.Core.Tests/EnemyItemExpansionTests.cs` | 修正 | アイテム合計数更新 |

---

## ブラッシュアップ記録

Phase 16全タスク完了後、プロジェクトガイドラインに従い自動ドキュメントブラッシュアップを実施。

### 更新対象ドキュメント

| ドキュメント | 更新内容 |
|-------------|---------|
| `docs/00_ドキュメント概要.md` | Ver.prt.0.16エントリ追加、フォルダ構成ツリー更新（0.14/0.16追加）、Ver.prt.0.14リンクパス修正、実装計画書群の説明更新、用語集にNpcTrainer/NpcLibrarian/HydrationValue/WorldMapSystem追加 |
| `docs/計画書/マスター実装計画書.md` | バージョン体系にVer.prt.0.13〜0.16追加、マイルストーン詳細にVer.prt.0.16セクション追加（Ver.αヘッダー欠落修正含む）、フロー図更新、開発フェーズ概要テーブルにVer.prt.0.16行追加 |
| `docs/企画設計書/11_クラス設計書.md` | 実装状況テーブルにPhase 16変更12項目追加（WorldMapSystem/FloorCache/SkillPoint/SkillTreeLevel/SkillTreeWindow/Food/ItemFactory/ThirstRecovery/TileType/Trainer-Librarian/LocationMapGenerator）、ItemFactoryアイテム数52→81更新 |
| `docs/企画設計書/13_GUIシステム設計書.md` | 変更履歴にVer.prt.0.16追加（スキルツリー下→上反転・Tier2 Lv5強化）、SkillTreeWindow説明をTier1-3/Lv1/5/10/ボトムアップ描画に更新 |
| `docs/企画設計書/14_マップシステム設計書.md` | 変更履歴にVer.prt.0.16追加（フロアキャッシュ拡張・NPC配置）、町マップ説明にNPC配置情報追加 |
| `docs/企画設計書/17_デバッグ・テスト設計書.md` | テスト総数5,557→5,570、Core 5,409→5,422に更新、テストピラミッド/プロジェクト一覧/Core.Tests内訳を最新化、VersionPrt016SystemTests.csをファイル一覧に追加 |
| `docs/企画設計書/10_プロジェクト構造設計書.md` | テスト数5,557→5,570、Core 5,409→5,422/134→135ファイルに更新 |

### ファイル移動

| 移動元 | 移動先 |
|-------|-------|
| `docs/実装計画書Ver.prt.0.14.md` | `docs/計画書/Ver.prt/実装計画書Ver.prt.0.14.md` |
| `docs/実装計画書Ver.prt.0.16.md` | `docs/計画書/Ver.prt/実装計画書Ver.prt.0.16.md` |
