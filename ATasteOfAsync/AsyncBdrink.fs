(* The following examples in Asynchronous programming comes from "The Book of F#". I was recreating them
 * here as I walked through the various classes, modules, functions etc. in https://msdn.microsoft.com/en-us/library/...
 * to try and understand things. Its a lot trickier than it looks.
 * Michael Olivas
 *)

open System
open System.IO
open System.Net
open System.Text.RegularExpressions
open System.Threading                   //for CancellationTokenSource

//Extend a TextReader to return an asyncronous workflow
type StreamReader with
    member x.AsyncReadToEnd () =
        async { do! Async.SwitchToNewThread()                       //Creates a new thread and runs in that thread
                let content = x.ReadToEnd()                         //Read all characters to end of stream
                do! Async.SwitchToThreadPool()                      //Queue a work item to run its continuation
                return content }                                    //Return a string of stream read in an Async<string>
                
[<EntryPoint>]
let main argv = 

    //function returns the source of webpage represented by uri
    let getPage (uri : Uri) = 
        async { let req       = WebRequest.Create uri               //create a WebRequest for a Uri scheme
                //printfn "Starting getpage %A" uri                   //testing
                use! response = req.AsyncGetResponse()              //wait for response to bind before moving on
                use stream    = response.GetResponseStream()        //get data stream from Uri
                use reader    = new StreamReader(stream)            //create instance of our extended TextReader
                return! reader.AsyncReadToEnd() }                   //return Async<string> of webspage content

    //function defined on Async class to start an Asynchronous workflow immediately then proceed with
    //one of three user-provided continuations. The parameters are explained at right below:
    Async.StartWithContinuations(
        getPage(Uri "http://google.com"),                           //Computation Type Async<'T>
        (fun c -> c.Substring(0, 50) |> printfn "%s..."),           //Continuation
        (printfn "Exception: %O"),                                  //Exception continuation
        (fun _ -> printfn "Cancelled"))                             //Cancellation continuation

(*When I run just the above, execution continues before the successful continuation completes so the substring is not output to the console.
 *If I put the main thread to sleep so it waits, (or do other work) then the results are displayed as expected. I can force a wait to bind 
 *within an async expression with a !, but not sure I can also expressly wait for a task from the TPL to complate with tas.Wait(), however 
 *I haven't yet found out how to get the continuation function(s) defined in the Async class library to wait.*)
    //System.Threading.Thread.Sleep(8000)
    
    //Lets define an array of Uris to allow for reuse
    let uriArray =
        [| Uri "http://google.com"
           Uri "http://www.habit-lang.org/"
           Uri "http://www.tryfsharp.org"
           Uri "http://fsharp.org" |]

    //Rather than use a continuation, we can use the RunSynchronously method to wait for a result.
    //This defeats the purpose for a single workflow, but we can run several workflows simultaneously
    //then pipe through the Parallel method to join into one workflow & RunSynchronously to wait for the result:
    uriArray 
      |> Array.map ( fun x -> getPage(x))                                        //Map each Uri to use getPage function
      |> Async.Parallel                                                          //Merge Asyncs into one workflow
      |> Async.RunSynchronously                                                  //Wait for all to finish
      |> Seq.iter (fun s -> let snippet = s.Substring(0, 60)                     //format/print a snipet of each result
                            Regex.Replace(snippet, @"[\r\n]| {2,}", "")
                            |> printfn "**%s...\n\n")
    

    //Here we define a function to test the "TryCancelled" method that takes two arguments:
    //An asynchronous workflow that may complete, and another that will execute if the workflow is cancelled.
    let displayPartialPage uri = 
        Async.TryCancelled(
          async {
            let! c = getPage uri
            Regex.Replace(c.Substring(0, 50), @"[\r\n]| {2,}", "")
            |> sprintf "[%O] %s...\n" uri
            |> Console.WriteLine },
          (sprintf "[%O] cancelled: %O\n" uri >> Console.WriteLine))
    
    Async.Start(displayPartialPage (Uri "http://www.habit-lang.org/"))
    //Async.CancelDefaultToken()

    //Overload of Async.Start allows us to pass in user-specified tokens.
    //So, we can assign a unique cancellation token for each asynchronous workflow in our array
    let tokens =
        uriArray |> Array.map (fun x -> let ts = new CancellationTokenSource()          //create a new cancellation token
                                        Async.Start(displayPartialPage x, ts.Token)     //start an async with Uri and cxl token
                                        ts)                                             //return token source to bind in array

    tokens.[0].Cancel()     //cancel google
    tokens.[2].Cancel()     //cancel tryfsharp
    
    System.Threading.Thread.Sleep(15000)  //give the threadpool time to complete before moving on to next exercise

    //We can use Async.Catch to handle exceptions from an asynchronous workflow
    //it returns a discriminated union with two cases "Choice1of2", if success, contains the result and
    //"Choice2of2" will contain the first raised exception, if failure.
    let success = 
      Uri "http://wiki.cs.pdx.edu/"
        |> getPage
        |> Async.Catch
        |> Async.RunSynchronously
        |> function
           | Choice1Of2 result -> Some result
           | Choice2Of2 ex     ->
             match ex with
             | :? NotSupportedException ->
               Console.WriteLine "Caught NotSupportedException"
             | :? OutOfMemoryException  ->
               Console.WriteLine "Caught OutOfMemoryException"
             | ex ->
               ex.Message |> sprintf "Exception: %s" |> Console.WriteLine
             None
    
    //check and output the result returned from Async.Catch
    match success with
      | None -> printf "Failure!"
      | x    -> x.Value.Substring(0, 50) |> printf "Success: %s..." 

    0 // return an integer exit code
