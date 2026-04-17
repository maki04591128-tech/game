# 実装計画書 Ver.prt.0.23 — アクセス不能要素の解消（未統合システム統合）

**ステータス**: ✅ 全タスク完了
**目的**: ゲームコード内に実装済みながらプレイヤーが一切触れられなかった15個のTry*メソッドをNPC対話・Searchキー拡張・自動トリガー経由でアクセス可能にする
**テスト結果**: 5,521テスト全合格（Core: 5,521）

---

## 背景

Ver.prt.0.13で全90システムをGameControllerに統合済みだったが、  
`TryCook`, `TryFish` 等15個のメソッドは**GameController内に実装済み**であるにも関わらず、  
プレイヤーの入力経路（キーバインド・NPC対話・タイル踏み込み）から一切呼び出されない状態だった。

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | 鍛冶屋対話: 装備修理(TrySmithRepair)・罠製作(TryCraftTrap) | NPC統合 | ✅ 完了 |
| 2 | 宿屋主人対話: 料理(TryCook/OnShowCooking)・賭博(TryGamble) | NPC統合 | ✅ 完了 |
| 3 | ギルド受付対話: 転職(TryClassChange)・スキル融合(TryFuseSkills) | NPC統合 | ✅ 完了 |
| 4 | 訓練師対話: 転職(TryClassChange) | NPC統合 | ✅ 完了 |
| 5 | 図書館司書対話: エンチャント(TryEnchant)・パズル(TryAttemptPuzzle) | NPC統合 | ✅ 完了 |
| 6 | 商人対話: 闇市場(TryBlackMarketBuy)・密輸(TrySmuggle)・投資(InvestmentSystem) | NPC統合 | ✅ 完了 |
| 7 | TrySearch拡張: 水辺での釣り(TryFish) | Search拡張 | ✅ 完了 |
| 8 | TrySearch拡張: フィールドでの採集(TryGather) | Search拡張 | ✅ 完了 |
| 9 | TrySearch拡張: ダンジョン内野営(TryCamp) | Search拡張 | ✅ 完了 |
| 10 | TryDescendStairs: 5階ごとにショートカット自動解放(TryUnlockShortcut) | 自動トリガー | ✅ 完了 |
| 11 | InvestmentSystemを正しく使用するよう修正 | 品質改善 | ✅ 完了 |

---

## 詳細

### タスク1-6: HandleNpcTile + DispatchNpcAction 拡張

各NPCの対話選択肢に新アクションを追加し、`DispatchNpcAction`に対応ハンドラーを実装した。

#### 鍛冶屋 (NpcBlacksmith)
- **追加選択肢**: 「装備を修理する（最も破損した装備）」→ `action:smith_repair_auto`
- **追加選択肢**: 「罠を製作する」→ `action:craft_trap_menu`（5種サブメニュー）
- **サブメニュー**: 棘罠/落とし穴/爆発罠/睡眠罠/警報罠の5種選択

#### 宿屋主人 (NpcInnkeeper)
- **追加選択肢**: 「料理する」→ `action:cook` → `OnShowCooking?.Invoke()`
- **追加選択肢**: 「賭博をする」→ `action:gamble_menu`（3種サブメニュー）
- **サブメニュー**: サイコロ/丁半/ハイ＆ローの3種選択（50G賭け）

#### ギルド受付 (NpcGuildReceptionist) - 登録済みバリアント
- **追加選択肢**: 「転職する」→ `action:class_change_menu`（全CharacterClass動的メニュー）
- **追加選択肢**: 「スキル融合」→ `action:fuse_skills_auto`（融合可能ペアをメニュー提示）

#### 訓練師 (NpcTrainer)
- **追加選択肢**: 「転職する」→ `action:class_change_menu`

#### 図書館司書 (NpcLibrarian)
- **追加選択肢**: 「エンチャントを依頼する」→ `action:enchant_menu`（全EnchantmentTypeサブメニュー）
- **追加選択肢**: 「パズルに挑戦する」→ `action:attempt_puzzle_menu`（ルーン語/属性/物理の3種）

#### 商人 (NpcShopkeeper)
- **追加選択肢**: 「投資する」→ `action:invest_shop`（InvestmentSystem.Invest()使用）
- **追加選択肢**: 「裏の商品を見る」→ `action:black_market_browse`（カルマ条件付き動的商品リスト）
- **追加選択肢**: 「密輸を依頼する」→ `action:smuggle`

### DispatchNpcActionの新アクション一覧

