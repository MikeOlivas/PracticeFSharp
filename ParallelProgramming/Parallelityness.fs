(* The following exercises are mostly taken from Chapter 11 of "The Book Of F#" by Dave Fancher.
 * as I try and wrap my head around parallel programming. Tinking with the Task Parallel Library functions etc.
 * Michael Olivas
 *)

open System
open System.Threading.Tasks          //for Parallel.For
open System.Collections.Concurrent   //for ConcurrentBag

//Helper function to output the state of a ParalellLoopResult
let loopResult (r:ParallelLoopResult) = 
    if r.IsCompleted 
        then printfn "Loop completed!"
    else if (r.LowestBreakIteration).HasValue
             then printfn "Break ended loop on iteration %i!" r.LowestBreakIteration.Value
         else printfn "Stop was used to end execution!"

[<EntryPoint>]
let main argv =
(* This first section includes examples of DATA PARALLELISM - performing some action against each value
 * in a sequence by distributing the work over many threads in parallel. 
 *)
    //The parallel class in Threading.Tasks namespace exposes Parallel.For and Parallel.ForEach functions
    //Work like for loop except uses threads from the threadpool to perform iterations in parallel
    //(For takes a range and a function, ForEach takes a sequence to iterate over and a function)
    printfn "Using Parallel.For or ForEach is problematic:"
    Parallel.ForEach([1..20], printf "%i ") 
      |> ignore


    //In above, multiple threads could be sending output to the console simultaneously
    //Could choose to lock the console while in use... but this negates any benefit gained from running in parallel
    printfn "\n\nCould use a lock, but that defeats the purpose:"
    Parallel.For(
        0,                                                      //inclusive start of range
        20,                                                     //exclusive end of range
        fun x -> lock Console.Out (fun () -> printf "%i " x)    //loop body
    ) |> ignore


    //Simple solution: use sprintf and function composition to avoid sharing the console
    //each call to sprintf uses an isolated StringBuilder - not a shared resource.
    //Once the final string is built, we output it to the console
    printfn "\n\nQuick solution: Use non-shared resource for parallelization:"
    Parallel.For(0, 20, (sprintf "%i ") >> Console.Write)
      |> ignore

    //Function takes a ParallelLoopState argument to determine how to shortCircuit the Parallel.For loop
    //...when 10,000th loop iteration is met
    let shortCircuitExample shortCircuit =
        let bag = ConcurrentBag<_>()
        //Use overload of Parallel.For where the third argument is of type Action<Int32, ParallelLoopState>
        Parallel.For(
            0,
            99999,
            fun i s -> if i < 10000 then bag.Add i else shortCircuit s
        ) |> loopResult //Try to debug using ParallelLoopResultStructure returned by overloaded Parallel.For
                        //(Behavior is odd depending on if we call with Break or Stop and in which order)
        (bag, bag.Count)
    
    //Trying to use shortCircuitExample to pre-empt our parallel for loop with Break and Stop
    printfn "\nTrying to use our shortCircuitExample:"
    try
        //Break causes the loop to terminate AFTER all threads complete up to the current iteration
        shortCircuitExample (fun s -> s.Break()) 
          |> printfn "%A\n"
        //Stop allows threads already executing to continue but otherwise stops at earliest convenience
        //(typically results in less actions than break)
        shortCircuitExample (fun s -> s.Stop())  
          |> printfn "%A\n"
    with
        | :? System.InvalidOperationException -> printfn "Can't call Stop and Break in sequence on same iteration!"
        | _ as ex                             -> printfn "Error: %s" (ex.Message)
    
    //Checking how many threads are available
    let wrkr, completion =  ref 0, ref 0
    System.Threading.ThreadPool.GetMaxThreads (wrkr, completion)
    printfn "Max Threads: %i" (!wrkr + !completion)

    
    //Function outputs an integer to console for "wait" miliseconds.
    //When the CancellationToken's timer expires, an exception is thrown
    //...which we catch and handle appropriately
    let parallelForWithCancellation (wait : int) =
        //Create a new instance of the CancellationTokenSource class
        //...that will be cancelled after a "wait" millisecond delay
        use tokenSource = new System.Threading.CancellationTokenSource(wait)

        try //Overload of Parallel.For that takes an instance of ParallelOptions 
            Parallel.For(
                0,                                                      //inclusive start of range
                Int32.MaxValue,                                         //exclusive end of range
                ParallelOptions(CancellationToken = tokenSource.Token), //Set CancellationToken property
                fun (i : int) -> Console.WriteLine i                   //Body of loop
            ) |> ignore
        with
            | :? OperationCanceledException -> printfn "Parallel Operation Cancelled!"
            | ex -> printfn "%O" ex
    
    //invoke with 1 millisecond delay
    parallelForWithCancellation 1
    (* Output explains our odd behavior in shortCircuitExample above. Algorithm can divide problem in a
     * ...multitude of ways. So, it was reaching the shortCircuit value earlier than we'd have expected.
     * "The only guarantee is that all of the loop's iterations will have run by the time the loop finishes."
     * - https://msdn.microsoft.com/en-us/library/ff963552.aspx *)

(* The following sections includes examples of Task parallelism - we manually create and manage independent tasks
 * which can be executed concurrently by the available threads.
 *)
    //Invoke allows us to specify a sequence of actions to be created and started
    //Here we create/start three tasks in parallel; the second is delayed by 100 ms
    //...and so, its output follows that of Task 3 - which is concurrently running
    //This method is very limited: doesn't expose any information about the individual tasks (did they succeed?).
    printfn "\n\nUsing \"Invoke\" to start an array of tasks"
    Parallel.Invoke(
        (fun () -> printfn "\tTask1"),
        (fun () -> Task.Delay(100).Wait()
                   printfn "\tTask2"),
        (fun () -> printfn "\tTask3")
    )

    //For greater control, we create tasks manually and start them with "new"
    printfn "\nGenerating manual tasks:"
    let t1 = new Task(fun () -> printfn "\tFirst Manual Task.")
    t1.Start()
    t1.Wait()

    //The Task class has a static default Task Factory.
    //We can use its StartNew method to combine the create and start of a task
    let t2 = Task.Factory.StartNew(fun () -> printfn "\tFactory Task.")
    t2.Wait()

    //Task constructor and StartNew provide overloads that allow for use of Task<'T>
    //The return type is inferred and we can access it through the Result property.
    let t3 = Task.Factory.StartNew(fun () -> System.Random().Next())
    t3.Result |> printfn "\tAccessed a returned task \"Result\": %i"


    //The following demonstrates the use of a continuation in F#
    //We generate an "antecdent" task which sleeps to simulate some long running process
    //...and returns a value of type string.
    printfn "\nContinueWith creates a continuation on a single task"
    let antecedent =
        new Task<string>(
          fun () ->
            Console.WriteLine("Started antecedent")
            System.Threading.Thread.Sleep(1000)
            Console.WriteLine("Completed antecedent")
            "Job's done"
        )

    //...then we use the ContinueWith method to create a continuation of that task
    //...outputing its status and result to the console.
    let continuation1 =
        antecedent.ContinueWith(
          fun (a : Task<string>) ->
            Console.WriteLine("Started continuation")
            Console.WriteLine("Antecedent status: {0}", a.Status)
            Console.WriteLine("Antecedent result: {0}", a.Result)
            Console.WriteLine("Completed continuation")
        )
    antecedent.Start()                                     //start the task
    Console.WriteLine("Waiting for continuation")          //This execution should complete before antecedent's output 
    continuation1.Wait()                                   //Wait for all output of task before continuing
    Console.WriteLine("Done")

    //The TaskFactory has a ContinueWhenAny and ContinueWhenAll method to allow for continuations on any/all tasks in an array
    printfn "\nUsing ContinueWhenAll based on an array of tasks"
    let antecedents =
      [|
        new Task(
          fun () ->
            Console.WriteLine("Started first antecedent")
            System.Threading.Thread.Sleep(1000)
            Console.WriteLine("Completed first antecdent"))
        new Task(
          fun () ->
            Console.WriteLine("Started second antecedent")
            System.Threading.Thread.Sleep(1250)
            Console.WriteLine("Completed second antecdent"))
        new Task(
          fun () ->
            Console.WriteLine("Started third antecedent")
            System.Threading.Thread.Sleep(1000)
            Console.WriteLine("Completed third antecdent"))
      |]

    //continuation will execute after ALL tasks in antecedents array complete
    let continuation2 = Task.Factory.ContinueWhenAll(
                          antecedents,                                                 //array of Task
                          fun (a: Task array) ->                                       //action for continuation
                            Console.WriteLine("Started continuation after tasks...")
                            for x in a do Console.WriteLine("Antecedent status: {0}", x.Status)
                            Console.WriteLine("...completed continuation after tasks")
                        )
    for a in antecedents do a.Start()
    
    Console.WriteLine("Waiting on array continuation.")
    continuation2.Wait()
    Console.WriteLine("Array continuation done.")
    0 // return an integer exit code
