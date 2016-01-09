(**
# HIGH PERFORMANCE REAL-TIME STOCKS COMPUTATION ENGINE - part 1

This blog explores a *High Performance Real-Time Stocks
Computation Engine* for data collection and processing.
It can be the basis of a *Real-time Trading System*.

High Performance is obtained by using *Reactive* and 
*Non-blocking* code.

The code has the following properties:

-   Developed for **F# REPL** script.

-   Uses *.NET Reactive Extensions* and *FSharp.Control.Reactive* 
for the **PUSH** pattern style coding.

-   Uses *F# Asynchonous workflow* for **PULL** pattern style coding.

-   Collects Stock data from "https://code.google.com/p/yahoo-finance-managed/wiki/YahooFinanceAPIs"

As an overall pattern, the **PUSH** pattern is the preferred style,
eventhough, the example, presented here, starts by, periodicly, 
pull data from the web.

The main reason is related to the fact that the communication with 
other processes/computers is expected to use *Publish/Subscribe*, 
and this is a **PUSH** pattern.

*)

(**
##Example steps

###Define references to the libraries.

For Reactive Extensions.
*)
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\System.Reactive.Core.dll"
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\System.Reactive.Interfaces.dll"
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\System.Reactive.Linq.dll"
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\System.Reactive.PlatformServices.dll"
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\System.Reactive.Providers.dll"
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\Microsoft.Reactive.Testing.dll"
(**
For FSharp.Control.Reactive.
*)
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\FSharp.Control.Reactive.dll" //Need to be the last reference
(**
Open the libraries.
*)
open System
open System.Collections.Generic
open System.Threading
open System.Net
(**
Open Reactive Extensions libraries.
*)
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Disposables
open System.Reactive.Concurrency
open System.Reactive.Subjects
open Microsoft.Reactive.Testing
(** 
Open FSharp.Control.Reactive libraries.
*)
open FSharp.Control.Reactive
open FSharp.Control.Reactive.Builders
open FSharp.Control.Reactive.Observable


module StatHelper=
    let randObs (timeInterv:double)=
        let rand= System.Random()
        Observable.interval (TimeSpan.FromMilliseconds(timeInterv))
                      |> Observable.map (fun value -> (double value) + rand.NextDouble())
        
    let noiseRandObs (a:double) (b:double) obsIn=
        let rand= System.Random()
        Observable.map (fun x->x+a*(rand.NextDouble()+b)) obsIn

