using System;
using static ExtensionPropertiesForCSharp.DynamicTypeCreator;

namespace ExtensionPropertiesForCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            ref ExampleStruct sXT = ref ExtensionOf(new ExampleStruct() { z = 14 }, "sXT");

            ExampleClass eClass = new ExampleClass() { x = 10, y = 77 };
            ref ExampleClass cXT = ref ExtensionOf(eClass, "cXT");

            ref int iXT = ref ExtensionOf(99);

            sXT.Tag("I´m a Tag.", "sXT");
            Console.WriteLine(sXT.Tag(id: "sXT"));

            sXT.Tag("I´m a new Tag.");
            Console.WriteLine(sXT.Tag());

            //      Finally, not to forget, if you want your core to stay in sync with your reference 
            //      you need to pass it as ref parameter. This way you can work with value types too.

            DoubleThis(ref sXT);
            Console.WriteLine(sXT.z);

            Console.ReadLine();
        }

        static void DoubleThis(ref ExampleStruct iXT)
        {
            iXT.z *= 2;
        }

    }

    class ExampleClass
    { public int x, y; }
    struct ExampleStruct
    { public int z; }
}
