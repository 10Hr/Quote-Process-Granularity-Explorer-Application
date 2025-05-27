using Microsoft.Win32;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ZSExplorer
{
    public partial class RightPanel : UserControl
    {

        public RightPanel()
        {
            InitializeComponent();

        }

        private void MarketDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;
        }

      
    }
}
