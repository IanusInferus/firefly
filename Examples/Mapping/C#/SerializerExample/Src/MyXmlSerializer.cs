//==========================================================================
//
//  File:        MyXmlSerializer.cs
//  Location:    Firefly.Examples <Visual Basic .Net>
//  Description: 自定义XML序列化器
//  Version:     2011.03.02.
//  Author:      F.R.C.
//  Copyright(C) public Domain
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Firefly;
using Firefly.TextEncoding;
using Firefly.Mapping;
using Firefly.Mapping.XmlText;

/// <summary>自定义序列化器，简便起见，这里直接序列化到字节数组，而不是流</summary>
public class MyXmlSerializer
{
    public MyXmlSerializer()
    { }

    //版本1用序列化器
    private XmlSerializer SerializerVersion1Value;
    private XmlSerializer SerializerVersion1
    {
        get
        {
            if (SerializerVersion1Value == null)
            {
                SerializerVersion1Value = new XmlSerializer();
                SerializerVersion1Value.PutReaderTranslator(new ByteArrayCodec());
                SerializerVersion1Value.PutReaderTranslator(new DataEntryVersion1To2Translator());
                SerializerVersion1Value.PutWriterTranslator(new ByteArrayCodec());
                SerializerVersion1Value.PutWriterTranslator(new DataEntryVersion1To2Translator());
            }
            return SerializerVersion1Value;
        }
    }

    //版本2用序列化器
    private XmlSerializer SerializerVersion2Value;
    private XmlSerializer SerializerVersion2
    {
        get
        {
            if (SerializerVersion2Value == null)
            {
                SerializerVersion2Value = new XmlSerializer();
                SerializerVersion2Value.PutReaderTranslator(new ByteArrayCodec());
                SerializerVersion2Value.PutWriterTranslator(new ByteArrayCodec());
            }
            return SerializerVersion2Value;
        }
    }

    private static String TryGetValue(XElement Element, String AttributeName)
    {
        var a = Element.Attribute(AttributeName);
        if (a == null)
        {
            return null;
        }
        else
        {
            return a.Value;
        }
    }

    /// <summary>读取版本1或2</summary>
    public DataObject Read(XElement Element)
    {
        var SchemaType = TryGetValue(Element, "SchemaType");
        if (SchemaType != "MyDataFormat") { throw new InvalidDataException("数据不是MYDF格式数据"); }
        var Version = int.Parse(TryGetValue(Element, "Version"));

        switch (Version)
        {
            case 1:
                return SerializerVersion1.Read<DataObject>(Element);
            case 2:
                return SerializerVersion2.Read<DataObject>(Element);
            default:
                throw new NotSupportedException("Unknown Version");
        }
    }

    /// <summary>写入版本1</summary>
    public XElement WriteVersion1(DataObject Obj)
    {
        var Element = SerializerVersion1.Write(Obj);
        Element.SetAttributeValue("SchemaType", "MyDataFormat");
        Element.SetAttributeValue("Version", 1);
        return Element;
    }

    /// <summary>写入版本2</summary>
    public XElement Write(DataObject Obj)
    {
        var Element = SerializerVersion2.Write(Obj);
        Element.SetAttributeValue("SchemaType", "MyDataFormat");
        Element.SetAttributeValue("Version", 2);
        return Element;
    }

    //用于将字节数组转换为字符串处理
    private class ByteArrayCodec :
        IProjectorToProjectorRangeTranslator<Byte[], String>, //Reader
        IProjectorToProjectorDomainTranslator<Byte[], String> //Writer
    {
        public Func<D, Byte[]> TranslateProjectorToProjectorRange<D>(Func<D, String> Projector)
        {
            return k => Regex.Split(Projector(k).Trim(" \t\r\n".Descape().ToCharArray()), @"( |\t|\r|\n)+", RegexOptions.ExplicitCapture).Select(s => Byte.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray();
        }
        public Func<Byte[], R> TranslateProjectorToProjectorDomain<R>(Func<String, R> Projector)
        {
            return ba => Projector(String.Join(" ", (ba.Select(b => b.ToString("X2")).ToArray())));
        }
    }
}
