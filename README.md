# VRChat Parameter Queue

VRChat のアバター上でパラメーターのキューを実装するためのツールです。

## 概要

ParameterQueue は、VRChat Animator パラメーターを使用してキューデータ構造を実現します。複数の値を順序付けして保存し、アニメーター ステートマシンから制御できます。

## 機能

- **キューの自動生成**：Inspector ボタンで VRChat Animator パラメーターと制御ロジックを自動生成
- **Int/Float 両対応**：キューの要素型を選択可能
- **自動パラメーター管理**：値の追加・削除が自動的に処理される

## 使用方法

1. ParameterQueue コンポーネントを GameObject に追加
2. AnimatorController を割り当て
3. Inspector の「Generate Parameter Queue」ボタンをクリック
4. 以下のパラメーターが自動生成されます

## パラメーター仕様

### キューの要素

```
{parameterName}_000
{parameterName}_001
{parameterName}_002
...
{parameterName}_{n}
```

- **型**：Int または Float（Queue Type で指定）
- **役割**：キューの n 番目の要素を保持

### キュー操作パラメーター

#### `{parameterName}_AddValue`

- **型**：Int または Float（Queue Type に依存）
- **役割**：キューに追加したい値を入れる
- **使用方法**：
  - `{parameterName}_Add` が false の時に値を設定
  - true にすると値がキューに追加される

#### `{parameterName}_Add`

- **型**：Bool
- **役割**：値をキューに追加するトリガー
- **動作**：
  - `false` の時：`{parameterName}_AddValue` に値を入力可能
  - `true` に変更：`{parameterName}_AddValue` の値がキューの末尾に追加
  - 処理完了後、自動的に `false` に戻る

#### `{parameterName}_Next`

- **型**：Bool
- **役割**：キューを1段進める
- **動作**：
  - `false` の時：キューは現在の状態を保持
  - `true` に変更：要素0を削除し、全要素が1段前にシフト
  - 処理完了後、自動的に `false` に戻る

#### `{parameterName}_Count`

- **型**：Int
- **役割**：キュー内の現在の要素数を表示
- **用途**：キューが空か、満杯かの判定に使用

## Animator での操作

### キューに値を追加

1. `{parameterName}_Add` が false であることを確認（処理中でないか確認）
2. `{parameterName}_AddValue` に追加したい値を設定
3. `{parameterName}_Add` を true に変更
4. 処理が自動的に実行され、値がキューに追加される
5. `{parameterName}_Add` は自動的に false に戻る

### キューを進める

1. `{parameterName}_Next` が false であることを確認（処理中でないか確認）
2. `{parameterName}_Next` を true に設定
3. キューがシフトされ、最初の要素が削除される
4. `{parameterName}_Next` は自動的に false に戻る

### キューの状態確認

- `{parameterName}_Count` で現在の要素数を確認
- `{parameterName}_000` 〜 `{parameterName}_{Count-1}` が有効な要素

## 例（Max Queue Size = 5, Parameter Name = "Queue"）

### 初期状態

```
Queue_000: 0
Queue_001: 0
Queue_002: 0
Queue_003: 0
Queue_004: 0
Queue_Count: 0
```

### 値 10, 20, 30 を追加後

```
Queue_000: 10
Queue_001: 20
Queue_002: 30
Queue_003: 0
Queue_004: 0
Queue_Count: 3
```

### Next でキューを進めた後

```
Queue_000: 20
Queue_001: 30
Queue_002: 0
Queue_003: 0
Queue_004: 0
Queue_Count: 2
```

## 注意点

- キューサイズを超える値の追加はできません
