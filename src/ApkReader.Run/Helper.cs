using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ApkReader.Run
{
    public static class Helper
    {
        public static IDictionary<ResChunkHeader, byte[]> GetChunks(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    return GetChunks(br);
                }
            }
        }
        public static IDictionary<ResChunkHeader, byte[]> GetChunks(BinaryReader binaryReader)
        {
            var dic = new Dictionary<ResChunkHeader, byte[]>();
            var stream = binaryReader.BaseStream;
            while (true)
            {
                var resChunk = ResChunkHeader.Read(binaryReader);
                stream.Seek(-8, SeekOrigin.Current);
                dic.Add(resChunk, binaryReader.ReadBytes(Convert.ToInt32(resChunk.Size)));
                if (stream.Length == stream.Position)
                {
                    break;
                }
            }
            return dic;
        }

        public static String[] ReadStringPool(Byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    var chunkHeader = ResChunkHeader.Read(br);
                    if (chunkHeader.Type != ResChunkHeader.eResChunkHeaderType.RES_STRING_POOL_TYPE)
                    {
                        throw new Exception();
                    }
                    var header = new ResStringPoolHeader
                    {
                        ResChunkHeader = chunkHeader,
                        StringCount = br.ReadUInt32(),
                        StyleCount = br.ReadUInt32(),
                        Flag = (ResStringPoolHeader.eResStringPoolHeaderFlag)br.ReadUInt32(),
                        StringsStart = br.ReadUInt32(),
                        StylesStart = br.ReadUInt32()
                    }; var stringOffsets = new int[header.StringCount];
                    var styleOffsets = new int[header.StyleCount];

                    var strings = new string[header.StringCount];
                    for (var i = 0; i < header.StringCount; i++)
                    {
                        stringOffsets[i] = br.ReadInt32();
                    }
                    for (var i = 0; i < header.StyleCount; i++)
                    {
                        styleOffsets[i] = br.ReadInt32();
                    }
                    //Read String
                    for (var i = 0; i < header.StringCount; i++)
                    {
                        ms.Seek(header.StringsStart + stringOffsets[i], SeekOrigin.Begin);
                        if (header.Flag == ResStringPoolHeader.eResStringPoolHeaderFlag.UTF8_FLAG)
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
                                strings[i] = string.Empty;
                            }
                        }
                        else
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
                            else
                            {
                                strings[i] = string.Empty;
                            }
                        }
                    }
                    return strings;
                }
            }
        }
    }
}