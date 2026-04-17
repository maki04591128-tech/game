# 実装計画書 Ver.prt.0.9（インベントリ改善・ショップD&D・図鑑連動・ギルド機能拡張）

**目標**: インベントリUX改善、ショップD&D統合、グリッド容量チェック、図鑑自動更新、ギルド仲間募集・クエストボード追加
**状態**: ✅ 全タスク完了 — テスト全体2,706件（GUIオートテスト除外）全合格
**前提**: Ver.prt.0.8b（町脱出外周移動・斜め移動バグ修正・ショップUI刷新）完了済み
**完了時テスト数**: 全体 = 2,706件（Core 2,559 + GUI 147）

---

## 1. 概要

Ver.prt.0.9 は Ver.prt.0.8b 完了後のプレイテストに基づき、
10個の改善・新機能を実装するフェーズである。

主な改善点:
- ロケーションマップの不要な脱出用階段（StairsUp）を完全除去
- インベントリウィンドウをコールバックパターンで開いたまま操作可能に変更
- インベントリに自動ソート機能を追加（カテゴリ→レアリティ→名前順）
- ショップUIをドラッグ＆ドロップ統合方式に刷新（タブ廃止、中央矢印ガイド）
- ショップでマウスホイールによる取引数量変更を追加
- アイテム取得時にグリッド容量チェックを追加（収まらないアイテムは取得不可）
- インベントリ外へのドラッグ＆ドロップでアイテムを地面に落とす機能を追加
- 図鑑を敵撃破時・アイテム取得時に自動更新する連動機能を追加
- ギルド受付NPCから仲間募集ウィンドウを表示する機能を追加
- ギルド受付NPCのクエスト確認をクエストボードウィンドウに変更

---

## 2. タスク一覧

### Phase I: インベントリ改善・ショップD&D・図鑑連動・ギルド機能拡張（10タスク）

| # | タスク名 | 内容 | 状態 |
|---|---------|------|------|
| T.1 | 脱出用階段除去 | LocationMapGeneratorの5種マップからSetStairsUp/SetEntrance呼び出しを削除 | ✅ 完了 |
| T.2 | インベントリ常時表示化 | InventoryWindowをコールバックパターン（onUseItem, onDropItem, getItems）で開いたまま操作可能に | ✅ 完了 |
| T.3 | インベントリ自動ソート | Sキー＋SortButtonでカテゴリ→レアリティ(降順)→名前順にソート | ✅ 完了 |
| T.4 | ショップD&D統合 | ShopWindowをタブ廃止の統合D&Dレイアウトに全面書き換え（中央矢印ガイド） | ✅ 完了 |
| T.5 | ショップスクロール数量 | マウスホイールで取引数量(_tradeQuantity)を変更可能に | ✅ 完了 |
| T.6 | グリッド容量チェック | TryPickupItemにCanFitInGrid()による容量シミュレーションを追加 | ✅ 完了 |
| T.7 | インベントリ外ドロップ | GridCanvas_MouseUpでグリッド外検出時にonDropItemコールバックを呼び出し | ✅ 完了 |
| T.8 | 図鑑自動更新 | 敵撃破・アイテム取得時にRegisterAndDiscoverEncyclopedia()で図鑑エントリを自動登録・発見 | ✅ 完了 |
| T.9 | 仲間募集機能 | ギルド受付に「仲間を募集する」選択肢追加、RecruitCompanionWindow新規作成 | ✅ 完了 |
| T.10 | クエストボード | ギルド受付の「クエストを確認する」をQuestBoardWindow表示に変更 | ✅ 完了 |

---

## 3. タスク詳細

### T.1: 脱出用階段除去

**目的**: ロケーションマップ（町・村・施設・神殿・フィールド）に不要な脱出用階段（StairsUp）が配置されていた問題を修正する

**根本原因**: `LocationMapGenerator` の各 `Generate*Map()` メソッドで `map.SetStairsUp()` / `map.SetEntrance()` が呼ばれていたが、Ver.prt.0.8b で外周移動方式の脱出が実装済みのため不要になっていた。

