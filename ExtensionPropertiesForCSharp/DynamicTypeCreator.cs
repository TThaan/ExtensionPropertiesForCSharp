using System;
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

        static DynamicTypeCreator()
        {
            AppDomain ad = AppDomain.CurrentDomain;
            assemblyName = new AssemblyName("ExtendIT");
            ab = ad.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            mb = ab.DefineDynamicModule(assemblyName.Name + ".dll", true);
        }

        public static ref T ExtensionOf<T>(T core, string id = default)
        {
            string typeName = GetTypeNameAndNumber(id, typeof(T));
            Type singletonBaseClass = CreateBaseClass(typeName);
            #region into baseCtor
            FieldInfo fi = singletonBaseClass.GetField("typeOfGenericParameter");
            fi.SetValue(singletonBaseClass, typeof(T));
            Type typeOfGenParam = (Type)fi.GetValue(singletonBaseClass); //test
            #endregion

            Type singletonClass_NonGeneric = CreateSingletonClass<T>(typeName, singletonBaseClass);
            Type singletonClass = singletonClass_NonGeneric.MakeGenericType(new[] { typeof(T) });
            object singletonInstance = GetSingletonInstance<T>(singletonClass);

            IExtendable<T> iext = (IExtendable<T>)singletonInstance;
            #region test
            var v1 = iext.Cell[0];
            iext.Tag = "Hallo";
            var v2 = iext.Tag;            
            iext.Cell[0] = core;
            var v3 = iext.TypeOfGenericParameter;
            #endregion

            return ref iext.GetReferenceToCore(core);
        }

        private static string GetTypeNameAndNumber(string id, Type genericType)
        {
            if (id == default) { id = $"{genericType.Name}Extended_{counter}"; }
            else { id = $"{id}_{genericType.Name}Extended"; }
            counter++;
            return id;
        }
        private static Type CreateBaseClass(string typeName)
        {
            TypeBuilder tb = mb.DefineType(
                "BaseOf_" + typeName,
                TypeAttributes.Class | TypeAttributes.NotPublic);                                          //ta attributes

            tb.AddInterfaceImplementation(typeof(IGetGenericParameter));

            FieldBuilder fb_typeOfGenericParameter = tb.DefineField(
                "typeOfGenericParameter",
                typeof(Type),
                FieldAttributes.Public | FieldAttributes.Static);                                         //ta attributes

            ConstructorBuilder ctorDef_base = tb.DefineDefaultConstructor(MethodAttributes.Public);

            PropertyBuilder pb_TypeOfGenericParameter = tb.DefineProperty(
                "TypeOfGenericParameter",
                PropertyAttributes.HasDefault,
                typeof(Type),
                null);

            MethodBuilder typeOfGenericParameterGetter = tb.DefineMethod(
                "get_TypeOfGenericParameter",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(Type),
                Type.EmptyTypes);

            ILGenerator cellGetterIL = typeOfGenericParameterGetter.GetILGenerator();
            //cellGetterIL.Emit(OpCodes.Ldarg_0);                                       //no 'this' if calling static member
            cellGetterIL.Emit(OpCodes.Ldsfld, fb_typeOfGenericParameter);
            cellGetterIL.Emit(OpCodes.Ret);

            pb_TypeOfGenericParameter.SetGetMethod(typeOfGenericParameterGetter);

            return tb.CreateType();
        }
        private static Type CreateSingletonClass<T>(string id, Type singletonBase)
        {
            TypeBuilder tb = mb.DefineType(
                id,
                TypeAttributes.Public,  // TypeAttributes.Class | 
                singletonBase);
            tb.AddInterfaceImplementation(typeof(IExtendable<>));
            GenericTypeParameterBuilder K = tb.DefineGenericParameters(new[] { "K" })[0];

            ConstructorBuilder ctorDef = tb.DefineDefaultConstructor(MethodAttributes.Public);              //no default!?

            FieldBuilder fb_instance = tb.DefineField("instance", tb, FieldAttributes.Private | FieldAttributes.Static);
            FieldBuilder fb_tag = tb.DefineField("tag", typeof(object), FieldAttributes.Private);
            FieldBuilder fb_cell = tb.DefineField("cell", K.MakeArrayType(), FieldAttributes.Private);
            
            PropertyBuilder pb_Cell = GetPropertyBuilder_Cell(tb, K, fb_cell);
            PropertyBuilder pb_Tag = GetPropertyBuilder_Tag(tb, fb_tag);

            MethodBuilder mb_Instantiate = GetMethodBuilder_Instantiate(tb, K, ctorDef, fb_instance, fb_cell);
            MethodBuilder mb_Bind = GetMethodBuider_GetReferenceToCore(tb, K, fb_cell);

            return tb.CreateType();
        }

        public static object GetSingletonClass<T>(T core, string id = default)
        {
            if (id == default)
            {
                string s = typeof(T).Name.ToString();
                var allTypes = AssemblyBuilder.GetTypes();
                var types = AssemblyBuilder.GetTypes().Where(x => x.Name.Contains($"{typeof(T).Name.ToString()}Extended") && !x.Name.Contains($"BaseOf"));

                foreach (Type type in types)
                {
                    dynamic instance = GetSingletonInstance<T>(type);
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
                dynamic instance = GetSingletonInstance<T>(type);
                return instance;
            }
            throw new ArgumentException("You were not supposed to reach this line of code!");
        }
        private static object GetSingletonInstance<T>(Type singletonClass)     //object vs typebuilder
        {
            MethodInfo instantiate = singletonClass.GetMethod("GetInstance");
            return instantiate.Invoke(singletonClass, null);
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
            MethodBuilder mb_GetInstance = tb.DefineMethod(
                            "GetInstance",
                            MethodAttributes.Public | MethodAttributes.Static,
                            tb,
                            null);

            ILGenerator mb_GetInstanceIL = mb_GetInstance.GetILGenerator();
            Label endOfIfBlock = mb_GetInstanceIL.DefineLabel();
            mb_GetInstanceIL.Emit(OpCodes.Ldsfld, fb_instance);
            mb_GetInstanceIL.Emit(OpCodes.Brtrue_S, endOfIfBlock);
            mb_GetInstanceIL.Emit(OpCodes.Newobj, constructor);
            mb_GetInstanceIL.Emit(OpCodes.Stsfld, fb_instance);
            mb_GetInstanceIL.Emit(OpCodes.Ldsfld, fb_instance);
            mb_GetInstanceIL.Emit(OpCodes.Ldc_I4_1);
            mb_GetInstanceIL.Emit(OpCodes.Newarr, K);
            mb_GetInstanceIL.Emit(OpCodes.Stfld, fb_cell);
            mb_GetInstanceIL.MarkLabel(endOfIfBlock);
            mb_GetInstanceIL.Emit(OpCodes.Ldsfld, fb_instance);
            mb_GetInstanceIL.Emit(OpCodes.Ret);

            return mb_GetInstance;
        }
        private static MethodBuilder GetMethodBuider_GetReferenceToCore(TypeBuilder tb, GenericTypeParameterBuilder K, FieldBuilder fb_cell)
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
