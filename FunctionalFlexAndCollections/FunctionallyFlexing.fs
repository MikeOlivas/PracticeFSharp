// A closer look at some of the functional language support F# has to offer
// ...including some collections especially suited for functional programming
// ...in addition to support for the "normal" .NET generics/collections.
// The following excersizes were inspired from Chapters 5 and 6 of "The Book of F#",
// by Dave Fancher.
// Michael Olivas

open System
open System.IO

//Discriminated union used for simple object hierarchy
type Shape = | Square    of side: float
             | Rectangle of width:float * height:float
             | Triangle  of leg1:float * leg2:float * leg3:float

             //they also allow for additional members
             override x.ToString() =
               match x with 
                 | Square(s)         -> sprintf "(Square: sideLength = %.2f)" s
                 | Rectangle(w,h)    -> sprintf "(Rectangle: %.2f x %.2f.)" w h
                 | Triangle(l1,l2,l3)-> sprintf "(Triangle: %.2f x %.2f x %.2f)" l1 l2 l3

(**Records** are defined by specifying the type keyword, an identifier, and a list of labels with type annotations in braces*)
type moneyClip = { Ones:int; Fives:int; Tens:int; Twenties:int }
                   override x.ToString() =
                     sprintf "Ones:%i, Fives:%i, Tens:%i, Twenties:%i" x.Ones x.Fives x.Tens x.Twenties
                   static member (+) (l:moneyClip, r:moneyClip) =
                     { Ones     = l.Ones + r.Ones
                       Fives    = l.Fives + r.Fives
                       Tens     = l.Tens + r.Tens
                       Twenties = l.Twenties + r.Twenties }

type wallet    = { Ones:int; Fives:int; Tens:int; Twenties:int }

