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

## カテゴリAJ: デッドコードシステム（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AJ-1 | `MerchantGuildSystem`が完全なデッドコード。初期化（GC:59）とReset（GC:3153）のみで、JoinGuild()/EstablishRoute()/ExecuteTrade()が一度も呼ばれない。交易システムが完全に無機能 | 高 | `MerchantGuildSystem.cs`, `GameController.cs:59,3153` | |
| AJ-2 | `TerritoryInfluenceSystem`が完全なデッドコード。初期化（GC:62）とReset（GC:3147）のみで、Initialize()/ModifyInfluence()/GetDominantFaction()が一度も呼ばれない | 高 | `TerritoryInfluenceSystem.cs`, `GameController.cs:62,3147` | |
| AJ-3 | `ModLoaderSystem`が完全なデッドコード。初期化（GC:72）のみでLoadMod()/Validate()が呼ばれない。ファイルI/Oも未実装でモッド読み込みは機能しない | 中 | `ModLoaderSystem.cs`, `GameController.cs:72` | |

---

## カテゴリAK: カルマ・無限ダンジョンの永続性欠落（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AK-1 | `KarmaSystem`のカルマ値・履歴がセーブされない。カルマはショップ価格（GC:5482-5485）・NPC態度・闇市場アクセスに影響するが、セーブ/ロードで全消失 | 致命的 | `GameController.cs:6694-6800(CreateSaveData)`, `KarmaSystem.cs` | |
| AK-2 | `_infiniteDungeonMode`フラグと`_infiniteDungeonKills`がセーブされない。無限ダンジョン進行中にセーブ→ロードするとモード解除され通常ダンジョンに戻る | 高 | `GameController.cs:164,167,6694-6800` | |
| AK-3 | `BaseConstructionSystem`の建設済み施設データがセーブされない。建設した7種の施設（キャンプ/鍛冶場/農場等）がロード時に全消失 | 高 | `BaseConstructionSystem.cs`, `GameController.cs:48,6694-6800` | |

---

## カテゴリAL: 天候効果の未適用（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AL-1 | `WeatherSystem.GetElementDamageModifier()`/`GetRangedHitModifier()`が戦闘ダメージ計算から呼ばれない。雨天の雷+20%/火-20%、嵐の遠距離-30%等が全て無効 | 致命的 | `WeatherSystem.cs:102,105`, `GameController.cs:1874-1885` | |
| AL-2 | `WeatherSystem.GetMovementCostModifier()`が移動処理から呼ばれない。吹雪の移動コスト+50%、雨の+10%等が全て無効 | 高 | `WeatherSystem.cs:105`, `GameController.cs` | |
| AL-3 | `WeatherSystem.GetSightModifier()`が視界計算から呼ばれない。霧の視界50%低下、嵐の40%低下が無効。視界は常にComputeFov(12)の固定値 | 高 | `WeatherSystem.cs:90`, `GameController.cs:2224,2718` | |

---

## カテゴリAM: 釣り・採掘アイテムID不一致（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AM-1 | `FishingSystem`の全9種の魚ID（fish_common_1/fish_common_2/fish_medium_1等）がItemFactoryに未定義。釣り成功時にItemFactory.Create()が失敗しアイテム生成不可 | 致命的 | `FishingSystem.cs:22-42`, `ItemFactory.cs` | |
| AM-2 | `GatheringSystem`の鉱石ID（ore_iron/ore_silver/ore_gold/ore_mithril/gem_rough）がItemFactoryに未定義。採掘成功時にアイテム生成不可。唯一存在する`material_iron_ore`とも異なるID | 致命的 | `GatheringSystem.cs:28-31`, `ItemFactory.cs` | |
| AM-3 | `FishingSystem`の魚IDとGatheringSystemの魚IDが不一致。FishingSystemは`fish_common_1`、GatheringSystemは`fish_common`を使用 | 高 | `FishingSystem.cs:22-42`, `GatheringSystem.cs:38-41` | |
| AM-4 | マップ上に釣りスポットが配置されない。DungeonFeatureGeneratorにWaterTileChanceが存在するが、水タイルへのインタラクション（釣り開始）がGameControllerに未実装 | 高 | `DungeonFeatureGenerator.cs`, `GameController.cs` | |

---

## カテゴリAN: ルーン学習・エンチャント永続性（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AN-1 | プレイヤーにKnownRunes/LearnedRunesフィールドが存在しない。RuneWordDatabase（54+ルーン語定義済み）をプレイヤーが学習する手段がない | 致命的 | `Player.cs`, `RuneWordDatabase.cs:176-183` | |
| AN-2 | ルーン組み合わせシステム（SpellParser）がGameControllerに未接続。ルーンを組み合わせて呪文を作成するUIもメソッドもない | 高 | `SpellParser.cs`, `GameController.cs` | |
| AN-3 | エンチャントデータがアイテムに永続化されない。Item.csにAppliedEnchantmentsフィールドがなく、セーブ/ロードでエンチャント情報が全消失 | 致命的 | `EnchantmentSystem.cs`, `Item.cs`, `SaveData.cs` | |
| AN-4 | ソウルジェムがItemFactoryに定義されていない。EnchantmentSystemがソウルジェムを必要とするが、アイテムとして入手不可能 | 高 | `EnchantmentSystem.cs:98-106`, `ItemFactory.cs` | |

---

## カテゴリAO: ボスフロア・クエスト報酬の不整合（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AO-1 | ボスフロアで階段が制限されない。TryDescendStairs()にIsBossFloorチェックがなく、プレイヤーがボス戦をスキップして次の階に進める | 致命的 | `GameController.cs:2635-2683` | |
| AO-2 | クエスト報酬のItemIdsが処理されない。QuestRewardにItemIds配列が定義済みだが、TurnInQuest()でGold/Experience/GuildPointsのみ処理されItemIdsは無視される | 致命的 | `NpcSystem.cs:434-456`, `GameController.cs:6333-6346` | |
| AO-3 | クエスト報酬アイテム付与時にインベントリ容量チェックがない。報酬アイテムがインベントリ満杯時に消失する可能性 | 高 | `NpcSystem.cs:434-456` | |
| AO-4 | `HasPrayedToday`の日次リセットが実装されていない。ProcessTurnEffects()に日替わりリセットコードがなく、祈りの1日1回制限が保持されない（AB-7と関連するが根本原因が異なる） | 高 | `Player.cs:303,317`, `GameController.cs:2125+` | |

---

## カテゴリAP: 建設システムの未接続（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AP-1 | `BaseConstructionSystem.Build()`メソッドが一度も呼ばれない。7種の施設（Camp/Workbench/Smithy/Storage/Farm/Barricade/Barracks）が建設不可能。GetRestHpRecoveryMultiplier()とGetCraftingSuccessBonus()のみ使用 | 致命的 | `BaseConstructionSystem.cs`, `GameController.cs:48,7287,7293` | |
| AP-2 | `AmbientSoundSystem.GetCurrentAmbientSound()`（GC:7968）が一度も呼ばれない。9種のアンビエント音（Dungeon/Forest/Mountain等）が定義済みだが再生システムなし。`_currentAmbientSound`（GC:640）は設定されるが消費されない | 中 | `AmbientSoundSystem.cs`, `GameController.cs:640,2668,7968` | |

---

## カテゴリAQ: 環境パズル・ターンカウンタの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AQ-1 | `EnvironmentalPuzzleSystem`の名前に反しパズルがダンジョン床に配置されない。NPC対話イベントとしてのみ動作し、探索型の環境パズルは存在しない | 中 | `EnvironmentalPuzzleSystem.cs`, `GameController.cs:7718-7734` | |
| AQ-2 | `TurnCount`がint型で宣言。int.MaxValue（2,147,483,647）到達後にオーバーフローし、TurnCount%600==0等の全周期処理が破綻。GameTimeのモジュロ演算も負値で不正動作 | 中 | `GameController.cs:191`, `GameTime.cs:38` | |

---

