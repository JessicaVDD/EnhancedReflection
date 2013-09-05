using System.Collections;
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
        public class concern<T> : Observes<T> where T : class
        {
            public static object get_a_value(FieldInfo fi)
            {
                return typeof(ClassValues).GetField(fi.Name).GetValue(null);
            }
            public static object get_a_value(PropertyInfo pi)
            {
                return typeof(ClassValues).GetField(pi.Name).GetValue(null);
            }
            public static object get_a_value(string name)
            {
                return typeof(ClassValues).GetField(name).GetValue(null);
            }
        }

        [Subject(typeof(ReflectedType<Person>), "Reflection")]
        public class when_accessing_a_field : concern<ReflectedType<StaticFieldClass>>
        {
            private static void test_accessor<T>(string fieldname)
            {
                var a = sut.Fields<T>(fieldname);
                a.ShouldNotBeNull();
                a.Accessor.ShouldNotBeNull();
                a.Accessor.Get.ShouldNotBeNull();
                a.Accessor.Set.ShouldNotBeNull();
            }

            private Establish e = () =>
            {
                validFieldnames = typeof(StaticFieldClass).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
                validFieldname = "_intField";
                invalidFieldname = "unknown";
                aValue = fake.a_value<int>();
            };

            Because context = () => 
            {
                typedResult = sut.Fields<int>(validFieldname);
                result = sut.Fields(validFieldname);
                invalidTypedResult = sut.Fields<int>(invalidFieldname);
                invalidResult = sut.Fields(invalidFieldname);
            };


            It should_return_a_valid_getter_setter_for_a_valid_field = () => 
            {
                foreach (var fieldname in validFieldnames)
                {
                    var res = sut.Fields(fieldname);
                    res.Accessor.ShouldNotBeNull();
                    res.ShouldNotBeNull();
                    res.Accessor.Get.ShouldNotBeNull();
                    res.Accessor.Set.ShouldNotBeNull();
                }

                test_accessor<int>("_int");
                test_accessor<string>("_string");
                test_accessor<DateTime>("_date");
                test_accessor<Action>("_action");
                test_accessor<object>("_object");
                test_accessor<ArrayList>("_arrayList");
                test_accessor<int>("pInt");
                test_accessor<string>("pString");
                test_accessor<DateTime>("pDate");
                test_accessor<Action>("pAction");
                test_accessor<object>("pObject");
                test_accessor<ArrayList>("pArrayList");
            };

            It should_return_null_for_an_invalid_field = () =>
            {
                invalidResult.ShouldBeNull();
                invalidTypedResult.ShouldBeNull();
            };

            It should_retrieve_the_accessor_from_the_cache_after_first_retrieve = () =>
            {
                //If the accessor comes from the cache, the reference is the same
                ReferenceEquals(sut.Fields<int>(validFieldname).Accessor, typedResult.Accessor).ShouldBeTrue();
                ReferenceEquals(sut.Fields(validFieldname).Accessor, result.Accessor).ShouldBeTrue();

                //Check if the accessor comes from the correct cache
                ReferenceEquals(sut.Fields<int>(validFieldname).Accessor, result.Accessor).ShouldBeFalse();
                ReferenceEquals(sut.Fields(validFieldname).Accessor, typedResult.Accessor).ShouldBeFalse();
            };

            private It should_correctly_get_and_set_a_value = () =>
            {
                sut.Fields<int>(validFieldname).Value = aValue;
                sut.Fields<int>(validFieldname).Value.ShouldEqual(aValue);

                sut.Fields(validFieldname).Value = aValue;
                sut.Fields(validFieldname).Value.ShouldEqual(aValue);
            };

            private static GetterSetter<StaticFieldClass, int> typedResult;
            private static GetterSetter<StaticFieldClass> result;
            private static GetterSetter<StaticFieldClass, int> invalidTypedResult;
            private static GetterSetter<StaticFieldClass> invalidResult;
            private static int aValue;
            private static string validFieldname;
            private static string invalidFieldname;
            private static string[] validFieldnames;
        }

        public class when_accessing_a_field_of_an_unknown_type : concern<ReflectedType>
        {
            Establish e = () =>
            {
                validFieldnames = typeof (StaticFieldClass).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
                invalidFieldname = "unknown";
                sut_factory.create_using(() => new ReflectedType(typeof(StaticFieldClass)));
            };

            Because context = () =>
            {
                invalidResult = sut.Fields(invalidFieldname);
            };

            It should_return_a_valid_getter_setter_for_a_valid_field = () =>
            {
                foreach (var fieldname in validFieldnames)
                {
                    var res = sut.Fields(fieldname);
                    res.Accessor.ShouldNotBeNull();
                    res.ShouldNotBeNull();
                    res.Accessor.Get.ShouldNotBeNull();
                    res.Accessor.Set.ShouldNotBeNull();
                }
            };
            
            It should_return_null_for_an_invalid_field = () =>
            {
                invalidResult.ShouldBeNull();
            };

            It should_retrieve_the_accessor_from_the_cache_after_first_retrieve = () =>
            {
                foreach (var fieldname in validFieldnames)
                {
                    var res = sut.Fields(fieldname);
                    ReferenceEquals(sut.Fields(fieldname).Accessor, res.Accessor).ShouldBeTrue();                    
                }
            };

            private It should_correctly_get_and_set_a_value = () =>
            {
                foreach (var fieldname in validFieldnames)
                {
                    var aValue = get_a_value(fieldname);
                    sut.Fields(fieldname).Value = aValue;
                    sut.Fields(fieldname).Value.ShouldEqual(aValue);                    
                }
            };

            private static GetterSetter invalidResult;
            private static string invalidFieldname;
            private static string[] validFieldnames;
        }
    }
}
