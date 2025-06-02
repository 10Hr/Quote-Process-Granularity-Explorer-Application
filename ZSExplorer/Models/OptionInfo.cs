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