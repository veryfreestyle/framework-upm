using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VeryFS.Framework.Editor;
using VeryFS.Framework.Editor.Resource;
using VeryFS.Framework.Runtime.Resource;

namespace VeryFS.Framework.Editor.Resource
{

public class ResourceBuildEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
    
    private const string OutputDir = "Product";

    private readonly ResourceBuilder mBuilder = new ResourceBuilder(OutputDir);

    private TextField mPackageName;
    private TextField mOutputPath;
    private Toggle mAutoUpdateVersion;
    private TextField mBuildVersion;
    private EnumField mBundleNameStyle;
    private Button mBtnStartBuild;
    
    
    
    void OnInspectorUpdate()
    {
        Repaint();
    }
    
    public void CreateGUI()
    {
        Undo.undoRedoPerformed -= FillData;
        Undo.undoRedoPerformed += FillData;
        
        var mPlatform = mBuilder.Platform;
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.StandaloneOSX:
                //case BuildTarget.StandaloneOSXIntel:
                mPlatform = ResourcePlatform.MacOS;
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                mPlatform = ResourcePlatform.Windows;
                break;
            case BuildTarget.iOS:
                mPlatform = ResourcePlatform.iOS;
                break;
            case BuildTarget.Android:
                mPlatform = ResourcePlatform.Android;
                break;
            case BuildTarget.WebGL:
                mPlatform = ResourcePlatform.WebGL;
                break;
        }

        mBuilder.Platform = mPlatform;
        
        VisualElement root = rootVisualElement;
        m_VisualTreeAsset.CloneTree(root);
        
        var btnOpen = root.Q<Button>("OpenFolder");
        btnOpen.clicked += () =>
        {
            EditorUtility.RevealInFinder(mBuilder.OutputPath);
        };

        var settingsAsset = root.Q<ObjectField>("settingsAsset");
        settingsAsset.RegisterValueChangedCallback(evt =>
        {
            string path = AssetDatabase.GetAssetPath(evt.newValue);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
                return;

            AssetDatabase.SaveAssets();
            if (evt.previousValue != null)
            {
                Undo.ClearUndo(evt.previousValue);
            }
                
            
            mBuilder.Settings = evt.newValue as ResourcePackageBuildSettings;
            
            FillData();
        });

        mPackageName = root.Q<TextField>("PackageName");
        mOutputPath = root.Q<TextField>("OutputPath");
        mBuildVersion = root.Q<TextField>("BuildVersion");
        mBuildVersion.RegisterValueChangedCallback(evt =>
        {
            var settings = mBuilder.Settings;
            if (settings != null)
            {
                Undo.RecordObject(mBuilder.Settings,"Change BuildVersion");
                EditorUtility.SetDirty(mBuilder.Settings);
                settings.BuildVersion = mBuildVersion.value;
            }
        });
        mAutoUpdateVersion = root.Q<Toggle>("AutoUpdateVersion");
        mAutoUpdateVersion.RegisterValueChangedCallback(evt =>
        {
            var settings = mBuilder.Settings;
            if (settings != null)
            {
                Undo.RecordObject(mBuilder.Settings,"Change AutoUpdateVersion");
                EditorUtility.SetDirty(mBuilder.Settings);
                settings.AutoUpdateVersion = mAutoUpdateVersion.value;
            }
        });
        
        mBundleNameStyle = root.Q<EnumField>("BundleNameStyle");
        mBundleNameStyle.Init(AssetBundleNameStyle.BundleName_HashName);
        mBundleNameStyle.RegisterValueChangedCallback(evt =>
        {
            var ctrl = evt.target as EnumField;
            var settings = mBuilder.Settings;
            if (settings != null)
            {
                Undo.RecordObject(mBuilder.Settings,"Change BundleNameStyle");
                EditorUtility.SetDirty(mBuilder.Settings);
                settings.BundleNameStyle = (AssetBundleNameStyle)ctrl.value;
            }
        });
        
