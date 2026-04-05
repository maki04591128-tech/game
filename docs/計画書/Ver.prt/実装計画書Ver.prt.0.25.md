# 実装計画書 Ver.prt.0.25 — 満腹度・渇き度システム全面改修 ＆ 行動ターン消費システム改修

**ステータス**: ⬜ 未着手
**目的**: 満腹度・渇き度システムを全面改修し、段階的なペナルティ・行動コスト加算・過食/飲み過ぎ状態を含む10段階制に拡張する。また、行動ターン消費システムを全面改修し、シチュエーション別のターンコストを全キャラクターに適用する
**前提バージョン**: Ver.prt.0.24（テスト5,539件全合格）

---

## 概要

現在の満腹度・渇き度システム（5段階、0〜100範囲）を廃止し、以下の仕様に全面改修する。

### 新仕様: 満腹度

| 満腹度範囲 | 状態名 | 効果 |
|-----------|--------|------|
| 120以上 | 吐き気 | 行動コスト+3、30%の確率で行動不可 |
| 100以上120未満 | 過食 | 行動コスト+2 |
| 80以上100未満 | 満腹 | 行動コスト+1 |
| 50以上80未満 | 通常 | なし |
| 40以上50未満 | 空腹（小） | 30%の確率で行動コスト+1 |
| 0以上40未満 | 空腹（大） | 行動コスト+1 |
| −1以上−8以下 | 飢餓 | 行動コスト+2、1ターン（秒）毎に1ダメージ |
| −9 | 餓死寸前 | 移動・攻撃不能、1ターン毎に10ダメージ |
| −10 | 餓死 | 即死（餓死として処理） |

- **通常消費**: 864ターン（秒）毎に満腹度1減少
- **0以下の消費**: 59,220ターン毎に満腹度1減少に変更

### 新仕様: 渇き度

満腹度と同一のシステムで、消費ターンを1/2にする。

| 渇き度範囲 | 状態名 | 効果 |
|-----------|--------|------|
| 120以上 | 吐き気 | 行動コスト+3、30%の確率で行動不可 |
| 100以上120未満 | 過飲 | 行動コスト+2 |
| 80以上100未満 | 満腹 | 行動コスト+1 |
| 50以上80未満 | 通常 | なし |
| 40以上50未満 | 渇き（小） | 30%の確率で行動コスト+1 |
| 0以上40未満 | 渇き（大） | 行動コスト+1 |
| −1以上−8以下 | 脱水 | 行動コスト+2、1ターン（秒）毎に1ダメージ |
| −9 | 干死寸前 | 移動・攻撃不能、1ターン毎に10ダメージ |
| −10 | 干死 | 即死（干死として処理） |

- **通常消費**: 432ターン（秒）毎に渇き度1減少（満腹度の1/2）
- **0以下の消費**: 29,610ターン毎に渇き度1減少に変更（59,220の1/2）

---

## 現行実装との差分

| 項目 | 現行 | 改修後 |
|------|------|--------|
| 満腹度範囲 | 0〜100 | −10〜上限なし（実装上は150をMaxとする） |
| 満腹度段階数 | 5段階（Full/Normal/Hungry/Starving/Famished） | 10段階（吐き気/過食/満腹/通常/空腹小/空腹大/飢餓/餓死寸前/餓死 + 0境界） |
| 満腹度消費間隔 | 600ターン（HungerDecayInterval） | 864ターン（通常時）/ 59,220ターン（0以下時） |
| 渇き度範囲 | 0〜100 | −10〜上限なし（実装上は150をMaxとする） |
| 渇き度段階数 | 5段階（Hydrated/Thirsty/Dehydrated/SevereDehydration/CriticalDehydration） | 10段階（吐き気/過飲/満腹/通常/渇き小/渇き大/脱水/干死寸前/干死 + 0境界） |
| 渇き度消費間隔 | 600ターン（HungerDecayIntervalと共通） | 432ターン（通常時）/ 29,610ターン（0以下時） |
| 行動コスト影響 | ステータス倍率のみ（攻撃力±20%等） | 行動コスト直接加算（+1〜+3）＋確率行動不可 |
| 過食・過飲状態 | なし | 新規追加（100以上で段階的ペナルティ） |
| 飢餓ダメージ | Famished時のみ（1HP/ターン） | 飢餓時1HP/ターン、餓死寸前時10HP/ターン |
| 移動・攻撃制限 | なし | 餓死寸前・干死寸前で移動・攻撃不能 |

---

## タスク一覧

