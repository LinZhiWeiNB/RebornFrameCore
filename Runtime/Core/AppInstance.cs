using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


namespace RebornFrame.Core
{
    public class AppInstance : MonoBehaviour
    {
        private string unLoadSubDll = "HotUpdateSub";
        private string mainDll = "Assembly-CSharp";
        public Action FirstUpdateActions;
        public Action UpdateActions;
        public Action LateUpdateActions;
        public Action FixedUpdateActions;
        public Action AppQuitActions;
        public Action<bool> FocusActions;
        public UpdateLauncher CustomUpdateLauncher;

        [HideInInspector] public Camera LoadingCamera;
        
        public IDataInit CustomDataInit;
        private static AppInstance _Instance = null;
        [HideInInspector] public bool Inited = false;
        [HideInInspector] private bool IsFirstStart = false;
        [HideInInspector] private bool IsTypeAwaked = false;
        [HideInInspector] private bool IsPrepared = false;
        [HideInInspector] private List<IBeforeInit> beforeAwakes = new List<IBeforeInit>();
        [HideInInspector] private List<IBeforeInit> afterAwake = new List<IBeforeInit>();
        private string sceneName;
        private int scenePackageVersion = 0;
        private Dictionary<string,AssetBundle> sceneAssetBundles = new Dictionary<string, AssetBundle>();
        public static AppInstance Instance
        {
            get { return _Instance; }
        }

        void Awake()
        {
            sceneName = gameObject.scene.name;

            if (_Instance != null)
            {
                return;
            }
            
            var cameraGameObject = new GameObject();
            LoadingCamera = cameraGameObject.AddComponent<Camera>();
            //set background solid
            LoadingCamera.clearFlags = CameraClearFlags.SolidColor;
            LoadingCamera.backgroundColor = Color.black;
            LoadingCamera.cullingMask = 0;
            //set priority
            LoadingCamera.depth = 100;
            
            _Instance = this;
            IsFirstStart = true;
            
            var beforeInitType = typeof(IBeforeInit);
            var beforeInits = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => { return s.GetTypes(); }).Where(p =>
            {
                string name = p.Assembly.GetName().Name;
                return beforeInitType.IsAssignableFrom(p) && !name.Contains(unLoadSubDll);
            });
            foreach (var t in beforeInits)
            {
                if (t.Name == "IBeforeInit")
                    continue;
                IBeforeInit beforeInit = (IBeforeInit)Activator.CreateInstance(t);
                if (beforeInit.BeforeAwake)
                {
                    bool flag = true;
                    for (var i = 0; i < beforeAwakes.Count; i++)
                    {
                        if (beforeAwakes[i].Priority > beforeInit.Priority)
                        {
                            beforeAwakes.Insert(i, beforeInit);
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        beforeAwakes.Add(beforeInit);
                    }
                }
                else
                {
                    bool flag = true;
                    for (var i = 0; i < afterAwake.Count; i++)
                    {
                        if (afterAwake[i].Priority > beforeInit.Priority)
                        {
                            beforeAwakes.Insert(i, beforeInit);
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        afterAwake.Add(beforeInit);
                    }
                }
            }

            if (beforeAwakes.Count > 0)
            {
                beforeAwakes[0].Start();
            }

            LoadType();
        }

        private void Start()
        {
            if (Instance.Inited)
                Instance.CustomDataInit.LoadLaunchUI(sceneName);

            if (!IsFirstStart)
                Destroy(gameObject);
            else
                DontDestroyOnLoad(gameObject);
        }

        public void InitData(Assembly hotUpdateAssembly)
        {
            var type = typeof(IDataInit);
            var types = hotUpdateAssembly.GetTypes().Where(p => type.IsAssignableFrom(p));
            if (types.Count() < 1)
            {
                Debug.LogError("没有任何应用初始化的实现");
                return;
            }

            foreach (var t in types)
            {
                if (t.Name == "IDataInit")
                    continue;

                CustomDataInit = (IDataInit)Activator.CreateInstance(t);
                break;
            }

            Preared();
        }


        /// <summary>
        /// 加载启动类
        /// </summary>
        public void LoadType()
        {
            
#if SIMULATE_PHONE
            var type = typeof(UpdateLauncher);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => { return s.GetTypes(); }).Where(p =>
            {
                string name = p.Assembly.GetName().Name;
                return type.IsAssignableFrom(p) && !name.Contains(unLoadSubDll);
            });
            if (types.Count() < 2)
            {
                Debug.LogError("没有任何热更初始化的实现");
                return;
            }

            foreach (var t in types)
            {
                if (t.Name == "UpdateLauncher")
                    continue;

                CustomUpdateLauncher = (UpdateLauncher)Activator.CreateInstance(t);
                break;
            }
#else
            var type = typeof(IDataInit);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p =>
                {
                    string name = p.Assembly.GetName().Name;
                    return type.IsAssignableFrom(p) && !name.Contains(unLoadSubDll);
                });
            if (types.Count() < 2)
            {
                Debug.LogError("没有任何应用初始化的实现");
                return;
            }


