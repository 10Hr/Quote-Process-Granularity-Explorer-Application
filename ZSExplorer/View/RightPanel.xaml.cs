using System.Windows;
using System.Windows.Controls;
using ZSExplorer.Services;
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
        private List<MarketDataRow> bidList;
        private List<MarketDataRow> askList;
        List<MarketDataRow> data;
        private List<MarketDataRow> filteredContractData;
        string contractText;
        private int _maxMicroseconds;
        private string _currentTimeUnit = "s";
        private double _timeScale = 1.0;          
        List<double> logReturn;
        private bool analyzeAllOptions = false;
        public PlotModel ECDFPlotModel => EcdfPlot.Model;


        public double[] ValidReturns { get; private set; }
        public List<MarketDataRow> SelectedList { get; private set; }
        public double KSTestStatistic { get; private set; }
        public double KSTestPValue { get; private set; }


        public RightPanel(List<MarketDataRow> callData, List<MarketDataRow> putData, string contractText)
        {
            InitializeComponent();

            OptionInfo info = ParseOptionsSymbol.Parse(contractText);

            if (info.OptionType == "Call")
            {
                this.data = callData;
            }
            else if (info.OptionType == "Put")
            {
                this.data = putData;
            }
            else
            {
                throw new ArgumentException("Invalid contract type. Expected 'Call' or 'Put'.");
            }

            this.contractText = contractText;

            this.Loaded += RightPanel_Loaded;
        }

        private async void RightPanel_Loaded(object sender, RoutedEventArgs e)
        {
            OptionInfo info = ParseOptionsSymbol.Parse(contractText);
            await UpdateUIFromLists(info);
            await Task.Delay(100); // Let UI elements fully initialize
            _ = RunCalculations();
        }


        private void UpdateSliderRangeForBidAsk()
        {
            bool filterBid = BidOnlyCheckbox.IsChecked == true;

            SelectedList = filterBid ? bidList : askList;

            if (SelectedList.Count > 1)
            {
                DateTime start = SelectedList.First().DateTime;
                DateTime end = SelectedList.Last().DateTime;
                SetupTimeSliderFromDateRange(start, end);
            }
        }

        public void UpdateKsTestResults(string testStatistic, string decision, string pValue)
        {
            KsTestStatText.Text = testStatistic;
            StatDecisionText.Text = decision;
            PValueText.Text = pValue;
        }


        private async Task UpdateUIFromLists(OptionInfo info)
        {
            ContractSymbolText.Text = info.Symbol;

            if (analyzeAllOptions)
            {
                // Group all contracts by type (Call or Put)
                filteredContractData = data
                    .Where(row =>
                    {
                        var opt = ParseOptionsSymbol.Parse(row.Symbol);
                        return opt.Symbol == info.Symbol && opt.OptionType == info.OptionType;
                    })
                    .OrderBy(row => row.DateTime)
                    .ToList();

                OptionDetailsText.Text = $"{info.Symbol} | All {info.OptionType}";
            }
            else
            {
                // Just analyze the specific strike
                filteredContractData = data
                    .Where(row => row.Symbol == contractText)
                    .OrderBy(row => row.DateTime)
                    .ToList();

                OptionDetailsText.Text = $" Underlying: {info.Symbol} | Type: {info.OptionType} | Exp: {info.ExpirationDate:MM-dd-yyyy} | Strike: {info.StrikePrice}";
            }

            var startTime = filteredContractData[0].DateTime;
            var endTime = filteredContractData[filteredContractData.Count - 1].DateTime;

            if (filteredContractData.Count > 1)
            {
                DateTime start = filteredContractData.First().DateTime;
                DateTime end = filteredContractData.Last().DateTime;

                SetupTimeSliderFromDateRange(start, end);
            }

            bidList = filteredContractData.Where(row => row.BidAsk == true).ToList();
            askList = filteredContractData.Where(row => row.BidAsk == false).ToList();

            StatusIndicator.Fill = new SolidColorBrush(Colors.Green);

        }
        
        private async Task UpdateUIFromLists()
        {

            OptionInfo info = ParseOptionsSymbol.Parse(contractText);
            ContractSymbolText.Text = info.Symbol;

            if (analyzeAllOptions)
            {
                // Group all contracts by type (Call or Put)
                filteredContractData = data
                    .Where(row =>
                    {
                        var opt = ParseOptionsSymbol.Parse(row.Symbol);
                        return opt.Symbol == info.Symbol && opt.OptionType == info.OptionType;
                    })
                    .OrderBy(row => row.DateTime)
                    .ToList();

                OptionDetailsText.Text = $"{info.Symbol} | All {info.OptionType}";
            }
            else
            {
                // Just analyze the specific strike contract
                filteredContractData = data
                    .Where(row => row.Symbol == contractText)
                    .OrderBy(row => row.DateTime)
                    .ToList();

                OptionDetailsText.Text = $" Underlying: {info.Symbol} | Type: {info.OptionType} | Exp: {info.ExpirationDate:MM-dd-yyyy} | Strike: {info.StrikePrice}";
            }

            var startTime = filteredContractData[0].DateTime;
            var endTime = filteredContractData[filteredContractData.Count - 1].DateTime;

            if (filteredContractData.Count > 1)
            {
                DateTime start = filteredContractData.First().DateTime;
                DateTime end = filteredContractData.Last().DateTime;

                SetupTimeSliderFromDateRange(start, end);
            }

            bidList = filteredContractData.Where(row => row.BidAsk == true).ToList();
            askList = filteredContractData.Where(row => row.BidAsk == false).ToList();

            StatusIndicator.Fill = new SolidColorBrush(Colors.Green);

        }

        public async Task RunCalculations()
        {
            StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
            try
            {
                bool filterBid = BidOnlyCheckbox.IsChecked == true;
                SelectedList = filterBid ? bidList : askList;

                // Time filtering based on slider
                double sliderValue = TimeWindowSlider.Value;
                if (sliderValue > 0)
                {
                    // Calculate cutoff time
                    TimeSpan timeWindow = TimeSpan.FromSeconds(sliderValue * _timeScale);
                    DateTime endTime = SelectedList.Last().DateTime;
                    DateTime cutoffTime = endTime - timeWindow;

                    SelectedList = SelectedList
                        .Where(row => row.DateTime >= cutoffTime)
                        .ToList();
                }

                var priceChangedRows = new List<MarketDataRow> { SelectedList[0] };
                for (int i = 1; i < SelectedList.Count; i++)
                {
                    if (SelectedList[i].Price != SelectedList[i - 1].Price)
                    {
                        priceChangedRows.Add(SelectedList[i]);
                    }
                }
                SelectedList = priceChangedRows;

                // Compute log returns
                logReturn = new List<double>();
                for (int i = 1; i < SelectedList.Count; i++)
                {
                    var prev = SelectedList[i - 1];
                    var curr = SelectedList[i];

                    if (prev.Price > 0 && curr.Price > 0)
                    {
                        double logRet = Math.Log((double)curr.Price / prev.Price);
                        logReturn.Add(logRet);
                    }

                }

                // Remove NaN and Infinity values
                ValidReturns = logReturn
                .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
                .ToArray();

                // Perform t-distribution fitting and KS test
                StudentTDistributionZeroMean tDist = new StudentTDistributionZeroMean();

                StudentTResult tDistResult = tDist.StudentT(ValidReturns);

                double location = tDistResult.Location;
                double std = tDistResult.Scale;
                double degreesFreedom = tDistResult.DegreesFreedom;

                var tDistArr = new TDistribution(degreesFreedom);

                double[] standardizedReturns = ValidReturns
                .Select(x => x / std)
                .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
                .ToArray();

                KolmogorovSmirnovTest ks = new KolmogorovSmirnovTest(standardizedReturns, tDistArr);


                // Sample statistics
                SampleSizeText.Text = ValidReturns.Length.ToString("N0");
                MeanReturnText.Text = ValidReturns.Average().ToString("F6");
                StdDevText.Text = MathNet.Numerics.Statistics.Statistics.StandardDeviation(ValidReturns).ToString("F6");

                // Fitted t-distribution parameters
                LocationParamText.Text = location.ToString("F6");
                ScaleParamText.Text = std.ToString("F6");
                DegreesFreedomText.Text = degreesFreedom.ToString("F2");

                KSTestStatistic = ks.Statistic;
                KSTestPValue = ks.PValue;

                // KS test results
                string statistic = $"Test Statistic: {ks.Statistic:F4}";
                string significance = $"Decision: {(ks.Significant ? "Reject H0 (Significant)" : "Fail to Reject H0")}";
                string pValue = $"P-value: {ks.PValue:E4} ";

                UpdateKsTestResults(statistic, significance, pValue);
                PlotEcdfWithTDistribution(standardizedReturns, tDistArr);
                StatusIndicator.Fill = new SolidColorBrush(Colors.Green);

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during calculations: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void PlotEcdfWithTDistribution(double[] standardizedReturns, TDistribution tDist)
        {
            var sortedReturns = standardizedReturns.OrderBy(x => x).ToArray();
            int n = sortedReturns.Length;

            var ecdfPoints = new List<DataPoint>();
            var tCdfPoints = new List<DataPoint>();

            for (int i = 0; i < n; i++)
            {
                double x = sortedReturns[i];
                double y = (i + 1.0) / n;
                ecdfPoints.Add(new DataPoint(x, y));
                tCdfPoints.Add(new DataPoint(x, tDist.DistributionFunction(x)));
            }

            var plotModel = new PlotModel
            {
                Title = "Empirical CDF vs Fitted t-Distribution",
                IsLegendVisible = true
            };

            plotModel.Legends.Add(new Legend
            {
                LegendPosition = LegendPosition.TopRight,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendBorderThickness = 0,
                LegendBackground = OxyColors.White
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Standardized Log Return",
                IsZoomEnabled = false,
                IsPanEnabled = false
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "CDF",
                Minimum = 0,
                Maximum = 1,
                IsZoomEnabled = false,
                IsPanEnabled = false
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

            //if (totalSeconds < 300) // Less than 5 minutes
            //{
                unit = "s";
                unitValue = totalSeconds;
           // }
            // else
            // {
            //     unit = "min";
            //     unitValue = totalSpan.TotalMinutes;
            // }

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

        // ========= Event Handlers =========
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

        private void AnalyzeAllCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            analyzeAllOptions = true;
            UpdateUIFromLists(); 
        }

        private void AnalyzeAllCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            analyzeAllOptions = false;
            UpdateUIFromLists();
        }

        private void TimeWindowSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Slider slider)
            {
                Point position = e.GetPosition(slider);
                double relativeClick = position.X / slider.ActualWidth;

                double newValue = slider.Minimum + (relativeClick * (slider.Maximum - slider.Minimum));
                slider.Value = newValue;
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
        }

        private void MarketDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;
        }
    }
}