## カテゴリAR: スキル・パッシブ・状態異常の不整合（8件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AR-1 | `Charm`状態異常が行動制限に含まれない。Stun/Freeze/Sleep/Petrificationは行動阻止するがCharmは阻止対象外で、魅了された敵/プレイヤーが通常通り攻撃可能 | 高 | `GameController.cs:935-943`, `StatusEffectSystem.cs:198-204` | |
| AR-2 | `Madness`状態異常のランダム行動ロジック未適用。`StatusEffectSystem.GetMadnessAction()`が定義済みだがGameControllerから一度も呼ばれず、狂気状態でも通常行動可能 | 高 | `StatusEffectSystem.cs:401-411`, `GameController.cs` | |
| AR-3 | `InstantDeath`状態異常が確実に即死しない。DamagePerTick=int.MaxValue, Duration=1で設計されるが、TickStatusEffectsの呼び出しタイミングに依存し即死が保証されない | 中 | `StatusEffectSystem.cs:235-240`, `GameController.cs:1973-1979` | |
| AR-4 | パッシブスキル（weapon_mastery「武器ダメージ+20%」、hp_boost「最大HP+10%」等）が定義済みだがBasePower値が実際のステータス/ダメージ計算に反映されない。学習しても効果ゼロ | 致命的 | `SkillSystem.cs:95,156-157`, `GameController.cs` | |
| AR-5 | `Character.TickStatusEffects()`がGameControllerのターンループから呼ばれない。バフ/デバフの持続時間がデクリメントされず、一度適用された効果が永続化する | 致命的 | `GameController.cs`, `Character.cs:161-187` | |
| AR-6 | 相反するバフ/デバフの共存チェックなし。Strength（攻撃力1.25倍）とWeakness（全ステ0.80倍）が同時に適用可能。相互排他ルールが未実装 | 中 | `Character.cs:124-146` | |
| AR-7 | スキル前提条件のID検証なし。SkillTreeNodeのPrerequisites配列にタイプミスのスキルIDが含まれていても検出されず、そのスキルが学習不能になるがエラー表示なし | 中 | `SkillSystem.cs:29-42,163-252` | |
| AR-8 | 誓約（Oath）の違反チェック`IsViolation()`が一度も呼ばれない。use_alcohol/attack_enemy/use_torch等の禁止行動を実行しても誓約違反が発生しない | 致命的 | `OathSystem.cs:62-71`, `GameController.cs` | |

---

## カテゴリAS: セーブデータ詳細欠落（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AS-1 | `Item.Grade`（アイテム品質）がItemSaveDataに未保存。ロード時に全アイテムがItemGrade.Standardにリセットされ、レア/レジェンダリー等の品質情報が消失 | 致命的 | `SaveData.cs:214-236`, `GameController.cs:6980-6989` | |
| AS-2 | 地面に置かれたアイテム（GroundItems）が一切セーブされない。CreateSaveDataにGroundItemsの保存処理なし。ロード時にマップが再生成され全アイテム消失 | 致命的 | `GameController.cs:189,6694-6801` | |
| AS-3 | マップ探索状態（Tile.IsExplored）がセーブされない。ロード時にマップ再生成で全タイルが未探索に戻り、霧の戦争（Fog of War）がリセットされる | 高 | `SaveData.cs`, `Tile.cs:262`, `GameController.cs:6886-6887` | |
| AS-4 | セーブデータのバージョン移行ロジックが存在しない。SaveData.Versionフィールド（=1）が定義済みだがLoadSaveDataでチェックされず、フォーマット変更時に旧セーブが破損するリスク | 中 | `SaveData.cs:11`, `GameController.cs:6806-6978` | |
| AS-5 | SkillTreeBonusProviderコールバックがセーブされない。ロード後にスキルツリーボーナスが未再計算のまま失われ、UIが再初期化するまでステータスが不正 | 中 | `Player.cs:251`, `SaveData.cs` | |

---

## カテゴリAT: 休息・回復メカニクスの不整合（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AT-1 | 休息時にMP回復が適用されない。TryCamp()でRestSystem.GetRecoveryRates()からmpRecoveryを取得するが、Player.CurrentMpに加算するコードがない | 高 | `GameController.cs:7354,7368-7369` | |
| AT-2 | 休息時にSanity回復が適用されない。RestSystemがsanityRecovery（0.05-0.2）を定義するが、Player.ModifySanity()がTryCamp()内で呼ばれない | 高 | `GameController.cs:7354,7376` | |
| AT-3 | 休息中に食料/水が消費されない。TryCamp()が35-50ターン分の時間を経過させるが、対応する空腹/渇き減少がない。休息にサバイバルコストなし | 中 | `GameController.cs:7382-7383` | |
| AT-4 | キャンプ可能判定のフロア深度ロジックが不明瞭。`!isIndoor && floorDepth == 0`の条件が地上=depth未定義と矛盾し、キャンプ許可条件が予測不能 | 低 | `RestSystem.cs:29-35` | |

---

## カテゴリAU: 移動コスト・地形効果の未適用（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AU-1 | 地形別移動コスト（Water=2.0/Forest=1.5/Mountain=2.0等）がTile.MovementCostに定義済みだが、移動計算で参照されない。全地形が一律1ターンコスト | 高 | `Tile.cs:272,384,429,435`, `GameController.cs:1200-1209` | |
| AU-2 | 斜め移動コスト計算がint切り捨てで正規コストと同一になる。`(int)(1 * 14/10) = (int)(1.4) = 1`となり、斜めも直進も1ターンで斜め移動が常に有利 | 中 | `GameController.cs:1202`, `GameConstants.cs:22-24` | |
| AU-3 | 重量超過ペナルティ（1.5倍移動コスト）がint切り捨てで無効化。`(int)(1 * 1.5) = (int)(1.5) = 1`となり、過重状態でも移動速度に変化なし | 中 | `GameController.cs:1200-1209` | |

---

## カテゴリAV: 時間帯・渇き・疲労・衛生効果の未適用（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AV-1 | 時間帯の視界修正がFOV計算に適用されない。TimeOfDaySystem.GetSightRangeModifier()（夜間0.4倍等）を計算するがComputeFov()は常にDefaultViewRadius(8)を使用 | 高 | `GameController.cs:2223-2228,3347,4542` | |
| AV-2 | 時間帯が敵スポーン率に影響しない。SpawnEnemies()が`4 + CurrentFloor * 2`の固定式を使用し、夜間の増加/昼間の減少が未実装 | 高 | `GameController.cs:726-814,1856-1860` | |
| AV-3 | ThirstSystem.GetThirstModifiers()（STR/AGI/INT修正0.2-1.0倍）が一度も呼ばれない。脱水状態でもステータスペナルティなし | 高 | `ThirstSystem.cs` | |
| AV-4 | 疲労修正値がメッセージ表示のみで実効果なし。fatigueMod取得後に攻撃力/移動速度への反映コードがなく、疲労はフレーバーテキスト | 高 | `GameController.cs:2237-2240` | |
| AV-5 | BodyConditionSystem.GetHygieneInfectionRisk()（衛生状態による感染リスク0.5-4.0倍）が一度も呼ばれない。不衛生でも病気リスク増加なし | 中 | `BodyConditionSystem.cs:54-74` | |
| AV-6 | 料理素材（生肉/塩/魚）がItemFactoryに未定義でCookingSystemの全レシピが作成不可能（W-1〜W-5と別カテゴリだが料理固有の問題） | 高 | `CookingSystem.cs`, `ItemFactory.cs:1159-1250` | |

---

## カテゴリAW: 誓約・信仰・評判の未機能（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AW-1 | 誓約ボーナス（GetTotalExpBonus/GetTotalDropBonus）が一度も呼ばれない。誓約を守る報酬として経験値/ドロップ率ボーナスが定義されるが適用されない | 致命的 | `OathSystem.cs:50-60`, `GameController.cs` | |
| AW-2 | 誓約破棄にペナルティなし。BreakOath()が「ペナルティあり」とコメントされるが、実装は`_activeOaths.Remove(type)`のみで信仰/正気度/ステータスへの影響ゼロ | 高 | `OathSystem.cs:45-48` | |
| AW-3 | 信仰システムに献金/供物メカニクスが存在しない。Pray()のみが信仰ポイント獲得手段で、ゴールド/アイテムを捧げる経済的手段が未実装 | 中 | `ReligionSystem.cs:574-592` | |
| AW-4 | 奇跡/神の祝福を発動する手段がない。信仰ランクに基づくパッシブ恩恵は存在するが、RequestMiracle()等のアクティブな神の介入メカニクスが未定義 | 中 | `ReligionSystem.cs` | |
| AW-5 | ReputationSystem.GetQuestAvailability()が一度も呼ばれない。評判ランクに基づくクエスト解放率（0.0-1.0）が定義済みだが、クエスト掲示板でのフィルタリングに使用されない | 高 | `ReputationSystem.cs:65-76` | |

---

## カテゴリAX: コンパニオンシステムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AX-1 | コンパニオンのDamageCompanion()が戦闘中に一度も呼ばれない。敵はコンパニオンを攻撃対象にせず、コンパニオンは被ダメージゼロで不死身状態 | 致命的 | `GameController.cs:1800-1810`, `CompanionSystem.cs` | |
| AX-2 | コンパニオンのステータスが雇用時に固定（Attack=5+level×3等）で成長メカニクスが存在しない。レベルアップ/経験値蓄積なし | 致命的 | `CompanionSystem.cs:1-196`, `GameController.cs:6245-6256` | |
| AX-3 | CompanionSystem.SetAIMode()がGameControllerから一度も呼ばれない。AIモードが雇用時のAggressive固定で変更不可 | 高 | `CompanionSystem.cs:57-64`, `GameController.cs:6188-6280` | |
| AX-4 | RemoveDeadCompanions()の戻り値（死亡コンパニオンリスト）が未処理。死亡通知/追悼イベント/クエスト連動なしでサイレント除去 | 中 | `CompanionSystem.cs:177-183`, `GameController.cs:1796` | |

