# 実装計画書 Ver.prt.0.24 — ゲームプレイ違和感・不整合の洗い出しと修正

**ステータス**: 🔍 洗い出し中（修正はユーザー判断後に実施）
**目的**: ゲーム内で各システムの要素に触れる際に違和感がある不整合を網羅的に調査し、修正対象を確定する
**前提**: 全90システム統合済み（Ver.prt.0.23完了時点）、Core.Tests 5,539件全合格

---

## 調査方法

以下のカテゴリ別にソースコード全体を監査し、名称・説明・実際の挙動の不一致を洗い出した。

---

## カテゴリA: 商人・ショップシステム（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| A-1 | `NpcType.Alchemist`（薬師）がEnum定義済みだが、どのNPCにも割り当てられていない | 高 | `Enums.cs` | |
| A-2 | `NpcType.MagicShopkeeper`（魔法商人）がEnum定義済みだが、どのNPCにも割り当てられていない | 高 | `Enums.cs` | |
| A-3 | `NpcType.BlackMarketDealer`（闇商人）がEnum定義済みだが、どのNPCにも割り当てられていない | 高 | `Enums.cs` | |
| A-4 | 「薬草師リーナ」のNpcTypeが汎用`Shopkeeper`。説明は「森の薬草に詳しい若い女性」であり`Alchemist`型が適切 | 高 | `NpcSystem.cs:41` | |
| A-5 | 「行商人ハッサン」の品揃えが説明と不一致。「珍しい品物を扱う」と記載されるが実際は他の雑貨店と同じ汎用品（価格30%増のみ） | 中 | `NpcSystem.cs:58`, `WorldMapSystem.cs` | |
| A-6 | マナポーションが雑貨店と魔法店の両方で販売されており差別化なし | 中 | `WorldMapSystem.cs:548,625` | |
| A-7 | 識別の巻物が雑貨店と魔法店の両方で販売。魔法店専売であるべき | 中 | `WorldMapSystem.cs:553,627` | |
| A-8 | 山岳武器店のアイテムID=`weapon_greatsword`（大剣）だが、表示名は「ミスリルダガー」（短剣）。価格800Gは大剣価格帯 | 高 | `WorldMapSystem.cs:589` | |
| A-9 | NPC型（NpcType）と施設型（FacilityType）の間にマッピングが存在しない。商人NPCがどの店を運営するか紐付けなし | 設計課題 | `NpcSystem.cs`, `WorldMapSystem.cs` | |

---

## カテゴリB: アイテム・消耗品システム（13件）

### B-a: 巻物の未実装（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| B-1 | `ScrollType.Freeze`（凍結の巻物）— ファクトリで生成可能だが`Scroll.Use()`にハンドラなし。「何も起こらなかった」と表示 | 致命的 | `Consumables.cs:274-318`, `ItemFactory.cs:750-762` | |
| B-2 | `ScrollType.Sanctuary`（聖域の巻物）— 同上。「周囲に結界を張り、敵を退ける」と記載されるが実装なし | 致命的 | `Consumables.cs:274-318`, `ItemFactory.cs:801-812` | |
| B-3 | `ScrollType.Summon`（召喚の巻物）— Enum定義のみ。ファクトリメソッドもハンドラもなし | 高 | `Consumables.cs:350` | |
| B-4 | `ScrollType.Return`（帰還の巻物）— Enum定義のみ。ハンドラなし | 高 | `Consumables.cs` | |

### B-b: ポーションの未実装（8件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| B-5 | `PotionType.StrengthBoost`（筋力増強薬）— ファクトリで生成可能だが`Potion.Use()`にハンドラなし | 致命的 | `ItemFactory.cs:511-535`, `Consumables.cs:47-101` | |
| B-6 | `PotionType.AgilityBoost`（敏捷増強薬）— 同上 | 致命的 | `ItemFactory.cs:511-535`, `Consumables.cs:47-101` | |
| B-7 | `PotionType.IntelligenceBoost`（知力増強薬）— Enum定義のみ。ファクトリもハンドラもなし | 高 | `Consumables.cs:109-147` | |
| B-8 | `PotionType.Invisibility`（透明化薬）— ファクトリで生成可能だがUse()ハンドラなし | 致命的 | `Consumables.cs:47-101` | |
| B-9 | `PotionType.FireResistance`（耐火薬）— ファクトリで生成可能だがUse()ハンドラなし | 致命的 | `Consumables.cs:47-101` | |
| B-10 | `PotionType.ColdResistance`（耐寒薬）— ファクトリで生成可能だがUse()ハンドラなし | 致命的 | `Consumables.cs:47-101` | |
| B-11 | `PotionType.Poison`（毒薬）— Enum定義のみ。ファクトリもハンドラもなし | 高 | `Consumables.cs:109-147` | |
| B-12 | `PotionType.Confusion`（混乱薬）— Enum定義のみ。ファクトリもハンドラもなし | 高 | `Consumables.cs:109-147` | |

### B-c: 装備・スキル関連（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| B-13 | 「鼓舞の歌」スキルのターゲットが`SkillTarget.Self`（自分のみ）。名称「歌」は範囲効果を暗示するが自己バフのみ | 中 | `SkillSystem.cs:141` | |

---

## カテゴリC: 装備カテゴリ・ステータス効果（6件）