[<EntryPoint>]
let main argv = 
    //F# abstracts away th cumbersome details of delegation with the -> token
    //so that we can make use of "higher-order functions". Need to be careful
    //when working with other languages' .NET librarys though. Behind the scenes
    //they use the "Func" and "action" delegate types in conjunction with
    //lambda expressions as opposed to F#s overloaded delegate types. They are
    //not interchangeable. (see page 105 for possible conversion functions)

    (*******************************************************************************************
     *Currying: In F#, every function accepts only one input and returns exactly one output.   *
     *So, "mulf" is bound to accept parameter 'a' and returns a function (flaot -> float). This     *
     *returned function then takes one parameter, 'b', as input and returns a float result. *
     *This function chaning is called "currying" and a key ingredient in a lot of functional   *
     *flavor that F# (and probably other functional languages) offers.                         *
     *******************************************************************************************)
    //the following two examples are inspired from The Book of F# to demonstrate currying visually
    //notice each has the same signature but the second definition more closely resembles the compiled code
    //let mul (a:float) (b:float) = a * b              //val mulf: (float -> float -> float)
    let mulf a   = fun b -> (*) (a:float) (b:float)    //val mul: (float -> float -> float)

    //This principle enables the ability to create new functions from existing, partially applied ones
    //i.e. "Partial Application. Note currying applies arguments one at a time from left to right.
    let mulfBy10 = mulf 10.             //val mulfBy10: (float -> float). Compiler binds partially evaluated function to identifyer
    mulfBy10 5. |> ignore               //val it : float = 50              Partial application. Now any float can be multiplied by 10.

    //Forward pipelining is show directly above; but you can do much more than simply ignore the output
    //As long as the last argument of the recieving function is compatible with the source's return type, you can chain many things
    let add x y = x + y
    let add1    = add 1.
    mulfBy10 5.  |> add1
                 |> printfn "10 x 5 + 1 = %f"

    //A Backward pipelining operator exists to send the result of an expression to another function as the final argument from right to left
    printfn "10 x 5 = %f" <| mulfBy10 5.

    //You can use both forward and backwards piping together.
    //Could be more expressive, for example, to pass operands to an infix operator
    //Notice the parenthesis here! Figured out what the problem was before - precedence, it's a thing!
    printfn "10 x 5 + 1 = %f" (mulf 10. 5. |> add <| 1.)

    // function composition defines a new function composed of other functions (not a one time operation)
    let dollarsToPounds usd = usd * 0.76                                            //current conversion rate
    let billsInUSD          = [1486.50;327.25;139.50;225.;975.;130.;250.;210.;85.]  //Mortage;car;truck;insurance;health;water;electric;cells;nwnatural;...
    
    // here is a one time operation using forward piping
    billsInUSD |> List.sum
               |> dollarsToPounds
               |> printfn "Total monthly bills are about %.2f. pounds"

    //Here we have a function, TotalInPounds, composed of the sum function and the dollarsToPounds function
    let TotalInPounds = List.sum >> dollarsToPounds            //val billsInPounds: (float list -> float)
    billsInUSD |> TotalInPounds                                //Input a list of floats to composed function
               |> printfn "Total bills are %.2f pounds"               //Pipe result to print function and viola!

    //Recursive functions are the preferred looping mechanism in functional programming
    //must include "rec" keyword in binding signature to signal to compiler this function
    //needs to be able to call itself; else compiler will comiplain
    let rec payDown total byAmnt =
        match (total <= 0.01) with
            | true  -> printfn "Bills payed off, yay!"
            | false -> //printfn "%.2f left to go" total
                       payDown (total - byAmnt) byAmnt         //This needs to calculate a value at every iteration
                                                               //why doesn't it need to add each call to the stack
                                                               //need to better understand tail recursion
    //call payDown function to pay down total by 50 pound per day
    billsInUSD |> TotalInPounds
               |> payDown <| 50.                              //Very expressive! Matches closely what we're actually trying to accomplish.

    //Lambda Expressions syntax forgoes identifiers in favor of func keyword and it takes its definition inline
    billsInUSD |> List.sum |> (fun usd -> usd * 0.76) |> printfn "We paid off %.2f pounds!"

    (******************************************************************************************************
     **Functional data types** such as tuples, records, and discriminated unions are data types that lend *
     * themselves nicely to functional programming; and, as such, are well-supported in F#                *
     ******************************************************************************************************)
    //Tuples:
    let tuBill_1 = (1486.50, "Mortgage")                     //signature uses cartesion product sytnax i.e.
    let tuBill_2 = (327.25,  "Car")                          //...both of these values are type float * string
    let tuBill_3 = 139.50, "Truck"                           //they need not be enclosed in parenthesis
    let tuBill_4 : float * string = 225., "AutoInsurance"    //you can also explicitly use type annotation to declare
    let tuBill_5 = (975., "HealthInsurance")
    let tuBill_6 = (130., "Water")
    let tuBill_7 = (250., "Electric")
    let tuBill_8 = (210., "CellPhones")
    let tuBill_9 = (85., "NWNatural")

    //Tuples are compiled in to one of several generic types all named System.Tuple (overloaded on the "arity")
    let tuRandom = "How", ' ', "many", ' ', "items", ' ', "can", ' ', "we", ' ', "store", ' ', "in", ' ', "a", ' ', "tuple", ' ', "?"

    //pairs have built in functions to extract the first (fst) and second (snd) values
    snd tuBill_1 |> printfn "The first bill is for the %s"

    //no syntactic help exists for triples (or tuples with higher arity) but you can easily define a function, if desired
    //the (_) is a wildcard and here we use it to ignore all but the fifth element
    let fifth (_, _, _, _, x, _, _, _, _, _, _, _, _, _, _, _, _, _, _) = x

    //you can see this would quickly get unmanagable because its only valid for the typle type which matches its "arity"
    fifth tuRandom |> printfn "The fifth element of tuRandom is \"%s\"."

    //more pracitcally, you can specify identifiers for individual tuple values when appropriate
    let triple = (1,2,3)
    let x1, y1, z1 = triple
    printfn "3D Point: (%i,%i,%i)" x1 y1 z1

    //tuples are NOT COLLECTIONS; they don't implement IEnumerable and cannot be iterated over
    //but they do implement the IStructuralEquatable interface which ensures equality comparisons are done component-wise
    match tuBill_1 = tuBill_2 with |true  -> printfn "tuBill_1 and _2 are equal"
                                   |false -> printfn "tuBill_1 and _2 NOT equal"

    match tuBill_1 = tuBill_1 with |true  -> printfn "tuBill_1 = tuBill_1"
                                   |false -> printfn "Something strange is afoot."

    //**Records** (wallet record defined at top of document)
    //unlike tuples, we don't need functions to extract elements because they each have labels
    let billfold = {Ones = 3; Fives = 2; Tens = 0; Twenties = 10}
    billfold.Twenties |> printfn "I have %i twenty dollar bills, ya'all."

    //but type inference can succeed and still not be what we expect if two records share the same structure
    //uses top-down evaluation... so most recently defined type that matches labels is assumed correct
    let blingBling = {Ones = 200; Fives = 0; Tens = 0; Twenties = 20}    //val blingBling: wallet (not moneyClip!)

    //However, you can be explicit in the first (or all) elements of your record expression
    let clip = {moneyClip.Ones = 200; Fives = 0; Tens = 0; Twenties = 20} //val clip: moneyClip

    //"Copy and update record expression" syntax makes it easy to clone a record and simply change one or two things
    let clipZ = { clip with Ones = 0; Tens = 20}                          //he coloured up his ones
    
    //You can add additional members to records (since they're basically sytactic sugar for classes)
    let moneyPile = clip + clipZ
    clipZ.ToString()     |> printfn "clipZ content = %s"
    clip.ToString()      |> printfn "clip content = %s"
    moneyPile.ToString() |> printfn "Pile o' cash = %s"

    //Discriminated unions are user-defined data types but limited to a known set of values (union cases)
    //Not just a label like enums in other languages (which aren't restricted to a range but rather just rename something)
    //Commonly used example is the Option type we've been using for nullibility
    (* definition from book:
       type Option<'T> =
       | None
       | Some of 'T
    *)

    //Discriminated unions are commonly used as simple object hierarchies
    //...they can replace forman classes and inheritance with a cleaner alternative.
    //**Defined up top!**

    //create a Shape instance:
    let s = Square(3.)
    let r = Rectangle(1., 5.)
    let t = Triangle(1., 2., 5.)

    //user overloaded additional member function to output legs of triangle
    printfn "%s" <| t.ToString()

    (*********************************************************************************************************************
     *A *SEQUENCE* is any type that implements IEnumerable<'T> in .NET - a collection of values that share a common type.*
     *i.e. NOT ArrayList nor Hashtable - which are legacy collection types that implement the nongeneric IEnumerable     *
     *interface. However, Dictionary<'TKey, 'TValue> and List<'T> - even String are sequences.                           *
     *********************************************************************************************************************)
    //Create a sequence by specifying a "sequence expression" in a sequence builder - using a do binding to yeild each element of sequence
    let bills = seq { use reader = new System.IO.StreamReader(@"C:\Users\Michael\Documents\Visual Studio 2015\Projects\FunctionalCollections\Bills.txt")
                      while not reader.EndOfStream do 
                      let line = reader.ReadLine().Split(',')
                      yield float line.[0], line.[1] }

    //only the first four elements will print, by default. Need to ennumerate over entire sequence if you want more.
    bills |> printfn "%A"

    //Range sequence expressions allow you to compactly create a sequence over a defined range of values
    let intRange  = seq { 0..20 }
    let stepRange = seq { 0..5..20 }    //supports sized steps but only over numerical values
    printfn "%A" <| stepRange           
    let decRange  = seq { 20..-1..0 }   //supports declining values with a negative step (integral)
    printfn "%A" <| decRange
    let fltRange  = seq {-5.0..15.0}
    let charRange = seq { 'a'..'z' }    //supports char ranges also

    //example from the book of using "init" function to create up to 10 randomly generated elements
    let random = System.Random()
    Seq.init 10 (fun _ -> random.Next(1000)) |> printfn "%A"

    //There exist some useful functions for working with sequences in the Seq module
    stepRange |> Seq.length |> printfn "stepRange has %d elements"          //determining the length of a sequence
    charRange |> Seq.iter (printf "%c")                                     //iterating over each element of a sequence
    printfn " ...now I know my abc's; next time won't you sing with meeeeeee!"

    //map applies a function to every element in a sequence. Since seqs are inherently immutible, it builds a new sequence with the results
    fltRange  |> Seq.map (fun x -> x + System.Math.PI) |> Seq.iter (printfn "%.3f") //because adding a little PI solves everything

    //sortBy allows you to sort a sequence by a particular element; here some bills are output in alphabetical order
    bills |> Seq.sortBy snd |> Seq.iter (printfn "%A")

    //filter allows you to work with only elements of a sequence that meet certain criteria
    bills |> Seq.filter (fun (amnt, _) -> amnt > 500.) |> Seq.iter (printfn "%A")   //inspect bills greater than $500

    //Seq.sum will computer the sum of a sequence
    let sumBills   = bills |> Seq.map (fun (amnt, _) -> dollarsToPounds amnt) |> Seq.sum
    let prevTotal  = billsInUSD |> TotalInPounds
    match sumBills = prevTotal with | true  -> printfn "Our sanity check \"paid off\"! The sum of the Sequence is the List sum"
                                    | false -> printfn "Either our math or our programming skills leave something to be desired. -_-"
    
    (*Arrays* in F# have a fixed number of (the same type of) values and are zero-based. 
     *The array binding is immutable but the individual elements themselves are mutable
     *so one must take care not to introduce unwanted side effects. *)
     //Arrays are often created with an "array expression" or semicolon-delimited list of values
     //enclosed between the tokens [| and |] as follows
    let arrayOfBills = [| tuBill_1; tuBill_2; tuBill_3; tuBill_4; tuBill_5; tuBill_6; tuBill_7; tuBill_8; tuBill_9 |]

    //You can also generate an array with a sequence expression as before (look familiar?)
    let billArray    = [| use reader = new System.IO.StreamReader(@"C:\Users\Michael\Documents\Visual Studio 2015\Projects\FunctionalCollections\Bills.txt")
                          while not reader.EndOfStream do 
                          let line = reader.ReadLine().Split(',')
                          yield float line.[0], line.[1] |]

    printfn "%A" arrayOfBills

    let sameArray (l:(float * string)[]) (r:(float * string)[]) = 
      let size = System.Math.Min(l.Length, r.Length)
      if l.Length > size || size < r.Length then
          //false
          printfn "The sizes are different" 
      else
          let rec isMatch (sz:int) =
            match l.[sz] = r.[sz] with | false -> //false
                                                  l.[sz] |> printfn "%A != %A" <| r.[sz]
                                       | _     -> if sz < 1 then
                                                    //true
                                                    printfn "They are the same!"
                                                  else
                                                      l.[sz] |> printfn "%A =? %A" <| r.[sz]
                                                      isMatch (sz - 1)
          isMatch (size-1)

    let result = sameArray arrayOfBills billArray

    match arrayOfBills = billArray with | true -> printfn "MATCH!"
                                        | false -> printfn "NO MATCH! :-("

    System.Console.ReadKey() |> ignore
    0 // return an integer exit code
