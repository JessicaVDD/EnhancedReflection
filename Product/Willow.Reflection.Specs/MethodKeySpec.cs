using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Testing.Observations.RhinoMocks;

namespace Willow.Reflection.Specs
{
    class MethodKeySpec
    {
        [Subject(typeof(MethodKey), "Reflection")]
        public class when_comparing_a_methodkey : Observes
        {
            Establish e = () => {};

            Because context = () => { };

            It should_equal_to_itself = () => { true.ShouldBeFalse(); };
            It should_equal_to_a_methodkey_created_with_the_same_input = () => { true.ShouldBeFalse(); };
            It should_have_same_hash_to_a_methodkey_created_with_the_same_input = () => { true.ShouldBeFalse(); };
            It should_not_equal_to_a_different_methodkey = () => { true.ShouldBeFalse(); };
            It should_not_have_same_hash_to_a_methodkey_created_with_different_input = () => { true.ShouldBeFalse(); };
        }
    }
}
