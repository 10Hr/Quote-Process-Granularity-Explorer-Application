using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using System.IO;
using ZSExplorer.Services;

namespace ZSExplorer
{
    internal static class ArrowDataLoader
    {
        public static async Task<(ArrowData calls, ArrowData puts)> LoadArrowFileAsync(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new ArrowFileReader(stream);

            var calls = new ArrowData();
            var puts = new ArrowData();

            RecordBatch recordBatch;
            while ((recordBatch = await reader.ReadNextRecordBatchAsync()) != null)
            {
                for (int row = 0; row < recordBatch.Length; row++)
                {
                    string rawSymbol = null;

                    var symbolFieldIndex = recordBatch.Schema.GetFieldIndex("sybmol");
                    if (symbolFieldIndex >= 0)
                    {
                        var symbolArray = recordBatch.Column(symbolFieldIndex) as StringArray;
                        rawSymbol = symbolArray.GetString(row);
                    }
                    else
                    {
                        throw new Exception("Symbol column not found");
                    }

                    OptionInfo optionInfo;
                    try
                    {
                        optionInfo = ParseOptionsSymbol.Parse(rawSymbol);
                    }
                    catch
                    {
                        continue;
                    }

                    ArrowData target = optionInfo.OptionType == "Call" ? calls : puts;

                    for (int colIndex = 0; colIndex < recordBatch.ColumnCount; colIndex++)
                    {
                        var field = recordBatch.Schema.GetFieldByIndex(colIndex);
                        var columnName = field.Name.ToLower();
                        var array = recordBatch.Column(colIndex);

                        switch (columnName)
                        {
                            case "sybmol":
                                var symbolArray = array as StringArray;
                                target.Symbol.Add(symbolArray.GetString(row));
                                break;
                            case "datetime":
                                var timestampArray = array as TimestampArray;
                                var timestampType = (TimestampType)field.DataType;
                                var timestampValue = timestampArray.GetValue(row);

                                DateTime dt;

                                switch (timestampType.Unit)
                                {
                                    case TimeUnit.Nanosecond:
                                        dt = DateTimeOffset.FromUnixTimeMilliseconds((long)(timestampValue ?? 0) / 1000000).UtcDateTime;
                                        break;
                                    case TimeUnit.Microsecond:
                                        dt = DateTimeOffset.FromUnixTimeMilliseconds((long)(timestampValue ?? 0) / 1000).UtcDateTime;
                                        break;
                                    case TimeUnit.Millisecond:
                                        dt = DateTimeOffset.FromUnixTimeMilliseconds((long)(timestampValue ?? 0)).UtcDateTime;
                                        break;
                                    case TimeUnit.Second:
                                        dt = DateTimeOffset.FromUnixTimeSeconds((long)(timestampValue ?? 0)).UtcDateTime;
                                        break;
                                    default:
                                        throw new NotSupportedException("Unsupported time unit");
                                }

                                target.DateTime.Add(dt);
                                break;
                            case "mmid":
                                var mmidArray = array as StringArray;
                                target.MMID.Add(mmidArray.GetString(row));
                                break;
                            case "bidask":
                                var boolArray = array as BooleanArray;
                                target.BidAsk.Add(boolArray.GetValue(row) ?? false);
                                break;
                            case "price":
                                var longArray = array as Int64Array;
                                target.Price.Add(longArray.GetValue(row) ?? 0L);
                                break;
                        }
                    }
                }
            }

            return (calls, puts);
        }
    }
}
