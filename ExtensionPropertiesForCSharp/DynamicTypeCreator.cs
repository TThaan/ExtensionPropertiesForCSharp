﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ExtensionPropertiesForCSharp
{
    class DynamicTypeCreator
    {
        static int counter = 0;
        static ModuleBuilder mb;
        static AssemblyName assemblyName;
        static AssemblyBuilder ab;
        public static AssemblyBuilder AssemblyBuilder { get { return ab; } }

        public static ref T ExtensionOf<T>(T core, string id = default)
        {
            if (ab == null) { ab = BuildAssemblyAndModule(); }  //Into constructor of DynamicTypeCreator!

            FieldBuilder fb_typeOfGenericParameter;
            Type baseClass = CreateBaseClass(core, id, out fb_typeOfGenericParameter);

            GenericTypeParameterBuilder K;
            TypeBuilder tb = GetTypeBuilder(id, out K, typeof(T), baseClass);
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public);

            FieldBuilder fb_instance = tb.DefineField("instance", tb, FieldAttributes.Private | FieldAttributes.Static);
            FieldBuilder fb_tag = tb.DefineField("tag", typeof(object), FieldAttributes.Private);
            FieldBuilder fb_cell = tb.DefineField("cell", K.MakeArrayType(), FieldAttributes.Private);

            PropertyBuilder pb_TypeOfGenericParameter = GetPropertyBuilder_TypeOfGenericParameter(tb, K, fb_typeOfGenericParameter);
            PropertyBuilder pb_Cell = GetPropertyBuilder_Cell(tb, K, fb_cell);
            PropertyBuilder pb_Tag = GetPropertyBuilder_Tag(tb, fb_tag);

            MethodBuilder mb_Instantiate = GetMethodBuilder_Instantiate(tb, K, constructor, fb_instance, fb_cell);
            MethodBuilder mb_Bind = GetMethodBuider_Bind(tb, K, fb_cell);

            Type type = tb.CreateType();            
            Type genericType = type.MakeGenericType(new[] { typeof(T) });
            /*
            MemberInfo[] mi_All = genericType.GetMembers();
            FieldInfo[] fi_All = genericType.GetFields();
            FieldInfo fi = genericType.GetField("typeOfGenericParameter", BindingFlags.Static);
            //fi.GetValue(genericType);*/

            PropertyInfo[] pi_All = genericType.GetProperties();
            PropertyInfo pi = genericType.GetProperty("TypeOfGenericParameter");
            //Console.WriteLine(pi.GetValue(genericType));
            
            dynamic instance = GetInstance<T>(type);
            //Console.WriteLine(instance.TypeOfGenericParameter);

            IExtendable<T> IExtendedObject = (IExtendable<T>)instance;
            IExtendedObject.Cell[0] = core;
            //Console.WriteLine(IExtendedObject.TypeOfGenericParameter);
            return ref IExtendedObject.GetReferenceToCore(core);
        }

        public static object GetExtendedObject<T>(T core, string id = default)
        {
            if (id == default)
            {
                string s = typeof(T).Name.ToString();
                var allTypes = AssemblyBuilder.GetTypes();
                var types = AssemblyBuilder.GetTypes().Where(x => x.Name.Contains($"{typeof(T).Name.ToString()}Extended") && !x.Name.Contains($"BaseOf"));

                foreach (Type type in types)
                {
                    dynamic instance = GetInstance<T>(type);
                    if (instance != null && instance.Cell[0].Equals(core))
                    {
                        return instance;
                    }
                }
                throw new ArgumentException("No ExtendedObject with a matching core found.");
            }
            else
            {
                id = $"{id}_{typeof(T).Name}Extended";
                Type type = AssemblyBuilder.GetType(id);
                if (type == null)
                { throw new ArgumentException("No object with this name found in ExtendIT Assembly."); }
                dynamic instance = GetInstance<T>(type);
                return instance;
            }
            throw new ArgumentException("You were not supposed to reach this line of code!");
        }
        private static object GetInstance<T>(Type type)     //object vs typebuilder
        {
            Type genericType = type.MakeGenericType(new[] { typeof(T) });
            MethodInfo instantiate = genericType.GetMethod("Instantiate");
            return instantiate.Invoke(genericType, null);
        }
        
        private static AssemblyBuilder BuildAssemblyAndModule()
        {
            assemblyName = new AssemblyName("ExtendIT");
            ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            mb = ab.DefineDynamicModule("ExtendedIT_Module", true);
            return ab;
        }

        private static Type CreateBaseClass<T>(T core, string id, out FieldBuilder fb_typeOfGenericParameter)
        {
            TypeBuilder tb = GetTypeBuilder_Base(id, typeof(T));
            fb_typeOfGenericParameter = tb.DefineField("typeOfGenericParameter", typeof(Type), FieldAttributes.Public | FieldAttributes.Static);     //kein protected vorhanden, stattdessen: Assembly a.o.

            Type type = tb.CreateType();
            return type;
        }

        private static TypeBuilder GetTypeBuilder(string id, out GenericTypeParameterBuilder K, Type genericType, Type baseClass)
        {
            if (id == default) { id = $"{genericType.Name}Extended_{counter}"; }
            else { id = $"{id}_{genericType.Name}Extended"; }
            counter++;
            TypeBuilder tb = mb.DefineType(id, TypeAttributes.Class | TypeAttributes.Public, baseClass);
            tb.AddInterfaceImplementation(typeof(IExtendable<>));
            K = tb.DefineGenericParameters(new[] { "K" })[0];
            return tb;
        }
        private static TypeBuilder GetTypeBuilder_Base(string id, Type genericType)
        {
            if (id == default) { id = $"BaseOf{genericType.Name}Extended_{counter}"; }
            else { id = $"BaseOf{id}_{genericType.Name}Extended"; }

            TypeBuilder tb = mb.DefineType(id, TypeAttributes.Class);
            return tb;
        }

        private static PropertyBuilder GetPropertyBuilder_TypeOfGenericParameter(TypeBuilder tb, GenericTypeParameterBuilder K, FieldBuilder fb_typeOfGenericParameter)
        {

            PropertyBuilder pb_TypeOfGenericParameter = tb.DefineProperty("TypeOfGenericParameter", PropertyAttributes.HasDefault, typeof(Type), null);

            MethodBuilder typeOfGenericParameterGetter = tb.DefineMethod(
                "get_TypeOfGenericParameter",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(Type),
                Type.EmptyTypes);

            ILGenerator cellGetterIL = typeOfGenericParameterGetter.GetILGenerator();
            cellGetterIL.Emit(OpCodes.Ldarg_0);
            cellGetterIL.Emit(OpCodes.Ldsfld, fb_typeOfGenericParameter);
            cellGetterIL.Emit(OpCodes.Ret);

            pb_TypeOfGenericParameter.SetGetMethod(typeOfGenericParameterGetter);
            return pb_TypeOfGenericParameter;
        }
        private static PropertyBuilder GetPropertyBuilder_Cell(TypeBuilder tb, GenericTypeParameterBuilder K, FieldBuilder fb_cell)
        {
            PropertyBuilder pb_Cell = tb.DefineProperty("Cell", PropertyAttributes.HasDefault, K.MakeArrayType(), null);

            MethodBuilder cellGetter = tb.DefineMethod(
                "get_Cell",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                K.MakeArrayType(),
                Type.EmptyTypes);

            ILGenerator cellGetterIL = cellGetter.GetILGenerator();
            cellGetterIL.Emit(OpCodes.Ldarg_0);
            cellGetterIL.Emit(OpCodes.Ldfld, fb_cell);
            cellGetterIL.Emit(OpCodes.Ret);

            MethodBuilder cellSetter = tb.DefineMethod(
                "set_Cell",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                null,
                new Type[] { K.MakeArrayType() });

            ILGenerator cellSetterIL = cellSetter.GetILGenerator();
            cellSetterIL.Emit(OpCodes.Ldarg_0);
            cellSetterIL.Emit(OpCodes.Ldarg_1);
            cellSetterIL.Emit(OpCodes.Stfld, fb_cell);
            cellSetterIL.Emit(OpCodes.Ret);

            pb_Cell.SetGetMethod(cellGetter);
            pb_Cell.SetSetMethod(cellSetter);
            return pb_Cell;
        }
        private static PropertyBuilder GetPropertyBuilder_Tag(TypeBuilder tb, FieldBuilder fb_tag)
        {
            PropertyBuilder pb_Tag = tb.DefineProperty("Tag", PropertyAttributes.HasDefault, typeof(object), null);

            MethodBuilder tagGetter = tb.DefineMethod(
                "get_Tag",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(object),
                Type.EmptyTypes);

            ILGenerator tagGetterIL = tagGetter.GetILGenerator();
            tagGetterIL.Emit(OpCodes.Ldarg_0);
            tagGetterIL.Emit(OpCodes.Ldfld, fb_tag);
            tagGetterIL.Emit(OpCodes.Ret);

            MethodBuilder tagSetter = tb.DefineMethod(
                "set_Tag",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                null,
                new Type[] { typeof(object) });

            ILGenerator tagSetterIL = tagSetter.GetILGenerator();
            tagSetterIL.Emit(OpCodes.Ldarg_0);
            tagSetterIL.Emit(OpCodes.Ldarg_1);
            tagSetterIL.Emit(OpCodes.Stfld, fb_tag);
            tagSetterIL.Emit(OpCodes.Ret);

            pb_Tag.SetGetMethod(tagGetter);
            pb_Tag.SetSetMethod(tagSetter);
            return pb_Tag;
        }

        private static MethodBuilder GetMethodBuilder_Instantiate(TypeBuilder tb, GenericTypeParameterBuilder K, ConstructorBuilder constructor, FieldBuilder fb_instance, FieldBuilder fb_cell)
        {
            MethodBuilder mb_Instantiate = tb.DefineMethod(
                            "Instantiate",
                            MethodAttributes.Public | MethodAttributes.Static,
                            tb,
                            null);

            ILGenerator mb_InstantiateIL = mb_Instantiate.GetILGenerator();
            Label endOfIfBlock = mb_InstantiateIL.DefineLabel();
            mb_InstantiateIL.Emit(OpCodes.Ldsfld, fb_instance);
            mb_InstantiateIL.Emit(OpCodes.Brtrue_S, endOfIfBlock);
            mb_InstantiateIL.Emit(OpCodes.Newobj, constructor);
            mb_InstantiateIL.Emit(OpCodes.Stsfld, fb_instance);
            mb_InstantiateIL.Emit(OpCodes.Ldsfld, fb_instance);
            mb_InstantiateIL.Emit(OpCodes.Ldc_I4_1);
            mb_InstantiateIL.Emit(OpCodes.Newarr, K);
            mb_InstantiateIL.Emit(OpCodes.Stfld, fb_cell);
            mb_InstantiateIL.MarkLabel(endOfIfBlock);
            mb_InstantiateIL.Emit(OpCodes.Ldsfld, fb_instance);
            mb_InstantiateIL.Emit(OpCodes.Ret);

            return mb_Instantiate;
        }
        private static MethodBuilder GetMethodBuider_Bind(TypeBuilder tb, GenericTypeParameterBuilder K, FieldBuilder fb_cell)
        {
            MethodBuilder mb_GetReferenceToCore = tb.DefineMethod(
                "GetReferenceToCore",
                MethodAttributes.Public | MethodAttributes.Virtual,
                K.MakeByRefType(),
                new Type[] { K });

            ILGenerator mb_GetReferenceToCoreIL = mb_GetReferenceToCore.GetILGenerator();
            mb_GetReferenceToCoreIL.Emit(OpCodes.Ldarg_0);
            mb_GetReferenceToCoreIL.Emit(OpCodes.Ldfld, fb_cell);
            mb_GetReferenceToCoreIL.Emit(OpCodes.Ldc_I4_0);
            mb_GetReferenceToCoreIL.Emit(OpCodes.Ldelema, K);
            mb_GetReferenceToCoreIL.Emit(OpCodes.Ret);
            return mb_GetReferenceToCore;
        }
    }
}
