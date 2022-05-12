using HtmlAgilityPack;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSend
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            BotSchedule scheduleBot = new BotSchedule();
            Console.WriteLine("1. Thuc hien tai thoi diem cu the moi ngay (thuc hien ngay khi chon )\n");
            Console.WriteLine("2. Thuc hien sau 1 thoi gian moi lan (unit : hour) (thuc hien ngay khi chon )\n");
            Console.WriteLine("3. Thuc hien sau 1 thoi gian moi lan (unit : second) (thuc hien ngay khi chon )\n");
            while (true)
            {
                char choose ;
                Console.WriteLine("\nNhap lua chon : ");
                choose =Convert.ToChar( Console.ReadLine());
                switch (choose)
                {
                    case '1':
                        int hour, minute;
                        Console.WriteLine("Nhap gio cu the :");
                        hour =Convert.ToInt32( Console.ReadLine());
                        Console.WriteLine("Nhap phut cu the :");
                        minute = Convert.ToInt32(Console.ReadLine());
                         await scheduleBot.Execute_Everyday_10h(hour,minute);
                        break;

                    case '2':
                        int hour_per_time;
                        Console.WriteLine("Nhap gio cu the :");
                        hour_per_time = Convert.ToInt32(Console.ReadLine());
                        await scheduleBot.Execute_Every_10min(hour_per_time);
                        break;

                    case '3':
                        int hour_per_sec;
                        Console.WriteLine("Nhap thoi gian muon lap lai (second) :");
                        hour_per_sec = Convert.ToInt32(Console.ReadLine());
                        await scheduleBot.Execute_Every_10s(hour_per_sec);
                        break;
                    case '4':
                        Console.WriteLine("Trung binh");
                        break;
                   
                    default:
                        Console.WriteLine("Gia tri khong hop le");
                        break;
                }
            }
           /* BotQueue botQueue = new BotQueue();
            botQueue.SendQueue();*/
        }
    }
}
