# Silvtek.ORMEngine

> **Archived project - for reference only.** This code is no longer maintained.
> Its use ended many years ago; it is kept here for reference and as a record
> of past work.

Silvtek.ORMEngine is an object-relational mapper (ORM) for .NET. I started it in
**2002**, at a time when ORMs were rare in .NET, and kept developing it until
**2014** (last version: 1.7). It was used in production applications that read
and wrote plain .NET objects instead of hand-written ADO.NET code.

**Documentation and examples:** https://csantisteban.github.io/silvtek.ormengine/

## What it did

- Mapped classes to tables with attributes, and
  created, loaded, saved and removed objects - synchronously or asynchronously.
- Querying with a criteria API, stored procedures, and a "free SQL with
  objects" text language that returned objects.
- Field-level **encryption** and **compression**, configured with one attribute.
- **Local caching** of the same record across the application, and **change
  broadcasting**: saving one copy of a record updated every other in-memory copy.
- **Record digests** to detect rows changed outside the application.
- Automatic **retry** of an operation after a configured SQL error, such as a
  deadlock.
- Pluggable providers for SQL Server and MySQL, and for encryption, compression,
  sequences and digests.

## Stack

- C#, .NET Framework 3.5 (earlier versions targeted .NET 1.1 and 2.0)
- SQL Server (built in) and MySQL (`Silvtek.ORMEngine.MySql`, MySql.Data 5.0.8.1)
- Built with Visual Studio solutions and NAnt; tests with NUnit 2.5.5
- Dependencies were kept in `_prj.references/` folders - this predates NuGet.
  The DLLs are not included; each folder's `readme.txt` lists them.

## Repository layout

| Path | Content |
|---|---|
| `Silvtek.ORMEngine/` | The core library |
| `Silvtek.ORMEngine.MySql/` | The MySQL provider |
| `Build/` | NAnt build files |
| `docs/` | The documentation site source (MkDocs) |

## License

MIT - see [LICENSE](LICENSE) and [COPYRIGHT](COPYRIGHT). Issues and pull
requests are not reviewed.
