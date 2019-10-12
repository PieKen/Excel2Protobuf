using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.SnoopyGame.proto;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {

        //只执行一次
        CfgMgr.GetIns().LoadAllConfig();

        //获取配置
        buffConfig cf = CfgMgr.GetCfg().Getbuff(1);
        Debug.LogError(cf.buff_name);

        testConfig tc = CfgMgr.GetCfg().Gettest(1);
        Debug.LogError(tc.buff_name);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
