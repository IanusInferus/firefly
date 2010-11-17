'==========================================================================
'
'  File:        Example.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 二进制序列化器示例
'  Version:     2010.11.17.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Mapping
Imports System.Xml

Public Module Example

    '本示例演示了使用Firefly.Mapping.BinarySerializer和Firefly.Mapping.XmlSerializer的方法。
    '
    'BinarySerializer支持的类型为
    ' 简单类型 ::= 简单类型
    '           | Byte(UInt8) | UInt16 | UInt32 | UInt64 | Int8(SByte) | Int16 | Int32 | Int64 | Float32(Single) | Float64(Double)
    '           | Boolean
    '           | 枚举
    '           | 数组(简单类型)
    '           | ICollection(简单类型)
    '           | 简单类或结构
    ' 简单类或结构 ::= 
    '               ( 类或结构(构造函数(参数(简单类型)*), 公共只读字段(简单类型)*, 公共可写属性{0}) AND (参数(简单类型)* = 公共只读字段(简单类型)*)
    '               | 类或结构(构造函数(参数(简单类型)*), 公共可写字段{0}, 公共只读属性(简单类型)*) AND (参数(简单类型)* = 公共只读属性(简单类型)*)
    '               | 类或结构(无参构造函数, 公共可读写字段(简单类型)*, 公共可写属性{0})
    '               | 类或结构(无参构造函数, 公共可写字段{0}, 公共可读写属性(简单类型)*)
    '               ) AND 类型结构为树状
    '
    'XmlSerializer支持的类型为
    ' 简单类型 ::= 简单类型
    '           | Byte(UInt8) | UInt16 | UInt32 | UInt64 | Int8(SByte) | Int16 | Int32 | Int64 | Float32(Single) | Float64(Double)
    '           | Boolean
    '           | String | Decimal
    '           | 枚举
    '           | 数组(简单类型)
    '           | ICollection(简单类型)
    '           | 简单类或结构
    ' 简单类或结构 ::= 
    '               ( 类或结构(构造函数(参数(简单类型)*), 公共只读字段(简单类型)*, 公共可写属性{0}) AND (参数(简单类型)* = 公共只读字段(简单类型)*)
    '               | 类或结构(构造函数(参数(简单类型)*), 公共可写字段{0}, 公共只读属性(简单类型)*) AND (参数(简单类型)* = 公共只读属性(简单类型)*)
    '               | 类或结构(无参构造函数, 公共可读写字段(简单类型)*, 公共可写属性{0})
    '               | 类或结构(无参构造函数, 公共可写字段{0}, 公共可读写属性(简单类型)*)
    '               ) AND 类型结构为树状
    '
    '其中简单类或结构是指不变记录和可变记录。不变记录应有公开的只读字段或属性及对应的构造函数。可变记录应有公开的可读写字段或属性及无参构造函数。
    '不应同时有有效的字段和属性，因为字段和属性不能有可预测的混合顺序。
    '但可变记录可以有只读的属性，可以被安全的忽略。
    '有效类型结构树中不应出现环，因为这会导致无法自动扁平化。如果出现环，应通过扩展机制提供手动处理。
    '
    '不同之处在于：
    '1) BinarySerializer不默认支持String、Decimal，需要自行添加映射；
    '2) BinarySerializer不支持空引用；
    '3) BinarySerializer不支持继承(即声明的类型与对象的实际类型不一致)，而XmlSerializer可以通过提供外部类型列表来支持。
    '
    '两个序列化器均提供三种扩展机制：
    '1) PutReader|PutWriter，用于提供直接的读写替代，直接操作需要读写的对象和数据流|数据树
    '2) PutReaderTranslator|PutWriterTranslator，提供更高层的抽象，用于将需要读写的对象替代成另一种对象，交给序列化器做后续处理
    '3) (ReaderResolver|WriterResolver).(ProjectorResolvers|AggregatorResolvers)，提供直接的类型解析替代，但此机制中类型均为运行时类型，编写代码较麻烦
    '两个序列化器均是通过第三种机制，使用动态代码生成建立起来的。
    '
    '关于代码污染的问题：
    '两个序列化器均不需要数据对象依赖于任何序列化器对象、接口或特性，一切的自定义均通过上述三种扩展机制完成。
    '这一点，相对于微软的实现是非常便利的。
    '
    '关于数据模型变更带来的版本问题：
    '本示例已经演示了版本管理的方法，即通过建立原始对象与新对象的适配器来进行。
    '这样，对于没有变化的对象，可以不用引入版本管理，在出现变化之后，再引入，且可对每个对象单独进行适配，不会出现因为某个被大量引用的类型发生变化，导致大量的代码改变的问题。
    '
    '关于兼容性的考虑：
    '对于BinarySerializer，因为足够灵活，可以被认为可以进行任意的二进制序列化，甚至是对已有的文件格式进行反序列化和再序列化。(*可能需要使用扩展机制3和ResolverTemplates)
    '对于XmlSerializer，提供兼容System.Xml.Serialization.XmlSerializer序列化的数据的方法，请参考Firefly.Setting.XmlCompatibility。
    'Firefly.Setting.XmlCompatibility主要提供对布尔类型的小写化、字节数组的Base64化、DateTime类型的特定格式、以及各种基元类型的类型名称映射等。
    '
    'Firefly.Mapping.XmlSerializer相对于System.Xml.Serialization.XmlSerializer的优势：
    '1)非插入式的扩展机制，简单版本管理。
    '2)默认支持泛型Dictionary(Tkey, TValue)等任何实现ICollection(T)一次的类型。
    '3)使用XElement，便于进一步查询等。
    '4)没有Base64的字节数组序列化，可以自行实现必要的序列化，提高可读性。
    '5)可对同一个对象树实现二进制序列化和XML序列化，便于传输、存储，以及调试、修改、配置。
    '
    Public Sub Execute()
        '创建自定义二进制序列化器实例
        Dim mbs As New MyBinarySerializer
        '创建自定义XML序列化器实例
        Dim mxs As New MyXmlSerializer

        '创建数据
        Dim Obj As New DataObject
        Obj.DataEntries.Add("DataEntry1", New DataEntry With {.Name = "DataEntry1", .Data = New Byte() {1, 2, 3, 4, 5}, .Attribute = "Version2Only"})
        Obj.DataEntries.Add("DataEntry2", New DataEntry With {.Name = "DataEntry2", .Data = New Byte() {6, 7, 8, 9, 10}, .Attribute = "Version2Only"})
        Obj.ImmutableDataEntries.Add("ImmutableDataEntry1", New ImmutableDataEntry(Of Byte())("ImmutableDataEntry1", New Byte() {1, 2, 3, 4, 5}))
        Obj.ImmutableDataEntries.Add("ImmutableDataEntry2", New ImmutableDataEntry(Of Byte())("ImmutableDataEntry2", New Byte() {6, 7, 8, 9, 10}))

        '二进制序列化
        Dim BinVersion1Bytes = mbs.WriteVersion1(Obj)
        Dim BinVersion2Bytes = mbs.Write(Obj)

        '二进制反序列化
        Dim BinVersion1RoundTripped = mbs.Read(BinVersion1Bytes)
        Dim BinVersion2RoundTripped = mbs.Read(BinVersion2Bytes)

        'XML序列化
        Dim XmlVersion1XElement = mxs.WriteVersion1(Obj)
        Dim XmlVersion2XElement = mxs.Write(Obj)

        'XML反序列化
        Dim XmlVersion1RoundTripped = mxs.Read(XmlVersion1XElement)
        Dim XmlVersion2RoundTripped = mxs.Read(XmlVersion2XElement)

        Dim Setting = New XmlWriterSettings With {.Encoding = Console.Out.Encoding, .Indent = True, .OmitXmlDeclaration = False}
        Using w = XmlWriter.Create(Console.Out, Setting)
            XmlVersion2XElement.Save(w)
        End Using

        Stop
    End Sub
End Module
