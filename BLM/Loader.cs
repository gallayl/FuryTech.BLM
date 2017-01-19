using System;
using System.Collections.Generic;
using System.Linq;
using BLM.Interfaces;
using BLM.Interfaces.Authorize;
using BLM.Interfaces.Interpret;

namespace BLM
{
    public static class Loader
    {
        static Loader()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

        }

        private static readonly object TypeLoaderLock = new object();
        private static List<Type> _loadedTypes;
        public static List<Type> Types
        {
            get
            {
                if (_loadedTypes == null)
                {
                    lock (TypeLoaderLock)
                    {
                        if (_loadedTypes == null)
                        {
                            LoadTypes();
                        }
                    }
                }
                return _loadedTypes;
            }
        }

        private static object _loadingTypes = new object();

        static void LoadTypes()
        {
            lock (_loadingTypes)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                _loadedTypes = new List<Type>();
                foreach (var assembly in assemblies)
                {
                    _loadedTypes.AddRange(
                        assembly.GetTypes().Where(a =>
                            a.GetInterfaces().Contains(typeof(IBlmEntry))
                            && a.IsClass
                            && !a.IsAbstract
                            ));
                }
            }
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            _loadedTypes = null;
            LoadTypes();
        }

        private static readonly Dictionary<string, IBlmEntry> BlmInstances = new Dictionary<string, IBlmEntry>();

        public static T GetInstance<T>() where T : class, IBlmEntry, new()
        {
            var key = typeof(T).FullName;
            IBlmEntry currentInstance;
            if (BlmInstances.TryGetValue(key, out currentInstance))
            {
                return (T)currentInstance;
            }
            var instance = new T();
            BlmInstances.Add(key, instance);
            return instance;
        }

        private static readonly Dictionary<string, List<IBlmEntry>> EntriesByTypeCache = new Dictionary<string, List<IBlmEntry>>();


        private static List<Type> GetAllEntriesFor(Type entityType)
        {
            return Types.Where(t =>
                t.GetInterfaces().Any(intr => intr.GenericTypeArguments.Any(gt => gt.IsAssignableFrom(entityType))))
                .ToList();
        }

        private static readonly object EntryLoadLock = new object();

        public static List<IBlmEntry> GetEntriesFor<T>() where T : class, IBlmEntry
        {
            var key = typeof(T).FullName;

            List<IBlmEntry> entries = null;

            if (EntriesByTypeCache.TryGetValue(key, out entries))
            {
                if (entries.Count != 0)
                {
                    return entries;
                }
                EntriesByTypeCache.Remove(key);
            }

            lock (EntryLoadLock)
            {
                if (EntriesByTypeCache.TryGetValue(key, out entries))
                {
                    if (entries.Count != 0)
                    {
                        return entries;
                    }
                    EntriesByTypeCache.Remove(key);
                }

                var entityType = typeof(T);
                var typesForEntity = GetAllEntriesFor(entityType.GetGenericArguments()[0]);
                var typesForBlmEntry = typesForEntity.Where(t =>
                    (t.GetInterfaces().Any(intr => intr.IsGenericType && entityType.GetGenericTypeDefinition().IsAssignableFrom(intr.GetGenericTypeDefinition())))
                    || (t.BaseType != null
                        && (t.BaseType.IsAssignableFrom(typeof(IAuthorizeCollection))
                            || t.BaseType.IsAssignableFrom(typeof(IInterpretBeforeCreate))
                            || t.BaseType.IsAssignableFrom(typeof(IInterpretBeforeModify))
                        )
                        && t.BaseType.GetInterfaces().Any(intr => intr.IsGenericType && entityType.GetGenericTypeDefinition().IsAssignableFrom(intr.GetGenericTypeDefinition()))
                    )
                );

                entries = typesForBlmEntry.Select(type => (IBlmEntry)typeof(Loader).GetMethod("GetInstance").MakeGenericMethod(type).Invoke(null, null)).ToList();

                EntriesByTypeCache.Add(key, entries);

                return entries;
            }
        }
    }
}
