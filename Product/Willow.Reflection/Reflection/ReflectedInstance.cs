using System;

namespace Willow.Reflection
{
    public class ReflectedInstance<TOwner>
    {
        private readonly TOwner _Owner;
        private readonly ReflectedType<TOwner> _OwnerReflectedType;

        private readonly IAccessorCache<TOwner> _FieldCache;
        private readonly IAccessorCache<TOwner> _PropertyCache;
        private readonly IMethodCache<TOwner> _MethodCache;
        
        public ReflectedInstance(TOwner owner) : this(owner, new ReflectedType<TOwner>(), ReflectedType<TOwner>.FieldCache, ReflectedType<TOwner>.PropertyCache, ReflectedType<TOwner>.MethodCache) { }

        public ReflectedInstance(TOwner owner, ReflectedType<TOwner> staticType, IAccessorCache<TOwner> fields, IAccessorCache<TOwner> properties, IMethodCache<TOwner> methods)
        {
            this._Owner = owner;
            this._OwnerReflectedType = staticType;
            this._FieldCache = fields;
            this._PropertyCache = properties;
            this._MethodCache = methods;
        }

        public ReflectedType<TOwner> Static { get { return this._OwnerReflectedType; } }
        public GetterSetter<TOwner> Fields(string name) { return GetterSetter.Create(this._Owner, this._FieldCache.GetAccessor(name)); }
        public GetterSetter<TOwner, TField> Fields<TField>(string name) { return GetterSetter.Create(this._Owner, this._FieldCache.GetAccessor<TField>(name)); }

        public GetterSetter<TOwner> Properties(string name) { return GetterSetter.Create(this._Owner, this._PropertyCache.GetAccessor(name)); }
        public GetterSetter<TOwner, TProperty> Properties<TProperty>(string name) { return GetterSetter.Create(this._Owner, this._PropertyCache.GetAccessor<TProperty>(name)); }

        public T Method<T>(string method) where T : class
        {
            return _MethodCache.GetAccessor<T>(method);
        }
        public Delegate Method(string method, Type returnValueType, params Type[] args)
        {
            return _MethodCache.GetAccessor(method, returnValueType, args);
        }
    }
}