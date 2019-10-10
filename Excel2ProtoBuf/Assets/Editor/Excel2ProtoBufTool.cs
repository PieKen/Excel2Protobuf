///**
// * Author: YinPeiQuan
// * Date  : 
//**/
//using UnityEngine;
//using System.Collections.Generic;
//using UnityEditor;
//using System.IO;
//using System.Collections;
//using System;
//using Excel;

//public class Excel2ProtoBufTool : Excel2ProtoBuf
//{
//    [MenuItem("Tools/导表工具")]
//    static void Open()
//    {
//        GetWindow<Excel2ProtoBufTool>("导表工具");
//        ServerConfigProtoSVNPathKey = Application.dataPath + "ServerConfigProtoSVNPath";
//        SVN_Path = PlayerPrefs.GetString(ServerConfigProtoSVNPathKey);
//        ResetConfig();
//    }

//    /// <summary>
//    /// 配置表路径
//    /// </summary>
//    private static string SVN_Path;

//    /// <summary>
//    /// 已经导过数据列表
//    /// </summary>
//    static List<string> protoFileList = new List<string>();

//    /// <summary>
//    /// 已经导过的表列表
//    /// </summary>
//    static List<string> FileInfoNames = new List<string>();

//    /// <summary>
//    /// 当前查找的结果列表
//    /// </summary>
//    static List<string> searchList = new List<string>();

//    /// <summary>
//    /// 新的配置表（没有导出过数据）
//    /// </summary>
//    static List<string> newConfigList = new List<string>();

//    /// <summary>
//    /// 是否在搜索状态
//    /// </summary>
//    static bool m_Search = false;

//    /// <summary>
//    /// 当前搜索的字符串
//    /// </summary>
//    static string m_CurrentSearchString = "";

//    /// <summary>
//    /// 是否程序员模式
//    /// </summary>
//    static bool admin;

//    /// <summary>
//    /// 是否检查excel是否打开
//    /// </summary>
//    static bool CheckExcel;

//    public override void OnGUI()
//    {
//        base.OnGUI();
//        GUILayout.BeginHorizontal();
//        SetJob();
//        SetCheckState();
//        GUILayout.EndHorizontal();
//        RefreshInfo();
//        SetPath();
//        RefreshConfig();
//        ShowNewConfig();
//        ShowOtherConfig();
//        if (FileInfoNames.Count == 0)
//        {
//            BuildConfigIndexObject();
//        }
//    }

//    public override void OnEnable()
//    {
//        ServerConfigProtoSVNPathKey = Application.dataPath + "ServerConfigProtoSVNPath";
//        SVN_Path = PlayerPrefs.GetString(ServerConfigProtoSVNPathKey);
//        protoFileList.Clear();

//        FileInfoNames.Clear();
//        searchList.Clear();
//        newConfigList.Clear();
//        BuildConfigIndexObject();
//        m_Search = false;
//    }

//    static void SetJob()
//    {
//        admin = PlayerPrefs.HasKey("Progromer") ? PlayerPrefs.GetInt("Progromer") == 1 ? true : false : false;
//        admin = GUILayout.Toggle(admin, "程序员");
//        PlayerPrefs.SetInt("Progromer", admin ? 1 : 0);
//    }

//    static void SetCheckState()
//    {
//        CheckExcel = PlayerPrefs.HasKey("Progromerexcel") ? PlayerPrefs.GetInt("Progromerexcel") == 1 ? true : false : true;
//        CheckExcel = GUILayout.Toggle(CheckExcel, "是否检测excel是否打开");
//        PlayerPrefs.SetInt("Progromerexcel", CheckExcel ? 1 : 0);
//    }

//    void RefreshInfo()
//    {
//        if (!DrawHeader("注意事项"))
//        {
//            return;
//        }
//        BeginContents();
//        ShowLabel("1.使用前关闭excel相关软件");
//        ShowLabel("2.新表导出，有两步操作，首先“导出Proto”，导出后等编译完（右下角圈圈转完）再操作“导出表数据”");
//        ShowLabel("3.旧表改了字段，首先“导出Proto”，导出后等编译完（右下角圈圈转完）再操作“导出表数据”");
//        EndContents();
        
