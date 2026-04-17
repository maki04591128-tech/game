# 実装計画書 Ver.prt.0.19 — バグ修正・機能追加バッチ（14件）

**ステータス**: ✅ 全タスク完了
**目的**: ユーザー報告の14件のバグ修正・機能要望に対応
**テスト結果**: 5,632テスト全合格（Core: 5,484 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | アイテム拾得時にインベントリ未反映 | バグ修正 | ✅ 完了 |
| 2 | 渇き・疲労・清潔度の数値表示 | 機能改善 | ✅ 完了 |
| 3 | 習得済みスキルにパッシブスキル欄追加 | 機能追加 | ✅ 完了 |
| 4 | 町/フィールドで階層(B1F)を非表示 | バグ修正 | ✅ 完了 |
| 5 | インベントリ装備欄から装備を外せない | バグ修正 | ✅ 完了 |
| 6 | インベントリソート状態がリセットされる | バグ修正 | ✅ 完了 |
| 7 | 街中で動作が重くなる | パフォーマンス | ✅ 完了 |
| 8 | 武器・防具を装備しても装備欄に反映されない | バグ修正 | ✅ 完了 |
| 9 | 探索済みマス増加で動作が重くなる | パフォーマンス | ✅ 完了 |
| 10 | 自動探索で下り階段→上り階段へ移動追加 | 機能追加 | ✅ 完了 |
| 11 | キーバインド変更機能 | 機能追加 | ✅ 完了 |
| 12 | ESCポーズ画面 | 機能追加 | ✅ 完了 |
| 13 | スキルツリーノード配置改善 | UI改善 | ✅ 完了 |
| 14 | 町建物ドアの別マップ遷移 | 機能追加 | ✅ 完了 |

---

## 詳細

### タスク1: アイテム拾得時にインベントリ未反映

- **問題**: 落ちているアイテムを拾った時にアイテムがインベントリに反映されないことが多々ある
- **根本原因**: `GameController.TryPickupItem()` で `Inventory.Add()` の戻り値（bool）を確認せずに処理していた。グリッド配置判定 `CanFitInGrid()` が配置済みアイテムの座標と重複判定していたため、インベントリが空でないと配置失敗するケースがあった
- **修正内容**:
  - `TryPickupItem()` で `Inventory.Add()` の戻り値を確認し、失敗時はメッセージ表示して処理中断
  - `CanFitInGrid()` の重複判定ロジックを修正
- **変更ファイル**: `GameController.cs`

### タスク2: 渇き・疲労・清潔度の数値表示

- **問題**: 渇き状態と疲労度、清潔度も満腹度と同じく数値で表示してほしい
- **修正内容**: `MainWindow.xaml.cs` の `UpdateDisplay()` でステータスバーに渇き・疲労・清潔度の数値（パーセント）を表示するよう変更
- **変更ファイル**: `MainWindow.xaml.cs`

### タスク3: 習得済みスキルにパッシブスキル欄追加

- **問題**: スキルツリーの習得済みスキル一覧にパッシブスキル（StatMinor/StatMajor）が含まれていない
- **修正内容**: `SkillTreeWindow.xaml.cs` の習得済みスキル表示にパッシブスキルのセクションを追加。アクティブスキルとパッシブスキルを分離表示
- **変更ファイル**: `SkillTreeWindow.xaml.cs`

### タスク4: 町/フィールドで階層非表示

- **問題**: 町やフィールドに入った時に階層がB1Fと表示されてしまう
- **修正内容**: `MainWindow.xaml.cs` の `UpdateDisplay()` で `IsInLocationMap` が true の場合、フロア表示を非表示にするよう変更
- **変更ファイル**: `MainWindow.xaml.cs`

### タスク5: 装備スロットからの装備解除

- **問題**: インベントリの装備欄にあるアイテムが選択できず装備を外すことができない
- **修正内容**:
  - `InventoryWindow.xaml.cs` にコールバック `onUnequipItem` を追加
  - 装備スロット右クリックで装備解除ダイアログを表示
  - `GameController.cs` に `UnequipItem()` メソッドを追加
- **変更ファイル**: `InventoryWindow.xaml.cs`, `GameController.cs`

### タスク6: インベントリソート状態の永続化

- **問題**: インベントリをソートした後の配置がインベントリを閉じて再度開くとリセットされる
- **修正内容**:
  - `MainWindow.xaml.cs` に `_inventorySorted` フラグを追加
  - `InventoryWindow` にソート状態フラグ `isSorted` を渡し、再オープン時にソート状態を維持
