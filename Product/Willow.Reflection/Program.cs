using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;

namespace GradualData
{
    class Program
    {
        static void Main(string[] args)
        {
            var tc = new TestClass();
            tc.Test();

            Console.ReadLine();
            return;
            //Data Source="c:\users\peter_v.radar.000\My Test Sources\GradualData\GradualData\GradualTestData.sdf"
            var s = new SqlCeConnection(@"Data Source=c:\users\peter_v.radar.000\My Test Sources\GradualData\GradualData\GradualTestData.sdf");
            var c = s.CreateCommand();
            c.CommandText = "Select * from person";
            c.CommandType = CommandType.Text;

            var persons = new List<Person>();
            try
            {
                s.Open();
                var reader = c.ExecuteReader();

                var firstRun = true;
                int idColumn = 0;
                int firstnameColumn = 0;
                int middlenameColumn = 0;
                int lastnameColumn = 0;
                int addressColumn = 0;
                int birthdayColumn = 0;

                while (reader.Read())
                {
                    if (firstRun)
                    {
                        idColumn = reader.GetOrdinal("Id");
                        firstnameColumn = reader.GetOrdinal("Firstname");
                        middlenameColumn = reader.GetOrdinal("Middlename");
                        lastnameColumn = reader.GetOrdinal("Lastname");
                        birthdayColumn = reader.GetOrdinal("Birthday");
                        addressColumn = reader.GetOrdinal("AddressId");
                        firstRun = false;
                    }

                    persons.Add(new Person
                    {
                        Id = reader.IsDBNull(idColumn) ? 0 : reader.GetInt64(idColumn),
                        Firstname = reader.IsDBNull(firstnameColumn) ? null : reader.GetString(firstnameColumn),
                        Middlename = reader.IsDBNull(middlenameColumn) ? null : reader.GetString(middlenameColumn),
                        Lastname = reader.IsDBNull(lastnameColumn) ? null : reader.GetString(lastnameColumn),
                        BirthDate = reader.IsDBNull(birthdayColumn) ? new DateTime() : reader.GetDateTime(birthdayColumn)
                    });
                }

                s.Close();

                foreach (var p in persons)
                {
                    Console.WriteLine(p.Firstname);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception has occurred: {0}, {1}", ex.Message, ex.GetType());
            }
            Console.ReadLine();
        }
    }
}
