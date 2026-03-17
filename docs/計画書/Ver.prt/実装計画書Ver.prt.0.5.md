# 実装計画書 Ver.prt.0.5（ゲーム完走フロー確立）

**目標**: Ver.prt.1.0（とりあえず最後まで遊べる状態）に向けて、ゲーム開始→ダンジョン探索→ボス戦→クリア→NG+の一連フローを完走可能にする
**状態**: ✅ 完了
**前提**: Ver.prt.0.4（全65タスク）完了済み — テスト2,061件（Core）+ 139件（GUI）= 2,200件
**完了時テスト数**: Core 2,218件 + GUI 139件 = 2,357件（新規111件追加）
**設計書参照**: [21_拡張システム設計書](../../企画設計書/21_拡張システム設計書.md)

---

## 1. 概要

Ver.prt.0.5 は Ver.prt.0.4 までに実装した全81システムを土台に、
「ゲームを最初から最後まで一通り遊べる状態」（Ver.prt.1.0）に到達するための
**ゲーム完走フロー確立**フェーズである。

### 1.1 Ver.prt.1.0 の定義

「とりあえず最後まで遊べる状態」とは以下を満たす状態を指す：

1. **ゲーム開始**: タイトル画面→キャラクター作成→ダンジョン1階に降り立てる
2. **探索ループ**: 各階を探索し、敵を倒し、アイテムを拾い、レベルアップしながら進める
3. **ボス戦**: 5階ごとのボスフロアにボス敵が配置され、ボス戦が発生する
4. **ゲームクリア**: 30階の最終ボスを撃破するとゲームクリアとなり、クリア画面が表示される
5. **ゲームオーバー**: 死亡時に死に戻り/タイトル画面戻り/終了の選択肢が機能する
6. **NG+（周回プレイ）**: クリア後にNG+で強化された2周目を開始できる
7. **システム統合**: 主要システム（仲間・料理・転職等）がGameControllerに接続され、ゲーム中に利用可能

### 1.2 現状分析（Ver.prt.0.4完了時点）

#### ✅ 完成済み（そのまま利用可能）

| 要素 | 状態 | 備考 |
|------|------|------|
| ダンジョン生成（BSP法） | ✅ 完全動作 | 30階層対応、ボス部屋生成あり |
| 敵配置・難易度スケーリング | ✅ 完全動作 | 階層別敵数/ステータス/タイプ変化 |
| 戦闘システム | ✅ 完全動作 | 物理/魔法/属性/状態異常/処刑 |
| 死に戻りシステム | ✅ 完全動作 | 正気度判定・知識引き継ぎ |
| セーブ/ロード | ✅ 完全動作 | マルチスロット対応 |
| タイトル画面 | ✅ 完全動作 | ニューゲーム/コンティニュー/設定/終了 |
| ターンシステム | ✅ 完全動作 | 優先度キュー・ゲーム内時刻管理 |
| 各種拡張システム（65個） | ✅ ロジック実装済み | Enum/レコード/メソッド定義完了 |

#### ❌ 未実装・不完全（本バージョンで対応）

| 要素 | 状態 | 問題点 |
|------|------|--------|
| ボス敵の配置 | ❌ 未実装 | ボス部屋は生成されるが、ボス敵が配置されない |
| ゲームクリア判定フロー | ⚠️ 不完全 | 30階到達でフラグを立てるのみ。ボス撃破判定なし |
| ゲームクリア画面表示 | ❌ 未実装 | GameClearSystemにテキスト定義はあるが、UI表示フローなし |
| ゲームオーバーメニュー | ⚠️ 不完全 | IsGameOver=trueのみ。タイトル戻り/終了の選択UIなし |
| NG+開始処理 | ❌ 未実装 | NewGamePlusSystemに定義はあるが、実行フローなし |
| 仲間の戦闘参加 | ❌ 未実装 | CompanionSystem管理機能のみ。AI行動・戦闘統合なし |
| 料理システム統合 | ❌ 未統合 | CookingSystemのGameController接続なし |
| スキル合成統合 | ❌ 未統合 | SkillFusionSystemのGameController接続なし |
| 転職システム統合 | ❌ 未統合 | MultiClassSystemのGameController接続なし |
| 拠点効果発動 | ❌ 未統合 | BaseConstructionSystemの施設効果がゲーム反映されない |
| クエスト自動進行 | ❌ 未実装 | 敵撃破/アイテム取得時のクエスト目標更新なし |
| メインクエストライン | ❌ 未定義 | ストーリー進行の骨組みなし |

