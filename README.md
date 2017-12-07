# PracticeFSharp
A beginners exploration into functional programming with F# using "The Book Of F# by Dave Fancher

The enclosed examples and exercises are my initial stumblings through the wonderful world of functional programming.
Please note: I do not claim originallity to most of the content here. Some examples I created, some were inspired
by similar ones in the book, and others were taken directly. This was just a safe place to try out a myriad of examples
and see how/if/when they work and what I can do to play with stuff.

I hope to take this research further and build upon these basics in the near future. I'm already thinking about a
mobile app... when time permits. The code is carefully commented throughout to give a sense of the intent and the
basic order of progression for this journey was as follows:

1.) BindingsScriptAndRpnCalc - learning about basic binding of values to identifiers, my first F# script, and
                               my first F# program - RpnCalc a reverse polish notation calcuator
2.) FiddlingWithFundamentals - learning some fundamentals: various kinds of commenting styles, type inference,
                               binding functions to identifiers, pattern matching, option, and exception handling.
3.) ObjectOrientedNess       - learning about F#s support for the OOParadigm: classes, fields, properties, methods,
                               inheritance (abstract classes virtual members etc.), static (fields, properties, methods) etc.
4.) FunctionalFlexAndCollections - learning about basic Functional programming design/techniques: functions as data,
                               currying, pipelining (foward, backward), function composition, recursion, and "functional"
                               data types like Tuples, Records, and Discriminated Unions - some playing with collections.
5.) PlayingWithPatthernMathching - learning the MANY ways F#'s pattern matching engine can serve me: match expressions,
                               guard clauses, identifier patterns, unions, tuple patterns, matching records, collections,
                               types, logical, active, partial patters... etc.
6.) InQueryingMindsWantToKnow - learning about Query expressions; basic, filtering, accessing data, sorting, grouping,
                               aggregating, joining multiple sources, and detecting items.
7.) Parallell Programming     - An introduction to the Task Parallel Library (TPL) - using Parallel.For / Parallel.ForEach
                                in different ways, and creating "Tasks", leveraging the available thread pool.
8.) ATasteOfSync              - A few examples of creating/manipulating asynchronous workflows using F#'s Asynch expressions
                                (which are really computational expressions).
