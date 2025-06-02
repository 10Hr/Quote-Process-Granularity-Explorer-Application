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
using OxyPlot;
using OxyPlot.SkiaSharp;

namespace ZSExplorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
/// 


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

    private async void LoadFeatherDataButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Arrow/Feather Files (*.arrow;*.feather)|*.arrow;*.feather"
        };

        if (openFileDialog.ShowDialog() == true)
        {
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

            // === Calculate sidebar values ===

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

            //PutsDataGrid.ItemsSource = putsItems;
            //CallsDataGrid.ItemsSource = callsItems;

        }
    }

    private void UpdateToolbarButtonStates()
    {
        RefreshButton.IsEnabled = fileLoaded;
        AddKsTestButton.IsEnabled = fileLoaded;
        ExportAllResults.IsEnabled = fileLoaded;
        ExportECDFPlots.IsEnabled = fileLoaded;
        ContractSearchBox.IsEnabled = fileLoaded;
        AddKsTestButton.IsEnabled = false;

    }

    private void MarketDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Column.IsReadOnly = true;
    }

    private void AddKsTest_Click(object sender, RoutedEventArgs e)
    {
        //RightPanelContainer.Content = null;

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


    private void OpenMenuItem_Click(object sender, RoutedEventArgs e) { /* logic */ }

    private void ExportMarkdownButton_Click(object sender, RoutedEventArgs e)
{
    var saveFileDialog = new SaveFileDialog
    {
        Filter = "Markdown Files (*.md)|*.md",
        DefaultExt = "md",
        FileName = "AllOptionsData.md"
    };

    if (saveFileDialog.ShowDialog() == true)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Exported Option Quotes\n");

        // Add summary
        sb.AppendLine($"- Export Date: {DateTime.Now:G}");
        sb.AppendLine($"- Total Calls: {callsItems.Count}");
        sb.AppendLine($"- Total Puts: {putsItems.Count}");
        sb.AppendLine($"- Total Quotes: {callsItems.Count + putsItems.Count}\n");

        // Markdown table header
        sb.AppendLine("## Call Quotes");
        sb.AppendLine("| Symbol | DateTime | MMID | BidAsk | Price |");
        sb.AppendLine("|--------|---------------------------|------|--------|--------|");

        foreach (var row in callsItems)
        {
            sb.AppendLine($"| {row.Symbol} | {row.DateTime:yyyy-MM-dd HH:mm:ss.ffffff} | {row.MMID} | {row.BidAsk} | {row.Price} |");
        }

        sb.AppendLine("\n## Put Quotes");
        sb.AppendLine("| Symbol | DateTime | MMID | BidAsk | Price |");
        sb.AppendLine("|--------|---------------------------|------|--------|--------|");

        foreach (var row in putsItems)
        {
            sb.AppendLine($"| {row.Symbol} | {row.DateTime:yyyy-MM-dd HH:mm:ss.ffffff} | {row.MMID} | {row.BidAsk} | {row.Price} |");
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





    //private void ExportMarkdownButton_Click(object sender, RoutedEventArgs e)
    //{

    // if (MarketDataGrid.ItemsSource is not IEnumerable<MarketDataRow> data)
    // {
    //     MessageBox.Show("No data to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
    //     return;
    // }

    // var sb = new StringBuilder();

    // // Write markdown table header
    // sb.AppendLine("| Symbol | DateTime | MMID | BidAsk | Price |");
    // sb.AppendLine("|--------|----------|------|--------|-------|");

    // // Write each row
    // foreach (var row in data)
    // {
    //     sb.AppendLine($"| {row.Symbol} | {row.DateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} | {row.MMID} | {row.BidAsk} | {row.Price} |");

    // }

    // // Save file dialog
    // var saveFileDialog = new SaveFileDialog
    // {
    //     Filter = "Markdown Files (*.md)|*.md",
    //     DefaultExt = "md",
    //     FileName = "MarketDataExport.md"
    // };

    // if (saveFileDialog.ShowDialog() == true)
    // {
    //     try
    //     {
    //         File.WriteAllText(saveFileDialog.FileName, sb.ToString());
    //         MessageBox.Show("Export completed successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
    //     }
    //     catch (Exception ex)
    //     {
    //         MessageBox.Show($"Failed to save file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //     }
    // }
    //}

    //private void ExportPlotImagesButton_Click(object sender, RoutedEventArgs e) { /* logic */ }
    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => this.Close();
    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("ZSExplorer\nVersion 1.0", "About");
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Refreshed at " + DateTime.Now.ToLongTimeString();
    }

    private void RunAnalysisButton_Click(object sender, RoutedEventArgs e)
    {
        //var selected = (MetricComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        //MessageBox.Show($"Running analysis for: {selected}");
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