### C-a: 装備カテゴリの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| C-1 | `WeaponType.Spear`（槍）の`EquipmentCategory`が`Sword`にマッピング。EquipmentCategoryにSpearが存在しない | 中 | `Equipment.cs:164-175` | |
| C-2 | `WeaponType.Whip`（鞭）の`EquipmentCategory`が`Sword`にマッピング。EquipmentCategoryにWhipが存在しない | 中 | `Equipment.cs:164-175` | |

### C-b: ステータス効果の未実装（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| C-3 | `StatusEffect.Charm`（魅了）— Enum定義のみ。適用ロジック・効果処理なし | 高 | `Enums.cs:232` | |
| C-4 | `StatusEffect.Madness`（狂気）— 同上 | 高 | `Enums.cs:233` | |
| C-5 | `StatusEffect.Petrification`（石化）— 同上 | 高 | `Enums.cs:234` | |
| C-6 | `StatusEffect.InstantDeath`（即死）— 同上 | 高 | `Enums.cs:235` | |

---

## カテゴリD: 敵・ダンジョンシステム（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| D-1 | ダンジョンボスのテーマ不一致 — 王都カタコンベ（アンデッドテーマ）のボスがキングスライム、海岸沈没船のボスがスケルトンロード等 | 中 | `EnemyFactory.cs:790-829` | |
| D-2 | 人型敵のテリトリー不整合 — オーク（Humanoid）が山岳、ダークエルフ（Humanoid）が南方に配置。テリトリーテーマと不一致 | 中 | `EnemyFactory.cs:296-339` | |
| D-3 | スケルトンが王都と海岸で重複。海岸は水棲テーマだが陸上のスケルトンが配置 | 低 | `EnemyFactory.cs:856-859` | |
| D-4 | 森の精霊（Common）のステータスがElite相当（MND=15、平均9.3）だが経験値25は最低値 | 中 | `EnemyFactory.cs:405-417` | |
| D-5 | 巨大蟹（Common）のRES=18がElite級。防御力がトレント（Elite）を超えるが経験値25のまま | 中 | `EnemyFactory.cs:494-506` | |

---

## カテゴリE: NPC対話・テキストの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| E-1 | 宿屋主人が「料理する」選択肢を提供。宿屋の役割（休息・宿泊）と矛盾。料理人NPCまたは厨房施設が適切 | 中 | `GameController.cs:5169` | |
| E-2 | 訓練師が「転職する」選択肢を提供。転職はギルド受付の機能であり重複。訓練師の口上にも転職の言及なし | 中 | `GameController.cs:5179` | |
| E-3 | 一般商人が「裏の商品を見る」「密輸を依頼する」を直接提供。正規商人と闇商人の役割が混在 | 中 | `GameController.cs:5149-5150` | |

---

## カテゴリF: 宗教システムの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| F-1 | 「無神論」（Atheism）が信仰ポイント制の恩恵体系を持つ。無神論は信仰を否定する思想であり、信仰ポイントで強化される仕組みは矛盾 | 中 | `ReligionSystem.cs:328-345` | |
| F-2 | 全宗教の祈り効果が同一（信仰度+2のみ）。各神殿の特色が祈り効果に反映されていない | 中 | `ReligionSystem.cs:550-566` | |
| F-3 | 「混沌神カオス」の名称が「Chaos God Chaos」の直訳で個性不足。恩恵も「ランダムバフ」のみで具体性に欠ける | 低 | `ReligionSystem.cs:307` | |

---

## カテゴリG: テリトリー・ワールドマップの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| G-1 | 海岸テリトリーの説明に「漁村が点在する」と記載されるが、FacilityTypeに釣り・漁業関連施設が存在しない | 中 | `WorldMapSystem.cs:156-159` | |
| G-2 | 森林テリトリーに「薬師の村」サブロケーションと薬草師NPCが存在するが、薬草・錬金術関連施設がない | 中 | `WorldMapSystem.cs:52,149` | |
| G-3 | 南方テリトリーの説明が「荒野と古代遺跡」だが、実際のサブロケーションに「雪原の村」「狩人の集落」が含まれ、テーマが不一致 | 中 | `WorldMapSystem.cs:83-91,161` | |

---

## カテゴリH: ペットシステムの不整合（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| H-1 | キャット（Cat）ペットの能力が「幸運（ドロップ率UP）」。猫と幸運・ドロップ率の関連性が薄く、「夜間視力」「身軽さ」等が適切 | 低 | `PetSystem.cs:46` | |

---

## カテゴリI: ArmorType不整合（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| I-1 | 鉄の兜・鉄のブーツに`ArmorType.Plate`（板金鎧）が設定。兜・靴にはLeatherやChainmailが適切 | 低 | `ItemFactory.cs:343-394` | |

---

## カテゴリJ: 経験値・レベルカーブの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| J-1 | 経験値カーブが指数関数`100×1.5^(Lv-1)`で急勾配すぎる。Lv25で168万exp必要、Lv30で1278万exp必要。最強ボスexp=2000なので Lv25到達にボス841体撃破が必要。**実質的な最大レベルがLv18-20** | 致命的 | `Player.cs:389-390`, `GameConstants.cs` | |
| J-2 | 推奨レベル関数`GetRecommendedLevel()`がフロア20でLv35、フロア30でLv45を返すが、J-1により到達不可能。フロア15以降の推奨レベルが全て非現実的 | 高 | `GameConstants.cs:208-216` | |
| J-3 | 敵の経験値が固定値でフロア深度によるスケーリングなし。ゴブリンはフロア1でもフロア30でも常に15exp。深層の敵を倒すインセンティブが低い | 中 | `EnemyFactory.cs:252-783` | |

