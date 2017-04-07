namespace ApkReader
{
    public class ResTableMapEntry : ResTableEntry
    {
        /// <summary>
        ///     指向父ResTable_map_entry的资源ID，如果没有父ResTable_map_entry，则等于0。
        /// </summary>
        public uint Parent { get; set; }

        /// <summary>
        ///     等于后面ResTable_map的数量
        /// </summary>
        public uint Count { get; set; }
    }
}