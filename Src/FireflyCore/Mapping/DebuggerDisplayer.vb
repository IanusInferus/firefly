'==========================================================================
'
'  File:        DebuggerDisplayer.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 调试序列化器
'  Version:     2011.07.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports Firefly
Imports Firefly.Mapping
Imports Firefly.Mapping.MetaProgramming

Namespace Mapping.MetaSchema
    Public NotInheritable Class DebuggerDisplayer
        Private Sub New()
        End Sub

        Private Shared d As New Displayer
        Public Shared Function ConvertToString(Of T)(ByVal v As T) As String
            Dim Type = GetType(T)
            Dim m = DirectCast(d.ResolveProjector(CreatePair(Type, GetType(String))), Func(Of T, String))
            Return m(v)
        End Function

        Private Class Displayer
            Implements IMapperResolver

            Private Resolver As IMapperResolver

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
                Return Resolver.TryResolveProjector(TypePair)
            End Function
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                Return Resolver.TryResolveAggregator(TypePair)
            End Function

            Public Sub New()
                Dim Root As New ReferenceMapperResolver
                Resolver = Root
                Dim ProjectorResolverList = New List(Of IProjectorResolver)({
                    New PrimitiveStringResolver,
                    New NullableStringResolver(Root.AsRuntimeDomainNoncircular),
                    TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, New StringAggregatorToProjectorRangeTranslator)
                })
                Dim AggregatorResolverList = New List(Of IAggregatorResolver)({
                    New CollectionPackerTemplate(Of PackerState)(New CollectionPacker(Root.AsRuntimeDomainNoncircular)),
                    New RecordPackerTemplate(Of PackerState)(
                        New FieldAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                        New AliasFieldAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                        New TagAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                        New TaggedUnionAlternativeAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                        New TupleElementAggregatorResolver(Root.AsRuntimeDomainNoncircular)
                    ),
                    TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, New StringProjectorToAggregatorRangeTranslator)
                })
                Root.Inner = CreateMapper(ProjectorResolverList.Concatenated, AggregatorResolverList.Concatenated)
            End Sub
        End Class

        Public Class CollectionPacker
            Implements IGenericCollectionAggregatorResolver(Of PackerState)

            Public Function ResolveAggregator(Of D, DCollection As ICollection(Of D))() As Action(Of DCollection, PackerState) Implements IGenericCollectionAggregatorResolver(Of PackerState).ResolveAggregator
                Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(String))), Func(Of D, String))
                Dim F =
                    Sub(c As DCollection, Value As PackerState)
                        For Each v In c
                            Value.List.Add(Mapper(v))
                        Next
                        Value.NoName = True
                    End Sub
                Return F
            End Function

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal Resolver As IProjectorResolver)
                Me.InnerResolver = Resolver
            End Sub
        End Class

        Public Class PackerState
            Public List As List(Of String)
            Public NoBraces As Boolean
            Public NoName As Boolean
        End Class

        Public Class StringAggregatorToProjectorRangeTranslator
            Implements IAggregatorToProjectorRangeTranslator(Of String, PackerState)

            Private CallStackDepth As Integer = 0
            Public Function TranslateAggregatorToProjectorRange(Of D)(ByVal Aggregator As Action(Of D, PackerState)) As Func(Of D, String) Implements IAggregatorToProjectorRangeTranslator(Of String, PackerState).TranslateAggregatorToProjectorRange
                Dim Name = GetType(D).Name
                Return Function(v)
                           CallStackDepth += 1
                           Try
                               If v Is Nothing Then Return "$Empty"
                               If CallStackDepth >= 5 Then Return ".."
                               Dim s As New PackerState With {.List = New List(Of String)(), .NoBraces = False, .NoName = False}
                               Aggregator(v, s)
                               If s.NoBraces Then
                                   If s.NoName Then
                                       Return s.List.Single
                                   Else
                                       Throw New InvalidOperationException
                                   End If
                               Else
                                   If s.NoName Then
                                       Return "{" & String.Join(", ", s.List.ToArray()) & "}"
                                   Else
                                       Return Name & "{" & String.Join(", ", s.List.ToArray()) & "}"
                                   End If
                               End If
                           Finally
                               CallStackDepth -= 1
                           End Try
                       End Function
            End Function
        End Class

        Public Class StringProjectorToAggregatorRangeTranslator
            Implements IProjectorToAggregatorRangeTranslator(Of PackerState, String)

            Public Function TranslateProjectorToAggregatorRange(Of D)(ByVal Projector As Func(Of D, String)) As Action(Of D, PackerState) Implements IProjectorToAggregatorRangeTranslator(Of PackerState, String).TranslateProjectorToAggregatorRange
                Return Sub(v, s) s.List.Add(Projector(v))
            End Function
        End Class

        ''' <remarks>基元解析器</remarks>
        Public Class PrimitiveStringResolver
            Implements IMapperResolver

            Private Shared Function ConvertStringToString(ByVal v As String) As String
                If v Is Nothing Then Return "$Empty"
                Return """" & v & """"
            End Function

            Private Shared Function ConvertToString(Of D)(ByVal v As D) As String
                If v Is Nothing Then
                    Return "$Empty"
                Else
                    Return v.ToString()
                End If
            End Function

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
                If TypePair.Value Is GetType(String) Then
                    If TypePair.Key Is GetType(String) Then Return DirectCast(AddressOf ConvertStringToString, Func(Of String, String))
                    If TypePair.Key.IsPrimitive Then
                        Return DirectCast(AddressOf ConvertToString(Of DummyType), Func(Of DummyType, String)).MakeDelegateMethodFromDummy(TypePair.Key)
                    End If
                End If
                Return Nothing
            End Function
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                Return Nothing
            End Function
        End Class

        ''' <remarks>可空解析器</remarks>
        Public Class NullableStringResolver
            Implements IMapperResolver

            Private Class Converter
                Public CallDelegate As [Delegate]

                Public Function ConvertToString(Of D As Structure)(ByVal v As Nullable(Of D)) As String
                    If v Is Nothing Then
                        Return "$Empty"
                    Else
                        Return DirectCast(CallDelegate, Func(Of D, String))(CType(v, D))
                    End If
                End Function
            End Class
            Private Shared Function GetConvertToStringFunc(Of D As Structure)(ByVal m As [Delegate]) As [Delegate]
                Dim c As New Converter With {.CallDelegate = m}
                Return DirectCast(AddressOf c.ConvertToString(Of D), Func(Of Nullable(Of D), String))
            End Function
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
                If TypePair.Value Is GetType(String) Then
                    Dim Domain = TypePair.Key
                    If Domain.IsGenericType AndAlso Not Domain.IsGenericTypeDefinition Then
                        If Domain.GetGenericTypeDefinition Is GetType(Nullable(Of )) Then
                            Dim UnderlyingDomain = Domain.GetGenericArguments(0)
                            Dim m = InnerResolver.TryResolveProjector(CreatePair(UnderlyingDomain, TypePair.Value))
                            If m Is Nothing Then Return Nothing
                            Dim md = DirectCast(AddressOf GetConvertToStringFunc(Of Integer), Func(Of [Delegate], [Delegate]))
                            Dim d = DirectCast(md.MakeDelegateMethod({UnderlyingDomain}, GetType(Func(Of [Delegate], [Delegate]))), Func(Of [Delegate], [Delegate]))
                            Return d(m)
                        End If
                    End If
                End If
                Return Nothing
            End Function
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                Return Nothing
            End Function

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal Resolver As IProjectorResolver)
                Me.InnerResolver = Resolver
            End Sub
        End Class

        Public Class FieldAggregatorResolver
            Implements IFieldAggregatorResolver(Of PackerState)

            Private Function Resolve(Of D)(ByVal Name As String) As Action(Of D, PackerState)
                Dim F =
                    Sub(k As D, s As PackerState)
                        Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(String))), Func(Of D, String))
                        If k Is Nothing Then
                            Dim e = "$Empty"
                            s.List.Add(Name & " = " & e)
                        Else
                            Dim e = Mapper(k)
                            s.List.Add(Name & " = " & e)
                        End If
                    End Sub
                Return F
            End Function

            Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
            Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IFieldAggregatorResolver(Of PackerState).ResolveAggregator
                Dim Name = Member.Name
                If Dict.ContainsKey(Type) Then
                    Dim m = Dict(Type)
                    Return m(Name)
                Else
                    Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Action(Of DummyType, PackerState)))
                    Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of String, [Delegate])()
                    Dict.Add(Type, m)
                    Return m(Name)
                End If
            End Function

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal Resolver As IProjectorResolver)
                Me.InnerResolver = Resolver
            End Sub
        End Class

        Public Class AliasFieldAggregatorResolver
            Implements IAliasFieldAggregatorResolver(Of PackerState)

            Private Function Resolve(Of D)() As Action(Of D, PackerState)
                Dim F =
                    Sub(k As D, s As PackerState)
                        Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(String))), Func(Of D, String))
                        If k Is Nothing Then
                            s.List.Add("$Empty")
                        Else
                            s.List.Add(Mapper(k))
                        End If
                        s.NoBraces = True
                        s.NoName = True
                    End Sub
                Return F
            End Function

            Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IAliasFieldAggregatorResolver(Of PackerState).ResolveAggregator
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Action(Of DummyType, PackerState)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of [Delegate])()
                Return m()
            End Function

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal Resolver As IProjectorResolver)
                Me.InnerResolver = Resolver
            End Sub
        End Class

        Public Class TagAggregatorResolver
            Implements ITagAggregatorResolver(Of PackerState)

            Private Function Resolve(Of D)() As Action(Of D, PackerState)
                Dim F =
                    Sub(k As D, s As PackerState)
                    End Sub
                Return F
            End Function

            Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal TagType As Type) As [Delegate] Implements ITagAggregatorResolver(Of PackerState).ResolveAggregator
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Action(Of DummyType, PackerState)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(TagType).AdaptFunction(Of [Delegate])()
                Return m()
            End Function

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal Resolver As IProjectorResolver)
                Me.InnerResolver = Resolver
            End Sub
        End Class

        Public Class TaggedUnionAlternativeAggregatorResolver
            Implements ITaggedUnionAlternativeAggregatorResolver(Of PackerState)

            Private Function Resolve(Of D)(ByVal Name As String) As Action(Of D, PackerState)
                Dim F =
                    Sub(k As D, s As PackerState)
                        Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(String))), Func(Of D, String))
                        If k Is Nothing Then
                            Dim e = "$Empty"
                            s.List.Add(Name & "{" & e & "}")
                        Else
                            Dim e = Mapper(k)
                            s.List.Add(Name & "{" & e & "}")
                        End If
                        s.NoBraces = True
                        s.NoName = True
                    End Sub
                Return F
            End Function

            Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
            Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements ITaggedUnionAlternativeAggregatorResolver(Of PackerState).ResolveAggregator
                Dim Name = Member.Name
                If Dict.ContainsKey(Type) Then
                    Dim m = Dict(Type)
                    Return m(Name)
                Else
                    Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Action(Of DummyType, PackerState)))
                    Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of String, [Delegate])()
                    Dict.Add(Type, m)
                    Return m(Name)
                End If
            End Function

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal Resolver As IProjectorResolver)
                Me.InnerResolver = Resolver
            End Sub
        End Class

        Public Class TupleElementAggregatorResolver
            Implements ITupleElementAggregatorResolver(Of PackerState)

            Private Function Resolve(Of D)(ByVal Index As Integer) As Action(Of D, PackerState)
                Dim F =
                    Sub(k As D, s As PackerState)
                        Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(String))), Func(Of D, String))
                        If k Is Nothing Then
                            s.List.Add("$Empty")
                        Else
                            s.List.Add(Mapper(k))
                        End If
                        s.NoName = True
                    End Sub
                Return F
            End Function

            Private Dict As New Dictionary(Of Type, Func(Of Integer, [Delegate]))
            Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Index As Integer, ByVal Type As Type) As [Delegate] Implements ITupleElementAggregatorResolver(Of PackerState).ResolveAggregator
                If Dict.ContainsKey(Type) Then
                    Dim m = Dict(Type)
                    Return m(Index)
                Else
                    Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Integer, Action(Of DummyType, PackerState)))
                    Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of Integer, [Delegate])()
                    Dict.Add(Type, m)
                    Return m(Index)
                End If
            End Function

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal Resolver As IProjectorResolver)
                Me.InnerResolver = Resolver
            End Sub
        End Class
    End Class
End Namespace
