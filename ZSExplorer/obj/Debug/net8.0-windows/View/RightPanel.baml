   M S B A M L   `   `   `  ����E  AZSExplorer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ZSExplorer
ZSExplorer  5          ZSExplorer.RightPanel   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      R NOxyPlot.Wpf, Version=2.2.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0Y UOxyPlot.Wpf.Shared, Version=2.2.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0"oxyhttp://oxyplot.org/wpf   5      &localclr-namespace:ZSExplorer   5      $	��350��5	      $	��600��6   
�������.��5      ��    PrimaryBrush#��[  6   $��2q�69     �� CornerRadius$  8��6M    �� Padding$ 10q�6^   $��5q�6k   ._�5   
   ��  �� Orientation$ Vertical=�6   $��Stretch=�6,   $��Stretch=�6H   .+�5      �� #��[  6   $  6��6@   $ 8q�6Q   $��0 0 0 10q�6]   ._�5      �  � ColumnDefinitions 5      �� 5      $��* �6+   �� 5      $
��Auto �6+   .+�5      �� $ Vertical=�6!   $��0��68   .+�5      �� -   5         ��Name$ ContractSymbolText��5   $   $��CONTRACT SYMBOL��5   $   $
��Bold"�5   $   $��16(�5   $   
�������5   $   �� 5      -   5        $ OptionDetailsText��5   $   $&�� Underlying | Expiration | Strike��5   $   $��12(�5    $   
�������$��0,4,0,0q�5"   $   5#      �� 5&      $ 
Horizontal=�6!   $��1��6:   $��Center=�6J   .+�5)      A� -   5         A�Name$ StatusIndicator��5)   "   $��14��6;   $��14��6F   
���  ��$��Center=�6]   $��0,0,10,0q�6x   �� 5,      -   5         ��Name$ RemoveButton��5,   !   $��Remove��67   $
��Auto��6H   $��24��6U   $
��Bold"�6a   
�������  PrimaryPressedBrush#��[ 5-   4   $��0q�5.   !    � Cursor$
 Hand��65   6C   5/      51      52      � 55      $��0 0 0 10q�6   $	��Top=�6%   $��Stretch=�6=    56      �� 57      $��1* �6'   �� 58      $��1* �6'   .+�5<      �� $��0��6   $��0,0,10,0q�6-   $	��Top=�6?   .+�5?      �� $ Vertical=�6   $��0,0,0,10q�64   .+�5@      �� $!��📉 Log Return Time Window��6    $
��Bold"�6A   $��16(�6S   �� 5A      $)��#Return Calculation Window (Seconds)��6    $
��Bold"�6K   �� 5C      $ 
Horizontal=�6!   $��Center=�6:   $��0,5,0,0q�6U   .+�5D      �� -   5         ��Name$ TimeWindowSlider��5D   !   $	��300��5E   !   	 � Minimum$	 0Q�5F   !   
 � Maximum$	
 100Q�5G   !    �� TickFrequency$ 1Q�5H   !    �� IsMoveToPointEnabled$
 True��5J   !   5L   !   �� 5M      -   5         ��Name$ MicrosecondInputBox��5M   "   $��80��5N   !   $��10,0,0,0q�5O   !   $��Center=�5P   !   $��Center=�5Q   !   $��0��5S   !   5T      �� 5V      $ 
Horizontal=�6!   $��0,5,0,0q�6:   $
��Left=�6K   .+�5W      �� -   5        $ 
TimeLabel0��5W   $   $��0s��68   $��60��6B   �� 5X      -   5        $ TimeLabel25��5X   $   $	��...��69   $��60��6D    �� TextAlignment$ Center=�6O   �� 5Y      -	   5        $ TimeLabel50��5Y   $   $	��...��69   $��60��6D   $ Center=�6O   �� 5Z      -
   5        $ TimeLabel75��5Z   $   $	��...��69   $��60��6D   $ Center=�6O   �� 5[      -   5        $ TimeLabel100��5[   $   $	��...��6:   $��60��6E   $ Right=�6P   5\      �� 5^      $ 
Horizontal=�6!   $��10q�6:   .+�5_      �� -   5         ��Name$ BidOnlyCheckbox��5`      $��Use Bid Ask��5a      $��5q�5b      5d      �� 5e      -   5        $ AnalyzeAllCheckbox��5f      $%��Analyze All Call or Put Options��5g      $��5q�5h      5j      5k      �� 5m      -   5        $ TimeWindowValueText��5m       $��SemiBold"�5n      $��0,5,0,0q�5o      $��12(�5p      5q      �� 5t      $��Sample Statistics��6    $
��Bold"�69   $��0,0,0,6q�6K   A� 5u       A� Columns$ 2��6"    A� Rows$ 3��6.   $��0,0,0,10q�67   .+�5v      �� $��Sample Size:��6$   �� 5w      -   5        $ SampleSizeText��5w   $   $��-��6<   �� 5y      $��Mean Return:��6$   �� 5z      -   5        $ MeanReturnText��5z   $   $��-��6<   �� 5|      $��Standard Deviation:��6$   �� 5}      -   5        $ 
StdDevText��5}   $   $��-��68   5~      5      �� 5�      -   5         ��Name$ StatisticalTestResults��5�      $��1��6=   $	��Top=�6M   .+�5�      �� $$��Fitted Distribution Parameters��6    $
��Bold"�6F   $��0,0,0,6q�6X   A� 5�      $ 2��6"   $ 4��6.   $��0,0,0,10q�67   .+�5�      �� $��Location Param:��6$   �� 5�      -   5        $ LocationParamText��5�   $   $��-��6?   �� 5�      $��Scale Param:��6$   �� 5�      -   5        $ ScaleParamText��5�   $   $��-��6<   �� 5�      $��Degrees of Freedom:��6$   �� 5�      -   5        $ DegreesFreedomText��5�   $   $��-��6@   5�      �� 5�        SecondaryBrush#��[ 6   $��1q�6K   $  6��6_   $ 8q�6p   ���	#FFF9F2D6._�5�      �� .+�5�      �� -   5        $ InsufficientDataText��5�   (   
���  ��$
��Bold"�6W   $��5q�6i   �� 5�      $��KS Test Results��6(   $
��Bold"�6?   $��0,0,0,6q�6Q   �� 5�      -   5        $ KsTestStatText��5�   (   $��Test Statistic: -��6@   $��18(�6Y   $
��Bold"�6g   �� 5�      -   5        $ StatDecisionText��5�   (   $��Decision: -��6B   $
��Bold"�6U   $��0,2,0,6q�6g   �� 5�      -   5        $ 
PValueText��5�   (   $��
P-value: -��6<   $��12(�6N   5�      5�      5�      5�      �� 5�      #��[  6   $��1q�6A   $  6��6U   
�������$
��Auto��6y   $��0,0,0,10q�6�   ._�5�        OxyPlot.Wpf.PlotView  -   5          Name$ EcdfPlot��5�      $	��500��61   $��10q�6>   5�      5�      5�      5�      