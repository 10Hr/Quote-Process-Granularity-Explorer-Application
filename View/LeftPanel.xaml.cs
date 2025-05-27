using Microsoft.Win32;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ZSExplorer
{
    public partial class LeftPanel : UserControl
    {

        DataFrame df;
        bool fileLoaded;

        public LeftPanel()
        {
            df = new DataFrame();
            fileLoaded = false;
            InitializeComponent();
            UpdateToolbarButtonStates();
        }
        
        private void UpdateToolbarButtonStates()
        {
            ExportAllResults.IsEnabled = fileLoaded;
            ExportECDFPlots.IsEnabled = fileLoaded;
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
                    df = await ArrowDataLoader.LoadArrowFileAsync(filePath);
                    fileLoaded = true;
                    UpdateToolbarButtonStates();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }



                // MessageBox.Show(sb.ToString(), "DataFrame Preview");

                // === Calculate sidebar values ===
                // 1. Total Quote Count
                TotalQuoteCountText.Text = $"Total Quote Count: {df.Rows.Count}";

                // 2. Date Range (min and max)
                var datetimes = df.Columns["datetime"];
                // parse datetimes to DateTime (assuming column stores string or DateTime objects)
                List<DateTime> dateList = new List<DateTime>();
                foreach (var val in datetimes)
                {
                    if (DateTime.TryParse(val.ToString(), out DateTime dt))
                        dateList.Add(dt);
                }
                if (dateList.Count > 0)
                {
                    var minDate = dateList.Min();
                    var maxDate = dateList.Max();
                    DateRangeText.Text = $"Date Range: {minDate:G} - {maxDate:G}";
                }
                else
                {
                    DateRangeText.Text = "Date Range: N/A";
                }

                // 3. Unique Contract Count (unique "sybmol")
                var symbols = df.Columns["sybmol"];
                var uniqueSymbols = new HashSet<string>();
                for (long i = 0; i < symbols.Length; i++)
                {
                    uniqueSymbols.Add(symbols[i]?.ToString());
                }
                UniqueContractCountText.Text = $"Unique Contract Count: {uniqueSymbols.Count}";

                // 4. Exchange Count (unique "MMID")
                var mmids = df.Columns["MMID"];
                var uniqueExchanges = new HashSet<string>();
                for (long i = 0; i < mmids.Length; i++)
                {
                    uniqueExchanges.Add(mmids[i]?.ToString());
                }
                ExchangeCountText.Text = $"Exchange Count: {uniqueExchanges.Count}";

                // Update file load summary text
                FileLoadSummaryText.Text = $"Loaded file: {System.IO.Path.GetFileName(filePath)} with {df.Rows.Count} rows.";


                // ======================== DATA GRID ========================

                var dataList = new List<MarketDataRow>();

                for (long i = 0; i < df.Rows.Count; i++)
                {
                    var row = new MarketDataRow
                    {
                        Symbol = (string)df.Columns["sybmol"][i],
                        DateTime = (DateTime)df.Columns["datetime"][i],
                        MMID = (string)df.Columns["MMID"][i],
                        BidAsk = (Boolean)df.Columns["BidAsk"][i],
                        Price = (long)df.Columns["Price"][i]

                    };
                    dataList.Add(row);
                }


                // Bind to DataGrid
                MarketDataGrid.ItemsSource = dataList;
            }
        }

        private void ExportMarkdownButton_Click(object sender, RoutedEventArgs e) { /* logic */ }

        private void ExportPlotImagesButton_Click(object sender, RoutedEventArgs e) { /* logic */ }

      
    }
}
