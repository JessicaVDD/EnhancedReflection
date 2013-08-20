using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Willow.Reflection;

namespace Willow.Reflection
{
    public class TestClass
    {
        public void Test()
        {
            var p = new Person
            {
                Id = 15, 
                Lastname = "Van den Daele", 
                Firstname = "Gabriël", 
                Middlename = "(none)", 
                BirthDate = new DateTime(2003, 12, 3),
                Address = new Address
                {
                    CityId = 1,
                    Id = 2,
                    Number = "1",
                    Street = "Bommelare"
                }
            };

            var maxLoop = 10000000;
            var riPerson = new ReflectedInstance<Person>(p);
            var personId = riPerson.Fields<long>("_ANiceId").Value; //provoque caching
            Console.WriteLine("Test initialized: Id={0}", personId);

            //---------------------------------------------------
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < maxLoop; i++)
            {
                var tmp = riPerson.Fields<long>("_ANiceId").Value;
            }
            sw.Stop();
            Console.WriteLine("Full run: {0}", sw.ElapsedMilliseconds );

            //---------------------------------------------------
            var getterVar = riPerson.Fields<long>("_ANiceId");
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                var tmp = getterVar.Value;
            }
            sw.Stop();
            Console.WriteLine("Getter cached run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            var dynMeth = riPerson.Fields<long>("_ANiceId").Accessor.Get;
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                var tmp = dynMeth(p);
            }
            sw.Stop();
            Console.WriteLine("DynMethod cached run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                var tmp = p.Id;
            }
            sw.Stop();
            Console.WriteLine("Direct access property run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            var fi = typeof(Person).GetField("_ANiceId", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null) throw new ArgumentException("The id has not be found with the GetField method.");
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                var tmp = (long) fi.GetValue(p);
            }
            sw.Stop();
            Console.WriteLine("GetField cached run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            var mi = typeof(Person).GetMethod("CreateAddress", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(long), typeof(string), typeof(string) }, null);
            if (mi == null) throw new ArgumentException("The CreateAddress method has not be found with the GetMethod method.");
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                mi.Invoke(p, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] {10, "Urselweg", "21A"}, null);
            }
            sw.Stop();
            Console.WriteLine("GetMethod cached run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            var caDel = riPerson.Method<Action<Person, long, string, string>>("CreateAddress");
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                caDel(p, 10, "Urselweg", "21A");
            }
            sw.Stop();
            Console.WriteLine("Method cached run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                p.CreateAddress(10, "Urselweg", "21A");
            }
            sw.Stop();
            Console.WriteLine("Direct Method call run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            var mi2 = typeof(Person).GetMethod("CreateAddress", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(long), typeof(string), typeof(string), typeof(long) }, null);
            if (mi2 == null) throw new ArgumentException("The CreateAddress method has not be found with the GetMethod method.");
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                var res = (long) mi2.Invoke(p, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] { 10, "Urselweg", "21A", 9990 }, null);
            }
            sw.Stop();
            Console.WriteLine("GetMethod cached with return run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            var caDel2 = riPerson.Method<Func<Person, long, string, string, long, long>>("CreateAddress");
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                var res = caDel2(p, 10, "Urselweg", "21A", 9990);
            }
            sw.Stop();
            Console.WriteLine("The result: {0}", caDel2(p, 11, "Urselweg", "21A", 9990));
            Console.WriteLine("Method cached with return run: {0}", sw.ElapsedMilliseconds);

            //---------------------------------------------------
            sw.Restart();
            for (var i = 0; i < maxLoop; i++)
            {
                var res = p.CreateAddress(10, "Urselweg", "21A", 9990);
            }
            sw.Stop();
            Console.WriteLine("Direct Method call with return run: {0}", sw.ElapsedMilliseconds);


            //Test if everything works:
            var lastId = riPerson.Static.Fields<long>("_LastId").Value;
            var lastId2 = (long)riPerson.Static.Fields("_LastId").Value;
            var lastIdProp = riPerson.Static.Properties<long>("LastId").Value;
            var lastIdProp2 = (long)riPerson.Static.Properties("LastId").Value;
            var np = riPerson.Static.Method("CreateNew", typeof(Person), typeof(string)).DynamicInvoke("Tribal") as Person;
            var np2 = riPerson.Static.Method<Func<string, Person>>("CreateNew")("No" + p.Lastname);

            var t = "boe";

            var reflectedPerson = new ReflectedInstance<Person>(p);
            object idFieldAsObject = reflectedPerson.Fields("_Id").Value;
            long idFieldAsLong = reflectedPerson.Fields<long>("_Id").Value;

            object idPropertyAsObject = reflectedPerson.Properties("Id").Value;
            long idPropertyAsLong = reflectedPerson.Properties<long>("Id").Value;

            string stringReturningMethod = reflectedPerson.Method<Func<Person, long, string>>("ConvertIdToString")(p, 5);
            string strReturningDelegate = (string)reflectedPerson.Method("ConvertIdToString", typeof(string), typeof(Person), typeof(long)).DynamicInvoke(p, new object[] { 5 });

            //the functions to cache:
            var fieldSetter = reflectedPerson.Fields("_Id").Accessor.Set;
            var propertyGetter = reflectedPerson.Properties("Id").Accessor.Get;
            var typedMethod = reflectedPerson.Method<Func<Person, long, string>>("ConvertIdToString");
            var method = reflectedPerson.Method("ConvertIdToString", typeof(string), typeof(Person), typeof(long));
        } 
    }
}