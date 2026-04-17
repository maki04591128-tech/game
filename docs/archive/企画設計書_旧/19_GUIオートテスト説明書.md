# GUIオートテスト説明書

本書は FlaUI.UIA3 を使用した GUIオートメーションテストの詳細仕様を記述する。

---

## 1. 概要

| 項目 | 値 |
|------|-----|
| テストフレームワーク | xUnit 2.7.0 + FlaUI.UIA3 4.0.0 |
| テストファイル | GuiAutomationTests.cs / GuiSystemVerificationTests.cs |
| テストメソッド数 | 11（オートメーション6 + システム検証5） |
| 検証項目数（Assert行） | 約230（オートメーション約108 + システム検証約121） |
| 起動方式 | デバッグマップ（固定シード） + `--skip-title` で高速起動 |
| テスト間分離 | `[Collection("GuiTests")]` で直列実行（GUI競合回避） |

### 設計方針

- **起動コスト削減**: アプリ起動は高コストなため、1メソッド内で多数の検証項目をまとめて実行する
- **責務分離**: GuiAutomationTests = UI存在チェック＋クラッシュ耐性、GuiSystemVerificationTests = 値レベル詳細検証
- **重複排除**: 両テストクラス間で検証内容が重複しないよう役割を明確に分離

---

## 2. GuiAutomationTests（6メソッド・約108検証項目）

UI要素の存在チェック＋全キーバインドのクラッシュ耐性を検証する。

| # | メソッド名 | 検証項目数 | テスト内容 |
|---|-----------|-----------|-----------|
| 1 | TitleScreen_ButtonsAndSettingsDialog | 約15項目 | タイトル画面ボタン4種存在確認、設定ダイアログ開閉、音量スライダー4種・ラベル4種確認、Escでダイアログ閉じる |
| 2 | TitleScreen_EscClosesWindow | 1項目 | タイトル画面でEscキーでアプリ終了（破壊的操作のため分離） |
| 3 | TitleScreen_NewGameFlow_CharacterCreation | 約10項目 | ニューゲーム→難易度選択→キャラクター作成画面のUI要素検証＋キャンセル動作 |
| 4 | TitleScreen_SettingsParameterChanges | 約10項目 | 設定ダイアログ内スライダー操作による値変化確認＋初期値リセット |
| 5 | MainWindow_FullIntegration | 約65項目 | デバッグマップ1回起動で全検証（下記詳細参照） |
| 6 | TitleScreen_ContinueFlow_SaveDataSelect | 約7項目 | セーブ後コンティニュー→セーブデータ選択画面検証 |

### 2.1 MainWindow_FullIntegration 詳細

デバッグマップ1回起動で以下を一括検証する最大のテストメソッド：

**初期値・表示形式検証**:
- ウィンドウタイトル・サイズ・メッセージログ初期表示（第1層、デバッグモード、WASD）
- 操作説明テキスト(KeyHelpText)存在＋内容チェック
- 領地名・地上/ダンジョン表示の値検証
- HP/MP/SP初期値フル（current==max）、形式 'X/Y'
- レベル=1、経験値形式、重量 'x.x/y.ykg'（初期非0）、通貨 'XG'、ターン制限
- 満腹度・正気度の初期値検証（数値、非0）
- 日時表示形式（'冒険暦XXXX年 ○○の月 X日 HH:MM'）・時間帯検証

**アイテム拾い・インベントリ・ドアインタラクト**:
- 地面アイテム位置へ移動→Gで拾取→Iでインベントリ確認
- ドア方向へ移動→ドア開放→Xでドア閉じ

**NPC対話・地形効果**:
- NPC位置へ移動→対話確認、水タイル移動

**各ダイアログ・キー操作**:
- ミニマップ切替（M）、ステータスバー全20要素、SkillSlotIconPanel存在確認
- ダイアログ: C/L/V/J/K/O/H/B/E/P/N、探索F、自動探索Tab

**移動・戦闘・階段・日時進行**:
- WASD/矢印/斜め、戦闘→HP確認、R射撃/T投擲、階段Shift+</>
- Space×65日付進行、スキルCD20ターン、詠唱ターン処理

**連打耐性・セーブロード・終了**:
- 15種キー×3ラウンド高速連打、F5/F9、Qキー終了
---

