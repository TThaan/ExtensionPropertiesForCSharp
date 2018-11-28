using System;

namespace ExtensionPropertiesForCSharp
{
    public interface IGetGenericParameter
    {
        Type TypeOfGenericParameter { get; }
    }

    public interface IExtendable<T> : IGetGenericParameter
    {
        //Type TypeOfGenericParameter { get; }
        ref T GetReferenceToCore(T source);
        T[] Cell { get; set; }
        object Tag { get; set; }
    }
}
