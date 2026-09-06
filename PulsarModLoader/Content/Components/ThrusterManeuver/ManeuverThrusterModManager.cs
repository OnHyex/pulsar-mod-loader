using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Content.Components.InertiaThruster;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.ManeuverThruster
{
    public class ManeuverThrusterModManager : LegacyComponentModManager<PLManeuverThruster, ManeuverThrusterMod, EManeuverThrusterType>
    {
        public readonly int VanillaManeuverThrusterMaxType = 0;
        private static ManeuverThrusterModManager m_instance = null;
        public readonly List<ManeuverThrusterMod> ManeuverThrusterTypes = new List<ManeuverThrusterMod>();
        public static ManeuverThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ManeuverThrusterModManager();
                }
                return m_instance;
            }
        }

        ManeuverThrusterModManager() : base((int)ESlotType.E_COMP_MANEUVER_THRUSTER)
        {
            VanillaManeuverThrusterMaxType = VanillaMaxType;
            ManeuverThrusterTypes = legacyModComps;
        }
        /// <summary>
        /// Finds ManeuverThruster type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find ManeuverThruster.
        /// </summary>
        /// <param name="ManeuverThrusterName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetManeuverThrusterIDFromName(string ManeuverThrusterName) => GetIDFromName(ManeuverThrusterName);
        protected override void ComponentModConstructor(PLManeuverThruster comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            ManeuverThrusterMod thruster = legacyComp as ManeuverThrusterMod;
            comp.m_MaxOutput = thruster.MaxOutput;
            comp.m_BaseMaxPower = thruster.MaxPowerUsage_Watts;
        }
        static ConstructorInfo constructor = typeof(PLManeuverThruster).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EManeuverThrusterType), typeof(int) }, null);
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
        public static PLManeuverThruster CreateManeuverThruster(int Subtype, int level)
        {
            return CreateManeuverThruster(Subtype, level, 0);
        }
        public static PLManeuverThruster CreateManeuverThruster(int Subtype, int level, int inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLManeuverThruster comp))
            {
                return comp;
            }
            return new PLManeuverThruster((EManeuverThrusterType)Subtype, level);
        }
    }
    //Converts hashes to ManeuverThrusters.
    [HarmonyPatch(typeof(PLManeuverThruster), "CreateManeuverThrusterFromHash")]
    class ManeuverThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = ManeuverThrusterModManager.CreateManeuverThruster(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
