   M S B A M L   `   `   `  ���� E  AZSExplorer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ZSExplorer
ZSExplorer      ZSExplorer.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     *localclr-namespace:ZSExplorer       � Title$  
ZSExplorer��$	��600��$
��1000��.��]� .+�� $	��Top=�   PrimaryBrush#��[  $��40��.+�� $
��Auto�� � ColumnDefinitions �� $��* ��� $
��Auto �.+���  �� Orientation$ 
Horizontal=�$��0��$��Center=�$��8,0,10,0q�.+��� $��Add Contract: ��$
��Bold"�$��14(�  ButtonTextBrush#��[ $��Center=��� -    ��Name$ ContractSearchBox��$	��200��$��8,0,0,0q�$��26�� �� 
IsEditable$ False�� �� IsTextSearchEnabled$
 True�� �� StaysOpenOnEdit$
 True��${�False��$��Center=��� $��1��$��Right=�$��Center=�$��0,0,8,0q�.+��� -    ��Name$ AddKsTestButton��$��Add KS Test���� $��Bottom=�.@��� .���� -    ��Name$ StatusTextBlock��$��Ready��� $	��Top=�$��5q� �� $��2* ��� $��3* �.+��� $��0��$��10q�
�������$��2q�	 �� CornerRadius$	 6��
 �� Padding$
 10q�._��  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0��
�������$��1q�$	 4��$
 10q�$��0,0,0,20q�._��� .+��� $��Data Summary��$
��Bold"�$��16(�$��0,0,0,10q��� -   $ TotalQuoteCountText��$��Total Quote Count: 0���� -   $ DateRangeText��$��Date Range: N/A���� -   $ UniqueContractCountText��$��Unique Contract Count: 0���� -   $ ExchangeCountText��$��Exchange Count: 0���� $��20��
������ �� -   $��Load Feather Data��$	��150��$��35���� -	   $ FileLoadSummaryText��$��No file loaded.��$��0,10,0,0q�$
��Wrap=��� $��2��
�������$��1q�$	 4��$
 10q�._��� $	��Top=�.+��� $��Global Settings��$
��Bold"�$��16(�$��0,0,0,10q��� $ 
Horizontal=�$��0,0,0,5q�.+��� $��Distribution Method:��$��SemiBold"�$	��140���� $��Regula Falsi Method���� $ 
Horizontal=�$��0,0,0,15q�.+��� $��Alpha Level:��$��SemiBold"�$	��140���� $��α = 0.05 (Fixed)���� $��3��
�������$��1q�$	 4��$
 10q�._��� $	��Top=�.+��� $��Export Options��$
��Bold"�$��16(�$��0,0,0,10q��� $ 
Horizontal=�$
��Left=�$��0q�.+��� -
   $ ExportAllResults��$��Export All Results (md)��$
��Auto��$��30��$��0,0,10,0q��� -   $ ExportECDFPlots��$��Export ECDF Plots (png)��$
��Auto��$��30���� $��4��
�������$��1q�$	 4��$
 10q�._�� $��10q� �� $��1* ��� $��1* � �� $
��Auto ��� $��* �.+��� $��0��$��0��$��	Call Data��$��16(�$
��Bold"����	#FF2C3E50$��Center=�$��0,0,0,5q��� $��1��$��0��$��Put Data��$��16(�$
��Bold"����	#FF2C3E50$��Center=�$��0,0,0,5q�&   System.Windows.Controls.DataGrid  -     Name$ CallsDataGrid��$��0��$��1��   AutoGenerateColumns$ False��   CanUserAddRows$ False��$��0,0,10,0q�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Symbol��   
IsReadOnly$
 True��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Symbol+  $ DateTime��$
 True�� �� *
DateTime+ �� StringFormat$  MM/dd/yyyy HH:mm:ss.ffffff��  $
 MMID��$
 True�� �� *MMID+4  .System.Windows.Controls.DataGridCheckBoxColumn  $ BidAsk��$
 True�� �� *BidAsk+  $ Price��$
 True�� �� *Price+  -   $ PutsDataGrid��$��1��$��1��$ False��$ False��   $ Symbol��$
 True�� �� *Symbol+  $ DateTime��$
 True�� �� *
DateTime+$  MM/dd/yyyy HH:mm:ss.ffffff��  $
 MMID��$
 True�� �� *MMID+  $ BidAsk��$
 True�� �� *BidAsk+  $ Price��$
 True�� �� *Price+�� -   $��RightPanelContainer��$��1��$��10q