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
            Because context = () => 
            {
                typedResult = sut.Fields<long>("_LastId");
                result = sut.Fields("_LastId");
                invalidTypedResult = sut.Fields<long>("unknown");
                invalidResult = sut.Fields("unknown");
            };

            It should_return_a_getter_setter_for_a_valid_field = () => 
            {
                result.Accessor.Get.ShouldNotBeNull();
                result.Accessor.Set.ShouldNotBeNull();
                typedResult.Accessor.Get.ShouldNotBeNull();
                typedResult.Accessor.Set.ShouldNotBeNull();
            };

            It should_return_null_getter_and_setter_for_an_invalid_field = () =>
            {
                invalidResult.Accessor.Get.ShouldBeNull();
                invalidResult.Accessor.Set.ShouldBeNull();
                invalidTypedResult.Accessor.Get.ShouldBeNull();
                invalidTypedResult.Accessor.Set.ShouldBeNull();
            };

            It should_retrieve_the_accessor_from_the_cache_after_first_retrieve = () =>
            {
                //If the accessor comes from the cache, the reference is the same
                ReferenceEquals(sut.Fields<long>("_LastId").Accessor, typedResult.Accessor).ShouldBeTrue();
                ReferenceEquals(sut.Fields("_LastId").Accessor, result.Accessor).ShouldBeTrue();
                ReferenceEquals(sut.Fields<long>("unknown").Accessor, invalidTypedResult.Accessor).ShouldBeTrue();
                ReferenceEquals(sut.Fields("unknown").Accessor, invalidResult.Accessor).ShouldBeTrue();

                //Check if the accessor comes from the correct cache
                ReferenceEquals(sut.Fields<long>("_LastId").Accessor, result.Accessor).ShouldBeFalse();
                ReferenceEquals(sut.Fields("_LastId").Accessor, typedResult.Accessor).ShouldBeFalse();
                ReferenceEquals(sut.Fields<long>("unknown").Accessor, invalidResult.Accessor).ShouldBeFalse();
                ReferenceEquals(sut.Fields("unknown").Accessor, invalidTypedResult.Accessor).ShouldBeFalse();
            };

            private static GetterSetter<Person, long> typedResult;
            private static GetterSetter<Person> result;
            private static GetterSetter<Person, long> invalidTypedResult;
            private static GetterSetter<Person> invalidResult;
        }

        public class when_accessing_a_field_of_an_unknown_type : Observes<ReflectedType>
        {
            Establish e = () =>
            {
                sut_factory.create_using(() => new ReflectedType(typeof(Person)));
            };

            Because context = () =>
            {
                result = sut.Fields("_LastId");
                invalidResult = sut.Fields("unknown");
            };

            It should_return_a_getter_setter_for_a_valid_field = () =>
            {
                result.ShouldNotBeNull();
                result.Accessor.Get.ShouldNotBeNull();
                result.Accessor.Set.ShouldNotBeNull();
            };
            
            It should_return_null_getter_and_setter_for_an_invalid_field = () =>
            {
                result.Accessor.Get.ShouldBeNull();
                result.Accessor.Set.ShouldBeNull();
            };

            It should_retrieve_the_accessor_from_the_cache_after_first_retrieve = () =>
            {
                ReferenceEquals(sut.Fields("_LastId").Accessor, result.Accessor).ShouldBeTrue();
                ReferenceEquals(sut.Fields("unknown").Accessor, invalidResult.Accessor).ShouldBeFalse();
            };

            private static GetterSetter<object> result;
            private static GetterSetter<object> invalidResult;
        }
    }
}
