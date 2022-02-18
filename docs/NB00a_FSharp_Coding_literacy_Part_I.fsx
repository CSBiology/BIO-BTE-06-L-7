(**
# NB00a FSharp Coding literacy part I

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB00a_FSharp_Coding_literacy_Part_I.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB00a/NB00a_FSharp_Coding_literacy_Part_I.ipynb)

1. Getting things done and understand values
    1. Numeric types
    2. String or character types
    3. F# handles your types.
2. Functions and operators: Do some operations
    1. Operations and types
    2. Functions
3. Namespace and modules
4. Control flow expressions


## Getting things done 

The purpose of this first part is to present the use of F# (pronounced “F Sharp”) programming language to facilitate and automate a wide variety of data manipulation
tasks typically faced in life science research. Here, we want to support your coding literacy by a practical programming approach without the attempt to detail every 
possible variation of the mechanisms. This offers a rapid introduction to F# programming that should allow you to understand code and do some manipulation to achieve 
your desired outcome.

There are many kinds of programming languages, with different purposes, styles, intended uses, etc. However, F# is particularly nice as it is easy to learn in the beginning 
but also extremely powerful for professional use cases. Simply put, F# is a beautiful language. It is effective for everything from teaching new programmers to advanced
computer science study, from simple scripts to sophisticated advanced applications. Therefore, it is extremely well suited for bioinformatics programming and 
application in live science.

### Numeric types

Definitely, F# can be a good friend in your daily work even when you are doing simple calculations, e.g. in the lab. Let's jump right in and some stuff done:
*)
// Define numerical values
let co2Oxygens = 2
let co2Carbon  = 1
let oxygenMass = 15.9994
let carbonMass = 12.0107
let avogadro = 6.023e23
(**
First things first: Any text on a line that follows `//` is handled as a comment and is ignored by the F# compiler. Comments help to write organized code with documentation 
and are therefore not executed as part of your program or script. Additionally, there is also the possibility to write comments that span multiple lines by putting text between 
parenthesis and asterisk `(* … *)`.

The example shows the two most common types of numbers used in F#:

* integer (`int`): co2Oxygens = 1
* floating point numbers (`float`): let oxygenMass = 15.9994

One of the most important action in programming is naming things, which is called binding. Binding is the naming process that associates an identifier (name) to a value or function. 
In F#, the `let` keyword is used to do such a binding and bind a name to a value in our example. We can use `let` binding at various level. It might be worth noting here that this 
process is often referred as declaring variables, but using the term “binding” is much nicer.
Bindings are case sensitive names and can contain letters, numbers as well as the underscore character. They cannot start with a number however and they cannot contain spaces. 

This makes the following three different bindings: 
*)
let myvariable = 1
let MyVariable = 3.172
let MYVARIABLE = "Undefined"

