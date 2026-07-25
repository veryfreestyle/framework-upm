using UnityEditor;

namespace VeryFS.Framework.Editor
{
    [CustomEditor(typeof(Resource.ResourcePackageBuildSettings))]
    public class ResourcePackageBuildSettingsInspector: UnityEditor.Editor
    {
        // public VisualTreeAsset m_InspectorXML;
        
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
        
        
        // public override VisualElement CreateInspectorGUI()
        // {
        //     // Create a new VisualElement to be the root of our inspector UI
        //     VisualElement root = new VisualElement();
        //
        //     // Add a simple label
        //     //myInspector.Add(new Label("This is a custom inspector"));
        //     // Load from default reference
        //     m_InspectorXML.CloneTree(root);
        //
        //     var AutoUpdateVersion = root.Q<Toggle>("AutoUpdateVersion");
        //     AutoUpdateVersion.SetEnabled(false);
        //
        //     var ctrl = root.Q<EnumField>("BundleNameStyle");
        //     ctrl.SetEnabled(false);
        //     // AutoUpdateVersion.RegisterValueChangedCallback(evt =>
        //     // {
        //     //     var ctrl = evt.target as Toggle;
        //     //     ctrl.set
        //     // });
        //     
        //     
        //     // Load and clone a visual tree from UXML
        //     // VisualTreeAsset visualTree =  AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Car_Inspector_UXML.uxml");
        //     // visualTree.CloneTree(myInspector);
        //     // Return the finished inspector UI
        //     return root;
        // }
    }
}