**変更内容**:
1. `GenerateTownMap()` から `SetStairsUp` / `SetEntrance` 呼び出しを削除
2. `GenerateVillageMap()` から同様に削除
3. `GenerateFacilityMap()` から同様に削除
4. `GenerateShrineMap()` から同様に削除
5. `GenerateFieldMap()` から同様に削除
6. テスト30件を修正: `HasEntrance` → `NoStairsUp` に変更、`StairsUpPosition` / `EntrancePosition` のNotNull → Null アサーションに変更

**変更ファイル**:
- `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs`
- `tests/RougelikeGame.Core.Tests/LocationMapGeneratorTests.cs`

**受入基準**:
- [x] 5種のロケーションマップにStairsUpが配置されない
- [x] 外周移動による脱出は引き続き動作する
- [x] 既存テスト30件を修正し全合格

### T.2: インベントリ常時表示化

**目的**: アイテム使用・装備変更のたびにインベントリウィンドウが閉じてしまう問題を修正し、操作後も開いたまま更新される方式に変更する

**変更内容**:
1. `InventoryWindow` コンストラクタにコールバック引数を追加:
   - `Action<int> onUseItem`: アイテム使用コールバック（インデックス指定）
   - `Action<int> onDropItem`: アイテムドロップコールバック（インデックス指定）
   - `Func<List<Item>> getItems`: 最新アイテムリスト取得デリゲート
2. アイテム使用時: コールバック呼び出し → `RefreshItems(getItems())` で再描画（ウィンドウは閉じない）
3. `MainWindow.ShowInventoryDialog()` を対応するコールバック引数付きに修正

**変更ファイル**:
- `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`
- `src/RougelikeGame.Gui/MainWindow.xaml.cs`

**受入基準**:
- [x] アイテム使用後もインベントリウィンドウが開いたまま
- [x] 装備変更後もインベントリが開いたまま
- [x] アイテム一覧が操作後に最新状態に更新される

### T.3: インベントリ自動ソート

**目的**: インベントリ内のアイテムをワンキーで自動整理する機能を追加する

**変更内容**:
1. `InventoryWindow.xaml` に「ソート」ボタン（SortButton）を追加
2. `InventoryWindow.xaml.cs` にSキーバインド + SortButton_Click ハンドラを追加
3. ソート順序: カテゴリ（Equipment → Consumable → Material → Other）→ レアリティ（降順、高レアリティ優先）→ 名前（昇順）
4. ソート後にグリッド再配置

**変更ファイル**:
- `src/RougelikeGame.Gui/InventoryWindow.xaml`
- `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`

**受入基準**:
- [x] Sキーまたはソートボタンクリックでアイテムがソートされる
- [x] カテゴリ→レアリティ(降順)→名前の順序で整列する
- [x] ソート後もグリッド表示が正しく更新される

### T.4: ショップD&D統合

**目的**: ショップUIを購入/売却タブ分離方式からドラッグ＆ドロップ統合方式に変更し、直感的な売買操作を実現する

**変更内容**:
1. **ShopWindow.xaml** を全面書き換え:
   - タブ（購入/売却）を廃止
   - 左: ショップ商品Canvas（ShopCanvas）、右: プレイヤーインベントリCanvas（PlayerCanvas）
   - 中央に矢印ガイド（←→）を配置してD&D方向を視覚的に示す
   - 下部に取引情報パネル（選択アイテム名、価格、数量、合計金額）
2. **ShopWindow.xaml.cs** を全面再作成（357行）:
   - ショップ→プレイヤー方向のD&D = 購入
   - プレイヤー→ショップ方向のD&D = 売却
   - `_tradeQuantity` フィールドで取引数量管理
   - 既存の `TryBuyItem()` / `TrySellItem()` API を利用

**変更ファイル**:
- `src/RougelikeGame.Gui/ShopWindow.xaml`
- `src/RougelikeGame.Gui/ShopWindow.xaml.cs`

**受入基準**:
- [x] タブなしの統合レイアウトで左右にグリッドが表示される
- [x] ショップ→プレイヤーへのD&Dで購入が実行される
- [x] プレイヤー→ショップへのD&Dで売却が実行される
- [x] 中央に矢印ガイドが表示される
- [x] 既存テストが全合格

