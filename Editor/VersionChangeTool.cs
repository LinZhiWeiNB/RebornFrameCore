using UnityEditor;

namespace RebornFrame.Core
{
    public class VersionChangeTool
    {
        static string[] versions = new string[] { "DEVELOPMENT", "RELEASE_TEST", "RELEASE" };


#if !DEVELOPMENT
        [MenuItem("RebornFrame/环境宏/[OFF] DEVELOPMENT", false, 1000)]
        public static void Version_DEVELOPMENT()
        {
            ChangeVersion("DEVELOPMENT");
        }
#else
    [MenuItem("RebornFrame/环境宏/[ON] DEVELOPMENT", false, 1000)]
    public static void Version_DEVELOPMENT()
    {
        ChangeVersion("");
    }
#endif

#if !RELEASE_TEST
        [MenuItem("RebornFrame/环境宏/[OFF] RELEASE_TEST", false, 1000)]
        public static void Version_RELEASE_TEST()
        {
            ChangeVersion("RELEASE_TEST");
        }
#else
    [MenuItem("RebornFrame/环境宏/[ON] RELEASE_TEST", false, 1000)]
    public static void Version_RELEASE_TEST()
    {
        ChangeVersion("");
    }
#endif

#if !RELEASE
        [MenuItem("RebornFrame/环境宏/[OFF] RELEASE", false, 1000)]
        public static void Version_RELEASE()
        {
            ChangeVersion("RELEASE");
        }
#else
    [MenuItem("RebornFrame/环境宏/[ON] RELEASE", false, 1000)]
    public static void Version_RELEASE()
    {
        ChangeVersion("");
    }
#endif


        private static void ChangeVersion(string version)
        {
            BuildTargetGroup buildTargetGroup =
                BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            // 动态获取 Android 平台的宏定义
            string tmpSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // 删除所有版本宏定义
            if (!string.IsNullOrEmpty(tmpSymbols))
            {
                foreach (var v in versions)
                {
                    tmpSymbols = tmpSymbols.Replace($";{v}", "").Replace($"{v};", "").Replace(v, "");
                }
            }

            if (version != "")
            {
                if (string.IsNullOrEmpty(tmpSymbols))
                {
                    tmpSymbols += version;
                }
                else
                {
                    tmpSymbols += $";{version}";
                }
            }


            // 动态修改宏定义
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, tmpSymbols);
        }
    }

}