public class MarketDataRow
{
    public string Symbol { get; set; }
    public DateTime DateTime { get; set; }
    public string MMID { get; set; }
    public bool BidAsk { get; set; }
    public long Price { get; set; }
}