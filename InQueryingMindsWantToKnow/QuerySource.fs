(* The following is a simple module that I spun up that contains some sequences of records
 * (resembling a relational database) for use with "QueryControl.fs". The intent is to explore
 * F# query expressions - operations, usage, syntax, composure etc.
 * Michael Olivas
 *)

module QuerySource
  open System
  
  //Discriminated union to describe the current level of a student's education
  //Assuming undergraduate for simplicity
  type ClassLvl =
    | Freshman
    | Sophmore
    | Junior
    | Senior

  //Simple record to define a student
  type Student = {id : int; name : string; lvl : ClassLvl; gpa : float}
                  override this.ToString() = sprintf "%s (%i), class: %A, gpa: %.2f" this.name this.id this.lvl this.gpa

  //Simple record to define a course
  type Course = {id : int; name : string; capacity : int; roomNum : Nullable<int>}
                 override this.ToString() = sprintf "%s-%i, capacity: %A roomNum: %i" this.name this.id this.capacity 
                                              (if this.roomNum.HasValue then this.roomNum.Value else -1)

  //Record to track relation of students to courses
  type StudentCourse = {sid : int; cid : int}

  //Collection of student records
  let students = 
    [{id = 1; name = "Mike Olivas";   lvl = Senior;   gpa = 4.0}
     {id = 2; name = "Suzy Queue";    lvl = Freshman; gpa = 3.9}
     {id = 3; name = "Cherry Garcia"; lvl = Sophmore; gpa = 2.5}
     {id = 4; name = "Bill E. Goat";  lvl = Junior;   gpa = 3.6}
     {id = 5; name = "Nunya Bidness"; lvl = Senior;   gpa = 2.4}
     {id = 6; name = "Kris Kringle";  lvl = Senior;   gpa = 4.0}]
  
  //Collection of class records
  let classes =
    [{id = 101; name = "Linear Algebra"; capacity = 10; roomNum = Nullable 215}       //has students[2;3]
     {id = 410; name = "LLP";            capacity = 20; roomNum = Nullable 322}       //has students[1;3;5]
     {id = 205; name = "Online class";   capacity = 5;  roomNum = Nullable<int>()}    //has students[1;3;4]
     {id = 322; name = "Horticulture";   capacity = 20; roomNum = Nullable 115}]      //no students

  //Collection relates enrollment of students in classes
  let studentCourses = 
    [{sid = 1; cid = 410}
     {sid = 1; cid = 205}
     {sid = 2; cid = 101}
     {sid = 2; cid = 205}
     {sid = 3; cid = 101}
     {sid = 3; cid = 410}
     {sid = 3; cid = 205}
     {sid = 4; cid = 205}
     {sid = 5; cid = 410}
     (*Kris Kringle is too busy this time of year to take classes*)]