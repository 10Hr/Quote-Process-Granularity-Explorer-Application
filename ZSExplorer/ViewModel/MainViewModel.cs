using System.Collections.ObjectModel;
using System.Windows.Input;
using Metalama.Patterns.Observability;
using Microsoft.Win32;
using System.Windows;
using System.Text;
using System.IO;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.SkiaSharp;
using Accord.Statistics.Distributions.Univariate;
using Metalama.Patterns.Wpf;
using ZSExplorer.Services;

namespace ZSExplorer;

[Observable]
public partial class MainViewModel
{
    public bool FileLoaded { get; set; } = false;

    public long QuoteCount { get; set; }  = 0;

    private string _selectedSymbol = "";
    
    public string SelectedSymbol 
    { 
        get => _selectedSymbol;
        set
        {
            if (_selectedSymbol != value)
            {
                _selectedSymbol = value;
                StatusText = $"Selected Contract: {value}";
            }
        }
    }

    public string DateRange { get; set; } = "";

    public string FileLoadSummary { get; set; } = "";

    public string StatusText { get; set; } = "Ready";

    // Sidebar Stats
    public int UniqueContractCount { get; set; } = 0;
    public int ExchangeCount { get; set; } = 0;

    // Dropdown list
    public ObservableCollection<string> UniqueSymbols { get; set; } = new();
    public ObservableCollection<string> UniqueExchanges { get; set; } = new();

    // DataGrids
    public ObservableCollection<MarketDataRow> Calls { get; } = new();
    public ObservableCollection<MarketDataRow> Puts  { get; } = new();

    //Local Variables

    public ArrowData callData, putData;

    // Right View Model

    public RightPanelViewModel RVM { get; set; }

    public RightPanel? RightPanelContainer { get; set; }


    // Constructors

    public MainViewModel() {}

    // Commands

    [Command]
    public async Task LoadFeatherData()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Arrow/Feather Files (*.arrow;*.feather)|*.arrow;*.feather"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            // ========= Load Arrow File =========

            string filePath = openFileDialog.FileName;

