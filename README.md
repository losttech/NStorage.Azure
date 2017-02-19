# NKeyValue
Key Value storage abstraction for .NET
Supports .NET Core
Supported backends: Azure Tables

# Example
```csharp
using LostTech.NKeyValue;

var keyValueStore = await AzureTable.OpenOrCreate("UseDevelopmentStorage=true", "Sample");
await keyValueStore.Put("42", new Dictionary<string, object> { ["Key"] = "value0" });
var answer = await keyValueStore.TryGetVersioned("42");
Console.WriteLine("answer version: {0}", answer.Version);
```