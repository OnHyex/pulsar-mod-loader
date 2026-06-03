#nullable enable
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static AkMIDIEvent;

namespace PulsarModLoader.Content.Components
{
    public class LegacyComponentCompatabilityFactory<TComp, TLegacyComp> where TComp : PLShipComponent where TLegacyComp : ComponentModBase
    {
        

        private readonly Action<ILGenerator> componentBaseConstructorBuilder;
        private static readonly AssemblyBuilder asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("LegacyComponentCompatabilityDynamicAssembly"), AssemblyBuilderAccess.Run);
        private static readonly ModuleBuilder module = asm.DefineDynamicModule("MainModule");
        private readonly Type? baseEnumType;
        private readonly Dictionary<MethodInfo, MethodInfo> methodOverrides;
        public LegacyComponentCompatabilityFactory(Type? Enum, Action<ILGenerator> componentBaseConstructorBuilder, Action<Dictionary<MethodInfo, MethodInfo>> nonComponentBaseMethods)
        {
            this.componentBaseConstructorBuilder = componentBaseConstructorBuilder;

            methodOverrides = new Dictionary<MethodInfo, MethodInfo>()
                {
                    {
                    // Forward Legacy AddStats to PLShipStats.AddStats()
                        ComponentModMethods.addStats,
                        LowestLevelBaseShipComponentMethods.addStats
                    },
                    {
                    // Forward Legacy FinalLateAddStats to PLShipStats.FinalLateAddStats()
                        ComponentModMethods.finalLateAddStats,
                        LowestLevelBaseShipComponentMethods.finalLateAddStats
                    },
                    {
                    // Forward Legacy LateAddStats to PLShipStats.LateAddStats()
                        ComponentModMethods.lateAddStats,
                        LowestLevelBaseShipComponentMethods.lateAddStats
                    },
                    {
                    // Forward Legacy GetStatLineRight to PLWare.GetStatLineRight()
                        ComponentModMethods.getStatLineRight,
                        LowestLevelBaseShipComponentMethods.getStatLineRight
                    },
                    {
                    // Forward Legacy GetStatLineLeft to PLWare.GetStatLineLeft()
                        ComponentModMethods.getStatLineLeft,
                        LowestLevelBaseShipComponentMethods.getStatLineLeft
                    },
                    {
                    // Forward Legacy OnWarp to PLShipComponent.OnWarp()
                        ComponentModMethods.onWarp,
                        LowestLevelBaseShipComponentMethods.onWarp
                    },
                    {
                    // Forward Legacy Tick to PLShipComponent.Tick()
                        ComponentModMethods.tick,
                        LowestLevelBaseShipComponentMethods.tick
                    }
                };

            //Finds the top level overriden version of each method or defaults to the provided ones
            foreach (MethodInfo modMethod in methodOverrides.Keys.ToArray())
            {
                MethodInfo? method = GetClosestParentMethod(typeof(TComp), methodOverrides[modMethod].Name);
                if (method is not null)
                {
                    methodOverrides[modMethod] = method;
                }
            }

            //Adds / Overwrites methods in the methodOverrides dictionary for that specific legacy component type to map to the most recent override for whatever subclass is being used
            nonComponentBaseMethods(methodOverrides);
        }
        private static readonly MethodInfo objectGetType = typeof(object).GetMethod(nameof(GetType));
        public Type DefineNewType(TLegacyComp legacy)
        {
            TypeBuilder typeBuilder = module.DefineType(legacy.GetType().Name + "_Compat", TypeAttributes.Public | TypeAttributes.Class, typeof(TComp));

            // static field for storing the legacy ComponentModBase Instance
            FieldBuilder legacyField = typeBuilder.DefineField(
                    "_legacy",
                    typeof(TLegacyComp),
                    FieldAttributes.Private | FieldAttributes.Static);

            // constructor taking SubType, Level, SubtypeData
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(int), typeof(int), typeof(short) });
            ILGenerator constructorGenerator = constructor.GetILGenerator();
            componentBaseConstructorBuilder(constructorGenerator);

            // Define legacy interface
            typeBuilder.AddInterfaceImplementation(typeof(ILegacyComponent));

            // Define GetLegacyType method
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(nameof(ILegacyComponent.GetLegacyType), MethodAttributes.Public | MethodAttributes.Virtual, typeof(Type), Type.EmptyTypes);
            ILGenerator il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, legacyField);
            il.Emit(OpCodes.Call, objectGetType);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder,typeof(ILegacyComponent).GetMethod(nameof(ILegacyComponent.GetLegacyType)));

            // Define GetComponentMod method
            methodBuilder = typeBuilder.DefineMethod(nameof(ILegacyComponent.GetComponentMod), MethodAttributes.Public | MethodAttributes.Virtual, typeof(ComponentModBase), Type.EmptyTypes);
            il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, legacyField);
            il.Emit(OpCodes.Castclass, typeof(ComponentModBase));
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, typeof(ILegacyComponent).GetMethod(nameof(ILegacyComponent.GetComponentMod)));

            // Forward ComponentModBaseMethods to base virtual methods of PLShipComponent / PLWare
            foreach (var kvp in methodOverrides)
            {
                DefineMethodForwarder
                    (
                    typeBuilder,
                    legacyField,
                    kvp.Value,
                    kvp.Key
                    );
            }

            Type generatedType = typeBuilder.CreateType();

            generatedType.GetField("_legacy", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, legacy);
            return generatedType;
        }
        private static MethodInfo shipStats = AccessTools.PropertyGetter(typeof(PLShipComponent), nameof(PLShipComponent.ShipStats));
        public static void DefineMethodForwarder(TypeBuilder typeBuilder, FieldInfo legacyField, MethodInfo targetMethod, MethodInfo legacyMethod)
        {
            ParameterInfo[] parameters = targetMethod.GetParameters();

            Type[] paramTypes = Array.ConvertAll(parameters, p => p.ParameterType);

            MethodBuilder method = typeBuilder.DefineMethod(
                    targetMethod.Name,
                    MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    targetMethod.ReturnType,
                    paramTypes
                );

            ILGenerator il = method.GetILGenerator();

            // Load this instance
            il.Emit(OpCodes.Ldarg_0);

            for (short i = 0; i < paramTypes.Length; i++)
            {
                switch (i + 1)
                {
                    case 1: il.Emit(OpCodes.Ldarg_1); break;
                    case 2: il.Emit(OpCodes.Ldarg_2); break;
                    case 3: il.Emit(OpCodes.Ldarg_3); break;
                    default:
                        il.Emit(OpCodes.Ldarg_S, i + 1);
                        break;
                }
            }

            // Call Closest Base Method
            il.Emit(OpCodes.Call, targetMethod);

            if (targetMethod.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Pop);
            }

            // Load static legacy instance
            il.Emit(OpCodes.Ldsfld, legacyField);



            // Load all parameters
            for (short i = 0; i < paramTypes.Length; i++)
            {
                //ComponentMod AddStats and several other methods take the PLShipComponent instead of PLShipStats which is supplied by Ldarg_0
                if (paramTypes[i] == typeof(PLShipStats))
                {
                    il.Emit(OpCodes.Ldarg_0);
                }
                switch (i + 1)
                {
                    case 1: il.Emit(OpCodes.Ldarg_1); break;
                    case 2: il.Emit(OpCodes.Ldarg_2); break;
                    case 3: il.Emit(OpCodes.Ldarg_3); break;
                    default:
                        il.Emit(OpCodes.Ldarg_S, i + 1);
                        break;
                }
            }

            // Call legacy method
            il.Emit(OpCodes.Callvirt, legacyMethod);

            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(
                method,
                targetMethod);
        }
        public static MethodInfo? GetClosestParentMethod(Type subClassType, string methodName)
        {
            Type currentType = subClassType;
            while (currentType != null)
            {
                // Search for the method declared ONLY in the current base type being checked
                var method = currentType.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
                );

                // If a method is found and is virtual/overridden, return it
                if (method != null && (method.IsVirtual || method.IsAbstract))
                {
                    return method;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }
    }
}
