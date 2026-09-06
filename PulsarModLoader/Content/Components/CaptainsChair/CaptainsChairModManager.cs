using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.CaptainsChair
{
    /// <summary>
    /// Manages Modded Captains Chairs
    /// </summary>
    public class CaptainsChairModManager : LegacyComponentModManager<PLCaptainsChair, CaptainsChairMod, ECaptainsChairType>
    {
        public readonly int VanillaCaptainsChairMaxType = 0;
        private static CaptainsChairModManager m_instance = null;
        public readonly List<CaptainsChairMod> CaptainsChairTypes = new List<CaptainsChairMod>();

        /// <summary>
        /// Static Manager Instance.
        /// </summary>
        public static CaptainsChairModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CaptainsChairModManager();
                }
                return m_instance;
            }
        }

        public CaptainsChairModManager() : base((int)ESlotType.E_COMP_CAPTAINS_CHAIR)
        {
            CaptainsChairTypes = legacyModComps;
            VanillaCaptainsChairMaxType = VanillaMaxType;
        }
        /// <summary>
        /// Finds CaptainsChair type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find CaptainsChair.
        /// </summary>
        /// <param name="CaptainsChairName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetCaptainsChairIDFromName(string CaptainsChairName) => GetIDFromName(CaptainsChairName);
        //No Extra methods from CaptainsChairMod
        protected override void ComponentModConstructor(PLCaptainsChair comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
        }
        static ConstructorInfo constructor = typeof(PLCaptainsChair).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ECaptainsChairType), typeof(int), typeof(short) }, null);
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
        public static PLCaptainsChair CreateCaptainsChair(int Subtype, int level)
        {
            return CreateCaptainsChair(Subtype, level, 0);
        }
        public static PLCaptainsChair CreateCaptainsChair(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLCaptainsChair comp))
            {
                return comp;
            }

            comp = new PLCaptainsChair((ECaptainsChairType)Subtype, level, (short)Subtypedata);
            return comp;
        }

        //Converts hashes to CaptainsChairs.
        [HarmonyPatch(typeof(PLCaptainsChair), "CreateCaptainsChairFromHash")]
        class CaptainsChairHashFix
        {
            static bool Prefix(int inSubType, int inLevel, short inSubTypeData, ref PLShipComponent __result)
            {
                __result = CaptainsChairModManager.CreateCaptainsChair(inSubType, inLevel, inSubTypeData);
                return false;
            }
        }
        //[HarmonyPatch(typeof(PLCaptainsChair), "LateAddStats")]
        //class CaptainsChairLateAddStatsPatch
        //{
        //    static void Postfix(PLShipStats inStats, PLCaptainsChair __instance)
        //    {
        //        int subtypeformodded = __instance.SubType - CaptainsChairModManager.Instance.VanillaCaptainsChairMaxType;
        //        if (subtypeformodded > -1 && subtypeformodded < CaptainsChairModManager.Instance.CaptainsChairTypes.Count && inStats != null)
        //        {
        //            CaptainsChairModManager.Instance.CaptainsChairTypes[subtypeformodded].LateAddStats(__instance);
        //        }
        //    }
        //}
    }
}
