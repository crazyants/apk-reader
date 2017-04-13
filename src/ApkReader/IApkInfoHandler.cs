using System.Xml;
using ApkReader.Arsc;

namespace ApkReader
{
    public interface IApkInfoHandler<in TApkInfo> where TApkInfo : ApkInfo
    {
        void Execute(XmlDocument androidManifest, ArscFile resources, TApkInfo apkInfo);
    }
}