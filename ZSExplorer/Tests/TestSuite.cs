using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Text;
using System.IO;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.SkiaSharp;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Distributions;
using Accord.Statistics.Testing;
using Metalama.Patterns.Wpf;
using Metalama.Patterns.Observability;
using ZSExplorer.Services;
using System.Windows.Threading;

namespace ZSExplorer.Tests;

public class TestSuite {

    // Note: Method-of-moments with kurtosis has inherent estimation variance.
    // Expect ~10-30% error for moderate DoF (10-30) with finite samples.
    // This is a property of the statistical method, not an implementation bug.
    public static void TestWithSyntheticData()
    {
        // Test parameters
        double trueLocation = 0.0;
        double trueScale = 0.05;
        double trueDof = 10.0;
        int sampleSize = 10000;
        
        // Generate synthetic data from t-distribution
        var random = new Random(42);
        var tDist = new MathNet.Numerics.Distributions.StudentT(trueLocation, trueScale, trueDof, random);
        double[] syntheticData = new double[sampleSize];
        for (int i = 0; i < sampleSize; i++)
        {
            syntheticData[i] = tDist.Sample();
        }
        
        // DIAGNOSTIC: Check actual sample kurtosis
        double s2 = syntheticData.Average(x => x * x);
        double sampleKurtosis = syntheticData.Average(x => Math.Pow(x, 4)) / (s2 * s2);
        double trueKurtosis = 3.0 * (trueDof - 2) / (trueDof - 4);
        
        System.Diagnostics.Debug.WriteLine("=== SYNTHETIC DATA TEST ===");
        System.Diagnostics.Debug.WriteLine($"True population kurtosis: {trueKurtosis:F4}");
        System.Diagnostics.Debug.WriteLine($"Actual sample kurtosis: {sampleKurtosis:F4}");
        System.Diagnostics.Debug.WriteLine($"Kurtosis difference: {Math.Abs(sampleKurtosis - trueKurtosis):F4}");
        
        // Fit using custom method
        StudentTDistributionZeroMean fitter = new StudentTDistributionZeroMean();
        StudentTResult result = fitter.StudentT(syntheticData);
        
        double fittedKurtosis = 3.0 * (result.DegreesFreedom - 2) / (result.DegreesFreedom - 4);
        System.Diagnostics.Debug.WriteLine($"Fitted kurtosis: {fittedKurtosis:F4} (should match sample)");
        

        // Check results
        System.Diagnostics.Debug.WriteLine($"\nTrue: Location={trueLocation:F4}, Scale={trueScale:F4}, DoF={trueDof:F2}");
        System.Diagnostics.Debug.WriteLine($"Fitted: Location={result.Location:F4}, Scale={result.Scale:F4}, DoF={result.DegreesFreedom:F2}");
        System.Diagnostics.Debug.WriteLine($"Location Error: {Math.Abs(result.Location - trueLocation):F6}");
        System.Diagnostics.Debug.WriteLine($"Scale Error: {Math.Abs(result.Scale - trueScale) / trueScale * 100:F2}%");
        System.Diagnostics.Debug.WriteLine($"DoF Error: {Math.Abs(result.DegreesFreedom - trueDof) / trueDof * 100:F2}%");
        
        // Standardize and run KS test
        double[] standardized = syntheticData.Select(x => x / result.Scale).ToArray();
        var testDist = new TDistribution(result.DegreesFreedom);
        var ks = new KolmogorovSmirnovTest(standardized, testDist);
        
        System.Diagnostics.Debug.WriteLine($"\nKS Test on Synthetic Data:");
        System.Diagnostics.Debug.WriteLine($"KS Statistic: {ks.Statistic:F6}");
        System.Diagnostics.Debug.WriteLine($"P-Value: {ks.PValue:F6}");
        System.Diagnostics.Debug.WriteLine($"Should NOT reject (p > 0.05): {!ks.Significant}");
        System.Diagnostics.Debug.WriteLine("========================\n");
    }
    public static void TestWithNormalData()
    {
        // Generate normal data
        var normal = new MathNet.Numerics.Distributions.Normal(0, 0.03, new Random(42));
        double[] normalData = Enumerable.Range(0, 10000).Select(_ => normal.Sample()).ToArray();
        
        // Fit using custom method
        StudentTDistributionZeroMean fitter = new StudentTDistributionZeroMean();
        StudentTResult result = fitter.StudentT(normalData);
        
        System.Diagnostics.Debug.WriteLine("=== NORMAL DATA TEST ===");
        System.Diagnostics.Debug.WriteLine($"Fitted DoF: {(double.IsInfinity(result.DegreesFreedom) ? "∞" : result.DegreesFreedom.ToString("F2"))}");
        System.Diagnostics.Debug.WriteLine($"Should be ∞ or > 100: {double.IsInfinity(result.DegreesFreedom) || result.DegreesFreedom > 100}");
        System.Diagnostics.Debug.WriteLine("========================\n");
    }

    public static void TestWithHeavyTailedData()
    {
        // Generate heavy-tailed data (DoF = 5)
        var tDist = new MathNet.Numerics.Distributions.StudentT(0, 0.02, 5, new Random(42));
        double[] heavyData = Enumerable.Range(0, 10000).Select(_ => tDist.Sample()).ToArray();
        
        // Fit using custom method
        StudentTDistributionZeroMean fitter = new StudentTDistributionZeroMean();
        StudentTResult result = fitter.StudentT(heavyData);
        
        System.Diagnostics.Debug.WriteLine("=== HEAVY-TAILED DATA TEST ===");
        System.Diagnostics.Debug.WriteLine($"True DoF: 5.00");
        System.Diagnostics.Debug.WriteLine($"Fitted DoF: {result.DegreesFreedom:F2}");
        System.Diagnostics.Debug.WriteLine($"Should be close to 5: {Math.Abs(result.DegreesFreedom - 5) < 1}");
        System.Diagnostics.Debug.WriteLine("========================\n");
    }

    public static void RunAllTests()
    {
        TestWithSyntheticData();
        TestWithNormalData();
        TestWithHeavyTailedData();
    }

}