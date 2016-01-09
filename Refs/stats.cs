using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.Immutable;
using System.Threading;

namespace RxStatistics
{
    public static class RxExt
    {
        public static IObservable<int> LiveCount<T>(this IObservable<T> source)
        {
            return source.Select((a, b) => b + 1);
        }
        public static IObservable<long> LiveLongCount<T>(this IObservable<T> source)
        {
            return source.Scan(0L, (a, b) => a + 1);
        }

        public static IObservable<int> LiveMin(this IObservable<int> source)
        {
            return source.Scan(Int32.MaxValue, Math.Min);
        }

        public static IObservable<double> LiveMin(this IObservable<double> source)
        {
            return source.Scan(double.MaxValue, Math.Min);
        }

        public static IObservable<decimal> LiveMin(this IObservable<decimal> source)
        {
            return source.Scan(decimal.MaxValue, Math.Min);
        }

        public static IObservable<int> LiveMax(this IObservable<int> source)
        {
            return source.Scan(Int32.MinValue, Math.Max);
        }

        public static IObservable<double> LiveMax(this IObservable<double> source)
        {
            return source.Scan(double.MinValue, Math.Max)
                        ;
        }

        public static IObservable<decimal> LiveMax(this IObservable<decimal> source)
        {
            return source.Scan(decimal.MinValue, Math.Max)
                        ;
        }

        public static IObservable<int> LiveSum(this IObservable<int> source)
        {
            return source.Scan(0, (a, b) => a + b)
                        ;
        }

        public static IObservable<decimal> LiveSum(this IObservable<decimal> source)
        {
            return source.Scan(0m, (a, b) => a + b)
                        ;
        }
        public static IObservable<double> LiveSum(this IObservable<double> source)
        {
            return source.Scan(0d, (a, b) => a + b)
                        ;
        }
        public static IObservable<int> LiveSum<T>(this IObservable<T> source, Func<T, int> func)
        {
            return source.Scan(0, (a, b) => a + func(b))
                        ;
        }

        public static IObservable<double> LiveSum<T>(this IObservable<T> source, Func<T, double> func)
        {
            return source.Scan(0D, (a, b) => a + func(b))
                        ;
        }

        public static IObservable<decimal> LiveSum<T>(this IObservable<T> source, Func<T, decimal> func)
        {
            return source.Scan(0m, (a, b) => a + func(b))
                        ;
        }

        public static IObservable<double> LiveAverage(this IObservable<int> source)
        {
            return source.Publish(p => p.LiveCount()
                                         .Zip(p.LiveSum(), (a, b) => (double)b / a));
        }

        public static IObservable<double> LiveAverage(this IObservable<double> source)
        {
            return source.Publish(p => p.LiveCount()
                                    .Zip(p.LiveSum(), (a, b) => b / a))
                        ;
        }
        public static IObservable<decimal> LiveAverage(this IObservable<decimal> source)
        {
            return source.Publish(p => p.LiveCount()
                                    .Zip(p.LiveSum(), (a, b) => b / a))
                        ;
        }

        public static IObservable<double> LiveAverage<T>(this IObservable<T> source, Func<T, int> func)
        {
            return source.Publish(p => p.LiveCount()
                                        .Zip(p.LiveSum(func), (a, b) => (double)b / a))
                        ;
        }

        public static IObservable<decimal> LiveAverage<T>(this IObservable<T> source, Func<T, decimal> func)
        {
            return source.Publish(p => p.LiveCount()
                                        .Zip(p.LiveSum(func), (a, b) => (decimal)b / a))
                        ;
        }

        public static IObservable<double> LiveAverage<T>(this IObservable<T> source, Func<T, double> func)
        {
            return source.Publish(p => p.LiveCount()
                                        .Zip(p.LiveSum(func), (a, b) => b / a))
                        ;
        }

