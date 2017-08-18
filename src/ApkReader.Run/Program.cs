using System;
using Newtonsoft.Json;
using System.IO;

namespace ApkReader.Run
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var reader = new ApkReader();
            var info = reader.Read(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "com.qihoo.magic.apk"));
            var json = JsonConvert.SerializeObject(info, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            Console.WriteLine(json);
            Console.ReadLine();
        }
    }
}