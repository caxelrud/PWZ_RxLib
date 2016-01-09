//http://enumeratethis.com/2011/07/26/financial-charts-reactive-extensions/
(*
var rand = new Random();

var prices = Observable.Generate(
    5d,
    i => i > 0, 
    i => i + rand.NextDouble() - 0.5,
    i => i,
    i => TimeSpan.FromSeconds(0.1)
);


from buffer in prices.Buffer(TimeSpan.FromSeconds(1))
select new
{
    Open = buffer.First(),
    High = buffer.Max(),
    Low = buffer.Min(),
    Close = buffer.Last()
}

ar source = new Subject<char>();
var timer = Observable.Interval(TimeSpan.FromSeconds(1));
var buffer = source.Buffer(() => timer);
buffer.Subscribe(Console.WriteLine);

source.OnNext('a');
source.OnNext('b');
source.OnNext('c');

Thread.Sleep(1100);

source.OnNext('d');
source.OnNext('e');
source.OnNext('f');
source.OnCompleted();

class OHLC
{
    public double? Open;
    public double? High;
    public double? Low;
    public double Close;
}

// (TAccumulate, TSource) -> TAccumulate
static OHLC Accumulate(OHLC state, double price)
{
    // Take the current values & apply the price update.    
    state.Open = state.Open ?? price;
    state.High = state.High.HasValue ? state.High > price ? state.High : price : price;
    state.Low = state.Low.HasValue ? state.Low < price ? state.Low : price : price;
    state.Close = price;
    return state;
}

from window in prices.Window(TimeSpan.FromSeconds(1))
from result in window.Aggregate(new OHLC(), Accumulate)
select result

using System;

namespace ConsoleApplication124
{
    using System.Reactive.Linq;

    class Program
    {
        static void Main()
        {
            var rand = new Random();
            var prices = Observable.Generate(
                5d, i => i > 0, i => i + rand.NextDouble() - 0.5, i => i, i => TimeSpan.FromSeconds(0.1));

            var query = from window in prices.Window(TimeSpan.FromSeconds(1))
                        from result in window.Aggregate(new Ohlc(), Accumulate)
                        select result;
            query.Subscribe(Console.WriteLine);
            Console.ReadLine();
        }

        class Ohlc
        {
            public double? Open;
            public double? High;
            public double? Low;
            public double Close;

            public override string ToString()
            {
                return (new { Open, High, Low, Close }).ToString();
            }
        }

        static Ohlc Accumulate(Ohlc current, double price)
        {
            current.Open = current.Open ?? price;
            current.High = current.High.HasValue ? current.High > price ? current.High : price : price;
            current.Low = current.Low.HasValue ? current.Low < price ? current.Low : price : price;
            current.Close = price;
            return current;
        }
    }
}

using System;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace Chart
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            // Configure the chart
            var series = chart1.Series[0];
            series.ChartType = SeriesChartType.Candlestick;
            series.XValueType = ChartValueType.Time;
            var area = chart1.ChartAreas[0];
            area.Axes[0].Title = "Time";
            area.AxisX.LabelStyle.IntervalType = DateTimeIntervalType.Seconds;
            area.AxisX.LabelStyle.Format = "T";

            
            // Test prices
            var rand = new Random();
            var prices = Observable.Generate(5d, i => i > 0, i => i + rand.NextDouble() - 0.5, i => i, i => TimeSpan.FromSeconds(0.1));

            // OHLC query
            var query =
                from window in prices.Window(TimeSpan.FromSeconds(1))
                from ohlc in window.Aggregate(new OHLC(), Accumulate)
                select ohlc;

            // Subscribe & display results
            query.ObserveOn(this).Subscribe(x => series.Points.AddXY(DateTime.Now, x.High, x.Low, x.Open, x.Close));

        }

        class OHLC
        {
            public double? Open;
            public double? High;
            public double? Low;
            public double Close;
        }

        static OHLC Accumulate(OHLC current, double price)
        {
            current.Open = current.Open ?? price;
            current.High = current.High.HasValue ? current.High > price ? current.High : price : price;
            current.Low = current.Low.HasValue ? current.Low < price ? current.Low : price : price;
            current.Close = price;
            return current;
        }
    }
}

// Colours
chart1.BackColor = Color.Black;
chart1.ChartAreas[0].Axes[0].LineColor = Color.LimeGreen;
chart1.ChartAreas[0].Axes[0].TitleForeColor = Color.LimeGreen;
chart1.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LimeGreen;
chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.LimeGreen;
chart1.ChartAreas[0].Axes[1].LineColor = Color.LimeGreen;
chart1.ChartAreas[0].Axes[1].TitleForeColor = Color.LimeGreen;
chart1.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LimeGreen;
chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.LimeGreen;
chart1.ChartAreas[0].BackColor = Color.Black;
series["PriceDownColor"] = "Red";
*)