# 実装計画書 Ver.prt.0.14 — バグ修正・UI改善パッチ

**ステータス**: ✅ 全タスク完了
**目的**: ゲームプレイテストで発見された7つのバグ修正およびUI改善
**テスト結果**: 5,557テスト全合格（Core: 5,409 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | 装備品をインベントリグリッドから除外 | UI改善 | ✅ 完了 |
| 2 | スキルツリーをプレイヤーの種族/職業/素性でフィルタリング | UI改善 | ✅ 完了 |
| 3 | WorldMap(J)とMessageLog(L)のキートグル閉じ | バグ修正 | ✅ 完了 |
| 4 | ショップ購入/売却の選択バグ修正＋Ctrl+Click＋D&D | バグ修正/UI改善 | ✅ 完了 |
| 5 | 人型モンスター撃破時の二重ゴールドログ修正 | バグ修正 | ✅ 完了 |
| 6 | ミミック擬態対象を宝箱・収納容器のみに制限 | 仕様変更 | ✅ 完了 |
| 7 | 地面アイテムピックアップ反映の改善 | バグ修正 | ✅ 完了 |
| 8 | Initialize内の初期ゴールド二重付与修正 | バグ修正 | ✅ 完了 |

---

## 詳細

### タスク1: 装備品のインベントリグリッド除外
- **問題**: 装備欄に装備された武器・防具がインベントリグリッド（持物欄）にも表示されており、矛盾のある状態
- **原因**: `InventoryWindow.PlaceItemsOnGrid()` が装備状態を考慮せずに全アイテムを描画
- **修正**:
  - `PlaceItemsOnGrid()` で `IsEquipped(item)` チェックを追加し、装備中アイテムをスキップ
  - `IsEquipped()` を全装備スロット（Head, Neck, Back, MainHand, Body, OffHand, Hands, Ring1, Waist, Ring2, Feet）に対応するよう拡張
  - `GameController.CanFitInGrid()` にもPlayer引数を追加し、装備品をグリッド容量計算から除外
- **変更ファイル**: `InventoryWindow.xaml.cs`, `GameController.cs`

### タスク2: スキルツリーのプレイヤー属性フィルタリング
- **問題**: すべての種族/職業/素性のスキルノードが表示され、選択内容に関係なく全ノードが見える
- **原因**: `SkillTreeWindow.RenderCurrentTab()` が `Tab` のみでフィルタし、`RequiredRace`/`RequiredClass`/`RequiredBackground` を無視
- **修正**: `RenderCurrentTab()` と `TreeCanvas_MouseDown()` の両方に `RequiredRace`/`RequiredClass`/`RequiredBackground` フィルタを追加
- **変更ファイル**: `SkillTreeWindow.xaml.cs`

### タスク3: WorldMap/MessageLogウィンドウのキートグル
- **問題**: J（世界マップ）/L（メッセージログ）キーでウィンドウを開けるが、同じキーで閉じられない
- **原因**: 各ウィンドウの `Window_KeyDown` にトグル閉じ処理がない
- **修正**:
  - `WorldMapWindow.Window_KeyDown` に `Key.J` → `Close()` を追加
  - `MessageLogWindow.Window_KeyDown` に `Key.L` → `Close()` を追加
- **変更ファイル**: `WorldMapWindow.xaml.cs`, `MessageLogWindow.xaml.cs`

### タスク4: ショップ購入/売却の選択バグ修正 + Ctrl+Click + D&D
- **問題**: 商品/アイテムをクリックしても選択ハイライトが消え、購入/売却ボタンが動作しない
- **原因**: `ShopGridCanvas_MouseDown` / `PlayerGridCanvas_MouseDown` 内で `RenderBothGrids()` を呼ぶと、`RenderShopGrid()` / `RenderPlayerGrid()` が `_selectedShopIndex = -1` / `_selectedPlayerIndex = -1` にリセットしてしまう
- **修正**:
  - `RenderShopGrid()` / `RenderPlayerGrid()` での選択インデックスリセットを削除
  - Ctrl+Click で即座に購入/売却するショートカットを追加
  - ショップ→プレイヤーグリッドへのD&D（購入）、プレイヤー→ショップグリッドへのD&D（売却）を追加
  - `ExecuteBuy()` / `ExecuteSell()` ヘルパーメソッドに共通化
- **変更ファイル**: `ShopWindow.xaml.cs`, `ShopWindow.xaml`

### タスク5: 人型モンスター撃破時の二重ゴールドログ修正
- **問題**: 人型モンスター撃破時にゴールド獲得ログが2回表示される
- **原因**: 撃破処理内の `CalculateGoldReward()` と `TryDropItem()` 内の `DropTableSystem.GenerateLoot()` の両方でゴールドが付与・ログ出力される
- **修正**: `DropTableId` がある敵の場合は `CalculateGoldReward()` をスキップし、`DropTableSystem` に一本化
- **変更ファイル**: `GameController.cs`

