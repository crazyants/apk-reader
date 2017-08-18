using System;
using System.IO;
using System.Text;
using System.Xml;
using ApkReader.Res;
using ValueType = ApkReader.Res.ValueType;
using System.Drawing;
namespace ApkReader
{
    public static class BinaryXmlConvert
    {
        public static XmlDocument ToXmlDocument(Stream source)
        {
            var xml = new XmlDocument();
            var parser = new ResXmlParser(source);
            var xmlStringBuilder = new StringBuilder();
            using (var sw = new StringWriter(xmlStringBuilder))
            {
                using (var xmlTextWriter = XmlWriter.Create(sw))
                {
                    XmlNamespace xmlNamespace = null;
                    do
                    {
                        switch (parser.Next())
                        {
                            case ResXmlParser.XmlParserEventCode.NotStarted:
                                throw new InvalidOperationException(parser.EventCode.ToString());
                            case ResXmlParser.XmlParserEventCode.BadDocument:
                                throw new InvalidOperationException(parser.EventCode.ToString());
                            case ResXmlParser.XmlParserEventCode.StartDocument:
                                xmlTextWriter.WriteStartDocument();
                                break;
                            case ResXmlParser.XmlParserEventCode.EndDocument:
                                xmlTextWriter.WriteEndDocument();
                                break;
                            case ResXmlParser.XmlParserEventCode.StartNamespace:
                                xmlNamespace = new XmlNamespace
                                {
                                    Uri = parser.NamespaceUri,
                                    Prefix = parser.NamespacePrefix
                                };
                                break;
                            case ResXmlParser.XmlParserEventCode.EndNamespace:
                                xmlNamespace = null;
                                break;
                            case ResXmlParser.XmlParserEventCode.StartTag:
                                if (xmlNamespace != null && parser.ElementNamespace == xmlNamespace.Uri)
                                {
                                    xmlTextWriter.WriteStartElement(xmlNamespace.Prefix, parser.ElementName,
                                        parser.ElementNamespace);
                                }
                                else
                                {
                                    xmlTextWriter.WriteStartElement(parser.ElementName, parser.ElementNamespace);
                                }
                                if (parser.AttributeCount > 0)
                                {
                                    for (uint i = 0; i < parser.AttributeCount; i++)
                                    {
                                        var attr = parser.GetAttribute(i);
                                        if (xmlNamespace != null && attr.Namespace == xmlNamespace.Uri)
                                        {
                                            xmlTextWriter.WriteStartAttribute(xmlNamespace.Prefix, attr.Name,
                                                attr.Namespace);
                                        }
                                        else
                                        {
                                            xmlTextWriter.WriteStartAttribute(attr.Name, attr.Namespace);
                                        }
                                        xmlTextWriter.WriteString(FormatValue(parser, attr.TypedValue));
                                        xmlTextWriter.WriteEndAttribute();
                                    }
                                }
                                break;
                            case ResXmlParser.XmlParserEventCode.EndTag:
                                xmlTextWriter.WriteEndElement();
                                break;
                            case ResXmlParser.XmlParserEventCode.Text:
                                break;
                        }
                    } while (parser.EventCode != ResXmlParser.XmlParserEventCode.EndDocument);
                }
            }
            xml.LoadXml(xmlStringBuilder.ToString());
            return xml;
        }

        private static string FormatValue(ResXmlParser parser, Res_value value)
        {
            Color c;
            int index0;
            switch (value.DataType)
            {
                case ValueType.TYPE_STRING:
                    return parser.GetString(value.StringValue);
                case ValueType.TYPE_NULL:
                    return "null";
                case ValueType.TYPE_FLOAT:
                    return value.FloatValue.ToString("g");
                case ValueType.TYPE_FRACTION:
                    index0 = (int) value.ComplexFractionUnit;
                    return $"{value.ComplexValue:g}{(index0 < 2 ? new[] {"%", "%p"}[index0] : "?")}";
                case ValueType.TYPE_DIMENSION:
                    index0 = (int) value.ComplexDimensionUnit;
                    return
                        $"{value.ComplexValue:g}{(index0 < 6 ? new[] {"px", "dip", "sp", "pt", "in", "mm"}[index0] : "?")}";
                case ValueType.TYPE_INT_DEC:
                    return $"{value.IntValue:d}";
                case ValueType.TYPE_INT_HEX:
                    return $"0x{value.IntValue:x}";
                case ValueType.TYPE_INT_BOOLEAN:
                    return value.IntValue == 0 ? "false" : "true";
                case ValueType.TYPE_INT_COLOR_ARGB8:
                    c = value.ColorValue;
                    return $"#{c.A:x2}{c.R:x2}{c.G:x2}{c.B:x2}";
                case ValueType.TYPE_INT_COLOR_ARGB4:
                    c = value.ColorValue;
                    return $"#{c.A / 51:x1}{c.R / 51:x1}{c.G / 51:x1}{c.B / 51:x1}";
                case ValueType.TYPE_INT_COLOR_RGB8:
                    c = value.ColorValue;
                    return $"#{c.R:x2}{c.G:x2}{c.B:x2}";
                case ValueType.TYPE_INT_COLOR_RGB4:
                    c = value.ColorValue;
                    return $"#{c.R / 51:x1}{c.G / 51:x1}{c.B / 51:x1}";
                case ValueType.TYPE_REFERENCE:
                    var ident = value.ReferenceValue.Ident;
                    if (ident == null)
                    {
                        return "@undef";
                    }
                    return $"@{ident.Value:x8}";
                default:
                    return $"({value.DataType}:{value.RawData:x8})";
            }
        }

        private class XmlNamespace
        {
            public string Uri { get; set; }
            public string Prefix { get; set; }
        }
    }
}