        var platformField = root.Q<EnumField>("Platform");
        platformField.Init(mBuilder.Platform);
        platformField.RegisterValueChangedCallback(evt =>
        {
            var ctrl = evt.target as EnumField;
            mBuilder.Platform = (ResourcePlatform)ctrl.value;
            FillData();
        });
        
        
        var buildModeField = root.Q<EnumField>("BuildMode");
        buildModeField.Init(mBuilder.Mode);
        buildModeField.RegisterValueChangedCallback(evt =>
        {
            var ctrl = evt.target as EnumField;
            mBuilder.Mode = (ResourceBuildMode)ctrl.value;
            FillData();
        });
        
        mBtnStartBuild = root.Q<Button>("StartButton");
        mBtnStartBuild.clicked += () =>
        {
            var settings = mBuilder.Settings;
            if (settings != null)
            {
                mBuilder.PerformBuild();
                // 显示本次实际写进 manifest 的版本号（来自 builder 内存值，不回写 .asset，避免 git churn）；
                // 打包 skip（没有变化）时 LastBuildVersion 未被赋值，保留原显示值、不刷成空
                if (!string.IsNullOrEmpty(mBuilder.LastBuildVersion))
                    mBuildVersion.SetValueWithoutNotify(mBuilder.LastBuildVersion);
                AssetDatabase.SaveAssets();
                Undo.ClearUndo(settings);
                AssetDatabase.Refresh();
                //EditorUtility.RevealInFinder(mBuilder.OutputPath);
            }
        };

        var mBtnLink = root.Q<Button>("LinkButton");
        mBtnLink.clicked += () =>
        {
            var settings = mBuilder.Settings;
            if (settings != null)
            {
                EditorTools.DeleteAllSymbolLinks(StreamingAssetsPath_Bundles);
                string src = Path.GetDirectoryName(mBuilder.OutputPath);
                string dest = GetLinkPath();

               // Debug.Log($"Link {src} to {dest}");
                EditorTools.SymbolLinkFolder(src, dest);
                string rawDir = Path.GetDirectoryName(mBuilder.RawPath);
                EditorTools.SymbolLinkFolder(rawDir, dest);
                AssetDatabase.Refresh();
            }
        };
        
        var mBtnUnlink = root.Q<Button>("UnlinkButton");
        mBtnUnlink.clicked += () =>
        {
            var settings = mBuilder.Settings;
            if (settings != null)
            {
                EditorTools.DeleteAllSymbolLinks(StreamingAssetsPath_Bundles);
                AssetDatabase.Refresh();
            }
        };

        FillData();
    }
    
    public    const string StreamingAssetsPath_Bundles = "Assets/StreamingAssets/Bundles" ;
    public  string GetLinkPath()
    {
        if (!Directory.Exists(StreamingAssetsPath_Bundles))
            Directory.CreateDirectory(StreamingAssetsPath_Bundles);
        
        return StreamingAssetsPath_Bundles + "/" + mBuilder.Platform  + "/";
    }

    private void OnDisable()
    {
        // 与 CreateGUI 里的订阅配对解绑；窗口关闭或域重载前都会走此处，
        // 否则半初始化/已销毁的窗口实例仍挂在 undoRedoPerformed 上，undo 时触发 FillData 抛 NRE
        Undo.undoRedoPerformed -= FillData;
    }

    private void OnDestroy()
    {
        if (mBuilder.Settings != null)
        {
            AssetDatabase.SaveAssets();
        }
    }

    private void FillData()
    {
        // CreateGUI 尚未构建完 UI（或已销毁）时可能被 undo 回调提前触发，控件为 null，直接忽略
        if (mBtnStartBuild == null)
            return;

        mOutputPath.SetValueWithoutNotify(mBuilder.OutputPath);
        var settings = mBuilder.Settings;
        if (settings != null)
        {
            mPackageName.SetValueWithoutNotify(settings.PackageName);
            mBuildVersion.SetValueWithoutNotify(settings.BuildVersion);
            mAutoUpdateVersion.SetValueWithoutNotify(settings.AutoUpdateVersion);
            mBundleNameStyle.SetValueWithoutNotify(settings.BundleNameStyle);
            mBtnStartBuild.SetEnabled(true);
        }
        else
        {
            mPackageName.SetValueWithoutNotify("");
            mBuildVersion.SetValueWithoutNotify("");
            mBtnStartBuild.SetEnabled(false);
        }
    }

}
}
