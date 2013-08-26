using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Testing.Observations.RhinoMocks;

namespace Willow.Reflection.Specs
{
    class ReflectedInstanceSpec
    {
        [Subject(typeof(ReflectedInstance<Person>), "Reflection")]
        public class when_creating_a_type_reflection : Observes
        {
            Because context = () => { };

            It should_work = () => { };
        }
    }
}
