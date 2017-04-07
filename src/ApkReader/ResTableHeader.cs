using System;
using System.Collections.Generic;
using System.Text;

namespace ApkReader
{
    public class ResTableHeader
    {
        public ResChunkHeader ResChunkHeader { get; set; }
        /// <summary>
        /// The number of ResTable_package structures.  
        /// </summary>
        public UInt32 PackageCount { get; set; }
    }
}
