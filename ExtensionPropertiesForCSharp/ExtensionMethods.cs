

using System;

namespace ExtensionPropertiesForCSharp
{
    static class Extension_Methods
    {
        public static object Tag<T>(this T core, object value = default, string id = default)
        {
            dynamic extendedObject = DynamicTypeCreator.GetExtendedObject(core, id);
            if (value != default) { extendedObject.Tag = value; }
            return extendedObject.Tag;
        }
    }
}