---

## カテゴリAY: ペットシステムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AY-1 | PetSystem.Feed()/Train()/TickHunger()が一度も呼ばれない。ペットの餌やり/訓練/空腹値減少が未接続で、空腹/忠誠度が初期値から変化しない | 致命的 | `PetSystem.cs:64-120`, `GameController.cs` | |
| AY-2 | ペット特殊能力（Wolf威嚇/Horse騎乗/Hawk偵察/Cat幸運/Bear防壁/Dragonブレス）がテキスト定義のみで実行コードなし。6種全ての能力が機能しない | 致命的 | `PetSystem.cs:43-48` | |
| AY-3 | PetSystem.GetMoveSpeedMultiplier()（Horse2.0/Bear1.3/Dragon2.5倍）がプレイヤー移動処理から呼ばれない。騎乗速度ボーナスがデータのみ | 高 | `PetSystem.cs:97-109`, `GameController.cs` | |
| AY-4 | PetSystem.GetObedienceRate()（忠誠度に基づく命令成功率）が一度も呼ばれない。ペットコマンドが忠誠度に関係なく100%成功 | 高 | `PetSystem.cs:132-137` | |

---

## カテゴリAZ: 敵AI・行動パターンの未機能（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AZ-1 | ボス敵にBerserkerBehaviorが割り当てられるが、バーサーク状態（HP30%以下でダメージ増加）の実効果なし。ボスの特殊行動パターンが非機能 | 中 | `EnemyFactory.cs:114-120` | |
| AZ-2 | SummonerBehavior（敵が仲間を召喚）が定義済みだがEnemyFactoryで一度も割り当てられない。召喚者タイプの敵が存在しない完全なデッドコード | 高 | `BasicBehaviors.cs:438-495`, `EnemyFactory.cs:65-128` | |
| AZ-3 | 敵の逃走ロジック（ShouldFlee() HP20%以下で撤退）が定義済みだが、戦闘メッセージに「敵が逃げた」が表示されず逃走行動が確認不能 | 中 | `Enemy.cs:169-174`, `GameController.cs:1796-1810` | |

---

## カテゴリBA: 対話・百科事典・シンボルマップイベントの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BA-1 | 16体のNPCの対話ID（dlg_leon_intro等）がDialogueSystemに未登録。StartDialogue()がnullを返しNPC固有の対話ツリーが機能しない。汎用対話にフォールバック | 高 | `NpcSystem.cs:26-66`, `GameController.cs:5649` | |
| BA-2 | SymbolMapEventSystemの10種イベント（商人キャラバン/盗賊襲撃/放浪治療師等）がGameControllerから一度もトリガーされない。メタデータ取得のみ | 中 | `SymbolMapEventSystem.cs:18-40`, `GameController.cs:7890-7893` | |
| BA-3 | TutorialTrigger.ReachFloor10が定義済みだが発火コードなし。10階到達チュートリアルが永久に表示されない | 中 | `SmithingSystem.cs:224`, `GameController.cs` | |
| BA-4 | EncyclopediaSystemのモンスター以外のカテゴリ（アイテム/場所/伝承）にUIアクセスポイントがない。百科事典エントリが登録可能だが閲覧不可 | 中 | `EncyclopediaSystem.cs:1-215`, `GameController.cs` | |

---

## カテゴリBB: ダンジョン生成・部屋タイプの未機能（10件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BB-1 | RoomTypeのShop/Trap/Prison/Storageの4タイプが一度も生成されない。DecorateRooms()がTreasure/Shrineのみ生成し、他は全てNormal部屋 | 高 | `DungeonGenerator.cs:468-481` | |
| BB-2 | 5つのRoomType（Entrance/Prison/Storage/Shop/Trap）にDecorateRoom()のハンドラがない。生成されても装飾/インタラクション不可 | 高 | `RoomCorridorGenerator.cs:104-131` | |
| BB-3 | DungeonFeatureGeneratorの9テーマ（Cave/Ruins/Sewer/Mine/Crypt/Temple/IceCavern/Volcanic/Forest）のパラメータがDungeonGeneratorから参照されない。WaterTileChance/LavaTileChance/CorridorTwistChance等が未適用 | 高 | `DungeonFeatureGenerator.cs`, `DungeonGenerator.cs` | |
| BB-4 | Water/Lavaタイルが定義済みだがダンジョン生成で一度も配置されない。テーマ別環境タイルが完全に欠落 | 高 | `DungeonGenerator.cs`, `DungeonFeatureGenerator.cs` | |
| BB-5 | SecretRoomSystem.CalculateDiscoveryChance()/CalculateSecretRoomCount()が定義済みだがDungeonGeneratorから呼ばれない。秘密の部屋が報酬なしのSecretDoorタイルのみ | 中 | `DungeonGenerator.cs:520-558` | |
| BB-6 | 宝箱がChestOpened=falseで配置されるが開封インタラクションシステムが未接続。宝箱を実際に開けて中身を取得する手段がない | 致命的 | `DungeonGenerator.cs:583-627` | |
| BB-7 | 祠（Shrine）部屋にAltarタイルが配置されるが祝福/バフ付与システムが未接続。祈祷やバフの実効果なし | 高 | `RoomCorridorGenerator.cs:108-109` | |
| BB-8 | フロア深度による部屋サイズのスケーリングなし。全階層で同一サイズの部屋が生成され、深層ほど複雑/広大になる設計が未実装 | 中 | `DungeonGenerator.cs:197-212` | |
| BB-9 | 無限ダンジョンの50階以降が全て同一ティア（Abyss）。マイルストーン報酬（50/100/150階等）やスコアボード永続化が未実装 | 中 | `InfiniteDungeonSystem.cs:17-38` | |
| BB-10 | 階段の到達性検証がDecorateRooms()後に再検証されない。装飾で配置された柱が階段へのルートを塞ぐ可能性あり | 中 | `DungeonGenerator.cs:365-409` | |

---

## カテゴリBC: 死亡・ゲームオーバーの不整合（7件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BC-1 | Ironmanモード（PermaDeath=true）でもセーブファイルが削除されない。SaveManager.DeleteSave()がGameOver時に呼ばれず、永久死がセーブ削除を伴わない | 致命的 | `GameController.cs:3061-3074`, `DifficultySettings.cs:45,172` | |
| BC-2 | DeathCauseに火傷/溺死/環境ダメージ/即死の4種が未定義。StatusEffect由来の死亡が全て「不明な原因」と表示される | 高 | `Enums.cs:126-139` | |
| BC-3 | GameOverSystem.GetDeathCauseDetail()にPoison/Fall/Suicide/SanityDeathのメッセージが未定義。これらの死因で「不明な原因」が表示される | 高 | `GameOverSystem.cs:76-88` | |
| BC-4 | TickStatusEffects()後にHP0チェックがない。状態異常ダメージで死亡した場合、死因追跡が不正確（後段の処理で別の原因に上書きされる可能性） | 高 | `GameController.cs:2184,2361-2366` | |
| BC-5 | ゲームオーバー画面にDeathLogSystemの詳細データ（与ダメージ/被ダメージ/敵撃破数等）が表示されない。フロア数と経過時間のみ表示 | 中 | `MainWindow.xaml.cs:631-665` | |
| BC-6 | 死亡時にゴールド/経験値ペナルティが一切ない。Rebirthで全リソースを保持したまま復活可能で、死のリスクが低い | 中 | `GameController.cs:3020-3076` | |
| BC-7 | 復活アイテム（フェニックスの羽等）/復活魔法が未実装。死亡回避手段がSanityによるRebirth以外に存在しない | 中 | `GameController.cs`, `ItemFactory.cs` | |

---

## カテゴリBD: キーバインド・UI・難易度設定の不整合（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BD-1 | Tキーに「アイテム投げ（ThrowItem）」と「街に入る（EnterTown）」の2アクションが重複バインド。コンテキスト判定で軽減されるが根本的なキー競合 | 致命的 | `KeyBindingSettings.cs:162,165` | |
| BD-2 | OpenCookingとQuitの2アクションにデフォルトキーバインドが未定義。キーボードからアクセス不可能 | 高 | `KeyBindingSettings.cs:145-193` | |
| BD-3 | DifficultySettingsの80%の乗数（ExpMultiplier/EnemyStatMultiplier/HungerDecayMultiplier/DamageTakenMultiplier/DamageDealtMultiplier）がGameControllerで使用されない。Hard/Nightmareを選択してもゲーム難易度がほぼ変わらない | 致命的 | `DifficultySettings.cs`, `GameController.cs:205` | |
| BD-4 | MainWindow.xamlのKeyBindActionマッピングにOpenCooking/Quitのcase文が欠落。キーバインドがあっても変換されずnull返却 | 高 | `MainWindow.xaml.cs:243-269` | |

