using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace VeryFS.Framework.Editor
{
    public enum AssetSearchType
    {
        All,
        RuntimeAnimatorController,
        AnimationClip,
        AudioClip,
        AudioMixer,
        Font,
        Material,
        Mesh,
        Model,
        PhysicMaterial,
        Prefab,
        Scene,
        Script,
        Shader,
        Sprite,
        Texture,
        VideoClip,
    }

    public static class EditorTools
    {
        public static string[] FindAssets(string searchInFolder, AssetSearchType searchType = AssetSearchType.All)
        {
            string filter = searchType == AssetSearchType.All ? string.Empty : $"t:{searchType}";
            var guids = AssetDatabase.FindAssets(filter, new[] { searchInFolder });

            // 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
            HashSet<string> hashSet = new HashSet<string>();
            List<string> list = new();
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (hashSet.Contains(assetPath) == false)
                {
                    hashSet.Add(assetPath);
                    list.Add(assetPath);
                }
            }

            return list.ToArray();
        }
        
        public static void EmptyDirectory(string path, string[] excludeDirs=null)
        {
            //清理子目录和文件
            string[] dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                bool isDel = true;
                if (excludeDirs != null)
                {
                    string name = Path.GetDirectoryName(dir);
                    foreach (var exclude in excludeDirs)
                    {
                        if (name == exclude)
                        {
                            isDel = false;
                            break;
                        }
                    }
                }
                if (isDel) Directory.Delete(dir, true);
            }
            
            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        
        /// <summary>
        /// 拷贝文件
        /// </summary>
        public static void CopyFile(string sourcePath, string destPath, bool overwrite)
        {
            if (File.Exists(sourcePath) == false)
                throw new FileNotFoundException(sourcePath);

            // 复制文件
            File.Copy(sourcePath, destPath, overwrite);
        }


        

        /// <summary>
        /// 创建文件夹
        /// </summary>
        public static bool CreateDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// 删除指定目录所有硬链接
        /// </summary>
        /// <param name="assetBundlesLinkPath"></param>
        public static void DeleteAllSymbolLinks(string assetBundlesLinkPath)
        {
            if (Directory.Exists(assetBundlesLinkPath))
            {
                foreach (var dirPath in Directory.GetDirectories(assetBundlesLinkPath))
                {
                    File.Delete(dirPath + ".meta");
                    DeleteSymbolLink(dirPath);
                }
            }

        }
        
        public static void SymbolLinkFolder(string srcFolderPath, string targetPath)
        {
            var os = Environment.OSVersion;
            if (os.ToString().Contains("Windows"))
            {
                ExecuteCommand(String.Format("mklink /J \"{0}\" \"{1}\"", targetPath, srcFolderPath));
            }
            else if (os.ToString().Contains("Unix"))
            {
                var fullPath = Path.GetFullPath(targetPath);
                if (fullPath.EndsWith("/"))
                {
                    fullPath = fullPath.Substring(0, fullPath.Length - 1);
                    fullPath = Path.GetDirectoryName(fullPath);
                }

                ExecuteCommand(String.Format("ln -s {0} {1}", Path.GetFullPath(srcFolderPath), fullPath));
            }
            else
            {
                Debug.LogError(String.Format("[SymbolLinkFolder]Error on OS: {0}", os.ToString()));
            }
        }

        
        /// <summary>
        /// 删除硬链接目录
        /// </summary>
        /// <param name="linkPath"></param>
        public static void DeleteSymbolLink(string linkPath)
        {
            var os = Environment.OSVersion;
            if (os.ToString().Contains("Windows"))
            {
                ExecuteCommand(String.Format("rmdir \"{0}\"", linkPath));
            }
            else if (os.ToString().Contains("Unix"))
            {
                ExecuteCommand(String.Format("rm -Rf \"{0}\"", linkPath));
            }
            else
            {
                Debug.LogError(String.Format("[SymbolLinkFolder]Error on OS: {0}", os.ToString()));
            }
        }
        
        
        /// <summary>
        /// 执行批处理命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="workingDirectory"></param>
        public static void ExecuteCommand(string command, string workingDirectory = null)
        {
            var fProgress = .1f;
            EditorUtility.DisplayProgressBar("ExecuteCommand", command, fProgress);

            try
            {
                string cmd;
                string preArg;
                var os = Environment.OSVersion;

                //Debug.Log(String.Format("[ExecuteCommand]Command on OS: {0}", os.ToString()));
                if (os.ToString().Contains("Windows"))
                {
                    cmd = "cmd.exe";
                    preArg = "/C ";
                }
                else
                {
                    cmd = "sh";
                    preArg = "-c ";
                }

                Debug.Log("[ExecuteCommand]" + command);
                var allOutput = new StringBuilder();
                using (var process = new Process())
                {
                    if (workingDirectory != null)
                        process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.FileName = cmd;
                    process.StartInfo.Arguments = preArg + "\"" + command + "\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    while (true)
                    {
                        var line = process.StandardOutput.ReadLine();
                        if (line == null)
                            break;
                        allOutput.AppendLine(line);
                        EditorUtility.DisplayProgressBar("[ExecuteCommand] " + command, line, fProgress);
                        fProgress += .001f;
                    }

                    var err = process.StandardError.ReadToEnd();
                    if (!String.IsNullOrEmpty(err))
                    {
                        Debug.LogError(String.Format("[ExecuteCommand] {0}", err));
                    }

                    process.WaitForExit();
                }

                //Debug.Log("[ExecuteResult]" + allOutput);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
    }
}