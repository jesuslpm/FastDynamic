using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastDynamic;

namespace FastDynamic.Samples
{
    // It works with internal classes
    internal class MyClass
    {
        public string Field1;
        public int Prop1 { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ItWorksWithAnonimousTypes();
            MicroBenchMark();
        }


        static void ItWorksWithAnonimousTypes()
        {
            var obj = new { Name = "Almudena López", Age = 10 };
            Console.WriteLine("Name: {0}, Age: {1}\n", obj.GetMemberValue("Name"), obj.GetMemberValue("Age"));
        }

        static void MicroBenchMark()
        {
            const int n = 1000000;
            Type type = typeof(MyClass);

            Console.Write("Using reflection.. ");
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < n; i++)
            {
                Type t = typeof(MyClass);
                object obj = Activator.CreateInstance(t);
                var pi = t.GetProperty("Prop1");
                pi.SetValue(obj, 6);
                var prop1 = (int)pi.GetValue(obj);

                var fi = t.GetField("Field1");
                fi.SetValue(obj, "5");
                var fiel1 = (string)fi.GetValue(obj);

            }
            watch.Stop();
            Console.WriteLine("{0} ms", watch.ElapsedMilliseconds);


            Console.Write("Using CreateObject, GetMemberValue and SetMemberValue.. ");
            watch.Restart();
            for (int i = 0; i < n; i++)
            {
                object obj = typeof(MyClass).CreateObject();

                obj.SetMemberValue("Prop1", 6);
                var prop1 = (int)obj.GetMemberValue("Prop1");

                obj.SetMemberValue("Field1", "5");
                var field1 = (string)obj.GetMemberValue("Field1");
            }
            watch.Stop();
            Console.WriteLine("{0} ms", watch.ElapsedMilliseconds);


            Console.Write("Using getters, setters and activator .. ");
            watch.Restart();

            var activator = type.GetActivator(); 
            var fieldSetter = type.GetSetter("Field1");
            var fieldGetter = type.GetGetter("Field1"); 
            var propSetter = type.GetSetter("Prop1");
            var propGetter = type.GetGetter("Prop1");

            for (int i = 0; i < n; i++)
            {
                object obj = activator();

                propSetter(obj, 6);
                var prop1 = (int)propGetter(obj);

                fieldSetter(obj, "5");
                var field1 = (string)fieldGetter(obj);
            }

            Console.WriteLine("{0} ms", watch.ElapsedMilliseconds);

            Console.Write("Known at compile time.. ");
            watch.Restart();
            for (int i = 0; i < n; i++)
            {
                var obj = new MyClass();

                obj.Prop1 = 6;
                var prop1 = obj.Prop1;

                obj.Field1 = "5";
                var field1 = obj.Field1;
            }
            Console.WriteLine("{0} ms", watch.ElapsedMilliseconds);
        }
    }
}