---

## カテゴリBE: パーティ・隊列システムの欠落（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BE-1 | パーティ隊列（Formation）システムが存在しない。CombatStanceSystemはプレイヤー個人の構え（攻撃的/防御的/均衡）のみで、複数メンバーの戦術的配置が未実装 | 高 | `CombatStanceSystem.cs:1-62` | |
| BE-2 | パーティ全体への回復/バフ伝播メカニクスが存在しない。コンパニオンへの範囲回復やグループバフが未実装 | 高 | `GameController.cs` | |

---

## カテゴリBF: ランダムイベントのクールダウン欠如（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BF-1 | RandomEventSystemにクールダウン/重複防止メカニズムがない。RollEvent()が毎ターン同じイベントを連続発生させる可能性あり（TutorialSystemにはHashSetでの防止あり） | 中 | `WorldMapSystem.cs:794-811` | |

---

## カテゴリBG: バフ・デバフ・状態異常の未機能（11件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BG-1 | StatusEffectのAttackMultiplier/DefenseMultiplier/AllStatsMultiplierが一度も適用されない。GetAllStatModifiers()がStatModifierのみ処理し、乗算フィールドを無視。CreateStrengthBuff(1.25x)/CreateProtection(1.50x)/CreateBlessing(1.10x)の全バフが実効果ゼロ | 致命的 | `Character.cs:32-37`, `StatusEffectSystem.cs:267,279` | |
| BG-2 | StatusEffectsリストがセーブデータに含まれない。ゲームロード時に全てのアクティブバフ/デバフ/呪いが消失する | 致命的 | `SaveData.cs`, `Character.cs:79` | |
| BG-3 | StatusEffect適用時にCheckResistance()がPoison免疫チェックのみ実行。他の状態異常（Stun/Freeze/Sleep等）の耐性値が完全に無視される | 高 | `StatusEffectSystem.cs:297-344`, `GameController.cs:1940-2000` | |
| BG-4 | Equipment.OnEquip()のStatBuffエフェクトループが空実装。装備エンチャントのバフ効果が一切適用されない | 高 | `Equipment.cs:107-114` | |
| BG-5 | 祈祷の祝福バフがAllStatsMultiplier=1.10fを設定するが、BG-1により実効果ゼロ。祈祷が完全に視覚的のみ | 高 | `GameController.cs:4414-4416` | |
| BG-6 | 呪いステータスがAllStatsMultiplier=0.80fを設定するが、BG-1により実効果ゼロ。呪いが設計より20%弱い | 高 | `StatusEffectSystem.cs:174-181` | |
| BG-7 | PotionType.CureAllがStun/Freeze/Sleep/Petrification/Curseを除去対象に含まない。重篤なデバフの治療手段がない | 中 | `Consumables.cs:85-96` | |
| BG-8 | 食品アイテムがバフ効果を一切付与しない。Food.Use()がModifyHunger()/Heal()のみ呼び出し、戦術的な食事バフが存在しない | 中 | `Consumables.cs:150-218` | |
| BG-9 | アクティブバフ数に上限がない。無制限のバフスタックが可能で、パフォーマンス劣化やバランス崩壊の原因となりうる | 中 | `Character.cs:124-146` | |
| BG-10 | 環境デバフ（寒冷/灼熱/湿潤）のStatusEffectTypeが未定義。環境ハザードが状態異常を付与しない | 中 | `StatusEffectSystem.cs`, `Enums.cs` | |
| BG-11 | 状態異常メッセージの一部がenum名（英語）をそのまま表示。ローカライズされた日本語名でなく「Vulnerability」等と表示される | 低 | `GameController.cs` | |

---

## カテゴリBH: 転職・職業・スキルの未機能（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BH-1 | Player.CharacterClassプロパティがprivate set。転職ロジック（TryClassChange）が動作してもクラスを実際に変更不可能。O-2の根本原因 | 致命的 | `Player.cs:19`, `GameController.cs:7261-7282` | |
| BH-2 | SkillDatabaseの6つのパッシブスキル（weapon_mastery/hp_boost/mp_boost/poison_resist/critical_eye/treasure_sense）が定義のみで効果実装なし。ダメージ計算/HP/MP/耐性/クリティカル/ルート率に一切影響しない | 致命的 | `SkillSystem.cs:95,156-160`, `GameController.cs:1440-1515`, `Player.cs:247-248` | |
| BH-3 | GetLearnableSkills()がスキルメニュー表示時に呼ばれない。職業制限なしで全スキルが習得可能 | 高 | `SkillSystem.cs:336-338`, `GameController.cs` | |
| BH-4 | パッシブスキル「knowledge_collect」（バード自動鑑定能力）が登録されるが自動鑑定ロジックなし | 高 | `SkillSystem.cs:142` | |
| BH-5 | ProficiencySystem.GetBonusDamage()が一つのコードパスでのみ呼ばれ、他の戦闘パスではバイパスされる | 中 | `GameController.cs:1455-1459` | |

---

## カテゴリBI: ショップ経済・投資・通貨の未機能（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BI-1 | ショップ在庫がランダム性ゼロの固定リスト。同じショップタイプで毎回同一アイテムが並ぶ | 高 | `WorldMapSystem.cs:527-631` | |
| BI-2 | ショップ在庫の時間経過リフレッシュが未実装。InitializeShop()が同一ロジックで再生成するのみ | 高 | `WorldMapSystem.cs:465-469`, `GameController.cs:5451-5454` | |
| BI-3 | InvestmentSystem.Invest()がGameControllerから一度も呼ばれない。投資したゴールドが永久に消失し、配当/損失通知なし | 高 | `InvestmentSystem.cs:1-72`, `GameController.cs:49` | |
| BI-4 | BaseConstructionSystem.GetDailyFoodProduction()がGameControllerから一度も呼ばれない。「畑」建設の自動食料生産が非機能 | 高 | `BaseConstructionSystem.cs:118-121`, `GameController.cs` | |
| BI-5 | PriceFluctuationSystemの需給/テリトリー/カルマ/評判修飾子がUI表示のみで実際のBuy()/Sell()価格に適用されない。表示価格と取引価格が不一致 | 高 | `PriceFluctuationSystem.cs:1-72`, `WorldMapSystem.cs:482-507` | |
| BI-6 | ショップ価格が難易度設定（ExpMultiplier等）を無視。Hard/Nightmareでも同一価格 | 中 | `WorldMapSystem.cs:516-541`, `GameController.cs:205` | |
| BI-7 | 取引に手数料/税金システムが存在しない。売却時の固定60%損失以外の経済シンクなし | 中 | `WorldMapSystem.cs:482-507` | |
| BI-8 | 通貨がゴールド単一。ジェム/トークン等の高額通貨や通貨交換メカニクスが未実装 | 中 | `Enums.cs:517`, `GameController.cs` | |
| BI-9 | NPC購入嗜好/値切り/物々交換システムが未実装。全NPCで同一の売買ロジック | 低 | `NpcSystem.cs`, `WorldMapSystem.cs` | |

---

## カテゴリBJ: ワールドマップ・テリトリー探索の未機能（10件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BJ-1 | RollTerritoryEvent()がGameControllerから一度も呼ばれない。テリトリー固有イベント（森林の精霊/山岳の崩落等）が完全に非トリガー | 致命的 | `WorldMapSystem.cs:743-791`, `GameController.cs` | |
| BJ-2 | 旅行イベント（商人遭遇/盗賊襲撃/宝箱発見等）が表示されるが解決ハンドラなし。報酬/戦闘/選択が発生しない装飾イベント | 高 | `GameController.cs:4961-4973` | |
| BJ-3 | テリトリー間で敵/アイテムの差異なし。ダンジョン生成がテリトリーコンテキストを無視して同一プール使用 | 高 | `DungeonGenerator.cs`, `LocationMapGenerator.cs` | |
| BJ-4 | シンボルマップが全ロケーションを即座に表示（霧の向こう機能なし）。IsExplored追跡がダンジョンタイルのみでワールドマップに存在しない | 高 | `SymbolMapSystem.cs:38-51` | |
| BJ-5 | テリトリー発見時にXP/報酬/実績が一切付与されない。VisitedTerritoriesの追跡が報酬に未連動 | 高 | `WorldMapSystem.cs`, `GameController.cs` | |
| BJ-6 | テリトリー間の高速移動（ファストトラベル）が未実装。隣接テリトリーへの順次移動のみで後半のバックトラッキングが冗長 | 中 | `WorldMapSystem.cs:208-238` | |
| BJ-7 | ダンジョン内イベントがテリトリー別に分化しない。全テリトリーで同一のイベントプール使用 | 中 | `WorldMapSystem.cs:794-811` | |
| BJ-8 | TravelEventType列挙型の6タイプ（Merchant/Ambush/HelpRequest/Shrine/BadWeather/TreasureChest）に解決ロジックなし | 中 | `WorldMapSystem.cs:317-325` | |
| BJ-9 | テリトリー境界が静的定義のみ。動的な国境変動/戦争/占領メカニクスが未実装 | 低 | `WorldMapSystem.cs:129-173` | |
| BJ-10 | SymbolMapEventSystemとWorldMapSystemに重複するイベント処理が存在。責任境界が不明確 | 低 | `SymbolMapEventSystem.cs:46-57`, `WorldMapSystem.cs:743-987` | |

