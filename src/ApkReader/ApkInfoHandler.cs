using System;
using System.Collections.Generic;
using System.Xml;
using ApkReader.Arsc;
using ApkReader.Res;

namespace ApkReader
{
    internal class ApkInfoHandler : IApkInfoHandler<ApkInfo>
    {
        public const string AndroidNamespace = "http://schemas.android.com/apk/res/android";

        public void Execute(XmlDocument androidManifest, ArscFile resources, ApkInfo apkInfo)
        {
            var manifest = androidManifest.DocumentElement;
            if (manifest == null)
            {
                throw new ApkReaderException("AndroidManifest.xml is empty");
            }
            if (manifest.Name != "manifest")
            {
                throw new ApkReaderException("manifest is not root element.");
            }
            foreach (var package in resources.Packages)
            {
                foreach (var table in package.Tables)
                {
                    var config = table.Config;
                    var local = config.GetLocal();
                    if (!String.IsNullOrEmpty(local) && !apkInfo.Locales.Contains(local))
                    {
                        apkInfo.Locales.Add(local);
                    }
                    if (config.ScreenTypeDensity != ConfigDensity.DENSITY_NONE &&
                        config.ScreenTypeDensity != ConfigDensity.DENSITY_DEFAULT)
                    {
                        var dpi = config.ScreenTypeDensity.ToString("D");
                        if (!apkInfo.Densities.Contains(dpi))
                        {
                            apkInfo.Densities.Add(dpi);
                        }
                    }
                }
            }
            foreach (XmlAttribute attribute in manifest.Attributes)
            {
                switch (attribute.LocalName)
                {
                    case "versionCode":
                        apkInfo.VersionCode = attribute.Value;
                        break;
                    case "versionName":
                        apkInfo.VersionName = attribute.Value;
                        break;
                    case "package":
                        apkInfo.PackageName = attribute.Value;
                        break;
                }
            }
            foreach (XmlNode node in manifest.ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "uses-sdk":
                        if (node.Attributes != null)
                        {
                            foreach (XmlAttribute attribute in node.Attributes)
                            {
                                switch (attribute.LocalName)
                                {
                                    case "minSdkVersion":
                                        apkInfo.MinSdkVersion = attribute.Value;
                                        break;
                                    case "targetSdkVersion":
                                        apkInfo.TargetSdkVersion = attribute.Value;
                                        break;
                                }
                            }
                        }
                        break;
                    case "uses-permission":
                        if (node.Attributes != null)
                        {
                            foreach (XmlAttribute attribute in node.Attributes)
                            {
                                if (attribute.LocalName == "name")
                                {
                                    apkInfo.Permissions.Add(attribute.Value);
                                }
                            }
                        }
                        break;
                    case "application":
                        if (node.Attributes != null)
                        {
                            foreach (XmlAttribute attribute in node.Attributes)
                            {
                                switch (attribute.LocalName)
                                {
                                    case "label":
                                    case "icon":
                                        {
                                            var id = Convert.ToUInt32(attribute.Value.TrimStart('@'), 16);
                                            foreach (var package in resources.Packages)
                                            {
                                                foreach (var table in package.Tables)
                                                {
                                                    if (table.Values.ContainsKey(id))
                                                    {
                                                        var value = table.Values[id];
                                                        var str = resources.GlobalStringPool.GetString(value.StringValue);
                                                        switch (attribute.LocalName)
                                                        {
                                                            case "label":
                                                                var local = table.Config.GetLocal();
                                                                if (string.IsNullOrEmpty(local))
                                                                {
                                                                    apkInfo.Label = str;
                                                                }
                                                                else
                                                                {
                                                                    apkInfo.Labels[local] = str;
                                                                }
                                                                break;
                                                            case "icon":
                                                                var dpi = table.Config.ScreenTypeDensity;
                                                                switch (dpi)
                                                                {
                                                                    case ConfigDensity.DENSITY_DEFAULT:
                                                                    case ConfigDensity.DENSITY_NONE:
                                                                        apkInfo.Icon = str;
                                                                        break;
                                                                    default:
                                                                        if (String.IsNullOrEmpty(apkInfo.Icon))
                                                                        {
                                                                            apkInfo.Icon = str;
                                                                        }
                                                                        apkInfo.Icons[dpi.ToString("D")] = str;
                                                                        break;
                                                                }
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        var xnames = new List<String>();
                        var ynames = new List<String>();
                        foreach (XmlNode innerNode in node.ChildNodes)
                        {
                            switch (innerNode.LocalName)
                            {
                                case "activity":
                                    var activityName = String.Empty;
                                    if (innerNode.Attributes != null)
                                    {
                                        foreach (XmlAttribute innerNodeAttribute in innerNode.Attributes)
                                        {
                                            if (innerNodeAttribute.LocalName == "name")
                                            {
                                                activityName = innerNodeAttribute.Value;
                                            }
                                        }
                                    }
                                    if (!String.IsNullOrEmpty(activityName))
                                    {
                                        foreach (XmlNode intentFilterNode in innerNode.ChildNodes)
                                        {
                                            foreach (XmlNode endNode in intentFilterNode.ChildNodes)
                                            {
                                                if (endNode.Attributes != null)
                                                {
                                                    foreach (XmlAttribute endNodeAttribute in endNode.Attributes)
                                                    {
                                                        if (endNodeAttribute.LocalName == "name")
                                                        {
                                                            switch (endNodeAttribute.Value)
                                                            {
                                                                case "android.intent.action.MAIN":
                                                                    xnames.Add(activityName);
                                                                    break;
                                                                case "android.intent.category.LAUNCHER":
                                                                    ynames.Add(activityName);
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        var znames = new List<String>();
                        foreach (var xname in xnames)
                        {
                            if (ynames.Contains(xname))
                            {
                                znames.Add(xname);
                            }
                        }
                        if (znames.Count > 0)
                        {
                            apkInfo.LaunchableActivity = znames[0];
                        }
                        break;
                }
            }
        }
    }
}