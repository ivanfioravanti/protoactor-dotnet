﻿// -----------------------------------------------------------------------
//  <copyright file="InMemoryProviderState.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proto.Persistence
{
    public class InMemoryProviderState : IProviderState
    {
        private readonly ConcurrentDictionary<string, Dictionary<long, object>> _events = new ConcurrentDictionary<string, Dictionary<long, object>>();

        private readonly IDictionary<string, (object Data, long Index)> _snapshots = new Dictionary<string, (object Data, long Index)>();

        public Task<(object Data, long Index)> GetSnapshotAsync(string actorName)
        {
            _snapshots.TryGetValue(actorName, out (object Data, long Index) snapshot);
            return Task.FromResult(snapshot);
        }

        public Task GetEventsAsync(string actorName, long indexStart, Action<object> callback)
        {
            if (_events.TryGetValue(actorName, out Dictionary<long, object> events))
            {
                foreach (var e in events.Where(e => e.Key > indexStart))
                {
                    callback(e.Value);
                }
            }
            return Task.FromResult(0);
        }

        public Task PersistEventAsync(string actorName, long index, object data)
        {
            var events = _events.GetOrAdd(actorName, new Dictionary<long, object>());
            long nextEventIndex = 1;
            if (events.Any())
            {
                nextEventIndex = events.Last().Key + 1;
            }
            events.Add(nextEventIndex, data);

            return Task.FromResult(0);
        }

        public Task PersistSnapshotAsync(string actorName, long index, object data)
        {
            _snapshots[actorName] = (data, index);
            return Task.FromResult(0);
        }

        public Task DeleteEventsAsync(string actorName, long fromIndex)
        {
            return Task.FromResult(0);
        }

        public Task DeleteSnapshotsAsync(string actorName, long fromIndex)
        {
            _snapshots.Remove(actorName);
            return Task.FromResult(0);
        }
    }
}