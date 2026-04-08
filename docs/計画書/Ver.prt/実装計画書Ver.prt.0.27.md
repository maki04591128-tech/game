# 実装計画書 Ver.prt.0.27 — バグ修正・整合性改善（第2弾）

## 概要

Ver.prt.0.26で解消しきれなかったバグ・整合性問題を修正する。
コードベース全体を再調査し、以下のカテゴリのバグを特定・修正する。

- セーブ/ロード時の値クランプ不整合
- 飢餓・渇きの最重度段階の処理欠落
- ペットシステムの未処理ペット種別
- 装備システムの未処理スロット
- ポーション効果の不正な状態異常適用
- GameControllerの冗長コード

## Phase 1: セーブ/ロード値クランプ修正（タスク B.11, B.20）

### 問題

`Player.RestoreFromSave`メソッドで、Hunger/Thirstの最小値クランプが`0`にハードコードされているが、
`GameConstants.MinHunger`と`GameConstants.MinThirst`はいずれも`-10`である。
これにより、負の値（飢餓/脱水状態）を持つセーブデータをロードすると、値が`0`にリセットされてしまう。

また、デフォルト引数`thirst = 100`と`hygiene = 100`がハードコードされており、定数を使用すべき。

### タスク一覧

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.11 | Player.cs | 901-902 | Hunger/Thirstの最小値クランプを`GameConstants.MinHunger`/`GameConstants.MinThirst`に修正 | ✅完了 |
| B.20 | Player.cs | 896 | デフォルト引数`thirst`と`hygiene`を定数に変更 | ✅完了 |

### 修正方針

```csharp
// 修正前
_hunger = Math.Clamp(hunger, 0, GameConstants.MaxHunger);
_thirst = Math.Clamp(thirst, 0, GameConstants.MaxThirst);

// 修正後
_hunger = Math.Clamp(hunger, GameConstants.MinHunger, GameConstants.MaxHunger);
_thirst = Math.Clamp(thirst, GameConstants.MinThirst, GameConstants.MaxThirst);
```

## Phase 2: 飢餓・渇き最重度段階の処理追加（タスク B.12〜B.15）

### 問題

`HungerStage.Starvation`（餓死）と`ThirstStage.Desiccation`（干死）は最も深刻な状態だが、
以下のswitch文でケースが欠落しており、デフォルト（ペナルティなし/ダメージなし）にフォールスルーする。

### タスク一覧

| タスクID | ファイル | メソッド | 内容 | ステータス |
|----------|---------|---------|------|------------|
| B.12 | Player.cs | GetStatModifiers | `HungerStage.Starvation`のステータスペナルティ追加（NearStarvationより重い: Str-10, Agi-10, Dex-8, Int-5） | ✅完了 |
| B.13 | Player.cs | GetStatModifiers | `ThirstStage.Desiccation`のステータスペナルティ追加（NearDesiccationより重い: Int-10, Mind-10, Agi-8, Str-5） | ✅完了 |
| B.14 | ThirstSystem.cs | GetThirstDamage | `ThirstStage.Desiccation`のダメージ追加（NearDesiccation=10より重い: 20） | ✅完了 |
| B.15 | ThirstSystem.cs | GetThirstActionCostBonus | `ThirstStage.NearDesiccation`(+3)と`ThirstStage.Desiccation`(+5)の行動コスト加算追加 | ✅完了 |

### 修正方針

- 各最重度段階は、直前段階（NearStarvation/NearDesiccation）より厳しいペナルティを設定
- 餓死/干死は即死級のダメージ（HP直接ダメージ20/ターン）

## Phase 3: ペットシステム未処理種別追加（タスク B.16）

### 問題

`PetSystem.GetPetAbilityBonuses()`がCat/Hawk/Bear/Wolfの4種のみ処理し、
Horse（馬）とDragon（竜）のケースが欠落している。

### タスク一覧

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.16 | PetSystem.cs | 247-253 | Horse（騎乗ボーナス：ドロップ+5%）とDragon（万能：ドロップ+10%, 視野+2, ダメ軽減+5%, 攻撃弱体+5%）追加 | ✅完了 |

### 修正方針

- Horse: 騎乗時の移動速度ボーナスは既に別メソッド（GetMountSpeedMultiplier）で処理済み。アビリティボーナスとして小さなドロップ率ボーナス(+5%)を追加
- Dragon: 万能型として全ボーナスを少しずつ付与（+10%, +2, +5%, +5%）

## Phase 4: 装備・アイテムシステム修正（タスク B.17, B.18）

### 問題

