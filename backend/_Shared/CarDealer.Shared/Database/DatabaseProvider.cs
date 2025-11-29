namespace CarDealer.Shared.Database;

/// <summary>
/// Proveedores de base de datos soportados.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// PostgreSQL (Npgsql)
    /// </summary>
    PostgreSQL,

    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer,

    /// <summary>
    /// MySQL (Pomelo.EntityFrameworkCore.MySql)
    /// </summary>
    MySQL,

    /// <summary>
    /// Oracle Database (Oracle.EntityFrameworkCore)
    /// </summary>
    Oracle,

    /// <summary>
    /// In-Memory (Testing only)
    /// </summary>
    InMemory
}
