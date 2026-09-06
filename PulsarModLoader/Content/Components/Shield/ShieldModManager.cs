using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.Shield
{
    public class ShieldModManager : LegacyComponentModManager<PLShieldGenerator,ShieldMod,EShieldGeneratorType>
    {
        public readonly int VanillaShieldMaxType = 0;
        private static ShieldModManager m_instance = null;
        public readonly List<ShieldMod> ShieldTypes = new List<ShieldMod>();
        public static ShieldModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ShieldModManager();
                }
                return m_instance;
            }
        }
        public ShieldModManager() : base((int)ESlotType.E_COMP_SHLD)
        {
            ShieldTypes = legacyModComps;
            VanillaShieldMaxType = VanillaMaxType;
        }
        /// <summary>
        /// Finds Shield type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Shield.
        /// </summary>
        /// <param name="ShieldName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetShieldIDFromName(string ShieldName) => GetIDFromName(ShieldName);
        protected override void ComponentModConstructor(PLShieldGenerator comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            ShieldMod shield = legacyComp as ShieldMod;
            comp.Max = shield.ShieldMax;
            comp.ChargeRateMax = shield.ChargeRateMax;
            comp.RecoveryRate = shield.RecoveryRate;
            comp.Deflection = shield.Deflection;
            comp.MinIntegrityPercentForQuantumShield = shield.MinIntegrityPercentForQuantumShield;
            comp.m_MaxPowerUsage_Watts = shield.MaxPowerUsage_Watts * 1.4f;
            comp.MinIntegrityAfterDamage = shield.MinIntegrityAfterDamage;
            if (comp.MinIntegrityAfterDamage == -1)
            {
                comp.MinIntegrityAfterDamage = Mathf.RoundToInt(comp.Max * 0.15f);
            }
            comp.MinIntegrityAfterDamage = Mathf.RoundToInt(comp.MinIntegrityAfterDamage * (1f - Mathf.Clamp(0.05f * comp.Level, 0f, 0.8f)));
            comp.CurrentMax = comp.Max;
            comp.Current = comp.Max;
        }
        static ConstructorInfo constructor = typeof(PLShieldGenerator).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EShieldGeneratorType), typeof(int) }, null);
        protected override void BaseClassConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }
        //No Methods extra
        protected override void ModComponentSubtypeMethods(Dictionary<MethodInfo, MethodInfo> methodOverrides)
        {
            return;
        }
        public static PLShieldGenerator CreateShield(int Subtype, int level)
        {
            return CreateShield(Subtype, level, 0);
        }
        public static PLShieldGenerator CreateShield(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLShieldGenerator comp))
            {
                return comp;
            }
            return new PLShieldGenerator((EShieldGeneratorType)Subtype, level);
        }
    }
    //Converts hashes to Shields.
    [HarmonyPatch(typeof(PLShieldGenerator), "CreateShieldGeneratorFromHash")]
    class ShieldHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = ShieldModManager.CreateShield(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
    //Applies the Tick of the modded shields
    //[HarmonyPatch(typeof(PLShieldGenerator), "Tick")]
    //class TickPatch
    //{
    //    static void Postfix(PLShieldGenerator __instance)
    //    {
    //        int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaShieldMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.ShieldTypes.Count && __instance.ShipStats != null)
    //        {
    //            ShieldModManager.Instance.ShieldTypes[subtypeformodded].Tick(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLShieldGenerator), "GetStatLineLeft")]
    //class LeftDescFix
    //{
    //    static void Postfix(PLShieldGenerator __instance, ref string __result)
    //    {
    //        int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaShieldMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.ShieldTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = ShieldModManager.Instance.ShieldTypes[subtypeformodded].GetStatLineLeft(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLShieldGenerator), "GetStatLineRight")]
    //class RightDescFix
    //{
    //    static void Postfix(PLShieldGenerator __instance, ref string __result)
    //    {
    //        int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaShieldMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.ShieldTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = ShieldModManager.Instance.ShieldTypes[subtypeformodded].GetStatLineRight(__instance);
    //        }
    //    }
    //}
}
