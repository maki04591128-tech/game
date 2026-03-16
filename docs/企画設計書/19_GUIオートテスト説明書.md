# GUIオートテスト説明書

## 概要

GUIオートテストは、FlaUI（UI Automation）を使ってWPFアプリケーションを実際に起動し、ユーザー操作をシミュレートして画面要素の表示・動作を検証する自動テストスイートである。以下の2つのテストクラスで構成される。

### テストクラス一覧

| クラス名 | xUnitテストメソッド数 | 内部検証項目数 | 目的 | ファイル |
|---|---|---|---|---|
| `GuiAutomationTests` | 4メソッド | 37項目 | UI要素の存在・基本操作の検証 | `GuiAutomationTests.cs` |
| `GuiSystemVerificationTests` | 2メソッド | 37項目 | Ver.prt.0.1で実装した各システムのGUI上での動作検証 | `GuiSystemVerificationTests.cs` |
| **合計** | **6メソッド** | **74項目** | | |

> **注記**: アプリケーション起動コスト削減のため、1つのテストメソッド内に複数の検証項目をまとめて実行する構成です。xUnit上のテスト数は6件ですが、検証網羅性は従来と同等の74項目です。

**テストフレームワーク**: xUnit 2.7.0 + FlaUI.UIA3 4.0.0  
**テストディレクトリ**: `tests/RougelikeGame.Gui.Tests/`

---

## デバッグマップによる高速化

### 課題

通常起動では以下の処理が発生し、テスト1件あたり数秒〜十数秒のオーバーヘッドが生じる：

- タイトル画面（`TitleWindow`）の表示・ユーザー操作待ち
- 60×30マップ（BSP部屋生成、敵・罠・アイテム配置、廊下接続）の生成

### 解決策

コマンドライン引数でテスト専用モードを提供：

| 引数 | 効果 |
|---|---|
| `--skip-title` | タイトル画面をスキップし、即座にメインウィンドウを起動 |
| `--debug-map` | 32×24の手動構築テストアリーナで初期化（通常60×30の約1/3のサイズ） |

### デバッグマップの仕様

| パラメータ | 値 | 通常マップとの比較 |
|---|---|---|
| マップ幅 | 32 | 60（約1/2） |
| マップ高さ | 24 | 30（約4/5） |
| メインルーム | (2,2)〜(29,21)の大部屋（28×20） | BSP生成（6+階層数部屋） |
| プレイヤー初期位置 | (16, 12) — 部屋中央付近 | ランダム部屋内 |
| 敵数 | 5体（Slime, Goblin, Skeleton, Orc, GiantSpider） | 密度ベース（多数） |
| 罠 | 2個（隠し罠・可視罠） | 0.005+階層×0.001 |
| 地面アイテム | 13個（ポーション3、食料3、巻物3、装備3、アクセサリ1） | 密度ベース（多数） |
| 特殊地形 | ドア2、階段2、祭壇、泉、宝箱、水3、柱2 | ランダム配置 |
| デバッグタイル | 4個（敵スポーンE、AI切替A、日数進行D、NPC対話N） | なし |

### 関連コード変更

| ファイル | 変更内容 |
|---|---|
| `GameController.cs` | `InitializeDebug()` / `GenerateDebugFloor()` メソッド追加 |
| `App.xaml.cs` | `--skip-title` / `--debug-map` 引数パース、`StartMainWindow` に `debugMap` パラメータ追加 |
| `MainWindow.xaml.cs` | `_debugMap` フィールド追加、`Window_Loaded` で分岐 |

---

## 起動方式

テストでは2種類の起動方式を使い分ける：

### 1. 通常起動（`LaunchAndGetTitleWindow`）

```
Application.Launch(exePath)  →  タイトル画面を取得
```

**用途**: タイトル画面・設定画面のテスト（7検証項目）

### 2. デバッグマップ起動（`LaunchWithDebugMap`）

```
Application.Launch(exePath, "--skip-title --debug-map")  →  メインウィンドウを即取得
```

**用途**: メインウィンドウ・キー操作・メッセージログのテスト（30検証項目）

---

## テスト一覧

> **注記**: 以下の検証項目は、実際のコードでは起動コスト削減のため少数のテストメソッドにまとめられています。
> - カテゴリ1〜2 → `TitleScreen_ButtonsAndSettingsDialog` メソッド（+ `TitleScreen_EscClosesWindow` / `TitleScreen_NewGameFlow`）
> - カテゴリ3〜5 → `MainWindow_FullIntegration` メソッド
>
> 以下の一覧は**検証項目レベル**の記述です。

