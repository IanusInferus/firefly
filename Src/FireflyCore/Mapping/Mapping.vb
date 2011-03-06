'==========================================================================
'
'  File:        Mapping.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 映射
'  Version:     2011.03.07.
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
    Public Interface IProjectorResolver
        ''' <param name="TypePair">(DomainType, RangeType)</param>
        ''' <returns>返回Func(Of ${DomainType}, ${RangeType})</returns>
        Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
    End Interface

    Public Interface IAggregatorResolver
        ''' <returns>返回Action(Of ${DomainType}, ${RangeType})</returns>
        Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
    End Interface

    Public Interface IMapperResolver
        Inherits IProjectorResolver
        Inherits IAggregatorResolver
    End Interface

    <DebuggerNonUserCode()>
    Public Module Mapping
        <Extension()> Public Function ResolveProjector(ByVal This As IProjectorResolver, ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            Dim Resolved = This.TryResolveProjector(TypePair)
            If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: Projector({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            Return Resolved
        End Function
        <Extension()> Public Function ResolveAggregator(ByVal This As IAggregatorResolver, ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
            Dim Resolved = This.TryResolveAggregator(TypePair)
            If Resolved Is Nothing Then Throw New NotSupportedException("NotResolved: Aggregator({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
            Return Resolved
        End Function

        <Extension()> Public Function ResolveProjector(Of D, R)(ByVal This As IProjectorResolver) As Func(Of D, R)
            Return DirectCast(This.ResolveProjector(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Func(Of D, R))
        End Function
        <Extension()> Public Function ResolveAggregator(Of D, R)(ByVal This As IAggregatorResolver) As Action(Of D, R)
            Return DirectCast(This.ResolveAggregator(New KeyValuePair(Of Type, Type)(GetType(D), GetType(R))), Action(Of D, R))
        End Function

        <Extension()> Public Function Project(Of D, R)(ByVal This As IProjectorResolver, ByVal Key As D) As R
            Return This.ResolveProjector(Of D, R)()(Key)
        End Function
        <Extension()> Public Sub Aggregate(Of D, R)(ByVal This As IAggregatorResolver, ByVal Key As D, ByVal Value As R)
            This.ResolveAggregator(Of D, R)()(Key, Value)
        End Sub

        Public Function CreateMapper(ByVal ProjectorResolver As IProjectorResolver, ByVal AggregatorResolver As IAggregatorResolver) As IMapperResolver
            Return New MapperResolver(ProjectorResolver, AggregatorResolver)
        End Function
        Public ReadOnly Property EmptyProjectorResolver() As IProjectorResolver
            Get
                Static e As New EmptyProjectorResolverClass
                Return e
            End Get
        End Property
        Public ReadOnly Property EmptyAggregatorResolver() As IAggregatorResolver
            Get
                Static e As New EmptyAggregatorResolverClass
                Return e
            End Get
        End Property

        ''' <remarks>获取不循环解析器，用于在出现循环引用时抛出异常。</remarks>
        <Extension()> Public Function AsNoncircular(ByVal This As IProjectorResolver) As IProjectorResolver
            Return New NoncircularProjectorResolver(This)
        End Function
        ''' <remarks>获取不循环解析器，用于在出现循环引用时抛出异常。</remarks>
        <Extension()> Public Function AsNoncircular(ByVal This As IAggregatorResolver) As IAggregatorResolver
            Return New NoncircularAggregatorResolver(This)
        End Function
        ''' <remarks>获取不循环解析器，用于在出现循环引用时抛出异常。</remarks>
        <Extension()> Public Function AsNoncircular(ByVal This As IMapperResolver) As IMapperResolver
            Return New MapperResolver(New NoncircularProjectorResolver(This), New NoncircularAggregatorResolver(This))
        End Function

        ''' <remarks>获取运行时循环解析器，用于在出现循环引用时延迟到运行时解析。</remarks>
        <Extension()> Public Function AsRuntimeNoncircular(ByVal This As IProjectorResolver) As IProjectorResolver
            Return New RuntimeNoncircularProjectorResolver(This)
        End Function
        ''' <remarks>获取运行时循环解析器，用于在出现循环引用时延迟到运行时解析。</remarks>
        <Extension()> Public Function AsRuntimeNoncircular(ByVal This As IAggregatorResolver) As IAggregatorResolver
            Return New RuntimeNoncircularAggregatorResolver(This)
        End Function
        ''' <remarks>获取运行时循环解析器，用于在出现循环引用时延迟到运行时解析。</remarks>
        <Extension()> Public Function AsRuntimeNoncircular(ByVal This As IMapperResolver) As IMapperResolver
            Return New MapperResolver(New RuntimeNoncircularProjectorResolver(This), New RuntimeNoncircularAggregatorResolver(This))
        End Function

        ''' <remarks>获取缓存解析器。</remarks>
        <Extension()> Public Function AsCached(ByVal This As IProjectorResolver) As IProjectorResolver
            Return New CachedProjectorResolver(This)
        End Function
        ''' <remarks>获取缓存解析器。</remarks>
        <Extension()> Public Function AsCached(ByVal This As IAggregatorResolver) As IAggregatorResolver
            Return New CachedAggregatorResolver(This)
        End Function
        ''' <remarks>获取缓存解析器。</remarks>
        <Extension()> Public Function AsCached(ByVal This As IMapperResolver) As IMapperResolver
            Return New MapperResolver(New CachedProjectorResolver(This), New CachedAggregatorResolver(This))
        End Function

        ''' <remarks>获取连接解析器。</remarks>
        <Extension()> Public Function Concatenated(ByVal This As IEnumerable(Of IProjectorResolver)) As IProjectorResolver
            Return New ConcatenatedProjectorResolver(This)
        End Function
        ''' <remarks>获取连接解析器。</remarks>
        <Extension()> Public Function Concatenated(ByVal This As IEnumerable(Of IAggregatorResolver)) As IAggregatorResolver
            Return New ConcatenatedAggregatorResolver(This)
        End Function
        ''' <remarks>获取连接解析器。</remarks>
        <Extension()> Public Function Concatenated(ByVal This As IEnumerable(Of IMapperResolver)) As IMapperResolver
            Return New MapperResolver(New ConcatenatedProjectorResolver(This), New ConcatenatedAggregatorResolver(This))
        End Function

        <DebuggerNonUserCode()>
        Private Class EmptyProjectorResolverClass
            Implements IProjectorResolver

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
                Return Nothing
            End Function
        End Class
        <DebuggerNonUserCode()>
        Private Class EmptyAggregatorResolverClass
            Implements IAggregatorResolver

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                Return Nothing
            End Function
        End Class

        <DebuggerNonUserCode()>
        Private Class MapperResolver
            Implements IMapperResolver

            Private ProjectorResolver As IProjectorResolver
            Private AggregatorResolver As IAggregatorResolver
            Public Sub New(ByVal ProjectorResolver As IProjectorResolver, ByVal AggregatorResolver As IAggregatorResolver)
                Me.ProjectorResolver = ProjectorResolver
                Me.AggregatorResolver = AggregatorResolver
            End Sub

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
                Return ProjectorResolver.TryResolveProjector(TypePair)
            End Function
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                Return AggregatorResolver.TryResolveAggregator(TypePair)
            End Function
        End Class

        <DebuggerNonUserCode()>
        Private Class NoncircularProjectorResolver
            Implements IProjectorResolver

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal InnerResolver As IProjectorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private ResolvingProjectorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
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
            Implements IAggregatorResolver

            Private InnerResolver As IAggregatorResolver
            Public Sub New(ByVal InnerResolver As IAggregatorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private ResolvingAggregatorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                If ResolvingAggregatorTypePairs.Contains(TypePair) Then Throw New InvalidOperationException("CircularReference: Aggregator({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
                ResolvingAggregatorTypePairs.Add(TypePair)
                Try
                    Return InnerResolver.TryResolveAggregator(TypePair)
                Finally
                    ResolvingAggregatorTypePairs.Remove(TypePair)
                End Try
            End Function
        End Class

        <DebuggerNonUserCode()>
        Private Class RuntimeNoncircularProjectorResolver
            Implements IProjectorResolver

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal InnerResolver As IProjectorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private Class DelayFunc(Of D, R)
                Public CallDelegate As Func(Of [Delegate])
                Public Function Invoke(ByVal Key As D) As R
                    Return DirectCast(CallDelegate(), Func(Of D, R))(Key)
                End Function
            End Class
            Private Class DelayFuncNoncircular(Of D, R)
                Public CallDelegate As Func(Of [Delegate])
                Private Dict As New HashSet(Of D)
                Public Function Invoke(ByVal Key As D) As R
                    If Key Is Nothing Then
                        Return DirectCast(CallDelegate(), Func(Of D, R))(Key)
                    End If
                    If Dict.Contains(Key) Then Throw New InvalidOperationException("CircularReference: Projector({0}, {1})".Formats(GetType(D).FullName, GetType(R).FullName))
                    Dict.Add(Key)
                    Try
                        Return DirectCast(CallDelegate(), Func(Of D, R))(Key)
                    Finally
                        Dict.Remove(Key)
                    End Try
                End Function
            End Class
            Private Shared Function GetDelayFunc(Of D, R)(ByVal f As Func(Of [Delegate])) As [Delegate]
                Dim c As New DelayFunc(Of D, R) With {.CallDelegate = f}
                Return DirectCast(AddressOf c.Invoke, Func(Of D, R))
            End Function
            Private Shared Function GetDelayFuncNoncircular(Of D, R)(ByVal f As Func(Of [Delegate])) As [Delegate]
                Dim c As New DelayFuncNoncircular(Of D, R) With {.CallDelegate = f}
                Return DirectCast(AddressOf c.Invoke, Func(Of D, R))
            End Function
            Private ResolvingProjectorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
                If ResolvingProjectorTypePairs.Contains(TypePair) Then
                    Dim f = DirectCast(Function() InnerResolver.TryResolveProjector(TypePair), Func(Of [Delegate]))
                    Dim df As [Delegate]
                    If TypePair.Key.IsValueType Then
                        df = DirectCast(AddressOf GetDelayFunc(Of DummyType, DummyType), Func(Of Func(Of [Delegate]), [Delegate])).MakeDelegateMethod({TypePair.Key, TypePair.Value}, GetType(Func(Of Func(Of [Delegate]), [Delegate])))
                    Else
                        df = DirectCast(AddressOf GetDelayFuncNoncircular(Of DummyType, DummyType), Func(Of Func(Of [Delegate]), [Delegate])).MakeDelegateMethod({TypePair.Key, TypePair.Value}, GetType(Func(Of Func(Of [Delegate]), [Delegate])))
                    End If
                    Return DirectCast(df, Func(Of Func(Of [Delegate]), [Delegate]))(f)
                End If
                ResolvingProjectorTypePairs.Add(TypePair)
                Try
                    Return InnerResolver.TryResolveProjector(TypePair)
                Finally
                    ResolvingProjectorTypePairs.Remove(TypePair)
                End Try
            End Function
        End Class
        <DebuggerNonUserCode()>
        Private Class RuntimeNoncircularAggregatorResolver
            Implements IAggregatorResolver

            Private InnerResolver As IAggregatorResolver
            Public Sub New(ByVal InnerResolver As IAggregatorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private Class DelayAction(Of D, R)
                Public CallDelegate As Func(Of [Delegate])
                Public Sub Invoke(ByVal Key As D, ByVal Value As R)
                    DirectCast(CallDelegate(), Action(Of D, R))(Key, Value)
                End Sub
            End Class
            Private Class DelayActionNoncircular(Of D, R)
                Public CallDelegate As Func(Of [Delegate])
                Private Dict As New HashSet(Of D)
                Public Sub Invoke(ByVal Key As D, ByVal Value As R)
                    If Dict.Contains(Key) Then Throw New InvalidOperationException("CircularReference: Aggregator({0}, {1})".Formats(GetType(D).FullName, GetType(R).FullName))
                    Dict.Add(Key)
                    Try
                        DirectCast(CallDelegate(), Action(Of D, R))(Key, Value)
                    Finally
                        Dict.Remove(Key)
                    End Try
                End Sub
            End Class
            Private Shared Function GetDelayAction(Of D, R)(ByVal f As Func(Of [Delegate])) As [Delegate]
                Dim c As New DelayAction(Of D, R) With {.CallDelegate = f}
                Return DirectCast(AddressOf c.Invoke, Action(Of D, R))
            End Function
            Private Shared Function GetDelayActionNoncircular(Of D, R)(ByVal f As Func(Of [Delegate])) As [Delegate]
                Dim c As New DelayActionNoncircular(Of D, R) With {.CallDelegate = f}
                Return DirectCast(AddressOf c.Invoke, Action(Of D, R))
            End Function
            Private ResolvingAggregatorTypePairs As New HashSet(Of KeyValuePair(Of Type, Type))
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                If ResolvingAggregatorTypePairs.Contains(TypePair) Then
                    Dim f = DirectCast(Function() InnerResolver.TryResolveAggregator(TypePair), Func(Of [Delegate]))
                    Dim df As [Delegate]
                    If TypePair.Key.IsValueType Then
                        df = DirectCast(AddressOf GetDelayAction(Of DummyType, DummyType), Func(Of Func(Of [Delegate]), [Delegate])).MakeDelegateMethod({TypePair.Key, TypePair.Value}, GetType(Func(Of Func(Of [Delegate]), [Delegate])))
                    Else
                        df = DirectCast(AddressOf GetDelayActionNoncircular(Of DummyType, DummyType), Func(Of Func(Of [Delegate]), [Delegate])).MakeDelegateMethod({TypePair.Key, TypePair.Value}, GetType(Func(Of Func(Of [Delegate]), [Delegate])))
                    End If
                    Return DirectCast(df, Func(Of Func(Of [Delegate]), [Delegate]))(f)
                End If
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
        Private Class CachedProjectorResolver
            Implements IProjectorResolver

            Private InnerResolver As IProjectorResolver
            Public Sub New(ByVal InnerResolver As IProjectorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private ProjectorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
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
            Implements IAggregatorResolver

            Private InnerResolver As IAggregatorResolver
            Public Sub New(ByVal InnerResolver As IAggregatorResolver)
                Me.InnerResolver = InnerResolver
            End Sub

            Private AggregatorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])

            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
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

        ''' <remarks>选择解析器</remarks>
        <DebuggerNonUserCode()>
        Private Class ConcatenatedProjectorResolver
            Implements IProjectorResolver

            Private InnerResolvers As IEnumerable(Of IProjectorResolver)
            Public Sub New(ByVal InnerResolvers As IEnumerable(Of IProjectorResolver))
                Me.InnerResolvers = InnerResolvers
            End Sub

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
                For Each r In InnerResolvers
                    Dim Resolved = r.TryResolveProjector(TypePair)
                    If Resolved IsNot Nothing Then
                        Return Resolved
                    End If
                Next
                Return Nothing
            End Function
        End Class
        ''' <remarks>选择解析器</remarks>
        <DebuggerNonUserCode()>
        Private Class ConcatenatedAggregatorResolver
            Implements IAggregatorResolver

            Private InnerResolvers As IEnumerable(Of IAggregatorResolver)
            Public Sub New(ByVal InnerResolvers As IEnumerable(Of IAggregatorResolver))
                Me.InnerResolvers = InnerResolvers
            End Sub

            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
                For Each r In InnerResolvers
                    Dim Resolved = r.TryResolveAggregator(TypePair)
                    If Resolved IsNot Nothing Then
                        Return Resolved
                    End If
                Next
                Return Nothing
            End Function
        End Class
    End Module

    ''' <remarks>基元解析器</remarks>
    <DebuggerNonUserCode()>
    Public Class PrimitiveResolver
        Implements IMapperResolver

        Private ProjectorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])
        Private AggregatorCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            If ProjectorCache.ContainsKey(TypePair) Then Return ProjectorCache(TypePair)
            Return Nothing
        End Function
        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
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

    ''' <remarks>引用解析器</remarks>
    <DebuggerNonUserCode()>
    Public Class ReferenceProjectorResolver
        Implements IProjectorResolver

        Public Property Inner As IProjectorResolver
        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Return Inner.TryResolveProjector(TypePair)
        End Function
    End Class

    ''' <remarks>引用解析器</remarks>
    <DebuggerNonUserCode()>
    Public Class ReferenceAggregatorResolver
        Implements IAggregatorResolver

        Public Property Inner As IAggregatorResolver
        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Return Inner.TryResolveAggregator(TypePair)
        End Function
    End Class

    ''' <remarks>引用解析器</remarks>
    <DebuggerNonUserCode()>
    Public Class ReferenceMapperResolver
        Implements IMapperResolver

        Public Property Inner As IMapperResolver
        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Return Inner.TryResolveProjector(TypePair)
        End Function
        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Return Inner.TryResolveAggregator(TypePair)
        End Function
    End Class
End Namespace
