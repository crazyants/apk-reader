using System.Collections.Generic;

namespace ApkReader
{
    public class ApkInfo
    {
        public string VersionName { get; set; }
        public string VersionCode { get; set; }

        public string TargetSdkVersion { get; set; }
        public List<string> Permissions { get; } = new List<string>();
        public string PackageName { get; set; }
        public string MinSdkVersion { get; set; }
        public string Icon { get; set; }
        public Dictionary<string, string> Icons { get; } = new Dictionary<string, string>();
        public string Label { get; set; }
        public Dictionary<string, string> Labels { get; } = new Dictionary<string, string>();
        public bool HasIcon => Icons.Count > 0 || !string.IsNullOrEmpty(Icon);
        public List<string> Locales { get; } = new List<string>();
        public List<string> Densities { get; } = new List<string>();
        public string LaunchableActivity { get; set; }
    }
}