using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;


namespace ERP.Infrastructure.Persistence.CommonDb
{
    public class DbExecutor : IDbExecutor
    {
        private readonly string _connectionString;
        private readonly string _dbType;

        public DbExecutor(string dbType, string connectionString)
        {
            _dbType = dbType.ToLower();
            _connectionString = connectionString;
        }

        // Create the correct DbConnection
        private DbConnection CreateConnection()
        {
            return _dbType switch
            {
                "sqlserver" => new SqlConnection(_connectionString),
                "postgresql" => new NpgsqlConnection(_connectionString),
                "oracle" => new OracleConnection(_connectionString),
                _ => throw new Exception("Unsupported database type")
            };
        }

        #region Basic Queries

        public async Task<T> QuerySingleAsync<T>(string sql, Dictionary<string, object> parameters)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<T>(sql, new DynamicParameters(parameters));
        }

        public async Task<int> ExecuteAsync(string sql, Dictionary<string, object> parameters)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            return await conn.ExecuteAsync(sql, new DynamicParameters(parameters));
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object? parameters = null)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.QueryAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteCommandAsync(string sql, object? parameters = null)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.ExecuteAsync(sql, parameters);
        }

        public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<T>(sql, parameters);
        }

        #endregion

        #region Stored Procedures

        public async Task<IEnumerable<T>> ExecuteStoredProcQueryAsync<T>(string procName, object? parameters = null)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.QueryAsync<T>(procName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ExecuteStoredProcCommandAsync(string procName, object? parameters = null)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.ExecuteAsync(procName, parameters, commandType: CommandType.StoredProcedure);
        }

        #endregion

        #region Transactions

        public async Task ExecuteInTransactionAsync(Func<DbConnection, IDbTransaction, Task> action)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                await action(conn, tran);
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        #endregion

        #region Multi-Mapping (Joins)

        public async Task<IEnumerable<TReturn>> ExecuteMultiMapQueryAsync<T1, T2, TReturn>(
            string sql,
            Func<T1, T2, TReturn> map,
            object? parameters = null,
            string splitOn = "Id")
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.QueryAsync(sql, map, parameters, splitOn: splitOn);
        }

        public async Task<IEnumerable<TReturn>> ExecuteMultiMapQueryAsync<T1, T2, T3, TReturn>(
            string sql,
            Func<T1, T2, T3, TReturn> map,
            object? parameters = null,
            string splitOn = "Id")
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.QueryAsync(sql, map, parameters, splitOn: splitOn);
        }

        #endregion
    }

    public interface IDbExecutor
    {
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object? parameters = null);
        Task<int> ExecuteCommandAsync(string sql, object? parameters = null);
        Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null);

        Task<IEnumerable<T>> ExecuteStoredProcQueryAsync<T>(string procName, object? parameters = null);
        Task<int> ExecuteStoredProcCommandAsync(string procName, object? parameters = null);

        Task ExecuteInTransactionAsync(Func<DbConnection, IDbTransaction, Task> action);

        Task<IEnumerable<TReturn>> ExecuteMultiMapQueryAsync<T1, T2, TReturn>(string sql, Func<T1, T2, TReturn> map, object? parameters = null, string splitOn = "Id");
        Task<IEnumerable<TReturn>> ExecuteMultiMapQueryAsync<T1, T2, T3, TReturn>(string sql, Func<T1, T2, T3, TReturn> map, object? parameters = null, string splitOn = "Id");
    }
}


