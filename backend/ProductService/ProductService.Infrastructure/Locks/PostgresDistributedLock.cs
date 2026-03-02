using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ProductService.Application.Interfaces;

namespace ProductService.Infrastructure.Locks;

public class PostgresDistributedLock(IConfiguration configuration) : IDistributedLock
{
    private readonly string _connectionString = configuration["PRODUCT_DB_CONNECTION"]!;

    public async Task<IAsyncDisposable?> TryAcquireLockAsync(string lockName, TimeSpan ttl)
    {
        long lockId = GenerateLockId(lockName);
        var connection = new NpgsqlConnection(_connectionString);

        try
        {
            await connection.OpenAsync();
            var result = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT pg_try_advisory_lock(@lockId)", new { lockId });

            if (result)
            {
                // Trả về Handle giữ connection mở
                return new PostgresLockHandle(connection, lockId);
            }

            await connection.DisposeAsync();
            return null;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private long GenerateLockId(string name)
    {
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(name));
        return BitConverter.ToInt64(hash, 0);
    }

    // Class nội bộ để thực hiện việc nhả lock và đóng connection
    private class PostgresLockHandle(NpgsqlConnection connection, long lockId) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await connection.ExecuteAsync("SELECT pg_advisory_unlock(@lockId)", new { lockId });
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}