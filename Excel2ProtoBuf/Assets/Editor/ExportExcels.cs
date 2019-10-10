/**
 * Author: YinPeiQuan
 * Date  : 
**/
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Collections;
using System;
using OfficeOpenXml;
using System.Linq;

/// <summary>
/// excel转proto界面逻辑
/// </summary>
public class Excel2ProtoViewTool : BaseEditor
{
    [MenuItem("Tools/导表工具")]
    static void Open()
    {
        GetWindow<Excel2ProtoViewTool>("导表工具");
        SVN_Path = PlayerPrefs.GetString(Excel2ProtoTool.ExcelPathKey);
        HotFixCfgPath = PlayerPrefs.GetString(Excel2ProtoTool.HotFixConfigPathKey);
        ResetConfig();
    }

    /// <summary>
    /// 配置表路径
    /// </summary>
    private static string SVN_Path;

    /// <summary>
    /// 热更cs放置目录
    /// </summary>
    private static string HotFixCfgPath;

    private static string ExportBytesPath;

    /// <summary>
    /// 已经导过的配置表（已经导出bytes）
    /// </summary>
    static List<string> HasExportExcelList = new List<string>();

    /// <summary>
    /// 所有配置表
    /// </summary>
    static List<string> AllExcelList = new List<string>();

    /// <summary>
    /// 新的配置表（没导出bytes）
    /// </summary>
    static List<string> NewExcelList = new List<string>();

    /// <summary>
    /// 程序员模式
    /// </summary>
    static bool admin;

    /// <summary>
    /// 是都检测当前打开excel
    /// </summary>
    static bool CheckExcel;

    public override void OnGUI()
    {
        base.OnGUI();
        GUILayout.BeginHorizontal();
        SetAccess();
        SetCheckState();
        GUILayout.EndHorizontal();
        RefreshInfo();
        SetPath();
        RefreshConfig();
        ShowInfo();
        ShowOtherConfig();
    }

    public override void OnEnable()
    {
        SVN_Path = PlayerPrefs.GetString(Excel2ProtoTool.ExcelPathKey);
        HotFixCfgPath = PlayerPrefs.GetString(Excel2ProtoTool.HotFixConfigPathKey);
        ExportBytesPath = PlayerPrefs.GetString(Excel2ProtoTool.ExportBytesPathKey);
        HasExportExcelList.Clear();

        AllExcelList.Clear();
        NewExcelList.Clear();
        BuildConfigIndexObject();
    }

    /// <summary>
    /// 设置权限
    /// </summary>
    static void SetAccess()
    {
        admin = PlayerPrefs.HasKey("Progromer") ? PlayerPrefs.GetInt("Progromer") == 1 ? true : false : false;
        admin = GUILayout.Toggle(admin, "程序员");
        PlayerPrefs.SetInt("Progromer", admin ? 1 : 0);
    }

    /// <summary>
    /// 检测是否打开excel
    /// </summary>
    static void SetCheckState()
    {
        CheckExcel = PlayerPrefs.HasKey("Progromerexcel") ? PlayerPrefs.GetInt("Progromerexcel") == 1 ? true : false : true;
        CheckExcel = GUILayout.Toggle(CheckExcel, "是否检测excel是否打开");
        PlayerPrefs.SetInt("Progromerexcel", CheckExcel ? 1 : 0);
    }

    /// <summary>
    /// 注意
    /// </summary>
    void RefreshInfo()
    {
        if (!DrawHeader("注意事项"))
        {
            return;
        }
        BeginContents();
        ShowLabel("1.使用前关闭excel相关软件");
        ShowLabel("2.新表导出，有两步操作，首先“导出Proto”，导出后等编译完（右下角圈圈转完）再操作“导出表数据”");
        ShowLabel("3.旧表改了字段，首先“导出Proto”，导出后等编译完（右下角圈圈转完）再操作“导出表数据”");
        EndContents();
       
    }