- **変更ファイル**: `MainWindow.xaml.cs`, `InventoryWindow.xaml.cs`

### タスク7: 街中パフォーマンス最適化

- **問題**: 街中で動作が重くなる
- **根本原因**: ロケーションマップ（町）でもダンジョンと同じFOV計算が毎ターン実行されていた
- **修正内容**: `GameController.cs` で `IsInLocationMap` が true の場合、`ComputeFov()` をスキップし全マス可視に設定
- **変更ファイル**: `GameController.cs`

### タスク8: 武器・防具の装備反映

- **問題**: インベントリ内の武器や防具を装備しようとしても装備欄に反映されない
- **修正内容**: `GameController.cs` の `UseItem()` で装備処理のロジックを修正。`Equipment.Equip()` の戻り値（前の装備品）をインベントリに戻す処理を追加
- **変更ファイル**: `GameController.cs`

### タスク9: ミニマップ描画最適化

- **問題**: 探索済みのマスが多くなると動作がとてつもなく重くなる
- **修正内容**: `GameRenderer.cs` のミニマップ描画を `DrawingVisual` から `WriteableBitmap` に変更。ピクセル単位の直接描画でGDIオブジェクト生成を回避
- **変更ファイル**: `GameRenderer.cs`

### タスク10: 自動探索の階段ナビゲーション

- **問題**: 下り階段タイル上でTabキーを押下した場合、上り階段へ移動するロジックがない
- **修正内容**: `GameController.cs` の自動探索ロジックで、プレイヤーが下り階段上にいる場合に上り階段（またはその逆）への経路を探索するよう追加
- **変更ファイル**: `GameController.cs`

### タスク11: カスタマイズ可能キーバインド

- **問題**: キーバインドを変更する機能がない
- **修正内容**:
  - `KeyBindingSettings.cs` 新規作成 — `KeyBindAction` 列挙型（35+アクション）、`KeyBinding` クラス（Key+ModifierKeys）、JSON永続化、`FindAction()` 逆引き
  - `KeyBindingWindow.xaml/.cs` 新規作成 — キーバインド変更UI、キャプチャオーバーレイ、競合検出＆スワップ、グループ別表示（移動/基本操作/画面/システム/スキルスロット）
  - `MainWindow.xaml.cs` のキーハンドラを辞書ベースのルックアップに全面リファクタ
  - `SettingsWindow.xaml` に「キーバインド設定を開く」ボタンを追加
- **変更ファイル**: `KeyBindingSettings.cs`（新規）, `KeyBindingWindow.xaml`（新規）, `KeyBindingWindow.xaml.cs`（新規）, `MainWindow.xaml.cs`, `SettingsWindow.xaml`, `SettingsWindow.xaml.cs`

### タスク12: ESCポーズ画面

- **問題**: プレイ中にESCキーでポーズ画面に遷移する機能がない
- **修正内容**:
  - `PauseWindow.xaml/.cs` 新規作成 — `PauseResult` 列挙型（Resume/Save/Load/Settings/KeyBindings/ReturnToTitle）、6ボタンレイアウト
  - `MainWindow.xaml.cs` に `ShowPauseDialog()` メソッド追加 — ポーズ画面からのアクション処理（セーブ/ロード/設定/キーバインド/タイトル画面復帰）、キーバインド更新反映
  - ESCキー押下で `ShowPauseDialog()` を呼び出し
- **変更ファイル**: `PauseWindow.xaml`（新規）, `PauseWindow.xaml.cs`（新規）, `MainWindow.xaml.cs`

### タスク13: スキルツリーノード配置改善

- **問題**: スキルツリーのノードの配置がごちゃごちゃして見にくい
- **修正内容**:
  - `SkillTreeWindow.xaml.cs` に `ScaleX = 1.4`, `ScaleY = 1.3` 定数を追加
  - `DrawConnection`, `DrawNode`, `TreeCanvas_MouseDown` のヒットテスト判定にスケーリング適用
  - オフセットを20→30、ラベル `MaxWidth` を100→110に拡大
  - `SkillTreeWindow.xaml` の全5キャンバスサイズを1200-1600→2200幅、280-500→560高さに拡張
- **変更ファイル**: `SkillTreeWindow.xaml.cs`, `SkillTreeWindow.xaml`

### タスク14: 町建物ドアの別マップ遷移