### カテゴリ1: タイトル画面テスト（4検証項目）

タイトル画面の各ボタン表示、キーボード操作、画面遷移を検証する。通常起動を使用。

| テスト名 | テスト内容 | 検証観点 |
|---|---|---|
| `Title_HasAllButtons` | タイトル画面に4つのボタン（NewGameButton, ContinueButton, SettingsButton, QuitButton）が全て存在するか確認 | UI要素の存在確認。AutomationIdによる要素探索が正常に動作すること |
| `Title_EscClosesWindow` | Escキーを押してタイトル画面が閉じ、アプリケーションが終了するか確認 | キーボードショートカットによるウィンドウ終了処理。`_app.HasExited == true` を検証 |
| `Title_SettingsOpensDialog` | 設定ボタン（SettingsButton）をクリックして設定ダイアログがモーダルウィンドウとして開くか確認 | ボタンのInvokeパターン、モーダルダイアログの表示。`window.ModalWindows.Length > 0` を検証 |
| `Title_NewGameNavigatesToMainWindow` | ニューゲームボタンにフォーカスしてEnterキーを押し、メインウィンドウ（タイトル「ローグライクゲーム」）に遷移するか確認 | 画面遷移フロー。ボタンのフォーカス＋キーボード操作、メインウィンドウのタイトル検証。プロセスが正常終了した場合も許容 |

### カテゴリ2: 設定画面テスト（3検証項目）

タイトル画面から設定ダイアログを開き、各スライダーやラベルの存在・動作を検証する。通常起動を使用。

| テスト名 | テスト内容 | 検証観点 |
|---|---|---|
| `Settings_HasVolumeSliders` | 設定ダイアログに4つのスライダー（MasterVolumeSlider, BgmVolumeSlider, SeVolumeSlider, FontSizeSlider）が全て存在するか確認 | 設定UIの完全性。各スライダーのAutomationId検索 |
| `Settings_HasVolumeLabels` | 設定ダイアログの4つの値表示ラベル（MasterVolumeText, BgmVolumeText, SeVolumeText, FontSizeText）が存在し、それぞれ数値を含むか確認 | ラベルとスライダーのバインディング。正規表現 `\d+` で数値表示を検証 |
| `Settings_EscClosesDialog` | 設定ダイアログを開いた状態でEscキーを押し、ダイアログが閉じるか確認 | モーダルダイアログのキーボード操作による終了。閉じた後 `window.ModalWindows` が空であることを検証 |

### カテゴリ3: メインウィンドウUI要素テスト（16検証項目）

デバッグマップで高速起動し、ステータスバーの各要素やゲーム画面の構成要素を検証する。

