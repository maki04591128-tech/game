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

---

## B.24: Food.Use() 渇き回復（HydrationValue）未実装

### 問題
- `Food`クラスに`HydrationValue`プロパティが定義され、`ItemFactory.CreateWater()`（HydrationValue=1）と`ItemFactory.CreateCleanWater()`（HydrationValue=2）で設定されているが、`Food.Use()`メソッドで`player.ModifyThirst()`が一切呼ばれていなかった
- 水や清水を飲んでも渇きが回復しないバグ

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.24 | Consumables.cs | 259-262 | `Food.Use()`に`HydrationValue > 0`時の`ModifyThirst()`呼び出しを追加 | ✅完了 |
| B.24 | Item.cs | 166 | `ItemEffectType`に`RestoreThirst`を追加 | ✅完了 |

---

## B.25: Food.Use() 腐食食で毒を受けた際に満腹度回復がスキップされるバグ

### 問題
- 腐った食べ物を食べて毒を受けた場合（30%確率）、早期returnにより`ModifyHunger()`が呼ばれず満腹度が回復しなかった
- メッセージには「満腹度+{nutrition}」と表示されるが、実際には回復していなかった

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.25 | Consumables.cs | 233-297 | 腐食食で毒を受けた際もModifyHunger/ModifyThirstを呼んでからメッセージ返却するよう修正 | ✅完了 |

### テスト（B.24+B.25合計9件追加、Core.Tests 6,427件全合格）
- `FoodUse_Water_RestoresThirst`: 水を飲んだら渇き+1回復
- `FoodUse_CleanWater_RestoresThirstAndHp`: 清水→渇き+2、HP回復
- `FoodUse_NormalFood_NoThirstRestore`: HydrationValue=0の食べ物は渇き変化なし
- `FoodUse_Water_MessageContainsThirst`: 水使用メッセージに「渇き」を含む
- `FoodUse_RottenFoodPoisoned_StillRestoresHunger`: 腐食食で毒→満腹度回復する
- `FoodUse_RottenFoodNotPoisoned_RestoresHunger`: 腐食食で毒なし→満腹度回復
- `FoodUse_RottenWaterPoisoned_RestoresHungerAndThirst`: 腐った水で毒→満腹度＋渇き両方回復
- `FoodUse_RottenFoodPoisoned_HasPoisonStatus`: 腐食食で毒→Poison状態異常付与
- `FoodUse_RottenFoodPoisoned_NutritionIsHalved`: 腐食食の栄養値は半減

---

## B.26: Food.Use() 渇き回復二重適用バグ

### 問題
- `Food.Use()`内で`ModifyThirst(HydrationValue)`が呼ばれた後、`GameController.cs`でも`ModifyThirst(HydrationValue * 10)`が重複して呼ばれていた
- 食料アイテムの渇き回復が二重に適用され、さらにGameController側では10倍の値が使われていた

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.26 | GameController.cs | 3854-3858 | 重複`ModifyThirst()`呼び出しを削除（メッセージ表示のみ残す） | ✅完了 |

---

## B.27: Companion HP割合計算の除算ゼロ対策

### 問題
- 回復魔法の味方ターゲット選択時に`(float)c.Hp / c.MaxHp`で`MaxHp`が0の場合に除算ゼロエラーが発生する可能性があった

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.27 | GameController.cs | 5639-5643 | `c.MaxHp > 0`のフィルタ条件を追加 | ✅完了 |

---

## B.28: 自動探索HP割合計算の除算ゼロ対策

### 問題
- `CheckAutoExploreStop()`内の`Player.CurrentHp / Player.MaxHp`で`MaxHp`が0の場合に除算ゼロエラーが発生する可能性があった

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.28 | GameController.cs | 10743 | `Player.MaxHp > 0`チェックを追加（他のHP割合計算と統一） | ✅完了 |

---

## B.29: HygieneStage enum コメント修正

