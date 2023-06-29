using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OrkestraLib
{
    public class EventRegistry
    {
        /// <summary>
        /// List of event names and functions to process data
        /// </summary>
        private readonly ConcurrentDictionary<string, List<Action<string>>> Events;

        public EventRegistry()
        {
            Events = new ConcurrentDictionary<string, List<Action<string>>>();
        }

        public void Register(string key, List<Action<string>> actions)
        {
            if (Events.ContainsKey(key))
            {
                Events[key].Clear();
                Events[key] = actions;
            }
            else Events.TryAdd(key, actions);
        }

        /// <summary>
        /// Check if a value exists. Returns the value if found.
        /// </summary>
        public bool TryGetValue(string key, out List<Action<string>> value)
        {
            return Events.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns the value of a key or a empty list if the key does not exist
        /// </summary>
        public List<Action<string>> Get(string key)
        {
            if (TryGetValue(key, out List<Action<string>> value))
            {
                return value;
            }
            else return new List<Action<string>>();
        }

        public void Add(string key, Action<string> action)
        {
            if (Events.ContainsKey(key))
            {
                Events[key].Add(action);
            }
            else
            {
                Events.TryAdd(key, new List<Action<string>>
                {
                    action
                });
            }
        }
    }
}
