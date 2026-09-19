using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Utilities.ExceptionImprovements
{
    [HarmonyPatch(typeof(Environment), "GetStackTrace")]
    static class EnvironmentGetStackTracePatch
    {
        public static bool Prefix(Exception e, bool needFileInfo, ref string __result)
        {
            try
            {
                var stackTrace = e == null ? new StackTrace(needFileInfo) : new StackTrace(e, needFileInfo);
                __result = ExceptionTools.ExtractHarmonyEnhancedStackTrace(stackTrace, false, out _);
                return false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"Stack Trace Enhancement Failed: {ex.Source}, {ex.Message}");
                return true;
            }
        }
    }
}
