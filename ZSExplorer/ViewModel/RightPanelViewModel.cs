using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Text;
using System.IO;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.SkiaSharp;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Testing;
using Metalama.Patterns.Wpf;
using Metalama.Patterns.Observability;
using ZSExplorer.Services;
using System.Windows.Threading;

namespace ZSExplorer;

[Observable]
public partial class RightPanelViewModel
{

        private List<MarketDataRow> bidList;
        private List<MarketDataRow> askList;


        private List<MarketDataRow> filteredContractData;
        private int _maxMicroseconds;
        private string _currentTimeUnit = "s";
        private double _timeScale = 1.0;          
        // List<double> logReturn;
        // private bool analyzeAllOptions = false;
        //public PlotModel ECDFPlotModel => EcdfPlot.Model;

        // public double[] ValidReturns { get; private set; }
        // public List<MarketDataRow> SelectedList { get; private set; }
        // public double KSTestStatistic { get; private set; }
        // public double KSTestPValue { get; private set; }
        
        public string SelectedSymbol { get; set; }

        List<MarketDataRow> Data { get; set; } = new();
        OptionInfo info { get; set; } = new();

        public event Action? RequestClose;

        // UI Bindings
        public string ContractSymbolText { get; set; } = "CONTRACT SYMBOL";
        public string OptionDetailsText { get; set; } = "Underlying | Expiration | Strike";

        public string StatusIndicator { get; set; } = "Red"; 
        
        public bool AnalyzeAllOptions { get; set; } = false;

        double TimeWindowSliderMinimum { get; set; } = 0.0;
        double TimeWindowSliderMaximum { get; set; } = 100.0;
        double TimeWindowSliderValue { get; set; } = 0.0;

        string MicrosecondInputBoxText { get; set; } = "0";

        string TimeWindowValueText { get; set; } = "0s";

        string TimeLabel0Text { get; set; } = "0s";
        string TimeLabel25Text { get; set; } = "...";
        string TimeLabel50Text { get; set; } = "...";
        string TimeLabel75Text { get; set; } = "...";
        string TimeLabel100Text { get; set; } = "...";

    // Constructors

    public RightPanelViewModel() {}

    public RightPanelViewModel(List<MarketDataRow> data, OptionInfo info, string selectedSymbol) 
    {

        this.Data = data;
        this.SelectedSymbol = selectedSymbol;
        this.info = info;

        
        UpdateUIFromLists();

        //await Task.Delay(100); // Let UI elements fully initialize
        RunCalculations();
    }

    [Command]
    public void RemoveKsTest()
    {
         var result = MessageBox.Show("Are you sure you want to remove the KS test and reset the panel?",
                                "Confirm Removal",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            RequestClose?.Invoke();  
        }
    }


    /*
    private async Task UpdateUIFromLists(OptionInfo info)
    {
        ContractSymbolText = info.Symbol;

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

            OptionDetailsText = $"{info.Symbol} | All {info.OptionType}";
        }
        else
        {
            // Just analyze the specific strike
            filteredContractData = data
                .Where(row => row.Symbol == contractText)
                .OrderBy(row => row.DateTime)
                .ToList();

            OptionDetailsText = $" Underlying: {info.Symbol} | Type: {info.OptionType} | Exp: {info.ExpirationDate:MM-dd-yyyy} | Strike: {info.StrikePrice}";
        }

        var startTime = filteredContractData[0].DateTime;
        var endTime = filteredContractData[filteredContractData.Count - 1].DateTime;

        if (filteredContractData.Count > 1)
        {
            DateTime start = filteredContractData.First().DateTime;
            DateTime end = filteredContractData.Last().DateTime;

            //SetupTimeSliderFromDateRange(start, end);
        }

        bidList = filteredContractData.Where(row => row.BidAsk == true).ToList();
        askList = filteredContractData.Where(row => row.BidAsk == false).ToList();

        StatusIndicator = "Green";

    }
    */
    
    [Command]
    public void ToggleAnalyzeAll()
    {
        AnalyzeAllOptions = !AnalyzeAllOptions;
        UpdateUIFromLists();
    }