---

## カテゴリK: 戦闘バランスの不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| K-1 | 魔法防御の係数が0.3で物理防御の0.5より大幅に低い。魔法攻撃INT×3に対し魔法防御MND×2×0.3=MND×0.6で、攻撃対防御の比率が5.2:1（物理は2.9:1）。**魔法が極端に強すぎる** | 高 | `DamageCalculator.cs:73-82` | |
| K-2 | 魔法攻撃はクリティカルヒットが不可能（ハードコードでfalse）。物理は1.5倍クリティカルが存在。物理と魔法の期待ダメージ差がさらに広がる場面がある | 中 | `CombatSystem.cs:269` | |
| K-3 | DEXのクリティカル率への寄与（0.3%/pt）がLUK（0.5%/pt）の60%しかない。攻撃系ステータスのDEXが幸運系のLUKより劣るのは不自然 | 中 | `DamageCalculator.cs:168-182` | |
| K-4 | 物理防御の係数0.5により、VIT投資の効率がSTR投資の約1/3。防御特化ビルドが成立しにくい | 中 | `DamageCalculator.cs:26-35` | |
| K-5 | ボスHP倍率がハードコード（3.0x～6.0x）で計算式ベースでない。深度スケーリング式(GetDepthStatMultiplier)との整合性なし | 中 | `GameConstants.cs:219-228` | |

---

## カテゴリL: ショップ経済の不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| L-1 | 買値倍率1.5、売値倍率0.3で**売買往復損失が80%**。100Gの武器を150Gで購入→30Gで売却。装備更新のたびにゴールドが大量消滅し、経済が回らない | 高 | `GameConstants.cs:330-338` | |
| L-2 | ポーション回復量が固定値（小30HP/中75HP/大150HP）でレベルスケーリングなし。Lv20（HP170）時点で小ポーションは17.6%回復、Lv30（HP300）では10%回復。**高レベルでは実質的に回復アイテムが機能しない** | 高 | `ItemFactory.cs:440-497` | |

---

## カテゴリM: セーブデータの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| M-1 | `Thirst`（渇き）・`Fatigue`（疲労）・`Hygiene`（衛生）がSaveDataに含まれていない。ロード時にこれらの値が初期値にリセットされる | 致命的 | `SaveData.cs`, `MultiSlotSaveSystem.cs` | |
| M-2 | コンパニオン・ペットデータがSaveDataに含まれていない。セーブ→ロードで仲間全員が消失する | 致命的 | `SaveData.cs`, `CompanionSystem.cs` | |
| M-3 | `DaysSinceLastPrayer`・`HasPrayedToday`がSaveDataに含まれていない。祈りの日次スケジュールがロード時にリセットされる | 中 | `SaveData.cs`, `Player.cs:302-303` | |

---

## カテゴリN: インベントリ・装備システムの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| N-1 | `Inventory.Add()`メソッドが重量チェックを行わない。`MaxWeight`プロパティは存在するが、Add時に`TotalWeight + item.Weight <= MaxWeight`の検証がない。重量上限を超えてアイテムを追加可能 | 致命的 | `Inventory.cs:35-56` | |
| N-2 | 両手武器装備時にオフハンド装備がnullに設定されるが、**インベントリへの返却処理が未実装**（コメントで「要実装」と記載あり）。オフハンド装備品が消失するバグ | 致命的 | `Equipment.cs:348-357` | |
| N-3 | 盾を装備した状態で両手武器を装備→盾消失は防がれるが、逆方向（両手武器装備中に盾を装備）の制約チェックがない | 中 | `Equipment.cs:348-357` | |

---

## カテゴリO: スキル・転職システムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| O-1 | スキルポイントの付与・消費・追跡システムが存在しない。`SkillSystem`にはスキル使用とクールダウンの管理はあるが、スキル習得のためのポイント経済がない。レベルアップ時にスキルポイント付与もなし | 高 | `SkillSystem.cs`, `Player.cs:366-387` | |
| O-2 | `MultiClassSystem.CanClassChange()`は転職可否の判定のみで、転職実行時のステータス再計算メソッドが存在しない。転職してもステータスが変化しない | 高 | `MultiClassSystem.cs:27-32` | |

---

## カテゴリP: マップ表示の不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| P-1 | `TileType.NpcTrainer`のDisplayCharケースが`GetDisplayChar()`に存在しない。訓練師タイルが`?`で表示される | 高 | `Tile.cs:476-525` | |
| P-2 | `TileType.NpcLibrarian`のDisplayCharケースが`GetDisplayChar()`に存在しない。図書館司書タイルが`?`で表示される | 高 | `Tile.cs:476-525` | |

---

## カテゴリQ: タイルプロパティのドキュメント不整合（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| Q-1 | `TileType.SymbolMountain`のEnum定義コメントが「山岳（通行不可）」だが、実装では`BlocksMovement=false`（通行可能、移動コスト2.0）。`SymbolWater`も同様に「水域（通行不可）」と記載されるが実際は通行可能（移動コスト1.8） | 中 | `Tile.cs:130-142`, `SymbolMapSystem.cs:136-141` | |

