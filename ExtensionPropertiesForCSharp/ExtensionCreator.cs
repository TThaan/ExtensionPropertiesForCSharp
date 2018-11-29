using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ExtensionPropertiesForCSharp
{
    partial class ExtensionCreator
    {
        static int counter = 0;
        readonly static ModuleBuilder mb;
        readonly static AssemblyName assemblyName;
        readonly static AssemblyBuilder ab;

        static ExtensionCreator()
        {
            AppDomain ad = AppDomain.CurrentDomain;
            assemblyName = new AssemblyName("ExtendIT");
            ab = ad.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            mb = ab.DefineDynamicModule(assemblyName.Name + ".dll", true);
        }

        public static ref T ExtendIT<T>(T core, string id = default)
        {
            string typeName = CreateTypeNameAndCount(id, typeof(T));

            Type extBaseClass = CreateBaseClass(typeName);

            Type extClass_NonGeneric = CreateExtensionClass<T>(typeName, extBaseClass);
            Type extClass = extClass_NonGeneric.MakeGenericType(new[] { typeof(T) });
            object extInstance = Activator.CreateInstance(extClass, core);
            IExtendable <T> iext = (IExtendable<T>)extInstance;
            return ref iext.GetReferenceToCore(core);
        }

        private static string CreateTypeNameAndCount(string id, Type genericType)
        {
            if (id == default) { id = $"{genericType.Name}_Extended{counter}"; }
            else { id = $"{id}_{genericType.Name}_Extended"; }
            counter++;
            return id;
        }
        public static object GetExtensionClass<T>(T core, string id = default)
        {
            if (id == default)
            {
                string s = typeof(T).Name.ToString();
                //var allTypes = AssemblyBuilder.GetTypes();    //2nd module for bases?
                var types = ab.GetTypes().Where(x => x.Name.Contains($"{s}_Extended") && !x.Name.Contains($"BaseOf"));

                foreach (Type type in types)
                {
                    dynamic extInstance = GetExtensionInstance<T>(type);
                    if (extInstance != null && extInstance.Cell[0].Equals(core))
                    {
                        return extInstance;
                    }
                }
                throw new ArgumentException("No ExtendedObject with a matching core found.");
            }
            else
            {
                id = $"{id}_{typeof(T).Name}_Extended";
                Type type = ab.GetType(id);
                if (type == null)
                { throw new ArgumentException("No object with this name found in ExtendIT Assembly."); }
                dynamic instance = GetExtensionInstance<T>(type);
                return instance;
            }
            throw new ArgumentException("You were not supposed to reach this line of code!");
        }
        private static object GetExtensionInstance<T>(Type extClass_NonGeneric)
        {
            Type extClass = extClass_NonGeneric.MakeGenericType(typeof(T));
            FieldInfo instance = extClass.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            return instance.GetValue(extClass);
        }

    }
}
