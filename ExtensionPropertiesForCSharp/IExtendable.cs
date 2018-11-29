using System;

namespace ExtensionPropertiesForCSharp
{
    public interface IGetGenericParameter
    {
        Type TypeOfGenericParameter { get; }
    }

    public interface IExtendable<T> : IGetGenericParameter
    {      
        T[] Cell { get; set; }
        object Tag { get; set; }
        ref T GetReferenceToCore(T source);
    }
}
