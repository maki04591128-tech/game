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

---

## B.34: CombatSystem ArmorClass がLight固定（ターゲット装備未参照）

### 問題
- `CombatSystem.BuildHitCheckParams()`で`ArmorClass`が`ArmorClass.Light`に固定されている
- ターゲット（Player）が重装鎧を装備していても、回避率が軽装ベース（10%）で計算される
- 重装（0%）・中装（5%）・ローブ（15%）・裸（20%）のいずれも適用されない

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.34 | CombatSystem.cs | 96-109 | Player ターゲットの Body 装備から ArmorType を取得し ArmorClass を導出。Enemy はデフォルト Light を維持 | ✅完了 |

## B.35: DamageCalculator クリティカル率 docstring 不整合

### 問題
- docstring: 「DEX × 0.3%」
- 実コード（K-3修正済み）: `DEX × 0.005`（= 0.5%/pt）
- 設計書にも旧値 0.3% が残存

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.35 | DamageCalculator.cs | 173 | docstring を「DEX × 0.5%」に修正、K-3修正注記追加 | ✅完了 |

## B.36: DamageCalculator 魔法ダメージ docstring 不整合

### 問題
- docstring: 「魔法防御 × 0.3」
- 実コード（K-1修正済み）: `魔法防御 × 0.5`

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.36 | DamageCalculator.cs | 76 | docstring を「魔法防御 × 0.5」に修正、K-1修正注記追加 | ✅完了 |

## B.37: 設計書のクリティカル率・魔法防御数値の更新

### 問題
- 06_戦闘システム設計書.md: クリティカル率「DEX × 0.3%」、魔法防御「× 0.3」が旧値のまま
- 05_キャラクター設計書.md: クリティカル率「DEX × 0.3%」が旧値のまま

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.37a | 06_戦闘システム設計書.md | 94 | 魔法防御「× 0.3」→「× 0.5」、K-1修正注記追加 | ✅完了 |
| B.37b | 06_戦闘システム設計書.md | 159 | クリティカル率「DEX × 0.3%」→「DEX × 0.5%」、K-3修正注記追加 | ✅完了 |
| B.37c | 05_キャラクター設計書.md | 663 | クリティカル率「DEX × 0.3%」→「DEX × 0.5%」、K-3修正注記追加 | ✅完了 |

## B.38: Enums.cs に重複 ItemType enum（死にコード）

### 問題
- `RougelikeGame.Core.ItemType`（Enums.cs）と `RougelikeGame.Core.Items.ItemType`（Item.cs）が重複
- Enums.cs側は完全未使用（参照0件）。定義が異なるため混乱の原因となる

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.38 | Enums.cs | 207-217 | 重複 `ItemType` enum 定義を削除。Items/Item.cs 側を正規定義として維持 | ✅完了 |

## B.39: Player.ExecuteAction() に Interact/Search case 欠落

### 問題
- `TurnActionType` enum に `Interact`/`Search` が定義されている
- `Player.ExecuteAction()` の switch 文にこれらの case がない
- 外部処理（GameController側）で実行されるが、switch 網羅性が不完全

### 修正内容

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.39 | Player.cs | 772-779 | `Interact` / `Search` の空 case をコメント付きで追加 | ✅完了 |

### テスト（B.34〜B.39合計9件追加、Core.Tests 6,447件全合格）
- `B34_DeriveArmorClass_PlayerWithPlateArmor_ReturnsHeavy`: Plate→HeavyArmor
- `B34_DeriveArmorClass_PlayerWithChainmailArmor_ReturnsMedium`: Chainmail→MediumArmor
- `B34_DeriveArmorClass_PlayerWithLeatherArmor_ReturnsLight`: Leather→LightArmor
- `B34_DeriveArmorClass_PlayerWithRobe_ReturnsRobe`: Robe→Robe
- `B34_DeriveArmorClass_PlayerWithNoArmor_BodySlotIsNull`: 裸→Body枠null
- `B35_B36_DamageCalculator_CriticalCheck_DEXContribution_MatchesK3Fix`: DEX/LUKクリティカル統一検証
- `B38_ItemType_InItemsNamespace_HasExpectedValues`: Items.ItemType値検証
- `B39_ExecuteAction_InteractType_DoesNotThrow`: Interact例外なし
- `B39_ExecuteAction_SearchType_DoesNotThrow`: Search例外なし

---

## Phase 7: B.40〜B.42 追加バグ調査・修正

