   M S B A M L   `   `   `  ����E  AZSExplorer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ZSExplorer
ZSExplorer  5   	       ZSExplorer.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:ZSExplorer     5   	     � Title$  
ZSExplorer��5   	   $	��600��6   $
��1000��6)   .��5      ]� .+�5      � $	��Top=�6      PrimaryBrush#��[  6    $��40��6L   .+�5      � $
��Auto��6    � ColumnDefinitions 5      �� 5      $��* �6#   �� 5      $
��Auto �6#   .+�5      ��  �� Orientation$ 
Horizontal=�6   $��0��62   $��Center=�6B   $��8,0,10,0q�6]   .+�5      �� $��Add Contract: ��6   $
��Bold"�5      $��14(�5        ButtonTextBrush#��[ 5      $��Center=�5      �� 5      -   5         ��Name$ ContractSearchBox��5      $	��200��5      $��8,0,0,0q�5      $��26��5       �� 
IsEditable$ False��5        �� IsTextSearchEnabled$
 True��5!       �� StaysOpenOnEdit$
 True��5"      ${�False��5#      $��Center=�5$      5%      5&      �� 5(      $��1��6   $��Right=�6)   $��Center=�6E   $��0,0,8,0q�6`   .+�5)      �� -   5         ��Name$ AddKsTestButton��5)      $��Add KS Test��62   6H   5*      5+      5,      �� 5/   
   $��Bottom=�6   .@�50      �� .��51      �� -   5         ��Name$ StatusTextBlock��51      $��Ready��65   52      53      � 56   
   $	��Top=�6   $��5q�6$    57      �� 58      $��2* �6#   �� 59      $��3* �6#   .+�5=      �� $��0��6   $��10q�6%   
�������$��2q�6D   	 �� CornerRadius$	 6��6X   
 �� Padding$
 10q�6i   ._�5>      �  � RowDefinitions 5?      �� 5@      $
��Auto �6(   �� 5A      $
��Auto �6(   �� 5B      $
��Auto �6(   �� 5C      $
��Auto �6(   �� 5D      $��* �6(   .+�5H      �� $��0��6   
�������$��1q�6>   $	 4��6R   $
 10q�6c   $��0,0,0,20q�6p   ._�5I      �� .+�5K      �� $��Data Summary��6$   $
��Bold"�68   $��16(�6J   $��0,0,0,10q�6X   �� 5L      -   5        $ TotalQuoteCountText��5L   @   $��Total Quote Count: 0��6$   6@   �� 5M      -   5        $ DateRangeText��5M   ;   $��Date Range: N/A��6$   6;   �� 5N      -   5        $ UniqueContractCountText��5N   D   $��Unique Contract Count: 0��6$   6D   �� 5O      -   5        $ ExchangeCountText��5O   =   $��Exchange Count: 0��6$   6=   �� 5R      $��20��6$   
������ 60   �� 5U      -   5        $��Load Feather Data��5U   !   $	��150��6=   $��35��6I   6U   �� 5V      -	   5        $ FileLoadSummaryText��5V   ;   $��No file loaded.��6$   $��0,10,0,0q�6X   $
��Wrap=�6j   5X      5Y      �� 5]      $��2��6   
�������$��1q�6B   $	 4��6V   $
 10q�6g   ._�5^      �� $	��Top=�6%   .+�5_      �� $��Global Settings��6(   $
��Bold"�6?   $��16(�6Q   $��0,0,0,10q�6_   �� 5a      $ 
Horizontal=�6)   $��0,0,0,5q�6B   .+�5b   "   �� $��Distribution Method:��6,   $��SemiBold"�6H   $	��140��6^   �� 5c   "   $��Regula Falsi Method��6,   5d      �� 5f      $ 
Horizontal=�6)   $��0,0,0,15q�6B   .+�5g   "   �� $��Alpha Level:��6,   $��SemiBold"�6@   $	��140��6V   �� 5h   "   $��α = 0.05 (Fixed)��6,   5i      5l      5m      �� 5p      $��3��6   
�������$��1q�6B   $	 4��6V   $
 10q�6g   ._�5q      �� $	��Top=�6%   .+�5r      �� $��Export Options��6(   $
��Bold"�6>   $��16(�6P   $��0,0,0,10q�6^   �� 5t      $ 
Horizontal=�6)   $
��Left=�6B   $��0q�6]   .+�5u   "   �� -
   5        $ ExportAllResults��5u   )   $��Export All Results (md)��6C   $
��Auto��6e   $��30��6r   $��0,0,10,0q�6�   �� 5v   "   -   5        $ ExportECDFPlots��5v   )   $��Export ECDF Plots (png)��6B   $
��Auto��6d   $��30��6q   6}   5w      5x      5y      �� 5{      $��4��6   
�������$��1q�6B   $	 4��6V   $
 10q�6g   ._�5|      � $��10q�6    5}      �� 5~   "   $��1* �63   �� 5   "   $��1* �63    5�      �� 5�   "   $
��Auto �60   �� 5�   "   $��* �60   .+�5�      �� $��0��6(   $��0��68   $��	Call Data��5�   %   $��16(�5�   %   $
��Bold"�5�   %   ���	#FF2C3E50$��Center=�5�   %   $��0,0,0,5q�5�   %   �� 5�      $��1��6(   $��0��68   $��Put Data��5�   %   $��16(�5�   %   $
��Bold"�5�   %   ���	#FF2C3E50$��Center=�5�   %   $��0,0,0,5q�5�   %   &   System.Windows.Controls.DataGrid  5�      -   5          Name$ CallsDataGrid��5�   '   $��0��5�   %   $��1��65      AutoGenerateColumns$ False��5�   %      CanUserAddRows$ False��5�   %   $��0,0,10,0q�5�   %      Columns 5�   "   0  *System.Windows.Controls.DataGridTextColumn  5�   &   ,  &System.Windows.Controls.DataGridColumn   Header$ Symbol��69      
IsReadOnly$
 True��6d   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6I   �� *Symbol+6d     5�   &   $ DateTime��69   $
 True��6�    6K   �� *
DateTime+ �� StringFormat$  MM/dd/yyyy HH:mm:ss.ffffff��6�     5�   &   $
 MMID��69   $
 True��6`    6G   �� *MMID+6`   4  .System.Windows.Controls.DataGridCheckBoxColumn  5�   &   $ BidAsk��6=   $
 True��6h    6M   �� *BidAsk+6h     5�   &   $ Price��69   $
 True��6b    6H   �� *Price+6b   5�        5�      -   5        $ PutsDataGrid��5�   '   $��1��5�   %   $��1��65   $ False��5�   %   $ False��5�   %    5�   "     5�   &   $ Symbol��69   $
 True��6d    6I   �� *Symbol+6d     5�   &   $ DateTime��69   $
 True��6�    6K   �� *
DateTime+$  MM/dd/yyyy HH:mm:ss.ffffff��6�     5�   &   $
 MMID��69   $
 True��6`    6G   �� *MMID+6`     5�   &   $ BidAsk��6=   $
 True��6h    6M   �� *BidAsk+6h     5�   &   $ Price��69   $
 True��6b    6H   �� *Price+6b   5�      5�      5�      5�      5�      �� 5�      -   5        $��RightPanelContainer��5�      $��1��68   $��10q�6H   5�      5�      5�      