| # | タスク | カテゴリ | 影響ファイル | ステータス |
|---|--------|---------|-------------|-----------|
| 1 | HungerStage enum 10段階化 | Enum定義 | Enums.cs | ⬜ 未着手 |
| 2 | ThirstStage enum 10段階化 | Enum定義 | Enums.cs | ⬜ 未着手 |
| 3 | GameConstants 満腹度・渇き度定数変更 | 定数 | GameConstants.cs | ⬜ 未着手 |
| 4 | Player.cs Hunger プロパティ範囲拡張 | コアエンティティ | Player.cs | ⬜ 未着手 |
| 5 | Player.cs GetHungerStage 閾値更新 | コアエンティティ | Player.cs | ⬜ 未着手 |
| 6 | Player.cs Thirst プロパティ範囲拡張 | コアエンティティ | Player.cs | ⬜ 未着手 |
| 7 | Player.cs GetThirstStage 閾値更新 | コアエンティティ | Player.cs | ⬜ 未着手 |
| 8 | Player.cs 満腹度ペナルティ更新 | コアエンティティ | Player.cs | ⬜ 未着手 |
| 9 | Player.cs 渇き度ペナルティ更新 | コアエンティティ | Player.cs | ⬜ 未着手 |
| 10 | ResourceSystem.cs HungerState enum 更新 | エンジン層 | ResourceSystem.cs | ⬜ 未着手 |
| 11 | ResourceSystem.cs GetHungerState/GetHungerEffect 更新 | エンジン層 | ResourceSystem.cs | ⬜ 未着手 |
| 12 | ThirstSystem.cs 全面改修 | エンジン層 | ThirstSystem.cs | ⬜ 未着手 |
| 13 | GameController.cs 満腹度消費ロジック（864ターン周期） | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 14 | GameController.cs 渇き度消費ロジック（432ターン周期） | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 15 | GameController.cs 0以下時の消費ターン切替（59,220 / 29,610） | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 16 | 飢餓・脱水ダメージ実装（毎ターン1HP） | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 17 | 餓死寸前・干死寸前の移動攻撃不能実装 | ゲームロジック | GameController.cs, TurnManager.cs | ⬜ 未着手 |
| 18 | 吐き気状態の30%行動不可実装 | ゲームロジック | GameController.cs, TurnManager.cs | ⬜ 未着手 |
| 19 | 空腹（小）・渇き（小）の30%行動コスト+1実装 | ゲームロジック | GameController.cs, TurnManager.cs | ⬜ 未着手 |
| 20 | 餓死・干死判定（−10で即死） | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 21 | 餓死寸前・干死寸前ダメージ（毎ターン10HP） | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 22 | 行動コスト加算のTurnManager統合 | 行動コスト | TurnManager.cs | ⬜ 未着手 |
| 23 | メッセージログ対応（各段階の警告メッセージ） | UI | GameController.cs | ⬜ 未着手 |
| 24 | GUI ステータスバー表示更新 | UI | MainWindow.xaml.cs | ⬜ 未着手 |
| 25 | 満腹度段階テスト | テスト | Core.Tests | ⬜ 未着手 |
| 26 | 渇き度段階テスト | テスト | Core.Tests | ⬜ 未着手 |
| 27 | 行動コスト統合テスト | テスト | Core.Tests / Gui.Tests | ⬜ 未着手 |
| 28 | 飢餓・脱水ダメージテスト | テスト | Core.Tests | ⬜ 未着手 |
| 29 | 餓死・干死判定テスト | テスト | Core.Tests | ⬜ 未着手 |
| 30 | 既存テスト修正（旧段階・旧閾値の更新） | テスト | Core.Tests / Gui.Tests | ⬜ 未着手 |
| **行動ターン消費システム改修** | | | | |
| 31 | 行動ターンコスト定数の全面改修 | 定数 | GameConstants.cs | ⬜ 未着手 |
| 32 | 装備部位別ターンコスト定数追加 | 定数 | GameConstants.cs | ⬜ 未着手 |
| 33 | シンボルマップ用ターンコスト定数追加 | 定数 | GameConstants.cs | ⬜ 未着手 |
| 34 | GameController.cs ProcessInput ターンコスト改修 | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 35 | 階層移動・祈りのターンコスト改修 | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 36 | インベントリソートのターンコスト追加 | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 37 | シンボルマップ移動コスト300ターン実装 | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 38 | Tキー進入ターン消費なし実装 | ゲームロジック | GameController.cs | ⬜ 未着手 |
| 39 | リアルタイムターン消費システム実装 | ゲームロジック | GameController.cs, TurnManager.cs | ⬜ 未着手 |
| 40 | NPC・敵キャラクターへのターンコスト統一適用 | ゲームロジック | TurnManager.cs, AIController.cs | ⬜ 未着手 |
| 41 | 変動ターンコスト（アイテム・スキル・呪文・領地）データ整備 | データ定義 | 各定義ファイル | ⬜ 未着手 |
| 42 | 行動ターン消費テスト | テスト | Core.Tests / Gui.Tests | ⬜ 未着手 |

---

## 詳細タスク説明

### タスク 1: HungerStage enum 10段階化

**ファイル**: `src/RougelikeGame.Core/Enums/Enums.cs`（45-52行）

現行の5段階 enum を以下の10段階に置換する。

```
現行: Full(80-100) / Normal(50-79) / Hungry(25-49) / Starving(1-24) / Famished(0)
↓
改修: Nausea(120+) / Overeating(100-119) / Full(80-99) / Normal(50-79) /
      SlightlyHungry(40-49) / VeryHungry(0-39) / Starving(-1〜-8) /
      NearStarvation(-9) / Starvation(-10)
```

**新 enum 定義**:
- `Nausea` — 吐き気（120以上）
- `Overeating` — 過食（100以上120未満）
- `Full` — 満腹（80以上100未満）
- `Normal` — 通常（50以上80未満）
- `SlightlyHungry` — 空腹（小）（40以上50未満）
- `VeryHungry` — 空腹（大）（0以上40未満）
- `Starving` — 飢餓（−1以上−8以下）
- `NearStarvation` — 餓死寸前（−9）
- `Starvation` — 餓死（−10）

※ 満腹度0は `VeryHungry` に含まれるが、0以下になった時点で消費ターンが59,220に切り替わる特殊境界。

---

### タスク 2: ThirstStage enum 10段階化

**ファイル**: `src/RougelikeGame.Core/Enums/Enums.cs`（1147-1159行）

現行の5段階 enum を以下の10段階に置換する。

```
現行: Hydrated(80-100) / Thirsty(50-79) / Dehydrated(25-49) / SevereDehydration(1-24) / CriticalDehydration(0)
↓
改修: Nausea(120+) / Overdrinking(100-119) / Full(80-99) / Normal(50-79) /
      SlightlyThirsty(40-49) / VeryThirsty(0-39) / Dehydrated(-1〜-8) /
      NearDesiccation(-9) / Desiccation(-10)
```

