   M S B A M L   `   `   `  ���� E  AZSExplorer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ZSExplorer
ZSExplorer      ZSExplorer.RightPanel   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     R NOxyPlot.Wpf, Version=2.2.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0Y UOxyPlot.Wpf.Shared, Version=2.2.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0"oxyhttp://oxyplot.org/wpf   &localclr-namespace:ZSExplorer   $	��350��$	��600��
�������.����    PrimaryBrush#��[  $��2q�  �� CornerRadius$  8�� �� Padding$ 10q�$��5q�._���  �� Orientation$ Vertical=�$��Stretch=�$��Stretch=�.+��� #��[  $  6��$ 8q�$��0 0 0 10q�._��  � ColumnDefinitions �� $��* ��� $
��Auto �.+��� $ Vertical=�$��0��.+��� -    ��Name$ ContractSymbolText��$��CONTRACT SYMBOL��$
��Bold"�$��16(�
��������� -   $ OptionDetailsText��$&�� Underlying | Expiration | Strike��$��12(�
�������$��0,4,0,0q��� $ 
Horizontal=�$��1��$��Center=�.+�A� -    A�Name$ StatusIndicator��$��14��$��14��
���  ��$��Center=�$��0,0,10,0q��� -    ��Name$ RemoveButton��$��Remove��$
��Auto��$��24��$
��Bold"�
�������  PrimaryPressedBrush#��[ $��0q� � Cursor$
 Hand��� $��0 0 0 10q�$	��Top=�$��Stretch=� �� $��1* ��� $��1* �.+��� $��0��$��0,0,10,0q�$	��Top=�.+��� $ Vertical=�$��0,0,0,10q�.+��� $!��📉 Log Return Time Window��$
��Bold"�$��16(��� $)��#Return Calculation Window (Seconds)��$
��Bold"��� $ 
Horizontal=�$��Center=�$��0,5,0,0q�.+��� -    ��Name$ TimeWindowSlider��$	��300��	 � Minimum$	 0Q�
 � Maximum$	
 100Q� �� TickFrequency$ 1Q� �� IsMoveToPointEnabled$
 True���� -    ��Name$ MicrosecondInputBox��$��80��$��10,0,0,0q�$��Center=�$��Center=�$��0���� $ 
Horizontal=�$��0,5,0,0q�$
��Left=�.+��� -   $ 
TimeLabel0��$��0s��$��60���� -   $ TimeLabel25��$	��...��$��60�� �� TextAlignment$ Center=��� -	   $ TimeLabel50��$	��...��$��60��$ Center=��� -
   $ TimeLabel75��$	��...��$��60��$ Center=��� -   $ TimeLabel100��$	��...��$��60��$ Right=��� $ 
Horizontal=�$��10q�.+��� -    ��Name$ BidOnlyCheckbox��$��Use Bid Ask��$��5q��� -   $ AnalyzeAllCheckbox��$%��Analyze All Call or Put Options��$��5q��� -   $ TimeWindowValueText��$��SemiBold"�$��0,5,0,0q�$��12(��� $��Sample Statistics��$
��Bold"�$��0,0,0,6q�A�  A� Columns$ 2�� A� Rows$ 3��$��0,0,0,10q�.+��� $��Sample Size:���� -   $ SampleSizeText��$��-���� $��Mean Return:���� -   $ MeanReturnText��$��-���� $��Standard Deviation:���� -   $ 
StdDevText��$��-���� -    ��Name$ StatisticalTestResults��$��1��$	��Top=�.+��� $$��Fitted Distribution Parameters��$
��Bold"�$��0,0,0,6q�A� $ 2��$ 4��$��0,0,0,10q�.+��� $��Location Param:���� -   $ LocationParamText��$��-���� $��Scale Param:���� -   $ ScaleParamText��$��-���� $��Degrees of Freedom:���� -   $ DegreesFreedomText��$��-����   SecondaryBrush#��[ $��1q�$  6��$ 8q����	#FFF9F2D6._��� .+��� -   $ InsufficientDataText��
���  ��$
��Bold"�$��5q��� $��KS Test Results��$
��Bold"�$��0,0,0,6q��� -   $ KsTestStatText��$��Test Statistic: -��$��18(�$
��Bold"��� -   $ StatDecisionText��$��Decision: -��$
��Bold"�$��0,2,0,6q��� -   $ 
PValueText��$��
P-value: -��$��12(��� #��[  $��1q�$  6��
�������$
��Auto��$��0,0,0,10q�._�  OxyPlot.Wpf.PlotView  -     Name$ EcdfPlot��$	��500��$��10q