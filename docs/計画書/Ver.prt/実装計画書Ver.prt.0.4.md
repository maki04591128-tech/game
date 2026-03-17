# 実装計画書 Ver.prt.0.4（大型新機能）

**目標**: パーティ/仲間/拠点/エンドコンテンツ等の大型システムを実装
**状態**: ✅ 全24タスク完了 — テスト2,061件（Core）
**前提**: Ver.prt.0.3（システム深化27タスク）完了
**設計書参照**: [21_拡張システム設計書](../../企画設計書/21_拡張システム設計書.md)

---

## 1. 概要

Ver.prt.0.4 は Ver.prt.0.3 までのシステム基盤・深化の上に、
パーティ/仲間/拠点/エンドコンテンツという大きなシステム変更を加えるフェーズ。

> **注**: 本バージョンで扱うのはシステム・仕組みの実装のみ。
> テキストコンテンツ（ストーリー・NPC会話内容・フレーバーテキスト等）は Ver.α で実装する。
> BGM/SE素材・グラフィック等のリソースは Ver.β で実装する。

---

## 2. 実装ロードマップ — ✅ 全24タスク完了

> パーティ/仲間/拠点という大きなシステム変更。

1. **P.27** 仲間・傭兵システム ✅
2. **P.23** 関係値システム拡張 ✅
3. **P.10** スキル合成システム ✅
4. **P.28** 身体状態システム ✅
5. **P.47** 拠点作成システム ✅（仲間システムと連動。建設・防衛）
6. **P.21** 具体的マップ作製基盤 ✅
7. **P.40** 罠作成システム ✅（P.9熟練度と連動。採用確定）
8. **P.29** 料理・調合システム拡張 ✅（P.9熟練度と連動。採用確定）
9. **P.57** グリッドインベントリ ✅（UI大型変更。EFT参考）
10. **P.30** ゲームオーバー後のタイトル画面戻り ✅
11. **P.31** ゲームクリア画面 ✅
12. **P.61** 環境パズル・仕掛けシステム ✅（ルーン語/属性パズル）
13. **P.64** ダンジョンショートカット永続開通 ✅（テンプレートマップ+TransferData）
14. **P.66** 誓約・縛りプレイシステム ✅（宗教連動。難易度カスタマイズ）
15. **P.67** マルチクラス・転職システム ✅（サブクラス/上位職）
16. **P.69** NPCの記憶・プレイヤー認知 ✅（噂伝播システム）
17. **P.70** 闇市場・裏社会ネットワーク ✅（P.22カルマ/P.42評判と連携）
18. **P.73** 飢餓・渇きシステム ✅（P.71病気と連携）
19. **P.76** 投資・出資システム ✅（P.51物品価値変動と連携）
20. **P.78** 密輸・禁制品取引システム ✅（P.70闘市場と連携）
21. **P.81** モジュラーHUD ✅（HUDカスタマイズUI）
22. **P.83** 無限ダンジョン・深層チャレンジ ✅（エンドコンテンツ）
23. **P.84** New Game+ ✅（エンドコンテンツ。NG+引き継ぎ）
24. **P.77** 賭博・ギャンブルシステム ✅（ミニゲーム基盤）

---

## 3. 実装記録

### 3.1 実装済みシステム一覧

| # | タスクID | システム名 | ファイル | テスト数 |
|---|---------|-----------|---------|---------|
| 1 | P.27 | 仲間・傭兵システム | CompanionSystem.cs | 6 |
| 2 | P.23 | 関係値システム拡張 | RelationshipSystem.cs | 6 |
| 3 | P.10 | スキル合成システム | SkillFusionSystem.cs | 6 |
| 4 | P.28 | 身体状態システム | BodyConditionSystem.cs | 5 |
| 5 | P.47 | 拠点作成システム | BaseConstructionSystem.cs | 5 |
| 6 | P.21 | 具体的マップ作製基盤 | TemplateMapSystem.cs | 5 |
| 7 | P.40 | 罠作成システム | TrapCraftingSystem.cs | 5 |
| 8 | P.29 | 料理・調合システム拡張 | CookingSystem.cs | 5 |
| 9 | P.57 | グリッドインベントリ | GridInventorySystem.cs | 6 |
| 10 | P.30 | ゲームオーバー後のタイトル画面戻り | GameOverSystem.cs | 5 |
| 11 | P.31 | ゲームクリア画面 | GameClearSystem.cs | 4 |
| 12 | P.61 | 環境パズル・仕掛けシステム | EnvironmentalPuzzleSystem.cs | 4 |
| 13 | P.64 | ダンジョンショートカット永続開通 | DungeonShortcutSystem.cs | 5 |
| 14 | P.66 | 誓約・縛りプレイシステム | OathSystem.cs | 5 |
| 15 | P.67 | マルチクラス・転職システム | MultiClassSystem.cs | 6 |
| 16 | P.69 | NPCの記憶・プレイヤー認知 | NpcMemorySystem.cs | 5 |
| 17 | P.70 | 闇市場・裏社会ネットワーク | BlackMarketSystem.cs | 5 |
| 18 | P.73 | 飢餓・渇きシステム | ThirstSystem.cs | 5 |
| 19 | P.76 | 投資・出資システム | InvestmentSystem.cs | 5 |
| 20 | P.78 | 密輸・禁制品取引システム | SmugglingSystem.cs | 5 |
| 21 | P.81 | モジュラーHUD | ModularHudSystem.cs | 5 |
| 22 | P.83 | 無限ダンジョン・深層チャレンジ | InfiniteDungeonSystem.cs | 5 |
| 23 | P.84 | New Game+ | NewGamePlusSystem.cs | 5 |
| 24 | P.77 | 賭博・ギャンブルシステム | GamblingSystem.cs | 6 |