## 3. GuiSystemVerificationTests（5メソッド・約121検証項目）

各ゲームシステムがGUI上で正しく動作しているかを「値レベル」で検証する。

| # | メソッド名 | 検証項目数 | テスト内容 |
|---|-----------|-----------|-----------|
| 1 | SystemVerification_DebugMap_FullIntegration | 約20項目 |
| 2 | SystemVerification_LongPlay_HungerAndEndurance | 約3項目 | 70ターン待機（日付進行+階層不変）、800ターン待機（満腹度減少）、200ターン連続操作（ステータスバー正常維持） |
| 3 | SystemVerification_CombatAndStatusTransition | 約12項目 | 初期ステータス全20要素記録、HP初期フル確認、30回移動戦闘、HP減少方向検証、戦闘後ステータスバー形式維持、自動探索(Tab)→3秒→中断(Space)フロー、追加50回移動安定性 |
| 4 | SystemVerification_StatusBarConsistencyAfterActions | 約10項目 | 拾う(G)/探索(F)/射撃(R)/投擲(T)/祈り(P)/ドア閉じ(X)各操作後ステータスバー全要素形式検証、ミニマップ(M)前後不変検証、階段(Shift+>)前後変化追跡、連続アクション90回後安定性 |
| 5 | SystemVerification_NewStatusBarFormats | 約14項目 | Season/Weather/Thirst/Karma/CompanionCountの初期値フォーマット検証、初期仲間数=0、100ターン後フォーマット維持、50回移動後フォーマット維持、全20要素最終整合性 |

### 3.1 GUI接続済みシステムのカバレッジ

| システム | ステータスバー要素 | 検証種別 |
|---------|-------------------|---------|
| SeasonSystem（季節） | SeasonText | 値検証（SystemVerification） |
| WeatherSystem（天候） | WeatherText | 値検証（SystemVerification） |
| ThirstSystem（渇き） | ThirstText | 値検証（SystemVerification） |
| KarmaSystem（カルマ） | KarmaText | 値検証（SystemVerification） |
| CompanionSystem（仲間） | CompanionCountText | 値検証（SystemVerification） |
| EncyclopediaSystem（図鑑） | Yキー画面遷移 | UI存在チェック（Automation） |
| DeathLogSystem（死亡記録） | Zキー画面遷移 | UI存在チェック（Automation） |
| CompanionSystem（仲間管理） | Uキー画面遷移 | UI存在チェック（Automation） |
| SkillSlotSystem（スキルスロット） | SkillSlotIconPanel | UI存在チェック（Automation） |
---

## 4. デバッグマップによる高速化

コマンドライン引数でテスト専用モードを提供：

| 引数 | 効果 |
|---|---|
| `--skip-title` | タイトル画面をスキップし、即座にメインウィンドウを起動 |
| `--debug-map` | 32×24の手動構築テストアリーナで初期化 |
| `--race <Race>` | 種族を指定（例: `--race Elf`） |
| `--class <Class>` | 職業を指定（例: `--class Mage`） |
| `--background <Bg>` | 素性を指定（例: `--background Scholar`） |

---

## 5. 実行方法

### 5.1 GUIオートメーションテストのみ実行

```powershell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; chcp 65001
cd C:\Users\maki0\source\repos\game
dotnet test RougelikeGame.sln --filter "FullyQualifiedName~GuiAutomationTests"
```

### 5.2 GUIシステム検証テストのみ実行

```powershell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; chcp 65001
cd C:\Users\maki0\source\repos\game
dotnet test RougelikeGame.sln --filter "FullyQualifiedName~GuiSystemVerificationTests"
```

### 5.3 GUIテストを除外した実行（推奨）

```powershell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; chcp 65001
cd C:\Users\maki0\source\repos\game
dotnet test RougelikeGame.sln --filter "FullyQualifiedName!~GuiAutomationTests&FullyQualifiedName!~GuiSystemVerificationTests"
```

---

## 6. 注意事項

- GUIテストは実際にアプリケーションウィンドウを起動するため、テスト実行中はマウス/キーボード操作を避けること
- `[Collection("GuiTests")]` により全GUIテストは直列実行される（並列実行不可）
- テスト失敗時はログ出力を `ITestOutputHelper` で確認可能
- 実行時間: 全11メソッドで約5～6分（環境依存）