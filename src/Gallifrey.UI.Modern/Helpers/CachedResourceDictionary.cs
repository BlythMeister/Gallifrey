using System;
using System.Collections.Generic;
using System.Windows;

namespace Gallifrey.UI.Modern.Helpers
{
    public class CachedResourceDictionary : ResourceDictionary
    {
        private Uri source;

        public new Uri Source
        {
            get => source;
            set
            {
                source = value;

                var (found, data) = ResourceDictionaryCache.Get(source);

                if (found)
                {
                    MergedDictionaries.Add(data);
                }
                else
                {
                    ResourceDictionaryCache.Add(source, this);
                    base.Source = source;
                }
            }
        }
    }

    internal static class ResourceDictionaryCache
    {
        private static readonly Dictionary<Uri, WeakReference> Cache;

        static ResourceDictionaryCache()
        {
            Cache = new Dictionary<Uri, WeakReference>();
        }

        public static void Add(Uri key, ResourceDictionary data)
        {
            if (Cache.ContainsKey(key))
            {
                Cache.Remove(key);
            }
            Cache.Add(key, new WeakReference(data, false));
        }

        public static (bool found, ResourceDictionary data) Get(Uri key)
        {
            return Cache.ContainsKey(key) && Cache[key].IsAlive ? (true, (ResourceDictionary)Cache[key].Target) : (false, null);
        }
    }
}
