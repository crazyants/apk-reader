using System;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace ApkReader.Run
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var fs = File.OpenRead(@"D:\tmp\Crisis_Action_v1.9.1 - 副本\application\resources.arsc"))
            {
                using (var binaryReader = new BinaryReader(fs))
                {
                    foreach (var l1Chunk in Helper.GetChunks(binaryReader))
                    {
                        if (l1Chunk.Key.Type != ResChunkHeader.eResChunkHeaderType.RES_TABLE_TYPE)
                        {
                            throw new Exception();
                        }
                        if (l1Chunk.Key.Size != fs.Length)
                        {
                            throw new Exception("The buffer size not matches to the resource table size.");
                        }
                        using (var l1MemoryStream = new MemoryStream(l1Chunk.Value))
                        {
                            using (var l1BinaryReader = new BinaryReader(l1MemoryStream))
                            {
                                var resTableHeader = new ResTableHeader
                                {
                                    ResChunkHeader = ResChunkHeader.Read(l1BinaryReader),
                                    PackageCount = l1BinaryReader.ReadUInt32()
                                };
                                foreach (var l2Chunk in Helper.GetChunks(l1BinaryReader))
                                {
                                    switch (l2Chunk.Key.Type)
                                    {
                                        case ResChunkHeader.eResChunkHeaderType.RES_STRING_POOL_TYPE:
                                            var strings = Helper.ReadStringPool(l2Chunk.Value);
                                            Console.WriteLine(strings.Length);
                                            break;
                                        case ResChunkHeader.eResChunkHeaderType.RES_TABLE_PACKAGE_TYPE:
                                            using (var l3MemoryStream = new MemoryStream(l2Chunk.Value))
                                            {
                                                using (var l3BinaryReader = new BinaryReader(l3MemoryStream))
                                                {
                                                    var resTablePackage = new ResTablePackage
                                                    {
                                                        ResChunkHeader = ResChunkHeader.Read(l3BinaryReader),
                                                        Id = l3BinaryReader.ReadUInt32(),
                                                        Name = Encoding.Unicode.GetString(l3BinaryReader.ReadBytes(128)).Trim('\0'),
                                                        TypeStrings = l3BinaryReader.ReadUInt32(),
                                                        LastPublicType = l3BinaryReader.ReadUInt32(),
                                                        KeyStrings = l3BinaryReader.ReadUInt32(),
                                                        LastPublicKey = l3BinaryReader.ReadUInt32()
                                                    };
                                                    l3MemoryStream.Seek(resTablePackage.ResChunkHeader.HeaderSize, SeekOrigin.Begin);
                                                    foreach (var l3Chunk in Helper.GetChunks(l3BinaryReader))
                                                    {
                                                        Console.WriteLine(l3Chunk.Key.Type);
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            Console.WriteLine(l2Chunk.Key.Type);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (args.Any())
            {
                var file = args[0];
                if (File.Exists(file))
                {
                    byte[] dataAndroidManifest = null;
                    byte[] dataResources = null;
                    using (var zip = ZipFile.Read(file))
                    {
                        foreach (var item in zip)
                        {
                            var name = item.FileName.ToLower();
                            switch (name)
                            {
                                case "androidmanifest.xml":
                                    using (var s = item.OpenReader())
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            s.CopyTo(ms);
                                            dataAndroidManifest = ms.ToArray();
                                        }
                                    }
                                    break;
                                case "resources.arsc":
                                    using (var s = item.OpenReader())
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            s.CopyTo(ms);
                                            dataResources = ms.ToArray();
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    if (dataAndroidManifest != null && dataResources != null)
                    {
                        var apkReader = new ApkReader();
                        var info = apkReader.ExtractInfo(dataAndroidManifest, dataResources);
                        Console.WriteLine($"Package Name: {info.PackageName}");
                        Console.WriteLine($"Version Name: {info.VersionName}");
                        Console.WriteLine($"Version Code: {info.VersionCode}");

                        Console.WriteLine($"App Has Icon: {info.HasIcon}");
                        if (info.IconFileName.Count > 0)
                        {
                            Console.WriteLine($"App Icon: {info.IconFileName[0]}");
                        }
                        Console.WriteLine($"Min SDK Version: {info.MinSdkVersion}");
                        Console.WriteLine($"Target SDK Version: {info.TargetSdkVersion}");

                        if (info.Permissions != null && info.Permissions.Count > 0)
                        {
                            Console.WriteLine("Permissions:");
                            info.Permissions.ForEach(f => { Console.WriteLine($"   {f}"); });
                        }
                        else
                        {
                            Console.WriteLine("No Permissions Found");
                        }

                        Console.WriteLine($"Supports Any Density: {info.SupportAnyDensity}");
                        Console.WriteLine($"Supports Large Screens: {info.SupportLargeScreens}");
                        Console.WriteLine($"Supports Normal Screens: {info.SupportNormalScreens}");
                        Console.WriteLine($"Supports Small Screens: {info.SupportSmallScreens}");
                        using (var fs = File.OpenWrite("1.csv"))
                        {
                            using (var sw = new StreamWriter(fs, Encoding.GetEncoding(936)))
                            {
                                sw.WriteLine("ID,Value");
                                foreach (var resString in info.ResStrings)
                                {
                                    foreach (var item in resString.Value)
                                    {
                                        sw.WriteLine("{0},\"{1}\"", resString.Key, item?.Replace("\"", "\"\""));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("File not found!");
                }
            }
        }
    }
}