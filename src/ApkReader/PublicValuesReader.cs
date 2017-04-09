// Copyright (c) 2015 Quamotion
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace ApkReader
{
    public class PublicValuesReader
    {
        private static readonly object ValuesLock = new object();
        private static Dictionary<uint, string> _values;

        public static Dictionary<uint, string> Values
        {
            get
            {
                // Prevent two threads from initializing the values field
                // at the same time. This can happen when this code is called from
                // parallel threads.
                lock (ValuesLock)
                {
                    if (_values == null)
                    {
                        Assembly assembly = null;

#if NETSTANDARD1_3
                        assembly = typeof(PublicValuesReader).GetTypeInfo().Assembly;
#else
                        assembly = Assembly.GetExecutingAssembly();
#endif
                        var xml = new XmlDocument();
                        using (var stream = assembly.GetManifestResourceStream("ApkReader.Values.public.xml"))
                        {
                            Debug.Assert(stream != null, "stream != null");
                            using (var sr = new StreamReader(stream, Encoding.UTF8))
                            {
                                xml.LoadXml(sr.ReadToEnd());
                            }
                        }
                        _values = new Dictionary<uint, string>();
                        if (xml.DocumentElement != null)
                        {
                            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                            {
                                if (node.Name == "public")
                                {
                                    if (node.Attributes != null)
                                    {
                                        var id = node.Attributes.GetNamedItem("id");
                                        var name = node.Attributes.GetNamedItem("name");
                                        if (id != null && name != null)
                                        {
                                            _values[Convert.ToUInt32(id.Value, 16)] = name.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return _values;
            }
        }
    }
}