---

## カテゴリBK: 正気度システムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BK-1 | 正気度が戦闘パフォーマンスに影響しない。Player.Sanityがミミック検出（line 2398）のみで使用され、命中率/ダメージ/防御力に一切の修正なし | 高 | `GameController.cs:2398`, `Character.cs` | |
| BK-2 | RestSystemの正気度回復がGameControllerで適用されない。休息品質に基づく回復率（0.05-0.2）が定義済みだが未呼出 | 高 | `RestSystem.cs:9`, `GameController.cs:7354,7376` | |
| BK-3 | 正気度回復アイテム（回復ポーション/瞑想/祈祷ボーナス）が未実装。正気度は減少のみで回復手段が死亡時のRescue以外にない | 中 | `Player.cs:400-407`, `ItemFactory.cs` | |
| BK-4 | 幻覚エフェクトが定義済み（Perception-0.3f）だが低正気度で発動しない。マップ歪曲/偽敵等の視覚効果も未実装 | 中 | `ExtendedStatusEffectSystem.cs:35`, `GameController.cs` | |

---

## カテゴリBL: カルマシステムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BL-1 | KarmaSystem.GetShopPriceModifier()がGameControllerから一度も呼ばれない。PriceFluctuationSystemが独自に再計算するため完全なデッドコード | 高 | `KarmaSystem.cs:60-70`, `GameController.cs:5483-5514` | |
| BL-2 | KarmaSystem.GetNpcDispositionModifier()がGameControllerから一度も呼ばれない。聖者も悪人もNPC対話結果が同一 | 高 | `KarmaSystem.cs:72-83`, `GameController.cs:5694,5707` | |
| BL-3 | KarmaSystem.CanEnterHolyGround()がGameControllerから一度も呼ばれない。Criminal/Villainプレイヤーが教会/神殿に無制限アクセス可能 | 高 | `KarmaSystem.cs:89`, `GameController.cs` | |
| BL-4 | カルマ変動トリガーが3箇所のみ（処刑+5/+3、密輸-5、闇市-2）。NPC支援/窃盗/対話中の嘘等の大半の選択がカルマ無影響 | 中 | `GameController.cs:1522,7646,7709` | |

---

## カテゴリBM: 評判システムの未機能（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BM-1 | ReputationSystem.GetShopDiscount()がGameControllerから一度も呼ばれない。PriceFluctuationSystem.GetReputationModifier()で代替計算され、本来のメソッドがデッドコード | 高 | `ReputationSystem.cs:53-63`, `GameController.cs:5482,5513` | |
| BM-2 | ReputationSystem.IsWelcome()がGameControllerから一度も呼ばれない。「嫌悪」評判でもテリトリーに自由アクセス可能 | 高 | `ReputationSystem.cs:79-80`, `GameController.cs` | |
| BM-3 | 評判は増加のみ（正の修正値）で低下メカニクスが存在しない。クエスト失敗/NPC裏切り/犯罪行為が評判無影響 | 中 | `GameController.cs`, `NpcSystem.cs` | |

---

## カテゴリBN: NPC関係性・好感度の未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BN-1 | NPC好感度データがRebirth（死亡→転生）時に引き継がれない。NpcSystem.CreateTransferData()が存在するがGameControllerで一度も呼ばれず、死亡時に全NPCの好感度が50にリセット | 致命的 | `Player.cs:484-496`, `NpcSystem.cs:122-129`, `GameController.cs:3017,3097,3120` | |
| BN-2 | ModifyAffinity()が対話システムの2箇所でのみ呼ばれる。クエスト完了/贈り物/戦闘協力/祈祷等のアクションが好感度に影響しない | 高 | `GameController.cs:5694,5707`, `NpcSystem.cs:108-112` | |
| BN-3 | 好感度ランクに基づく特殊対話/クエスト解放が未実装。高好感度NPCも低好感度NPCも同一の対話ツリー | 高 | `NpcSystem.cs:177-196`, `GameController.cs:5641-5710` | |
| BN-4 | NpcDefinition.GetAffinityRank()がUI表示に使用されない。プレイヤーがNPCとの関係レベルを確認する手段がない | 中 | `NpcSystem.cs:16-23`, `GameController.cs:6184` | |

---

## カテゴリBO: RelationshipSystemの完全未使用（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BO-1 | RelationshipSystem全体がGameControllerで初期化・リセットのみされ、SetRelation()/GetRelation()/ModifyRelation()が一度も呼ばれない。種族/テリトリー/宗教/個人関係の4種の関係性追跡が完全にデッドコード | 致命的 | `RelationshipSystem.cs:1-78`, `GameController.cs:55,3146` | |

---

## カテゴリBP: 価格計算システムの重複・不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BP-1 | ReputationSystem/KarmaSystem/RelationshipSystemの各GetShopDiscount()が全てデッドコード。PriceFluctuationSystemのみが使用され、3システムの価格メソッドが冗長 | 高 | `ReputationSystem.cs:53-63`, `KarmaSystem.cs:60-70`, `RelationshipSystem.cs:52-59` | |
| BP-2 | カルマ/評判がNPCエンカウント率に影響しない。WorldMapSystem.RollRandomEvent()の確率がカルマ/評判無関係の固定値 | 中 | `WorldMapSystem.cs:794-811` | |

---

## カテゴリBQ: セーブデータ永続性 — システムステート大量未保存（24件）

24個のシステムがCreateSaveData()/LoadSaveData()に含まれず、ロード時に全進捗が消失する。

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BQ-1 | KarmaSystemステート（カルマ値/履歴）がセーブされない。店舗価格/NPC態度/聖地アクセスに影響するカルマが消失 | 高 | `GameController.cs:41,6694-6801` | |
| BQ-2 | ReputationSystemステート（テリトリー別評判値）がセーブされない | 高 | `GameController.cs:42,6694-6801` | |
| BQ-3 | CompanionSystemステート（仲間/忠誠度/ステータス）がセーブされない。パーティ全消失 | 致命的 | `GameController.cs:43,6694-6801` | |
| BQ-4 | EncyclopediaSystemステート（モンスター討伐数/発見ティア）がセーブされない | 中 | `GameController.cs:44,6694-6801` | |
| BQ-5 | DeathLogSystemステート（死亡記録/統計）がセーブされない | 中 | `GameController.cs:45,6694-6801` | |
| BQ-6 | OathSystemステート（誓約/盟約進捗）がセーブされない | 中 | `GameController.cs:46,6694-6801` | |
| BQ-7 | SkillTreeSystemステート（習得スキル/パッシブ）がセーブされない。スキルツリー進捗全消失 | 致命的 | `GameController.cs:47,6694-6801` | |
| BQ-8 | BaseConstructionSystemステート（建設済み建物/アップグレード）がセーブされない。拠点建設全消失 | 致命的 | `GameController.cs:48,6694-6801` | |
| BQ-9 | InvestmentSystemステート（投資記録/リターン）がセーブされない | 中 | `GameController.cs:49,6694-6801` | |
| BQ-10 | GridInventorySystemステート（グリッドレイアウト）がセーブされない | 中 | `GameController.cs:50,6694-6801` | |
| BQ-11 | NpcMemorySystemステート（NPC記憶/知識）がセーブされない | 中 | `GameController.cs:54,6694-6801` | |
| BQ-12 | RelationshipSystemステート（NPC間関係値）がセーブされない | 中 | `GameController.cs:55,6694-6801` | |
| BQ-13 | ItemIdentificationSystemステート（アイテム識別状態）がセーブされない | 低 | `GameController.cs:56,6694-6801` | |
| BQ-14 | DungeonEcosystemSystemステート（生態系状態）がセーブされない | 中 | `GameController.cs:57,6694-6801` | |
| BQ-15 | PetSystemステート（ペット名簿/ステータス）がセーブされない。全ペット消失 | 致命的 | `GameController.cs:58,6694-6801` | |
| BQ-16 | MerchantGuildSystemステート（ギルドランク/進捗）がセーブされない | 中 | `GameController.cs:59,6694-6801` | |
| BQ-17 | InscriptionSystemステート（刻印ルーン/効果）がセーブされない | 中 | `GameController.cs:60,6694-6801` | |
| BQ-18 | FactionWarSystemステート（派閥戦争/領土）がセーブされない | 中 | `GameController.cs:61,6694-6801` | |
| BQ-19 | TerritoryInfluenceSystemステート（テリトリー影響力）がセーブされない | 中 | `GameController.cs:62,6694-6801` | |
| BQ-20 | ProficiencySystemステート（武器/スキル熟練度）がセーブされない | 中 | `GameController.cs:65,6694-6801` | |
| BQ-21 | DungeonShortcutSystemステート（発見済みショートカット）がセーブされない | 中 | `GameController.cs:66,6694-6801` | |
| BQ-22 | SmithingSystemステート（鍛冶レシピ/進捗）がセーブされない | 中 | `GameController.cs:67,6694-6801` | |
| BQ-23 | AchievementSystemステート（解除済み実績）がセーブされない。実績がロード時にリセット | 高 | `GameController.cs:68,6694-6801` | |
| BQ-24 | TutorialSystemステート（完了済みチュートリアルステップ）がセーブされない。毎回チュートリアル再表示 | 高 | `GameController.cs,SmithingSystem.cs:167-173` | |

