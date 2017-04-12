using System;
using Newtonsoft.Json;

namespace ApkReader.Run
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var arsFile = ArscReader.Read(@"D:\tmp\wx - 副本\resources.arsc");
            var json = JsonConvert.SerializeObject(arsFile);
            Console.WriteLine(json);
        }
    }
}