type Observable with
        //Main Library Extensions 
        /// Applies an accumulator function over an observable sequence and returns each intermediate result.
        static member scan (accumulator:'a->'a->'a) (seed:'a)  source =
            Observable.Scan(source,seed, Func<'a,'a,'a> accumulator  )


type Observable with
        //Main Library Extensions 
        /// Applies an accumulator function over an observable sequence and returns each intermediate result.
        //static member scan (accumulator:'a->'a->'a) (seed:'a)  source =
        //    Observable.Scan(source,seed, Func<'a,'a,'a> accumulator  )
 
        static member LiveCount<'T> (source: IObservable<'T>)=
            Observable.Select<'T,int>(source,Func<'T,int,int>(fun a b->b+1))

        static member LiveLongCount<'T> (source: IObservable<'T>)=
            Observable.Scan<'T,int64>(source,0L,Func<int64,'T,int64>(fun a b->a+1L))

        static member LiveMin (source: IObservable<int>)=
            Observable.Scan<int,int>(source,Int32.MaxValue,Func<int,int,int>(fun a b->Math.Min(a,b)))
        static member LiveMin (source: IObservable<int64>)=
            Observable.Scan<int64,int64>(source,Int64.MaxValue,Func<int64,int64,int64>(fun a b->Math.Min(a,b)))
        static member LiveMin (source: IObservable<double>)=
            Observable.Scan<double,double>(source,Double.MaxValue,Func<double,double,double>(fun a b->Math.Min(a,b)))
        static member LiveMin (source: IObservable<decimal>)=
            Observable.Scan<decimal,decimal>(source,Decimal.MaxValue,Func<decimal,decimal,decimal>(fun a b->Math.Min(a,b)))

        static member LiveMax (source: IObservable<int>)=
            Observable.Scan<int,int>(source,Int32.MinValue,Func<int,int,int>(fun a b->Math.Max(a,b)))
        static member LiveMax (source: IObservable<int64>)=
            Observable.Scan<int64,int64>(source,Int64.MinValue,Func<int64,int64,int64>(fun a b->Math.Max(a,b)))
        static member LiveMax (source: IObservable<double>)=
            Observable.Scan<double,double>(source,Double.MinValue,Func<double,double,double>(fun a b->Math.Max(a,b)))
        static member LiveMax (source: IObservable<decimal>)=
            Observable.Scan<decimal,decimal>(source,Decimal.MinValue,Func<decimal,decimal,decimal>(fun a b->Math.Max(a,b)))

        static member LiveSum (source: IObservable<int>)=
            Observable.Scan<int,int>(source,0,Func<int,int,int>(fun a b->a+b))
        static member LiveSum (source: IObservable<int64>)=
            Observable.Scan<int64,int64>(source,0L,Func<int64,int64,int64>(fun a b->a+b))
        static member LiveSum (source: IObservable<decimal>)=
            Observable.Scan<decimal,decimal>(source,0M,Func<decimal,decimal,decimal>(fun a b->a+b))
        static member LiveSum (source: IObservable<double>)=
            Observable.Scan<double,double>(source,0.0,Func<double,double,double>(fun a b->a+b))
        (*        
        static member LiveSum<'T> (func:'T->int) (source:IObservable<'T>) =
            Observable.Scan<'T,int>(source,0,Func<int,'T,int>(fun a b->a+func(b)))
        static member LiveSum<'T> (func:'T->double) (source:IObservable<'T>)  =
            Observable.Scan<'T,double>(source,0.0,Func<double,'T,double>(fun a b->a+func(b)))
        static member LiveSum<'T> (func:'T->decimal) (source:IObservable<'T>) =
            Observable.Scan<'T,decimal>(source,0M,Func<decimal,'T,decimal>(fun a b->a+func(b)))
        *)
        static member LiveAverage (source:IObservable<int>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> Observable.LiveCount) (source |> Observable.LiveSum)

        static member LiveAverage (source:IObservable<int64>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> Observable.LiveCount) (source |> Observable.LiveSum)
            
        static member LiveAverage (source:IObservable<decimal>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> Observable.LiveCount) (source |> Observable.LiveSum)

        static member LiveAverage (source:IObservable<double>)=
            Observable.zipWith (fun a b-> ((double b)/(double a))) (source |> Observable.LiveCount) (source |> Observable.LiveSum)
        //Range-----------    
        static member liveRange (source:IObservable<int>)=
            Observable.zipWith (fun a b -> a - b) (source |> Observable.LiveMax) (source |> Observable.LiveMin)
            |> Observable.skip 1 
        //Filter
        static member filter1stOrd lambda (seed:double) (source:IObservable<double>)=
            Observable.scan
                        (fun (state:double) (value:double)->
                                    if Double.IsNaN(state) then 
                                        value
                                    else
                                        state*lambda + value*(1.0-lambda)
                         )

 (*
 
 const double lambda = 0.99;
    IObservable<double> emaSequence = noisySequence.Scan(Double.NaN, (emaValue, value) =>
        {
            if (Double.IsNaN(emaValue))
            {
                emaValue = value;
            }
            else
            {
                emaValue = emaValue*lambda + value*(1-lambda);
            }
            return emaValue;
        }).Select(emaValue => emaValue);
  
//Buffer
 var movingAvg = noisySequence.Scan(new List<double>(),
(buffer, value)=>
{
    buffer.Add(value);
    if(buffer.Count>MaxSize)
    {
        buffer.RemoveAt(0);
    }
    return buffer;
}).Select(buffer=>buffer.Average());

//Window
noisySequence.Window(10)
   .Select(window=>window.Average())
   .SelectMany(averageSequence=>averageSequence);

 *)

//Test 1
let o1a=Observable.range 10 15 
let o1b=Observable.LiveCount o1a
let o1c=Observable.LiveSum o1a

