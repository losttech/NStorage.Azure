# Azure implementation of storage abstractions <img src="https://losttech.visualstudio.com/_apis/public/build/definitions/8b2acd05-c1ea-4699-8d57-6a9770317b2c/6/badge" alt="Build Status"/>
Azure implementations for LostTech.Storage (.NET)

Supports .NET Core

# Install
```powershell
Install-Package LostTech.Storage.Azure -Pre    # to enable Azure backend
```

# Example
```csharp
using LostTech.Storage;

var keyValueStore = await AzureTable.OpenOrCreate("UseDevelopmentStorage=true", "Sample");
await keyValueStore.Put("42", new Dictionary<string, object> { ["Key"] = "value0" });
var answer = await keyValueStore.TryGetVersioned("42");
Console.WriteLine("answer version: {0}", answer.Version);
```
