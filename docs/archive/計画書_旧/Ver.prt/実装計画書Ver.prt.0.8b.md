# 実装計画書 Ver.prt.0.8b（町脱出外周移動・斜め移動バグ修正・ショップUI刷新）

**目標**: 町脱出を外周移動方式に修正、NPC対話後の斜め移動バグ修正、ショップUIを双方グリッドレイアウトに刷新
**状態**: ✅ 全タスク完了 — テスト全体2,706件（GUIオートテスト除外）全合格
**前提**: Ver.prt.0.8（NPC会話アクション・町脱出修正）完了済み
**完了時テスト数**: 全体 = 2,706件（Core 2,559 + GUI 147）

---

## 1. 概要

Ver.prt.0.8b は Ver.prt.0.8 完了後のプレイテストに基づき、
3つの問題を修正するフェーズである。

主な改善点:
- 町脱出が階段（StairsUp）でしか行えなかった問題を修正。外周壁を通行可能にし、マップ端移動で脱出可能に
- NPC対話（DialogueWindow）後にShowDialogによるフォーカス喪失で_heldMovementKeysに残留キーが残り、斜め移動が発生するバグを修正
- ショップUIをListBox方式からInventoryWindow風の双方Canvas グリッドレイアウトに全面刷新

---

## 2. タスク一覧

### Phase H: 町脱出修正・斜め移動バグ修正・ショップUI刷新（3タスク）

| # | タスク名 | 内容 | 状態 |
|---|---------|------|------|
| T.1 | 町脱出外周移動修正 | LocationMapGeneratorの外周壁を通行可能タイルに変更 | ✅ 完了 |
| T.2 | NPC対話後斜め移動バグ修正 | MainWindow.Activatedイベントで_heldMovementKeysをクリア | ✅ 完了 |
| T.3 | ショップUI双方グリッド化 | ShopWindow.xaml/xaml.csを双方Canvasグリッドレイアウトに全面書き換え | ✅ 完了 |

---

## 3. タスク詳細

### T.1: 町脱出外周移動修正

**目的**: 町（ロケーションマップ）からの脱出を、階段だけでなく外周端からの移動で行えるようにする

**根本原因**: `LocationMapGenerator.DrawBorder()` が全てのロケーションマップの外周に `TileType.Wall`（BlocksMovement=true）を配置していたため、プレイヤーが外周端に到達できず、`TryMove()` の境界チェック（`newPos.X < 0` 等 → `TryLeaveTown()`）が発動しなかった。

**変更内容**:
1. **GenerateTownMap / GenerateVillageMap**: `DrawBorder(map)` の呼び出しを削除。元々 `FillAll(Grass)` で全面草地のため、外周はGrassのまま通行可能
2. **GenerateFacilityMap / GenerateShrineMap**: `FillAll(Wall)` で全面壁のため、新規 `DrawPassableBorder(map)` を呼び出して外周のみ `TileType.Floor` に変更
3. **DrawBorder** → **DrawPassableBorder** にリネーム: Wall → Floor に変更

**変更ファイル**: `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs`

**受入基準**:
- [x] 町・村マップで外周端に立って外方向に移動するとシンボルマップに帰還する
- [x] 施設・神殿マップでも外周端から脱出可能
- [x] StairsUp（階段）での脱出も引き続き動作する
- [x] 既存テストが全て合格する

### T.2: NPC対話後斜め移動バグ修正

**目的**: NPC対話（DialogueWindow）やショップ（ShopWindow）等のモーダルダイアログ表示後に、WASDキーの各1キーを押しても斜め移動になるバグを修正する

**根本原因**: `MainWindow._heldMovementKeys`（HashSet&lt;Key&gt;）は `Window_KeyDown` でキー追加、`Window_KeyUp` でキー削除されるが、`ShowDialog()` でDialogueWindow等が表示されると MainWindow がフォーカスを失う。この間に物理キーが離されても `Window_KeyUp` が MainWindow に届かず、HashSet に古いキーが残留する。結果として、ダイアログ閉じた後に1キーしか押していないのに「2キー同時押し」と誤判定され斜め移動になる。

**変更内容**:
1. MainWindow コンストラクタに `Activated` イベントハンドラを追加
2. ウィンドウが再アクティブ化（= ShowDialog終了後にフォーカス復帰）されるたびに `_heldMovementKeys.Clear()` を実行
3. この方式により、個別の ShowDialog 呼び出し箇所（約15箇所）に個別修正を入れる必要がなく、将来の新しいダイアログ追加時にも自動的に対応される

