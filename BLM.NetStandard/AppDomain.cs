using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Loader;

namespace BLM.NetStandard
{
    public class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }

        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }
        public Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                try
                {
                    var assemblyName = AssemblyLoadContext.GetAssemblyName(module.FileName);
                    var assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    // ignore native modules
                }
            }

            return assemblies.ToArray();
        }
        //public Assembly[] GetAssemblies()
        //{
        //    var assemblies = new List<Assembly>();
        //    var dependencies = DependencyContext.Default.RuntimeLibraries;
        //    foreach (var library in dependencies)
        //    {
        //        if (IsCandidateCompilationLibrary(library))
        //        {
        //            var assembly = Assembly.Load(new AssemblyName(library.Name));
        //            assemblies.Add(assembly);
        //        }
        //    }
        //    return assemblies.ToArray();
        //}

        //private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        //{
        //    return compilationLibrary.Name == (Assembly.GetEntryAssembly().FullName)
        //        || compilationLibrary.Dependencies.Any(d => d.Name.StartsWith("Specify"));
        //}
    }
}
