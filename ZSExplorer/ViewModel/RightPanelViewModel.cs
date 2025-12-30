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
using Accord.Statistics.Distributions;
using Accord.Statistics.Testing;
using Metalama.Patterns.Wpf;
using Metalama.Patterns.Observability;
using ZSExplorer.Services;
using System.Windows.Threading;
using ZSExplorer.Tests;

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
    List<double> logReturn;
    public PlotModel ECDFPlotModel { get; set; }

    public double[] ValidReturns { get; private set; }
    public List<MarketDataRow> SelectedList { get; private set; } = new();
    public double KSTestStatistic { get; private set; }
    public double KSTestPValue { get; private set; }

    public string SelectedSymbol { get; set; }

    List<MarketDataRow> Data { get; set; } = new();
    OptionInfo info { get; set; } = new();

    public event Action? RequestClose;

    // UI Bindings
    public string ContractSymbolText { get; set; } = "CONTRACT SYMBOL";
    public string OptionDetailsText { get; set; } = "Underlying | Expiration | Strike";

    public Brush StatusIndicatorBrush { get; set; } = Brushes.Red;

    public bool AnalyzeAllOptions { get; set; } = false;
    public bool UseBidPrices { get; set; } = false;

    public string TimeWindowValueText { get; set; } = "0s"; // extra 0s in the ui????????????????
    public string TimeLabel0Text { get; set; } = "0s";
    public string TimeLabel25Text { get; set; } = "...";
    public string TimeLabel50Text { get; set; } = "...";
    public string TimeLabel75Text { get; set; } = "...";
    public string TimeLabel100Text { get; set; } = "...";

    public string SampleSizeText { get; set; } = "-";
    public string MeanReturnText { get; set; } = "-";
    public string StdDevText { get; set; } = "-";

    public string LocationParamText { get; set; } = "-";
    public string ScaleParamText { get; set; } = "-";
    public string DegreesFreedomText { get; set; } = "-";

    public string KsTestStatText { get; set; } = "Test Statistic: -";
    public string StatDecisionText { get; set; } = "Decision: -";
    public string PValueText { get; set; } = "P-value: -";

    public double TimeWindowSliderMinimum { get; set; } = 0.0;
    public double TimeWindowSliderMaximum { get; set; } = 100.0;

    private bool _isInternalUpdate;

    private string _microsecondInputText = "0";
    public string MicrosecondInputText
    {
        get => _microsecondInputText;
        set
        {
            if (_microsecondInputText == value)
                return;

            _microsecondInputText = value;

            // Ignore updates coming from slider
            if (_isInternalUpdate)
                return;

            if (!int.TryParse(value, out int parsed))
                return;

            // Clamp / expand slider range (matches old behavior)
            if (parsed > TimeWindowSliderMaximum)
                TimeWindowSliderMaximum = parsed;

            // This triggers everything else
            TimeWindowSliderValue = parsed;
        }
    }

    private double _timeWindowSliderValue;
    public double TimeWindowSliderValue
    {
        get => _timeWindowSliderValue;
        set
        {
            if (Math.Abs(_timeWindowSliderValue - value) < 1e-9)
                return;

            _timeWindowSliderValue = value;

            // Prevent feedback loop
            _isInternalUpdate = true;
            MicrosecondInputText = ((int)value).ToString();
            _isInternalUpdate = false;


            RunCalculations();
        }
    }

    // Constructors

    public RightPanelViewModel() { }

    public RightPanelViewModel(List<MarketDataRow> data, OptionInfo info, string selectedSymbol)
    {

        this.Data = data;
        this.SelectedSymbol = selectedSymbol;
        this.info = info;

        MicrosecondInputText = "0";

        //RunValidationTests();
        UpdateUIFromLists();
        RunCalculations();
    }

    [Command]
    public void RunValidationTests()
    {
        TestSuite.RunAllTests();
    
        MessageBox.Show("Validation tests complete. Check Debug output.");
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

    public void OnAnalyzeAllOptionsChanged()
    {
        UpdateUIFromLists();
    }

    public void OnUseBidPricesChanged()
    {
        RunCalculations();
    }

    private void UpdateUIFromLists()
    {
        ContractSymbolText = info.Symbol;

        // Check if we have data
        if (Data == null || Data.Count == 0)
        {
            StatusIndicatorBrush = Brushes.Red;
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
                StatusIndicatorBrush = Brushes.Red;
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
            StatusIndicatorBrush = Brushes.Red;
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

        StatusIndicatorBrush = Brushes.Green;
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
        TimeWindowSliderValue = 0;
        TimeWindowSliderMaximum = unitValue;


        _maxMicroseconds = (int)(unitValue * (unit == "s" ? 1000000 : 60000000));
        _currentTimeUnit = unit;
        _timeScale = (unit == "s") ? 1 : 60;

        TimeWindowValueText = $"0 {unit}";
        MicrosecondInputText = "0";

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

/*
    public void RunCalculations()
    {
        StatusIndicatorBrush = Brushes.Red;
        try
        {

            SelectedList = UseBidPrices ? bidList : askList;

            if (SelectedList == null || SelectedList.Count < 2)
            {
                SetInsufficientData("Not enough price observations.");
                return;
            }

            // Time filtering based on slider
            double sliderValue = TimeWindowSliderValue;
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

            if (SelectedList.Count < 2)
            {
                SetInsufficientData("Time window too small.");
                return;
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

            if (ValidReturns.Length < 2)
            {
                SetInsufficientData("Insufficient returns for statistics.");
                return;
            }

            // Perform t-distribution fitting and KS test
            StudentTDistributionZeroMean tDist = new StudentTDistributionZeroMean();

            // StudentTResult tDistResult = tDist.StudentT(ValidReturns);

            // double location = tDistResult.Location;
            // double std = tDistResult.Scale;
            // double degreesFreedom = tDistResult.DegreesFreedom;

            // var tDistArr = new TDistribution(degreesFreedom);

            // double[] standardizedReturns = ValidReturns
            // .Select(x => x / std)
            // .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            // .ToArray();

            // KolmogorovSmirnovTest ks = new KolmogorovSmirnovTest(standardizedReturns, tDistArr);

            StudentTResult tDistResult = tDist.StudentT(ValidReturns);

            double location = tDistResult.Location;
            double scale = tDistResult.Scale;
            double degreesFreedom = tDistResult.DegreesFreedom;

            double[] standardizedReturns;
            KolmogorovSmirnovTest ks;
            
            standardizedReturns = ValidReturns
                .Select(x => x / scale) 
                .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
                .ToArray();

            // Handle effectively normal case
            if (double.IsInfinity(degreesFreedom))
            {
                var standardNormal = new NormalDistribution(0, 1);
                ks = new KolmogorovSmirnovTest(standardizedReturns, standardNormal);
                PlotEcdfWithTDistribution(standardizedReturns, tDistArr);

            }
            else
            {
                var tDistArr = new TDistribution(degreesFreedom);
                ks = new KolmogorovSmirnovTest(standardizedReturns, tDistArr);
                PlotEcdfWithTDistribution(standardizedReturns, tDistArr);
            
            }

            // Sample statistics
            SampleSizeText = ValidReturns.Length.ToString("N0");
            MeanReturnText = ValidReturns.Average().ToString("F6");
            StdDevText = MathNet.Numerics.Statistics.Statistics.StandardDeviation(ValidReturns).ToString("F6");

            // // Fitted t-distribution parameters
            LocationParamText = location.ToString("F6");
            ScaleParamText = scale.ToString("F6");
            DegreesFreedomText = double.IsInfinity(degreesFreedom) ? "∞" : degreesFreedom.ToString("F2");

            KSTestStatistic = ks.Statistic;
            KSTestPValue = ks.PValue;

            // // KS test results
            string statistic = $"Test Statistic: {ks.Statistic:F4}";
            string significance = $"Decision: {(ks.Significant ? "Reject H0 (Significant)" : "Fail to Reject H0")}";
            string pValue = $"P-value: {ks.PValue:E4} ";

            UpdateKsTestResults(statistic, significance, pValue);
            

            StatusIndicatorBrush = Brushes.Green;

        }
        catch (Exception ex)
        {
            StatusIndicatorBrush = Brushes.Red;
            SetInsufficientData($"Error during calculations: {ex.Message}\n{ex.StackTrace}");
            return;
        }
    }

    */
    public void RunCalculations()
    {
        InsufficientDataText = "";
        StatusIndicatorBrush = Brushes.Red;
        try
        {
            SelectedList = UseBidPrices ? bidList : askList;

            if (SelectedList == null || SelectedList.Count < 2)
            {
                SetInsufficientData("Not enough price observations.");
                return;
            }

            // Time filtering based on slider
            double sliderValue = TimeWindowSliderValue;
            if (sliderValue > 0)
            {
                TimeSpan timeWindow = TimeSpan.FromSeconds(sliderValue * _timeScale);
                DateTime endTime = SelectedList.Last().DateTime;
                DateTime cutoffTime = endTime - timeWindow;

                SelectedList = SelectedList
                    .Where(row => row.DateTime >= cutoffTime)
                    .ToList();
            }

            if (SelectedList.Count < 2)
            {
                SetInsufficientData("Time window too small.");
                return;
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

            if (ValidReturns.Length < 2)
            {
                SetInsufficientData("Insufficient returns for statistics.");
                return;
            }

            // Perform t-distribution fitting and KS test
            StudentTDistributionZeroMean tDist = new StudentTDistributionZeroMean();
            StudentTResult tDistResult = tDist.StudentT(ValidReturns);

            double location = tDistResult.Location;
            double scale = tDistResult.Scale;
            double degreesFreedom = tDistResult.DegreesFreedom;

            // Standardize returns (location is 0, so just divide by scale)
            double[] standardizedReturns = ValidReturns
                .Select(x => x / scale)
                .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
                .ToArray();

            // Handle effectively normal case vs t-distribution
            KolmogorovSmirnovTest ks;
            IUnivariateDistribution distributionForPlot;
            
            if (double.IsInfinity(degreesFreedom) || degreesFreedom > 10000)
            {
                // Data is effectively normal
                var standardNormal = new NormalDistribution(0, 1);
                ks = new KolmogorovSmirnovTest(standardizedReturns, standardNormal);
                distributionForPlot = standardNormal;
                
                // Cap for display
                if (double.IsInfinity(degreesFreedom))
                {
                    DegreesFreedomText = "∞ (Normal)";
                }
                else
                {
                    DegreesFreedomText = $"{degreesFreedom:F0} (≈ Normal)";
                }
            }
            else
            {
                // Use t-distribution
                var tDistribution = new TDistribution(degreesFreedom);
                ks = new KolmogorovSmirnovTest(standardizedReturns, tDistribution);
                distributionForPlot = tDistribution;
                DegreesFreedomText = degreesFreedom.ToString("F2");
            }

            // Sample statistics
            SampleSizeText = ValidReturns.Length.ToString("N0");
            MeanReturnText = ValidReturns.Average().ToString("F6");
            StdDevText = MathNet.Numerics.Statistics.Statistics.StandardDeviation(ValidReturns).ToString("F6");

            // Fitted t-distribution parameters
            LocationParamText = location.ToString("F6");
            ScaleParamText = scale.ToString("F6");

            KSTestStatistic = ks.Statistic;
            KSTestPValue = ks.PValue;

            // KS test results
            string statistic = $"Test Statistic: {ks.Statistic:F4}";
            string significance = $"Decision: {(ks.Significant ? "Reject H0 (Significant)" : "Fail to Reject H0")}";
            string pValue = $"P-value: {ks.PValue:E4}";

            UpdateKsTestResults(statistic, significance, pValue);
            PlotEcdfWithDistribution(standardizedReturns, distributionForPlot, degreesFreedom);

            StatusIndicatorBrush = Brushes.Green;
        }
        catch (Exception ex)
        {
            StatusIndicatorBrush = Brushes.Red;
            SetInsufficientData($"Error during calculations: Insufficient Data");
            return;
        }
    }

    public void PlotEcdfWithDistribution(double[] standardizedReturns, IUnivariateDistribution dist, double degreesFreedom)
    {
        var sortedReturns = standardizedReturns.OrderBy(x => x).ToArray();
        int n = sortedReturns.Length;

        var ecdfPoints = new List<DataPoint>();
        var distCdfPoints = new List<DataPoint>();

        for (int i = 0; i < n; i++)
        {
            double x = sortedReturns[i];
            double y = (i + 1.0) / n;
            ecdfPoints.Add(new DataPoint(x, y));
            distCdfPoints.Add(new DataPoint(x, dist.DistributionFunction(x)));
        }

        var plotModel = new PlotModel
        {
            Title = "Empirical CDF vs Fitted Distribution",
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

        // Dynamic label based on distribution type
        string distLabel;
        if (double.IsInfinity(degreesFreedom) || degreesFreedom > 10000)
        {
            distLabel = "Fitted Distribution (Normal)";
        }
        else
        {
            distLabel = $"Fitted t-Distribution (ν={degreesFreedom:F2})";
        }

        var distCdfSeries = new LineSeries
        {
            Title = distLabel,
            StrokeThickness = 2,
            Color = OxyColors.Red
        };
        distCdfSeries.Points.AddRange(distCdfPoints);

        plotModel.Series.Add(ecdfSeries);
        plotModel.Series.Add(distCdfSeries);

        ECDFPlotModel = plotModel;
        plotModel.InvalidatePlot(true);
    }

    public string InsufficientDataText { get; set; } = "";

    private void SetInsufficientData(string reason)
    {
        InsufficientDataText = reason;

        SampleSizeText = "-";
        MeanReturnText = "-";
        StdDevText = "-";
        LocationParamText = "-";
        ScaleParamText = "-";
        DegreesFreedomText = "-";
        KsTestStatText = "Test Statistic: -";
        StatDecisionText = "Decision: -";
        PValueText = "P-value: -";
        ECDFPlotModel = new PlotModel
        {
            Title = "Insufficient data"
        };
    }


    public void UpdateKsTestResults(string testStatistic, string decision, string pValue)
    {
        KsTestStatText = testStatistic;
        StatDecisionText = decision;
        PValueText = pValue;
    }
}