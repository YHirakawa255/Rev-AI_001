using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

public class BoadCls : DirCls{
    const int Black = 1;
    const int White = -1;
    const int Blank = 0;
    public bool ActiveFirstPlayer;
    bool Checked;
    public double e, eMax, eMin;
    // public int nBranchLast;
    public int nPutHist;    //
    public int nPutList;    //
    int parentPointe { get; }
    public PutOperationCls put = new PutOperationCls();
    public PutOperationCls[] putHist = new PutOperationCls[60];//配置の履歴
    public PutOperationCls[] putList = new PutOperationCls[128];//配置候補
    string SConsolPrintOut = "";
    // string SGameLog = "";
    string SPutLog = "";//配置の履歴
    public int[,] Stone = new int[8,8];
    public bool[,] StoneEdge = new bool[8,8];
    DebugCls Debug = new DebugCls();
    //KKKKKKKKKKKKKKKKKKKK
    public void InitBoad(){//盤面の初期化：ゲーム開始時にする
        //盤面の初期化
        // SGameLog = "";
        nPutHist = 0;//配置履歴の数(何手目か)
        Stone = new int[8,8];//配置石情報
        StoneEdge = new bool[8,8];//境界情報（境界以外のマスだけ配置可能（挟んでひっくり返せる）か確かめれば良い
        for(int i1=0; i1<putHist.Length; i1++){
            putHist[i1] = new PutOperationCls();
        }
        for(int i1=0; i1<putList.Length; i1++){
            putList[i1] = new PutOperationCls();
        }
        ActiveFirstPlayer = true;//リバーシは黒先攻
        Stone[3, 3] = White;
        Stone[4, 3] = Black;
        Stone[3, 4] = Black;
        Stone[4, 4] = White;
        EdgeCheck(3, 3, ref putHist[0]);
        EdgeCheck(4, 3, ref putHist[0]);
        EdgeCheck(3, 4, ref putHist[0]);
        EdgeCheck(4, 4, ref putHist[0]);
        putHist[0] = new PutOperationCls();
        PutCheck();//この初期盤面に対して配置可能な箇所を調べる
        SPutLog = "";
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void AutoPut(int p, ref AgentCls AI1){//AIの評価に従って自動で一手配置する
        int i1 = p;//配置候補のp番目に配置する//i1は以前の実装の名残
        Put(ref putList[i1]);//着手
        AI1.Put(ref putList[i1]);//AIの内部盤面も更新
        SPutLog += AutoPutString(ref putList[i1]);//配置の履歴を残す（デバッグ）
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void AutoPut(int p, ref AgentCls AI1, ref AgentCls AI2 ){//AIの評価に従って自動で一手配置する
        int i1 = p;//配置候補のp番目に配置する//i1は以前の実装の名残
        Put(ref putList[i1]);//着手
        AI1.Put(ref putList[i1]);//AIの内部盤面も更新
        AI2.Put(ref putList[i1]);//AIの内部盤面も更新
        SPutLog += AutoPutString(ref putList[i1]);//配置の履歴を残す（デバッグ）
    }
    //KKKKKKKKKKKKKKKKKKKK
    string AutoPutString(ref PutOperationCls aug){//配置履歴を出力する(SPutLogに)
        string s;
        int tmp;
        if(aug.Color == 1){
            s = "B";
        }else{
            s = "W";
        }
        tmp = aug.x + 1;
        s += tmp.ToString();
        tmp = aug.y + 1;
        s += tmp.ToString() + ",";
        return s;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public (double e, int p) CalcEBranch(ref AgentCls AI, ref bool fD, int nDeep, int p ){//分岐先まで見て最善手と評価を返すはずだった
        //!!!ややこしい実装になっているので、深度探索は未実装
        //分岐先の盤面を評価し、分岐先の最良値をputListに格納する
        //nDeep：残り分岐深度、p：putListの開始ポインタ（今の所0で固定）
        //putListはグローバル変数で共有している（変えようと思う)
        //だから、どこからが調べるべき分岐なのかpで指示している
        // string s;
        // p = 0;
        int Color = putList[p].Color;   //手番の色
        int pEnd = putList[p].nPutList;//配置候補の数
        for(int i1 = 0; i1<pEnd; i1++){//候補すべての評価値を調べる
            Put(ref putList[i1]);//着手=>評価=>保存=>もとに戻す、を繰り返す
            // AI.Put(ref putList[i1]);//AI内部盤面も着手
            AI.SetBoad(ref Stone);//(効率化ではなく安定を優先)
            putList[i1].e = AI.E(fD);//評価値を調べる//=CalcEBranch
            PutInverse();//着手を戻す
            // AI.PutInverse(ref putList[i1]);//着手を戻す
        }
        AI.SetBoad(ref Stone);//AI内部盤面をもとに戻す(?いらない)
        //最良値調査
        p = CalcEBranchMinMax(p, pEnd, putList[p].Color);
        e = putList[p].e;//最良の評価値を返す

        //再帰の終端の処理：盤面評価値を返す
        // if( nDeep <= 1){
        //     e = AI.E( fD );//評価値
        //     i = p;//ポインタ
        // }else{
            //分岐先の最良値を求める
            //分岐の候補は計算済み
            // for(int i1=p; i1<putList[p].nPutList; i1++){//分岐先ポインタから計算開始
            //     Put(ref putList[i1]);   //着手する
            //     AI.Put(ref putList[i1]);    //AIの内部盤面も合わせて更新
            //     //nPutList番目以降に分岐が計算される：追加される
            //     if( PutCheck( putList[p].nPutList ) <= 0){  
            //         //置ける場所がない：パスの場合
            //         ChangeActivePlayer();   
            //         if( PutCheck( putList[p].nPutList ) <=0 ){  
            //             //置ける場所がない：ゲーム終了時
            //             s = "Banch-"+nDeep+":"+i1+"\n\r";
            //             // ConsolPrint(s);
            //             putList[i1].e = CalcEAbsolute();//終局盤面絶対評価値
            //         }else{//おける時：相手がパスで手番が続く時
            //             s = "Banch-"+nDeep+":"+i1+"\n\r";
            //             // ConsolPrint(s);
            //             (putList[i1].e, ) = CalcEBranch(ref AI, ref fD, nDeep-2, putList[p].nPutList);  //再帰
            //         }
            //     }else{
            //         //置ける場所がある：手番交代
            //         s = "Banch-"+nDeep+":"+i1+"\n\r";
            //         // ConsolPrint(s);
            //         (putList[i1].e, ) = CalcEBranch(ref AI, ref fD, nDeep-1, putList[p].nPutList);  //再帰
            //     }
            //     AI.PutInverse(ref putList[i1]);//AIの内部盤面を戻す
            //     PutInverse();//盤面を戻す
            //     s = "Banch-"+(nDeep+1)+":"+p+"\n\r";
            //     // ConsolPrint(s);
            // }//for i1
            //最良値選択
            // eMinMax = putList[p].e;
            // for(int i1 = p+1; i1<putList[p].nPutList; i1++){
            //     if(Color == Black){ //黒手番の時：最大値
            //         eMinMax = Math.Max( eMinMax, putList[i1].e );
            //     }else{  //白手番の時 ：最小値
            //         eMinMax = Math.Min( eMinMax, putList[i1].e );
            //     }
            // }
            // e = eMinMax;
            // i = 0;
        // }
        return (e, p);
    }
    //KKKKKKKKKKKKKKKKKKKK
    int CalcEBranchMinMax(int ps, int pe, int fMin){//ps-pe番目の配置候補の最善手を返す
        // string s = "CalcEBranchMinMax-"+nPutHist+":\n\r";//デバッグ
        double e = putList[ps].e;
        int p = ps;
        if(fMin>=0){//黒手番
            // s += "Max\n\r";
            for(int i1=ps; i1<pe; i1++){
                // s += i1 + ":" + putList[i1].e + "\n\r";
                if(e<putList[i1].e){
                    p = i1;
                    e = putList[i1].e;
                }
            }
            // s += "Max:" + p + ":" + e;
        }else{//白手番
            // s += "Min\n\r";
            for(int i1=ps; i1<pe; i1++){
                // s += i1 + ":" + putList[i1].e + "\n\r";
                if(e>putList[i1].e){  
                    p = i1;
                    e = putList[i1].e;
                }
            }
            // s += "Min:" + p + ":" + e;
        }
        // Debug.Log(s);
        return p;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public int PutCheck(int pList = 0){//置ける場所の確認
        //!!!探索時の実装に合わせて変更をすべき

        //配置可能場所を検定し、配置座標を基点とした各方向に
        //いくつの石を反転できるか、putListに保持する
        //配置可能箇所の数を返す
        //引数は分岐の保存起点ポインタ
        //pList//配置場所リストの後端を示す
        if(Checked){
            return putList[pList].nPutList;   //既にPutCheckを実行している場合、処理を省略する
        }
        int PutColor, pListPre;//, pPutList;
        pList = 0;
        pListPre = pList;//これまでの配置可能場所リストの数
        if(ActiveFirstPlayer){   //反転後の石の色
            PutColor = Black;
        }else{
            PutColor = White;
        }
        for(int iy=0; iy<8; iy++){  //全盤面探索
            for(int ix=0; ix<8; ix++){
                if( StoneEdge[ix,iy] ){ //配置候補かどうか
                    if( PutCheckSub(ref putList[pList], ref ix, ref iy, PutColor) ){  //配置可能かどうか
                        putList[pList].x = ix;  //配置場所などの情報を保存
                        putList[pList].y = iy;
                        putList[pList].Color = PutColor;
                        pList++;
                    }
                }
            }
        }
        putList[pList].x = -1;  //表示のバグ回避
        putList[pList].y = -1;
        // nPutList = pList - pListPre;
        nPutList = pList;
        for(int i1=pListPre; i1<pList; i1++){
            putList[i1].nPutList = pList;
        }
        if( pListPre - pList > 0 ){ Checked = true; }   //チェック済み
        // Debug.Log("PutCheck n:" + nPutList);
        // ConsolPrint();
        return nPutList;    //配置可能場所の数を返す
    }
    //KKKKKKKKKKKKKKKKKKKK
    bool PutCheckSub(ref PutOperationCls aug, ref int x, ref int y, int PutColor){//分割
        //(x,y)地点が配置可能場所かどうか調べ、boolを返す
        //augに反点数を書き込んでおく
        int px, py; //ポインタ
        bool f1 = false;
        for(int id=0; id<8; id++){  //各方向に対して
            aug.reverseDirection[id] = 0;   //初期化
            for(int ir=1; ir<10; ir++){ //無限ループ回避
                px = x+ir*DirX[id];
                py = y+ir*DirY[id];
                if( !Inside(ref px, ref py ) ){ //ポインタが盤面内かどうか
                    aug.reverseDirection[id] = 0;   //ポインタが盤面外:挟めなかったとき
                    break;
                }else{
                    if( Stone[px, py] == -PutColor ){ //相手の石のとき
                        aug.reverseDirection[id]++; //カウンタ+1
                    }else if( Stone[px, py] == PutColor ){ //自分の石のとき
                        if( aug.reverseDirection[id]>0 ){
                            f1 = true;  //フラグセットし
                        }
                        break;  //次の方向へ
                    }else{  //空白のとき:挟めなかった時
                        aug.reverseDirection[id] = 0;   //一つも返せません
                        break;  //次の方向へ
                    }
                }
            }
        }
        return f1;  //配置可能かどうか
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void Put(ref PutOperationCls aug){//配置履歴クラスを引数に着手する
        //自動で、配置履歴を残す
        //変更される盤面は Stone[,], StoneEdge[,]
        int px, py;
        Checked = false;    //配置可能場所が変更になったことを表す
        aug.nPutHist = nPutHist;    //何手目
        putHist[nPutHist] = aug;    //履歴を残す
        Stone[aug.x, aug.y] = aug.Color;    //置いた場所
        EdgeCheck(aug.x, aug.y, ref putHist[nPutHist] );    //配置候補更新
        for(int id=0; id<8; id++){ 
            //各方向(id)に対して、石を反転させる
            for(int ir=1; ir<=aug.reverseDirection[id]; ir++){//返せる枚数が繰り返しの上限となる
                px = aug.x + ir*DirX[id];
                py = aug.y + ir*DirY[id];
                Stone[px, py] = aug.Color;
            }
        }
        nPutHist++; //履歴の数をインクリメント
        ChangeActivePlayer();   //自動交代
    }
    //KKKKKKKKKKKKKKKKKKKK
    void EdgeCheck(int x, int y, ref PutOperationCls aug){//新規エッジの確認
        //配置候補を更新する
        int px, py;
        aug.edgeAdd = new bool[8];//初期化　ここに追加したエッジだけ格納
        StoneEdge[x,y] = false;//この関数が走るのは、(x,y)に配置したとき
        for(int id=0; id<8; id++){
            px = x + DirX[id];
            py = y + DirY[id];
            if( Inside(ref px, ref py) ){
                if( Stone[px, py] == 0 && StoneEdge[px, py] == false ){//空白であり、まだエッジと登録されていない場合
                    StoneEdge[px, py] = true;//エッジに登録
                    aug.edgeAdd[id] = true;//新規登録したことを登録
                }
            }
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void PutInverse(){//着手を戻す(引数不要、配置履歴から戻すため)
        nPutHist--;//配置履歴の数が減る
        int px, py;
        for(int id=0; id<8; id++){//方向ポインタ
            for(int ir=1; ir<=putHist[nPutHist].reverseDirection[id]; ir++){//距離ポインタ
                px = putHist[nPutHist].x + ir*DirX[id];
                py = putHist[nPutHist].y + ir*DirY[id];
                Stone[px, py] = - putHist[nPutHist].Color;//-Colorは裏側の色を表す
            }
        }
        EdgeCheckInverse(ref putHist[nPutHist]);//エッジ情報ももとに戻す
        Stone[ putHist[nPutHist].x, putHist[nPutHist].y ] = Blank;//(?)
        if(putHist[nPutHist].Color == Black){
            ActiveFirstPlayer = true;
        }else{
            ActiveFirstPlayer = false;
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    void EdgeCheckInverse(ref PutOperationCls aug){//一手戻したときの境界判定を一手ぶん戻す
        int px, py;
        StoneEdge[aug.x,aug.y] = true;//一手戻したのだから、その座標は境界である
        for(int id=0; id<8; id++){
            if(aug.edgeAdd[id]){//着手時の追加境界情報を元に、境界情報を復元する
                px = aug.x + DirX[id];
                py = aug.y + DirY[id];
                StoneEdge[px,py] = false;//その一手で境界になったのだから、境界ではなくなる　
            }
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    public double CalcEAbsolute(){//ゲーム終了盤面の絶対評価値
        //空白を残すと得点が高くなるような式にしている
        int nB = 0;
        int nW = 0;
        for(int iy=0; iy<8; iy++){
            for(int ix=0; ix<8; ix++){
                if(Stone[ix,iy]==Black){
                    nB++;
                }else if(Stone[ix,iy]==White){
                    nW++;
                }
            }
        }
        // ConsolPrintOut();
        return 4096.0 * (nB-nW) / Math.Pow(nB+nW, 2);
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public void ConsolPrint(string s = "", int Select = -1){//盤面をコンソール出力する
        Debug.Log( GetConsolPrint(s, Select));
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void ConsolPrintOut(){//コンソール出力用の文字列を出力してリセット（デバッグ用）
        Debug.Log(SConsolPrintOut);
        SConsolPrintOut = "";
    }
    //KKKKKKKKKKKKKKKKKKKK
    public string GetConsolPrint(string s = "", int Select = -1){//盤面のコンソール出力用文字列を生成する
        //盤面の簡易表示
        string S = $"{s}\n\r";//表示するときに好きな文字列を表示できる
        int N = 0;
        int putP = 0;
        for(int iy = 0; iy<8; iy++){
            for(int ix=0; ix<8; ix++){
                if(Stone[ix,iy]==Black){
                    S += "國";//黒
                }else if(Stone[ix,iy]==White){
                    S += "口";//白
                }else if( (putList[putP].x == ix) && (putList[putP].y == iy) ){ 
                    if(N<10){
                        S += $"_{N}";
                    }else{
                        S += $"{N}";
                    }
                    N++;
                    putP++;
                }else if( StoneEdge[ix,iy] ){
                    S += "＿";//配置可能検定候補
                }else{
                    S += "＿";//完全な空白
                }
            }
            S += "\n\r";
        }
        S += "\n\r" + "\n\r";
        // SConsolPrintOut += S;//?
        return S;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public string GetPutString(){//棋譜を返して、棋譜を消去する
        string s = SPutLog;
        SPutLog = "";
        return s;
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    bool Inside(ref int x, ref int y){//ポインタが盤面内かどうかを判定する
        if(x<0){
            return false;
        }else if(x>=8){
            return false;
        }else if(y<0){
            return false;
        }else if(y>=8){
            return false;
        }
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    int ColorMy(){//ActiveFirstPlayerを元に、今の手番の色を返す
        if(ActiveFirstPlayer){
            return Black;
        }
        return White;
    }
    //KKKKKKKKKKKKKKKKKKKK
    int ColorOppo(){//ActiveFirstPlayerを元に、今の手番ではない色を返す
        if(ActiveFirstPlayer){
            return White;
        }
        return Black;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void ChangeActivePlayer(){//手番交代するだけの関数
        if(ActiveFirstPlayer){//黒手番=>白手番
            ActiveFirstPlayer = false;
        }else{//白手番=>黒手番
            ActiveFirstPlayer = true;
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
}
//KKKKKKKKKKKKKKKKKKKK
//KKKKKKKKKKKKKKKKKKKK
//KKKKKKKKKKKKKKKKKKKK
// public class PointerCls{
//     public void InitPointer(){
//     }
// }
//KKKKKKKKKKKKKKKKKKKK
public class PutOperationCls{//配置情報を記録するクラス（配置候補と配置履歴に使う）
    public int x, y, Color, nPutList, nPutHist, parentList;
    public double e;
    public int[] reverseDirection = new int[8];
    public bool[] edgeAdd = new bool[8];
}
//KKKKKKKKKKKKKKKKKKKK