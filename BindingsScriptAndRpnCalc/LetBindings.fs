(* The following is a simple exploration of some of the various bindings available in F#
 * and how to operate on them.
 * Michael Olivas *)

module letBindings = 

    //Let bindings (most common)
    let intValue = 1                    //bind an int value
    let strValue = "Hello F# World"     //bind a string value
    let add a b = a + b                 //bind a function
    let sum = add 2 intValue            //bind an expression
    let test = float 10/4.0             //bind a float
    let bool = true                     //bind a bool value

    //A Literal decorator is used to declare a true .NET constant value
    [<Literal>]
    let CONST = 1024

    //The above are all immutable bindings. must use "mutable" keyword to be able to update its value
    let mutable name = "MikeO"          //bind a mutable value
    name <- "Mike Olivas"               //F# Assignment operator updates value OK
    //strValue <- "Invalid"             //Compiler complains: can't update immutable binding

    let tuple = (add sum CONST, name) //bind a tuple object (1027, "Mike Olivas")

    [<EntryPoint>]
    let main argv =
        (* printf is preferable to composite formatting (e.g. Console.WriteLine) *
        * as it is statically type checked and supports native F# types         *)
        printfn "Int: %i" sum
        printfn "String: %s" strValue
        printfn "Float: %f" test
        printfn "Bool: %b" bool
        printfn "Tuple: %A" tuple
        0