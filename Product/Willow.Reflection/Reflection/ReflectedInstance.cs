using System;

namespace Willow.Reflection
{
    public class ReflectedInstance
    {
        private readonly object _Owner;
        private readonly ReflectedType _OwnerReflectedType;
        private readonly ReflectionCache _Cache;

        public ReflectedInstance(object owner) : this(owner, new ReflectedType(owner.GetType()), ReflectedType.Cache(owner.GetType())) { }
        public ReflectedInstance(object owner, ReflectedType staticType, ReflectionCache cache)
        {
            this._Owner = owner;
            this._OwnerReflectedType = staticType;
            this._Cache = cache;
        }

        public ReflectedType Static { get { return this._OwnerReflectedType; } }

        public GetterSetter Fields(string name) { return GetterSetter.Create(this._Owner, this._Cache.Fields.GetAccessor(name)); }
        public GetterSetter Properties(string name) { return GetterSetter.Create(this._Owner, this._Cache.Properties.GetAccessor(name)); }

        //public T Method<T>(string method) where T : class { return _Cache.Methods.GetAccessor<T>(method); }
        //public Delegate Method(string method, Type returnValueType, params Type[] args) { return _Cache.Methods.GetAccessor(method, returnValueType, args); }
    }

    public class ReflectedInstance<TOwner>
    {
        private readonly TOwner _Owner;
        private readonly ReflectedType<TOwner> _OwnerReflectedType;
        private readonly ReflectionCache<TOwner> _Cache; 
        
        public ReflectedInstance(TOwner owner) : this(owner, new ReflectedType<TOwner>(), ReflectedType<TOwner>.Cache) { }
        public ReflectedInstance(TOwner owner, ReflectedType<TOwner> staticType, ReflectionCache<TOwner> cache)
        {
            this._Owner = owner;
            this._OwnerReflectedType = staticType;
            this._Cache = cache;
        }

        public ReflectedType<TOwner> Static { get { return this._OwnerReflectedType; } }

        public GetterSetter<TOwner> Fields(string name) { return GetterSetter.Create(this._Owner, this._Cache.Fields.GetAccessor(name)); }
        public GetterSetter<TOwner, TField> Fields<TField>(string name) { return GetterSetter.Create(this._Owner, this._Cache.Fields.GetAccessor<TField>(name)); }

        public GetterSetter<TOwner> Properties(string name) { return GetterSetter.Create(this._Owner, this._Cache.Properties.GetAccessor(name)); }
        public GetterSetter<TOwner, TProperty> Properties<TProperty>(string name) { return GetterSetter.Create(this._Owner, this._Cache.Properties.GetAccessor<TProperty>(name)); }

        public T Method<T>(string method) where T : class { return _Cache.Methods.GetAccessor<T>(method); }
        public Delegate Method(string method, Type returnValueType, params Type[] args) { return _Cache.Methods.GetAccessor(method, returnValueType, args); }
    }
}