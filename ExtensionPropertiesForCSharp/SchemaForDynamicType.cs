using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionPropertiesForCSharp
{
    /// <summary>
    /// Just to show how the dynamically created types look like.
    /// </summary>


    class GenericParameterofSingleton
    {
        protected static Type typeOfGenericParameter;
    }


    class DynamicSingleton<K> : GenericParameterofSingleton, IExtendable<K>
    {
        DynamicSingleton() { }

        static DynamicSingleton<K> instance;
        K[] cell;
        object tag;

        public K[] Cell { get { return cell; } set { cell = value; } }
        public object Tag { get { return tag; } set { tag = value; } }
        public Type TypeOfGenericParameter { get { return typeOfGenericParameter; } }

        public static DynamicSingleton<K> Instantiate()
        {
            if (instance == null)
            {
                instance = new DynamicSingleton<K>();
                instance.cell = new K[1];
                typeOfGenericParameter = typeof(K);
            }
            return instance;
        }

        public ref K GetReferenceToCore(K source)
        {
            cell[0] = source;
            return ref cell[0];
        }
    }
}
