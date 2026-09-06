using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.Thruster
{
    public class ThrusterModManager : LegacyComponentModManager<PLThruster, ThrusterMod, EThrusterType>
    {
        public readonly int VanillaThrusterMaxType = 0;
        private static ThrusterModManager m_instance = null;
        public readonly List<ThrusterMod> ThrusterTypes = new List<ThrusterMod>();
        public static ThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ThrusterModManager();
                }
                return m_instance;
            }
        }

        ThrusterModManager() : base((int)ESlotType.E_COMP_THRUSTER)
        {
            VanillaThrusterMaxType = VanillaMaxType;
            ThrusterTypes = legacyModComps;
        }
        /// <summary>
        /// Finds Thruster type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Thruster.
        /// </summary>
        /// <param name="ThrusterName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetThrusterIDFromName(string ThrusterName) => GetIDFromName(ThrusterName);
        protected override void ComponentModConstructor(PLThruster comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            ThrusterMod thruster = legacyComp as ThrusterMod;
            comp.m_MaxOutput = thruster.MaxOutput;
            comp.m_BaseMaxPower = thruster.MaxPowerUsage_Watts;
        }
        static ConstructorInfo constructor = typeof(PLThruster).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EThrusterType), typeof(int) }, null);
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
        public static PLThruster CreateThruster(int Subtype, int level)
        {
            return CreateThruster(Subtype, level, 0);
        }
        public static PLThruster CreateThruster(int Subtype, int level, int inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLThruster comp))
            {
                return comp;
            }
            return new PLThruster((EThrusterType)Subtype, level);
        }
    }
    //Converts hashes to Thrusters.
    [HarmonyPatch(typeof(PLThruster), "CreateThrusterFromHash")]
    class ThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = ThrusterModManager.CreateThruster(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
