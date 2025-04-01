
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArchitectHS.AddressableManage.Editor
{
    /// <summary>
    /// Processor class that runs on domain reload to verify and setup UniTask
    /// </summary>
    [InitializeOnLoad]
    public class ARMUniTaskSetupProcessor
    {
        private const string ARM_UNITASK_SYMBOL = "ARM_UNITASK";
        
#if !ARM_UNITASK
        static ARMUniTaskSetupProcessor()
        {
            // 도메인 리로드 후 실행 (지연 실행)
            EditorApplication.delayCall += () =>
            {
                // 유니태스크 어셈블리 확인 후 설치 또는 심볼 추가
                CheckAndSetupUniTask();
            };
        }
        
        /// <summary>
        /// 유니태스크 어셈블리를 확인하고 필요한 작업 수행
        /// </summary>
        private static void CheckAndSetupUniTask()
        {
            // 유니태스크 어셈블리가 이미 로드되어 있는지 확인
            if (IsUniTaskAssemblyLoaded())
            {
                Debug.Log("ARM: UniTask assembly is loaded. Adding ARM_UNITASK symbol.");
                
                // 심볼 추가
                AddArmUniTaskSymbol();
            }
            else
            {
                Debug.Log("ARM: UniTask assembly not found. Installing UniTask package...");
                
                // 유니태스크 설치
                InstallUniTask();
                
                // 설치가 완료되면 도메인이 다시 로드되면서 이 코드가 다시 실행될 것임
            }
        }
        
        /// <summary>
        /// 유니태스크 어셈블리가 로드되어 있는지 확인
        /// </summary>
        private static bool IsUniTaskAssemblyLoaded()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.GetName().Name == "UniTask")
                    {
                        Type type = assembly.GetType("Cysharp.Threading.Tasks.UniTask");
                        if (type != null)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking UniTask assembly: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// ARM_UNITASK 심볼 추가
        /// </summary>
        private static void AddArmUniTaskSymbol()
        {
            // 유효한 빌드 타겟 그룹만 가져오기
            BuildTargetGroup[] targetGroups = GetAllValidBuildTargetGroups();
            
            foreach (BuildTargetGroup targetGroup in targetGroups)
            {
#pragma warning disable CS0618
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#pragma warning restore CS0618
                
                if (!ContainsSymbol(symbols, ARM_UNITASK_SYMBOL))
                {
                    symbols = AddSymbol(symbols, ARM_UNITASK_SYMBOL);
                    
#pragma warning disable CS0618
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, symbols);
#pragma warning restore CS0618
                    
                    Debug.Log($"ARM: Added ARM_UNITASK symbol to {targetGroup}");
                }
            }
            
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// 유니태스크 패키지 설치
        /// </summary>
        private static void InstallUniTask()
        {
            try
            {
                // OpenUPM 레지스트리 추가 및 유니태스크 패키지 설치
                AddOpenUPMRegistryAndUniTaskPackage();
                
                Debug.Log("ARM: UniTask installation initiated. Please wait for Unity to reload packages.");
                Debug.Log("ARM: ARM_UNITASK symbol will be added after the reload.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ARM: Failed to install UniTask: {ex.Message}");
            }
        }
        
        /// <summary>
        /// OpenUPM 레지스트리 및 유니태스크 패키지 추가
        /// </summary>
        private static void AddOpenUPMRegistryAndUniTaskPackage()
        {
            const string manifestPath = "Packages/manifest.json";
            
            // manifest.json 파일이 없으면 종료
            if (!System.IO.File.Exists(manifestPath))
            {
                Debug.LogError("ARM: manifest.json not found!");
                return;
            }
            
            string manifestContent = System.IO.File.ReadAllText(manifestPath);
            bool manifestChanged = false;
            
            // OpenUPM 레지스트리 추가
            if (!manifestContent.Contains("package.openupm.com"))
            {
                string openUpmRegistry = @",
  ""scopedRegistries"": [
    {
      ""name"": ""package.openupm.com"",
      ""url"": ""https://package.openupm.com"",
      ""scopes"": [
        ""com.cysharp""
      ]
    }
  ]";
                
                int lastBraceIndex = manifestContent.LastIndexOf('}');
                if (lastBraceIndex >= 0)
                {
                    manifestContent = manifestContent.Insert(lastBraceIndex, openUpmRegistry);
                    manifestChanged = true;
                }
            }
            
            // 유니태스크 패키지 추가
            if (!manifestContent.Contains("\"com.cysharp.unitask\""))
            {
                int dependenciesIndex = manifestContent.IndexOf("\"dependencies\"");
                if (dependenciesIndex >= 0)
                {
                    int openBraceIndex = manifestContent.IndexOf('{', dependenciesIndex);
                    if (openBraceIndex >= 0)
                    {
                        string unitaskDependency = "\n    \"com.cysharp.unitask\": \"2.5.0\",";
                        manifestContent = manifestContent.Insert(openBraceIndex + 1, unitaskDependency);
                        manifestChanged = true;
                    }
                }
            }
            
            // 변경된 내용 저장
            if (manifestChanged)
            {
                System.IO.File.WriteAllText(manifestPath, manifestContent);
                Debug.Log("ARM: Updated manifest.json with UniTask dependency");
                
                // 무조건 패키지 데이터베이스를 갱신하도록 요청
                UnityEditor.PackageManager.Client.Resolve();
            }
        }
        
        private static BuildTargetGroup[] GetAllValidBuildTargetGroups()
        {
            return Enum.GetValues(typeof(BuildTargetGroup))
                .Cast<BuildTargetGroup>()
                .Where(group => group != BuildTargetGroup.Unknown && 
                                (int)group != -1 && 
                                IsValidBuildTargetGroup(group))
                .ToArray();
        }
        
        private static bool IsValidBuildTargetGroup(BuildTargetGroup group)
        {
            try
            {
                // 폐지된 플랫폼 제외
                if (group == (BuildTargetGroup)5 ||  // WebPlayer
                    group == (BuildTargetGroup)13 || // XBOX360
                    group == (BuildTargetGroup)15 || // PS3
                    group == (BuildTargetGroup)22)   // Nintendo3DS
                {
                    return false;
                }
                
#pragma warning disable CS0618
                var temp = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#pragma warning restore CS0618
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool ContainsSymbol(string symbolString, string symbol)
        {
            string[] symbols = symbolString.Split(new char[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return symbols.Any(s => s.Trim() == symbol);
        }
        
        private static string AddSymbol(string symbolString, string symbol)
        {
            if (string.IsNullOrEmpty(symbolString))
                return symbol;
                
            if (ContainsSymbol(symbolString, symbol))
                return symbolString;
                
            return symbolString + ";" + symbol;
        }
#endif
    }
}