### B.40: ResourceSystem.cs 重複CharacterClass enum統一

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.40 | ResourceSystem.cs | 417-438 | 重複`CharacterClass` enum削除（`Warrior`→Core層`Fighter`に統一） | ✅完了 |
| B.40 | ResourceSystem.cs | 34,95 | switch内`Warrior`→`Fighter`修正 | ✅完了 |
| B.40 | Program.cs | 837-840 | `Engine.Combat.CharacterClass.Warrior`→`CharacterClass.Fighter`修正 | ✅完了 |
| B.40 | EngineUnitTests.cs | 99,104,118 | テスト内参照修正 | ✅完了 |
| B.40 | CombatSystemTests.cs | 443,461 | テスト内参照修正 | ✅完了 |

### B.41: InscriptionSystem.cs 除算ゼロ対策

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.41 | InscriptionSystem.cs | 55 | `RequiredLevel`が0の場合`Math.Max(1, ...)`でガード追加 | ✅完了 |

### B.42: Enemy.ExecuteAction Interact case追加

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.42 | Enemy.cs | 385-389 | `TurnActionType.Interact`を待機系caseに追加 | ✅完了 |

### テスト（B.40〜B.42合計7件追加、Core.Tests全合格）
- `B40_ResourceSystem_UsesCoreFighterClass_HpCalculation`: Fighter HP成長値=15確認
- `B40_ResourceSystem_UsesCoreFighterClass_MpCalculation`: Fighter MP成長値=2確認
- `B40_ResourceSystem_AllCoreClasses_HaveValidHpBonus`: 全CharacterClassのHP計算正常確認
- `B41_InscriptionSystem_ZeroRequiredLevel_NoDivisionByZero`: RequiredLevel=0除算ゼロ回避
- `B41_InscriptionSystem_LowRequiredLevel_ProgressCalculation`: 低RequiredLevel進捗計算
- `B42_Enemy_ExecuteAction_InteractDoesNotThrow`: Interact例外なし
- `B42_Enemy_ExecuteAction_AllActionTypes_NoThrow`: 全ActionType例外なし

---

## Phase 8: 追加調査バグ修正（B.43〜B.46）

### B.43: InscriptionSystem.GetPartialText 空文字列防御

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.43 | InscriptionSystem.cs | 93-98 | `fullText`が空文字列の場合に`fullText[..1]`でIndexOutOfRangeException → `string.IsNullOrEmpty`チェック追加、`revealChars`上限クランプ追加 | ✅完了 |

### B.44: DamageCalculator 魔法クリティカルコメント修正

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.44 | DamageCalculator.cs | 99 | コメント「魔法はクリティカルなし」→「K-2: クリティカル判定はCombatSystem側で適用」に修正（CombatSystem.csでisCritical時1.3倍適用済み） | ✅完了 |

### B.45: BasicBehaviors.BerserkerBehavior MaxHp除算ゼロ対策

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.45 | BasicBehaviors.cs | 427-432 | `enemy.MaxHp`で除算する前に`MaxHp <= 0`チェック追加 | ✅完了 |

### B.46: Enemy.ExecuteAction UseItem MaxHp除算ゼロ対策

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.46 | Enemy.cs | 401-407 | `MaxHp / 2`および`MaxHp / 10`の前に`MaxHp > 0`チェック追加 | ✅完了 |

### テスト（B.43〜B.46合計6件追加、BugFixVer027全107件合格）
- `InscriptionSystem_GetPartialText_EmptyDecodedText_NoException`: 空文字列で例外なし
- `InscriptionSystem_GetPartialText_HighProgress_NoOverflow`: 範囲外アクセスなし
- `DamageCalculator_MagicalDamage_IsCriticalFalse_InResult`: DamageCalculatorのIsCritical=false確認
- `BerserkerBehavior_IsApplicable_NoException`: BerserkerBehavior除算ゼロ安全
- `Enemy_ExecuteAction_UseItem_NoException`: UseItem例外なし
- `Enemy_ExecuteAction_UseItem_HealsWhenLowHp`: HP低下時回復確認

---

## Phase 10: B.47〜B.51 コードベース全体調査による追加バグ修正

### B.47: HungerState.Satiated→Full統一（Core層HungerStageとの名前不整合）

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.47 | ResourceSystem.cs | 214,235,458 | `HungerState.Satiated`→`HungerState.Full`に統一（Core層`HungerStage.Full`と名前一致） | ✅完了 |

### B.48: PotionType.StaminaSuper欠落（Healing/Manaとの対称性確保）

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.48 | Consumables.cs | 72,179 | `PotionType.StaminaSuper` enum値追加＋switch case追加（HealingSuper/ManaSuperと対称性確保） | ✅完了 |

### B.49: ArmorType.Shield明示case追加（DeriveArmorClass）

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.49 | CombatSystem.cs | 146-154 | `DeriveArmorClass`で`ArmorType.Shield`の明示case追加（default fallthrough防止） | ✅完了 |

### B.50: Program.cs TurnCostModifier除算ゼロ対策

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.50 | Program.cs | 798 | `1 / effect.TurnCostModifier`の前に`> 0f`ガード追加 | ✅完了 |

