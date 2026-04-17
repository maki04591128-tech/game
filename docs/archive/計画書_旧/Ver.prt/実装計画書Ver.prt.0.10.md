# 実装計画書 Ver.prt.0.10（ダンジョン品質改善・ショップバグ修正・スキルツリー拡張・インベントリUI強化）

**目標**: ダンジョン生成品質の向上、ショップ売買バグ修正、ルーン学習UI整理、スキルツリーの種族/職業別拡張、インベントリ装備パネル追加、スキルスロットD&D実装
**状態**: ✅ 全タスク完了 — テスト全体2,706件（GUIオートテスト除外）全合格
**前提**: Ver.prt.0.9（インベントリ改善・ショップD&D・図鑑連動・ギルド機能拡張）完了済み
**完了時テスト数**: 全体 = 2,706件（Core 2,559 + GUI 147）

---

## 1. 概要

Ver.prt.0.10 は Ver.prt.0.9 完了後のプレイテストに基づき、
14個の改善・バグ修正・新機能を実装するフェーズである。

主な改善点:
- ダンジョン生成で最低5部屋+階段を保証するリトライロジック追加
- 壁際の無意味なドア配置を防止するバリデーション追加
- ダンジョンアイテム再生成の修正（FloorCacheにGroundItems保存）
- ダンジョンボス配置と討伐後の脱出口出現機能
- 建物マップに出口ドア配置（施設・神殿）
- 町・村・フィールドにスポーンポイント（入口+脱出階段）追加
- ショップ購入時にアイテムがインベントリに追加されないバグ修正
- ルーン学習ボタン撤去と初期表示の空状態化
- スキルツリーを種族/職業別に63ノードへ大幅拡張
- インベントリウィンドウに装備パネル+ステータス表示追加
- ソート状態の装備変更後の維持
- スキルツリーに「習得済み/スロット」タブとD&Dスキルスロット追加
- 図鑑Ver.α拡充計画ドキュメント更新

---

## 2. タスク一覧

### Phase 10: ダンジョン品質改善・ショップバグ修正・スキルツリー拡張・インベントリUI強化（14タスク）

| # | タスク名 | 内容 | 状態 |
|---|---------|------|------|
| T.1 | ダンジョン最低部屋数保証 | BSP生成にMinRooms=5リトライロジック追加、階段配置保証 | ✅ 完了 |
| T.2 | 無意味ドア配置防止 | HasWalkableBeyondWall()で壁方向先の通行可能判定を追加 | ✅ 完了 |
| T.3 | ダンジョンアイテム再生成修正 | FloorCacheにGroundItemsフィールド追加、24時間キャッシュ制御 | ✅ 完了 |
| T.4 | ダンジョンボス配置+脱出口 | EnemyDefinitions.GetDungeonBoss()追加、ボス討伐後StairsUp出現 | ✅ 完了 |
| T.5 | 建物出口ドア配置 | 施設・神殿マップにPlaceExitDoor()で出口ドア+StairsUp追加 | ✅ 完了 |
| T.6 | 町・村・フィールドスポーンポイント | 3種マップにSetEntrance()+SetStairsUp()でスポーン/脱出地点設定 | ✅ 完了 |
| T.7 | ショップ購入バグ修正 | TryBuyItem()でItemDefinitions.Create()+inventory.Add()によるアイテム生成・追加 | ✅ 完了 |
| T.8 | ルーン学習UI整理 | LearnRuneButton撤去、Lキーバインド削除、ルーン0件時の初期表示を空に変更 | ✅ 完了 |
| T.9 | スキルツリー種族/職業拡張 | RequiredRaceフィールド追加、14→63ノード（共有9+キーストーン4+職業30+種族20） | ✅ 完了 |
| T.10 | インベントリ装備パネル+ステータス | 横幅850px化、右カラムに装備11スロット表示+全ステータス表示 | ✅ 完了 |
| T.11 | ソート状態維持修正 | _isSortedフラグ追加、装備変更後もソート順序を維持 | ✅ 完了 |
| T.12 | スキルスロットD&D | TabControlに「習得済み/スロット」タブ追加、D&Dで6スロットにスキル配置 | ✅ 完了 |
| T.13 | 図鑑Ver.α拡充計画 | 拡張システム設計書に§54.6追加、実装計画書Ver.αにα.26b〜α.26e追加 | ✅ 完了 |
| T.14 | ビルド・テスト・ドキュメント | 全テスト合格確認、実装計画書Ver.prt.0.10作成 | ✅ 完了 |

