using Microsoft.Win32;
using System.Windows;
using System.Text;
using System.IO;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.SkiaSharp;

namespace ZSExplorer;

public partial class MainWindow : Window
{
    bool fileLoaded;

    public ArrowData callData, putData;
    long quoteCount;

    HashSet<string> uniqueSymbols;
    HashSet<string> uniqueExchanges;

    List<MarketDataRow> callsItems;
    List<MarketDataRow> putsItems;

    public MainWindow()
    {
        callData = new ArrowData();
        putData = new ArrowData();
        fileLoaded = false;
        InitializeComponent();
        UpdateToolbarButtonStates();
    }

    // ========= Update ToolBar Buttons =========
    private void UpdateToolbarButtonStates()
    {
        AddKsTestButton.IsEnabled = fileLoaded;
        ExportAllResults.IsEnabled = fileLoaded;
        ExportECDFPlots.IsEnabled = fileLoaded;
        ContractSearchBox.IsEnabled = fileLoaded;
        AddKsTestButton.IsEnabled = false;

    }

    // ========= Button Event Handlers =========

    private async void LoadFeatherDataButton_Click(object sender, RoutedEventArgs e)
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
                FileLoadSummaryText.Text = "Loading...";

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                (callData, putData) = await ArrowDataLoader.LoadArrowFileAsync(filePath);
                stopwatch.Stop();
                double seconds = stopwatch.Elapsed.TotalSeconds;
                quoteCount = callData.Symbol.Count + putData.Symbol.Count;

                fileLoaded = true;
                UpdateToolbarButtonStates();


                FileLoadSummaryText.Text = $"Loaded file: {Path.GetFileName(filePath)} with {quoteCount} rows in {seconds:F2} seconds";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ========= Calculate sidebar values =========

            // 1. Total Quote Count
            TotalQuoteCountText.Text = $"Total Quote Count: {quoteCount}";

            // 2. Date Range (min and max)

            var callsDateTimes = callData.DateTime;
            var putsDateTimes = putData.DateTime;

            DateTime callsMin = callsDateTimes.First();
            DateTime callsMax = callsDateTimes.Last();

            DateTime putsMin = putsDateTimes.First();
            DateTime putsMax = putsDateTimes.Last();

            DateTime minDate = callsMin < putsMin ? callsMin : putsMin;
            DateTime maxDate = callsMax > putsMax ? callsMax : putsMax;

            DateRangeText.Text = $"Date Range: {minDate:G} - {maxDate:G}";

            // 3. Fill Data Grids | Unique Contract Count (unique "sybmol") | Exchange Count (unique "MMID")

            uniqueSymbols = new HashSet<string>();
            uniqueExchanges = new HashSet<string>();

            callsItems = new List<MarketDataRow>();
            putsItems = new List<MarketDataRow>();

            for (int i = 0; i < putData.Symbol.Count; i++)
            {
                uniqueSymbols.Add(putData.Symbol[i].ToString());
                uniqueExchanges.Add(putData.MMID[i].ToString());

                putsItems.Add(new MarketDataRow
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
                uniqueSymbols.Add(callData.Symbol[i].ToString());
                uniqueExchanges.Add(callData.MMID[i].ToString());

                callsItems.Add(new MarketDataRow
                {
                    Symbol = callData.Symbol[i],
                    DateTime = callData.DateTime[i],
                    MMID = callData.MMID[i],
                    BidAsk = callData.BidAsk[i],
                    Price = callData.Price[i]
                });
            }
            ContractSearchBox.ItemsSource = uniqueSymbols.OrderBy(s => s).ToList();

            UniqueContractCountText.Text = $"Unique Contract Count: {uniqueSymbols.Count}";
            ExchangeCountText.Text = $"Exchange Count: {uniqueExchanges.Count}";

            PutsDataGrid.ItemsSource = putsItems;
            CallsDataGrid.ItemsSource = callsItems;

        }
    }

    private void MarketDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Column.IsReadOnly = true;
    }

    private void AddKsTest_Click(object sender, RoutedEventArgs e)
    {
        if (RightPanelContainer.Content == null)
        {
            RightPanelContainer.Content = new RightPanel(callsItems, putsItems, (string)ContractSearchBox.SelectedItem);
        }
    }

    public void RemoveKsTest()
    {
         var result = MessageBox.Show("Are you sure you want to remove the KS test and reset the panel?",
                                "Confirm Removal",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            RightPanelContainer.Content = null;      
        }

    }

    private void ExportMarkdownButton_Click(object sender, RoutedEventArgs e)
    {

        if (RightPanelContainer.Content is not RightPanel panel)
        {
            MessageBox.Show("No plots to export. Please run an analysis first.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Markdown Files (*.md)|*.md",
            DefaultExt = "md",
            FileName = "AllOptionsLogReturnData.md"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var sb = new StringBuilder();

            int nPrices = panel.SelectedList.Count;
            int nReturns = panel.ValidReturns.Length;

            sb.AppendLine("# Exported Option Quotes\n");

            // Add summary
            sb.AppendLine($"- Export Date: {DateTime.Now:G}");
            sb.AppendLine($"- Total Quotes: {nPrices}\n");
            sb.AppendLine();
            sb.AppendLine("# KS Test Results");
            sb.AppendLine();
            sb.AppendLine($"- KS Statistic: {panel.KSTestStatistic:F4}");
            sb.AppendLine($"- P-Value: {panel.KSTestPValue:E4}");
            sb.AppendLine();

            sb.AppendLine("## Prices and Log Returns");
            sb.AppendLine();
            sb.AppendLine("| Index | Price    | Log Return |");
            sb.AppendLine("|-------|----------|------------|");



            for (int i = 0; i < nPrices; i++)
            {
                string indexStr = i.ToString().PadLeft(6);
                string priceStr = panel.SelectedList[i].Price.ToString("G6").PadLeft(8).PadRight(1);
                string logReturnStr = (i == 0 || i - 1 >= nReturns) ? "".PadLeft(13) : panel.ValidReturns[i - 1].ToString("G6").PadLeft(10);

                sb.AppendLine($"|{indexStr} |{priceStr} |{logReturnStr} |");
            }

            try
            {
                File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                MessageBox.Show("Markdown export completed successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


    private void ExportPlotImagesButton_Click(object sender, RoutedEventArgs e)
    {
        if (RightPanelContainer.Content is not RightPanel panel)
        {
            MessageBox.Show("No plots to export. Please run an analysis first.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "PNG Image (*.png)|*.png",
            DefaultExt = "png",
            FileName = "ECDFPlot.png"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var plotModel = panel.ECDFPlotModel; 
                plotModel.Background = OxyColors.White;
                using var stream = File.Create(saveFileDialog.FileName);
                var exporter = new PngExporter { Width = 600, Height = 400};
                exporter.Export(plotModel, stream);

                MessageBox.Show("Plot exported successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export plot: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ContractSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ContractSearchBox.SelectedItem is string selectedSymbol)
        {
            StatusTextBlock.Text = $"Selected Contract: {selectedSymbol}";
            AddKsTestButton.IsEnabled = fileLoaded && !string.IsNullOrWhiteSpace(selectedSymbol);
        }
        else
        {
            AddKsTestButton.IsEnabled = false;
        }
    }


}