### B.51: RoomType.Entrance明示case追加（DecorateRoom）

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.51 | RoomCorridorGenerator.cs | 131-133 | `DecorateRoom`で`RoomType.Entrance`の明示case追加（装飾なし） | ✅完了 |

### テスト（B.47〜B.51合計10件追加、BugFixVer027全117件合格）
- `B47_HungerState_Full_MatchesHungerStage`: HungerState.Full値が80-99範囲で返されること
- `B47_HungerState_Full_BoundaryValues`: 79→Normal、100→Overeating境界値確認
- `B47_HungerState_Full_HungerEffect_ActionCostBonus1`: 満腹時ActionCostBonus=1
- `B48_StaminaSuper_PotionType_Exists`: StaminaSuper enum存在確認
- `B48_StaminaSuper_Potion_RestoresSp`: 超スタミナポーションSP回復確認
- `B49_DeriveArmorClass_AllArmorTypes_Handled`: ArmorType.Shield含む全値確認
- `B49_DeriveArmorClass_ShieldEquipped_NoException`: CombatSystem経由でDeriveArmorClass例外なし
- `B50_TurnCostModifier_Zero_NoException`: TurnCostModifier=0で除算除外
- `B50_TurnCostModifier_Negative_NoException`: 負値で除算除外
- `B51_RoomType_Entrance_DecorateRoom_NoException`: Entrance部屋装飾例外なし

---

## Phase 7: B.52〜B.58 計算式・設計書準拠修正

### B.52/B.58: ResourceSystem.CalculateRequiredExp 定数化

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.52/B.58 | ResourceSystem.cs | 293-299 | `level>=99`→`MaxLevel`、`1.5`→`GameConstants.ExpGrowthRate`に統一 | ✅完了 |

### B.53: ThirstActionCostBonus GameController適用

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.53 | GameController.cs | 1597-1614,5001-5011 | ThirstActionCostBonusを移動/攻撃・スキル両箇所に追加 | ✅完了 |

### B.56: Character.TakeDamage Elemental明示コメント

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.56 | Character.cs | 85-92 | Elemental caseに明示コメント追加 | ✅完了 |

### B.57: SlightlyThirsty 30%確率コスト+1

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.57 | GameController.cs | (移動/スキル) | SlightlyThirsty段階で30%確率コスト+1追加 | ✅完了 |

### テスト（B.52〜B.58合計10件追加、BugFixVer027全127件合格）

---

## Phase 8: B.59〜B.63 追加バグ修正

### B.59: MainWindow.xaml.cs MaxHp除算ゼロ対策

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.59 | MainWindow.xaml.cs | 387 | `hpRatio`計算時に`MaxHp > 0`ガード追加 | ✅完了 |

### B.60: GameRenderer.cs map.Width/Height除算ゼロ対策

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.60 | GameRenderer.cs | 362-363 | ミニマップスケール計算で`map.Width <= 0 || map.Height <= 0`の場合早期return | ✅完了 |

### B.61: RoomCorridorGenerator.DecorateRoom - RoomType未対応case追加

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.61 | RoomCorridorGenerator.cs | 136-142 | Prison, Storage, Secret, Shop, TrapRoom の明示case追加 | ✅完了 |

### B.62: GrowthSystem.CalculateTotalExpForLevel 除算ゼロ対策

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.62 | GrowthSystem.cs | 162 | `raceExpMultiplier <= 0`の場合1.0にフォールバック | ✅完了 |

### B.63: Enemy.DecidePatrolAction PatrolIndex境界値チェック

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.63 | Enemy.cs | 313 | `PatrolIndex < 0 || PatrolIndex >= PatrolRoute.Count`の場合0にリセット | ✅完了 |

### テスト（B.59〜B.63合計16件追加、BugFixVer027全143件合格、Core.Tests全6496件合格）
- `B61_DecorateRoom_AllRoomTypes_NoException` (×11): 全RoomTypeで例外なし
- `B62_CalculateTotalExpForLevel_ZeroMultiplier_NoException`: raceExpMultiplier=0で例外なし
- `B62_CalculateTotalExpForLevel_NegativeMultiplier_NoException`: 負値で例外なし
- `B62_CalculateTotalExpForLevel_NormalMultiplier_ReturnsPositive`: 正常値で正の結果
- `B63_Enemy_PatrolIndex_OutOfBounds_SetTo99`: 範囲外PatrolIndex設定確認
- `B63_Enemy_PatrolIndex_NegativeValue_SetToMinus1`: 負値PatrolIndex設定確認

## Phase 9: B.64〜B.67 switch文不足・属性エンチャント修正

