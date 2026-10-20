#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using RebornFrame.Core;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataConfigManager
{
   #region AsyncResourcePool

   private static Dictionary<string, ResourceRequest> AsyncResourcePool = new Dictionary<string, ResourceRequest>();

   
   
   
   
   #endregion
   
   public static byte[] LoadData(string fileName)
   {
#if !USE_AB
      var txt = Resources.Load<TextAsset>("Configs/" + fileName);
      if (txt == null)
         return new byte[0];
      return txt.bytes;
      
#elif UNITY_EDITOR && !SIMULATE_PHONE

         var path = "Assets/Arts/Configs/" + fileName + ".bytes";
         if(File.Exists(path))
            return AssetDatabase.LoadAssetAtPath<TextAsset>(path).bytes;
         else 
            return new byte[0];
#else
         var path = fileName;
         return LoadAssets<TextAsset>("ABPath://configs/" + path,"").bytes;
#endif
    
   }

   public static Sprite LoadAnimationSprite(string s)
   {
      return LoadAssets<Sprite>(s,".png");
   } 
   public static DataLoadRequest<Sprite> LoadAnimationSpriteAsync(string s)
   {
      if (s.StartsWith("Resources://"))
      {
         var filePath = s.Replace("Resources://", "");
         if (AsyncResourcePool.ContainsKey(filePath))
         {
            return new DataLoadRequest<Sprite>(AsyncResourcePool[filePath]);
         }
         else
         {
            var resourceRequest = Resources.LoadAsync<Sprite>(filePath);
            AsyncResourcePool.Add(filePath,resourceRequest);
            return new DataLoadRequest<Sprite>(resourceRequest);
         }
      }
      else
      {
         #if UNITY_EDITOR && !SIMULATE_PHONE
         return new DataLoadRequest<Sprite>(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Arts/" + s.Replace("ABPath://", "") + ".png"));
         #else
         // var infos = path.Split('/');
         // return new DataLoadRequest<Sprite>(ABPackageManager.LoadAssetsFromABPathAsync<Sprite>(infos[0], $"{infos[1]}.{infos[2]}.{infos[3]}".ToLower(),
         //    infos[4]));
         var infos = s.Split('/');
         var assetName = infos[infos.Length - 1];
         var path = s.Replace("ABPath://", "");
         // return ABPackageManager.LoadAssetsFromABPath<Sprite>(infos[0], $"{infos[1]}.{infos[2]}.{infos[3]}".ToLower(),
         //    infos[4]);


         return new DataLoadRequest<Sprite>(
            ABPackageManager.Instance.LoadAssetsFromABPathAsync<Sprite>(path + ".png"));
#endif
      }
   }
   
   public static T LoadAssets<T>(string path, string extends) where T : Object
   {
      if (path.StartsWith("Resources://"))
      {
         return Resources.Load<T>(path.Replace("Resources://", ""));
      }
      else
      {
#if UNITY_EDITOR && !SIMULATE_PHONE
         return AssetDatabase.LoadAssetAtPath<T>("Assets/Arts/" + path.Replace("ABPath://", "") + extends);
#else
         path = path.Replace("ABPath://", "Arts/");
         // return ABPackageManager.LoadAssetsFromABPath<Sprite>(infos[0], $"{infos[1]}.{infos[2]}.{infos[3]}".ToLower(),
         //    infos[4]);
         
#if UNITY_EDITOR
         var asset =  ABPackageManager.Instance.LoadAssetsFromABPath<T>(path + extends);
         Renderer[] renderers = null;
         if (asset is GameObject gameObject)
         {
            renderers = gameObject.GetComponentsInChildren<Renderer>();
         }else if (asset is Transform transform)
         {
            renderers = transform.GetComponentsInChildren<Renderer>();
         }
         
         List<Material> materials = new List<Material>();

         if (renderers != null)
         {
            for (var i = 0; i < renderers.Length; i++)
            {
               materials.AddRange(renderers[i].sharedMaterials);
            }

            for (var i = 0; i < materials.Count; i++)
            {
                if(materials[i] == null || materials[i].shader == null || string.IsNullOrEmpty(materials[i].shader.name))
                  continue;
               
               materials[i].shader = Shader.Find(materials[i].shader.name);
            }
         }
         return asset;
#else
         return ABPackageManager.Instance.LoadAssetsFromABPath<T>(path + extends);
#endif
#endif
      }
   }
   
   public static DataLoadRequest<T> LoadAssetsAsync<T>(string path, string extends) where T : Object
   {
      
      if (path.StartsWith("Resources://"))
      {
         var filePath = path.Replace("Resources://", "");
         if (AsyncResourcePool.ContainsKey(filePath))
         {
            return new DataLoadRequest<T>(AsyncResourcePool[filePath]);
         }
         else
         {
            var resourceRequest = Resources.LoadAsync<T>(filePath);
            AsyncResourcePool.Add(filePath,resourceRequest);
            return new DataLoadRequest<T>(resourceRequest);
         }
      }
      else
      {
#if UNITY_EDITOR && !SIMULATE_PHONE
         return new DataLoadRequest<T>(AssetDatabase.LoadAssetAtPath<T>("Assets/Arts/" + path.Replace("ABPath://", "") + extends));
#else
         var infos = path.Split('/');
         var assetName = infos[infos.Length - 1];
         path = path.Replace("ABPath://", "Arts/");
         return new DataLoadRequest<T>(ABPackageManager.Instance.LoadAssetsFromABPathAsync<T>(path + extends));
#endif
      }
   }
}

public class DataLoadRequest<T> where T : Object
{
   private ResourceRequest request;
   private Object _asset;
   private Action<T> _OnComplete;
   public DataLoadRequest(ResourceRequest request)
   {
      this.request = request;
      request.completed += RequestOncompleted;
   }

   private void RequestOncompleted(AsyncOperation obj)
   {
      _OnComplete?.Invoke(asset);
   }

   public DataLoadRequest(Object asset)
   {
      this.request = null;
      _asset = asset;
   }


   public bool isDone => request == null || request.isDone;
   public float progress => request?.progress ?? 1;
   public T asset => (T)(request == null ? _asset : request.asset);

   public void OnComplete(Action<T> onComplete)
   {
      if (isDone)
      {
         onComplete?.Invoke(asset);
      }
      else
      {
         _OnComplete += onComplete;
      }
   }
}