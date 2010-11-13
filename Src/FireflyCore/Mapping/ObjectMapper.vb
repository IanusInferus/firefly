'==========================================================================
'
'  File:        ObjectMapper.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Object映射
'  Version:     2010.11.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices

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

    ''' <remarks>解析器适配器，用于在无法解析或者出现循环引用时抛出异常。</remarks>
    Public Class ObjectMapperAbsoluteResolver
        Private InnerResolver As IObjectMapperResolver

        Private ResolvingProjectorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
        Private ResolvingAggregatorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
        Public Sub New(ByVal InnerResolver As IObjectMapperResolver)
            Me.InnerResolver = InnerResolver
        End Sub

        Function ResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            If ResolvingProjectorTypePairs.Contains(TypePair) Then Throw New InvalidOperationException("CircularReference: ({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            ResolvingProjectorTypePairs.Add(TypePair)
            Try
                Dim Resolved = InnerResolver.TryResolveProjector(TypePair)
                If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: ({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
                Return Resolved
            Finally
                ResolvingProjectorTypePairs.Remove(TypePair)
            End Try
        End Function
        Function ResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            If ResolvingAggregatorTypePairs.Contains(TypePair) Then Throw New InvalidOperationException("CircularReference: ({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            ResolvingAggregatorTypePairs.Add(TypePair)
            Try
                Dim Resolved = InnerResolver.TryResolveAggregator(TypePair)
                If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: ({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
                Return Resolved
            Finally
                ResolvingAggregatorTypePairs.Remove(TypePair)
            End Try
        End Function
    End Class

    Public Class ObjectMapper
        Implements IObjectMapperResolver

        Private AbsoluteResolver As New ObjectMapperAbsoluteResolver(Me)
        Private ProjectorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])
        Private AggregatorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])
        Private ProjectorResolversValue As New List(Of IObjectProjectorResolver)
        Private AggregatorResolversValue As New List(Of IObjectAggregatorResolver)
        Public ReadOnly Property ProjectorResolvers As List(Of IObjectProjectorResolver)
            Get
                Return ProjectorResolversValue
            End Get
        End Property
        Public ReadOnly Property AggregatorResolvers As List(Of IObjectAggregatorResolver)
            Get
                Return AggregatorResolversValue
            End Get
        End Property
        Public Sub New()
        End Sub

        Private Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
            If ProjectorCache.ContainsKey(TypePair) Then Return ProjectorCache(TypePair)
            For Each r In ProjectorResolversValue
                Dim Resolved = r.TryResolveProjector(TypePair)
                If Resolved IsNot Nothing Then
                    ProjectorCache.Add(TypePair, Resolved)
                    Return Resolved
                End If
            Next
            Return Nothing
        End Function
        Private Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
            If AggregatorCache.ContainsKey(TypePair) Then Return AggregatorCache(TypePair)
            For Each r In AggregatorResolversValue
                Dim Resolved = r.TryResolveAggregator(TypePair)
                If Resolved IsNot Nothing Then
                    AggregatorCache.Add(TypePair, Resolved)
                    Return Resolved
                End If
            Next
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

        Public Function GetProjector(Of D, R)() As Func(Of D, R)
            Return DirectCast(AbsoluteResolver.ResolveProjector(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Func(Of D, R))
        End Function
        Public Function GetAggregator(Of D, R)() As Action(Of D, R)
            Return DirectCast(AbsoluteResolver.ResolveAggregator(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Action(Of D, R))
        End Function

        Public Function Project(Of D, R)(ByVal Key As D) As R
            Return GetProjector(Of D, R)()(Key)
        End Function
        Public Sub Aggregate(Of D, R)(ByVal Key As D, ByVal Value As R)
            GetAggregator(Of D, R)()(Key, Value)
        End Sub
    End Class
End Namespace
