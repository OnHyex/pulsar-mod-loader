using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Content.Components
{
    internal static class ComponentModMethods
    {
        internal static readonly MethodInfo addStats = typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.AddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo finalLateAddStats = typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.FinalLateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo lateAddStats = typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.LateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo getStatLineRight = typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.GetStatLineRight), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo getStatLineLeft = typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.GetStatLineLeft), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo onWarp = typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.OnWarp), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo tick = typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.Tick), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
    }
    internal static class LowestLevelBaseShipComponentMethods
    {
        internal static readonly MethodInfo addStats = typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.AddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo finalLateAddStats = typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.FinalLateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo lateAddStats = typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.LateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo getStatLineRight = typeof(PLWare).GetMethod(nameof(PLWare.GetStatLineRight), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo getStatLineLeft = typeof(PLWare).GetMethod(nameof(PLWare.GetStatLineLeft), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo onWarp = typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.OnWarp), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        internal static readonly MethodInfo tick = typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.Tick), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
    }
}