**新 enum 定義**:
- `Nausea` — 吐き気（120以上）
- `Overdrinking` — 過飲（100以上120未満）
- `Full` — 満腹（80以上100未満）
- `Normal` — 通常（50以上80未満）
- `SlightlyThirsty` — 渇き（小）（40以上50未満）
- `VeryThirsty` — 渇き（大）（0以上40未満）
- `Dehydrated` — 脱水（−1以上−8以下）
- `NearDesiccation` — 干死寸前（−9）
- `Desiccation` — 干死（−10）

---

### タスク 3: GameConstants 満腹度・渇き度定数変更

**ファイル**: `src/RougelikeGame.Core/Constants/GameConstants.cs`

**変更する定数**:

| 定数名 | 現行値 | 新値 | 説明 |
|--------|-------|------|------|
| `MaxHunger` | 100 | 150 | 満腹度上限（吐き気状態の余裕を確保） |
| `InitialHunger` | 100 | 70 | 初期満腹度（通常範囲で開始） |
| `MinHunger` | （なし、0でClamp） | −10 | 満腹度下限（餓死ライン） |
| `MaxThirst` | 100 | 150 | 渇き度上限 |
| `InitialThirst` | 100 | 70 | 初期渇き度（通常範囲で開始） |
| `MinThirst` | （なし、0でClamp） | −10 | 渇き度下限（干死ライン） |
| `HungerDecayInterval` | 600 | 864 | 満腹度通常消費間隔（ターン） |
| `HungerDecayIntervalStarving` | （新規） | 59220 | 満腹度0以下消費間隔（ターン） |
| `ThirstDecayInterval` | （新規） | 432 | 渇き度通常消費間隔（ターン） |
| `ThirstDecayIntervalStarving` | （新規） | 29610 | 渇き度0以下消費間隔（ターン） |

**補足**: 
- 初期値70は通常範囲（50-79）の中央付近であり、ゲーム開始直後に食事の必要性が発生しない
- MaxHunger=150は吐き気状態（120+）でも30ポイントの余裕があり、食べ過ぎによる上限到達を防ぐ

---

### タスク 4: Player.cs Hunger プロパティ範囲拡張

**ファイル**: `src/RougelikeGame.Core/Entities/Player.cs`（42-58行）

Hungerプロパティの `Math.Clamp(value, 0, GameConstants.MaxHunger)` を `Math.Clamp(value, GameConstants.MinHunger, GameConstants.MaxHunger)` に変更し、負の値を許容する。

---

### タスク 5: Player.cs GetHungerStage 閾値更新

**ファイル**: `src/RougelikeGame.Core/Entities/Player.cs`（483-490行）

現行の `GetHungerStage` メソッドを新しい10段階の閾値に更新する。

```
hunger switch
{
    >= 120 => HungerStage.Nausea,
    >= 100 => HungerStage.Overeating,
    >= 80 => HungerStage.Full,
    >= 50 => HungerStage.Normal,
    >= 40 => HungerStage.SlightlyHungry,
    >= 0 => HungerStage.VeryHungry,
    >= -8 => HungerStage.Starving,
    >= -9 => HungerStage.NearStarvation,    // -9 のみ
    _ => HungerStage.Starvation             // -10 以下
};
```

---

### タスク 6: Player.cs Thirst プロパティ範囲拡張

**ファイル**: `src/RougelikeGame.Core/Entities/Player.cs`（60-76行）

Thirstプロパティの `Math.Clamp` を `Math.Clamp(value, GameConstants.MinThirst, GameConstants.MaxThirst)` に変更。

---

### タスク 7: Player.cs GetThirstStage 閾値更新

**ファイル**: `src/RougelikeGame.Core/Entities/Player.cs`（492-499行）

`GetThirstStage` メソッドを新しい10段階の閾値に更新する。

```
thirst switch
{
    >= 120 => ThirstStage.Nausea,
    >= 100 => ThirstStage.Overdrinking,
    >= 80 => ThirstStage.Full,
    >= 50 => ThirstStage.Normal,
    >= 40 => ThirstStage.SlightlyThirsty,
    >= 0 => ThirstStage.VeryThirsty,
    >= -8 => ThirstStage.Dehydrated,
    >= -9 => ThirstStage.NearDesiccation,
    _ => ThirstStage.Desiccation
};
```

---

### タスク 8: Player.cs 満腹度ペナルティ更新

**ファイル**: `src/RougelikeGame.Core/Entities/Player.cs`

現行の `hungerPenalty`（Starving/Hungry時のStatModifier）を新段階に合わせて更新する。

行動コスト加算は別途TurnManager側で処理するため、Player側ではステータス低下のみを管理する。飢餓以上の深刻な段階では、Strength/Agility/Dexterity等に大幅なペナルティを追加。

---

### タスク 9: Player.cs 渇き度ペナルティ更新

**ファイル**: `src/RougelikeGame.Core/Entities/Player.cs`

現行の `thirstPenalty` を新段階に合わせて更新する。脱水以上では Intelligence/Mind/Agility に大幅ペナルティ。

---

### タスク 10: ResourceSystem.cs HungerState enum 更新

**ファイル**: `src/RougelikeGame.Engine/Combat/ResourceSystem.cs`（468-482行）

現行の6段階 `HungerState` enum を廃止し、`HungerStage`（タスク1で定義）に統一する。または `HungerState` を同じ10段階に拡張してマッピングを更新する。

※ `HungerStage`（Core層）と `HungerState`（Engine層）が重複しているため、Engine層の `HungerState` を廃止して `HungerStage` に統一することを推奨。統一が困難な場合は両方を10段階に拡張する。

---

### タスク 11: ResourceSystem.cs GetHungerState/GetHungerEffect 更新

**ファイル**: `src/RougelikeGame.Engine/Combat/ResourceSystem.cs`（208-238行）

`GetHungerState` の閾値と `GetHungerEffect` の効果を新仕様に合わせて更新する。

**新しい HungerEffect マッピング**:

