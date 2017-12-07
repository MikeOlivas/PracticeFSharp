(* The following exercises are my attempt at learning F# query expressions. This uses "QuerySource.fs"
 * to run queries on - must be in same project folder. These examples were inspired by chapter 10 of
 * Dave Fancher's "The Book of F#".
 * Michael Olivas
 *)

module QueryControl
open QuerySource
open System.Linq              //Only needed to call LINQ extension methods directly. NOT for Query expressions
open Microsoft.FSharp.Linq.NullableOperators //Needed for operators that support coimparison of nullable types

[<EntryPoint>]
let main argv = 

(* The following snippet is just for comparison. It demonstrates  *
 * calling LINQ extension methods directly to query data          *
  [1..100]
    .Where(fun x -> x % 10 = 0)
    .OrderBy(fun x -> x)
    |> Seq.iter (printfn "%i")
*)

(* Query expressions allow us to use a more SQL-like syntax   *
 * The following results in the same behavior described above *)
  printfn "Query Expressions example:"
  query { for n in [1..100] do
          where (n % 10 = 0)
          sortBy n }
    |> Seq.iter (printfn "%i")

  //select is a projection operator which can define a structure for each item (like the map function); 
  //if a structure is not specified, it will project the data item directly
  printfn "\nBasic select projection:"
  query { for s in QuerySource.students do select s } |> printfn "%A"

  //Here we use the where operator to spcify a predicate-based filter.
  //where takes a boolean expression as an argument and filters out records that don't satisfy the predicate.
  printfn "\nUsing predicate-based filter and projecting with ToString():"
  query { for s in QuerySource.students do
          where  (s.lvl = QuerySource.Senior)
          select (s.ToString()) } |> printfn "%A"         //Notice we specify a structure for the select operator with "ToString"
                                                          //This changes the way the resulting output is formatted

  //We can also transform the source sequence to project a different type
  printfn "\nTransforming source sequence by projecting into triple:"
  query { for s in QuerySource.students do
          select (s.name, s.lvl, s.gpa)} |> printfn "%A"  //Record is projected as a triple

  //Must be aware of the underlying data types when using predicate based filters
  //May have to work with Null values, which standard comparison operators caon't handle
  //Full range of Nullable Operators defined in Microsoft.FSharp.Linq.NullableOperators module
  printfn "\nFiltering nullable data types:"
  query { for c in QuerySource.classes do
          where  (c.roomNum ?>= 200) //filter out rooms below the 2nd floor
          select (c.ToString()) } |> Seq.iter (printfn "%s")

  //The distinct operator will filter out duplicates, just like in SQL
  printfn "\nUsing \"distinct\" to filter out duplicates:"
  query { for r in QuerySource.studentCourses do
          select r.cid
          distinct } |> Seq.iter (printfn "%i")
  
  //Can access individual items of a sequence
  printfn "\nAccessing individual items:"
  query { for s in QuerySource.students do head  } |> printfn "head ->\n%A"
  query { for s in QuerySource.students do last  } |> printfn "last ->\n%A"
  query { for s in QuerySource.students do nth 3 } |> printfn "nth  ->\n%A"  //Accessing nth item is zero-based

  //Find operator also returns a single item; its like using the where clause
  //except it only projects the first item which satisfies the predicate
  printfn "\nUsing the \"find\" operator to search with criteria:"
  query { for s in QuerySource.students do
          find (s.gpa = 4.0) } |> printfn "%A"

  //Can enforce a unique key constraint with "exactlyOne" operator
  //The following query expression should throw an exception. 
  //To see it succeed, change the predicate to (s.id = 1)
  printfn "\nUsing the \"exactlyOne\" operator to enforce uniqueness:"
  try
    query { for s in QuerySource.students do
            where (s.gpa = 4.0) 
            exactlyOne } |> printfn "%A"
  with                                                                          //Try...with expression from "Fundamentals" exercises
  | :? System.InvalidOperationException -> printfn "Error: There is more than one such element in the sequence!"      //Type-Test pattern
  | _                                   -> printfn "Error processing sequence." //Shouldn't strictly be needed but better safe than sorry

  //Used sortBy briefly above... but can also "sortByDescending"
  //"sortByNullable" and "sortByNullableDescending" do exactly what you'd expect also, properly handling Nullable<_> values
  printfn "\nUsing the \"sortByDescending\" operator:"
  query { for s in QuerySource.students do
          sortByDescending s.name
          select (s.ToString())} |> Seq.iter (printfn "%s")

  //You can also subsort by chaining multiple sort operators as follows.
  //Possible variations include thenBy, thenByNullable, thenBySDescending, thenByNullableDescending
  printfn "\nSpecifying subsequent sorts with \"thenBy\" operator(s):"
  query { for c in QuerySource.classes do
          sortBy c.name
          thenByNullable c.roomNum
          thenByDescending c.id
          select (c.ToString())} |> Seq.iter (printfn "%s")

  //The GroupBy operator allows you to query a collection and specify a value
  //on which to group the returned data by. It maps data to a value of Type IGrouping<key,Obj>.
  //That is, we can get a sequence of (unique_key, sequence X) objects as follows
  //where X is a sequence of all the objects that match the unique_key.
  printfn "\nUsing \"groupBy\" to output [(key, sequence)] tuples:"
  query { for s in QuerySource.students do
          groupBy s.lvl into g                          //intermediate sequence
          select (g.Key, g)} |> printfn "%A"

  //Or you can use groupValBy operator to access specific properties of the object, rather than the whole object.
  //It takes two inputs: (the desired return value, and the key)
  printfn "\n\"groupValBy\" allows you to return a value(s) rather than the whole object"
  query { for s in QuerySource.students do
          groupValBy s.name s.lvl into g
          sortBy g.Key
          select (g.Key, g) } |> printfn "%A"

  //There are also query expressions to operate on agregated data such as count - but it could get expensive.
  //Other aggregate operators not used include minBy, maxBy, sumBy, and averageBy.
  query { for s in QuerySource.students do count} |> printfn "\nThere are %i students enrolled."
  query { for x in query { for sc in QuerySource.studentCourses do
                           groupValBy sc.sid sc.sid into g
                           select (g.Key) } do count } |> printfn "But only %i are in classes."
  query { for a in QuerySource.students do
          averageBy a.gpa } |> printfn "The average gpa of enrolled students is %.2f."

  //Instead of trying to get a specific item froma sequence, we might just want to see if
  //one exists inside a collection. The "contains" operator enumerates the entire sequence to check.
  let critereon = "Mike Olivas"
  match (query { for s in QuerySource.students do
                 select s.name
                 contains critereon }) with
  | true  -> printfn "The enrollment list CONTAINS %s!" critereon
  | false -> printfn "This enrollment does NOT contain %s!" critereon

  //The "exists" operator will stop enumerating as soon as an item matches it's predicate.
  //So, this is likely to have better performance.
  if (query { for s in QuerySource.students do
              exists (s.name = critereon) }) then printfn "%s EXISTS in the student body." critereon
  else printfn "%s does not exist. He is a figment of your imagination." critereon

  //Exists will also support other predicates and allow NullableOperators
  if (query {for c in QuerySource.classes do
             exists (c.roomNum ?> 200) }) then printfn "There is at least one class above the 2nd floor."
  else printfn "There are currently NO classes offerred above the 2nd floor."

  //Query expressions allow joining sequences; we can do this by specifying a join expression
  //for each relationship on which we wish to join
  printfn "\nChaning \"Join\" expressions together to express n:n relationship produces:"
  query { for s in QuerySource.students do
          join sc in QuerySource.studentCourses on (s.id = sc.sid)
          join c in QuerySource.classes on (sc.cid = c.id)
          select (s.name, s.id, c.name, c.id) } 
          |> Seq.iter (fun (a, b, c, d) -> printfn "%s (%i) is enrolled in %s (%i)" a b c d)

  //Helper function to create a formatted string from a sequence of (string, int) tuples
  //of the form "Name_0 (id_0), Name_1 (id_1), ..., Name_n (id_n)"
  let getSeqStr (top:seq<string*int>) =
    let nextHd, nextTl = Seq.tail top |> (fun x      -> Seq.head x, Seq.tail x)  //extract next arguments for first recursive call
    let topStr         = Seq.head top |> (fun (x, y) -> sprintf "%s (%i)" x y)   //form first str element from top level argument
    let rec getStr hd tl str =                                                                 //define inner recursion
      if Seq.isEmpty tl then str                                                               //if last element, append empty string
      else getStr (Seq.head tl) (Seq.tail tl) (sprintf "%s, %s (%i)" str (fst hd) (snd hd))    //else append next string element
    getStr nextHd nextTl topStr                                                                //begin recursion

  //groupJoin allows you to join two sequences but project the result into a new sequence to
  //be used in a nested select clause
  printfn "\nCreate intermediate sequence with groupJoin for use in nested query expression:"
  query { for c in QuerySource.classes do
          groupJoin sc in QuerySource.studentCourses on (c.id = sc.cid) into intermediate
          select (c.name, query { for i in intermediate do
                                  join s in QuerySource.students on (i.sid = s.id)
                                  select (s.name, s.id)})}
  |> Seq.iter (fun (a, x) -> if Seq.isEmpty x then printfn "\"%s\" has no students." a
                             else a |> printfn "The following students are in \"%s\":\n\t%s" <| getSeqStr x)
(**)
  0 // return an integer exit code