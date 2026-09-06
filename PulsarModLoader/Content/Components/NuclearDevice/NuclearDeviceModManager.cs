using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace PulsarModLoader.Content.Components.NuclearDevice
{
    public class NuclearDeviceModManager : LegacyComponentModManager<PLNuclearDevice, NuclearDeviceMod, ENuclearDeviceType>
    {
        public readonly int VanillaNuclearDeviceMaxType = 0;
        private static NuclearDeviceModManager m_instance = null;
        public readonly List<NuclearDeviceMod> NuclearDeviceTypes = new List<NuclearDeviceMod>();
        public static NuclearDeviceModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new NuclearDeviceModManager();
                }
                return m_instance;
            }
        }

        NuclearDeviceModManager() : base((int)ESlotType.E_COMP_NUCLEARDEVICE)
        {
            VanillaNuclearDeviceMaxType = VanillaMaxType;
            NuclearDeviceTypes = legacyModComps;
        }
        /// <summary>
        /// Finds NuclearDevice type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find NuclearDevice.
        /// </summary>
        /// <param name="NuclearDeviceName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetNuclearDeviceIDFromName(string NuclearDeviceName) => GetIDFromName(NuclearDeviceName);
        protected override void ComponentModConstructor(PLNuclearDevice comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            NuclearDeviceMod nuke = legacyComp as NuclearDeviceMod;
            comp.m_MaxDamage = nuke.MaxDamage;
            comp.m_Range = nuke.Range;
            comp.m_FuelBurnRate = nuke.FuelBurnRate;
            comp.m_TurnRate = nuke.TurnRate;
            comp.m_IntimidationBonus = nuke.IntimidationBonus;
            comp.m_Health = nuke.Health;
        }

        static ConstructorInfo constructor = typeof(PLNuclearDevice).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ENuclearDeviceType), typeof(int) }, null);
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
        public static PLNuclearDevice CreateNuclearDevice(int Subtype, int level)
        {
            return CreateNuclearDevice(Subtype, level, 0);
        }
        public static PLNuclearDevice CreateNuclearDevice(int Subtype, int level, int inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLNuclearDevice comp))
            {
                return comp;
            }
            return new PLNuclearDevice((ENuclearDeviceType)Subtype, level);
        }
    }
    //Converts hashes to NuclearDevices.
    [HarmonyPatch(typeof(PLNuclearDevice), "CreateNuclearDeviceFromHash")]
    class NuclearDeviceHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = NuclearDeviceModManager.CreateNuclearDevice(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
    //[HarmonyPatch(typeof(PLNuclearDevice), "GetStatLineLeft")]
    //class LeftDescFix
    //{
    //    static void Postfix(PLNuclearDevice __instance, ref string __result)
    //    {
    //        int subtypeformodded = __instance.SubType - NuclearDeviceModManager.Instance.VanillaNuclearDeviceMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < NuclearDeviceModManager.Instance.NuclearDeviceTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = NuclearDeviceModManager.Instance.NuclearDeviceTypes[subtypeformodded].GetStatLineLeft(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLNuclearDevice), "GetStatLineRight")]
    //class RightDescFix
    //{
    //    static void Postfix(PLNuclearDevice __instance, ref string __result)
    //    {
    //        int subtypeformodded = __instance.SubType - NuclearDeviceModManager.Instance.VanillaNuclearDeviceMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < NuclearDeviceModManager.Instance.NuclearDeviceTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = NuclearDeviceModManager.Instance.NuclearDeviceTypes[subtypeformodded].GetStatLineRight(__instance);
    //        }
    //    }
    //}
}