### タスク6: ミミック擬態対象の制限
- **問題**: ミミックがあらゆる地面アイテムに擬態する
- **修正**:
  - `TryPickupItem()` のミミック判定を宝箱タイル（`TileType.Chest`）上のアイテムのみに制限
  - `MimicSystem.GetDisguiseTypes()` を「宝箱, 木箱, 収納箱」の3種類に変更
- **変更ファイル**: `GameController.cs`, `MimicSystem.cs`

### タスク7: 地面アイテムピックアップ反映の改善
- **問題**: 一部アイテムを拾った際にインベントリに反映されない
- **原因**: `CanFitInGrid()` が装備中アイテムもグリッド占有として計算し、実際より空きが少なく判定されていた
- **修正**: タスク1の `CanFitInGrid()` 修正により装備品を除外、グリッド容量計算が正確に
- **変更ファイル**: `GameController.cs`（タスク1で修正済み）

### タスク8: Initialize内の初期ゴールド二重付与修正
- **問題**: テスト実行で初期ゴールドが期待の100Gではなく200Gになっていた
- **原因**: `Player.Create()` 内と `GameController.Initialize()` 内の両方で `bgDef.StartingGold` が付与されていた
- **修正**: `Initialize()` 内の重複付与コードを削除
- **変更ファイル**: `GameController.cs`

---

## ブラッシュアップ記録

Phase 14完了に伴い、以下のドキュメントを更新しました。

| 対象ドキュメント | 更新内容 |
|-----------------|----------|
| 13_GUIシステム設計書.md | ヘッダーにVer.prt.0.14追記、InventoryWindow装備品除外・ShopWindow Ctrl+Click/D&D・SkillTreeWindowフィルタリング・トグルキー一覧(J/L)更新 |
| 06_戦闘システム設計書.md | ゴールドドロップ仕様にDropTableId優先ルール（二重付与防止）を追記 |
| 21_拡張システム設計書.md | ミミック偽装種別を「宝箱型/木箱型/収納箱型（収納容器のみ）」に更新 |
| 11_クラス設計書.md | Phase 14の8項目を実装状況テーブルに追加 |
| 17_デバッグ・テスト設計書.md | テスト数5,557件で既に最新、追加更新なし |
| 10_プロジェクト構造設計書.md | テスト数5,557件で既に最新、構造変更なし |
| マスター実装計画書.md | Ver.prt.0.12/0.13/0.14セクション追加、フェーズ概要フロー更新、テーブル3行追加 |
| 13_GUIシステム設計書.md（追加） | ステータスバー色設計テーブルに構え/疲労/衛生/病気/季節/天候/渇き/カルマ/仲間数/スキルスロットの10項目を追記 |
| MainWindow.xaml.cs（追加） | DiseaseText病気なし時「健康」(LimeGreen)表示、罹患時「🤒病名」(OrangeRed)表示に修正（GUIオートテスト合格対応） |
| GuiAutomationTests.cs（追加） | ヘッダーコメントにDiseaseText健康時表示を明記 |

---

## 変更ファイル一覧

| ファイル | 変更内容 |
|---------|---------|
| `src/RougelikeGame.Gui/InventoryWindow.xaml.cs` | 装備品グリッド除外、IsEquipped全スロット対応 |
| `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs` | 種族/職業/素性フィルタリング追加 |
| `src/RougelikeGame.Gui/WorldMapWindow.xaml.cs` | Key.Jトグル閉じ追加 |
| `src/RougelikeGame.Gui/MessageLogWindow.xaml.cs` | Key.Lトグル閉じ追加 |
| `src/RougelikeGame.Gui/ShopWindow.xaml.cs` | 選択バグ修正、Ctrl+Click、D&D実装 |
| `src/RougelikeGame.Gui/ShopWindow.xaml` | MouseMove/Drop/DragOverイベント追加 |
| `src/RougelikeGame.Gui/GameController.cs` | 二重ゴールド修正、CanFitInGrid装備品除外、ミミック宝箱制限、初期ゴールド修正、IsItemEquipped追加 |
| `src/RougelikeGame.Core/Systems/MimicSystem.cs` | GetDisguiseTypes宝箱・収納容器のみに変更 |
| `tests/RougelikeGame.Core.Tests/SystemExpansionPhase6Tests.cs` | ミミックテスト更新 |
| `tests/RougelikeGame.Core.Tests/DungeonMechanicsTests.cs` | ミミックテスト閾値更新 |
| `src/RougelikeGame.Gui/MainWindow.xaml.cs` | DiseaseText病気なし時「健康」表示に修正 |
| `tests/RougelikeGame.Gui.Tests/GuiAutomationTests.cs` | ヘッダーコメント更新（DiseaseText健康時表示明記） |
