using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor;

namespace VeryFS.Framework.Editor.Resource
{

    public class RuleDisplayItem
    {
        public string caption;
        public string name;
    }
    
    public class RuleHelper<T>
    {
        private  readonly Dictionary<string, Type> _cacheRuleTypes = new ();
        
        private  readonly Dictionary<string, T> _cacheRuleInstance = new ();
        private readonly List<RuleDisplayItem> _cacheRuleDisplayItems = new();

        public  List<RuleDisplayItem> DisplayItems => _cacheRuleDisplayItems;
        
        public RuleHelper()
        {
            _cacheRuleTypes.Clear();
            _cacheRuleInstance.Clear();
            _cacheRuleDisplayItems.Clear();
                
            // 获取所有类型
            List<Type> types = new List<Type>(100);
            var customTypes = TypeCache.GetTypesDerivedFrom(typeof(T));
            foreach (var type in customTypes)
            {
                if (!type.IsAbstract && type.GetCustomAttribute<DisplayNameAttribute>()!=null)
                    types.Add(type);
            }
            //types.AddRange(customTypes);
            types.Sort((type1, type2) =>
            {
                var attribute1 = type1.GetCustomAttribute<DisplayNameAttribute>(false);
                var attribute2 = type2.GetCustomAttribute<DisplayNameAttribute>(false);
                int order1 = attribute1 != null ? attribute1.Order : 100;
                int order2 = attribute2 != null ? attribute2.Order : 100;
                return order1.CompareTo(order2);
            });

            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                _cacheRuleTypes.TryAdd(type.Name, type);
                
                _cacheRuleDisplayItems.Add(
                    new RuleDisplayItem()
                    {
                        caption =  GetRuleDisplayName(type),
                        name = type.Name
                    });
            }
        }

        public RuleDisplayItem GetDisplayItem(string typeName)
        {
            for (int i = 0; i < _cacheRuleDisplayItems.Count; i++)
            {
                var item = _cacheRuleDisplayItems[i];
                if (item.name == typeName)
                    return item;
            }
            return null;
        }
        
        private  string GetRuleDisplayName(Type type)
        {
            var attribute = type.GetCustomAttribute<DisplayNameAttribute>(false);
            if (attribute != null && string.IsNullOrEmpty(attribute.DisplayName) == false)
                return attribute.DisplayName;
            else
                return type.Name;
        }

      

        public  T GetRuleInstance(string ruleName)
        {
            if (_cacheRuleInstance.TryGetValue(ruleName, out var instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (T)Activator.CreateInstance(type);
                _cacheRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new Exception($"{nameof(T)} is invalid：{ruleName}");
            }
        }
        
    }

    
    // public class ResourcePackageBuilder
    // {
    //     // public class ABBuildTarget
    //     // {
    //     //     public string bundleName = string.Empty;
    //     //     
    //     //     public List<CollectAssetInfo> assetNames=new List<CollectAssetInfo>();
    //     //     
    //     //     public void AddAsset(string asset)
    //     //     {
    //     //         asset = asset.ToLower();
    //     //         asset = asset.Replace("\\", "/");
    //     //         assetNames.Add(asset);
    //     //     }
    //     // }
    //     
    //     // private  Dictionary<string, ABBuildTarget> buildTargetDict=new Dictionary<string, ABBuildTarget>();
    //     // private  List<ABBuildTarget> buildTargetList=new List<ABBuildTarget>();
    //     
    //     private readonly Dictionary<string, BuildBundleInfo> _bundleInfoDic = new Dictionary<string, BuildBundleInfo>(10000);
    //
    //     
    //     
    //     public UnityEngine.AssetBundleManifest Build
    // }
    
}