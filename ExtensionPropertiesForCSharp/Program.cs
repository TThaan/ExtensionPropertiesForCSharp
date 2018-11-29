using System;
using static ExtensionPropertiesForCSharp.ExtensionCreator;

namespace ExtensionPropertiesForCSharp
{
    class ExampleClass { public int x, y; }
    struct ExampleStruct { public int z; }


    class Program
    {
        static void Main(string[] args)
        {
            //extending the type ExampleStruct and giving it a name (id):

            ref ExampleStruct sXT = ref ExtendIT(new ExampleStruct() { z = 14 }, "sXT");



            //extending the type ExampleClass (after defining it) and giving it a name (id):

            ExampleClass eClass = new ExampleClass() { x = 10, y = 77 };
            ref ExampleClass cXT = ref ExtendIT(eClass, "cXT");



            //extending the type int32 with implicit conversion and no name:

            ref int iXT = ref ExtendIT(99);


            
            //setting/getting Tag of the extended ExampleStruct with the help of a name:

            sXT.Tag("I´m a Tag.", "sXT");
            Console.WriteLine(sXT.Tag(id: "sXT"));



            //setting/getting Tag of the extended ExampleStruct without help:

            sXT.Tag("I´m a new Tag.");
            Console.WriteLine(sXT.Tag());



            //Working with the extended type as ref to keep the reference to the extension class:

            DoubleThis(ref sXT);
            Console.WriteLine(sXT.z);
            Console.ReadLine();
        }

        static void DoubleThis(ref ExampleStruct iXT)
        {
            iXT.z *= 2;
        }
    }
}