    /// <summary>
    /// 刷新
    /// </summary>
    void RefreshConfig()
    {
        if (!DrawHeader("刷新配置表"))
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新", GUILayout.Width(50), GUILayout.Height(50)))
            {
                ResetConfig();
            }
            GUILayout.EndHorizontal();
        }
    }

    static void ResetConfig()
    {
        HasExportExcelList.Clear();
        AllExcelList.Clear();
        NewExcelList.Clear();
        BuildConfigIndexObject();
    }

    /// <summary>
    /// 设置路径
    /// </summary>
    private void SetPath()
    {
        if (!DrawHeader("设置配置表路径（必选）"))
        {

        }
        BeginContents();
        GUILayout.BeginHorizontal();
        TextField("路径",ref SVN_Path);
        if (GUILayout.Button("设置"))
        {
            SVN_Path = EditorUtility.OpenFolderPanel("open res path", SVN_Path, "");
            if (!string.IsNullOrEmpty(SVN_Path))
            {
                PlayerPrefs.SetString(Excel2ProtoTool.ExcelPathKey, SVN_Path);
            }
            HasExportExcelList.Clear();
            AllExcelList.Clear();
            NewExcelList.Clear();
            BuildConfigIndexObject();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        TextField("导出CS路径", ref HotFixCfgPath);
        if (GUILayout.Button("设置"))
        {
            HotFixCfgPath = EditorUtility.OpenFolderPanel("open res path", HotFixCfgPath, "");
            if (!string.IsNullOrEmpty(HotFixCfgPath))
            {
                PlayerPrefs.SetString(Excel2ProtoTool.HotFixConfigPathKey, HotFixCfgPath);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        TextField("导出配置文件路径", ref ExportBytesPath);
        if (GUILayout.Button("设置"))
        {
            ExportBytesPath = EditorUtility.OpenFolderPanel("open res path", ExportBytesPath, "");
            if (!string.IsNullOrEmpty(ExportBytesPath))
            {
                PlayerPrefs.SetString(Excel2ProtoTool.ExportBytesPathKey, ExportBytesPath);
            }
        }
        GUILayout.EndHorizontal();
        EndContents();
    }

    static void BuildConfigIndexObject()
    {
        string serverConfigProtoPath = PlayerPrefs.GetString(Excel2ProtoTool.ExcelPathKey);
        if (string.IsNullOrEmpty(serverConfigProtoPath))
        {
            Debug.Log("没有设置服务器SNV目录");
            return;
        }

        DirectoryInfo BytesFileDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
        FileInfo[] bytesFile = BytesFileDirectoryInfo.GetFiles("*.bytes", SearchOption.TopDirectoryOnly);

        foreach (FileInfo fileInfo in bytesFile)
        {
            string str = fileInfo.Name.Replace(".bytes", ".xlsx");
            HasExportExcelList.Add(str);
        }

        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
        FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

        foreach (FileInfo fileInfo in serverFileInfos)
        {
            string str = fileInfo.Name;
            if (Excel2ProtoTool.ingoreList.Contains(str)) { continue; }
            AllExcelList.Add(str);
            if (!HasExportExcelList.Contains(str))
            {
                NewExcelList.Add(str);
            }
        }
    }

    void ShowInfo()
    {
        if (admin)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("一键导出proto"))
            {


                ExportVersionRes_ExcelSelect(AllExcelList);
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("一键导出表数据"))
            {
                if (CheckExcelIsOpen())
                {
                    return;
                }
                Excel2ProtoTool.ExportVersionRes_Excel();
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("一键导出表数据"))
            {
                if (CheckExcelIsOpen())
                {
                    return;
                }
                Excel2ProtoTool.ExportVersionRes_Excel();
            }
            GUILayout.EndHorizontal();
        }
    }

    void ShowOtherConfig()
    {
        if (!DrawHeader("所有表"))
        {
            return;
        }
        BeginScroll(0, false, true);
        foreach (string s in AllExcelList)
        {
            EditorGUILayout.BeginHorizontal();
            if(NewExcelList.Contains(s))
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("NEW ", GUILayout.Width(40));
                GUI.contentColor = Color.white;
            }

            GUILayout.Label(s, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();
        }
        EndScroll();
    }

    /// <summary>
    /// 导出多个excel proto,生成cs文件
    /// </summary>
    /// <param name="list"></param>
    static void ExportVersionRes_ExcelSelect(List<string> list)
    {
        if (CheckExcelIsOpen())
        {
            return;
        }
		Excel2ProtoTool.ExportProto(list);
		AssetDatabase.Refresh ();
        //将生成的proto导出到cs文件
        GenerateProtoCSfile.GenerateConfigProtoCSfile();
        AssetDatabase.Refresh();
        Excel2ProtoTool.CopyConfigProtoInExcelPath();

    }

    /// <summary>
    /// 检查excel是否打开
    /// </summary>
    /// <returns></returns>
    static bool CheckExcelIsOpen()
    {
        bool isopen = false;
        if(!CheckExcel)
        {
            return false;
        }
        foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
        {
            if (p!=null&&(p.ProcessName == "et" || p.ProcessName == "Excel" || p.ProcessName.Contains("Wps")))
            {
                EditorUtility.DisplayDialog("警告", "excel已经打开,请关闭再操作", "确定");
                isopen = true;
            }
        }
        return isopen;
    }
}