| テスト名 | テスト内容 | 検証観点 |
|---|---|---|
| `MainWindow_HasCorrectTitle` | ウィンドウタイトルが「ローグライクゲーム」であるか確認 | `Window.Title` プロパティの正確性 |
| `MainWindow_StatusBar_ShowsFloor` | FloorText要素が存在し、「第1層」を含むか確認 | 階層表示の初期値。デバッグマップは常に第1層 |
| `MainWindow_StatusBar_ShowsDate` | DateText要素が存在し、日付形式（「歴」「年」「の月」「日」「:」を含む）であるか確認 | カレンダーシステムの表示形式。「冒険歴1024年 緑風の月 15日 08:00」等 |
| `MainWindow_StatusBar_ShowsTimePeriod` | TimePeriodText要素が存在し、有効な時間帯名（明け方/朝/午前/昼/午後/夕方/夜/深夜のいずれか）を表示しているか確認 | 時間帯表示システムの動作 |
| `MainWindow_StatusBar_ShowsLevel` | LevelText要素が存在し、数値（レベル値）を含むか確認 | レベル表示。XAMLの「Lv:」ラベルは別TextBlockなので、LevelTextは数値のみ |
| `MainWindow_StatusBar_ShowsExp` | ExpText要素が存在し、「/」（現在値/最大値形式）を含むか確認 | 経験値バーの表示形式（例: "0/100"） |
| `MainWindow_StatusBar_ShowsHpMpSp` | HpText, MpText, SpText要素が全て存在し、各々「/」（現在値/最大値形式）を含むか確認 | HP・MP・SPの3つのステータス値が正しい形式で表示されること |
| `MainWindow_StatusBar_ShowsHunger` | HungerText要素が存在し、空白でない文字列が表示されているか確認 | 満腹度表示。値は数値のみ（例: "100"） |
| `MainWindow_StatusBar_ShowsSanity` | SanityText要素が存在し、数値を含むか確認 | 正気度表示。値は数値のみ（例: "100"） |
| `MainWindow_StatusBar_ShowsGold` | GoldText要素が存在し、空白でない文字列が表示されているか確認 | 所持金表示（例: "0G"） |
| `MainWindow_StatusBar_ShowsWeight` | WeightText要素が存在し、「/」（現在値/最大値形式）を含むか確認 | 重量表示形式（例: "3.5/50.0kg"） |
| `MainWindow_StatusBar_ShowsTurnLimit` | TurnLimitText要素が存在し、空白でない文字列が表示されているか確認 | ターン制限表示（例: "残り365日" または "制限なし"） |
| `MainWindow_HasGameCanvas` | ウィンドウの幅が800px以上、高さが500px以上であるか確認 | WPF Canvasは UIオートメーションツリーに直接公開されないため、ウィンドウサイズで間接確認 |
| `MainWindow_HasMinimap` | Mキー（ミニマップ切替）を2回押してクラッシュしないか確認 | Canvas/Border要素はUIオートメーション非対応のため、Mキー操作で間接的にミニマップの存在を確認 |
| `MainWindow_HasMessageLog` | MessageLog要素が存在するか確認 | メッセージログ表示領域のAutomationId検索 |
| `MainWindow_WindowSize_IsPositive` | ウィンドウの幅・高さが0より大きいか確認 | ウィンドウが正常に描画されていることの基本確認 |

### カテゴリ4: キー操作テスト（12検証項目）

デバッグマップで高速起動し、各種キーボード入力でクラッシュや異常終了が発生しないことを検証する。

| テスト名 | テスト内容 | 検証観点 |
|---|---|---|
| `Key_Wait_AdvancesDate` | Space（待機）キーを65回連続押下し、日時表示が変化するか確認 | ターン処理の動作確認。60ターン=1分なので65回で必ず分の値が変わる。初期値と比較して`Assert.NotEqual` |
| `Key_WASD_Movement_NoCrash` | W, S, A, Dキーを順に押下してクラッシュしないか確認 | WASD移動入力のハンドリング。壁にぶつかっても例外が発生しないこと |
| `Key_ArrowKeys_Movement_NoCrash` | ↑, ↓, ←, →キーを順に押下してクラッシュしないか確認 | 矢印キー移動入力のハンドリング |
| `Key_Inventory_OpensDialog` | Iキーを押下してインベントリ画面が開くか確認（空の場合はダイアログなしも許容） | インベントリ表示処理。モーダルダイアログが表示された場合は閉じる |
| `Key_Status_OpensDialog` | Cキーを押下してステータス画面が開くか確認 | キャラクターステータス表示処理 |
| `Key_MessageLog_OpensDialog` | Lキーを押下してメッセージログ画面が開くか確認 | メッセージログ表示処理 |
| `Key_Pickup_NoCrash` | Gキー（アイテム拾い）を押下してクラッシュしないか確認 | 足元にアイテムがない場合でもエラーにならないこと |
| `Key_Search_NoCrash` | Fキー（探索）を押下してクラッシュしないか確認 | 周囲に特殊タイルがない場合でもエラーにならないこと |
| `Key_AutoExplore_NoCrash` | Tabキー（自動探索開始）→Spaceキー（キャンセル）を押下してクラッシュしないか確認 | 自動探索の開始・停止処理 |
| `Key_Save_NoCrash` | F5キー（セーブ）を押下してクラッシュしないか確認 | セーブ処理のエラーハンドリング |
| `Key_RapidInput_NoCrash` | 11種類のキー（W/A/S/D/Space/G/F/↑↓←→）を3周×各30ms間隔で高速連打し、クラッシュしないか確認 | 高速連打耐性テスト。入力キューの溢れや競合状態が発生しないこと |
| `Key_DiagonalMovement_NoCrash` | Home, PageUp, End, PageDownキー（斜め移動）を押下してクラッシュしないか確認 | 斜め移動入力のハンドリング |

