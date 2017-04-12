using System.Collections.Generic;
using ApkReader.Res;

namespace ApkReader.Models
{
    public class ArscPackage
    {
        public ArscPackage()
        {
            Tables = new List<ArscTable>();
        }

        public uint Id { get; set; }
        public string Name { get; set; }
        public ResStringPool TypeStringPool { get; set; }
        public ResStringPool KeyStringPool { get; set; }
        public uint[] TypeSpecsData { get; set; }
        public IList<ArscTable> Tables { get; }
    }
}