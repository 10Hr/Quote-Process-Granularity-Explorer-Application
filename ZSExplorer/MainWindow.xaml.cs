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
            RightPanelContainer.Content = new RightPanel(callsItems, (string)ContractSearchBox.SelectedItem);
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
    }

    private void ExportPlotImagesButton_Click(object sender, RoutedEventArgs e) { /* logic */ }
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
