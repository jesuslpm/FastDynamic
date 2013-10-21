using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using System.Web;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace FastDynamic
{
    public delegate void Setter(object obj, object value);
    public delegate object Getter(object obj);

    public static class MemberAccessors
    {
        private static ConcurrentDictionary<Type, Lazy<IDictionary<string, Setter>>> SettersCache = new ConcurrentDictionary<Type, Lazy<IDictionary<string, Setter>>>();
        private static ConcurrentDictionary<Type, Lazy<IDictionary<string, Getter>>> GettersCache = new ConcurrentDictionary<Type, Lazy<IDictionary<string, Getter>>>();

        private static IDictionary<string, Setter> CreateSetters(Type type)
        {
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public );
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, Setter> setters = new Dictionary<string, Setter>(props.Length + fields.Length);
            foreach (PropertyInfo pi in props)
            {
                Setter setter = CreatePropertySetter(pi);
                if (setter != null)
                {
                    setters.Add(pi.Name, setter);
                }
            }
            foreach (FieldInfo fi in fields)
            {
                setters.Add(fi.Name, CreateFieldSetter(fi));
            }

            return new ReadOnlyDictionary<string, Setter>(setters);
        }

        /// <summary>
        /// Returns a dictionary containing the property an field setters of a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDictionary<string, Setter> GetSetters(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return SettersCache.GetOrAdd(type, (t) => new Lazy<IDictionary<string, Setter>>(() => CreateSetters(t), true)).Value;
        }

        private static Setter CreateFieldSetter(FieldInfo fi)
        {
            DynamicMethod dm = new DynamicMethod("Set" + fi.Name + "Value", null, new Type[] { typeof(object), typeof(object) }, true);
            ILGenerator il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, fi.DeclaringType);
            il.Emit(OpCodes.Ldarg_1);

            if (fi.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, fi.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, fi.FieldType);
            }
            il.Emit(OpCodes.Stfld, fi);
            il.Emit(OpCodes.Ret);

            return (Setter)dm.CreateDelegate(typeof(Setter));

        }


        private static Setter CreatePropertySetter(PropertyInfo pi)
        {
            MethodInfo mi = pi.GetSetMethod();
            if (mi == null) return null;

            DynamicMethod dm = new DynamicMethod("Set" + pi.Name + "Value", null, new Type[] { typeof(object), typeof(object) }, true);
            ILGenerator il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, pi.DeclaringType);
            il.Emit(OpCodes.Ldarg_1);

            if (pi.PropertyType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, pi.PropertyType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, pi.PropertyType);
            }
            il.Emit(OpCodes.Callvirt, mi);
            il.Emit(OpCodes.Ret);

            return (Setter)dm.CreateDelegate(typeof(Setter));
        }

        /// <summary>
        /// Returns a dictionary containing the property and field getters of a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDictionary<string, Getter> GetGetters(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return GettersCache.GetOrAdd(type, (t) => new Lazy<IDictionary<string, Getter>>(() => CreateGetters(t), true)).Value;
        }


        public static Getter GetGetter(this Type type, string memberName)
        {
            return type.GetGetters()[memberName];
        }


        public static Setter GetSetter(this Type type, string memberName)
        {
            return type.GetSetters()[memberName];
        }


        private static IDictionary<string, Getter> CreateGetters(Type type)
        {
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, Getter> getters = new Dictionary<string, Getter>(props.Length + fields.Length);

            foreach (PropertyInfo pi in props)
            {
                Getter getter = CreatePropertyGetter(pi);
                if (getter != null)
                {
                    getters.Add(pi.Name, getter);
                }
            }
            foreach (FieldInfo fi in fields)
            {
                getters.Add(fi.Name, CreateFieldGetter(fi));
            }

            return new ReadOnlyDictionary<string, Getter>(getters);
        }

        private static Getter CreateFieldGetter(FieldInfo fi)
        {
            DynamicMethod dm = new DynamicMethod("Get" + fi.Name + "Value", typeof(object), new Type[] { typeof(object) }, true);

            ILGenerator il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, fi.DeclaringType);
            il.Emit(OpCodes.Ldfld, fi);

            if (fi.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, fi.FieldType);
            }

            il.Emit(OpCodes.Ret);

            return (Getter)dm.CreateDelegate(typeof(Getter));

        }

        private static Getter CreatePropertyGetter(PropertyInfo pi)
        {
            MethodInfo mi = pi.GetGetMethod();
            if (mi == null) return null;

            DynamicMethod dm = new DynamicMethod("Get" + pi.Name + "Value", typeof(object), new Type[] { typeof(object) }, true);

            ILGenerator il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, pi.DeclaringType);
            il.Emit(OpCodes.Callvirt, mi);

            if (pi.PropertyType.IsValueType)
            {
                il.Emit(OpCodes.Box, pi.PropertyType);
            }

            il.Emit(OpCodes.Ret);

            return (Getter)dm.CreateDelegate(typeof(Getter));
        }

        /// <summary>
        /// Sets the property value of an object
        /// </summary>
        /// <param name="obj">the object that has the property or field to set</param>
        /// <param name="memberName">the name of the property or field to set</param>
        /// <param name="value">the value of the property or field you to set</param>
        public static void SetMemberValue(this object obj, string memberName, object value)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (string.IsNullOrEmpty(memberName)) throw new ArgumentException("Invalid property or field", "memberName");
            var setters = GetSetters(obj.GetType());
            Setter setter;
            if (setters.TryGetValue(memberName, out setter))
            {
                setter(obj, value);
            }
            else
            {
                throw new ArgumentException("Cannot set property or field " + memberName + ". It doesn't exist or it's read only");
            }
        }

        /// <summary>
        /// gets the property value of an object
        /// </summary>
        /// <param name="obj">the object that has the property you want to get</param>
        /// <param name="propertyName">the property name</param>
        /// <returns>the value of the property</returns>
        public static object GetMemberValue(this object obj, string propertyName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property", "propertyName");
            var getters = GetGetters(obj.GetType());
            Getter getter;
            if (getters.TryGetValue(propertyName, out getter))
            {
                return getter(obj);
            }
            else
            {
                throw new ArgumentException("Cannot get property " + propertyName + ". It doesn't exist or it's write only");
            }
        }
    }
}