            foreach (var t in types)
            {
                if (t.Name == "IDataInit")
                    continue;

                CustomDataInit = (IDataInit)Activator.CreateInstance(t);
                break;
            }
#endif

        }

        /// <summary>
        /// 开始加载启动类
        /// </summary>
        public void AwakeType()
        {
            //更新全局AB包
#if SIMULATE_PHONE
            CustomUpdateLauncher?.Awake();
            CustomUpdateLauncher?.PullUpdate();
#else
            CustomUpdateLauncher = new UpdateLauncher();
            Preared();
#endif
            
            IsTypeAwaked = true;
        }

        public void Preared()
        {
            CustomDataInit?.InitConfigs();
            CustomDataInit?.BindAll();
            IsPrepared = true;
            if (afterAwake.Count > 0)
            {
                afterAwake[0].Start();
            }
        }
        
        public void InitData()
        {
            CustomDataInit?.Awake();
            CustomDataInit?.LoadLaunchUI(sceneName);
            Inited = true;
            Destroy(LoadingCamera.gameObject);
            LoadingCamera = null;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Inited)
            {
                if (!IsTypeAwaked)
                {
                    if(beforeAwakes.Count == 0)
                    {
                        AwakeType();
                    }
                    else
                    {
                        beforeAwakes[0].Update();
                        if (beforeAwakes[0].IsFinished)
                        {
                            if (beforeAwakes.Count > 1)
                            {
                                beforeAwakes[1].Start();
                            }
                            beforeAwakes.RemoveAt(0);
                            if(beforeAwakes.Count == 0)
                            {
                                AwakeType();
                            }
                        }
                    }
                }
                
                if (IsPrepared)
                {
                    if(afterAwake.Count == 0)
                    {
                        InitData();
                    }
                    else
                    {
                        afterAwake[0].Update();
                        if (afterAwake[0].IsFinished)
                        {
                            if (afterAwake.Count > 1)
                            {
                                afterAwake[1].Start();
                            }
                            afterAwake.RemoveAt(0);
                            if(afterAwake.Count == 0)
                            {
                                InitData();
                            }
                        }
                    }
                }
            }
           
            
            
            FirstUpdateActions?.Invoke();