/// <summary>
/// excel转proto工具，主要逻辑在这里
/// </summary>
public class Excel2ProtoTool
{
    /// <summary>
    /// excel路径key
    /// </summary>
    public static string ExcelPathKey
    {
        get
        {
            return Application.dataPath + "ExcelPathKey";
        }
    }

    /// <summary>
    /// excel路径
    /// </summary>
    public static string ExcelPath
    {
        get
        {
            if (PlayerPrefs.HasKey(ExcelPathKey))
            {
                return PlayerPrefs.GetString(ExcelPathKey);
            }
            return "";
        }
    }

    /// <summary>
    /// 放热更cs文件路径key
    /// </summary>
    public static string HotFixConfigPathKey
    {
        get
        {
            return Application.dataPath + "HotFixConfigPathKey";
        }
    }

    /// <summary>
    /// 放热更cs文件路径
    /// </summary>
    public static string HotFixConfigCSPath
    {
        get
        {
            if (PlayerPrefs.HasKey(HotFixConfigPathKey))
            {
                return PlayerPrefs.GetString(HotFixConfigPathKey);
            }
            return "";
        }
    }

    public static string ExportBytesPathKey
    {
        get
        {
            return Application.dataPath + "ExportBytesPathKey";
        }
    }

    /// <summary>
    /// 放热更cs文件路径
    /// </summary>
    public static string ExportBytesPath
    {
        get
        {
            if (PlayerPrefs.HasKey(ExportBytesPathKey))
            {
                return PlayerPrefs.GetString(ExportBytesPathKey);
            }
            return "";
        }
    }

    public static string ignorePath
    {
        get
        {
            return Application.dataPath + "/Editor/ExportExcel/ignore.txt";
        }
    }

    /// <summary>
    /// 就是导出cs的命名空间，可以自定义
    /// </summary>
    public const string PackageName = "com.SnoopyGame.proto";

    /// <summary>
    /// 导出CS文件临时路径（放在editor下，方便序列化数据）
    /// </summary>
    public const string scriptsProtoConfigProto = "Plugins/Proto";

    /// <summary>
    /// 忽略配置列表
    /// </summary>
    public static List<string> ingoreList = new List<string>();


    public static void ExportVersionRes_Excel()
    {
        //导出所有配置前端用
        ExportExcelConfigAll();
        //导出单个配置，可以给服务器用
        ExportExcelConfig();
    }

    static void ExportExcelConfigAll()
    {
        string m_ExcelPath = ExcelPath;
        if (string.IsNullOrEmpty(m_ExcelPath))
        {
            Debug.Log("没有设置服务器SNV目录");
            return;
        }

        #region 设置好需要导出数据的配置表相关信息（因为变量名是驼峰命名，所以提前存起来）
        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(m_ExcelPath);
        System.IO.FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);
        Dictionary<string, string> dicPath = new Dictionary<string, string>();
        Dictionary<string, string> classNameDic = new Dictionary<string, string>();
        foreach (System.IO.FileInfo fileInfo in serverFileInfos)
        {
            string name = fileInfo.Name; name = name.Replace(".xlsx", "");
            classNameDic[name] = name;
            dicPath[name] = m_ExcelPath + "/" + fileInfo.Name;
        }
        #endregion

        #region 实例化总表数据
        Type AllConfigType = SysUtil.GetTypeByName(PackageName + ".ConfigDatabase");
        if (AllConfigType == null)
        {
            return;
        }

