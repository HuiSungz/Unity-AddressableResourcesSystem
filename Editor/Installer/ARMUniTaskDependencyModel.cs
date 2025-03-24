
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Model class for handling UniTask dependency and define symbols
    /// </summary>
    public class ARMUniTaskDependencyModel
    {
        private const string ARM_UNITASK_SYMBOL = "ARM_UNITASK";
        private const string UNITASK_PACKAGE_NAME = "com.cysharp.unitask";
        private const string MANIFEST_PATH = "Packages/manifest.json";
        private const string SCOPED_REGISTRY_NAME = "package.openupm.com";

        /// <summary>
        /// Check if UniTask is installed
        /// </summary>
        public bool IsUniTaskInstalled()
        {
            if (!File.Exists(MANIFEST_PATH))
                return false;

            string manifestContent = File.ReadAllText(MANIFEST_PATH);
            
            // Check if UniTask is in dependencies
            return manifestContent.Contains($"\"{UNITASK_PACKAGE_NAME}\"");
        }

        /// <summary>
        /// Check if OpenUPM registry is configured
        /// </summary>
        public bool IsOpenUPMRegistryConfigured()
        {
            if (!File.Exists(MANIFEST_PATH))
                return false;

            string manifestContent = File.ReadAllText(MANIFEST_PATH);
            
            // Check if OpenUPM registry is configured
            return manifestContent.Contains(SCOPED_REGISTRY_NAME);
        }

        /// <summary>
        /// Add OpenUPM registry to manifest
        /// </summary>
        public bool AddOpenUPMRegistry()
        {
            if (!File.Exists(MANIFEST_PATH))
                return false;

            try
            {
                string manifestContent = File.ReadAllText(MANIFEST_PATH);
                
                // Check if scopedRegistries section exists
                if (!manifestContent.Contains("\"scopedRegistries\""))
                {
                    // Add scopedRegistries section
                    string openUpmRegistry = @",
  ""scopedRegistries"": [
    {
      ""name"": ""package.openupm.com"",
      ""url"": ""https://package.openupm.com"",
      ""scopes"": [
        ""com.cysharp"",
        ""com.neuecc"",
        ""com.unity"",
        ""jp.cysharp""
      ]
    }
  ]";
                    
                    // Insert before the last closing brace
                    int lastBraceIndex = manifestContent.LastIndexOf('}');
                    if (lastBraceIndex >= 0)
                    {
                        manifestContent = manifestContent.Insert(lastBraceIndex, openUpmRegistry);
                        File.WriteAllText(MANIFEST_PATH, manifestContent);
                        return true;
                    }
                }
                else if (!manifestContent.Contains(SCOPED_REGISTRY_NAME))
                {
                    // Find the scopedRegistries array
                    int registriesStart = manifestContent.IndexOf("\"scopedRegistries\"");
                    if (registriesStart >= 0)
                    {
                        int arrayStart = manifestContent.IndexOf('[', registriesStart);
                        if (arrayStart >= 0)
                        {
                            string openUpmRegistry = @"
    {
      ""name"": ""package.openupm.com"",
      ""url"": ""https://package.openupm.com"",
      ""scopes"": [
        ""com.cysharp"",
        ""com.neuecc"",
        ""com.unity"",
        ""jp.cysharp""
      ]
    },";
                            
                            // Insert after the opening bracket
                            manifestContent = manifestContent.Insert(arrayStart + 1, openUpmRegistry);
                            File.WriteAllText(MANIFEST_PATH, manifestContent);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to add OpenUPM registry: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Add UniTask package to dependencies
        /// </summary>
        public bool AddUniTaskPackage()
        {
            if (!File.Exists(MANIFEST_PATH))
                return false;

            try
            {
                string manifestContent = File.ReadAllText(MANIFEST_PATH);
                
                // Check if dependencies section exists
                if (manifestContent.Contains("\"dependencies\""))
                {
                    // Find the dependencies section
                    int dependenciesStart = manifestContent.IndexOf("\"dependencies\"");
                    if (dependenciesStart >= 0)
                    {
                        int openBraceIndex = manifestContent.IndexOf('{', dependenciesStart);
                        if (openBraceIndex >= 0)
                        {
                            string unitaskDependency = $"\n    \"{UNITASK_PACKAGE_NAME}\": \"2.5.0\",";
                            
                            // Insert after the opening brace
                            manifestContent = manifestContent.Insert(openBraceIndex + 1, unitaskDependency);
                            File.WriteAllText(MANIFEST_PATH, manifestContent);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to add UniTask package: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Check if ARM_UNITASK define symbol is added
        /// </summary>
        public bool IsArmUniTaskSymbolAdded()
        {
            // Check all build target groups
            BuildTargetGroup[] targetGroups = GetAllBuildTargetGroups();
            
            foreach (BuildTargetGroup targetGroup in targetGroups)
            {
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                
                if (!ContainsSymbol(symbols, ARM_UNITASK_SYMBOL))
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Add ARM_UNITASK define symbol to all build target groups
        /// </summary>
        public void AddArmUniTaskSymbol()
        {
            // Add to all build target groups
            BuildTargetGroup[] targetGroups = GetAllBuildTargetGroups();
            
            foreach (BuildTargetGroup targetGroup in targetGroups)
            {
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                
                if (!ContainsSymbol(symbols, ARM_UNITASK_SYMBOL))
                {
                    symbols = AddSymbol(symbols, ARM_UNITASK_SYMBOL);
                    
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, symbols);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                }
            }
            
            // Save changes
            AssetDatabase.SaveAssets();
        }

        private BuildTargetGroup[] GetAllBuildTargetGroups()
        {
            // 유효한 BuildTargetGroup만 필터링
            return Enum.GetValues(typeof(BuildTargetGroup))
                .Cast<BuildTargetGroup>()
                .Where(group => group != BuildTargetGroup.Unknown && 
                                (int)group != -1 && 
                                IsValidBuildTargetGroup(group))
                .ToArray();
        }

        /// <summary>
        /// 유효한 BuildTargetGroup인지 확인
        /// </summary>
        private bool IsValidBuildTargetGroup(BuildTargetGroup group)
        {
            try
            {
                // 대표적인 폐지된 플랫폼들 명시적으로 제외
                if (group == (BuildTargetGroup)5 ||           // WebPlayer
                    group == (BuildTargetGroup)13 ||          // XBOX360
                    group == (BuildTargetGroup)15 ||          // PS3
                    group == (BuildTargetGroup)22)            // Nintendo3DS
                {
                    return false;
                }
            
                // 이 메서드가 예외를 던지지 않으면 유효한 그룹
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                var temp = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                return true;
            }
            catch
            {
                // 예외가 발생하면 유효하지 않은 그룹
                return false;
            }
        }

        /// <summary>
        /// Check if symbol string contains specific symbol
        /// </summary>
        private bool ContainsSymbol(string symbolString, string symbol)
        {
            string[] symbols = symbolString.Split(new char[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i].Trim() == symbol)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Add new symbol to symbol string
        /// </summary>
        private string AddSymbol(string symbolString, string symbol)
        {
            if (string.IsNullOrEmpty(symbolString))
                return symbol;
                
            if (ContainsSymbol(symbolString, symbol))
                return symbolString;
                
            return symbolString + ";" + symbol;
        }
    }
}