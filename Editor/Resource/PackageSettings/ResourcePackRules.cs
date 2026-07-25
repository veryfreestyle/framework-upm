
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VeryFS.Framework.Runtime.Resource;


namespace VeryFS.Framework.Editor.Resource
{

    // public class RuleData
    // {
    //     public string assetPath;
    //     public string groupName;
    //     public string collectPath;
    //     
    // }
    
    
    public interface IAddressRule
    {
        string GetAssetAddress(string assetPath,string groupName,string collectPath);
    }

    
    [DisplayName("禁用寻址",3)]
    public class AddressDisable : IAddressRule
    {
        public string GetAssetAddress(string assetPath,string groupName,string collectPath)
        {
            return "";
        }
    }

    [DisplayName("寻址：相对路径",0)]
    public class AddressByPath : IAddressRule
    {
        public virtual string GetAssetAddress(string assetPath,string groupName,string collectPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string dir = Path.GetDirectoryName(assetPath);
            
            assetPath.Replace(collectPath, string.Empty).TrimStart('/').TrimStart('\\');
            
            dir= Path.GetRelativePath( collectPath,dir);

            if (dir == ".")
            {
                return fileName;
            }
            else
            {
                return dir + "/" + fileName;
            }
        }
    }
    
    [DisplayName("寻址：文件名",1)]
    public class AddressByFileName: IAddressRule
    {
        public virtual string GetAssetAddress(string assetPath,string groupName,string collectPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            return fileName;
        }
    }
    
    [DisplayName("寻址：组+相对路径",3)]
    public class AddressByGroupAndPath : AddressByPath
    {
        public override string GetAssetAddress(string assetPath,string groupName,string collectPath)
        {
            string address = base.GetAssetAddress(assetPath,groupName,collectPath);
            return groupName + ":" + address;
        }
        
    }
    
    [DisplayName("寻址：组+文件名",2)]
    public class AddressByGroupAndFileName : AddressByFileName
    {
        public override string GetAssetAddress(string assetPath,string groupName,string collectPath)
        {
            string address = base.GetAssetAddress(assetPath,groupName,collectPath);
            return groupName + ":" + address;
        }
    }
    
    
    public interface IPackRule
    {
        // string GetBundleName(string collectPath,string assetPath);
        
        bool DisableFileFilterRule { get; }

        string GetBundleName(string assetPath, string groupName, string collectPath);

        void MakeCollectAssetInfos(List<CollectAssetInfo> list,string groupName,string collectPath,IAddressRule addressRule, IFileFilterRule filterRule);
    }

    
    public abstract class  PackRuleBase : IPackRule
    {
        public virtual bool DisableFileFilterRule => false;

        public abstract string GetBundleName(string assetPath, string groupName, string collectPath);
        
        
        public virtual void MakeCollectAssetInfos(List<CollectAssetInfo> list,string groupName,string collectPath,IAddressRule addressRule, IFileFilterRule filterRule)
        {
            if (!AssetDatabase.IsValidFolder(collectPath))
            {
                Debug.LogError("Invalid Collect Path: " +collectPath);
                return;
            }
            string[] findAssets = EditorTools.FindAssets(collectPath);

            foreach (var assetPath in findAssets)
            {
                if (!IsValidateAsset(assetPath))
                    continue;
                if (!DisableFileFilterRule && !filterRule.Filter(assetPath))
                    continue;
                string address = addressRule.GetAssetAddress(assetPath, groupName, collectPath);
                
                string bundleName = GetBundleName(assetPath, groupName, collectPath) ;
                if (string.IsNullOrEmpty(bundleName))
                    continue;

                var info = new CollectAssetInfo(collectPath, address, assetPath,bundleName, new string[]{});
                list.Add(info);
            }
        }
        
