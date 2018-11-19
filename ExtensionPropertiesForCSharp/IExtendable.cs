using System;

namespace ExtensionPropertiesForCSharp
{
    public interface IExtendable<T>
    {
        Type TypeOfGenericParameter { get; }
        ref T GetReferenceToCore(T source);
        T[] Cell { get; set; }
        object Tag { get; set; }
    }
}
