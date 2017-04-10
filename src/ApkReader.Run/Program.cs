using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ApkReader.Res;
using ValueType = ApkReader.Res.ValueType;

namespace ApkReader.Run
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            var dic = new Dictionary<String, List<String>>();
            using (var fs = File.OpenRead(@"D:\tmp\wx - 副本\resources.arsc"))
            {
                using (var reader = new ResReader(fs))
                {
                    var tableTypeHeader = reader.ReadResChunk_header();
                    if (tableTypeHeader.Type != ResourceType.RES_TABLE_TYPE)
                    {
                        throw new Exception();
                    }
                    if (tableTypeHeader.Size != fs.Length)
                    {
                        throw new Exception();
                    }
                    var resTable = reader.ReadResTable_header(tableTypeHeader);
                    var gs = reader.ReadResStringPool_header(reader.ReadResChunk_header());
                    var gsp = reader.ReadResStringPool(gs);
                    for (int p = 0; p < resTable.PackageCount; p++)
                    {
                        var packageHeader = reader.ReadResTable_package(reader.ReadResChunk_header());
                        var resTypeStrings =
                            reader.ReadResStringPool(reader.ReadResStringPool_header(reader.ReadResChunk_header()));
                        var resNameStrings =
                            reader.ReadResStringPool(reader.ReadResStringPool_header(reader.ReadResChunk_header()));
                        do
                        {
                            var pos = fs.Position;
                            var header = reader.ReadResChunk_header();
                            switch (header.Type)
                            {
                                case ResourceType.RES_TABLE_TYPE_SPEC_TYPE:
                                    var th = reader.ReadResTable_typeSpec(header);
                                    var arr = new UInt32[th.EntryCount];
                                    for (int i = 0; i < th.EntryCount; i++)
                                    {
                                        arr[i] = reader.ReadUInt32();
                                    }
                                    break;
                                case ResourceType.RES_TABLE_TYPE_TYPE:
                                    var t2 = reader.ReadResTable_type(header);
                                    var config = t2.Config;
                                    var entryIndices = new Int32[t2.EntryCount];
                                    for (int i = 0; i < t2.EntryCount; i++)
                                    {
                                        entryIndices[i] = reader.ReadInt32();
                                    }
                                    var entries = new ResTable_entry[t2.EntryCount];
                                    for (int i = 0; i < t2.EntryCount; i++)
                                    {
                                        if (entryIndices[i] == -1)
                                        {
                                            continue;
                                        }

                                        var resourceId = (packageHeader.Id << 24) | (t2.RawID << 16) | i;
                                        var entry = reader.ReadResTable_entry();
                                        var key = resNameStrings.GetString(entry.Key);
                                        entries[i] = entry;
                                        if ((entry.Flags & EntryFlags.FLAG_COMPLEX) == 0)
                                        {
                                            var value = reader.ReadRes_value();
                                            if (resourceId.ToString("x8") == "7f080000" || resourceId.ToString("x8") == "7f02038c")
                                            {
                                                Console.WriteLine(new String(config.LocaleLanguage) + "-" + new String(config.LocaleCountry));
                                                Console.WriteLine(gsp.GetString(value.StringValue));
                                            }
                                        }
                                        else
                                        {
                                            var parent = reader.ReadInt32();
                                            var count = reader.ReadInt32();
                                            for (int x = 0; x < count; x++)
                                            {
                                                var refName = reader.ReadUInt32();
                                                var value = reader.ReadRes_value();
                                            }
                                        }
                                    }
                                    fs.Seek(pos + header.Size, SeekOrigin.Begin);
                                    break;
                                default:
                                    Console.WriteLine($"Unknow:{header.Type}");
                                    break;
                            }
                        } while (fs.Position < fs.Length);
                    }
                }
            }
        }
    }
}