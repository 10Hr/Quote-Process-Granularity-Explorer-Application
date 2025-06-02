# ZSExplorer

ZSExplorer is a C# WPF desktop application designed for interactive analysis of high frequency option quote data. It loads `.arrow` files containing high frequency options quote data, provides summaries and statistics, and allows users to perform a Kolmogorov–Smirnov (KS) test on the log returns of selected option contracts, which are fitted to a Student’s t-distribution. The empirical CDF is compared to a fitted Student’s t-distribution and visualized with OxyPlot.

---

## 📁 Features

- Load Arrow files with high frequency options quote data
- Display summary statistics (quote count, date range, contracts, exchanges)
- Show full quote data in tables
- Filter quotes by contract symbol and strike price and price type (Bid or Ask)
- Run log return analysis and KS test against Student’s t-distribution
- Display and export ECDF plot for selected contracts
- Export data summary and quotes in Markdown format

---

## 🖼️ MainWindow.xaml.cs

### Responsibilities

- Handles UI and control logic for loading data, displaying quotes, and running KS test.
- Orchestrates loading `.arrow` files and building UI components.

### Key Components

- **LoadFeatherDataButton_Click**: Opens file dialog, loads Arrow file asynchronously, populates quote grids and sidebar statistics.
- **UpdateToolbarButtonStates**: Enables/disables UI controls based on file load state.
- **ContractSearchBox_SelectionChanged**: Allows user to select a specific contract symbol and/or strike price and enable KS Test.
- **AddKsTest_Click**: Instantiates `RightPanel` for KS testing and visualization.
- **ExportMarkdownButton_Click**: Exports all quote data into a well-formatted Markdown table.
- **ExportPlotImagesButton_Click**: Saves the ECDF + t-distribution plot as a `.png` file using OxyPlot.
- **RemoveKsTest**: Clears the right panel content and resets the state.

---

## 📊 RightPanel.xaml.cs

### Responsibilities

- Handles statistical analysis and visualization for the selected contract.
- Performs log return calculation, t-distribution fitting, KS testing, and generates ECDF plot.

### Core Methods

- **RunCalculations**:
  - Filters quotes (based on bid/ask)
  - Computes log returns
  - Fits a Student’s t-distribution using `MathNet.Numerics` and a custom class [`StudentTDistributionZeroMean.cs`](./ZSExplorer/Services/StudentTDistributionZeroMean.cs) for zero mean estimation. This class estimates the degrees of freedom (ν) by matching the sample kurtosis to the theoretical kurtosis of the t-distribution using the Regula Falsi root finding method.

  - Performs KS Test against fitted distribution
  - Displays summary stats in the UI

- **CreateECDFPlot**:
  - Generates OxyPlot model for ECDF and theoretical CDF
  - Plots ECDF points (log returns) and overlay with fitted CDF curve

### UI Components

- **`BidOnlyCheckbox`**: When checked, filters the option contract data to include **only bid prices**. When unchecked, it filters to include **only ask prices**. This allows the user to analyze data specific to bid or ask quotes.

- **`AnalyzeAllCheckbox`**: When checked, instructs the analysis to consider **all calls or all puts together**, ignoring strike price differentiation. When unchecked, the analysis respects strike price groups, processing each strike separately but can is still split by bid ask.

- **`TimeWindowSlider`**: This slider moves back and forth and is between the start and end date from the filtered dataset. Next to the slider is a text input that is the amount of seconds of data being included in the calculations.

- `Sample Statistics`: Displays sample size, mean, and standard deviation.
- `Statistical Test Results`: Displays fitted distribution parameters location, scale, and degrees of freedom. Also displays KS Test results test statistic, decision, and p-value.
- `EcdfPlot`: Renders ECDF vs t-distribution graph.

---

### Design Decisions

This application uses two WPF components:
- `MainWindow.xaml`: Hosts the core layout, including the top bar and left panel.
- `RightPanel.xaml`: A dynamic control used for each KS Test analysis.

While the top bar and left panel could have been extracted into separate `UserControl`s, they remain inside `MainWindow.xaml` for simplicity and to streamline development, given their static nature and limited logic.

---

## 📦 Dependencies

- [.NET 8 WPF](https://dotnet.microsoft.com/)
- [Apache.Arrow](https://arrow.apache.org/) (for Arrow data reading)
- [MathNet.Numerics](https://numerics.mathdotnet.com/) (for statistics and distribution fitting)
- [Accord.Net](http://accord-framework.net/) (for statistics and distribution fitting)
- [OxyPlot](https://oxyplot.readthedocs.io/en/latest/) (for plotting)

---

## 📝 Example Markdown Export

```markdown
# Exported Option Quotes

- Export Date: 6/2/2025 5:01:26 PM
- Total Quotes: 6161


# KS Test Results

- KS Statistic: 0.4010
- P-Value: 0.0000E+000

## Prices and Log Returns

| Index | Price    | Log Return |
|-------|----------|------------|
|     0 |     345 |              |
|     1 |     340 |-0.0145988 |
|     2 |     345 | 0.0145988 |
|     3 |       4 |  -4.45725 |
|     4 |     350 |   4.47164 |
|   ... |     ... |       ... |
|  6160 |     365 | -0.252835 |
```

---

## 🚀 Usage

1. Download zip file
2. Extract to folder
3. Open **/ZSExplorer** and open **ZSExplorer.exe**
4. Run the application.
5. Click **"Load Arrow File"** to select a `.arrow` option quote dataset.
6. Use **Contract Search** to filter quotes by symbol and strike.
7. Click **"Run KS Test"** to analyze log returns.
8. Move slider or enter a value in text field to the right to change time window.
9. View ECDF plot + summary, and optionally **export results** as image or markdown.

---

## 🧪 KS Test Background

The **Kolmogorov–Smirnov test** is used to determine if the empirical distribution of log returns follows a theoretical **Student’s t-distribution**. It compares the maximum distance between the empirical cumulative distribution function (ECDF) and the fitted CDF.

---

## 🧹 Future Improvements

- Add multiple contract comparison in same panel
- Remove some redundancies with Parsing contractText

---

## ⚠️ Known Issues

- **Time Slider Label Not Updating**  
  The time slider correctly filters the dataset by timestamp, but the visible label underneath the slider may not update in real time. This is a minor UI synchronization issue and does not affect functionality.

- **Green Status Dot Turns Red on KS ∞**  
  When the Kolmogorov–Smirnov test statistic is infinite, the green status indicator dot unexpectedly turns red. This does not indicate a crash or failure, but is likely an unhandled edge case in the UI coloring logic.

---