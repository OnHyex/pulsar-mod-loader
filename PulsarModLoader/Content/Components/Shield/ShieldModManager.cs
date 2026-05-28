using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.Shield
{
#warning "not complete inheritance done and CreateShield should be reviewed / maybe override the base constructor stuff then decide on constructor pattern"
    public class ShieldModManager : ComponentModManager<PLShieldGenerator,ShieldMod,EShieldGeneratorType>
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
        /// <summary>
        /// Finds Shield type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Shield.
        /// </summary>
        /// <param name="ShieldName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetShieldIDFromName(string ShieldName) => GetIDFromName(ShieldName);
        protected override void ComponentModConstructor(PLShieldGenerator comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            ShieldMod shield = legacyComp as ShieldMod;
            base.ComponentModConstructor(comp, shield, subType, level, subTypeData);
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
        }
        public static PLShieldGenerator CreateShield(int Subtype, int level)
        {
            PLShieldGenerator InShield;
            if (Subtype >= Instance.VanillaShieldMaxType)
            {
                InShield = new PLShieldGenerator(EShieldGeneratorType.E_SG_ID_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaShieldMaxType;
                if (subtypeformodded <= Instance.ShieldTypes.Count && subtypeformodded > -1)
                {
                    ShieldMod ShieldType = Instance.ShieldTypes[Subtype - Instance.VanillaShieldMaxType];
                    InShield.SubType = Subtype;
                    InShield.Name = ShieldType.Name;
                    InShield.Desc = ShieldType.Description;
                    InShield.m_IconTexture = ShieldType.IconTexture;
                    InShield.Max = ShieldType.ShieldMax;
                    InShield.ChargeRateMax = ShieldType.ChargeRateMax;
                    InShield.RecoveryRate = ShieldType.RecoveryRate;
                    InShield.Deflection = ShieldType.Deflection;
                    InShield.MinIntegrityPercentForQuantumShield = ShieldType.MinIntegrityPercentForQuantumShield;
                    InShield.MinIntegrityAfterDamage = ShieldType.MinIntegrityAfterDamage;
                    InShield.m_MaxPowerUsage_Watts = (ShieldType.MaxPowerUsage_Watts * 1.4f);
                    InShield.m_MarketPrice = ShieldType.MarketPrice;
                    InShield.CargoVisualPrefabID = ShieldType.CargoVisualID;
                    InShield.CanBeDroppedOnShipDeath = ShieldType.CanBeDroppedOnShipDeath;
                    InShield.Experimental = ShieldType.Experimental;
                    InShield.Unstable = ShieldType.Unstable;
                    InShield.Contraband = ShieldType.Contraband;
                    InShield.Price_LevelMultiplierExponent = ShieldType.Price_LevelMultiplierExponent;
                    if (InShield.MinIntegrityAfterDamage == -1)
                    {
                        InShield.MinIntegrityAfterDamage = Mathf.RoundToInt(InShield.Max * 0.15f);
                    }
                    InShield.MinIntegrityAfterDamage = Mathf.RoundToInt(InShield.MinIntegrityAfterDamage * (1f - Mathf.Clamp(0.05f * InShield.Level, 0f, 0.8f)));
                    InShield.CurrentMax = InShield.Max;
                    InShield.Current = InShield.Max;
                }
            }
            else
            {
                InShield = new PLShieldGenerator((EShieldGeneratorType)Subtype, level);
            }
            return InShield;
        }
    }
    //Converts hashes to Shields.
    [HarmonyPatch(typeof(PLShieldGenerator), "CreateShieldGeneratorFromHash")]
    class ShieldHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ShieldModManager.CreateShield(inSubType, inLevel);
            return false;
        }
    }
    //Applies the Tick of the modded shields
    [HarmonyPatch(typeof(PLShieldGenerator), "Tick")]
    class TickPatch
    {
        static void Postfix(PLShieldGenerator __instance)
        {
            int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaShieldMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.ShieldTypes.Count && __instance.ShipStats != null)
            {
                ShieldModManager.Instance.ShieldTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShieldGenerator), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLShieldGenerator __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaShieldMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.ShieldTypes.Count && __instance.ShipStats != null)
            {
                __result = ShieldModManager.Instance.ShieldTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShieldGenerator), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLShieldGenerator __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaShieldMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.ShieldTypes.Count && __instance.ShipStats != null)
            {
                __result = ShieldModManager.Instance.ShieldTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
}