### T.5: ショップスクロール数量

**目的**: ショップでの取引数量をマウスホイールで直感的に変更できるようにする

**変更内容**:
1. `ShopWindow.xaml.cs` にMouseWheelイベントハンドラを追加
2. ホイール上回転: `_tradeQuantity` を+1（在庫/所持数上限まで）
3. ホイール下回転: `_tradeQuantity` を-1（最小1）
4. 数量変更時にUIの取引情報パネルを更新

**変更ファイル**: `src/RougelikeGame.Gui/ShopWindow.xaml.cs`

**受入基準**:
- [x] マウスホイール上で取引数量が増加する
- [x] マウスホイール下で取引数量が減少する
- [x] 数量が在庫上限/所持数を超えない
- [x] 数量変更がUIに即座に反映される

### T.6: グリッド容量チェック

**目的**: インベントリグリッドに収まらないサイズのアイテムを取得できないようにする

**変更内容**:
1. `GameController` に以下の静的メソッドを追加:
   - `CanFitInGrid(List<Item> currentItems, Item newItem)`: 10×6グリッドをシミュレーションし、全既存アイテム配置後に新アイテムが収まるか判定
   - `FindFreeGridPosition(bool[,] grid, int itemW, int itemH)`: 空き領域をスキャンして配置可能位置を返す
   - `GetItemGridSize(Item item)`: アイテムのGridItemSizeから(幅, 高さ)タプルを返す
2. `TryPickupItem()` で `CanFitInGrid()` を呼び出し、収まらない場合は取得を拒否してメッセージを表示

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] グリッドに収まるアイテムは従来通り取得できる
- [x] グリッドに収まらないアイテムは取得が拒否される
- [x] 拒否時に「インベントリに空きがありません」メッセージが表示される
- [x] 既存テストが全合格

### T.7: インベントリ外ドロップ

**目的**: インベントリウィンドウのグリッド外にアイテムをドラッグ＆ドロップした場合、その場にアイテムを落とす機能を追加する

**変更内容**:
1. `InventoryWindow.xaml.cs` の `GridCanvas_MouseUp` イベントハンドラで、ドロップ位置がグリッド境界外かどうかを判定
2. 境界外の場合、`_onDropItem` コールバックを呼び出してGameController側でアイテムドロップ処理を実行
3. ドロップ後は `RefreshItems()` でグリッドを更新

**変更ファイル**: `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`

**受入基準**:
- [x] グリッド外へのD&Dでアイテムが地面に落とされる
- [x] 落としたアイテムがインベントリから除去される
- [x] グリッド内へのD&Dは従来通りの移動動作

### T.8: 図鑑自動更新

**目的**: 図鑑（EncyclopediaSystem）が空のままだった問題を修正し、ゲームプレイ中に自動的に図鑑エントリが登録・発見されるようにする

**変更内容**:
1. `GameController` にヘルパーメソッド `RegisterAndDiscoverEncyclopedia()` を追加:
   - エントリが未登録なら `RegisterEntry()` で自動登録（3段階の説明テキスト付き）
   - `IncrementDiscovery()` で発見度を増加
   - 初回発見時（発見度1→2）にメッセージを表示
2. `OnEnemyDefeated()` 内で敵撃破時に `Monster` カテゴリで呼び出し
3. `TryPickupItem()` 内でアイテム取得時に `Item` カテゴリで呼び出し

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] 敵を倒すと図鑑のモンスターカテゴリにエントリが追加される
- [x] アイテムを拾うと図鑑のアイテムカテゴリにエントリが追加される
- [x] 初回発見時にメッセージが表示される
- [x] 既存テストが全合格

### T.9: 仲間募集機能

**目的**: ギルド受付NPCから仲間候補を閲覧・雇用できる機能を追加する

