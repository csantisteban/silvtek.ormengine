# Batches

`ORMBatch` collected creates, saves and removes of many objects and ran them
together, instead of one round trip per object.

```csharp
ORMBatch batch = new ORMBatch();
batch.IsTransactional = true;

batch.AddCreate(newOrder);
batch.AddSave(customer);
batch.AddRemove(cancelledOrder);

batch.ExecuteBatch("Sales", ExecuteBatchMode.SingleCall);

Console.WriteLine(batch.LastBatchSql);
Console.WriteLine(batch.TotalMilliseconds + " ms");
```

| Member | Purpose |
|---|---|
| `AddCreate`, `AddSave`, `AddRemove` | Queue an object (an `ORMObject` or a mapped plain object) |
| `ExecuteBatch(connectionName, mode)` | Run the queue on a named connection. It returns an `int`, but in 1.7 always `0` - do not use the return value ([known issue](known-issues.md#ormbatchexecutebatch-always-returns-0)) |
| `ExecuteBatchMode.SingleCall` | All statements sent as one SQL batch in a single command |
| `ExecuteBatchMode.MultipleCalls` | One command per statement, on the same connection |
| `IsTransactional` | Run inside a database transaction |
| `PreLoadUnboundObjects` | Load objects that were never loaded before building their statements |
| `RollbackObjects(exception)` | Put the in-memory objects back after a failed batch |
| `BeginExecuteBatch` / `EndExecuteBatch` | Asynchronous version |
| `LastBatchSql`, `TotalMilliseconds` | What ran and how long it took. In a transactional batch, `LastBatchSql` ends up holding the commit statement rather than the batch |
| `Clear()` | Empty the queue |

Compared with a [Transaction](crud-and-events.md#transactions), which undid
operations afterwards with compensating operations, a transactional batch
relied on the database transaction itself.
