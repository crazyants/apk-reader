// ReSharper disable InconsistentNaming

namespace ApkReader
{
    public class ResStringPoolHeader
    {
        public enum eResStringPoolHeaderFlag : uint
        {
            // If set, the string index is sorted by the string values (based on strcmp16()).  
            SORTED_FLAG = 1 << 0,

            // String pool is encoded in UTF-8  
            UTF8_FLAG = 1 << 8
        }

        public ResChunkHeader ResChunkHeader { get; set; }

        /// <summary>
        ///     Number of strings in this pool (number of uint32_t indices that follow in the data).
        /// </summary>
        public uint StringCount { get; set; }

        /// <summary>
        ///     Number of style span arrays in the pool (number of uint32_t indices follow the string indices).
        /// </summary>
        public uint StyleCount { get; set; }

        /// <summary>
        ///     If flags is 0x0, string pool is encoded in UTF-16
        /// </summary>
        public eResStringPoolHeaderFlag Flag { get; set; }

        /// <summary>
        ///     Index from header of the string data.
        /// </summary>
        public uint StringsStart { get; set; }

        /// <summary>
        ///     Index from header of the style data.
        /// </summary>
        public uint StylesStart { get; set; }
    }
}