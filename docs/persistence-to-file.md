# Saving objects to a file

A data object could be written to a file with its mapped values, and read back
later as if it had been loaded from the database - useful for offline work,
snapshots, or moving a record between systems.

```csharp
Customer customer = new Customer(42);
customer.Load();
customer.PersistToFile(@"C:\Snapshots\customer-42.rec");

// Later, possibly in another process:
Customer restored = new Customer();
restored.LoadFromFile(@"C:\Snapshots\customer-42.rec");
// restored behaves like a loaded object: edit it and Save() it.
```

- `PersistToFile(path)` wrote every mapped value, including binary members,
  with each member's formatter.
- `LoadFromFile(path)` worked only on an object that was not already bound to a
  row, raised the `BeforeLoaded` event, and restored the
  [record digest](record-digest.md) version, so a later save could still detect
  a conflicting change.

## Serialization helpers

`ORMSerializer` offered general helpers for any object:

| Method | Purpose |
|---|---|
| `Serialize(obj)` / `Deserialize(bytes)` | To and from a byte array |
| `SerializeToFile(file, obj)` / `DeserializeFromFile(file)` | To and from a file, with an optional `IFormatter` |
| `CreateXml(orm)` / `WriteXml(stream, orm)` | The object's values as XML |

`ORMObject` itself was serializable (`ISerializable`), so data objects could
also cross .NET Remoting boundaries or be kept in a session state store.
