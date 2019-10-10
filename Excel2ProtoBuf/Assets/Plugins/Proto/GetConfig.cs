using System.Collections.Generic;
using com.SnoopyGame.proto;
public class GetConfig{ 
private ConfigDatabase AllConfig;
public void InitConfig(ConfigDatabase data){
AllConfig = data;
}

private Dictionary<int, buffConfig> buffDic;
public buffConfig Getbuff(int id)
{
if(null == buffDic){buffDic = new Dictionary<int, buffConfig>(); foreach(buffConfig oneConfig in AllConfig.m_buff_data.config)  buffDic[oneConfig.id] = oneConfig;}
if(buffDic.ContainsKey(id))  return buffDic[id]; return null;
}
public List<buffConfig> GetbuffList(){return AllConfig.m_buff_data.config;}
}