**変更ファイル**: `src/RougelikeGame.Gui/MainWindow.xaml.cs`

**受入基準**:
- [x] NPC対話後にWASD各キー単独押下で正しく4方向移動できる
- [x] ショップ・インベントリ・ステータス等のダイアログ後も同様
- [x] ダイアログ中の操作（WASD以外のキー）が残留しない
- [x] 既存テストが全て合格する

### T.3: ショップUI双方グリッド化

**目的**: ショップUIをListBox方式からInventoryWindow風のCanvas双方グリッドレイアウトに変更する

**変更内容**:

#### ShopWindow.xaml
1. ウィンドウサイズを 550x500 → 880x560 に拡大
2. ListBox を削除し、左右に分割した2つの Canvas グリッドを配置
   - 左側: `ShopCanvas`（ショップ商品グリッド 400x240、CellSize=40、10x6）
   - 右側: `PlayerCanvas`（プレイヤーインベントリグリッド 400x240、同仕様）
3. 各グリッドにラベル（「ショップ商品」「所持品」）を追加
4. タイトル+所持金を1行に統合

#### ShopWindow.xaml.cs
1. `ShopGridCellInfo` クラスを新規追加（Index, Name, Price, Stock, IsShopItem, GridX/Y, Width/Height, PlayerItem）
2. InventoryWindow 同様の `DrawGridLines()` / `FindFreePosition()` メソッドを実装
3. `PlaceShopItems()`: ショップ商品を1x1セルでグリッドに配置（名前+価格+在庫表示）
4. `PlacePlayerItems()`: プレイヤーアイテムをGridItemSizeに基づくサイズでグリッドに配置（レアリティ色+売値表示）
5. `AddShopItemVisual()`: Rectangle+名前TextBlock+価格TextBlock+在庫TextBlockを Canvas に描画
6. クリック選択: `OnItemClicked()` でショップ/プレイヤーどちらのアイテムかを識別
7. 売買ロジックは既存の `TryBuyItem()` / `TrySellItem()` をそのまま利用
8. 旧 `ShopItemViewModel` クラス、`ItemList` ListBox 関連コードを全て削除

**変更ファイル**:
- `src/RougelikeGame.Gui/ShopWindow.xaml`
- `src/RougelikeGame.Gui/ShopWindow.xaml.cs`

**受入基準**:
- [x] ショップ画面に左右2つのグリッドが表示される
- [x] 購入モード: 左側にショップ商品、右側にプレイヤーインベントリが表示される
- [x] 売却モード: 右側のプレイヤーインベントリからアイテムを選択して売却できる
- [x] アイテムクリックで選択、Enter で売買実行、Esc で閉じる
- [x] プレイヤーアイテムがレアリティに応じた色で表示される
- [x] 既存テストが全て合格する

---

## 4. 実装結果

| 項目 | 値 |
|------|-----|
| 変更ファイル数 | 4ファイル |
| テスト合計 | 2,706件（GUIオート除外） |
| テスト合格 | 2,706件（100%） |
| ビルドエラー | 0件 |

### 変更ファイル一覧

| ファイル | 変更種別 | 内容 |
|---------|----------|------|
| LocationMapGenerator.cs | 修正 | DrawBorder削除/DrawPassableBorder追加、Town/Village/Facility/Shrineマップの外周通行可能化 |
| MainWindow.xaml.cs | 修正 | Activatedイベントで_heldMovementKeys.Clear()追加 |
| ShopWindow.xaml | 全面書き換え | 880x560、双方Canvas グリッドレイアウト（ShopCanvas + PlayerCanvas） |
| ShopWindow.xaml.cs | 全面書き換え | ShopGridCellInfo、Canvas描画、クリック選択、双方グリッド売買ロジック |

---

## 5. ブラッシュアップ記録

| 日時 | 対象 | 内容 |
|------|------|------|
| Phase H完了時 | 実装計画書Ver.prt.0.8b | 新規作成 |
| Phase H完了時 | マスター実装計画書 | Ver.prt.0.8b行追加、フェーズ概要更新 |
| Phase H完了時 | GUIシステム設計書 | ShopWindow記述を双方グリッドレイアウトに更新 |
| Phase H完了時 | マップシステム設計書 | LocationMapGeneratorの外周処理記述を更新 |
