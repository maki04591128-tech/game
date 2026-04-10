# 実装計画書 Ver.prt.0.26 — バグ修正・整合性改善

## 概要

コードベース全体を調査し、以下のバグ・整合性問題を修正する。

- CS8524コンパイラ警告（switch式の網羅性不足）
- 旧enum（FatigueLevel/ThirstLevel/HygieneLevel）と新enum（FatigueStage/ThirstStage/HygieneStage）の共存による混乱
- テストコードの未使用変数警告（CS0219/CS0169）

## Phase 1: CS8524警告修正（タスク1〜4）

### 問題

tuple型switch式で、enumの全値をパターンマッチしていても、「unnamed enum value」（enumにキャストされた未定義整数値）がカバーされていないため警告が発生。

### タスク一覧

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.1 | StatFlagEventData.cs | 17 | GetStatFlagNpcReaction switch式にデフォルトケース`_ =>`追加 | ✅完了 |
| B.2 | KarmaRelatedData.cs | 20 | GetKarmaReaction switch式にデフォルトケース`_ =>`追加 | ✅完了 |
| B.3 | SeasonalEventData.cs | 17 | GetSeasonalNpcGreeting switch式にデフォルトケース`_ =>`追加 | ✅完了 |
| B.4 | SeasonalEventData.cs | 70 | GetSeasonAtmosphereText switch式にデフォルトケース`_ =>`追加 | ✅完了 |

### 修正方針

各switch式の最後に以下のようなデフォルトケースを追加：
```csharp
_ => "（デフォルト反応テキスト）"
```

## Phase 2: 旧enum廃止マーキング（タスク5〜7）

### 問題

`FatigueLevel`（5段階）、`ThirstLevel`（4段階）、`HygieneLevel`（5段階）が`FatigueStage`（8段階）、`ThirstStage`（9段階）、`HygieneStage`（5段階）と共存している。GameController.csは全て新enum（Stage系）を使用済みだが、旧enumとそのオーバーロードメソッドが残存している。

### タスク一覧

| タスクID | 対象 | 内容 | ステータス |
|----------|------|------|------------|
| B.5 | Enums.cs | FatigueLevel/ThirstLevel/HygieneLevel に`[Obsolete]`属性追加 | ✅完了 |
| B.6 | BodyConditionSystem.cs | 旧enumオーバーロード（GetFatigueModifier(FatigueLevel)等）に`[Obsolete]`属性追加 | ✅完了 |
| B.7 | ThirstSystem.cs | 旧enumオーバーロード（GetThirstModifiers(ThirstLevel)等）に`[Obsolete]`属性追加 | ✅完了 |

### 修正方針

- 旧enumとメソッドを削除せず、`[Obsolete("FatigueStage/ThirstStage/HygieneStageを使用してください")]`を付与
- テストコードの旧enum使用箇所に`#pragma warning disable CS0618`を追加（既存テストの互換性維持）

## Phase 3: テストコード警告修正（タスク8〜10）

### 問題

テストコード内に未使用変数等の警告がある。

### タスク一覧

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.8 | BalanceAndTutorialTests.cs | 533 | 未使用変数`depth`の除去 | ✅完了 |
| B.9 | BalanceAndTutorialTests.cs | 548 | 未使用フィールド`_counter`の除去 | ✅完了 |
| B.10 | RandomEventSystemTests.cs | 83 | 未使用変数`foundTerritoryEvent`の除去 | ✅完了 |

## 実装順序

1. Phase 1（タスクB.1〜B.4）: CS8524警告修正
2. Phase 2（タスクB.5〜B.7）: 旧enum廃止マーキング
3. Phase 3（タスクB.8〜B.10）: テストコード警告修正
4. ビルド・テスト全合格確認
5. ドキュメントブラッシュアップ

## テスト方針

- 全既存テスト（Core.Tests 6,353件）が引き続き合格すること
- 旧enumテストは`#pragma warning disable`で警告を抑制しつつ維持
- 新規テスト追加は不要（既存テストで十分カバー）

## 実装結果

### ビルド結果
- **警告: 0件**（修正前: CS8524×4件、CS0219×1件、CS0169×1件 = 計6件の実コード警告を解消）
- **エラー: 0件**

### テスト結果
- **Core.Tests: 6,353件全合格**（Passed: 6,353, Failed: 0, Skipped: 0）

### 修正ファイル一覧
| ファイル | 変更内容 |
|---------|---------|
| src/RougelikeGame.Core/Data/StatFlagEventData.cs | switch式にデフォルトケース追加 |
| src/RougelikeGame.Core/Data/KarmaRelatedData.cs | switch式にデフォルトケース追加 |
| src/RougelikeGame.Core/Data/SeasonalEventData.cs | switch式にデフォルトケース追加（2箇所） |
| src/RougelikeGame.Core/Enums/Enums.cs | FatigueLevel/ThirstLevel/HygieneLevel に[Obsolete]追加 |
| src/RougelikeGame.Core/Systems/BodyConditionSystem.cs | 旧enumメソッド4件に[Obsolete]追加 |
| src/RougelikeGame.Core/Systems/ThirstSystem.cs | 旧enumメソッド3件に[Obsolete]追加 |
| tests/RougelikeGame.Core.Tests/BalanceAndTutorialTests.cs | 未使用変数depth・_counter除去 |
| tests/RougelikeGame.Core.Tests/RandomEventSystemTests.cs | 未使用変数foundTerritoryEvent除去 |
| tests/RougelikeGame.Core.Tests/BodyConditionSystemTests.cs | #pragma warning disable CS0618追加 |
| tests/RougelikeGame.Core.Tests/ThirstSystemTests.cs | #pragma warning disable CS0618追加 |
| tests/RougelikeGame.Core.Tests/VersionPrt017SystemTests.cs | #pragma warning disable CS0618追加 |
| tests/RougelikeGame.Core.Tests/VersionPrt019SystemTests.cs | #pragma warning disable CS0618追加 |
| tests/RougelikeGame.Core.Tests/SystemExpansionPhase5Tests.cs | #pragma warning disable CS0618追加 |
| tests/RougelikeGame.Core.Tests/SystemExpansionPhase6Tests.cs | #pragma warning disable CS0618追加 |
| tests/RougelikeGame.Core.Tests/SystemExpansionPhase8Tests.cs | #pragma warning disable CS0618追加 |