**変更内容**:
1. **GameController**:
   - `HandleNpcTile()` のギルド受付選択肢に「仲間を募集する」を追加
   - `DispatchNpcAction()` に `recruit_companion` アクションを追加
   - `HandleRecruitCompanion()`: 仲間候補リスト生成→ `OnShowRecruitCompanion` イベント発火
   - `GenerateCompanionCandidates()`: ギルドランク・プレイヤーレベルに応じた2〜4体の仲間候補をランダム生成
   - `TryHireCompanion(CompanionData)`: ゴールド消費→ `CompanionSystem.AddCompanion()` で仲間追加
   - `OnShowRecruitCompanion` イベント追加（`Action<List<CompanionSystem.CompanionData>>`）
2. **RecruitCompanionWindow.xaml**: 仲間候補一覧ウィンドウ新規作成（ScrollViewer + StackPanel）
3. **RecruitCompanionWindow.xaml.cs**: 候補者ごとに名前・タイプ・レベル・ステータス・雇用コストを表示、「雇う」ボタンで雇用実行
4. **MainWindow.xaml.cs**: `ShowRecruitCompanionDialog()` ハンドラ追加、`OnShowRecruitCompanion` イベント購読

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`
- `src/RougelikeGame.Gui/RecruitCompanionWindow.xaml`（新規）
- `src/RougelikeGame.Gui/RecruitCompanionWindow.xaml.cs`（新規）
- `src/RougelikeGame.Gui/MainWindow.xaml.cs`

**受入基準**:
- [x] ギルド受付で「仲間を募集する」選択肢が表示される
- [x] 仲間候補が2〜4体表示される
- [x] 「雇う」ボタンでゴールドを消費して仲間が追加される
- [x] ゴールド不足時はエラーメッセージが表示される

### T.10: クエストボード

**目的**: ギルド受付の「クエストを確認する」を専用のクエストボードウィンドウに変更し、クエストの受注・報告を行えるようにする

**変更内容**:
1. **GameController**:
   - `DispatchNpcAction()` の `view_quests` アクションを `OnShowQuestBoard` イベント発火に変更
   - `OnShowQuestBoard` イベント追加（`Action`）
2. **QuestBoardWindow.xaml**: 3タブ構成（受注可能/進行中/完了済み）のクエストボードウィンドウ新規作成
3. **QuestBoardWindow.xaml.cs**:
   - `ShowAvailableQuests()`: 受注可能クエスト一覧＋受注ボタン
   - `ShowActiveQuests()`: 進行中クエスト一覧＋報告ボタン（達成時のみ有効）
   - `ShowCompletedQuests()`: 完了済みクエスト一覧
   - 既存 `QuestSystem` API（`GetAvailableQuests` / `GetActiveQuests` / `TryAcceptQuest` / `TryTurnInQuest`）を利用
4. **MainWindow.xaml.cs**: `ShowQuestBoardDialog()` ハンドラ追加、`OnShowQuestBoard` イベント購読

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`
- `src/RougelikeGame.Gui/QuestBoardWindow.xaml`（新規）
- `src/RougelikeGame.Gui/QuestBoardWindow.xaml.cs`（新規）
- `src/RougelikeGame.Gui/MainWindow.xaml.cs`

**受入基準**:
- [x] ギルド受付で「クエストを確認する」選択肢でクエストボードが表示される
- [x] 3タブ（受注可能/進行中/完了済み）が切り替え可能
- [x] 受注可能クエストの「受注」ボタンでクエストが受注される
- [x] 進行中クエストの「報告」ボタンで達成報告が行える

---

## 4. 実装結果

| 項目 | 値 |
|------|-----|
| 変更ファイル数 | 12ファイル（新規4 + 修正8） |
| テスト合計 | 2,706件（GUIオート除外） |
| テスト合格 | 2,706件（100%） |
| ビルドエラー | 0件 |
| テスト修正 | 30件（LocationMapGeneratorTests: HasEntrance→NoStairsUp） |

### 変更ファイル一覧