        public static IObservable<IGrouping<int, int>> LiveModa(this IObservable<int> source)
        {
            return source.GroupBy(a => a)
                      .Select(a => a.LiveCount().Select(b => Tuple.Create(a.Key, b)))
                      .Merge()
                      .Scan(default(KeyValuePair<int, ImmutableStack<int>>), (a, b) =>
                      {
                          if (a.Key > b.Item2)
                              return a;

                          if (a.Key == b.Item2)
                              return new KeyValuePair<int, ImmutableStack<int>>(b.Item2, a.Value.Push(b.Item1));

                          return new KeyValuePair<int, ImmutableStack<int>>(b.Item2, ImmutableStack<int>.Empty.Push(b.Item1));
                      })
                      .Select(a => a.Value.AsGroup(a.Key))
                          ;
        }
        public static IObservable<IGrouping<int, decimal>> LiveModa(this IObservable<decimal> source)
        {
            return source.GroupBy(a => a)
                      .Select(a => a.LiveCount().Select(b => Tuple.Create(a.Key, b)))
                      .Merge()
                      .Scan(default(KeyValuePair<int, ImmutableStack<decimal>>), (a, b) =>
                      {
                          if (a.Key > b.Item2)
                              return a;

                          if (a.Key == b.Item2)
                              return new KeyValuePair<int, ImmutableStack<decimal>>(b.Item2, a.Value.Push(b.Item1));

                          return new KeyValuePair<int, ImmutableStack<decimal>>(b.Item2, ImmutableStack<decimal>.Empty.Push(b.Item1));
                      })
                      .Select(a => a.Value.AsGroup(a.Key))
                          ;
        }
        public static IObservable<int> LiveRange(this IObservable<int> source)
        {
            return source.Publish(p => p.LiveMax().Zip(p.LiveMin(), (a, b) => a - b).Skip(1))
                            ;
        }

        public static IObservable<double> LiveRange(this IObservable<double> source)
        {
            return source.Publish(p => p.LiveMax().Zip(p.LiveMin(), (a, b) => a - b).Skip(1))
                            ;
        }

        public static IObservable<decimal> LiveRange(this IObservable<decimal> source)
        {
            return source.Publish(p => p.LiveMax().Zip(p.LiveMin(), (a, b) => a - b).Skip(1))
                            ;
        }

        public static IObservable<double> LiveMedian(this IObservable<int> source)
        {
            return source.AccumulateAllOrdered()
                         .Select(a => (a.Count % 2) == 0
                                        ? (double)(a[(a.Count / 2) - 1] + a[a.Count / 2]) / 2
                                        : (double)(a[(a.Count / 2)])
                                )
                          ;
        }


        public static IObservable<decimal> LiveMedian(this IObservable<decimal> source)
        {
            return source.AccumulateAllOrdered()
                         .Select(a => (a.Count % 2) == 0
                                        ? (a[(a.Count / 2) - 1] + a[a.Count / 2]) / 2
                                        : (a[(a.Count / 2)])
                                )
                          ;
        }


        public static IObservable<double> LiveMedian(this IObservable<double> source)
        {
            return source.AccumulateAllOrdered()
                         .Select(a => (a.Count % 2) == 0
                                        ? (a[(a.Count / 2) - 1] + a[a.Count / 2]) / 2
                                        : (a[(a.Count / 2)])
                                )
                          ;
        }

        public static IObservable<IReadOnlyList<T>> AccumulateAll<T>(this IObservable<T> next)
        {
            return next.Scan(ImmutableList<T>.Empty, (a, b) => a.Add(b));
        }

        public static IObservable<IReadOnlyList<int>> AccumulateAllOrdered(this IObservable<int> source)
        {
            return source.Scan(ImmutableList<int>.Empty, (a, b) =>
            {
                var res = a.BinarySearch(b);
                if (res <= -1)
                    res = ~res;

                return a.Insert(res, b);
            });

        }
        public static IObservable<IReadOnlyList<double>> AccumulateAllOrdered(this IObservable<double> source)
        {
            return source.Scan(ImmutableList<double>.Empty, (a, b) =>
            {
                var res = a.BinarySearch(b);
                if (res <= -1)
                    res = ~res;

                return a.Insert(res, b);
            });
        }