---

## 3. タスク詳細

### T.1: ダンジョン最低部屋数保証

**目的**: ダンジョン生成でBSP分割の確率により部屋数が極端に少なくなる（0〜2部屋）場合や、階段が配置されない場合を防止する

**根本原因**: BSP分割は確率ベースで分割をスキップするため、乱数次第では部屋が非常に少なくなり、ゲームプレイに支障が出ることがあった

**変更内容**:
1. `DungeonGenerator.Generate()` に `MinRooms=5` 定数と `MaxRetries=5` のリトライループを追加
2. BSP生成後に `rooms.Count < MinRooms` なら再生成（リトライ時は `forceMinRooms=true`）
3. `SplitBSP()` に `forceMinRooms` パラメータ追加: 浅い階層（depth < 3）で確実に分割
4. 階段（StairsDown/StairsUp）が配置されなかった場合もリトライ対象

**変更ファイル**:
- `src/RougelikeGame.Core/Map/Generation/DungeonGenerator.cs`

**受入基準**:
- [x] 生成されるダンジョンに最低5部屋が含まれる
- [x] 階段が必ず配置される
- [x] 既存テスト全合格

### T.2: 無意味ドア配置防止

**目的**: 壁に隣接しているだけで通路として機能しないドアが配置される問題を修正する

**変更内容**:
1. `RoomCorridorGenerator.IsDoorCandidate()` に `HasWalkableBeyondWall()` 呼び出しを追加
2. ドアの通路方向（上下左右）の先に実際に通行可能タイルが存在するか検証
3. 壁方向のさらに奥に通路/部屋がなければドア候補から除外

**変更ファイル**:
- `src/RougelikeGame.Core/Map/Generation/RoomCorridorGenerator.cs`

**受入基準**:
- [x] 壁際に無意味なドアが配置されない
- [x] 部屋と廊下の境界にある有効なドアは引き続き配置される

### T.3: ダンジョンアイテム再生成修正

**目的**: 同じフロアに戻った際にアイテムが毎回再生成される問題を修正する

