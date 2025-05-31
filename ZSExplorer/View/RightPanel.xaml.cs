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

namespace ZSExplorer
{
    public partial class RightPanel : UserControl
    {
       // DataFrame df;


            //List<MarketDataRow> useBidList;
            //List<MarketDataRow> useAskList;

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
        
        private async void BidOnlyCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            _ = RunCalculations();
        }

        private void BidOnlyCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            _ = RunCalculations();
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

            StatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

        }

    public async Task RunCalculations()
    {
            try
            {
                bool filterBid = BidOnlyCheckbox.IsChecked == true;

                // Split list
                var bidList = filteredContractData.Where(row => row.BidAsk == true).ToList();
                var askList = filteredContractData.Where(row => row.BidAsk == false).ToList();

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
                
                Console.WriteLine($"Valid returns count: {string.Join(", ", tDistArr)}");

                var ks = new KolmogorovSmirnovTest(normalizedReturns, tDistArr);


                //Update sample statistics
                SampleSizeText.Text = validReturns.Length.ToString("N0");
                MeanReturnText.Text = validReturns.Average().ToString("F6");
                StdDevText.Text = MathNet.Numerics.Statistics.Statistics.StandardDeviation(validReturns).ToString("F6");

                // Update fitted t-distribution parameters
                LocationParamText.Text = location.ToString("F6");
                ScaleParamText.Text = std.ToString("F6");
                DegreesFreedomText.Text = degreesFreedom.ToString("F2");
                ConvergenceIterationsText.Text = "N/A"; // If not iterated, use N/A or replace with actual value if available

                // Update KS test results
                KsTestStatText.Text = $"Test Statistic: {ks.Statistic:F4}";
                StatDecisionText.Text = $"Decision: {(ks.Significant ? "Reject H0 (Significant)" : "Fail to Reject H0")}";
                PValueText.Text = $"P-value: {ks.PValue:E4}";
                CriticalValueText.Text = "Critical value: -"; // MathNet doesn't expose critical value, can remove or leave as placeholder

                // Optional: Fit diagnostics (if using iterative fitting later)
                ConvIterationsText.Text = "N/A";
                ErrorToleranceText.Text = "N/A";
                FitQualityText.Text = "N/A";
                FitStatusText.Text = "Completed";


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during calculations: {ex.Message}\n{ex.StackTrace}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}

/*
public async Task RunCalculations()
{
    try
    {

                return;
        // Step 0: Read filter choice from checkboxes
        //         bool filterBid = BidOnlyCheckbox.IsChecked == true;

        // // Step 1: Get columns
        // var dateCol = df.Columns["datetime"] as PrimitiveDataFrameColumn<DateTime>;
        // var priceCol = df.Columns["Price"] as PrimitiveDataFrameColumn<long>;
        // var bidAskCol = df.Columns["BidAsk"] as PrimitiveDataFrameColumn<bool>;

        // if (dateCol == null || priceCol == null || bidAskCol == null)
        //     throw new Exception("Missing required columns.");

        // // Step 2: Apply filter
        // List<int> filteredIndices = new List<int>();
        // for (int i = 0; i < df.Rows.Count; i++)
        // {
        //     bool bidAskValue = bidAskCol[i].GetValueOrDefault();

        //     if ((!filterBid ) || // no filter selected, take all
        //         (filterBid && !bidAskValue)) 
        //     {
        //         filteredIndices.Add(i);
        //     }
        // }

        // if (filteredIndices.Count < 2)
        //     throw new Exception("Not enough data after filtering.");

        // // Step 3: Calculate log returns on filtered data
        // var logReturns = new DoubleDataFrameColumn("LogReturn");
        // logReturns.Append(0); // first value has no return

        // for (int j = 1; j < filteredIndices.Count; j++)
        // {
        //     int prevIdx = filteredIndices[j - 1];
        //     int currIdx = filteredIndices[j];

        //     long prevPrice = priceCol[prevIdx].GetValueOrDefault();
        //     long currPrice = priceCol[currIdx].GetValueOrDefault();

        //     if (prevPrice > 0 && currPrice > 0)
        //     {
        //         double logRet = Math.Log((double)currPrice / prevPrice);
        //         logReturns.Append(logRet);
        //     }
        //     else
        //     {
        //         logReturns.Append(double.NaN);
        //     }
        // }

        // // Pad rest of DataFrame with NaN (for alignment)
        // while (logReturns.Length < df.Rows.Count)
        // {
        //     logReturns.Append(double.NaN);
        // }

        // // Step 4: Add to DataFrame (remove existing column first if needed)

        // df.Columns.Add(logReturns);

        // Step 5: Build display rows
        // List<MarketDataRowLog> dataList = new List<MarketDataRowLog>();
        // for (long i = 0; i < df.Rows.Count; i++)
        // {
        //     var row = new MarketDataRowLog
        //     {
        //         Symbol = (string)df.Columns["sybmol"][i],
        //         DateTime = (DateTime)df.Columns["datetime"][i],
        //         MMID = (string)df.Columns["MMID"][i],
        //         BidAsk = (bool)df.Columns["BidAsk"][i],
        //         Price = (long)df.Columns["Price"][i],
        //         LogReturn = (double)df.Columns["LogReturn"][i]
        //     };
        //     dataList.Add(row);
        // }

        // MarkDataGrid.ItemsSource = dataList;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error during calculations: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
*/
        //     public async Task RunCalculations()
        //     {
        //         try
        //         {
        //         //     // Step 1: Cast the column to its actual type
        //         //     var dateCol = df.Columns["DateTime"] as PrimitiveDataFrameColumn<DateTime>;
        //         //     var priceCol = df.Columns["Price"] as PrimitiveDataFrameColumn<long>;
        //         //     HashSet<DateTime> uniqueDates = new HashSet<DateTime>();

        //         //     // Step 2: Create new Double column
        //         //     var logReturns = new DoubleDataFrameColumn("LogReturn");
        //         //     logReturns.Append(0); // First value has no previous log return

        //         //     for (int i = 0; i < dateCol.Length; i++)
        //         //     {
        //         //         if (!uniqueDates.Contains(dateCol[i].GetValueOrDefault()))
        //         //         {

        //         //             uniqueDates.Add(dateCol[i].GetValueOrDefault());
        //         //             long prev = priceCol[i - 1].GetValueOrDefault();
        //         //             long curr = priceCol[i].GetValueOrDefault();

        //         //             double logReturn = Math.Log((double)curr) - Math.Log((double)prev);
        //         //             logReturns.Append(logReturn);

        //         //         }
        //         //     }   
        //         //     for (int i = 1; i < priceCol.Length; i++)
        //         //         {

        //         //             long prev = priceCol[i - 1].GetValueOrDefault();
        //         //             long curr = priceCol[i].GetValueOrDefault();

        //         //             if (prev > 0 && curr > 0)
        //         //             {
        //         //                 double logReturn = Math.Log((double)curr) - Math.Log((double)prev);
        //         //                 logReturns.Append(logReturn);
        //         //             }
        //         //             else
        //         //             {
        //         //                 logReturns.Append(double.NaN); // Handle edge case
        //         //             }
        //         //         }


        //         //     // Step 3: Compute log returns
        //         //     for (int i = 1; i < priceCol.Length; i++)
        //         //     {

        //         //         long prev = priceCol[i - 1].GetValueOrDefault();
        //         //         long curr = priceCol[i].GetValueOrDefault();

        //         //         if (prev > 0 && curr > 0)
        //         //         {
        //         //             double logReturn = Math.Log((double)curr) - Math.Log((double)prev);
        //         //             logReturns.Append(logReturn);
        //         //         }
        //         //         else
        //         //         {
        //         //             logReturns.Append(double.NaN); // Handle edge case
        //         //         }
        //         //     }

        //         //     // Step 4: Add to DataFrame
        //         //     df.Columns.Add(logReturns);

        //         List<MarketDataRowLog> dataList = new List<MarketDataRowLog>();
        //         for (long i = 0; i < df.Rows.Count; i++)
        //         {
        //             // Create a new MarketDataRow for each row in the DataFrame
        //             var row = new MarketDataRowLog
        //             {
        //                 Symbol = (string)df.Columns["sybmol"][i],
        //                 DateTime = (DateTime)df.Columns["datetime"][i],
        //                 MMID = (string)df.Columns["MMID"][i],
        //                 BidAsk = (Boolean)df.Columns["BidAsk"][i],
        //                 Price = (long)df.Columns["Price"][i],
        //                 //LogReturn = (double)df.Columns["LogReturn"][i]
        //             };
        //             dataList.Add(row);
        //         }



        //         //Bind to DataGrid
        //         MarkDataGrid.ItemsSource = dataList;


        //         // MessageBox.Show($"{df.Columns["LogReturn"]} Log returns calculated and added to DataFrame.", "Calculation Complete", MessageBoxButton.OK, MessageBoxImage.Information);




        //         // // Ensure the Price column exists and has enough data
        //         // var priceColumn = df.Columns["Price"] as PrimitiveDataFrameColumn<long>;
        //         // if (priceColumn == null || priceColumn.Length < 2)
        //         //     return;

        //         // // Convert price data to double
        //         // List<double> prices = priceColumn
        //         //     .Where(p => p.HasValue)
        //         //     .Select(p => Convert.ToDouble(p.Value))
        //         //     .ToList();

        //         // if (prices.Count < 2)
        //         //     return;

        //         // // Calculate log returns
        //         // List<double> returns = new List<double>();
        //         // for (int i = 1; i < prices.Count; i++)
        //         // {
        //         //     if (prices[i - 1] > 0 && prices[i] > 0)
        //         //     {
        //         //         double ret = Math.Log(prices[i] / prices[i - 1]);
        //         //         if (!double.IsNaN(ret) && !double.IsInfinity(ret))
        //         //             returns.Add(ret);
        //         //     }
        //         // }

        //         // if (returns.Count < 2)
        //         //     return;

        //         // // Sample statistics
        //         // int sampleSize = returns.Count;
        //         // double mean = returns.Average();
        //         // double stdDev = Math.Sqrt(returns.Select(r => Math.Pow(r - mean, 2)).Average());

        //         // UpdateSampleStatistics(sampleSize, mean, stdDev);

        //         // // Fit Student's t-distribution using Accord.NET
        //         // //var tDist = new Accord.Statistics.Distributions.Univariate.StudentTDistribution();
        //         // var tDist = new MathNet.Numerics.Distributions.StudentT(0, 1, 10); // initial guess: mean=0, scale=1, dof=10

        //         // tDist.Fit(returns.ToArray());
        //         // // Fit the StudentT distribution to the returns data
        //         // tDist.Fit(returns.ToArray());
        //         // string location = tDist.Location.ToString("F4");
        //         // string scale = tDist.Scale.ToString("F4");
        //         // string degreesFreedom = tDist.DegreesOfFreedom.ToString("F2");

        //         // UpdateFittedDistribution(location, scale, degreesFreedom, "20"); // placeholder for iterations

        //         // // Kolmogorov–Smirnov Test
        //         // var ksTest = new Accord.Statistics.Testing.KolmogorovSmirnovTest(returns.ToArray(), tDist);

        //         // string testStat = ksTest.Statistic.ToString("F4");
        //         // string decision = ksTest.Significant ? "Reject Null" : "Fail to Reject";
        //         // string pValue = ksTest.PValue.ToString("F4");
        //         // string criticalValue = ksTest.CriticalValue.ToString("F4");

        //         // UpdateKsTestResults(testStat, decision, pValue, criticalValue);
        //     }
        //         catch (Exception ex)
        //         {
        //             MessageBox.Show($"Error during calculations: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //         }
        // }
    

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
