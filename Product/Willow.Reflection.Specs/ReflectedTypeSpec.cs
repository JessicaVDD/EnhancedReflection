using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Testing.Observations.RhinoMocks;
using Willow.Testing;
using Willow.Testing.Core;
using Willow.Testing.Extensions;
using Willow.Testing.Faking;
using System.Reflection;

namespace Willow.Reflection.Specs
{
    class ReflectedTypeSpec
    {
        [Subject(typeof(ReflectedType<Person>), "Reflection")]
        public class when_accessing_a_field : Observes<ReflectedType<Person>>
        {
            Establish e = () =>
            {
                var fld = typeof(ReflectedType<Person>).GetField("_StaticFieldCache", BindingFlags.Static | BindingFlags.NonPublic);
                fieldAccessor = fld.GetValue(null) as IAccessorCache<Person>;
            };

            Because context = () => 
            {
                typedResult = sut.Fields<long>("_LastId");
                result = sut.Fields("_LastId");
            };

            It should_return_a_getter_setter_for_a_valid_field = () => 
            {
                typedResult.ShouldNotBeNull();
                result.ShouldNotBeNull();
            };

            It should_return_null_for_an_invalid_field = () =>
            {
            };

            It should_retrieve_the_accessor_from_the_cache_after_first_retrieve = () =>
            {
                ReferenceEquals(sut.Fields<long>("_LastId").Accessor, typedResult.Accessor).ShouldBeTrue();
                ReferenceEquals(sut.Fields("_LastId").Accessor, result.Accessor).ShouldBeTrue();
                ReferenceEquals(sut.Fields<long>("_LastId").Accessor, result.Accessor).ShouldBeFalse();
                ReferenceEquals(sut.Fields("_LastId").Accessor, typedResult.Accessor).ShouldBeFalse();
            };

            It should_add_null_to_the_cache_for_an_invalid_field = () =>
            {
            };

            private static IAccessorCache<Person> fieldAccessor;
            private static GetterSetter<Person, long> typedResult;
            private static GetterSetter<Person> result;
        }
    }
}
