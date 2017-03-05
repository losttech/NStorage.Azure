namespace LostTech.Storage
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    partial class AzureTable
    {
        class ContinuationToken
        {
            [JsonProperty("r")]
            [JsonRequired]
            public string Row { get; set; }
            [JsonProperty("p")]
            [JsonRequired]
            public string Partition { get; set; }

            public static string Serialize(TableContinuationToken continuation)
            {
                if (continuation == null)
                    return null;

                var token = new ContinuationToken {
                    Partition = continuation.NextPartitionKey,
                    Row = continuation.NextRowKey,
                };
                return JsonConvert.SerializeObject(token);
            }

            public static TableContinuationToken Deserialize(string continuation)
            {
                if (continuation == null)
                    return null;

                var token = JsonConvert.DeserializeObject<ContinuationToken>(continuation);
                if (token.Row == null || token.Partition == null)
                    throw new FormatException();

                return new TableContinuationToken {
                    NextPartitionKey = token.Partition,
                    NextRowKey = token.Row,
                };
            }
        }
    }
}
