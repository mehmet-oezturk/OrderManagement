using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace OrderManagement.Infrastructure.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _redisDb;
        private IConnectionMultiplexer redis;
        private const string OrderKeyPrefix = "order:";
        private const int DefaultExpirationMinutes = 60;

        public RedisCacheService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis");
            var redis = ConnectionMultiplexer.Connect(connectionString);
            _redisDb = redis.GetDatabase();
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getDataFunc, int? expirationMinutes = null)
        {
            var redisKey = OrderKeyPrefix + key;
            var value = await _redisDb.StringGetAsync(redisKey);

            if (!value.HasValue)
            {
                var data = await getDataFunc();
                var serializedData = JsonSerializer.Serialize(data);
                await _redisDb.StringSetAsync(redisKey, serializedData, 
                    TimeSpan.FromMinutes(expirationMinutes ?? DefaultExpirationMinutes));
                return data;
            }

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task RemoveAsync(string key)
        {
            var redisKey = OrderKeyPrefix + key;
            await _redisDb.KeyDeleteAsync(redisKey);
        }
    }
} 