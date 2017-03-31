using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace ApkReader
{
    public class ApkResourceFinder
    {
        private static readonly short RES_STRING_POOL_TYPE = 0x0001;
        private static readonly short RES_TABLE_TYPE = 0x0002;
        private static readonly short RES_TABLE_PACKAGE_TYPE = 0x0200;
        private static readonly short RES_TABLE_TYPE_TYPE = 0x0201;
        private static readonly short RES_TABLE_TYPE_SPEC_TYPE = 0x0202;

        //// Contains no data.
        //static byte TYPE_NULL = 0x00;
        //// The 'data' holds an attribute resource identifier.
        //static byte TYPE_ATTRIBUTE = 0x02;
        //// The 'data' holds a single-precision floating point number.
        //static byte TYPE_FLOAT = 0x04;
        //// The 'data' holds a complex number encoding a dimension value,
        //// such as "100in".
        //static byte TYPE_DIMENSION = 0x05;
        //// The 'data' holds a complex number encoding a fraction of a
        //// container.
        //static byte TYPE_FRACTION = 0x06;
        //// The 'data' is a raw integer value of the form n..n.
        //static byte TYPE_INT_DEC = 0x10;
        //// The 'data' is a raw integer value of the form 0xn..n.
        //static byte TYPE_INT_HEX = 0x11;
        //// The 'data' is either 0 or 1, for input "false" or "true" respectively.
        //static byte TYPE_INT_BOOLEAN = 0x12;
        //// The 'data' is a raw integer value of the form #aarrggbb.
        //static byte TYPE_INT_COLOR_ARGB8 = 0x1c;
        //// The 'data' is a raw integer value of the form #rrggbb.
        //static byte TYPE_INT_COLOR_RGB8 = 0x1d;
        //// The 'data' is a raw integer value of the form #argb.
        //static byte TYPE_INT_COLOR_ARGB4 = 0x1e;
        //// The 'data' is a raw integer value of the form #rgb.
        //static byte TYPE_INT_COLOR_RGB4 = 0x1f;

        // The 'data' holds a ResTable_ref, a reference to another resource
        // table entry.
        private static readonly byte TYPE_REFERENCE = 0x01;
        // The 'data' holds an index into the containing resource table's
        // global value string pool.
        private static readonly byte TYPE_STRING = 0x03;

        private readonly Dictionary<int, List<string>> _entryMap = new Dictionary<int, List<string>>();
        private string[] _keyStringPool;

        private int _packageId;


        private Dictionary<string, List<string>> _responseMap;
        private string[] _typeStringPool;

        private string[] _valueStringPool;

        public Dictionary<string, List<string>> ProcessResourceTable(byte[] data, List<string> resIdList)
        {
            _responseMap = new Dictionary<string, List<string>>();

            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    var type = br.ReadInt16();
                    var headerSize = br.ReadInt16();
                    var size = br.ReadInt32();
                    var packageCount = br.ReadInt32();


                    if (type != RES_TABLE_TYPE)
                    {
                        throw new Exception("No RES_TABLE_TYPE found!");
                    }
                    if (size != br.BaseStream.Length)
                    {
                        throw new Exception(
                            "The buffer size not matches to the resource table size.");
                    }

                    var realStringPoolCount = 0;
                    var realPackageCount = 0;


                    while (true)
                    {
                        var pos = br.BaseStream.Position;
                        var t = br.ReadInt16();
                        var hs = br.ReadInt16();
                        var s = br.ReadInt32();

                        if (t == RES_STRING_POOL_TYPE)
                        {
                            if (realStringPoolCount == 0)
                            {
                                // Only the first string pool is processed.
                                Debug.WriteLine("Processing the string pool ...");


                                br.BaseStream.Seek(pos, SeekOrigin.Begin);
                                var buffer = br.ReadBytes(s);
                                //br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                                _valueStringPool = processStringPool(buffer);
                            }
                            realStringPoolCount++;
                        }
                        else if (t == RES_TABLE_PACKAGE_TYPE)
                        {
                            // Process the package
                            //Debug.WriteLine("Processing package {0} ...", realPackageCount);
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            var buffer = br.ReadBytes(s);
                            //br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);
                            ProcessPackage(buffer);

                            realPackageCount++;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported Type");
                        }
                        br.BaseStream.Seek(pos + s, SeekOrigin.Begin);
                        if (br.BaseStream.Position == br.BaseStream.Length)
                        {
                            break;
                        }
                    }

                    if (realStringPoolCount != 1)
                    {
                        throw new Exception("More than 1 string pool found!");
                    }
                    if (realPackageCount != packageCount)
                    {
                        throw new Exception(
                            "Real package count not equals the declared count.");
                    }

                    return _responseMap;
                }
            }
        }

        private void ProcessPackage(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    //HEADER
                    var type = br.ReadInt16();
                    var headerSize = br.ReadInt16();
                    var size = br.ReadInt32();

                    var id = br.ReadInt32();
                    _packageId = id;

                    //PackageName
                    var name = new char[256];
                    for (var i = 0; i < 256; ++i)
                    {
                        name[i] = br.ReadChar();
                    }
                    var typeStrings = br.ReadInt32();
                    var lastPublicType = br.ReadInt32();
                    var keyStrings = br.ReadInt32();
                    var lastPublicKey = br.ReadInt32();

                    if (typeStrings != headerSize)
                    {
                        throw new Exception("TypeStrings must immediately follow the package structure header.");
                    }

                    Debug.WriteLine("Type strings:");
                    var lastPosition = br.BaseStream.Position;
                    br.BaseStream.Seek(typeStrings, SeekOrigin.Begin);
                    var bbTypeStrings = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                    br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                    _typeStringPool = processStringPool(bbTypeStrings);

                    Debug.WriteLine("Key strings:");

                    br.BaseStream.Seek(keyStrings, SeekOrigin.Begin);
                    var keyType = br.ReadInt16();
                    var keyHeaderSize = br.ReadInt16();
                    var keySize = br.ReadInt32();

                    lastPosition = br.BaseStream.Position;
                    br.BaseStream.Seek(keyStrings, SeekOrigin.Begin);
                    var bbKeyStrings = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                    br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                    _keyStringPool = processStringPool(bbKeyStrings);


                    // Iterate through all chunks
                    //

                    br.BaseStream.Seek(keyStrings + keySize, SeekOrigin.Begin);

                    while (true)
                    {
                        var pos = (int)br.BaseStream.Position;
                        var t = br.ReadInt16();
                        var hs = br.ReadInt16();
                        var s = br.ReadInt32();

                        if (t == RES_TABLE_TYPE_SPEC_TYPE)
                        {
                            // Process the string pool
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            var buffer = br.ReadBytes(s);

                            ProcessTypeSpec(buffer);
                        }
                        else if (t == RES_TABLE_TYPE_TYPE)
                        {
                            // Process the package
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            var buffer = br.ReadBytes(s);

                            ProcessType(buffer);
                        }

                        br.BaseStream.Seek(pos + s, SeekOrigin.Begin);
                        if (br.BaseStream.Position == br.BaseStream.Length)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void PutIntoMap(string resId, string value)
        {
            List<string> valueList = null;
            if (_responseMap.ContainsKey(resId.ToUpper()))
            {
                valueList = _responseMap[resId.ToUpper()];
            }
            if (valueList == null)
            {
                valueList = new List<string>();
            }
            valueList.Add(value);
            if (_responseMap.ContainsKey(resId.ToUpper()))
            {
                _responseMap[resId.ToUpper()] = valueList;
            }
            else
            {
                _responseMap.Add(resId.ToUpper(), valueList);
            }
        }

        private void ProcessType(byte[] typeData)
        {
            using (var ms = new MemoryStream(typeData))
            {
                using (var br = new BinaryReader(ms))
                {
                    var type = br.ReadInt16();
                    var headerSize = br.ReadInt16();
                    var size = br.ReadInt32();
                    var id = br.ReadByte();
                    var res0 = br.ReadByte();
                    var res1 = br.ReadInt16();
                    var entryCount = br.ReadInt32();
                    var entriesStart = br.ReadInt32();

                    var refKeys = new Dictionary<string, int>();

                    var configSize = br.ReadInt32();

                    // Skip the config data
                    br.BaseStream.Seek(headerSize, SeekOrigin.Begin);


                    if (headerSize + entryCount * 4 != entriesStart)
                    {
                        throw new Exception("HeaderSize, entryCount and entriesStart are not valid.");
                    }

                    // Start to get entry indices
                    var entryIndices = new int[entryCount];
                    for (var i = 0; i < entryCount; ++i)
                    {
                        entryIndices[i] = br.ReadInt32();
                    }

                    // Get entries

                    for (var i = 0; i < entryCount; ++i)
                    {
                        if (entryIndices[i] == -1)
                        {
                            continue;
                        }

                        var resourceId = (_packageId << 24) | (id << 16) | i;

                        var pos = br.BaseStream.Position;
                        var entrySize = br.ReadInt16();
                        var entryFlag = br.ReadInt16();
                        var entryKey = br.ReadInt32();

                        // Get the value (simple) or map (complex)
                        var FLAG_COMPLEX = 0x0001;

                        if ((entryFlag & FLAG_COMPLEX) == 0)
                        {
                            // Simple case
                            var valueSize = br.ReadInt16();
                            var valueRes0 = br.ReadByte();
                            var valueDataType = br.ReadByte();
                            var valueData = br.ReadInt32();

                            var idStr = resourceId.ToString("X4");
                            var keyStr = _keyStringPool[entryKey];
                            string data = null;

                            Debug.WriteLine("Entry 0x" + idStr + ", key: " + keyStr + ", simple value type: ");

                            List<string> entryArr = null;
                            if (_entryMap.ContainsKey(int.Parse(idStr, NumberStyles.HexNumber)))
                            {
                                entryArr = _entryMap[int.Parse(idStr, NumberStyles.HexNumber)];
                            }

                            if (entryArr == null)
                            {
                                entryArr = new List<string>();
                            }

                            entryArr.Add(keyStr);
                            if (_entryMap.ContainsKey(int.Parse(idStr, NumberStyles.HexNumber)))
                            {
                                _entryMap[int.Parse(idStr, NumberStyles.HexNumber)] = entryArr;
                            }
                            else
                            {
                                _entryMap.Add(int.Parse(idStr, NumberStyles.HexNumber), entryArr);
                            }

                            if (valueDataType == TYPE_STRING)
                            {
                                data = _valueStringPool[valueData];
                                Debug.WriteLine(", data: " + _valueStringPool[valueData] + "");
                            }
                            else if (valueDataType == TYPE_REFERENCE)
                            {
                                var hexIndex = valueData.ToString("X4");
                                refKeys.Add(idStr, valueData);
                            }
                            else
                            {
                                data = valueData.ToString();
                                Debug.WriteLine(", data: " + valueData + "");
                            }

                            // if (inReqList("@" + idStr)) {
                            PutIntoMap("@" + idStr, data);
                        }
                        else
                        {
                            var entryParent = br.ReadInt32();
                            var entryCount2 = br.ReadInt32();
                            for (var j = 0; j < entryCount2; ++j)
                            {
                                var refName = br.ReadInt32();
                                var valueSize = br.ReadInt16();
                                var valueRes0 = br.ReadByte();
                                var valueDataType = br.ReadByte();
                                var valueData = br.ReadInt32();
                            }

                            Debug.WriteLine("Entry 0x"
                                            + resourceId.ToString("X4") + ", key: "
                                            + _keyStringPool[entryKey]
                                            + ", complex value, not printed.");
                        }
                    }
                    //var refKs = new HashSet<string>(refKeys.Keys);

                    foreach (var refK in refKeys.Keys)
                    {
                        List<string> values = null;
                        if (_responseMap.ContainsKey("@" + refKeys[refK].ToString("X4").ToUpper()))
                        {
                            values = _responseMap["@" + refKeys[refK].ToString("X4").ToUpper()];
                        }

                        if (values != null)
                        {
                            foreach (var value in values)
                            {
                                PutIntoMap("@" + refK, value);
                            }
                        }
                    }
                }
            }
        }


        private string[] processStringPool(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    var type = br.ReadInt16();
                    var headerSize = br.ReadInt16();
                    var size = br.ReadInt32();
                    var stringCount = br.ReadInt32();
                    var styleCount = br.ReadInt32();
                    var flags = br.ReadInt32();
                    var stringsStart = br.ReadInt32();
                    var stylesStart = br.ReadInt32();

                    var isUtf8 = (flags & 256) != 0;

                    var offsets = new int[stringCount];
                    for (var i = 0; i < stringCount; ++i)
                    {
                        offsets[i] = br.ReadInt32();
                    }
                    var strings = new string[stringCount];

                    for (var i = 0; i < stringCount; i++)
                    {
                        var pos = stringsStart + offsets[i];
                        br.BaseStream.Seek(pos, SeekOrigin.Begin);
                        strings[i] = "";
                        if (isUtf8)
                        {
                            int u16Len = br.ReadByte(); // u16len
                            if ((u16Len & 0x80) != 0)
                            {
                                // larger than 128
                                u16Len = ((u16Len & 0x7F) << 8) + br.ReadByte();
                            }

                            int u8Len = br.ReadByte(); // u8len
                            if ((u8Len & 0x80) != 0)
                            {
                                // larger than 128
                                u8Len = ((u8Len & 0x7F) << 8) + br.ReadByte();
                            }

                            if (u8Len > 0)
                            {
                                strings[i] = Encoding.UTF8.GetString(br.ReadBytes(u8Len));
                            }
                            else
                            {
                                strings[i] = "";
                            }
                        }
                        else // UTF_16
                        {
                            int u16Len = br.ReadUInt16();
                            if ((u16Len & 0x8000) != 0)
                            {
                                // larger than 32768
                                u16Len = ((u16Len & 0x7FFF) << 16) + br.ReadUInt16();
                            }

                            if (u16Len > 0)
                            {
                                strings[i] = Encoding.Unicode.GetString(br.ReadBytes(u16Len * 2));
                            }
                        }
                        Debug.WriteLine("Parsed value: {0}", strings[i]);
                    }
                    return strings;
                }
            }
        }

        private void ProcessTypeSpec(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    var type = br.ReadInt16();
                    var headerSize = br.ReadInt16();
                    var size = br.ReadInt32();
                    var id = br.ReadByte();
                    var res0 = br.ReadByte();
                    var res1 = br.ReadInt16();
                    var entryCount = br.ReadInt32();


                    Debug.WriteLine("Processing type spec {0}", _typeStringPool[id - 1]);

                    var flags = new int[entryCount];
                    for (var i = 0; i < entryCount; ++i)
                    {
                        flags[i] = br.ReadInt32();
                    }
                }
            }
        }
    }
}