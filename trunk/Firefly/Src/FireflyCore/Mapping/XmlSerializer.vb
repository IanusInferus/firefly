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
    ''' 对于类对象，允许出现null。
    ''' </remarks>
    Public Class XmlSerializer
        Private PrimitiveResolver As PrimitiveResolver
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
            MyClass.New(New Type() {})
        End Sub
        Public Sub New(ByVal ExternalTypes As IEnumerable(Of Type))
            'Reader
            'proj <- proj
            'PrimitiveResolver: (String proj Primitive) <- null
            'EnumResolver: (String proj Enum) <- null
            'ListProjectorUnpacker: (XElement proj {R}) <- (XElement.SubElement proj R)
            'FieldOrPropertyProjectorResolver: (Dictionary(String, XElement) proj R) <- (XElement.SubElement proj R.Field)
            'XElementToStringDomainTranslator: (XElement proj R) <- (String proj R)
            'InheritanceResolver: (XElement proj R) <- (XElement proj R.Derived)
            'XElementProjectorToProjectorDomainTranslator: (XElement proj R) <- (Dictionary(String, XElement) proj R)
            '
            'Writer
            'proj <- proj/aggr
            'PrimitiveResolver: (Primitive proj String) <- null
            'EnumResolver: (Enum proj String) <- null
            'XElementToStringRangeTranslator: (D proj XElement) <- (D proj String)
            'InheritanceResolver: (D proj XElement) <- (D.Derived proj XElement)
            'XElementAggregatorToProjectorRangeTranslator: (D proj XElement) <- (D aggr List(XElement))
            '
            'Writer
            'aggr <- proj/aggr
            'ListAggregatorPacker: ({D} aggr List(XElement)) <- (D aggr List(XElement))
            'FieldOrPropertyAggregatorResolver: (D aggr List(XElement)) <- (D.Field proj XElement)
            'XElementProjectorToAggregatorRangeTranslator: (D aggr List(XElement)) <- (D proj XElement)

            PrimitiveResolver = New PrimitiveResolver

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
            PutWriter(Function(s As String) s)
            PutWriter(Function(d As Decimal) d.ToString(Globalization.CultureInfo.InvariantCulture))

            ReaderResolverSet = New AlternativeResolver
            ReaderCache = New CachedResolver(ReaderResolverSet)
            Dim ReaderList = New List(Of IObjectProjectorResolver) From {
                PrimitiveResolver,
                New EnumResolver,
                New CollectionUnpackerTemplate(Of XElement)(New ListProjectorUnpacker(ReaderCache)),
                New RecordUnpackerTemplate(Of Dictionary(Of String, XElement))(New FieldOrPropertyProjectorResolver(ReaderCache)),
                TranslatorResolver.Create(ReaderCache, New XElementToStringDomainTranslator),
                New InheritanceResolver(ReaderCache, ExternalTypes),
                TranslatorResolver.Create(ReaderCache, New XElementProjectorToProjectorDomainTranslator)
            }
            For Each r In ReaderList
                ReaderResolverSet.ProjectorResolvers.AddLast(r)
            Next
            WriterResolverSet = New AlternativeResolver
            WriterCache = New CachedResolver(WriterResolverSet)
            Dim WriterProjectorList = New List(Of IObjectProjectorResolver) From {
                PrimitiveResolver,
                New EnumResolver,
                TranslatorResolver.Create(WriterCache, New XElementToStringRangeTranslator),
                New InheritanceResolver(WriterCache, ExternalTypes),
                TranslatorResolver.Create(WriterCache, New XElementAggregatorToProjectorRangeTranslator)
            }
            For Each r In WriterProjectorList
                WriterResolverSet.ProjectorResolvers.AddLast(r)
            Next
            Dim WriterAggregatorList = New List(Of IObjectAggregatorResolver) From {
                New CollectionPackerTemplate(Of List(Of XElement))(New ListAggregatorPacker(WriterCache)),
                New RecordPackerTemplate(Of List(Of XElement))(New FieldOrPropertyAggregatorResolver(WriterCache)),
                TranslatorResolver.Create(WriterCache, New XElementProjectorToAggregatorRangeTranslator)
            }
            For Each r In WriterAggregatorList
                WriterResolverSet.AggregatorResolvers.AddLast(r)
            Next
            ReaderMapper = New ObjectMapper(ReaderCache)
            WriterMapper = New ObjectMapper(WriterCache)
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of String, T))
            PrimitiveResolver.PutProjector(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, String))
            PrimitiveResolver.PutProjector(Writer)
        End Sub
        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ReaderResolverSet.ProjectorResolvers.AddFirst(TranslatorResolver.Create(ReaderCache, Translator))
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            WriterResolverSet.ProjectorResolvers.AddFirst(TranslatorResolver.Create(WriterCache, Translator))
        End Sub

        Public Function Read(Of T)(ByVal s As XElement) As T
            Dim m = ReaderMapper.GetProjector(Of XElement, T)()
            Return m(s)
        End Function
        Public Function Write(Of T)(ByVal Value As T) As XElement
            Dim m = WriterMapper.GetProjector(Of T, XElement)()
            Return m(Value)
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

        Private Class EnumResolver
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
                If DomainType Is GetType(String) AndAlso RangeType.IsEnum Then
                    Dim DummyMethod = DirectCast(AddressOf StringToEnum(Of DummyType), Func(Of String, DummyType))
                    Dim m = DummyMethod.MakeDelegateMethodFromDummy(RangeType)
                    Return m
                End If
                If RangeType Is GetType(String) AndAlso DomainType.IsEnum Then
                    Dim DummyMethod = DirectCast(AddressOf EnumToString(Of DummyType), Func(Of DummyType, String))
                    Dim m = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                    Return m
                End If
                Return Nothing
            End Function
        End Class

        Private Class ListProjectorUnpacker
            Implements IGenericListProjectorResolver(Of XElement)

            Public Function ResolveProjector(Of R, RList As {New, ICollection(Of R)})() As Func(Of XElement, RList) Implements IGenericListProjectorResolver(Of XElement).ResolveProjector
                Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
                Dim F =
                    Function(Key As XElement) As RList
                        If Not Key.IsEmpty Then
                            Dim List = New RList()
                            For Each k In Key.Elements
                                List.Add(Mapper(k))
                            Next
                            Return List
                        Else
                            Return Nothing
                        End If
                    End Function
                Return F
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
            End Sub
        End Class

        Private Class ListAggregatorPacker
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

        Private Class XElementToStringRangeTranslator
            Implements IProjectorToProjectorRangeTranslator(Of XElement, String)

            Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, XElement) Implements IProjectorToProjectorRangeTranslator(Of XElement, String).TranslateProjectorToProjectorRange
                Dim FriendlyName = GetTypeFriendlyName(GetType(D))
                Return Function(v)
                           Dim s = Projector(v)
                           Return New XElement(FriendlyName, s)
                       End Function
            End Function
        End Class

        Private Class XElementToStringDomainTranslator
            Implements IProjectorToProjectorDomainTranslator(Of XElement, String)

            Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of String, R)) As Func(Of XElement, R) Implements IProjectorToProjectorDomainTranslator(Of XElement, String).TranslateProjectorToProjectorDomain
                Return Function(v) Projector(v.Value)
            End Function
        End Class

        Private Class XElementProjectorToProjectorDomainTranslator
            Implements IProjectorToProjectorDomainTranslator(Of XElement, Dictionary(Of String, XElement))

            Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of Dictionary(Of String, XElement), R)) As Func(Of XElement, R) Implements IProjectorToProjectorDomainTranslator(Of XElement, Dictionary(Of String, XElement)).TranslateProjectorToProjectorDomain
                Return Function(Element) As R
                           If Not Element.IsEmpty Then
                               Return Projector(Element.Elements.ToDictionary(Function(e) e.Name.LocalName, StringComparer.OrdinalIgnoreCase))
                           Else
                               Return Nothing
                           End If
                       End Function
            End Function
        End Class

        Private Class XElementAggregatorToProjectorRangeTranslator
            Implements IAggregatorToProjectorRangeTranslator(Of XElement, List(Of XElement))

            Public Function TranslateAggregatorToProjectorRange(Of D)(ByVal Aggregator As Action(Of D, List(Of XElement))) As Func(Of D, XElement) Implements IAggregatorToProjectorRangeTranslator(Of XElement, List(Of XElement)).TranslateAggregatorToProjectorRange
                Dim FriendlyName = GetTypeFriendlyName(GetType(D))
                Return Function(v)
                           If v IsNot Nothing Then
                               Dim l As New List(Of XElement)
                               Aggregator(v, l)
                               If l.Count = 0 Then
                                   Return New XElement(FriendlyName, "")
                               Else
                                   Return New XElement(FriendlyName, l.ToArray())
                               End If
                           Else
                               Return New XElement(FriendlyName, Nothing)
                           End If
                       End Function
            End Function
        End Class

        Private Class XElementProjectorToAggregatorRangeTranslator
            Implements IProjectorToAggregatorRangeTranslator(Of List(Of XElement), XElement)

            Public Function TranslateProjectorToAggregatorRange(Of D)(ByVal Projector As Func(Of D, XElement)) As Action(Of D, List(Of XElement)) Implements IProjectorToAggregatorRangeTranslator(Of List(Of XElement), XElement).TranslateProjectorToAggregatorRange
                Dim FriendlyName = GetTypeFriendlyName(GetType(D))
                Return Sub(v, l) l.Add(Projector(v))
            End Function
        End Class

        Private Class FieldOrPropertyProjectorResolver
            Implements IFieldOrPropertyProjectorResolver(Of Dictionary(Of String, XElement))

            Private Function Resolve(Of R)(ByVal Name As String) As Func(Of Dictionary(Of String, XElement), R)
                Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
                Dim F =
                    Function(d As Dictionary(Of String, XElement)) As R
                        If d.ContainsKey(Name) Then
                            Return Mapper(d(Name))
                        Else
                            Return Nothing
                        End If
                    End Function
                Return F
            End Function

            Public Function ResolveProjector(ByVal Info As FieldOrPropertyInfo) As [Delegate] Implements IFieldOrPropertyProjectorResolver(Of Dictionary(Of String, XElement)).ResolveProjector
                Dim Name = Info.Member.Name
                Dim m = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Func(Of Dictionary(Of String, XElement), DummyType)))
                Return DirectCast(m.MakeDelegateMethodFromDummy(Info.Type).DynamicInvoke(Name), [Delegate])
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New CachedResolver(New NoncircularResolver(Resolver)))
            End Sub
        End Class
        Private Class FieldOrPropertyAggregatorResolver
            Implements IFieldOrPropertyAggregatorResolver(Of List(Of XElement))

            Private Function Resolve(Of D)(ByVal Name As String) As Action(Of D, List(Of XElement))
                Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
                Dim TypeName = GetTypeFriendlyName(GetType(D))
                Dim F =
                    Sub(k As D, l As List(Of XElement))
                        Dim e = Mapper(k)
                        If e.Name <> TypeName Then
                            Stop
                        End If
                        e.Name = Name
                        l.Add(e)
                    End Sub
                Return F
            End Function

            Public Function ResolveAggregator(ByVal Info As FieldOrPropertyInfo) As [Delegate] Implements IFieldOrPropertyAggregatorResolver(Of List(Of XElement)).ResolveAggregator
                Dim Name = Info.Member.Name
                Dim m = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Action(Of DummyType, List(Of XElement))))
                Return DirectCast(m.MakeDelegateMethodFromDummy(Info.Type).DynamicInvoke(Name), [Delegate])
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New CachedResolver(New NoncircularResolver(Resolver)))
            End Sub
        End Class

        Private Class InheritanceResolver
            Implements IObjectProjectorResolver

            Private Function ResolveRange(Of R)() As Func(Of XElement, R)
                Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
                Dim F =
                    Function(k As XElement) As R
                        Return Mapper(k)
                    End Function
                Return F
            End Function
            Private Function ResolveDomain(Of D)() As Func(Of D, XElement)
                Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
                Dim F =
                    Function(k As D) As XElement
                        If k Is Nothing Then Return Mapper(k)
                        Dim RealType = k.GetType()
                        If RealType Is GetType(D) Then Return Mapper(k)
                        Dim RealTypeName = GetTypeFriendlyName(RealType)
                        If Not ExternalTypeDict.ContainsKey(RealTypeName) Then Throw New InvalidOperationException("ExternalTypeNotFound: {0}".Formats(RealTypeName))
                        If ExternalTypeDict(RealTypeName) IsNot RealType Then Throw New InvalidOperationException("ExternalTypeMismatched: {0}".Formats(RealTypeName))

                        Dim TypePair = CreatePair(RealType, GetType(XElement))
                        ProjectorCache.Add(TypePair)
                        Try
                            Dim DynamicMapper = AbsResolver.ResolveProjector(TypePair)
                            Return DirectCast(DynamicMapper.DynamicInvoke(k), XElement)
                        Finally
                            ProjectorCache.Remove(TypePair)
                        End Try
                    End Function
                Return F
            End Function

            Private ProjectorCache As New HashSet(Of KeyValuePair(Of Type, Type))
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType Is GetType(XElement) AndAlso RangeType.IsClass Then
                    If ProjectorCache.Contains(TypePair) Then Return Nothing
                    ProjectorCache.Add(TypePair)
                    Try
                        Dim DummyMethod = DirectCast(AddressOf ResolveRange(Of DummyType), Func(Of Func(Of XElement, DummyType)))
                        Dim m = DummyMethod.MakeDelegateMethodFromDummy(RangeType)
                        Return DirectCast(m.DynamicInvoke(), [Delegate])
                    Finally
                        ProjectorCache.Remove(TypePair)
                    End Try
                End If
                If RangeType Is GetType(XElement) AndAlso DomainType.IsClass Then
                    If ProjectorCache.Contains(TypePair) Then Return Nothing
                    ProjectorCache.Add(TypePair)
                    Try
                        Dim DummyMethod = DirectCast(AddressOf ResolveDomain(Of DummyType), Func(Of Func(Of DummyType, XElement)))
                        Dim m = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                        Return DirectCast(m.DynamicInvoke(), [Delegate])
                    Finally
                        ProjectorCache.Remove(TypePair)
                    End Try
                End If
                Return Nothing
            End Function

            Private AbsResolver As AbsoluteResolver
            Private ExternalTypeDict As Dictionary(Of String, Type)
            Public Sub New(ByVal Resolver As IObjectMapperResolver, ByVal ExternalTypes As IEnumerable(Of Type))
                Me.AbsResolver = New AbsoluteResolver(New CachedResolver(New NoncircularResolver(Resolver)))
                Me.ExternalTypeDict = ExternalTypes.ToDictionary(Function(type) GetTypeFriendlyName(type), StringComparer.OrdinalIgnoreCase)
            End Sub
        End Class
    End Class
End Namespace
