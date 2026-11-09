# Compression

Large binary members could be stored compressed with one attribute. Saving
compressed the value; loading decompressed it.

```csharp
[Table("Documents", "Sales")]
public class OrderDocument : ORMObject
{
    [Column("DocumentID", DataType.Int32), PrimaryKey, DatabaseIdentity]
    private int id;

    [Column("Content", DataType.Binary)]
    [Compress]
    private byte[] content;

    [Column("Scan", DataType.Binary)]
    [Compress]
    private Stream scan;
}
```

Compression applied to binary members: `byte[]`, `Stream` / `DataStream`, and
a `string` mapped as `DataType.Binary`, which held the **path of a file** whose
content was stored in the column (on load, the content was written to a file
in the configured temporary folder).

When a member was both compressed and [encrypted](encryption.md), the value was
compressed first and then encrypted.

## The default provider

`DefaultCompressionProvider` used GZip (`System.IO.Compression.GZipStream`).

## Replacing it

```csharp
using System.IO;
using Silvtek.ORMEngine.Compression;

public class FastCompression : CompressionProvider
{
    public override Stream Compress(Stream sourceStream)
    {
        return MyCodec.Compress(sourceStream);     // your algorithm
    }

    public override Stream Decompress(Stream stream)
    {
        return MyCodec.Decompress(stream);
    }

    public override void Dispose()
    {
    }
}

[Column("Content", DataType.Binary)]
[Compress(typeof(FastCompression))]
private byte[] content;
```

In the XML mapping design (unfinished in 1.7), the member's `<map>` element held
`<compressProvider class="..." assembly="..."/>`.

Data written with one provider must be read with the same provider, so a
provider change needs a migration of the existing rows.
