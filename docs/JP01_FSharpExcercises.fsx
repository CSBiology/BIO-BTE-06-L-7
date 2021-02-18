(** 
This notebook contains introductional excercises for the FSharp programming language and also some small code examples for correct formatting.
If you have questions at any point please ask us. We will discuss the first batch (task 1-6) before you can start the rest.

# FSharp Introduction Excercises

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP01_FSharpExcercises.ipynb)


1. [Code Examples](#Code-Examples)
2. [Excercises](#Excercises)
    * [Task 1](#Task-1)
    * [Task 2](#Task-2)
    * [Task 3](#Task-3)
    * [Task 4](#Task-4)
    * [Task 5](#Task-5)
    * [Task 6](#Task-6)
    * [Task 7](#Task-7)
    * [Task 8](#Task-8)
    * [Task 9](#Task-9)
    * [Task 10](#Task-10)
    * [Task 11](#Task-11)

<hr>

## Code Examples
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>


We will beginn with some minor code examples to show you correct formatting. You can even use these examples to help you understand minor programming logics for your excercises below.

*)

// Define a function 'mulitplyBy2', which should double any input Float.

let mulitplyBy2 x = x * 2.

mulitplyBy2 3.

(*** include-it ***)

(**
If a excercise asks you to bind something to a specific name, keep that name! It might be used later on!
Also remember to use camel case for names. This works by having a regular first letter and then have a capital letter at the start of each new word.

exmp: <code>thisIsACamelCaseExample</code>

*)

// Create a function 'calculateCylinderVolume', which should, with a given radius and lenght, 
// calculate and return the volume of a cylinder AND print "The volume of the cylinder is: XX." into the console. 'XX' is,
// of course, the correct volume for the cylinder. (The output of a printfn function is Unit.)

let calculateCylinderVolume radius length : float =
   // function body
   let pi = 3.14159 //or System.Math.PI
   let volume = length * pi * radius * radius
   printfn "Das Volumen des Zylinders ist: %f" volume
   volume

calculateCylinderVolume 2. 10.
   
(*** include-it ***)
(*** include-output ***)

(**

<ul>
    <li>Try to follow the excercise as close as possible, to not overlook something like ".. calculate (..) the volume of a cylinder AND print .." as these small differences migth be a valuable part of the excercise.</li>
    <li>Lines of code do not have any cost, don't be to stingy about them, e.g. have an extra line with a binding of <code>let pi = 3.14159</code>, so you can just use "pi" instead of the number. </li>
    <li>This improves readability and keeps the function modular.</li>
    <li>By defining pi inside of the scope of <code>calculateCylinderVolume</code> it cannot be accessed from outside, keeping your overall code nice and clean.</li>
</ul>

*)

// A chessboard is a 8x8 field. The rows and columns have the indices 0 - 7.
// Create a function, which should return true if two queens can hit each other or false if they can't.
// The functions parameters should be two Tuples (int*int).

let canHit (queen1 : int*int) (queen2 : int*int) =
    let (posX1, posY1),(posX2, posY2)= (queen1,queen2)
    if 
        // check if both queens are on the same x-axis
        (posX1 = posX2) 
        // check if both queens are on the same y-axis
        || (posY1 = posY2) 
        // check if both queens are on the same diagonal
        || (abs (posX1 - posX2) = abs (posY1 - posY2))
    then
        printfn "Both queens can hit each other"
        true
    else 
        printfn "Both queen cannot hit each other"
        false
        
canHit (3,5) (5,7)

(*** include-it ***)
(*** include-output ***)

(**
<hr>
 
## Excercises 
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>

### Task 1
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>

*)

// Bind a String of your name ("Name") to the name 'myFirstName'.

// Solution

let myFirstName = "Kevin"

myFirstName

(*** include-it ***)

(**
### Task 2
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Define a variable 'myName', by adding 'myFirstName' to the rest of your name.

// Solution

let myName = myFirstName + " Frey"

myName

(*** include-it ***)

(** 
### Task 3
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Define a variable 'summeXY' as the sum of any two Integer numbers. Do this, by defining x and y as separate 
// let - bindings inside the functional scope of the 'summeXY' function. 
// (The function MUST contain three let - bindings)

// Solution

let summeXY =
    let x = 2
    let y = 34
    x + y


(*** include-value:summeXY ***)

(** 
### Task 4
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Declare a Tuple, consisting of 2 and "February" and bind the Tuple to the name 'tuple1'

// Solution 

let tuple1 = (2,"February")

(*** include-value:tuple1 ***)

(**
### Task 5
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Declare a Tuple, consisting of 2, "February" and "spring" and bind the Tuple to the name 'tuple2'

// Solution 

let tuple2 = (2,"February","spring")

(*** include-value:tuple2 ***)

(**
### Task 6 
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Access the second variable of the Tuple 'tuple1'.
// Access the second variable of the Tuple 'tuple2'.

// Solution 

let solution1 = snd tuple1

let solution2 = (fun (x,y,z) -> y) tuple2 // or: let (x,y,z) = tuple2

(*** include-value:solution1 ***)
(*** include-value:solution2 ***)

(** 
### Task 7
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Create a Record Type with the name 'Month'. 'Month' should contain the fields 'Number' (int),
// 'Name' (string) and 'Season' (string).

// Solution

type Month =
    {
        Number: int
        Name:   string
        Season: string
    }
    
(** 
### Task 8
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Declare a function 'createMonth' with three input parameters and which should return a 'Month' - Record Type.
// Use the values from 'tuple2' and create a month with these as input.  

// Solution

let createMonth number name season =
    {
        Number = number
        Name   = name
        Season = season
    }
    
let feb = createMonth 2 "February" "spring"
feb

(*** include-value:feb ***)

(** 
### Task 9
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Create a function called 'myMonthsPrinter' which should take a 'Month' as input and return a unit output 
// (printfn - command) in the form of "My favorite month is [Month Name]. It is the [Number of Month] month
// in the year and i especially like this time because it is part of [Season]."

// Solution

let myMonthsPrinter (month:Month) =
    printfn "
        My favorite month is %s. It is the %i month in the year 
        and i especially like this time because it is part of %s."
        month.Name
        month.Number
        month.Season
        
myMonthsPrinter feb

(*** include-output ***)

(** 
### Task 10
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// We want to put additional emphasis on readability of code ...

// Oh no, the following function is not readable. Write the function new and use pipe operators!

let unreadableFunc x = (string ((x - 7) * 2) + "Generic").ToUpper().ToCharArray()

// Solution

let readableFunc (x:int) = 
    x
    |> fun x -> x - 7
    |> fun x -> x * 2
    |> string
    |> fun x -> x + "Generic"
    |> fun x -> x.ToUpper().ToCharArray()
    
readableFunc 4 = unreadableFunc 4

(*** include-it ***)

(** 
### Task 11
<a href="#FSharp-Introduction-Excercises" style="display: inline-block"><sup>&#8593;back</sup></a><br>
*)

// Create a function 'replaceSmallNumbers', which should replace all 
// integers in a list with a 0 if the integer is smaller than 5.

let numList = [0 .. 20]

// Solution

let replaceSmallNumbers (numList:int list) =
    numList
    |> List.map (
        fun x ->
            if x < 5 then
                0
            else x
        )
    
replaceSmallNumbers numList

(*** include-it ***)
(**
<nav class="level is-mobile">
    <div class="level-item">
        <button class="button is-primary is-outlined" onclick="location.href='/JP00_Systems_Biology_FSharp_Introduction.html';">&#171; JP00</button>
    </div>
    <div class="level-item">
        <button class="button is-primary is-outlined" onclick="location.href='/JP02_Plant_Systems_Biology.html';">JP02 &#187;</button>
    </div>
</nav>
*)