# 実装計画書 Ver.prt.0.17 — バグ修正・バランス調整パッチ

**ステータス**: ✅ 全タスク完了
**目的**: ゲームプレイで発見された8つのバグ修正・パフォーマンス改善・バランス調整
**テスト結果**: 5,590テスト全合格（Core: 5,442 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | ショップ購入時にインベントリが満杯だとゴールドだけ消費される | バグ修正 | ✅ 完了 |
| 2 | 自動探索で探索完了後に下り階段へ移動しない | 機能改善 | ✅ 完了 |
| 3 | スキルツリーのノードがクリックで選択できない | バグ修正 | ✅ 完了 |
| 4 | スキルポイントがあってもスキルツリーの解放ボタンが無効のまま | バグ修正 | ✅ 完了 |
| 5 | 町での移動が異様に重い | パフォーマンス改善 | ✅ 完了 |
| 6 | 渇きの悪化速度が異様に速い | バランス調整 | ✅ 完了 |
| 7 | 疲労度の悪化速度が速すぎる | バランス調整 | ✅ 完了 |
| 8 | 宿屋で疲労度・衛生状態が回復しない | バグ修正 | ✅ 完了 |

---

## 詳細

### タスク1: ショップ購入時にインベントリが満杯だとゴールドだけ消費される
- **問題**: `ShopSystem.Buy()`がゴールドを消費した後に`Inventory.Add()`を呼ぶため、インベントリが満杯の場合にゴールドだけ失われアイテムが取得できない
- **根本原因**: `TryBuyItem()`で購入前のインベントリ容量チェックがなく、`Buy()`内のゴールド消費とアイテム追加の順序問題でロールバックもなかった
- **修正**:
  - **事前容量チェック**: 購入前にアイテムをプレビュー生成し、`IStackable`によるスタック可能性も考慮した空き容量チェックを追加
  - **フォールバック安全策**: 万一`Inventory.Add()`が失敗した場合のゴールド返金ロジックを追加
- **変更ファイル**: `GameController.cs`（TryBuyItem）

### タスク2: 自動探索で探索完了後に下り階段へ移動しない
- **問題**: 全タイルの探索が完了すると「探索する場所がない」と表示され、下り階段への自動移動が行われない
- **根本原因**: `StepAutoExplore()`で`FindNextExploreStep()`がnullを返した場合の処理が「メッセージ表示＋タイマー停止」のみで、階段への経路探索フォールバックがなかった
- **修正**:
  - **BFS経路探索**: `FindPathToStairs()`メソッドを新規実装。マップ上の`TileType.StairsDown`タイルへのBFS経路を探索
  - **フォールバック統合**: `StepAutoExplore()`で未探索タイルがない場合に階段への経路探索を実行し、経路が見つかれば自動移動を継続
  - **地上・ロケーション判定**: `_isInLocationMap`または地上階の場合は階段探索をスキップ（階段が存在しないため）
- **変更ファイル**: `GameController.cs`（StepAutoExplore、FindPathToStairs新規メソッド）

### タスク3: スキルツリーのノードがクリックで選択できない
- **問題**: スキルツリー画面でノードをクリックしても選択状態にならない
- **根本原因**: `TreeCanvas_MouseDown`イベントハンドラでクリック座標のY軸判定が`node.TreeY + offsetY`を使用していたが、描画処理は`maxTreeY - node.TreeY`でY軸反転していたため、クリック位置と表示位置が不一致
- **修正**: クリック判定のY座標を`(maxTreeY - node.TreeY) + offsetY`に修正し、描画と同一の座標系を使用
- **変更ファイル**: `SkillTreeWindow.xaml.cs`（TreeCanvas_MouseDown）

### タスク4: スキルポイントがあってもスキルツリーの解放ボタンが無効のまま
- **問題**: スキルポイントを保持しノードを選択しても「解放」ボタンが`IsEnabled=false`のまま
- **根本原因**: `RenderCurrentTab()`が毎回`ClearDetail()`を呼び出し、`UnlockButton.IsEnabled=false`にリセットしていた。ノード選択後のUI更新時にも`RenderCurrentTab()`が呼ばれるため、選択状態が即座にリセットされていた
- **修正**: `RenderCurrentTab()`で`_selectedNodeId`が設定済みかつ現在のタブに属するノードの場合、`ClearDetail()`をスキップし選択状態を復元するロジックを追加
- **変更ファイル**: `SkillTreeWindow.xaml.cs`（RenderCurrentTab）

