# 実装計画書 Ver.prt.0.12（UI刷新・システム改善・ロケーション統合）

**目標**: タイトル画面変更、町システム改善、自動探索強化、バグ修正、ダンジョン品質改善、売買システム修正、PoE風装備パネル、GrimDawn風スキルツリー、ロケーション統合
**状態**: ✅ 全タスク完了 — テスト全体3,349件（GUIオートテスト除外）全合格
**前提**: Ver.prt.0.11（スキルスロット6拡張・未実装魔法エフェクト7種実装）完了済み
**完了時テスト数**: 全体 = 3,349件（Core 3,201 + GUI 148）

---

## 1. 概要

Ver.prt.0.12 はユーザーからの9つの改善要望に基づき、
UI全面刷新・ゲームシステム改善・ロケーション構造の統合を行うフェーズである。

主な改善点:
- タイトル画面のタイトル・サブタイトル変更
- 町マップから階段を撤去し、エントランス方式に統一
- Tab自動探索に閉じたドアを開ける機能を追加
- 死亡後の正気度バグ修正（ステータス表示の反映漏れ）
- ダンジョン部屋の最小サイズを5×5マスに引き上げ
- 売買システムをリスト表示形式に全面書き換え
- インベントリ装備欄をPath of Exile風の身体配置デザインに変更
- スキルツリーにTier制・レベル制限を導入しGrimDawn風に改修
- 各領のロケーションを統合し一つの街として表示

---

## 2. タスク一覧

### Phase 12: UI刷新・システム改善・ロケーション統合（9タスク）

| # | タスク名 | 内容 | 状態 |
|---|---------|------|------|
| T.1 | タイトル画面変更 | タイトルを「タイトル未定」、サブタイトルを「ろーぐらいくげーむ」に変更 | ✅ 完了 |
| T.2 | 町の階段撤去 | 町/村/フィールドからSetStairsUp撤去、EntrancePosition方式に統一 | ✅ 完了 |
| T.3 | Tab自動探索にドア開け追加 | AutoExploreStepにドア自動開放ロジックを追加 | ✅ 完了 |
| T.4 | 正気度バグ修正 | 死亡後のApplyTransferDataで正気度がステータスに反映されない問題を修正 | ✅ 完了 |
| T.5 | ダンジョン部屋最小サイズ変更 | MinRoomSize 5→7（壁含み）で実質5×5マスの歩行可能領域を確保 | ✅ 完了 |
| T.6 | 売買システム修正 | ShopWindow.xaml/xaml.csを全面書き換え、リスト表示形式に変更 | ✅ 完了 |
| T.7 | PoE風装備パネル | InventoryWindowの装備欄をCanvas身体配置デザインに変更（11スロット空間配置） | ✅ 完了 |
| T.8 | GrimDawn風スキルツリー | Tier制（1-4）・レベル制限（Lv1/5/10/15）導入、63ノード全てにTier割当 | ✅ 完了 |
| T.9 | ロケーション統合 | 各領の非ダンジョンロケーションを統合し6つの街エントリに集約 | ✅ 完了 |

---

## 3. タスク詳細

### T.1: タイトル画面変更

**目的**: タイトル画面の表示テキストを変更する

**変更内容**:
1. TitleWindow.xamlのタイトルテキストを「タイトル未定」に変更
2. サブタイトルテキストを「ろーぐらいくげーむ」に変更

**変更ファイル**:
- `src/RougelikeGame.Gui/TitleWindow.xaml`

**受入基準**:
- [x] タイトル画面に「タイトル未定」と表示される
- [x] サブタイトルに「ろーぐらいくげーむ」と表示される

### T.2: 町の階段撤去

**目的**: 町/村/フィールドマップからStairsUpを撤去し、プレイヤーがエントランス位置にスポーンするよう統一する

**変更内容**:
1. LocationMapGenerator: GenerateTownMap/GenerateVillageMap/GenerateFieldMapからSetStairsUp呼び出しを削除
2. SetEntrance呼び出しのみ残す（出入り口の位置情報は保持）
3. GameController: スポーン位置決定ロジックで `EntrancePosition ?? StairsUpPosition` の優先順に変更
4. 施設/神殿マップはPlaceExitDoor経由のStairsUpを維持（Shift+<で退出するため）

**変更ファイル**:
- `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs`
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] 町/村/フィールドマップにStairsUpが配置されない
- [x] プレイヤーがEntrancePositionにスポーンする
- [x] 施設/神殿マップの退出機能は維持

### T.3: Tab自動探索にドア開け追加

**目的**: Tab自動探索中に閉じたドアを自動的に開ける機能を追加する

**変更内容**:
1. GameController.AutoExploreStepに閉じたドア検出ロジックを追加
2. プレイヤー周囲の4方向に閉じたドア（DoorClosed）があれば自動的に開ける

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] 自動探索中に閉じたドアが自動で開く
- [x] 既存の探索ロジックに影響しない

### T.4: 正気度バグ修正

