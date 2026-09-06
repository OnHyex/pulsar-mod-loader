using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Content.Components.Thruster;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.InertiaThruster
{
    public class InertiaThrusterModManager : LegacyComponentModManager<PLInertiaThruster, InertiaThrusterMod, EInertiaThrusterType>
    {
        public readonly int VanillaInertiaThrusterMaxType = 0;
        private static InertiaThrusterModManager m_instance = null;
        public readonly List<InertiaThrusterMod> InertiaThrusterTypes = new List<InertiaThrusterMod>();
        public static InertiaThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new InertiaThrusterModManager();
                }
                return m_instance;
            }
        }

        InertiaThrusterModManager() : base((int)ESlotType.E_COMP_INERTIA_THRUSTER)
        {
            VanillaInertiaThrusterMaxType = VanillaMaxType;
            InertiaThrusterTypes = legacyModComps;
        }
        /// <summary>
        /// Finds InertiaThruster type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find InertiaThruster.
        /// </summary>
        /// <param name="InertiaThrusterName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetInertiaThrusterIDFromName(string InertiaThrusterName) => GetIDFromName(InertiaThrusterName);
        protected override void ComponentModConstructor(PLInertiaThruster comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            InertiaThrusterMod thruster = legacyComp as InertiaThrusterMod;
            comp.m_MaxOutput = thruster.MaxOutput;
            comp.m_BaseMaxPower = thruster.MaxPowerUsage_Watts;
        }
        static ConstructorInfo constructor = typeof(PLInertiaThruster).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EInertiaThrusterType), typeof(int) }, null);
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
        public static PLInertiaThruster CreateInertiaThruster(int Subtype, int level)
        {
            return CreateInertiaThruster(Subtype, level, 0);
        }
        public static PLInertiaThruster CreateInertiaThruster(int Subtype, int level, int inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLInertiaThruster comp))
            {
                return comp;
            }
            return new PLInertiaThruster((EInertiaThrusterType)Subtype, level);
        }
    }
    //Converts hashes to InertiaThrusters.
    [HarmonyPatch(typeof(PLInertiaThruster), "CreateInertiaThrusterFromHash")]
    class InertiaThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = InertiaThrusterModManager.CreateInertiaThruster(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
