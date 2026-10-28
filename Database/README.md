# Database schema export

`erp_identity_schema.sql` is the portable SQLite schema export for this project. It is generated
from `ApplicationDbContext` and the migrations in `../Migrations`.

## Use in another project

1. Create an empty SQLite database.
2. Execute `erp_identity_schema.sql` against that database.
3. Use the resulting database connection in the other project.

Example with the SQLite command-line tool:

```bash
sqlite3 another-project.db ".read Database/erp_identity_schema.sql"
```

The export includes ASP.NET Identity tables as well as the ERP module tables, relationships,
indexes, and `__EFMigrationsHistory`.

## Regenerate

Run these commands from the project root after changing entities or adding a migration:

```bash
dotnet ef migrations add YourMigrationName
dotnet ef migrations script --no-build --output Database/erp_identity_schema.sql
```

This is a fresh-database script. SQLite does not support EF Core's `--idempotent` migration
script option. For an existing database, use the migration chain instead:

```bash
dotnet ef database update
```