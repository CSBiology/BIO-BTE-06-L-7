(**
# NB00b FSharp Coding literacy part II

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB00b_FSharp_Coding_literacy_Part_II.ipynb)


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

Definitely, F# can be a good friend in your daily work even when you are doing simple calculations e.g., in the lab. Let’s jump right in and some stuff done:
*)
// Define numerical values
let co2Oxygens = 2
let co2Carbon  = 1
let oxygenMass = 15.9994
let carbonMass = 12.0107
let avogadro = 6.023e23
