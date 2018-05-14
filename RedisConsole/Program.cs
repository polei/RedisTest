using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace RedisConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");

            var Configuration = builder.Build();

            Console.WriteLine($"option1 = {Configuration["Option1"]}");
            Console.WriteLine($"option2 = {Configuration["option2"]}");
            Console.WriteLine(
                $"suboption1 = {Configuration["subsection:suboption1"]}");
            Console.WriteLine();

            Console.WriteLine("Wizards:");
            Console.Write($"{Configuration["wizards:0:Name"]}, ");
            Console.WriteLine($"age {Configuration["wizards:0:Age"]}");
            Console.Write($"{Configuration["wizards:1:Name"]}, ");
            Console.WriteLine($"age {Configuration["wizards:1:Age"]}");
            Console.WriteLine();

            Console.WriteLine("Press a key...");


            try
            {
                //RedisInsertString();
                DatabaseInsert();
            }
            catch(Exception e)
            {

            }


            Console.ReadLine();
        }

        static void RedisInsertString()
        {
            var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379");

            StackExchange.Redis.IDatabase db = redis.GetDatabase();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Enumerable.Range(0, 100000).ToList().ForEach(a => {
                db.StringSetAsync($"strKey{a.ToString().PadLeft(7, '0')}", $"strValue{a.ToString().PadLeft(7, '0')}");
                if (a % 1000 == 0)
                {
                    Console.WriteLine($"it takes {sw.Elapsed.TotalMilliseconds}mm");
                }
            });
            sw.Stop();

            Console.WriteLine($"it takes {sw.Elapsed.TotalMilliseconds}mm");
        }

        static void DatabaseInsert()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var list = new List<DB.Models.Student>();

            Enumerable.Range(0, 100000).ToList().ForEach(a => {
                list.Add(new DB.Models.Student {
                    Name = $"strKey{a.ToString().PadLeft(7, '0')}",
                    Address = $"strValue{a.ToString().PadLeft(7, '0')}"
                });

                if (a % 1 == 0)
                {
                    var context1 = new DB.Models.EFTestContext();
                    context1.Student.AddRange(list);
                    context1.SaveChanges();

                    list.Clear();
                    Console.WriteLine($"it takes {sw.Elapsed.TotalMilliseconds}mm");
                }
            });
            var context = new DB.Models.EFTestContext();
            context.SaveChanges();
            list.Clear();
            sw.Stop();

            Console.WriteLine($"total it takes {sw.Elapsed.TotalMilliseconds}mm");
        }
    }
}
