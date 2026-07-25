// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;
using LitJson;
using UnityEngine;
using VeryFS.Framework.Runtime.Resource;
using VeryFS.Framework.Runtime.UI;

namespace VeryFS.Framework.Runtime
{
    /// <summary>
    /// 语言设置数据结构，包含配置名、默认字体、标题字体和UI分支等信息。
    /// </summary>
    public class LanguageSettings
    {
        public string configName;
        public string defaultFont;
        public string titleFont;
        public string uiBranch;

        public static readonly Dictionary<SystemLanguage, string> Map = new()
        {
            { SystemLanguage.ChineseSimplified, "zh_CN" },
            { SystemLanguage.ChineseTraditional, "zh_TC" },
            { SystemLanguage.English, "en" },
            { SystemLanguage.French, "fr" },
            { SystemLanguage.German, "de" },
            { SystemLanguage.Italian, "it" },
            { SystemLanguage.Japanese, "ja" },
            { SystemLanguage.Korean, "ko" },
            { SystemLanguage.Spanish, "es" },
            { SystemLanguage.Russian, "ru" },
        };

    }


    /// <summary>
    /// UIModule 提供了对 UI 相关功能的封装，包括加载翻译数据、初始化语言设置、预加载和卸载 UI 包等。
    /// </summary>
    public static class UIModule
    {
        /// <summary>
        /// 异步加载指定语言的翻译数据，并设置为FairyGUI的字符串源。
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static async UniTask LoadTranslationData(string configName)
        {
            try
            {
                using (var handle = await ResourceModule.Default.LoadAssetAsync($"ui:{configName}"))
                {
                    var asset = handle.Asset as TextAsset;
                    if (asset == null)
                    {
                        Debug.LogError($"LoadTranslationData: 'ui:{configName}' is not a TextAsset.");
                        return;
                    }

                    FairyGUI.Utils.XML xml = new FairyGUI.Utils.XML(asset.text);
                    UIPackage.SetStringsSource(xml);
                }
            }
            catch (Exception e)
            {
                // 翻译缺失可降级到默认语言，不中断启动，但必须出声可排查
                Debug.LogError($"LoadTranslationData '{configName}' failed: {e.Message}");
            }
        }

        /// <summary>
        /// 初始化语言设置，包括默认字体、标题字体和UI分支等。
        /// </summary>
        /// <param name="defaultFontName"></param>
        /// <param name="titleFontName"></param>
        /// <param name="settings"></param>
        public static void InitLanguage(string defaultFontName, string titleFontName,
            LanguageSettings settings)
        {
            UIConfig.defaultFont = defaultFontName;

            Debug.Log($"Language: {settings.configName} {settings.uiBranch}");

            var font1 = FontManager.GetFont(settings.defaultFont);
            Debug.Assert(font1 is TMPFont,
                $"Font {settings.defaultFont} is not a TMPFont.");
            FontManager.RegisterFont(font1, defaultFontName);

            if (defaultFontName != titleFontName && !string.IsNullOrEmpty(settings.titleFont))
            {
                var font2 = FontManager.GetFont(settings.titleFont);
                Debug.Assert(font2 is TMPFont,
                    $"Font {settings.titleFont} is not a TMPFont.");
                FontManager.RegisterFont(font2, titleFontName);

                Debug.Log($"'{defaultFontName}': {settings.defaultFont}, '{titleFontName}': {settings.titleFont}");
            }
            else
            {
                Debug.Log($"'{defaultFontName}': {settings.defaultFont}");
            }

            UIPackage.branch = settings.uiBranch;
        }

        /// <summary>
        /// 异步加载指定语言的设置，包括默认字体、标题字体和UI分支等。
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<LanguageSettings> LoadSettingsAsync(SystemLanguage language)
        {
            string configName = LanguageSettings.Map.GetValueOrDefault(language, "en");
            string json = await ResourceModule.Default.LoadTextAssetTextAsync("ui:languages");

            // 配置整个丢失属打包错误，显式失败好过错字体带病上线
            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("LoadSettingsAsync: 'ui:languages' missing or empty.");
            }

            var arraySettings = JsonMapper.ToObject<LanguageSettings[]>(json);
            if (arraySettings == null || arraySettings.Length == 0)
            {
                throw new Exception("LoadSettingsAsync: 'ui:languages' has no entries.");
            }

            foreach (var item in arraySettings)
            {
                if (string.Equals(item.configName, configName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            // 目标语言缺失：回退真实存在的 "en" 条目，而非捏造字体名
            foreach (var item in arraySettings)
            {
                if (string.Equals(item.configName, "en", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"Language '{configName}' not found in ui:languages, fallback to 'en'.");
                    return item;
                }
            }

            Debug.LogWarning($"Language '{configName}' and 'en' not found in ui:languages, using first entry '{arraySettings[0].configName}'.");
            return arraySettings[0];
        }
        // private static readonly CancellationTokenSource mCancelSource = new();
        // public static CancellationToken CancelToken => mCancelSource.Token;


        private static readonly Dictionary<string, UIPackageHandle> mUIHandles = new();

        /// <summary>
        /// 预加载指定的UI包，避免在首次使用时出现卡顿。
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask PreloadUIPackageAsync(string packageName)
        {
            if (!mUIHandles.TryAdd(packageName, null))   // 占位：挡重复与并发
                return;

            try
            {
                var handle = await ResourceModule.Default.LoadUIPackageAsync(packageName);
                if (handle.IsError)
                {
                    handle.Dispose();
                    throw new Exception($"PreloadUIPackageAsync '{packageName}' failed.");
                }
                mUIHandles[packageName] = handle;
            }
            catch
            {
                mUIHandles.Remove(packageName);   // 回滚占位，允许重试
                throw;
            }
        }

        /// <summary>
        /// 卸载指定的UI包，释放资源。
        /// </summary>
        /// <param name="packageName"></param>
        public static void UnloadUIPackage(string packageName)
        {
            if (mUIHandles.Remove(packageName, out var package))
            {
                package?.Dispose();
            }
        }

        /// <summary>
        /// 卸载所有已加载的UI包，释放资源。
        /// </summary>
        public static void UnloadAllUIPackages()
        {
            foreach (var package in mUIHandles.Values)
            {
                package?.Dispose();
            }
            mUIHandles.Clear();
        }
    }
}