using System;
using System.Collections.Generic;

namespace Willow.Reflection
{
    public interface IAccessorCache
    {
        GetterSetterAccessor GetAccessor(string name);        
    }
    public interface IAccessorCache<TOwner>
    {
        GetterSetterAccessor<TOwner> GetAccessor(string name);
        GetterSetterAccessor<TOwner, TField> GetAccessor<TField>(string name);
    }

    public abstract class AccessorCache : IAccessorCache
    {
        protected readonly Type OwnerType;
        private readonly Dictionary<string, GetterSetterAccessor> _Fields = new Dictionary<string, GetterSetterAccessor>();

        protected AccessorCache(Type ownerType)
        {
            this.OwnerType = ownerType;
        }

        protected abstract Func<object, object> CreateGetter(string name);
        protected abstract Action<object, object> CreateSetter(string name);

        public GetterSetterAccessor GetAccessor(string name)
        {
            GetterSetterAccessor gsa;
            if (this._Fields.TryGetValue(name, out gsa)) return gsa;

            var getMethod = this.CreateGetter(name);
            var setMethod = this.CreateSetter(name);

            gsa = new GetterSetterAccessor(getMethod, setMethod);
            if (!gsa.IsValid) gsa = null;

            this._Fields.Add(name, gsa);
            return gsa;
        }
    }
    public abstract class AccessorCache<TOwner> : IAccessorCache<TOwner>
    {
        private readonly Dictionary<string, GetterSetterAccessor<TOwner>> _Fields = new Dictionary<string, GetterSetterAccessor<TOwner>>();
        private readonly Dictionary<string, object> _TypedFields = new Dictionary<string, object>();

        protected abstract Func<TOwner, TField> CreateGetter<TField>(string name);
        protected abstract Action<TOwner, TField> CreateSetter<TField>(string name);

        public GetterSetterAccessor<TOwner> GetAccessor(string name)
        {
            GetterSetterAccessor<TOwner> gsa;
            if (this._Fields.TryGetValue(name, out gsa)) return gsa;

            var getMethod = this.CreateGetter<object>(name);
            var setMethod = this.CreateSetter<object>(name);

            gsa = new GetterSetterAccessor<TOwner>(getMethod, setMethod);
            if (!gsa.IsValid) gsa = null;

            this._Fields.Add(name, gsa);
            return gsa;
        }

        public GetterSetterAccessor<TOwner, TField> GetAccessor<TField>(string name)
        {
            //Use object, the delegate type is correct but different for each field
            object gsaObject;
            if (this._TypedFields.TryGetValue(name, out gsaObject)) return (GetterSetterAccessor<TOwner, TField>) gsaObject;

            var getMethod = this.CreateGetter<TField>(name);
            var setMethod = this.CreateSetter<TField>(name);

            var gsa = new GetterSetterAccessor<TOwner, TField>(getMethod, setMethod);
            if (!gsa.IsValid) gsa = null;

            this._TypedFields.Add(name, gsa);
            return gsa;
        }

    }

    public abstract class StaticAccessorCache : AccessorCache
    {
        protected StaticAccessorCache(Type ownerType) : base(ownerType) { }

        protected class SetterHelper
        {
            private readonly Action<object> _SetterHelper;
            public SetterHelper(Action<object> setterHelper)
            {
                this._SetterHelper = setterHelper;
            }

            public void CreateSetter(object owner, object prop)
            {
                this._SetterHelper(prop);
            }
        }

        protected class GetterHelper
        {
            private readonly Func<object> _GetterHelper;
            public GetterHelper(Func<object> getterHelper)
            {
                this._GetterHelper = getterHelper;
            }

            public object CreateGetter(object owner)
            {
                return this._GetterHelper();
            }
        }
    }
    public abstract class StaticAccessorCache<TOwner> : AccessorCache<TOwner>
    {
        protected class SetterHelper<T>
        {
            private readonly Action<T> _SetterHelper;
            public SetterHelper(Action<T> setterHelper)
            {
                this._SetterHelper = setterHelper;
            }

            public void CreateSetter(TOwner owner, T prop)
            {
                this._SetterHelper(prop);
            }
        }

        protected class GetterHelper<T>
        {
            private readonly Func<T> _GetterHelper;
            public GetterHelper(Func<T> getterHelper)
            {
                this._GetterHelper = getterHelper;
            }

            public T CreateGetter(TOwner owner)
            {
                return this._GetterHelper();
            }
        }
    }

    public class FieldAccessorCache : AccessorCache
    {
        public FieldAccessorCache(Type ownerType) : base(ownerType) { }