---

## カテゴリR: 建設・投資システムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| R-1 | `BaseConstructionSystem.GetDailyFoodProduction()`が定義されているが、GameControllerから一度も呼び出されていない。畑を建設しても食料が自動生産されない | 高 | `BaseConstructionSystem.cs:93`, `GameController.cs` | |
| R-2 | `InvestmentSystem`で投資が記録されるが、投資の完了・配当支払いを処理するコードがGameControllerに存在しない。投資したゴールドが回収不能 | 高 | `InvestmentSystem.cs`, `GameController.cs` | |

---

## カテゴリS: ステータスシステムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| S-1 | ThirstStageとThirstLevelの2つの列挙型で`SevereDehydration`のダメージ値が異なる（ThirstStage=2、ThirstLevel=3）。レガシーコードとの不整合 | 中 | `ThirstSystem.cs:80-94` | |
| S-2 | 宗教の信仰上限ペナルティ（再加入時-20）が累積しない。何度脱退・再加入しても1回分の-20しか適用されず、繰り返しペナルティ回避が可能 | 中 | `Player.cs:320-322`, `GameConstants.cs` | |

---

## カテゴリT: Enum未使用値（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| T-1 | `ExtendedItemCategory`列挙型（8値: Material/SoulGem/TrapKit/RepairTool/CookedFood/Book/Key/Instrument）が**全値未使用**。定義のみで参照箇所なし | 高 | `Enums.cs:1478-1496` | |
| T-2 | `SkillTarget.SingleAlly`/`AllAllies`が定義済みだが使用なし。味方対象スキルが存在しない | 中 | `Enums.cs:442-450` | |
| T-3 | `TradeRouteStatus.Closed`/`Blocked`が定義済みだが使用なし。交易路の閉鎖・遮断状態が機能しない | 中 | `Enums.cs:1605-1615` | |

---

## カテゴリU: 未使用食材タイプ（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| U-1 | `FoodType`の5値（RawMeat/RawFish/Fruit/Vegetable/Rotten）が定義済みだが対応する食品アイテムが生成されない。料理素材として入手不能 | 高 | `Consumables.cs:223-251` | |

---

## カテゴリV: 魔法・呪文システムの不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| V-1 | ルーン語"loka"（閉じる）が`CategorizeEffect()`のswitchに存在しない。デフォルトでDamage型に分類されるため、ドア閉鎖呪文が攻撃呪文として処理される | 致命的 | `SpellCastingSystem.cs:421-441`, `RuneWordDatabase.cs:92` | |
| V-2 | `DetermineElement()`にPoison元素のマッピングがない。毒属性呪文が`Element.None`として処理され、耐性計算が無効化される | 致命的 | `SpellCastingSystem.cs:388-419` | |
| V-3 | `StatusEffect.Invisibility`/`Slow`/`Vulnerability`/`Apostasy`の4種にCreate系ファクトリメソッドが存在しない。これらの状態異常を生成できない | 高 | `StatusEffectSystem.cs`, `Enums.cs:214-239` | |
| V-4 | Dark→Curse元素の相性が0.0（無効化）だが、逆方向Curse→Darkは1.0（中立）。一方向の完全耐性は不自然 | 中 | `ElementSystem.cs:143-164` | |
| V-5 | "springa"（爆発）の難易度が2でMP=10だが、同MPの"hylja"（隠す）は難易度3。高火力呪文が先に習得可能で学習曲線が崩壊 | 中 | `RuneWordDatabase.cs:55-92` | |
| V-6 | バフ"berserk"のAttackMultiplier=0.5fだが説明は「攻撃力+50%」。0.5は-50%を意味し、説明と効果が逆転している可能性 | 高 | `ExtendedStatusEffectSystem.cs:41-42` | |

---

## カテゴリW: クラフト・料理システムの不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| W-1 | `material_coal`（石炭）がItemFactoryに存在せず入手不能。鋼鉄の剣レシピが作成不可 | 致命的 | `CraftingSystem.cs:275`, `ItemFactory.cs` | |
| W-2 | `material_leather`（革）がItemFactoryに存在せず入手不能。革鎧レシピが作成不可 | 致命的 | `CraftingSystem.cs:301`, `ItemFactory.cs` | |
| W-3 | `material_raw_meat`（生肉）がItemFactoryに存在せず入手不能。調理肉・非常食レシピ＋料理の焼き肉・干し肉が全て作成不可 | 致命的 | `CraftingSystem.cs:336,344`, `CookingSystem.cs:21,24` | |
| W-4 | `material_magical_essence`（魔法のエッセンス）がItemFactoryに存在せず入手不能。超回復薬レシピが作成不可 | 致命的 | `CraftingSystem.cs:328` | |
| W-5 | `material_fish`（魚）がItemFactoryに存在せず入手不能。蒸し魚の料理レシピが作成不可 | 致命的 | `CookingSystem.cs:23` | |
| W-6 | `material_salt`（塩）がItemFactoryに存在せず入手不能。干し肉の料理レシピが作成不可 | 致命的 | `CookingSystem.cs:24` | |

---

