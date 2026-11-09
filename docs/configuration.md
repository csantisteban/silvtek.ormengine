# Configuration

The engine read its settings from a custom section of the application
configuration file (`app.config` or `web.config`).

## Registering the section

```xml
<configuration>
  <configSections>
    <section name="Silvtek.ORMEngine"
             type="Silvtek.ORMEngine.Configs.ORMConfigurationHandler, Silvtek.ORMEngineV1.7"
             allowLocation="false" />
  </configSections>

  <Silvtek.ORMEngine version="1.0">
    <!-- global settings and connections, see below -->
  </Silvtek.ORMEngine>
</configuration>
```

The `version` attribute chose the parser: `version="1.0"` is the format on
this page. Older applications used an earlier format, still read by
`LegacyORMConfiguration`.

## Global settings

```xml
<global>
  <map-file path="C:\MyApp\Mappings" />     <!-- folder with XML mapping files -->
  <working-folder path="" />
  <temporary-folder path="" />
  <shared path="\\fileserver\orm-versions" /> <!-- web farm: shared record versions -->
  <cache enabled="true" />                   <!-- local caching of records -->
  <broadcast-changes enabled="true" />       <!-- change broadcasting -->
  <query cache-limit-ms="60000" />           <!-- default lifetime of cached queries -->
  <buffer size="2056" />
  <encoding value="ASCII" />                 <!-- encoding for text data -->
</global>
```

| Element | Purpose | See |
|---|---|---|
| `map-file` | Folder with the [XML mapping](data-objects.md#mapping-with-an-xml-file) files (unfinished in 1.7) | |
| `shared` | Shared folder where each server wrote record versions, so a web farm stayed consistent | [Change broadcasting](change-broadcasting.md#across-servers) |
| `cache` | Turns local caching on or off | [Local caching](caching.md) |
| `broadcast-changes` | Turns change broadcasting on or off | [Change broadcasting](change-broadcasting.md) |
| `query cache-limit-ms` | How long a cached query result lived | [Local caching](caching.md#query-cache) |
| `encoding` | Encoding used for string data | |

## Connections

Each connection had a name. Classes referred to a connection by that name, so
the same code could run against different databases.

```xml
<connections>
  <connection name="Sales" default="true" timeout="45000"
              command-timeout="45000" prepared-statement="false">
    <connection-string>[connection-string]</connection-string>
    <provider  class="Silvtek.ORMEngine.MSSql.MSSqlProvider" assembly="Silvtek.ORMEngineV1.7" />
    <formatter class="Silvtek.ORMEngine.MSSql.MSSqlType"     assembly="Silvtek.ORMEngineV1.7" />
    <retry-exceptions>
      <!-- see "Retry after a SQL error" -->
    </retry-exceptions>
  </connection>
</connections>
```

| Element / attribute | Purpose |
|---|---|
| `name` | The name classes use (`[Table("Customers", "Sales")]`) |
| `default` | Connection used by classes that name none |
| `timeout`, `command-timeout` | Connection and command timeouts |
| `prepared-statement` | INSERT and UPDATE use ADO.NET parameters instead of values written into the SQL (objects with binary members always do). Queries are not affected - see [Known issues](known-issues.md#object-sql-never-uses-database-parameters) |
| `provider` | The [connection provider](providers.md) class and assembly |
| `formatter` | The class that formats values for this database's SQL |
| `retry-exceptions` | [Errors that make an operation retry](sql-retry.md) |

## Configuring from code

`ORMConfigurator` exposed the settings as static members, for example
`ORMConfigurator.GlobalCacheEnabled`, `ORMConfigurator.UpdateBroadCastingEnabled`
and `ORMConfigurator.QueryCacheTimeMs`. `ORMConfigurator.SetConfigurationFile(path)`
loaded the section from a separate file, and `ORM.SetConnectionName(typeof(Customer), "Reports")`
moved a mapped class to another connection at run time.