| 段階 | AttackMultiplier | CanUseSkill | CanUseItem | DamagePerTick | ActionCostBonus |
|------|-----------------|-------------|------------|---------------|-----------------|
| 吐き気 | 0.7f | true | true | 0 | +3（30%行動不可） |
| 過食 | 0.9f | true | true | 0 | +2 |
| 満腹 | 1.0f | true | true | 0 | +1 |
| 通常 | 1.0f | true | true | 0 | 0 |
| 空腹（小） | 0.95f | true | true | 0 | 30%で+1 |
| 空腹（大） | 0.9f | true | true | 0 | +1 |
| 飢餓 | 0.7f | true | true | 1 | +2 |
| 餓死寸前 | 0f | false | false | 10 | N/A（行動不能） |
| 餓死 | N/A | N/A | N/A | N/A | N/A（即死） |

※ `HungerEffect` クラスに `ActionCostBonus` フィールドと `ActionBlockChance` フィールドの追加が必要。

---

### タスク 12: ThirstSystem.cs 全面改修

**ファイル**: `src/RougelikeGame.Core/Systems/ThirstSystem.cs`

`GetThirstModifiers`、`GetThirstDamage` を新段階に合わせて全面改修する。

**新しい渇き度効果マッピング**:

| 段階 | StrMod | AgiMod | IntMod | DamagePerTick | ActionCostBonus |
|------|--------|--------|--------|---------------|-----------------|
| 吐き気 | 0.7f | 0.7f | 0.7f | 0 | +3（30%行動不可） |
| 過飲 | 0.9f | 0.9f | 0.9f | 0 | +2 |
| 満腹 | 1.0f | 1.0f | 1.0f | 0 | +1 |
| 通常 | 1.0f | 1.0f | 1.0f | 0 | 0 |
| 渇き（小） | 0.95f | 0.95f | 0.95f | 0 | 30%で+1 |
| 渇き（大） | 0.9f | 0.9f | 0.85f | 0 | +1 |
| 脱水 | 0.6f | 0.6f | 0.6f | 1 | +2 |
| 干死寸前 | 0.2f | 0.2f | 0.3f | 10 | N/A（行動不能） |
| 干死 | N/A | N/A | N/A | N/A | N/A（即死） |

**追加メソッド**:
- `GetThirstActionCostBonus(ThirstStage stage)` — 行動コスト加算値を返す
- `GetThirstActionBlockChance(ThirstStage stage)` — 行動不可確率を返す（吐き気: 0.3f、他: 0f）
- `GetThirstStageName(ThirstStage stage)` — 日本語状態名を返す

---

### タスク 13: GameController.cs 満腹度消費ロジック（864ターン周期）

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

現行の満腹度消費ロジック（`TurnCount % TimeConstants.HungerDecayInterval == 0` で減少）を以下に変更：

- 満腹度 > 0 の場合: `TurnCount % 864 == 0` で満腹度 −1
- 満腹度 ≤ 0 の場合: `TurnCount % 59220 == 0` で満腹度 −1

**実装方針**: 
- ターン処理内で現在の満腹度を確認し、適用する消費間隔を動的に切り替える
- 直近の消費ターンをフィールドとして保持し、経過ターンが閾値を超えたら消費する方式を推奨（`_lastHungerDecayTurn` フィールド追加）

---

### タスク 14: GameController.cs 渇き度消費ロジック（432ターン周期）

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

満腹度と同様の方式で渇き度消費を実装する。

- 渇き度 > 0 の場合: 432ターン毎に渇き度 −1
- 渇き度 ≤ 0 の場合: 29,610ターン毎に渇き度 −1

**実装方針**: `_lastThirstDecayTurn` フィールドを追加し、経過ターン方式で管理。

---

### タスク 15: GameController.cs 0以下時の消費ターン切替（59,220 / 29,610）

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

タスク13・14の一部として、満腹度/渇き度が0以下になった瞬間に消費間隔を切り替えるロジックを実装する。

**注意点**:
- 食事・水分補給で0以上に回復した場合は通常の消費間隔に戻す
- 切り替え時に `_lastHungerDecayTurn` / `_lastThirstDecayTurn` をリセットして不整合を防ぐ

---

### タスク 16: 飢餓・脱水ダメージ実装（毎ターン1HP）

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

飢餓状態（満腹度 −1〜−8）および脱水状態（渇き度 −1〜−8）で、1ターン（秒）毎に1HPダメージを与える。

**実装方針**:
- ターン処理の先頭で満腹度/渇き度の段階を確認
- 飢餓/脱水状態であれば `Player.TakeDamage(1)` を毎ターン実行
- メッセージログに「🍖 飢えで1ダメージ！」「💧 渇きで1ダメージ！」を表示

---

### タスク 17: 餓死寸前・干死寸前の移動攻撃不能実装

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`, `src/RougelikeGame.Engine/TurnSystem/TurnManager.cs`

餓死寸前（満腹度 −9）・干死寸前（渇き度 −9）状態で移動・攻撃を不能にする。

**実装方針**:
- 移動キー入力時に満腹度/渇き度チェックを追加し、該当状態であれば「飢えで動けない…」「渇きで動けない…」メッセージを表示して入力を無視
- 攻撃コマンド実行時にも同様のチェック
- 待機・アイテム使用は可能（食事/水分補給で脱出可能にするため）
- 毎ターン10HPダメージ（タスク21で詳細実装）

---

### タスク 18: 吐き気状態の30%行動不可実装

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`, `src/RougelikeGame.Engine/TurnSystem/TurnManager.cs`

吐き気状態（満腹度120以上または渇き度120以上）で行動時に30%の確率で行動不可にする。

**実装方針**:
- 行動実行前にランダム判定（`Random.Next(100) < 30`）
- 行動不可の場合、「吐き気で行動できない！」メッセージを表示し、ターンのみ消費（行動コスト分）
- 行動コスト+3は行動不可の場合でも適用される

