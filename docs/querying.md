# Query API

!!! info "This was 2002"
    There was no LINQ, no generics in .NET 1.x and no lambdas. Queries were
    built from objects - criteria items, criteria groups and joins - and
    results came back as `Array`. It is verbose by today's standards, but it
    let applications query without writing SQL and stay database-neutral.

Member names in queries are the **mapped member names** (the field or property
the `Column` attribute is on), not column names.

## Simple finds

```csharp
// One condition on one member.
Query query = new Query(typeof(Customer));
Array found = query.Find("email", "ada@example.com");

// Every row of a type (realtime = true skips the query cache).
Array all = Query.GetAll(typeof(Customer), true);

// By column name instead of member name.
Array byColumn = query.FindByColumnName("Email", "ada@example.com");

foreach (Customer c in found)
{
    Console.WriteLine(c.Name);
}
```

## Criteria

```csharp
Query query = new Query(typeof(Customer));

Criterias where = QueryBase.CreateCriteria();

CriteriaItem byName = new CriteriaItem(typeof(Customer), "name");
byName.LikeTo("Ad%");

CriteriaItem active = new CriteriaItem(typeof(Customer), "status");
active.EqualTo(CustomerStatus.Active);

where.Add(byName);
where.And(active);

query.AddCriteria(where);
query.OrderBy(typeof(Customer), "name", OrderDirection.Ascending);
query.Execute();

Array customers = query.ResultSet.GetObjects(typeof(Customer));
```

`CriteriaItem` conditions: `EqualTo`, `NotEqualTo`, `GreaterThan`,
`GreaterEqualThan`, `LessThan`, `LessEqualThan`, `LikeTo`, `Between`,
`NotBetween`, `Contains` / `NotContains` (an `IN` list), `ContainsIn` /
`NotContainsIn` (an `IN` sub-query from another `Query`), or
`Condition(Comparison, value)` with any `Comparison` value (`IsNull`,
`IsNotNull`, ...). Groups combine with `Add`, `And` and `Or`, and a
`Criterias` can contain other `Criterias` for parentheses.

## Joins

```csharp
Query query = new Query(typeof(Customer));
Join join = query.CreateJoinFor(typeof(Order), "customerId");
join.InnerJoin(Comparison.Equal, typeof(Customer), "id");
query.AddJoin(join);

Criterias where = QueryBase.CreateCriteria();
CriteriaItem bigOrders = new CriteriaItem(typeof(Order), "total");
bigOrders.GreaterThan(1000m);
where.Add(bigOrders);
query.AddCriteria(where);

query.Distinct = true;
query.Execute();
Array customersWithBigOrders = query.ResultSet.GetObjects(typeof(Customer));
```

`LeftJoin`, `RightJoin` and `OuterJoin` work the same way.

## Aggregates

```csharp
Query query = new Query(typeof(Order));
Criterias totals = QueryBase.CreateAggregateCriteria();
totals.Add(new CriteriaItem(Aggregate.Sum, false, typeof(Order), "total"));
query.AddAggregateCriteria(totals);
query.Execute();
```

`Aggregate`: `Sum`, `Max`, `Min`, `Avg`, `Count`, `CountAll`.

## Results

`query.ResultSet` held rows and columns, one column per registered type:
`GetObjects(type)`, `GetObjects(startIndex, count)` for paging, `GetRow(index)`,
`Find(key)` after `query.RegisterKeyValue(type, member)`, and `Sql` with the
statement that ran. `query.BindObjects` and `query.LockObjects` controlled
whether the returned objects were bound to their rows or locked.

## Running a query

| Call | Effect |
|---|---|
| `Execute()` | Run, using the [query cache](caching.md#query-cache) when allowed |
| `Execute(true)` | Real time: always run against the database |
| `Execute(TimeSpan span, bool slideTime)` | Cache the result for `span`, optionally sliding |
| `Execute(DateTime expirationTime)` | Cache the result until a fixed time |
| `BeginExecute(...)` / `EndExecute(ar)` | The same, asynchronously |

`query.LoadInBackground` and `query.RowNumberBuffer` let a large result start
returning rows before all of them were read.

## Stored procedures

```csharp
// Objects from a stored procedure that returns every mapped column.
SPObjectQuery top = new SPObjectQuery(typeof(Customer), "GetTopCustomers");
top.AppendParameter("@count", 10);
top.Execute();
Array best = top.ResultSet.GetObjects(typeof(Customer));

// Any other stored procedure.
SPQuery archive = new SPQuery("ArchiveOrders", "Sales");
archive.AppendParameter("@before", new DateTime(2010, 1, 1));
int rows = archive.ExecuteNonQuery();
```

`SPQuery` also had `ExecuteScalar`, `ExecuteReader`, `ExecuteDataTable` and
`ExecuteObject(type)`.

For SQL text that still returns objects, see
[Free SQL with objects](object-sql.md).
