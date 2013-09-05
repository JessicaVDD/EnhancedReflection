namespace Willow.Reflection
{
    public class GetterSetter<TOwner, TField>
    {
        private readonly TOwner _Owner;
        private readonly GetterSetterAccessor<TOwner, TField> _Accessor;

        public GetterSetter(TOwner owner, GetterSetterAccessor<TOwner, TField> accessor)
        {
            this._Owner = owner;
            this._Accessor = accessor;
        }

        public GetterSetterAccessor<TOwner, TField> Accessor {get { return this._Accessor; }}

        public TField Value
        {
            get { return this._Accessor.Get(this._Owner); }
            set { this._Accessor.Set(this._Owner, value); }
        }
    }

    public class GetterSetter<TOwner> : GetterSetter<TOwner, object>
    {
        public GetterSetter(TOwner owner, GetterSetterAccessor<TOwner, object> accessor) : base(owner, accessor) { }


    }

    public class GetterSetter
    {
        public static GetterSetter Create(object owner, GetterSetterAccessor accessor) { return ReferenceEquals(accessor, null) ? null : new GetterSetter(owner, accessor); }

        public static GetterSetter<TOwner> Create<TOwner>(TOwner owner, GetterSetterAccessor<TOwner> accessor) { return ReferenceEquals(accessor, null) ? null : new GetterSetter<TOwner>(owner, accessor); }
        public static GetterSetter<TOwner, TField> Create<TOwner, TField>(TOwner owner, GetterSetterAccessor<TOwner, TField> accessor) { return ReferenceEquals(accessor, null) ? null : new GetterSetter<TOwner, TField>(owner, accessor); }

        public static GetterSetter<TOwner> Create<TOwner>(GetterSetterAccessor<TOwner> accessor) { return ReferenceEquals(accessor, null) ? null : new GetterSetter<TOwner>(default(TOwner), accessor); }
        public static GetterSetter<TOwner, TField> Create<TOwner, TField>(GetterSetterAccessor<TOwner, TField> accessor) { return ReferenceEquals(accessor, null) ? null : new GetterSetter<TOwner, TField>(default(TOwner), accessor); }

        private readonly object _Owner;
        private readonly GetterSetterAccessor _Accessor;

        public GetterSetter(object owner, GetterSetterAccessor accessor)
        {
            this._Owner = owner;
            this._Accessor = accessor;
        }

        public GetterSetterAccessor Accessor { get { return this._Accessor; } }

        public object Value
        {
            get { return this._Accessor.Get(this._Owner); }
            set { this._Accessor.Set(this._Owner, value); }
        }
    }

}