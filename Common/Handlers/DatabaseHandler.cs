using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using LiteDB;

namespace BonusBot.Common.Handlers
{
    public sealed class DatabaseHandler : IHandler
    {
        private readonly LiteDatabase _database;
        private readonly ConcurrentDictionary<object, BaseEntity> _cache;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public DatabaseHandler()
        {
            _database = new LiteDatabase($"Filename={nameof(BonusBot)}.db; Upgrade=true; Connection=direct");
            _cache = new ConcurrentDictionary<object, BaseEntity>();

            BsonMapper.Global.RegisterType(
                serialize: value => unchecked((long)value + long.MinValue),
                deserialize: value => unchecked((ulong)(value - long.MinValue))

            );
        }

        public T Get<T>(object id) where T : BaseEntity
        {
            if (_cache.TryGetValue(id, out var cached))
                return (T)cached;
            BsonValue bsonValue = BsonMapper.Global.Serialize(id);
                
            T get = GetCollection<T>(collection => 
            {
                var all = collection.FindAll().ToList();
                return collection.FindById(bsonValue);
            }).Result;

            if (!(get is null))
                _cache.TryAdd(get.Id, get);

            return get;
        }

        public void Save<T>(T document) where T : BaseEntity
        {
            GetCollection<T>(collection => 
            {
                if (!collection.FindAll().Any(x => x.Id == document.Id))
                {
                    collection.Insert(document);
                    _cache.TryAdd(document.Id, document);
                }
                else
                {
                    collection.Update(BsonMapper.Global.Serialize(document.Id), document);
                    _cache.TryUpdate(document.Id, document, null);
                }
            }).Wait();
        }

        public void Delete<T>(T document) where T : BaseEntity
        {
            GetCollection<T>(collection => 
            {
                collection.Delete(document);
                _cache.TryRemove(document.Id, out _);
            }).Wait();
            
        }

        public void VerifyGuilds(IEnumerable<ulong> guildIds)
        {
            GetCollection<GuildEntity>(collection => 
            {
                var fetchAll = collection.FindAll().Select(x => Convert.ToUInt64(x.Id.ToString())).ToHashSet();
                var entities = guildIds
                    .Where(x => !fetchAll.Contains(x))
                    .Select(x => new GuildEntity
                    {
                        Id = x,
                        Prefix = '!',
                        WelcomeMessage = "HEY %u%, WELCOME TO %g%! Enjoy your stay!",
                        RoleForMutedSuffix = "Muted",
                        SupportRequestMinTitleLength = 10,
                        SupportRequestMaxTitleLength = 80,
                        SupportRequestMinTextLength = 10,
                        SupportRequestMaxTextLength = 255,
                    });
                collection.InsertBulk(entities);
            }).Wait();
           
        }

        public async Task GetCollection<T>(Action<ILiteCollection<T>> action) where T : BaseEntity
        {
            await _semaphoreSlim.WaitAsync(2000);
            try
            {
                action(_database.GetCollection<T>(typeof(T).Name.SanitzeEntity()));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<T> GetCollection<T>(Func<ILiteCollection<T>, T> action) where T : BaseEntity
        {
            await _semaphoreSlim.WaitAsync(2000);
            try
            {
                return action(_database.GetCollection<T>(typeof(T).Name.SanitzeEntity()));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<R> GetCollection<T, R>(Func<ILiteCollection<T>, R> action) where T : BaseEntity
        {
            await _semaphoreSlim.WaitAsync(2000);
            try
            {
                return action(_database.GetCollection<T>(typeof(T).Name.SanitzeEntity()));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task GetCollection<T>(Func<ILiteCollection<T>, Task> func) where T : BaseEntity
        {
            await _semaphoreSlim.WaitAsync(2000);
            try
            {
                await func(_database.GetCollection<T>(typeof(T).Name.SanitzeEntity()));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            
        }
    }
}