| アクション | 呼び出し先 |
|-----------|----------|
| `cook` | `OnShowCooking?.Invoke()` |
| `gamble_menu` | サブメニューダイアログ生成 |
| `gamble_dice` / `gamble_chohan` / `gamble_card` | `TryGamble(GamblingGameType.*, 50, _random.Next(...))` |
| `smith_repair_auto` | インベントリから最低耐久装備を自動選択→`TrySmithRepair()` |
| `craft_trap_menu` | 5種罠サブメニューダイアログ生成 |
| `craft_trap_spike/pitfall/explosive/sleep/alarm` | `TryCraftTrap(PlayerTrapType.*)` |
| `class_change_menu` | 全CharacterClass動的サブメニューダイアログ生成 |
| `fuse_skills_auto` | CanFuse判定済みペアをサブメニュー提示 |
| `enchant_menu` | 全EnchantmentType動的サブメニューダイアログ生成 |
| `attempt_puzzle_menu` | 3種パズルサブメニューダイアログ生成 |
| `attempt_puzzle_rune/elemental/physical` | `TryAttemptPuzzle(PuzzleType.*)` |
| `black_market_browse` | KarmaValueでGetAvailableItems→動的商品リスト生成 |
| `smuggle` | `TrySmuggle("")` |
| `invest_shop` | `_investmentSystem.Invest(InvestmentType.Shop, ...)` |
| `class_change_{Class}` | `TryClassChange(CharacterClass)` (defaultパース) |
| `enchant_{Type}` | `TryEnchant(mainHand, EnchantmentType)` (defaultパース) |
| `black_market_buy_{Name}` | `TryBlackMarketBuy(item)` (defaultパース) |
| `fuse_{skillA}_{skillB}` | `TryFuseSkills(skillA, skillB)` (defaultパース) |

### タスク7-9: TrySearch拡張

`TrySearch()`メソッドをコンテキスト感知型に拡張。隠し通路・罠発見のコア機能は変更なし。

```
水辺隣接(nearWater) && 何も見つからなかった:
  → 50%で TryFish() 発動
  → 不発時ヒントメッセージ「近くに水辺がある...」

フィールドマップ(_isLocationField) && 何も見つからなかった:
  → 50%でタイルに応じた TryGather(GatheringType) 発動
    - Grass → Herb  
    - Tree → Logging
    - それ以外 → Foraging
  → 不発時ヒントメッセージ「採集できそうな場所がある...」

ダンジョン内 && 非地上 && 非ロケーションマップ && 非戦闘 && 何も見つからなかった:
  → TryCamp(SleepQuality.Nap) 発動（仮眠）
```

### タスク10: TryDescendStairs拡張

`TryDescendStairs()`で`CurrentFloor % 5 == 0`の場合に`TryUnlockShortcut()`を自動呼び出し。  
5・10・15・20・25・30階に降りるたびに現在階～5階前のショートカットが解放される。

### タスク11: InvestmentSystem正式接続

以前は単にゴールドを差し引くだけだったが、`_investmentSystem.Invest()`を正しく呼び出し、  
期待リターン・成功確率を含む詳細なフィードバックメッセージを表示するよう修正。

---

## アクセス可能になったシステム一覧

| # | メソッド | システム | アクセス経路 |
|---|---------|---------|------------|
| 1 | `TryCook` | CookingSystem | 宿屋主人 → 料理する |
| 2 | `TryFish` | FishingSystem | Fキー（水辺隣接時50%） |
| 3 | `TryGather` | GatheringSystem | Fキー（フィールド時50%） |
| 4 | `TryGamble` | GamblingSystem | 宿屋主人 → 賭博をする |
| 5 | `TryFuseSkills` | SkillFusionSystem | ギルド受付 → スキル融合（候補ペアメニュー） |
| 6 | `TryCamp` | RestSystem | Fキー（ダンジョン内非戦闘時） |
| 7 | `TryCraftTrap` | TrapCraftingSystem | 鍛冶屋 → 罠を製作する（5種選択） |
| 8 | `TrySmuggle` | SmugglingSystem | 商人 → 密輸を依頼する |
| 9 | `TryBlackMarketBuy` | BlackMarketSystem | 商人 → 裏の商品を見る（カルマ条件） |
| 10 | `TryAttemptPuzzle` | EnvironmentalPuzzleSystem | 図書館司書 → パズルに挑戦する |
| 11 | `TryUnlockShortcut` | DungeonShortcutSystem | 5・10・15...階降下時自動 |
| 12 | `TrySmithRepair` | SmithingSystem | 鍛冶屋 → 装備を修理する |
| 13 | `TryEnchant` | EnchantmentSystem | 図書館司書 → エンチャントを依頼する |
| 14 | `TryClassChange` | MultiClassSystem | ギルド受付/訓練師 → 転職する |
| 15 | `InvestmentSystem.Invest` | InvestmentSystem | 商人 → 投資する（正式接続） |

---

## 変更ファイル一覧

| ファイル | 変更種別 | 概要 |
|---------|---------|------|
| `src/RougelikeGame.Gui/GameController.cs` | 修正 | HandleNpcTile拡張、DispatchNpcAction拡張、TrySearch拡張、TryDescendStairs拡張、InvestmentSystem正式接続 |
