using System;
using System.Collections.Generic;

namespace Willow.Reflection
{
    public class ReflectedType
    {
        private static readonly Dictionary<Type, ReflectionCache> _Caches = new Dictionary<Type, ReflectionCache>();
        public static ReflectionCache Cache(Type ownerType)
        {
            ReflectionCache cache;
            if (_Caches.TryGetValue(ownerType, out cache))
                return cache;

            cache = new ReflectionCache(ownerType);
            _Caches.Add(ownerType, cache);
            return cache;
        }

        private readonly ReflectionCache _Cache;
        public ReflectedType(Type ownerType)
        {
            _Cache = Cache(ownerType);
        }

        public GetterSetter Fields(string name) { return GetterSetter.Create(_Cache.OwnerType, _Cache.StaticFields.GetAccessor(name)); }
        public GetterSetter Properties(string name) { return GetterSetter.Create(_Cache.OwnerType, _Cache.StaticProperties.GetAccessor(name)); }
    }

    public class ReflectedType<TOwner>
    {
        public static readonly ReflectionCache<TOwner> Cache = new ReflectionCache<TOwner>();

        public GetterSetter<TOwner> Fields(string name) { return GetterSetter.Create(Cache.StaticFields.GetAccessor(name)); }
        public GetterSetter<TOwner, TField> Fields<TField>(string name) { return GetterSetter.Create(Cache.StaticFields.GetAccessor<TField>(name)); }

        public GetterSetter<TOwner> Properties(string name) { return GetterSetter.Create(Cache.StaticProperties.GetAccessor(name)); }
        public GetterSetter<TOwner, TProperty> Properties<TProperty>(string name) { return GetterSetter.Create(Cache.StaticProperties.GetAccessor<TProperty>(name)); }

        public TReturn Method<TReturn>(string method) where TReturn : class
        {
            return Cache.StaticMethods.GetAccessor<TReturn>(method);
        }
        public Delegate Method(string method, Type returnValueType, params Type[] args)
        {
            return Cache.StaticMethods.GetAccessor(method, returnValueType, args);
        }
    }

}