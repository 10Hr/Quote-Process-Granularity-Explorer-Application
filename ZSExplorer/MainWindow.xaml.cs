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

namespace ZSExplorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
/// 


public partial class MainWindow : Window
{
    //DataFrame df;
    bool fileLoaded;

    ArrowData callData, putData;
    long quoteCount;

    public MainWindow()
    {
        //df = new DataFrame();
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

                (callData, putData) = await ArrowDataLoader.LoadArrowFileAsync(filePath);
                quoteCount = callData.Symbol.Count + putData.Symbol.Count;



                FileLoadSummaryText.Text = $"Loaded file: {System.IO.Path.GetFileName(filePath)} with {quoteCount} rows.";
                MessageBox.Show($"Loaded file: with {callData.Symbol.Count} call rows and {putData.Symbol.Count} put rows.", "File Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                //MessageBox.Show($"{callData.Symbol.Count} {callData.DateTime.Count} {callData.MMID.Count} {callData.BidAsk.Count} {callData.Price.Count} call rows, {putData.Symbol.Count} {putData.DateTime.Count} {putData.MMID.Count} {putData.BidAsk.Count} {putData.Price.Count} put rows", "File Loaded", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            fileLoaded = true;
            UpdateToolbarButtonStates();





            // === Calculate sidebar values ===
            // 1. Total Quote Count
            TotalQuoteCountText.Text = $"Total Quote Count: {quoteCount}";

            // // 2. Date Range (min and max)

            // var datetimeColumn = df.Columns["datetime"] as PrimitiveDataFrameColumn<DateTime>;

            // if (datetimeColumn != null && datetimeColumn.Length > 0)
            // {
            //     var minDate = datetimeColumn.Min();
            //     var maxDate = datetimeColumn.Max();
            //     DateRangeText.Text = $"Date Range: {minDate:G} - {maxDate:G}";
            // }
            // else
            // {
            //     DateRangeText.Text = "Date Range: N/A";
            // }
            
        var callsDateTimes = callData.DateTime;
        var putsDateTimes = putData.DateTime;

        DateTime callsMin = callsDateTimes.First();
        DateTime callsMax = callsDateTimes.Last();

        DateTime putsMin = putsDateTimes.First();
        DateTime putsMax = putsDateTimes.Last();
        
        MessageBox.Show($"Call Date Range: {callsMin:G} - {callsMax:G}\nPut Date Range: {putsMin:G} - {putsMax:G}", "Date Ranges", MessageBoxButton.OK, MessageBoxImage.Information);

        // Find overall min and max
            DateTime minDate = callsMin < putsMin ? callsMin : putsMin;
        DateTime maxDate = callsMax > putsMax ? callsMax : putsMax;

        DateRangeText.Text = $"Date Range: {minDate:G} - {maxDate:G}";


        List<MarketDataRow> items = new List<MarketDataRow>();

        // Assuming you have some data source like your ArrowData:
        for (int i = 0; i < putData.Symbol.Count; i++)
        {
            items.Add(new MarketDataRow
            {
                Symbol = putData.Symbol[i],
                DateTime = putData.DateTime[i],
                MMID = putData.MMID[i],
                BidAsk = putData.BidAsk[i],
                Price = putData.Price[i]
            });
        }

        // Set the ItemsSource of the DataGrid:
        MarketDataGrid.ItemsSource = items;


            // var datetimes = df.Columns["datetime"];
            // // parse datetimes to DateTime (assuming column stores string or DateTime objects)
            // List<DateTime> dateList = new List<DateTime>();
            // foreach (var val in datetimes)
            // {
            //     if (DateTime.TryParse(val.ToString(), out DateTime dt))
            //         dateList.Add(dt);
            // }
            // if (dateList.Count > 0)
            // {
            //     var minDate = dateList.Min();
            //     var maxDate = dateList.Max();
            //     DateRangeText.Text = $"Date Range: {minDate:G} - {maxDate:G}";
            // }
            // else
            // {
            //     DateRangeText.Text = "Date Range: N/A";
            // }







            // 3. Unique Contract Count (unique "sybmol")
            // var symbols = df.Columns["sybmol"];
            // var uniqueSymbols = new HashSet<string>();
            // for (long i = 0; i < symbols.Length; i++)
            // {
            //     uniqueSymbols.Add(symbols[i]?.ToString());
            // }
            // UniqueContractCountText.Text = $"Unique Contract Count: {uniqueSymbols.Count}";

            // // 4. Exchange Count (unique "MMID")
            // var mmids = df.Columns["MMID"];
            // var uniqueExchanges = new HashSet<string>();
            // for (long i = 0; i < mmids.Length; i++)
            // {
            //     uniqueExchanges.Add(mmids[i]?.ToString());
            // }
            // ExchangeCountText.Text = $"Exchange Count: {uniqueExchanges.Count}";

            // // Update file load summary text
            // FileLoadSummaryText.Text = $"Loaded file: {System.IO.Path.GetFileName(filePath)} with {df.Rows.Count} rows.";


            // // ======================== DATA GRID ========================

            // var dataList = new List<MarketDataRow>();

            // for (long i = 0; i < df.Rows.Count; i++)
            // {
            //     var row = new MarketDataRow
            //     {
            //         Symbol = (string)df.Columns["sybmol"][i],
            //         DateTime = (DateTime)df.Columns["datetime"][i],
            //         MMID = (string)df.Columns["MMID"][i],
            //         BidAsk = (Boolean)df.Columns["BidAsk"][i],
            //         Price = (long)df.Columns["Price"][i]

            //     };
            //     dataList.Add(row);
            // }



            // //Bind to DataGrid
            // MarketDataGrid.ItemsSource = dataList;
        }
    }

    private void UpdateToolbarButtonStates()
    {
        RefreshButton.IsEnabled = fileLoaded;
        AddKsTestButton.IsEnabled = fileLoaded;
        ExportAllResults.IsEnabled = fileLoaded;
        ExportECDFPlots.IsEnabled = fileLoaded;
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
            //RightPanelContainer.Content = new RightPanel(df);
        }

    }

    public void RemoveKsTest()
    {
        RightPanelContainer.Content = null;
    }


    private void OpenMenuItem_Click(object sender, RoutedEventArgs e) { /* logic */ }

    private void ExportMarkdownButton_Click(object sender, RoutedEventArgs e)
    {

        if (MarketDataGrid.ItemsSource is not IEnumerable<MarketDataRow> data)
        {
            MessageBox.Show("No data to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sb = new StringBuilder();

        // Write markdown table header
        sb.AppendLine("| Symbol | DateTime | MMID | BidAsk | Price |");
        sb.AppendLine("|--------|----------|------|--------|-------|");

        // Write each row
        foreach (var row in data)
        {
            sb.AppendLine($"| {row.Symbol} | {row.DateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} | {row.MMID} | {row.BidAsk} | {row.Price} |");

        }

        // Save file dialog
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Markdown Files (*.md)|*.md",
            DefaultExt = "md",
            FileName = "MarketDataExport.md"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                MessageBox.Show("Export completed successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
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


}


public class MarketDataRow
{
    public string Symbol { get; set; }
    public DateTime DateTime { get; set; }
    public string MMID { get; set; }
    public bool BidAsk { get; set; }
    public long Price { get; set; }
}