o1a |>Observable.subscribe (fun i-> printfn "o1a-->%A" i)  
o1b |>Observable.subscribe (fun i-> printfn "o1b-->%A" i)  
o1c |>Observable.subscribe (fun i-> printfn "o1c-->%A" i)  


//Test 2
let t2=Observable.interval (TimeSpan.FromSeconds(3.0))
let o2a=t2 |> Observable.take(10) 
let o2b=Observable.LiveCount o2a
let o2c=Observable.LiveMin o2a
let o2d=Observable.LiveMax o2a
let o2e=Observable.LiveSum o2a
let o2f=Observable.LiveAverage o2a


o2a |>Observable.subscribe (fun i-> printfn "o2a-->%A" i)  
o2b |>Observable.subscribe (fun i-> printfn "Count(o2b)-->%A" i)  
o2c |>Observable.subscribe (fun i-> printfn "Min(o2c)-->%A" i)  
o2d |>Observable.subscribe (fun i-> printfn "Max(o2d)-->%A" i)  
o2e |>Observable.subscribe (fun i-> printfn "Sum(o2e)-->%A" i)  
o2f |>Observable.subscribe (fun i-> printfn "Avg(o2e)-->%A" i)  


//==============================================================================
(** 
###Define a Helper module. 
It includes several observers to help display results.
*)
module Helper=
    let name=String.Empty
    let PrintThread (location:string)=
            printfn "Loc:%s Thread:%d" location Thread.CurrentThread.ManagedThreadId
    let f1=fun (i:'T)->PrintThread "OnNext"
                       printfn "%s-->%A" name i
    let f2=fun (ex:Exception)->printfn "%s-->%A" name ex.Message
    let f3=fun ()-> PrintThread "OnCompleted"
                    printfn "%s completed" name
    let Dump1<'T>  (name:string) (source:IObservable<'T>) =
        Observable.subscribeWithCallbacks f1 f2 f3 source              
    let Dump2<'T>  (name:string) (source:IObservable<'T>) =
        source |> (Observable.subscribeOn Scheduler.Default) 
               |>  Observable.subscribeWithCallbacks f1 f2 f3              
    let Dump3<'T>  (name:string) (source:IObservable<'T>) =
        source |> (Observable.subscribeOn Scheduler.Default)   
               |> (Observable.observeOn Scheduler.Default) 
               |>  Observable.subscribeWithCallbacks f1 f2 f3              

(**
The following fields are being collected. They are combined to form the field `f=nsl1op`in 
http://download.finance.yahoo.com/d/quotes.csv?s=MSFT,GOOG&f=nsl1op&e=.csv URL:

-   name(n),symbol(s),latestValue(l1),open(o),closeValue(p)

Define the URL.
*)
let url = "http://download.finance.yahoo.com/d/quotes.csv?s="
(**
Define a Stock type to hold Stocks information.
 *)
type Stocks={Name:string;Symbol:string;LatestValue:float;OpenValue:float;CloseValue:float}

(**
###Define an Asynchonous Workflow to get Stock data. 
*)
let getStockData stock fields =
    async {
        //try 
            let wc = new WebClient()
            Helper.PrintThread "Async before Download"
            let! data = wc.AsyncDownloadString(Uri(url + stock + "&f="+ fields + "&e=.csv"))
            Helper.PrintThread "Async after Download"
            //let data = wc.DownloadString(url + stock + "&f="+ fields + "&e=.csv")
            //printfn "%s" data
            let dataLines = data.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries) 
            let s=seq { for line in dataLines do
                            let infos = line.Split(',')
                            yield {Name=infos.[0]; Symbol=infos.[1];
                                    LatestValue=float infos.[2]; OpenValue=float infos.[3]; 
                                    CloseValue=float infos.[4]}
                       }
                |> Array.ofSeq 
            return s //Some s
        //with _ -> return None
      }

(**
You can test `getStockInfo` using: *)
let r=(getStockData "GOOG,MSFT" "nsl1op") |>Async.RunSynchronously
r.[0],r.[1];;

