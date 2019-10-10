/**
 * 配置管理器
**/
using com.SnoopyGame.proto;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 配置管理器
/// </summary>
public class CfgMgr
{
    private static CfgMgr instance;

    public static CfgMgr GetIns()
    {
        if(null == instance)
        {
            instance = new CfgMgr();
        }
        return instance;
    }


    /// <summary>
    /// 初始化配置管理器
    /// </summary>
    public void Init()
    {

    }

    /// <summary>
    /// 多语言表
    /// </summary>
    private Dictionary<string, Dictionary<int, string>> contentDic = new Dictionary<string, Dictionary<int, string>>();

    #region 加载配置
    /// <summary>
    /// 读取所有表信息
    /// </summary>
    public void LoadAllConfig()
    {
        string resName = "Config/ConfigDatabase";
        TextAsset ta = Resources.Load<TextAsset>(resName);
        if (null != ta)
        {
            //初始化所有配置
            ConfigDatabase data = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(new MemoryStream(ta.bytes), null, typeof(ConfigDatabase)) as ConfigDatabase;
            mGetCfg = new GetConfig();
            mGetCfg.InitConfig(data);
        }
        LoadContent();
    }

    private GetConfig mGetCfg;

    public static GetConfig GetCfg()
    {
        return GetIns().mGetCfg;
    }

    /// <summary>
    /// 单独读取语言表
    /// </summary>
    public void LoadContent()
    {

    }
    #endregion

    #region 获取多语言接口
    public string GetContent(int id)
    {
        if (contentDic.ContainsKey("chinese"))
        {
            if (contentDic["chinese"].ContainsKey(id))
                return contentDic["chinese"][id];
        }
        return "";
    }

    #endregion

    #region 所有配置数据
    private ConfigDatabase _ConfigDataBase;

    public ConfigDatabase m_ConfigDataBase
    {
        get
        {
            return _ConfigDataBase;
        }
    }
    #endregion

    #region 单个类型配置对应的数据字典（用于查找数据）
    private Dictionary<System.Type, object> _ConfigDicDataDic = null;

    private Dictionary<System.Type, object> ConfigDicDataDic
    {
        get
        {
            if (_ConfigDicDataDic == null)
            {
                _ConfigDicDataDic = new Dictionary<System.Type, object>();
            }
            return _ConfigDicDataDic;
        }
    }
    #endregion
}