### カテゴリ5: メッセージログ統合テスト（2検証項目）

デバッグマップで高速起動し、メッセージログの初期表示と更新を検証する。

| テスト名 | テスト内容 | 検証観点 |
|---|---|---|
| `MessageLog_ShowsInitialMessages` | MessageLog要素が「デバッグモード」を含むか確認 | `InitializeDebug()` が出力する「【デバッグモード】テストマップに入った！」メッセージの表示 |
| `MessageLog_UpdatesAfterWait` | Space（待機）を5回押下した後にクラッシュしないか確認 | メッセージログが複数ターン経過後も正常に更新されること |

---

## テスト実行方法

### GUIオートテスト全件実行（GuiAutomationTests + GuiSystemVerificationTests）

```bash
dotnet test RougelikeGame.sln --filter "FullyQualifiedName~GuiAutomationTests|FullyQualifiedName~GuiSystemVerificationTests"
```

### UI要素テストのみ実行（GuiAutomationTests）

```bash
dotnet test RougelikeGame.sln --filter "FullyQualifiedName~GuiAutomationTests"
```

### システム検証テストのみ実行（GuiSystemVerificationTests）

```bash
dotnet test RougelikeGame.sln --filter "FullyQualifiedName~GuiSystemVerificationTests"
```

### GUIオートテストを除外して実行

```bash
dotnet test RougelikeGame.sln --filter "FullyQualifiedName!~GuiAutomationTests&FullyQualifiedName!~GuiSystemVerificationTests"
```

### 全テスト実行

```bash
dotnet test RougelikeGame.sln
```

---

## テスト技術メモ

### FlaUI の要素探索

WPFの `x:Name` 属性は自動的に `AutomationId` としてUIオートメーションツリーに公開される。テストでは `FindFirstDescendant(cf => cf.ByAutomationId(...))` で要素を検索する。

ただし、以下の制約がある：

- **Canvas要素**: WPFの `Canvas` はUIオートメーションツリーに公開されないことがある（子要素が動的描画のため）。`GameCanvas` と `MinimapCanvas` はこの制約に該当するため、間接的な検証方法を採用している
- **タイムアウト**: 要素探索は最大5秒のリトライ（250ms間隔）で実行。起動直後の描画完了を待つため

### ボタン操作の注意点

`Button.Invoke()` はWPFのコマンドバインディングを直接呼び出すが、タイトル画面の `NewGameButton` のように `ShowDialog()` フローを中断する場合がある。そのため `Title_NewGameNavigatesToMainWindow` では `Focus()` + `Enter` キー送信を使用している。

### テストの独立性

各テストは独自のアプリケーションインスタンスを起動・終了する（`IDisposable` で `_app.Close()` / `_app.Dispose()`）。テスト間の状態共有はない。テストコレクション `[Collection("GuiTests")]` により直列実行を保証している。

---

## テスト結果サマリ

### GuiAutomationTests（4メソッド / 37検証項目）

| メソッド名 | 検証項目数 | 起動方式 | 平均所要時間 |
|---|---|---|---|
| TitleScreen_ButtonsAndSettingsDialog | 約15項目（タイトル画面+設定画面） | 通常起動 | 約5〜10秒 |
| TitleScreen_EscClosesWindow | 1項目 | 通常起動 | 約2〜3秒 |
| TitleScreen_NewGameFlow | 1項目 | 通常起動 | 約3〜5秒 |
| MainWindow_FullIntegration | 約20項目（UI要素+キー操作+連打耐性） | デバッグマップ | 約30〜60秒 |
| **小計** | **37検証項目** | — | **約1〜2分** |

### GuiSystemVerificationTests（2メソッド / 37検証項目）

| メソッド名 | 検証項目数 | 起動方式 | 平均所要時間 |
|---|---|---|---|
| SystemVerification_DebugMap_FullIntegration | 約30項目（Phase 1〜6全システム検証） | デバッグマップ | 約1〜2分 |
| SystemVerification_LongPlay_HungerAndEndurance | 約7項目（長時間プレイ検証） | デバッグマップ | 約1〜2分 |
| **小計** | **37検証項目** | — | **約2〜4分** |

### 合計

| | xUnitテストメソッド数 | 検証項目数 | 合計所要時間 |
|---|---|---|---|
| **GUIオートテスト全体** | **6メソッド** | **74検証項目** | **約3〜6分** |