> **注**: テキストコンテンツ（詳細ストーリー・NPC会話・フレーバーテキスト）は Ver.α の領域。
> 本バージョンではシステムの骨組み・フロー・接続のみを実装する。

---

## 2. 方針

- Ver.prt.0.4 の全システムが動作する状態を前提とする
- **ゲーム完走フローの確立**を最優先とする
- 既存システムの大幅な設計変更は避け、接続・統合・補完に注力する
- テキストコンテンツは最低限のプレースホルダーとし、Ver.αで本格実装
- リソース（グラフィック・BGM/SE）には手を出さない（Ver.βの領域）
- 全タスクにテストを付与し、テストカバレッジを維持する

---

## 3. タスク一覧

### Phase A: ゲームクリアフロー確立（必須・最優先）

> ゲームを最初から最後までプレイ可能にするための必須タスク。

| # | タスク名 | 内容 | 依存 | 優先度 |
|---|---------|------|------|--------|
| T.1 | ボス敵定義・配置システム | EnemyFactoryに6体のフロアボス（5階/10階/15階/20階/25階/30階）を定義。SpawnEnemies()でボスフロアのボス部屋にボス敵を配置。ボス固有ステータス・行動パターン | — | 🔴最高 |
| T.2 | ゲームクリア判定の完成 | 30階ボス撃破時にゲームクリアフラグを発火。GameClearSystemのスコア計算（ターン数/死亡数/ランク）を呼び出し、OnGameClearイベントを発行 | T.1 | 🔴最高 |
| T.3 | ゲームクリア画面表示フロー | OnGameClearイベントからクリア画面を表示。素性別クリアテキスト・スコア・ランク・プレイ統計を表示。NG+選択肢への遷移 | T.2 | 🔴最高 |
| T.4 | ゲームオーバーメニュー完成 | 死亡→死に戻り不可時のメニュー実装。「タイトル画面に戻る」「ゲーム終了」の選択肢をUI表示し、実際に機能させる | — | 🔴最高 |
| T.5 | NG+開始処理の実装 | クリア後のNG+選択→NewGamePlusSystemの引き継ぎデータ生成→新ゲーム開始。NG+段階に応じた敵強化/経験値倍率/引き継ぎ項目の適用 | T.3 | 🔴最高 |
| T.6 | メインクエストライン骨組み | 「ダンジョン最深部のボスを倒せ」をメインクエストとして定義。各ボスフロアをチェックポイントとしたクエスト進行。QuestSystemへの登録 | T.1 | 🟡高 |

### Phase B: 主要システムのGameController統合

> Ver.prt.0.4で実装したが、GameControllerに未接続のシステムを統合する。

| # | タスク名 | 内容 | 依存 | 優先度 |
|---|---------|------|------|--------|
| T.7 | 仲間の戦闘AI統合 | CompanionSystemの仲間がProcessEnemyTurns内で行動。AIMode別の行動決定（攻撃/防御/待機/追従）。仲間のHP管理・死亡処理 | — | 🟡高 |
| T.8 | 料理システムのGameController接続 | CookingSystemをGameControllerに接続。料理実行→素材消費→アイテム生成のフロー完成。既存OnShowCookingイベントとの統合 | — | 🟡高 |
| T.9 | スキル合成のGameController接続 | SkillFusionSystemをGameControllerに接続。スキル合成UI→素材チェック→合成実行→新スキル習得のフロー | — | 🟡高 |
| T.10 | 転職システムのGameController接続 | MultiClassSystemをGameControllerに接続。転職条件チェック→サブクラス選択→スキルツリー変更のフロー | — | 🟡高 |
| T.11 | 拠点施設効果の有効化 | BaseConstructionSystemの各施設（作業台/鍛冶場/宿舎等）が実際にゲーム内効果を発揮するよう接続。休息ボーナス・製作ボーナス等 | — | 🟡高 |

### Phase C: ゲームプレイ品質の向上

