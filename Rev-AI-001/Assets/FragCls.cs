using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagCls{
    //KKKKKKKKKKKKKKKKKKKK<<Start>>
    public bool FPlayerBattle = false;//プレイヤー対戦モード（False: 強化学習）
    public bool FSelectMode = true;//強化学習とプレイヤー対戦の選択待ち状態
    public bool FBitBoadCheck = false;//Bit Boad の動作チェック
    public int FMode = 0;// 0:モード選択, 1:プレイヤーと対戦, 2:学習, 3:ビットボードの動作チェック（削除予定）
    //KKKKKKKKKKKKKKKKKKKK<<OPTIMIZATION>>
    public bool FOptSet;
    public bool FOptPost;
    public bool FOptUpdate;
    public bool FOptRoundRobinBase;
    public bool FOptRoundRobinDB;
    public bool FOptRoundRobinDW;
    //KKKKKKKKKKKKKKKKKKKK<<VS Game>>
    public bool FSelectBW = true;//BW選択状態
    public bool FPlayerBlack = true;//人間が黒のときtrue
    public bool FSelectAI = true;//AI選択状態
    public int BattleAI;//選択AI
    public bool FFinGame = false;
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    public FlagCls(){//オーバーロード
        InitFlag();
    }
    //KKKKKKKKKKKKKKKKKKKK
    public bool SetFOptRoundRobinBase(){
        AllReset();
        FOptRoundRobinBase = true;
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public void InitFlag(){
        SetFOptRoundRobinBase();
        FMode = 0;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public bool SetFOptSet(){
        AllReset();
        FOptSet = true;
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    // public bool SetFOptPre(){
    //     AllReset();
    //     FOptPre = true;
    //     return true;
    // }
    //KKKKKKKKKKKKKKKKKKKK
    public bool SetFOptPost(){
        AllReset();
        FOptPost = true;
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public bool SetFOptUpdate(){
        AllReset();
        FOptUpdate = true;
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public bool SetFOptRoundRobinDB(){
        AllReset();
        FOptRoundRobinDB = true;
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public bool SetFOptRoundRobinDW(){
        AllReset();
        FOptRoundRobinDW = true;
        return true;
    }
    //KKKKKKKKKKKKKKKKKKKK
    void AllReset(){
        FOptSet = false;
        // FOptPre = false;
        // FOptRondomRobin = false;
        FOptPost = false;
        FOptUpdate = false;
        FOptRoundRobinBase = false;
        FOptRoundRobinDB = false;
        FOptRoundRobinDW = false;
    }
    //KKKKKKKKKKKKKKKKKKKK
}
