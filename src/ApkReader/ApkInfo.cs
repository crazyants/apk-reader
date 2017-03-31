using System.Collections.Generic;

namespace Iteedee.ApkReader
{
    public class ApkInfo
    {
        public ApkInfo()
        {
            HasIcon = false;
            SupportSmallScreens = false;
            SupportNormalScreens = false;
            SupportLargeScreens = false;
            SupportAnyDensity = true;
            VersionCode = null;
            VersionName = null;
            IconFileName = null;

            Permissions = new List<string>();
        }

        public List<string> IconFileName { get; set; }

        public string MinSdkVersion { get; set; }
        public string PackageName { get; set; }
        public List<string> Permissions { get; }
        public byte[] ResourcesFileBytes { get; set; }
        public string ResourcesFileName { get; set; }
        public Dictionary<string, List<string>> ResStrings { get; set; }
        public bool SupportAnyDensity { get; set; }
        public bool SupportLargeScreens { get; set; }
        public bool SupportNormalScreens { get; set; }
        public bool SupportSmallScreens { get; set; }
        public string TargetSdkVersion { get; set; }
        public string VersionCode { get; set; }
        public string VersionName { get; set; }

        public string Label { get; set; }
        public bool HasIcon { get; set; }

        public static bool SupportSmallScreen(byte[] dpi)
        {
            if (dpi[0] == 1)
            {
                return true;
            }
            return false;
        }

        public static bool SupportNormalScreen(byte[] dpi)
        {
            if (dpi[1] == 1)
            {
                return true;
            }
            return false;
        }

        public static bool SupportLargeScreen(byte[] dpi)
        {
            if (dpi[2] == 1)
            {
                return true;
            }
            return false;
        }
    }
}