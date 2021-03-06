using System;

namespace Rev_AI_001
{
    public class Program
    {
        static void Main(string[] args){
            //！別のポートフォリオを作成しました。是非ご覧になってください！
            //ポリオミノパズルの解探索プログラム
            //https://github.com/YHirakawa255/SolvePolyomino
            
            
            
            //オセロAIの学習と、対戦ができるプログラムである
            //もともとUnityで開発していたものを.Net Coreで動くように変更をしたため、
            //一部の機能が未実装である（AIパラメータの保存など）

            //AIは石の配置後の盤面評価値でもって行動を決定する
            //AIのエージェント同士で対戦を行い、学習をすすめる

            //＜開発経緯など＞
            //言語とAIの勉強としてオセロのAIを作ることにした
            //強化学習に、学習アルゴリズムとして確率的勾配法と二分法を組み合わせたようなアルゴリズムを使用している。
            //最初に確率的勾配法を使用したが、オセロのスコアは離散的で、勾配が計算できない問題に直面した
            //解決策として、二分法のアイデアを元にした探索アルゴリズムを使用したが、AIの学習が思うように進まない問題が発生した
            //原因として過学習が発生していると考えられる
            //今後は遺伝的アルゴリズムで学習をしようと考えている
            //また、エージェントの対戦相手についても検討が必要そうと考えている

            Start();
            //StartはUnityオブジェクトが読み込まれた時、最初に一度だけ実行される
            while(!Flag.FFinProgram){//プログラムの終了フラグが立つまで実行される
                Update();
                //UpdateはUnity内の１フレーム毎に実行される（実質的にはMain）
            }
        }
        //KKKKKKKKKKKKKKKKKKKK
        static public AgentCls[] Agent = new AgentCls[8];//AIエージェント 及びその数
        static BoadCls Boad = new BoadCls();//盤面
        static BitBoadCls BitBoad = new BitBoadCls();//ビットボード（高速化のために導入途中）
        static string DebugString;//デバッグ情報をフレームの最後に表示するための文字列
        public static FlagCls Flag = new FlagCls();//各種フラグをまとめたクラス
        static int iB, iW, iGeneration, iIteration;//繰り返しのカウンタやポインタ
        // PointerCls Pointer = new PointerCls();//
        static OptiCls RRGameResult = new OptiCls();//Round Robin：総当り
        static public System.Random rnd = new System.Random();
        static string SLog = "";
        static System.Diagnostics.Stopwatch Timer = new System.Diagnostics.Stopwatch();//一定時間(1s)毎に表示するために導入
        const long TimeInterval = 1000;
        static SaveCls DataSave = new SaveCls();
        static PlayerUISC PlayerUI = new PlayerUISC();
        static DebugCls Debug = new DebugCls();
        //KKKKKKKKKKKKKKKKKKKK
        static void Start(){//インスタンスの初期化や学習の前処理
            Init();//インスタンスの初期化など
            // LoadAll();
            // DataSave.Load(ref Agent);
            // Boad.ConsolPrint();
            // Boad.PutCheck();
            // Boad.ConsolPrint();
            Flag.InitFlag();

            // Flag.FMode = 3;//選択スキップ
        }
        //KKKKKKKKKKKKKKKKKKKK
        static void Update(){
            bool f;
            int n;
            switch (Flag.FMode){
                //KKKKKKKKKKKKKKKKKKKK
                case 0://モード選択
                    (f, n) = PlayerUI.SelectMode();
                    if (f){
                        Flag.FMode = n + 1;//各モードに移行
                    }
                    break;
                //KKKKKKKKKKKKKKKKKKKK
                case 1://プレイヤーと対戦
                    VSGame();
                    break;
                //KKKKKKKKKKKKKKKKKKKK
                case 2://学習
                    Timer.Start();//タイマー開始
                    Optimization();
                    Timer.Reset();
                    break;
                //KKKKKKKKKKKKKKKKKKKK
                case 3://ビットボードの動作チェック
                    BitBoadGame();
                    break;
                //KKKKKKKKKKKKKKKKKKKK
                default:
                    Flag.FMode = 0;
                    break;
                    //KKKKKKKKKKKKKKKKKKKK
            }
            //  if(Flag.FPlayerBattle){//プレイヤーと対戦
            //     VSGame();//VS人間
            // }else{//強化学習
            //     Timer.Start();//タイマー開始
            //     Optimization();
            //     Timer.Reset();
            //     // DebugSLog();//行動ログ表示
            // }
        }
        //KKKKKKKKKKKKKKKKKKKK(Init)
        static void Init(){//各インスタンスの初期化
            Debug.Log($"nAgent = {Agent.Length}");
            AgentCls AItmp = new AgentCls();
            // Pointer.InitPointer();//ポインタクラスは現時点で使われていない
            // DataSave = new SaveCls();
            iGeneration = 0;//世代数初期化
            iIteration = 0;//繰り返し計算数初期化
            for(int i1 = 0; i1<Agent.Length; i1++){//エージェント初期化
                Agent[i1] = new AgentCls();
                Agent[i1].InitAgent(i1);//SetRandom含む
            }
            LoadAll();//以前の実行時のデータを読み込む
            //Unity移行から時間がなく、まだデータ読み込み、保存機能が完成していない
            Boad.InitBoad();//盤面初期化
        }
        //KKKKKKKKKKKKKKKKKKKK<<BitBoad>>
        //KKKKKKKKKKKKKKKKKKKK
        static void BitBoadGame(){//ビットボードを用いて１ゲーム行う
            BitBoad.AutoGame();
        }
        //KKKKKKKKKKKKKKKKKKKK<<OPTIMIZATION>>
        //KKKKKKKKKKKKKKKKKKKK
        static double AutoGame(ref AgentCls AI1, ref AgentCls AI2, bool fD1, bool fD2){
            //AIエージェントを引数として渡し、AI同士の対戦を行う
            //fD1, fD2は、変動ありのパラメータを使うかどうかの設定
            double e1;
            double r;
            int Select;
            // Debug.Log("AutoGame Start");//デバッグ
            // Debug.Log( AutoGameDebug(AI1.MyID, AI2.MyID, fD1, fD2) );
            //初期化
            AI1.InitAgentBoad();
            AI2.InitAgentBoad();
            Boad.InitBoad();
            //ゲームが終了するまでwhile内が続く
            while (Boad.nPutList > 0){//石を置くところがあるかぎり→探索、着手
                if (Boad.ActiveFirstPlayer){
                    (e1, Select) = Boad.CalcEBranch(ref AI1, ref fD1, 3, 0);//最大評価値を計算
                }else{
                    (e1, Select) = Boad.CalcEBranch(ref AI2, ref fD2, 3, 0);//最大評価値を計算
                }
                Boad.AutoPut(Select, ref AI1, ref AI2);//最大評価値と一致するところに着手
                if (Boad.PutCheck() == 0){
                    //石を置くところがない→パス（番交代）→置けるところを探す
                    Boad.ChangeActivePlayer();
                    Boad.PutCheck();
                }
                // Boad.ConsolPrint();
            }
            r = Boad.CalcEAbsolute();//ゲーム終了時の絶対評価値を返す
            return r;
        }
        //KKKKKKKKKKKKKKKKKKKK
        static string AutoGameDebug(int ib, int iw, bool fD1, bool fD2){//ゲーム結果の表示（デバッグ用)
            string S = "GEN : " + iGeneration + " / ITR : " + iIteration + "\n\r";
            if (fD1){
                S += "DB" + ib;
            }else{
                S += "B" + ib;
            }if (fD2){
                S += " vs DW " + iw + " E=";
            }else{
                S += " vs W" + iw + " E=";
            }
            return S;
        }
        //KKKKKKKKKKKKKKKKKKK
        static void Optimization(){//最適化
            //二分法ベースでエージェントの全パラメータを最適化する
            //エージェントの数を100まで増やして計算を行ったが、うまくエージェントの学習が行われなかった
            //狙っていた最適化処理が行われている（バグがない）ことは確認できた
            //二分法ではパラメータ変更の度に過学習となり、うまく学習が進まないと考えている
            //今後、時間的余裕をみて遺伝的アルゴリズムによる学習アルゴリズムを作成予定

            //以下、学習の概要
            //①初期評価値（RoundRobinALL:現在の評価値を計算)
            //②探索方向ベクトルの決定（OptimizationSet）（乱数で決定）
            //③探索方向ベクトルで各パラメータに摂動を与え、

            //実行順
            //①繰り返し計算前の評価値計算(FOptRoundRobinBase)
            //②評価値の登録、探索方向ベクトル生成、初期摂動p取得・反映（OptimizationSet）
            //③摂動後の評価値の計算（RoundRobin_dB_W（dW_B））
            //④繰り返し計算完了判定、次の探索点p、（OptimizationPost）
            //　完了=>⑥、未完了=>摂動pの反映、③へ
            //⑤AIの更新・保存（OptimizationUpdate）
            //　=>①
            //対戦結果のまとめをしている

            //①摂動前の評価値を計算（総当たり）
            if (Flag.FOptRoundRobinBase){
                if (RoundRobinAll()){//総当たり：完了したらtrueを返す
                    Flag.SetFOptSet();//次ステップ
                }
            }
            //②最適化初期準備（繰り返し1回目目だけ）
            if (Flag.FOptSet){
                if (OptimizationSet()){//最適化準備
                    RoundRobinReset();
                    Flag.SetFOptRoundRobinDB();//次の工程へ
                    StackSLog("Set");
                }
            }
            //③総当り（黒変動）
            if (Flag.FOptRoundRobinDB){
                if (RoundRobin_dB_W()){
                    RoundRobinReset();
                    Flag.SetFOptRoundRobinDW();
                    StackSLog("IN if RRdB");
                }
            }
            //総当り（白変動）
            if (Flag.FOptRoundRobinDW){
                if (RoundRobin_dW_B()){
                    RRGameResult.CalcSumResultDel();
                    RoundRobinReset();
                    Flag.SetFOptPost();
                    StackSLog("IN if RRdW");
                }
            }
            //④総当り後処理
            if (Flag.FOptPost){
                if (OptimizationPost()){
                    StackSLog("Post");//デバッグ 
                    if (CheckOptFin()){//すべてのエージェントの繰り返し計算が終了しているか
                        Flag.SetFOptUpdate();//⑥アップデート処理へ
                    }else{
                        Flag.SetFOptRoundRobinDB();//③次の探索点へ
                    }
                }
            }
            //⑤AIの更新、次の世代へ
            if (Flag.FOptUpdate){
                if (OptimizationUpdate()){
                    Flag.SetFOptRoundRobinBase();//=>①
                    StackSLog("Update");
                    DebugSLog();
                }
            }
            // PrintDebugString();
        }
        //KKKKKKKKKKKKKKKKKKKK(RoundRobinAll)
        static bool RoundRobinAll(){//総当り(変動なし)
            //一定時間毎に計算を中断し、結果を表示できる
            //再通過時に途中から再開できる
            //終了時にはtrueを返す
            StackSLog("RR ALL");
            for (int ib = iB; ib < Agent.Length; ib++){//再開サフィックス読み込み
                for (int iw = iW; iw < Agent.Length; iw++){
                    if (TimeIntervalCheck()){//中断処理
                        iB = ib; iW = iw;//再開時のサフィックス
                        StackSLog("RR ALL BREAK");
                        return false;//完了していないことを表す
                    }
                    if (ib == iw){//自身vs自身=>0とする
                        RRGameResult.Result[ib, iw] = 0;
                    }else{
                        RRGameResult.Result[ib, iw] = AutoGame(ref Agent[ib], ref Agent[iw], false, false);
                    }
                    SLog += "RR df (" + ib + "-" + iw + "):" + RRGameResult.Result[ib, iw] + "\n\r";
                }//iw;
                iW = 0;//再開時に、0から始まらないバグあり
            }
            iB = 0;//再開時に、0から始まらないバグあり
            StackSLog("RR ALL FIN");
            RRGameResult.CalcSumResult();//ゲーム結果を評価値にまとめる
            return true;//完了したことを表す
        }
        //KKKKKKKKKKKKKKKKKKKK(Set)
        static bool OptimizationSet(){// 最適化Start
            if (Flag.FOptSet){//繰り返しの最初だけ通過
                DebugSet();//コンソール表示の文字列格納
                RRGameResult.InitOptRPSetAndPerturbation(ref Agent);//基準評価値の
                return true;
            }
            return false;
        }
        //KKKKKKKKKKKKKKKKKKKK(RoundRobinDel)
        static void RoundRobinReset(){//再開サフィックスの初期化
            iB = 0; iW = 0;
        }
        //KKKKKKKKKKKKKKKKKKKK
        static bool RoundRobin_dB_W(){//総当り、黒側変動あり
            StackSLog("RR dB");//デバッグ
            for (int ib = iB; ib < Agent.Length; ib++){//再開サフィックス読み込み
                if (!Agent[ib].BisecManager.FFinIteration){//繰り返し計算進行中のエージェントのみ計算
                    for (int iw = iW; iw < Agent.Length; iw++){
                        if (TimeIntervalCheck()){//中断処理
                            iB = ib; iW = iw;//再開時のサフィックス
                            StackSLog("RR dB BREAK");
                            return false;//総当たり中断
                        }
                        if (ib == iw){//自身vs自身
                            RRGameResult.ResultDelBlack[ib, iw] = 0;
                        }else{
                            RRGameResult.ResultDelBlack[ib, iw] = AutoGame(ref Agent[ib], ref Agent[iw], true, false);
                        }
                        SLog += "RR dB (" + ib + "-" + iw + "):" + RRGameResult.ResultDelBlack[ib, iw] + "\n\r";
                    }//iw;
                    iW = 0;
                }
            }
            iB = 0;
            StackSLog("RR dB FIN");
            return true;//総当たり完了
        }
        //KKKKKKKKKKKKKKKKKKKK
        static bool RoundRobin_dW_B(){//総当り、白側変動あり
            StackSLog("RR dW");
            for (int iw = iW; iw < Agent.Length; iw++){//再開サフィックス読み込み
                if (!Agent[iw].BisecManager.FFinIteration){//繰り返し計算進行中のエージェントのみ計算
                    for (int ib = iB; ib < Agent.Length; ib++){//再開サフィックス読み込み
                        if (TimeIntervalCheck()){//中断処理
                            iB = ib; iW = iw;//再開時のサフィックス
                            StackSLog("RR dW BREAK");
                            return false;//総当たり中断
                        }if (ib == iw){//自身vs自身
                            RRGameResult.ResultDelWhite[ib, iw] = 0;
                        }else{
                            RRGameResult.ResultDelWhite[ib, iw] = AutoGame(ref Agent[ib], ref Agent[iw], false, true);
                        }
                        SLog += "RR dW (" + ib + "-" + iw + "):" + RRGameResult.ResultDelBlack[ib, iw] + "\n\r";
                    }//iw;
                    iB = 0;
                }
                // if(FBreak){ break;  }//中断処理
            }
            iW = 0;
            StackSLog("RR dW FIN");
            return true;//総当たり完了
        }
        //KKKKKKKKKKKKKKKKKKKK(Post)
        static bool OptimizationPost(){//後処理（総当り後）
            // string s = RRGameResult.DebugResult() + "\r\n";//総当たり結果の表示
            // s += RRGameResult.DebugResultDel();//総当たり結果の表示
            // Debug.Log(s);//総当たり結果の表示
            //
            //次の摂動、報酬値セット
            RRGameResult.NextPerturbationCalc(ref Agent);//
            //終了判定
            if (RRGameResult.FFinOptimizationAgent(ref Agent)){//終了時
                Flag.SetFOptUpdate();//アップデートへ
            }else{//繰り返し続行
                //摂動を評価に反映
                RRGameResult.SetPerturbation(ref Agent);//摂動を反映
                Flag.SetFOptRoundRobinDB();//次の繰り返しへ
            }
            return true;
        }
        //KKKKKKKKKKKKKKKKKKKK
        static bool CheckOptFin(){//Post:各エージェントの最適化が終了しているかの確認
            foreach (AgentCls a in Agent){
                if (!a.BisecManager.FFinIteration){
                    return false;
                }
            }
            return true;
        }
        //KKKKKKKKKKKKKKKKKKKK(Update)
        static bool OptimizationUpdate(){
            string s = RRGameResult.DebugResult() + "\r\n";//総当たり結果の表示
            s += RRGameResult.DebugResultDel();//総当たり結果の表示
            Debug.Log(s);//総当たり結果の表示
            iGeneration++;
            RRGameResult.UpdateAI(ref Agent);//AI更新
            SaveAll();
            return true;
        }
        //KKKKKKKKKKKKKKKKKKKK
        static void SaveAll(){//Update:世代ごとに最適化結果を保存する
         //これらのデータは次の実行時に読み込まれる
         // DataSave.Save(ref Agent);//AIの保存
         // DataSave.Save(ref RRGameResult);//総当たり計算途中の保存
         // DataSave.Save(ref Flag);//計算制御フラグの保存
         // DataSave.SaveToHDD();//HDDに書き込み
        }
        //KKKKKKKKKKKKKKKKKKKK
        static void LoadAll(){
            // DataSave.Load(ref Agent);//AIの読み込み  
            // DataSave.Load(ref RRGameResult);//総当たり計算途中の読み込み
            // DataSave.Load(ref Flag);//計算制御フラグの読み込み
        }
        //KKKKKKKKKKKKKKKKKKKK<<VS>>
        //KKKKKKKKKKKKKKKKKKKK
        static void VSGame(){
            bool Selected = false;
            if (Flag.FSelectBW)
            {//白黒選択状態
                if (PlayerUI.SelectBW())
                {//選択が終了したときtrue
                    Flag.FPlayerBlack = PlayerUI.FPlayerBlack;
                    Flag.FSelectBW = false;//白黒選択終了
                    Flag.FSelectAI = true;//AIの選択
                }
            }
            else if (Flag.FSelectAI)
            {//AI選択状態
                (Selected, Flag.BattleAI) = PlayerUI.SelectAI(ref Agent);
                if (Selected)
                {
                    Flag.FSelectAI = false;//選択終了=>ゲーム開始
                    Flag.FFinGame = false;//ゲーム終了フラグ
                }
            }
            else
            {//ゲーム対戦中
                VSGameSub();
            }
        }
        //KKKKKKKKKKKKKKKKKKKK
        static void VSGameSub(){
            //ここに入る前に、初期化が必要かもしれない
            double e1, r;
            int Select;
            bool FChange, FDecide;
            bool FD = false;//固定
            //着手
            if (Boad.ActiveFirstPlayer == Flag.FPlayerBlack){
                //人の手番のとき
                Boad.ConsolPrint($"Selecting {PlayerUI.GetSelect()}", -1);
                (FChange, FDecide, Select) = PlayerUI.SelectPut(Boad.nPutList - 1);//配置場所選択
                if (FChange){//配置ポインタ変更=>表示
                    Boad.ConsolPrint($"Selecting {PlayerUI.GetSelect()}", Select);
                }
                if (FDecide){//配置場所決定=>配置して表示
                    Boad.AutoPut(Select, ref Agent[Flag.BattleAI]);//着手
                    Boad.ConsolPrint($"put", Select);//表示
                }
            }
            else
            {
                //AIの手番のとき
                (e1, Select) = Boad.CalcEBranch(ref Agent[Flag.BattleAI], ref FD, 3, 0);
                Boad.AutoPut(Select, ref Agent[Flag.BattleAI]);
                FDecide = true;//手番交代処理へ
            }
            //手番交代処理
            if (FDecide)
            {
                if (Boad.PutCheck() == 0)
                {//配置場所を確認 / ==0: 置ける場所がないとき
                    Boad.ChangeActivePlayer();//白黒手番交代
                    if (Boad.PutCheck() == 0)
                    {//==0: 白も黒も置ける場所がない=>ゲーム終了
                        Flag.FFinGame = true;
                    }
                }
            }
            //ゲーム終了時はここが処理される
            if (Flag.FFinGame)
            {
                Debug.Log("Game Fin");
                Flag.FFinProgram = true;
            }

        }
        //KKKKKKKKKKKKKKKKKKKK<<DEBUG>>
        //KKKKKKKKKKKKKKKKKKKK
        static void DebugSLog(){//SLogに蓄積されたデバッグ情報を出力する
            string s = SLog;
            SLog = "";
            Debug.Log(s);
        }
        //KKKKKKKKKKKKKKKKKKKK
        static bool TimeIntervalCheck(){//時間経過を計測する
            //計算量が多いためにコンソール表示までの時間がかかり、
            //各繰り返しでの状況がわからなかったため、デバッグの意味で導入
            if (Timer.ElapsedMilliseconds >= TimeInterval){
                // Debug.Log(Timer.ElapsedMilliseconds);
                return true;
            }
            return false;
        }
        //KKKKKKKKKKKKKKKKKKKK
        static void StackSLog(string s, bool f = true){//引数をSLogに蓄える
            SLog += s;
            if (f) { SLog += "\n\r"; }
        }
        //KKKKKKKKKKKKKKKKKKKK
        static void DebugSet(){//世代と繰り返し計算回数の表示
            DebugString = "GEN : " + iGeneration + ", ITR : " + iIteration + "\n\r" + "\n\r";
        }
        //KKKKKKKKKKKKKKKKKKKK
        // void PrintDebugString(){
        //     Debug.Log(DebugString);
        //     DebugString = "";
        // }
        //KKKKKKKKKKKKKKKKKKKK
        // void RoundRobinDebug(){
        //     string s = "Agent n=" + Agent.Length;
        //     for(int iB=0; iB<Agent.Length; iB++){
        //         s+="\n\r"+"No."+iB;
        //         for(int iW=0; iW<Agent.Length; iW++){
        //             s+=" | "+RRGameResult.Result[iB,iW];
        //         }
        //     }
        //     Debug.Log(s);
        // }
        //KKKKKKKKKKKKKKKKKKKK
        // void LoadAll(){//最適化の途中ロード
        //     DataSave.Load(ref Agent);
        //     DataSave.Load(ref RRGameResult);
        // }
        //KKKKKKKKKKKKKKKKKKKK

        //学習は前世代に対して強くなるように進む

        //リバーシは一般的に後攻が有利である。
        //同じエージェント同士の対戦は学習に考慮しない(評価値=0)とした
        //一定時間毎に経過を表示させるため、フラグ管理で処理を中断できるようにした

        //勝ったときに高得点を目指すのではなく、負けない学習が進むようにしたかった
        //リバーシは一般的に後攻が有利であることを考慮すると、
        //先行が得意（期待値より高得点）な学習が進んでしまう可能性がある
    }
}

