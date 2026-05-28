#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Content.Components
{
    public class LegacyComponentCompatabilityFactory<TComp, TLegacyComp> where TComp : PLShipComponent where TLegacyComp : ComponentModBase
    {
        

        private readonly Action<ILGenerator> componentBaseConstructorBuilder;
        private readonly Action<TypeBuilder,FieldBuilder> nonComponentBaseMethodAddition;
        private readonly AssemblyBuilder asm;
        private readonly ModuleBuilder module;
        private readonly Type? baseEnumType;
        public LegacyComponentCompatabilityFactory(Type? Enum, Action<ILGenerator> componentBaseConstructorBuilder, Action<TypeBuilder, FieldBuilder> nonComponentBaseMethodAddition)
        {
            this.componentBaseConstructorBuilder = componentBaseConstructorBuilder;
            this.nonComponentBaseMethodAddition = nonComponentBaseMethodAddition;
            this.baseEnumType = Enum;

            var asmName = new AssemblyName($"{typeof(TComp).Name}" + "DynamicAssembly");

            asm = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);

            module = asm.DefineDynamicModule("MainModule");
        }
        private static readonly ReadOnlyDictionary<MethodInfo, MethodInfo> componentModBaseMethodOverrides;
        private static readonly MethodInfo objectGetType;
    
        static LegacyComponentCompatabilityFactory()
        {
            objectGetType = typeof(object).GetMethod(nameof(GetType));
            componentModBaseMethodOverrides = new ReadOnlyDictionary<MethodInfo, MethodInfo>(new Dictionary<MethodInfo, MethodInfo>()
                {
                    {
                    // Forward Legacy AddStats to PLShipStats.AddStats()
                        typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.AddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                        typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.AddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    },
                    {
                    // Forward Legacy FinalLateAddStats to PLShipStats.FinalLateAddStats()
                        typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.FinalLateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                        typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.FinalLateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    },
                    {
                    // Forward Legacy LateAddStats to PLShipStats.LateAddStats()
                        typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.LateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                        typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.LateAddStats), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    },
                    {
                    // Forward Legacy GetStatLineRight to PLWare.GetStatLineRight()
                        typeof(PLShipComponent).GetMethod(nameof(PLWare.GetStatLineRight), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                        typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.GetStatLineRight), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    },
                    {
                    // Forward Legacy GetStatLineLeft to PLWare.GetStatLineLeft()
                        typeof(PLShipComponent).GetMethod(nameof(PLWare.GetStatLineLeft), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                        typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.GetStatLineLeft), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    },
                    {
                    // Forward Legacy OnWarp to PLShipComponent.OnWarp()
                        typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.OnWarp), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                        typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.OnWarp), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    },
                    {
                    // Forward Legacy Tick to PLShipComponent.Tick()
                        typeof(PLShipComponent).GetMethod(nameof(PLShipComponent.Tick), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                        typeof(ComponentModBase).GetMethod(nameof(ComponentModBase.Tick), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    }
                }
                );
        }

        public Type DefineNewType(TLegacyComp legacy)
        {
            TypeBuilder typeBuilder = module.DefineType(typeof(TLegacyComp).Name + "_Compat", TypeAttributes.Public | TypeAttributes.Class, typeof(TComp));

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
            foreach (var kvp in componentModBaseMethodOverrides)
            {
                DefineMethodForwarder(
                    typeBuilder,
                    legacyField,
                    kvp.Key,
                    kvp.Value
                    );
            }

            //DefineMethodForwarders for methods not present in the base ComponentModBase class
            nonComponentBaseMethodAddition(typeBuilder, legacyField);

            Type generatedType = typeBuilder.CreateType();

            generatedType.GetField("_legacy").SetValue(null, legacy);
            return generatedType;
        }

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

            // Call Closest Base Method
            MethodInfo? closestMethodInfo = GetClosestParentMethod(typeBuilder.BaseType, targetMethod.Name);
            if (closestMethodInfo is not null)
            {
                il.Emit(OpCodes.Call, closestMethodInfo);
            }
            else
            {
                il.Emit(OpCodes.Call, targetMethod);
            }


            // Load static legacy instance
            il.Emit(OpCodes.Ldsfld, legacyField);

            // Loads this instance
            il.Emit(OpCodes.Ldarg_0);

            // Load all parameters
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

            // Call legacy method
            il.Emit(OpCodes.Callvirt, legacyMethod);

            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(
                method,
                targetMethod);
        }
        public static MethodInfo? GetClosestParentMethod(Type subClassType, string methodName)
        {
            Type currentType = subClassType.BaseType;

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