## カテゴリX: ポーション効果値の未定義（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| X-1 | `potion_invisibility`（透明化薬）のEffectValue=0。持続ターン数は設定されているが効果量が未定義 | 高 | `ItemFactory.cs:537-547` | |
| X-2 | `potion_cure_all`（万能薬）のEffectValue未定義。回復量のバランス基準がない | 高 | `ItemFactory.cs:573-582` | |
| X-3 | `potion_fire_resist`（耐火薬）の耐性パーセンテージ未定義。持続ターン50のみ設定 | 高 | `ItemFactory.cs:549-559` | |
| X-4 | `potion_cold_resist`（耐冷薬）の耐性パーセンテージ未定義。持続ターン50のみ設定 | 高 | `ItemFactory.cs:561-571` | |

---

## カテゴリY: 種族特性の未実装（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| Y-1 | スライム種族の「分裂」能力（HP50%以下で味方スライム召喚）が定義済みだが、戦闘/ダメージシステムにチェックロジックなし。特性が完全に不活性 | 致命的 | `RacialTraitSystem.cs:96-98`, `CharacterCreation.cs:73-77` | |
| Y-2 | デーモン種族の「魔力吸収」能力（魔法被弾時MP5%回復）が定義済みだが、ダメージ計算にチェックなし | 致命的 | `RacialTraitSystem.cs:87`, `CharacterCreation.cs:61-65` | |
| Y-3 | 堕天使種族の「浮遊」能力（穴落下罠無効・水上移動可能）が定義済みだが、`IsLevitating()`が罠/移動システムから呼び出されない | 致命的 | `RacialTraitSystem.cs:91,143-144` | |
| Y-4 | スライム種族の「装備制限」（武器・防具の大部分が装備不可）が定義済みだが、`HasEquipmentRestriction()`が装備ロジックから呼び出されない。全装備可能 | 致命的 | `RacialTraitSystem.cs:98,147-148` | |
| Y-5 | 堕天使種族の「光闇両属性」特性が定義済みだが、スキルシステムに属性制限がなく全種族が全属性使用可能。特性の差別化がない | 中 | `RacialTraitSystem.cs:92` | |
| Y-6 | 種族の隠しペナルティ（オーク:INT-3/CHA-2、アンデッド:正気度消耗1.3倍、堕天使:LUK-4）が説明文に記載なし。プレイヤーが把握できない | 中 | `CharacterCreation.cs:6-82` | |

---

## カテゴリZ: 背景・コンパニオンシステムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| Z-1 | 全10職歴（冒険者/兵士/学者等）の初期装備品が`BackgroundBonusData`に定義されているが、`Player.Create()`で一度も適用されない。職歴固有の初期装備が受け取れない | 致命的 | `RacialTraitSystem.cs:218-262`, `Player.cs:625-674` | |
| Z-2 | `CompanionSystem`（仲間システム）が完全実装済み（追加/除去/AI行動/ターン処理）だが、GameControllerから`AddCompanion()`を呼ぶコードパスがない。仲間の募集が不可能 | 高 | `CompanionSystem.cs`, `GameController.cs` | |

---

## カテゴリAA: トラップ・GameControllerの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AA-1 | `TriggerTrap()`のswitch文に`TrapType.Poison`/`Arrow`/`Fire`/`Sleep`/`Confusion`の5種のケースが欠落。`TrapDefinition.cs`で定義されているが踏んでも何も起こらない | 致命的 | `GameController.cs:3340-3376`, `TrapDefinition.cs:112-150` | |
| AA-2 | GameController内に14箇所以上のマジックナンバー（ドロップ率25/2、訓練費用50/100/150、投資上限100-500等）が定数化されずハードコードされている | 中 | `GameController.cs:1289,1561,1597,5364,5380,5396,6077-6078` | |

---

## カテゴリAB: セーブデータ追加欠落（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AB-1 | `PetSystem`のペット状態（ID/名前/種類/レベル/経験値/空腹度/忠誠度/HP/騎乗状態）がセーブされない。ロード時にペット全消失 | 致命的 | `GameController.cs:6694-6801`, `PetSystem.cs` | |
| AB-2 | `CombatStance`（戦闘構え）がセーブされない。ロード時にBalancedにリセット。攻撃/防御修正が失われる | 高 | `GameController.cs:87,6694-6801` | |
| AB-3 | キャラクターの全`StatusEffects`（活性バフ/デバフとその残りターン数）がセーブされない。ロード時に全状態異常が消滅 | 致命的 | `Character.cs:79`, `GameController.cs:6694-6801` | |
| AB-4 | `ProficiencySystem`の全12カテゴリ（武器/採掘/釣り/鍛冶/料理等）の習熟レベル・経験値がセーブされない | 致命的 | `GameController.cs:65,6694-6801`, `ProficiencySystem.cs` | |
| AB-5 | `DiseaseSystem`の現在の病気種類と残りターン数（`_playerDisease`/`_diseaseRemainingTurns`）がセーブされない | 致命的 | `GameController.cs:90,2256-2293,6694-6801` | |
| AB-6 | `BodyConditionSystem`の傷の状態（切り傷/打撲/穿刺傷/骨折/火傷とその回復ターン）がセーブされない | 高 | `BodyConditionSystem.cs:8-24`, `GameController.cs:6694-6801` | |
| AB-7 | `Player.HasPrayedToday`フラグがセーブされない。ロード後に再度祈りが可能（1日1回制限が無効化） | 中 | `Player.cs:303`, `GameController.cs:6694-6801` | |
| AB-8 | `WeaponProficiency`（武器熟練度）の個別武器種ごとのレベル/経験値がセーブされない | 高 | `WeaponProficiencySystem.cs`, `GameController.cs:6694-6801` | |
| AB-9 | `Player.RestoreFromSave()`がThirst/Fatigue/Hygieneのパラメータを受け付けるが、LoadSaveData()から値が渡されず常にデフォルト100（最大値）でリセットされる | 致命的 | `Player.cs:698-724`, `GameController.cs:6810-6822` | |

