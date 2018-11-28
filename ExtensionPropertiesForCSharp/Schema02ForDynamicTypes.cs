using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionPropertiesForCSharp
{
    class Schema02ForDynamicTypes
    {
        /// <summary>
        /// Classes below this are just to show how the dynamically created types look like.
        /// </summary>
    }

    class BaseOfSingleton01 : IGetGenericParameter
    {
        public static Type typeOfGenericParameter;
        public Type TypeOfGenericParameter { get { return typeOfGenericParameter; } }
    }


    class Singleton01<T> : BaseOfSingleton01    //, IExtendable<T>
    {
        private Singleton01()
        { }

        static Singleton01<T> instance;
        T[] cell;
        object tag;

        public static Singleton01<T> Instance { get { return instance; } }
        public T[] Cell { get { return cell; } }
        public object Tag { get { return tag; } set { tag = value; } }

        public static ref T Extend(T source)
        {
            if (instance == null)
            {
                instance = new Singleton01<T>();
                typeOfGenericParameter = typeof(T);
                instance.cell = new T[1];
                instance.cell[0] = source;
            }
            return ref instance.cell[0];
        }
    }
}
