using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Testing;
using Willow.Testing.Core;
using Willow.Testing.Observations.RhinoMocks;
using Willow.Testing.Extensions;

namespace Willow.Reflection.Specs
{
    class MethodKeySpec
    {
        [Subject(typeof(MethodKey), "Reflection")]
        public class when_comparing_a_methodkey : Observes<MethodKey>
        {
            Establish e = () => 
            {
                aString = fake.a_sealed(()=> "blabla");
                aReturnType = fake.a_sealed(() => typeof(int));
                firstType = fake.a_sealed(() => typeof(DateTime));
                secondType = fake.a_sealed(() => typeof(List<string>));
                
                sut_factory.create_using(() => new MethodKey(aString, aReturnType, firstType, secondType));
            };

            Because context = () => { };

            It should_equal_to_itself = () => 
            {
                sut.Equals(sut).ShouldBeTrue(); 
            };
            
            It should_equal_to_a_methodkey_created_with_the_same_input = () => 
            { 
                sut.ShouldEqual(new MethodKey(new string(aString.ToCharArray()), aReturnType, firstType, secondType)); 
            };

            It should_have_same_hash_to_a_methodkey_created_with_the_same_input = () => 
            {
                var sutHash = sut.GetHashCode();
                var cloneHash = new MethodKey(new string(aString.ToCharArray()), aReturnType, firstType, secondType).GetHashCode();
                sutHash.ShouldEqual(cloneHash);
            };

            It should_not_equal_to_a_different_methodkey = () => 
            {
                var anotherString = fake.a_sealed(() => "soft");
                var anotherReturn = fake.a_sealed(() => typeof(double));
                var anotherFirstType = fake.a_sealed(() => typeof(int));
                var anotherSecondType = fake.a_sealed(() => typeof(string));

                sut.ShouldNotEqual(new MethodKey(anotherString, anotherReturn, anotherFirstType, anotherSecondType));
            };
            
            It should_not_have_same_hash_to_a_methodkey_created_with_different_input = () => 
            {
                var sutHash = sut.GetHashCode();

                var anotherString = fake.a_sealed(() => "soft");
                var anotherReturn = fake.a_sealed(() => typeof(double));
                var anotherFirstType = fake.a_sealed(() => typeof(int));
                var anotherSecondType = fake.a_sealed(() => typeof(string));
                var cloneHash = new MethodKey(anotherString, anotherReturn, anotherFirstType, anotherSecondType).GetHashCode();

                sutHash.ShouldNotEqual(cloneHash);
            };

            private static string aString;
            private static Type aReturnType;
            private static Type firstType;
            private static Type secondType;
        }
    }
}
