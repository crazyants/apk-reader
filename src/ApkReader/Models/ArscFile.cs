using System;
using System.Collections.Generic;
using ApkReader.Res;

namespace ApkReader.Models
{
    public class ArscFile
    {
        public ArscFile()
        {
            Packages = new List<ArscPackage>();
        }

        public long Length { get; set; }
        public uint PackageCount => Convert.ToUInt32(Packages.Count);
        public ResStringPool GlobalStringPool { get; set; }
        public IList<ArscPackage> Packages { get; }
    }
}