using System;
using System.IO;
using ApkReader.Models;
using ApkReader.Res;

namespace ApkReader
{
    public static class ArscReader
    {
        public static ArscFile Read(Stream stream)
        {
            var file = new ArscFile();
            using (var reader = new ResReader(stream))
            {
                var chunkHeader = reader.ReadResChunk_header();
                if (chunkHeader.Type != ResourceType.RES_TABLE_TYPE)
                {
                    throw new ApkReaderException("No RES_TABLE_TYPE found!");
                }
                if (chunkHeader.Size != stream.Length)
                {
                    throw new ApkReaderException("The buffer size not matches to the resource table size.");
                }
                file.Length = chunkHeader.Size;
                var resTable = reader.ReadResTable_header(chunkHeader);
                var globalStringPoolHeader = reader.ReadResStringPool_header(reader.ReadResChunk_header());
                var globalStringPool = reader.ReadResStringPool(globalStringPoolHeader);
                file.GlobalStringPool = globalStringPool;
                for (var packageIndex = 0; packageIndex < resTable.PackageCount; packageIndex++)
                {
                    var package = new ArscPackage();
                    file.Packages.Add(package);
                    chunkHeader = reader.ReadResChunk_header();
                    var endPosition = stream.Position - 8 + chunkHeader.Size;
                    if (chunkHeader.Type != ResourceType.RES_TABLE_PACKAGE_TYPE)
                    {
                        throw new ApkReaderException();
                    }
                    var packageHeader = reader.ReadResTable_package(chunkHeader);
                    package.Id = packageHeader.Id;
                    package.Name = packageHeader.Name.TrimEnd('\0');
                    chunkHeader = reader.ReadResChunk_header();
                    if (chunkHeader.Type != ResourceType.RES_STRING_POOL_TYPE)
                    {
                        throw new ApkReaderException();
                    }
                    package.TypeStringPool = reader.ReadResStringPool(reader.ReadResStringPool_header(chunkHeader));
                    chunkHeader = reader.ReadResChunk_header();
                    if (chunkHeader.Type != ResourceType.RES_STRING_POOL_TYPE)
                    {
                        throw new ApkReaderException();
                    }
                    package.KeyStringPool = reader.ReadResStringPool(reader.ReadResStringPool_header(chunkHeader));
                    do
                    {
                        chunkHeader = reader.ReadResChunk_header();
                        switch (chunkHeader.Type)
                        {
                            case ResourceType.RES_TABLE_TYPE_SPEC_TYPE:
                                var th = reader.ReadResTable_typeSpec(chunkHeader);
                                package.TypeSpecsData = new uint[th.EntryCount];
                                for (var i = 0; i < th.EntryCount; i++)
                                {
                                    package.TypeSpecsData[i] = reader.ReadUInt32();
                                }
                                break;
                            case ResourceType.RES_TABLE_TYPE_TYPE:
                                var table = reader.ReadResTable_type(chunkHeader);
                                var arscTable = new ArscTable
                                {
                                    Config = table.Config
                                };
                                package.Tables.Add(arscTable);
                                var entryIndices = new int[table.EntryCount];
                                for (var i = 0; i < table.EntryCount; i++)
                                {
                                    entryIndices[i] = reader.ReadInt32();
                                }
                                var entries = new ResTable_entry[table.EntryCount];
                                for (var i = 0; i < table.EntryCount; i++)
                                {
                                    if (entryIndices[i] == -1)
                                    {
                                        continue;
                                    }
                                    var resourceId = (packageHeader.Id << 24) | (table.RawID << 16) | i;
                                    var entry = reader.ReadResTable_entry();
                                    entries[i] = entry;
                                    if ((entry.Flags & EntryFlags.FLAG_COMPLEX) == 0)
                                    {
                                        var value = reader.ReadRes_value();
                                        arscTable.Values[Convert.ToUInt32(resourceId)] = value;
                                    }
                                    else
                                    {
                                        var parent = reader.ReadInt32();
                                        var count = reader.ReadInt32();
                                        for (var x = 0; x < count; x++)
                                        {
                                            var refName = reader.ReadUInt32();
                                            var value = reader.ReadRes_value();
                                        }
                                    }
                                }
                                break;
                            default:
                                throw new ApkReaderException($"Unknow Data Type : {chunkHeader.Type}");
                        }
                    } while (stream.Position < endPosition);
                }
            }
            return file;
        }

        public static ArscFile Read(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return Read(fs);
            }
        }
    }
}