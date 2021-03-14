(**
# NB00b FSharp Coding literacy part II

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB00b_FSharp_Coding_literacy_Part_II.ipynb)


1. Making sense of sequences
    1. Strings have their own module 
    2. Bio-collections are F# collections
    3. All collections have their modules
    4. Seq module contains the skeleton key functions
    5. Build a map to associate values with a key 
2. Higher-order functions     
    1. Functions can consume other functions
    2. Pipe-forward operator |> 
3. More interesting types 
    1. Tuples are ad hoc data structures 
    2. Record types provide more organization

## Making sense of sequences

Sequences are a key part of biology, and therefore, storing, searching, and analyzing protein and nucleic sequences plays an important role in computational biology. 
In many languages, biological sequences are simply represented as strings. 

Here’s a very simple DNA sequence: 
*)

let mySequence = "atcg" 

(**
In F# there are a lot of functions available by default to manipulate strings all of which can be found in the String module. 

### Strings have their own module 

We learned previously that modules are simply a container to organize your functionality. To make use of that functionality you need to know the name of the respective 
module and be aware that you can access everything with the `.` operator. The name of the module for ‘string’ types is ‘String’ with a capital to distinguish the module 
from its type. *- FYI -* this is true for all core types in F#.
*)

String.length "atcg"

(**
Here, the ` String.Length ` returns the length of the string sequence. If you are using a nice text editor with more sophisticated code completion (intelliSense), you can 
explore the functionality provided within a module by pressing `Strg + Space`. Otherwise, you need to look it up in one of many [documentations.](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-stringmodule.html)

Another interesting feature of strings is that their characters can also be accessed according to their numerical position, starting from 0 up to 1 minus the length of the 
string. You can also slice strings up using a [from .. to] syntax:
*)

"atcg".[2]
"atcg".[1..2]

(**
Technically, strings are specialized collection of characters and we have seen how to represent a nucleotide sequence.

### Bio-collections are F# collections

In contrast to the alphabet of letters we use in natural language, biological nucleotide and amino acid alphabets have a smaller set of valid characters. This allows us to 
represent biological sequence data that encode DNA and proteins more efficiently using BioFSharp.
*)
// Get BioFSharp from nuget 
#r "nuget: BioFSharp, 2.0.0-beta5"
open BioFSharp 

let adenine = Nucleotides.A
let alanine = AminoAcids.Ala

(**
Sequence types or collection types are a series of any items or values rather than only characters. There are different variations of collection types with different properties 
regarding there manipulation and efficiency. To cover all different kinds and their methods to manipulate them would go far beyond the scope of this introduction. So, we will 
focus on the most important types and methods of manipulation. Collections provide a more flexible way to work with groups of items. In the case of our Bio-collections from 
BioFSharp the items of the biological alphabets. Here is an example of a list of nucleotides: 
*)
open BioFSharp.Nucleotides 

let nucs = [ A; T; C; G; ]

(**
Notice here that we have stored the exact same sequence as in our previous string example, but this time in list format. Notice how the items are separated by a semicolon and
enclosed in square brackets instead of quotes. Analogously, we can access each item by its index, however, in case of lists this is not very efficient. 
*)

// Accessing position 3 (remember index start at 0)
nucs.[2]
// Add elongates the list (very efficient operation on lists)
let nucs' = G::A::nucs

(**
If you need a collection type with the opposite properties, meaning fast random access but very inefficient adding or deleting elements an array is what you want to use instead. 
Just add the additional `|` and you have an array.
*)
// The same sequence but as an array
let nucsArr = [| A; T; C; G; |]

