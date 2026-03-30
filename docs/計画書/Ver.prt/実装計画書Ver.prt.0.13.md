# 実装計画書 Ver.prt.0.13 — 全システムGUI統合

**ステータス**: ✅ 全タスク完了
**目的**: ゲームとしてGUI上で動く必要がある全48システムをGameControllerに統合し、GUI上で動作確認が行える状態にする

---

## Phase 1: システム統合基盤

| # | タスク | ステータス |
|---|--------|-----------|
| 1 | 新システムフィールド宣言（ProficiencySystem, DungeonShortcutSystem, SmithingSystem, AchievementSystem, AccessibilitySystem, ContextHelpSystem, ModularHudSystem） | ✅ 完了 |
| 2 | プレイヤー状態変数追加（CombatStance, FatigueLevel, HygieneLevel, DiseaseType, DiseaseRemainingTurns） | ✅ 完了 |

## Phase 2: ターン効果統合 (ProcessTurnEffects)

| # | タスク | ステータス |
|---|--------|-----------|
| 3 | TimeOfDaySystem — 時間帯による視界範囲変動 | ✅ 完了 |
| 4 | BodyConditionSystem — 疲労蓄積（300ターン毎）・衛生低下（1200ターン毎） | ✅ 完了 |
| 5 | DiseaseSystem — 病気進行・自然回復・感染判定 | ✅ 完了 |
| 6 | DurabilitySystem — 装備耐久度警告（200ターン毎） | ✅ 完了 |
| 7 | NpcRoutineSystem — NPC行動スケジュール更新 | ✅ 完了 |
| 8 | ProficiencySystem — 未使用熟練度減衰（600ターン毎） | ✅ 完了 |
| 9 | WeatherSystem — 季節に応じた天候変化 | ✅ 完了 |
| 10 | ThirstSystem — 渇き進行（180ターン毎） | ✅ 完了 |
| 11 | AchievementSystem — マイルストーン実績チェック | ✅ 完了 |

## Phase 3: 戦闘統合

| # | タスク | ステータス |
|---|--------|-----------|
| 12 | CombatStanceSystem — スタンス攻撃/防御修飾 | ✅ 完了 |
| 13 | ElementalAffinitySystem — 属性ダメージ倍率 | ✅ 完了 |
| 14 | WeaponProficiencySystem — 武器スケーリングボーナス | ✅ 完了 |
| 15 | MonsterRaceSystem — モンスター種族特性参照 | ✅ 完了 |
| 16 | ExecutionSystem — 処刑判定・カルマ影響 | ✅ 完了 |
| 17 | DurabilitySystem — 武器/防具耐久度消耗 | ✅ 完了 |
| 18 | ProficiencySystem — 武器熟練度経験値獲得 | ✅ 完了 |
| 19 | TimeOfDaySystem — 敵活性度修飾（被ダメージ） | ✅ 完了 |

## Phase 4: 敵撃破・アイテム統合

| # | タスク | ステータス |
|---|--------|-----------|
| 20 | HarvestSystem — 素材収集 | ✅ 完了 |
| 21 | SecretRoomSystem — 秘密通路発見判定 | ✅ 完了 |
| 22 | DungeonEcosystemSystem — 生態系記録 | ✅ 完了 |
| 23 | PriceFluctuationSystem — 購入/売却価格変動 | ✅ 完了 |
| 24 | ItemGradeSystem — アイテム等級表示 | ✅ 完了 |

## Phase 5: アクティビティメソッド追加