            UpdateActions?.Invoke();
        }

        void LateUpdate()
        {
            LateUpdateActions?.Invoke();
        }

        void FixedUpdate()
        {
            FixedUpdateActions?.Invoke();
        }

        public void DelayCall(float second, Action callback)
        {
            StartCoroutine(_DelayCall(second, callback));
        }


        IEnumerator _DelayCall(float second, Action callback)
        {
            if (callback == null)
                yield break;
            yield return new WaitForSeconds(second);
            callback();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            FocusActions?.Invoke(hasFocus);
        }

        private void OnApplicationQuit()
        {
            AppQuitActions?.Invoke();
        }

            public void LoadSceneInABPackage(string loadPackage, string sceneName,LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool overridePackage = false,Action onCompleted = null)
        {
            loadPackage = ABPackageManager.Instance.LoadABDependencies(loadPackage);
            
            AssetBundle sceneAssetBundle = null;
            if (sceneAssetBundles.ContainsKey(loadPackage))
                sceneAssetBundle = sceneAssetBundles[loadPackage];
            
            if (overridePackage || sceneAssetBundle == null)
            {
                scenePackageVersion++;
                
                if (sceneAssetBundle != null)
                {
                    sceneAssetBundle.Unload(true);
                    sceneAssetBundles.Remove(loadPackage);
                }
                
                StartCoroutine(_LoadSceneInABPackage(loadPackage, sceneName,loadSceneMode,scenePackageVersion,onCompleted));
            }
            else if (sceneAssetBundle != null) {
                _LoadScene(sceneName,loadSceneMode,onCompleted);
            }
        }
        
        private IEnumerator _LoadSceneInABPackage(string loadPackage, string sceneName,LoadSceneMode loadSceneMode,int version,Action onCompleted)
        {

#if UNITY_EDITOR
            var sceneURL= "file://" + CustomUpdateLauncher.GetABRootPath(loadPackage) + loadPackage;
            #else
            var dir = CustomUpdateLauncher.GetABRootPath(loadPackage);
            var sceneURL= (dir.StartsWith(Application.streamingAssetsPath)?"":"file://") + dir + loadPackage;
            #endif
            var bundle = UnityWebRequestAssetBundle.GetAssetBundle(sceneURL);
            // WWW bundleWWW = WWW.LoadFromCacheOrDownload(sceneURL, version);
            yield return bundle.SendWebRequest();
            
            if (bundle.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download AssetBundle: " + bundle.error);
                yield break;
            }

            // Get the AssetBundle
            var sceneAssetBundle = DownloadHandlerAssetBundle.GetContent(bundle);
 
            if (sceneAssetBundle.isStreamedSceneAssetBundle) {
                sceneAssetBundles.Add(loadPackage,sceneAssetBundle);
                _LoadScene(sceneName,loadSceneMode,onCompleted);
            }
            else
            {
                sceneAssetBundle.Unload(true);
            }
        }

        void _LoadScene(string sceneName,LoadSceneMode loadSceneMode,Action onCompleted)
        {
            SceneManager.LoadScene(sceneName,loadSceneMode);
            
#if UNITY_EDITOR
            StartCoroutine(_LoadSceneMats());
#endif
            onCompleted?.Invoke();
        }

        IEnumerator _LoadSceneMats()
        {
            yield return null;
            var gameobjects = FindObjectsOfType<Transform>();
            
            List<Renderer> renderers = new List<Renderer>();
            foreach (var go in gameobjects)
            {
                renderers.AddRange(go.GetComponentsInChildren<Renderer>());
            }
         
            List<Material> materials = new List<Material>();

            if (renderers != null)
            {
                for (var i = 0; i < renderers.Count; i++)
                {
                    materials.AddRange(renderers[i].sharedMaterials);
                    Debug.Log(renderers[i].gameObject.name);
                }

                for (var i = 0; i < materials.Count; i++)
                {
                    if(materials[i] == null || materials[i].shader == null || string.IsNullOrEmpty(materials[i].shader.name))
                        continue;
               
                    materials[i].shader = Shader.Find(materials[i].shader.name);
                    Debug.Log(materials[i].shader.name);
                }
            }
            
            
            Material currentSkybox = RenderSettings.skybox;
            if (currentSkybox != null && currentSkybox.shader != null &&
                !string.IsNullOrEmpty(currentSkybox.shader.name))
            {
                currentSkybox.shader = Shader.Find(currentSkybox.shader.name);
            }
        }
        
        
        public void ClearSceneAssetBundles()
        {
            foreach (var sceneAssetBundle in sceneAssetBundles)
            {
                sceneAssetBundle.Value.Unload(true);
            }
            sceneAssetBundles.Clear();
            scenePackageVersion = 0;
        }
    }
}

