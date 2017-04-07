namespace ApkReader
{
    public class ResTableConfig
    {
        public CImsi Imsi { get; set; }
        public CLocale Locale { get; set; }
        public CScreenType ScreenType { get; set; }
        public CInput Input { get; set; }
        public CScreenSize ScreenSize { get; set; }
        public CVersion Version { get; set; }
        public CScreenConfig ScreenConfig { get; set; }
        public CScreenSizeDp ScreenSizeDp { get; set; }

        public class CImsi
        {
            /// <summary>
            ///     Mobile country code (from SIM).  0 means "any".
            /// </summary>
            public ushort Mcc { get; set; }

            /// <summary>
            ///     Mobile network code (from SIM).  0 means "any".
            /// </summary>
            public ushort Mnc { get; set; }
        }

        public class CLocale
        {
            /// <summary>
            ///     \0\0 means "any".  Otherwise, en, fr, etc.
            /// </summary>
            public string Language { get; set; }

            /// <summary>
            ///     \0\0 means "any".  Otherwise, US, CA, etc.
            /// </summary>
            public string Country { get; set; }
        }

        public class CScreenType
        {
            public byte Orientation { get; set; }
            public byte Touchscreen { get; set; }
            public ushort Density { get; set; }
        }

        public class CInput
        {
            public byte Keyboard { get; set; }
            public byte Navigation { get; set; }
            public byte InputFlags { get; set; }
            public byte InputPad0 { get; set; }
        }

        public class CScreenSize
        {
            public ushort ScreenWidth { get; set; }
            public ushort ScreenHeight { get; set; }
        }

        public class CVersion
        {
            public ushort SdkVersion { get; set; }
            // For now minorVersion must always be 0!!!  Its meaning  
            // is currently undefined.  
            public ushort MinorVersion { get; set; }
        }

        public class CScreenConfig
        {
            public byte ScreenLayout { get; set; }
            public byte UiMode { get; set; }
            public ushort SmallestScreenWidthDp { get; set; }
        }

        public class CScreenSizeDp
        {
            public ushort ScreenWidthDp { get; set; }
            public ushort ScreenHeightDp { get; set; }
        }
    }
}