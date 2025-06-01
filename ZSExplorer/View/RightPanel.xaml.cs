using Microsoft.Win32;
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
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Legends;

namespace ZSExplorer
{
    public partial class RightPanel : UserControl
    {
       // DataFrame df;


        private List<MarketDataRow> bidList;
        private List<MarketDataRow> askList;

        List<MarketDataRow> data;
        private List<MarketDataRow> filteredContractData;
        string contractText;
        private int _maxMicroseconds;
        private string _currentTimeUnit = "s";    // or "min"
        private double _timeScale = 1.0;          // 1 for seconds, 60 for minutes

        List<double> logReturn;



        public RightPanel(List<MarketDataRow> data, string contractText)
        {
            InitializeComponent();
            this.data = data;
            this.contractText = contractText;

            this.Loaded += RightPanel_Loaded;
        }

        private async void RightPanel_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateUIFromLists();
            _ = RunCalculations();
        }
        
        private void BidOnlyCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            _ = RunCalculations();
            UpdateSliderRangeForBidAsk();
        }


        private void BidOnlyCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            _ = RunCalculations();
            UpdateSliderRangeForBidAsk();
        }


        private void UpdateSliderRangeForBidAsk()
        {
            bool filterBid = BidOnlyCheckbox.IsChecked == true;

            var selectedList = filterBid ? bidList : askList;

            if (selectedList.Count > 1)
            {
                DateTime start = selectedList.First().DateTime;
                DateTime end = selectedList.Last().DateTime;
                SetupTimeSliderFromDateRange(start, end);
            }
        }




        private async Task UpdateUIFromLists()
        {

            OptionInfo info = ParseOptionsSymbol.Parse(contractText);
            ContractSymbolText.Text = info.Symbol;
            OptionDetailsText.Text = $" Underlying: {info.Symbol} | Type: {info.OptionType} | Exp: {info.ExpirationDate:MM-dd-yyyy} | Strike: {info.StrikePrice}";


            await Task.Delay(50);

            filteredContractData = data
                .Where(row => row.Symbol == contractText)
                .OrderBy(row => row.DateTime)
                .ToList();

            var startTime = filteredContractData[0].DateTime;
            var endTime = filteredContractData[filteredContractData.Count - 1].DateTime;

            // Show in message box
            // MessageBox.Show($"Contract '{contractText}'\nStart: {startTime:G}\nEnd: {endTime:G}'\nStartax: {startTimeax:G}\nEndax: {endTimeax:G}", 
            //     "Filtered Contract Time Range", 
            //     MessageBoxButton.OK, 
            //     MessageBoxImage.Information);

            if (filteredContractData.Count > 1)
            {
                DateTime start = filteredContractData.First().DateTime;
                DateTime end = filteredContractData.Last().DateTime;

                SetupTimeSliderFromDateRange(start, end);
            }

            bidList = filteredContractData.Where(row => row.BidAsk == true).ToList();
            askList = filteredContractData.Where(row => row.BidAsk == false).ToList();

            StatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

        }

    public async Task RunCalculations()
    {
            try
            {
                bool filterBid = BidOnlyCheckbox.IsChecked == true;

                // Split list
                //bidList = filteredContractData.Where(row => row.BidAsk == true).ToList();
                //askList = filteredContractData.Where(row => row.BidAsk == false).ToList();

                Console.WriteLine($"Bid list count: {bidList.Count}, Ask list count: {askList.Count}");

                // Use full list by default
                List<MarketDataRow> selectedList = filterBid ? bidList : askList;

                // Time filtering based on slider
                double sliderValue = TimeWindowSlider.Value;
                if (sliderValue > 0)
                {
                    // Calculate cutoff time
                    TimeSpan timeWindow = TimeSpan.FromSeconds(sliderValue * _timeScale);
                    DateTime endTime = selectedList.Last().DateTime;
                    DateTime cutoffTime = endTime - timeWindow;

                    selectedList = selectedList
                        .Where(row => row.DateTime >= cutoffTime)
                        .ToList();
                }


                var priceChangedRows = new List<MarketDataRow>();
                priceChangedRows.Add(selectedList[0]); // always keep first row
                for (int i = 1; i < selectedList.Count; i++)
                {
                    if (selectedList[i].Price != selectedList[i - 1].Price)
                    {
                        priceChangedRows.Add(selectedList[i]);
                    }
                }
                selectedList = priceChangedRows;

                // Compute log returns
                logReturn = new List<double>();
                for (int i = 1; i < selectedList.Count; i++)
                {
                    var prev = selectedList[i - 1];
                    var curr = selectedList[i];

                    if (prev.Price > 0 && curr.Price > 0)
                    {
                        double logRet = Math.Log((double)curr.Price / prev.Price);
                        logReturn.Add(logRet);
                    }

                }

                //var logReturnArray = logReturn.ToArray();
                double[] validReturns = logReturn
                .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
                .ToArray();



                StudentTDistributionZeroMean tDist = new StudentTDistributionZeroMean();

                StudentTResult tDistResult = tDist.StudentT(validReturns);

                double location = tDistResult.Location;
                double std = tDistResult.Scale;
                double degreesFreedom = tDistResult.DegreesFreedom;

                double[] normalizedReturns = validReturns
                .Select(x => (x - location) / std)
                .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
                .ToArray();

                var tDistArr = new TDistribution(tDistResult.DegreesFreedom);

                KolmogorovSmirnovTest ks = new KolmogorovSmirnovTest(normalizedReturns, tDistArr);


                //Update sample statistics
                SampleSizeText.Text = validReturns.Length.ToString("N0");
                MeanReturnText.Text = validReturns.Average().ToString("F6");
                StdDevText.Text = MathNet.Numerics.Statistics.Statistics.StandardDeviation(validReturns).ToString("F6");

                // Update fitted t-distribution parameters
                LocationParamText.Text = location.ToString("F6");
                ScaleParamText.Text = std.ToString("F6");
                DegreesFreedomText.Text = degreesFreedom.ToString("F2");

                // Update KS test results
                KsTestStatText.Text = $"Test Statistic: {ks.Statistic:F4}";
                StatDecisionText.Text = $"Decision: {(ks.Significant ? "Reject H0 (Significant)" : "Fail to Reject H0")}";
                PValueText.Text = $"P-value: {ks.PValue:E4} ";
                
                PlotEcdfWithTDistribution(normalizedReturns, tDistArr);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during calculations: {ex.Message}\n{ex.StackTrace}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void PlotEcdfWithTDistribution(double[] normalizedReturns, TDistribution tDist)
        {
            var sortedReturns = normalizedReturns.OrderBy(x => x).ToArray();
            int n = sortedReturns.Length;

            var ecdfPoints = new List<DataPoint>();
            var tCdfPoints = new List<DataPoint>();

            for (int i = 0; i < n; i++)
            {
                double x = sortedReturns[i];
                double y = (i + 1.0) / n;
                ecdfPoints.Add(new DataPoint(x, y));
                tCdfPoints.Add(new DataPoint(x, tDist.DistributionFunction(x))); // normalized x, so no scaling needed
            }

            var plotModel = new PlotModel
            {
                Title = "Empirical CDF vs Fitted t-Distribution",
                IsLegendVisible = true
            };

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Normalized Log Return"
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "CDF",
                Minimum = 0,
                Maximum = 1
            });

            var ecdfSeries = new LineSeries
            {
                Title = "Empirical CDF",
                StrokeThickness = 2,
                Color = OxyColors.Blue
            };
            ecdfSeries.Points.AddRange(ecdfPoints);

            var tCdfSeries = new LineSeries
            {
                Title = "Fitted t-Distribution CDF",
                StrokeThickness = 2,
                Color = OxyColors.Red
            };
            tCdfSeries.Points.AddRange(tCdfPoints);

            plotModel.Series.Add(ecdfSeries);
            plotModel.Series.Add(tCdfSeries);

            EcdfPlot.Model = plotModel;
            EcdfPlot.InvalidatePlot(true);
        }


        public void DrawEcdfPlot(double[] sortedReturns, TDistribution tDist, double location, double scale, double degreesFreedom)
        {


            // // Compute ECDF points
            // var ecdfPoints = sortedReturns
            //     .Select((x, i) => new DataPoint(x, (i + 1.0) / sortedReturns.Length))
            //     .ToList();

            // // Fitted t-distribution
            // var tCdfPoints = sortedReturns
            //     .Select(x => new DataPoint(x, tDist.DistributionFunction((x - location) / scale)))
            //     .ToList();

            // // Build OxyPlot model
            // var plotModel = new PlotModel
            // {
            //     Title = "Empirical CDF vs Fitted t-Distribution",
            //     IsLegendVisible = true
            // };

            // plotModel.Legends.Add(new Legend
            // {
            //     LegendPosition = LegendPosition.RightTop,
            //     LegendPlacement = LegendPlacement.Outside
            // });

            // plotModel.Series.Add(new LineSeries
            // {
            //     Title = "Empirical CDF",
            //     StrokeThickness = 2,
            //     Color = OxyColors.Blue,
            //     ItemsSource = ecdfPoints
            // });

            // plotModel.Series.Add(new LineSeries
            // {
            //     Title = "Fitted t-Distribution CDF",
            //     StrokeThickness = 2,
            //     Color = OxyColors.Red,
            //     ItemsSource = tCdfPoints
            // });

            // plotModel.Axes.Add(new LinearAxis
            // {
            //     Position = AxisPosition.Bottom,
            //     Title = "Log Return"
            // });

            // plotModel.Axes.Add(new LinearAxis
            // {
            //     Position = AxisPosition.Left,
            //     Title = "CDF",
            //     Minimum = 0,
            //     Maximum = 1
            // });

            // EcdfPlot.Model = plotModel;
        }



        private void UpdateSliderTimeLabels(List<MarketDataRow> filteredData, string unit, double totalUnits)
        {
            if (filteredData == null || filteredData.Count < 2) return;


            TimeLabel0.Text = $"0{unit}";
            TimeLabel25.Text = $"{Math.Round(totalUnits * 0.25)}{unit}";
            TimeLabel50.Text = $"{Math.Round(totalUnits * 0.5)}{unit}";
            TimeLabel75.Text = $"{Math.Round(totalUnits * 0.75)}{unit}";
            TimeLabel100.Text = $"{Math.Round(totalUnits)}{unit}";
        }


        
        private void SetupTimeSliderFromDateRange(DateTime start, DateTime end)
        {
            TimeSpan totalSpan = end - start;
            double totalSeconds = totalSpan.TotalSeconds;

            double unitValue;
            string unit;

            if (totalSeconds < 300) // Less than 5 minutes
            {
                unit = "s";
                unitValue = totalSeconds;
            }
            else
            {
                unit = "min";
                unitValue = totalSpan.TotalMinutes;
            }

            TimeWindowSlider.Minimum = 0;
            TimeWindowSlider.Maximum = unitValue;
            TimeWindowSlider.Value = 0;

            _maxMicroseconds = (int)(unitValue * (unit == "s" ? 1000000 : 60000000));
            _currentTimeUnit = unit;
            _timeScale = (unit == "s") ? 1 : 60;

            TimeWindowValueText.Text = $"0 {unit}";
            MicrosecondInputBox.Text = "0";

            UpdateSliderTimeLabels(filteredContractData, unit, unitValue);
        }