---

## カテゴリAC: クエストシステムの不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AC-1 | クエスト`quest_rat_hunt`の討伐対象`"enemy_rat"`がEnemyFactoryに存在しない。クエスト完了不可能 | 致命的 | `NpcSystem.cs:785-788`, `EnemyFactory.cs` | |
| AC-2 | クエスト`quest_bandit_clear`の討伐対象`"enemy_bandit"`がEnemyFactoryに存在しない。クエスト完了不可能 | 致命的 | `NpcSystem.cs:801-804`, `EnemyFactory.cs` | |
| AC-3 | クエスト`quest_herb_collect`の収集対象IDが`"item_herb"`だが、実際のアイテムIDは`"material_herb"`。ID不一致でクエスト完了判定が動作しない | 致命的 | `NpcSystem.cs:790-793`, `ItemFactory.cs` | |
| AC-4 | クエスト目標のNPC参照が名前ベース（`"deliver_ore_marco"`/`"talk_elwen"`）でNPC IDベース（`"npc_xxx"`）と不一致。完了判定の整合性なし | 高 | `NpcSystem.cs:808,819` | |
| AC-5 | クエスト目標のフロア参照（`"floor_forest_3"`/`"floor_coast_5"`/`"boss_mine_golem"`/`"floor_ruins_10"`）が検証不能。対応するフロア構造の実在が不明 | 中 | `NpcSystem.cs:797,813,825,841` | |

---

## カテゴリAD: 実績・エンディングシステムの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AD-1 | `AchievementSystem`に実績が**1件も登録されていない**。`Register()`が一度も呼ばれず、インフラだけが存在。Unlock()は動作するが実績一覧が空 | 致命的 | `AchievementSystem.cs:42-46`, `GameController.cs:68` | |
| AD-2 | `DetermineEnding()`の`allTerritoriesVisited`パラメータが**常にfalse**でハードコード。放浪者エンディング（Wanderer）が達成不可能 | 致命的 | `GameController.cs:7792-7795`, `MultiEndingSystem.cs:54` | |
| AD-3 | 実績ボーナス効果（`"stat_boost_small"=1`/`"gold_bonus"=50`/`"exp_multiplier"=10`等）が定義済みだが、実績未登録のためCalculateNextPlayBonus()が常に0を返す到達不能コード | 中 | `AchievementSystem.cs:86-95` | |

---

## カテゴリAE: マップ生成・タイルの不整合（7件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AE-1 | `TileType.DeepWater`/`Lava`/`Pit`/`Tree`の4種がプロパティ定義済みだがダンジョン生成で一切配置されない。遭遇不可能な地形 | 高 | `DungeonGenerator.cs`, `Tile.cs` | |
| AE-2 | `TileType.Altar`が祠部屋に配置されるが、GameControllerにインタラクションハンドラなし。踏んでも何も起こらない | 致命的 | `RoomCorridorGenerator.cs:487`, `GameController.cs:1406-1430` | |
| AE-3 | `TileType.Fountain`が通常部屋に50%確率で配置されるが、インタラクションハンドラなし。踏んでも何も起こらない | 致命的 | `RoomCorridorGenerator.cs:197`, `GameController.cs` | |
| AE-4 | `RoomType.Library`/`Prison`/`Storage`の3種が定義済みだが生成ロジックなし。ダンジョンに出現しない | 高 | `Tile.cs:531-577`, `DungeonGenerator.cs` | |
| AE-5 | `FeaturePlacer.cs`（秘密部屋生成）がコードベース全体で一度もインスタンス化されない。`RoomType.Secret`の部屋が完全に無効化 | 高 | `FeaturePlacer.cs`, 全ソース | |
| AE-6 | `GameRenderer.TileColors`辞書に21種のTileType（DeepWater/Lava/Pit/Pillar/RuneInscription/SecretDoor/Grass/Tree/全SymbolMap10種/NpcTrainer/NpcLibrarian）のカラーマッピングがない | 高 | `GameRenderer.cs:35-65` | |
| AE-7 | `EnvironmentalCombatSystem`のSurfaceType（Water/Fire/Poison等）とTileType（Lava/Water/DeepWater）が未接続。溶岩タイルを踏んでも火炎ダメージが適用されない | 高 | `EnvironmentalCombatSystem.cs`, `GameController.cs` | |

---

## カテゴリAF: ランダムイベントの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AF-1 | `RandomEventType.NpcEncounter`（NPC遭遇）がイベントプールに登録済みだがResolveメソッドが存在しない。イベント発生時にハンドラなし | 致命的 | `WorldMapSystem.cs:729,749` | |
| AF-2 | `RandomEventType.MerchantEncounter`（商人遭遇）が同様にResolveメソッドなし | 致命的 | `WorldMapSystem.cs:730,761` | |
| AF-3 | `RandomEventType.AmbushEvent`（待ち伏せ）が同様にResolveメソッドなし。イベントタイプからハンドラへのディスパッチswitch文自体が存在しない | 致命的 | `WorldMapSystem.cs:733,771` | |

