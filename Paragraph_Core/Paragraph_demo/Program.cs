using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paragraph_Core;

namespace Paragraph_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //string first = Console.ReadLine();
            //string second = Console.ReadLine();
            goAsync();
            
            Console.ReadKey();
        }
        static async Task goAsync()
        {
            User Alex = new User("556899.ok@gmail.com", "Alex3011", false, false);
            //User Kir = new User("kirill6g@yandex.ru", "6gkirill", false, false);

            List<string> x = await Alex.GetInfoAsync();
            //List<string> y = await Kir.GetInfoAsync();

            for (int i = 0; i < x.Count; i++)
            {
                Console.WriteLine("x[" + i + "]:");
                Console.WriteLine(x[i]);
            }
            
        }
    }
}
