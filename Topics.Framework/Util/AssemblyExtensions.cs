using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Topics.Framework.Util
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
        
        public static Type[] GetAllTypesOf<T>(this Assembly assembly)
        {
            Type[] controllersTypes =
                (from type in GetLoadableTypes(assembly)
                 where type.IsSubclassOf(typeof(T))
                 select type).ToArray();

            return controllersTypes;
        }

        public static Type[] GetAllTypesOf<T>(this IEnumerable<Assembly> assemblies)
        {
            Type[] controllersTypes =
                (from assembly in assemblies
                 from type in assembly.GetTypes()
                 where type.IsSubclassOf(typeof(T))
                 select type).ToArray();

            return controllersTypes;
        }
    }
}
