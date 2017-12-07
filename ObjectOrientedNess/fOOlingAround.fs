//Michael Olivas
//The following examples were inspired by those provided in Chapter 4 of The book of F# "Staying Objective"
//I was simply fOOling around with OO (Object Oriented) princinples in f#, trying to become comfortable with the language.
module fOO

open System
//open Microsoft.FSharp.Math

//helper function to determine floating point value is within a threshold
//this was taken from the book of F# by Dave Fancher
let approxEqual (x:float) (y:float) (threshold:float) =
  Math.Abs(x-y) <= Math.Abs(threshold)

//All classes are declared with the "type" keyword
//primary constructor w/three arguments & 3 read only properties
type Idea (name : string, desc: string, worth: int) =
  let inception : System.DateTime = System.DateTime.Now    //fields created with let binding are private to class; must be initialized in primary constructor
  member x.Inception                                       //explicit property for access control to field backer
    with get() = inception                                 //Read only (no setter defined)
  member x.Name = name                                     //properties automatically have fields available; no need to explicitly map them
  member x.Desc = desc
  member x.Worth = worth

//create an instance of the Idea class with primary constructor
let bright = Idea("vague", "A semblence of a notion", 3)
printfn "I had a %s idea - %s on %A" bright.Name bright.Desc bright.Inception

//bright.Inception <- (System.DateTime.Now).AddDays(1.)     //private field is read only, cannot be set
//printfn "Now my idea is from %A" bright.Inception

//Explicitly defined field don't have to be immediately initialized but in
//classes with primary constructors they must be decorated with DefaultValue attribute
type Grade() =
  [<DefaultValue>] val mutable mark : float                  //decorator allows field to be set to appropriate "zero" value
  member (*private*) x.Mark                                  //property allows control over private field
    with get() = x.mark
    and set(v) = x.mark <-v                                  //value can be updated (mutable) with setter
  //member x.Mark with get() = x.mark                        //Alternative syntax
  //member x.Mark with internal set(v) = x.mark <- v         //properties public by default (can specify modifier: public, private, internal)

let final = Grade()
printfn "Final Grade: %.2f" final.Mark                       //if you uncomment the private access specifier above, the compiler will complain
final.mark <- 100.
printfn "Final Grade: %.2f" final.Mark

//Implicit properties must appear before other member definitions; compiler generates backing store.
//Their accessibility can only be changed at the property level.
type caffein() =
  member val Type = "Rockstar" with get, set                //omit setter accessor for read only

//Cup class uses optional parameter in primaryy constructor and self identifier
type Cup (kind: string, ?capacity: float) as this =
  let typeChangedEvent  = Event<_>()                                     //event object member
  let mutable bevType   = kind                                           //backing field
  let mutable remainder = defaultArg capacity 100.                       //syntactic sugar for Option keyword [i.e. rather than Some(capacity) or None]
  do printfn "Creating a %s cup that is %.2f percent full." kind remainder //do binding in primary constructor will output string upon construction
  member x.TypeChanged  = typeChangedEvent.Publish                       //expose event's publish property
  member x.Remainder                                                     //public property type infered
    with get() = remainder
    and set(v) = remainder <-v
  member x.Kind 
    with get() = bevType
    and internal set(v) = bevType <-v
  new (kind)   = Cup(kind)                                              //chained alternative constructors begin with "new" keyword
  new ()       = Cup("Coffee")                                          //default constructor without parameters (Of course its coffee, by default!)
  member private x.drinkPortion(percent)  =                             //private instance method operates on property to (re)set field
    x.Remainder <- if (x.Remainder - percent) >= 0.0001 then
                     x.Remainder - percent
                   else
                     0.
  member private x.refillPortion(percent) = 
    x.Remainder <- if (x.Remainder + percent) <= 100. then
                     x.Remainder + percent
                   else
                     100.
  member x.alter(bev, amnt)    =                                        //public instance method calls private methods and displays results
    x.drinkPortion(this.Remainder) |> ignore                            //forward pipe returned value to ignore to avoid compile error
    x.refillPortion(amnt) |> ignore                                     //compiler expects you to do something with all returned values
    x.Kind <- bev                                                       //utilize setter accessor method
    typeChangedEvent.Trigger (x, bev)                                   //trigger event
    printfn "This %s cup is NOW %.2f percent full!" x.Kind x.Remainder  //display results of change
  member x.alter(bev) =                                                 //Overloaded method (unnecessarily contrived for demo purposes)
    x.Kind <- bev
    printfn "Now its a cup for %s." x.Kind
    typeChangedEvent.Trigger (x, bev)                   //trigger event


let thermos = Cup()                                     //Uses all chained constructors and default property; outputs state of thermos
let beerHandler, handler =                              //declares two event handlers to watch cup class
  thermos.TypeChanged                                   //pipe forward the publish property into partition function
  |> Event.partition
       (fun ea ->                                       //lambda expression 2nd argument of partition function
          let b = thermos.Kind
          b.Equals("Beer", StringComparison.InvariantCultureIgnoreCase)) //ignore case (Returns string comparitor); order strings for partition
beerHandler.Add (fun _ -> printfn "It's time for Beer!")                 //Beer:30
handler.Add (fun _ -> printfn "Cup type changed.")                       //<music> STOP! Sober-time </music>

//Test Cup class
thermos.alter(amnt=50., bev="Hot chocolate")            //utilizes "Named Arguments" to pass parameters out of order to instance method
thermos.alter("Beer")                                   //test overloaded alter method

[<AbstractClass>]
type Shape(name) = 
  [<DefaultValue>] val mutable shapeType : string
  member x.ShapeType = name
  abstract member GetArea : unit -> float
  override x.ToString() = sprintf "This %s has an area of %.2f" (x.ShapeType) (x.GetArea())

type Square(side : float) =
  inherit Shape("Square")
  let length = side
  override x.GetArea() = side ** 2.
  //override x.ToString() = sprintf "This %s has an area of %.2f" (base.ToString()) (x.GetArea())

type Triangle(bs, hgt) =
  inherit Shape("Triangle")
  member val baseLen = bs
  member val height  = hgt
  override x.GetArea() = 0.5 * x.baseLen * x.height
  //override x.ToString() = sprintf "This %s has an area of %.2f" (base.ToString()) (x.GetArea())

let sqr = Square(5.)
printf "%s" (sqr.ToString())

let tri = Triangle(4., 10.)
printf "%s" (tri.ToString())