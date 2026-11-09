# Providers

Each [connection](configuration.md#connections) named two classes:

- a **connection provider** (`ConnectionProvider`) that creates the ADO.NET
  connection and command objects and reads a table's schema, and
- a **formatter** (`DbTypeFormatter`) that knows the SQL dialect: quoting,
  date and GUID literals, the parameter prefix, the "now" expression, case
  sensitivity.

| Database | Provider | Formatter | Assembly |
|---|---|---|---|
| SQL Server | `Silvtek.ORMEngine.MSSql.MSSqlProvider` | `Silvtek.ORMEngine.MSSql.MSSqlType` | `Silvtek.ORMEngineV1.7` |
| MySQL | `Silvtek.ORMEngine.MySql.MySqlProvider` | `Silvtek.ORMEngine.MySql.MySQLType` | provider in `Silvtek.ORMEngine.MySqlV1.7`, formatter in `Silvtek.ORMEngineV1.7` |

The MySQL provider lived in its own assembly so that applications using only
SQL Server did not need `MySql.Data`.

```xml
<connection name="Shop" timeout="30000" command-timeout="30000">
  <connection-string>[connection-string]</connection-string>
  <provider  class="Silvtek.ORMEngine.MySql.MySqlProvider" assembly="Silvtek.ORMEngine.MySqlV1.7" />
  <formatter class="Silvtek.ORMEngine.MySql.MySQLType"     assembly="Silvtek.ORMEngineV1.7" />
</connection>
```

## Writing a provider

A provider for another database implemented five members:

```csharp
public class MyDbProvider : ConnectionProvider
{
    public override ProviderType Type
    {
        get { return ProviderType.ODBC; }
    }

    public override string ProviderAssembly
    {
        get { return "MyDb.Data"; }
    }

    public override IDbConnection GetConnection()
    {
        return new MyDbConnection();          // the ADO.NET driver's connection
    }

    public override IDbCommand GetCommand()
    {
        return new MyDbCommand();
    }

    public override TableSchema GetTableSchema(string tableName, string connectionName)
    {
        // read the table's columns from the database catalog
        return ReadSchemaFromCatalog(tableName, connectionName);
    }
}
```

`ProviderType` listed the families the engine knew: `MSSql`, `MySQL`, `Oracle`,
`ODBC`, `DB2`, `PostGreSQL` (and `Undefined`). A matching `DbTypeFormatter` subclass supplied
members such as `SingleQuote`, `ParameterizedPrefix`, `Now`, `Today`,
`DBNullValue`, `IsCaseSensitive`, `FormatDate`, `FormatDateTime`, `FormatGuid`
and `FormatBoolean`.
