using MathNet.Numerics.Distributions;

namespace ZSExplorer.Services;

public class StudentTDistributionZeroMean
{
    public StudentTDistributionZeroMean() {}
    public StudentTResult StudentT(double[] data)
    {
        int n = data.Length;
        if (n == 0)
            throw new ArgumentException("Data array is empty");

        // μ is fixed to zero by problem statement
        double mu = 0;

        // Calculate s² = (1/n) * Σ(x_i^2)
        double sumSq = data.Sum(x => x * x);
        double s2 = sumSq / n;

        // Calculate sample kurtosis k = (1/n) * Σ(x_i^4) / (s²)^2
        double sumFourth = data.Sum(x => Math.Pow(x, 4));
        double k = (sumFourth / n) / (s2 * s2);

        // Define the function f(ν) = theoretical_kurtosis - sample_kurtosis
        Func<double, double> f = nu =>
        {
            if (nu <= 4) return double.PositiveInfinity; // undefined for nu <= 4
            double theoreticalKurtosis = 3.0 * (nu - 2) / (nu - 4);
            return theoreticalKurtosis - k;
        };


        // Use Regula Falsi to find ν in [4.1, 100] with tolerance 1e-6
        double nu = RegulaFalsi(f, 4.1, 100, 1e-6, 100);

        // Calculate σ = sqrt(s² * (ν - 2) / ν)
        double sigma = Math.Sqrt(s2 * (nu - 2) / nu);

        return new StudentTResult
        {
            Location = mu,
            Scale = sigma,
            DegreesFreedom = nu
        };
    }
    
    public static double RegulaFalsi(Func<double, double> f, double a, double b, double tolerance = 1e-6, int maxIterations = 100)
    {
        double fa = f(a);
        double fb = f(b);

        double c = a; 

        for (int i = 1; i <= maxIterations; i++)
        {
            c = (a * fb - b * fa) / (fb - fa);
            double fc = f(c);

            if (Math.Abs(fc) < tolerance)
                return c;

            if (fc * fa < 0)
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }
        }

        return c;
    }
}