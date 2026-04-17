# 実装計画書 Ver.prt.0.21 — 魔法言語システム修正・碑文システム構築

**ステータス**: ✅ 全タスク完了
**目的**: 魔法言語システムのビルドエラー修正、碑文システムの機能拡充（古代の書アイテム、図書館フロア、ルーン語一覧UI）
**テスト結果**: 5,665テスト全合格（Core: 5,517 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | TryReadRuneInscriptionメソッド未定義エラー修正 | バグ修正 | ✅ 完了 |
| 2 | SpellCastingSystem.LearnRandomWord参照エラー修正 | バグ修正 | ✅ 完了 |
| 3 | MagicLanguageSystemTestsの3件テスト失敗修正 | バグ修正 | ✅ 完了 |
| 4 | 古代の書（ancient_book）アイテム実装 | 新規実装 | ✅ 完了 |
| 5 | 図書館フロアのアイテム・碑文追加配置 | 新規実装 | ✅ 完了 |
| 6 | 習得済みルーン語一覧ウィンドウ（VocabularyWindow）実装 | 新規実装 | ✅ 完了 |
| 7 | Bキーバインド + GameAction.OpenVocabulary連携 | 新規実装 | ✅ 完了 |
| 8 | 新規テスト追加・既存テスト調整 | テスト | ✅ 完了 |

---

## 詳細

### タスク1: TryReadRuneInscriptionメソッド未定義エラー修正

- **問題**: `GameController.cs` で `TryReadRuneInscription()` が呼び出されていたが、メソッドが定義されていなかった（CS0103）
- **修正内容**:
  - `TryReadRuneInscription(Tile tile)` メソッドを新規作成
  - `tile.InscriptionWordId`、`tile.InscriptionRead` を参照し、`VocabularyAcquisitionSystem.LearnFromRuneStone()` で学習処理を実行
  - 既読碑文は再度読めないガード付き
- **変更ファイル**: `GameController.cs`

### タスク2: SpellCastingSystem.LearnRandomWord参照エラー修正

- **問題**: `GameController.cs` で `SpellCastingSystem.LearnRandomWord()` が呼び出されていたが、該当メソッドは `VocabularyAcquisitionSystem` に存在していた（CS0117）
- **修正内容**: `SpellCastingSystem.LearnRandomWord` → `VocabularyAcquisitionSystem.LearnRandomWord` に参照先を修正
- **変更ファイル**: `GameController.cs`

### タスク3: MagicLanguageSystemTestsの3件テスト失敗修正

- **問題**: 3つのテストで「未習得語」として `"brenna"` を使用していたが、Mageクラスの初期ルーン語セットに `"brenna"` が含まれていたためテストが失敗
- **修正内容**: テスト内の未習得語を `"brenna"` → `"binda"` に変更（どのクラスの初期ルーン語にも含まれない語）
- **対象テスト**:
  - `SpellCastingSystem_AddWord_UnlearnedWord_Fails`
  - `VocabularyAcquisition_LearnFromRuneStone_NewWord`
  - `Player_GetWordMastery_UnlearnedWord_ReturnsZero`
- **変更ファイル**: `MagicLanguageSystemTests.cs`

### タスク4: 古代の書（ancient_book）アイテム実装

- **問題**: `RacialTraitSystem` 等で `ancient_book` アイテムが参照されていたが、ItemFactory/ItemDefinitionsに定義が存在しなかった
- **修正内容**:
  - `ScrollType.AncientBook` 列挙値を追加
  - `ItemEffectType.LearnRuneWord` 列挙値を追加
  - `Scroll.Use()` に `ScrollType.AncientBook` のケースを追加（`ItemEffectType.LearnRuneWord` 効果を返す）
  - `ItemFactory.CreateAncientBook()` ファクトリメソッドを追加（レア度: Rare、基本価格: 300、効果値: 3）
  - `ItemDefinitions` の `_items` 辞書に `["ancient_book"]` を登録
  - `GameController.UseItem()` に `ItemEffectType.LearnRuneWord` ハンドリングを追加
- **変更ファイル**: `Consumables.cs`, `Item.cs`, `ItemFactory.cs`, `GameController.cs`

### タスク5: 図書館フロアのアイテム・碑文追加配置

- **問題**: `SpecialFloorType.Library` フロアに特別なアイテム配置がなく、通常フロアと同じ内容だった
- **修正内容**:
  - `GenerateFloor()` に図書館フロア検出コードを追加（`DetermineSpecialFloorType()` + メッセージ表示）
  - `SpawnLibraryFloorItems()` メソッドを新規作成:
    - 古代の書を2〜4冊ランダム配置（`ItemDefinitions.Create("ancient_book")`）
    - ルーン碑文を3〜5個追加配置（知識系の語プール: vita, sja, opna, loka, afrita, banna, ljos, myrkr, helgr, eilifr, heimr, styra）
    - 語の重複配置を防止