> ゲーム完走時の体験品質を高めるための補完タスク。

| # | タスク名 | 内容 | 依存 | 優先度 |
|---|---------|------|------|--------|
| T.12 | クエスト自動進行システム | 敵撃破/アイテム取得/フロア到達時にQuestSystemのObjective自動更新。クエスト完了判定・報酬自動付与 | T.6 | 🟡高 |
| T.13 | 無限ダンジョン解放統合 | ゲームクリア後にInfiniteDungeonSystemが解放。メニューからの開始フロー。スコア記録 | T.2 | 🟢中 |
| T.14 | ゲームバランス最終調整 | ボス敵HP/攻撃力、フロア別経験値、ドロップ率、レベルアップ曲線の総合調整。30階クリアまでの想定プレイ時間に基づく調整 | T.1 | 🟢中 |
| T.15 | 残存システム統合（P.13/P.26/P.75/P.82） | Ver.prt.2.0に分類されていた残タスクのうち、ゲーム完走に必要な部分を統合。P.13アイテムカテゴリ拡充、P.26 NPC拡充、P.75マルチED骨組み、P.82環境音基盤 | — | 🟢中 |

---

## 4. タスク詳細設計

### T.1 ボス敵定義・配置システム

**目的**: 5階ごとのボスフロアにフロアボスを配置し、ゲームに明確な目標と緊張感を与える

**実装内容**:
1. `EnemyFactory` に6体のフロアボス定義を追加
   - 5階ボス: 大型スライム（初見プレイヤー向け）
   - 10階ボス: ゴブリンキング（集団戦入門）
   - 15階ボス: スケルトンロード（不死系ボス）
   - 20階ボス: ダークエルフ将軍（高速型ボス）
   - 25階ボス: ドラゴン（高HP・高火力）
   - 30階ボス: 深淵の王（最終ボス）
2. `SpawnEnemies()` でボスフロア判定時にボス敵を配置
3. ボス固有ステータス（HP/攻撃力は通常敵の3〜5倍）
4. ボス撃破時の特別ドロップ・経験値ボーナス

**対象ファイル**: `EnemyFactory.cs`, `GameController.cs`
**テスト**: ボス定義の存在確認、ボスフロア判定、ボスステータス検証

### T.2 ゲームクリア判定の完成

**目的**: 30階ボス撃破をゲームクリア条件として確立する

**実装内容**:
1. 30階ボス（深淵の王）撃破を検知するロジック追加
2. `GameClearSystem.CalculateScore()` のスコア計算呼び出し
3. `OnGameClear` イベントの定義と発行
4. クリアランク判定（S/A/B/C/D）

**対象ファイル**: `GameController.cs`, `GameClearSystem.cs`
**テスト**: クリア条件判定、スコア計算、ランク判定

### T.3 ゲームクリア画面表示フロー

**目的**: クリア後にスコアと統計を表示し、NG+への導線を作る

**実装内容**:
1. `GameController` に `OnGameClear` イベント追加
2. クリア画面に表示する情報: 素性別クリアテキスト、ターン数、死亡回数、クリアランク、プレイ時間
3. クリア画面からの遷移: 「NG+で続ける」/「タイトルに戻る」
4. NG+ランク解放条件の表示

**対象ファイル**: `GameController.cs`, `GameClearSystem.cs`
**テスト**: クリアイベント発行、クリアテキスト取得、遷移フロー

### T.4 ゲームオーバーメニュー完成

**目的**: 死亡→死に戻り不可時のユーザー体験を完成させる

**実装内容**:
1. `GameOverSystem` に選択肢処理メソッド追加
2. 「タイトル画面に戻る」選択時の状態リセット処理
3. 「ゲーム終了」選択時のアプリケーション終了処理
4. `GameController` の `OnGameOver` イベント経由でメニュー表示

**対象ファイル**: `GameController.cs`, `GameOverSystem.cs`
**テスト**: ゲームオーバー判定、選択肢処理、状態リセット

### T.5 NG+開始処理の実装

**目的**: クリア後の周回プレイを可能にする