---

## カテゴリAG: ギャンブルシステムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AG-1 | `GamblingSystem.GetLuckBonus()`が定義済み（LUK1あたり0.5%、上限5%）だが**一度も呼び出されない**。高LUCKキャラでもギャンブル勝率に影響なし | 高 | `GamblingSystem.cs:40-44` | |
| AG-2 | カードゲーム（ハイロー）の引き分けが「負け」扱い。他のゲーム（サイコロ/丁半）は50%勝率だがカードは約48%で期待値0.912（8.8%のハウスエッジ）。他ゲームの5%と比較して不公平 | 中 | `GamblingSystem.cs:24-29` | |

---

## カテゴリAH: Engine層のEnum/計算不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AH-1 | `ResourceSystem.GetClassHpBonus()`/`GetClassMpBonus()`がCore層と異なるクラス名（Warrior/Berserker/Summoner）を使用。Fighter/Knight/Thief/Ranger/Bard/Alchemistの6クラスがデフォルト値にフォールスルーし不正なHP/MP成長率 | 致命的 | `ResourceSystem.cs:30-102` | |
| AH-2 | `CalculateExpGain()`で`param.PlayerLevel`のゼロチェックがない。PlayerLevel=0時に`DivideByZeroException`でクラッシュ | 致命的 | `ResourceSystem.cs:309` | |
| AH-3 | `SpellCastingSystem`の三項演算子が常にMagicalを返す: `element == Element.None ? DamageType.Magical : DamageType.Magical`。純粋/物理ダメージ呪文が作成不可 | 高 | `SpellCastingSystem.cs:369` | |
| AH-4 | `Character.GetResistanceAgainst()`が常に0fを返す。種族の属性耐性（火耐性/毒耐性等）がダメージ計算に反映されない | 高 | `Character.cs:105` | |
| AH-5 | `Character.ConsumeMp()`に入力検証なし。負の値を渡すとMPが増加する（`ConsumeMp(-50)`でMP+50）。セキュリティリスク | 高 | `Character.cs:119` | |

---

## カテゴリAI: ダメージ計算の不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AI-1 | `DamageType.Pure`と`DamageType.Healing`がswitch文のdefaultケース（防御力0）にフォールスルー。明示的な処理がなく挙動が曖昧 | 中 | `Character.cs:85-90` | |
| AI-2 | 経験値計算で`Math.Pow(1.5, 98)`がint.MaxValueを超える可能性。レベル99以上の経験値テーブルがオーバーフロー | 中 | `ResourceSystem.cs:280-287` | |

---

## 全体集計

| カテゴリ | 致命的 | 高 | 中 | 低 | 設計課題 | 合計 |
|---------|--------|---|---|---|---------|------|
| A: 商人・ショップ | 0 | 4 | 4 | 0 | 1 | 9 |
| B: アイテム・消耗品 | 6 | 6 | 1 | 0 | 0 | 13 |
| C: 装備・ステータス | 0 | 4 | 2 | 0 | 0 | 6 |
| D: 敵・ダンジョン | 0 | 0 | 4 | 1 | 0 | 5 |
| E: NPC対話・テキスト | 0 | 0 | 3 | 0 | 0 | 3 |
| F: 宗教システム | 0 | 0 | 2 | 1 | 0 | 3 |
| G: テリトリー | 0 | 0 | 3 | 0 | 0 | 3 |
| H: ペットシステム | 0 | 0 | 0 | 1 | 0 | 1 |
| I: ArmorType | 0 | 0 | 0 | 1 | 0 | 1 |
| J: 経験値・レベルカーブ | 1 | 1 | 1 | 0 | 0 | 3 |
| K: 戦闘バランス | 0 | 1 | 4 | 0 | 0 | 5 |
| L: ショップ経済 | 0 | 2 | 0 | 0 | 0 | 2 |
| M: セーブデータ | 2 | 0 | 1 | 0 | 0 | 3 |
| N: インベントリ・装備 | 2 | 0 | 1 | 0 | 0 | 3 |
| O: スキル・転職 | 0 | 2 | 0 | 0 | 0 | 2 |
| P: マップ表示 | 0 | 2 | 0 | 0 | 0 | 2 |
| Q: タイルドキュメント | 0 | 0 | 1 | 0 | 0 | 1 |
| R: 建設・投資 | 0 | 2 | 0 | 0 | 0 | 2 |
| S: ステータスシステム | 0 | 0 | 2 | 0 | 0 | 2 |
| T: Enum未使用値 | 0 | 1 | 2 | 0 | 0 | 3 |
| U: 未使用食材タイプ | 0 | 1 | 0 | 0 | 0 | 1 |
| V: 魔法・呪文システム | 2 | 2 | 2 | 0 | 0 | 6 |
| W: クラフト・料理素材 | 5 | 0 | 0 | 0 | 0 | 5 |
| X: ポーション効果値 | 0 | 4 | 0 | 0 | 0 | 4 |
| Y: 種族特性の未実装 | 4 | 0 | 2 | 0 | 0 | 6 |
| Z: 背景・コンパニオン | 1 | 1 | 0 | 0 | 0 | 2 |
| AA: トラップ・GC不整合 | 1 | 0 | 1 | 0 | 0 | 2 |
| **AB: セーブデータ追加欠落** | **5** | **3** | **1** | **0** | **0** | **9** |
| **AC: クエストシステム** | **3** | **1** | **1** | **0** | **0** | **5** |
| **AD: 実績・エンディング** | **2** | **0** | **1** | **0** | **0** | **3** |
| **AE: マップ生成・タイル** | **2** | **4** | **0** | **0** | **1** | **7** |
| **AF: ランダムイベント** | **3** | **0** | **0** | **0** | **0** | **3** |
| **AG: ギャンブルシステム** | **0** | **1** | **1** | **0** | **0** | **2** |
| **AH: Engine層Enum/計算** | **2** | **3** | **0** | **0** | **0** | **5** |
| **AI: ダメージ計算** | **0** | **0** | **2** | **0** | **0** | **2** |
| **合計** | **41** | **45** | **41** | **4** | **2** | **133** |

