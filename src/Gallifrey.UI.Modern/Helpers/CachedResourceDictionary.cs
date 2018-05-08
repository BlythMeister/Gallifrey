using System;
using System.Collections.Generic;
using System.Windows;

namespace Gallifrey.UI.Modern.Helpers
{
    public class CachedResourceDictionary : ResourceDictionary
    {
        private static readonly Dictionary<Uri, WeakReference> Cache;
        private Uri source;

        static CachedResourceDictionary()
        {
            Cache = new Dictionary<Uri, WeakReference>();
        }

        public new Uri Source
        {
            get => source;
            set
            {
                source = value;
                if (!Cache.ContainsKey(source))
                {
                    AddToCache();
                }
                else
                {
                    WeakReference weakReference = Cache[source];
                    if (weakReference != null && weakReference.IsAlive)
                    {
                        MergedDictionaries.Add((ResourceDictionary)weakReference.Target);
                    }
                    else
                    {
                        AddToCache();
                    }
                }

            }
        }

        private void AddToCache()
        {
            base.Source = source;
            if (Cache.ContainsKey(source))
            {
                Cache.Remove(source);
            }
            Cache.Add(source, new WeakReference(this, false));
        }
    }
}
