using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FastDynamic
{
    public static class DynamicActivators
    {
        private static ConcurrentDictionary<Type, Lazy<Func<object>>> activatorsCache = new ConcurrentDictionary<Type, Lazy<Func<object>>>();

        public static Func<object> GetActivator(this Type type)
        {
            return activatorsCache.GetOrAdd(type, (t) => new Lazy<Func<object>>(() => CreateActivator(t), true)).Value;
        }

        private static Func<object> CreateActivator(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new ArgumentException(type.Name + " has no public default constructor");
            }
            DynamicMethod dm = new DynamicMethod("Create" + type.Name, typeof(object), null, true);
            ILGenerator il = dm.GetILGenerator();
            
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);
            return (Func<object>)dm.CreateDelegate(typeof(Func<object>));
        }

        public static object CreateObject(this Type type)
        {
            return GetActivator(type)();
        }
    }
}