            try
            {
                StatusText = "Loading...";

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                (callData, putData) = await ArrowDataLoader.LoadArrowFileAsync(filePath);
                stopwatch.Stop();
                double seconds = stopwatch.Elapsed.TotalSeconds;
                QuoteCount = callData.Symbol.Count + putData.Symbol.Count;

                Calls.Clear();
                Puts.Clear();
                UniqueSymbols.Clear();

                FileLoaded = true;
                FileLoadSummary = $"Loaded file: {Path.GetFileName(filePath)} with {QuoteCount} rows in {seconds:F2} seconds";


                // ========= Calculate sidebar values =========

                // 1. Date Range (min and max)

                var callsDateTimes = callData.DateTime;
                var putsDateTimes = putData.DateTime;

                DateTime callsMin = callsDateTimes.First();
                DateTime callsMax = callsDateTimes.Last();

                DateTime putsMin = putsDateTimes.First();
                DateTime putsMax = putsDateTimes.Last();

                DateTime minDate = callsMin < putsMin ? callsMin : putsMin;
                DateTime maxDate = callsMax > putsMax ? callsMax : putsMax;

                DateRange = $"{minDate:G} - {maxDate:G}";

                // 2. Fill Data Grids | Unique Contract Count (unique "sybmol") | Exchange Count (unique "MMID")

                // Use local HashSets for fast adding
                var tempSymbols = new HashSet<string>();
                var tempExchanges = new HashSet<string>();

                for (int i = 0; i < putData.Symbol.Count; i++)
                {
                    tempSymbols.Add(putData.Symbol[i].ToString());
                    tempExchanges.Add(putData.MMID[i].ToString());

                    Puts.Add(new MarketDataRow
                    {
                        Symbol = putData.Symbol[i],
                        DateTime = putData.DateTime[i],
                        MMID = putData.MMID[i],
                        BidAsk = putData.BidAsk[i],
                        Price = putData.Price[i]
                    });
                }

                for (int i = 0; i < callData.Symbol.Count; i++)
                {
                    tempSymbols.Add(callData.Symbol[i].ToString());
                    tempExchanges.Add(callData.MMID[i].ToString());

                    Calls.Add(new MarketDataRow
                    {
                        Symbol = callData.Symbol[i],
                        DateTime = callData.DateTime[i],
                        MMID = callData.MMID[i],
                        BidAsk = callData.BidAsk[i],
                        Price = callData.Price[i]
                    });
                }

                UniqueSymbols.Clear();
                UniqueExchanges.Clear();

                foreach (var symbol in tempSymbols.OrderBy(s => s))
                    UniqueSymbols.Add(symbol);
                
                foreach (var exchange in tempExchanges.OrderBy(e => e))
                    UniqueExchanges.Add(exchange);

                SelectedSymbol = UniqueSymbols.FirstOrDefault() ?? "";
                UniqueContractCount = UniqueSymbols.Count;
                ExchangeCount = callData.MMID.Concat(putData.MMID).Select(m => m.ToString()).Distinct().Count();

                FileLoaded = true;

                StatusText = $"Selected Contract: {SelectedSymbol}";//$"Loaded {Path.GetFileName(openFileDialog.FileName)} ({QuoteCount:N0} rows in {seconds:F2}s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "File Loading Failed";
                return;
            }
        }
    }

    [Command]
    private void AddKsTest()
    {
        if (RightPanelContainer != null)
            return;
        
        OptionInfo info = ParseOptionsSymbol.Parse(SelectedSymbol);

        var panel = info.OptionType switch
        {
            "Call" => new RightPanel(Calls.ToList(), info, SelectedSymbol),
            "Put"  => new RightPanel(Puts.ToList(),  info, SelectedSymbol),
            _ => throw new ArgumentException("Invalid contract type.")
        };

        panel.RequestClose += () => RightPanelContainer = null;

        RightPanelContainer = panel;
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
            RightPanelContainer = null;      
        }
    }

    private void ExportMarkdownButton_Click(object sender, RoutedEventArgs e)
    {

        // if (RightPanelContainer.Content is not RightPanel panel)
        // {
        //     MessageBox.Show("No data to export. Please run an analysis first.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
        //     return;
        // }

        // var saveFileDialog = new SaveFileDialog
        // {
        //     Filter = "Markdown Files (*.md)|*.md",
        //     DefaultExt = "md",
        //     FileName = "AllOptionsLogReturnData.md"
        // };

        // if (saveFileDialog.ShowDialog() == true)
        // {
        //     var sb = new StringBuilder();

        //     int nPrices = panel.SelectedList.Count;
        //     int nReturns = panel.ValidReturns.Length;

        //     sb.AppendLine("# Exported Option Quotes\n");

        //     // Add summary
        //     sb.AppendLine($"- Export Date: {DateTime.Now:G}");
        //     sb.AppendLine($"- Total Quotes: {nPrices}\n");
        //     sb.AppendLine();
        //     sb.AppendLine("# KS Test Results");
        //     sb.AppendLine();
        //     sb.AppendLine($"- KS Statistic: {panel.KSTestStatistic:F4}");
        //     sb.AppendLine($"- P-Value: {panel.KSTestPValue:E4}");
        //     sb.AppendLine();

        //     sb.AppendLine("## Prices and Log Returns");
        //     sb.AppendLine();
        //     sb.AppendLine("| Index | Price    | Log Return |");
        //     sb.AppendLine("|-------|----------|------------|");



        //     for (int i = 0; i < nPrices; i++)
        //     {
        //         string indexStr = i.ToString().PadLeft(6);
        //         string priceStr = panel.SelectedList[i].Price.ToString("G6").PadLeft(8).PadRight(1);
        //         string logReturnStr = (i == 0 || i - 1 >= nReturns) ? "".PadLeft(13) : panel.ValidReturns[i - 1].ToString("G6").PadLeft(10);

        //         sb.AppendLine($"|{indexStr} |{priceStr} |{logReturnStr} |");
        //     }

        //     try
        //     {
        //         File.WriteAllText(saveFileDialog.FileName, sb.ToString());
        //         MessageBox.Show("Markdown export completed successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        //     }
        //     catch (Exception ex)
        //     {
        //         MessageBox.Show($"Failed to save file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //     }
        // }
    }


    private void ExportPlotImagesButton_Click(object sender, RoutedEventArgs e)
    {
        // if (RightPanelContainer.Content is not RightPanel panel)
        // {
        //     MessageBox.Show("No plots to export. Please run an analysis first.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
        //     return;
        // }

        // var saveFileDialog = new SaveFileDialog
        // {
        //     Filter = "PNG Image (*.png)|*.png",
        //     DefaultExt = "png",
        //     FileName = "ECDFPlot.png"
        // };

        // if (saveFileDialog.ShowDialog() == true)
        // {
        //     try
        //     {
        //         var plotModel = panel.ECDFPlotModel; 
        //         plotModel.Background = OxyColors.White;
        //         using var stream = File.Create(saveFileDialog.FileName);
        //         var exporter = new PngExporter { Width = 600, Height = 400};
        //         exporter.Export(plotModel, stream);

        //         MessageBox.Show("Plot exported successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        //     }
        //     catch (Exception ex)
        //     {
        //         MessageBox.Show($"Failed to export plot: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //     }
        // }
    }
}
