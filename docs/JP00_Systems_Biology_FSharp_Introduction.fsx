(**
# Systems Biology

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP00_Systems_Biology_FSharp_Introduction.ipynb)


This notebook introduces the field of Systems Biology and explains why programming is a necessary skill to it. You will get a short introduction to the programming language F# and some links to resource for further studies.

1. [Systems Biology: A brief introduction](#Systems-Biology-A-brief-introduction)
2. [Starting with FSharp](#Starting-with-FSharp)<br>
    1. [Functions](#Functions)<br>
    2. [Binding function values and simple values](#Binding-function-values-and-simple-values)<br>
        1. [Side note: Lambda expressions](#Side-note:-Lambda-expressions)<br>
    3. [Simple values](#Simple-values)
    4. [Lists & Arrays](#Lists-&-Arrays)
    5. [Higher-order functions](#Higher-order-functions)
        1. [Side Note: Pipe-forward operator |>](#Side-Note:-Pipe-forward-operator-|>)
    6. [Control flow expressions](#Control-flow-expressions)
        1. [if-then-else](#if-then-else)
        2. [Pattern Matching](#Pattern-Matching)
    7. [Complex Data Types](#Complex-Data-Types)
        1. [Tuples](#Tuples)
        2. [Record Types](#Record-Types)
    8. [Code organization](#Code-organization)
        1. [Namespaces and modules](#Namespaces-and-modules)
        2. [Comments](#Comments)
3. [References](#References)

## Systems Biology: A brief introduction

<div class="columns">
<div class="column is-8">
The term “systems theory” was introduced by the biologist L. von Bertalanffy. He defined a system as a set of related components that work together in a particular environment to perform whatever functions are required to achieve the system's objective<sup><a href="#1" id="ref1">1</a></sup>. The hierarchical organization orchestrating the interaction of thousands of molecules with individual properties allows complex biological functions. Biological processes like cell division, biomass production, or a systemic response to perturbations are molecular physiological functions which result from a complex dynamic interplay between genes, proteins and metabolites (<a href="#figure1">Figure 1</a>). To gain a holistic understanding of a biological system, all parts of the system need to be studied simultaneously by quantitative measures<sup><a id="ref2" href="#2">2</a></sup>. The focus on a system-wide perspective lies on the quantitative understanding of the organizational structure, functional state, robustness and dynamics of a biological system and led to the coining of the term “Systems Biology”<sup><a href="#3">3</a></sup>.

The current challenges of Systems Biology approaches are mainly along four lines<sup><a href="#2">2</a>,<a href="#4">4</a></sup>: 

 - (**i**) - system-wide quantification of transcriptome, proteome (including protein modifications) and metabolome
 
 - (**ii**) - identification of physical interactions between these components
 
 - (**iii**) - inference of structure, type and quantity of found interactions
 
 - (**iv**) - analysis and integration of the resulting large amounts of heterogeneous data. It becomes obvious that an interdisciplinary effort is needed to resolve these challenges in Systems Biology<sup><a href="#5">5</a></sup>. Here Biology dictates which analytical, experimental and computational methods are required.

Modern analytical methods to measure the identity and quantity of biomolecules system-wide, summarized under the term “quantitative omics”-technologies, address the first two mentioned challenges of Systems Biology. Among these “omics”-technologies are transcriptomics based on microarrays/next generation sequencing and proteomics/metabolomics based on mass-spectrometry.

Tying in with the area of genome sequencing, the focus is set on the accurate profiling of gene/protein expression and metabolite concentrations, as well as on the determination of biological protein modifications and of physical interactions between proteins.

Addressing the abovementioned challenges three and four of Systems Biology, the development of numerous computational approaches reaches out to unravel the intrinsic complexity of biological systems<sup><a href="#6">6</a></sup>. These computational approaches focus on knowledge discovery and on in silico simulation or modeling<sup><a href="#7">7</a></sup>. In the latter approach knowledge on a biological process is converted into a mathematical model. In silico simulations based on such a model can provide predictions that may subsequently be tested experimentally. Computation-based knowledge discovery (also known as data mining) aims to extract hidden patterns from complex and high-dimensional data to generate hypotheses. Therefore, the first step is to describe information on a biological system such that it is sustainably stored in a format rendering it readable and manipulable for machines and humans. The second step is to integrate the huge amount of differently structured data, often referred to as the “big data” challenge. In a last step, statistical or machine learning methods are applied to extract the information or underlying principles hidden in the data.

The most flexible way of working with huge amounts of data is using a lightweight programming language with a succinct syntax. Therefore, it becomes necessary that biologist become familiar with a suitable programming language to solve real world problems in (Systems) Biology.
</div>
<div class="column is-4">
![](img/OmicSpace.png)
<b>Figure 1: A conceptual view of the omic space.</b> The omics space comprises of genomic, transcriptomic, proteomic, metabolomic and phenomic systems level represented as a plane. Complex biological function is the result of the interplay between molecules of one and/or different systems level.
</div>
</div>

## Starting with FSharp


F# (pronounced “F Sharp”) is a simple and expressive programming language. It can be described as statically typed impure functional language that supports functional, imperative and object-oriented paradigm and also several other programming styles including data-driven, event-driven and parallel programming. This makes it an excellent tool for introducing programming as well as programming paradigms.

F# is supported by the <a href="http://fsharp.org">F# Software Foundation</a> and a worldwide community of contributors. Microsoft and other companies develop professional tooling for F#. The <a href="https://docs.microsoft.com/en-us/dotnet/articles/fsharp/">F# Language Reference</a> is a reference for the F# language, and the <a href="https://docs.microsoft.com/en-us/dotnet/articles/fsharp/">F# Guide</a> covers general topics. There are lots of excellent <a href="http://fsharp.org/learn.html">F# learning resources</a> available online.
   
To learn more about how to use Jupyter notebooks, see the [Jupyter documentation](http://jupyter-notebook.readthedocs.io/) and the [Jupyter keyboard shortcuts](https://www.cheatography.com/weidadeyue/cheat-sheets/jupyter-notebook/). You can find more information baoutthe F# and Jupyter tooling locally using <a href="https://github.com/fsprojects/IfSharp">IfSharp</a>.
  
With the help of the following FSharp coding information/examples, you will be able to solve all exercises in <a href="JP01_FSharpExcercises.ipynb" style="color: orange">JP01_FsharpExcercises</a>!
   
Let's start with our basic introduction:

## Functions

The impetus behind functional programming comes from mathematics. Mathematical functions have a number of very nice features that functional languages try to emulate in the real world.
So first, let’s start with a mathematical function that adds 1 to a number.

```
Add1(x) = x+1
```

What does this really mean? Well it seems pretty straightforward. It means that there is an operation that starts with a number, and adds one to it.
   
**Let’s introduce some terminology:**

 - The set of values that can be used as input to the function is called the domain. In this case, it could be the set of real numbers, but to make life simpler for now, let’s restrict it to integers only.

 - The set of possible output values from the function is called the range (technically, the image on the codomain). In this case, it is also the set of integers.

 - The function is said to map the domain to the range.

A diagram of a general function would be:

![](img/function.png)

<b>Figure 2: diagram of a general function</b>

## Binding function values and simple values

The process of using a name to represent a function or a value is called “binding“. A binding is done by using the `let` keyword in F#. Let’s look at the simple function we used previously:

*)

let add1 x = x + 1
// evaluate
add1

(**
```
val add1: 
   x: int 
   -> int
```

What does the “x” mean here? It means:

 - Accept some value from the input domain.

 - Use the name ”x” to represent that value so that we can refer to it later. The name “x” is "bound" to the input value. So if we evaluate the function with the input 5 say, what is happening is that everywhere we see “x” in the original definition, we replace it with “5”, sort of like search and replace in a word processor

*)

add1 5
// replace "x" with "5"
// add1 5 = 5 + 1 = 6
// result is 6

(***include-it***)

(**
If you think about this a bit more, you will see that the name `add1` itself is just a binding to _the function that adds one to its input_. 
The function itself is independent of the name it is bound to. When you type let `add1` x = x + 1 you are telling the F# compiler “every time you see the name add1, 
replace it with _the function that adds one to its input_”. `add1` is called a function value. To see that the function is independent of its name, try:
*)

let plus1 = add1
add1 5 = plus1 5

(***include-it***)

(**
You can see that `add1` and `plus1` are two names that refer ("bound to") to the same function. You can always identify a function value because its signature has the standard form domain -> range. 
Here is a generic function value signature:

```
val functionName : domain -> range
```

Side note: Lambda expressions

In F# it is possible to use function without giving them a name and use the keyword `fun` instead and the `=` becomes `->`. 
This is called anonymous function, or - referring to lambda calculus - **lambda expression**. This kind of functions are often used for convenience. To write `add1` as lambda expression:
*)

fun x -> x + 1

(**
## Simple values

![](img/valueBinding.png)
      
Imagine an operation that always returned the integer 5 and didn’t have any input.
   
This would be a “constant” operation.
How would we write this in F#? We want to tell the F# compiler “every time you see the name c, replace it with 5”. Here’s how:
*)

let c = 5
// evaluate
c

(***include-it***)

(**
There is no mapping arrow this time, just a single int. What’s new is an equals sign with the actual value printed after it. The F# compiler knows that this binding has a known value which it will always return, namely the value 5.
In other words, we’ve just defined a constant, or in F# terms, a simple value.

## Lists & Arrays

Square brackets `[]` create a list with semicolon `;` delimiters.
*)

let twoToFive = [2; 3; 4; 5]
twoToFive

(***include-it***)

(**
`::` creates a list with a new element appended to the front of the list.
*)

let oneToFive = 1::twoToFive
oneToFive

(***include-it***)

(**
Square brackets with dashes `[||]` create an array with semicolon `;` delimiters.
*)

let oneToFour = [|1; 2; 3; 4|]

(**
Elements can be accessed using dot `.[i]`, where i is the (zero-based) index of the desired element
*)

oneToFour.[0]

(***include-it***)

(**
_Note_: commas are **never** used as delimiters in collection types, only semicolons!

## Higher-order functions

<div class="columns">
<div class="column is-8">
A higher-order function is a function that takes another function as a parameter. 
This is simple, but leads to one of the most important concepts of functional programming: The conceptual operation: <strong>map</strong>

The higher-order and polytypic function `map` applies a function working on the normal space to an elevated space.
This concept is so important that all collection types (lists, arrays, ...) have a build in `map` function. 
Let's look at an example of what that means. Therefore, we first define a function working on the normal space:
</div>
<div class="column is-4">
![](img/map.png)
<b>Figure 3: A conceptual view of the `map` concept.</b>
</div>
</div>
*)

let square x = x * x
// evaluate
square 3

(***include-it***)

(**
Now, let's apply this function to every number in a list.
*)

List.map square [3;2;6;7]

(***include-it***)

(**
Be aware, that this concept of mapping is restricted to the actual function called `map`. A `filter` function, for example, is also the same kind of operation.
*)

let evens list =
   let isEven x = x%2 = 0
   List.filter isEven list 
   
//isEven 5      // the value or constructor isEven is not defined. .. this is because it is only defined inside 
                // of the functional scope of `evens`.

(**
Additionaly, you can see in this example how to define a multiline function. Just use indents! No `;` needed.
Define `isEven` as an inner ("nested") function. In this case the function `isEven` is defined in the scope of the function `evens`. It cannot be accessed outside of this scope.
`List.filter` is a library function with two parameters: a `predicate` function, returning `true` or `false` depending on the input - and a `list` to work on.
*)

evens [1..5]

(***include-it***)

(**
You can use `()` to clarify precedence (think brackets in math). In this example, do `List.map` first, with two parameters. Then do `List.sum` on the result. `List.map` applies a function to all elements in the list.
*)

let sumOfSquaresTo100 =
   List.sum (List.map square [1..100])

sumOfSquaresTo100

(***include-it***)

(**
Without the `()`, `List.map` would be passed as an parameter to `List.sum`.

## Side Note: Pipe-forward operator `|>`

The Pipe-forward operator lets you pass an intermediate result (value) onto the next function, it’s defined as: 

*)

let (|>) x f = f x 

(**

Now, you can pipe the output of one operation to the next using `|>`.
Here is the same `sumOfSquares` function written using pipes.

*)

let sumOfSquaresTo100piped =
   [1..100] 
   |> List.map square 
   |> List.sum

sumOfSquaresTo100piped

(***include-it***)

(**
In this case one often uses anonymous functions using the `fun` keyword. This saves time to think about a name and the function can be writen inline.
*)

let sumOfSquaresTo100withFun =
   [1..100] 
   |> List.map (fun x -> x * x) 
   |> List.sum
   
sumOfSquaresTo100withFun

(***include-it***)

(**
<div class="columns">
<div class="column is-8">

You already used the `List.sum` function. It is important to notice that this function doesn’t follow the `map` concept. There is a second related concept called `fold`.  The higher-order and polytypic function `fold` applies a function working on the normal space to an elevated space and reduces the elevated space into the normal space. This results in an aggregation. A simple but concreate example would be to sum a list of numeric values.

</div>
<div class="column is-4">

![](img/fold.png)

<b>Figure 4: A conceptual view of the `fold` concept.</b>

</div>
</div>

## Control flow expressions

Control flow expressions are used to determine the program pathing under multiple possible conditions. These different paths must always lead to the same `Type` (e.g. `string`).

### if-then-else

`if-then-else` is an expression and must return a value of a particular type.
It evaluates to a different value depending on the `boolean` expression given.

Both branches must return the same type!

*)

let v = if true then "a" else "b"
v

(***include-it***)

(**

### Pattern Matching

Pattern matchings are one method to apply these control flow expressions. These function similiar to the <a href="#if-then-else">if then else</a> expression, but much more powerful.

*)

let simplePatternMatch x =
   match x with
    | "a" -> printfn "input is a"
    | "b" -> printfn "input is b"
    | _   -> printfn "input is something else"

(**
<div Style="text-align: justify ; font-size: 1.8rem ; margin-top: 2rem ; line-height: 1.5">
    Underscore `_` matches anything
</div>
*)

simplePatternMatch "a" 

(***include-output***)

simplePatternMatch "I will not match"

(***include-output***)

(**
In the following we will use a `printfn` function. Normally in FSharp only the last output is returned, but side effects, can always be returned. As a rule of thumb: All Unit outputs are side effects. <br>
In this case, this means, we will print the result and still can keep working with the output.
Also you will notice, that the last output is only `f (1=3)` -> "b", but we still will get all other results, as we print them below.
*)

let f x = 
    if x then 
        printfn "a";
        "a" 
    else
        printfn "b"
        "b"

f false

(***include-it***)

f true

(***include-it***)

f (1=1)

(***include-it***)

f (1=3)

(***include-it***)

(**
## Complex Data Types

### Tuples

Tuple types are pairs, triples, and so on of values.

Tuples use commas `,` as delimiter.

*)

let twoTuple = 1,2
twoTuple

(***include-it***)

let threeTuple = "a",2,true
threeTuple

(***include-it***)

(**
### Record Types

Record types have named fields. They use Semicolons `;` as separators.

*)

type Person = {FirstName:string; LastName:string}

let person1 = {FirstName = "John"; LastName = "Doe"}
person1

(***include-it***)

(**
Field of a record type can be acessed individually with a dot `.Name`
*)

person1.FirstName

(***include-it***)

(**
## Code organization

Sometimes it can be necessary to organize code for example to ship a library to other users. Namespaces und Modules are top-level and low-level constructs to organize code. 

### Namespaces and modules

You can think of namespaces and modules as containers and sub containers, respectively, in which you can put function and type definitions. The hierarchy is defined that you can have multiple modules in one namespace, also nested modules in a module, but no namespace in another namespace. You can acces namespaces and modules with the `.` operator.

*)

//Module “container” 
module WidgetsModule =
    let widgetName = "FSharp"
    let widgetFunction x y =
        sprintf "%s %s" x y
        // printfn "%s %s" x y

// Calls the function from the module
WidgetsModule.widgetFunction "Hello" WidgetsModule.widgetName

(***include-it***)

(**
### Comments

Comments also help to write organized code.

**Comments are text written in code area (often marked green) which will be ignored by the compiler and not be executed.**

`//` single line comments use a double slash

(* multi-line or in-line comments use `(* . . . *)` pair -end of multi line comment- *)

*)

type PersonalInformation =
    {
        //First name of a person
        FirstName  :string
        //Last name of a person
        LastName   :string
        (*Address and
        phone number of a person*)
        Address    : (*int*) string
        PhoneNumber: int
    }

(**
## References

<ol>
<li Id="1"> Bertalanffy, L. von. Zu einer allgemeinen Systemlehre. Blätter für deutsche Philosophie 18 (1945).</li>

<li Id="2"> Sauer, U., Heinemann, M. & Zamboni, N. Genetics. Getting closer to the whole picture. Science 316, 550–551; 10.1126/science.1142502 (2007).</li>

<li Id="3"> Kitano, H. Systems biology. a brief overview. Science 295, 1662–1664; 10.1126/science.1069492 (2002).</li>

<li Id="4"> Joyce, A. R. & Palsson, B. O. The model organism as a system. integrating 'omics' data sets. Nat Rev Mol Cell Bio 7, 198–210; 10.1038/Nrm1857 (2006).</li>

<li Id="5"> Aderem, A. Systems biology. Its practice and challenges. Cell 121, 511–513; 10.1016/j.cell.2005.04.020 (2005).</li>

<li Id="6"> Kahlem, P. & Birney, E. Dry work in a wet world. computation in systems biology. Mol Syst Biol 2 (2006).</li>

<li Id="7"> Kitano, H. Computational systems biology. Nature 420, 206–210; 10.1038/nature01254 (2002). </li>
</ol>
*)