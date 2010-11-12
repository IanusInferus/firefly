'==========================================================================
'
'  File:        XmlSerializer.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: Xml序列化类
'  Version:     2010.11.12.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

''' <remarks>
''' 对于非简单类型，应提供自定义序列化器
''' 简单类型 ::= 简单类型
'''           | Byte(UInt8) | UInt16 | UInt32 | UInt64 | Int8(SByte) | Int16 | Int32 | Int64 | Float32(Single) | Float64(Double)
'''           | String | Decimal
'''           | 枚举
'''           | 数组(简单类型)
'''           | ICollection(简单类型)
'''           | 简单类或结构
''' 简单类或结构 ::= 
'''               ( 类或结构(构造函数(参数(简单类型)*), 公共只读字段(简单类型)*, 公共可写属性{0}) AND (参数(简单类型)* = 公共只读字段(简单类型)*)
'''               | 类或结构(构造函数(参数(简单类型)*), 公共可写字段{0}, 公共只读属性(简单类型)*) AND (参数(简单类型)* = 公共只读属性(简单类型)*)
'''               | 类或结构(无参构造函数, 公共可读写字段(简单类型)*, 公共可写属性{0})
'''               | 类或结构(无参构造函数, 公共可写字段{0}, 公共可读写属性(简单类型)*)
'''               ) AND 类型结构为树状
''' </remarks>
Public Class XmlSerializer
    Private ReaderMapperValue As ObjectOneToManyMapper(Of XElement)
    Public ReadOnly Property ReaderMapper As ObjectOneToManyMapper(Of XElement)
        Get
            Return ReaderMapperValue
        End Get
    End Property
    Private WriterMapperValue As ObjectManyToOneMapper(Of XElement)
    Public ReadOnly Property WriterMapper As ObjectManyToOneMapper(Of XElement)
        Get
            Return WriterMapperValue
        End Get
    End Property

    Public Sub New()
        ReaderMapperValue = New ObjectOneToManyMapper(Of XElement)
        ReaderMapperValue.Resolvers.AddRange(New List(Of IObjectOneToManyMapperResolver(Of XElement)) From {
            New PrimitiveMapperResolver(),
            New ObjectOneToManyMapper(Of XElement).EnumMapperResolver(AddressOf ReaderMapperValue.Map),
            New ObjectOneToManyMapper(Of XElement).CollectionMapperResolver(New CollectionOneToManyMapperResolverDefaultProvider(AddressOf ReaderMapperValue.Map)),
            New ObjectOneToManyMapper(Of XElement).ClassAndStructureMapperResolver(AddressOf ReaderMapperValue.Map)
        })
        WriterMapperValue = New ObjectManyToOneMapper(Of XElement)
        WriterMapperValue.Resolvers.AddRange(New List(Of IObjectManyToOneMapperResolver(Of XElement)) From {
            New PrimitiveMapperResolver(),
            New ObjectManyToOneMapper(Of XElement).EnumMapperResolver(AddressOf Me.Write),
            New ObjectManyToOneMapper(Of XElement).CollectionMapperResolver(New CollectionManyToOneMapperResolverDefaultProvider(AddressOf Me.Write)),
            New ObjectManyToOneMapper(Of XElement).ClassAndStructureMapperResolver(AddressOf Me.Write)
        })
    End Sub

    Public Sub PutReader(Of T)(ByVal Reader As Func(Of XElement, T))
        ReaderMapperValue.PutMapper(Of T)(Reader)
    End Sub
    Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, XElement))
        WriterMapperValue.PutMapper(Of T)(Writer)
    End Sub

    Public Function GetReader(Of T)() As Func(Of XElement, T)
        Return ReaderMapperValue.GetMapper(Of T)()
    End Function
    Public Function GetWriter(Of T)() As Action(Of T, XElement)
        Return WriterMapperValue.GetMapper(Of T)()
    End Function

    Public Function Read(Of T)(ByVal s As XElement) As T
        Return GetReader(Of T)()(s)
    End Function
    Public Sub Write(Of T)(ByVal Value As T, ByVal s As XElement)
        GetWriter(Of T)()(Value, s)
    End Sub

    Private Shared Function GetTypeFriendlyName(ByVal Type As Type) As String
        If Type.IsArray Then
            Dim n = Type.GetArrayRank
            Dim ElementTypeName = GetTypeFriendlyName(Type.GetElementType)
            If n = 1 Then
                Return "ArrayOf" & ElementTypeName
            End If
            Return "Array" & n & "Of" & ElementTypeName
        End If
        If Type.IsGenericType Then
            Dim Name = Regex.Match(Type.Name, "^(?<Name>.*?)`.*$", RegexOptions.ExplicitCapture).Result("${Name}")
            Return Name & "Of" & String.Join("And", (From t In Type.GetGenericArguments() Select GetTypeFriendlyName(t)).ToArray)
        End If
        Return Type.Name
    End Function

    Public Class PrimitiveMapperResolver
        Implements IObjectOneToManyMapperResolver(Of XElement)
        Implements IObjectManyToOneMapperResolver(Of XElement)

        Public Function TryResolveReader(ByVal RangeType As System.Type) As [Delegate] Implements IObjectOneToManyMapperResolver(Of XElement).TryResolve
            If Readers.ContainsKey(RangeType) Then Return Readers(RangeType)
            Return Nothing
        End Function

        Public Function TryResolveWriter(ByVal DomainType As System.Type) As [Delegate] Implements IObjectManyToOneMapperResolver(Of XElement).TryResolve
            If Writers.ContainsKey(DomainType) Then Return Writers(DomainType)
            Return Nothing
        End Function

        Private Sub PutReader(Of T)(ByVal Reader As Func(Of XElement, T))
            Readers.Add(GetType(T), Reader)
        End Sub
        Private Sub PutWriter(Of T)(ByVal Writer As Action(Of T, XElement))
            Writers.Add(GetType(T), Writer)
        End Sub

        Private Readers As New Dictionary(Of Type, [Delegate])
        Private Writers As New Dictionary(Of Type, [Delegate])

        Public Sub New()
            PutReader(Function(s As XElement) Byte.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) UInt16.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) UInt32.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) UInt64.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) SByte.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) Int16.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) Int32.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) Int64.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) Single.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) Double.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As XElement) s.Value)
            PutReader(Function(s As XElement) Decimal.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))

            PutWriter(Sub(b As Byte, s As XElement) s.Value = b.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(i As UInt16, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(i As UInt32, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(i As UInt64, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(i As SByte, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(i As Int16, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(i As Int32, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(i As Int64, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(f As Single, s As XElement) s.Value = f.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(f As Double, s As XElement) s.Value = f.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Sub(str As String, s As XElement) s.Value = str)
            PutWriter(Sub(d As Decimal, s As XElement) s.Value = d.ToString(Globalization.CultureInfo.InvariantCulture))
        End Sub
    End Class

    Public Class CollectionOneToManyMapperResolverDefaultProvider
        Implements ICollectionOneToManyMapperResolverDefaultProvider(Of XElement)

        Public Function DefaultArrayMapper(Of R)(ByVal Key As XElement) As R() Implements ICollectionOneToManyMapperResolverDefaultProvider(Of XElement).DefaultArrayMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(R)), Func(Of XElement, R))
            Return Key.Elements.Select(Function(e) Mapper(e)).ToArray
        End Function
        Public Function DefaultListMapper(Of R, RList As {New, ICollection(Of R)})(ByVal Key As XElement) As RList Implements ICollectionOneToManyMapperResolverDefaultProvider(Of XElement).DefaultListMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(R)), Func(Of XElement, R))
            Dim list = New RList()
            For Each v In Key.Elements.Select(Function(e) Mapper(e))
                list.Add(v)
            Next
            Return list
        End Function

        Private Map As Func(Of XElement, DummyType)
        Public Sub New(ByVal Map As Func(Of XElement, DummyType))
            Me.Map = Map
        End Sub
    End Class

    Public Class CollectionManyToOneMapperResolverDefaultProvider
        Implements ICollectionMapperResolverDefaultProvider(Of XElement)

        Public Sub DefaultArrayMapper(Of D)(ByVal arr As D(), ByVal Value As XElement) Implements ICollectionMapperResolverDefaultProvider(Of XElement).DefaultArrayMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(D)), Action(Of D, XElement))
            Dim NumElement = arr.Length
            For n = 0 To NumElement - 1
                Dim Element As New XElement
                Mapper(arr(n), Element)
                Value.Add(Element)
            Next
        End Sub
        Public Sub DefaultListMapper(Of D, DList As ICollection(Of D))(ByVal list As DList, ByVal Value As XElement) Implements ICollectionMapperResolverDefaultProvider(Of XElement).DefaultListMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(D)), Action(Of D, XElement))
            For Each v In list
                Dim Element As New XElement
                Mapper(v, Element)
                Value.Add(Element)
            Next
        End Sub

        Private Map As Action(Of DummyType, XElement)
        Public Sub New(ByVal Map As Action(Of DummyType, XElement))
            Me.Map = Map
        End Sub
    End Class
End Class