//    }

//    void RefreshConfig()
//    {
//        if (!DrawHeader("刷新配置表"))
//        {

//        }
//        GUILayout.BeginHorizontal();
//        if (GUILayout.Button("刷新", GUILayout.Width(50),GUILayout.Height(50)))
//        {
//            ResetConfig();
//        }
//        GUILayout.EndHorizontal();
//    }

//    static void ResetConfig()
//    {
//        protoFileList.Clear();
//        FileInfoNames.Clear();
//        searchList.Clear();
//        newConfigList.Clear();
//        BuildConfigIndexObject();
//        m_Search = false;
//    }

//    private void SetPath()
//    {
//        if (!DrawHeader("设置配置表路径（必选）"))
//        {

//        }
//        BeginContents();
//        GUILayout.BeginHorizontal();
//        TextField("路径",ref SVN_Path);
//        if (GUILayout.Button("设置"))
//        {
//            SVN_Path = EditorUtility.OpenFolderPanel("open res path", SVN_Path, "");
//            if (!string.IsNullOrEmpty(SVN_Path))
//            {
//                PlayerPrefs.SetString(ServerConfigProtoSVNPathKey, SVN_Path);
//            }
//            protoFileList.Clear();
//            FileInfoNames.Clear();
//            searchList.Clear();
//            newConfigList.Clear();
//            BuildConfigIndexObject();
//        }
//        EndContents();
//        GUILayout.EndHorizontal();
//    }

//    static void BuildConfigIndexObject()
//    {
//        string serverConfigProtoPath = PlayerPrefs.GetString(ServerConfigProtoSVNPathKey);
//        if (string.IsNullOrEmpty(serverConfigProtoPath))
//        {
//            Debug.Log("没有设置服务器SNV目录");
//            return;
//        }

//        DirectoryInfo BytesFileDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
//        FileInfo[] bytesFile = BytesFileDirectoryInfo.GetFiles("*.bytes", SearchOption.TopDirectoryOnly);

//        foreach (FileInfo fileInfo in bytesFile)
//        {
//            string str = fileInfo.Name.Replace(".bytes", ".xlsx");
//            protoFileList.Add(str);
//        }

//        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
//        FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

//        foreach (FileInfo fileInfo in serverFileInfos)
//        {
//            string str = fileInfo.Name;
//            if (ingoreList.Contains(str)) { continue; }
//            if(!protoFileList.Contains(str))
//            {
//                if(!newConfigList.Contains(str))
//                newConfigList.Add(str);
//            }
//            else
//            {
//                FileInfoNames.Add(str);
//            }
//        }
//    }

//    /// <summary>
//    /// 搜索表格
//    /// </summary>
//    static void searchAction()
//    {
//        GUILayout.BeginHorizontal();
//        GUILayout.Label("搜索表：", GUILayout.Width(100));
//        string pattern = EditorGUILayout.TextField(string.Empty, m_CurrentSearchString, "SearchTextField");
//        if (GUILayout.Button(string.Empty, "SearchCancelButton"))
//        {
//            pattern = string.Empty;
//            GUIUtility.keyboardControl = 0;
//            m_Search = false;
//        }
//        if(pattern != m_CurrentSearchString)
//        {
//            m_Search = true;
//            Search(pattern);
//        }
//        GUILayout.EndHorizontal();
//    }

//    void ShowNewConfig()
//    {
//        if (!DrawHeader("新表")) { }
//        if (newConfigList.Count == 0) { GUILayout.Label("没有新的配表"); return; }
//        GUILayout.Label("统一处理（有两步操作，先导Proto,等编译完再导出表）");
//        GUILayout.BeginHorizontal();
//        if(admin)
//        {
//            if (GUILayout.Button("一键导出Proto"))
//            {
//                ExportVersionRes_ExcelSelect(newConfigList);
//                AssetDatabase.Refresh();
//            }
//        }
//        if(GUILayout.Button("一键导出表"))
//        {
//			ExportVersionRes_Excel();
//        }
//        GUILayout.EndHorizontal();