private void TimeWindowSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    if (sender is Slider slider)
    {
        Point position = e.GetPosition(slider);
        double relativeClick = position.X / slider.ActualWidth;

        double newValue = slider.Minimum + (relativeClick * (slider.Maximum - slider.Minimum));
        slider.Value = newValue;

        //e.Handled = true; // prevent default behavior
    }
}

        private void TimeWindowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int timeValue = (int)TimeWindowSlider.Value;
            TimeWindowValueText.Text = $"{timeValue:N0} {_currentTimeUnit}";

            // Prevent infinite update loop
            if (MicrosecondInputBox.Text != timeValue.ToString())
            {
                MicrosecondInputBox.Text = timeValue.ToString();
            }
            
        }

        private void MicrosecondInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(MicrosecondInputBox.Text, out int timeValue))
            {
                if (timeValue > TimeWindowSlider.Maximum)
                {
                    TimeWindowSlider.Maximum = timeValue;
                }

                TimeWindowSlider.Value = timeValue;
            }
            _ = RunCalculations();
        }

        public void RemoveKsTest(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).RemoveKsTest();
            //df.Columns.Remove("LogReturn");
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
            // ConvIterationsText.Text = iterations;
            // ErrorToleranceText.Text = errorTolerance;
            // FitQualityText.Text = fitQuality;
            // FitStatusText.Text = status;
        }

    }
}
