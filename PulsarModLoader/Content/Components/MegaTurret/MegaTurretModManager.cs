using HarmonyLib;
using ProtoBuf.Meta;
using PulsarModLoader.Content.Components.AutoTurret;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Cinemachine.DocumentationSortingAttribute;

namespace PulsarModLoader.Content.Components.MegaTurret
{
    public class MegaTurretModManager : LegacyInstantiatableComponentModManager<PLTurret, MegaTurretMod>
    {
        public readonly int VanillaMegaTurretMaxType = 0;
        private static MegaTurretModManager m_instance = null;
        public readonly List<MegaTurretMod> MegaTurretTypes = new List<MegaTurretMod>();
        public static MegaTurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MegaTurretModManager();
                }
                return m_instance;
            }
        }

        MegaTurretModManager() : base((int)ESlotType.E_COMP_MAINTURRET, 8)
        {

        }
        protected override string GetLegacyComponentName(MegaTurretMod comp)
        {
            return comp.Name;
        }
        protected override Type GetComponentType(MegaTurretMod comp)
        {
            Type type = GetConstructorTypeFromMethodBody(AccessTools.PropertyGetter(comp.GetType(), nameof(MegaTurretMod.PLMegaTurret)));
            if (type is not null)
            {
                return type;
            }
            throw new Exception($"What is going on in {comp.Name} get component property");
        }
        protected override bool ValidTypeCheck<T>(Type type)
        {
            return base.ValidTypeCheck<T>(type) && type.GetCustomAttribute<TurretType>()?.type == EModdedTurretType.Mega;
        }
        /// <summary>
        /// Finds MegaTurret type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find MegaTurret.
        /// </summary>
        /// <param name="MegaTurretName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetMegaTurretIDFromName(string MegaTurretName) => GetIDFromName(MegaTurretName);
        internal PLTurret CreateModdedTurret(int Subtype, int level, int Subtypedata)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLTurret comp))
            {
                return comp;
            }
            return null;
        }
    }
    //Converts hashes to MegaTurrets.
    [HarmonyPatch(typeof(PLMegaTurret), "CreateMainTurretFromHash")]
    class MegaTurretHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = MegaTurretModManager.Instance.CreateModdedTurret(inSubType, inLevel, inSubTypeData);
            if (__result is not null)
                return false;
            return true;
        }
    }
}