//        GUILayout.Label("单独处理（有两步操作，先导Proto,等编译完再导出表）");
//        BeginContents();
//        for (int i=0;i<newConfigList.Count;i++)
//        {
//            string s = newConfigList[i];
//            GUILayout.BeginHorizontal();
//            if(admin)
//            {
//                if (GUILayout.Button("导出proto", GUILayout.Width(70), GUILayout.Height(20)))
//                {
//                    List<string> list = new List<string>();
//                    list.Add(s);
//                    ExportVersionRes_ExcelSelect(list);
//                    AssetDatabase.Refresh();
//                }
//            }
//            if (GUILayout.Button("导出表", GUILayout.Width(70), GUILayout.Height(20)))
//            {
//                ExportCnfigData(s);
//                AssetDatabase.Refresh();
//            }
//            GUILayout.Label(s, GUILayout.Width(250));
//            GUILayout.EndHorizontal();
//        }
//        EndContents();
//    }

//    void ShowOtherConfig()
//    {
//        if (!DrawHeader("已添加的表"))
//        {
//            return;
//        }
//        if(admin)
//        {
//            GUILayout.BeginHorizontal();
//            if (GUILayout.Button("一键导出proto"))
//            {
//                ExportVersionRes_ExcelSelect(FileInfoNames);
//                AssetDatabase.Refresh();
//            }
//            if (GUILayout.Button("一键导出表数据"))
//            {
//                if (CheckExcelIsOpen())
//                {
//                    return;
//                }
//                ExportVersionRes_Excel();
//            }
//            GUILayout.EndHorizontal();
//        }
//        else
//        {
//            GUILayout.BeginHorizontal();
//            if (GUILayout.Button("一键导出表数据"))
//            {
//                if (CheckExcelIsOpen())
//                {
//                    return;
//                }
//                ExportVersionRes_Excel();
//            }
//            GUILayout.EndHorizontal();
//        }

//        searchAction();

//        BeginScroll(0);
//        if (m_Search)
//        {
//            foreach (string s in searchList)
//            {
//                GUILayout.Space(7f);
//                EditorGUILayout.BeginHorizontal();
//                if(admin)
//                {
//                    if (GUILayout.Button("导出proto",GUILayout.Width(70), GUILayout.Height(20)))
//                    {
//                        List<string> list = new List<string>();
//                        list.Add(s);
//                        ExportVersionRes_ExcelSelect(list);
//                        AssetDatabase.Refresh();
//                    }
//                }
//                if (GUILayout.Button("导出表数据", GUILayout.Width(70), GUILayout.Height(20)))
//                {
//                    ExportCnfigData(s);

//                    AssetDatabase.Refresh();
//                }
//                GUILayout.Label(s, GUILayout.Width(150));
//                EditorGUILayout.EndHorizontal();
//            }
//        }
//        else
//        {
//            foreach (string s in FileInfoNames)
//            {
//                EditorGUILayout.BeginHorizontal();
//                if(admin)
//                {
//                    if (GUILayout.Button("导出proto", GUILayout.Width(70), GUILayout.Height(20)))
//                    {
//                        List<string> list = new List<string>();
//                        list.Add(s);
//                        ExportVersionRes_ExcelSelect(list);
//                        AssetDatabase.Refresh();
//                    }
//                }
//                if (GUILayout.Button("导出表数据",GUILayout.Width(70), GUILayout.Height(20)))
//                {
//                    ExportCnfigData(s);
//                    AssetDatabase.Refresh();
//                }
//                GUILayout.Label(s, GUILayout.Width(250));
//                EditorGUILayout.EndHorizontal();
//            }

//        }
//        EndScroll();
//    }

//    /// <summary>
//    /// 查找功能
//    /// </summary>
//    /// <param name="fileName"></param>
//    static void Search(string fileName)
//    {
//        searchList.Clear();
//        foreach(string s in FileInfoNames)
//        {
//            if (s.ToLower().Contains(fileName.ToLower()))
//            {
//                searchList.Add(s);
//            }
//        }
//    }

