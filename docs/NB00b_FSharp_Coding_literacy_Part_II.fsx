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
Here, the ` String.Length ` returns the length of the string sequence. If you are using a nice text editor with more sophisticated code completion (intelliSence), you can 
explore the functionality provided within a module by pressing `strg - space`. Otherwise, you need to look it up in one of many [documentations.](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-stringmodule.html)

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
BioFSharp the items of the biological alphabets. Here, is an example of a list of nucleotides: 
*)
open BioFSharp.Nucleotides 

let nucs = [ A; T; C; G; ]

(**
Notice here that we have stored the exact same sequence as in our previous string example, but this time in list format. Notice how the items are separated by a semicolon and
enclosed in square brackets instead of quotes. Analogously, we can access each item by its index, however, in case of lists this is not very efficient. 
*)

// Accessing position 3 (remember index start at 0)
nucs.[2]
// Add elongate the list (very efficient operation on lists)
let nucs' = G::A::nucs

(**
If you need a collection type with the opposite properties, meaning fast random access but very inefficient adding or deleting elements an array is what you want to use instead. 
Just add the additional `|` and you have an array.
*)
// The same sequence but as an array
let nucsArr = [| A; T; C; G; |]

(**
### All collections have their modules 

Whenever you need functions to manipulate bio-collections, you might find them in the respective module. You can either use intelliSence to browse the available functions or you 
can have a look at the documentation [here.](https://csbiology.github.io/BioFSharp)

With functions you find in the Bio-collection modules you can easily compute for example the complementary mRNA sequence or translate it into the corresponding peptide sequence.
*)

// Reverse complement of a sequence as a BioArray
BioArray.reverseComplement nucsArr

// Translate to peptide 
BioList.translate 0 [A; U; C; G; C; G]

// Use the Arra module to find the index of the first occurring cytosine
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

It is worth mentioning, that in the module of F# collections you will find functions to convert from one collection type to another. I believe that you can imagen the reason 
for such an operation. You know by now that depending on the use case one collection might be advantages compared to the other. For example, if you know that you need to access 
your sequence multiple times at different position, you want to convert it from a list into an array.
*)

// Returns the length of the array
Seq.length [| A; T; C; G; |]
// Returns the length of the list  
Seq.length [ A; T; C; G; ]
// The input is a lit type the output an array type 
List.toArray [ A; T; C; G; ]

(**
Due to those performance issues, certain functionality is only offered on particular collection types and you need to do the conversion first, before you can apply this function.

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

ipso

### Tuples are ad hoc data structures 

ipso
### Record types provide more organization

ipso
*)