using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ExtensionPropertiesForCSharp
{
    partial class ExtensionCreator
    {
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
            cellGetterIL.Emit(OpCodes.Ldsfld, fb_typeOfGenericParameter);
            cellGetterIL.Emit(OpCodes.Ret);

            pb_TypeOfGenericParameter.SetGetMethod(typeOfGenericParameterGetter);

            return tb.CreateType();
        }

        private static Type CreateExtensionClass<T>(string id, Type extBaseClass)
        {
            TypeBuilder tb = mb.DefineType(
                id,
                TypeAttributes.Public,  // TypeAttributes.Class | 
                extBaseClass);
            tb.AddInterfaceImplementation(typeof(IExtendable<>));
            GenericTypeParameterBuilder K = tb.DefineGenericParameters(new[] { "K" })[0];
            FieldBuilder fb_instance = tb.DefineField("instance", tb, FieldAttributes.Private | FieldAttributes.Static);
            FieldBuilder fb_tag = tb.DefineField("tag", typeof(object), FieldAttributes.Private);
            FieldBuilder fb_cell = tb.DefineField("cell", K.MakeArrayType(), FieldAttributes.Private);
            ConstructorBuilder ctor = GetConstructorBuilder(tb, extBaseClass, K, fb_instance, fb_cell);
            PropertyBuilder pb_Cell = GetPropertyBuilder_Cell(tb, K, fb_cell);
            PropertyBuilder pb_Tag = GetPropertyBuilder_Tag(tb, fb_tag);
            MethodBuilder mb_GetReferenceToCore = GetMethodBuider_GetReferenceToCore(tb, K, fb_cell);

            return tb.CreateType();
        }
        private static ConstructorBuilder GetConstructorBuilder(TypeBuilder tb, Type extBaseClass, GenericTypeParameterBuilder K, FieldBuilder fb_instance, FieldBuilder fb_cell)
        {
            FieldInfo fi_typeOfGenericParameter = extBaseClass.GetField("typeOfGenericParameter");

            ConstructorBuilder ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { K });  //check!
            ILGenerator ctor0IL = ctor.GetILGenerator();
            ctor0IL.Emit(OpCodes.Ldarg_0);
            ctor0IL.Emit(OpCodes.Call, extBaseClass.GetConstructor(Type.EmptyTypes));  //empty?!
            ctor0IL.Emit(OpCodes.Ldarg_0);
            ctor0IL.Emit(OpCodes.Stsfld, fb_instance);
            ctor0IL.Emit(OpCodes.Ldtoken, K);
            ctor0IL.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
            ctor0IL.Emit(OpCodes.Stsfld, extBaseClass.GetField("typeOfGenericParameter"));    //value setter?
            ctor0IL.Emit(OpCodes.Ldsfld, fb_instance);
            ctor0IL.Emit(OpCodes.Ldc_I4_1);
            ctor0IL.Emit(OpCodes.Newarr, K);
            ctor0IL.Emit(OpCodes.Stfld, fb_cell);
            ctor0IL.Emit(OpCodes.Ldsfld, fb_instance);
            ctor0IL.Emit(OpCodes.Ldfld, fb_cell);
            ctor0IL.Emit(OpCodes.Ldc_I4_0);
            ctor0IL.Emit(OpCodes.Ldarg_1);
            ctor0IL.Emit(OpCodes.Stelem, K);
            ctor0IL.Emit(OpCodes.Ret);

            return ctor;
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
