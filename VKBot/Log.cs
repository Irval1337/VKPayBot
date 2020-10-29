using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKBot
{
    class Log
    {
        public static void Add(string message, long? userId, bool isResponse)
        {
            try
            {
                string[] separator = { ";;;5" };
                if (!Directory.Exists($@"Logs"))
                    Directory.CreateDirectory($@"Logs");

                var today = DateTime.Now;
                string date = $"{(today.Day >= 10 ? today.Day.ToString() : "0" + today.Day.ToString())}.{(today.Month >= 10 ? today.Month.ToString() : "0" + today.Month.ToString())}.{today.Year}";
                string time = $"{(today.Hour >= 10 ? today.Hour.ToString() : "0" + today.Hour.ToString())}:{(today.Minute >= 10 ? today.Minute.ToString() : "0" + today.Minute.ToString())}";

                string path = $@"Logs\{date}.txt";
                if (!File.Exists(path))
                    File.Create(path).Close();

                Task.Factory.StartNew(() =>
                {
                    File.AppendAllLines(path, new string[] { !isResponse ? $"[{time}] (MESSAGE) {userId} >>> {message}" : $"[{time}] (RESPONSE) SERVER >>> {userId}, {message}" }, Encoding.GetEncoding(1251));
                });
            }
            catch
            {
            }
        }

        public static void Payment(string[] Info, long? userId)
        {
            try
            {
                string[] separator = { ";;;5" };
                if (!Directory.Exists($@"Logs\Payments"))
                    Directory.CreateDirectory($@"Logs\Payments");

                var today = DateTime.Now;
                string date = $"{(today.Day >= 10 ? today.Day.ToString() : "0" + today.Day.ToString())}.{(today.Month >= 10 ? today.Month.ToString() : "0" + today.Month.ToString())}.{today.Year}";
                string time = $"{(today.Hour >= 10 ? today.Hour.ToString() : "0" + today.Hour.ToString())}:{(today.Minute >= 10 ? today.Minute.ToString() : "0" + today.Minute.ToString())}";

                string path = $@"Logs\Payments\{date}.txt";
                if (!File.Exists(path))
                    File.Create(path).Close();

                Task.Factory.StartNew(() =>
                {
                    File.AppendAllLines(path, new string[] { $"{time};;;5{userId};;;5{Info[1]};;;5{Info[3]};;;5{Info[5]}" }, Encoding.GetEncoding(1251));
                });
            }
            catch
            {
            }
        }
    }
}
