//==========================================================================
//
//  File:        MyBinarySerializer.cs
//  Location:    Firefly.Examples <Visual Basic .Net>
//  Description: 自定义二进制序列化器
//  Version:     2011.03.02.
//  Author:      F.R.C.
//  Copyright(C) public Domain
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using Firefly;
using Firefly.Streaming;
using Firefly.TextEncoding;
using Firefly.Mapping;
using Firefly.Mapping.Binary;

/// <summary>自定义二进制序列化器，简便起见，这里直接序列化到字节数组，而不是流</summary>
public class MyBinarySerializer
{
    public MyBinarySerializer()
    { }

    //版本1用序列化器
    private BinarySerializer SerializerVersion1Value;
    private BinarySerializer SerializerVersion1
    {
        get
        {
            if (SerializerVersion1Value == null)
            {
                SerializerVersion1Value = new BinarySerializer();
                SerializerVersion1Value.PutReaderTranslator(new StringCodec());
                SerializerVersion1Value.PutReaderTranslator(new DataEntryVersion1To2Translator());
                SerializerVersion1Value.PutWriterTranslator(new StringCodec());
                SerializerVersion1Value.PutWriterTranslator(new DataEntryVersion1To2Translator());
            }
            return SerializerVersion1Value;
        }
    }

    //版本2用序列化器
    private BinarySerializer SerializerVersion2Value;
    private BinarySerializer SerializerVersion2
    {
        get
        {
            if (SerializerVersion2Value == null)
            {
                SerializerVersion2Value = new BinarySerializer();
                SerializerVersion2Value.PutReaderTranslator(new StringCodec());
                SerializerVersion2Value.PutWriterTranslator(new StringCodec());
            }
            return SerializerVersion2Value;
        }
    }

    /// <summary>读取版本1或2</summary>
    public DataObject Read(Byte[] Bytes)
    {
        using (var s = Streams.CreateMemoryStream())
        {
            s.Write(Bytes);
            s.Position = 0;

            if (s.ReadSimpleString(4) != "MYDF") { throw new InvalidDataException("数据不是MYDF格式数据"); }
            var Version = s.ReadUInt32();

            switch (Version)
            {
                case 1:
                    return SerializerVersion1.Read<DataObject>(s);
                case 2:
                    return SerializerVersion2.Read<DataObject>(s);
                default:
                    throw new NotSupportedException("Unknown Version");
            }
        }
    }

    /// <summary>写入版本1</summary>
    public Byte[] WriteVersion1(DataObject Obj)
    {
        using (var s = Streams.CreateMemoryStream())
        {
            var Version = 1U;
            s.WriteSimpleString("MYDF", 4);
            s.WriteUInt32(Version);
            SerializerVersion1.Write(s, Obj);

            s.Position = 0;
            return s.Read(Convert.ToInt32(s.Length));
        }
    }

    /// <summary>写入版本2</summary>
    public Byte[] Write(DataObject Obj)
    {
        using (var s = Streams.CreateMemoryStream())
        {
            var Version = 2U;
            s.WriteSimpleString("MYDF", 4);
            s.WriteUInt32(Version);
            SerializerVersion2.Write(s, Obj);

            s.Position = 0;
            return s.Read(Convert.ToInt32(s.Length));
        }
    }

    //用于将字符串转换为字节数组处理
    private class StringCodec :
        IProjectorToProjectorRangeTranslator<String, Byte[]>, //Reader
        IAggregatorToAggregatorDomainTranslator<String, Byte[]> //Writer
    {
        public Func<D, String> TranslateProjectorToProjectorRange<D>(Func<D, Byte[]> Projector)
        {
            return v => TextEncoding.UTF16.GetString(Projector(v));
        }
        public Action<String, R> TranslateAggregatorToAggregatorDomain<R>(Action<Byte[], R> Aggregator)
        {
            return (s, v) => Aggregator(TextEncoding.UTF16.GetBytes(s), v);
        }
    }
}
