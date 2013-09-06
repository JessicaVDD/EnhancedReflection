using Machine.Specifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Willow.Testing.Observations.RhinoMocks;

namespace Willow.Reflection.Specs
{
    class ReflectedInstanceSpec
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

        [Subject(typeof(ReflectedInstance<FieldClass>), "Reflection")]
        public class when_accessing_a_field : concern<ReflectedInstance<FieldClass>>
        {
            private static void test_accessor<T>(string fieldname)
            {
                var a = sut.Fields<T>(fieldname);
                a.ShouldNotBeNull();
                a.Accessor.ShouldNotBeNull();
                a.Accessor.Get.ShouldNotBeNull();
                a.Accessor.Set.ShouldNotBeNull();
            }
            private static void test_equality<T>(string fieldname)
            {
                var aValue = (T)get_a_value(fieldname);
                sut.Fields<T>(fieldname).Value = aValue;
                sut.Fields<T>(fieldname).Value.ShouldEqual(aValue);
            }
            private static void test_caching<T>(string fieldname)
            {
                var gsa = sut.Fields<T>(fieldname);
                ReferenceEquals(sut.Fields<T>(fieldname).Accessor, gsa.Accessor).ShouldBeTrue();
            }

            private Establish e = () =>
            {
                validFieldnames = typeof(FieldClass).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
                invalidFieldname = "unknown";
            };

            Because context = () =>
            {
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
                foreach (var fieldname in validFieldnames)
                {
                    var gsa = sut.Fields(fieldname);
                    ReferenceEquals(sut.Fields(fieldname).Accessor, gsa.Accessor).ShouldBeTrue();
                }

                test_caching<int>("_int");
                test_caching<string>("_string");
                test_caching<DateTime>("_date");
                test_caching<Action>("_action");
                test_caching<object>("_object");
                test_caching<ArrayList>("_arrayList");
                test_caching<int>("pInt");
                test_caching<string>("pString");
                test_caching<DateTime>("pDate");
                test_caching<Action>("pAction");
                test_caching<object>("pObject");
                test_caching<ArrayList>("pArrayList");
            };

            It should_correctly_get_and_set_a_value = () =>
            {
                foreach (var fieldname in validFieldnames)
                {
                    var aValue = get_a_value(fieldname);
                    sut.Fields(fieldname).Value = aValue;
                    sut.Fields(fieldname).Value.ShouldEqual(aValue);

                }

                test_equality<int>("_int");
                test_equality<string>("_string");
                test_equality<DateTime>("_date");
                test_equality<Action>("_action");
                test_equality<object>("_object");
                test_equality<ArrayList>("_arrayList");
                test_equality<int>("pInt");
                test_equality<string>("pString");
                test_equality<DateTime>("pDate");
                test_equality<Action>("pAction");
                test_equality<object>("pObject");
                test_equality<ArrayList>("pArrayList");

            };

            private static GetterSetter<FieldClass, int> invalidTypedResult;
            private static GetterSetter<FieldClass> invalidResult;
            private static string invalidFieldname;
            private static string[] validFieldnames;
        }

