using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Content.Components.Missile;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.MissionShipComponent
{
    public class MissionShipComponentModManager : LegacyComponentModManager<PLMissionShipComponent, MissionShipComponentMod>
    {
        public readonly int VanillaMissionShipComponentMaxType = 0;
        private static MissionShipComponentModManager m_instance = null;
        public readonly List<MissionShipComponentMod> MissionShipComponentTypes = new List<MissionShipComponentMod>();
        public static MissionShipComponentModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MissionShipComponentModManager();
                }
                return m_instance;
            }
        }

        MissionShipComponentModManager() : base((int)ESlotType.E_COMP_MISSION_COMPONENT, 13)
        {
            VanillaMissionShipComponentMaxType = VanillaMaxType;
            MissionShipComponentTypes = legacyModComps;
        }
        /// <summary>
        /// Finds MissionShipComponent type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find MissionShipComponent.
        /// </summary>
        /// <param name="MissionShipComponentName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetMissionShipComponentIDFromName(string MissionShipComponentName) => GetIDFromName(MissionShipComponentName);
        static ConstructorInfo constructor = typeof(PLMissionShipComponent).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int), typeof(int) }, null);
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
        public static PLMissionShipComponent CreateMissionShipComponent(int Subtype, int level)
        {
            return CreateMissionShipComponent(Subtype, level, 0);
        }
        public static PLMissionShipComponent CreateMissionShipComponent(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLMissionShipComponent comp))
            {
                return comp;
            }
            return new PLMissionShipComponent(Subtype, level);
        }
    }
    //Converts hashes to MissionShipComponents.
    [HarmonyPatch(typeof(PLMissionShipComponent), "CreateMissionComponentFromHash")]
    class MissionShipComponentHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = MissionShipComponentModManager.CreateMissionShipComponent(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
