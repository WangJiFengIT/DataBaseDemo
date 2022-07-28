using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlBulkCopyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            List<student> students = new List<student>();
            for (int i = 0; i < 10000; i++)
            {
                students.Add(new student
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = "jifeng wang",
                    Age = new Random().Next(0, 99)
                });
            }
            Console.WriteLine(DateTime.Now);

            //调用MySqlBulkLoader，往student表中插入stuList
            int insertCount = MySqlBulkLoaderHelper.BulkInsert<student>(students, "student");
            Console.WriteLine(DateTime.Now);
            Console.WriteLine($"成功插入{insertCount}条数据");
            Console.ReadKey();
        }
    }
}