        System.Object AllConfigData = Activator.CreateInstance(AllConfigType);
        #endregion

        #region 遍历总表属性，把对应的数据赋值到总表实例里面（通过属性设值）
        System.Reflection.PropertyInfo[] fields = AllConfigType.GetProperties();
        foreach (System.Reflection.PropertyInfo f in fields)
        {
            if (f.Name.EndsWith("data"))
            {
                var start = Time.realtimeSinceStartup;//System.DateTime.Now.Ticks;


                string name = f.Name.Substring(2, f.Name.Length - 2);
                name = name.Substring(0, f.Name.Length - 7);
                if (!dicPath.ContainsKey(name))
                {
                    continue;
                }
                string filePath = dicPath[name];

                System.Object tempData = CreateData(dicPath[name], classNameDic[name]);

                if (null == tempData) { continue; }

                f.SetValue(AllConfigData, tempData, null);
            }
        }
        #endregion

        System.IO.MemoryStream MstreamConfig = new System.IO.MemoryStream();
        ProtoBuf.Serializer.Serialize(MstreamConfig, AllConfigData);
        byte[] dataConfig = MstreamConfig.ToArray();
        var pathConfig = string.Format("{0}/{1}.bytes", ExportBytesPath, "ConfigDatabase");
        System.IO.FileStream FstreamConfig = System.IO.File.Create(pathConfig);
        System.IO.BinaryWriter bwConfig = new System.IO.BinaryWriter(FstreamConfig);
        bwConfig.Write(dataConfig);
        FstreamConfig.Close();
        bwConfig.Close();
        MstreamConfig.Close();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 每个表单独导出一份bytes，可以给后端用
    /// </summary>
    static void ExportExcelConfig()
    {
        string serverConfigProtoPath = PlayerPrefs.GetString(ExcelPathKey);
        if (string.IsNullOrEmpty(serverConfigProtoPath))
        {
            Debug.Log("没有设置服务器SNV目录");
            return;
        }

        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
        System.IO.FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

        foreach (System.IO.FileInfo fileInfo in serverFileInfos)
        {
            if (fileInfo.Name.StartsWith("~"))
                continue;

            var start = Time.realtimeSinceStartup;//System.DateTime.Now.Ticks;
            string fullName = fileInfo.Name;
            string name = fileInfo.Name.Replace(".xlsx", "");
            System.Object configObj = CreateData(serverConfigProtoPath + "/" + fullName, name);
            System.IO.MemoryStream MstreamConfig = new System.IO.MemoryStream();
            ProtoBuf.Serializer.Serialize(MstreamConfig, configObj);
            byte[] dataConfig = MstreamConfig.ToArray();
            var pathConfig = serverConfigProtoPath + "/" + name + ".bytes";

            System.IO.FileStream FstreamConfig = System.IO.File.Create(pathConfig);
            System.IO.BinaryWriter bwConfig = new System.IO.BinaryWriter(FstreamConfig);
            bwConfig.Write(dataConfig);
            FstreamConfig.Close();
            bwConfig.Close();
            MstreamConfig.Close();
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 根据excel路径和对应的类名来实例化数据
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    static System.Object CreateData(string filePath, string name)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        ExcelPackage excelReader = new ExcelPackage(stream);
        ExcelWorkbook result = excelReader.Workbook;
        ExcelWorksheet workSheet = result.Worksheets.First();

        //单个数据
        Type dataType = SysUtil.GetTypeByName(PackageName + "." + name + "Config");
        if (dataType == null)
        {
            Debug.LogError("type====" + name + "===is not find");
            return null;
        }

        //列表数据
        Type configType = SysUtil.GetTypeByName(PackageName + "." + name + "ConfigData");
        if (configType == null)
        {
            Debug.LogError("type=====" + name + "Config=======is not find");
            return null;
        }

        //获取列表变量
        System.Reflection.FieldInfo field = configType.GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy |
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.GetField);


        if (field == null)
        {
            Debug.LogError("field not find !!! ======" + configType.Name + "._config");
            return null;
        }

        #region 遍历整个excel表读取每一行数据（可以扩展列表，枚举，其他表数据，这里只列出基本数据类型）
        int columns = workSheet.Dimension.End.Column;
        int rows = workSheet.Dimension.End.Row;
        System.Reflection.PropertyInfo[] tmpFileds = dataType.GetProperties(System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        System.Object configObj = Activator.CreateInstance(configType);

        IList m_DataList = field.GetValue(configObj) as IList;
        for (int i = 0; i < rows; i++)
        {
            if (i > 2)
            {
                System.Object target = Activator.CreateInstance(dataType);
                if (columns > 0)
                {
                    string dd = workSheet.GetValue<string>(i + 1, 1);
                    if (string.IsNullOrEmpty(dd))
                    {
                        break;
                    }
                }

                for (int j = 0, FiledsIndex = 0; j < columns; j++)
                {
                    string kk = workSheet.GetValue<string>(i + 1, j + 1);

                    if (FiledsIndex >= tmpFileds.Length)
                    {
                        continue;
                    }

                    TypeCode tpy = Type.GetTypeCode(tmpFileds[FiledsIndex].PropertyType);

                    string value = workSheet.GetValue<string>(i + 1, j + 1);
                    if (string.IsNullOrEmpty(value))
                    {
                        value = "";
                    }
                    value = value.TrimEnd(' ');
                    if (!tmpFileds[FiledsIndex].CanWrite)
                    {
                        continue;
                    }
                    switch (tpy)
                    {
                        case TypeCode.Int32:

                            if (kk != null)
                            {

                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                try
                                {
                                    tmpFileds[FiledsIndex].SetValue(target, Int32.Parse(value), null);
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogError(ex.ToString());
                                    Debug.LogError(string.Format("Data error: {0} : {2}:[{1}] is not int", name, workSheet.GetValue<string>(i + 1, j + 1), tmpFileds[j].Name));

                                    string key = workSheet.GetValue<string>(i + 1, 1);
                                    int keyValue;
                                    if (int.TryParse(key, out keyValue))
                                    {
                                        Debug.LogError("上条错误对应的ID：" + keyValue);
                                    }
                                }

                            }
                            else
                            {
                                tmpFileds[FiledsIndex].SetValue(target, 0, null);
                            }

                            break;

                        case TypeCode.String:
                            if (kk != null)
                            {
                                tmpFileds[FiledsIndex].SetValue(target, workSheet.GetValue<string>(i + 1, j + 1), null);
                            }
                            else
                            {
                                tmpFileds[FiledsIndex].SetValue(target, "", null);
                            }
                            break;

                        case TypeCode.Single:
                            if (kk != null)
                            {
                                try
                                {

                                    if (string.IsNullOrEmpty(value))
                                    {
                                        value = "0";
                                    }
                                    tmpFileds[FiledsIndex].SetValue(target, float.Parse(value), null);
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogError(ex.ToString());
                                    Debug.LogError(string.Format("Data error: {0} : {2}:[{1}] is not float", name, workSheet.GetValue<string>(i + 1, j + 1), tmpFileds[j].Name));
                                }

                            }
                            else
                            {
                                tmpFileds[FiledsIndex].SetValue(target, 0, null);
                            }

                            break;
                        case TypeCode.Boolean:
                            tmpFileds[FiledsIndex].SetValue(target, workSheet.GetValue<string>(i + 1, j + 1), null);
                            break;
                        default:
                            break;
                    }

                    FiledsIndex++;
                }

                m_DataList.Add(target);

            }
        }
        #endregion

        #region 校验数据
        stream.Close();
        #endregion
        return configObj;
    }

    /// <summary>
    /// 导出proto文件
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="selectList"></param>
    public static void ExportProto(List<string> selectList = null)
    {
        string serverConfigProtoPath = PlayerPrefs.GetString(ExcelPathKey);
        if (string.IsNullOrEmpty(serverConfigProtoPath))
        {
            Debug.Log("没有设置服务器SNV目录");
            return;
        }

        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
        System.IO.FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);

        //可导配置列表
        List<string> excelNames = new List<string>();
        //可导配置相关信息（用于创建初始化代码）
        List<CreateConfigScrptData> scriptDataList = new List<CreateConfigScrptData>();

        #region 遍历所有配置导出单个proto文件
        foreach (System.IO.FileInfo fileInfo in serverFileInfos)
        {
            if (fileInfo.Name.StartsWith("~"))
                continue;

            if (Excel2ProtoTool.ingoreList.Contains(fileInfo.Name)) { continue; }

            string name = fileInfo.Name.Replace(".xlsx", "");

            if (!fileInfo.Name.EndsWith(".xlsx"))
            {
                Debug.Log("跳过文件  " + name + " 因为它不是一个表文件");
                continue;
            }
            //单个配置相关数据
            CreateConfigScrptData scriptData = new CreateConfigScrptData();
            scriptDataList.Add(scriptData);
            scriptData.excelName = name;
            excelNames.Add(name);

            if (selectList != null && !selectList.Contains(fileInfo.Name))
            {
                continue;
            }

            Debug.Log("build Proto:" + name);

            var start = Time.realtimeSinceStartup;

            FileStream stream = File.Open(serverConfigProtoPath + "/" + fileInfo.Name, FileMode.Open, FileAccess.Read);

            ExcelPackage excelReader = new ExcelPackage(stream);
            ExcelWorkbook result = excelReader.Workbook;
            ExcelWorksheet workSheet = result.Worksheets.First();
            int columns = workSheet.Dimension.End.Column;
            int rows = workSheet.Dimension.End.Row;

            if (rows < 3)
            {
                Debug.LogError("选择的excel表行数小于4");
                return;
            }
            string SaveValue = "package " + PackageName + ";\n\n";
            SaveValue += "message " + name + "Config{\n";

            for (int j = 0; j <= columns; j++)
            {
                if(j == 0) { continue; }
                if (string.IsNullOrEmpty(workSheet.GetValue<string>(1, j)))
                {
                    continue;
                }
                string valueName = workSheet.GetValue<string>(3, j);
                if (string.IsNullOrEmpty(valueName))
                {
                    Debug.LogError(fileInfo.Name + "第" + (j + 1) + "列变量名为空");
                    return;
                }
                valueName = valueName.TrimEnd(' ');
                string explain = workSheet.GetValue<string>(1, j);
                string type = workSheet.GetValue<string>(2, j);
                //保存第一个字段的类型以及变量名
                if (j == 1)
                {
                    scriptData.VariableName = valueName;
                    scriptData.TypeName = type;
                }
                if (type == "int")
                    type = "int32";
                else if (type == "long")
                    type = "int64";
                SaveValue += "\trequired " + type + " " + valueName + " = " + (j + 1) + ";\t\t//" + explain + "\n";
            }

            SaveValue += "}\n\n";

            SaveValue += "message " + name + "ConfigData{\n";
            SaveValue += "\trepeated " + name + "Config config = 1;\n";
            SaveValue += "}\n";

            System.IO.FileStream Fstream = System.IO.File.Create(Application.dataPath + "/" + scriptsProtoConfigProto + "/" + name + ".proto");
            char[] data = SaveValue.ToCharArray();
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(Fstream);
            bw.Write(data);
            bw.Close();
            Fstream.Close();

            var end = Time.realtimeSinceStartup;
            Debug.LogFormat("Build,Start:{0}s end:{1}s duration:{2}s name:{3}", start, end, end - start, name);
        }
        #endregion

        #region 创建一个总表proto文件
        string protoNewValue = "package " + PackageName +  ";\r\n\r\n";
        foreach (string str in excelNames)
        {
            protoNewValue += " import \"" + str + ".proto\";\r\n";
        }
        protoNewValue += "\r\nmessage ConfigDatabase {\r\n";
        for (int i = 0; i < excelNames.Count; i++)
        {
            protoNewValue += "\trequired " + excelNames[i] + "ConfigData m_" + excelNames[i] + "_data = " + (i + 1) + ";\r\n";
        }
        protoNewValue += "}\r\n";

        System.IO.FileStream Fstream2 = System.IO.File.Create(Application.dataPath + "/" + scriptsProtoConfigProto + "/ConfigDatabaseFile.proto");
        char[] data2 = protoNewValue.ToCharArray();
        System.IO.BinaryWriter bw2 = new System.IO.BinaryWriter(Fstream2);
        bw2.Write(data2);
        bw2.Close();
        Fstream2.Close();
        #endregion

        #region 自动补齐初始化代码
        CreateConfigScrpt(scriptDataList);
        #endregion
    }