- **変更ファイル**: `GameController.cs`

### タスク6: 習得済みルーン語一覧ウィンドウ（VocabularyWindow）実装

- **問題**: プレイヤーが習得したルーン語の一覧を確認するUIが存在しなかった
- **修正内容**:
  - `VocabularyWindow.xaml` + `VocabularyWindow.xaml.cs` を新規作成
  - カテゴリフィルタ（全て / 効果語 / 対象語 / 属性語 / 修飾語 / 範囲語 / 時間語 / 条件語）
  - 各語の表示: 古ノルド語、意味、発音、理解度ラベル（初/知/学/習/熟/極）
  - 詳細パネル: 名前、発音、分類、MP/ターンコスト、理解度プログレスバー、難度（★表示）
  - Esc / R キーで閉じる
  - ダークテーマUI（既存ウィンドウと統一）
- **新規ファイル**: `VocabularyWindow.xaml`, `VocabularyWindow.xaml.cs`

### タスク7: Bキーバインド + GameAction.OpenVocabulary連携

- **問題**: VocabularyWindowを開くためのキーバインドとGameAction連携が未実装
- **修正内容**:
  - `KeyBindAction.OpenVocabulary` 列挙値を追加
  - `GameAction.OpenVocabulary` 列挙値を追加
  - `GameController` に `OnShowVocabulary` イベントを追加、`ProcessInput` でディスパッチ
  - デフォルトキーバインドに `Key.B` → `OpenVocabulary` を追加
  - 表示名に「ルーン語一覧」を追加
  - `MainWindow.xaml.cs` に `ShowVocabularyDialog()` ハンドラと `OnShowVocabulary` イベント接続を追加
- **変更ファイル**: `KeyBindingSettings.cs`, `GameController.cs`, `MainWindow.xaml.cs`

### タスク8: 新規テスト追加・既存テスト調整

- **追加テスト（4件）**:
  - `ItemDefinitions_Create_AncientBook_ReturnsScroll` — ancient_bookアイテムの生成確認
  - `AncientBook_Use_Returns_LearnRuneWord_Effect` — 使用時のLearnRuneWord効果確認
  - `SuccessRate_HighMastery_GivesBonus` — 理解度100での成功率ボーナス確認
  - `SuccessRate_LowMastery_HasPenalty` — 低理解度での成功率ペナルティ確認
- **既存テスト調整（1件）**:
  - `ItemDefinitions_TotalItemCount` — Expected値を81→82に更新（ancient_book追加分）
- **変更ファイル**: `MagicLanguageSystemTests.cs`, `EnemyItemExpansionTests.cs`

---

## 変更ファイル一覧

| ファイル | 種別 | 変更内容 |
|---------|------|---------|
| `src/RougelikeGame.Gui/GameController.cs` | 修正 | TryReadRuneInscription実装、LearnRandomWord参照修正、LearnRuneWord効果処理、Library検出、SpawnLibraryFloorItems追加、OpenVocabularyアクション追加 |
| `src/RougelikeGame.Core/Items/Consumables.cs` | 修正 | ScrollType.AncientBook追加、Scroll.Use()にAncientBookケース追加 |
| `src/RougelikeGame.Core/Items/Item.cs` | 修正 | ItemEffectType.LearnRuneWord追加 |
| `src/RougelikeGame.Core/Items/ItemFactory.cs` | 修正 | CreateAncientBook()ファクトリ追加、ItemDefinitions登録 |
| `src/RougelikeGame.Gui/VocabularyWindow.xaml` | 新規 | ルーン語一覧ウィンドウXAML |
| `src/RougelikeGame.Gui/VocabularyWindow.xaml.cs` | 新規 | ルーン語一覧ウィンドウコードビハインド |
| `src/RougelikeGame.Gui/KeyBindingSettings.cs` | 修正 | OpenVocabulary列挙値、Bキーバインド、表示名追加 |
| `src/RougelikeGame.Gui/MainWindow.xaml.cs` | 修正 | ShowVocabularyDialog、イベント接続、キーバインドマッピング追加 |
| `tests/RougelikeGame.Core.Tests/MagicLanguageSystemTests.cs` | 修正 | brenna→binda修正（3件）、新テスト4件追加 |
| `tests/RougelikeGame.Core.Tests/EnemyItemExpansionTests.cs` | 修正 | アイテム総数81→82更新 |
| `docs/計画書/マスター実装計画書.md` | 修正 | Ver.prt.0.19, 0.20行追加 |
