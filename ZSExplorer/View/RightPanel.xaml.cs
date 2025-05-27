using Microsoft.Win32;
using Microsoft.Data.Analysis;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System.IO;
using Apache.Arrow.Ipc;
using System.Windows.Controls;
using System.Linq;
using MathNet;
using Accord.Statistics;
using ZSExplorer.Services;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Testing;
using System.Windows.Input;
using System.Windows.Media;

namespace ZSExplorer
{
    public partial class RightPanel : UserControl
    {
        DataFrame df;

        public RightPanel(DataFrame dataFrame)
        {
            df = dataFrame;
            InitializeComponent();
            UpdateUIFromDataFrame();
        }



        /*

        Symbol = (string)df.Columns["sybmol"][i],
        DateTime = (DateTime)df.Columns["datetime"][i],
        MMID = (string)df.Columns["MMID"][i],
        BidAsk = (Boolean)df.Columns["BidAsk"][i],
        Price = (long)df.Columns["Price"][i]

        */



        private async Task UpdateUIFromDataFrame()
        {

            OptionInfo info = ParseOptionsSymbol.Parse(df.Columns["sybmol"][0].ToString());
            ContractSymbolText.Text = info.Symbol;
            OptionDetailsText.Text = $" Underlying: {info.Symbol} | Type: {info.OptionType} | Exp: {info.ExpirationDate:MM-dd-yyyy} | Strike: {info.StrikePrice}";

            await Task.Delay(50);
            await RunCalculations();
            SetupTimeSliderFromPriceColumn();
            StatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

        }

        public async Task RunCalculations()
        {
            try
            {
                // Step 1: Cast the column to its actual type
                var priceCol = df.Columns["Price"] as PrimitiveDataFrameColumn<long>;

                // Step 2: Create new Double column
                var logReturns = new DoubleDataFrameColumn("LogReturn");
                logReturns.Append(0); // First value has no previous log return

                // Step 3: Compute log returns
                for (int i = 1; i < priceCol.Length; i++)
                {
                    long prev = priceCol[i - 1].GetValueOrDefault();
                    long curr = priceCol[i].GetValueOrDefault();

                    if (prev > 0 && curr > 0)
                    {
                        double logReturn = Math.Log((double)curr) - Math.Log((double)prev);
                        logReturns.Append(logReturn);
                    }
                    else
                    {
                        logReturns.Append(double.NaN); // Handle edge case
                    }
                }

                // Step 4: Add to DataFrame
                df.Columns.Add(logReturns);

            List<MarketDataRowLog> dataList = new List<MarketDataRowLog>();
            for (long i = 0; i < df.Rows.Count; i++)
            {
                    // Create a new MarketDataRow for each row in the DataFrame
                var row = new MarketDataRowLog
                {
                    Symbol = (string)df.Columns["sybmol"][i],
                    DateTime = (DateTime)df.Columns["datetime"][i],
                    MMID = (string)df.Columns["MMID"][i],
                    BidAsk = (Boolean)df.Columns["BidAsk"][i],
                    Price = (long)df.Columns["Price"][i],
                    LogReturn = (double)df.Columns["LogReturn"][i]
                };
                dataList.Add(row);
            }



            //Bind to DataGrid
            MarkDataGrid.ItemsSource = dataList;


               // MessageBox.Show($"{df.Columns["LogReturn"]} Log returns calculated and added to DataFrame.", "Calculation Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                


                // // Ensure the Price column exists and has enough data
                // var priceColumn = df.Columns["Price"] as PrimitiveDataFrameColumn<long>;
                // if (priceColumn == null || priceColumn.Length < 2)
                //     return;

                // // Convert price data to double
                // List<double> prices = priceColumn
                //     .Where(p => p.HasValue)
                //     .Select(p => Convert.ToDouble(p.Value))
                //     .ToList();

                // if (prices.Count < 2)
                //     return;

                // // Calculate log returns
                // List<double> returns = new List<double>();
                // for (int i = 1; i < prices.Count; i++)
                // {
                //     if (prices[i - 1] > 0 && prices[i] > 0)
                //     {
                //         double ret = Math.Log(prices[i] / prices[i - 1]);
                //         if (!double.IsNaN(ret) && !double.IsInfinity(ret))
                //             returns.Add(ret);
                //     }
                // }

                // if (returns.Count < 2)
                //     return;

                // // Sample statistics
                // int sampleSize = returns.Count;
                // double mean = returns.Average();
                // double stdDev = Math.Sqrt(returns.Select(r => Math.Pow(r - mean, 2)).Average());

                // UpdateSampleStatistics(sampleSize, mean, stdDev);

                // // Fit Student's t-distribution using Accord.NET
                // //var tDist = new Accord.Statistics.Distributions.Univariate.StudentTDistribution();
                // var tDist = new MathNet.Numerics.Distributions.StudentT(0, 1, 10); // initial guess: mean=0, scale=1, dof=10

                // tDist.Fit(returns.ToArray());
                // // Fit the StudentT distribution to the returns data
                // tDist.Fit(returns.ToArray());
                // string location = tDist.Location.ToString("F4");
                // string scale = tDist.Scale.ToString("F4");
                // string degreesFreedom = tDist.DegreesOfFreedom.ToString("F2");

                // UpdateFittedDistribution(location, scale, degreesFreedom, "20"); // placeholder for iterations

                // // Kolmogorov–Smirnov Test
                // var ksTest = new Accord.Statistics.Testing.KolmogorovSmirnovTest(returns.ToArray(), tDist);

                // string testStat = ksTest.Statistic.ToString("F4");
                // string decision = ksTest.Significant ? "Reject Null" : "Fail to Reject";
                // string pValue = ksTest.PValue.ToString("F4");
                // string criticalValue = ksTest.CriticalValue.ToString("F4");

                // UpdateKsTestResults(testStat, decision, pValue, criticalValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during calculations: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
private int _maxMicroseconds;

        private void SetupTimeSliderFromPriceColumn()
        {
            var priceColumn = df.Columns["Price"];
            _maxMicroseconds = (int)priceColumn.Length;

            TimeWindowSlider.Minimum = 0;
            TimeWindowSlider.Maximum = _maxMicroseconds - 1;
            TimeWindowSlider.Value = 0;

            TimeWindowValueText.Text = $"0 μs";
            MicrosecondInputBox.Text = "0";
        }

private void TimeWindowSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    if (sender is Slider slider)
    {
        Point position = e.GetPosition(slider);
        double relativeClick = position.X / slider.ActualWidth;

        double newValue = slider.Minimum + (relativeClick * (slider.Maximum - slider.Minimum));
        slider.Value = newValue;

        e.Handled = true; // prevent default behavior
    }
}

        private void TimeWindowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int μs = (int)TimeWindowSlider.Value;
            TimeWindowValueText.Text = $"{μs:N0} μs";

            // Prevent infinite update loop
            if (MicrosecondInputBox.Text != μs.ToString())
            {
                MicrosecondInputBox.Text = μs.ToString();
            }
        }

private void MicrosecondInputBox_TextChanged(object sender, TextChangedEventArgs e)
{
    if (int.TryParse(MicrosecondInputBox.Text, out int μs))
    {
        if (μs > TimeWindowSlider.Maximum)
        {
            TimeWindowSlider.Maximum = μs;
        }

        TimeWindowSlider.Value = μs;
    }
}




