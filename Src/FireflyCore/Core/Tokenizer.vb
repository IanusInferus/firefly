'==========================================================================
'
'  File:        Tokenizer.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 词法分析器
'  Version:     2009.11.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Public Class Automaton(Of TState, TSymbol)
    Public State As TState
    Public Transition As Func(Of TState, TSymbol, TState)

    Public Sub Feed(ByVal Symbol As TSymbol)
        State = Transition(State, Symbol)
    End Sub
End Class

Public Class TokenizerState(Of TToken, TSymbol)
    Public BestValue As StringEx(Of TToken) = Nothing
    Public BestLength As Integer = 0
    Public Matched As New List(Of TSymbol)
    Public MatchedStr As New ListPartStringEx(Of TSymbol)(Matched)
    Public Buffer As New LinkedList(Of TSymbol)
End Class

Public Class Tokenizer(Of TSymbol, TToken)
    Private Dict As Dictionary(Of StringEx(Of TSymbol), StringEx(Of TToken))
    Private DirectDict As Dictionary(Of StringEx(Of TSymbol), StringEx(Of TToken))
    Private Num As Integer
    Private MaxTokenPerSymbolValue As Integer

    Public Sub New(ByVal Dict As Dictionary(Of StringEx(Of TSymbol), StringEx(Of TToken)))
        Me.Dict = Dict
        Num = Dict.Select(Function(p) p.Key.Count).Max
        DirectDict = New Dictionary(Of StringEx(Of TSymbol), StringEx(Of TToken))()
        For n = 1 To Num
            Dim k = n
            Dim sd = (From p In Dict Where p.Key.Count = k).ToArray
            For Each p In sd
                If k > 1 Then
                    Dim a = p.Key.ToArray
                    Dim s = New ListPartStringEx(Of TSymbol)(a, 0, 1)
                    If DirectDict.ContainsKey(s) Then DirectDict.Remove(s)
                    For i = 2 To k - 1
                        s = New ListPartStringEx(Of TSymbol)(s, 1)
                        If DirectDict.ContainsKey(s) Then DirectDict.Remove(s)
                    Next
                End If
                DirectDict.Add(p.Key, p.Value)
            Next
        Next
        MaxTokenPerSymbolValue = Dict.Select(Function(p) p.Value.Count).Max
    End Sub

    Public ReadOnly Property MaxSymbolPerToken() As Integer
        Get
            Return Num
        End Get
    End Property

    Public ReadOnly Property MaxTokenPerSymbol() As Integer
        Get
            Return MaxTokenPerSymbolValue
        End Get
    End Property

    Public Function GetDefaultState() As TokenizerState(Of TToken, TSymbol)
        Return New TokenizerState(Of TToken, TSymbol)
    End Function

    Public Function Transit(ByVal State As TokenizerState(Of TToken, TSymbol), ByVal WriteOutput As Action(Of TToken), ByVal ThrowException As Action(Of TSymbol, Integer)) As TokenizerState(Of TToken, TSymbol)
        With State
            While .Buffer.Count > 0
                .Matched.Add(.Buffer.First.Value)
                .MatchedStr = New ListPartStringEx(Of TSymbol)(.MatchedStr, 1)
                .Buffer.RemoveFirst()
                While .Matched.Count > 0
                    Dim DValue As StringEx(Of TToken) = Nothing
                    If DirectDict.TryGetValue(.MatchedStr, DValue) Then
                        'Reduce
                        For Each v In DValue
                            WriteOutput(v)
                        Next
                        .BestValue = Nothing
                        .BestLength = 0
                        .Matched.Clear()
                        .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                        Exit While
                    End If
                    If .Matched.Count >= Num Then
                        '强行结束，如果无法Reduce则出错
                        Dim Value As StringEx(Of TToken) = Nothing
                        If Dict.TryGetValue(.MatchedStr, Value) Then
                            'Reduce
                            For Each v In Value
                                WriteOutput(v)
                            Next
                            .BestValue = Nothing
                            .BestLength = 0
                            .Matched.Clear()
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                            Exit While
                        ElseIf .BestValue IsNot Nothing Then
                            'Reduce
                            For Each v In .BestValue
                                WriteOutput(v)
                            Next
                            .BestValue = Nothing
                            .BestLength = 0
                            .Matched.RemoveRange(0, .BestLength)
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                            Exit While
                        Else
                            'Fallback
                            For n = .Matched.Count - 1 To 1 Step -1
                                .Buffer.AddFirst(.Matched(n))
                            Next
                            .Matched.RemoveRange(1, .Matched.Count - 1)
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                            ThrowException(.Matched(0), -(.Matched.Count + .Buffer.Count))
                            .BestValue = Nothing
                            .BestLength = 0
                            .Matched.Clear()
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                        End If
                    Else
                        Dim Value As StringEx(Of TToken) = Nothing
                        If Dict.TryGetValue(.MatchedStr, Value) Then
                            'Shift
                            .BestValue = Value
                            .BestLength = .MatchedStr.Count
                            Exit While
                        Else
                            'Shift
                            Exit While
                        End If
                    End If
                End While
            End While
        End With
        Return State
    End Function

    Public Function Finish(ByVal State As TokenizerState(Of TToken, TSymbol), ByVal WriteOutput As Action(Of TToken), ByVal ThrowException As Action(Of TSymbol, Integer)) As TokenizerState(Of TToken, TSymbol)
        With State
            While .Buffer.Count > 0 OrElse .Matched.Count > 0
                If .Buffer.Count > 0 Then
                    .Matched.Add(.Buffer.First.Value)
                    .MatchedStr = New ListPartStringEx(Of TSymbol)(.MatchedStr, 1)
                    .Buffer.RemoveFirst()
                End If
                While .Matched.Count > 0
                    Dim DValue As StringEx(Of TToken) = Nothing
                    If DirectDict.TryGetValue(.MatchedStr, DValue) Then
                        'Reduce
                        For Each v In DValue
                            WriteOutput(v)
                        Next
                        .BestValue = Nothing
                        .BestLength = 0
                        .Matched.Clear()
                        .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                        Exit While
                    End If
                    If .Matched.Count >= Num OrElse .Buffer.Count = 0 Then
                        '强行结束，如果无法Reduce则出错
                        Dim Value As StringEx(Of TToken) = Nothing
                        If Dict.TryGetValue(.MatchedStr, Value) Then
                            'Reduce
                            For Each v In Value
                                WriteOutput(v)
                            Next
                            .BestValue = Nothing
                            .BestLength = 0
                            .Matched.Clear()
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                            Exit While
                        ElseIf .BestValue IsNot Nothing Then
                            'Reduce
                            For Each v In .BestValue
                                WriteOutput(v)
                            Next
                            .BestValue = Nothing
                            .BestLength = 0
                            .Matched.RemoveRange(0, .BestLength)
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                            Exit While
                        Else
                            'Fallback
                            For n = .Matched.Count - 1 To 1 Step -1
                                .Buffer.AddFirst(.Matched(n))
                            Next
                            .Matched.RemoveRange(1, .Matched.Count - 1)
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                            ThrowException(.Matched(0), -(.Matched.Count + .Buffer.Count))
                            .BestValue = Nothing
                            .BestLength = 0
                            .Matched.Clear()
                            .MatchedStr = New ListPartStringEx(Of TSymbol)(.Matched)
                        End If
                    Else
                        Dim Value As StringEx(Of TToken) = Nothing
                        If Dict.TryGetValue(.MatchedStr, Value) Then
                            'Shift
                            .BestValue = Value
                            .BestLength = .MatchedStr.Count
                            Exit While
                        Else
                            'Shift
                            Exit While
                        End If
                    End If
                End While
            End While
        End With
        Return State
    End Function

    Public Function Feed(ByVal State As TokenizerState(Of TToken, TSymbol), ByVal Symbol As TSymbol) As TokenizerState(Of TToken, TSymbol)
        State.Buffer.AddLast(Symbol)
        Return State
    End Function

    Public Function IsStateFinished(ByVal State As TokenizerState(Of TToken, TSymbol)) As Boolean
        Return State.Matched.Count = 0 AndAlso State.Buffer.Count = 0
    End Function
End Class