---

## カテゴリBR: セーブデータ永続性 — プレイヤー/ゲームステート（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BR-1 | 病気タイプ/残りターン（_playerDisease/_diseaseRemainingTurns）がセーブされない。ロード時に病気が自動治癒 | 致命的 | `GameController.cs:90-93,6694-6801` | |
| BR-2 | 戦闘スタンス（_playerStance）がセーブされない。ロード時にBalancedにリセット | 中 | `GameController.cs:87,6694-6801` | |
| BR-3 | NG+ティア（_ngPlusTier）とゲームクリア状態（_hasCleared）がセーブされない。NG+進行が追跡不可 | 致命的 | `GameController.cs:155-158,6694-6801` | |
| BR-4 | 無限ダンジョンモード（_infiniteDungeonMode）とキル数（_infiniteDungeonKills）がセーブされない。100キル実績進捗消失 | 高 | `GameController.cs:164-167,6694-6801` | |
| BR-5 | ダンジョン特性（_currentDungeonFeature）がセーブされない。敵密度/ルート倍率/罠確率がリセット | 高 | `GameController.cs:78,6694-6801` | |
| BR-6 | Player.HasPrayedTodayがセーブされない。毎日の祈りフラグがリセット | 中 | `Player.cs:303,GameController.cs:6694-6801` | |

---

## カテゴリBS: Engine層戦闘計算不整合（20件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BS-1 | 武器ダメージレンジ（Weapon.DamageRange）が物理ダメージ計算で完全に無視される。EffectiveStats.PhysicalAttackのみ使用 | 致命的 | `Engine/Combat/CombatSystem.cs:154` | |
| BS-2 | 両手武器ダメージボーナスが未実装。IsTwoHandedプロパティがダメージ計算で参照されない | 高 | `Engine/Combat/CombatSystem.cs:140-213` | |
| BS-3 | 武器熟練度がSkillMultiplierとして誤適用（攻撃力ボーナスではなくスキル倍率に適用） | 中 | `Engine/Combat/CombatSystem.cs:195` | |
| BS-4 | 防御貫通（Armor Penetration）が完全未実装。防御は常にdefensePower×0.5の固定割合 | 高 | `Engine/Combat/DamageCalculator.cs:35` | |
| BS-5 | クリティカルダメージ倍率が1.5f固定。設計書の「特化ビルドで最大×2.5」が不可能 | 高 | `Engine/Combat/CombatSystem.cs:199` | |
| BS-6 | 防御計算が物理0.5/魔法0.3のハードコード定数。GameConstantsに未定義 | 中 | `Engine/Combat/DamageCalculator.cs:35,82` | |
| BS-7 | 再生（Regeneration）回復量がhealPerTick=3固定。MND/INTスケーリングなし | 中 | `Engine/StatusEffectSystem.cs:286` | |
| BS-8 | レベルによるダメージスケーリングが完全未実装。PhysicalDamageParams/MagicalDamageParamsにレベルフィールドなし | 高 | `Engine/Combat/DamageCalculator.cs,CombatSystem.cs` | |
| BS-9 | WeaponCritBonus/SkillCritBonusが常に0にハードコード。武器固有クリティカルボーナスが無効 | 高 | `Engine/Combat/CombatSystem.cs:293-294` | |
| BS-10 | ダメージ分散範囲が0.9-1.1固定。名前付き定数未定義 | 低 | `Engine/Combat/DamageCalculator.cs:39,86` | |
| BS-11 | 盾ブロック/パリィメカニクスがDamageCalculatorに未実装。Shield.BlockChanceが参照されない | 高 | `Engine/Combat/DamageCalculator.cs:192` | |
| BS-12 | 防具タイプ別物理耐性なし。重装/軽装が同じ防御計算 | 中 | `Engine/Combat/DamageCalculator.cs:32` | |
| BS-13 | 回復魔法の威力がBaseMpCost×3固定。MND/INTスケーリングなし | 高 | `Engine/Magic/SpellCastingSystem.cs:362-363` | |
| BS-14 | 呪文成功率にキャスターステータス（INT/MND）が反映されない。SpellParser固定値のみ | 高 | `Engine/Magic/SpellCastingSystem.cs:161` | |
| BS-15 | DoTダメージ（毒2%/猛毒5%）がハードコード。毒源による威力差なし | 中 | `Engine/StatusEffectSystem.cs:26,41` | |
| BS-16 | 攻撃者/防御者のレベル差によるダメージスケーリングなし | 中 | `Engine/Combat/DamageCalculator.cs,CombatSystem.cs` | |
| BS-17 | 武器の属性ダメージ増強未適用。Weapon.Elementは攻撃シグネチャにのみ使用 | 中 | `Engine/Combat/CombatSystem.cs:196` | |
| BS-18 | 武器攻撃速度（AttackSpeed）がダメージ分散に影響しない | 低 | `Engine/Combat/DamageCalculator.cs` | |
| BS-19 | オフハンド武器ダメージが未計算。二刀流時にメインハンドのみ処理 | 高 | `Engine/Combat/CombatSystem.cs:154` | |
| BS-20 | 武器射程（Weapon.Range）がダメージ計算で無視。近接武器が任意距離で全ダメージ | 低 | `Engine/Combat/CombatSystem.cs` | |

---

## カテゴリBT: アイテム生成・エンチャント・クラフト不整合（17件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BT-1 | GenerateRandomEquipment()が4種のみ生成（IronSword/Dagger/LeatherArmor/WoodenShield）。Greatsword/BattleAxe/Spear等10+装備が生成不可 | 致命的 | `ItemFactory.cs:865-874` | |
| BT-2 | GenerateRandomConsumable()が4種のみ生成。8+巻物/7+ポーション（Fireball/Lightning/Strength/Invisibility等）が生成不可 | 致命的 | `ItemFactory.cs:910-919` | |
| BT-3 | Scroll.Use()のswitchで3種（Freeze/Summon/Return）がdefaultに落ち「何も起こらなかった」を返す | 高 | `Consumables.cs:256-319` | |
| BT-4 | レアリティスケーリングが弱すぎ。Depth 1とDepth 100のLegendary確率が同じ5%上限 | 中 | `ItemFactory.cs:848-861` | |
| BT-5 | エンチャントがEquipmentItemに実際に保存されない。Enchant()がsuccessを返すが**アイテムオブジェクトを変更しない** | 致命的 | `EnchantmentSystem.cs:139,Equipment.cs:60-76` | |
| BT-6 | エンチャント種別制限なし。ダメージエンチャント（FireDamage等）が防具に適用可。Lifestealが防具に適用可 | 高 | `EnchantmentSystem.cs:128-134` | |
| BT-7 | 8エンチャント種（Lifesteal/ManaSteal/ParalysisChance/ExpBoost/DropBoost/CriticalBoost/SpeedBoost/DefenseBoost）の効果計算が未実装（return 0） | 致命的 | `EnchantmentSystem.cs:186-206` | |
| BT-8 | エンチャント除去/置換メソッドが存在しない。RemoveEnchantment()/ReplaceEnchantment()なし | 高 | `EnchantmentSystem.cs:1-209` | |
| BT-9 | 複数エンチャント非対応。同一装備に2つ以上のエンチャントを持てない | 中 | `Equipment.cs:60-76` | |
| BT-10 | クラフトレシピが参照する`material_coal`がItemDefinitionsに定義されていない。鋼鉄の剣がクラフト不可 | 致命的 | `CraftingSystem.cs:275,ItemFactory.cs:1159-1260` | |
| BT-11 | クラフトにスキル/ツール/職業要件なし。全職業が全アイテムをクラフト可能 | 中 | `CraftingSystem.cs:92-111` | |
| BT-12 | ItemFactory.GenerateLoot()がDropTableSystemを使用しない。ボスドラゴンがゴブリンと同じランダムアイテムをドロップ | 致命的 | `ItemFactory.cs:950-969` | |
| BT-13 | 非人型敵（Dragon/Undead/Demon）がゴールドをドロップしない。Humanoidのみゴールド生成 | 高 | `DropTableSystem.cs:188` | |
| BT-14 | 宝箱システム（TreasureChest/ChestGeneration）が存在しない。ダンジョン宝箱のアイテム生成不可 | 高 | リポジトリ全体 | |
| BT-15 | 強化レベルの上限検証なし。Legendary生成でEnhancementLevel=9が可能だが上限チェックなし | 低 | `ItemFactory.cs:879-886` | |
| BT-16 | 未識別アイテムのUnidentifiedNameが設定されない。IsIdentified=falseだが未識別名なし | 中 | `ItemFactory.cs:888-890` | |
| BT-17 | 呪いアイテム率の計算式が不適切。Math.Min(5+depth*2,30)でdepth=13で31→30にclamp | 低 | `ItemFactory.cs:892-896` | |

