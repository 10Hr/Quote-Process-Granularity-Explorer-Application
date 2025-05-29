
namespace ZSExplorer
{
    public class ArrowData
    {
        public List<string> Symbol { get; } = new();
        public List<DateTime> DateTime { get; } = new();
        public List<string> MMID { get; } = new();
        public List<bool> BidAsk { get; } = new();
        public List<long> Price { get; } = new();
    }
}