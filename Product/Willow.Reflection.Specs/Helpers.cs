using System;
using System.Collections;
using System.ComponentModel;

namespace Willow.Reflection.Specs
{
    public class FieldClass
    {
        private int _int;
        private string _string;
        private DateTime _date;
        private Action _action;
        private object _object;
        private ArrayList _arrayList;

        public int pInt;
        public string pString;
        public DateTime pDate;
        public Action pAction;
        public object pObject;
        public ArrayList pArrayList;
    }

    public class StaticFieldClass
    {
        private static int _int;
        private static string _string;
        private static DateTime _date;
        private static Action _action;
        private static object _object;
        private static ArrayList _arrayList;

        public static int pInt;
        public static string pString;
        public static DateTime pDate;
        public static Action pAction;
        public static object pObject;
        public static ArrayList pArrayList;
    }

    public class PropertyClass
    {
        private int _int { get; set; }
        private string _string { get; set; }
        private DateTime _date { get; set; }
        private Action _action { get; set; }
        private object _object { get; set; }
        private ArrayList _arrayList { get; set; }

        public int pInt { get; set; }
        public string pString { get; set; }
        public DateTime pDate { get; set; }
        public Action pAction { get; set; }
        public object pObject { get; set; }
        public ArrayList pArrayList { get; set; }
    }

    public class StaticPropertyClass
    {
        private static int _int { get; set; }
        private static string _string { get; set; }
        private static DateTime _date { get; set; }
        private static Action _action { get; set; }
        private static object _object { get; set; }
        private static ArrayList _arrayList { get; set; }

        public static int pInt { get; set; }
        public static string pString { get; set; }
        public static DateTime pDate { get; set; }
        public static Action pAction { get; set; }
        public static object pObject { get; set; }
        public static ArrayList pArrayList { get; set; }
    }

    public static class ClassValues
    {
        public static int _int = 5;
        public static string _string = "boe";
        public static DateTime _date = new DateTime(2007, 5, 5);
        public static Action _action = () => { var x = "blabla"; };
        public static object _object = new { Naam = "R", Lengte = "120" };
        public static ArrayList _arrayList = new ArrayList(6);

        public static int pInt = 5;
        public static string pString = "Boe";
        public static DateTime pDate = new DateTime(2003, 12, 3);
        public static Action pAction = () => { var x = "bla"; };
        public static object pObject = new { Name = "G", Length = "140" };
        public static ArrayList pArrayList = new ArrayList(10);
    }

}