        [Subject(typeof(ReflectedInstance), "Reflection")]
        public class when_accessing_a_field_of_an_unknown_type : concern<ReflectedInstance>
        {
            Establish e = () =>
            {
                validFieldnames = typeof(FieldClass).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
                invalidFieldname = "unknown";
                sut_factory.create_using(() => new ReflectedInstance(new FieldClass()));
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

        [Subject(typeof(ReflectedInstance<PropertyClass>), "Reflection")]
        public class when_accessing_a_property : concern<ReflectedInstance<PropertyClass>>
        {
            private static void test_accessor<T>(string fieldname)
            {
                var a = sut.Properties<T>(fieldname);
                a.ShouldNotBeNull();
                a.Accessor.ShouldNotBeNull();
                a.Accessor.Get.ShouldNotBeNull();
                a.Accessor.Set.ShouldNotBeNull();
            }
            private static void test_equality<T>(string fieldname)
            {
                var aValue = (T)get_a_value(fieldname);
                sut.Properties<T>(fieldname).Value = aValue;
                sut.Properties<T>(fieldname).Value.ShouldEqual(aValue);
            }
            private static void test_caching<T>(string fieldname)
            {
                var gsa = sut.Properties<T>(fieldname);
                ReferenceEquals(sut.Properties<T>(fieldname).Accessor, gsa.Accessor).ShouldBeTrue();
            }

            private Establish e = () =>
            {
                validPropnames = typeof(PropertyClass).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
                invalidPropname = "unknown";
            };

            Because context = () =>
            {
                invalidTypedResult = sut.Properties<int>(invalidPropname);
                invalidResult = sut.Properties(invalidPropname);
            };


            It should_return_a_valid_getter_setter_for_a_valid_property = () =>
            {
                foreach (var propname in validPropnames)
                {
                    var res = sut.Properties(propname);
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

            It should_return_null_for_an_invalid_property = () =>
            {
                invalidResult.ShouldBeNull();
                invalidTypedResult.ShouldBeNull();
            };

            It should_retrieve_the_accessor_from_the_cache_after_first_retrieve = () =>
            {
                foreach (var propname in validPropnames)
                {
                    var gsa = sut.Properties(propname);
                    ReferenceEquals(sut.Properties(propname).Accessor, gsa.Accessor).ShouldBeTrue();
                }

                test_caching<int>("_int");
                test_caching<string>("_string");
                test_caching<DateTime>("_date");
                test_caching<Action>("_action");
                test_caching<object>("_object");
                test_caching<ArrayList>("_arrayList");
                test_caching<int>("pInt");
                test_caching<string>("pString");
                test_caching<DateTime>("pDate");
                test_caching<Action>("pAction");
                test_caching<object>("pObject");
                test_caching<ArrayList>("pArrayList");
            };

            It should_correctly_get_and_set_a_value = () =>
            {
                foreach (var propname in validPropnames)
                {
                    var aValue = get_a_value(propname);
                    sut.Properties(propname).Value = aValue;
                    sut.Properties(propname).Value.ShouldEqual(aValue);

                }

                test_equality<int>("_int");
                test_equality<string>("_string");
                test_equality<DateTime>("_date");
                test_equality<Action>("_action");
                test_equality<object>("_object");
                test_equality<ArrayList>("_arrayList");
                test_equality<int>("pInt");
                test_equality<string>("pString");
                test_equality<DateTime>("pDate");
                test_equality<Action>("pAction");
                test_equality<object>("pObject");
                test_equality<ArrayList>("pArrayList");

            };

            private static GetterSetter<PropertyClass, int> invalidTypedResult;
            private static GetterSetter<PropertyClass> invalidResult;
            private static string invalidPropname;
            private static string[] validPropnames;
        }

        [Subject(typeof(ReflectedInstance), "Reflection")]
        public class when_accessing_a_property_of_an_unknown_type : concern<ReflectedInstance>
        {
            Establish e = () =>
            {
                validPropnames = typeof(PropertyClass).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
                invalidPropname = "unknown";
                sut_factory.create_using(() => new ReflectedInstance(new PropertyClass()));
            };

            Because context = () =>
            {
                invalidResult = sut.Properties(invalidPropname);
            };

            It should_return_a_valid_getter_setter_for_a_valid_field = () =>
            {
                foreach (var propname in validPropnames)
                {
                    var res = sut.Properties(propname);
                    res.Accessor.ShouldNotBeNull();
                    res.ShouldNotBeNull();
                    res.Accessor.Get.ShouldNotBeNull();
                    res.Accessor.Set.ShouldNotBeNull();
                }
            };

            It should_return_null_for_an_invalid_property = () =>
            {
                invalidResult.ShouldBeNull();
            };

            It should_retrieve_the_accessor_from_the_cache_after_first_retrieve = () =>
            {
                foreach (var propname in validPropnames)
                {
                    var res = sut.Properties(propname);
                    ReferenceEquals(sut.Properties(propname).Accessor, res.Accessor).ShouldBeTrue();
                }
            };

            private It should_correctly_get_and_set_a_value = () =>
            {
                foreach (var propname in validPropnames)
                {
                    var aValue = get_a_value(propname);
                    sut.Properties(propname).Value = aValue;
                    sut.Properties(propname).Value.ShouldEqual(aValue);
                }
            };

            It should_set_a_value_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new PropertyClass();
                var propname = "pInt";
                var fv = (int)get_a_value(propname);
                fc.pInt = fv;

                sut.Properties(propname).Value = fv;

                for (var i = 0; i < 10; i++) sut.Properties(propname).Value = fv;

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) sut.Properties(propname).Value = fv;
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                fc.pInt = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fc.pInt = fv;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static GetterSetter invalidResult;
            private static string invalidPropname;
            private static string[] validPropnames;
        }

    }
}