**実装内容**:
1. `NewGamePlusSystem.StartNewGamePlus(tier)` の実装
2. 引き継ぎデータ生成（レベル/装備/ゴールド/知識/スキル）
3. NG+段階に応じた敵強化倍率の適用
4. `GameController.InitializeNewGamePlus()` の追加
5. 新しいゲーム開始時にNG+データを適用

**対象ファイル**: `GameController.cs`, `NewGamePlusSystem.cs`
**テスト**: NG+段階判定、引き継ぎデータ生成、敵強化倍率

### T.6 メインクエストライン骨組み

**目的**: プレイヤーに明確な目標を提示する

**実装内容**:
1. メインクエスト「深淵の探索」の定義（QuestSystem登録）
2. 各ボスフロアをチェックポイントとした6段階の目標
3. ボス撃破時のクエスト進行通知
4. クエストログでの進捗表示

**対象ファイル**: `NpcSystem.cs`（QuestSystem部分）, `GameController.cs`
**テスト**: メインクエスト定義、進行段階、完了判定

### T.7 仲間の戦闘AI統合

**目的**: 仲間がパーティメンバーとして戦闘に参加する

**実装内容**:
1. `CompanionSystem` に `ProcessCompanionTurns()` メソッド追加
2. AIMode別行動決定: Attack→最寄り敵攻撃、Defend→プレイヤー隣接待機、Follow→追従、Free→自由行動
3. 仲間のダメージ計算・HP管理
4. 仲間死亡時の処理（パーティ離脱・復活条件）
5. `GameController.ProcessEnemyTurns()` 内で仲間行動を呼び出し

**対象ファイル**: `CompanionSystem.cs`, `GameController.cs`
**テスト**: 仲間行動AI、ダメージ計算、死亡処理

### T.8 料理システムのGameController接続

**目的**: 料理システムをゲーム中に利用可能にする

**実装内容**:
1. `GameController` に `TryCook(string recipeId)` メソッド追加
2. 素材チェック→消費→料理アイテム生成のフロー
3. CookingSystem.FindRecipe() とInventoryの連携
4. 料理実行時のメッセージ表示

**対象ファイル**: `GameController.cs`, `CookingSystem.cs`
**テスト**: 料理実行フロー、素材消費、アイテム生成

### T.9 スキル合成のGameController接続

**目的**: スキル合成をゲーム中に利用可能にする

**実装内容**:
1. `GameController` に `TryFuseSkills(string skillA, string skillB)` メソッド追加
2. SkillFusionSystem.CanFuse() によるチェック
3. 合成実行→新スキル習得のフロー
4. 合成結果のメッセージ表示

**対象ファイル**: `GameController.cs`, `SkillFusionSystem.cs`
**テスト**: 合成条件チェック、合成実行、スキル習得

### T.10 転職システムのGameController接続

**目的**: 転職をゲーム中に実行可能にする

**実装内容**:
1. `GameController` に `TryClassChange(string classChangeId)` メソッド追加
2. MultiClassSystem.CanClassChange() によるチェック
3. サブクラス設定・スキル倍率適用
4. 転職結果のメッセージ表示

**対象ファイル**: `GameController.cs`, `MultiClassSystem.cs`
**テスト**: 転職条件チェック、転職実行、サブクラス適用

### T.11 拠点施設効果の有効化

**目的**: 拠点建設した施設がゲーム内効果を発揮する

**実装内容**:
1. `BaseConstructionSystem` の各施設に効果メソッド追加
2. 休息時の施設ボーナス（宿舎→HP回復量UP）
3. 製作時の施設ボーナス（鍛冶場→武器強化確率UP）
4. `GameController` で拠点にいる場合の効果適用

**対象ファイル**: `GameController.cs`, `BaseConstructionSystem.cs`
**テスト**: 施設効果計算、ボーナス適用

### T.12 クエスト自動進行システム

**目的**: ゲーム中のアクションでクエストが自動的に進行する

**実装内容**:
1. 敵撃破時に `QuestSystem.UpdateKillObjective()` 呼び出し
2. アイテム取得時に `QuestSystem.UpdateCollectObjective()` 呼び出し
3. フロア到達時に `QuestSystem.UpdateExploreObjective()` 呼び出し
4. クエスト完了時の自動報酬付与

**対象ファイル**: `GameController.cs`, `NpcSystem.cs`
**テスト**: 目標更新、完了判定、報酬付与

