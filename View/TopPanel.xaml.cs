using Microsoft.Win32;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ZSExplorer
{
    public partial class TopPanel : UserControl
    {
        public DataFrame df;

        public TopPanel()
        {
            InitializeComponent();
            UpdateToolbarButtonStates();
        }

        private void UpdateToolbarButtonStates()
        {
            RefreshButton.IsEnabled = true;
            AddKsTestButton.IsEnabled = true;
        }


        private void RefreshButton_Click(object sender, RoutedEventArgs e) { }
        
        private void AddKsTest_Click(object sender, RoutedEventArgs e) { }

      
    }
}
