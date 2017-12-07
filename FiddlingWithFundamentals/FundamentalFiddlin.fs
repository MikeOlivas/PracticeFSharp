///<summary>
///a sandbox for trying out examples from chapter 3 of "The Book of F#" and tweeking them
///to practice/learn about the F# language. Here we learn about type inference, three kinds
///of comments available, binding functions to identifiers, some pattern matching, Option,
///exception handling, and how to open a couple different file types.
///Michael Olivas
///</summary> 
open System
open System.Drawing           //for Image abstract class which implements IDisposable
open System.Diagnostics       //for interacting with system processes, event logs, & performance counters
open System.IO                //for FileNotFoundException (and other file I/O)

//Type safe language, but rarely need to specify the type
//Has an incredible type inference system
let square x = x * x
let squared = List.map square [1;2;3;]      //map function on List generic maps list of numbers to a new list of squares

let add x y = x + y                         //define a function to add two integers (type inferred)
let result = add 4 5                        //use add function and bind result to an identifier
let add4   = add 4                          //use add function to create new partially applied function via currying
let newResult = add4 5                      //use new partially applied function to add four to any integer
printf "result: %d newResult: %d" result newResult  //values should match

[<EntryPoint>]
let main argv = 
    let verbatimFileName = @"C:\Users\Michael\Pictures\Saved Pictures\MikeAndShelly.jpg" //immutable verbatim string ignores escape sequences
    try     //try...with construct for handling exceptions; try block evaluated and with block executed if exception is raised
      (* "using" function takes a resource that implements the IDisposable interface and a function that
       * accepts the instance. Automatically calls Dispose to clean up resources after completion of function *)
        let w, h, s = using (Image.FromFile(verbatimFileName))
                         (fun img -> (img.Width, img.Height, img.Size)) //anonymous function returns with, height, and size {w, h}
        printfn "Dimensions: %i x %i \nSize:       %A pixels" w h s     //%i==integer; %A prints any value with F# default layout settings
    with                                                                //F# pattern matching used to locate appropriate
        | :? FileNotFoundException as e ->                              //bind matched exception to name to access members
          printfn "%s was not found" e.FileName                         //output formatted filename member
          //reraise()                                                   //preserve stack trace embedded in exception and send up a level
        | _ -> printfn "Error loading file."
               //raise()                         //raise an exception next level up but don't preserve stack


    //F# lets you enclose text with double-back-ticks to use virtually any string as valid identifier
    let ``FileName that doesn't exist`` = @"C:\Users\Michael\DoesntExist.txt"
    
    //Try to open a file and bind the result to a named option
    let fileContents =
      try //try to use a stream reader to a particular file name
          use reader = File.OpenText ``FileName that doesn't exist``
          Some <| reader.ReadToEnd()                                    //if successful, store contents in Option's value
      with
        | :? FileNotFoundException as e ->                              //if file not found, print error and return None to Option
          printfn "%s was not found" e.FileName
          None
        | _ ->                                                          //Unrecognized exception, pass up to client for decision
          printfn "Error loading file"
          reraise()
    
    //Here we can check the results of our attempt to open file (that doesn't exist)
    //If it did exist we'd print its contents to the console, else simply "No value!"
    if fileContents.IsNone
      then printfn "No value!"
    else
      printfn "Value: %s" fileContents.Value

    0 // return an integer exit code