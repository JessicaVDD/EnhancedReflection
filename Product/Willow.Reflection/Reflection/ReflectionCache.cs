using System;

namespace Willow.Reflection
{
    public class ReflectionCache
    {
        private readonly Type _OwnerType;
        private readonly IAccessorCache _Fields;
        private readonly IAccessorCache _Properties;
        private readonly IAccessorCache _StaticFields;
        private readonly IAccessorCache _StaticProperties;
        private readonly IMethodCache _Methods;
        private readonly IMethodCache _StaticMethods;

        public ReflectionCache(Type ownerType)
        {
            this._OwnerType = ownerType;
            this._Fields = new FieldAccessorCache(ownerType);
            this._Properties = new PropertyAccessorCache(ownerType);
            this._StaticFields = new StaticFieldAccessorCache(ownerType);
            this._StaticProperties = new StaticPropertyAccessorCache(ownerType);
            _Methods = new InstanceMethodCache(ownerType);
            _StaticMethods = new StaticMethodCache(ownerType);
        }

        public Type OwnerType { get { return this._OwnerType; } }
        public IAccessorCache Fields { get { return this._Fields; } }
        public IAccessorCache Properties { get { return this._Properties; } }
        public IAccessorCache StaticFields { get { return this._StaticFields; } }
        public IAccessorCache StaticProperties { get { return this._StaticProperties; } }

        public IMethodCache Methods { get { return this._Methods; } }
        public IMethodCache StaticMethods { get { return this._StaticMethods; } }
    }

    public class ReflectionCache<TOwner>
    {
        private readonly IAccessorCache<TOwner> _Fields = new FieldAccessorCache<TOwner>();
        private readonly IAccessorCache<TOwner> _Properties = new PropertyAccessorCache<TOwner>();
        private readonly IAccessorCache<TOwner> _StaticFields = new StaticFieldAccessorCache<TOwner>();
        private readonly IAccessorCache<TOwner> _StaticProperties = new StaticPropertyAccessorCache<TOwner>();
        private readonly IMethodCache<TOwner> _Methods = new InstanceMethodCache<TOwner>();
        private readonly IMethodCache<TOwner> _StaticMethods = new StaticMethodCache<TOwner>();
        public IAccessorCache<TOwner> Fields { get { return this._Fields; } }
        public IAccessorCache<TOwner> Properties { get { return this._Properties; } }
        public IAccessorCache<TOwner> StaticFields { get { return this._StaticFields; } }
        public IAccessorCache<TOwner> StaticProperties { get { return this._StaticProperties; } }
        public IMethodCache<TOwner> Methods { get { return this._Methods; } }
        public IMethodCache<TOwner> StaticMethods { get { return this._StaticMethods; } }
    }


}