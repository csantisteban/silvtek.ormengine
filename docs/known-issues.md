# Known issues

These defects were found in **2026, long after the library was retired**, while
the source code was read to write this documentation. They were never fixed:
the project is archived and no new version will be released. They are listed
so that anyone reading the code knows where it does not do what its API
suggests.

## Retry rules added in code never trigger a retry

`RetryExceptions.AddRetryException(RetryExceptionMatch, Action[])` registers the
exception type and the actions, but does not store the `RetryExceptionMatch`
itself. When an error occurs, the rule has no match to test, so the operation
is never retried.

```csharp
// Data/RetryExceptions.cs (1.7)
public void AddRetryException(RetryExceptionMatch retry, Action[] actions)
{
    ExceptionEntry exceptionEntryEntry = new ExceptionEntry(retry.Type, actions);
    // `retry` is never added to the entry's match list
    ...
}
```

**Works:** rules in the configuration file (`<retry-exceptions>`), which are
read with `ReadDefaultXmlNode` and do store each `<match>`. See
[Retry after a SQL error](sql-retry.md).

## Object SQL never uses database parameters

Parameter values in [free SQL with objects](object-sql.md) (`@name` in a
`CommandText`) are always formatted by the database formatter and written into
the SQL text. The connection setting `prepared-statement="true"` is not
consulted for queries: it switches only INSERT and UPDATE to ADO.NET
parameters (objects with binary members always use them).

```csharp
// DataQuery/QueryLanguage/InternalQLangParser.cs (1.7)
foreach (CommandParameter parameter in parameters)
{
    this.beforefrom.Replace(parameter.Name, parameter.GetFormattedValue());
    this.afterfrom.Replace(parameter.Name, parameter.GetFormattedValue());
}
```

String values are quoted and escaped by the formatter, but the statement text
changes with every value, so the database cannot reuse its plan the way it can
with parameters.

## `ORMBatch.ExecuteBatch` always returns 0

The batch runs correctly - every queued create, save and remove is executed -
but the method discards the row count of both modes and returns `0`. Do not
use the return value to check how many rows were affected.

```csharp
// ORMBatch.cs (1.7) - both modes run, their result is not kept
public int ExecuteBatch(string connectionName, ExecuteBatchMode mode)
{
    switch (mode)
    {
        case ExecuteBatchMode.SingleCall:
            this.ExecuteBatchSingleMode(connectionName);
            break;
        case ExecuteBatchMode.MultipleCalls:
            this.ExecuteBatchMultipleCallsMode(connectionName);
            break;
        default:
            return 0;
    }
    return 0;
}
```

See [Batches](batches.md).

## XML mapping throws after loading the map file

The loader finds `<map-file path>\<Namespace.ClassName>.xml`, parses it, and
then throws an `ORMException` unconditionally, so a class mapped only by XML
cannot be used. Attribute mapping is unaffected.

```csharp
// Schema/ObjectSchemaInfoLoader.cs (1.7)
if (filePath.Length > 0)
{
    info = new ObjectXmlSchemaInfo(source, filePath);
    info.Load();
    string exMsg = String.Format("Object '{0}' is missing xml map file: {1}. ", source.FullName, filePath);
    throw new ORMException(exMsg);
}
```

See [Data objects](data-objects.md#mapping-with-an-xml-file).

## XML mapping drops `queryAlias` names

The parser reads each `<queryAlias name="..."/>` into a local list that is
never used, so the aliases are not available to queries. (Moot in 1.7, since
XML mapping itself does not load - see above.)

```csharp
// XmlMapping/ObjectXmlSchemaInfo.cs (1.7)
StringCollection queryNamesList = new StringCollection();
...
        queryNamesList.Add(queryNameValue);
...
memberlist.Add(memberInfo, mappedMemberProperties, DbTypeFormatter);   // the list is not passed on
```

## `Transaction.Commit()` raises deferred events for the first object only

`Commit()` clears its table of pending operations inside the loop, so every
object after the first one is skipped and does not get its deferred `Created`,
`Saved` or `Removed` event. The database operations themselves already ran
when each object was saved; only the events are lost.

```csharp
// Transaction.cs (1.7)
foreach (ORMObject obj in dataObjects)
{
    if (!dataObjectCommand.Contains(obj.GetRecordIdentifier())) continue;
    ...
    dataObjectCommand.Clear();      // inside the loop
}
```

See [Transactions](crud-and-events.md#transactions).

## `Encrypt(typeof(Provider))` sets the provider's password and salt to null

The overload that takes only a provider type never sets the attribute's
password and salt, and `GetProvider()` always copies them onto the provider. A
provider used this way receives `Password = null` and `Salt = null` and must
get its key material elsewhere.

```csharp
// EncryptAttribute.cs (1.7)
EncryptionProvider returnValue = (EncryptionProvider)Activator.CreateInstance(provider, true);
returnValue.Password = password;    // null with Encrypt(typeof(...))
returnValue.Salt = salt;            // null with Encrypt(typeof(...))
return returnValue;
```

See [Encryption](encryption.md#plugging-it-in).