**変更内容**:
1. `FloorCache` レコードに第3パラメータ `List<(Item Item, Position Position)> GroundItems` を追加
2. フロア離脱時に地面アイテムをFloorCacheに保存
3. フロア帰還時にキャッシュからアイテムを復元（24時間経過でキャッシュ無効化）

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`（FloorCache定義、保存・復元ロジック）
- `tests/RougelikeGame.Gui.Tests/GameControllerTests.cs`（FloorCacheテスト修正）

**受入基準**:
- [x] 同フロアに戻るとアイテム配置が保持される
- [x] 24時間経過後はキャッシュが無効になりアイテムが再生成される

### T.4: ダンジョンボス配置+脱出口

**目的**: ダンジョンの特定フロアにボス敵を配置し、討伐後に脱出口が出現する仕組みを追加する

**変更内容**:
1. `EnemyDefinitions.GetDungeonBoss()` メソッド追加: マップ名に応じたボス敵を返却
2. `GameController.GenerateFloor()` でボスフロアにボス敵を配置
3. `OnEnemyDefeated()` でボス撃破時にその場にStairsUpを生成
4. メッセージ「⚡ ボスを倒した！ 脱出口が出現した！」を表示

**変更ファイル**:
- `src/RougelikeGame.Core/Factories/EnemyFactory.cs`（GetDungeonBoss）
- `src/RougelikeGame.Gui/GameController.cs`（ボス配置・討伐処理）

**受入基準**:
- [x] ボスフロアにボス敵が配置される
- [x] ボス討伐後にStairsUpが出現する
- [x] 脱出口からダンジョン外へ移動できる

### T.5: 建物出口ドア配置

**目的**: 施設・神殿マップに出口ドアを配置し、建物から外へ出られるようにする

**変更内容**:
1. `LocationMapGenerator.PlaceExitDoor()` プライベートメソッド新規作成
2. 指定座標にDoorClosedタイルを配置し、ドアの先にStairsUp+Entranceを設定
3. `GenerateFacilityMap()` と `GenerateShrineMap()` からPlaceExitDoorを呼び出し

**変更ファイル**:
- `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs`

**受入基準**:
- [x] 施設マップに出口ドアが配置される
- [x] 神殿マップに出口ドアが配置される
- [x] ドアの先にStairsUpが存在し脱出可能

### T.6: 町・村・フィールドスポーンポイント

**目的**: 町・村・フィールドマップにプレイヤーのスポーン地点と脱出用階段を配置する

**変更内容**:
1. `GenerateTownMap()` に `SetEntrance()` + `SetStairsUp()` 呼び出しを追加（入口座標）
2. `GenerateVillageMap()` に同様の処理を追加
3. `GenerateFieldMap()` に同様の処理を追加
4. テストを `NoStairsUp` → `HasStairsUp`（Assert.NotNull）に修正

**変更ファイル**:
- `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs`
- `tests/RougelikeGame.Core.Tests/LocationMapGeneratorTests.cs`

**受入基準**:
- [x] 町マップにStairsUpが配置される
- [x] 村マップにStairsUpが配置される
- [x] フィールドマップにStairsUpが配置される
- [x] テスト全合格

### T.7: ショップ購入バグ修正

**目的**: ショップでアイテム購入時にゴールドは減るがアイテムがインベントリに追加されないバグを修正する

**根本原因**: `TryBuyItem()` が `_shopSystem.Buy()` を呼んで `ItemId` を取得していたが、実際にアイテムを生成してインベントリに追加する処理が欠落していた

**変更内容**:
1. `TryBuyItem()` で `result.Success && result.ItemId is not null` の場合に `ItemDefinitions.Create(result.ItemId)` でアイテムを生成
2. 生成されたアイテムを `inventory.Add(newItem)` でプレイヤーインベントリに追加
3. インベントリが一杯の場合はメッセージを表示

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] ショップ購入後にアイテムがインベントリに追加される
- [x] インベントリ満杯時は適切なメッセージが表示される
- [x] ゴールド消費は従来通り正常に動作する

### T.8: ルーン学習UI整理

**目的**: SpellCastingWindowのルーン学習ボタンと関連機能を撤去し、UIをシンプルにする

**変更内容**:
1. `SpellCastingWindow.xaml` から `LearnRuneButton` を削除（旧lines 140-142）
2. `SpellCastingWindow.xaml.cs` から `LearnRuneButton_Click` / `LearnRuneWord` メソッドを削除
3. `Key.L` キーハンドラを削除
4. ルーン0件時の「全ルーン表示」フォールバックを削除（初期表示は空欄に）

**変更ファイル**:
- `src/RougelikeGame.Gui/SpellCastingWindow.xaml`
- `src/RougelikeGame.Gui/SpellCastingWindow.xaml.cs`

**受入基準**:
- [x] ルーン学習ボタンが表示されない
- [x] Lキー押下で何も起きない
- [x] ルーン未習得時は空の一覧が表示される

### T.9: スキルツリー種族/職業拡張

**目的**: スキルツリーを全種族・全職業に対応した充実したノード構成に拡張する

**変更内容**:
1. `SkillNodeDefinition` レコードに `Race? RequiredRace = null` パラメータを追加
2. `GetNodesForClass()` でRaceパラメータによるフィルタリングを追加
3. ノード数を14→63に拡張:
   - 共有ノード: 9（全職業/種族共通）
   - キーストーンノード: 4（上位汎用ノード）
   - 職業ノード: 30（10職業 × 3ノード）
   - 種族ノード: 20（10種族 × 2ノード）
4. SkillTreeWindow.xaml.cs で種族情報表示とソート対応

**変更ファイル**:
- `src/RougelikeGame.Core/Systems/SkillTreeSystem.cs`
- `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs`

**受入基準**:
- [x] 全10種族のスキルノードが利用可能
- [x] 全10職業のスキルノードが利用可能
- [x] RequiredRaceフィルタリングが正しく動作する
- [x] 既存テスト全合格

### T.10: インベントリ装備パネル+ステータス表示

**目的**: インベントリウィンドウに装備中アイテムの一覧と詳細ステータスを表示する右カラムを追加する

**変更内容**:
1. `InventoryWindow.xaml` のWidth を550→850に拡張
2. 右カラムにStackPanel `EquipmentPanel` を追加（11装備スロット表示）
3. 右カラムにTextBlock `StatsText` を追加（HP/MP・9基本ステータス・物理/魔法攻防・装備重量）
4. `RenderEquipmentPanel()` メソッド: 11スロット（MainHand/OffHand/Head/Body/Hands/Feet/Neck/Ring1/Ring2/Back/Waist）の装備状態を表示
5. `RenderStats()` メソッド: EffectiveStatsの全値を表示
6. コンストラクタと `RefreshItems()` から両メソッドを呼び出し

**変更ファイル**:
- `src/RougelikeGame.Gui/InventoryWindow.xaml`
- `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`

**受入基準**:
- [x] 11スロットの装備状態が右カラムに表示される
- [x] HP/MP・全ステータスが表示される
- [x] 装備変更後にパネルとステータスが更新される

### T.11: ソート状態維持修正

**目的**: ソート実行後に装備変更を行うとソート順序がリセットされるバグを修正する

**変更内容**:
1. `InventoryWindow.xaml.cs` に `_isSorted` フラグを追加
2. `SortItems()` で `_isSorted = true` に設定
3. `RefreshItems()` で `_isSorted == true` の場合にソートを再適用

**変更ファイル**:
- `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`

**受入基準**:
- [x] ソート後に装備変更してもソート順序が維持される
- [x] ソート未実行時は従来通りの表示順

### T.12: スキルスロットD&D

**目的**: スキルツリーウィンドウに「習得済み/スロット」タブを追加し、習得したスキルをD&Dで6スロットに配置する機能を追加する

**変更内容**:
1. `SkillTreeSystem` に `EquipSkillToSlot()` / `UnequipSkillFromSlot()` / `UnequipSkillSlot()` メソッド追加
2. `MaxSkillSlots = 6` 定数、`_equippedSkillSlots` リスト、`EquippedSkillSlots` プロパティ追加
3. `Reset()` でスキルスロットもクリア
4. `SkillTreeWindow.xaml` に TabControl を追加:
   - 「ツリー」タブ: 既存のスキルツリー表示
   - 「習得済み / スロット」タブ: 習得済みスキルリスト + 6スロットパネル
5. WPF DragDrop でスキルリストからスロットへのD&Dを実装
6. スロットの「×」ボタンでスキル解除
7. `UnlockedSkillViewModel` クラスを追加

**変更ファイル**:
- `src/RougelikeGame.Core/Systems/SkillTreeSystem.cs`
- `src/RougelikeGame.Gui/SkillTreeWindow.xaml`
- `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs`

**受入基準**:
- [x] 「習得済み / スロット」タブが表示される
- [x] 習得済みスキルをD&Dでスロットに配置できる
- [x] 最大6スロットまで配置可能
- [x] 「×」ボタンでスロットからスキルを解除できる
- [x] 既存テスト全合格

### T.13: 図鑑Ver.α拡充計画

**目的**: Ver.αでの図鑑拡充に関するドキュメントを更新する

**変更内容**:
1. `拡張システム設計書.md` に §54.6「Ver.α 図鑑拡充計画」セクションを追加（7項目）
2. `実装計画書Ver.α.md` にタスク α.26b〜α.26e を追加

**変更ファイル**:
- `docs/企画設計書/21_拡張システム設計書.md`
- `docs/計画書/Ver.α/実装計画書Ver.α.md`

**受入基準**:
- [x] 図鑑拡充計画が設計書に記載されている
- [x] 実装計画書に対応タスクが追加されている

### T.14: ビルド・テスト・ドキュメント

**目的**: 全変更のビルド確認、テスト全合格、実装計画書作成

**変更内容**:
1. ビルドエラー修正: `ItemFactory.Create` → `ItemDefinitions.Create`
2. テスト修正: FloorCacheコンストラクタに第3引数GroundItems追加
3. テスト修正: LocationMapGeneratorTests の `NoStairsUp` → `HasStairsUp`（7テスト）
4. 実装計画書Ver.prt.0.10.md 作成

**受入基準**:
- [x] ビルド: 0エラー、0警告（xUnit分析警告を除く）
- [x] テスト: 2,706件全合格
- [x] 実装計画書が作成されている

---

## 4. 実装状況サマリー

| 項目 | 値 |
|------|-----|
| 総タスク数 | 14 |
| 完了タスク数 | 14 |
| テスト総数 | 2,706件（Core 2,559 + GUI 147） |
| ビルド状態 | ✅ 0エラー |
| テスト結果 | ✅ 全合格 |

---

## 5. 変更ファイル一覧

| ファイル | 変更種別 |
|---------|---------|
| `src/RougelikeGame.Core/Map/Generation/DungeonGenerator.cs` | 修正（リトライロジック追加） |
| `src/RougelikeGame.Core/Map/Generation/RoomCorridorGenerator.cs` | 修正（HasWalkableBeyondWall追加） |
| `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs` | 修正（PlaceExitDoor、スポーンポイント追加） |
| `src/RougelikeGame.Core/Map/DungeonMap.cs` | 修正（ClearRooms追加） |
| `src/RougelikeGame.Core/Factories/EnemyFactory.cs` | 修正（GetDungeonBoss追加） |
| `src/RougelikeGame.Core/Systems/SkillTreeSystem.cs` | 修正（RequiredRace、63ノード、スキルスロット） |
| `src/RougelikeGame.Gui/GameController.cs` | 修正（TryBuyItem、FloorCache、ボス配置・討伐） |
| `src/RougelikeGame.Gui/SpellCastingWindow.xaml` | 修正（LearnRuneButton削除） |
| `src/RougelikeGame.Gui/SpellCastingWindow.xaml.cs` | 修正（ルーン学習関連メソッド削除） |
| `src/RougelikeGame.Gui/InventoryWindow.xaml` | 修正（850px幅、装備パネル+ステータス追加） |
| `src/RougelikeGame.Gui/InventoryWindow.xaml.cs` | 修正（RenderEquipmentPanel、RenderStats、_isSorted） |
| `src/RougelikeGame.Gui/SkillTreeWindow.xaml` | 修正（TabControl追加） |
| `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs` | 修正（D&Dスキルスロット、UnlockedSkillViewModel） |
| `docs/企画設計書/21_拡張システム設計書.md` | 修正（§54.6追加） |
| `docs/計画書/Ver.α/実装計画書Ver.α.md` | 修正（α.26b〜α.26e追加） |
| `tests/RougelikeGame.Core.Tests/LocationMapGeneratorTests.cs` | 修正（NoStairsUp→HasStairsUp） |
| `tests/RougelikeGame.Gui.Tests/GameControllerTests.cs` | 修正（FloorCache 3パラメータ対応） |

---

## 6. ドキュメントブラッシュアップ記録

Phase 10完了に伴い、以下のドキュメントを最新化した。

| ドキュメント | 更新内容 |
|-------------|---------|
| マスター実装計画書 | Ver.prt.0.10エントリ追加（ロードマップ表、マイルストーン詳細、フェーズフロー図、フェーズ表） |
| ドキュメント概要 | Ver.prt.0.10エントリ追加（一覧表、フォルダ構成、実装計画書群参照） |
| プロジェクト構造設計書 | テスト件数を2,706件（Core 2,559 + GUI 147）に更新 |
| GUIシステム設計書 | InventoryWindow（装備パネル+ステータス、850px）、SpellCastingWindow（ルーン学習ボタン撤去）、SkillTreeWindow（習得済み/スロットタブ+D&D）の説明を更新 |
| マップシステム設計書 | ダンジョン生成リトライロジック、HasWalkableBeyondWallドアバリデーション、ロケーションマップStairsUp仕様を追記 |
| デバッグ・テスト設計書 | テスト件数を2,706件に更新、GUI.Tests 147件に修正 |
