(* The following excercises were used to explore the many different types of pattern matching
 * that is avaialble in F#. Some of the examples are pulled directly from Chapter 7 of Dave Fancher's
 * "The Book of F#" and others will bare a strong resemblence to the material. I was trying to
 * emulate and test all of the different possibilities he was presenting and add them to my repertois.
 * Michael Olivas
 *)

//Discriminated union used for simple object hierarchy
type Shape = | Square    of side: float
             | Rectangle of width:float * height:float
             | Triangle  of leg1:float * leg2:float * leg3:float

             override x.ToString() =
               match x with 
                 | Square(s)         -> sprintf "(Square: sideLength = %.2f)" s
                 | Rectangle(w,h)    -> sprintf "(Rectangle: %.2f x %.2f.)" w h
                 | Triangle(l1,l2,l3)-> sprintf "(Triangle: %.2f x %.2f x %.2f)" l1 l2 l3

//Decorate some identifiers with LiteralAttribute for later pattern matching
[<LiteralAttribute>]
let NullCap    = 0
[<LiteralAttribute>]
let ConsoleCap = 1
[<LiteralAttribute>]
let WindowCap  = 2

[<EntryPoint>]
let main argv = 
    //Match expressions in F# are more powerful than switch statements in other languages
    //Can evaluate more than just constant values
    //Sequentially evaluates patterns, must list from most to least specific
    let checkSome opt =
      match opt with
      | Some(x) -> printfn "Some exists (%i)!" x         //Requires "Exhaustive" matching
      | None    -> printfn "None."                     //Compiler complains if every pattern isn't listed in expression body
                                                         //If function were called with a case not covered by pattern(s),
                                                         //would through a MatchFailureException
    let exists = Some(7)
    let none   = None
    checkSome exists
    checkSome none

    //Guard clauses allow you to specify extra criterea for filtering
    //when there are identical match patterns
    let test num =
      match num with
      | x when x > 0 -> printfn "%i is positive." x      //"Variable patterns" match value and bind to identifier
      | x when x < 0 -> printfn "%i is negative." x      //Then is available for use within guard clauses.
      | _            -> printfn "Zero."

    test  0
    test -1
    test  2

    //Rather than just using match expressions, can also define a pattern matching function
    let funcTest =          //alternate syntax shortcut for creating a pattern matching lambda expression
      function
      | x when x > 0 -> printfn "Positive (%i)" x
      | x when x < 0 -> printfn "Negative (%i)" x
      | _            -> printfn "Zero."                 //Wildcard pattern works just like variable pattern but discards rather than binds matched value

    funcTest  0
    funcTest -1
    funcTest  2
    //can pass pattern matching functions to higher order functions
    [-3; 3; -2; 0; 2; -1; 1] |> List.map (funcTest) |> ignore

    //Matching Constant Values (chars, strings, and enum values)
    let toUpper =
      function
      | 'a' -> "A"
      | 'b' -> "B"
      | 'c' -> "C"
      |  x  -> System.Char.ToUpper x |> sprintf "%c"

    ['a'..'z'] |> List.map(toUpper) |> List.iter(printfn "%s")

    //When a pattern consists of more than a single character and starts with an uppercase letter,
    //the compiler tries to resolve it as a name - an "Identifier pattern" - which is usually
    //a discriminated union, LiteralAttribute, or Exception

    //Given the simple hierachy defined as a discriminated union above, we can define a function
    //that uses an Identifier pattern to calculate a shape's parameter
    let calculatePerimeter =
      function
      | Square(s)            -> s * 4.
      | Rectangle(w, h)      -> w * h * 2.
      | Triangle(l1, l2, l3) -> l1 + l2 + l3

    //declare a few instances for testing
    let s = Square(3.)
    let r = Rectangle(1., 5.)
    let t = Triangle(1., 2., 5.)

    //Iterate over list of objects outputting shape and perimeter using patern matching function
    [s;r;t;] |> List.iter(fun x -> x |> printfn "%A -> has a Perimeter of: %s." <| string (calculatePerimeter s))

    //Matching Literal identifiers : the compiler treats these like a constant pattern
    let capabilityMatch =
      function
      | NullCap    -> "isNullCap!"
      | ConsoleCap -> "isConsoleCap!"
      | WindowCap  -> "isWindowCap!"
      | _          -> "What could it be???!"

    capabilityMatch NullCap |> printfn "%s"

    //Using constant, wildcard, and variable patterns nested within tuple matching patterns
    let gps t =
      match t with
      | (0, 0, 0) -> sprintf "%A is at the origin." t
      | (0, _, _) -> sprintf "%A is on the x-axis." t
      | (_, 0, _) -> sprintf "%A is on the y-axis." t
      | (_, _, 0) -> sprintf "%A is on the z-axis." t
      | (x, y, z) -> sprintf "Point is at (%i, %i, %i)" x y z

    //test points
    let onX  = (0,1,2)
    let onY  = (1,0,2)
    let onZ  = (1,2,0)
    let orig = (0,0,0)
    let rand = (9,5,2)

    let pointList = [rand;onY;onX;onZ;orig]
    
    pointList |> List.map(gps) |> List.iter(printfn "%s")

    //Record patterns can similarly be used to break down constituent parts of Record types
    //...

    //Can use Cons patterns to decompose a list and match particular elements
    let findOrigin points =
      let rec find len l = 
        match l with
        | []                -> sprintf "Not found"
        | (0, 0, 0) :: tail -> sprintf "Origin is the %i element of the list" len
        | _ :: tail         -> find (len + 1) tail
      find 0 points

    findOrigin pointList |> printfn "%s"

    //Type annotated pattern helps compiler determine type of implicit parameter
    //Example also uses conjunctive and disjunctive pattern matching, guards,
    //...constant patterns, and a wildard pattern
    let startsWithVowel =
      function
      | (s:string) when s.Length > 0 && 
            ( System.Char.ToUpper s.[0] = 'A'
           || System.Char.ToUpper s.[0] = 'E'
           || System.Char.ToUpper s.[0] = 'I'
           || System.Char.ToUpper s.[0] = 'O'
           || System.Char.ToUpper s.[0] = 'U' ) -> true
      | _                                       -> false

    //Highly nested pattern matching is possible
    ["Awesome!"; "Not so awesome."] |> List.map(fun x -> x, (startsWithVowel x)) 
                                    |> List.iter(function
                                                 | (s, true)   -> printfn "%s DOES start with a vowel" s
                                                 | (s, false)  -> printfn "%s Does NOT start with a vowel" s)
    
    //"As" patterns allow us to bind a name to a value and to each of its constituent parts simultaneously
    let x, y, z as point = rand //(9,5,2)
    printfn "%A = (%i, %i, %i)" point x y z

    //Active patterns allow you to define your own mattern matching expressions when the built in types aren't enough

    //Here is an active recognizer that defines specific cases and a parameter to check
    //multiples of 3 return case Fizz, multiples of 5 return case Buzz, and multiples of 3 and 5 return case FizzBuzz
    let (|Fizz|Buzz|FizzBuzz|Other|) n =
      match (n % 3, n % 5) with
      | (0, 0) -> FizzBuzz
      | (0, _) -> Fizz
      | (_, 0) -> Buzz
      | _      -> Other n

    //a function to convert the identified case into a string so it can be output
    let fizzBuzz =
      function
      | Fizz -> "Fizz"
      | Buzz -> "Buzz"
      | FizzBuzz -> "FizzBuzz"
      | Other n -> string n

    //test FizzBuzz solution by outputting identified string(s) to console for first 100 integers
    seq {1..100} |> Seq.map(fizzBuzz) |> Seq.iter(printfn "%s")

    0 // return an integer exit code
