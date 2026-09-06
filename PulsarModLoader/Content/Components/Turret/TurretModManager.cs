using HarmonyLib;
using PulsarModLoader.Content.Components.MegaTurret;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarModLoader.Content.Components.Turret
{
    public class TurretModManager : LegacyInstantiatableComponentModManager<PLTurret, TurretMod, ETurretType>
    {
        public readonly int VanillaTurretMaxType = 0;
        private static TurretModManager m_instance = null;
        public readonly List<TurretMod> TurretTypes = new List<TurretMod>();
        public static TurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TurretModManager();
                }
                return m_instance;
            }
        }

        TurretModManager() : base((int)ESlotType.E_COMP_TURRET)
        {
            VanillaTurretMaxType = VanillaMaxType;
            TurretTypes = legacyModComps;
        }
        protected override string GetLegacyComponentName(TurretMod comp)
        {
            return comp.Name;
        }
        protected override Type GetComponentType(TurretMod comp)
        {
            Type type = GetConstructorTypeFromMethodBody(AccessTools.PropertyGetter(comp.GetType(), nameof(TurretMod.PLTurret)));
            if (type is not null)
            {
                return type;
            }
            throw new Exception($"What is going on in {comp.Name} get component property");
        }
        protected override bool ValidTypeCheck<T>(Type type)
        {
            return base.ValidTypeCheck<T>(type) && type.GetCustomAttribute<TurretType>()?.type == EModdedTurretType.Normal;
        }
        /// <summary>
        /// Finds Turret type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Turret.
        /// </summary>
        /// <param name="TurretName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetTurretIDFromName(string TurretName) => GetIDFromName(TurretName);
        internal PLTurret CreateModdedTurret(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLTurret comp))
            {
                return comp;
            }
            return null;
        }
    }
    //Converts hashes to Turrets.
    [HarmonyPatch(typeof(PLTurret), "CreateTurretFromHash")]
    class TurretHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = TurretModManager.Instance.CreateModdedTurret(inSubType, inLevel, inSubTypeData);
            if (__result is not null)
                return false;
            return true;
        }
    }
    /*[HarmonyPatch(typeof(PLTurret), "LateAddStats")]
    class TurretLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLTurret __instance)
        {
            int subtypeformodded = __instance.SubType - TurretModManager.Instance.VanillaTurretMaxType;
            if (subtypeformodded > -1 && subtypeformodded < TurretModManager.Instance.TurretTypes.Count && inStats != null)
            {
                TurretModManager.Instance.TurretTypes[subtypeformodded].LateAddStats(inStats);
            }
        }
    }*/
}
