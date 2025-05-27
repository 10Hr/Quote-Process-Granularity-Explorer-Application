using System.Text.RegularExpressions;

namespace ZSExplorer.Services;

public class ParseOptionsSymbol
{
    // This method parses an option symbol string and returns an OptionInfo object.
    // The expected format is: SYMBOLYYMMDDTYPESTRIKE
    // Example: AAPL240615C150 (AAPL, 2024-06-15, Call, Strike 150)
    public static OptionInfo Parse(string rawSymbol)
    {
        // Remove leading period if present
        if (rawSymbol.StartsWith("."))
            rawSymbol = rawSymbol.Substring(1);

         // Match pattern: SYMBOL(1+) [C|P](STRIKE)(1+)
        var match = Regex.Match(rawSymbol, @"^([A-Z]+)(\d{6})([CP])(\d+)$");
        if (!match.Success)
            throw new ArgumentException("Invalid option symbol format.");

        return new OptionInfo
        {
            Symbol = match.Groups[1].Value,
            ExpirationDate = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd", null),
            OptionType = match.Groups[3].Value == "C" ? "Call" : "Put",
            StrikePrice = double.Parse(match.Groups[4].Value)
        };
    }
}

public class OptionInfo
{
    public string Symbol { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string OptionType { get; set; }
    public double StrikePrice { get; set; }

    public override string ToString()
    {
        return $"{Symbol} {ExpirationDate:MMddyy} {OptionType} {StrikePrice}";
    }
}