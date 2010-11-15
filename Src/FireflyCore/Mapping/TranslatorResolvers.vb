'==========================================================================
'
'  File:        TranslatorResolvers.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 映射分解器
'  Version:     2010.11.15.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Diagnostics

Namespace Mapping
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IProjectorToProjectorDomainTranslator(Of D, M)
        Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of M, R)) As Func(Of D, R)
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IAggregatorToAggregatorDomainTranslator(Of D, M)
        Function TranslateAggregatorToAggregatorDomain(Of R)(ByVal Aggregator As Action(Of M, R)) As Action(Of D, R)
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IProjectorToProjectorRangeTranslator(Of R, M)
        Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, M)) As Func(Of D, R)
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IProjectorToAggregatorRangeTranslator(Of R, M)
        Function TranslateProjectorToAggregatorRange(Of D)(ByVal Projector As Func(Of D, M)) As Action(Of D, R)
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IAggregatorToProjectorRangeTranslator(Of R, M)
        Function TranslateAggregatorToProjectorRange(Of D)(ByVal Aggregator As Action(Of D, M)) As Func(Of D, R)
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IAggregatorToAggregatorRangeTranslator(Of R, M)
        Function TranslateAggregatorToAggregatorRange(Of D)(ByVal Aggregator As Action(Of D, M)) As Action(Of D, R)
    End Interface

    ''' <summary>映射分解器</summary>
    ''' <remarks>
    ''' IProjectorToProjectorDomainTranslator(D, M) = Projector(M, R) -> Projector(D, R)
    ''' IAggregatorToAggregatorDomainTranslator(D, M) = Aggregator(M, R) -> Aggregator(D, R)
    ''' IAggregatorToProjectorRangeTranslator(R, M) = Aggregator(D, M) -> Projector(D, R)
    ''' IProjectorToAggregatorRangeTranslator(R, M) = Projector(D, M) -> Aggregator(D, R)
    ''' 这样就能把(D, R)的映射器转换为(M, R)或者(D, M)的映射器，是一种化简。
    ''' 不过使用的前提是(D, M)或者(R, M)静态已知。
    ''' 本解析器应小心放置，以防止死递归导致无法解析。
    ''' </remarks>
    <DebuggerNonUserCode()>
    Public Class TranslatorResolver
        Private Sub New()
        End Sub

        Public Shared Function Create(Of D, M)(ByVal Resolver As IObjectMapperResolver, ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M)) As IObjectProjectorResolver
            Return New DPP(Of D, M) With {.Inner = New NoncircularResolver(Resolver), .Translator = Translator}
        End Function
        Public Shared Function Create(Of D, M)(ByVal Resolver As IObjectMapperResolver, ByVal Translator As IAggregatorToAggregatorDomainTranslator(Of D, M)) As IObjectAggregatorResolver
            Return New DAA(Of D, M) With {.Inner = New NoncircularResolver(Resolver), .Translator = Translator}
        End Function
        Public Shared Function Create(Of R, M)(ByVal Resolver As IObjectMapperResolver, ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M)) As IObjectProjectorResolver
            Return New RPP(Of R, M) With {.Inner = New NoncircularResolver(Resolver), .Translator = Translator}
        End Function
        Public Shared Function Create(Of R, M)(ByVal Resolver As IObjectMapperResolver, ByVal Translator As IProjectorToAggregatorRangeTranslator(Of R, M)) As IObjectAggregatorResolver
            Return New RPA(Of R, M) With {.Inner = New NoncircularResolver(Resolver), .Translator = Translator}
        End Function
        Public Shared Function Create(Of R, M)(ByVal Resolver As IObjectMapperResolver, ByVal Translator As IAggregatorToProjectorRangeTranslator(Of R, M)) As IObjectProjectorResolver
            Return New RAP(Of R, M) With {.Inner = New NoncircularResolver(Resolver), .Translator = Translator}
        End Function
        Public Shared Function Create(Of R, M)(ByVal Resolver As IObjectMapperResolver, ByVal Translator As IAggregatorToAggregatorRangeTranslator(Of R, M)) As IObjectAggregatorResolver
            Return New RAA(Of R, M) With {.Inner = New NoncircularResolver(Resolver), .Translator = Translator}
        End Function


        'Domain

        <DebuggerNonUserCode()>
        Private Class DPP(Of D, M)
            Implements IObjectProjectorResolver
            Public Inner As IObjectMapperResolver
            Public Translator As IProjectorToProjectorDomainTranslator(Of D, M)
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType Is GetType(D) Then
                    Static DummyMethod As Func(Of Func(Of M, DummyType), Func(Of D, DummyType)) = AddressOf Translator.TranslateProjectorToProjectorDomain(Of DummyType)
                    Dim t = DummyMethod.MakeDelegateMethodFromDummy(RangeType)
                    Dim m = Inner.TryResolveProjector(CreatePair(GetType(M), RangeType))
                    If m Is Nothing Then Return Nothing
                    Return DirectCast(t.DynamicInvoke(m), [Delegate])
                End If
                Return Nothing
            End Function
        End Class
        <DebuggerNonUserCode()>
        Private Class DAA(Of D, M)
            Implements IObjectAggregatorResolver
            Public Inner As IObjectMapperResolver
            Public Translator As IAggregatorToAggregatorDomainTranslator(Of D, M)
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType Is GetType(D) Then
                    Static DummyMethod As Func(Of Action(Of M, DummyType), Action(Of D, DummyType)) = AddressOf Translator.TranslateAggregatorToAggregatorDomain(Of DummyType)
                    Dim t = DummyMethod.MakeDelegateMethodFromDummy(RangeType)
                    Dim m = Inner.TryResolveAggregator(CreatePair(GetType(M), RangeType))
                    If m Is Nothing Then Return Nothing
                    Return DirectCast(t.DynamicInvoke(m), [Delegate])
                End If
                Return Nothing
            End Function
        End Class


        'Range

        <DebuggerNonUserCode()>
        Private Class RPP(Of R, M)
            Implements IObjectProjectorResolver
            Public Inner As IObjectMapperResolver
            Public Translator As IProjectorToProjectorRangeTranslator(Of R, M)
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType Is GetType(R) Then
                    Static DummyMethod As Func(Of Func(Of DummyType, M), Func(Of DummyType, R)) = AddressOf Translator.TranslateProjectorToProjectorRange(Of DummyType)
                    Dim t = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                    Dim m = Inner.TryResolveProjector(CreatePair(DomainType, GetType(M)))
                    If m Is Nothing Then Return Nothing
                    Return DirectCast(t.DynamicInvoke(m), [Delegate])
                End If
                Return Nothing
            End Function
        End Class
        <DebuggerNonUserCode()>
        Private Class RPA(Of R, M)
            Implements IObjectAggregatorResolver
            Public Inner As IObjectMapperResolver
            Public Translator As IProjectorToAggregatorRangeTranslator(Of R, M)
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType Is GetType(R) Then
                    Static DummyMethod As Func(Of Func(Of DummyType, M), Action(Of DummyType, R)) = AddressOf Translator.TranslateProjectorToAggregatorRange(Of DummyType)
                    Dim t = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                    Dim m = Inner.TryResolveProjector(CreatePair(DomainType, GetType(M)))
                    If m Is Nothing Then Return Nothing
                    Return DirectCast(t.DynamicInvoke(m), [Delegate])
                End If
                Return Nothing
            End Function
        End Class
        <DebuggerNonUserCode()>
        Private Class RAP(Of R, M)
            Implements IObjectProjectorResolver
            Public Inner As IObjectMapperResolver
            Public Translator As IAggregatorToProjectorRangeTranslator(Of R, M)
            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType Is GetType(R) Then
                    Static DummyMethod As Func(Of Action(Of DummyType, M), Func(Of DummyType, R)) = AddressOf Translator.TranslateAggregatorToProjectorRange(Of DummyType)
                    Dim t = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                    Dim m = Inner.TryResolveAggregator(CreatePair(DomainType, GetType(M)))
                    If m Is Nothing Then Return Nothing
                    Return DirectCast(t.DynamicInvoke(m), [Delegate])
                End If
                Return Nothing
            End Function
        End Class
        <DebuggerNonUserCode()>
        Private Class RAA(Of R, M)
            Implements IObjectAggregatorResolver
            Public Inner As IObjectMapperResolver
            Public Translator As IAggregatorToAggregatorRangeTranslator(Of R, M)
            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType Is GetType(R) Then
                    Static DummyMethod As Func(Of Action(Of DummyType, M), Action(Of DummyType, R)) = AddressOf Translator.TranslateAggregatorToAggregatorRange(Of DummyType)
                    Dim t = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                    Dim m = Inner.TryResolveAggregator(CreatePair(DomainType, GetType(M)))
                    If m Is Nothing Then Return Nothing
                    Return DirectCast(t.DynamicInvoke(m), [Delegate])
                End If
                Return Nothing
            End Function
        End Class
    End Class
End Namespace
