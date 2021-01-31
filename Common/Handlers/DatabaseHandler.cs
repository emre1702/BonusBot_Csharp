using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BonusBot.Common.Entities;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using LiteDB;

namespace BonusBot.Common.Handlers
{
    public sealed class DatabaseHandler : IHandler, IDisposable
    {
        private readonly ConcurrentDictionary<string, BaseEntity> _cache;
        private LiteDatabase _liteDatabase;

        public DatabaseHandler()
        {
            _cache = new ConcurrentDictionary<string, BaseEntity>();
        }

        public T Get<T>(object id) where T : BaseEntity
        {
            var strId = id.ToString();
            if (_cache.TryGetValue(strId, out var cached))
                return (T)cached;

            var collection = GetCollection<T>();
            var get = collection.FindOne(x => x.Id == strId);

            if (!(get is null))
                _cache.TryAdd(get.Id, get);

            return get;
        }

        public void Save<T>(T document) where T : BaseEntity
        {
            var collection = GetCollection<T>();
            if (!collection.Exists(x => x.Id == document.Id))
            {
                collection.Insert(document);
                _cache.TryAdd(document.Id, document);
            }
            else
            {
                collection.Update(document);
                _cache.TryUpdate(document.Id, document, null);
            }
        }

        public void Delete<T>(T document) where T : BaseEntity
        {
            var collection = GetCollection<T>();
            collection.Delete(new BsonValue(document.Id));
            _cache.TryRemove(document.Id, out _);
        }

        public void VerifyGuilds(IEnumerable<ulong> guildIds)
        {
            var collection = GetCollection<GuildEntity>();
            var fetchAll = collection.FindAll().Select(x => ulong.Parse(x.Id.ToString())).ToHashSet();
            var entities = guildIds
                .Where(x => !fetchAll.Contains(x))
                .Select(x => new GuildEntity
                {
                    Id = x.ToString(),
                    Prefix = '!',
                    WelcomeMessage = "HEY %u%, WELCOME TO %g%! Enjoy your stay!",
                    RoleForMutedSuffix = "Muted",
                    SupportRequestMinTitleLength = 10,
                    SupportRequestMaxTitleLength = 80,
                    SupportRequestMinTextLength = 10,
                    SupportRequestMaxTextLength = 255,
                });
            collection.InsertBulk(entities);
        }

        public ILiteCollection<T> GetCollection<T>() where T : BaseEntity
        {
            if (_liteDatabase is null)
                _liteDatabase = new LiteDatabase($"Filename=/bonusbot-data/{nameof(BonusBot)}.db;Upgrade=true");
            return _liteDatabase.GetCollection<T>(typeof(T).Name.SanitzeEntity());
        }

        public void Dispose()
        {
            _cache.Clear();
            _liteDatabase?.Dispose();
            _liteDatabase = null;
            GC.SuppressFinalize(this);
        }
    }
}