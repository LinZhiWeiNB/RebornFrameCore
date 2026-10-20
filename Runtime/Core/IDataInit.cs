using System;
using System.Reflection;
using RebornFrame.Core;
using RebornFrame.Utils;
using UnityEngine;

namespace RebornFrame.Core
{
    public abstract class IDataInit
    {
        public void InitConfigs()
        {
            Type[] binders = ReflectionUtils.GetTypesOfMethod(Assembly.GetAssembly(GetType()), "Init",
                BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < binders.Length; i++)
            {
                MethodInfo method = binders[i].GetMethod("InitDefine", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    method = binders[i].GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
                }

                method.Invoke(binders[i], null);
            }

            Debug.Log("配置文件加载完成");
        }

        public void BindAll()
        {
            Type[] binders = ReflectionUtils.GetTypesOfMethod(Assembly.GetAssembly(GetType()), "BindAll",
                BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < binders.Length; i++)
            {
                MethodInfo method = binders[i].GetMethod("BindAll", BindingFlags.Public | BindingFlags.Static);
                method.Invoke(binders[i], null);
            }

            Debug.Log("FGUI绑定完成");
        }

        public abstract void LoadLaunchUI(string sceneName);
        public abstract void Awake();
    }
}