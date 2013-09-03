using System;
using System.Linq;
using System.Collections;
using Machine.Specifications;
using System.Reflection;
using Willow.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using Willow.Testing.Observations.RhinoMocks;
using System.Diagnostics;

namespace Willow.Reflection.Specs
{
    public class DynamicMethodGeneratorSpecs
    {
        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_field_setter : Observes
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate.DynamicInvoke(fc, Convert.ChangeType(fieldDefault, fields[i].FieldType));
                    fields[i].GetValue(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new FieldClass();
                var fd = DynamicMethodGenerator.GenerateInstanceFieldSetter<FieldClass, object>(fields[0]);
                var fv = fields[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                for (var i = 0; i < 10; i++)
                {
                    fd(fc, fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fc, fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fc.pIntField = (int) fv;
                sw.Stop();
                var factor = ((float) elapsed/(float) sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_field_getter : Observes
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fields[i].SetValue(fc, fieldDefault);
                    fieldDelegate.DynamicInvoke(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new FieldClass();
                var fd = DynamicMethodGenerator.GenerateInstanceFieldGetter<FieldClass, object>(fields[0]);
                var fv = fields[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                fields[0].SetValue(fc, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd(fc);

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd(fc);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                fc.pIntField = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fc.pIntField;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_property_setter : Observes
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate.DynamicInvoke(fc, Convert.ChangeType(fieldDefault, props[i].PropertyType));
                    props[i].GetGetMethod(true).Invoke(fc, null).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new PropertyClass();
                var fd = DynamicMethodGenerator.GenerateInstancePropertySetter<PropertyClass, object>(props[0]);
                var fv = props[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                for (var i = 0; i < 10; i++)
                {
                    fd(fc, fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fc, fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fc.IntProperty = (int)fv;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_property_getter : Observes
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    props[i].GetSetMethod(true).Invoke(fc, new object[] { fieldDefault});
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    props[i].SetValue(fc, fieldDefault);
                    fieldDelegate.DynamicInvoke(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fc = new PropertyClass();
                var fd = DynamicMethodGenerator.GenerateInstancePropertyGetter<PropertyClass, object>(props[0]);
                var fv = props[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                props[0].SetValue(fc, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd(fc);

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd(fc);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                fc.IntProperty = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fc.IntProperty;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_field_setter : Observes
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate(fieldDefault);
                    fields[i].GetValue(null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_field = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticFieldSetter<StaticFieldClass, object>(fields[i]);
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate.DynamicInvoke(Convert.ChangeType(fieldDefault, fields[i].FieldType));
                    fields[i].GetValue(null).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticFieldSetter<StaticFieldClass, object>(fields[0]);
                var fv = fields[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                for (var i = 0; i < 10; i++)
                {
                    fd(fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) StaticFieldClass.pIntField = (int)fv;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_field_getter : Observes
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fields[i].SetValue(null, fieldDefault);
                    fieldDelegate().ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_field = () =>
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticFieldGetter<StaticFieldClass, object>(fields[i]);
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = fields[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fields[i].SetValue(null, fieldDefault);
                    fieldDelegate.DynamicInvoke().ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticFieldGetter<StaticFieldClass, object>(fields[0]);
                var fv = fields[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                fields[0].SetValue(null, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd();
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                StaticFieldClass.pIntField = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = StaticFieldClass.pIntField;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<FieldInfo> fields;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_property_setter : Observes
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate(fieldDefault);
                    props[i].GetGetMethod(true).Invoke(null, null).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_with_untyped_property = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticPropertySetter<StaticPropertyClass, object>(props[i]);
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate.DynamicInvoke(Convert.ChangeType(fieldDefault, props[i].PropertyType));
                    props[i].GetGetMethod(true).Invoke(null, null).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticPropertySetter<StaticPropertyClass, object>(props[0]);
                var fv = props[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                for (var i = 0; i < 10; i++)
                {
                    fd(fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) fd(fv);
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) StaticPropertyClass.IntProperty = (int)fv;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

        [Subject(typeof(DynamicMethodGenerator), "Reflection")]
        public class when_creating_a_static_property_getter : Observes
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    props[i].GetSetMethod(true).Invoke(null, new object[] { fieldDefault });
                    fieldDelegate().ShouldEqual(fieldDefault);
                }
            };


            It should_return_a_valid_delegate_when_working_with_untyped_property = () =>
            {
                for (var i = 0; i < props.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateStaticPropertyGetter<StaticPropertyClass, object>(props[i]);
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
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
                    var fieldDefault = props[i].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                    props[i].SetValue(null, fieldDefault);
                    fieldDelegate.DynamicInvoke().ShouldEqual(fieldDefault);
                }
            };

            It should_execute_max_3_times_slower_then_direct_call = () =>
            {
                var fd = DynamicMethodGenerator.GenerateStaticPropertyGetter<StaticPropertyClass, object>(props[0]);
                var fv = props[0].GetCustomAttributes(typeof(DefaultValueAttribute)).OfType<DefaultValueAttribute>().First().Value;
                props[0].SetValue(null, fv);
                object res;
                for (var i = 0; i < 10; i++) res = fd();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = fd();
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                StaticPropertyClass.IntProperty = (int)fv;
                sw.Restart();
                for (var i = 0; i < 10 * 1000 * 1000; i++) res = StaticPropertyClass.IntProperty;
                sw.Stop();
                var factor = ((float)elapsed / (float)sw.ElapsedMilliseconds);

                Debug.WriteLine("Debug Performance: Reflection = {0} ms, Direct call = {1} ms, Factor = {2}", elapsed, sw.ElapsedMilliseconds, factor);
                factor.ShouldBeLessThanOrEqualTo(3.0);
            };

            private static List<PropertyInfo> props;
        }

        public class FieldClass 
        {
            [DefaultValue(5)]
            private int _intField;
            [DefaultValue("boe")]
            private string _stringField;
            [DefaultValue(typeof(DateTime), "")]
            private DateTime _dateField;
            [DefaultValue(typeof(Action), null)]
            private Action _actionField;
            [DefaultValue(typeof(object), null)]
            private object _objectField;
            [DefaultValue(typeof(ArrayList),null)]
            private ArrayList _arrayListField;

            [DefaultValue(5)]
            public int pIntField;
            [DefaultValue("boe")]
            public string pStringField;
            [DefaultValue(typeof(DateTime), "")]
            public DateTime pDateField;
            [DefaultValue(typeof(Action), null)]
            public Action pActionField;
            [DefaultValue(typeof(object), null)]
            public object pObjectField;
            [DefaultValue(typeof(ArrayList), null)]
            public ArrayList pArrayListField;

        }
        public class PropertyClass
        {
            [DefaultValue(5)]
            private int pIntProperty { get; set; }
            [DefaultValue("boe")]
            private string pStringProperty { get; set; }
            [DefaultValue(typeof(DateTime), "")]
            private DateTime pDateProperty { get; set; }
            [DefaultValue(typeof(Action), null)]
            private Action pActionProperty { get; set; }
            [DefaultValue(typeof(object), null)]
            private object pObjectProperty { get; set; }
            [DefaultValue(typeof(ArrayList), null)]
            private ArrayList pArrayListProperty { get; set; }

            [DefaultValue(5)]
            public int IntProperty { get; set; }
            [DefaultValue("boe")]
            public string StringProperty { get; set; }
            [DefaultValue(typeof(DateTime), "")]
            public DateTime DateProperty { get; set; }
            [DefaultValue(typeof(Action), null)]
            public Action ActionProperty { get; set; }
            [DefaultValue(typeof(object), null)]
            public object ObjectProperty { get; set; }
            [DefaultValue(typeof(ArrayList), null)]
            public ArrayList ArrayListProperty { get; set; }

        }
        public class StaticFieldClass
        {
            [DefaultValue(5)]
            private static int _intField;
            [DefaultValue("boe")]
            private static string _stringField;
            [DefaultValue(typeof(DateTime), "")]
            private static DateTime _dateField;
            [DefaultValue(typeof(Action), null)]
            private static Action _actionField;
            [DefaultValue(typeof(object), null)]
            private object _objectField;
            [DefaultValue(typeof(ArrayList), null)]
            private static ArrayList _arrayListField;

            [DefaultValue(5)]
            public static int pIntField;
            [DefaultValue("boe")]
            public static string pStringField;
            [DefaultValue(typeof(DateTime), "")]
            public static DateTime pDateField;
            [DefaultValue(typeof(Action), null)]
            public static Action pActionField;
            [DefaultValue(typeof(object), null)]
            public static object pObjectField;
            [DefaultValue(typeof(ArrayList), null)]
            public static ArrayList pArrayListField;

        }
        public class StaticPropertyClass
        {
            [DefaultValue(5)]
            private static int pIntProperty { get; set; }
            [DefaultValue("boe")]
            private static string pStringProperty { get; set; }
            [DefaultValue(typeof(DateTime), "")]
            private static DateTime pDateProperty { get; set; }
            [DefaultValue(typeof(Action), null)]
            private static Action pActionProperty { get; set; }
            [DefaultValue(typeof(object), null)]
            private static object pObjectProperty { get; set; }
            [DefaultValue(typeof(ArrayList), null)]
            private static ArrayList pArrayListProperty { get; set; }

            [DefaultValue(5)]
            public static int IntProperty { get; set; }
            [DefaultValue("boe")]
            public static string StringProperty { get; set; }
            [DefaultValue(typeof(DateTime), "")]
            public static DateTime DateProperty { get; set; }
            [DefaultValue(typeof(Action), null)]
            public static Action ActionProperty { get; set; }
            [DefaultValue(typeof(object), null)]
            public static object ObjectProperty { get; set; }
            [DefaultValue(typeof(ArrayList), null)]
            public static ArrayList ArrayListProperty { get; set; }

        }
    }
}
