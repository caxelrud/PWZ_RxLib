//PWZ_RXLib
//RXLib_test_1.fsx

#I @"C:\Project(Comp)\Dev_2016\PWZ_RxLib_2016"
#load "RXLib_lib_1.fsx"

//System libraries
open System

//FSharp.Control.Reactive libraries.
open FSharp.Control.Reactive

//RXLib
open RXLib

//------------------------------------------------------------------------
//Test 1
let o1a=Observable.range 10 15 
let o1b=Observable.liveCount o1a
let o1c=Observable.liveSum o1a
o1a |>Observable.subscribe (fun i-> printfn "o1a-->%A" i)  
o1b |>Observable.subscribe (fun i-> printfn "o1b-->%A" i)  
o1c |>Observable.subscribe (fun i-> printfn "o1c-->%A" i)  

//Test 2
let t2=Observable.interval (TimeSpan.FromSeconds(3.0))
let o2a=t2 |> Observable.take(10) 
let o2b=Observable.liveCount o2a
let o2c=Observable.liveLongMin o2a
let o2d=Observable.liveLongMax o2a
let o2e=Observable.liveLongSum o2a
let o2f=Observable.liveLongAverage o2a
let o2g=Observable.liveLongRange o2a

let o2ad=Observable.map (fun x-> double x) o2a
let o2h=Observable.filter1stOrd Double.NaN o2ad
let o2i=Observable.addNoiseTransf (double 1.0) (double 0.5) o2ad 

o2a |>Observable.subscribe (fun i-> printfn "o2a-->%A" i)  
o2b |>Observable.subscribe (fun i-> printfn "Count(o2b)-->%A" i)  
o2c |>Observable.subscribe (fun i-> printfn "Min(o2c)-->%A" i)  
o2d |>Observable.subscribe (fun i-> printfn "Max(o2d)-->%A" i)  
o2e |>Observable.subscribe (fun i-> printfn "Sum(o2e)-->%A" i)  
o2f |>Observable.subscribe (fun i-> printfn "Avg(o2f)-->%A" i)  
o2f |>Observable.subscribe (fun i-> printfn "Range(o2g)-->%A" i)  
o2h |>Observable.subscribe (fun i-> printfn "Filter(o2h)-->%A" i)  
o2i |>Observable.subscribe (fun i-> printfn "Noise(o2i)-->%A" i)  

