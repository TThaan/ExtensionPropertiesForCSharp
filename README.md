# ExtensionPropertiesForCSharp

    This program shall serve as a sketch of how to build Extension Properties in C#.
    Since extension methods are no extension properties and the first suggestions I found 
    in the net were neither, I thought finding an own solution might be a good way to be-
    come more intimate with C#. So I did and here is my one approaches. 
    Its not meant as a ready-to-use-tool but an early draft of an idea hoping for some 
     attention and (dis)approval. It's not thread safe nor did I make large-scaled tests. 
    But it works with reference and value types and doesnt foil the rules of OOP with a 
    public static collection containing all extended objects and accessories.
    
    So, anyone feel welcome to comment on any aspect you like, may it be functionality 
    issues, bad designing, using wrong terminology or just that this whole approach makes 
    no sense at all. Please remember it's an early version.
    
    The gist of this program is 
    (1) taking any object you want, eg an integer, (I call it 'core' in here.)
    (2) wrapping it in another one, (created dynamically as some kind of a singleton)
    (3) defining an object (of integer type as well of course) as a reference to this core
    (4) and adding an extension method that can access the wrappers' "extension properties".
    
    The biggest problem in this approach was to to make sure the extension method will 
    access the properties of the correct wrapper. 
    Even though you perpertually act upon the core with the reference it bypasses all of 
    the wrappers' members other than the core field in it. Sadly you can't take a property
    as a ref target and I didn't come up with a way to utilize the connection between 
    the reference  and its' referent/datum(don´t knnow the right term?) to build a path 
    to the right properties that I want to use as extensions or tags if you want so.
    
    So two possibilities I came up with are the following.
    While defining the ref object and calling its extensions there are two options:
            
    (I) Using an additional string parameter serving as a name/id for the wrapper 
        thus getting it out of the ExtendIT Assembly by Name. Since I didn't see a way
        to get an instantiated object by name I dynamically create singletons as wrappers
        each with its' own type name. In this case I propose to name the wrapper by a cer-
        tain convention, eg appending 'XT' to the reference name.
        To distinguish between objects that are references to a core and those that don't 
        it would help to name the first ones by a rule too, eg appending an X as well, just for now.

                  ref ExampleStruct sXT = ref ExtensionOf(new ExampleStruct() { z = 14 }, "sXT");

        or:

                  ExampleClass eClass = new ExampleClass() { x = 10, y = 77 };
                  ref ExampleClass cXT = ref ExtensionOf(eClass, "cXT");

        Sounds not too elegant but should be fast and practicable.

    (II)Using no id.

                  ref int iXT = ref ExtensionOf(99);

        Obviously more practicable, but forcing the program to search through 
        the ExtendIT assembly comparing the reference with each core until an 
        equal pair will be found. 
        Self-evidently the more objects you extend the slower you go.

        Now you can 'extend the reference' or just work with it as usual.
        With id I still need to use a named parameter while getting the tag,
        maybe I'll change this:

                  sXT.Tag("I´m a Tag.", "sXT");
                  Console.WriteLine(sXT.Tag(id: "sXT"));  // output: I´m a Tag.

        Or without id:

                  sXT.Tag("I´m a new Tag.");
                  Console.WriteLine(sXT.Tag());           // output: I´m a new Tag.

        Finally, not to forget, if you want your core to stay in sync with your reference 
        you need to pass it as ref parameter. This way you can work with value types too.

                  DoubleThis(ref sXT);
                  Console.WriteLine(sXT.z);               // output: 28