### B.64: RoomCorridorGenerator.AddRandomDecoration case 3 明示化

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.64 | RoomCorridorGenerator.cs | 228 | `random.Next(4)`のcase 3（装飾なし）を明示的に追加 | ✅完了 |

### B.65: TravelEventWindow.xaml.cs Ambush case追加

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.65 | TravelEventWindow.xaml.cs | 54-58 | `TravelEventType.Ambush`のボタン表示case追加（応戦/交渉/逃げる） | ✅完了 |

### B.66: GameController.cs EnchantmentType 属性ダメージ付与case追加

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.66 | GameController.cs | 2006-2037 | Fire/Ice/Lightning/Poison/Holy/DarkDamageの6属性エンチャントでDamage.Magical適用+状態異常付与。パッシブ系(ExpBoost等)はdefaultで処理 | ✅完了 |

### B.67: GameController.cs ResolveRandomEvent 全RandomEventType対応

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.67 | GameController.cs | 9784-9816 | Trap(罠ダメージ), Ruins(メッセージ), MysteriousItem(メッセージ), MonsterHouse(敵生成), CursedRoom(メッセージ), BlessedRoom(HP/MP回復), HiddenShop(メッセージ)の7caseを追加 | ✅完了 |

### テスト（B.64〜B.67合計26件追加、BugFixVer027全169件合格、Core.Tests全6522件合格）
- `AddRandomDecoration_Case3_IsExplicitlyDefined`: case 3明示化確認
- `TravelEventType_Ambush_ExistsInEnum`: Ambush列挙値存在確認
- `TravelEventType_AllValues_AreDefined`: 全6値の存在確認
- `EnchantmentType_AllDamageTypes_ExistInEnum` (×9): 全ダメージ系エンチャント列挙値確認
- `EnchantmentType_ElementalDamage_CanCreateMagicalDamage` (×6): 属性別Damage.Magical作成確認
- `RandomEventType_AllValues_AreDefined`: 全15値の存在確認
- `RandomEventType_NewlyHandledValues_ExistInEnum` (×7): 新規対応7イベントの列挙値確認

### B.68: GameController.cs CriticalBoostエンチャント効果実装

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.68 | GameController.cs | 1918-1936 | CriticalBoostエンチャント装備時、クリティカル判定で非クリティカルの場合に+10%/個の追加判定。クリティカル昇格時はダメージ1.5倍 | ✅完了 |

### B.69: GameController.cs SpeedBoostエンチャント効果実装

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.69 | GameController.cs | 1707-1714 | SpeedBoostエンチャント装備時、攻撃コストを-15%/個軽減（最低0.3倍、最低コスト1） | ✅完了 |

### B.70: GameController.cs DefenseBoostエンチャント効果実装

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.70 | GameController.cs | 2598-2607 | DefenseBoostエンチャント装備時、被ダメージから-5/個を軽減（最低ダメージ1） | ✅完了 |

### B.71: GameController.cs Thornsエンチャント効果実装

| タスクID | ファイル | 行 | 内容 | ステータス |
|----------|---------|-----|------|------------|
| B.71 | GameController.cs | 2613-2624 | Thornsエンチャント装備時、被ダメージの20%/個を敵に反射（Damage.Pure、最低1ダメージ） | ✅完了 |

### テスト（B.68〜B.71合計15件追加、BugFixVer027全184件合格、Core.Tests全6537件合格）
- `CriticalBoost_IsValidForWeaponOnly`: 武器にのみ適用可能確認
- `CriticalBoost_Definition_HasCorrectEffectValue`: EffectValue=0.1確認
- `Damage_CanBeCreatedWithIsCriticalTrue`: IsCritical=trueのDamage作成確認
- `CombatResult_CanPromoteToCritical`: CombatResultクリティカル昇格確認
- `SpeedBoost_IsValidForWeaponAndArmor`: 武器/防具両方に適用可能確認
- `SpeedBoost_Definition_HasCorrectEffectValue`: EffectValue=0.15確認
- `SpeedBoost_CanBeAppliedToWeapon`: 武器に適用可能確認
- `DefenseBoost_IsValidForArmorOnly`: 防具にのみ適用可能確認
- `DefenseBoost_Definition_HasCorrectEffectValue`: EffectValue=5.0確認
- `DefenseBoost_CanBeAppliedToArmor`: 防具に適用可能確認
- `Thorns_IsValidForArmorOnly`: 防具にのみ適用可能確認
- `Thorns_Definition_HasCorrectEffectValue`: EffectValue=0.2確認
- `Thorns_CalculateEnchantedDamageBonus_ReturnsReflectionDamage`: 反射ダメージ計算確認
- `Thorns_CanBeAppliedToArmor`: 防具に適用可能確認
- `AllEnchantmentTypes_HaveDefinitions`: 全EnchantmentTypeに定義存在確認
