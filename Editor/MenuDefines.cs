using System.IO;
using UnityEditor;
using UnityEngine;
using VeryFS.Framework.Editor.Resource;

namespace VeryFS.Framework.Editor
{
    public static class MenuDefines
    {
        public const string MENU = "Framework/";
        public const string MENU_UTILITY = "Framework/Utility/";


        [MenuItem(MENU + "资源打包", false, 1)]
        private static void OpenResourceBuildEditor()
        {
            //ResourceModule.InitPath();
            var window = EditorWindow.GetWindow<ResourceBuildEditor>(
                "资源打包", true, typeof(ResourcePackageEditor));
            window.Show();
        }

        [MenuItem(MENU_UTILITY + "Clear PlayerPrefs")]
        public static void MenuClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs Cleared!");
        }

        [MenuItem(MENU_UTILITY + "打开PersistentData目录")]
        public static void OpenPersistentDataPath()
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }

        [MenuItem(MENU_UTILITY + "Clear PersistentData")]
        public static void ClearPersistentDataPath()
        {
            foreach (string dir in Directory.GetDirectories(Application.persistentDataPath))
            {
                Directory.Delete(dir, true);
            }
            foreach (string file in Directory.GetFiles(Application.persistentDataPath))
            {
                File.Delete(file);
            }
        }
    }
}