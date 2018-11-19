using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionPropertiesForCSharp
{
    class SchemaForDynamicType<K> : IExtendable<K>
    {        
        /// <summary>
        /// Just to show how the dynamically created type looks like.
        /// </summary>

        SchemaForDynamicType() { }

        static Type typeOfGenericParameter;
        static SchemaForDynamicType<K> instance;
        K[] cell;
        object tag;

        public K[] Cell { get { return cell; } set { cell = value; } }
        public object Tag { get { return tag; } set { tag = value; } }
        public Type TypeOfGenericParameter { get { return typeOfGenericParameter; } }

        public static SchemaForDynamicType<K> Instantiate()
        {
            if (instance == null)
            {
                instance = new SchemaForDynamicType<K>();
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
