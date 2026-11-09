# Record digest

A record digest is a hash of a row's values, stored in the row itself. It let
the application answer two questions:

- **Was this row changed outside the application** (by a script, a report tool,
  someone editing the table by hand)?
- **Did this object change at all**, so a save can be skipped?

## Mapping

```csharp
[Table("Orders", "Sales")]
public class Order : ORMObject
{
    [Column("OrderNumber", DataType.Int64), PrimaryKey, Sequence(typeof(OrderNumberSequence))]
    private long number;

    [Column("Total", DataType.Decimal)]
    private decimal total;

    [Column("Notes", DataType.String, 500, ModeType = ColumnModeType.ExcludeFromDigest)]
    private string notes;                 // free text, not part of the digest

    [Column("RowDigest", DataType.String, 100)]
    [RecordDigest]
    private string digest;                // maintained by the engine
}
```

- `RecordDigest` marks the member that stores the digest.
  `RecordDigest(typeof(MyDigestProvider))` used another provider.
- `DefaultRecordDigestProvider` computed a SHA-1 hash of the mapped values.
- `ColumnModeType.ExcludeFromDigest` left a member out of the hash.
- The stored digest also carried a version number, used by
  [change broadcasting across servers](change-broadcasting.md#across-servers).

## Checking a row

```csharp
Order order = new Order(1001);
order.Load();

DigestComparisonResult check = order.CompareRecordDigest();
// or, for a plain object: ORM.CompareRecordDigest(record)

if (check == DigestComparisonResult.Different)
{
    Alert("Order 1001 was changed outside the application.");
}
```

`DigestComparisonResult`: `Equal`, `Different`, `DirtyObject`, `NotBind`,
`EmptyDigest`, `NoDigestMemberExist`, `NotDefined`, `Inconclusive`.

## Skipping unnecessary saves

On `Save()`, when the digest of the current values matched the stored one, the
engine skipped the UPDATE and set `Result` to `ActionResult.NotModified`.

## A custom provider

```csharp
public class Sha256Digest : RecordDigestProvider
{
    public override string GenerateDigest(object[] values)
    {
        return MyHashing.Sha256Of(values);
    }

    public override DigestComparisonResult ValidateDigest(string digest, object[] values)
    {
        return GenerateDigest(values) == digest
            ? DigestComparisonResult.Equal
            : DigestComparisonResult.Different;
    }
}
```
