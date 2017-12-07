(* The functionality for MyFirstScript is taken from https://docs.microsoft.com/en-us/dotnet/fsharp/tutorials/fsharp-interactive/
 * as a demonstration of how to read command line arguments in a script using a couple directives in F# Interactive. 
 * #r directive allows you to reference assembly from a separate (compiled) module... in this case, RpnCalc.dll 
 * #t ["on" | "off"] turns on or off the performance evaluation to output real time, CPU time, and number of garbage collection ops.
 * First compile the F# source code for RpnCalc.fs at the commandline with "fsc -a RpnCalc.fs".
 * Then, you can execute this script at the commandline with "fsi --exec MyFirstScript.fsx" with one or more arguments & examine the output. *)

#r "RpnCalc.dll"
#time "on"

printfn "Command line arguments: "

//output all arguments from the commandline
for arg in fsi.CommandLineArgs do
    printfn "%s" arg

//ouptut the results of the evalRpnExpr (the evaluation of 5 x 10)
printfn "%A" (TheBookOfFSharp.RpnCalc.evalRpnExpr "5 10 *")

#time "off"