| ファイル | 変更種別 | 内容 |
|---------|----------|------|
| LocationMapGenerator.cs | 修正 | 5種マップからSetStairsUp/SetEntrance削除 |
| LocationMapGeneratorTests.cs | 修正 | 30テスト: HasEntrance→NoStairsUp、アサーション変更 |
| InventoryWindow.xaml | 修正 | SortButton追加 |
| InventoryWindow.xaml.cs | 修正 | コールバックパターン、ソート機能、グリッド外ドロップ検出 |
| ShopWindow.xaml | 全面書き換え | タブ廃止、統合D&Dレイアウト、中央矢印ガイド |
| ShopWindow.xaml.cs | 全面書き換え | D&D売買、MouseWheelスクロール数量、357行 |
| GameController.cs | 修正 | CanFitInGrid/FindFreeGridPosition/GetItemGridSize追加、RegisterAndDiscoverEncyclopedia追加、HandleRecruitCompanion/GenerateCompanionCandidates/TryHireCompanion追加、OnShowRecruitCompanion/OnShowQuestBoard追加、AddMessage公開化 |
| MainWindow.xaml.cs | 修正 | ShowInventoryDialogコールバック化、ShowRecruitCompanionDialog/ShowQuestBoardDialog追加、Activated時_heldMovementKeys.Clear()追加 |
| RecruitCompanionWindow.xaml | 新規 | 仲間募集候補一覧UI |
| RecruitCompanionWindow.xaml.cs | 新規 | 候補表示・雇用ボタン・ゴールド消費処理 |
| QuestBoardWindow.xaml | 新規 | 3タブクエストボードUI |
| QuestBoardWindow.xaml.cs | 新規 | 受注可能/進行中/完了済みクエスト表示・受注/報告処理 |

---

## 5. 新規クラス・メソッド一覧

### 新規ウィンドウクラス

| クラス名 | ファイル | 用途 |
|---------|--------|------|
| RecruitCompanionWindow | RecruitCompanionWindow.xaml/.cs | 仲間募集候補一覧・雇用UI |
| QuestBoardWindow | QuestBoardWindow.xaml/.cs | クエストボード（3タブ: 受注可能/進行中/完了済み） |

### GameController 追加メソッド

| メソッド | 種別 | 用途 |
|---------|------|------|
| CanFitInGrid(List\<Item\>, Item) | public static | 10×6グリッドに新アイテムが収まるかシミュレーション判定 |
| FindFreeGridPosition(bool[,], int, int) | public static | グリッド内の空き位置をスキャンして返す |
| GetItemGridSize(Item) | public static | アイテムのGridItemSizeから(幅, 高さ)を返す |
| RegisterAndDiscoverEncyclopedia(...) | private | 図鑑エントリの自動登録・発見度増加・初回発見メッセージ |
| HandleRecruitCompanion() | private | 仲間候補生成→OnShowRecruitCompanionイベント発火 |
| GenerateCompanionCandidates() | private | ギルドランク・レベルに応じた2〜4体の仲間候補生成 |
| TryHireCompanion(CompanionData) | public | ゴールド消費→CompanionSystem.AddCompanion() |

### GameController 追加イベント

| イベント | 型 | 用途 |
|---------|-----|------|
| OnShowRecruitCompanion | Action\<List\<CompanionData\>\> | 仲間募集ウィンドウ表示要求 |
| OnShowQuestBoard | Action | クエストボードウィンドウ表示要求 |

---

## 6. ブラッシュアップ記録

| 日時 | 対象 | 内容 |
|------|------|------|
| Phase I完了時 | 実装計画書Ver.prt.0.9 | 新規作成（本ファイル） |
| Phase I完了時 | マスター実装計画書 | Ver.prt.0.9行追加、フェーズ概要更新 |
| Phase I完了時 | クラス設計書 | GameController追加メソッド/イベント、RecruitCompanionWindow/QuestBoardWindow追記 |
| Phase I完了時 | GUIシステム設計書 | RecruitCompanionWindow/QuestBoardWindowをサブウィンドウ一覧に追加、ShopWindow/InventoryWindow記述更新 |
| Phase I完了時 | 拡張システム設計書 | 図鑑自動更新・ギルド仲間募集・クエストボードの実装状況を更新 |
| Phase I完了時 | ドキュメント概要 | Ver.prt.0.9行追加、フォルダ構成更新 |
