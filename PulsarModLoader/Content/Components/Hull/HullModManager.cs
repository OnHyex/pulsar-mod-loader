using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Content.Components.Shield;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.Hull
{
    public class HullModManager : LegacyComponentModManager<PLHull, HullMod, EHullType>
    {
        public readonly int VanillaHullMaxType = 0;
        private static HullModManager m_instance = null;
        public readonly List<HullMod> HullTypes = new List<HullMod>();
        public static HullModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new HullModManager();
                }
                return m_instance;
            }
        }

        HullModManager() : base((int)ESlotType.E_COMP_HULL)
        {
            HullTypes = legacyModComps;
            VanillaHullMaxType = VanillaMaxType;
        }
        protected override void ComponentModConstructor(PLHull comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            HullMod hull = legacyComp as HullMod;
            comp.Max = hull.HullMax;
            comp.Armor = hull.Armor;
            comp.Defense = hull.Defense;
        }
        static ConstructorInfo constructor = typeof(PLHull).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EHullType), typeof(int) }, null);
        protected override void BaseClassConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }
        /// <summary>
        /// Finds Hull type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Hull.
        /// </summary>
        /// <param name="HullName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetHullIDFromName(string HullName) => GetIDFromName(HullName);
        //No Methods extra
        protected override void ModComponentSubtypeMethods(Dictionary<MethodInfo, MethodInfo> methodOverrides)
        {
            return;
        }
        public static PLHull CreateHull(int Subtype, int level)
        {
            return CreateHull(Subtype, level, 0);
        }
        public static PLHull CreateHull(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLHull comp))
            {
                return comp;
            }
            return new PLHull((EHullType)Subtype, level);
        }
    }
    //Converts hashes to Hulls.
    [HarmonyPatch(typeof(PLHull), "CreateHullFromHash")]
    class HullHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = HullModManager.CreateHull(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
    //[HarmonyPatch(typeof(PLHull), "Tick")]
    //class TickPatch
    //{
    //    static void Postfix(PLHull __instance)
    //    {
    //        int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && __instance.ShipStats != null)
    //        {
    //            HullModManager.Instance.HullTypes[subtypeformodded].Tick(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLHull), "GetStatLineLeft")]
    //class LeftDescFix 
    //{
    //    static void Postfix(PLHull __instance, ref string __result) 
    //    {
    //        int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = HullModManager.Instance.HullTypes[subtypeformodded].GetStatLineLeft(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLHull), "GetStatLineRight")]
    //class RightDescFix
    //{
    //    static void Postfix(PLHull __instance, ref string __result)
    //    {
    //        int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && __instance.ShipStats != null)
    //        {
    //            __result = HullModManager.Instance.HullTypes[subtypeformodded].GetStatLineRight(__instance);
    //        }
    //    }
    //}
}