    public static void CreateConfigScrpt(List<CreateConfigScrptData> scriptDataList)
    {
        string temp = "";
        temp += "using System.Collections.Generic;\n";
        temp += "using " + PackageName + ";\n";
        temp += "public class GetConfig{ \n";
        temp += "private ConfigDatabase AllConfig;\n";
        temp += "public void InitConfig(ConfigDatabase data){\n";
        temp += "AllConfig = data;\n";
        temp += "}\n";
        for (int i = 0; i < scriptDataList.Count; i++)
        {
            CreateConfigScrptData tempData = scriptDataList[i];
            string dicname = tempData.excelName + "Dic";
            string dataListName = "AllConfig.m_" + tempData.excelName + "_data.config";
            string camelcaseStr = tempData.excelName;
            string configName = tempData.excelName + "Config";
            temp += "\n";
            temp += string.Format("private Dictionary<{0}, {1}Config> {2};\n", tempData.TypeName, tempData.excelName, dicname);
            temp += string.Format("public {0}Config Get{1}({2} id)\n", tempData.excelName, camelcaseStr, tempData.TypeName);
            temp += "{\n";
            temp += string.Format("if(null == {0})", dicname);
            temp += "{";
            temp += string.Format("{0} = new Dictionary<{1}, {2}Config>();", dicname, tempData.TypeName, tempData.excelName);
            temp += string.Format(" foreach({0} oneConfig in {1})  {2}[oneConfig.{3}] = oneConfig;", configName, dataListName, dicname, tempData.VariableName);
            temp += "}\n";
            temp += string.Format("if({0}.ContainsKey(id))  return {1}[id]; return null;", dicname, dicname);
            temp += "\n}\n";
            temp += string.Format("public List<{0}> Get{1}List()", configName, camelcaseStr);
            temp += "{";
            temp += string.Format("return {0};", dataListName);
            temp += "}\n";
        }
        temp += "}";
        string filePath = HotFixConfigCSPath + "/GetConfig.cs";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
        //获得字节数组
        byte[] wiritedata = System.Text.Encoding.Default.GetBytes(temp);
        //开始写入
        fs.Write(wiritedata, 0, wiritedata.Length);
        //清空缓冲区、关闭流
        fs.Flush();
        fs.Close();
    }