### 問題
- `HygieneStage.Filthy`のコメントが「不衛生」だが、設計書§17.2.3では「不潔」
- `HygieneStage.Foul`のコメントが「不潔」だが、設計書§17.2.3では「悪臭」

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.29 | Enums.cs | 1219-1222 | `Filthy`コメント: 不衛生→不潔、`Foul`コメント: 不潔→悪臭 | ✅完了 |

### テスト（B.26〜B.29合計5件追加、Core.Tests 6,432件全合格）
- `B26_FoodUse_HydrationAppliedOnlyOnce`: 渇き回復が1回だけ適用される
- `B26_FoodUse_WithHealAndHydration_ThirstRestoredOnce`: HP回復＋渇き回復の食料で渇きが1回だけ回復
- `B26_FoodUse_NoHydration_ThirstUnchanged`: HydrationValue=0の食料は渇き変化なし
- `B29_HygieneStage_FilthyHasCorrectPenalty`: Filthy段階のCHAペナルティ-5確認
- `B29_HygieneStage_FoulHasCorrectPenalty`: Foul段階のCHAペナルティ-10確認

---

## B.30: ThirstSystem Desiccation段階 除算ゼロ対策

### 問題
- `ThirstSystem.GetThirstModifiers(ThirstStage.Desiccation)`が`(0f, 0f, 0f)`を返す
- `GameController.cs`で`actionCost / thirstMoveMod`を計算する際、`thirstMoveMod=0f`で除算ゼロが発生
- `(int)float.PositiveInfinity`はC#で未定義動作（オーバーフロー）

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.30 | GameController.cs | 1599 | `thirstMoveMod > 0f`チェックを追加（fatigueModと同一パターン） | ✅完了 |

---

## B.31: armorSpeedMod 除算ゼロ対策

### 問題
- `armorSpeedMod`は複数装備の`SpeedModifier`の積で計算される
- `SpeedModifier=0`の防具が存在した場合、`armorSpeedMod=0`で除算ゼロが発生する可能性

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.31 | GameController.cs | 1577 | `armorSpeedMod > 0f`チェックを追加 | ✅完了 |

---

## B.32: Accessory.Category がEquipmentCategory.Swordのまま

### 問題
- `Accessory`クラスが`Category`をオーバーライドしておらず、基底クラスの`EquipmentCategory.Sword`がデフォルト値
- 装備時の職業適性判定で、アクセサリが剣カテゴリとして判定され、剣非習熟の職業で「非習熟：攻撃力低下」の誤メッセージが表示される

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.32a | Enums.cs | 403 | `EquipmentCategory`に`Accessory`値を追加 | ✅完了 |
| B.32b | Equipment.cs | 335 | `Accessory`クラスに`Category => EquipmentCategory.Accessory`オーバーライド追加 | ✅完了 |
| B.32c | GameController.cs | 3888 | アクセサリ装備時は常に適性あり判定（`equipItem is Accessory`チェック） | ✅完了 |

---

## B.33: DamageCalculator 魔法防御係数コメント不整合

### 問題
- 物理防御係数: 0.65（K-4で引上げ）
- 魔法防御係数: 0.5
- コメント「K-1: 魔法防御係数を物理と統一」だが実際は不一致

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.33 | DamageCalculator.cs | 89 | コメント修正: 「物理と統一」→「魔法防御係数0.5（物理0.65とは別調整）」 | ✅完了 |

### テスト（B.30〜B.33合計7件追加、Core.Tests 6,439件全合格）
- `B30_ThirstDesiccation_ModifiersReturnZero`: Desiccation段階のModifiersが(0,0,0)
- `B30_ThirstNearDesiccation_ModifiersReturnPositive`: NearDesiccation段階のModifiersが正
- `B30_ThirstActionCostBonus_DesiccationHasHighestCost`: Desiccation行動コスト加算が最大
- `B32_Accessory_HasAccessoryCategory`: アクセサリのCategoryがAccessory
- `B32_Accessory_NotSwordCategory`: アクセサリがSwordカテゴリでない
- `B31_ArmorSpeedModifier_DefaultIsOne`: Armor SpeedModifierデフォルト値1.0f
