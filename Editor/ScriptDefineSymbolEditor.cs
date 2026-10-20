using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScriptDefineSymbolEditor : MonoBehaviour
{
#if !USE_AB
    [MenuItem("RebornFrame/框架宏/[OFF] USE_AB", false, 1000)]    
    public static void MenuUSE_ABDefineSymbols()
    {
        AddSymbol("USE_AB");
    }
#else
    [MenuItem("RebornFrame/框架宏/[ON] USE_AB", false, 1000)]
    public static void MenuUSE_ABDefineSymbols()
    {
        RemoveSymbol("USE_AB");
    }
#endif
    
#if !SIMULATE_PHONE
    [MenuItem("RebornFrame/框架宏/[OFF] SIMULATE_PHONE", false, 1000)]    
    public static void MenuSIMULATE_PHONEDefineSymbols()
    {
        AddSymbol("SIMULATE_PHONE");
    }
#else
    [MenuItem("RebornFrame/框架宏/[ON] SIMULATE_PHONE", false, 1000)]
    public static void MenuSIMULATE_PHONEDefineSymbols()
    {
        RemoveSymbol("SIMULATE_PHONE");
    }
#endif
        
#if !LOGIC_LOG
    [MenuItem("RebornFrame/框架宏/[OFF] LOGIC_LOG", false, 1000)]    
    public static void MenuSLOGIC_LOGDefineSymbols()
    {
        AddSymbol("LOGIC_LOG");
    }
#else
    [MenuItem("RebornFrame/框架宏/[ON] LOGIC_LOG", false, 1000)]
    public static void MenuLOGIC_LOGDefineSymbols()
    {
        RemoveSymbol("LOGIC_LOG");
    }
#endif
    
        
#if !REBORN_FS
    [MenuItem("RebornFrame/框架宏/[OFF] REBORN_FS", false, 1000)]    
    public static void MenuSREBORN_FSDefineSymbols()
    {
        AddSymbol("REBORN_FS");
    }
#else
    [MenuItem("RebornFrame/框架宏/[ON] REBORN_FS", false, 1000)]
    public static void MenuREBORN_FSDefineSymbols()
    {
        RemoveSymbol("REBORN_FS");
    }
#endif
    
    
    public static void AddSymbol(string symbol)
    {
        BuildTargetGroup buildTargetGroup =
            BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        // 动态获取 Android 平台的宏定义
        string tmpSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (string.IsNullOrEmpty(tmpSymbols))
        {
            tmpSymbols += symbol;
        }
        else
        {
            tmpSymbols += $";{symbol}";
        }

        // 动态修改宏定义
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, tmpSymbols);
    }
    
    public static void RemoveSymbol( string symbo)
    {
        
        BuildTargetGroup buildTargetGroup =
            BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        
        // 动态获取 Android 平台的宏定义
        string tmpSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        tmpSymbols = tmpSymbols.Replace($";{symbo}", "").Replace($"{symbo};", "").Replace(symbo, "");
        
        // 动态修改宏定义
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, tmpSymbols);
    }
}