//    /// <summary>
//    /// 导出多个excel proto,生成cs文件
//    /// </summary>
//    /// <param name="list"></param>
//    static void ExportVersionRes_ExcelSelect(List<string> list)
//    {
//        if (CheckExcelIsOpen())
//        {
//            return;
//        }
//        //导出excel proto文件
//		ExportProto(list);
//        //根据proto文件生成cs文件
//		GenerateProtoCSfile.GenerateConfigProtoCSfile ();
//		AssetDatabase.Refresh ();
//        CopyConfigProtoInSVNPath();
//    }

//    /// <summary>
//    /// 导出单个表数据
//    /// </summary>
//    /// <param name="str"></param>
//    public static void ExportCnfigData(string str)
//    {
//        if(CheckExcelIsOpen())
//        {
//            return;
//        }
//        List<string> selectList = new List<string>();
//        selectList.Add(str);
//        //把表数据序列化byte文件
//		ExportExcelConfig (selectList);
//    }

//    /// <summary>
//    /// 检查excel是否打开
//    /// </summary>
//    /// <returns></returns>
//    static bool CheckExcelIsOpen()
//    {
//        bool isopen = false;
//        if(!CheckExcel)
//        {
//            return false;
//        }
//        foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
//        {
//            if (p!=null&&(p.ProcessName == "et" || p.ProcessName == "Excel" || p.ProcessName.Contains("Wps")))
//            {
//                EditorUtility.DisplayDialog("警告", "excel已经打开,请关闭再操作", "确定");
//                isopen = true;
//            }
//        }
//        return isopen;
//    }
//}

//public class Excel2ProtoBuf : BaseEditor
//{
//    public static string ServerConfigProtoSVNPathKey;

//    /// <summary>
//    /// proto导出路径
//    /// </summary>
//    public static string scriptsProtoConfigProto { get { return Application.dataPath + "/Plugins/Proto/ConfigProto"; } }

//    /// <summary>
//    /// bytes导出路径
//    /// </summary>
//    public static string exportBytesPath { get { return Application.dataPath + "/Resoueces/Config/"; } }

//    /// <summary>
//    /// 忽略配置
//    /// </summary>
//    public static List<string> ingoreList = new List<string>();

//    /// <summary>
//    /// proto名字
//    /// </summary>
//    public const string packageName = "com.SnoopyGame.proto";

//    public static void ExportVersionRes_Excel()
//    {
//        ExportExcelConfig();
//    }

//    public static void ExportExcelConfig(List<string> selectList = null)
//    {
//        string serverConfigProtoPath = PlayerPrefs.GetString(ServerConfigProtoSVNPathKey);
//        if (string.IsNullOrEmpty(serverConfigProtoPath))
//        {
//            Debug.Log("没有设置服务器SNV目录");
//            return;
//        }

//        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
//        System.IO.FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

//        foreach (System.IO.FileInfo fileInfo in serverFileInfos)
//        {
//            if (fileInfo.Name.StartsWith("~"))
//                continue;

//            var start = Time.realtimeSinceStartup;


//            string name = fileInfo.Name;
//            if (selectList != null && !selectList.Contains(name))
//            {
//                continue;
//            }

//            FileStream stream = File.Open(serverConfigProtoPath + "/" + name, FileMode.Open, FileAccess.Read);
//            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
//            System.Data.DataSet result = null;

//            try
//            {
//                result = excelReader.AsDataSet();
//            }
//            catch (InvalidCastException e)
//            {
//                Debug.LogError("读表" + name + "出错，这通常是因为这张表里面不只有一个Sheet有数据" + e.Message);
//                continue;
//            }

//            name = name.Replace(".xlsx", "");

//            Type dataType = SysUtil.GetTypeByName(packageName + "." + name + "Config");
//            if (dataType == null)
//            {
//                Debug.LogError("type====" + name + "===is not find");
//                continue;
//            }

//            Type configType = SysUtil.GetTypeByName(packageName + "." + name + "ConfigData");
//            if (configType == null)
//            {
//                Debug.LogError("type=====" + name + "Config=======is not find");
//                continue;
//            }

