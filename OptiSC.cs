using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

public class OptiCls{
    public int Counter;
    public int CounterMax;
    // public bool FBaseRoundRobin = true;
    // public bool FOptSetRequire;//?
    // public bool FAgentUpdateRequire;
    static public int nAgent = Rev_AI_001.Program.Agent.Length;
    public double[,] Result = new double[nAgent, nAgent];//戦績の配列
    public double[,] ResultDelBlack = new double[nAgent, nAgent];//戦績の配列（黒摂動あり）
    public double[,] ResultDelWhite = new double[nAgent, nAgent];//戦績の配列（白摂動あり）
    public double[] SumResultBlack = new double[nAgent];//戦績集計値（先攻時）
    public double[] SumResultDelBlack = new double[nAgent];//戦績集計値（黒摂動あり）
    public double[] SumResultWhite = new double[nAgent];//戦績集計値（後攻時）
    public double[] SumResultDelWhite = new double[nAgent];//戦績集計値（白摂動あり）
    public double[] SumResult = new double[nAgent];//戦績集計値
    public double[] SumResultDel = new double[nAgent];//戦績集計値（摂動あり）
    string SLog = "";
    BisecManageClass BisecManager = new BisecManageClass();
    DebugCls Debug = new DebugCls();
    //KKKKKKKKKKKKKKKKKKKK(RoundRobinAll)
    public void CalcSumResult(){//総当たりの合計戦績を計算
        string S = "CalcSumResult\n\r";
        for(int iB=0; iB<nAgent; iB++){ SumResultBlack[iB] = 0; }//リセット
        for(int iW=0; iW<nAgent; iW++){ SumResultWhite[iW] = 0; }//リセット
        for(int iB=0; iB<nAgent; iB++){
            for(int iW=0; iW<nAgent; iW++){
                SumResultBlack[iB] += QBlack( Result[iB, iW] );
                SumResultWhite[iW] += QWhite( Result[iB, iW] );
            }
        }
        for(int i1 = 0; i1<nAgent; i1++){//デバック
            SumResult[i1] = SumResultBlack[i1] - SumResultWhite[i1];
            S += i1  + " B:" + SumResultBlack[i1].ToString() + ", W:" + SumResultWhite[i1].ToString() + "\n\r";
        }
        // Debug.Log(S);
    }
    //KKKKKKKKKKKKKKKKKKKK(Set)
    public void InitOptRPSetAndPerturbation(ref AgentCls[] Agent){//(in OptimizationSet)//クラス初期化
        CounterMax = 3;//繰り返し計算の打ち切り
        for(int i1=0; i1<Agent.Length;  i1++){
            Agent[i1].SetGradientVector();//探索方向ベクトルをランダム生成、反映
            Agent[i1].OptPRSet(ref SumResult[i1]);//最適化コントローラのリセット、基準となる評価値の取得、初期摂動を取得・反映
            Agent[i1].SetPerturbation();//初期摂動を評価計算に反映
        }
    }
    //KKKKKKKKKKKKKKKKKKKK(RoundRobinDel)
    public void CalcSumResultDel(){//摂動時の評価結果の集計
        // string S = "CalcSumResultDel\n\r";//デバッグ
        for(int i1=0; i1<nAgent; i1++){//!!!!!!!!!!FFinIteration参照にしたい
            CalcSumResultDelBlack(i1);
            CalcSumResultDelWhite(i1);
            SumResultDel[i1] = SumResultDelBlack[i1] - SumResultDelWhite[i1];
        }
        for(int i1=0; i1<nAgent; i1++){
            // S += i1+SumResultDel[i1].ToString()+"\n\r";
        }
        // Debug.Log(S);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public double CalcSumResultDelBlack(int iB){//iBの評価値を集計
        SumResultDelBlack[iB] = 0;
        for(int iW=0; iW<nAgent; iW++){
            SumResultDelBlack[iB] += QBlack( ResultDelBlack[iB, iW] );
        }
        return SumResultDelBlack[iB];
    }
    //KKKKKKKKKKKKKKKKKKKK
    public double CalcSumResultDelWhite(int iW){//iWの評価値を集計
        SumResultDelWhite[iW] = 0;
        for(int iB=0; iB<nAgent; iB++){
            SumResultDelWhite[iW] += QWhite( ResultDelWhite[iB, iW] );
        }
        return SumResultDelWhite[iW];
    }
    //KKKKKKKKKKKKKKKKKKKK
    public string DebugResult(){//総当たり結果の表示
        string s = "Result def \n\r";
        for(int ib = 0; ib<nAgent; ib++){
            s+=ib+":";
            for(int iw = 0; iw<nAgent; iw++){
                s+=Result[ib,iw]+",";
            }
            s+="\n\r";
        }
        return s;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public string DebugResultDel(){//総当たり結果の表示
        string s = "Result Del\n\r";
        for(int ib = 0; ib<nAgent; ib++){
            s+=ib+":";
            for(int iw = 0; iw<nAgent; iw++){
                s+=ResultDelBlack[ib,iw]+",";
            }
            s+="\n\r";
        }
            s+="\n\r";
        for(int ib = 0; ib<nAgent; ib++){
            s+=ib+":";
            for(int iw = 0; iw<nAgent; iw++){
                s+=ResultDelWhite[ib,iw]+",";
            }
            s+="\n\r";
        }
        return s;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double QBlack(double aug){
        if( aug<=0 ){
            return aug * aug * aug - 100;
        }
        return aug;
    }
    //KKKKKKKKKKKKKKKKKKKK
    double QWhite(double aug){
        if( 0<=aug ){
            return aug * aug * aug + 100;
        }
        return aug;
    }
    //KKKKKKKKKKKKKKKKKKKK(Post)
    public void NextPerturbationCalc(ref AgentCls[] aug){//(in OptimizationPost)次の探索点を決める
        foreach(AgentCls a in aug){
            if(!a.BisecManager.FFinIteration){//探索完了していないエージェントのみ実行
                a. GetNextP( SumResultDel[a.MyID] );//報酬値をセットし、次の摂動を取得
            }
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void SetPerturbation(ref AgentCls[] aug){//(in OptimizationPre=>Set)//摂動を各エージェントが取得し、評価計算に反映
        foreach(AgentCls a in aug){
            if(!a.BisecManager.FFinIteration){//探索完了していないエージェントのみ実行
                a.SetPerturbation();
            }
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    public bool FFinOptimizationAgent(ref AgentCls[] aug){//各エージェントの探索が終了していたらtrueを返す
        foreach(AgentCls a in aug){
            if(!a.BisecManager.FFinIteration){
                return false;
            }
        }
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK(Update)
    public bool UpdateAI(ref AgentCls[] aug){//(in OptimizationPost)//各AIのパラメータを更新
        string S = "";
        double SumDeltaParam;
        //反復終了判定
        bool FUpDate = true;
        for(int i1=0; i1<aug.Length; i1++){
            if(! aug[i1].BisecManager.FFinIteration){//反復終了フラグの確認
                FUpDate = false;
                break;
            }
        }
        //更新
        if(FUpDate){
            foreach(AgentCls a in aug){
                SumDeltaParam = a.UpdateParam();
                S += "AIAI" + a.MyID + "∑⊿λ = " + SumDeltaParam + "\n\r";
            }
        }
        Debug.Log(S);
        return FUpDate;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public string GetSLog(){
        string s = SLog;
        SLog = "";
        return s;
    }
    //KKKKKKKKKKKKKKKKKKKK
    // void SetResultDef(ref AgentCls[] aug){//基準となる戦績を各AIに格納する
    //     CalcSumResult();//評価値を
    //     for(int i1=0; i1<aug.Length; i1++){
    //         // aug[i1].SetROpti( SumResultBlack[i1] - SumResultWhite[i1] );//基準評価値をセット
    //         aug[i1].InitOpt( SumResultBlack[i1] - SumResultWhite[i1] );//基準評価値をセット
    //     }
    // }
    //KKKKKKKKKKKKKKKKKKKK
}