(**
### All collections have their modules 

Whenever you need functions to manipulate bio-collections, you might find them in the respective module. You can either use intelliSense to browse the available functions or you 
can have a look at the documentation [here.](https://csbiology.github.io/BioFSharp)

With functions you find in the Bio-collection modules you can easily compute for example the complementary mRNA sequence or translate it into the corresponding peptide sequence.
*)

// Reverse complement of a sequence as a BioArray
BioArray.reverseComplement nucsArr

// Translate to peptide 
BioList.translate 0 [A; U; C; G; C; G]

// Use the Array module to find the index of the first occurring cytosine
Array.findIndex (fun n -> n = C)  [| A; T; C; G; |]

(**
However, it is worth noticing that the bio-collections are normal F# collections. The Bio* modules just enhance the functionalities of the default modules. In the example, we 
apply the `Array.findIndex` functions from the default array module on a BioArray to find the index of the first occurring cytosine. 

A full overview of the default collections and their respective modules can be found [here.](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/fsharp-collection-types)


There is one module that you might see quite often when working with collections in F#. It is more generic compared to the others previously discussed as it is able to manipulate 
all enumerable collections. The `Seq.`  module contains functions that can take list collections as well as array collections as input
 (or others that are [enumerable).]( https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/fsharp-collection-types).
In the following example we apply the same functions on a list and on an array. 

### Seq module contains the skeleton key functions

It is worth mentioning that in the module of F# collections you will find functions to convert from one collection type to another. I believe that you can imagine the reason 
for such an operation. You know by now that depending on the use case one collection might be advantages compared to the other. For example, if you know that you need to access 
your sequence multiple times at different position, you want to convert it from a list into an array.
*)

// Returns the length of the array
Seq.length [| A; T; C; G; |]
// Returns the length of the list  
Seq.length [ A; T; C; G; ]
// The input is a list type the output an array type 
List.toArray [ A; T; C; G; ]

(**
Due to those performance issues, certain functionality is only offered on particular collection types and you need to do the conversion first before you can apply this function.

### Build a map to associate values with a key

We learned that you can access the elements within collections by its numerical position index. To build more complex examples, it might be necessary to have a more sophisticated 
way to associate values with a key. A map is built from a list of key-value pairs using the so-called Map constructor. 

This just means you need to write `Map` in front of that key-value pair list:
*)

let mass = Map ["Hydrogen",1.0079; "Carbon", 12.001]

(**
Extracting a value from a Map work similar to accessing an array using the key instead of the index. 
*)

mass.["Carbon"]

Map.find "Cabon" mass

(**
Analogously to the List and Array module there is a Map module. 
A full overview of all map function can be found [here]( https://fsharp.github.io/fsharp-core-docs/reference/fsharp-collections-fsharpmap-2.html).


## Higher-order functions     

A higher-order function is a function that takes another function as a parameter. This is simple, but leads to one of the most important concepts of functional programming.

### Functions can consume other functions

ipso

### Pipe-forward operator |> 

The Pipe-forward operator lets you pass an intermediate result (value) onto the next function, it’s defined as:

## More interesting types 

We already learned about type annotation that it defines the kind of the value you and the compiler have to deal with. Therefore, you can think of type annotation as a sort of 
data modelling tool that allows you to represent a real-world domain in your code. The better the type definitions reflect the real-world domain, the better they will statically  
encode the rules. This means you will always be warned if you violate the rules you defined. That will help you to avoid mistakes within your code. In practice, if you try to  
sum binding x + y and x is bound to the number 5 (datatype = int) whereas y is bound to the word “PEPTIDE” (datatype = string) the compiler will not allow it, while if y is 
bound to number 7 you will get 12 as a valid result. 

However, using only literal or primitive types may be not enough. You most probably want to do something more interesting that is not based solely on numbers or letters. To do 
so you need to be able to produce your own types.   

The key to understanding the power of types in F# is that most new types are constructed from other types just combing types that already exist. You can think of your own type 
definition as an organization or grouping of other types. To do so, every type definition is similar, even though the specific details may vary. All type definitions start with 
a "type" keyword, followed by an name or identifier for the type, which then is followed by any existing type(s). 

For example, here are some type definitions for a variety of types:
*)

type A = int * int

type B = {AminoAcidName:string; Mass:float}

(**
### Tuples are ad hoc data structures 
Tuples have several advantages over other more complex types. They can be used on the fly because they are always available without being defined, and thus are perfect for small, 
temporary, lightweight structures. Some people think of tuples as small list with a fixed length and different types. If you look at the way to create a list and compare it to a 
tuple, you will see why:
*)
// Creating a list
[ 1.1; 3.5; 2.0 ]

// Creating a tuple by changing the semicolon to a comma and the square brackets to curved brackets
(  1.1, 3.5, 2.0 )

// Creating another tuple
( 115.026943, "asp" )

(**
However, while collections can only contain elements of the same type, with tuples you can combine different types in the same tuple type. Also accessing values from a tuple is quite 
different compared to a collection type. 

*)
// Accessing the value at position 2 of a list
[ 1.1; 3.5; 2.0 ].[1]

// Accessing the value at position 2 of a tuple
let monoMass, threeLetterCode = ( 115.026943, "asp" )
threeLetterCode

(**
You notice that accessing a value from a tuple means create a named binding that has the same structure (a process called deconstruction). After this, the individual values have their 
own names and can be used separately. Therefore, it is easy to define functions that can access individual positions of tuple types. 

Let's define a function that returns the three-letter:
*)
let getThreeLetterCode (monoMass, threeLetterCode) =
    threeLetterCode

getThreeLetterCode ( 115.026943, "asp" )

(**
Tuples are also very useful in the common scenario when you want to return multiple values from a function rather than just one.
*)

let returnTwoValues () =
    ('A',100)

(**

### Record types provide more organization

Tuples are useful in many cases. But they have some disadvantages too. Because all tuple types are pre-defined, you can't distinguish between a string and a float used for an 
amino acid mass with one-letter-code say, vs. a similar tuple used for nucleotide. 

And when tuples have more than a few elements, it is easy to get confused about which element is in which place. In these situations, what you would like to do is label each 
slot in the tuple, which will both document what each element is for and force a distinction between tuples made from the same types. A record type is exactly that: A tuple 
where each element is labeled.
*)
// Define a record type amino acid
type AminoAcidMass = { OneLetterCode: char; Mass: float }
// Define a record type nucleotide
type NucleotideMass = { Symbol: char; Mass: float }

// Bind an amino acid value to a name binding
let asp = { OneLetterCode = 'D'; Mass = 115.026943 }

(**
A record type has the standard preamble: `type` typename = followed by curly braces. Inside the curly braces is a list of label: type pairs, separated by semicolons (notice: We 
will see later that all lists in F# use semicolon separators -- commas are for tuples).

Let's compare the "type syntax" for a record type with a tuple type:
*)

// Definition of a record type 
type AminoAcidMassRecord = { OneLetterCode: char; Mass: float }
// Definition of a tuple type 
type AminoAcidMassTuple = char * float

(**
You see that record types have named fields that make them easily accessible. A Field of a record type can be accessed individually with the dot operator `.Fieldname`
*)

//Create cysteine  
let cys = { OneLetterCode = 'C'; Mass = 103.0091848 }
// Access the one-letter-code of cysteine
cys.OneLetterCode
// Access the monoisotopic mass of cysteine and multiply it times 3
cys.Mass * 3.