**目的**: 死亡後にログに「正気度90」と表示されるがステータスバー上では100のままになるバグを修正する

**変更内容**:
1. Player.ApplyTransferDataメソッドで正気度をステータスに正しく反映するよう修正

**変更ファイル**:
- `src/RougelikeGame.Core/Entities/Player.cs`

**受入基準**:
- [x] 死亡後の正気度がステータスバーに正しく反映される
- [x] ログ表示とステータス表示が一致する

### T.5: ダンジョン部屋最小サイズ変更

**目的**: ダンジョンに極端に小さい部屋が生成されないようにする（最低5×5マスの歩行可能領域）

**変更内容**:
1. DungeonGenerator.MinRoomSizeを5→7に変更（壁2マス含むため実質5×5）

**変更ファイル**:
- `src/RougelikeGame.Core/Map/Generation/DungeonGenerator.cs`

**受入基準**:
- [x] 生成される部屋の歩行可能領域が最低5×5マス
- [x] ダンジョン生成の全テストが合格

### T.6: 売買システム修正

**目的**: 売買システムの動作不良を修正し、リスト表示形式に全面刷新する

**変更内容**:
1. ShopWindow.xamlを全面書き換え（リスト表示形式、購入・売却タブ）
2. ShopWindow.xaml.csを全面書き換え（アイテムリスト表示、選択購入/売却）

**変更ファイル**:
- `src/RougelikeGame.Gui/ShopWindow.xaml`
- `src/RougelikeGame.Gui/ShopWindow.xaml.cs`

**受入基準**:
- [x] アイテムがリスト形式で表示される
- [x] 購入・売却が正しく動作する

### T.7: PoE風装備パネル

**目的**: インベントリウィンドウの装備欄をPath of Exile風の身体シルエット配置デザインに変更する

**変更内容**:
1. InventoryWindow.xaml: `StackPanel x:Name="EquipmentPanel"` → `Canvas x:Name="EquipmentCanvas"` (244×340)
2. InventoryWindow.xaml.cs: `RenderEquipmentPanel()` をCanvas空間配置方式に全面書き換え
3. 11スロット空間配置:
   - Head (95,2) 54×54 — 頭部（上部中央）
   - Neck (28,12) 54×54 — 首（左上）
   - Back (162,12) 54×54 — 背中（右上）
   - MainHand (6,72) 54×72 — メインハンド（左側）
   - Body (72,62) 100×100 — 胴体（中央大）
   - OffHand (184,72) 54×72 — オフハンド（右側）
   - Hands (14,152) 54×54 — 手（左下）
   - Ring1 (176,152) 54×54 — リング1（右下）
   - Waist (95,168) 54×54 — 腰（下部中央）
   - Ring2 (28,230) 54×54 — リング2（左下方）
   - Feet (95,232) 54×72 — 足（最下部中央）
4. `DrawBodySilhouette()` ヘルパー: ガイドライン描画
5. `EquipSlot_Click()` ハンドラー: スロットクリックでアイテム情報表示

**変更ファイル**:
- `src/RougelikeGame.Gui/InventoryWindow.xaml`
- `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`

**受入基準**:
- [x] 装備欄が身体シルエット配置で表示される
- [x] 各装備スロットがクリック可能
- [x] レアリティに応じた色分け表示

### T.8: GrimDawn風スキルツリー

**目的**: スキルツリーにTier制・レベル制限を導入しGrimDawnのような段階的解放システムにする

**変更内容**:
1. `SkillNodeDefinition` レコードに `Tier` (int, default 1) と `RequiredLevel` (int, default 1) を追加
2. `CanUnlock(nodeId, playerLevel)` にレベル制限チェックを追加
3. `UnlockNode(nodeId, playerLevel)` にplayerLevelパラメータを追加
4. 全63ノードにTier割当:
   - Tier 1 (Lv1): 共有ステータス小ノード9 + 種族初期ノード10 = 19ノード
   - Tier 2 (Lv5): 共有ステータス大ノード4 + クラス初期ノード10 + 種族2ndノード10 = 24ノード
   - Tier 3 (Lv10): クラス2nd/3rdノード20
   - Tier 4 (Lv15): キーストーンノード4
5. SkillTreeWindow.xaml: TierLabel, LevelTextバインディング追加
6. SkillTreeWindow.xaml.cs: Tier順ソート、🔒アイコンでレベルロック表示

**変更ファイル**:
- `src/RougelikeGame.Core/Systems/SkillTreeSystem.cs`
- `src/RougelikeGame.Gui/SkillTreeWindow.xaml`
- `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs`

**受入基準**:
- [x] Tier 1-4がレベル1/5/10/15で段階的に解放される
- [x] レベル不足のノードにロックアイコンが表示される
- [x] ノードリストがTier順にソートされる
- [x] 既存のスキルツリーテストが全合格（CanUnlockのデフォルトパラメータにより後方互換）

### T.9: ロケーション統合

**目的**: 各領の非ダンジョンロケーションを統合し、一つの街として表示する

