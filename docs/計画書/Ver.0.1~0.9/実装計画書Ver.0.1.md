# 実装計画書 Ver.0.1〜0.9（テストローンチ）

**目標**: デバッグ、遊びやすさ改善、Steam対応、正式リリース準備
**状態**: 🔧 実装中

---

## 1. 概要

Ver.0.1〜0.9 はテストローンチフェーズ。
Phase 6 の残りタスク（Steam対応、最終ビルド）を実装し、
総合的なデバッグ・バランス調整・遊びやすさ改善を行い Ver.1.0（正式リリース）に備える。

---

## 2. Steam対応・リリース準備

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 6.5 | Steam対応（Steamworks統合） | ✅ | プラットフォーム抽象化レイヤー実装（IPlatformService/IPlatformAchievementService/IPlatformCloudSaveService/IPlatformStatsService）。LocalPlatformService（ローカルフォールバック）＋SteamPlatformService（スタブ）＋PlatformManager（自動選択・実績連携）。全25実績のSteamAPIマッピング＋10統計項目定義。テスト12件追加 |
| 6.6 | 最終ビルド・リリースノート作成 | ✅ | リリースノートVer.1.0テンプレート作成。主要機能/品質保証/動作環境/変更履歴を記載 |

---

## 3. テスト・デバッグ

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| T.1 | 統合テスト | ✅ | AccessibilitySystem×GameSettings統合・SaveData×Difficulty統合・Tutorial×Help統合・DifficultySettings全レベル検証・GameSettings極値クランプ・ContextHelpSystem内容検証・TutorialSystem全ステップ完了検証。テスト10件追加 |
| T.2 | エッジケース対応 | ✅ | セーブデータ自動バックアップ・破損時復旧・値バリデーション（SaveData.Validate/SaveManager.TryLoadBackup）。負値クランプ・nullコレクション防御・HP/MP/満腹度/渇き/正気度範囲チェック。テスト8件追加 |
| T.3 | メモリリーク検出 | ✅ | ResourceTrackerシステム新規実装（リソース追跡・リーク検出・ピークメモリ監視）。GC回収可能性テスト・大量リスト生成テスト。テスト10件追加 |
| T.4 | パフォーマンステスト | ✅ | DungeonMap生成（80x50/200x200）・DungeonGenerator標準生成・5階層連続生成・100体敵生成・SaveDataシリアライズ/デシリアライズ・大量インベントリ(500件)・Validate1000回ベンチマーク。テスト9件追加 |
| T.5 | クロスバージョンセーブ互換 | ✅ | バージョン1ロード・新フィールド欠落時デフォルト値・未知フィールド無視・nullコレクションValidate初期化・同バージョンラウンドトリップ・PlayerData全フィールド保持・全難易度レベル保持・空コレクション保持・破損JSON検出・未来バージョン検出。テスト10件追加 |

---

## 4. 遊びやすさ改善

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| U.1 | チュートリアル強化 | ✅ | チュートリアルトリガー18種をGameControllerの各イベントに接続（GameStart/FirstEnemySight/FirstItemPickup/FirstStairs/FirstDeath/FirstLevelUp/FirstEquipChange/FirstPotionUse/FirstCrafting/FirstSpellCast/FirstNpcTalk/FirstShopVisit/FirstTempleVisit/FirstGuildVisit/FirstBossEncounter/ReachFloor5/ReachFloor10/FirstMagicWord） |
| U.2 | ヒント表示システム | ✅ | ContextHelpSystemを7→26トピックに拡充。移動4/戦闘5/インベントリ4/魔法2/クラフト2/サバイバル5/上級4のヘルプトピック。キーバインド検索・コンテキスト検索対応 |
| U.3 | ゲームオーバー画面改善 | ✅ | 死因詳細テキスト表示、統計情報（レベル/種族/職業/撃破数/最深階層/所持金/アイテム数/正気度/ターン数）。GameOverStatisticsレコード、GetGameOverStatistics()メソッド追加。TotalEnemiesDefeated/DeepestFloorReachedをセーブ/ロード対応 |
| U.4 | 難易度バランス最終調整 | ✅ | 全5難易度（Easy/Normal/Hard/Nightmare/Ironman）の倍率検証テスト追加。難易度間の順序関係検証（Easy<Normal<Hard<Nightmare）。Normal基準値1.0検証。Ironman永久死亡＋救出0回検証。表示名・説明テスト。テスト13件追加 |
| U.5 | アクセシビリティ | ✅ | GameSettingsに色覚モード(ColorBlindMode)/ハイコントラスト/ゲーム速度/スクリーンリーダー/大きなポインタ追加。AccessibilitySystem.ApplyFromGameSettingsで統合。GameController.ApplyAccessibilitySettings()接続。設定のValidate・Clone・JSON永続化対応。テスト23件追加 |

---

## 5. コミュニティ・リリース

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| C.1 | コミュニティテスト実施 | ⬜ | テスター募集、フィードバック収集 |
| C.2 | フィードバック反映 | ⬜ | 報告された不具合・要望の対応 |
| C.3 | Steamストアページ作成 | ⬜ | スクリーンショット、PV、説明文、タグ |
| C.4 | Ver.1.0 最終確認 | ⬜ | 全コンテンツ・全システムの最終チェック |

---

## 6. テスト実績

| バージョン | テスト数 | 合格 | 不合格 | 備考 |
|-----------|---------|------|--------|------|
| Ver.0.1 | 139 | 139 | 0 | TestLaunchVer01Tests（Tutorial 20件 + ContextHelp 18件 + SaveData 4件 + Accessibility 23件 + EdgeCase 8件 + DifficultyBalance 13件 + Integration 10件 + using 2件 + ResourceTracker 10件 + Performance 9件 + SaveCompat 10件 + Steam/Platform 12件） |
| Core.Tests全体 | 7058 | 7058 | 0 | GUIテスト除外 |

---

## 7. ブラッシュアップ記録

| 日付 | 対象 | 内容 |
|------|------|------|
| 2026-04-15 | T.1-T.5, U.1-U.5, 6.5完了 | テスト・遊びやすさ・Steam対応セクション完了に伴うドキュメントブラッシュアップ。00_ドキュメント概要のVer.0.1ステータス更新、17_デバッグ・テスト設計書のテスト数7,058件に最新化、マスター実装計画書のVer.0.1説明更新、リリースノートVer.1.0テンプレート作成 |

---

> **Ver.1.0（正式リリース）**: 上記全タスク完了後にSteam正式リリース
