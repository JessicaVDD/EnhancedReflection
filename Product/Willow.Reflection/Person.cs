using System;

namespace GradualData
{
    public class Person
    {
        private static long _LastId = 1;
        public static Person CreateNew(string name)
        {
            return new Person {Id = _LastId++, Lastname = name};
        }

        public static long LastId { get { return _LastId; } set { _LastId = value; }}

        private long _ANiceId;
        public long Id { get { return _ANiceId; } set { _ANiceId = value; } } 
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public DateTime BirthDate { get; set; }
        public Address Address { get; set; }

        public void CreateAddress(long id, string street, string number)
        {
            Address = new Address {Id = id, Street = street, Number = number};
        }
        public long CreateAddress(long id, string street, string number, long cityId)
        {
            Address = new Address { Id = id, Street = street, Number = number, CityId = 9990};
            return Address.Id;
        }

    }

    public class Address
    {
        public long Id { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public long CityId { get; set; }
    }
}