---

## カテゴリBU: 時間・デイリー処理・チュートリアル・実績不整合（17件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BU-1 | 日変更検出メカニズムが存在しない。GameTime.Dayの変化を検出するパターンなし | 致命的 | `GameController.cs:2125+` | |
| BU-2 | ProcessReligionDailyTick()が定義済みだが**一度も呼び出されない**。信仰減衰/背教呪い/祈り日数が機能しない | 高 | `GameController.cs:4882` | |
| BU-3 | PetSystem.TickHunger()が一度も呼び出されない。ペットの空腹/忠誠度減衰が機能しない | 中 | `GameController.cs:58,PetSystem.cs:112` | |
| BU-4 | GameTimeのStartYear/StartMonth/StartDay/StartHour/StartMinuteがSaveDataに含まれない。ロード時にデフォルト値にリセット | 中 | `GameTime.cs:52-64,SaveData.cs:241-244` | |
| BU-5 | コンパニオンの忠誠度減衰/空腹メカニクスが未実装。戦闘時のみ処理される | 中 | `CompanionSystem.cs,GameController.cs:1796` | |
| BU-6 | 施設/ショップの営業時間チェックなし。TimeOfDaySystemが施設可用性に接続されていない | 低 | `WorldMapSystem.cs:372` | |
| BU-7 | NPC行動スケジュールが600ターンごとの固定間隔で更新。時間帯変更との連動なし | 中 | `GameController.cs:2313-2320` | |
| BU-8 | 天候システムの日次リセットなし。300ターンごとの固定間隔で変化 | 低 | `GameController.cs:2329-2337` | |
| BU-9 | TurnCountとGameTime.TotalTurnsの二重追跡。同期保証メカニズムなし | 低 | `GameController.cs:191-192` | |
| BU-10 | DifficultySettings.ExpMultiplier（0.6～1.5）が経験値獲得に適用されない。GoldMultiplierは適用済み | 高 | `GameController.cs:1540,DifficultySettings.cs:18-42` | |
| BU-11 | AchievementSystemステートがセーブされない（BQ-23と同根）。実績解除がセッション間で消失 | 高 | `GameController.cs:68,1727,2357-2359` | |
| BU-12 | TutorialSystemステートがセーブされない（BQ-24と同根）。完了済みチュートリアルが毎回再表示 | 高 | `SmithingSystem.cs:167-173` | |
| BU-13 | チュートリアル個別スキップ機能なし。IsEnabled=falseで全無効のみ。個別ステップのスキップ不可 | 中 | `SmithingSystem.cs:92,140-152` | |
| BU-14 | 難易度設定がゲーム中に制限なく変更可能。SetDifficulty()にターン数/ダンジョン状態のバリデーションなし | 中 | `GameController.cs:3228-3233` | |
| BU-15 | EncyclopediaSystemの発見進捗がセーブされない（BQ-4と同根）。モンスター討伐数が消失 | 中 | `GameController.cs:44,7092` | |
| BU-16 | DeathLogSystemの死亡記録がセーブされない（BQ-5と同根）。メタ統計が消失 | 中 | `GameController.cs:45,DeathLogSystem.cs:42-79` | |
| BU-17 | 時間ベースのステータス効果期限切れ未実装。「日没まで持続」等の効果パターンなし | 低 | `StatusEffect.cs,TimeOfDaySystem.cs` | |

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
| AB: セーブデータ追加欠落 | 5 | 3 | 1 | 0 | 0 | 9 |
| AC: クエストシステム | 3 | 1 | 1 | 0 | 0 | 5 |
| AD: 実績・エンディング | 2 | 0 | 1 | 0 | 0 | 3 |
| AE: マップ生成・タイル | 2 | 4 | 0 | 0 | 1 | 7 |
| AF: ランダムイベント | 3 | 0 | 0 | 0 | 0 | 3 |
| AG: ギャンブルシステム | 0 | 1 | 1 | 0 | 0 | 2 |
| AH: Engine層Enum/計算 | 2 | 3 | 0 | 0 | 0 | 5 |
| AI: ダメージ計算 | 0 | 0 | 2 | 0 | 0 | 2 |
| AJ: デッドコードシステム | 0 | 2 | 1 | 0 | 0 | 3 |
| AK: カルマ・無限D永続性 | 1 | 2 | 0 | 0 | 0 | 3 |
| AL: 天候効果の未適用 | 1 | 2 | 0 | 0 | 0 | 3 |
| AM: 釣り・採掘ID不一致 | 2 | 2 | 0 | 0 | 0 | 4 |
| AN: ルーン・エンチャント | 2 | 2 | 0 | 0 | 0 | 4 |
| AO: ボスフロア・報酬 | 2 | 2 | 0 | 0 | 0 | 4 |
| AP: 建設・サウンド未接続 | 1 | 0 | 1 | 0 | 0 | 2 |
| AQ: パズル・ターンカウンタ | 0 | 0 | 2 | 0 | 0 | 2 |
| AR: スキル・パッシブ・状態異常 | 3 | 2 | 3 | 0 | 0 | 8 |
| AS: セーブデータ詳細欠落 | 2 | 1 | 2 | 0 | 0 | 5 |
| AT: 休息・回復メカニクス | 0 | 2 | 1 | 1 | 0 | 4 |
| AU: 移動コスト・地形効果 | 0 | 1 | 2 | 0 | 0 | 3 |
| AV: 時間帯・渇き・疲労・衛生 | 0 | 5 | 1 | 0 | 0 | 6 |
| AW: 誓約・信仰・評判 | 1 | 2 | 2 | 0 | 0 | 5 |
| **AX: コンパニオンシステム** | **2** | **1** | **1** | **0** | **0** | **4** |
| **AY: ペットシステム未機能** | **2** | **2** | **0** | **0** | **0** | **4** |
| **AZ: 敵AI・行動パターン** | **0** | **1** | **2** | **0** | **0** | **3** |
| **BA: 対話・百科事典・イベント** | **0** | **1** | **3** | **0** | **0** | **4** |
| **BB: ダンジョン生成・部屋タイプ** | **1** | **5** | **4** | **0** | **0** | **10** |
| **BC: 死亡・ゲームオーバー** | **1** | **3** | **3** | **0** | **0** | **7** |
| **BD: キーバインド・UI・難易度** | **2** | **2** | **0** | **0** | **0** | **4** |
| **BE: パーティ・隊列システム** | **0** | **2** | **0** | **0** | **0** | **2** |
| **BF: ランダムイベントクールダウン** | **0** | **0** | **1** | **0** | **0** | **1** |
| **BG: バフ・デバフ・状態異常** | **2** | **4** | **4** | **1** | **0** | **11** |
| **BH: 転職・職業・スキル** | **2** | **2** | **1** | **0** | **0** | **5** |
| **BI: ショップ経済・投資・通貨** | **0** | **5** | **3** | **1** | **0** | **9** |
| **BJ: ワールドマップ・テリトリー** | **1** | **4** | **3** | **2** | **0** | **10** |
| **BK: 正気度システム** | **0** | **2** | **2** | **0** | **0** | **4** |
| **BL: カルマシステム** | **0** | **3** | **1** | **0** | **0** | **4** |
| **BM: 評判システム** | **0** | **2** | **1** | **0** | **0** | **3** |
| **BN: NPC関係性・好感度** | **1** | **2** | **1** | **0** | **0** | **4** |
| **BO: RelationshipSystem未使用** | **1** | **0** | **0** | **0** | **0** | **1** |
| **BP: 価格計算の重複** | **0** | **1** | **1** | **0** | **0** | **2** |
| **BQ: セーブ永続性(システム)** | **4** | **4** | **14** | **1** | **0** | **24** |
| **BR: セーブ永続性(プレイヤー)** | **2** | **2** | **2** | **0** | **0** | **6** |
| **BS: Engine層戦闘計算** | **1** | **10** | **6** | **3** | **0** | **20** |
| **BT: アイテム生成・エンチャント** | **6** | **4** | **4** | **3** | **0** | **17** |
| **BU: 時間・チュートリアル・実績** | **1** | **4** | **6** | **3** | **0** | **17** |
| **合計** | **85** | **133** | **115** | **19** | **2** | **365** |

