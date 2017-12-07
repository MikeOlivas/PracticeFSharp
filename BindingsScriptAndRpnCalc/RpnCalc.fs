// The following code is an excerpt from "The Book of F#" by Dave Fancher (page 11)
// A calculator for solving mathematical expressions in posfix (Reverse Polish) notation
// My first F# Program (in leiu of "Hello Word")
// The author likes to start with this example (even though we don't yet understand it all)
// because it "not only shows some idiomaatic F# code, but it also demonstrates a
// number of important concepts, such as default immutability, functions as data, pattern matching, 
// recursion, library functions, partial application, F# lists, and pipelining."

[<AutoOpen>]
module TheBookOfFSharp.RpnCalc

open System

let evalRpnExpr (s : string) = 
  let solve items current =
    match (current, items) with
    | "+", y::x::t -> (x + y)::t
    | "-", y::x::t -> (x - y)::t
    | "*", y::x::t -> (x * y)::t
    | "/", y::x::t -> (x / y)::t
    | _ -> (float current)::items
  (s.Split(' ') |> Seq.fold solve []).Head

//uncomment the following decorator before compiling and running main
//[<EntryPoint>]    
let main argv = 
  [ "4 2 5 * + 1 3 2 * + /"
    "5 4 6 + /"
    "10 4 3 + 2 * -"
    "2 3 +"
    "90 34 12 33 55 66 + * - + -"
    "90 3 -" ]
  |> List.map (fun expr -> expr, evalRpnExpr expr)
  |> List.iter (fun (expr, result) -> printfn "(%s) = %A" expr result)
  Console.ReadLine() |> ignore
  0