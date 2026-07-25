using System;
using System.Collections.Generic;
using UnityEngine;

namespace VeryFS.Framework.Editor.Resource
{

    [Serializable]
    public class AssetBundleCollectorGroup
    {
        public string GroupName = string.Empty;

        public string GroupDesc = string.Empty;

        public bool GroupEnabled = true;

        public List<AssetBundleCollector> Collectors = new();

        public void CollectAssets(List<CollectAssetInfo> list)
        {
            foreach (var collector in Collectors)
            {
                collector.CollectAssets(list, GroupName);
            }
        }
    }

    public class CollectAssetInfo
    {
        public string Address { get; }
        public string AssetPath { get; }

        public string BundleName { get; }
        // public string AssetRelativePath { get; }
        public bool isRaw = false;
        public string[] Dependencies { get; }
        
        public bool IsAddressBundle { get; }

        public CollectAssetInfo(string collectPath, string addr, string path, string bundle,
            string[] depends,bool isAddressBundle=false)
        {
            Address = addr;
            AssetPath = path;
            BundleName = bundle ;
            Dependencies = depends;
            IsAddressBundle = isAddressBundle;
            isRaw = false;
            //AssetRelativePath = path.Replace(collectPath, string.Empty).TrimStart('/').TrimStart('\\');
        }


    }
    
    [Serializable]
    public class AssetBundleCollector
    {
        // public AssetBundleCollectorType CollectorType 
        
        public string Path = string.Empty;
        
        public string AddressRuleName = nameof(AddressByPath);

        public string PackRuleName = nameof(PackSingle);
        
        public string FilterRuleName = nameof(CollectAll);

  
        public void CollectAssets(List<CollectAssetInfo> list,string groupName)
        {
            IAddressRule addressRule = 
                ResourcePackageBuildSettings.AddressRuleHelper.GetRuleInstance(AddressRuleName);
            Debug.Assert(addressRule!=null,"AddressRuleName: "+AddressRuleName);
            
            IPackRule packRule = 
                ResourcePackageBuildSettings.PackRuleHelper.GetRuleInstance(PackRuleName);
            Debug.Assert(packRule!=null,"PackRuleName: "+PackRuleName);
            
            IFileFilterRule filterRule = 
                ResourcePackageBuildSettings.FilterRuleHelper.GetRuleInstance(FilterRuleName);
            Debug.Assert(filterRule!=null,"FilterRuleName: "+FilterRuleName);

            packRule.MakeCollectAssetInfos(list,groupName, Path, addressRule, filterRule);

        }
    }
}