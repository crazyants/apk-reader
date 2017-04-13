using System;
using Newtonsoft.Json;

namespace ApkReader.Run
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var reader = new ApkReader();
            var info = reader.Read(@"D:\tmp\wx.apk");
            Console.Clear();
            var json = JsonConvert.SerializeObject(info,new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            Console.WriteLine(json);
            Console.ReadLine();
        }
    }
}