---

## 修正方針（ユーザー判断待ち）

ユーザーが各項目の「修正判断」欄に ✅（修正する）/ ❌（修正しない）/ 🔄（保留）を記入後、
確定した修正対象をまとめて実装する。

### 修正優先度の目安
1. **致命的**（41件）: 使用するとクラッシュ/データ消失/機能しない → 最優先で修正推奨
2. **高**（45件）: 設計と実装の明確な乖離 → 修正推奨
3. **中**（41件）: 違和感・バランス問題 → 選択的に修正
4. **低**（4件）: 軽微なテーマ不一致 → 余裕があれば修正
5. **設計課題**（2件）: アーキテクチャ改善 → 長期検討

### 新規追加カテゴリの概要（第2回調査分）
- **J: 経験値カーブ** — 1.5倍指数関数により実質Lv20が上限。推奨レベルが到達不能
- **K: 戦闘バランス** — 魔法防御が弱すぎ、クリティカル率の不整合、ボスHP式がハードコード
- **L: ショップ経済** — 売買差額80%損失、ポーション回復量スケーリングなし
- **M: セーブデータ** — 渇き/疲労/衛生/コンパニオンがセーブされない
- **N: インベントリ・装備** — 重量チェックなし、両手武器装備時のオフハンド消失バグ
- **O: スキル・転職** — スキルポイント経済なし、転職時ステータス再計算なし
- **P: マップ表示** — NpcTrainer/NpcLibrarianの表示文字未定義
- **Q: タイルドキュメント** — 山岳・水域の「通行不可」コメントと実装の矛盾
- **R: 建設・投資** — 食料自動生産・投資配当が呼び出されない
- **S: ステータスシステム** — 渇きダメージ値の列挙型間不整合、信仰ペナルティ非累積

### 新規追加カテゴリの概要（第3回調査分）
- **T: Enum未使用値** — ExtendedItemCategory全8値未使用、SkillTarget味方対象未使用、TradeRouteStatus未使用
- **U: 未使用食材タイプ** — FoodType 5値（RawMeat/RawFish等）にアイテム生成なし
- **V: 魔法・呪文システム** — ルーン語"loka"未分類、Poison元素マッピング欠落、状態異常4種のファクトリ欠落、berserkバフ効果値逆転
- **W: クラフト・料理素材** — 石炭/革/生肉/魔法エッセンス/魚/塩の6素材がItemFactoryに存在せず全レシピ作成不可
- **X: ポーション効果値** — 透明化薬/万能薬/耐火薬/耐冷薬のEffectValue未定義
- **Y: 種族特性の未実装** — スライム分裂/デーモン魔力吸収/堕天使浮遊/スライム装備制限が定義のみで不活性
- **Z: 背景・コンパニオン** — 全10職歴の初期装備未適用、CompanionSystem未接続
- **AA: トラップ・GC不整合** — TriggerTrap()に5種のトラップケース欠落、14箇所のマジックナンバー

### 新規追加カテゴリの概要（第4回調査分）
- **AB: セーブデータ追加欠落** — ペット/戦闘構え/状態異常/習熟度/病気/傷/祈りフラグ/武器熟練度の9システムがセーブ非対応。Thirst/Fatigue/Hygieneが常に最大値でリセット
- **AC: クエストシステム** — enemy_rat/enemy_banditがEnemyFactory未定義でクエスト完了不可、item_herbのID不一致、NPC名/ID混在
- **AD: 実績・エンディング** — AchievementSystemに実績0件登録、放浪者エンディングのパラメータがfalseハードコードで達成不可能
- **AE: マップ生成・タイル** — Altar/Fountainにインタラクションなし、Library/Prison/Storage部屋未生成、FeaturePlacer未使用、21種のTileColorマッピング欠落
- **AF: ランダムイベント** — NpcEncounter/MerchantEncounter/AmbushEventの3イベントにResolveハンドラなし
- **AG: ギャンブルシステム** — GetLuckBonus()未呼び出し、カードゲームのハウスエッジが他ゲームの約2倍
- **AH: Engine層Enum/計算** — ResourceSystemがCore層と異なるクラス名使用（6クラスのHP/MP成長率が不正）、PlayerLevel=0でゼロ除算、呪文DamageType三項演算子が常にMagical
- **AI: ダメージ計算** — DamageType.Pure/Healingの明示的処理なし、レベル99+の経験値テーブルオーバーフロー