(** 
###Create Observables Sequences.

-   Create an "event" (observable) every 5 seconds (fast for testing pursose)

-   Take 3 elements

-   Get Stocks information asynchoronous

-   Split into individual Stocks

*)
let t1=Observable.interval (TimeSpan.FromSeconds(5.0))
let o1=t1 |> Observable.take(3) 
let o2= Observable.SelectMany(o1,fun _-> Observable.ofAsync (getStockData "GOOG,MSFT" "nsl1op"))
let oStock1=o2 |> Observable.map (fun (i:Stocks[])->i.[0])
let oStock2=o2 |> Observable.map (fun (i:Stocks[])->i.[1])

(**
###Observe with different Observers

####Comments: 
Case 1:

-   Main thread (1) is not blocked.

-   Observable Sequence is blocking Thread 12.

-   Obsever (OnNext) is called in the same thread as the Observable Sequence

Case 2:

-   Obsever (OnNext) is called in the same thread as the Observable Sequence

Case 3:

-   No blocking of Threads.

*)

(** 
####Case 1 
(using Dump1 observer) 
*)
Helper.PrintThread "REPL example1-1-begin"
let d1=Helper.Dump1 "Stock1" oStock1
Helper.PrintThread "REPL example1-1-end"
(** Use `d1.Dispose()` to stop it. *)

(** 
####Case 2 
(using Dump2 observer) 
*)
Helper.PrintThread "REPL example1-2-begin"
let d2=Helper.Dump2 "Stock2" oStock2
Helper.PrintThread "REPL example1-2-end"
(** Use `d2.Dispose()` to stop it. *)

(** 
####Case 3 
(using Dump3 observer) 
*)
Helper.PrintThread "REPL example1-3-begin"
let d3=Helper.Dump3 "Stock3" oStock2
Helper.PrintThread "REPL example1-3-end"
(** Use `d3.Dispose()` to stop it. *)

(** The output of the Case 1 is the following: *)

//Output:
(*
Loc:REPL example1-1-begin Thread:1
Loc:REPL example1-1-end Thread:1
    Loc:Async before Download Thread:12 //*
    Loc:Async after Download Thread:16
    Loc:OnNext Thread:16
        Stock1-->{Name = ""Google Inc."";Symbol = ""GOOG"";LatestValue = 523.4;OpenValue = 521.08;CloseValue = 521.84;}
    Loc:Async before Download Thread:12 //*
    Loc:Async after Download Thread:15
    Loc:OnNext Thread:15
        Stock1-->{Name = ""Google Inc."";Symbol = ""GOOG"";LatestValue = 523.4;OpenValue = 521.08;CloseValue = 521.84;}
    Loc:Async before Download Thread:12 //*
    Loc:Async after Download Thread:20
    Loc:OnNext Thread:20
        Stock1-->{Name = ""Google Inc."";Symbol = ""GOOG"";LatestValue = 523.4;OpenValue = 521.08;CloseValue = 521.84;}
    Loc:OnCompleted Thread:20
Stock1 completed
*)

(** The output of the Case 2 is the following: *)

//Output:
(*
Loc:REPL example1-2-begin Thread:1
Loc:REPL example1-2-end Thread:1
    Loc:Async before Download Thread:7
    Loc:Async after Download Thread:7 //*
    Loc:OnNext Thread:7 //*
        Stock2-->{Name = ""Microsoft Corporation"";Symbol = ""MSFT"";LatestValue = 44.4;OpenValue = 44.48;CloseValue = 44.44;}
    Loc:Async before Download Thread:20
    Loc:Async after Download Thread:8 //*
    Loc:OnNext Thread:8 //*
        Stock2-->{Name = ""Microsoft Corporation"";Symbol = ""MSFT"";LatestValue = 44.4;OpenValue = 44.48;CloseValue = 44.44;}
    Loc:Async before Download Thread:20
    Loc:Async after Download Thread:7 //*
    Loc:OnNext Thread:7 //*
        Stock2-->{Name = ""Microsoft Corporation"";Symbol = ""MSFT"";LatestValue = 44.4;OpenValue = 44.48;CloseValue = 44.44;}
    Loc:OnCompleted Thread:7 //*
Stock2 completed
*)

