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
        [Subject(typeof(DynamicMethodGenerator))]
        public class when_creating_a_field_setter : Observes
        {
            Because b = () => 
            {
                fields = typeof(FieldClass).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            };

            It should_return_a_valid_delegate_when_working_untyped = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var fieldDelegate = DynamicMethodGenerator.GenerateInstanceFieldSetter<FieldClass, object>(fields[i]);
                    var fieldDefault = fields[i].CustomAttributes.OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate(fc, fieldDefault);
                    fields[i].GetValue(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_return_a_valid_delegate_when_working_typed = () =>
            {
                var fc = new FieldClass();
                for (var i = 0; i < fields.Count; i++)
                {
                    var method = typeof(DynamicMethodGenerator).GetMethods().Where(x => x.Name.Equals("GenerateInstanceFieldSetter") && x.GetParameters().First().ParameterType == typeof(FieldInfo) && x.GetGenericArguments().Count() == 2).FirstOrDefault();
                    method = method.MakeGenericMethod(typeof(FieldClass), fields[i].FieldType);
                    var fieldDelegate = method.Invoke(null, new object[] { fields[i] }) as Delegate;
                    var fieldDefault = fields[i].CustomAttributes.OfType<DefaultValueAttribute>().First().Value;
                    fieldDelegate.DynamicInvoke(fc, Convert.ChangeType(fieldDefault, fields[i].FieldType));
                    fields[i].GetValue(fc).ShouldEqual(fieldDefault);
                }
            };

            It should_execute_1M_times_in_less_then_1_second = () =>
            {
                var fc = new FieldClass();
                var fd = DynamicMethodGenerator.GenerateInstanceFieldSetter<FieldClass, object>(fields[0]);
                var fv = fields[0].CustomAttributes.OfType<DefaultValueAttribute>().First().Value;
                for (var i = 0; i < 10; i++)
                {
                    fd(fc, fv);
                }

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 1000000; i++)
                {
                    fd(fc, fv);
                }
                sw.Stop();
                sw.ElapsedMilliseconds.ShouldBeLessThan(1000);
            };

            private static List<FieldInfo> fields;
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
    }
}
