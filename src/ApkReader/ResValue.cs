// ReSharper disable InconsistentNaming

namespace ApkReader
{
    public class ResValue
    {
        public enum ResValueDataType : byte
        {
            TYPE_NULL = 0x00,
            TYPE_REFERENCE = 0x01,
            TYPE_ATTRIBUTE = 0x02,
            TYPE_STRING = 0x03,
            TYPE_FLOAT = 0x04,
            TYPE_DIMENSION = 0x05,
            TYPE_FRACTION = 0x06,
            TYPE_FIRST_INT = 0x10,
            TYPE_INT_DEC = 0x10,
            TYPE_INT_HEX = 0x11,
            TYPE_INT_BOOLEAN = 0x12,
            TYPE_FIRST_COLOR_INT = 0x1c,
            TYPE_INT_COLOR_ARGB8 = 0x1c,
            TYPE_INT_COLOR_RGB8 = 0x1d,
            TYPE_INT_COLOR_ARGB4 = 0x1e,
            TYPE_INT_COLOR_RGB4 = 0x1f,
            TYPE_LAST_COLOR_INT = 0x1f,
            TYPE_LAST_INT = 0x1f
        }

        /// <summary>
        ///     Res_value头部大小
        /// </summary>
        public ushort Size { get; set; }

        /// <summary>
        ///     保留,始终为0
        /// </summary>
        public byte Res0 { get; set; }

        /// <summary>
        ///     数据的类型
        /// </summary>
        public ResValueDataType DataType { get; set; }

        /// <summary>
        ///     数据对应的索引
        /// </summary>
        public uint Data { get; set; }
    }
}