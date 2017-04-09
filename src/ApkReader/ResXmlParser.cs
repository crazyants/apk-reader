// Copyright (c) 2012 Markus Jarderot
// Copyright (c) 2016 Quamotion
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.IO;
using ApkReader.Res;
using ApkReader.Utils;

namespace ApkReader
{
    public class ResXmlParser
    {
        #region XmlParserEventCode enum

        public enum XmlParserEventCode
        {
            NotStarted,
            BadDocument,
            StartDocument,
            EndDocument,
            Closed,

            StartNamespace = ResourceType.RES_XML_START_NAMESPACE_TYPE,
            EndNamespace = ResourceType.RES_XML_END_NAMESPACE_TYPE,
            StartTag = ResourceType.RES_XML_START_ELEMENT_TYPE,
            EndTag = ResourceType.RES_XML_END_ELEMENT_TYPE,
            Text = ResourceType.RES_XML_CDATA_TYPE
        }

        #endregion

        private readonly IEnumerator<XmlParserEventCode> _parserIterator;

        private List<ResXMLTree_attribute> _attributes;
        private object _currentExtension;
        private ResXMLTree_node _currentNode;
        private ResReader _reader;

        public ResXmlParser(Stream source)
        {
            _reader = new ResReader(source);
            EventCode = XmlParserEventCode.NotStarted;
            _parserIterator = ParserIterator().GetEnumerator();
        }

        public ResStringPool Strings { get; private set; }

        public ResResourceMap ResourceMap { get; private set; }

        public XmlParserEventCode EventCode { get; private set; }

        public uint? CommentId => _currentNode?.Comment.Index;

        public string Comment => GetString(CommentId);

        public uint? LineNumber => _currentNode?.LineNumber;

        public uint? NamespacePrefixId
        {
            get
            {
                var namespaceExt = _currentExtension as ResXMLTree_namespaceExt;
                return namespaceExt?.Prefix.Index;
            }
        }

        public string NamespacePrefix => GetString(NamespacePrefixId);

        public uint? NamespaceUriId
        {
            get
            {
                var namespaceExt = _currentExtension as ResXMLTree_namespaceExt;
                return namespaceExt?.Uri.Index;
            }
        }

        public string NamespaceUri => GetString(NamespaceUriId);

        public uint? CDataId
        {
            get
            {
                var cdataExt = _currentExtension as ResXMLTree_cdataExt;
                return cdataExt?.Data.Index;
            }
        }

        public string CData => GetString(CDataId);

        public uint? ElementNamespaceId
        {
            get
            {
                var attrExt = _currentExtension as ResXMLTree_attrExt;
                if (attrExt != null)
                {
                    return attrExt.Namespace.Index;
                }
                var endElementExt = _currentExtension as ResXMLTree_endElementExt;
                return endElementExt?.Namespace.Index;
            }
        }

        public string ElementNamespace => GetString(ElementNamespaceId);

        public uint? ElementNameId
        {
            get
            {
                var attrExt = _currentExtension as ResXMLTree_attrExt;
                if (attrExt != null)
                {
                    return attrExt.Name.Index;
                }
                var endElementExt = _currentExtension as ResXMLTree_endElementExt;
                return endElementExt?.Name.Index;
            }
        }

        public string ElementName => GetString(ElementNameId);

        public uint? ElementIdIndex
        {
            get
            {
                var attrExt = _currentExtension as ResXMLTree_attrExt;
                return attrExt?.IdIndex;
            }
        }

        public AttributeInfo ElementId => GetAttribute(ElementIdIndex);

        public uint? ElementClassIndex
        {
            get
            {
                var attrExt = _currentExtension as ResXMLTree_attrExt;
                return attrExt?.ClassIndex;
            }
        }

        public AttributeInfo ElementClass => GetAttribute(ElementClassIndex);

        public uint? ElementStyleIndex
        {
            get
            {
                var attrExt = _currentExtension as ResXMLTree_attrExt;
                return attrExt?.StyleIndex;
            }
        }

        public AttributeInfo ElementStyle => GetAttribute(ElementStyleIndex);

        public uint AttributeCount => _attributes == null ? 0 : (uint) _attributes.Count;

        public void Restart()
        {
            throw new NotSupportedException();
        }

        public XmlParserEventCode Next()
        {
            if (_parserIterator.MoveNext())
            {
                EventCode = _parserIterator.Current;
                return _parserIterator.Current;
            }
            EventCode = XmlParserEventCode.EndDocument;
            return EventCode;
        }

        internal string GetString(uint? index)
        {
            if (index == null)
            {
                return "";
            }
            if (index < ResourceMap.ResouceIds.Count)
            {
                var identifier = ResourceMap.ResouceIds[(int) index];
                if (PublicValuesReader.Values.ContainsKey(identifier))
                {
                    return PublicValuesReader.Values[identifier];
                }
            }
            return Strings.GetString(index);
        }

        public string GetString(ResStringPool_ref reference)
        {
            return GetString(reference.Index);
        }

        private void ClearState()
        {
            _currentNode = null;
            _currentExtension = null;
            _attributes = null;
        }

