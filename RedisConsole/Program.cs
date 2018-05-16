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
            try
            {
                //RedisInsertStringWithTran();
                //DatabaseInsert();

                //SetRedisList();

                //GetDatabaseList();

                GetRedisList();
            }
            catch (Exception e)
            {

            }


            Console.ReadLine();
        }

        //1697
        static List<DB.Models.Student> GetDatabaseList()
        {
            var sw = new System.Diagnostics.Stopwatch();
            var context = new DB.Models.EFTestContext();
            var temp = context.Student.FirstOrDefault();

            sw.Start();

            var list = context.Student.OrderBy(a => a.Id).Skip(0).Take(30 * 10000).ToList();


            sw.Stop();

            Console.WriteLine($"GetDatabaseList takes {sw.Elapsed.TotalMilliseconds}mm");

            return list;
        }

        static void GetRedisList()
        {
            var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379");

            StackExchange.Redis.IDatabase db = redis.GetDatabase();

            var keys = db.HashKeys("database_student_100003");
            var values = db.HashValues("database_student_100003");
        }

        static void SetRedisList()
        {
            var list = GetDatabaseList();

            var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379");

            StackExchange.Redis.IDatabase db = redis.GetDatabase();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var tran = db.CreateTransaction();
            var index = 0;
            list.ForEach(item =>
            {
                index++;
                var dic = ToDic(item).Select(a => new StackExchange.Redis.HashEntry(a.Key, a.Value ?? "")).ToArray();
                tran.HashSetAsync("database_student_" + item.Id, dic);

                if (index % 10000 == 0)
                {
                    tran.Execute();
                    Console.WriteLine($"SetRedisList {index} {sw.Elapsed.TotalMilliseconds}mm");
                }
            });

            tran.Execute();
            sw.Stop();
            Console.WriteLine($"SetRedisList save complete {sw.Elapsed.TotalMilliseconds}mm");
        }

        //49704 
        static void RedisInsertString()
        {
            var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379");

            StackExchange.Redis.IDatabase db = redis.GetDatabase();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Enumerable.Range(0, 100000).ToList().ForEach(a => {
                db.StringSet($"strKey{a.ToString().PadLeft(7, '0')}", $"strValue{a.ToString().PadLeft(7, '0')}");
                if (a % 1000 == 0)
                {
                    Console.WriteLine($"it takes {sw.Elapsed.TotalMilliseconds}mm");
                }
            });
            sw.Stop();

            Console.WriteLine($"total takes {sw.Elapsed.TotalMilliseconds}mm");
        }

        //1043 //不支持异常回滚
        static void RedisInsertStringWithTran()
        {
            var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379");

            StackExchange.Redis.IDatabase db = redis.GetDatabase();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var tran = db.CreateTransaction();

            Enumerable.Range(0, 100000).ToList().ForEach(a => {
                tran.StringSetAsync($"strKey{a.ToString().PadLeft(7, '0')}", $"strValue{a.ToString().PadLeft(7, '0')}");
                if (a % 1000 == 0)
                {
                    string key1 = "key1";
                    tran.SetAddAsync(key1, "key1");
                    tran.SetAddAsync(key1, "key2");
                    tran.SetAddAsync("key1", "key1");
                    if (!tran.Execute())
                    {
                        Console.WriteLine("false");
                    }
                    tran = db.CreateTransaction();
                    Console.WriteLine($"it takes {sw.Elapsed.TotalMilliseconds}mm");
                }
            });

            tran.Execute();
            sw.Stop();

            Console.WriteLine($"total takes {sw.Elapsed.TotalMilliseconds}mm");
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

        static Dictionary<string, string> ToDic(object obj)
        {
            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            var dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
            return dic;
        }
    }
}
