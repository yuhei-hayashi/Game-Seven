# アプリ名
七☆並☆王

# 概要
七並べと遊戯王を足したようなカードゲームになります。
1対1の対戦型カードゲームになります。

# ルール
## 勝利条件
黒もしくは赤のカードどちらかを13枚全て場に出したら勝利

## ゲームの準備
* デッキの準備  
お互いのプレイヤーはトランプの7を除くスペード 、ハート合計24枚のデッキを使います。
* 先攻後攻決定  
ランダムに先攻、後攻が決定します。
* 初期手札の配置  
お互いのプレイヤーは手札5枚でゲームをスタートします。
* マリガン  
ゲーム開始時に手札を好きな枚数入れ替えることが出来ます。

## ゲームの流れ
お互いのプレイヤーのマリガンが終了したら先攻プレイヤーよりゲームが開始します。
* ドロー  
ターンの始めにカードを1枚ドローします。
しかし、先攻1ターン目はドローを行いません。
* カードを場に出す  
七並べのように場に出ているカードと同じマークの隣り合ったカードを場に出すことが出来ます。
カードを場に出した場合、そのターンを終了し相手ターンに移行します。
また13のカードが出ている場合1のカードを出すことが出来ます。1が出ている場合の13も同様です。
* PASS  
手札に出せるカードがない場合PASSをします。
そのままの状態でターンを終了し相手ターンに移行します。

上記の流れでターンを繰り返しどちらかのプレイヤーが勝利条件を達成するまでゲームが続きます。

# 本番環境
https://unityroom.com/games/seven-29781
2人のプレイヤーがRoomに入室した段階でゲームが開始します。  
同じ端末から別タブで2つURLを開いても問題なくゲームを開始することが出来ます。

# 制作背景
学生の時から自分でゲームを作りたいと思っており今回ゲームを作成しました。  
普段から様々なゲームをプレイしており、ゲームがどのように動いているのかずっと興味を持っておりました。  
そこでゲーム開発に使われているUnityを用いてゲーム開発を勉強し、特に自分が好きなカードゲームを作成しました。

# 工夫したポイント
## 手札のカードと場のカードのプレハブの変更
手札のカードと場のカードを別のプレハブを参照することで挙動を変化させた。  
具体的には手札のカードは持つことが出来るが場のカードは持つことができない。またクリック選択においても同様である。

## 相手の手札の見た目だけ変更
相手の手札情報に関してcardIDは相手の手札の情報だが見た目だけ裏面に変更を行なった。  
またカード移動した際に元の見た目が表示されるようにmodelのiconを修正するのではなくSpriteのみ変更を行なった。  
これにより相手の手札は裏面表示となり、カードを移動させた際に元の見た目を表示させることが出来る。

# 苦労したポイント
## 同期機能
2人のプレイヤーの動きを同期させることに苦労しました。特に手札の位置に関してはプレイヤーによって配置が違うため  
ただ同期をさせただけでは上手くいかないので苦労しました。  
オブジェクトを同期させるのではなくPRCによりメソッドを同期させることで問題を解決しました。

## 遅延機能
マリガンの際にカードを選ぶ場面で他の動きをとめるのに苦労しました。  
特にマリガンが終了しても相手のマリガンを待ってからゲームを開始しなければいけないためマリガンのChangeボタンで進めることができないのは苦労しました。  
コルーチンを理解することで動きの遅延を行うことができ、両プレイヤーのマリガンが終了するまで処理を止めることに成功しました。

# 追加したい機能
## カード効果の追加
現在、場に出した時の効果が全くなく戦略性などが少ない。カードに効果を持たせることで選択肢が増えゲーム性が向上する。  
具体的にはDictionaryにActionを格納し対応するカードが場に出た時にActionを実行するようにすることで効果を持たせることができる。

## 場に出ているカード情報の取得
現在、どちらのプレイヤーがどのカードを出したのかがわからず覚える必要がある。  
どちらのプレイヤーが何を出したのかを一覧表示させる機能をつけることによりゲームがやりやすくなる。

# DEMO

## マリガン画面
![マリガン画面](https://i.gyazo.com/529c5ebda7d3890e00e41763e86d37f8.png "マリガン画面")

## ゲーム画面
![ゲーム画面](https://i.gyazo.com/2b5b915e790285defab087b163f6b74e.png "ゲーム画面")

# 開発環境
* C#
* Unity
* PUN2
* UnityRoom