        public class MarketDataRowLog
        {
            public string Symbol { get; set; }
            public DateTime DateTime { get; set; }
            public string MMID { get; set; }
            public bool BidAsk { get; set; }
            public long Price { get; set; }

            public double LogReturn { get; set; }
        }






        // private void TimeWindowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        // {
        //     int index = (int)e.NewValue;
        //     if (_microsecondOffsets != null && index >= 0 && index < _microsecondOffsets.Count)
        //     {
        //         long μs = _microsecondOffsets[index];
        //         TimeWindowValueText.Text = $"{μs:N0} μs";
        //     }
        // }


        // private void TimeWindowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        // {

        //     // if (TimeWindowValueText != null)
        //     //     TimeWindowValueText.Text = $"{(int)e.NewValue} (units)";
        //     // OnTimeWindowChanged((int)e.NewValue);
        // }

        public void RemoveKsTest(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).RemoveKsTest();
            df.Columns.Remove("LogReturn");
        }

    private void MarketDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            //OnRemoveClicked();
        }

        // Placeholder methods for logic you will implement

        public void OnTimeWindowChanged(int newValue)
        {
            // TODO: Implement slider changed logic
        }



        public void UpdateStatusIndicator(bool isCalculating)
        {
            // TODO: Change StatusIndicator fill color accordingly
        }

        public void UpdateContractSymbol(string symbol)
        {
            ContractSymbolText.Text = symbol;
        }

        public void UpdateOptionDetails(string details)
        {
            OptionDetailsText.Text = details;
        }

        public void UpdateSampleStatistics(int sampleSize, double meanReturn, double stdDev)
        {
            SampleSizeText.Text = sampleSize.ToString();
            MeanReturnText.Text = meanReturn.ToString("F4");
            StdDevText.Text = stdDev.ToString("F4");
        }

        public void UpdateFittedDistribution(string location, string scale, string degreesFreedom, string convergenceIterations)
        {
            LocationParamText.Text = location;
            ScaleParamText.Text = scale;
            DegreesFreedomText.Text = degreesFreedom;
            ConvergenceIterationsText.Text = convergenceIterations;
        }

        public void UpdateKsTestResults(string testStatistic, string decision, string pValue, string criticalValue)
        {
            KsTestStatText.Text = testStatistic;
            StatDecisionText.Text = decision;
            PValueText.Text = pValue;
            CriticalValueText.Text = criticalValue;
        }

        public void UpdateConvergenceInfo(string iterations, string errorTolerance, string fitQuality, string status)
        {
            ConvIterationsText.Text = iterations;
            ErrorToleranceText.Text = errorTolerance;
            FitQualityText.Text = fitQuality;
            FitStatusText.Text = status;
        }

        public void DrawEcdfPlot()
        {
            // TODO: Implement ECDF drawing on EcdfPlotCanvas
        }
    }
}
