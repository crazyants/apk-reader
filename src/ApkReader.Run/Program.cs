using System;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace ApkReader.Run
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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