using System.Windows;
using System.Windows.Controls;
using ZSExplorer.Services;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Testing;
using System.Windows.Input;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Metalama.Patterns.Observability;
using System.Collections.ObjectModel;

namespace ZSExplorer
{
    public partial class RightPanel : UserControl
    {
        public RightPanelViewModel? RVM { get; set; }

        public event Action? RequestClose;

        public RightPanel() { InitializeComponent(); }

        public RightPanel(List<MarketDataRow> data, OptionInfo info, string selectedSymbol) : this() 
        {
            if (data == null)
            {
                return;
            }

            RVM = new RightPanelViewModel(data, info, selectedSymbol);
            RVM.RequestClose += () => RequestClose?.Invoke();

            DataContext = RVM;
        }
    }
}
