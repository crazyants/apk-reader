using System;
using System.IO;

namespace ApkReader.Run
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var dir = @"";
            var apkReader = new Iteedee.ApkReader.ApkReader();
            var info = apkReader.ExtractInfo(File.ReadAllBytes(dir + "AndroidManifest.xml"),
                File.ReadAllBytes(dir + "resources.arsc"));
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
            Console.ReadKey();
        }
    }
}