        public static IObservable<IReadOnlyList<decimal>> AccumulateAllOrdered(this IObservable<decimal> source)
        {
            return source.Scan(ImmutableList<decimal>.Empty, (a, b) =>
            {
                var res = a.BinarySearch(b);
                if (res <= -1)
                    res = ~res;
               
                return a.Insert(res, b);
            });
        }


    }
}
//==========================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxStatistics
{
    public static class LinqExt
    {
        public static IGrouping<K,V> AsGroup<K,V>(this IEnumerable<V> source, K key)
        {
            return SimpleGroupWrapper.Create(key, source); 
        }
    }

    internal static class SimpleGroupWrapper
    {
        public static SimpleGroupWrapper<K,V> Create<K,V>(K key, IEnumerable<V> source)
        {
            return new SimpleGroupWrapper<K, V>(key, source);
        }
    }
    internal class SimpleGroupWrapper<K,V> : IGrouping<K,V>
    {
        private readonly IEnumerable<V> _source;
        private readonly K _key;
        public SimpleGroupWrapper(K key, IEnumerable<V> source)
        {
            if (source == null)
                throw new NullReferenceException("source");

            _source = source;
            _key = key;
        }
        public K Key
        {
            get { return _key; }
        }

        public IEnumerator<V> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}
//==========================================
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace RxStatistics.WPF
{
    public class SimpleUsageViewModel : ReactiveObject
    {
        private readonly ReactiveList<decimal> _sum = new ReactiveList<decimal>();

        private readonly ReactiveList<IGrouping<int, decimal>> _modaValue = new ReactiveList<IGrouping<int, decimal>>();
        private readonly ReactiveList<string> _modaText = new ReactiveList<string>();

        private readonly ReactiveList<decimal> _median = new ReactiveList<decimal>();
        private readonly ReactiveList<decimal> _average = new ReactiveList<decimal>();
        private readonly ReactiveList<decimal> _max = new ReactiveList<decimal>();
        private readonly ReactiveList<decimal> _min = new ReactiveList<decimal>();
        private readonly ReactiveList<int> _counter = new ReactiveList<int>();

        private decimal _value;
        private readonly ReactiveList<KeyValuePair<int, decimal>> _history = new ReactiveList<KeyValuePair<int, decimal>>();

        public SimpleUsageViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            InsertValueCommand = ReactiveCommand.Create();
            IObservable<decimal> addCommand = InsertValueCommand.Select(a => Value)
                                                .Publish().RefCount();

            ClearStatisticsCommand = ReactiveCommand.Create();
            IObservable<object> cleanMethod = ClearStatisticsCommand;

            addCommand.Select((a, b) => new KeyValuePair<int, decimal>(b + 1, a))
                        .TakeUntil(cleanMethod).Repeat()
                        .Do(a => _history.Insert(0, a))
                        .Subscribe();

            var liveCount = addCommand.LiveCount().TakeUntil(cleanMethod).Repeat();
            liveCount.Subscribe(a => _counter.Insert(0, a));

            var liveAverage = addCommand.LiveAverage().TakeUntil(cleanMethod).Repeat();
            liveAverage.Subscribe(a => _average.Insert(0, a));

            var liveSum = addCommand.LiveSum().TakeUntil(cleanMethod).Repeat();
            liveSum.Subscribe(a => _sum.Insert(0, a));

            var liveMax = addCommand.LiveMax().TakeUntil(cleanMethod).Repeat();
            liveMax.Subscribe(a => _max.Insert(0, a));

            var liveMin = addCommand.LiveMin().TakeUntil(cleanMethod).Repeat();
            liveMin.Subscribe(a => _min.Insert(0, a));

            var liveMedian = addCommand.LiveMedian().TakeUntil(cleanMethod).Repeat();
            liveMedian.Subscribe(a => _median.Insert(0, a));

            var liveModa = addCommand.LiveModa().TakeUntil(cleanMethod).Repeat();
            liveModa.Subscribe(a => _modaValue.Insert(0, a));

            var textModa = _modaValue.ItemsAdded.Select(a => a.Key + "x = " +
                                                         new string(
                                                         a.Select(b => string.Format("{0:C2}", b))
                                                             .Aggregate((b, c) => b + " & " + c)
                                                             .ToArray()
                                                         ));
            textModa.Subscribe(a => _modaText.Insert(0, a));

            cleanMethod.Do(a => _counter.Clear())
                        .Do(a => _average.Clear())
                        .Do(a => _sum.Clear())
                        .Do(a => _max.Clear())
                        .Do(a => _min.Clear())
                        .Do(a => _modaText.Clear())
                        .Do(a => _median.Clear())
                        .Do(a => _history.Clear())
                        .Subscribe();
        }

        public ReactiveCommand<object> InsertValueCommand { get; set; }

        public ReactiveCommand<object> ClearStatisticsCommand { get; set; }

        public decimal Value
        {
            get { return _value; }
            set { this.RaiseAndSetIfChanged(ref _value, value); }
        }

        public ReactiveList<int> Counter
        {
            get { return _counter; }
        }

        public ReactiveList<decimal> Min
        {
            get { return _min; }
        }

        public ReactiveList<decimal> Max
        {
            get { return _max; }
        }

        public ReactiveList<decimal> Average
        {
            get { return _average; }
        }

        public ReactiveList<decimal> Sum
        {
            get { return _sum; }
        }

        public ReactiveList<string> ModaText
        {
            get { return _modaText; }
        }

        public ReactiveList<decimal> Median
        {
            get { return _median; }
        }
        public ReactiveList<KeyValuePair<int, decimal>> Pairs
        {
            get { return _history; }
        }
    }
}