- **問題**: 町の建物のドアに入ると別マップ（建物内部）に遷移する機能がない
- **修正内容**:
  - `Tile.cs` に `TileType.BuildingEntrance` / `TileType.BuildingExit` を追加、`BuildingId` プロパティ追加
  - `LocationMapGenerator.cs` の `PlaceBuilding()` を改修 — 建物内部を壁で充填、`BuildingEntrance` タイルに `buildingId` を設定
  - 町建物に一意のID割り当て: inn, shop, smithy, guild, church, training, library, magic_shop
  - 村建物: village_house_1/2/3
  - NPCは町マップから削除（建物内部マップに移動）
  - `GenerateBuildingInterior(string buildingId)` 新規メソッド — 12x10の部屋、壁囲み、buildingIdに応じたNPC配置、`BuildingExit` タイルに `buildingId` 設定、入口位置設定
  - `GameController.cs` に `_buildingReturnMap`, `_buildingReturnPosition`, `_currentBuildingId` フィールド追加
  - `EnterBuilding()` / `ExitBuilding()` メソッド追加 — 町マップ保存/復元、内部マップ生成、全マス可視設定
  - 移動ロジックに `BuildingEntrance`/`BuildingExit` 判定を追加
  - `GetBuildingDisplayName()` で建物IDの日本語名を返す
  - `GameRenderer.cs` に BuildingEntrance/BuildingExit タイルの色を追加
- **変更ファイル**: `Tile.cs`, `LocationMapGenerator.cs`, `GameController.cs`, `GameRenderer.cs`

---

## テスト追加

| テストファイル | テスト数 | 内容 |
|--------------|---------|------|
| `VersionPrt019SystemTests.cs` | 22 | Phase 19全14タスクのシステムテスト |

### テスト内訳

| テスト名 | タスク | 内容 |
|---------|--------|------|
| `Inventory_Add_ReturnsFalse_WhenFull` | Task 1 | 満杯インベントリへの追加が失敗する |
| `Inventory_Add_ReturnsTrue_WhenHasSpace` | Task 1 | 空きがあればアイテム追加が成功する |
| `ThirstSystem_ThirstLevel_HasNumericValues` | Task 2 | ThirstLevel列挙型が数値を持つ |
| `BodyConditionSystem_FatigueLevel_HasNumericValues` | Task 2 | FatigueLevel列挙型が数値を持つ |
| `BodyConditionSystem_HygieneLevel_HasNumericValues` | Task 2 | HygieneLevel列挙型が数値を持つ |
| `SkillNodeType_Passive_Exists` | Task 3 | パッシブスキルタイプが存在する |
| `SkillNodeType_StatMinor_And_StatMajor_Exist` | Task 3 | StatMinor/StatMajorが存在する |
| `Equipment_Equip_ReturnsPreviousItem` | Task 8 | 装備交換時に前の装備品が返される |
| `Equipment_Unequip_ReturnsItem` | Task 8 | 装備解除でアイテムが返される |
| `AutoExploreSystem_StopReason_Contains_StairsFound` | Task 10 | StairsFoundが停止理由に含まれる |
| `TileType_BuildingEntrance_Exists` | Task 14 | BuildingEntrance型が存在する |
| `TileType_BuildingExit_Exists` | Task 14 | BuildingExit型が存在する |
| `Tile_BuildingEntrance_IsWalkable` | Task 14 | BuildingEntranceが移動可能 |
| `Tile_BuildingExit_IsWalkable` | Task 14 | BuildingExitが移動可能 |
| `Tile_BuildingEntrance_HasBuildingId` | Task 14 | BuildingEntranceにBuildingId設定可能 |
| `TownMap_HasBuildingEntrances_WithBuildingIds` | Task 14 | 町マップに8建物の入口がある |
| `BuildingInterior_Inn_HasInnkeeper` | Task 14 | 宿屋内部に宿屋主人がいる |
| `BuildingInterior_Shop_HasShopkeeper` | Task 14 | 商店内部に商人がいる |
| `BuildingInterior_Guild_HasGuildReceptionist` | Task 14 | ギルド内部に受付がいる |
| `BuildingInterior_HasEntrance` | Task 14 | 建物内部に入口位置がある |
| `BuildingInterior_Exit_HasBuildingId` | Task 14 | 建物出口にBuildingIdがある |
| `VillageMap_HasBuildingEntrances` | Task 14 | 村マップに建物入口がある |

### 既存テスト修正

| テストファイル | テスト名 | 修正内容 |
|--------------|---------|---------|
| `LocationMapGeneratorTests.cs` | `GenerateTownMap_HasDoors` | DoorClosed→BuildingEntrance チェックに変更 |
| `VersionPrt016SystemTests.cs` | `ContainsTrainerAndLibrarian` | 町マップ→建物内部マップでNPCチェックに変更 |

---