(** 
*But what is a value?*

In general, computer programs manipulate data. An individual item of data is called a value. F# is a statically typed language and every value has a type that identifies the kind of value it is. 
For example, the type of `co2Oxygens` is `int`. Each value can be seen an instance of a particular type, later we will complete this point of view by recognizing that each object is an 
instance of a particular class (type).

### String or character types

Similar to declaring numbers in F#, we also can bind text (aka. `string`) or characters to names just by enclosing the declared value in double or single quotes.
*)
// Define string or character values
let buffer = "Tris"
let hydrogenMass = "1.00794"
let nucleotide = 'A'
(**
Here, single quotes are used for single character (`char`), while double quotes indicate a string. Importantly, if you would like to use quotes (or other special characters) in a string, you must 
quote the quotes with a backslash `\`. 

One more way to declare strings in F# that allows you to even include line breaks in the strings. It's useful, for example, for declaring paragraphs of text as strings. To use it, you have to enclose
the string itself in triple quotes.
*)
let poem = """
    Nature's first green is gold,
    Her hardest hue to hold.
    Her early leaf's a flower;
    But only so an hour.
"""
(**
There are many different specialized primitive types available in F#, [see here](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/literals) for the full overview. However, covering 
Boolean (`bool`) values in the following, we now covered the most common primitive types.
*)

let yes = true
let no  = false

(**
Very often, in programming, you will need a data type that can only have one of two values, like: *Yes/No, On/Off or TRUE/FALSE*. A boolean type is declared with the bool keyword and can only take the values 
`true` or `false`.

### F# handles your types.
The F# compiler needs to always know the type of your value. Luckily, this does not mean that you need to specify the type all the time explicitly by yourself. In the process of type inference, the 
F# compiler infers the types of values, variables, parameters and return values.
The compiler infers the type based on the context. If the type is not otherwise specified, it is inferred to be generic. If the code uses a value inconsistently, in such a way that there is no single 
inferred type that satisfies all the uses of a value, the compiler reports an error. 

We have seen this happen through all the examples so far, but how does it work and what can you do if it goes wrong?
In that case, you can apply explicit type annotations using `: type name`, as shown in the following examples: 
*)
// Explicitly define the type of on as bool. Brackets () are not necessary
let (on : bool) = true

(**

## Functions and operators: Do some operations

In the beginning, I introduced F# as your friendly pal. Let’s dive into a quick example, that actually does some operations.
*)

let co2Oxygens' = 2
let co2Carbon'  = 1
let oxygenMass' = 15.9994
let carbonMass' = 12.0107

let co2Mass = float co2Oxygens' * oxygenMass' + float co2Carbon' * carbonMass'

printfn "Molecular weight of CO2 = %f" co2Mass


(**
Here we use the bindings we did before to calculate the mass of a CO2 molecule and bind the result to the name co2Mass. In the next line `printfn` is used to output the result to the F# console which is the 
default stream for any output from the code. 
If you run this code into your interactive notebook and saw the output, you just ran your first, small F# program!

### Operations and types

The F# type system makes sure that you can not mix types during calculation. While it is possible to concatenate two string using the `+` operator. However, using two different types will cause the compiler 
to point out the error.  

It is worth to notice, that because `int` and `float` are different types even though they are both numbers. Therefore, to calculate the example above it is necessary to explicitly cast between `int` and `float` for 
operations such as multiplication.

### Functions 
   

At this point we have our first functionality, meaning a small F# program that can perform a calculation. In most cases you want to organize the functionalities your code provides into nice building blocks that 
can be reused and applied multiple times. You can think of functions as exactly those kinds of self-contained building blocks of code that accomplish a specific task. Functions usually consume input data called 
parameter, process it in their function body, and return a result. Once you have a function defined, it can be used over and over and over again. 
The best way to understand an F# function is to see it in action. Let’s dive into it:
*)

// Example of indented code
let explainIndentedCode () =
    printfn "This indented line is part of the function"
    printfn "So is this one"
printfn "This unindented line is not a part of the function"

(**
This code exemplifies two important point. First, indentation is syntactically meaningful in F# and not just for comfortable reading. It is used to show which particular block of code belongs together. Therefore, 
all code inside a function must be indented to define it as belonging to the function.

Secondly, functions can be called from the inside of other functions. Here, we use the `printfn` function you already know inside the outer function.  The function `printfn` is a build in function and is already 
defined in the core of F#. Thus, we do not need to define it ourselves and can call it as we desire.

With this knowledge we can do a more meaningful example:
*)

//A simple function to calculate molar volumes
let calculateGasDensity molarMass pressure temperatureK =
    let gasConstant = 0.082057
    let density : float = (molarMass * pressure) / (gasConstant * temperatureK)
    density

let co2MolarVolume = calculateGasDensity co2Mass 4. 546.
printfn "The density of carbon dioxide at 546 K and 4.00 atmospheres pressure is %f g/L" co2MolarVolume

(**
As the biochemistry textbooks tell us, the density of 1 mole of CO2 at 546 Kelvin and 4.00 atmospheres pressure is indeed 3.93 g/L. We make use of the `let` keyword and define or declare our function, followed by the function call where the result is 
bound to the name `co2MolarVolume`.

*Side note:* Signature of a function

A function signature shows the function prototype. It tells you name, input type(s) and output type of a function. You will later learn that the type defines the kind of the value for functions to act upon (input) 
and return (output). Just by examining a function’s signature, you can often get some idea of what it does.

*Side note:* Lambda expressions

In F# it is possible to use function without giving them a name and use the keyword `fun` instead and the `=` becomes `->`. This is called anonymous function or referring to lambda calculus lambda expression. 
This kind of functions are often used for convenience. To write 'add1' as lambda expression `fun x -> x + 1`

## Namespace and modules

Sometimes it can be necessary to go a little bit further in terms of code organization. For example, to ship code in a library to other users. Namespaces und Modules are top-level and low-level constructs to 
organize code. You can think of namespaces and modules as containers and sub containers, respectively, in which you can put function and type definitions. The hierarchy is defined that you can have multiple 
modules in one namespace, also nested modules in a module, but no namespace in another namespace. 

The most important bit is, that you can access namespaces and modules with the `.` operator. This is also how you find functionality in libraries which you can load from other sources.  This is required because 
it means you do not need to write all code and functionality by yourself.
In the following you can see how you can profit from the amazing F# community efforts to provide you with various libraries. It seems obvious that for our example we load BioFSharp.
*)

#r "nuget: BioFSharp, 2.0.0-beta5"

// Access adenine without 'open'
BioFSharp.Nucleotides.A

// Access adenine with 'open'
open BioFSharp
Nucleotides.A

(**
The first line automatically pulls the necessary files from the internet and allows you to use the library in your scripting environment. Due to the `open` statement you can shorten the path to access the modules 
and functions in the library. This just saves you the effort to always type `BioFSharp.` in front of everything… 

## Control flow expressions

Everything you have seen so far has consisted of sequential execution, in which expressions are always evaluated one after the next, in exactly the order specified. But the world is often more complicated than that. 
Frequently, a program needs to skip over or choose between alternate sets of paths to execute.
That is where control structures come in. A control structure directs the order of evaluation of expressions in a program (referred to as the program’s control flow).
*)

//Function for calculating buffer recipes that uses if-conditionals 
let bufferRecipe buffer molarity =
    let grams = 
        if buffer = "Tris" then
            121.14
        elif buffer = "MES" then
            217.22 
        elif buffer = "HEPES" then
            238.30 
        else
            failwith "Huh???"
    let gramsPerLiter = grams * molarity 
    gramsPerLiter

(**
The function `bufferRecipe` takes two arguments that specify the name of the `buffer` and `molarity` of the stock solution you want to make. Based upon the buffer name that you provide; it then binds the appropriate value 
to the name grams and returns the required weight of buffer (in grams) to make up a 1 L solution.

As with other programming languages, the equality operator in F# is just one of a bunch of comparison you can do:

* `=`  equals 
* `<>` not equals
* `>` greater than 
* `<` less than 
* `>=` greater than OR equals 
* `<=` less than OR equals

However, there is a more powerful control flow construct called Matching expression. You can write the `bufferRecipe` example from above as follows:
*)

//Function for calculating buffer recipes that uses match-conditions 
let bufferRecipe' buffer molarity =
    let grams =
        match buffer with
        | "Tris"  -> 121.14
        | "MES"   -> 217.22 
        | "HEPES" -> 238.30 
        | _       -> failwith "Huh???"
    grams * molarity 
    

(**
Each `|` defines a condition, the `->` means "if the condition is true, follow this path...". The `_` is the default pattern, meaning that it matches anything, sort of like a wildcard. Occasionally, it's not enough to 
match an input against a particular value; we can add filters, or guards, to patterns using the `when` keyword. We can rewrite our `bufferRecipe` function once again. 
*)

//Function for calculating buffer recipes that uses match-conditions with guards
let bufferRecipe'' buffer molarity =
    let grams =
        match buffer with
        | x when x = "Tris"  -> 121.14
        | x when x = "MES"   -> 217.22 
        | x when x = "HEPES" -> 238.30 
        | _       -> failwith "Huh???"
    grams * molarity 
    
(**
Maybe we should notice that in this function we use the function `failwith` to throw an error, if we do not know the buffer. This is not particular nice, however necessary in our example. 
*)