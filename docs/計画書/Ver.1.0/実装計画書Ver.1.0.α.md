# 実装計画書 Ver.1.0.α（清算期〜再構築期〜世界観差し替え）

> **📌 本書について**
>
> 新企画書 `docs/01_新企画書.md` §9.1 の 12 ヶ月スケジュールに基づく Ver.1.0 アルファ期の実装計画書です。
> Phase B〜D（清算 → 再構築 → 世界観差し替え）をカバーします。
> Phase A（本書自体の作成を含むドキュメント整理）は完了しました。

---

## 0. 前提

| 項目 | 値 |
|------|-----|
| バージョン | Ver.1.0.α.1〜Ver.1.0.α.3 |
| 目標期間 | 新企画書 §9.1 の Month 1〜7（清算期 → 再構築期 → アルファ版） |
| 対象 Phase | B（清算）／ C（コアシステム改修）／ D（世界観差し替え） |
| 前提ドキュメント | `docs/01_新企画書.md`、`docs/流用資産マップ.md`、`docs/企画設計書/02_〜18_*.md` |

---

## 1. Ver.1.0.α.1：清算期（Phase B：Month 1〜2）

**目標**：旧 137 システムから新 28 システム構成へ「**表面上**」縮減する。ただし **コードは一切削除せず**、参照解除＋フィーチャフラグ切替による非破壊アプローチを採る。

### 1.1 方針（2026-04-17 改訂）

- ❌ ファイル・クラス・テストを**削除しない**
- ✅ `GameSettings`（または新規 `RuneMageSettings`）に `UseRuneMageMode` 等のフィーチャフラグを導入
- ✅ `GameController` 側で **休眠システムの初期化を条件分岐**してスキップ
- ✅ 既存テスト（～7,000 件）は原則そのまま通す。壊れる場合のみ最小修正
- ✅ 休眠対象システムにも `[Obsolete("Ver.1.0 休眠。Ver.2.0 で再評価", error: false)]` を付与し、IDE 上で識別可能にする（任意）

### 1.2 タスク

- [ ] B-1. フィーチャフラグ基盤の追加
  - `GameSettings.UseRuneMageMode`（デフォルト `true`）
  - `EnableSixElements` / `EnableSixStatusEffects` / `EnableDiaryUi` / `EnableFiveTerritories` / `EnablePermanentKnowledge` を追加
- [ ] B-2. `GameController` で休眠対象システムの初期化をフラグ条件下に置く（`docs/流用資産マップ.md` §6.1 の 35+ システム）
  - Religion / Karma / Reputation / TerritoryInfluence
  - Proficiency / WeaponProficiency
  - Skill / SkillTree / SkillFusion
  - CombatStance / Smithing / Cooking / Fishing / Gathering / Harvest
  - Gambling / Investment / Smuggling / Steal
  - Companion / Pet / BaseConstruction
  - ItemIdentification / Enchantment / MerchantGuild / PriceFluctuation / BlackMarket
  - WanderingBoss / ItemGrade / MultiClass / RacialTrait / MonsterRace
  - Disease / InfiniteDungeon / Oath / Relationship / FactionWar / DungeonFaction
  - Execution / TrapCrafting / Mimic / DungeonEcosystem / DungeonShortcut
  - EnvironmentalPuzzle / EnvironmentalCombat / Season / ModLoader
- [ ] B-3. 休眠対象 GUI 画面 14 枚について、タイトル・メインメニュー・キーバインドからの**起動エントリを外す**（XAML／コードビハインドはそのまま残す）
- [ ] B-4. 既存テストが全合格のまま通ることを継続的に確認
  - `dotnet test RougelikeGame.sln --filter "FullyQualifiedName!~GuiAutomationTests&FullyQualifiedName!~GuiSystemVerificationTests"`
- [ ] B-5. GUI オートテスト（GuiAutomationTests / GuiSystemVerificationTests）を新 GUI 構成に合わせて修正（削除画面が消えることの確認は「非表示」確認に置き換える）
- [ ] B-6. 休眠対象システムに `[Obsolete]` 属性を付与（任意、レビュー後に一括処理）

### 1.3 完了条件

- 新企画書 §10.2「完全削除リスト」109 項目のうち、休眠化可能なものがすべて `GameController` から参照解除済み
- フィーチャフラグ OFF で旧仕様が動作することが確認できる（回帰保護）
- ビルド通過・全テスト合格
- `GameController` の**表面上の**アクティブシステム数が約 10 に圧縮

---

## 2. Ver.1.0.α.2：再構築期（Phase C：Month 3〜4）

**目標**：新企画書 §5.1 のコアシステム（魔法言語・理解度・詠唱・石碑解読・正気度・死に戻り）を、**既存クラスを拡張する非破壊の形で**完全実装する。

### 2.1 方針