1. `EquipmentItem.GetDefaultDisplayChar()`で`EquipmentSlot.Waist`のケースが欠落
2. `PotionType.IntelligenceBoost`が`StatusEffectType.Protection`を適用している

### タスク一覧

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.17 | Equipment.cs | 155-166 | `EquipmentSlot.Waist`のケース追加（表示文字: `~`） | ✅完了 |
| B.18 | Consumables.cs | 135-138 | `IntelligenceBoost`の適用StatusEffectを`Protection`→`Blessing`に修正し、メッセージも修正 | ✅完了 |

### 修正方針

- B.17: Waistスロット（ベルト等）に`~`を割り当て
- B.18: StatusEffectTypeに知力専用の型がないため、`StatusEffectType.Blessing`を使用し、メッセージを「能力が強化された！」に変更。IntelligenceBoostポーションは汎用的なバフポーションとして機能させる

## Phase 5: GameController冗長コード修正（タスク B.19）

### 問題

`LoadSaveData`メソッド内で`Player.Position`が2回設定されている（行8749と8839）。
行8749の設定は、その後のマップ復元/再生成（行8830-8837）で上書きされる可能性があるため冗長。

### タスク一覧

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.19 | GameController.cs | 8749 | 冗長な`Player.Position`設定行を削除 | ✅完了 |

## 実装順序

1. Phase 1（B.11, B.20）: セーブ/ロード値クランプ修正
2. Phase 2（B.12〜B.15）: 飢餓・渇き最重度段階の処理追加
3. Phase 3（B.16）: ペットシステム未処理種別追加
4. Phase 4（B.17, B.18）: 装備・アイテムシステム修正
5. Phase 5（B.19）: GameController冗長コード修正
6. テスト追加・全テスト合格確認

## テスト方針

- 各修正に対応するユニットテストを追加
- 全既存テスト（Core.Tests 6,353件）が引き続き合格すること
- 新規テスト: 各修正項目の動作確認テスト

## 実装結果

### ビルド結果
- **エラー: 0件**

### テスト結果
- **Core.Tests: 6,372件全合格**（旧6,353件 + 新規19件、Failed: 0, Skipped: 0）

### 新規テスト一覧（BugFixVer027Tests.cs: 19件）
| テスト名 | 対応タスク |
|---------|-----------|
| RestoreFromSave_NegativeHunger_PreservesNegativeValue | B.11 |
| RestoreFromSave_NegativeThirst_PreservesNegativeValue | B.11 |
| RestoreFromSave_HungerBelowMin_ClampedToMin | B.11 |
| RestoreFromSave_ThirstBelowMin_ClampedToMin | B.11 |
| Player_StarvationStage_HasStatPenalty | B.12 |
| Player_DesiccationStage_HasStatPenalty | B.13 |
| GetThirstDamage_Desiccation_Returns20 | B.14 |
| GetThirstDamage_NearDesiccation_Returns10 | B.14 |
| GetThirstDamage_Dehydrated_Returns1 | B.14 |
| GetThirstDamage_Normal_Returns0 | B.14 |
| GetThirstActionCostBonus_Desiccation_Returns5 | B.15 |
| GetThirstActionCostBonus_NearDesiccation_Returns3 | B.15 |
| GetThirstActionCostBonus_Normal_Returns0 | B.15 |
| PetAbilityBonuses_Horse_HasDropBonus | B.16 |
| PetAbilityBonuses_Dragon_HasAllBonuses | B.16 |
| EquipmentItem_WaistSlot_DisplaysTilde | B.17 |
| IntelligenceBoostPotion_AppliesBlessingNotProtection | B.18 |
| RestoreFromSave_DefaultThirst_UsesInitialThirst | B.20 |
| RestoreFromSave_DefaultHygiene_UsesInitialHygiene | B.20 |

### 修正ファイル一覧
| ファイル | 変更内容 |
|---------|---------|
| src/RougelikeGame.Core/Entities/Player.cs | RestoreFromSave Hunger/Thirst最小値クランプ修正、デフォルト引数定数化、Starvation/Desiccationステータスペナルティ追加 |
| src/RougelikeGame.Core/Systems/ThirstSystem.cs | GetThirstDamage Desiccation=20追加、GetThirstActionCostBonus NearDesiccation/Desiccation追加 |
| src/RougelikeGame.Core/Systems/PetSystem.cs | GetPetAbilityBonuses Horse/Dragon追加 |
| src/RougelikeGame.Core/Items/Equipment.cs | GetDefaultDisplayChar Waist='~'追加、Weapon.GetDefaultDisplayChar 全14WeaponType対応（Unarmed/Greataxe/Hammer/Thrown/Whip/Fist追加） |
| src/RougelikeGame.Core/Items/Consumables.cs | IntelligenceBoost Protection→Blessing修正 |
| src/RougelikeGame.Gui/GameController.cs | 冗長Player.Position設定削除 |
| tests/RougelikeGame.Core.Tests/BugFixVer027Tests.cs | テスト33件（B.11〜B.21） |