        private static readonly HashSet<string> _ignoreFileExtensions = new HashSet<string>() { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc", ".hlsl" };
        
        protected static bool IsValidateAsset(string assetPath)
        {
            // 忽略文件夹
            if (AssetDatabase.IsValidFolder(assetPath))
                return false;
            
            // 忽略编辑器下的类型资源
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (assetType == typeof(LightingDataAsset))
                return false;
            
            string fileExtension = System.IO.Path.GetExtension(assetPath);
            if (_ignoreFileExtensions.Contains(fileExtension))
                return false;
            
            return true;
        }
    }
    
    
    [DisplayName("FairyGUI打包", 4)]
    public class PackFairyGUI : PackRuleBase
    {
        private const string FUI_KEY = "_fui.bytes";
        public override bool DisableFileFilterRule => true;

        
         
        public override string GetBundleName(string assetPath, string groupName, string collectPath)
        {
            string uiPackage = "";
            if (assetPath.EndsWith(FUI_KEY))
            {
                uiPackage = assetPath.Substring(0, assetPath.Length- FUI_KEY.Length);
            }
            else 
            {
                int idx = assetPath.IndexOf("_atlas", StringComparison.Ordinal);
                if (Path.GetExtension(assetPath) == ".png" && idx>0)
                {
                    uiPackage = assetPath.Substring(0, idx);
                }
                else
                {
                    return "";
                }
            }
            string bundleName = uiPackage.Replace('/', '_').Replace('\\', '_');

            return bundleName.ToLower()+ ResourcePath.BundleExtension;
        }
        
        private  string GetUIPackageName(string assetPath)
        {
            string uiPackage = "";
            if (assetPath.EndsWith(FUI_KEY))
            {
                uiPackage = assetPath.Substring(0, assetPath.Length- FUI_KEY.Length);
            }
            else 
            {
                int idx = assetPath.IndexOf("_atlas", StringComparison.Ordinal);
                if (Path.GetExtension(assetPath) == ".png" && idx>0)
                {
                    uiPackage = assetPath.Substring(0, idx);
                }
                else
                {
                    return uiPackage;
                }
            }
            string bundleName = uiPackage.Replace('/', '_').Replace('\\', '_');
            int idx2 = bundleName.LastIndexOf('_');
            if (idx2 > 0)
            {
                bundleName = bundleName.Substring(idx2+1);
                return bundleName;
            }

            return "";

        }

        public override void  MakeCollectAssetInfos(List<CollectAssetInfo> list,string groupName, string collectPath,
            IAddressRule addressRule, IFileFilterRule filterRule)
        {
            // return base.MakeCollectAssetInfos(groupName, collectPath, addressRule, filterRule);
            if (!AssetDatabase.IsValidFolder(collectPath))
            {
                Debug.LogError("Invalid Collect Path: " + collectPath);
                return ;
            }

            string[] findAssets = EditorTools.FindAssets(collectPath);

            foreach (var assetPath in findAssets)
            {
                if (!IsValidateAsset(assetPath))
                    continue;
                // if (!DisableFileFilterRule && !filterRule.Filter(assetPath))
                //     continue;
                //string address = addressRule.GetAssetAddress(assetPath, groupName, collectPath);

                string bundleName = GetBundleName(assetPath, groupName, collectPath);
                if (string.IsNullOrEmpty(bundleName))
                    continue;

                string address = GetUIPackageName(assetPath);

                var info = new CollectAssetInfo(collectPath, address, assetPath, bundleName, new string[] { }, true);
                list.Add(info);
            }

            return ;
        }
    }

    [DisplayName("每个文件单独打包",3)]
    public class PackSeparately : PackRuleBase
    {
        public override string GetBundleName(string assetPath, string groupName, string collectPath)
        {
            int index = assetPath.LastIndexOf(".", StringComparison.Ordinal);
            if (index != -1)
            {
                assetPath = assetPath.Remove(index);
            }
            
            string bundleName = assetPath.Replace('/', '_').Replace('\\', '_');
            return  bundleName.ToLower()+ ResourcePath.BundleExtension;
        }
    }
    
