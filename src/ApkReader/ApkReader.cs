using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ApkReader.Arsc;
#if INNER_ZIP
using System.IO.Compression;
#else
using Ionic.Zip;

#endif

namespace ApkReader
{
    public class ApkReader : ApkReader<ApkInfo>
    {
    }

    public class ApkReader<TApkInfo> where TApkInfo : ApkInfo, new()
    {
        private List<IApkInfoHandler<TApkInfo>> _apkInfoHandlers;

        public IList<IApkInfoHandler<TApkInfo>> ApkInfoHandlers
        {
            get
            {
                if (_apkInfoHandlers == null)
                {
                    lock (this)
                    {
                        if (_apkInfoHandlers == null)
                        {
                            _apkInfoHandlers = new List<IApkInfoHandler<TApkInfo>>();
                            foreach (var handler in GetApkInfoHandlers())
                            {
                                _apkInfoHandlers.Add(handler);
                            }
                        }
                    }
                }
                return _apkInfoHandlers;
            }
        }

        protected virtual IEnumerable<IApkInfoHandler<TApkInfo>> GetApkInfoHandlers()
        {
            yield return new ApkInfoHandler();
        }

        protected virtual bool CheckIsResources(string fileName)
        {
            if ("AndroidManifest.xml".Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            if ("resources.arsc".Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        protected virtual Dictionary<string, byte[]> ReadResources(Stream stream)
        {
            var dic = new Dictionary<string, byte[]>(StringComparer.CurrentCultureIgnoreCase);
#if INNER_ZIP
            using (var zip = new ZipArchive(stream))
            {
                foreach (var entry in zip.Entries)
                {
                    if (CheckIsResources(entry.FullName))
                    {
                        using(var fs = entry.Open())
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            dic[entry.FullName] = ms.ToArray();
                        }
                    }
                }
            }
#else
            using (var zip = ZipFile.Read(stream))
            {
                foreach (var entry in zip.Entries)
                {
                    if (CheckIsResources(entry.FileName))
                    {
                        using (var zs = entry.OpenReader())
                        using (var ms = new MemoryStream())
                        {
                            var buffer = new byte[2048];
                            int total;
                            do
                            {
                                total = zs.Read(buffer, 0, buffer.Length);
                                if (total > 0)
                                {
                                    ms.Write(buffer, 0, total);
                                }
                            } while (total > 0);
                            dic[entry.FileName] = ms.ToArray();
                        }
                    }
                }
            }
#endif
            return dic;
        }

        public TApkInfo Read(Stream apkStream)
        {
            //解压ZIP文件
            //找到两个资源文件。
            var resources = ReadResources(apkStream);
            if (!resources.ContainsKey("AndroidManifest.xml"))
            {
                throw new ApkReaderException("can not find 'AndroidManifest.xml' in apk file.");
            }
            if (!resources.ContainsKey("resources.arsc"))
            {
                throw new ApkReaderException("can not find 'resources.arsc' in apk file.");
            }
            XmlDocument xmlDocument;
            ArscFile arscFile;
            using (var ms = new MemoryStream(resources["AndroidManifest.xml"]))
            {
                xmlDocument = BinaryXmlConvert.ToXmlDocument(ms);
            }
            using (var ms = new MemoryStream(resources["resources.arsc"]))
            {
                arscFile = ArscReader.Read(ms);
            }
            var apkInfo = new TApkInfo();
            foreach (var handler in ApkInfoHandlers)
            {
                handler.Execute(xmlDocument, arscFile, apkInfo);
            }
            return apkInfo;
        }

        public virtual TApkInfo Read(string apkPath)
        {
            using (var fs = File.OpenRead(apkPath))
            {
                return Read(fs);
            }
        }
    }
}