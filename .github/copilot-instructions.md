# Copilot Instructions

## プロジェクト ガイドライン
- 修正を行うたびに全てのテスト（dotnet test RougelikeGame.sln）を実行して確認すること。GUIオートメーションテストを除外する場合は --filter "FullyQualifiedName!~GuiAutomationTests" を使用する。
- ターミナルでコマンドを実行する前に、毎回 `[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; chcp 65001` を先頭に付けてUTF-8エンコーディングを設定し、文字化けを防止すること。
- チャット上での説明・思考過程は必ず日本語で記述すること。英語を混ぜない。
- システムの実装が完了するたびに、docs/03_実装計画書.md の該当タスクのステータスを更新すること。実装計画書は包括的な1ファイルではなく、各バージョン段階ごとに分割して作成する（例: 実装計画書Ver.prt.0.1.md、実装計画書Ver.prt.0.2.md、実装計画書Ver.α.md等）。確認の効率を上げるため。
- 「ドキュメントブラッシュアップ」と指示された場合、全ドキュメントで情報が不足もしくは間違っている箇所がないかを確認して追記および修正を行い、必要なドキュメント（GUI関係、マップシステム関係、BGM/SE関係、演出関係、デバッグ関係、設定資料（世界観、ストーリー）、ドキュメント類の概要）を作成すること。
- 実装計画書の各Phase（Phase 4、Phase 5、Phase 6等）の全タスクが完了した時点で、自動的にドキュメントブラッシュアップを実施すること。具体的には、完了したPhaseで実装・変更された内容を全設計書・計画書の実装状況セクションに反映し、テスト数・クラス一覧・ディレクトリ構造等の数値や記述を最新化し、実装計画書にブラッシュアップ記録セクションを追加すること。ユーザーからの明示的な指示がなくても、Phase完了を検知したら自動で行う。
- GUI関連のシステムを新規実装または変更した場合は、必ず以下のGUIオートテスト対応を同時に行うこと:
  1. **テスト追加**: 新規キーバインド・ダイアログ・ステータスバー要素・画面遷移等に対応するテストを追加する。
     - キー操作のクラッシュ耐性テスト → `GuiAutomationTests.MainWindow_FullIntegration` に追加（該当セクションに挿入、連打耐性の `rapidKeys` 配列にも追加）
     - ステータスバー新要素 → `GuiAutomationTests.MainWindow_FullIntegration` の `statusBarIds` 配列に追加
     - 値レベルの詳細検証（初期値・形式・変化確認） → `GuiSystemVerificationTests.SystemVerification_DebugMap_FullIntegration` に追加
     - タイトル画面の変更 → `GuiAutomationTests.TitleScreen_ButtonsAndSettingsDialog` を更新
  2. **ブラッシュアップ**: テスト追加後、両GUIテストファイルのヘッダーコメント（テスト構成・カバレッジ一覧）を最新化し、GuiAutomationTests（UI存在チェック＋クラッシュ耐性）と GuiSystemVerificationTests（値レベル詳細検証）の間で重複がないか確認・整理する。
  3. **テスト実行**: GUIオートテストを含む全テストを実行して合格を確認する。
- GUI実装時はMainWindow.xaml.csへのGUI反映も必ず同時に行うこと（キーバインド追加、ステータスバー更新、ダイアログ接続等）。