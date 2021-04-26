using System.Collections;
using System.Collections.Generic;
using System;
// using UnityEngine;

public class DirCls{
    public int[] DirX = new int[]{-1,0,1,-1,1,-1,0,1};
    public int[] DirY = new int[]{-1,-1,-1,0,0,1,1,1};
}

public class DebugCls{
    public void Log(string s){
        Console.WriteLine(s);
    }
}