### タスク5: 町での移動が異様に重い
- **問題**: 町マップでの移動が他のマップと比較して体感的に重い
- **根本原因**: `ProcessEnemyTurns()`が町マップ（ロケーションマップ）でも毎ターン実行されていた。敵が存在しない町でも全タイルを走査する処理が不要に実行されていた
- **修正**: `ProcessInput()`内の`ProcessEnemyTurns()`呼び出しに`if (!_isInLocationMap)`ガードを追加。ロケーションマップ（町）では敵ターン処理をスキップ
- **変更ファイル**: `GameController.cs`（ProcessInput）

### タスク6: 渇きの悪化速度が異様に速い
- **問題**: 渇きの悪化速度が満腹度の約3.3倍速い（180ターンごとvs満腹度600ターンごと）
- **根本原因**: `ProcessTurnEffects()`内の渇き進行判定が`_totalTurns % 180 == 0`とハードコードされていた
- **修正**: 渇き進行間隔を`TimeConstants.HungerDecayInterval`（600ターン）に統一
- **変更ファイル**: `GameController.cs`（ProcessTurnEffects 渇きセクション）

### タスク7: 疲労度の悪化速度が速すぎる
- **問題**: 疲労度の悪化速度が満腹度の2倍速い（300ターンごとvs満腹度600ターンごと）
- **根本原因**: `ProcessTurnEffects()`内の疲労進行判定が`_totalTurns % 300 == 0`とハードコードされていた
- **修正**: 疲労進行間隔を`TimeConstants.HungerDecayInterval`（600ターン）に統一
- **変更ファイル**: `GameController.cs`（ProcessTurnEffects 疲労セクション）

### タスク8: 宿屋で疲労度・衛生状態が回復しない
- **問題**: 宿屋で宿泊するとHP/MP/SPは回復するが、疲労度と衛生状態がリセットされない
- **根本原因**: `TryUseInn()`が`TownSystem.RestAtInn()`を呼び出してHP/MP/SP回復のみ処理し、`_playerFatigue`/`_playerHygiene`/`PlayerThirstLevel`のリセットを行っていなかった
- **修正**: `TryUseInn()`のRestAtInn成功後に以下を追加:
  - `_playerFatigue = FatigueLevel.Fresh`（疲労完全回復）
  - `_playerHygiene = HygieneLevel.Clean`（衛生状態完全回復）
  - `PlayerThirstLevel = ThirstLevel.Hydrated`（渇き完全回復）
- **変更ファイル**: `GameController.cs`（TryUseInn）

---

## テスト追加

| テストファイル | テスト数 | 内容 |
|--------------|---------|------|
| `VersionPrt017SystemTests.cs` | 18 | インベントリ容量×2、ショップ購入×1、スキルツリーCanUnlock/UnlockNode×6、渇き定数×2、疲労レベル×2、宿屋RestAtInn×3、衛生状態×2、ダンジョンマップ階段/FOV×2 |

### テスト内訳

