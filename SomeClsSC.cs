using System.Collections;
using System.Collections.Generic;
using System;
// using UnityEngine;

//オセロの配置可能検定に用いる
public class DirCls{
    public int[] DirX = new int[]{-1,0,1,-1,1,-1,0,1};
    public int[] DirY = new int[]{-1,-1,-1,0,0,1,1,1};
}
//Unityのデバッグ用の出力関数と名前を合わせて、変更を最小限にとどめた
public class DebugCls{
    public void Log(string s){
        Console.WriteLine(s);
    }
}

