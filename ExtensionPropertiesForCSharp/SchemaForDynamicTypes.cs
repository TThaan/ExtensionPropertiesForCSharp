using System;

namespace ExtensionPropertiesForCSharp
{
    class SchemaForDynamicTypes
    {
        /// <summary>
        /// Classes below this are just to show how the dynamically created types look like.
        /// </summary>
    }

    class BaseOfExtension01 : IGetGenericParameter
    {
        public static Type typeOfGenericParameter;
        public Type TypeOfGenericParameter { get { return typeOfGenericParameter; } }
        //An instance of Singleton01 here would spare me the effort to MakeGeneric on SingletonClass again
        //and could help while searching the Extensions in ExtensionCreator.GetExtensionClass<T>?
    }

    class Extension01<T> : BaseOfExtension01, IExtendable<T>
    {
        public Extension01(T source)
        {
            //if (instance != null)
            //{ throw new ArgumentException("This singleton already exists."); }
            //else
            //{
                instance = this;
                typeOfGenericParameter = typeof(T);
                instance.cell = new T[1];
                instance.cell[0] = source;
            //}
        }

        private static Extension01<T> instance;

        T[] cell;
        object tag;

        public T[] Cell { get { return cell; } set { cell = value; } }
        public object Tag { get { return tag; } set { tag = value; } }

        public ref T GetReferenceToCore(T source)
        {
            return ref instance.cell[0];
        }
    }
}
