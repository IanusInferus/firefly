'==========================================================================
'
'  File:        ObjectMapper.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Object映射器
'  Version:     2011.02.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Runtime.CompilerServices
Imports Firefly

Namespace Mapping
    Public Interface IObjectProjectorResolver
        ''' <param name="TypePair">(DomainType, RangeType)</param>
        ''' <returns>返回Func(Of ${DomainType}, ${RangeType})</returns>
        Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
    End Interface

    Public Interface IObjectAggregatorResolver
        ''' <returns>返回Action(Of ${DomainType}, ${RangeType})</returns>
        Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
    End Interface

    Public Interface IObjectMapperResolver
        Inherits IObjectProjectorResolver
        Inherits IObjectAggregatorResolver
    End Interface

    <DebuggerNonUserCode()>
    Public Module Mapping
        <Extension()> Public Function ResolveProjector(ByVal This As IObjectProjectorResolver, ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            Dim Resolved = This.TryResolveProjector(TypePair)
            If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: Projector({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            Return Resolved
        End Function
        <Extension()> Public Function ResolveAggregator(ByVal This As IObjectAggregatorResolver, ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            Dim Resolved = This.TryResolveAggregator(TypePair)
            If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: Aggregator({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            Return Resolved
        End Function

        <Extension()> Public Function ResolveProjector(Of D, R)(ByVal This As IObjectProjectorResolver) As Func(Of D, R)
            Return DirectCast(This.ResolveProjector(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Func(Of D, R))
        End Function
        <Extension()> Public Function ResolveAggregator(Of D, R)(ByVal This As IObjectAggregatorResolver) As Action(Of D, R)
            Return DirectCast(This.ResolveAggregator(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Action(Of D, R))
        End Function

        <Extension()> Public Function Project(Of D, R)(ByVal This As IObjectProjectorResolver, ByVal Key As D) As R
            Return This.ResolveProjector(Of D, R)()(Key)
        End Function
        <Extension()> Public Sub Aggregate(Of D, R)(ByVal This As IObjectAggregatorResolver, ByVal Key As D, ByVal Value As R)
            This.ResolveAggregator(Of D, R)()(Key, Value)
        End Sub

        Public Function CreateMapper(ByVal ProjectorResolver As IObjectProjectorResolver, ByVal AggregatorResolver As IObjectAggregatorResolver) As IObjectMapperResolver
            Return New ObjectMapper(ProjectorResolver, AggregatorResolver)
        End Function

        ''' <remarks>获取不循环解析器，用于在出现循环引用时抛出异常。</remarks>
        <Extension()> Public Function AsNoncircular(ByVal This As IObjectProjectorResolver) As IObjectProjectorResolver
            Return New NoncircularProjectResolver(This)
        End Function
        ''' <remarks>获取不循环解析器，用于在出现循环引用时抛出异常。</remarks>
        <Extension()> Public Function AsNoncircular(ByVal This As IObjectAggregatorResolver) As IObjectAggregatorResolver
            Return New NoncircularAggregatorResolver(This)
        End Function
        ''' <remarks>获取不循环解析器，用于在出现循环引用时抛出异常。</remarks>
        <Extension()> Public Function AsNoncircular(ByVal This As IObjectMapperResolver) As IObjectMapperResolver
            Return New ObjectMapper(New NoncircularProjectResolver(This), New NoncircularAggregatorResolver(This))
        End Function

        ''' <remarks>获取缓存解析器。</remarks>
        <Extension()> Public Function AsCached(ByVal This As IObjectProjectorResolver) As IObjectProjectorResolver
            Return New CachedProjectResolver(This)
        End Function
        ''' <remarks>获取缓存解析器。</remarks>
        <Extension()> Public Function AsCached(ByVal This As IObjectAggregatorResolver) As IObjectAggregatorResolver
            Return New CachedAggregatorResolver(This)
        End Function
        ''' <remarks>获取缓存解析器。</remarks>
        <Extension()> Public Function AsCached(ByVal This As IObjectMapperResolver) As IObjectMapperResolver
            Return New ObjectMapper(New CachedProjectResolver(This), New CachedAggregatorResolver(This))
        End Function

        ''' <summary>Object映射器</summary>
        <DebuggerNonUserCode()>
        Public Class ObjectMapper
            Implements IObjectMapperResolver

            Private ProjectorResolver As IObjectProjectorResolver
            Private AggregatorResolver As IObjectAggregatorResolver
            Public Sub New(ByVal ProjectorResolver As IObjectProjectorResolver, ByVal AggregatorResolver As IObjectAggregatorResolver)
                Me.ProjectorResolver = ProjectorResolver
                Me.AggregatorResolver = AggregatorResolver
            End Sub

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Return ProjectorResolver.TryResolveProjector(TypePair)
            End Function
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                Return AggregatorResolver.TryResolveAggregator(TypePair)
            End Function
        End Class

        <DebuggerNonUserCode()>
        Private Class NoncircularProjectResolver
            Implements IObjectProjectorResolver

            Private InnerResolver As IObjectProjectorResolver
            Public Sub New(ByVal InnerResolver As IObjectProjectorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private ResolvingProjectorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                If ResolvingProjectorTypePairs.Contains(TypePair) Then Throw New InvalidOperationException("CircularReference: Projector({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
                ResolvingProjectorTypePairs.Add(TypePair)
                Try
                    Return InnerResolver.TryResolveProjector(TypePair)
                Finally
                    ResolvingProjectorTypePairs.Remove(TypePair)
                End Try
            End Function
        End Class

        <DebuggerNonUserCode()>
        Private Class NoncircularAggregatorResolver
            Implements IObjectAggregatorResolver

            Private InnerResolver As IObjectAggregatorResolver
            Public Sub New(ByVal InnerResolver As IObjectAggregatorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private ResolvingAggregatorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                If ResolvingAggregatorTypePairs.Contains(TypePair) Then Throw New InvalidOperationException("CircularReference: Aggregator({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
                ResolvingAggregatorTypePairs.Add(TypePair)
                Try
                    Return InnerResolver.TryResolveAggregator(TypePair)
                Finally
                    ResolvingAggregatorTypePairs.Remove(TypePair)
                End Try
            End Function
        End Class

        ''' <remarks>缓存解析器</remarks>
        <DebuggerNonUserCode()>
        Private Class CachedProjectResolver
            Implements IObjectProjectorResolver

            Private InnerResolver As IObjectProjectorResolver
            Public Sub New(ByVal InnerResolver As IObjectProjectorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private ProjectorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                If ProjectorCache.ContainsKey(TypePair) Then Return ProjectorCache(TypePair)
                Dim Resolved = InnerResolver.TryResolveProjector(TypePair)
                If Resolved IsNot Nothing Then
                    '如果一个解析依赖于相同类型对的子解析，可能导致子解析已被加入缓存
                    If ProjectorCache.ContainsKey(TypePair) Then
                        ProjectorCache(TypePair) = Resolved
                    Else
                        ProjectorCache.Add(TypePair, Resolved)
                    End If
                    Return Resolved
                End If
                Return Nothing
            End Function
        End Class

        ''' <remarks>缓存解析器</remarks>
        <DebuggerNonUserCode()>
        Private Class CachedAggregatorResolver
            Implements IObjectAggregatorResolver

            Private InnerResolver As IObjectAggregatorResolver
            Public Sub New(ByVal InnerResolver As IObjectAggregatorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private AggregatorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])

            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                If AggregatorCache.ContainsKey(TypePair) Then Return AggregatorCache(TypePair)
                Dim Resolved = InnerResolver.TryResolveAggregator(TypePair)
                If Resolved IsNot Nothing Then
                    '如果一个解析依赖于相同类型对的子解析，可能导致子解析已被加入缓存
                    If AggregatorCache.ContainsKey(TypePair) Then
                        AggregatorCache(TypePair) = Resolved
                    Else
                        AggregatorCache.Add(TypePair, Resolved)
                    End If
                    Return Resolved
                End If
                Return Nothing
            End Function
        End Class
    End Module

    ''' <remarks>基元解析器</remarks>
    <DebuggerNonUserCode()>
    Public Class PrimitiveResolver
        Implements IObjectMapperResolver

        Private ProjectorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])
        Private AggregatorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
            If ProjectorCache.ContainsKey(TypePair) Then Return ProjectorCache(TypePair)
            Return Nothing
        End Function
        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
            If AggregatorCache.ContainsKey(TypePair) Then Return AggregatorCache(TypePair)
            Return Nothing
        End Function

        Public Sub PutProjector(Of D, R)(ByVal Projector As Func(Of D, R))
            Dim TypePair = CreatePair(GetType(D), GetType(R))
            If ProjectorCache.ContainsKey(TypePair) Then
                ProjectorCache(TypePair) = Projector
            Else
                ProjectorCache.Add(TypePair, Projector)
            End If
        End Sub
        Public Sub PutAggregator(Of D, R)(ByVal Aggregator As Action(Of D, R))
            Dim TypePair = CreatePair(GetType(D), GetType(R))
            If AggregatorCache.ContainsKey(TypePair) Then
                AggregatorCache(TypePair) = Aggregator
            Else
                AggregatorCache.Add(TypePair, Aggregator)
            End If
        End Sub
    End Class


    ''' <remarks>选择解析器</remarks>
    <DebuggerNonUserCode()>
    Public Class AlternativeResolver
        Implements IObjectMapperResolver

        Public Sub New()
        End Sub

        Private ProjectorResolversValue As New LinkedList(Of IObjectProjectorResolver)
        Private AggregatorResolversValue As New LinkedList(Of IObjectAggregatorResolver)
        Public ReadOnly Property ProjectorResolvers As LinkedList(Of IObjectProjectorResolver)
            Get
                Return ProjectorResolversValue
            End Get
        End Property
        Public ReadOnly Property AggregatorResolvers As LinkedList(Of IObjectAggregatorResolver)
            Get
                Return AggregatorResolversValue
            End Get
        End Property

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
            For Each r In ProjectorResolversValue
                Dim Resolved = r.TryResolveProjector(TypePair)
                If Resolved IsNot Nothing Then
                    Return Resolved
                End If
            Next
            Return Nothing
        End Function
        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
            For Each r In AggregatorResolversValue
                Dim Resolved = r.TryResolveAggregator(TypePair)
                If Resolved IsNot Nothing Then
                    Return Resolved
                End If
            Next
            Return Nothing
        End Function
    End Class
End Namespace