### 3.2 追加された Enum 一覧

| Enum名 | 値数 | 定義ファイル |
|--------|------|-------------|
| CompanionType | 3 | Enums.cs |
| CompanionAIMode | 4 | Enums.cs |
| RelationshipType | 4 | Enums.cs |
| BodyWoundType | 5 | Enums.cs |
| FatigueLevel | 5 | Enums.cs |
| HygieneLevel | 5 | Enums.cs |
| FacilityCategory | 7 | Enums.cs |
| TemplateMapType | 5 | Enums.cs |
| PlayerTrapType | 5 | Enums.cs |
| CookingMethod | 5 | Enums.cs |
| GridItemSize | 5 | Enums.cs |
| PuzzleType | 3 | Enums.cs |
| OathType | 5 | Enums.cs |
| ClassTier | 3 | Enums.cs |
| RumorType | 4 | Enums.cs |
| BlackMarketCategory | 4 | Enums.cs |
| ThirstLevel | 4 | Enums.cs |
| WaterQuality | 4 | Enums.cs |
| InvestmentType | 3 | Enums.cs |
| ContrabandType | 4 | Enums.cs |
| HudElement | 5 | Enums.cs |
| InfiniteDungeonTier | 4 | Enums.cs |
| NewGamePlusTier | 5 | Enums.cs |
| GamblingGameType | 3 | Enums.cs |

### 3.3 テスト統計

- **Ver.prt.0.3 完了時**: Core 1,726件 + GUI 139件 = 1,865件
- **Ver.prt.0.4 完了時**: Core 1,907件 + GUI 139件 = 2,046件
- **テスト全システムカバレッジ補完後**: Core 2,061件 + GUI 139件 = 2,200件
- **追加テスト数**: 335件（Core のみ、Ver.prt.0.3完了時から）

### 3.4 ドキュメントブラッシュアップ記録

| # | 対象 | 内容 |
|---|------|------|
| D.130 | 実装計画書Ver.prt.0.4 | Ver.prt.1.0→Ver.prt.0.4名称変更。24タスク全完了 |
| D.131 | 21_拡張システム設計書 | Ver.prt.1.0→Ver.prt.0.4名称変更（19箇所） |
| D.132 | マスター実装計画書 | ロードマップ更新（0.2→0.3→0.4→2.0の4段階） |
| D.133 | 00_ドキュメント概要 | テスト数更新（Core1907+GUI139=2046件）、計画書状態更新 |
| D.134 | テスト全カバレッジ補完 | 18システムの不足テスト154件追加（Core2061件）。全システムにテスト対応完了 |
| D.135 | GUI統合 | GameControllerに新システム10種統合。ステータスバーに季節/天候/渇き/カルマ/仲間数追加。新画面遷移6種（図鑑/死亡録/スキルツリー/仲間/料理/拠点）。StatusWindowに身体状態・環境セクション追加。キーバインドY/U/Z追加 |
| D.136 | GUIオートテスト更新 | GuiAutomationTests: ステータスバー15→20要素、ダイアログキーY/U/Z追加、連打耐性15→18種。GuiSystemVerificationTests: 季節/天候/渇き/カルマ/仲間数の値レベル検証追加。ヘッダーコメント最新化 |
| D.137 | 実装漏れ確認＋ドキュメント最新化 | 19_GUIオートテスト説明書.md本文をコード実態に整合（カテゴリ3に5要素追加、カテゴリ4に13キー追加、連打耐性11→18種、カテゴリ6新設、検証項目数74→62に修正）。00_ドキュメント概要の検証項目数更新 |
| D.138 | ドキュメント全体漏れ補完 | 17_デバッグ・テスト設計書: テスト数980→2061件、ファイル数19→87、GUIオート37→38検証項目、システム検証37→24検証項目、テスト一覧68ファイル追加。13_GUIシステム設計書: キーバインドY/U/Z追加。20_GUI画面遷移図: 新ウィンドウ6種追加（18→24）。10_プロジェクト構造設計書: テスト数/Systems説明更新。11_クラス設計書: Ver.prt.0.2-0.4新クラス14件追加 |
| D.139 | ドキュメント全体精査 | 10_プロジェクト構造設計書: 表1.2のxUnitテスト数1,119→2,200件に修正。全21設計書+6計画書の数値・記述の整合性を精査し、D.138で漏れた表1.2の古い数値を修正完了 |
| D.140 | 全体ブラッシュアップ最終精査 | 00_ドキュメント概要: フォルダ構成図のVer.1~1.9/に「未着手・ディレクトリ未作成」注釈追加。全21設計書+6計画書+コード実態を網羅精査し、数値・記述の完全一致を確認完了 |
| D.141 | XMLコメント警告修正 | Enums.cs/GamblingSystem.csのCS1570警告修正（ハイ&amp;ロー→ハイ&amp;amp;ロー）。GuiSystemVerificationTests.csのCS1570修正（Shift+&amp;lt;/&amp;gt;エスケープ）。ビルド警告ゼロ+テスト2,061件全合格+ドキュメント全27ファイル整合性再確認完了 |