        protected override Func<object, object> CreateGetter(string name)
        {
            return DynamicMethodGenerator.GenerateInstanceFieldGetter(OwnerType, name);
        }

        protected override Action<object, object> CreateSetter(string name)
        {
            return DynamicMethodGenerator.GenerateInstanceFieldSetter(OwnerType, name);
        }
    }
    public class FieldAccessorCache<TOwner> : AccessorCache<TOwner>
    {
        protected override Func<TOwner, TField> CreateGetter<TField>(string name)
        {
            return DynamicMethodGenerator.GenerateInstanceFieldGetter<TOwner, TField>(name);
        }

        protected override Action<TOwner, TField> CreateSetter<TField>(string name)
        {
            return DynamicMethodGenerator.GenerateInstanceFieldSetter<TOwner, TField>(name);
        }
    }

    public class PropertyAccessorCache : AccessorCache
    {
        public PropertyAccessorCache(Type ownerType) : base(ownerType) { }

        protected override Func<object, object> CreateGetter(string name)
        {
            return DynamicMethodGenerator.GenerateInstancePropertyGetter(OwnerType, name);
        }

        protected override Action<object, object> CreateSetter(string name)
        {
            return DynamicMethodGenerator.GenerateInstancePropertySetter(OwnerType, name);
        }
    }
    public class PropertyAccessorCache<TOwner> : AccessorCache<TOwner>
    {
        protected override Func<TOwner, TProp> CreateGetter<TProp>(string name)
        {
            return DynamicMethodGenerator.GenerateInstancePropertyGetter<TOwner, TProp>(name);
        }

        protected override Action<TOwner, TProp> CreateSetter<TProp>(string name)
        {
            return DynamicMethodGenerator.GenerateInstancePropertySetter<TOwner, TProp>(name);
        }
    }

    public class StaticFieldAccessorCache : StaticAccessorCache
    {
        public StaticFieldAccessorCache(Type ownerType) : base(ownerType) { }

        protected override Func<object, object> CreateGetter(string name)
        {
            var del = DynamicMethodGenerator.GenerateStaticFieldGetter(OwnerType, name);
            if (del == null) return null;

            var helper = new GetterHelper(del);
            return helper.CreateGetter;
        }

        protected override Action<object, object> CreateSetter(string name)
        {

            var del = DynamicMethodGenerator.GenerateStaticFieldSetter(OwnerType, name);
            if (del == null) return null;

            var helper = new SetterHelper(del);
            return helper.CreateSetter;
        }
    }
    public class StaticFieldAccessorCache<TOwner> : StaticAccessorCache<TOwner>
    {
        protected override Func<TOwner, TField> CreateGetter<TField>(string name)
        {
            var del = DynamicMethodGenerator.GenerateStaticFieldGetter<TOwner, TField>(name);
            if (del == null) return null;

            var helper = new GetterHelper<TField>(del);
            return helper.CreateGetter;
        }

        protected override Action<TOwner, TField> CreateSetter<TField>(string name)
        {
            var del = DynamicMethodGenerator.GenerateStaticFieldSetter<TOwner, TField>(name);
            if (del == null) return null;

            var helper = new SetterHelper<TField>(del);
            return helper.CreateSetter;
        }
    }

    public class StaticPropertyAccessorCache : StaticAccessorCache
    {
        public StaticPropertyAccessorCache(Type ownerType) : base(ownerType) { }

        protected override Func<object, object> CreateGetter(string name)
        {
            var del = DynamicMethodGenerator.GenerateStaticPropertyGetter(OwnerType, name);
            if (del == null) return null;

            var helper = new GetterHelper(del);
            return helper.CreateGetter;
        }

        protected override Action<object, object> CreateSetter(string name)
        {
            var del = DynamicMethodGenerator.GenerateStaticPropertySetter(OwnerType, name);
            if (del == null) return null;

            var helper = new SetterHelper(del);
            return helper.CreateSetter;
        }
    }
    public class StaticPropertyAccessorCache<TOwner> : StaticAccessorCache<TOwner>
    {
        protected override Func<TOwner, TProp> CreateGetter<TProp>(string name)
        {
            var del = DynamicMethodGenerator.GenerateStaticPropertyGetter<TOwner, TProp>(name);
            if (del == null) return null;

            var helper = new GetterHelper<TProp>(del);
            return helper.CreateGetter;
        }

        protected override Action<TOwner, TProp> CreateSetter<TProp>(string name)
        {
            var del = DynamicMethodGenerator.GenerateStaticPropertySetter<TOwner, TProp>(name);
            if (del == null) return null;

            var helper = new SetterHelper<TProp>(del);
            return helper.CreateSetter;
        }
    }


}