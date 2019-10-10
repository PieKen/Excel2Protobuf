/**
 * 游戏工具类  跟业务逻辑没什么关系的
 * Author: Liyaowen
 * Date  : 2017-06-02 15:30:00
**/

using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;


public class SysUtil
{
    public static Type GetTypeByName(string str)
    {
        Type typ = Type.GetType(str + ",Assembly-CSharp-firstpass");
        if (typ == null)
        {
            typ = Type.GetType(str + ",Assembly-CSharp");
            if (typ == null)
            {
                Debug.LogError("not find!!! ");
                return null;
            }
        }
        return typ;
    }
}