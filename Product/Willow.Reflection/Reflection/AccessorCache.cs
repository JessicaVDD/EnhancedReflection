using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Willow.Reflection
{
    public interface IAccessorCache<TOwner>
    {
        GetterSetterAccessor<TOwner> GetAccessor(string name);
        GetterSetterAccessor<TOwner, TField> GetAccessor<TField>(string name);
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
            this._Fields.Add(name, gsa);
            return gsa;
        }

        public GetterSetterAccessor<TOwner, TField> GetAccessor<TField>(string name)
        {
            object gsaObject;
            if (this._TypedFields.TryGetValue(name, out gsaObject)) return (GetterSetterAccessor<TOwner, TField>) gsaObject;

            var getMethod = this.CreateGetter<TField>(name);
            var setMethod = this.CreateSetter<TField>(name);

            var gsa = new GetterSetterAccessor<TOwner, TField>(getMethod, setMethod);
            this._TypedFields.Add(name, gsa);
            return gsa;
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