| タスク | テスト名 | 内容 |
|-------|---------|------|
| 1 | `Inventory_Add_ReturnsFalse_WhenFull` | 満杯インベントリへの追加が失敗することを確認 |
| 1 | `Inventory_Add_StackableItem_SucceedsEvenWhenFull` | スタック可能アイテムは満杯でも追加可能 |
| 1 | `ShopSystem_Buy_ReturnsItemId_OnSuccess` | ショップ購入成功時にアイテムIDが返される |
| 3-4 | `SkillTreeSystem_CanUnlock_WithPoints_ReturnsTrue` | ポイント有＋条件満たすとCanUnlock=true |
| 3-4 | `SkillTreeSystem_CanUnlock_WithoutPoints_ReturnsFalse` | ポイントなしでCanUnlock=false |
| 3-4 | `SkillTreeSystem_CanUnlock_LevelTooLow_ReturnsFalse` | レベル不足でCanUnlock=false |
| 3-4 | `SkillTreeSystem_UnlockNode_DeductsPoints` | UnlockNodeでポイント消費＋解放記録 |
| 3-4 | `SkillTreeSystem_UnlockNode_WithPrerequisites_RequiresParent` | 前提ノード未解放だとCanUnlock=false |
| 3-4 | `SkillTreeSystem_AlreadyUnlocked_CannotUnlockAgain` | 解放済みノードの再解放不可 |
| 6 | `HungerDecayInterval_Equals600` | HungerDecayInterval定数=600 |
| 6 | `ThirstSystem_ThirstLevels_AreOrdered` | ThirstLevelの列挙順序確認 |
| 7 | `FatigueLevels_AreOrdered` | FatigueLevelの列挙順序確認 |
| 7 | `BodyConditionSystem_FatigueModifier_DecreasesWithLevel` | 疲労レベル上昇でモディファイア低下 |
| 8 | `RestAtInn_RestoresHP_MP_SP` | RestAtInnでHP全回復 |
| 8 | `RestAtInn_FailsWithoutGold` | ゴールド不足でRestAtInn失敗 |
| 8 | `RestAtInn_ReturnsTurnCost` | RestAtInnがターンコストを返す |
| 8 | `HygieneLevel_Clean_HasLowInfectionRisk` | Clean状態で感染リスクが低い |
| 8 | `HygieneLevel_Filthy_HasHighInfectionRisk` | Filthy状態で感染リスクが高い |
| 2 | `DungeonMap_StairsDown_IsAccessible` | StairsDownタイルが歩行可能 |
| 2 | `DungeonMap_ComputeFov_ExploresTiles` | ComputeFovでタイルがExplored状態になる |

---

## 変更ファイル一覧

| ファイル | 変更種別 | 概要 |
|---------|---------|------|
| `src/RougelikeGame.Gui/GameController.cs` | 修正 | タスク1,2,5,6,7,8: 購入前インベントリチェック＋返金、FindPathToStairs BFS新規メソッド、町ProcessEnemyTurnsスキップ、渇き600ターン化、疲労600ターン化、宿屋で疲労/衛生/渇き回復 |
| `src/RougelikeGame.Gui/SkillTreeWindow.xaml.cs` | 修正 | タスク3,4: TreeCanvas_MouseDown Y座標反転修正、RenderCurrentTab選択状態復元 |
| `tests/RougelikeGame.Core.Tests/VersionPrt017SystemTests.cs` | 新規 | Phase17テスト18件 |

---

## ブラッシュアップ記録

Phase 17全タスク完了後、プロジェクトガイドラインに従い自動ドキュメントブラッシュアップを実施。

### 更新対象ドキュメント

| ドキュメント | 更新内容 |
|-------------|---------|
| `docs/00_ドキュメント概要.md` | Ver.prt.0.17エントリ追加、フォルダ構成ツリーに0.17追加、バージョン範囲0.16→0.17更新、用語集にFindPathToStairs追加 |
| `docs/計画書/マスター実装計画書.md` | バージョン体系に0.13-0.17追加、マイルストーン詳細にVer.prt.0.17セクション追加、フロー図に0.17追加、開発フェーズ概要テーブルにVer.prt.0.17行追加 |
| `docs/企画設計書/11_クラス設計書.md` | 実装状況テーブルにPhase 17変更9項目追加（ショップ容量チェック/自動探索階段/ノード選択修正/解放ボタン修正/町パフォーマンス/渇き速度/疲労速度/宿屋回復拡張） |
| `docs/企画設計書/13_GUIシステム設計書.md` | 変更履歴にVer.prt.0.17追加（スキルツリーY座標修正・解放ボタン修正） |
| `docs/企画設計書/14_マップシステム設計書.md` | 変更履歴にVer.prt.0.17追加、自動探索テーブルに階段フォールバック行追加 |
| `docs/企画設計書/17_デバッグ・テスト設計書.md` | テスト総数5,570→5,590、Core 5,422→5,442に更新、テストピラミッド/プロジェクト一覧/Core.Tests内訳を最新化、VersionPrt017SystemTests.csをファイル一覧に追加 |
| `docs/企画設計書/10_プロジェクト構造設計書.md` | テスト数5,570→5,590、Core 5,422→5,442/135→136ファイルに更新 |
