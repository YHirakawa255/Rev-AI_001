using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUISC{
    int Select = 0;//現在の選択を表す
    int Selected = 0;
    int SelectMax = 0;//選択肢の最大値
    int PreSelect = -1;
    bool FSelectReternWait = true;
    public bool FPlayerBlack = false;
    //KKKKKKKKKKKKKKKKKKKK
    public (bool b, int n) SelectMode(){
        bool b = false;
        int n = -1;
        SelectMax = 2;
        LRSelecting();//入力受付
        if(SelectChangeAndValue(0)){
            Debug.Log( "  <<VS Player>> / Learning / Bit Boad\r\n Press Enter" );
        }else if(SelectChangeAndValue(1)){
            Debug.Log( "  VS Player / <<Learning>> / Bit Boad\r\n Press Enter" );
        }else if(SelectChangeAndValue(2)){
            Debug.Log( "  VS Player / Learning / <<Bit Boad>>\r\n Press Enter" );
        }
        if( PushReturn() ){//Enter押されたとき
            n = Select;
            SelectReset();
            b = true;
        }
        return (b, n);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public bool SelectBW(){
        SelectMax = 1;
        LRSelecting();//入力受付
        if(SelectChangeAndValue(0)){
            Debug.Log( "  <<Black>> / White \r\n Press Enter" );
        }else if(SelectChangeAndValue(1)){
            Debug.Log( "  Black / <<White>> \r\n Press Enter" );
        }
        if( PushReturn() ){//Enter押されたとき
            if(Select==0){
                FPlayerBlack = true;
            }else{
                FPlayerBlack = false;
            }
            Selected = Select;    
            SelectReset();
            return true;
        }
        return false;
    }
    //KKKKKKKKKKKKKKKKKKKK
    public (bool FSelect, int BattleAI) SelectAI(ref AgentCls[] aug){
        bool FSelect = false;
        int BattleAI = Selected;
        string s;
        SelectMax = aug.Length - 1;
        LRSelecting();//入力受付
        if(FPlayerBlack){
            s = "Player : <<Black>>\r\n";
        }else{
            s = "Player : <<White>>\r\n";
        }
        if( SelectChanged() ){
            for(int i1 = 0; i1<=SelectMax; i1++){
                if( SelectChangeAndValue(i1) ){
                    s += $"AI{i1} Strength = {aug[i1].GetStrength()} <<Select>>\r\n";
                }else{
                    s += $"AI{i1} Strength = {aug[i1].GetStrength()}\r\n";
                }
            }
            Debug.Log(s);
        }
        if( PushReturn() ){//Enter押されたとき
            Selected = Select;
            BattleAI = Select;
            SelectReset();
            FSelect = true;
        }
        return (FSelect, BattleAI);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public (bool Change, bool Decide, int S) SelectPut(int nPutList){
        //配置場所が変更になったらTrueを返す
        SelectMax = nPutList;
        bool Change = SelectChanged(true);
        Change |= LRSelecting();//入力受付（変更があるときtrue）
        bool Decide = PushReturn();
        int S = Select;
        if(Change){ Selected = Select;  }
        if(Decide){
            SelectReset();
        }
        return (Change, Decide, S);
    }
    //KKKKKKKKKKKKKKKKKKKK
    //KKKKKKKKKKKKKKKKKKKK
    bool LRSelecting(){
        bool r = false;
        r |= SelectUp( Input.GetKeyDown(KeyCode. RightArrow) );
        r |= SelectDonw( Input.GetKeyDown(KeyCode. LeftArrow) );
        return r;
    }
    //KKKKKKKKKKKKKKKKKKKK
    bool SelectUp(bool Key){//Selectの制御関数
        if(Key){
            if(SelectMax < ++Select){   Select = 0; }
            Debug.Log($"Up {Select}");
        }
        return Key;
    }
    //KKKKKKKKKKKKKKKKKKKK
    bool SelectDonw(bool Key){//Selectの制御関数
        if(Key){
            if(--Select < 0){   Select = SelectMax; }
            Debug.Log($"Down {Select}");
        }
        return Key;
    }
    //KKKKKKKKKKKKKKKKKKKK
    void SelectReset(int aug = 0){//Selectのリセット
        Select = aug;
        PreSelect = -1;
    }
    //KKKKKKKKKKKKKKKKKKKK
    bool SelectChanged(bool F = false){
        bool r = PreSelect != Select;
        if( F && r ){
            PreSelect = Select;
        }
        return r;
    }
    //KKKKKKKKKKKKKKKKKKKK
    bool SelectChangeAndValue(int aug){
        //Selectが変更になったとき　かつ　引数とSelectが同じ値のときTrueを返す
        if(PreSelect != Select){
            PreSelect = Select;
            FSelectReternWait = true;//結果の受け渡し待ち
        }
        if(FSelectReternWait && (aug==Select) ){
            //Select変更後の一回だけtrueを返す
            FSelectReternWait = false;
            return true;
        }
        return false;
    }
    //KKKKKKKKKKKKKKKKKKKK
    bool PushReturn(){
        return Input.GetKeyDown(KeyCode.Return);
    }
    //KKKKKKKKKKKKKKKKKKKK
    public int GetSelect(){
        return Selected;
    }
}
