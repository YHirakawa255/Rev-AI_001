using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

//AIパラメータの保存時に走るクラス
//Unityで用意されていたメソッドを用いていたため、現状では機能していない
public class SaveCls{
    //Agent
    DebugCls Debug = new DebugCls();
    static public int nAgent = Rev_AI_001.Program.Agent.Length;
    double[] cA1 = new double[nAgent];
    double[] cB1 = new double[nAgent];
    double[] cC1 = new double[nAgent];
    double[,,] A = new double[nAgent,65,64];
    double[,,] dA1 = new double[nAgent,65,64];
    double[,,] DA1 = new double[nAgent,65,64];
    double[,] B = new double[nAgent,64];
    double[,] dB1 = new double[nAgent,64];
    double[,] DB1 = new double[nAgent,64];
    double[,] C = new double[nAgent,64];
    double[,] dC1 = new double[nAgent,64];
    double[,] DC1 = new double[nAgent,64];
    double[] pDef = new double[nAgent];
    double[] pPre = new double[nAgent];
    double[] pTmp = new double[nAgent];
    double[] pOpti = new double[nAgent];
    double[] pNeg = new double[nAgent];
    double[] pPos = new double[nAgent];
    double[] pE = new double[nAgent];
    double[] pNext = new double[nAgent];
    double[] RPre = new double[nAgent];
    double[] RTmp = new double[nAgent];
    double[] ROpti = new double[nAgent];
    double[] RNeg = new double[nAgent];
    double[] RPos = new double[nAgent];
    //KKKKKKKKKKKKKKKKKKKKセーブ、ロード用変数
    public SaveBufACls BufA = new SaveBufACls();
    public SaveBufBCls BufB = new SaveBufBCls();
    public SaveBufCCls BufC = new SaveBufCCls();
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public void Save(ref AgentCls aug){//Aiのセーブ
        string DebugString = $"Save Agent {aug.MyID}\r\n";
        string Key, sJson;
        // int tmp;
        int Layer, i1, i2;
        //A
        for(Layer = 0; Layer<5; Layer++){
            for(i1 = 0; i1<64; i1++){
                for(i2=0; i2<64; i2++){
                    // tmp = 64 * i1 + i2;//1次元配列までしか対応していないための処置
                    BufA.A[i2] = aug.A[Layer,i1,i2];//配列を移し替える
                }
                Key = GetKeyString(aug.MyID, "A", Layer, i1);//キーを生成
            // sJson = JsonUtility.ToJson(BufA);//AをJson形式に変換
            // PlayerPrefs.SetString(Key, sJson);//Josn文字列として保存
                DebugString += Key + "\r\n";//何を保存したのか（デバッグ）
                // Debug.Log("Save Key:"+Key+" : "+sJson);
            }
        }
        //B
        for(Layer = 0; Layer<5; Layer++){
            for(i1 = 0; i1<64; i1++){
                BufB.B[i1] = aug.B[Layer,i1];
            }
            Key = GetKeyString(aug.MyID, "B", Layer, 0);//キーを生成
        // sJson = JsonUtility.ToJson(BufB);//BをJson形式に変換
        // PlayerPrefs.SetString(Key, sJson);//Json文字列として保存
            DebugString += Key + "\r\n";//何を保存したのか（デバッグ）
            // Debug.Log("Save Key:"+Key+" : "+sJson);
        }
        //C
        for(i1 = 0; i1<64; i1++){
            BufC.C[i1] = aug.C[i1];
        }
        Key = GetKeyString(aug.MyID, "C", 0, 0);//キーを生成
    // sJson = JsonUtility.ToJson(BufC);//CをJson形式に変換
    // PlayerPrefs.SetString(Key, sJson);//Json文字列として保存
        DebugString += Key + "\r\n";//何を保存したのか（デバッグ）
        // Debug.Log("Save Key:"+Key+" : "+sJson);

        Debug.Log($"Saved AI {aug.MyID}\r\n{DebugString}");
    }
    // KKKKKKKKKKKKKKKKKKKK
    public void Load(ref AgentCls aug){//AIのロード
        // double Error = 0;
        string DebugString = $"Load AI {aug.MyID}\r\n";
        // string DebugError = $"Load AI {aug.MyID}\r\n";
        string Key, sJson;
        int Layer=0, i1=0, i2=0;
        //A
        for(Layer = 0; Layer<5; Layer++){
            for(i1 = 0; i1<64; i1++){
                Key = GetKeyString(aug.MyID, "A", Layer, i1);//キー取得
            // sJson = JsonUtility.ToJson(BufA);//デフォルト値
            // sJson= PlayerPrefs.GetString(Key, sJson);//Json形式のロード
                // Debug.Log("Load Key:"+Key+" : "+sJson);
            // BufA = JsonUtility.FromJson<SaveBufACls>(sJson);//Json形式の変換
                //コピー
                for(i2=0; i2<64; i2++){
                    // Error += Math.Abs( aug.A[Layer,i1,i2] - BufA.A[i2] );
                    // if( aug.A[Layer,i1,i2] != BufA.A[i2] ){
                    //     DebugError += $"A:{Layer}-{i1}[{i2}] : {aug.A[Layer,i1,i2] - BufA.A[i2]} = {aug.A[Layer,i1,i2]} - {BufA.A[i2]}\r\n";
                    // }
                    aug.A[Layer,i1,i2] = BufA.A[i2];//コピー
                }
                DebugString += Key + "\r\n";//何をロードしたのか（デバッグ）
            }
        }
        //B
        for(Layer = 0; Layer<5; Layer++){
            Key = GetKeyString(aug.MyID, "B", Layer, 0);//キー取得
        // sJson = JsonUtility.ToJson(BufB);//デフォルト値
        // sJson = PlayerPrefs.GetString(Key, sJson);//Json形式のロード
            // Debug.Log("Load Key:"+Key+" : "+sJson);
        // BufB = JsonUtility.FromJson<SaveBufBCls>(sJson);//Json形式の変換
            //コピー
            for(i1 = 0; i1<64; i1++){
                // Error += Math.Abs( aug.B[Layer,i1] - BufB.B[i1] );
                // if( aug.B[Layer,i1] != BufB.B[i1] ){
                //     DebugError += $"B:{Layer}-[{i1}] : {aug.B[Layer,i1] - BufB.B[i1]} = {aug.B[Layer,i1]} - {BufB.B[i1]}\r\n";
                // }
                aug.B[Layer,i1] = BufB.B[i1];//コピー
            }
            DebugString += Key + "\r\n";//何をロードしたのか（デバッグ）
        }
        //C
        Key = GetKeyString(aug.MyID, "C", 0, 0);//キー取得
    // sJson = JsonUtility.ToJson(BufC);//デフォルト値
    // sJson = PlayerPrefs.GetString(Key, sJson);//Json形式のロード
        // Debug.Log("Load Key:"+Key+" : "+sJson);
    // BufC = JsonUtility.FromJson<SaveBufCCls>(sJson);//Json形式の変換
        //コピー
        for(i1 = 0; i1<64; i1++){
            // Error += Math.Abs( aug.C[i1] - BufC.C[i1] );
            // if( aug.C[i1] != BufC.C[i1] ){
            //     DebugError += $"C:-[{i1}] : {aug.C[i1] - BufC.C[i1]} = {aug.C[i1]} - {BufC.C[i1]}\r\n";
            // }
            aug.C[i1] = BufC.C[i1];//コピー
        }
        DebugString += Key + "\r\n";//何をロードしたのか（デバッグ）
        Debug.Log($"Loaded AI {aug.MyID}\r\n{DebugString}");
        // Debug.Log($"Loaded AI {aug.MyID} Error = {Error}\r\n{DebugError}");
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void Save(ref AgentCls[] aug){
        for(int i1=0; i1<aug.Length; i1++){
            Save(ref aug[i1]);
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void Save(ref OptiCls aug){
    // string s = JsonUtility.ToJson(aug);
        string sName = "OptiCls";
    // PlayerPrefs.SetString( sName, s );
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void Save(ref FlagCls aug){
    // string s = JsonUtility.ToJson(aug);
        string sName = "FlagCls";
    // PlayerPrefs.SetString( sName, s );
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void SaveToHDD(){
    // PlayerPrefs.Save();
    }
    // KKKKKKKKKKKKKKKKKKKK
    public void Load(ref AgentCls[] aug){
        for(int i1=0; i1<aug.Length; i1++){
            aug[i1].InitAgent(i1);
            Load(ref aug[i1]);
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void Load(ref OptiCls aug){
        string sName = "OptiCls";
        OptiCls def = new OptiCls();
    // string s = PlayerPrefs.GetString( sName, JsonUtility.ToJson(def) );
    // aug = JsonUtility.FromJson<OptiCls>( s );
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void Load(ref FlagCls aug){
        string sName = "FlagCls";
        FlagCls def = new FlagCls();
        def.InitFlag();
    // string s = PlayerPrefs.GetString( sName, JsonUtility.ToJson(def) );
    // aug = JsonUtility.FromJson<FlagCls>( s );
    }
    //KKKKKKKKKKKKKKKKKKKK
    string GetKeyString(int MyID, string Kind, int Layer, int i1){//キー文字列を生成取得する
        string s = $"{MyID}{Kind}{Layer}-{i1}";
        return s;
    }
    //KKKKKKKKKKKKKKKKKKKK
    // public bool CompareError(int ABC, int Layer, int i1, int i2, ref AgentCls aug){//デバッグで利用。保存前後で値が正しく保存できているかの確認をする。
    //     // int tmp;
    //     if(ABC == 0){
    //         // tmp = 64 * i1 + i2;
    //         // Debug.Log(tmp);
    //         if(BufA.A[i2] != aug.A[Layer,i1,i2]){
    //             Debug.Log($"A{Layer}-[{i1},{i2}] not equal");
    //             return false;
    //         }
    //     }else if(ABC == 1){
    //         if(BufB.B[i1] != aug.B[Layer,i1]){
    //             Debug.Log($"B{Layer}-[{i1}] not equal");
    //             return false;
    //         }
    //     }else if(ABC == 2){
    //         if(BufC.C[i1] != aug.C[i1]){
    //             Debug.Log($"C[{i1}] not equal");
    //             return false;
    //         }
    //     }
    //     return true;
    // }
    //KKKKKKKKKKKKKKKKKKKK
    // string CompareError2(double d1, double d2){//デバッグで利用。正しく保存できているかの確認をする
    //     string s = "";
    //     double tmp;
    //     tmp = d1 - d2;
    //     if( 0==tmp ){
    //         return $"Not Equal div:{tmp}\r\n";
    //     }
    //     return s;
    // }
    //KKKKKKKKKKKKKKKKKKKK

}





//KKKKKKKKKKKKKKKKKKKK//セーブ用のクラス。配列をJson化できなかったため、クラスを用意した
public class SaveBufACls{
    public double[] A = new double[64];
}
//KKKKKKKKKKKKKKKKKKKK
public class SaveBufBCls{
    public double[] B = new double[64];
}
//KKKKKKKKKKKKKKKKKKKK
public class SaveBufCCls{
    public double[] C = new double[64];
}
