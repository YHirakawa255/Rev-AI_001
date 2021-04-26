using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

public class AgentCls : DirCls{
    public int BisecState = 0;//二分法の状態管理
    const int Black = 1;
    const int White = -1;
    public double[] LearningRate = new double[128];
    public int MyID;
    public int nPutHist;
    public int[] Input = new int[64];
    public double cA1, cB1, cC1;
    public double[,,] A = new double[5,64,64];
    public double[,,] dA = new double[5,64,64];
    public double[,,] DA = new double[5,64,64];
    public double[,] B = new double[5,64];
    public double[,] dB = new double[5,64];
    public double[,] DB = new double[5,64];
    public double[] C = new double[64];
    public double[] dC = new double[64];
    public double[] DC = new double[64];
    public double pDef, pE;//探索幅、探索幅の基準値（？）
    public double pNow, RNow;//現在探索値
    public BisecManageClass BisecManager = new BisecManageClass();
    double Strength = 0;
    DebugCls Debug = new DebugCls();
    //KKKKKKKKKKKKKKKKKKKK(Init)
    public void InitAgent(int id){//エージェントの初期化、ランダム生成（今後、最適化中断データを読み込めるように）
        MyID = id;
        BisecManager.MyID = MyID;
        cA1 = 1;//摂動の大きさやバランスを取るパラメータ
        cB1 = 1;//摂動の大きさやバランスを取るパラメータ
        cC1 = 1;//摂動の大きさやバランスを取るパラメータ
        pDef = 1;//摂動のデフォルト（今後、最適化の結果によって変動することを検討）
        pE = 1;//摂動のデフォルト初期値
        SetRandom();
        // FFinIteration = false;//初期化が未達成
        // BisecManager.InitBisec(pE);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void SetRandom(){
        int i1, i2, Layer;
        for(Layer=0; Layer<5; Layer++){
            for(i1=0; i1<64; i1++){
                for(i2=0; i2<64; i2++){
                    A[Layer, i1, i2] = Rev_AI_001.Program.rnd.NextDouble() - 0.5;//-0.5~0.5
                }
            }
        }
        for(Layer=0; Layer<5; Layer++){
            for(i1=0; i1<64; i1++){
                B[Layer, i1] = Rev_AI_001.Program.rnd.NextDouble() - 0.5;//-0.5~0.5
            }
        }
        for(i1=0; i1<64; i1++){
            C[i1] = Rev_AI_001.Program.rnd.NextDouble() - 0.5;//-0.5~0.5
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void InitAgentBoad(){//AIの内部盤面をリセットする
        for(int i1=0; i1<Input.Length; i1++){
            Input[i1] = 0;
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK(Game)
    public double E(bool fD){//盤面評価値を返す!!!layer<1
        //fD=trueのとき、摂動後の評価値を返す
        double[] buf = new double[64];
        double e;
        string s = "";
        
        if(fD){//摂動あり
            e = ESubPertur(ref s);
        }else{//摂動なし
            e = ESubBase(ref s);
        }
        // s += "e = " + e;
        // Debug.Log(s);
        return e;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double ESubBase(ref string s){//摂動なしの評価値
        int Layer, i1, i2;
        double e = 0;
        double[] bufA = new double[64];
        double[] bufB = new double[64];
        // s = "FALSE\n\r";
        // Debug.Log(MyID+"E:False");
        //初層
        Layer = 0;
        for(i1=0; i1<64; i1++){
            bufA[i1] = 0;
            for(i2=0; i2<64; i2++){
                bufA[i1] += Input[i2] * A[Layer,i1,i2];
                // s += i2+":"+Input[i2]+"*"+A[i2,i1]+"+"+B[i1]+" = "+buf[i1]+"\n\r";
            }
            bufA[i1] += B[Layer,i1];
            // bufB[i1] = tanhExp( bufA[i1] );
            bufB[i1] = ReLU( bufA[i1] );
        }
        //2層目以降
        for(Layer=1; Layer<5; Layer++){
            for(i1=0; i1<64; i1++){
                bufA[i1] = 0;
                for(i2=0; i2<64; i2++){
                    bufA[i1] += bufB[i2] * A[Layer,i1,i2];
                    // s += i2+":"+Input[i2]+"*"+A[i2,i1]+"+"+B[i1]+" = "+buf[i1]+"\n\r";
                }
                bufA[i1] += B[Layer,i1];
                // bufB[i1] = tanhExp( bufA[i1] );
                bufB[i1] = ReLU( bufA[i1] );
            }
        }
        //最終層
        for(i1=0; i1<64; i1++){
            e += C[i1] * bufB[i1];
            // s += "e += "+C[i1]+"*"+tanhExp(ref buf[i1])+"\n\r";
        }
        return e;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double ESubPertur(ref string s){//摂動ありの評価値
        int Layer, i1, i2;
        double e = 0;
        double[] bufA = new double[64];
        double[] bufB = new double[64];
        // s = "True\n\r";
        // Debug.Log(MyID+"E:True");
        //初層
        Layer = 0;
        for(i1=0; i1<64; i1++){
            bufA[i1] = 0;
            for(i2=0; i2<64; i2++){
                bufA[i1] += Input[i2] * DA[Layer,i1,i2];
                // s += i2+":"+Input[i2]+"*"+A[i2,i1]+"+"+B[i1]+" = "+buf[i1]+"\n\r";
            }
            bufA[i1] += DB[Layer,i1];
            // bufB[i1] = tanhExp( bufA[i1] );
            bufB[i1] = ReLU( bufA[i1] );
        }
        //2層目以降
        for(Layer=1; Layer<5; Layer++){
            for(i1=0; i1<64; i1++){
                bufA[i1] = 0;
                for(i2=0; i2<64; i2++){
                    bufA[i1] += bufB[i2] * DA[Layer,i1,i2];
                    // s += i2+":"+Input[i2]+"*"+A[i2,i1]+"+"+B[i1]+" = "+buf[i1]+"\n\r";
                }
                bufA[i1] += DB[Layer,i1];
                // bufB[i1] = tanhExp( bufA[i1] );
                bufB[i1] = ReLU( bufA[i1] );
            }
        }
        //最終層
        for(i1=0; i1<64; i1++){
            e += DC[i1] * bufB[i1];
            // s += "e += "+C[i1]+"*"+tanhExp(ref buf[i1])+"\n\r";
        }
        return e;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double ReLU(double aug){
        if(aug>0){
            return aug;
        }else{
            return 0;
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    double tanhExp(double aug){//
        return Math. Tanh(Math. Exp(aug) );
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void SetBoad(ref int[,] aug){//盤面状態をAI内部盤面にセット
        int tmp;
        for(int ix = 0; ix<8; ix++){
            for(int iy = 0; iy<8; iy++){
                tmp = ix + iy * 8;
                Input[tmp] = aug[ix,iy];
            }
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public void Put(ref PutOperationCls aug){//AIの内部盤面に着手する
        int p = aug.x + 8 * aug.y;
        nPutHist = aug.nPutHist;
        Input[p] = aug.Color;
        for(int id=0; id<8; id++){
            for(int i1=1; i1<=aug.reverseDirection[id]; i1++){
                p = aug.x + i1 * DirX[id] + 8 * ( aug.y + i1 * DirY[id] );
                Input[p] = aug.Color;
            }
        }
        // Input[64] = 0;//aug. nPutList;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void PutInverse(ref PutOperationCls aug){//AI内部盤面を一手分戻す
        int p = aug.x + 8 * aug.y;
        nPutHist = aug.nPutHist - 1;
        Input[p] = 0;
        for(int id=0; id<8; id++){
            for(int i1=1; i1<=aug.reverseDirection[id]; i1++){
                p = aug.x + i1 * DirX[id] + 8 * ( aug.y + i1 * DirY[id] );
                Input[p] = - aug.Color;
            }
        }
        Input[64] = 0;//aug. nPutList;
    }
    //KKKKKKKKKKKKKKKKKKKK(Set)
    public void OptPRSet(ref double rOpt){//(in OptimizationSet)//各世代の最初に実行
        // FFinIteration = false;
        // ROpt = rOpt;//基準値セット
        pE = 1;//デバッグ用：将来的には削除
        pNow = BisecManager.InitBisec(ref pE, ref rOpt);//基準Rセット、初期摂動の取得、初期化
        Strength = rOpt;//強さの登録
        // Debug.Log(MyID+"AI:InitOpt:P:"+pNow);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void SetGradientVector(){//(Set>OptSet)探索方向ベクトルを生成する
        // Debug.Log("AI:SetGradientVector" + MyID);//デバッグ用
        //dA
        for(int Layer = 0; Layer<5; Layer++){
            for(int i1=0; i1<64; i1++){
                for(int i2=0; i2<64; i2++){
                    dA[Layer,i1,i2] = cA1 * (2 * Rev_AI_001.Program.rnd.NextDouble() - 1 );//d:探索方向ベクトル
                    // DA[i1,i2] = A[i1,i2] + dA[i1,i2];
                }
            }
        }
        //dB
        for(int Layer=0; Layer<5; Layer++){
            for(int i1=0; i1<64; i1++){
                dB[Layer,i1] = cB1 * ( 2 * Rev_AI_001.Program.rnd.NextDouble() - 1 );//-0.5~0.5
                // DB[i1] = B[i1] + dB[i1];
            }
        }
        //dC
        for(int i1=0; i1<64; i1++){
            dC[i1] = cC1 * ( 2 * Rev_AI_001.Program.rnd.NextDouble() - 1 );//-0.5~0.5
            // DC[i1] = C[i1] + dC[i1];
        }
    }
    //KKKKKKKKKKKKKKKKKKKK(Set/Post)
    public void SetPerturbation(double p){//(Set/Post)//次の探索点を取得し、評価関数に反映する
        // pNow = BisecManager.GetNextP();//次の探索点を取得=>これは分ける
        // Debug.Log(MyID+" SetP:"+p);
        double tmp;
        for(int Layer=0; Layer<5; Layer++){
            for(int i1=0; i1<64; i1++){
                for(int i2=0; i2<64; i2++){
                    tmp = Math.Max( -5, (Math.Min( 5, A[Layer,i1,i2] + p * dA[Layer,i1,i2] ) ) );
                    DA[Layer,i1,i2] = tmp;
                    // DA[Layer,i1,i2] = A[Layer,i1,i2] + p * dA[Layer,i1,i2];
                    // DA[i1,i2] = dA[i1,i2];
                }
            }
        }
        for(int Layer=0; Layer<5; Layer++){
            for(int i1=0; i1<64; i1++){
                tmp = Math.Max( -1000, (Math.Min( 1000, B[Layer,i1] + p * dB[Layer,i1] ) ) );
                DB[Layer,i1] = tmp;
                // DB[Layer,i1] = B[Layer,i1] + p * dB[Layer,i1];
                // DB[i1] = dB[i1];
            }
        }
        for(int i1=0; i1<64; i1++){
            tmp = Math.Max( -5, (Math.Min( 5, C[i1] + p * dC[i1] ) ) );
            DC[i1] = tmp;
            // DC[i1] = C[i1] + p * dC[i1];
            // DC[i1] = dC[i1];
        }
        // DebugParam();
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void SetPerturbation(){//オーバーロード
        SetPerturbation(pNow);
    }
    //KKKKKKKKKKKKKKKKKKKK//(Post)
    public void GetNextP(double rnow){//摂動評価値から次の探索点を獲得する!!!!!!!!!!絶対必要
        RNow = rnow;
        pNow = BisecManager.GetNextP(ref RNow);
    }
    //KKKKKKKKKKKKKKKKKKKK(Update)
    public double UpdateParam(){//(in OptimizationUpdate)パラメータ更新
        double SumDeltaParam = 0;//パラメータの総和（変更がなされているかの確認用）
        // BisecManager.StackLogPrint();
        if(BisecManager.RBest >= BisecManager.ROpt){//終了判定と更新判定
            // Debug.Log(MyID+"FindBetterParameters : "+BisecManager.pBest);//より良い点を見つけました
            SumDeltaParam = UpdateParamSub(BisecManager.pBest);//摂動pでパラメータを更新（総和を返す）
        }
        return SumDeltaParam;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double UpdateParamSub(double pBest){//パラメータの総和を求める（変更のデバッグ用）
        //摂動の再セット
        SetPerturbation(pBest);
        //摂動時の値をデフォルトにコピー
        double SumDeltaParam = 0;//変更量の総和（デバッグ）
        // A = DA;
        for(int Layer=0; Layer<5; Layer++){
            for(int i1=0; i1<64; i1++){
                for(int i2=0; i2<64; i2++){
                    SumDeltaParam += Math.Abs( A[Layer,i1,i2]-DA[Layer,i1,i2] );
                    A[Layer,i1,i2] = DA[Layer,i1,i2];
                }
            }
        }
        // B = DB;
        for(int Layer=0; Layer<5; Layer++){
            for(int i1=0; i1<64; i1++){
                SumDeltaParam += Math.Abs( B[Layer,i1]-DB[Layer,i1] );
                B[Layer,i1] = DB[Layer,i1];
            }
        }
        // C = DC;
        for(int i1=0; i1<64; i1++){
            SumDeltaParam += Math.Abs( C[i1]-DC[i1] );
            C[i1] = DC[i1];
        }
        Debug.Log("AI Update"+MyID+" Set p="+pBest+":"+SumDeltaParam);
        return SumDeltaParam;
    }
    //KKKKKKKKKKKKKKKKKKKK
    void UpdateLearningRate(){//学習率を取得//!!!未完成
        for(int i1=LearningRate.Length-1; i1 >= 1; i1--){
            LearningRate[i1] = LearningRate[i1-1];
        }
        // LearningRate[0] = CalcLearningRate();
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void DebugParam(){//AIのパラメータをコンソール表示する（デバッグ用）
        double sum = 0;
        double sumd = 0;
        string s = "";
        for(int Layer=0; Layer<5; Layer++){
            for(int i1 = 0; i1<65; i1++){
                for(int i2 = 0; i2<64; i2++){
                    sum += A[Layer,i1,i2];
                    s += A[Layer,i1,i2] + "\n\r";
                }
            }
            for(int i1=0; i1<64; i1++){
                sum += B[Layer,i1];
                s += B[Layer,i1] + "\n\r";
            }
        }
        for(int i1=0; i1<64; i1++){
            sum += C[i1];
            s += C[i1] + "\n\r";
        }
        s += "\n\r";
        for(int Layer=0; Layer<5; Layer++){
            for(int i1 = 0; i1<65; i1++){
                for(int i2 = 0; i2<64; i2++){
                    sumd += DA[Layer,i1,i2];
                    s += DA[Layer,i1,i2] + "\n\r";
                }
            }
            for(int i1=0; i1<64; i1++){
                sumd += DB[Layer,i1];
                s += DB[Layer,i1] + "\n\r";
            }
        }
        for(int i1=0; i1<64; i1++){
            sumd += DC[i1];
            s += DC[i1] + "\n\r";
        }
        Debug.Log(s);
        Debug.Log(MyID+"Param\n\r"+sum+"\n\r"+sumd);
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public double GetStrength(){
        return Strength;
    }
    //KKKKKKKKKKKKKKKKKKKK
    // public void InitAgent(){//エージェント初期化（読み込みなど）
    //     FFinIteration = false;
    //     InitAgent(MyID);
    //     BisecManager.MyID = MyID;
    //     pDef = 1;
    //     pE = 1;
    // }
    //KKKKKKKKKKKKKKKKKKKK
    // public void SetSameHist(ref BoadCls Boad, int targetHist){//配置履歴を確認して、現在の盤面と同じ状態に盤面を変更する（現在、使われていない）
    //     if(nPutHist>targetHist){
    //         while(nPutHist>targetHist){
    //             PutInverse(ref Boad.putHist[nPutHist]);
    //         }
    //     }else if(nPutHist<targetHist){
    //         while(nPutHist<targetHist){
    //             Put(ref Boad.putHist[nPutHist+1]);
    //         }
    //     }
    // }
    //KKKKKKKKKKKKKKKKKKKK
    // public void SetInput(ref BoadCls Boad){
    //     int p;
    //     for(int iy = 0; iy < 8; iy++){
    //         for(int ix = 0; ix < 8; ix++){
    //             p = iy*8+ix;
    //             if(Boad. Stone[ix,iy] == Black){
    //                 Input[p] = 1;
    //             }else if(Boad. Stone[ix,iy] == White){
    //                 Input[p] = -1;
    //             }else{
    //                 Input[p] = 0;
    //             }
    //         }
    //     }
    //     Input[64] = 0;//Boad. nPutList;
    // }
    //KKKKKKKKKKKKKKKKKKKK
    // public void SetROpt(ref double rOpt){//基準評価値をセット
    //     ROpt = rOpt;
    //     Debug.Log("R="+ROpt);
    //     BisecManager.ROpt = ROpt;
    //     BisecManager.InitBisec(pE);//二分法コントローラに基準値を渡す
    // }
    //KKKKKKKKKKKKKKKKKKKK
}
//KKKKKKKKKKKKKKKKKKKK





//KKKKKKKKKKKKKKKKKKKK
public class BisecManageClass{
    public bool FFinIteration;//繰り返し計算終了
    public int MyID;
    // int NMaxIteration = 10;//設定値
    int Iteration;
    double pDif, pE;//探索点幅 / デフォルト値
    double pPos, RPos;//プラス側評価値
    double pMed, RMed;//中央点評価値
    double pNeg, RNeg;//マイナス側評価値
    public double pOpt, ROpt;//基準評価値
    double pPre, RPre;//前回探索点
    public double pBest, RBest;//現状最適値
    double pNow;
    public double RNow;//現在探索点
    string SLog = "";//探索の履歴
    bool statePN;//true:プラス側探索とマイナス側探索が決まっていない状態
    bool statePos;//true:プラス側探索、fasle:ネガティブ側探索
    bool stateEx;//ture:極値探索モード
    bool stateEx1;//true:探索の片側だけ（現在値の反対側も調べる必要があることを示す）
    bool stateEq, stateEqSub;
    DebugCls Debug = new DebugCls();
    //KKKKKKKKKKKKKKKKKKKK
    public double InitBisec(ref double pe, ref double rOpt){//(Set)探索開始時、世代のはじめに実行される
        //この関数自体は最初の探索点を返す
        pE = pe;   pDif = pE;//初期摂動セット
        ROpt = rOpt;//基準値セット
        RMed = ROpt;   pMed = 0;//探索の初期中央値
        RBest = RMed;   pBest = pMed;
        pNow = pDif;
        StackLog("Init Bisec " + MyID + " : 0 : " + ROpt+">>P:"+pNow, true);//探索の経過を記録
        FFinIteration = false;//探索未完了
        statePN = true;//探索方向を示す//最初は極値がどちら側にあるのかわからない
        statePos = true;//プラス側を探索したことの記録
        stateEx = false;//極値を見つけられたか
        stateEx1 = false;//ture:極値の両側を探索する場合の片側だけしか調べられていない
        stateEq = false;//探索点の両側が同じ値だったことの一回目
        stateEqSub = false;//特殊状態
        // Iteration = NMaxIteration;繰り返し計算の上限
        return pDif;//最初の探索点
    }
    //KKKKKKKKKKKKKKKKKKKK
    public double GetNextP(ref double rNow){//次の探索点を返す
        //①両側を調べて探索方向を決定する
        //・プラスか、マイナスがより良い評価値=>②
        //・両側とも低い評価値=>③
        //②極値を超えるまで摂動を等間隔で与え続ける
        //・新しい探索点の値が同じか低い値=>③極値発見
        //③二分法で極値を探す
        //・探索範囲を狭めて、両側の評価値が一致=>収束条件達成、探索終了

        //RMed, pMedはInitBisecでセット済み
        // SLog+="GN";
        RNow = rNow;//現在地セット
        pPre = pNow;
        //確認用(0番以外は学習が進まない)
        // if(MyID>0){
        //     pNow = 0;
        //     pBest = 0;
        //     rNow = 0;
        //     RBest = 0;
        //     FFinIteration = true;
        //     StackLogPrint();
        // }
        if(MyID==3){
            rNow = RMed;
            RPos = RMed;
            RNeg = RMed;
        }
        // pNow = pNow_;
        if(stateEx){//③極値周辺が見つかった時（山を超えた時)(二分法モード)
            // SLog+="-Ex";
            // return GetNextPBisec();
            pNow = GetNextPBisec();
        }else{//②極値周辺がまだ見つかっていない時(等差分探索モード)
            if(statePN){//ポジディブ、ネガティブ判別
                // SLog+="-PN";
                // return GetNextPStart();//最初に通過する
                pNow = GetNextPStart();
            }else{//①=>②statePN=false: 探索方向が決まった場合
                // SLog+="-SP";
                // return GetNextPSpace();
                pNow = GetNextPSpace();
            }//statePN
        }//stateEx
        // Debug.Log(MyID+"NextP:"+pNow);
        if(MyID==3){
            pNow = 0;
            FFinIteration = true;
            // Debug.Log(MyID+" is = 0");
            // pNow += pE;
            // }else{
            //     // Debug.Log(MyID+" is =! 0");
            //     pNow = 0;
        }
        // pNow = 0;
        return pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double GetNextPStart(){//①最初に実行、探索方向を決定する
        if(statePos){//ポジティブ側探索後(最初に通過する)
            if(RMed<RNow){//ポジティブ側の新しい点の評価値が良い時
                statePN = false;//探索方向決定=>等間隔探索
                statePos = true;//ポジディブ側
                RNeg = RMed;    pNeg = pMed;
                pRSetMed();
                // RBest = RNow;   pBest = pNow;
                pNow = pMed + pDif;//探索点をポジティブ側に等間隔で移動
                StackLogPR("Start(Pos)>>ToPos",true);
            }else{//ポジディブ側の評価値が低い時=>ネガティブ側を調べる
                statePos = false;//ネガティブ側
                pRSetPos();
                pNow = - pDif;//ネガティブ側の点
                StackLogPR("Start(Pos)>>Check Neg",true);
            }
        }else{//ネガティブ側探索後
            if(RMed<RNow){//ネガティブ側の新しい点の評価値が良い時
                statePN = false;//探索方向決定
                statePos = false;//ネガティブ側
                RPos = RMed;    pPos = pMed;
                pRSetMed();
                // RBest = RNow;   pBest = pNow;
                StackLogPR("Start(Neg)>>To Neg",true);
                pNow = pMed - pDif;
            }else{//基準位置(p=0)周辺が最適値の時(基準位置の両側の値が良くなかった)
                statePN = false;//探索方向決定(p=0を中央値とする)
                stateEx = true;//極値周辺モード(二分法)
                stateEx1 = true;//探索点の片側だけ
                pRSetNeg();
                pDif /= 2;//探索幅を半分に
                if(RNeg <= RPos){
                    pNow = UsualNextPoint(1);
                }else{
                    pNow = UsualNextPoint(-1);
                }
                StackLogPR("Start(Neg)>>Ex Opti",true);
            }
        }//statePos
        return pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double GetNextPSpace(){//②探索方向が決まった後、山が現れるまで等間隔で探索してゆく
        if(RMed<RNow){//新しい探索点が良い=>等間隔探索が続く
            if(statePos){//ポジティブ探索
                StackLogPR("To Pos>>",true);
                RNeg = RMed;    pNeg = pMed;
                pRSetMed();
                pNow = UsualNextPoint(1);
                return pNow;
            }else{//ネガティブ探索
                StackLogPR("To Neg>>",true);
                RPos = RMed;   pPos = pMed;
                pRSetMed();
                pNow = UsualNextPoint(-1);
                return pNow;
            }
        }else{//前回の探索点が良い(山を発見)=>二分法へ
            stateEx = true; //極値周辺モード(二分法)に移行
            stateEx1 = true;//まずどちらか一方を調べる
            pDif /= 2;//探索幅を狭める
            if(statePos){//最新点を対応する側にセット
                pRSetPos();
            }else{   
                pRSetNeg();
            }
            //どちら側から先に調べるか決定する
            if(RNeg<=RPos){
                pNow = UsualNextPoint(1);
                StackLogPR("Find Ex(Pos)",true);
            }else{
                pNow = UsualNextPoint(-1);
                StackLogPR("Find Ex(Neg)",true);
            }
            return pNow;
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    double GetNextPBisec(){//③山を超えた後、二分法ベースで値を探索する
        if(stateEqSub){//特殊状態
            // SLog+="SB";
            return GetNextPBisecSub();
        }else if(RMed<RNow){//より良い点が見つかった
            GetNextPBisecFindBetter();
        }else{//中央点がよいか、同じ値
            // SLog+="SM";
            if(statePos){//新しい探索点のセット
                pRSetPos();
            }else{
                pRSetNeg();
            }

            if(stateEx1){//まだ片側だけの探索=>もう片側
                stateEx1 = false;//残りの片側を探索する
                // SLog+="E1";
                if(statePos){//ポジティブ側から確かめた時
                    // pRSetPos();
                    pNow = UsualNextPoint(-1);
                    StackLogPR("(Pos)=>(Neg)",true);
                }else{//ネガティブ側から確かめた時
                    // pRSetNeg();
                    pNow = UsualNextPoint(1);
                    StackLogPR("(Neg)=>(Pos)",true);
                }
            }else{//stateEx1//両側調べた=>探索幅を狭めるor終了判定
                // SLog+="BsT";
                // pDif /=2;//探索幅を狭める
                if( (RNeg==RMed) && (RMed == RPos) ){//3点が同じ評価値
                    if(stateEq){//前回の探索幅でも同じ評価値だった=>極値到達、繰り返し完了
                        RBest = RMed;   pBest = pMed;//探索の最適値を更新
                        FFinIteration = true;//探索終了
                        StackLogPR("Eq 3P 2nd=>Fin");
                        StackLogPrint();
                        return pBest;//この値は特に使われない
                    }else{//3点の評価値が同じ、一回目
                        stateEq = true;//前回幅の探索の結果、3点が同じ評価値であったことを示す
                        stateEx1 = true;//片側探索
                        pDif /= 2;//探索幅を狭めて再度検証
                        pNow = UsualNextPoint(1);//プラス側から探索
                        StackLogPR("Eq 3P 1st=>(Pos)");
                    }
                }else if(RMed==RPos){//2点が同じ値=>
                    stateEqSub = true;//2点が同じ時のルーチンへ
                    statePos = true;
                    pDif /= 2;
                    RNeg = RMed;    pNeg = pMed;//探索範囲がプラス側に狭まる
                    pMed = (pPos-pNeg)/2;//中央点の算出
                    pNow = pMed;//中央点を調べる
                    StackLogPR("(Neg)<(Med)=(Pos):=>(Med)");
                }else if(RNeg==RMed){//2点が同じ値=>特殊状態
                    stateEqSub = true;//2点が同じ時のルーチンへ
                    statePos = false;
                    pDif /= 2;
                    RPos = RMed;    pPos = pMed;//探索範囲がマイナス側に狭まる
                    pMed = (pPos-pNeg)/2;//中央点の算出
                    pNow = pMed;//中央点を調べる
                    StackLogPR("(Neg)=(Med)>(Pos):=>(Med)");
                }else{//中央が最良=>範囲を狭める
                    stateEq = false;//不一致
                    stateEx1 = true;//片側の探索
                    pDif /= 2;
                    if(RNeg<=RPos){
                        pNow = UsualNextPoint(1);
                        StackLogPR("Best(Med)=>(Pos)");
                    }else{
                        pNow = UsualNextPoint(-1);
                        StackLogPR("Best(Med)=>(Neg)");
                    }
                }
            }
        }
        return pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
    void GetNextPBisecFindBetter(){//より良い点を見つけたとき
        stateEq = false;
        // pDif /=2;//探索幅を半分に
        if(statePos){//それはプラス側
            pDif = NarrowHalf(1);
            StackLog("Better P(Pos)=>",false);
        }else{//それはマイナス側
            pDif = NarrowHalf(-1);
            StackLog("Better P(Neg)=>",false);
        }
        if(RNeg<=RPos){//狭めた範囲のよりよい評価値（プラス）の側から探索する
            pNow = UsualNextPoint(1);
            StackLogPR("(Pos)",true);
        }else{//狭めた範囲のより良い評価値（マイナス）の側から探索する
            pNow = UsualNextPoint(-1);
            StackLogPR("(Neg)",true);
        }
        stateEx1 = true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double GetNextPBisecSub(){//(3.5)分割した　極値探索の終了付近で現れる中央点を探索するパターン
        pDif /=2;//必ず探索範囲を狭めることになる
        stateEqSub = false;//必ずここには戻ってこない
        stateEx1 = true;//必ず片側の探索をすることになる
        pRSetMed();//今の評価値を中央にセットする
        if( (RNeg==RNow) && (RNow==RPos) ){//3点が同じ評価値=>範囲を狭める
            stateEq = true;//3点同じ評価が1回あった
            pNow = UsualNextPoint(1);//プラス側から（固定）
            StackLogPR("(Sub)Eq 3P 1st=>(Pos)");
        }else if(RPos < RNow){//中央が良い評価=>範囲を狭める
            stateEq = false;//3点同じ評価ではないので
            pNow = UsualNextPoint(1);//プラス側から（固定）
            StackLogPR("Best(Med)=>(Pos)");
        }else{//中央点の評価値が低い場合(極値が2つ見つかる場合)(多分レアケース)
            //暫定的に、初期探索点から遠い側に探索範囲を狭める処理にする
            if(0<=pMed){
                // NarrowHalf(1);
                RMed = RPos;    pMed = pPos;//遠い側の暫定極値を中央にする
                pNow = UsualNextPoint(1);
                StackLogPR("(Med)<(Neg)=(Pos):=>(Pos=>Med)(Pos)");
            }else{
                RMed = RNeg;    pMed = pNeg;//遠い側の暫定極値を中央にする
                pNow = UsualNextPoint(-1);
                StackLogPR("(Med)<(Neg)=(Pos):=>(Neg=>Med)(Pos)");
            }
        }//
        return pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public double GetPNow(){//現在の探索点を返す（探索点決定のアルゴリズムは走らない）
        return pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void SetRNow(ref double rNow){//計算された評価値を獲得する
        RNow = rNow;
        // Debug.Log(MyID+"BSM:RNow="+RNow);
    }
    //KKKKKKKKKKKKKKKKKKKK(Debug)
    void StackLog(string s, bool r = true){//デバッグ用の文字列を格納する
        SLog += s;
        Debug.Log($"\n\r AI: {MyID} Stack Log : \n\r{SLog}" );
        if(r){
            SLog += "\n\r";
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    void StackLogPR(string s, bool r = true){//デバッグ用の文字列を格納する（その時の探索点と評価値付きで）
        s = $"({RNeg})({RMed})({RPos}) : {s} >> p={pNow}";
        // s += " : "+pPre+" : "+RNow+" >> "+pNow;
        // Debug.Log(MyID+"StackLogPR:"+s);
        StackLog(s, r);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void StackLogPrint(){//保持しておいたデバッグ情報を出力し、保持データをリセットする
        Debug.Log(SLog);
        SLog = "";
    }
    //KKKKKKKKKKKKKKKKKKKK
    double NarrowHalf(int PN){//+,-のどちらかに探索範囲を狭める（現在地が中央値になる）
       if(0<PN){//+方向
           RNeg = RMed; pNeg = pMed;
       } else if(PN<0){//-方向
           RPos = RMed; pPos = pMed;
       }
       stateEx1 = true;
       RMed = RNow; pMed = pNow;
       return pDif / 2;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double UsualNextPoint(int PN){//よく使う探索点を返す//statePosも変更する
        if(0<PN){
            statePos = true;
            return pMed + pDif;
        }else{
            statePos = false;
            return pMed - pDif;
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    void pRSetPos(){//現在探索点をプラス側にセットするだけの関数
        RPos = RNow;    pPos = pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
    void pRSetMed(){//現在探索点を中央側にセットするだけの関数
        RMed = RNow;    pMed = pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
    void pRSetNeg(){//現在探索点をマイナス側にセットするだけの関数
        RNeg = RNow;    pNeg = pNow;
    }
    //KKKKKKKKKKKKKKKKKKKK
}