- 既存クラスの public API は壊さない（新メソッド・新コンストラクタ・新プロパティを追加する方向で拡張）
- 旧挙動はフィーチャフラグ OFF で再現できる状態を維持

### 2.1 タスク

- [ ] C-1. 魔法言語 50 語の最終選定（`docs/企画設計書/02_魔法言語設計書.md`）
  - 旧 `docs/archive/企画設計書_旧/08_魔法言語設計書.md` の 79 語から選抜
  - 古ノルド語の実在性・文法正当性チェック（AI + ネイティブレビュー）
- [ ] C-2. 文法ボーナス・暴発リスク・条件語によるスクリプト詠唱の実装
- [ ] C-3. 理解度システム実装（`03_理解度システム設計書.md`）
  - 0〜100% 理解度、使用・成功で上昇、死に戻りで永続
- [ ] C-4. 詠唱システム改修（`04_詠唱システム設計書.md`）
  - 消費 MP = 語の基礎 MP × 乗算係数 × 理解度補正
  - 詠唱ターン = 語の詠唱コスト合計
- [ ] C-5. 石碑・古文書解読システム実装（`05_石碑・古文書解読システム設計書.md`）
- [ ] C-6. 正気度システム言語統合版実装（`06_正気度システム設計書（言語統合版）.md`）
  - Normal → Uneasy → Anxious → Unstable → Madness → Broken
  - ルーン語誤認（`brenna` を `binda` と読む）
- [ ] C-7. 死に戻りシステム改修（`07_死に戻りシステム設計書.md`）
  - 肉体リセット ／ 知識永続
- [ ] C-8. ステータス極小化：VIT/STR/AGI/DEX/INT/MND/PER/CHA/LUK → HP/MP/Lv/正気度/理解度
  - 旧 9 ステータスは内部保持、新 5 指標を UI/バランス面で前面化
  - HP：重量 = 5:1 連動
- [ ] C-9. 属性 12 → 6（eldr / vatn / jörð / vindr / ljós / myrkr）：6 属性を追加定義し、フィーチャフラグで 12→6 に絞り込み。旧属性定義は保全
- [ ] C-10. 状態異常 20+ → 6（brenndr / frosinn / verkur / blóð / blindr / eitr）：新 6 状態異常を追加。旧状態異常は保全
- [ ] C-11. チュートリアル（最初の 10 分、`brenna` 習得まで）完全実装

### 2.2 完了条件

- 新ルーン語 50 語で詠唱・戦闘・石碑解読が成立
- 正気度・死に戻りループが動作
- チュートリアルプレイで最初の 10 分が通し可能

---

## 3. Ver.1.0.α.3：世界観差し替え（Phase D：Month 5〜7）

**目標**：5 地域・65 NPC・プロローグ・ルート A ストーリー・日記 UI を実装し、アルファ版として通しプレイ可能にする。

### 3.1 タスク

- [ ] D-1. 5 地域データの実装（Vitruhæðir / Hressvöllur / Myrkviðr / Svikafjall / Helgrind）
- [ ] D-2. 国全体マップ・領地シンボルマップの 5 地域対応
- [ ] D-3. NPC 65 人（主要 7 / 商店 12 / 住民 35 / 特殊 11）の配置
- [ ] D-4. プロローグ実装（1978 年遺跡 → `öld rennr aptr` 暴走 → 古代タイムスリップ）
- [ ] D-5. 30 日タイムリミット（2,592,000 ターン、1 ターン ≒ 1 秒）
- [ ] D-6. ルート A（外交・救済）完全実装
- [ ] D-7. ルート B / C は分岐点のみ（簡易エンディング）
- [ ] D-8. 日記 UI（1978 年手書きフィールドノート風）実装
- [ ] D-9. 敵 66 → 15〜20 体への再選定と北欧神話系敵配置（Garmr 等）
- [ ] D-10. GUI 画面 29 → 15 画面への削減完了
- [ ] D-11. アイテムカテゴリ再編（古文書／言語学道具／食料／医療）
- [ ] D-12. サバイバル簡素化（満腹度・時間帯・天候のみ）

### 3.2 完了条件

- ルート A エンディングまで通しプレイ可能
- タイムリミット 30 日が機能
- 5 地域すべてに到達可能

---

## 4. 依存関係と順序制約

```
Phase A (docs)  ── 完了（2026-04-17）
        │
        ▼
Phase B (清算)  ── Ver.1.0.α.1
        │
        ▼
Phase C (コア)  ── Ver.1.0.α.2
        │
        ▼
Phase D (世界観) ── Ver.1.0.α.3
        │
        ▼
Phase E (ブランディング) ── Ver.1.0.β 直前
```

Phase B の完了前に Phase C に着手するとコード競合が激化するため、順序厳守。

---

## 更新履歴

| 日付 | 更新内容 |
|------|---------|
| 2026-04-17 | Phase A 完了時点で Ver.1.0.α 実装計画書を新規作成 |
