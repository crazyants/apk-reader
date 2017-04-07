// ReSharper disable InconsistentNaming

using System.IO;

namespace ApkReader
{
    public class ResChunkHeader
    {
        public enum eResChunkHeaderType : ushort
        {
            RES_NULL_TYPE = 0x0000,
            RES_STRING_POOL_TYPE = 0x0001,
            RES_TABLE_TYPE = 0x0002,
            RES_XML_TYPE = 0x0003,
            RES_XML_FIRST_CHUNK_TYPE = 0x0100,
            RES_XML_START_NAMESPACE_TYPE = 0x0100,
            RES_XML_END_NAMESPACE_TYPE = 0x0101,
            RES_XML_START_ELEMENT_TYPE = 0x0102,
            RES_XML_END_ELEMENT_TYPE = 0x0103,
            RES_XML_CDATA_TYPE = 0x0104,
            RES_XML_LAST_CHUNK_TYPE = 0x017f,
            RES_XML_RESOURCE_MAP_TYPE = 0x0180,
            RES_TABLE_PACKAGE_TYPE = 0x0200,
            RES_TABLE_TYPE_TYPE = 0x0201,
            RES_TABLE_TYPE_SPEC_TYPE = 0x0202
        }

        /// <summary>
        ///     当前这个chunk的类型
        /// </summary>
        public eResChunkHeaderType Type { get; set; }

        /// <summary>
        ///     当前这个chunk的头部大小
        /// </summary>
        public ushort HeaderSize { get; set; }

        /// <summary>
        ///     当前这个chunk的大小
        /// </summary>
        public uint Size { get; set; }

        public static ResChunkHeader Read(BinaryReader binaryReader)
        {
            var item = new ResChunkHeader
            {
                Type = (eResChunkHeaderType) binaryReader.ReadInt16(),
                HeaderSize = binaryReader.ReadUInt16(),
                Size = binaryReader.ReadUInt32()
            };
            return item;
        }
    }
}