## 変更ファイル一覧

| ファイル | 変更種別 | 概要 |
|---------|---------|------|
| `src/RougelikeGame.Gui/GameController.cs` | 修正 | TryPickupItem修正、CanFitInGrid修正、UseItem装備処理修正、UnequipItem追加、FOVスキップ（町）、自動探索階段ナビ、EnterBuilding/ExitBuilding追加、GetBuildingDisplayName追加 |
| `src/RougelikeGame.Gui/MainWindow.xaml.cs` | 修正 | _keyBindingsフィールド、Window_KeyDown辞書ベース化、ShowPauseDialog追加、フロア表示制御、数値ステータス表示、_inventorySortedフラグ |
| `src/RougelikeGame.Gui/GameRenderer.cs` | 修正 | WriteableBitmapミニマップ、BuildingEntrance/Exitタイル色追加 |
| `src/RougelikeGame.Gui/InventoryWindow.xaml.cs` | 修正 | onUnequipItemコールバック、isSortedフラグ、装備スロット右クリック解除 |
| `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs` | 修正 | ScaleX/Y定数、スケーリング描画、パッシブスキルセクション |
| `src/RougelikeGame.Gui/SkillTreeWindow.xaml` | 修正 | 全5キャンバスサイズ拡張（2200x560） |
| `src/RougelikeGame.Gui/SettingsWindow.xaml` | 修正 | キーバインド設定ボタン追加 |
| `src/RougelikeGame.Gui/SettingsWindow.xaml.cs` | 修正 | OpenKeyBindButton_Clickハンドラ追加 |
| `src/RougelikeGame.Gui/KeyBindingSettings.cs` | 新規 | KeyBindAction列挙型、KeyBindingクラス、JSON永続化、FindAction逆引き |
| `src/RougelikeGame.Gui/KeyBindingWindow.xaml` | 新規 | キーバインド変更UI |
| `src/RougelikeGame.Gui/KeyBindingWindow.xaml.cs` | 新規 | キャプチャモード、競合検出、グループ別表示 |
| `src/RougelikeGame.Gui/PauseWindow.xaml` | 新規 | ESCポーズ画面（6ボタン） |
| `src/RougelikeGame.Gui/PauseWindow.xaml.cs` | 新規 | PauseResult列挙型、設定/キーバインド統合 |
| `src/RougelikeGame.Core/Map/Tile.cs` | 修正 | BuildingEntrance/BuildingExit型追加、BuildingIdプロパティ追加 |
| `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs` | 修正 | PlaceBuilding改修、buildingId割り当て、GenerateBuildingInterior追加 |
| `tests/RougelikeGame.Core.Tests/VersionPrt019SystemTests.cs` | 新規 | Phase 19テスト22件 |
| `tests/RougelikeGame.Core.Tests/LocationMapGeneratorTests.cs` | 修正 | BuildingEntrance対応 |
| `tests/RougelikeGame.Core.Tests/VersionPrt016SystemTests.cs` | 修正 | 建物内部NPC対応 |

---

## ブラッシュアップ記録

Phase 19全タスク完了後、プロジェクトガイドラインに従い自動ドキュメントブラッシュアップを実施。

### 更新対象ドキュメント

| ドキュメント | 更新内容 |
|-------------|---------|
| `00_ドキュメント概要.md` | Ver.prt.0.19実装計画書をインデックスに追加 |
| `11_クラス設計書.md` | KeyBindingSettings/KeyBindingWindow/PauseWindow/GameController建物遷移/LocationMapGenerator建物内部/TileType建物タイル/MainWindowキーバインド統合/GameRenderer WriteableBitmap/InventoryWindow装備解除/SkillTreeWindow配置改善を追加（11クラス/改修） |
| `13_GUIシステム設計書.md` | 実装状況にVer.prt.0.19追記、設定画面キーバインドボタン更新、KeyBindingWindow/PauseWindowをサブウィンドウ一覧に追加、BuildingEntrance/Exitタイル色追加、ESCキーをポーズ画面に更新、ミニマップWriteableBitmap化追記 |
| `14_マップシステム設計書.md` | 実装状況にVer.prt.0.19追記、BuildingEntrance/BuildingExitタイル追加、BuildingIdプロパティ追加 |
| `17_デバッグ・テスト設計書.md` | テスト数を5,632件（Core 5,484 + GUI 148）に最新化 |
| `20_GUI画面遷移図.md` | ウィンドウ総数20→22に更新、ESC→PauseWindow→SettingsWindow/KeyBindingWindow遷移を追加、建物入口/出口マップ遷移を追加 |