    [DisplayName("按目录打包",2)]
    public class PackDirectory :  PackRuleBase
    {
        
        public override string GetBundleName(string assetPath, string groupName, string collectPath)
        {     
            string bundleName = Path.GetDirectoryName(assetPath)?.Replace('/', '_').Replace('\\', '_');
            return bundleName.ToLower()+ ResourcePath.BundleExtension; 
        }
    }
    
    
    [DisplayName("打成一个包",0)]
    public class PackSingle :  PackRuleBase
    {
        public override string GetBundleName(string assetPath, string groupName, string collectPath)
        {
            string bundleName =  collectPath.TrimEnd(Path.PathSeparator).Replace('/', '_').Replace('\\', '_');
            return bundleName.ToLower()+ ResourcePath.BundleExtension; 
        }
    }
     
    [DisplayName("按分组打包",1)]
    public class PackGroup : PackRuleBase
    {
        public override string GetBundleName(string assetPath, string groupName, string collectPath)
        {
            return groupName.ToLower()+ ResourcePath.BundleExtension;
        }
    }


    [DisplayName("原始资源", 5)]
    public class PackRawFile : PackRuleBase
    {
        public override bool DisableFileFilterRule => true;

        public override string GetBundleName(string assetPath, string groupName, string collectPath)
        {
            // Debug.LogError($"{assetPath}, {collectPath}");
            //"Raw/"+groupName+"/"+
            //string filename = Path.GetRelativePath(collectPath, assetPath);
            //string filename = groupName + "/" + assetPath.Replace(collectPath, "");// Path.GetFileName(assetPath);
           // return groupName + "/" + filename; //assetPath.Replace('/', '_').Replace('\\', '_').ToLower() ;
            
            return "-";
        }

        public override void MakeCollectAssetInfos(List<CollectAssetInfo> list, string groupName, string collectPath,
            IAddressRule addressRule, IFileFilterRule filterRule)
        {
            List<CollectAssetInfo> list2 = new();
            base.MakeCollectAssetInfos(list2, groupName, collectPath,
                new AddressRaw(), filterRule);
            
            foreach (var item in list2)
            {
                item.isRaw = true;
                list.Add(item);
            }
        }
    }
    
    /// <summary>
    /// 针对原始资源
    /// </summary>
    internal class AddressRaw : IAddressRule
    {
        public string GetAssetAddress(string assetPath,string groupName,string collectPath)
        {
            string filename = Path.GetRelativePath(collectPath, assetPath);
            filename=filename.Replace("\\", "/");
            return groupName + "/" + filename;
        }
    }


    //ui_package.ab, ui_package_res.ab
    // [DisplayName("FairyGUI打包(配置和资源分开)",5)]
    // public class PackFairyGUI2 : IPackRule
    // {
    //     public bool DisableFileFilterRule => true;
    //     public string GetBundleName(string assetPath, string groupName, string collectPath)
    //     {
    //         return "fairygui2";
    //     }
    // }
    //
    
    public interface IFileFilterRule
    {
        bool Filter(string assetPath);
    }
    
    [DisplayName("收集所有资源",0)]
    public class CollectAll : IFileFilterRule
    {
        public bool Filter(string assetPath)
        {
            return true;
        }
    }
    
    [DisplayName("收集场景",1)]
    public class CollectScene : IFileFilterRule
    {
        public bool Filter(string assetPath)
        {
            return Path.GetExtension(assetPath) == ".unity";
        }
    }
    
    [DisplayName("收集预制体",2)]
    public class CollectPrefab : IFileFilterRule
    {
        public bool Filter(string assetPath)
        {
            return Path.GetExtension(assetPath) == ".prefab";
        }
    }
    
    [DisplayName("收集精灵类型的纹理",3)]
    public class CollectSprite : IFileFilterRule
    {
        public bool Filter(string assetPath)
        {
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (mainAssetType == typeof(Texture2D))
            {
                var texImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }


    
}