//            System.Reflection.FieldInfo field = configType.GetField("_config",
//                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy |
//                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance |
//                System.Reflection.BindingFlags.GetField
//                );
//            if (field == null)
//            {
//                Debug.LogError("field not find !!! ======" + configType.Name + "._config");
//                continue;
//            }

//            int columns = result.Tables[0].Columns.Count;
//            int rows = result.Tables[0].Rows.Count;
//            System.Reflection.PropertyInfo[] tmpFileds = dataType.GetProperties(System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

//            System.Object configObj = Activator.CreateInstance(configType);

//            IList m_DataList = field.GetValue(configObj) as IList;

//            for (int i = 0; i < rows; i++)
//            {
//                if (i > 2)
//                {
//                    System.Object target = Activator.CreateInstance(dataType);
//                    if (columns > 0)
//                    {
//                        string dd = result.Tables[0].Rows[i][0].ToString();
//                        if (string.IsNullOrEmpty(dd))
//                        {
//                            continue;
//                        }
//                    }
//                    for (int j = 0, FiledsIndex = 0; j < columns; j++)
//                    {
//                        string kk = result.Tables[0].Rows[i][j].ToString();

//                        if (FiledsIndex >= tmpFileds.Length)
//                        {
//                            continue;
//                        }

//                        TypeCode tpy = Type.GetTypeCode(tmpFileds[FiledsIndex].PropertyType);

//                        string value = result.Tables[0].Rows[i][j].ToString();
//                        value = value.TrimEnd(' ');
//                        if (!tmpFileds[FiledsIndex].CanWrite)
//                        {
//                            continue;
//                        }
//                        switch (tpy)
//                        {
//                            case TypeCode.Int32:

//                                if (kk != null)
//                                {

//                                    if (string.IsNullOrEmpty(value))
//                                    {
//                                        value = "0";
//                                    }
//                                    try
//                                    {
//                                        tmpFileds[FiledsIndex].SetValue(target, Int32.Parse(value), null);
//                                    }
//                                    catch (System.Exception ex)
//                                    {
//                                        Debug.LogError(ex.ToString());
//                                        Debug.LogError(string.Format("Data error: {0} : {2}:[{1}] is not int", name, result.Tables[0].Rows[i][j].ToString(), tmpFileds[j].Name));

//                                        string key = result.Tables[0].Rows[i][0].ToString();
//                                        int keyValue;
//                                        if (int.TryParse(key, out keyValue))
//                                        {
//                                            Debug.LogError("上条错误对应的ID：" + keyValue);
//                                        }
//                                    }

//                                }
//                                else
//                                {
//                                    tmpFileds[FiledsIndex].SetValue(target, 0, null);
//                                }

//                                break;

//                            case TypeCode.String:
//                                if (kk != null)
//                                {
//                                    tmpFileds[FiledsIndex].SetValue(target, result.Tables[0].Rows[i][j].ToString(), null);
//                                }
//                                else
//                                {
//                                    tmpFileds[FiledsIndex].SetValue(target, "", null);
//                                }
//                                break;

//                            case TypeCode.Single:
//                                if (kk != null)
//                                {
//                                    try
//                                    {
//                                        if (string.IsNullOrEmpty(value))
//                                        {
//                                            value = "0";
//                                        }
//                                        tmpFileds[FiledsIndex].SetValue(target, float.Parse(value), null);
//                                    }
//                                    catch (System.Exception ex)
//                                    {
//                                        Debug.LogError(ex.ToString());
//                                        Debug.LogError(string.Format("Data error: {0} : {2}:[{1}] is not float", name, result.Tables[0].Rows[i][j].ToString(), tmpFileds[j].Name));
//                                    }

//                                }
//                                else
//                                {
//                                    tmpFileds[FiledsIndex].SetValue(target, 0, null);
//                                }

//                                break;
//                            case TypeCode.Boolean:
//                                tmpFileds[FiledsIndex].SetValue(target, result.Tables[0].Rows[i][j], null);
//                                break;
//                            default:
//                                break;
//                        }

//                        FiledsIndex++;
//                    }