    public static void CopyConfigProtoInExcelPath()
    {
        string serverConfigProtoPath = PlayerPrefs.GetString(ExcelPathKey);
        if (string.IsNullOrEmpty(serverConfigProtoPath))
        {
            return;
        }


        DirectoryInfo serverDirectoryInfo = new DirectoryInfo(serverConfigProtoPath);
        System.IO.FileInfo[] serverFileInfos = serverDirectoryInfo.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
        foreach (System.IO.FileInfo fi in serverFileInfos)
        {
            if (fi.Name.EndsWith(".proto"))
            {
                fi.Delete();
            }
        }

        string[] allExcelFiles = System.IO.Directory.GetFiles(serverConfigProtoPath, "*.xlsx", SearchOption.AllDirectories);
        List<string> protoNames = new List<string>();
        foreach (string str in allExcelFiles)
        {
            if (str.StartsWith("~"))
                continue;

            string temp = str.Remove(0, serverConfigProtoPath.Length + 1);
            temp = temp.Remove(temp.Length - 5, 5);
            temp += ".proto";
            protoNames.Add(temp);
        }

        string unityConfigProtoPath = Application.dataPath + "/" + scriptsProtoConfigProto;
        DirectoryInfo unityDirectoryInfo = new DirectoryInfo(unityConfigProtoPath);
        System.IO.FileInfo[] unityFileInfos = unityDirectoryInfo.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
        foreach (System.IO.FileInfo fi in unityFileInfos)
        {
            if (protoNames.Contains(fi.Name) || fi.Name == "ConfigDatabaseFile.proto")
            {
                fi.CopyTo(serverConfigProtoPath + "/" + fi.Name);
            }
        }
    }
}

/// <summary>
/// 脚本数据
/// </summary>
public class CreateConfigScrptData
{
    /// <summary>
    /// excel名
    /// </summary>
    public string excelName;
    /// <summary>
    /// 类型名
    /// </summary>
    public string TypeName;
    /// <summary>
    /// 变量名
    /// </summary>
    public string VariableName;
}
