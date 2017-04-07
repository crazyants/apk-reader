namespace ApkReader
{
    public class ResTablePackage
    {
        public ResChunkHeader ResChunkHeader { get; set; }

        /// <summary>
        ///     包的ID,等于Package Id,一般用户包的值Package Id为0X7F,系统资源包的Package Id为0X01。
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        ///     包名称  char16_t name[128];
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     类型字符串资源池相对头部的偏移
        /// </summary>
        public uint TypeStrings { get; set; }

        /// <summary>
        ///     最后一个导出的Public类型字符串在类型字符串资源池中的索引，目前这个值设置为类型字符串资源池的元素个数。
        /// </summary>
        public uint LastPublicType { get; set; }

        /// <summary>
        ///     资源项名称字符串相对头部的偏移
        /// </summary>
        public uint KeyStrings { get; set; }

        /// <summary>
        ///     最后一个导出的Public资源项名称字符串在资源项名称字符串资源池中的索引，目前这个值设置为资源项名称字符串资源池的元素个数。
        /// </summary>
        public uint LastPublicKey { get; set; }
    }
}