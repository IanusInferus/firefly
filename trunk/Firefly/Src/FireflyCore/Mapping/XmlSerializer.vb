'==========================================================================
'
'  File:        XmlSerializer.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Xml序列化类
'  Version:     2010.11.15.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

Namespace Mapping
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
        Private PrimRes As PrimitiveResolver
        Private ReaderMapper As ObjectMapper
        Private WriterMapper As ObjectMapper
        Private ReaderCache As CachedResolver
        Private WriterCache As CachedResolver
        Private ReaderResolverSet As AlternativeResolver
        Private WriterResolverSet As AlternativeResolver

        Public ReadOnly Property ReaderResolver As AlternativeResolver
            Get
                Return ReaderResolverSet
            End Get
        End Property
        Public ReadOnly Property WriterResolver As AlternativeResolver
            Get
                Return WriterResolverSet
            End Get
        End Property

        Public Sub New()
            PrimRes = New PrimitiveResolver
            ReaderResolverSet = New AlternativeResolver
            ReaderCache = New CachedResolver(ReaderResolverSet)
            Dim ReaderList = New List(Of IObjectProjectorResolver) From {
                New CollectionUnpacker(Of XElement)(New GenericListProjectorResolver(ReaderCache)),
                TranslatorResolver.Create(ReaderCache, New XElementDomainTranslator),
                PrimRes,
                New EnumResolver(ReaderCache),
                New RecordUnpacker(Of XElement)(ReaderCache)
            }
            For Each r In ReaderList
                ReaderResolverSet.ProjectorResolvers.AddLast(r)
            Next
            WriterResolverSet = New AlternativeResolver
            WriterCache = New CachedResolver(WriterResolverSet)
            Dim WriterProjectorList = New List(Of IObjectProjectorResolver) From {
                TranslatorResolver.Create(WriterCache, New XElementRangeTranslator),
                TranslatorResolver.Create(WriterCache, New CollectionToXElementListTranslator),
                PrimRes,
                New EnumResolver(WriterCache)
            }
            For Each r In WriterProjectorList
                WriterResolverSet.ProjectorResolvers.AddLast(r)
            Next
            Dim WriterAggregatorList = New List(Of IObjectAggregatorResolver) From {
                New CollectionPacker(Of List(Of XElement))(New GenericListAggregatorResolver(WriterCache)),
                TranslatorResolver.Create(WriterCache, New XElementListToCollectionTranslator),
                New RecordPacker(Of XElement)(WriterCache)
            }
            For Each r In WriterAggregatorList
                WriterResolverSet.AggregatorResolvers.AddLast(r)
            Next
            ReaderMapper = New ObjectMapper(ReaderCache)
            WriterMapper = New ObjectMapper(WriterCache)
        End Sub

        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ReaderResolverSet.ProjectorResolvers.AddFirst(TranslatorResolver.Create(ReaderCache, Translator))
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            WriterResolverSet.ProjectorResolvers.AddFirst(TranslatorResolver.Create(WriterCache, Translator))
        End Sub

        Public Function Read(Of T)(ByVal s As XElement) As T
            Return ReaderMapper.GetProjector(Of XElement, T)()(s)
        End Function
        Public Function Write(Of T)(ByVal Value As T) As XElement
            Return WriterMapper.GetProjector(Of T, XElement)()(Value)
        End Function

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

        Public Class PrimitiveResolver
            Implements IObjectProjectorResolver

            Private Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType Is GetType(String) Then Return TryResolveReader(RangeType)
                If RangeType Is GetType(String) Then Return TryResolveWriter(DomainType)
                Return Nothing
            End Function

            Public Function TryResolveReader(ByVal RangeType As Type) As [Delegate]
                If Readers.ContainsKey(RangeType) Then Return Readers(RangeType)
                Return Nothing
            End Function

            Public Function TryResolveWriter(ByVal DomainType As Type) As [Delegate]
                If Writers.ContainsKey(DomainType) Then Return Writers(DomainType)
                Return Nothing
            End Function

            Public Sub PutReader(Of T)(ByVal Reader As Func(Of String, T))
                Readers.Add(GetType(T), Reader)
            End Sub
            Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, String))
                Writers.Add(GetType(T), Writer)
            End Sub

            Private Readers As New Dictionary(Of Type, [Delegate])
            Private Writers As New Dictionary(Of Type, [Delegate])

            Public Sub New()
                PutReader(Function(s As String) Byte.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) UInt16.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) UInt32.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) UInt64.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) SByte.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) Int16.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) Int32.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) Int64.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) Single.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) Double.Parse(s, Globalization.CultureInfo.InvariantCulture))
                PutReader(Function(s As String) s)
                PutReader(Function(s As String) Decimal.Parse(s, Globalization.CultureInfo.InvariantCulture))

                PutWriter(Function(b As Byte) b.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(i As UInt16) i.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(i As UInt32) i.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(i As UInt64) i.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(i As SByte) i.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(i As Int16) i.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(i As Int32) i.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(i As Int64) i.ToString(Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(f As Single) f.ToString("r", Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(f As Double) f.ToString("r", Globalization.CultureInfo.InvariantCulture))
                PutWriter(Function(str As String) str)
                PutWriter(Function(d As Decimal) d.ToString(Globalization.CultureInfo.InvariantCulture))
            End Sub
        End Class

        Public Class EnumResolver
            Implements IObjectProjectorResolver

            Public Shared Function StringToEnum(Of R)(ByVal s As String) As R
                Return DirectCast([Enum].Parse(GetType(R), s), R)
            End Function

            Public Shared Function EnumToString(Of D)(ByVal v As D) As String
                Return v.ToString()
            End Function

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType.IsEnum Then
                    Dim DummyMethod = DirectCast(AddressOf StringToEnum(Of DummyType), Func(Of String, DummyType))
                    Dim m = DummyMethod.MakeDelegateMethodFromDummy(RangeType)
                    Dim Mapper = AbsResolver.ResolveProjector(CreatePair(DomainType, GetType(String)))
                    Return Mapper.Compose(m)
                End If
                If DomainType.IsEnum Then
                    Dim DummyMethod = DirectCast(AddressOf EnumToString(Of DummyType), Func(Of DummyType, String))
                    Dim m = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                    Dim Mapper = AbsResolver.ResolveProjector(CreatePair(GetType(String), RangeType))
                    Return m.Compose(Mapper)
                End If
                Return Nothing
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
            End Sub
        End Class

        Public Class GenericListProjectorResolver
            Implements IGenericListProjectorResolver(Of XElement)

            Public Function ResolveProjector(Of R, RList As {New, ICollection(Of R)})() As Func(Of XElement, RList) Implements IGenericListProjectorResolver(Of XElement).ResolveProjector
                Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
                Dim F =
                    Function(Key As XElement) As RList
                        Dim List = New RList()
                        For Each k In Key.Elements
                            List.Add(Mapper(k))
                        Next
                        Return List
                    End Function
                Return F
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
            End Sub
        End Class

        Public Class GenericListAggregatorResolver
            Implements IGenericListAggregatorResolver(Of List(Of XElement))

            Public Function ResolveAggregator(Of D, DList As ICollection(Of D))() As Action(Of DList, List(Of XElement)) Implements IGenericListAggregatorResolver(Of List(Of XElement)).ResolveAggregator
                Dim Mapper = DirectCast(AbsResolver.ResolveAggregator(CreatePair(GetType(D), GetType(List(Of XElement)))), Action(Of D, List(Of XElement)))
                Dim F =
                    Sub(List As DList, Value As List(Of XElement))
                        For Each v In List
                            Mapper(v, Value)
                        Next
                    End Sub
                Return F
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
            End Sub
        End Class

        Public Class XElementRangeTranslator
            Implements IProjectorToProjectorRangeTranslator(Of XElement, String)

            Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, XElement) Implements IProjectorToProjectorRangeTranslator(Of XElement, String).TranslateProjectorToProjectorRange
                Dim FriendlyName = GetTypeFriendlyName(GetType(D))
                Return Function(v)
                           Dim s = Projector(v)
                           Return New XElement(FriendlyName, s)
                       End Function
            End Function
        End Class

        Public Class XElementDomainTranslator
            Implements IProjectorToProjectorDomainTranslator(Of XElement, String)

            Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of String, R)) As Func(Of XElement, R) Implements IProjectorToProjectorDomainTranslator(Of XElement, String).TranslateProjectorToProjectorDomain
                Return Function(v) Projector(v.Value)
            End Function
        End Class

        Public Class CollectionToXElementListTranslator
            Implements IAggregatorToProjectorRangeTranslator(Of XElement, List(Of XElement))

            Public Function TranslateAggregatorToProjectorRange(Of D)(ByVal Aggregator As Action(Of D, List(Of XElement))) As Func(Of D, XElement) Implements IAggregatorToProjectorRangeTranslator(Of XElement, List(Of XElement)).TranslateAggregatorToProjectorRange
                Dim FriendlyName = GetTypeFriendlyName(GetType(D))
                Return Function(v)
                           Dim l As New List(Of XElement)
                           Aggregator(v, l)
                           Return New XElement(FriendlyName, l.ToArray())
                       End Function
            End Function
        End Class

        Public Class XElementListToCollectionTranslator
            Implements IProjectorToAggregatorRangeTranslator(Of List(Of XElement), XElement)

            Public Function TranslateProjectorToAggregatorRange(Of D)(ByVal Projector As Func(Of D, XElement)) As Action(Of D, List(Of XElement)) Implements IProjectorToAggregatorRangeTranslator(Of List(Of XElement), XElement).TranslateProjectorToAggregatorRange
                Dim FriendlyName = GetTypeFriendlyName(GetType(D))
                Return Sub(v, l) l.Add(Projector(v))
            End Function
        End Class
    End Class
End Namespace
