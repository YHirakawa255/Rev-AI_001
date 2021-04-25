using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

public class PointerCls{
    public int x, y, d;
    public int Color;
    public double e, eMax, eMin;
    public int pNow = 0;
    public int[] Pointer = new int[128];
    //KKKKKKKKKKKKKKKKKKKK
    public void InitPointer(){
        x=-1;
        y=-1;
        e=0;
        eMax=0;
        eMin=0;
        for(int i1=0; i1<Pointer.Length; i1++){
            Pointer[i1] = -1;
        }
    }
}
