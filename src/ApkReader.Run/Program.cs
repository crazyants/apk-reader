using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ApkReader.Res;
using ValueType = ApkReader.Res.ValueType;

namespace ApkReader.Run
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            using (var fs = File.OpenRead(@"D:\tmp\wx - 副本\AndroidManifest.xml"))
            {
                //var reader = new AndroidXmlReader(fs);
                //var xml = XDocument.Load(reader);
                //Console.WriteLine(xml.ToString());
                var xml = BinaryXmlConvert.ToXmlDocument(fs).InnerXml;
                Console.WriteLine(xml);
                Console.WriteLine(BinaryXmlConvert.ToXmlDocument(fs).InnerXml);
            }
        }
    }
}