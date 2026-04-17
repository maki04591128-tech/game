# 実装計画書 Ver.prt.0.7（ゲームプレイ改善・バグ修正）

**目標**: NPC操作性改善、UI操作性向上、魔法詠唱システム修正、インベントリD&D修正、ダンジョンキャッシュ修正
**状態**: ✅ 全タスク完了 — テスト全体2,717件（GUIオートテスト除外2,706件）全合格
**前提**: Ver.prt.0.6（新規システム拡張＋死に戻りメカニクス修正）完了済み
**完了時テスト数**: 全体 = 2,717件（Core 2,559 + GUI 158）、GUIオートテスト除外時 = 2,706件

---

## 1. 概要

Ver.prt.0.7 は Ver.prt.0.6 完了後のプレイテストに基づき、
ゲームプレイの操作性改善と既存バグの修正を行うフェーズである。

主な改善点:
- NPC操作をタイル踏み型から隣接インタラクト型に変更
- 各UIウィンドウのトグル操作（同じキーで開閉）
- 魔法詠唱ターン遅延の実装
- インベントリD&Dドロップの修正
- ダンジョンごとのフロアキャッシュ分離
- 町脱出をマップ端移動方式に変更

---

## 2. タスク一覧

### Phase F: ゲームプレイ改善・バグ修正（7タスク）

| # | タスク名 | 内容 | 状態 |
|---|---------|------|------|
| T.1 | NPC隣接インタラクト | NPC隣接時に方向キー押下でインタラクト（タイル踏み不要） | ✅ 完了 |
| T.2 | NPC会話UI接続 | HandleNpcTileをDialogueWindow経由の会話UIに接続 | ✅ 完了 |
| T.3 | ウィンドウトグル操作 | 8ウィンドウ（E/Y/U/Z/K/O/I/V）で同キー押下による閉じ操作 | ✅ 完了 |
| T.4 | 町脱出方式変更 | 町マップ端に移動で脱出（階段方式から変更） | ✅ 完了 |
| T.5 | 魔法詠唱ターン遅延 | TurnCost>1の魔法に詠唱ターン数の遅延を実装、詠唱中ログ出力 | ✅ 完了 |
| T.6 | インベントリD&Dドロップ修正 | ドラッグ後のドロップでグリッド位置が保存されない問題を修正 | ✅ 完了 |
| T.7 | ダンジョン別フロアキャッシュ | フロアキャッシュのキーを(ダンジョン名, 階層)タプルに変更 | ✅ 完了 |

---

## 3. タスク詳細

### T.1: NPC隣接インタラクト

**目的**: プレイヤーがNPCの隣に立った状態でNPC方向への移動キー押下でインタラクトを行えるようにする

**変更内容**:
1. `GameController.TryMove()` でNPCタイル判定を `BlocksMovement` チェックの前に配置
2. NPCタイルの場合、プレイヤーを移動させずに `HandleNpcTile()` を呼び出す
3. `IsNpcTile()` 静的ヘルパーメソッドを追加
4. 移動後の `HandleNpcTile` 呼び出しを削除

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] NPCに隣接して方向キーを押すとインタラクトが発生する
- [x] プレイヤーがNPCタイルの上に移動しない
- [x] 既存テストが全て合格する

### T.2: NPC会話UI接続

**目的**: NPC会話をDialogueWindowを使用したUIで表示する

**変更内容**:
1. `HandleNpcTile()` を書き換え、`DialogueNode` を作成して `OnShowDialogue` イベントを発火
2. 既存の `DialogueWindow` を再利用（新規ウィンドウ不要）

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] NPCインタラクト時にDialogueWindowが表示される
- [x] 既存テストが全て合格する

### T.3: ウィンドウトグル操作

**目的**: 各ウィンドウを開いたキーと同じキーで閉じられるようにする

**変更内容**:
8つのウィンドウの `Window_KeyDown` ハンドラに、開くキーを `Escape` と同様に閉じるキーとして追加:
- SkillTreeWindow: Key.E
- EncyclopediaWindow: Key.Y
- CompanionWindow: Key.U
- DeathLogWindow: Key.Z
- QuestLogWindow: Key.K
- ReligionWindow: Key.O
- InventoryWindow: Key.I
- SpellCastingWindow: Key.V

**変更ファイル**:
- `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs`
- `src/RougelikeGame.Gui/EncyclopediaWindow.xaml.cs`
- `src/RougelikeGame.Gui/CompanionWindow.xaml.cs`
- `src/RougelikeGame.Gui/DeathLogWindow.xaml.cs`
- `src/RougelikeGame.Gui/QuestLogWindow.xaml.cs`
- `src/RougelikeGame.Gui/ReligionWindow.xaml.cs`
- `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`
- `src/RougelikeGame.Gui/SpellCastingWindow.xaml.cs`

**受入基準**:
- [x] 各ウィンドウを開いたキーで閉じることができる
- [x] Escapeキーでも引き続き閉じることができる
- [x] 既存テストが全て合格する

### T.4: 町脱出方式変更

**目的**: 町からの脱出を階段ではなくマップ外周部への移動で行えるようにする

**変更内容**:
1. `GameController.TryMove()` で移動先がマップ範囲外かつ `_isInLocationMap` が true の場合、`TryLeaveTown()` を呼び出す

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] 町マップの端で外方向に移動するとTryLeaveTownが呼ばれる
- [x] 既存テストが全て合格する