        private IEnumerable<XmlParserEventCode> ParserIterator()
        {
            while (true)
            {
                ClearState();

                if (_reader.BaseStream.Position >= _reader.BaseStream.Length)
                {
                    // If we're at the end of the file, stop reading chunks.
                    // Don't try to catch an EndOfStreamException - this way,
                    // we avoid an exception being created.
                    break;
                }

                ResChunk_header header;
                try
                {
                    header = _reader.ReadResChunk_header();
                }
                catch (EndOfStreamException)
                {
                    // Keep this just in case.
                    break;
                }

                var subStream = new BoundedStream(_reader.BaseStream, header.Size - 8);
                var subReader = new ResReader(subStream);
                switch (header.Type)
                {
                    case ResourceType.RES_XML_TYPE:
                        yield return XmlParserEventCode.StartDocument;
                        _reader = subReader; // Bound whole file
                        continue; // Don't skip content
                    case ResourceType.RES_STRING_POOL_TYPE:
                        var stringPoolHeader = subReader.ReadResStringPool_header(header);
                        Strings = subReader.ReadResStringPool(stringPoolHeader);
                        break;
                    case ResourceType.RES_XML_RESOURCE_MAP_TYPE:
                        var resourceMap = subReader.ReadResResourceMap(header);
                        ResourceMap = resourceMap;
                        break;
                    case ResourceType.RES_XML_START_NAMESPACE_TYPE:
                        _currentNode = subReader.ReadResXMLTree_node(header);
                        _currentExtension = subReader.ReadResXMLTree_namespaceExt();
                        yield return XmlParserEventCode.StartNamespace;
                        break;
                    case ResourceType.RES_XML_END_NAMESPACE_TYPE:
                        _currentNode = subReader.ReadResXMLTree_node(header);
                        _currentExtension = subReader.ReadResXMLTree_namespaceExt();
                        yield return XmlParserEventCode.EndNamespace;
                        break;
                    case ResourceType.RES_XML_START_ELEMENT_TYPE:
                        _currentNode = subReader.ReadResXMLTree_node(header);
                        var attrExt = subReader.ReadResXMLTree_attrExt();
                        _currentExtension = attrExt;

                        _attributes = new List<ResXMLTree_attribute>();
                        for (var i = 0; i < attrExt.AttributeCount; i++)
                        {
                            _attributes.Add(subReader.ReadResXMLTree_attribute());
                        }
                        yield return XmlParserEventCode.StartTag;
                        break;
                    case ResourceType.RES_XML_END_ELEMENT_TYPE:
                        _currentNode = subReader.ReadResXMLTree_node(header);
                        _currentExtension = subReader.ReadResXMLTree_endElementExt();
                        yield return XmlParserEventCode.EndTag;
                        break;
                    case ResourceType.RES_XML_CDATA_TYPE:
                        _currentNode = subReader.ReadResXMLTree_node(header);
                        _currentExtension = subReader.ReadResXMLTree_cdataExt();
                        yield return XmlParserEventCode.Text;
                        break;
                    default:
                        Console.WriteLine("Warning: Skipping chunk of type {0} (0x{1:x4})",
                            header.Type, (int) header.Type);
                        break;
                }
                var junk = subStream.ReadFully();
                if (junk.Length > 0)
                {
                    Console.WriteLine("Warning: Skipping {0} bytes at the end of a {1} (0x{2:x4}) chunk.",
                        junk.Length, header.Type, (int) header.Type);
                }
            }
        }

        public AttributeInfo GetAttribute(uint? index)
        {
            if (index == null || _attributes == null)
            {
                return null;
            }
            if (index >= _attributes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var attr = _attributes[(int) index];
            return new AttributeInfo(this, attr);
        }

        public uint? IndexOfAttribute(string ns, string attribute)
        {
            var nsId = Strings.IndexOfString(ns);
            var nameId = Strings.IndexOfString(attribute);
            if (nameId == null)
            {
                return null;
            }
            uint index = 0;
            foreach (var attr in _attributes)
            {
                if (attr.Namespace.Index == nsId && attr.Name.Index == nameId)
                {
                    return index;
                }
                index++;
            }
            return null;
        }

        public void Close()
        {
            if (EventCode == XmlParserEventCode.Closed)
            {
                return;
            }
            EventCode = XmlParserEventCode.Closed;
            (_reader as IDisposable)?.Dispose();
        }

        #region Nested type: AttributeInfo

        public class AttributeInfo
        {
            private readonly ResXmlParser _parser;

            public AttributeInfo(ResXmlParser parser, ResXMLTree_attribute attribute)
            {
                _parser = parser;
                TypedValue = attribute.TypedValue;
                ValueStringId = attribute.RawValue.Index;
                NameId = attribute.Name.Index;
                NamespaceId = attribute.Namespace.Index;
            }

            public uint? NamespaceId { get; }

            public string Namespace => _parser.GetString(NamespaceId);

            public uint? NameId { get; }

            public string Name => _parser.GetString(NameId);

            public uint? ValueStringId { get; }

            public string ValueString => _parser.GetString(ValueStringId);

            public Res_value TypedValue { get; private set; }
        }

        #endregion
    }
}