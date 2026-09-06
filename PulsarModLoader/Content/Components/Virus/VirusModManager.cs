using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.Virus
{
    public class VirusModManager : LegacyComponentModManager<PLVirus, VirusMod, EVirusType>
    {
        public readonly int VanillaVirusMaxType = 0;
        private static VirusModManager m_instance = null;
        public readonly List<VirusMod> VirusTypes = new List<VirusMod>();
        public static VirusModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new VirusModManager();
                }
                return m_instance;
            }
        }

        VirusModManager() : base((int)ESlotType.E_COMP_VIRUS)
        {
            VanillaVirusMaxType = VanillaMaxType;
            VirusTypes = legacyModComps;
        }
        /// <summary>
        /// Finds Virus type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Virus.
        /// </summary>
        /// <param name="VirusName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetVirusIDFromName(string VirusName) => GetIDFromName(VirusName);
        protected override void ComponentModConstructor(PLVirus comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            VirusMod virus = legacyComp as VirusMod;
            comp.InfectionTimeLimitMs = virus.InfectionTimeLimitMs;
        }
        static ConstructorInfo constructor = typeof(PLVirus).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EVirusType), typeof(int), typeof(short) }, null);
        protected override void BaseClassConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }
        protected override void ModComponentSubtypeMethods(Dictionary<MethodInfo, MethodInfo> methodOverrides)
        {
            return;
        }
        public static PLVirus CreateVirus(int Subtype, int level)
        {
            return CreateVirus(Subtype, level, 0);
        }
        public static PLVirus CreateVirus(int Subtype, int level, short inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLVirus comp))
            {
                return comp;
            }
            return new PLVirus((EVirusType)Subtype, level, inSubTypeData);
        }
    }
    //Converts hashes to Viruss.
    [HarmonyPatch(typeof(PLVirus), "CreateVirusFromHash")]
    class VirusHashFix
    {
        static bool Prefix(int inSubType, int inLevel, short inSubTypeData, ref PLShipComponent __result)
        {
            __result = VirusModManager.CreateVirus(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
