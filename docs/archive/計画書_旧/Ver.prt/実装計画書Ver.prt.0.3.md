# 実装計画書 Ver.prt.0.3（システム深化）

**目標**: Ver.prt.0.2で構築した基盤システムの上に、成長・戦闘・経済・環境の深みを追加
**状態**: ✅ 全27タスク完了 — テスト1,726件（Core）
**前提**: Ver.prt.0.2（基盤拡張14タスク）完了
**設計書参照**: [21_拡張システム設計書](../../企画設計書/21_拡張システム設計書.md)

---

## 1. 概要

Ver.prt.0.3 は Ver.prt.0.2 で構築した基盤（種族分類・属性・等級・熟練度・カルマ等）の上に、
ゲームプレイの深みを追加するフェーズ。戦闘・環境・経済・QoLの各方面でシステムを深化させる。

> **注**: 本バージョンで扱うのはシステム・仕組みの実装のみ。
> テキストコンテンツ（ストーリー・NPC会話内容・フレーバーテキスト等）は Ver.α で実装する。
> BGM/SE素材・グラフィック等のリソースは Ver.β で実装する。

---

## 2. 実装ロードマップ — ✅ 全27タスク完了

> 基盤の上に成長・戦闘・経済の深みを追加。

1. **P.8** スキルツリー拡張 ✅
2. **P.3** 方向・向きシステム ✅
3. **P.4** 装備耐久値システム ✅
4. **P.24** 能力値フラグシステム ✅
5. **P.25** フラグ選択肢変動システム ✅
6. **P.7** 状態異常の拡充 ✅
7. **P.17** 季節システム ✅
8. **P.18** 天候システム ✅（P.17季節と連動。採用確定）
9. **P.48** 採取システム ✅（P.9熟練度と連動。素材収集基盤）
10. **P.49** 釣りシステム ✅（P.48採取のサブシステム）
11. **P.51** 物品価値変動システム ✅（P.22カルマ/P.42評判と連動）
12. **P.52** 描画範囲外抑制・軽量化 ✅（パフォーマンス基盤）
13. **P.54** シンボルマップランダムイベント拡張 ✅
14. **P.56** 勢力地変動システム ✅（P.1/P.22と連動）
15. **P.35** マルチスロットセーブ ✅
16. **P.58** 環境利用戦闘システム ✅（P.45攻撃属性と連携。地表面×属性連鎖）
17. **P.59** 戦闘スタンス切替システム ✅（P.2武器種別と連携）
18. **P.60** 処刑・止めの一撃システム ✅（P.22カルマと連携）
19. **P.62** ダンジョン内派閥・生態系 ✅（P.1敵種族と連携）
20. **P.63** 隠し通路・シークレットルーム ✅（BSP法拡張。PER判定基盤）
21. **P.65** ミミック・偽装オブジェクト ✅（正気度/鑑定連動）
22. **P.71** 病気・疫病システム ✅（P.28身体状態と連携 ※0.3では簡易版で先行実装、0.4で統合）
23. **P.72** 睡眠・野営システム ✅（P.28身体状態(疲労度)と連携 ※同上）
24. **P.68** NPC日常行動ルーティン ✅（P.53時刻行動変化と連携）
25. **P.74** 知識図鑑・百科事典 ✅（TransferData連動。死に戻りテーマ強化）
26. **P.79** 自動探索・マクロ操作 ✅（探索QoL向上）
27. **P.80** 死因統計・死亡ログ ✅（死に戻りテーマ強化）

---

## 3. 実装記録

### 3.1 実装済みシステム一覧

| # | タスクID | システム名 | ファイル | テスト数 |
|---|---------|-----------|---------|---------|
| 1 | P.8 | スキルツリー拡張 | SkillTreeSystem.cs | 12 |
| 2 | P.3 | 方向・向きシステム | DirectionSystem.cs | 11 |
| 3 | P.4 | 装備耐久値システム | DurabilitySystem.cs | 14 |
| 4 | P.24 | 能力値フラグシステム | StatFlagSystem.cs | 8 |
| 5 | P.25 | フラグ選択肢変動システム | FlagConditionSystem.cs | 14 |
| 6 | P.17 | 季節システム | SeasonSystem.cs | 15 |
| 7 | P.18 | 天候システム | WeatherSystem.cs | 15 |
| 8 | P.7 | 状態異常の拡充 | ExtendedStatusEffectSystem.cs | 8 |
| 9 | P.48 | 採取システム | GatheringSystem.cs | 6 |
| 10 | P.49 | 釣りシステム | FishingSystem.cs | 6 |
| 11 | P.51 | 物品価値変動システム | PriceFluctuationSystem.cs | 5 |
| 12 | P.59 | 戦闘スタンス切替 | CombatStanceSystem.cs | 4 |
| 13 | P.60 | 処刑・止めの一撃 | ExecutionSystem.cs | 4 |
| 14 | P.58 | 環境利用戦闘 | EnvironmentalCombatSystem.cs | 5 |
| 15 | P.62 | ダンジョン内派閥 | DungeonFactionSystem.cs | 4 |
| 16 | P.63 | 隠し通路・シークレットルーム | SecretRoomSystem.cs | 3 |
| 17 | P.65 | ミミック・偽装オブジェクト | MimicSystem.cs | 4 |
| 18 | P.71 | 病気・疫病システム | DiseaseSystem.cs | 3 |
| 19 | P.72 | 睡眠・野営システム | RestSystem.cs | 6 |
| 20 | P.68 | NPC日常行動ルーティン | NpcRoutineSystem.cs | 4 |
| 21 | P.74 | 知識図鑑・百科事典 | EncyclopediaSystem.cs | 2 |
| 22 | P.80 | 死因統計・死亡ログ | DeathLogSystem.cs | 3 |
| 23 | P.56 | 勢力地変動システム | TerritoryInfluenceSystem.cs | 2 |
| 24 | P.35 | マルチスロットセーブ | MultiSlotSaveSystem.cs | 3 |
| 25 | P.54 | シンボルマップランダムイベント | SymbolMapEventSystem.cs | 4 |
| 26 | P.52 | 描画範囲外抑制 | RenderOptimizationSystem.cs | 5 |
| 27 | P.79 | 自動探索・マクロ操作 | AutoExploreSystem.cs | 5 |

### 3.2 追加された Enum 一覧

| Enum名 | 値数 | 定義ファイル |
|--------|------|-------------|
| SkillNodeType | 5 | Enums.cs |
| AttackDirection | 3 | Enums.cs |
| DurabilityStage | 5 | Enums.cs |
| StatFlag | 9 | Enums.cs |
| FlagConditionType | 7 | Enums.cs |
| Season | 4 | Enums.cs |
| Weather | 5 | Enums.cs |
| CombatStance | 3 | Enums.cs |
| GatheringType | 5 | Enums.cs |
| DiseaseType | 5 | Enums.cs |
| SleepQuality | 4 | Enums.cs |
| EncyclopediaCategory | 4 | Enums.cs |

### 3.3 テスト統計

- **Ver.prt.0.2 完了時**: Core 1,503件 + GUI 139件 = 1,642件
- **Ver.prt.0.3 完了時**: Core 1,726件 + GUI 139件 = 1,865件
- **追加テスト数**: 223件（Core のみ）