### T.13 無限ダンジョン解放統合

**目的**: ゲームクリア後のエンドコンテンツを機能させる

**実装内容**:
1. ゲームクリア後に `InfiniteDungeonSystem` 解放フラグ設定
2. タイトル画面から無限ダンジョンモードを選択可能に
3. 無限ダンジョン用の敵スケーリング適用
4. スコア記録機能

**対象ファイル**: `GameController.cs`, `InfiniteDungeonSystem.cs`
**テスト**: 解放条件、敵スケーリング、スコア記録

### T.14 ゲームバランス最終調整

**目的**: 30階クリアまでの適切な難易度曲線を確立する

**実装内容**:
1. ボスHP/攻撃力の階層別調整
2. フロア別経験値テーブルの調整
3. ドロップ率の最適化
4. レベルアップ曲線の微調整（30階クリア時想定レベル: 35-40）

**対象ファイル**: `GameConstants.cs`, `EnemyFactory.cs`
**テスト**: バランスパラメータ検証

### T.15 残存システム統合

**目的**: Ver.prt.2.0に分類されていた残タスクの骨組みを統合する

**実装内容**:
1. **P.13 アイテムカテゴリ拡充**: 素材/魂石/罠キット/修理道具/料理/書物/鍵/楽器の8新カテゴリ定義
2. **P.26 NPC・味方拡充基盤**: 新NPCタイプの定義追加
3. **P.75 マルチエンディング骨組み**: カルマ/死に戻り回数によるエンディング分岐条件定義
4. **P.82 環境音基盤**: 環境音切替のフックポイント追加（実音源はVer.β）

**対象ファイル**: 各システムファイル
**テスト**: 各定義の存在・条件判定テスト

---

## 5. 実装順序・依存関係

```
Phase A（ゲームクリアフロー）:
  T.1 ボス敵定義・配置
    ↓
  T.2 ゲームクリア判定 ──→ T.3 クリア画面表示 ──→ T.5 NG+開始処理
    ↓                                                    ↓
  T.6 メインクエスト骨組み                          T.13 無限ダンジョン解放
    ↓
  T.4 ゲームオーバーメニュー（T.1と独立、並行実装可）

Phase B（システム統合）: Phase A完了後
  T.7  仲間の戦闘AI統合
  T.8  料理システム接続
  T.9  スキル合成接続
  T.10 転職システム接続
  T.11 拠点施設効果
  （T.7〜T.11は相互に独立、並行実装可）

Phase C（品質向上）: Phase A完了後
  T.12 クエスト自動進行（T.6依存）
  T.14 ゲームバランス調整（T.1依存）
  T.15 残存システム統合（独立）
```

**推定実装順序**:
1. T.1 → T.2 → T.3 → T.5 （クリアフローの縦串を通す）
2. T.4 （並行でゲームオーバー側を完成）
3. T.6 → T.12 （クエスト系）
4. T.7〜T.11 （システム統合、並行可）
5. T.13〜T.15 （仕上げ）

---

## 6. テスト方針

- 各タスクに対応するユニットテストを作成
- 既存テスト（2,200件）の回帰テストを維持
- ゲーム完走フローの統合テスト（T.1〜T.5の一連フロー検証）を追加
- 想定追加テスト数: 150〜200件

---

## 7. Ver.prt.0.5 完了後の展望

### Ver.prt.1.0（とりあえず最後まで遊べる状態）への残タスク

Ver.prt.0.5完了後、以下が達成されていれば Ver.prt.1.0 と見なす：

- [x] ゲーム開始→30階クリアまでの完走が可能
- [x] ボス戦が全6フロアで発生
- [x] ゲームクリア画面とスコア表示
- [x] ゲームオーバーメニューの完全動作
- [x] NG+による周回プレイ
- [x] 主要システムのGameController統合

### Ver.α以降のロードマップ（変更なし）

| バージョン | 目標 |
|-----------|------|
| Ver.α.0.1〜2 | テキストコンテンツ充実（ストーリー/NPC会話/フレーバーテキスト） |
| Ver.β.0.1〜2 | リソース強化（グラフィック/BGM/SE/演出） |
| Ver.0.1〜0.9 | デバッグ・バランス調整・Steam対応 |
| Ver.1.0 | 正式リリース |

