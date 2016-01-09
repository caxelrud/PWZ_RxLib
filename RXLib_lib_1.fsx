//PWZ_RXLib
//RXLib_Test_1
//Define references to the libraries.
//For Reactive Extensions.
(*
Ideas and code from "Real-time statistics with Rx" by John L. Vidal,  10 Jan 2015 
http://www.codeproject.com/Tips/853256/Real-time-statistics-with-Rx-Statistical-Demo-App

*)



namespace RXLib

#I @"C:\Project(Comp)\Dev_2016\PWZ_RxLib_2016"
#r @".\libs\System.Reactive.Core.dll"
#r @".\libs\System.Reactive.Interfaces.dll"
#r @".\libs\System.Reactive.Linq.dll"
#r @".\libs\System.Reactive.PlatformServices.dll"
#r @".\libs\System.Reactive.Providers.dll"
#r @".\libs\Microsoft.Reactive.Testing.dll"

//For FSharp.Control.Reactive.
#r @".\libs\FSharp.Control.Reactive.dll" //Need to be the last reference

//Open the libraries.
open System
open System.Collections.Generic
open System.Threading
open System.Net

//Open Reactive Extensions libraries.
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Disposables
open System.Reactive.Concurrency
open System.Reactive.Subjects
open Microsoft.Reactive.Testing
 
//Open FSharp.Control.Reactive libraries.

open FSharp.Control.Reactive
open FSharp.Control.Reactive.Builders
open FSharp.Control.Reactive.Observable



//Add functions to the Observable module
module Observable=
        //Filter
        let filter1stOrd (lambda:double) (source:IObservable<double>)=
            Observable.scan
                        (fun (state:double) (value:double)->
                                    if Double.IsNaN(state) then 
                                        value
                                    else
                                        state*lambda + value*(1.0-lambda)
                         ) source
        //Create an Random Observable
        let randObs (timeInterv:double)=
            let rand= System.Random()
            Observable.interval (TimeSpan.FromMilliseconds(timeInterv))
                          |> Observable.map (fun _ -> rand.NextDouble())
                          //|> Observable.map (fun value -> (double value) + rand.NextDouble())
        //Add noise to an Observable    
        let addNoiseTransf (amp:double) (tauFilter:double) (obsIn: IObservable<double>)=
            let rand= System.Random()
            let obs1=Observable.map (fun _->amp*rand.NextDouble()) obsIn
            let obs2=filter1stOrd tauFilter obs1 
            Observable.zipWith (fun a b-> ((double a)+(double b))) obsIn obs2

        let scan (accumulator:'a->'a->'a) (seed:'a)  source =
            Observable.Scan(source,seed, Func<'a,'a,'a> accumulator  )

        let liveCount<'T> (source: IObservable<'T>)=
            Observable.Select<'T,int>(source,Func<'T,int,int>(fun a b->b+1))

        let liveLongCount<'T> (source: IObservable<'T>)=
            Observable.Scan<'T,int64>(source,0L,Func<int64,'T,int64>(fun a b->a+1L))

        let liveMin (source: IObservable<int>)=
            Observable.Scan<int,int>(source,Int32.MaxValue,Func<int,int,int>(fun a b->Math.Min(a,b)))
        let liveLongMin (source: IObservable<int64>)=
            Observable.Scan<int64,int64>(source,Int64.MaxValue,Func<int64,int64,int64>(fun a b->Math.Min(a,b)))
        let liveDoubleMin (source: IObservable<double>)=
            Observable.Scan<double,double>(source,Double.MaxValue,Func<double,double,double>(fun a b->Math.Min(a,b)))
        let liveDecMin (source: IObservable<decimal>)=
            Observable.Scan<decimal,decimal>(source,Decimal.MaxValue,Func<decimal,decimal,decimal>(fun a b->Math.Min(a,b)))

        let liveMax (source: IObservable<int>)=
            Observable.Scan<int,int>(source,Int32.MinValue,Func<int,int,int>(fun a b->Math.Max(a,b)))
        let liveLongMax (source: IObservable<int64>)=
            Observable.Scan<int64,int64>(source,Int64.MinValue,Func<int64,int64,int64>(fun a b->Math.Max(a,b)))
        let liveDoubleMax (source: IObservable<double>)=
            Observable.Scan<double,double>(source,Double.MinValue,Func<double,double,double>(fun a b->Math.Max(a,b)))
        let liveDecMax (source: IObservable<decimal>)=
            Observable.Scan<decimal,decimal>(source,Decimal.MinValue,Func<decimal,decimal,decimal>(fun a b->Math.Max(a,b)))

        //let LiveSum<'T> (source: IObservable<'T>)=
        //    Observable.Scan<'T,'T>(source,0,Func<'T,'T,'T>(fun a b->a+b))

        let liveSum (source: IObservable<int>)=
            Observable.Scan<int,int>(source,0,Func<int,int,int>(fun a b->a+b))
        let liveLongSum (source: IObservable<int64>)=
            Observable.Scan<int64,int64>(source,0L,Func<int64,int64,int64>(fun a b->a+b))
        let liveDecSum (source: IObservable<decimal>)=
            Observable.Scan<decimal,decimal>(source,0M,Func<decimal,decimal,decimal>(fun a b->a+b))
        let liveDoubleSum (source: IObservable<double>)=
            Observable.Scan<double,double>(source,0.0,Func<double,double,double>(fun a b->a+b))
        (*        
        static member LiveSum<'T> (func:'T->int) (source:IObservable<'T>) =
            Observable.Scan<'T,int>(source,0,Func<int,'T,int>(fun a b->a+func(b)))
        static member LiveSum<'T> (func:'T->double) (source:IObservable<'T>)  =
            Observable.Scan<'T,double>(source,0.0,Func<double,'T,double>(fun a b->a+func(b)))
        static member LiveSum<'T> (func:'T->decimal) (source:IObservable<'T>) =
            Observable.Scan<'T,decimal>(source,0M,Func<decimal,'T,decimal>(fun a b->a+func(b)))
        *)

     
        let liveAverage (source:IObservable<int>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> liveCount) (source |> liveSum)

        let liveLongAverage (source:IObservable<int64>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> liveCount) (source |> liveLongSum)
            
        let liveDecAverage (source:IObservable<decimal>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> liveCount) (source |> liveDecSum)

        let liveDoubleAverage (source:IObservable<double>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> liveCount) (source |> liveDoubleSum)
        //Range-----------    
        let liveRange (source:IObservable<int>)=
            Observable.zipWith (fun a b -> a - b) (source |> liveMax) (source |> liveMin)
            |> Observable.skip 1 
        let liveLongRange (source:IObservable<int64>)=
            Observable.zipWith (fun a b -> a - b) (source |> liveLongMax) (source |> liveLongMin)
            |> Observable.skip 1 

