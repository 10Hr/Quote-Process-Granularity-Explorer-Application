using Microsoft.Win32;
using Microsoft.Data.Analysis;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System.IO;
using Apache.Arrow.Ipc;
using System.Windows.Controls;
using System.Linq;

namespace ZSExplorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
   DataFrame df;

    public MainWindow()
    {
        df = new DataFrame();
        InitializeComponent();
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
        df = await ArrowDataLoader.LoadArrowFileAsync(filePath);

            //return;
        var sb = new StringBuilder();

        sb.AppendLine("sybmol\t\tdatetime\t\t\tMMID\tBidAsk\tPrice");



            for (long i = 0; i < df.Rows.Count; i++)
            {
                var symbol = df.Columns["sybmol"][i];
                var datetime = df.Columns["datetime"][i];
                var mmid = df.Columns["MMID"][i];
                var bidAsk = df.Columns["BidAsk"][i];
                var price = df.Columns["Price"][i];

                sb.AppendLine($"{symbol}\t{datetime}\t{mmid}\t{bidAsk}\t{price}");
            }


        MessageBox.Show(sb.ToString(), "DataFrame Preview");
    }
}


    // private async void LoadFeatherDataButton_Click(object sender, RoutedEventArgs e)
    // {
    //      var openFileDialog = new OpenFileDialog
    //     {
    //         Filter = "Arrow/Feather Files (*.arrow;*.feather)|*.arrow;*.feather"
    //     };

    //     if (openFileDialog.ShowDialog() == true)
    //     {
    //         string filePath = openFileDialog.FileName;
    //         df = await ArrowDataLoader.LoadArrowFileAsync(filePath);

    //         //                 var symbolCol = df["sybmol"];
    //         //                 var datetimeCol = df["datetime"];
    //         //                 var mmidCol = df["MMID"];
    //         //                 var bidAskCol = df["BidAsk"];
    //         //                 var priceCol = df["Price"];

    //         MessageBox.Show($"{df["sybmol"]} {df["datetime"]} {df["MMID"]}  {df["Price"]}  ", "DataFrame Loaded");



    //     }
    // }


    private void AddKsTest_Click(object sender, RoutedEventArgs e) { }


    private void OpenMenuItem_Click(object sender, RoutedEventArgs e) { /* logic */ }
    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => this.Close();
    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("KSExplorer\nVersion 1.0", "About");
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Refreshed at " + DateTime.Now.ToLongTimeString();
    }

    private void RunAnalysisButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = (MetricComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        MessageBox.Show($"Running analysis for: {selected}");
    }


}


