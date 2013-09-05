using System;
using System.Linq;
using System.Collections;
using Machine.Specifications;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using Willow.Testing.Observations.RhinoMocks;
using System.Diagnostics;

namespace Willow.Reflection.Specs
{
    public class DynamicMethodGeneratorSpecs
    {
        public class concern : Observes
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

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_field_setter : concern
        {
            Because context = () => 
            {
                fields = typeof(FieldClass).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstanceFieldSetter<object, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fieldDelegate(fc, fieldDefault);
                    fields[i].GetValue(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_field = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstanceFieldSetter<FieldClass, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fieldDelegate(fc, fieldDefault);
                    fields[i].GetValue(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateInstanceFieldSetter") && x.GetParameters().First().ParameterType == typeof(FieldInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(FieldClass), fields[i].FieldType);
                    var fieldDelegate = method.Invoke(null, new object[] { fields[i] }) as Delegate;
                    var fieldDefault = get_a_value(fields[i]);
                    fieldDelegate.DynamicInvoke(fc, fieldDefault);
                    fields[i].GetValue(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new FieldClass();
                var fd = DynamicMethodGenerator.GenerateInstanceFieldSetter<FieldClass, object>(fields[0]);
                var fv = get_a_value(fields[0]);
                for (var i = 0; i < 10; i++)
                {
                    fd(fc, fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fc, fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fc.pInt = (int) fv;
                sw.Stop();
                var factor = ((float) elapsed/(float) sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_field_getter : concern
        {
            Because context = () =>
            {
                fields = typeof(FieldClass).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstanceFieldGetter<object, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fields[i].SetValue(fc, fieldDefault);
                    fieldDelegate(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_field = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstanceFieldGetter<FieldClass, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fields[i].SetValue(fc, fieldDefault);
                    fieldDelegate(fc).ShouldEqual(fieldDefault);                    
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateInstanceFieldGetter") && x.GetParameters().First().ParameterType == typeof(FieldInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(FieldClass), fields[i].FieldType);
                    var fieldDelegate = method.Invoke(null, new object[] { fields[i] }) as Delegate;
                    var fieldDefault = get_a_value(fields[i]);
                    fields[i].SetValue(fc, fieldDefault);
                    fieldDelegate.DynamicInvoke(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new FieldClass();
                var fd = DynamicMethodGenerator.GenerateInstanceFieldGetter<FieldClass, object>(fields[0]);
                var fv = get_a_value(fields[0]);
                fields[0].SetValue(fc, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd(fc);

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd(fc);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                fc.pInt = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fc.pInt;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_property_setter : concern
        {
            Because context = () =>
            {
                props = typeof(PropertyClass).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                var fc = new PropertyClass();
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstancePropertySetter<object, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    fieldDelegate(fc, fieldDefault);
                    props[i].GetGetMethod(true).Invoke(fc, null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_property = () =>
            {
                var fc = new PropertyClass();
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstancePropertySetter<PropertyClass, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    fieldDelegate(fc, fieldDefault);
                    props[i].GetGetMethod(true).Invoke(fc, null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                var fc = new PropertyClass();
                for (var i = 0; i < props.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateInstancePropertySetter") && x.GetParameters().First().ParameterType == typeof(PropertyInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(PropertyClass), props[i].PropertyType);
                    var fieldDelegate = method.Invoke(null, new object[] { props[i] }) as Delegate;
                    var fieldDefault = get_a_value(props[i]);
                    fieldDelegate.DynamicInvoke(fc, fieldDefault);
                    props[i].GetGetMethod(true).Invoke(fc, null).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new PropertyClass();
                var fd = DynamicMethodGenerator.GenerateInstancePropertySetter<PropertyClass, object>(props[0]);
                var fv = get_a_value(props[0]);
                for (var i = 0; i < 10; i++)
                {
                    fd(fc, fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fc, fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fc.pInt = (int)fv;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_property_getter : concern
        {
            Because context = () =>
            {
                props = typeof(PropertyClass).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                var fc = new PropertyClass();
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstancePropertyGetter<object, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    props[i].GetSetMethod(true).Invoke(fc, new object[] { fieldDefault });
                    fieldDelegate(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_property = () =>
            {
                var fc = new PropertyClass();
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstancePropertyGetter<PropertyClass, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    props[i].GetSetMethod(true).Invoke(fc, new object[] { fieldDefault });
                    fieldDelegate(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                var fc = new PropertyClass();
                for (var i = 0; i < props.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateInstancePropertyGetter") && x.GetParameters().First().ParameterType == typeof(PropertyInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(PropertyClass), props[i].PropertyType);
                    var fieldDelegate = method.Invoke(null, new object[] { props[i] }) as Delegate;
                    var fieldDefault = get_a_value(props[i]);
                    props[i].SetValue(fc, fieldDefault);
                    fieldDelegate.DynamicInvoke(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new PropertyClass();
                var fd = DynamicMethodGenerator.GenerateInstancePropertyGetter<PropertyClass, object>(props[0]);
                var fv = get_a_value(props[0]);
                props[0].SetValue(fc, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd(fc);

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd(fc);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                fc.pInt = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fc.pInt;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_field_setter : concern
        {
            Because context = () =>
            {
                fields = typeof(StaticFieldClass).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticFieldSetter<object, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fieldDelegate(fieldDefault);
                    fields[i].GetValue(null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_field = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticFieldSetter<StaticFieldClass, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fieldDelegate(fieldDefault);
                    fields[i].GetValue(null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateStaticFieldSetter") && x.GetParameters().First().ParameterType == typeof(FieldInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(StaticFieldClass), fields[i].FieldType);
                    var fieldDelegate = method.Invoke(null, new object[] { fields[i] }) as Delegate;
                    var fieldDefault = get_a_value(fields[i]);
                    fieldDelegate.DynamicInvoke(fieldDefault);
                    fields[i].GetValue(null).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticFieldSetter<StaticFieldClass, object>(fields[0]);
                var fv = get_a_value(fields[0]);
                for (var i = 0; i < 10; i++)
                {
                    fd(fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) StaticFieldClass.pInt = (int)fv;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_field_getter : concern
        {
            Because context = () =>
            {
                fields = typeof(StaticFieldClass).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticFieldGetter<object, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fields[i].SetValue(null, fieldDefault);
                    fieldDelegate().ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_field = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticFieldGetter<StaticFieldClass, object>(fields[i]);
                    var fieldDefault = get_a_value(fields[i]);
                    fields[i].SetValue(null, fieldDefault);
                    fieldDelegate().ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateStaticFieldGetter") && x.GetParameters().First().ParameterType == typeof(FieldInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(StaticFieldClass), fields[i].FieldType);
                    var fieldDelegate = method.Invoke(null, new object[] { fields[i] }) as Delegate;
                    var fieldDefault = get_a_value(fields[i]);
                    fields[i].SetValue(null, fieldDefault);
                    fieldDelegate.DynamicInvoke().ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticFieldGetter<StaticFieldClass, object>(fields[0]);
                var fv = get_a_value(fields[0]);
                fields[0].SetValue(null, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd();
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                StaticFieldClass.pInt = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = StaticFieldClass.pInt;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_property_setter : concern
        {
            Because context = () =>
            {
                props = typeof(StaticPropertyClass).GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticPropertySetter<object, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    fieldDelegate(fieldDefault);
                    props[i].GetGetMethod(true).Invoke(null, null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_property = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticPropertySetter<StaticPropertyClass, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    fieldDelegate(fieldDefault);
                    props[i].GetGetMethod(true).Invoke(null, null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateStaticPropertySetter") && x.GetParameters().First().ParameterType == typeof(PropertyInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(StaticPropertyClass), props[i].PropertyType);
                    var fieldDelegate = method.Invoke(null, new object[] { props[i] }) as Delegate;
                    var fieldDefault = get_a_value(props[i]);
                    fieldDelegate.DynamicInvoke(fieldDefault);
                    props[i].GetGetMethod(true).Invoke(null, null).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticPropertySetter<StaticPropertyClass, object>(props[0]);
                var fv = get_a_value(props[0]);
                for (var i = 0; i < 10; i++)
                {
                    fd(fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) StaticPropertyClass.pInt = (int)fv;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_property_getter : concern
        {
            Because context = () =>
            {
                props = typeof(StaticPropertyClass).GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticPropertyGetter<object, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    props[i].GetSetMethod(true).Invoke(null, new object[] { fieldDefault });
                    fieldDelegate().ShouldEqual(fieldDefault);
                }
            };


            It should_return_a_valid_delegate_when_working_with_untyped_property = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticPropertyGetter<StaticPropertyClass, object>(props[i]);
                    var fieldDefault = get_a_value(props[i]);
                    props[i].GetSetMethod(true).Invoke(null, new object[] { fieldDefault });
                    fieldDelegate().ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().First(x => x.Name.Equals("GenerateStaticPropertyGetter") && x.GetParameters().First().ParameterType == typeof(PropertyInfo) && x.GetGenericArguments().Count() == 2);
                    method = method.MakeGenericMethod(typeof(StaticPropertyClass), props[i].PropertyType);
                    var fieldDelegate = method.Invoke(null, new object[] { props[i] }) as Delegate;
                    var fieldDefault = get_a_value(props[i]);
                    props[i].SetValue(null, fieldDefault);
                    fieldDelegate.DynamicInvoke().ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticPropertyGetter<StaticPropertyClass, object>(props[0]);
                var fv = get_a_value(props[0]);
                props[0].SetValue(null, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd();
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                StaticPropertyClass.pInt = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = StaticPropertyClass.pInt;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

    }
}
