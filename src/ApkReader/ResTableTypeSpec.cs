namespace ApkReader
{
    public class ResTableTypeSpec
    {
        public ResChunkHeader ResChunkHeader { get; set; }

        /// <summary>
        ///     标识资源的Type ID,Type ID是指资源的类型ID，从1开始。资源的类型有animator、anim、color、drawable、layout、menu、raw、string和xml等等若干种，每一种都会被赋予一个ID。
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
    }
}