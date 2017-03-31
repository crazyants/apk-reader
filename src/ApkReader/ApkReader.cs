using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace ApkReader
{
    public class ApkReader
    {
        //private static Logger log = Logger.getLogger("APKReader");

        private const int VER_ID = 0;
        private const int ICN_ID = 1;
        private const int LABEL_ID = 2;

        // Some possible tags and attributes
        private readonly string[] _tags = {"manifest", "application", "activity"};
        private readonly string[] _verIcn = new string[3];

        public string FuzzFindInDocument(XmlDocument doc, string tag, string attr)
        {
            foreach (var t in _tags)
            {
                var nodelist = doc.GetElementsByTagName(t);
                for (var i = 0; i < nodelist.Count; i++)
                {
                    var element = nodelist.Item(i);
                    if (element?.NodeType == XmlNodeType.Element)
                    {
                        var map = element.Attributes;
                        for (var j = 0; j < map?.Count; j++)
                        {
                            var element2 = map.Item(j);
                            if (element2.Name.EndsWith(attr))
                            {
                                return element2.Value;
                            }
                        }
                    }
                }
            }
            return null;
        }


        private void ExtractPermissions(ApkInfo info, XmlDocument doc)
        {
            ExtractPermission(info, doc, "uses-permission", "name");
            ExtractPermission(info, doc, "permission-group", "name");
            ExtractPermission(info, doc, "service", "permission");
            ExtractPermission(info, doc, "provider", "permission");
            ExtractPermission(info, doc, "activity", "permission");
        }

        private bool ReadBoolean(XmlDocument doc, string tag, string attribute)
        {
            var str = FindInDocument(doc, tag, attribute);
            bool ret;
            try
            {
                ret = Convert.ToBoolean(str);
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        private void ExtractSupportScreens(ApkInfo info, XmlDocument doc)
        {
            info.SupportSmallScreens = ReadBoolean(doc, "supports-screens", "android:smallScreens");
            info.SupportNormalScreens = ReadBoolean(doc, "supports-screens", "android:normalScreens");
            info.SupportLargeScreens = ReadBoolean(doc, "supports-screens", "android:largeScreens");

            if (info.SupportSmallScreens || info.SupportNormalScreens || info.SupportLargeScreens)
            {
                info.SupportAnyDensity = false;
            }
        }

        public ApkInfo ExtractInfo(byte[] manifestFileData, byte[] resourcesArsx)
        {
            if (manifestFileData == null)
            {
                throw new ArgumentNullException(nameof(manifestFileData));
            }
            var manifest = new ApkManifest();

            var manifestXml = manifest.ReadManifestFileIntoXml(manifestFileData);
            var doc = new XmlDocument();
            doc.LoadXml(manifestXml);
            return ExtractInfo(doc, resourcesArsx);
        }

        public ApkInfo ExtractInfo(XmlDocument manifestXml, byte[] resourcesArsx)
        {
            var info = new ApkInfo();
            _verIcn[VER_ID] = "";
            _verIcn[ICN_ID] = "";
            _verIcn[LABEL_ID] = "";
            var doc = manifestXml;
            if (doc == null)
            {
                throw new Exception("Document initialize failed");
            }
            info.ResourcesFileName = "resources.arsx";
            info.ResourcesFileBytes = resourcesArsx;
            // Fill up the permission field
            ExtractPermissions(info, doc);

            // Fill up some basic fields
            info.MinSdkVersion = FindInDocument(doc, "uses-sdk", "minSdkVersion");
            info.TargetSdkVersion = FindInDocument(doc, "uses-sdk", "targetSdkVersion");
            info.VersionCode = FindInDocument(doc, "manifest", "versionCode");
            info.VersionName = FindInDocument(doc, "manifest", "versionName");
            info.PackageName = FindInDocument(doc, "manifest", "package");

            int labelId;
            info.Label = FindInDocument(doc, "application", "label");
            if (info.Label.StartsWith("@"))
            {
                _verIcn[LABEL_ID] = info.Label;
            }
            else if (int.TryParse(info.Label, out labelId))
            {
                _verIcn[LABEL_ID] = $"@{labelId:X4}";
            }

            // Fill up the support screen field
            ExtractSupportScreens(info, doc);

            if (info.VersionCode == null)
            {
                info.VersionCode = FuzzFindInDocument(doc, "manifest",
                    "versionCode");
            }

            if (info.VersionName == null)
            {
                info.VersionName = FuzzFindInDocument(doc, "manifest",
                    "versionName");
            }
            else if (info.VersionName.StartsWith("@"))
            {
                _verIcn[VER_ID] = info.VersionName;
            }

            var id = FindInDocument(doc, "application", "android:icon") ?? FuzzFindInDocument(doc, "manifest", "icon");

            if (null == id)
            {
                Debug.WriteLine("icon resId Not Found!");
                return info;
            }

            // Find real strings
            if (!info.HasIcon)
            {
                if (id.StartsWith("@android:"))
                {
                    _verIcn[ICN_ID] = "@"
                                      + id.Substring("@android:".Length);
                }
                else
                {
                    _verIcn[ICN_ID] = $"@{Convert.ToInt32(id):X4}";
                }

                var resId = new List<string>();

                foreach (var t in _verIcn)
                {
                    if (t.StartsWith("@"))
                    {
                        resId.Add(t);
                    }
                }

                var finder = new ApkResourceFinder();
                info.ResStrings = finder.ProcessResourceTable(info.ResourcesFileBytes, resId);

                if (!_verIcn[VER_ID].Equals(""))
                {
                    List<string> versions = null;
                    if (info.ResStrings.ContainsKey(_verIcn[VER_ID].ToUpper()))
                    {
                        versions = info.ResStrings[_verIcn[VER_ID].ToUpper()];
                    }
                    if (versions != null)
                    {
                        if (versions.Count > 0)
                        {
                            info.VersionName = versions[0];
                        }
                    }
                    else
                    {
                        throw new Exception(
                            "VersionName Cant Find in resource with id "
                            + _verIcn[VER_ID]);
                    }
                }

                List<string> iconPaths = null;
                if (info.ResStrings.ContainsKey(_verIcn[ICN_ID].ToUpper()))
                {
                    iconPaths = info.ResStrings[_verIcn[ICN_ID].ToUpper()];
                }
                if (iconPaths != null && iconPaths.Count > 0)
                {
                    info.IconFileName = new List<string>();
                    foreach (var iconFileName in iconPaths)
                    {
                        if (iconFileName != null)
                        {
                            if (iconFileName.Contains(@"/"))
                            {
                                info.IconFileName.Add(iconFileName);
                                info.HasIcon = true;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Icon Cant Find in resource with id "
                                        + _verIcn[ICN_ID]);
                }

                if (!_verIcn[LABEL_ID].Equals(""))
                {
                    List<string> labels = null;
                    if (info.ResStrings.ContainsKey(_verIcn[LABEL_ID]))
                    {
                        labels = info.ResStrings[_verIcn[LABEL_ID]];
                    }
                    if (labels?.Count > 0)
                    {
                        info.Label = labels[0];
                    }
                }
            }

            return info;
        }


        private void ExtractPermission(ApkInfo info, XmlDocument doc, string keyName, string attribName)
        {
            var usesPermissions = doc.GetElementsByTagName(keyName);
            for (var s = 0; s < usesPermissions.Count; s++)
            {
                var permissionNode = usesPermissions.Item(s);
                if (permissionNode?.NodeType == XmlNodeType.Element)
                {
                    var node = permissionNode.Attributes?.GetNamedItem(attribName);
                    if (node != null)
                    {
                        info.Permissions.Add(node.Value);
                    }
                }
            }
        }

        private string FindInDocument(XmlDocument doc, string keyName,
            string attribName)
        {
            var usesPermissions = doc.GetElementsByTagName(keyName);

            for (var s = 0; s < usesPermissions.Count; s++)
            {
                var permissionNode = usesPermissions.Item(s);
                if (permissionNode != null && permissionNode.NodeType == XmlNodeType.Element)
                {
                    var node = permissionNode.Attributes?.GetNamedItem(attribName);
                    if (node != null)
                    {
                        return node.Value;
                    }
                }
            }
            return null;
        }
    }
}