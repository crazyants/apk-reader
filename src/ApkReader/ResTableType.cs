namespace ApkReader
{
    public class ResTableType
    {
        public ResChunkHeader ResChunkHeader { get; set; }

        /// <summary>
        ///     标识资源的Type ID
        /// </summary>
        public byte Id { get; set; }

        /// <summary>
        ///     保留,始终为0
        /// </summary>
        public byte Res0 { get; set; }

        /// <summary>
        ///     保留,始终为0
        /// </summary>
        public ushort Res1 { get; set; }

        /// <summary>
        ///     等于本类型的资源项个数,指名称相同的资源项的个数。
        /// </summary>
        public uint EntryCount { get; set; }

        /// <summary>
        ///     等于资源项数据块相对头部的偏移值。
        /// </summary>
        public uint EntriesStart { get; set; }

        /// <summary>
        ///     指向一个ResTable_config,用来描述配置信息,地区,语言,分辨率等
        /// </summary>
        public ResTableConfig ResTableConfig { get; set; }
    }
}