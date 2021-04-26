using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEngine;
using System.Linq;
using System.Threading.Tasks;//並列処理

public class BitBoadCls{
    const int Black = 1;
    const int White = -1;
    int iMy, iOppo;
    public bool ActiveFirstPlayer;
    public UInt64[] StoneBW  = new UInt64[2];
    UInt64[] Pointers = new UInt64[64];
    UInt64 StoneBlank = new UInt64();
    UInt64[] CanPutDirection = new UInt64[8];
    UInt64 CanPut = new UInt64();
    UInt64[] PutBuffer = new UInt64[8];
    int[] DirShiftN = new int[8];
    bool[] DirShiftF = new bool[8];
    bool[] DirShiftWallF = new bool[8];
    int[] DirShiftSwitch = new int[8];
    UInt64 Wall = new UInt64();
    DebugCls Debug = new DebugCls();
    //KKKKKKKKKKKKKKKKKKKK
    public BitBoadCls(){//オーバーロード
        PointerPrepare();
        InitBoad();
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void InitBoad(){
        StoneBW[0] = 0;   StoneBW[1] = 0;
        StoneBW[0] |= Pointing64(4,3);
        StoneBW[0] |= Pointing64(3,4);
        StoneBW[1] |= Pointing64(3,3);
        StoneBW[1] |= Pointing64(4,4);
        ConsolPrint("Init Bit Boad");
        ActiveFirstPlayer = false;
        ChangeActivePlayer();//黒手番
    }
    //KKKKKKKKKKKKKKKKKKKK
    void PointerPrepare(){//ポインタ行列を準備する
        int i1;
        Pointers[0] = 1;
        for(i1=1; i1<64; i1++){
            Pointers[i1] = Pointers[i1-1] << 1;
            // Debug.Log(Pointers[i1]);
        }
        // for(i1=0; i1<8; i1++){
        //     PutBuffer[i1] = new UInt64();
        // }
        DirShiftN[0] = 9;
        DirShiftN[1] = 8;
        DirShiftN[2] = 7;
        DirShiftN[3] = 1;
        DirShiftN[4] = 1;
        DirShiftN[5] = 7;
        DirShiftN[6] = 8;
        DirShiftN[7] = 9;
        DirShiftF[0] = true;
        DirShiftF[1] = true;
        DirShiftF[2] = true;
        DirShiftF[3] = true;
        DirShiftF[4] = false;
        DirShiftF[5] = false;
        DirShiftF[6] = false;
        DirShiftF[7] = false;
        DirShiftWallF[0] = true;
        DirShiftWallF[1] = false;
        DirShiftWallF[2] = true;
        DirShiftWallF[3] = true;
        DirShiftWallF[4] = true;
        DirShiftWallF[5] = true;
        DirShiftWallF[6] = false;
        DirShiftWallF[7] = true;
        DirShiftSwitch[0] = 0;
        DirShiftSwitch[1] = 2;
        DirShiftSwitch[2] = 0;
        DirShiftSwitch[3] = 0;
        DirShiftSwitch[4] = 1;
        DirShiftSwitch[5] = 1;
        DirShiftSwitch[6] = 3;
        DirShiftSwitch[7] = 1;
        Wall = 0X7E7E_7E7E_7E7E_7E7E;
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public (int nPut, UInt64 PutPlace) PutCheck(UInt64 StoneMy, UInt64 StoneOppo, int pList = 0){
        int N, nPut;
        UInt64 PutPlace = 0;
        UInt64[] PutBuffer = new UInt64[8];
        UInt64 Buffer = new UInt64();
        UInt64 WallOppo;// = StoneOppo & Wall;//壁と相手石の論理積
        Func<UInt64, UInt64> Shift = (a) => a << 0;
        StoneBlank = ~( StoneMy | StoneOppo );//空白マス
        // ConsolBitPrint(StoneBlank,"Blank");
            // StoneBlank >>= 1;//最上位ビットは0になるようだ
        // StoneBlank <<= -2;
        // ConsolBitPrint(StoneBlank,"Blank << -2");
        // ConsolBitPrint(Wall,"Wall");
        // for(int Dir = 0; Dir<8; Dir++){
        //     // ConsolBitPrint( ShiftBoad(StoneBlank,Dir) & Wall, $"Shift Dir:{Dir}");
        // }
        for(int id=0; id<8; id++){
            //前準備
            N = DirShiftN[id];//シフト数
            if(DirShiftF[id]){//シフト方向
                Shift = (a) =>a << N;
            }else{
                Shift = (a) =>a >> N;
            }
            if(DirShiftWallF[id]){//壁の有無
                WallOppo = StoneOppo & Wall;
            }else{
                WallOppo = StoneOppo;
            }
            //配置可能検定
            Buffer = WallOppo & Shift(StoneMy);
            for(int ir=0; ir<5; ir++){
                Buffer |= WallOppo & Shift(Buffer);
            }
            PutBuffer[id] = StoneBlank & Shift(Buffer);
        }
        //配置可能検定

        foreach(UInt64 aug in PutBuffer){
            PutPlace |= aug;//各方向の判定をまとめる
        }
        CanPut = PutPlace;
        nPut = CountBit(PutPlace);
        if( (PutPlace & (StoneMy|StoneOppo))>0 ){
            string s = GetConsolBitPrint(StoneMy);
            s += GetConsolBitPrint(StoneOppo);
            s += GetConsolBitPrint(StoneBlank);
            s += GetConsolBitPrint(PutPlace);
            Debug.Log($"!!!Warning PutCheck Error!!!{s}");
        }
        // ConsolBitPrint(PutPlace,$"PutPlace {nPut}");
        return (nPut, PutPlace);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public (UInt64 MyReturn, UInt64 OppoReturn) Put(UInt64 My, UInt64 Oppo, UInt64 Paug){
        string s = "Put";
        UInt64 P = Paug;
        UInt64 MyReturn = My;
        UInt64 OppoReturn = Oppo;
        // if( (Paug&CanPut) == 0){
        //     //End
        // }
        UInt64 Putting = Paug;
        UInt64 Mask;
        int N, id7;
        //ポインタがおかしいときの処理
        if( ( (My|Oppo) & P ) > 0 ){
            s += GetConsolBitPrint(My);
            s += GetConsolBitPrint(Oppo);
            s += GetConsolBitPrint(P);
            Debug.Log($"!!! Warning Put Error !!!{s}");
            return (0, 0);
        }
        Func<UInt64, UInt64> ShiftConst = (p) => p = p<<1;//ポインタの移動処理
        for(int id=0; id<8; id++){//各方向について処理する
            P = Paug;//起点ポインタ（配置場所）
            // if( (PutBuffer[id]&P) > 0 ){//その方向に返す石があるときだけ処理
            //下準備
            id7 = 7-id;//PutCheckとは移動方向が逆になるため
            N = DirShiftN[id7];//シフト数
            if(DirShiftF[id7]){//シフト方向について
                ShiftConst = (p) => p = p<<N;
            }else{
                ShiftConst = (p) => p = p>>N;//最上位ビットは0になることを確認済み
            }
            if(DirShiftWallF[id7]){//両側の壁の有無
                Mask = Wall & Oppo;
            }else{
                Mask = Oppo;
            }
            //メイン処理
            Putting = 0;
            for(int ir=0; ir<6; ir++){
                P = ShiftConst(P);
                if( (P & My) > 0 ){
                    MyReturn |= Putting;
                    break;
                }
                P =  Mask & P;//相手の石が続く限り、ポインタをずらしてゆく
                // ConsolBitPrint(P,$"Put Serch id{id}-ir{ir}");
                //高々6回の処理なので、if, break処理はしないことにした
                Putting |= P;//返す場所を追加してゆく
            }
            // }
        }//各方向について処理する
        //盤面に反映
        MyReturn |= Paug;//論理和
        OppoReturn &= ~MyReturn;//自分の石との重複を0にする
        return (MyReturn, OppoReturn);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void AutoGame(){
        bool FFin = false;
        UInt64 P;
        int N;
        string s;
        InitBoad();

        for(int i1=0; i1<64; i1++){
            (N, P) = PutCheck( StoneBW[iMy], StoneBW[iOppo] );
            if(N==0){
                ChangeActivePlayer();
                (N, P) = PutCheck( StoneBW[iMy], StoneBW[iOppo] );
                if(N==0){
                    FFin = true;
                    ConsolPrint("Fin");
                    break;
                }
            }
            ConsolPrint();
            // P = SelectBit(P,0);
            if(ActiveFirstPlayer){
                (P, _) = SelectMinMax(StoneBW[iMy], StoneBW[iOppo], 5, ActiveFirstPlayer);
                // (P, _, s ) = SelectMinMax(StoneBW[iMy], StoneBW[iOppo], 1, ActiveFirstPlayer);
            }else{
                (P, _) = SelectMinMax(StoneBW[iMy], StoneBW[iOppo], 1, ActiveFirstPlayer);
                // (P, _, s ) = SelectMinMax(StoneBW[iMy], StoneBW[iOppo], 7, ActiveFirstPlayer);
            }
            // Debug.Log(s);
            (StoneBW[iMy], StoneBW[iOppo]) = Put(StoneBW[iMy], StoneBW[iOppo], P);
            StoneBlank = ~(StoneBW[iMy] & StoneBW[iOppo]);
            ChangeActivePlayer();
        }

        
    }
    //KKKKKKKKKKKKKKKKKKKK
    (UInt64 P, float Score) SelectMinMax(UInt64 Off, UInt64 Dif, int nDeep, bool MinMax){
        // string s, S="";
        UInt64 P, OffBuf, DifBuf;
        float Score;
        if(MinMax){
            // S = GetConsolPrint(Off,Dif,"SelectMinMax");
        }else{
            // S = GetConsolPrint(Dif,Off,"SelectMinMax");
        }
        if(nDeep<=0){//終端条件
            P = 0;
            Score = CountBit(Off) - CountBit(Dif);
        }else{
            (int NBranch, UInt64 PBranch) = PutCheck(Off, Dif);//分岐先を取得
            if(NBranch==0){//分岐無し
                // (_, Score, s) = SelectMinMax(Dif, Off, nDeep-1, !MinMax);
                (_, Score) = SelectMinMax(Dif, Off, nDeep-1, !MinMax);
                // S += s;
                // return (0, Score, S);
                return (0, Score);
            }
            //分岐あり
            UInt64[] PList = new UInt64[NBranch];//配置ポインタ
            float[] ScoreList = new float[NBranch];//得点
            //並列処理
            for(int ip = 0; ip<NBranch; ip++){
                PList[ip]=SelectBit(PBranch,ip);
            }
            Parallel.For(0,NBranch-1, i=>{
                ScoreList[i] = MinMaxSub(Off, Dif, PList[i], nDeep, MinMax);
            });
                // for(int ip=0; ip<NBranch; ip++){
                //     PList[ip]=SelectBit(PBranch,ip);//配置先を一つだけ選択
                //     (OffBuf, DifBuf) = Put(Off, Dif, PList[ip]);//配置後の盤面を取得
                //     // (_, ScoreList[ip],s) = SelectMinMax(DifBuf, OffBuf, nDeep-1, !MinMax);//再帰
                //     (_, ScoreList[ip]) = SelectMinMax(DifBuf, OffBuf, nDeep-1, !MinMax);//再帰
                //     // S += s;
                // }
            //最大（最小）選択
            // float MM = ScoreList.Take(NBranch).Max();
            Array.Sort(ScoreList, PList, 0, NBranch);

            // string s = "";
            // s = $"Deep:{nDeep}:{MinMax}";
            if(MinMax){
                // s = GetConsolPrint(Off, Dif, $"nDeep:{nDeep}");
                P = PList[NBranch-1];
                Score = ScoreList[NBranch-1];
            }else{
                // s = GetConsolPrint(Dif, Off, $"nDeep:{nDeep}");
                P = PList[0];
                Score = ScoreList[0];
            }
            // for(int i1=0; i1<NBranch; i1++){
            //     s += $"\r\n i1:{i1}, S={ScoreList[i1]}, P={PList[i1]},";
            // }
            // Debug.Log(s);
            // S = $"\r\n Select:{Score}:{P}{s}\r\n{S}";
        }
        // return (P, Score, S);
        return (P, Score);
    }
    //KKKKKKKKKKKKKKKKKKKK
    float MinMaxSub(UInt64 Off, UInt64 Dif, UInt64 P, int nDeep, bool MinMax){
        UInt64 OffBuf, DifBuf;
        float Score;
        (OffBuf, DifBuf) = Put(Off, Dif, P);//配置後の盤面を取得
        (_, Score) = SelectMinMax(DifBuf, OffBuf, nDeep-1, !MinMax);//再帰
        return Score;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void ChangeActivePlayer(){
        if(ActiveFirstPlayer){
            ActiveFirstPlayer = false;
            iMy = 1;    iOppo = 0;
        }else{
            ActiveFirstPlayer = true;
            iMy = 0;    iOppo = 1;
        }
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public void ConsolPrint(string s = "", int Select = -1){
        Debug.Log( GetConsolPrint(s, Select) );
    }
    //KKKKKKKKKKKKKKKKKKKK
    public string GetConsolPrint(UInt64 B = 0, UInt64 W = 0, string s = "", int Select = -1){
        string S = $"{s}\n\r";
        UInt64 P = 0x8000_0000_0000_0000;
        int N = 0;
        // ConsolBitPrint(StoneBW[0],"Black");
        // ConsolBitPrint(StoneBW[1],"White");
        // ConsolBitPrint(CanPut,"Putable");
        if(B == 0 && W == 0){
            B = StoneBW[0];
            W = StoneBW[1];
        }
        for(int iy = 0; iy<8; iy++){
            for(int ix = 0; ix<8; ix++){
                if( (B & P) > 0 ){
                    S += "國";//黒
                }else if( (W & P) > 0 ){
                    S += "口";//白
                }else if( (CanPut & P) > 0 ){
                    if(N<10){
                        S += $" {N}";
                    }else{
                        S += $"{N}";
                    }
                    N++;
                }else{
                    S += "＿";//完全な空白
                }
                P >>= 1;
            }
            S += "\n\r";
        }
        S += "\n\r" + "\n\r";
        return S;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public string GetConsolPrint(string s, int Select = -1){
        return GetConsolPrint(0, 0, s, -1);
    }
    //KKKKKKKKKKKKKKKKKKKK
    string GetConsolBitPrint(UInt64 aug, string s = ""){
        string S = $"{s}\n\r";
        UInt64 P = 0x8000_0000_0000_0000;
        for(int iy = 0; iy<8; iy++){
            for(int ix = 0; ix<8; ix++){
                if( (aug&P)>0 ){
                    S += "國";//1
                }else{
                    S += "口";//0
                }
                P >>= 1;
            }
            S += "\n\r";
        }
        // Debug.Log(S);
        return S;
    }
    //KKKKKKKKKKKKKKKKKKKK
    void ConsolBitPrint(UInt64 aug, string s = ""){
        Debug.Log($"{s}{GetConsolBitPrint(aug,s)}");
    }
    //KKKKKKKKKKKKKKKKKKKK
    int CountBit(UInt64 aug){
        aug = (aug & 0x5555_5555_5555_5555) + (aug>>1 & 0x5555_5555_5555_5555);
        aug = (aug & 0x3333_3333_3333_3333) + (aug>>2 & 0x3333_3333_3333_3333);
        aug = (aug & 0x0F0F_0F0F_0F0F_0F0F) + (aug>>4 & 0x0F0F_0F0F_0F0F_0F0F);
        aug = (aug & 0x00FF_00FF_00FF_00FF) + (aug>>8 & 0x00FF_00FF_00FF_00FF);
        aug = (aug & 0x0000_FFFF_0000_FFFF) + (aug>>16 & 0x0000_FFFF_0000_FFFF);
        aug = (aug & 0x0000_0000_FFFF_FFFF) + (aug>>32 & 0x0000_0000_FFFF_FFFF);
        return (int)aug;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public UInt64 SelectBit(UInt64 aug, int N){
        UInt64 P = 1;
        int n = 0;
        for(int i1=0; i1<64; i1++){
            if( (aug&P) > 0) {
                if( n==N ){
                    return P;
                }else{
                    n++;
                }
            }
            P <<= 1;
        }
        return 0;
    }
    //KKKKKKKKKKKKKKKKKKKK
    UInt64 Pointing64(int x, int y){
        int Offset = x + 8 * y;
        return Pointers[Offset];
    //KKKKKKKKKKKKKKKKKKKK
    }
}
