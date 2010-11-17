'==========================================================================
'
'  File:        ObjectMapper.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Object映射器
'  Version:     2010.11.17.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
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

    ''' <remarks>解析器适配器，用于在无法解析时抛出异常。</remarks>
    <DebuggerNonUserCode()>
    Public Class AbsoluteResolver
        Private InnerResolver As IObjectMapperResolver
        Public Sub New(ByVal InnerResolver As IObjectMapperResolver)
            Me.InnerResolver = InnerResolver
        End Sub

        Function ResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            Dim Resolved = InnerResolver.TryResolveProjector(TypePair)
            If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: Projector({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            Return Resolved
        End Function
        Function ResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            Dim Resolved = InnerResolver.TryResolveAggregator(TypePair)
            If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: Aggregator({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            Return Resolved
        End Function
    End Class

    ''' <remarks>不循环解析器，用于在出现循环引用时抛出异常。</remarks>
    <DebuggerNonUserCode()>
    Public Class NoncircularResolver
        Implements IObjectMapperResolver

        Private InnerResolver As IObjectMapperResolver
        Public Sub New(ByVal InnerResolver As IObjectMapperResolver)
            Me.InnerResolver = InnerResolver
        End Sub

        Private ResolvingProjectorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
        Private ResolvingAggregatorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
            If ResolvingProjectorTypePairs.Contains(TypePair) Then Throw New InvalidOperationException("CircularReference: Projector({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            ResolvingProjectorTypePairs.Add(TypePair)
            Try
                Return InnerResolver.TryResolveProjector(TypePair)
            Finally
                ResolvingProjectorTypePairs.Remove(TypePair)
            End Try
        End Function
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

    ''' <remarks>缓存解析器</remarks>
    <DebuggerNonUserCode()>
    Public Class CachedResolver
        Implements IObjectMapperResolver

        Private InnerResolver As IObjectMapperResolver
        Public Sub New(ByVal InnerResolver As IObjectMapperResolver)
            Me.InnerResolver = InnerResolver
        End Sub

        Private ProjectorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])
        Private AggregatorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])

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

    ''' <summary>Object映射器</summary>
    <DebuggerNonUserCode()>
    Public Class ObjectMapper
        Private AbsResolver As AbsoluteResolver
        Public Sub New(ByVal Resolver As IObjectMapperResolver)
            Me.AbsResolver = New AbsoluteResolver(Resolver)
        End Sub

        Public Function GetProjector(Of D, R)() As Func(Of D, R)
            Return DirectCast(AbsResolver.ResolveProjector(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Func(Of D, R))
        End Function
        Public Function GetAggregator(Of D, R)() As Action(Of D, R)
            Return DirectCast(AbsResolver.ResolveAggregator(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Action(Of D, R))
        End Function

        Public Function Project(Of D, R)(ByVal Key As D) As R
            Return GetProjector(Of D, R)()(Key)
        End Function
        Public Sub Aggregate(Of D, R)(ByVal Key As D, ByVal Value As R)
            GetAggregator(Of D, R)()(Key, Value)
        End Sub
    End Class
End Namespace
