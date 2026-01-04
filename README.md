# PE 2026

Unity 6000.3.2f1 を使用した3DアクションRPGプロジェクトです。VRMキャラクターを使用し、JSONベースのマップ生成システムとPE（Persona）スタイルの戦闘システムを実装しています。

## プロジェクト概要

このプロジェクトは、以下の主要機能を備えています：

- **マップ生成システム**: JSONファイルから動的にマップを生成
- **ポータルシステム**: マップ間の移動機能
- **プレイヤーコントローラー**: VRMキャラクターを使用した移動・戦闘システム
- **ATB戦闘システム**: ゲージが溜まったら構えモードに入り、敵を攻撃
- **敵システム**: HP管理とダメージ処理
- **UI管理**: ATBゲージの表示

## 必要な環境

- **Unity**: 6000.3.2f1 以降
- **必要なパッケージ**:
  - Cinemachine
  - Input System
  - UniTask (Cysharp.Threading.Tasks)
  - VRM (VRMキャラクター用)

## プロジェクト構造

```
Assets/
├── Character/          # VRMキャラクター（aya.vrm）
├── Scripts/           # スクリプトファイル
│   ├── MapManager.cs      # マップ管理システム
│   ├── MapGenerator.cs    # マップ生成システム
│   ├── MapPortal.cs       # ポータル機能
│   ├── PortalData.cs      # ポータルデータ定義
│   ├── PlayerController.cs # プレイヤー制御
│   ├── Enemy.cs           # 敵システム
│   └── UIManager.cs       # UI管理
├── Resources/
│   ├── Maps/          # マップJSONファイル
│   │   └── Stage01/   # ステージ1のマップデータ
│   └── Prefabs/       # プレハブ（床、壁、ドアなど）
└── Scenes/            # Unityシーンファイル
```

## 主要機能

### マップ生成システム

JSONファイルからマップを動的に生成します。マップデータは以下の形式で定義されます：

```json
{
  "mapId": "Stage01_Entrance",
  "tileSize": 2.5,
  "layout": [
    "WWWWWW",
    "W....W",
    "WP...1",
    "W....W",
    "WWWWWW"
  ],
  "portals": [
    {
      "triggerChar": "1",
      "targetMapId": "Stage01_Corridor",
      "targetSpawnId": 1,
      "doorPrefabName": "Prefabs/Door02"
    }
  ],
  "floorPrefabName": "Prefabs/Floor02",
  "wallPrefabName": "Prefabs/Wall02"
}
```

#### マップレイアウト記号

- `W`: 壁（Wall）
- `.`: 床（Floor）
- `P`: プレイヤースポーン地点（0番）
- `0-9`: ポータル地点（数字がスポーンID）

### プレイヤーコントローラー

#### 移動システム

- **通常移動**: WASDキーで移動
- **歩行/走行**: Shiftキーで切り替え（押していると歩行、離すと走行）
- **カメラ相対移動**: カメラの向きを基準に移動

#### 戦闘システム（PEスタイル）

1. **ATBゲージ**: 移動中に自動的にゲージが溜まる（デフォルト3秒）
2. **構えモード**: ゲージが最大になったらマウス左クリックで構えモードに入る
   - 一番近い敵を自動的にターゲット
   - 銃が表示される
   - 攻撃範囲が表示される
3. **攻撃**: 構えモード中にマウス左クリックで発砲
4. **キャンセル**: 構えモード中にマウス右クリックでキャンセル

### ポータルシステム

マップ間を移動するためのポータル機能です。プレイヤーがポータルのトリガーエリアに入ると、指定されたマップの指定されたスポーン地点に移動します。

### 敵システム

- HP管理（デフォルト3）
- ダメージ処理
- 死亡時の処理

## セットアップ手順

1. **Unityプロジェクトを開く**
   - Unity Hubからプロジェクトを開く
   - Unity 6000.3.2f1 以降を使用

2. **必要なパッケージのインストール**
   - Package Managerから以下をインストール：
     - Cinemachine
     - Input System
     - UniTask