---

### タスク 19: 空腹（小）・渇き（小）の30%行動コスト+1実装

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`, `src/RougelikeGame.Engine/TurnSystem/TurnManager.cs`

空腹（小）（満腹度40-49）・渇き（小）（渇き度40-49）で行動時に30%の確率で行動コスト+1を適用する。

**実装方針**:
- 行動コスト計算時にランダム判定
- 30%で+1、70%でペナルティなし
- メッセージログに「空腹で動きが鈍い…」「渇きで動きが鈍い…」を確率発動時のみ表示

---

### タスク 20: 餓死・干死判定（−10で即死）

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

満腹度が−10に達した場合は `DeathCause.Starvation`（餓死）、渇き度が−10に達した場合は `DeathCause.Dehydration`（干死）として死亡処理を実行する。

**実装方針**:
- 満腹度/渇き度が変動するたびに−10チェック
- 先に到達した方の死因を適用
- 死亡処理は既存の死に戻りメカニクスに接続

---

### タスク 21: 餓死寸前・干死寸前ダメージ（毎ターン10HP）

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

餓死寸前（満腹度 −9）・干死寸前（渇き度 −9）状態で、1ターン毎に10HPダメージを与える。

**実装方針**:
- タスク16の飢餓ダメージ（1HP）とは別枠で処理
- 餓死寸前と脱水が同時の場合はダメージが重複する（両方10HP = 合計20HP/ターン）
- メッセージ:「🍖 飢えで10ダメージ！もう限界だ…」「💧 渇きで10ダメージ！もう限界だ…」
- このダメージでHP0になった場合も餓死/干死として処理

---

### タスク 22: 行動コスト加算のTurnManager統合

**ファイル**: `src/RougelikeGame.Engine/TurnSystem/TurnManager.cs`

`ProcessTurn` メソッドの行動コスト計算に、満腹度・渇き度による行動コスト加算を統合する。

**実装方針**:
- `CalculateFinalCost` メソッドに `hungerCostBonus` / `thirstCostBonus` パラメータを追加
- または、既存の `statusModifier` を活用して満腹度/渇き度の行動コスト加算を反映
- 複数のペナルティが同時に発生する場合は加算（例: 飢餓+2 + 脱水+2 = 合計+4）

---

### タスク 23: メッセージログ対応（各段階の警告メッセージ）

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

段階変化時にメッセージログへ警告メッセージを出力する。

| 段階変化 | メッセージ例 |
|----------|-------------|
| 通常→空腹（小） | 「🍖 少し空腹を感じる…」 |
| 空腹（小）→空腹（大） | 「🍖 空腹だ！何か食べたい…」 |
| 空腹（大）→飢餓 | 「🍖 飢えが身体を蝕む…（毎ターン1ダメージ）」 |
| 飢餓→餓死寸前 | 「🍖 もう動けない…食料がなければ死ぬ…（毎ターン10ダメージ・移動攻撃不能）」 |
| 通常→渇き（小） | 「💧 少し喉が渇いてきた…」 |
| 渇き（小）→渇き（大） | 「💧 喉が渇いた！水が欲しい…」 |
| 渇き（大）→脱水 | 「💧 脱水症状だ…（毎ターン1ダメージ）」 |
| 脱水→干死寸前 | 「💧 もう動けない…水がなければ死ぬ…（毎ターン10ダメージ・移動攻撃不能）」 |
| 過食→吐き気 | 「🤢 食べ過ぎて吐き気がする…（30%行動不可）」 |
| 過飲→吐き気 | 「🤢 飲み過ぎて吐き気がする…（30%行動不可）」 |

---

### タスク 24: GUI ステータスバー表示更新

**ファイル**: `src/RougelikeGame.Gui/MainWindow.xaml.cs`

ステータスバーの満腹度・渇き度表示を新段階に対応させる。

**変更内容**:
- 段階名の日本語表示を新仕様の名称に更新
- 負の値の表示に対応（例:「満腹度: −5/150 [飢餓]」）
- 危険な段階（飢餓以下/脱水以下）は赤色で表示
- 過食/過飲以上は黄色で表示

---

### タスク 25-30: テスト

#### タスク 25: 満腹度段階テスト
- 各閾値境界でのHungerStage判定テスト（120, 119, 100, 99, 80, 79, 50, 49, 40, 39, 0, -1, -8, -9, -10）
- HungerStageChangedイベント発火テスト

#### タスク 26: 渇き度段階テスト
- 各閾値境界でのThirstStage判定テスト（同上）
- ThirstStageChangedイベント発火テスト

#### タスク 27: 行動コスト統合テスト
- 各段階の行動コスト加算値テスト
- 30%確率行動コスト+1のテスト（100回実行で期待値近辺を確認）
- 30%行動不可のテスト
- 満腹度+渇き度ペナルティ重複テスト

#### タスク 28: 飢餓・脱水ダメージテスト
- 飢餓状態で毎ターン1HPダメージテスト
- 脱水状態で毎ターン1HPダメージテスト
- 餓死寸前で毎ターン10HPダメージテスト
- 干死寸前で毎ターン10HPダメージテスト
- 飢餓+脱水同時ダメージ重複テスト

#### タスク 29: 餓死・干死判定テスト
- 満腹度−10で餓死テスト（DeathCause.Starvation）
- 渇き度−10で干死テスト（DeathCause.Dehydration）
- ダメージによるHP0→餓死/干死判定テスト

#### タスク 30: 既存テスト修正
- 旧HungerStage/ThirstStageの5段階を使用しているテストを全検索して新段階に更新
- 旧閾値（80, 50, 25, 1, 0等）のハードコーディングを新閾値に更新
- HungerDecayInterval=600を使用しているテストを864に更新

---

## 実装順序（依存関係考慮）

```
Phase 1: 定義変更（タスク 1-3）
  ↓
