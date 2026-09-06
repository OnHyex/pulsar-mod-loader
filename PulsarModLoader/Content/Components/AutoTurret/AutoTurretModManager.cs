using HarmonyLib;
using PulsarModLoader.Content.Components.Shield;
using PulsarModLoader.Content.Components.Turret;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PulsarModLoader.Content.Components.AutoTurret
{
    /// <summary>
    /// Manages Modded AutoTurrets
    /// </summary>
    public class AutoTurretModManager : LegacyInstantiatableComponentModManager<PLTurret, AutoTurretMod, EAutoTurretType>
    {
        public readonly int VanillaAutoTurretMaxType = 0;
        private static AutoTurretModManager m_instance = null;
        public readonly List<AutoTurretMod> AutoTurretTypes = new List<AutoTurretMod>();

        /// <summary>
        /// Static Manager Instance
        /// </summary>
        public static AutoTurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new AutoTurretModManager();
                }
                return m_instance;
            }
        }

        AutoTurretModManager() : base((int)ESlotType.E_COMP_AUTO_TURRET)
        {
            VanillaAutoTurretMaxType = 1;
            AutoTurretTypes = legacyModComps;
        }
        protected override string GetLegacyComponentName(AutoTurretMod comp)
        {
            return comp.Name;
        }
        protected override Type GetComponentType(AutoTurretMod comp)
        {
            Type type = GetConstructorTypeFromMethodBody(AccessTools.PropertyGetter(comp.GetType(), nameof(AutoTurretMod.PLAutoTurret)));
            if (type is not null)
            {
                return type;
            }
            throw new Exception($"What is going on in {comp.Name} get component property");
        }
        protected override bool ValidTypeCheck<T>(Type type)
        {
            return base.ValidTypeCheck<T>(type) && type.GetCustomAttribute<TurretType>()?.type == EModdedTurretType.Auto;
        }
        /// <summary>
        /// Finds AutoTurret type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find AutoTurret.
        /// </summary>
        /// <param name="AutoTurretName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetAutoTurretIDFromName(string AutoTurretName) => GetIDFromName(AutoTurretName);

        public static PLTurret CreateAutoTurret(int Subtype, int level, int Subtypedata = 0)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLTurret comp))
            {
                return comp;
            }
            return new PLAutoTurret(level, Subtypedata);
        }

        //Converts hashes to AutoTurrets.
        [HarmonyPatch(typeof(PLAutoTurret), "CreateAutoTurretFromHash")]
        class AutoTurretHashFix
        {
            static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
            {
                __result = AutoTurretModManager.CreateAutoTurret(inSubType, inLevel, inSubTypeData);
                return false;
            }
        }
    }
}
