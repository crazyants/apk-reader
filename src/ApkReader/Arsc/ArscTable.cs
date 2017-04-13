using System;
using System.Collections.Generic;
using ApkReader.Res;

namespace ApkReader.Arsc
{
    public class ArscTable
    {
        public ArscTable()
        {
            this.Values = new Dictionary<uint, Res_value>();
        }
        public ResTable_config Config { get; set; }
        public IDictionary<UInt32, Res_value> Values { get; }
    }
}