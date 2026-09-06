using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.CPU
{
    /// <summary>
    /// Manages Modded CPUs
    /// </summary>
    public class CPUModManager : LegacyComponentModManager<PLCPU, CPUMod, ECPUClass>
    {
        public readonly int VanillaCPUMaxType = 0;
        private static CPUModManager m_instance = null;
        public readonly List<CPUMod> CPUTypes = new List<CPUMod>();

        /// <summary>
        /// Static Manager Instance
        /// </summary>
        public static CPUModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CPUModManager();
                }
                return m_instance;
            }
        }

        public CPUModManager() : base((int)ESlotType.E_COMP_CPU)
        {
            VanillaCPUMaxType = VanillaMaxType;
            CPUTypes = legacyModComps;
        }
        /// <summary>
        /// Finds CPU type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find CPU.
        /// </summary>
        /// <param name="CPUName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetCPUIDFromName(string CPUName) => GetIDFromName(CPUName);
        protected override void ComponentModConstructor(PLCPU comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            CPUMod cpu = legacyComp as CPUMod;
            comp.Speed = cpu.Speed;
            comp.m_Defense = cpu.Defense;
            comp.m_MaxCompUpgradeLevelBoost = cpu.MaxCompUpgradeLevelBoost;
            comp.m_MaxPawnItemUpgradeLevelBoost = cpu.MaxItemUpgradeLevelBoost;
            comp.SysInstConduit = cpu.SysInstConduit;
            comp.m_MaxPowerUsage_Watts = cpu.MaxPowerUsage_Watts;
        }
        static ConstructorInfo constructor = typeof(PLCPU).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ECPUClass), typeof(int) }, null);
        protected override void BaseClassConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }
        //One Method Extra
        protected override void ModComponentSubtypeMethods(Dictionary<MethodInfo, MethodInfo> methodOverrides)
        {
            //Doesn't work as base method is not virtual or abstract
            //methodOverrides.Add(AccessTools.Method(typeof(CPUMod), nameof(CPUMod.WhenProgramIsRun)), AccessTools.Method(typeof(PLCPU), nameof(PLCPU.WhenProgramIsRun)));
            return;
        }

        /// <summary>
        /// Creates a CPU based on input parameters.
        /// </summary>
        /// <param name="Subtype"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static PLCPU CreateCPU(int Subtype, int level)
        {
            return CreateCPU(Subtype, level, 0);
        }
        public static PLCPU CreateCPU(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLCPU comp))
            {
                return comp;
            }

            comp = new PLCPU((ECPUClass)Subtype, level);
            return comp;
        }

        //Converts hashes to CPUs.
        [HarmonyPatch(typeof(PLCPU), "CreateCPUFromHash")]
        class CPUHashFix
        {
            static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
            {
                __result = CPUModManager.CreateCPU(inSubType, inLevel, inSubTypeData);
                return false;
            }
        }
        //[HarmonyPatch(typeof(PLCPU), "FinalLateAddStats")]
        //class CPUFinalLateAddStatsPatch
        //{
        //    static void Postfix(PLCPU __instance)
        //    {
        //        int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
        //        if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
        //        {
        //            CPUModManager.Instance.CPUTypes[subtypeformodded].FinalLateAddStats(__instance);
        //        }
        //    }
        //}
        [HarmonyPatch(typeof(PLCPU), "WhenProgramIsRun")]
        class CPWhenProgramIsRunPatch
        {
            static void Postfix(PLWarpDriveProgram inProgram, PLCPU __instance)
            {
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count && inProgram != null)
                {
                    CPUModManager.Instance.CPUTypes[subtypeformodded].WhenProgramIsRun(inProgram);
                }
            }
        }
        //[HarmonyPatch(typeof(PLCPU), "AddStats")]
        //class CPUAddStatsPatch
        //{
        //    static void Postfix(PLCPU __instance)
        //    {
        //        int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
        //        if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
        //        {
        //            CPUModManager.Instance.CPUTypes[subtypeformodded].AddStats(__instance);
        //        }
        //    }
        //}
        //[HarmonyPatch(typeof(PLCPU), "Tick")]
        //class CPUTickPatch
        //{
        //    static void Postfix(PLCPU __instance)
        //    {
        //        int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
        //        if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
        //        {
        //            CPUModManager.Instance.CPUTypes[subtypeformodded].Tick(__instance);
        //        }
        //    }
        //}
        //[HarmonyPatch(typeof(PLCPU), "GetStatLineRight")]
        //class CPUGetStatLineRightPatch
        //{
        //    static void Postfix(PLCPU __instance, ref string __result)
        //    {
        //        int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
        //        if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
        //        {
        //            __result = CPUModManager.Instance.CPUTypes[subtypeformodded].GetStatLineRight(__instance);
        //        }
        //    }
        //}
        //[HarmonyPatch(typeof(PLCPU), "GetStatLineLeft")]
        //class CPUGetStatLineLeftPatch
        //{
        //    static void Postfix(PLCPU __instance, ref string __result)
        //    {
        //        int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
        //        if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
        //        {
        //            __result = CPUModManager.Instance.CPUTypes[subtypeformodded].GetStatLineLeft(__instance);
        //        }
        //    }
        //}
    }
}
