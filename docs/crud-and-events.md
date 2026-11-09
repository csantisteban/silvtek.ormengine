# Create, load, save, remove and events

## The basic operations

```csharp
// Create: INSERT, the identity value is filled in.
Customer customer = new Customer();
customer.Name = "Ada";
customer.Email = "ada@example.com";
customer.Create();

// Load: set the key, then load.
Customer loaded = new Customer(customer.Id);
loaded.Load();
if (loaded.Result == ActionResult.RecordNotFound) { /* ... */ }

// Save: UPDATE only the columns that changed.
loaded.Email = "ada@sample.org";
loaded.Save();

// Refresh: always read the row again from the database (Load may use the cache).
loaded.Refresh();

// Remove: DELETE.
loaded.Remove();
```

After each operation, `Result` held an `ActionResult`: `Succeed`, `Failed`,
`RecordNotFound`, `NotModified` (nothing to save) or `TransactionNotPerformed`.
`ObjectInfo` described the object's state:

| Member | Meaning |
|---|---|
| `ObjectInfo.BindingStatus` | `Unbind` (not loaded), `Bound` (loaded from or saved to a row), `Removed` |
| `ObjectInfo.IsDirty` | The object changed since it was loaded |
| `ObjectInfo.SetReadOnly()` | Further saves throw `ReadOnlyException` |
| `ObjectInfo.Locked()` | Loads and saves throw `LockORMException` |
| `ObjectInfo.Isolated()` | This instance is not registered for [change broadcasting](change-broadcasting.md) (its `Load()` can still copy from the cache) |
| `ObjectInfo.LastSql` | The last SQL statement the object ran |

With a plain class, the same operations were static calls: `ORM.Create(c)`,
`ORM.Load(c)`, `ORM.Save(c)`, `ORM.Remove(c)`, `ORM.Refresh(c)`.

## Asynchronous operations

Every operation had an asynchronous pair in the .NET 1.1/2.0 style:

```csharp
IAsyncResult ar = customer.BeginSave(OnSaved, null);
// ... do other work ...

void OnSaved(IAsyncResult ar)
{
    ORMObject saved = customer.EndSave(ar);
}

// The facade works the same way with plain objects:
IAsyncResult ar2 = ORM.BeginLoad(record, null, null);
ORMObject result = ORM.EndLoad(ar2);
```

## Events and lifecycle hooks

A class that inherits `ORMObject` could override the hooks:

```csharp
public class Customer : ORMObject
{
    protected override void OnBeforeSaved()
    {
        if (Name == null || Name.Length == 0)
            throw new ApplicationException("A customer needs a name.");
        base.OnBeforeSaved();
    }

    protected override void OnAfterRemoved()
    {
        AuditLog.Write("Customer removed: " + Id);
        base.OnAfterRemoved();
    }
}
```

Hooks: `OnBeforeCreated`, `OnAfterCreated`, `OnCreated`, `OnBeforeSaved`, `OnAfterSaved`, `OnSaved`,
`OnBeforeRemoved`, `OnAfterRemoved`, `OnRemoved`, `OnBeforeLoaded`, `OnAfterLoaded`,
`OnRecordNotFound`, `OnFailed`, `OnError`, `OnDataBinding` and `OnInit`.

Other code could subscribe to the events of any `ORMObject` - including the
wrapper of a plain object returned by `ORM.Instanciate`:

```csharp
ORMObject orm = ORM.Instanciate(record);

orm.AfterSaved += delegate(object sender, ORMEventArgs e)
{
    Console.WriteLine("Saved " + e.DataObject);
};

orm.Failed += delegate(object sender, ActionFailedEventArgs e)
{
    Console.WriteLine(e.Action + " failed: " + e.Reason);
};

orm.Error += delegate(object sender, ORMErrorEventArgs e)
{
    Console.WriteLine(e.Action + " error: " + e.Exception.Message + " SQL: " + e.Sql);
};

orm.Save();
```

| Event | Raised |
|---|---|
| `BeforeCreated`, `AfterCreated`, `Created` | Around an INSERT |
| `BeforeSaved`, `AfterSaved`, `Saved` | Around an UPDATE |
| `BeforeLoaded`, `AfterLoaded` | Around a load (from the database or the [cache](caching.md)) |
| `BeforeRemoved`, `AfterRemoved`, `Removed` | Around a DELETE |
| `RecordNotFound` | A load found no row |
| `Failed` (`ActionFailedEventArgs`) | An operation did not succeed - with the action and the reason |
| `Error` (`ORMErrorEventArgs`) | An exception - with the action, the exception and the SQL |
| `DataBinding`, `Init` | Binding and initialization of the object |

`ObjectInfo.CancelEvents` turned event raising off for an instance.

## Transactions

`Transaction` grouped operations on several objects. Each operation ran
immediately; `RollBack()` undid them with compensating operations (a created
object was removed, a saved object got its earlier values back, a removed
object was created again). `Commit()` raised the `Created`, `Saved` and
`Removed` events, which waited until the commit.

!!! warning "Known limitation in 1.7"
    `Commit()` clears its list of pending operations inside its loop, so only
    the first object in the transaction gets its deferred event. See
    [Known issues](known-issues.md#transactioncommit-raises-deferred-events-for-the-first-object-only).

```csharp
using (Transaction tx = new Transaction())
{
    tx.Add(customer);
    tx.Add(order);
    try
    {
        customer.Save();
        order.Create();
        tx.Commit();
    }
    catch
    {
        tx.RollBack();
        throw;
    }
}
```

For a single database round trip, see [Batches](batches.md).