(** The output of the Case 3 is the following: *)

//Output:
(*
Loc:REPL example1-3-begin Thread:1
Loc:REPL example1-3-end Thread:1
    Loc:Async before Download Thread:6
    Loc:Async after Download Thread:6
    Loc:OnNext Thread:15
        Stock3-->{Name = ""Microsoft Corporation"";Symbol = ""MSFT"";LatestValue = 44.4;OpenValue = 44.48;CloseValue = 44.44;}
    Loc:Async before Download Thread:16
    Loc:Async after Download Thread:6
    Loc:OnNext Thread:15
        Stock3-->{Name = ""Microsoft Corporation"";Symbol = ""MSFT"";LatestValue = 44.4;OpenValue = 44.48;CloseValue = 44.44;}
    Loc:Async before Download Thread:5
    Loc:Async after Download Thread:6
    Loc:OnNext Thread:15
        Stock3-->{Name = ""Microsoft Corporation"";Symbol = ""MSFT"";LatestValue = 44.4;OpenValue = 44.48;CloseValue = 44.44;}
    Loc:OnCompleted Thread:15
Stock3 completed
*)

(** 
## Query Style

Repeat the same example using *Query* style programming.

-   Create an "event" (observable) every 5 seconds (fast for testing pursose)

-   Use `observe` to create a Observable Sequence that gets Stocks data asynchoronous.

-   Use `rxquery` to take 5 elements of the Stock Observable Sequence.

-   Use `observe` to split into individual Stocks Observable Sequence.

 
*)
let t1a=Observable.interval (TimeSpan.FromSeconds(5.0))
let QStocksA= observe {
        let! r=Observable.ofAsync (getStockData "GOOG,MSFT" "nsl1op") //Is it blocking?
        yield r
    }
let QStocksB= rxquery {
        for i in t1a do
        take 5
        for j in QStocksA do
        select j
}
let QStocksC1= observe {
        let! r=QStocksB
        yield r.[0].LatestValue
    }
let QStocksC2= observe {
        let! r=QStocksB
        yield r.[1].LatestValue
    }

let d3a=Helper.Dump3 "Stock1" QStocksC1
let d3b=Helper.Dump3 "Stock1" QStocksC2

(** Use `d3a.Dispose()` and `d3b.Dispose()` to stop observation. *)

(**
####To do:
**Change the Query style solution to make it efficient.*
To keep it simple, this Query solution creates one Observable Sequence for each Stock.
This makes one Stock data internet collection per each Stock.
*)


(*** hide ***)
//============================================================================
//Under Construction
//=============================================================================

//-------------------------------------------------------------------
// Testing & Examples
//-------------------------------------------------------------------
//r1.Dispose() //Stops the subscription
//Observable get stocks prices
//let oStocks=Observable.ofAsync rStocks
//Split for each Stock
//let oStock1=oStocks |> Observable.map (fun (i:Stocks[])->i.[0]) //let oStock1=oStocks.Select(fun i->i.[0])
//let oStock2=oStocks |> Observable.map (fun (i:Stocks[])->i.[1]) 
//let r1=Observable.subscribe (fun result ->printfn "%A" result) oStock1
//let r2=Observable.subscribe (fun result ->printfn "%A" result) oStock2

//-------------------------------------------------------------------
//map example(Select)
//let source=Observable.Range(0,5)
//let d10=Observable.map (fun (i:int)->i+3) source |>Helper.Dump1<int> "+3"


//#I @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1"

//For Unquote (for automated testing - future)
//#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\Unquote.dll"

