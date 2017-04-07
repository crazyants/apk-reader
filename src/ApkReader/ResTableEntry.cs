// ReSharper disable InconsistentNaming

using System;

namespace ApkReader
{
    public class ResTableEntry
    {
        public enum ResTableEntryFlag : ushort
        {
            //如果flags此位为1,则ResTable_entry后跟随ResTable_map数组,为0则跟随一个Res_value。  
            FLAG_COMPLEX = 0x0001,
            //如果此位为1,这个一个被引用的资源项  
            FLAG_PUBLIC = 0x0002
        }
        /// <summary>
        /// 表示资源项头部大小。  
        /// </summary>

        public ushort Size { get; set; }
        /// <summary>
        /// 资源项标志位  
        /// </summary>
        public ResTableEntryFlag Flags { get; set; }
        /// <summary>
        /// 资源项名称在资源项名称字符串资源池的索引  
        /// </summary>
        public UInt32 Key { get; set; }
    }
}