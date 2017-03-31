using System;
using System.Text;

//Source: http://stackoverflow.com/a/19063830/2554459

namespace Iteedee.ApkReader
{
    public class ApkManifest
    {
        // decompressXML -- Parse the 'compressed' binary form of Android XML docs 
        // such as for AndroidManifest.xml in .apk files
        public static int StartDocTag = 0x00100100;
        public static int EndDocTag = 0x00100101;
        public static int StartTag = 0x00100102;
        public static int EndTag = 0x00100103;
        public static int TextTag = 0x00100104;


        public static string Spaces = "                                             ";
        private string _result = "";

        public string ReadManifestFileIntoXml(byte[] manifestFileData)
        {
            if (manifestFileData.Length == 0)
            {
                throw new Exception("Failed to read manifest data.  Byte array was empty");
            }
            // Compressed XML file/bytes starts with 24x bytes of data,
            // 9 32 bit words in little endian order (LSB first):
            //   0th word is 03 00 08 00
            //   3rd word SEEMS TO BE:  Offset at then of StringTable
            //   4th word is: Number of strings in string table
            // WARNING: Sometime I indiscriminently display or refer to word in 
            //   little endian storage format, or in integer format (ie MSB first).
            var numbStrings = Lew(manifestFileData, 4 * 4);

            // StringIndexTable starts at offset 24x, an array of 32 bit LE offsets
            // of the length/string data in the StringTable.
            var sitOff = 0x24; // Offset of start of StringIndexTable

            // StringTable, each string is represented with a 16 bit little endian 
            // character count, followed by that number of 16 bit (LE) (Unicode) chars.
            var stOff = sitOff + numbStrings * 4; // StringTable follows StrIndexTable

            // XMLTags, The XML tag tree starts after some unknown content after the
            // StringTable.  There is some unknown data after the StringTable, scan
            // forward from this point to the flag for the start of an XML start tag.
            var xmlTagOff = Lew(manifestFileData, 3 * 4); // Start from the offset in the 3rd word.
            // Scan forward until we find the bytes: 0x02011000(x00100102 in normal int)
            for (var ii = xmlTagOff; ii < manifestFileData.Length - 4; ii += 4)
            {
                if (Lew(manifestFileData, ii) == StartTag)
                {
                    xmlTagOff = ii;
                    break;
                }
            } // end of hack, scanning for start of first start tag

            // XML tags and attributes:
            // Every XML start and end tag consists of 6 32 bit words:
            //   0th word: 02011000 for startTag and 03011000 for endTag 
            //   1st word: a flag?, like 38000000
            //   2nd word: Line of where this tag appeared in the original source file
            //   3rd word: FFFFFFFF ??
            //   4th word: StringIndex of NameSpace name, or FFFFFFFF for default NS
            //   5th word: StringIndex of Element Name
            //   (Note: 01011000 in 0th word means end of XML document, endDocTag)

            // Start tags (not end tags) contain 3 more words:
            //   6th word: 14001400 meaning?? 
            //   7th word: Number of Attributes that follow this tag(follow word 8th)
            //   8th word: 00000000 meaning??

            // Attributes consist of 5 words: 
            //   0th word: StringIndex of Attribute Name's Namespace, or FFFFFFFF
            //   1st word: StringIndex of Attribute Name
            //   2nd word: StringIndex of Attribute Value, or FFFFFFF if ResourceId used
            //   3rd word: Flags?
            //   4th word: str ind of attr value again, or ResourceId of value

            // TMP, dump string table to tr for debugging
            //tr.addSelect("strings", null);
            //for (int ii=0; ii<numbStrings; ii++) {
            //  // Length of string starts at StringTable plus offset in StrIndTable
            //  String str = compXmlString(xml, sitOff, stOff, ii);
            //  tr.add(String.valueOf(ii), str);
            //}
            //tr.parent();

            // Step through the XML tree element tags and attributes
            var off = xmlTagOff;
            var indent = 0;
            var startDocTagCounter = 1;
            while (off < manifestFileData.Length)
            {
                var tag0 = Lew(manifestFileData, off);
                //int tag1 = LEW(manifestFileData, off+1*4);
                var lineNo = Lew(manifestFileData, off + 2 * 4);
                //int tag3 = LEW(manifestFileData, off+3*4);
                var nameNsSi = Lew(manifestFileData, off + 4 * 4);
                var nameSi = Lew(manifestFileData, off + 5 * 4);

                if (tag0 == StartTag)
                {
                    // XML START TAG
                    var tag6 = Lew(manifestFileData, off + 6 * 4); // Expected to be 14001400
                    var numbAttrs = Lew(manifestFileData, off + 7 * 4); // Number of Attributes to follow
                    //int tag8 = LEW(manifestFileData, off+8*4);  // Expected to be 00000000
                    off += 9 * 4; // Skip over 6+3 words of startTag data
                    var name = CompXmlString(manifestFileData, sitOff, stOff, nameSi);
                    //tr.addSelect(name, null);

                    // Look for the Attributes

                    var sb = "";
                    for (var ii = 0; ii < numbAttrs; ii++)
                    {
                        var attrNameNsSi = Lew(manifestFileData, off); // AttrName Namespace Str Ind, or FFFFFFFF
                        var attrNameSi = Lew(manifestFileData, off + 1 * 4); // AttrName String Index
                        var attrValueSi = Lew(manifestFileData, off + 2 * 4); // AttrValue Str Ind, or FFFFFFFF
                        var attrFlags = Lew(manifestFileData, off + 3 * 4);
                        var attrResId = Lew(manifestFileData, off + 4 * 4);
                        // AttrValue ResourceId or dup AttrValue StrInd
                        off += 5 * 4; // Skip over the 5 words of an attribute

                        var attrName = CompXmlString(manifestFileData, sitOff, stOff, attrNameSi);
                        var attrValue = attrValueSi != -1
                            ? CompXmlString(manifestFileData, sitOff, stOff, attrValueSi)
                            : /*"resourceID 0x" + */attrResId.ToString();
                        sb += " " + attrName + "=\"" + attrValue + "\"";
                        //tr.add(attrName, attrValue);
                    }
                    PrtIndent(indent, "<" + name + sb + ">");
                    indent++;
                }
                else if (tag0 == EndTag)
                {
                    // XML END TAG
                    indent--;
                    off += 6 * 4; // Skip over 6 words of endTag data
                    var name = CompXmlString(manifestFileData, sitOff, stOff, nameSi);
                    PrtIndent(indent, "</" + name + ">  \r\n" /*+"(line " + startTagLineNo + "-" + lineNo + ")"*/);
                    //tr.parent();  // Step back up the NobTree
                }
                else if (tag0 == StartDocTag)
                {
                    startDocTagCounter++;
                    off += 4;
                }
                else if (tag0 == EndDocTag)
                {
                    // END OF XML DOC TAG
                    startDocTagCounter--;
                    if (startDocTagCounter == 0)
                    {
                        break;
                    }
                }
                else if (tag0 == TextTag)
                {
                    // code "copied" https://github.com/mikandi/php-apk-parser/blob/fixed-mikandi-version/lib/ApkParser/XmlParser.php
                    var sentinal = 0xffffffff;
                    while (off < manifestFileData.Length)
                    {
                        var curr = (uint) Lew(manifestFileData, off);
                        off += 4;
                        if (off > manifestFileData.Length)
                        {
                            throw new Exception("Sentinal not found before end of file");
                        }
                        if (curr == sentinal && sentinal == 0xffffffff)
                        {
                            sentinal = 0x00000000;
                        }
                        else if (curr == sentinal)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    Prt("  Unrecognized tag code '" + tag0.ToString("X")
                        + "' at offset " + off);
                    break;
                }
            } // end of while loop scanning tags and attributes of XML tree
            //prt("    end at offset " + off);


            return _result;
        } // end of decompressXML


        public string CompXmlString(byte[] xml, int sitOff, int stOff, int strInd)
        {
            if (strInd < 0)
            {
                return null;
            }
            var strOff = stOff + Lew(xml, sitOff + strInd * 4);
            return CompXmlStringAt(xml, strOff);
        }

        public void PrtIndent(int indent, string str)
        {
            Prt(Spaces.Substring(0, Math.Min(indent * 2, Spaces.Length)) + str);
        }

        private void Prt(string p)
        {
            _result += p;
        }


        // compXmlStringAt -- Return the string stored in StringTable format at
        // offset strOff.  This offset points to the 16 bit string length, which 
        // is followed by that number of 16 bit (Unicode) chars.
        public string CompXmlStringAt(byte[] arr, int strOff)
        {
            var strLen = ((arr[strOff + 1] << 8) & 0xff00) | (arr[strOff] & 0xff);
            var chars = new byte[strLen];
            for (var ii = 0; ii < strLen; ii++)
            {
                chars[ii] = arr[strOff + 2 + ii * 2];
            }


            return Encoding.UTF8.GetString(chars); // Hack, just use 8 byte chars
        } // end of compXmlStringAt


        // LEW -- Return value of a Little Endian 32 bit word from the byte array
        //   at offset off.
        public int Lew(byte[] arr, int off)
        {
            //return (int)(arr[off + 3] << 24 & 0xff000000 | arr[off + 2] << 16 & 0xff0000 | arr[off + 1] << 8 & 0xff00 | arr[off] & 0xFF);
            return
                (int)
                ((((uint) arr[off + 3] << 24) & 0xff000000) | (((uint) arr[off + 2] << 16) & 0xff0000) |
                 (((uint) arr[off + 1] << 8) & 0xff00) | ((uint) arr[off] & 0xFF));
        } // end of LEW
    }
}