Phase 2: コアエンティティ変更（タスク 4-9）
  ↓
Phase 3: エンジン層変更（タスク 10-12）
  ↓
Phase 4: ゲームロジック（タスク 13-21）
  ↓
Phase 5: 行動コスト統合（タスク 22）
  ↓
Phase 6: UI更新（タスク 23-24）
  ↓
Phase 7: テスト — 満腹度・渇き度（タスク 25-30）
  ↓
Phase 8: 行動ターン消費 — 定数改修（タスク 31-33）
  ↓
Phase 9: 行動ターン消費 — ゲームロジック（タスク 34-38）
  ↓
Phase 10: 行動ターン消費 — リアルタイム・NPC統一・データ整備（タスク 39-41）
  ↓
Phase 11: 行動ターン消費 — テスト（タスク 42）
```

**注意**: Phase 1〜3 は下位層から変更するため、各Phase完了後にビルド確認を行うこと。Phase 4 以降はGameController の大規模変更となるため、タスク単位でのビルド・テスト確認を推奨する。Phase 8〜11 は行動ターン消費システム改修であり、Phase 7 完了後に着手する。

---

## 設計上の注意事項

### 1. 消費間隔の実装方式
現行の `TurnCount % Interval == 0` 方式は、0以下になった瞬間に間隔が変わるため不正確になる可能性がある。
**推奨**: `_lastHungerDecayTurn` / `_lastThirstDecayTurn` フィールドを持ち、`(CurrentTurn - _lastDecayTurn) >= interval` で判定する方式に変更する。

### 2. 確率判定の再現性
30%行動不可・30%行動コスト+1は乱数に依存するため、テスト時には `IRandom` インターフェースを注入してモック可能にすることを推奨。

### 3. セーブ/ロード対応
- 満腹度・渇き度の負の値がセーブ/ロードで正しく保存・復元されることを確認
- `_lastHungerDecayTurn` / `_lastThirstDecayTurn` もセーブ対象に追加

### 4. 過食・過飲への到達経路
食事・水分補給アイテムの回復量が100を超える場合に過食/過飲状態に到達する。既存の食事/水分アイテムの回復量と `MaxHunger=150` の関係を確認し、過食状態に到達可能な回復量のアイテムが存在することを確認する。

### 5. 既存の ResourceSystem.HungerState との統合
Core層の `HungerStage` とEngine層の `HungerState` が重複している。本改修を機に統一することを推奨するが、影響範囲が大きい場合は両方を10段階に拡張する。

---

## 影響ファイル一覧

| ファイル | 変更種別 | 影響度 |
|---------|---------|-------|
| `src/RougelikeGame.Core/Enums/Enums.cs` | Enum改修 | 高 |
| `src/RougelikeGame.Core/Constants/GameConstants.cs` | 定数変更 | 高 |
| `src/RougelikeGame.Core/Entities/Player.cs` | プロパティ・メソッド改修 | 高 |
| `src/RougelikeGame.Core/Systems/ThirstSystem.cs` | 全面改修 | 高 |
| `src/RougelikeGame.Engine/Combat/ResourceSystem.cs` | Enum・メソッド改修 | 高 |
| `src/RougelikeGame.Engine/TurnSystem/TurnManager.cs` | 行動コスト統合・リアルタイム消費・NPC統一 | 高 |
| `src/RougelikeGame.Gui/GameController.cs` | 消費ロジック・ダメージ・制限・ターンコスト改修 | 高 |
| `src/RougelikeGame.Gui/MainWindow.xaml.cs` | ステータス表示更新 | 中 |
| `tests/RougelikeGame.Core.Tests/` | テスト追加・修正 | 中 |
| `tests/RougelikeGame.Gui.Tests/` | テスト追加・修正 | 中 |

---

## 行動ターン消費システム改修

### 概要

現行の行動ターン消費システムを全面改修する。ターン（秒）の消費はシチュエーションによって変動し、全キャラクター（プレイヤー・敵・NPC）に統一適用される。

### 行動ターンコスト一覧

#### 通常状態（ダンジョン・ワールドマップ内）

| ターン数 | 行動 |
|---------|------|
| **1ターン** | 移動、通常攻撃、装備着脱（武器・アクセサリー）、ポーション使用（クイックユーズ）、拾う、ドア開閉 |
| **5ターン** | 周辺調査、遠距離攻撃（投擲・射撃） |
| **10ターン** | 階層移動、祈り、装備着脱（腕・頭） |
| **20ターン** | 装備着脱（胴）、インベントリソート |
| **各自割当** | アイテム使用、料理、クラフト、医療用具使用、付呪、スキル使用、呪文詠唱、領地移動（※各アイテム・エンチャント・スキル・呪文・領地に個別設定されたターン数を使用） |
| **リアルタイム** | NPCとの交流、インベントリ操作、クエスト画面、スキルツリー画面等の別ウィンドウ操作時、釣り（※操作中もリアルタイムでターンが経過する） |

#### シンボルマップ状態

| ターン数 | 行動 |
|---------|------|
| **300ターン** | 移動（シンボルマップ上の移動のみ） |
| **0ターン** | Tキー押下によるシンボルマップへの進入 |

### 現行値との差分

| 行動 | 現行定数 | 現行値 | 改修後 | 備考 |
|------|---------|-------|--------|------|
| 移動 | `MoveNormal` | 1 | 1 | 変更なし |
| 通常攻撃 | `AttackNormal` | 3 | 1 | 3→1に短縮 |
| 非武装攻撃 | `AttackUnarmed` | 2 | 1 | 2→1に短縮（通常攻撃に統合） |
| 両手武器攻撃 | `AttackTwoHanded` | 5 | 1 | 5→1に短縮（通常攻撃に統合） |
| 射撃攻撃 | `AttackBow` | 4 | 5 | 4→5に変更（遠距離攻撃統一） |
| 投擲 | `AttackThrow` | 2 | 5 | 2→5に変更（遠距離攻撃統一） |
| ポーション使用 | `UsePotion` | 2 | 1 | 2→1に短縮（クイックユーズ） |
| 拾う | （MoveNormal流用） | 1 | 1 | 変更なし |
| ドア開閉 | `OpenDoor` | 1 | 1 | 変更なし |
| 周辺調査 | `Search` | 5 | 5 | 変更なし |
| 階層移動 | （MoveNormal流用） | 1 | 10 | 1→10に増加（新規） |
| 祈り | `Pray` | 10 | 10 | 変更なし |
| 装備着脱（武器・アクセサリー） | `EquipChange` | 5 | 1 | 5→1に短縮（部位別に分離） |
| 装備着脱（腕・頭） | `EquipChange` | 5 | 10 | 5→10に増加（部位別に分離） |
| 装備着脱（胴） | `EquipChange` | 5 | 20 | 5→20に増加（部位別に分離） |
| インベントリソート | （新規） | — | 20 | 新規追加 |
| シンボルマップ移動 | （新規） | — | 300 | 新規追加 |
| Tキー進入（シンボルマップ） | （新規） | — | 0 | 新規追加 |

### 設計上の注意事項

#### 1. 装備着脱の部位別コスト分離

現行の `EquipChange=5` を部位別に3分割する。

```
EquipWeapon = 1        // 武器
EquipAccessory = 1     // アクセサリー
EquipArms = 10         // 腕装備
EquipHead = 10         // 頭装備
EquipBody = 20         // 胴装備
```

装備スロットの `EquipmentSlot` enum を参照し、GameController 内の装備着脱処理で部位に応じたコストを選択する。

#### 2. リアルタイムターン消費

別ウィンドウ操作（NPCとの交流、インベントリ操作、クエスト画面、スキルツリー画面）や釣り等の実時間活動中は、実世界の経過時間に応じてターンが進行する仕組みを実装する。

- ウィンドウ開放時にタイマーを開始し、1秒毎に1ターン消費
- ウィンドウ閉鎖時にタイマーを停止し、累計ターンを加算
- 釣りは釣り開始から終了までの実時間でターン消費

#### 3. 変動ターンコスト

以下の行動は各データ定義に個別のターンコストフィールドを持つ：

| 行動 | データソース | フィールド |
|------|-----------|----------|
| アイテム使用 | アイテム定義 | `UseTurnCost` |
| 料理 | レシピ定義 | `CookTurnCost` |
| クラフト | レシピ定義 | `CraftTurnCost` |
| 医療用具使用 | アイテム定義 | `UseTurnCost` |
| 付呪 | エンチャント定義 | `EnchantTurnCost` |
| スキル使用 | スキル定義 | `TurnCost`（既存） |
| 呪文詠唱 | 呪文定義 | `CastTurnCost`（既存） |
| 領地移動 | 領地定義 | `TravelTurnCost` |

#### 4. NPC・敵キャラクターへの統一適用

全キャラクター（プレイヤー・敵・NPC）が同じターンコストテーブルを使用する。TurnManager の行動コスト計算を共通化し、AI行動決定時にも同一の `TurnCosts` 定数を参照する。

#### 5. 戦闘移動コストとの関係

現行の `MoveCombat=10` / `MoveStealth=10` 等の戦闘状態別移動コストは、TurnAction.CalculateFinalCost() 内の CombatState 修正として引き続き適用される。通常状態の基本移動コスト（1ターン）に対する倍率として機能する。

---

### 詳細タスク説明（タスク 31〜42）

#### タスク 31: 行動ターンコスト定数の全面改修

**ファイル**: `src/RougelikeGame.Core/Constants/GameConstants.cs`（9-57行）

TurnCosts クラスの既存定数値を以下のとおり変更する。

| 定数名 | 現行値 | 新値 |
|--------|-------|------|
| `AttackNormal` | 3 | 1 |
| `AttackUnarmed` | 2 | 1 |
| `AttackTwoHanded` | 5 | 1 |
| `AttackBow` | 4 | 5 |
| `AttackThrow` | 2 | 5 |
| `UsePotion` | 2 | 1 |

---

#### タスク 32: 装備部位別ターンコスト定数追加

**ファイル**: `src/RougelikeGame.Core/Constants/GameConstants.cs`

TurnCosts クラスに以下の定数を追加し、既存の `EquipChange` を非推奨にする。

```
EquipWeapon = 1          // 武器着脱
EquipAccessory = 1       // アクセサリー着脱
EquipArms = 10           // 腕装備着脱
EquipHead = 10           // 頭装備着脱
EquipBody = 20           // 胴装備着脱
InventorySort = 20       // インベントリソート
UseStairs = 10           // 階層移動（階段使用）
```

---

#### タスク 33: シンボルマップ用ターンコスト定数追加

**ファイル**: `src/RougelikeGame.Core/Constants/GameConstants.cs`

TurnCosts クラスに以下の定数を追加する。

```
SymbolMapMove = 300      // シンボルマップ上の移動
SymbolMapEntry = 0       // Tキーによるシンボルマップ進入
```

---

#### タスク 34: GameController.cs ProcessInput ターンコスト改修

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`（1034-1419行）