//open System.Diagnostics

    (*
    let createObserver<'T> (name:string)=
        let f1=fun (i:'T)->PrintThread "OnNext"
                           printfn "%s-->%A" name i
        let f2=fun (ex:Exception)->printfn "%s-->%A" name ex.Message
        let f3=fun ()-> PrintThread "OnCompleted"
                        printfn "%s completed" name
        let observer=Observer.Create(onNext=f1,onError=f2,onCompleted=f3)
        observer

    let Dump<'T>  (name:string) (source:IObservable<'T>) =
        Observable.subscribeObserver (createObserver<'T> name)
    *)

    (*
    let Dump0<'T>  (name:string) (source:IObservable<'T>) =
        Observable.subscribeWithCallbacks (fun (i:'T)->PrintThread "OnNext"
                                                       printfn "%s-->%A" name i
                                          ) 
                                          (fun (ex:Exception)->printfn "%s-->%A" name ex.Message)
                                          (fun _->PrintThread "OnCompleted"
                                                  printfn "%s completed" name
                                          ) source              
    let Dump1<'T> (name:string) (source:IObservable<'T>) =
        source |> (Observable.subscribeOn Scheduler.Default) |>  
            Observable.subscribeWithCallbacks (fun (i:'T)->PrintThread "OnNext"
                                                           printfn "%s-->%A" name i
                                          ) 
                                          (fun (ex:Exception)->printfn "%s-->%A" name ex.Message)
                                          (fun _->PrintThread "OnCompleted"
                                                  printfn "%s completed" name
                                          )            
    let Dump2<'T> (name:string) (source:IObservable<'T>) =
        source |> (Observable.subscribeOn Scheduler.Default)
               |> (Observable.observeOn Scheduler.Default) |>  
            Observable.subscribeWithCallbacks (fun (i:'T)->PrintThread "OnNext"
                                                           printfn "%s-->%A" name i
                                          ) 
                                          (fun (ex:Exception)->printfn "%s-->%A" name ex.Message)
                                          (fun _->PrintThread "OnCompleted"
                                                  printfn "%s completed" name
                                          )            
    *)

(*
//AsyncSeq
//#r @"C:\Project(comp)\Dev_2015\StreamAnalytics_1\StAn_1\packages\FSharp.Control.AsyncSeq.1.15.0\lib\net40\FSharp.Control.AsyncSeq.dll"
//#r @"C:\Project(comp)\Dev_2015\StreamAnalytics_1\StAn_1\packages\HtmlAgilityPack.1.4.9\lib\Net45\HtmlAgilityPack.dll"
//Check the option with FSharpx.Async
//http://fsprojects.github.io/FSharpx.Async/reference/fsharpx-control-observableextensions.html
*)



(*
#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\FSharp.Data.dll"
open FSharp.Data

type Stocks = CsvProvider<"MSFT.csv">

 // Download the stock prices
let msft = Stocks.Load("http://ichart.finance.yahoo.com/table.csv?s=MSFT")

(*I will use here the tags of 
 name(n), symbol(s), the latest value(l1), open(o) and the close value of the last trading day(p)
*)
type StocksFormat1 = CsvProvider<"Format1.csv">

let goog = StocksFormat1.Load("http://download.finance.yahoo.com/d/quotes.csv?s=GOOG&f=nsl1op&e=.csv")

// Look at the most recent row. Note the 'Date' property
// is of type 'DateTime' and 'Open' has a type 'decimal'
let firstRow = msft.Rows |> Seq.head
let lastDate = firstRow.Date
let lastOpen = firstRow.Open

// Print the prices in the HLOC format
for row in msft.Rows do
  printfn "HLOC: (%A, %A, %A, %A %A)" row.Date row.High row.Low row.Open row.Close

#r @"C:\Project(comp)\Dev_2015\Stocks_1\Appl1\lib\FSharp.Charting.dll"
open System
open FSharp.Charting
*)

(*
let rec randomCrawl url = 
  let visited = new System.Collections.Generic.HashSet<_>()
  
  let rec loop url = observable {
    if visited.Add(url) then
      let! doc = (downloadDocument url) |> fromAsync
      match doc with 
      | Some doc ->
          yield url, getTitle doc
          for link in extractLinks doc do
            yield! loop link 
      | _ -> () }
  loop url

rxquery {
  for (url, title) in randomCrawl "http://news.bing.com" do
  where (url.Contains("bing.com") |> not)
  select title
  take 10 into gr
  iter (printfn "%s" gr)
  }
|> ObservableExtensions.Subscribe |> ignore

*)