### T.5: 魔法詠唱ターン遅延

**目的**: TurnCostが1より大きい魔法に対して詠唱ターン遅延を実装する

**変更内容**:
1. `_chantRemainingTurns` と `_pendingSpellResult` フィールドを追加
2. `TryCastSpell()` を書き換え: TurnCost>1の場合、詠唱状態に入る（即時発動しない）
3. `ProcessChanting()` メソッドを追加: 毎ターン詠唱カウントダウンを行い、完了時に魔法効果を適用
4. `ProcessTurnEffects()` の先頭で `ProcessChanting()` を呼び出す
5. `IsChanting` プロパティを追加

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] TurnCost>1の魔法が詠唱ターン分の遅延で発動する
- [x] 詠唱中は毎ターン「…詠唱中…」ログが出力される
- [x] TurnCost<=1の魔法は従来通り即時発動する
- [x] 既存テストが全て合格する

### T.6: インベントリD&Dドロップ修正

**目的**: ドラッグ&ドロップでアイテムをグリッド内の別位置に移動した後、位置が保持されない問題を修正する

**変更内容**:
1. `_savedPositions` クラスレベルフィールド（`Dictionary<int, (int GridX, int GridY)>`）を追加
2. `RenderGrid()` で `_gridCells.Clear()` の前に位置情報を `_savedPositions` に保存
3. `PlaceItemsOnGrid()` が `_savedPositions` から位置情報を読み取る（クリア済みの `_gridCells` ではなく）

**根本原因**: `RenderGrid()` が `_gridCells.Clear()` を呼んだ後に `PlaceItemsOnGrid()` が `_gridCells` の（空の）コピーから位置を復元しようとしていたため、ドラッグ移動後の位置が常に失われていた。

**変更ファイル**: `src/RougelikeGame.Gui/InventoryWindow.xaml.cs`

**受入基準**:
- [x] D&Dでアイテムを移動した位置が保持される
- [x] RenderGrid後もアイテム位置が維持される
- [x] 既存テストが全て合格する

### T.7: ダンジョン別フロアキャッシュ

**目的**: 異なるダンジョンのフロアレイアウトが共有されてしまう問題を修正する

**変更内容**:
1. `_floorCache` のキーを `Dictionary<int, FloorCache>` から `Dictionary<(string DungeonName, int Floor), FloorCache>` に変更
2. `GenerateFloor()` でキャッシュの検索・保存に `(_currentMapName, CurrentFloor)` タプルを使用

**根本原因**: フロアキャッシュが階層番号のみをキーとしていたため、異なるダンジョンの同一階層が同じキャッシュエントリを共有し、レイアウトと探索状態が混在していた。

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] 異なるダンジョンの同一階層が別のレイアウトを持つ
- [x] 各ダンジョンの探索状態が独立している
- [x] 既存テストが全て合格する

---

## 4. 実装結果

| 項目 | 値 |
|------|-----|
| 変更ファイル数 | 10ファイル |
| テスト合計 | 2,717件（GUIオート除外2,706件） |
| テスト合格 | 2,717件（100%） |
| ビルドエラー | 0件 |

### 変更ファイル一覧

| ファイル | 変更種別 | 内容 |
|---------|----------|------|
| GameController.cs | 修正 | NPC隣接インタラクト、町脱出方式、魔法詠唱遅延、フロアキャッシュ分離 |
| InventoryWindow.xaml.cs | 修正 | D&Dドロップ位置保存修正、Iキートグル |
| SkillTreeWindow.xaml.cs | 修正 | Eキートグル |
| EncyclopediaWindow.xaml.cs | 修正 | Yキートグル |
| CompanionWindow.xaml.cs | 修正 | Uキートグル |
| DeathLogWindow.xaml.cs | 修正 | Zキートグル |
| QuestLogWindow.xaml.cs | 修正 | Kキートグル |
| ReligionWindow.xaml.cs | 修正 | Oキートグル |
| SpellCastingWindow.xaml.cs | 修正 | Vキートグル |

---

## 5. ドキュメントブラッシュアップ記録

Phase F（Ver.prt.0.7）完了に伴い、以下のドキュメントを更新：

| ドキュメント | 更新内容 |
|-------------|---------|
| マスター実装計画書 | Ver.prt.0.7エントリ追加（ロードマップ表・マイルストーン詳細・フェーズ概要テーブル・フロー図） |
| 実装計画書Ver.prt.0.7 | 新規作成（全7タスク詳細・実装結果・ブラッシュアップ記録） |
| 13_GUIシステム設計書 | 実装状況バナー更新、NPC会話起動条件変更、D&D説明更新、イベント追加（OnShowDialogue/OnCastingStarted）、処理フロー更新（NPC隣接・町脱出・詠唱）、キー説明更新（トグル操作）、将来計画にVer.prt.0.7実装済み6項目追加 |
| 08_魔法言語設計書 | 詠唱ターン消費の実装状況にProcessChanting詠唱遅延の詳細を追記 |
| 14_マップシステム設計書 | 実装状況バナーにフロアキャッシュ分離（タプルキー）と町脱出方式変更を追記 |
| 11_クラス設計書 | FloorCacheキー変更、InventoryWindow _savedPositions追加、GameController詠唱/NPC隣接の新規エントリ追加 |
| 00_ドキュメント概要 | Ver.prt.0.7計画書エントリ追加、フォルダ構成にファイル追加 |