ProcessInput メソッドのswitch-case内で以下を変更する。

| 行動 | 現行の指定 | 改修後 |
|------|-----------|--------|
| 通常攻撃（TryMove内の敵接触） | `TurnCosts.AttackNormal` | `TurnCosts.AttackNormal`（定数値が3→1に変更済み） |
| RangedAttack | `TurnCosts.AttackBow` | `TurnCosts.AttackBow`（定数値が4→5に変更済み） |
| ThrowItem | `TurnCosts.AttackThrow` | `TurnCosts.AttackThrow`（定数値が2→5に変更済み） |
| Pickup | `TurnCosts.MoveNormal` | `TurnCosts.MoveNormal`（変更なし） |
| 装備変更 | `TurnCosts.EquipChange`（一律5） | 部位に応じて `EquipWeapon`/`EquipAccessory`/`EquipArms`/`EquipHead`/`EquipBody` を選択 |

---

#### タスク 35: 階層移動・祈りのターンコスト改修

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

| 行動 | 現行の指定 | 改修後 |
|------|-----------|--------|
| UseStairs | `TurnCosts.MoveNormal`（1） | `TurnCosts.UseStairs`（10） |
| AscendStairs | `TurnCosts.MoveNormal`（1） | `TurnCosts.UseStairs`（10） |