| # | タスク | ステータス |
|---|--------|-----------|
| 25 | RestSystem — 野営 (TryCamp) | ✅ 完了 |
| 26 | GamblingSystem — 賭博 (TryGamble) | ✅ 完了 |
| 27 | FishingSystem — 釣り (TryFish) | ✅ 完了 |
| 28 | GatheringSystem — 採集 (TryGather) | ✅ 完了 |
| 29 | SmithingSystem — 鍛冶 (TrySmithEnhance/TrySmithRepair) | ✅ 完了 |
| 30 | EnchantmentSystem — エンチャント (TryEnchant) | ✅ 完了 |
| 31 | DungeonShortcutSystem — ショートカット (TryUnlockShortcut) | ✅ 完了 |
| 32 | SmugglingSystem — 密輸 (TrySmuggle) | ✅ 完了 |
| 33 | TrapCraftingSystem — 罠作成 (TryCraftTrap) | ✅ 完了 |
| 34 | BlackMarketSystem — 闇市場 (GetBlackMarketItems/TryBlackMarketBuy) | ✅ 完了 |
| 35 | EnvironmentalPuzzleSystem — パズル (TryAttemptPuzzle) | ✅ 完了 |

## Phase 6: プロパティ・アクセサ追加

| # | タスク | ステータス |
|---|--------|-----------|
| 36 | MultiEndingSystem — エンディング判定 | ✅ 完了 |
| 37 | GrowthSystem — レベルアップボーナス | ✅ 完了 |
| 38 | ReligionSkillSystem — 宗教スキル取得 | ✅ 完了 |
| 39 | DungeonFactionSystem — 派閥敵対判定 | ✅ 完了 |
| 40 | ExtendedStatusEffectSystem — 拡張バフ/デバフ | ✅ 完了 |
| 41 | ModularHudSystem — HUD管理 | ✅ 完了 |
| 42 | RenderOptimizationSystem — 描画最適化 | ✅ 完了 |
| 43 | TemplateMapSystem — テンプレートマップ | ✅ 完了 |
| 44 | SymbolMapEventSystem — シンボルマップイベント | ✅ 完了 |
| 45 | AutoExploreSystem — 自動探索停止条件 | ✅ 完了 |
| 46 | FlagConditionSystem — フラグ条件評価 | ✅ 完了 |
| 47 | StatFlagSystem — ステータスフラグ評価 | ✅ 完了 |

## Phase 7: GUI表示

| # | タスク | ステータス |
|---|--------|-----------|
| 48 | MainWindow.xaml — ステータスバー拡張（構え/疲労/衛生/病気） | ✅ 完了 |
| 49 | MainWindow.xaml.cs — Nキーでスタンス切替 | ✅ 完了 |
| 50 | MainWindow.xaml.cs — ステータスバー更新ロジック追加 | ✅ 完了 |

---

## 統合されたシステム一覧（全48件）

### ターン処理に統合
TimeOfDaySystem, BodyConditionSystem, DiseaseSystem, DurabilitySystem, NpcRoutineSystem, ProficiencySystem, WeatherSystem, ThirstSystem, AchievementSystem

### 戦闘に統合
CombatStanceSystem, ElementalAffinitySystem, WeaponProficiencySystem, MonsterRaceSystem, ExecutionSystem, DirectionSystem（参照のみ）, EnvironmentalCombatSystem（参照のみ）

### 敵撃破時に統合
HarvestSystem, SecretRoomSystem, DungeonEcosystemSystem

### 経済に統合
PriceFluctuationSystem, ItemGradeSystem

### アクティビティとして追加
RestSystem, GamblingSystem, FishingSystem, GatheringSystem, SmithingSystem, EnchantmentSystem, DungeonShortcutSystem, SmugglingSystem, TrapCraftingSystem, BlackMarketSystem, EnvironmentalPuzzleSystem

### プロパティ/アクセサとして公開
MultiEndingSystem, GrowthSystem, ReligionSkillSystem, DungeonFactionSystem, ExtendedStatusEffectSystem, ModularHudSystem, RenderOptimizationSystem, TemplateMapSystem, SymbolMapEventSystem, AutoExploreSystem, FlagConditionSystem, StatFlagSystem, AccessibilitySystem, ContextHelpSystem, MimicSystem（参照のみ）

---

## 新規キーバインド

| キー | アクション |
|------|-----------|
| N | 戦闘スタンス切替（均衡→攻撃→防御→均衡） |

## 新規ステータスバー項目