---

## 8. 実装記録

### 8.1 実装済みシステム一覧

| # | タスク | 対象ファイル | 実装内容 |
|---|--------|------------|----------|
| T.1 | ボス敵定義・配置 | EnemyFactory.cs, GameController.cs | フロアボス6体定義（5/10/15/20/25/30階）、SpawnEnemiesでボス配置 |
| T.2 | ゲームクリア判定 | GameClearSystem.cs, GameController.cs | IsFinalBossDefeated、CalculateScore、ClearScore record追加 |
| T.3 | クリア画面フロー | GameClearSystem.cs, GameController.cs | GetClearText、OnGameClear、HandleGameClear |
| T.4 | ゲームオーバーメニュー | GameOverSystem.cs, GameController.cs | ProcessChoice、GameOverActionResult、GetDeathCauseDetail |
| T.5 | NG+開始処理 | NewGamePlusSystem.cs, GameController.cs | NgPlusCarryOver、GetNextTier、InitializeNewGamePlus |
| T.6 | メインクエストライン | NpcSystem.cs, GameController.cs | RegisterMainQuest（6目標）、IsMainQuestComplete |
| T.7 | 仲間戦闘AI | CompanionSystem.cs, GameController.cs | ProcessCompanionTurns、DamageCompanion、HealCompanion |
| T.8 | 料理接続 | GameController.cs | TryCook（素材チェック/消費/品質計算） |
| T.9 | スキル合成接続 | GameController.cs | TryFuseSkills |
| T.10 | 転職接続 | GameController.cs | TryClassChange |
| T.11 | 拠点施設効果 | BaseConstructionSystem.cs, GameController.cs | FacilityBonus、GetTotalBonus、各種ボーナスメソッド |
| T.12 | クエスト自動進行 | NpcSystem.cs, GameController.cs | UpdateKillObjective、UpdateExploreObjective、UpdateCollectObjective |
| T.13 | 無限ダンジョン解放 | InfiniteDungeonSystem.cs, GameController.cs | CalculateScore、GetFloorDescription、StartInfiniteDungeon |
| T.14 | バランス調整 | GameConstants.cs | FloorBossHpMultiplier、FloorBossAttackMultiplier、BossExpBonus |
| T.15 | 残存統合 | Enums.cs, MultiEndingSystem.cs, AmbientSoundSystem.cs | ExtendedItemCategory(8種)、EndingType(5種)、AmbientSoundType(9種)、NPC6種追加 |

### 8.2 テスト統計

- **Ver.prt.0.4 完了時**: Core 2,061件 + GUI 139件 = 2,200件
- **Ver.prt.0.5 追加テスト**: 111件（GameCompletionFlowTests.cs）
- **Ver.prt.0.5 完了時**: Core 2,218件 + GUI 139件 = 2,357件

### 8.3 ドキュメントブラッシュアップ記録

| # | 対象 | 内容 |
|---|------|------|
| D.142 | 実装計画書Ver.prt.0.5 | 新規作成。ゲーム完走フロー確立のための15タスク定義 |
| D.143 | 実装計画書Ver.prt（マスター） | ロードマップにVer.prt.0.5追加 |
| D.144 | 00_ドキュメント概要 | Ver.prt.0.5計画書をドキュメント一覧に追加 |
| D.145 | 実装計画書Ver.prt.0.2 | Ver.prt.0.5へのリンク追加、ロードマップ更新 |
| D.146 | 17_デバッグ・テスト設計書 | テスト数2,061→2,218件、ファイル87→89。テスト一覧にGameCompletionFlowTests/SymbolMapTransitionTests追加。テストピラミッド更新 |
| D.147 | 10_プロジェクト構造設計書 | テスト数2,200→2,357件。Coreテスト2,061→2,218件/89ファイルに更新 |
| D.148 | 00_ドキュメント概要 | Ver.prt.0.5ステータス⬜→✅ 全15タスク完了 |
| D.149 | 11_クラス設計書 | Ver.prt.0.5新クラス4件追加（MultiEndingSystem/AmbientSoundSystem/SymbolMapGenerator/SymbolMapSystem） |