祈りは現行 `TurnCosts.Pray=10` で変更なし。

---

#### タスク 36: インベントリソートのターンコスト追加

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

インベントリソート実行時に `TurnCosts.InventorySort`（20ターン）を消費するように変更する。現行でソートがターン消費なしの場合は新規追加。

---

#### タスク 37: シンボルマップ移動コスト300ターン実装

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

シンボルマップ状態での移動行動を検出し、通常の `TurnCosts.MoveNormal`（1）の代わりに `TurnCosts.SymbolMapMove`（300）を適用する。

実装方針:
- `ProcessInput` の移動処理で、現在のマップ状態がシンボルマップかどうかを判定
- シンボルマップの場合、移動コストを `TurnCosts.SymbolMapMove` に差し替え
- シンボルマップ状態では移動以外の行動は不可（既存制約を維持）

---

#### タスク 38: Tキー進入ターン消費なし実装

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`

Tキー押下によるシンボルマップへの進入時にターン消費を0にする。

実装方針:
- シンボルマップ進入アクションのコストを `TurnCosts.SymbolMapEntry`（0）に設定
- ターン消費0の場合は `TurnCount` の加算とGameTime の進行をスキップ

---

#### タスク 39: リアルタイムターン消費システム実装

**ファイル**: `src/RougelikeGame.Gui/GameController.cs`, `src/RougelikeGame.Engine/TurnSystem/TurnManager.cs`

別ウィンドウ操作時および釣り中のリアルタイムターン消費を実装する。

実装方針:
- `RealTimeTurnTimer` を導入し、以下のタイミングでターンを加算:
  - NPCとの交流ウィンドウ開放中
  - インベントリ画面操作中
  - クエスト画面操作中
  - スキルツリー画面操作中
  - その他の別ウィンドウ操作中
  - 釣り中
- 1秒毎に1ターン消費（ゲーム内1ターン＝1秒の基準に準拠）
- ウィンドウ開放時にタイマー開始、閉鎖時にタイマー停止
- 累計リアルタイムターン数を `TurnCount` に加算

---

#### タスク 40: NPC・敵キャラクターへのターンコスト統一適用

**ファイル**: `src/RougelikeGame.Engine/TurnSystem/TurnManager.cs`, AI関連ファイル

全キャラクター（敵・NPC）が同じ `TurnCosts` 定数テーブルを使用するように改修する。

実装方針:
- AI行動決定時に使用するターンコストを `TurnCosts` 定数から取得するように統一
- NPC の移動・攻撃・スキル使用等がプレイヤーと同一のコスト体系で処理されることを保証
- TurnManager の `ProcessTurn` / `ExecutePlayerAction` を共通の行動コスト計算メソッドに統合

---

#### タスク 41: 変動ターンコスト（アイテム・スキル・呪文・領地）データ整備

**ファイル**: 各データ定義ファイル

個別ターンコストを持つ行動のデータ定義を確認・整備する。

| 行動 | 確認対象 | 対応 |
|------|---------|------|
| アイテム使用 | アイテム定義の `UseTurnCost` | 未設定のアイテムにデフォルト値を設定 |
| 料理 | レシピ定義の `CookTurnCost` | 未設定のレシピにデフォルト値を設定 |
| クラフト | レシピ定義の `CraftTurnCost` | 未設定のレシピにデフォルト値を設定 |
| 医療用具使用 | アイテム定義の `UseTurnCost` | 未設定のアイテムにデフォルト値を設定 |
| 付呪 | エンチャント定義の `EnchantTurnCost` | 未設定のエンチャントにデフォルト値を設定 |
| スキル使用 | スキル定義の `TurnCost` | 既存フィールドを確認 |
| 呪文詠唱 | 呪文定義の `CastTurnCost` | 既存フィールドを確認 |
| 領地移動 | 領地定義の `TravelTurnCost` | 未設定の領地にデフォルト値を設定 |

---

#### タスク 42: 行動ターン消費テスト

**ファイル**: `tests/RougelikeGame.Core.Tests/`, `tests/RougelikeGame.Gui.Tests/`

以下のテストを追加する。

| テスト項目 | 検証内容 |
|-----------|---------|
| 1ターン行動テスト | 移動・通常攻撃・武器着脱・アクセサリー着脱・ポーション使用・拾う・ドア開閉が1ターン消費 |
| 5ターン行動テスト | 周辺調査・投擲・射撃が5ターン消費 |
| 10ターン行動テスト | 階層移動・祈り・腕装備着脱・頭装備着脱が10ターン消費 |
| 20ターン行動テスト | 胴装備着脱・インベントリソートが20ターン消費 |
| シンボルマップ移動テスト | シンボルマップ移動が300ターン消費 |
| シンボルマップ進入テスト | Tキー進入が0ターン消費 |
| リアルタイム消費テスト | ウィンドウ操作中にリアルタイムでターンが経過 |
| NPC/敵統一テスト | NPC・敵がプレイヤーと同一のターンコストで行動 |
| 変動コストテスト | アイテム・スキル・呪文が個別設定のターンコストで実行 |
