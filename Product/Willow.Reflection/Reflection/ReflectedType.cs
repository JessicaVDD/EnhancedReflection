using System;
using System.Collections.Generic;

namespace Willow.Reflection
{
    public class ReflectedType<TOwner>
    {
        public static readonly IAccessorCache<TOwner> FieldCache = new FieldAccessorCache<TOwner>();
        public static readonly IAccessorCache<TOwner> PropertyCache = new PropertyAccessorCache<TOwner>();
        public static readonly IMethodCache<TOwner> MethodCache = new InstanceMethodCache<TOwner>();
        private static readonly IAccessorCache<TOwner> _StaticFieldCache = new StaticFieldAccessorCache<TOwner>();
        private static readonly IAccessorCache<TOwner> _StaticPropertyCache = new StaticPropertyAccessorCache<TOwner>();
        private static readonly IMethodCache<TOwner> _StaticMethodCache = new StaticMethodCache<TOwner>();

        public GetterSetter<TOwner> Fields(string name) { return GetterSetter.Create(_StaticFieldCache.GetAccessor(name)); }
        public GetterSetter<TOwner, TField> Fields<TField>(string name) { return GetterSetter.Create(_StaticFieldCache.GetAccessor<TField>(name)); }

        public GetterSetter<TOwner> Properties(string name) { return GetterSetter.Create(_StaticPropertyCache.GetAccessor(name)); }
        public GetterSetter<TOwner, TProperty> Properties<TProperty>(string name) { return GetterSetter.Create(_StaticPropertyCache.GetAccessor<TProperty>(name)); }

        public TReturn Method<TReturn>(string method) where TReturn : class
        {
            return _StaticMethodCache.GetAccessor<TReturn>(method);
        }
        public Delegate Method(string method, Type returnValueType, params Type[] args)
        {
            return _StaticMethodCache.GetAccessor(method, returnValueType, args);
        }
    }

    public class ReflectedType
    {
        private static readonly Dictionary<Type, IAccessorCache<object>> _StaticFieldCacheDictionary = new Dictionary<Type, IAccessorCache<object>>();
        private static readonly Dictionary<Type, IAccessorCache<object>> _StaticPropertyCacheDictionary = new Dictionary<Type, IAccessorCache<object>>();

        private readonly IAccessorCache<object> _StaticFieldCache;
        private readonly IAccessorCache<object> _StaticPropertyCache;
        public ReflectedType(Type ownerType)
        {
            IAccessorCache<object> result;
            if (_StaticFieldCacheDictionary.TryGetValue(ownerType, out result))
                _StaticFieldCache = result;
            else
            {
                _StaticFieldCache = new StaticFieldAccessorCache(ownerType);
            }

            if (_StaticFieldCacheDictionary.TryGetValue(ownerType, out result))
                _StaticPropertyCache = result;
            else
            {
                _StaticPropertyCache = new StaticPropertyAccessorCache(ownerType);
            }
        }

        public GetterSetter<object> Fields(string name) { return GetterSetter.Create(_StaticFieldCache.GetAccessor(name)); }
        public GetterSetter<object> Properties(string name) { return GetterSetter.Create(_StaticPropertyCache.GetAccessor(name)); }

    }
}