    private void UpdateUIFromLists()
    {
        ContractSymbolText = info.Symbol;

        // Check if we have data
        if (Data == null || Data.Count == 0)
        {
            StatusIndicator = "Red";
            OptionDetailsText = $"ERROR: No data found. AnalyzeAll={AnalyzeAllOptions}, Contract={SelectedSymbol}, DataCount={Data?.Count ?? 0}";
            return;
        }

        if (AnalyzeAllOptions)
        {


            // Group all contracts by type (Call or Put)
            filteredContractData = Data
                .Where(row =>
                {
                    var opt = ParseOptionsSymbol.Parse(row.Symbol);
                    return opt.Symbol == info.Symbol && opt.OptionType == info.OptionType;
                })
                .OrderBy(row => row.DateTime)
                .ToList();

            OptionDetailsText = $"{info.Symbol} | All {info.OptionType}";
        }
        else
        {

            if (string.IsNullOrEmpty(SelectedSymbol))
            {
                StatusIndicator = "Red";
                OptionDetailsText = "ERROR: Contract symbol not set";
                return;
            }

            filteredContractData = Data
                .Where(row => row.Symbol == SelectedSymbol)
                .OrderBy(row => row.DateTime)
                .ToList();

            OptionDetailsText = $" Underlying: {info.Symbol} | Type: {info.OptionType} | Exp: {info.ExpirationDate:MM-dd-yyyy} | Strike: {info.StrikePrice}";

        }

        // Check if data was processed and we have data
        if (filteredContractData == null || filteredContractData.Count == 0)
        {
            StatusIndicator = "Red";
            OptionDetailsText = $"ERROR: No filtered data found. AnalyzeAll={AnalyzeAllOptions}, Contract={SelectedSymbol}, filteredContractDataCount={filteredContractData?.Count ?? 0}";
            return;
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

        StatusIndicator = "Green";
    }

    private void SetupTimeSliderFromDateRange(DateTime start, DateTime end)
    {
        TimeSpan totalSpan = end - start;
        double totalSeconds = totalSpan.TotalSeconds;

        double unitValue;
        string unit;

        unit = "s";
        unitValue = totalSeconds;

        TimeWindowSliderMinimum = 0;
        TimeWindowSliderMaximum = unitValue;
        TimeWindowSliderValue = 0;

        _maxMicroseconds = (int)(unitValue * (unit == "s" ? 1000000 : 60000000));
        _currentTimeUnit = unit;
        _timeScale = (unit == "s") ? 1 : 60;

        TimeWindowValueText = $"0 {unit}";
        MicrosecondInputBoxText = "0";

        if (filteredContractData == null || filteredContractData.Count < 2) return;

        UpdateSliderTimeLabels(unit, unitValue);
    }

    private void UpdateSliderTimeLabels(string unit, double totalUnits)
    {
        TimeLabel0Text = $"0{unit}";
        TimeLabel25Text = $"{Math.Round(totalUnits * 0.25)}{unit}";
        TimeLabel50Text = $"{Math.Round(totalUnits * 0.5)}{unit}";
        TimeLabel75Text = $"{Math.Round(totalUnits * 0.75)}{unit}";
        TimeLabel100Text = $"{Math.Round(totalUnits)}{unit}";
    }

    public void RunCalculations()
        {
            // StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
            // try
            // {
            //     bool filterBid = BidOnlyCheckbox.IsChecked == true;
            //     SelectedList = filterBid ? bidList : askList;

            //     // Time filtering based on slider
            //     double sliderValue = TimeWindowSlider.Value;
            //     if (sliderValue > 0)
            //     {
            //         // Calculate cutoff time
            //         TimeSpan timeWindow = TimeSpan.FromSeconds(sliderValue * _timeScale);
            //         DateTime endTime = SelectedList.Last().DateTime;
            //         DateTime cutoffTime = endTime - timeWindow;

            //         SelectedList = SelectedList
            //             .Where(row => row.DateTime >= cutoffTime)
            //             .ToList();
            //     }

            //     var priceChangedRows = new List<MarketDataRow> { SelectedList[0] };
            //     for (int i = 1; i < SelectedList.Count; i++)
            //     {
            //         if (SelectedList[i].Price != SelectedList[i - 1].Price)
            //         {
            //             priceChangedRows.Add(SelectedList[i]);
            //         }
            //     }
            //     SelectedList = priceChangedRows;

            //     // Compute log returns
            //     logReturn = new List<double>();
            //     for (int i = 1; i < SelectedList.Count; i++)
            //     {
            //         var prev = SelectedList[i - 1];
            //         var curr = SelectedList[i];

            //         if (prev.Price > 0 && curr.Price > 0)
            //         {
            //             double logRet = Math.Log((double)curr.Price / prev.Price);
            //             logReturn.Add(logRet);
            //         }

            //     }

            //     // Remove NaN and Infinity values
            //     ValidReturns = logReturn
            //     .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            //     .ToArray();

            //     // Perform t-distribution fitting and KS test
            //     StudentTDistributionZeroMean tDist = new StudentTDistributionZeroMean();

            //     StudentTResult tDistResult = tDist.StudentT(ValidReturns);

            //     double location = tDistResult.Location;
            //     double std = tDistResult.Scale;
            //     double degreesFreedom = tDistResult.DegreesFreedom;

            //     var tDistArr = new TDistribution(degreesFreedom);

            //     double[] standardizedReturns = ValidReturns
            //     .Select(x => x / std)
            //     .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            //     .ToArray();

            //     KolmogorovSmirnovTest ks = new KolmogorovSmirnovTest(standardizedReturns, tDistArr);


            //     // Sample statistics
            //     SampleSizeText.Text = ValidReturns.Length.ToString("N0");
            //     MeanReturnText.Text = ValidReturns.Average().ToString("F6");
            //     StdDevText.Text = MathNet.Numerics.Statistics.Statistics.StandardDeviation(ValidReturns).ToString("F6");

            //     // Fitted t-distribution parameters
            //     LocationParamText.Text = location.ToString("F6");
            //     ScaleParamText.Text = std.ToString("F6");
            //     DegreesFreedomText.Text = degreesFreedom.ToString("F2");

            //     KSTestStatistic = ks.Statistic;
            //     KSTestPValue = ks.PValue;

            //     // KS test results
            //     string statistic = $"Test Statistic: {ks.Statistic:F4}";
            //     string significance = $"Decision: {(ks.Significant ? "Reject H0 (Significant)" : "Fail to Reject H0")}";
            //     string pValue = $"P-value: {ks.PValue:E4} ";

            //     UpdateKsTestResults(statistic, significance, pValue);
            //     PlotEcdfWithTDistribution(standardizedReturns, tDistArr);
            //     StatusIndicator.Fill = new SolidColorBrush(Colors.Green);

            // }
            // catch (Exception ex)
            // {
            //     throw new InvalidOperationException($"Error during calculations: {ex.Message}\n{ex.StackTrace}");
            // }
        }


}