//                    m_DataList.Add(target);

//                }
//            }

//            System.IO.MemoryStream MstreamConfig = new System.IO.MemoryStream();
//            ProtoBuf.Serializer.Serialize(MstreamConfig, configObj);
//            byte[] dataConfig = MstreamConfig.ToArray();
//            if (!Directory.Exists(exportBytesPath))
//            {
//                Directory.CreateDirectory(exportBytesPath);
//            }
//            var pathConfig = exportBytesPath + name + ".bytes";
//            System.IO.FileStream FstreamConfig = System.IO.File.Create(pathConfig);
//            System.IO.BinaryWriter bwConfig = new System.IO.BinaryWriter(FstreamConfig);
//            bwConfig.Write(dataConfig);
//            FstreamConfig.Close();
//            bwConfig.Close();
//            MstreamConfig.Close();

//            // 拷贝到svn
//            System.IO.FileInfo fiConfig = new System.IO.FileInfo(pathConfig);
//            fiConfig.CopyTo(serverConfigProtoPath + "/" + fiConfig.Name, true);
//            AssetDatabase.Refresh();
//            var end = Time.realtimeSinceStartup;
//            Debug.LogFormat("Build,Start:{0}s end:{1}s duration:{2}s name:{3}", start, end, end - start, name);
//        }
//        AssetDatabase.Refresh();
//    }

//    public static void ExportProtoWithExcel()
//    {
//        Debug.Log("ExportProtoWithExcel");
//        ExportProto();
//        AssetDatabase.Refresh();
//        //将生成的proto导出到cs文件
//        GenerateProtoCSfile.GenerateConfigProtoCSfile();
//        AssetDatabase.Refresh();
//        //将生成的protp复制到服务器SVN目录
//        CopyConfigProtoInSVNPath();
//    }

//    /// <summary>
//    /// 导出proto文件
//    /// </summary>
//    /// <param name="platform"></param>
//    /// <param name="selectList"></param>
//    public static void ExportProto(List<string> selectList = null)
//    {
//        string serverConfigProtoPath = PlayerPrefs.GetString(ServerConfigProtoSVNPathKey);
//        if (string.IsNullOrEmpty(serverConfigProtoPath))
//        {
//            Debug.Log("没有设置服务器SNV目录");
//            return;
//        }

//        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
//        System.IO.FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

//        List<string> excelNames = new List<string>();
//        foreach (System.IO.FileInfo fileInfo in serverFileInfos)
//        {
//            if (fileInfo.Name.StartsWith("~"))
//                continue;

//            if (Excel2ProtoBuf.ingoreList.Contains(fileInfo.Name)) { continue; }

//            string name = fileInfo.Name.Replace(".xlsx", "");

//            if (!fileInfo.Name.EndsWith(".xlsx"))
//            {
//                Debug.Log("跳过文件  " + name + " 因为它不是一个表文件");
//                continue;
//            }

//            excelNames.Add(name);

//            if (selectList != null && !selectList.Contains(fileInfo.Name))
//            {
//                continue;
//            }

//            Debug.Log("build Proto:" + name);

//            var start = Time.realtimeSinceStartup;

//            FileStream stream = File.Open(serverConfigProtoPath + "/" + fileInfo.Name, FileMode.Open, FileAccess.Read);
//            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

//            System.Data.DataSet result = excelReader.AsDataSet();

//            int columns = result.Tables[0].Columns.Count;
//            int rows = result.Tables[0].Rows.Count;

//            if (rows < 3)
//            {
//                Debug.LogError("选择的excel表行数小于4");
//                return;
//            }

//            string SaveValue = "package " + packageName + ";\n\n";
//            SaveValue += "message " + name + "Config{\n";

//            for (int j = 0; j < columns; j++)
//            {
//                if (string.IsNullOrEmpty(result.Tables[0].Rows[1][j].ToString()))
//                {
//                    continue;
//                }
//                string valueName = result.Tables[0].Rows[2][j].ToString();
//                if(string.IsNullOrEmpty(valueName))
//                {
//                    Debug.LogError(fileInfo.Name + "第" + (j+1) + "列变量名为空");
//                    return;
//                }
//                valueName = valueName.TrimEnd(' ');
//                string explain = result.Tables[0].Rows[0][j].ToString();
//                string type = result.Tables[0].Rows[1][j].ToString();
//                if (type == "int")
//                    type = "int32";
//                else if (type == "long")
//                    type = "int64";