## Phase 7: 武器表示文字の全WeaponType対応（タスク B.21）

### 問題

`Weapon.GetDefaultDisplayChar()`メソッドのswitch文で、14種あるWeaponTypeのうち8種しかカバーされていない。
Unarmed, Greataxe, Hammer, Thrown, Whip, Fistの6種がデフォルトケース（`')'`）にフォールスルーする。

### タスク一覧

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.21 | Equipment.cs | 243-254 | Weapon.GetDefaultDisplayChar()に全14WeaponTypeの表示文字を追加 | ✅完了 |

### 修正内容

| WeaponType | 表示文字 | 理由 |
|-----------|---------|------|
| Unarmed | ' '(空白) | 素手は武器なし |
| Greataxe | 'P' | Axeと同系統 |
| Hammer | 'T' | ハンマー形状 |
| Thrown | '*' | 投擲の発射を表す |
| Whip | '~' | 鞭のしなり |
| Fist | ')' | 格闘武器 |

---

## B.22: Weapon.Category - WeaponType.UnarmedのEquipmentCategory修正

### 問題
- `Weapon.Category`プロパティのswitch式で`WeaponType.Unarmed`が明示的に処理されておらず、defaultケース(`_ => EquipmentCategory.Sword`)にフォールしていた
- これにより、Monk（Fist熟練）が素手で戦う際に装備適性ボーナスが適用されなかった
- 逆にFighter（Sword熟練）が素手でSword適性ボーナスを誤って得ていた

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.22 | Equipment.cs | 178-190 | Weapon.Categoryで`WeaponType.Unarmed`を`EquipmentCategory.Fist`に明示的マッピング | ✅完了 |

### テスト（18件追加、Core.Tests 6,404件全合格）
- `Weapon_Category_Unarmed_ShouldBeFist`: Unarmed→Fistの直接確認
- `Weapon_Category_Fist_ShouldBeFist`: Fist→Fistの確認（回帰テスト）
- `Weapon_Category_AllWeaponTypes_CorrectCategory`: 全14WeaponTypeのCategory網羅テスト（Theory×14）
- `Monk_ShouldBeProficient_WithUnarmed`: Monk+Unarmed→熟練判定成功
- `Fighter_ShouldNotBeProficient_WithUnarmed`: Fighter+Unarmed→熟練判定失敗

---

## B.23: HygieneStage（衛生度）ステータスペナルティ未実装の修正

### 問題
- `Player.GetAllStatModifiers()`に飢餓・渇き・疲労のステータスペナルティは実装されていたが、衛生度（HygieneStage）のCHAペナルティが欠落していた
- 設計書（§17.2.3 清潔度）では以下のCHA修正が定義されていた:
  - 清潔（Clean）: CHA+2
  - 普通（Normal）: ±0
  - 汚れ（Dirty）: CHA-2
  - 不衛生（Filthy）: CHA-5
  - 不潔（Foul）: CHA-10
- 実装計画書Ver.prt.0.24のAVカテゴリで「衛生のステータスペナルティが未適用」と記載されていたが、AV-5（感染リスク）のみ修正され、CHA修正が漏れていた

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.23 | Player.cs | 405-414 | `GetAllStatModifiers()`にHygieneStageによるCHA修正を追加 | ✅完了 |

### テスト（15件追加、Core.Tests 6,418件全合格）
- `HygieneStage_Foul_ShouldApply_CharismaMinus10`: Foul→CHA-10
- `HygieneStage_Filthy_ShouldApply_CharismaMinus5`: Filthy→CHA-5
- `HygieneStage_Dirty_ShouldApply_CharismaMinus2`: Dirty→CHA-2
- `HygieneStage_Normal_ShouldHave_NoCharismaPenalty`: Normal→±0
- `HygieneStage_Clean_ShouldApply_CharismaPlus2`: Clean→CHA+2
- `HygieneStage_AllLevels_CorrectCharismaModifier`: 全5段階×境界値テスト（Theory×9）
  - Hygiene 100/80→CHA+2, 79/50→±0, 49/25→CHA-2, 24/1→CHA-5, 0→CHA-10