---

## 修正方針（ユーザー判断待ち）

ユーザーが各項目の「修正判断」欄に ✅（修正する）/ ❌（修正しない）/ 🔄（保留）を記入後、
確定した修正対象をまとめて実装する。

### 修正優先度の目安
1. **致命的**（85件）: 使用するとクラッシュ/データ消失/機能しない → 最優先で修正推奨
2. **高**（133件）: 設計と実装の明確な乖離 → 修正推奨
3. **中**（115件）: 違和感・バランス問題 → 選択的に修正
4. **低**（19件）: 軽微なテーマ不一致 → 余裕があれば修正
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

### 新規追加カテゴリの概要（第5回調査分）
- **AJ: デッドコードシステム** — MerchantGuildSystem/TerritoryInfluenceSystem/ModLoaderSystemの3システムが完全なデッドコード。初期化・リセットのみで機能メソッド未呼び出し
- **AK: カルマ・無限D永続性** — KarmaSystemのカルマ値（ショップ価格・NPC態度に影響）がセーブ非対応。無限ダンジョンモード・建設済み施設もロード時消失
- **AL: 天候効果の未適用** — WeatherSystemの戦闘ダメージ修正/移動コスト修正/視界修正の3種がGameControllerから呼ばれず表示テキストのみ
- **AM: 釣り・採掘ID不一致** — FishingSystemの全9魚/GatheringSystemの全5鉱石のIDがItemFactoryに未定義。釣り・採掘の成果物が生成不能
- **AN: ルーン・エンチャント** — プレイヤーにルーン学習フィールドなし、エンチャントデータがItem/SaveDataに未永続化、ソウルジェムがItemFactory未定義
- **AO: ボスフロア・報酬** — ボスフロアで階段制限なし（ボス戦スキップ可能）、クエスト報酬ItemIdsが処理されず消失
- **AP: 建設・サウンド未接続** — BaseConstructionSystem.Build()未呼び出しで全施設建設不可、AmbientSoundの再生システムなし
- **AQ: パズル・ターンカウンタ** — EnvironmentalPuzzleがNPC対話のみでダンジョン配置なし、TurnCountがintでオーバーフローリスク

### 新規追加カテゴリの概要（第6回調査分）
- **AR: スキル・パッシブ・状態異常** — パッシブスキル（weapon_mastery/hp_boost等）が定義のみで効果ゼロ、TickStatusEffects()未呼び出しでバフ永続化、Charm/Madness状態異常の行動制限未実装、誓約違反チェック未呼び出し
- **AS: セーブデータ詳細欠落** — Item.Grade（品質）未保存でレア品がStandardに降格、地面アイテム全消失、マップ探索状態リセット、セーブバージョン移行ロジックなし
- **AT: 休息・回復メカニクス** — 休息時MP/Sanity回復が未適用（取得のみ）、休息中の食料/水消費なし
- **AU: 移動コスト・地形効果** — 地形別移動コスト（森1.5/山2.0等）がTileに定義済みだが移動計算で無視、斜め移動/重量超過ペナルティがint切り捨てで無効化
- **AV: 時間帯・渇き・疲労・衛生効果** — 時間帯の視界/スポーン率修正が未適用、渇き/疲労/衛生のステータスペナルティが未適用（表示テキストのみ）
- **AW: 誓約・信仰・評判** — 誓約ボーナス（経験値/ドロップ率）未適用、誓約破棄ペナルティなし、信仰の献金メカニクス未実装、評判によるクエスト解放未使用

### 新規追加カテゴリの概要（第7回調査分）
- **AX: コンパニオンシステム** — コンパニオンが被ダメージゼロで不死身、ステータス成長なし、AIモード変更不可、死亡時の通知なし
- **AY: ペットシステム未機能** — Feed()/Train()/TickHunger()未呼び出し、6種の特殊能力が全てテキスト定義のみ、騎乗速度ボーナス未適用、忠誠度が装飾
- **AZ: 敵AI・行動パターン** — ボスのバーサーク状態が無効果、SummonerBehaviorが完全なデッドコード、敵の逃走行動が確認不能
- **BA: 対話・百科事典・イベント** — 16体NPCの対話IDが未登録、シンボルマップイベント10種が未トリガー、10階チュートリアル未発火、百科事典の閲覧UI不在
- **BB: ダンジョン生成・部屋タイプ** — 4つのRoomType未生成、9テーマのパラメータ未適用、Water/Lavaタイル未配置、宝箱開封不可、祠バフ未接続、無限ダンジョンマイルストーン未実装
- **BC: 死亡・ゲームオーバー** — Ironmanモードでセーブ未削除、4種の死因メッセージ未定義、状態異常死後のHP0チェック欠如、死亡統計未表示、復活アイテム未実装
- **BD: キーバインド・UI・難易度** — Tキー重複バインド、2アクションのキー未定義、難易度設定の80%の乗数が未使用
- **BE: パーティ・隊列システム** — 隊列システム自体が存在しない、パーティ全体回復/バフ伝播なし
- **BF: ランダムイベントクールダウン** — イベント連続発生防止メカニズムなし

### 新規追加カテゴリの概要（第8回調査分）
- **BG: バフ・デバフ・状態異常** — バフ乗数（AttackMultiplier/DefenseMultiplier/AllStatsMultiplier）が完全未適用、状態異常がセーブに含まれない、耐性チェックがPoison免疫のみ、装備エンチャントバフ空実装、祝福/呪い実効果ゼロ
- **BH: 転職・職業・スキル** — Player.CharacterClassがprivate setで変更不可（転職の根本障害）、6パッシブスキル効果未実装、職業制限なしで全スキル習得可能
- **BI: ショップ経済・投資・通貨** — 在庫ランダム性ゼロ/リフレッシュなし、InvestmentSystem完全未呼出、BaseConstruction食料生産未適用、PriceFluctuation修飾子がBuy/Sell未適用（UI表示のみ）
- **BJ: ワールドマップ・テリトリー** — テリトリー固有イベント未トリガー、旅行イベント解決ハンドラなし、テリトリー間の敵/アイテム差異なし、霧の向こう機能なし、探索XP/報酬なし
- **BK: 正気度システム** — 正気度が戦闘パフォーマンス無影響、休息の正気度回復未適用、回復アイテム/幻覚エフェクト未実装
- **BL: カルマシステム** — GetShopPriceModifier/GetNpcDispositionModifier/CanEnterHolyGround全てデッドコード、カルマ変動トリガーが3箇所のみ
- **BM: 評判システム** — GetShopDiscount/IsWelcome全てデッドコード、評判低下メカニクス不在
- **BN: NPC関係性・好感度** — 好感度データがRebirth時に消失（CreateTransferData未呼出）、好感度変動が対話2箇所のみ、好感度ランクによる対話分岐なし
- **BO: RelationshipSystem未使用** — 全メソッドが完全デッドコード（初期化/リセットのみ）
- **BP: 価格計算の重複** — 3システムの価格メソッドがPriceFluctuationSystemに重複、エンカウント率がカルマ/評判無関係

### 新規追加カテゴリの概要（第9回調査分）
- **BQ: セーブデータ永続性（システムステート大量未保存）** — Karma/Reputation/Companion/SkillTree/Pet/Achievement/Encyclopedia/BaseConstruction等24システムのステートがSaveDataに含まれない。ロード時に全進捗消失
- **BR: セーブデータ永続性（プレイヤー/ゲームステート）** — 病気状態/戦闘スタンス/NG+ティア/ゲームクリア状態/無限ダンジョンモード/ダンジョン特性がセーブされない
- **BS: Engine層戦闘計算不整合** — 武器ダメージレンジ未参照/両手武器ボーナスなし/防御貫通なし/クリティカル上限固定/盾ブロックなし/オフハンド攻撃なし/レベルスケーリングなし
- **BT: アイテム生成・エンチャント・クラフト不整合** — 装備生成が4種のみ/消耗品生成が4種のみ/エンチャント効果が実際に適用されない/8エンチャント種が効果ゼロ/クラフト素材定義欠落
- **BU: 時間・デイリー処理・チュートリアル・実績** — 日変更検出なし/宗教デイリーティック未呼出/ペット空腹ティック未呼出/難易度ExpMultiplier未適用/実績セーブなし/チュートリアルスキップ不可