//                SaveValue += "\trequired " + type + " " + valueName + " = " + (j + 1) + ";\t\t//" + explain + "\n";
//            }

//            SaveValue += "}\n\n";

//            SaveValue += "message " + name + "ConfigData{\n";
//            SaveValue += "\trepeated " + name + "Config config = 1;\n";
//            SaveValue += "}\n";
            
//            if(!Directory.Exists(scriptsProtoConfigProto))
//            {
//                Directory.CreateDirectory(scriptsProtoConfigProto);
//            }
//            System.IO.FileStream Fstream = System.IO.File.Create(scriptsProtoConfigProto + "/" + name + ".proto");
//            char[] data = SaveValue.ToCharArray();
//            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(Fstream);
//            bw.Write(data);
//            bw.Close();
//            Fstream.Close();

//            var end = Time.realtimeSinceStartup;//System.DateTime.Now.Ticks;
//            Debug.LogFormat("Build,Start:{0}s end:{1}s duration:{2}s name:{3}", start, end, end - start, name);
//        }

//        //重新生成configDatabaseFile
//        string protoNewValue = "package " + packageName +  ";\r\n\r\n";
//        foreach (string str in excelNames)
//        {
//            protoNewValue += " import \"" + str + ".proto\";\r\n";
//        }
//        protoNewValue += "\r\nmessage ConfigDatabase {\r\n";
//        for (int i = 0; i < excelNames.Count; i++)
//        {
//            protoNewValue += "\trequired " + excelNames[i] + "ConfigData m_" + excelNames[i] + "_data = " + (i + 1) + ";\r\n";
//        }
//        protoNewValue += "}\r\n";

//        System.IO.FileStream Fstream2 = System.IO.File.Create(scriptsProtoConfigProto + "/ConfigDatabaseFile.proto");
//        char[] data2 = protoNewValue.ToCharArray();
//        System.IO.BinaryWriter bw2 = new System.IO.BinaryWriter(Fstream2);
//        bw2.Write(data2);
//        bw2.Close();
//        Fstream2.Close();

//        Debug.Log("配置表导出proto完毕");
//    }

//    private static void CreateGetConfigScript(List<string> excelNames)
//    {

//    }

//    public static void CopyConfigProtoInSVNPath()
//    {
//        string serverConfigProtoPath = PlayerPrefs.GetString(ServerConfigProtoSVNPathKey);
//        if (string.IsNullOrEmpty(serverConfigProtoPath)) { return; }

//        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
//        System.IO.FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
//        foreach (System.IO.FileInfo fi in serverFileInfos)
//        {
//            if (fi.Name.EndsWith(".proto"))
//            {
//                fi.Delete();
//            }
//        }

//        string[] allExcelFiles = System.IO.Directory.GetFiles(serverConfigProtoPath, "*.xlsx", SearchOption.AllDirectories);
//        List<string> protoNames = new List<string>();
//        foreach (string str in allExcelFiles)
//        {
//            if (str.StartsWith("~"))
//                continue;

//            string temp = str.Remove(0, serverConfigProtoPath.Length + 1);
//            temp = temp.Remove(temp.Length - 5, 5);
//            temp += ".proto";
//            protoNames.Add(temp);
//        }

//        DirectoryInfo unityDirectoryInfo = new DirectoryInfo(scriptsProtoConfigProto);
//        System.IO.FileInfo[] unityFileInfos = unityDirectoryInfo.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
//        foreach (System.IO.FileInfo fi in unityFileInfos)
//        {
//            if (protoNames.Contains(fi.Name) || fi.Name == "ConfigDatabaseFile.proto")
//            {
//                fi.CopyTo(serverConfigProtoPath + "/" + fi.Name);
//            }
//        }
//    }
//}