**変更内容**:
1. `LocationDefinition` レコードに `SubLocationIds` (string[]?, default null) を追加
2. 6つの統合街エントリを新規作成:
   - `capital_town` (王都) — SubLocationIds: castle, guild, academy, cathedral, market, arena, slum
   - `forest_town` (森の都) — SubLocationIds: city, herbalist, worldtree, deep, spring
   - `mountain_town` (鉄床城) — SubLocationIds: fortress, miner, shrine, road
   - `coast_town` (海港都市) — SubLocationIds: port, vineyard, field, darkchapel
   - `southern_town` (南方城) — SubLocationIds: castle, village, hunter, plain
   - `frontier_town` (辺境砦) — SubLocationIds: fort, outlaw, ruins_city, wasteland, chaos_altar
3. `GetSymbolLocations(territory)` 静的メソッド追加: SubLocationIds!=null || Type==Dungeon のみ返す
4. `GetSubLocations()` インスタンスメソッド追加: SubLocationIdsから子ロケーションを返す
5. SymbolMapGenerator.Generate: `GetByTerritory` → `GetSymbolLocations` に変更

**変更ファイル**:
- `src/RougelikeGame.Core/Systems/WorldMapSystem.cs`
- `src/RougelikeGame.Core/Map/Generation/SymbolMapGenerator.cs`

**受入基準**:
- [x] シンボルマップに統合された街エントリのみ表示される
- [x] 各統合エントリのサブロケーションが正しく取得可能
- [x] ダンジョンは従来通り個別にシンボルマップに表示

---

## 4. テスト修正

Phase 12の実装に伴い、以下のテストを修正した:

| テストファイル | 修正内容 | 修正理由 |
|---------------|---------|---------|
| SymbolMapTransitionTests.cs | GetByTerritory→GetSymbolLocations（2テスト） | ロケーション統合によりシンボルマップの配置ロジック変更 |
| LocationMapGeneratorTests.cs | StairsUpPosition→EntrancePosition（Town/Village/Field 3テスト） | 町/村/フィールドマップからStairsUp撤去 |
| LocationMapGeneratorTests.cs | GenerateForLocation_HasStairsUp: 条件分岐追加 | Facility/ShrineはStairsUp、他はEntrancePosition |
| LocationMapGeneratorTests.cs | GenerateStartLocationMap: StairsUp OR Entrance確認に変更 | 全スタート地点でいずれかの入口が存在することを検証 |

---

## 5. 実装状況サマリ

| 項目 | 値 |
|------|-----|
| 総タスク数 | 9 |
| 完了タスク | 9 |
| テスト総数 | 3,349（Core 3,201 + GUI 148） |
| ビルドエラー | 0 |
| 変更ファイル数 | 15 |
| 主要変更ファイル | GameController.cs, LocationMapGenerator.cs, WorldMapSystem.cs, SkillTreeSystem.cs, InventoryWindow.xaml/cs, SkillTreeWindow.xaml/cs, ShopWindow.xaml/cs, TitleWindow.xaml, Player.cs, DungeonGenerator.cs, SymbolMapGenerator.cs |
| テスト修正数 | 7ファイル修正（SymbolMapTransitionTests, LocationMapGeneratorTests） |

---

## 6. ブラッシュアップ記録

| 日付 | 対象ドキュメント | 更新内容 |
|------|-----------------|---------|
| Phase 12完了時 | 実装計画書Ver.prt.0.12 | 新規作成、全9タスクの詳細記載 |
| Phase 12完了時 | マスター実装計画書 | Ver.prt.0.12エントリ追加 |
| Phase 12完了時 | ドキュメント概要 | Ver.prt.0.12エントリ追加（一覧テーブル＋フォルダ構成ツリー） |
| Phase 12完了時 | プロジェクト構造設計書 | GUIテスト数147→148件更新 |
| Phase 12完了時 | GUIシステム設計書 | 実装状況ヘッダーにVer.prt.0.12追記、InventoryWindow（PoE風Canvas身体配置11スロット）、ShopWindow（リスト表示形式）、SkillTreeWindow（GrimDawn風Tier制・レベル制限）の説明更新 |
| Phase 12完了時 | マップシステム設計書 | 実装状況ヘッダーにVer.prt.0.12追記（階段撤去・部屋サイズ・ドア自動開放・ロケーション統合）、部屋最小サイズ5×5→7×7更新 |
| Phase 12完了時 | 領地設計書 | ロケーション統合情報追記（6街エントリ・SubLocationIds・GetSymbolLocations）、プレイマップ生成ルールに階段撤去・Entrance方式統一の注記追加 |
| Phase 12完了時 | 戦闘システム設計書 | スキルツリーTier制詳細テーブル追加（Tier 1-4、Lv1/5/10/15、63ノード分類） |
| Phase 12完了時 | 死に戻り・正気度システム設計書 | ApplyTransferDataの正気度反映バグ修正（Ver.prt.0.12）を引き継ぎシステム行に追記 |
| Phase 12完了時 | デバッグ・テスト設計書 | GUIテスト数147→148件更新 |
