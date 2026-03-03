using System.Windows;
using System.Windows.Controls;

namespace ZSExplorer;

public partial class MainWindow : Window
{
    public MainViewModel VM { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = VM;
    }

    // ========= Button Event Handlers =========
    
    private void MarketDataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Column.IsReadOnly = true;
    }
}
