using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Content.Components.NuclearDevice;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.PolytechModule
{
    public class PolytechModuleModManager : LegacyComponentModManager<PLPolytechModule, PolytechModuleMod, EPolytechModuleType>
    {
        public readonly int VanillaPolytechModuleMaxType = 0;
        private static PolytechModuleModManager m_instance = null;
        public readonly List<PolytechModuleMod> PolytechModuleTypes = new List<PolytechModuleMod>();
        public static PolytechModuleModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PolytechModuleModManager();
                }
                return m_instance;
            }
        }

        PolytechModuleModManager() : base((int)ESlotType.E_COMP_POLYTECH_MODULE)
        {
            VanillaPolytechModuleMaxType = VanillaMaxType;
            PolytechModuleTypes = legacyModComps;
        }
        /// <summary>
        /// Finds PolytechModule type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find PolytechModule.
        /// </summary>
        /// <param name="PolytechModuleName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetPolytechModuleIDFromName(string PolytechModuleName) => GetIDFromName(PolytechModuleName);
        protected override void ComponentModConstructor(PLPolytechModule comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            PolytechModuleMod polytech = legacyComp as PolytechModuleMod;
            comp.m_MaxPowerUsage_Watts = polytech.MaxPowerUsage_Watts;
        }
        static ConstructorInfo constructor = typeof(PLPolytechModule).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EPolytechModuleType), typeof(int) }, null);
        protected override void BaseClassConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }
        protected override void ModComponentSubtypeMethods(Dictionary<MethodInfo, MethodInfo> methodOverrides)
        {
            return;
        }
        public static PLPolytechModule CreatePolytechModule(int Subtype, int level)
        {
            return CreatePolytechModule(Subtype, level, 0);
        }
        public static PLPolytechModule CreatePolytechModule(int Subtype, int level, int inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLPolytechModule comp))
            {
                return comp;
            }
            return new PLPolytechModule((EPolytechModuleType)Subtype, level);
        }
    }
    //Converts hashes to PolytechModules.
    [HarmonyPatch(typeof(PLPolytechModule), "CreatePolytechModuleFromHash")]
    class HashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = PolytechModuleModManager.CreatePolytechModule(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
    //[HarmonyPatch(typeof(PLPolytechModule), "Tick")]
    //class TickPatch
    //{
    //    static void Postfix(PLPolytechModule __instance)
    //    {
    //        int subtypeformodded = __instance.SubType - PolytechModuleModManager.Instance.VanillaPolytechModuleMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < PolytechModuleModManager.Instance.PolytechModuleTypes.Count && __instance.ShipStats != null && __instance.IsEquipped)
    //        {
    //            PolytechModuleModManager.Instance.PolytechModuleTypes[subtypeformodded].Tick(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLPolytechModule), "FinalLateAddStats")]
    //class FinalLateAddStatsPatch
    //{
    //    static void Postfix(PLPolytechModule __instance)
    //    {
    //        int subtypeformodded = __instance.SubType - PolytechModuleModManager.Instance.VanillaPolytechModuleMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < PolytechModuleModManager.Instance.PolytechModuleTypes.Count && __instance.ShipStats != null)
    //        {
    //            PolytechModuleModManager.Instance.PolytechModuleTypes[subtypeformodded].FinalLateAddStats(__instance);
    //        }
    //    }
    //}
}
