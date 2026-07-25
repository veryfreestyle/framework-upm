using System;
using System.Collections.Generic;
using UnityEngine;
using VeryFS.Framework.Runtime.Resource;

namespace VeryFS.Framework.Editor.Resource
{



    [CreateAssetMenu(fileName = "PackageBuild", menuName = MenuDefines.MENU + "Create ResourcePackageBuildSettings")]
    public class ResourcePackageBuildSettings : ScriptableObject
    {
        public string PackageName = "DefaultPackage";

        public string BuildVersion;

        public bool AutoUpdateVersion = true;

        public AssetBundleNameStyle BundleNameStyle = AssetBundleNameStyle.BundleName;

        public List<AssetBundleCollectorGroup> Groups = new();

        public ResourcePackageBuildSettings()
        {
            UpdateVersion();
        }


        public void UpdateVersion()
        {
            BuildVersion = FormatVersion(DateTime.Now);
        }

        /// <summary>
        /// 纯逻辑：把时刻格式化成 "yyyyMMdd_当日分钟数"（当日分钟数 = 时*60+分，取值 0..1439）。
        /// 抽成静态便于单测；build 时用它算局部版本号喂 manifest，不回写本 .asset（见 ResourceBuilder）。
        /// </summary>
        public static string FormatVersion(DateTime now)
        {
            int totalMinutes = now.Hour * 60 + now.Minute;
            return now.ToString("yyyyMMdd") + "_" + totalMinutes;
        }

        public void NewGroup()
        {

            string groupName = "Group";
            for (int i = 1; i < 2000; i++)
            {
                groupName = "Group" + i;
                int idx = Groups.FindIndex(group => group.GroupName == groupName);
                if (idx < 0)
                    break;
            }

            Groups.Add(new AssetBundleCollectorGroup()
            {
                GroupEnabled = true,
                GroupName = groupName,
                GroupDesc = ""
            });
        }

        public static readonly RuleHelper<IFileFilterRule> FilterRuleHelper = new();
        public static readonly RuleHelper<IPackRule> PackRuleHelper = new();
        public static readonly RuleHelper<IAddressRule> AddressRuleHelper = new();


        public void CollectAssets(List<CollectAssetInfo> list)
        {
            foreach (var group in Groups)
            {
                group.CollectAssets(list);
            }
        }

    }




}