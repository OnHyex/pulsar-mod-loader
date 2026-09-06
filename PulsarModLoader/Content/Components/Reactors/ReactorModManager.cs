using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.Reactor
{
    public class ReactorModManager : LegacyComponentModManager<PLReactor, ReactorMod, EReactorType>
    {
        public readonly int VanillaReactorMaxType = 0;
        private static ReactorModManager m_instance = null;
        public readonly List<ReactorMod> ReactorTypes = new List<ReactorMod>();
        public static ReactorModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ReactorModManager();
                }
                return m_instance;
            }
        }

        ReactorModManager() : base((int)ESlotType.E_COMP_REACTOR)
        {
            VanillaReactorMaxType = VanillaMaxType;
            ReactorTypes = legacyModComps;
        }
        /// <summary>
        /// Finds reactor type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find reactor.
        /// </summary>
        /// <param name="ReactorName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetReactorIDFromName(string ReactorName) => GetIDFromName(ReactorName);
        protected override void ComponentModConstructor(PLReactor comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            ReactorMod ReactorType = legacyComp as ReactorMod;
            comp.OriginalEnergyOutputMax = ReactorType.EnergyOutputMax;
            comp.EnergyOutputMax = ReactorType.EnergyOutputMax;
            comp.EnergySignatureAmt = ReactorType.EnergySignatureAmount;
            comp.TempMax = ReactorType.MaxTemp;
            comp.EmergencyCooldownTime = ReactorType.EmergencyCooldownTime;
            comp.HeatOutput = ReactorType.HeatOutput;
        }
        static ConstructorInfo constructor = typeof(PLReactor).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EReactorType), typeof(int) }, null);
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
        public static PLReactor CreateReactor(int Subtype, int level)
        {
            return CreateReactor(Subtype, level, 0);
        }
        public static PLReactor CreateReactor(int Subtype, int level, int inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLReactor comp))
            {
                return comp;
            }
            return new PLReactor((EReactorType)Subtype, level);
        }
    }
    //Converts hashes to reactors.
    [HarmonyPatch(typeof(PLReactor), "CreateReactorFromHash")]
    class HashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ReactorModManager.CreateReactor(inSubType, inLevel);
            return false;
        }
    }
}
