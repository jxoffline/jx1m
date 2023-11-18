using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;


namespace Facebook.Unity.PostProcess
{
    /// <summary>
    /// Automatically disables Bitcode on iOS builds
    /// </summary>
    public static class DisableBitcode
    {
        [PostProcessBuildAttribute(999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
        {

         }
    }
}
