using Apache.Arrow.Ipc;
using Microsoft.Data.Analysis;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ZSExplorer
{
    internal static class ArrowDataLoader
    {

        public static async Task<DataFrame> LoadArrowFileAsync(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new ArrowFileReader(stream))
            {
                var recordBatch = await reader.ReadNextRecordBatchAsync();

            var df = new DataFrame();

                for (int colIndex = 0; colIndex < recordBatch.ColumnCount; colIndex++)
                {
                    var field = recordBatch.Schema.GetFieldByIndex(colIndex);
                    var columnName = field.Name;
                    var arrowArray = recordBatch.Column(colIndex);
                    

                     if (arrowArray.GetType() == typeof(Apache.Arrow.BooleanArray))
                         {
                             var boolColumn = new PrimitiveDataFrameColumn<bool>(columnName, arrowArray.Length);

                            
                        var boolArray = (Apache.Arrow.BooleanArray)arrowArray;
                        for (int i = 0; i < boolArray.Length; i++)
                        {
                            boolColumn[i] = boolArray.GetValue(i);
                        }

                             df.Columns.Add(boolColumn);
                         }
                         else
                         {
                            var tempDf = DataFrame.FromArrowRecordBatch(recordBatch);
                            df.Columns.Add(tempDf.Columns[columnName]);
                         }
                }

                return df;
            }
        }


    }
}