| 項目 | 表示内容 | 色変え |
|------|---------|--------|
| 構え | CombatStance名 | 攻撃=赤, 防御=青, 均衡=緑 |
| 疲労 | FatigueLevel名 | 元気=緑, 軽疲労=黄, 疲労=橙, 重疲労=赤 |
| 衛生 | HygieneLevel名 | 清潔=青, 普通=緑, 汚れ=黄, 不衛生=橙 |
| 病気 | DiseaseType名（罹患時のみ） | 赤 |

## テスト拡充Phase 8

**追加テスト**: SystemExpansionPhase8Tests.cs (287件)
**対象**: GUI統合ロジックパスの包括テスト (15システム)

| システム | テスト件数 | テスト内容 |
|---------|-----------|-----------|
| TimeOfDaySystem | 16件 | 時間帯判定/名前/視界修正/活動パターン/統計修正 |
| ProficiencySystem | 11件 | 経験値獲得/レベルアップ/減衰/武器カテゴリマッピング |
| ItemGradeSystem | 10件 | グレード情報/ステ倍率/価格倍率/ドロップ率/決定ロジック |
| EnvironmentalPuzzleSystem | 6件 | パズル取得/タイプ名/成功率/試行可否 |
| GamblingSystem | 14件 | サイコロ/丁半/ハイロー判定/配当/最低賭け金/中毒 |
| CombatStanceSystem | 11件 | 攻撃/防御/回避/クリティカル修正/名前/トレードオフ |
| BodyConditionSystem | 8件 | 疲労/衛生名前/修正値/感染リスク/傷 |
| DiseaseSystem | 5件 | 全病気取得/感染/自然回復/治療コスト |
| HarvestSystem | 4件 | 収穫可否/種族別/ランク別/アイテムリスト |
| WeaponProficiencySystem | 5件 | プロファイル/スケーリングボーナス/ダメージ計算 |
| ElementalAffinitySystem | 6件 | 耐性レベル/ダメージ倍率/武器攻撃タイプ/物理修正 |
| ExecutionSystem | 8件 | HP閾値/経験値/ドロップ/カルマ/アニメーション |
| 戦闘統合テスト | 7件 | スタンス+属性+疲労+処刑+時刻+熟練度連携 |
| ターン効果連携テスト | 5件 | 疲労進行/衛生進行/感染/価格変動/グレード価格 |
| アクティビティ連携テスト | 15件 | 野営/賭博/釣り/採集/鍛冶/ショートカット/密輸/闘市場 |

## テスト拡充Phase 9（最終フェーズ）

**追加テスト**: SystemExpansionPhase9Tests.cs (290件)
**対象**: 9システム統合・境界値・Enum網羅テスト

| システム | テスト件数 | テスト内容 |
|---------|-----------|-----------|
| WeatherSystem | 25件 | 天候×属性×季節複合条件/視界/命中/移動コスト/足跡/天候決定 |
| NpcSystem | 25件 | NPC定義/好感度/対話/クエスト受注/テリトリー別NPC/リセット |
| WorldMapSystem | 25件 | テリトリー移動/隣接判定/ロケーション/施設/宿屋/銀行 |
| ReligionSystem | 25件 | 信仰段階/祈祷/恩恵/タブー/入信/棄教/日次処理 |
| SkillSystem | 25件 | スキル習得/使用/クールダウン/スキルツリー/カテゴリ別 |
| PetSystem | 20件 | ペット追加/餌/訓練/騎乗/移動速度/空腹/服従率 |
| SymbolMapSystem | 15件 | テリトリー別生成/ロケーション種別判定/到着メッセージ |
| RacialBehaviors | 15件 | 6種族AI行動判定/優先度/不一致種族拒否/DecideAction |
| Enum境界値網羅 | 20件 | Weather/Element/CombatStance/Territory/Religion/Pet/Season/TimePeriod/Class/Difficulty |

**テスト総数**: Core 5,331件(134ファイル) + GUI 148件(5ファイル) = **5,479件(139ファイル)**