3. **シーンの設定**
   - シーンに以下を配置：
     - `MapManager` オブジェクト（MapManager.cs をアタッチ）
     - `MapGenerator` オブジェクト（MapGenerator.cs をアタッチ）
     - カメラ（CinemachineCamera を設定）
     - UI Canvas（UIManager.cs をアタッチ）

4. **プレイヤーの設定**
   - VRMキャラクターをシーンに配置
   - `PlayerController.cs` をアタッチ
   - CharacterController コンポーネントを追加
   - Input Actions ファイルを設定
   - Animator を設定（上半身レイヤー付き）

5. **マップデータの準備**
   - `Assets/Resources/Maps/` にJSONファイルを配置
   - MapManager の Inspector でマップファイルを登録

## 使用方法

### マップの作成

1. `Assets/Resources/Maps/` に新しいJSONファイルを作成
2. マップレイアウトを記号で記述
3. ポータル情報を設定
4. MapManager の Inspector でマップファイルを登録

### プレイヤーの操作

- **移動**: WASDキー
- **歩行/走行切り替え**: Shiftキー
- **構えモード**: ATBゲージ最大時にマウス左クリック
- **攻撃**: 構えモード中にマウス左クリック
- **キャンセル**: 構えモード中にマウス右クリック

## スクリプト説明

### MapManager.cs

マップの管理とロードを担当するシングルトンクラスです。

- `LoadMap(string mapId, int spawnNumber)`: 指定されたマップをロードし、指定されたスポーン地点にプレイヤーを配置

### MapGenerator.cs

JSONデータからマップを生成するクラスです。

- `GenerateMapFromJson(string json, int spawnNumber)`: JSON文字列からマップを生成

### PlayerController.cs

プレイヤーの移動と戦闘を制御するクラスです。

- `GetChargeRatio()`: ATBゲージの現在の比率を取得（0.0～1.0）
- `IsBattleReady()`: ATBゲージが最大かどうかを取得

### MapPortal.cs

ポータルのトリガー処理を担当するクラスです。プレイヤーが接触すると、MapManagerにマップ移動を依頼します。

### Enemy.cs

敵のHP管理とダメージ処理を担当するクラスです。

- `TakeDamage(int damage)`: ダメージを受ける処理

### UIManager.cs

UI要素（ATBゲージなど）の表示を管理するクラスです。

## カスタマイズ

### プレハブの設定

マップごとに異なるプレハブを使用できます。JSONファイルで以下のプロパティを指定：

- `floorPrefabName`: 床のプレハブ（Resourcesフォルダからの相対パス）
- `wallPrefabName`: 壁のプレハブ
- `doorPrefabName`: ドアのプレハブ（ポータルごとに設定可能）

### 戦闘パラメータの調整

PlayerController.cs の Inspector で以下を調整可能：

- `walkSpeed`: 歩行速度
- `runSpeed`: 走行速度
- `chargeTime`: ATBゲージが溜まるまでの時間
- `attackRange`: 攻撃可能距離

## ライセンス

このプロジェクトのライセンス情報については、プロジェクトオーナーにご確認ください。

## 開発者向け情報

### 未実装項目

以下の機能は現在未実装です：

#### マップシステム関連
- マップ移動時のフェード処理とカメラ即時移動
- マップのタイルサイズと壁の高さの設定分け
- マップデータ、'W'を使わない仕様にする

#### 敵・戦闘システム関連
- 雑魚敵
- ボス
- 戦闘処理
  - 自分のHP
- ゲームオーバー表示
- クリア表示
- 射撃モーション改善
- 攻撃範囲表示改善
- 攻撃SE実装（射撃時）
- 射撃対象がない場合の挙動

#### その他
- アイテム管理

### 今後の拡張予定

- より複雑な敵AI
- スキルシステム
- セーブ/ロード機能

### 既知の問題

現在、特に報告されている問題はありません。問題を発見した場合は、Issueとして報告してください。

