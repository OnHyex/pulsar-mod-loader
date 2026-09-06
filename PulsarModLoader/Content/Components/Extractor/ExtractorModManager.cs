using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.Extractor
{
    public class ExtractorModManager : LegacyComponentModManager<PLExtractor, ExtractorMod, EExtractorType>
    {
        public readonly int VanillaExtractorMaxType = 0;
        private static ExtractorModManager m_instance = null;
        public readonly List<ExtractorMod> ExtractorTypes = new List<ExtractorMod>();
        public static ExtractorModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ExtractorModManager();
                }
                return m_instance;
            }
        }

        public ExtractorModManager() : base((int)ESlotType.E_COMP_SALVAGE_SYSTEM)
        {
            VanillaExtractorMaxType = VanillaMaxType;
            ExtractorTypes = legacyModComps;
        }
        /// <summary>
        /// Finds Extractor type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Extractor.
        /// </summary>
        /// <param name="ExtractorName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetExtractorIDFromName(string ExtractorName) => GetIDFromName(ExtractorName);
        protected override void ComponentModConstructor(PLExtractor comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            ExtractorMod extractor = legacyComp as ExtractorMod;
            comp.m_Stability = extractor.Stability;
        }
        static ConstructorInfo constructor = typeof(PLExtractor).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EExtractorType), typeof(int), typeof(short) }, null);
        protected override void BaseClassConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }
        //No Methods extra
        protected override void ModComponentSubtypeMethods(Dictionary<MethodInfo, MethodInfo> methodOverrides)
        {
            return;
        }
        public static PLExtractor CreateExtractor(int Subtype, int level)
        {
            return CreateExtractor(Subtype, level, 0);
        }
        public static PLExtractor CreateExtractor(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLExtractor comp))
            {
                return comp;
            }

            comp = new PLExtractor((EExtractorType)Subtype, level, (short)Subtypedata);
            return comp;
        }
    }
    //Converts hashes to Extractors.
    [HarmonyPatch(typeof(PLExtractor), "CreateExtractorFromHash")]
    class ExtractorHashFix
    {
        static bool Prefix(int inSubType, int inLevel, short inSubTypeData, ref PLShipComponent __result)
        {
            __result = ExtractorModManager.CreateExtractor(inSubType, inLevel, (int)inSubTypeData);
            return false;
        }
    }
    //[HarmonyPatch(typeof(PLExtractor), "GetStatLineLeft")]
    //class LeftDescFix
    //{
    //    static void Postfix(PLExtractor __instance, ref string __result)
    //    {
    //        int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].GetStatLineLeft(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLExtractor), "GetStatLineRight")]
    //class RightDescFix
    //{
    //    static void Postfix(PLExtractor __instance, ref string __result)
    //    {
    //        int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].GetStatLineRight(__instance);
    //        }
    //    }
    //}
}
