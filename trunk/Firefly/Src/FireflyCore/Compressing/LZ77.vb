'==========================================================================
'
'  File:        LZ77.vb
'  Location:    Firefly.Compressing <Visual Basic .Net>
'  Description: LZ77算法类
'  Version:     2009.11.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On

Imports System
Imports System.Math
Imports System.Collections.Generic

Namespace Compressing
    ''' <summary>
    ''' LZ77算法类
    ''' 完成一个完整压缩的时间复杂度为O(n * MaxHashStringLength)，空间复杂度为O(MaxHashStringLength)
    ''' 压缩时，逐次调用Match获得当前匹配，调用Proceed移动数据指针。
    ''' </summary>
    ''' <remarks>
    ''' 本类不用于较长数据的压缩，如需进行较长数据的压缩，需要将本类用到的一些数组修改成缓存流。
    ''' </remarks>
    Public Class LZ77
        Private Data As Byte()
        Private Offset As Integer
        Private LowerOffset As Integer
        Private IndexTable As Queue(Of List(Of ListPartStringEx(Of Byte)))
        Private InvertTable As Dictionary(Of ListPartStringEx(Of Byte), LinkedList(Of Integer))
        Private SlideWindowLength As Integer
        Private MinMatchLength As Integer
        Private MaxMatchLength As Integer
        Private MinHashStringLength As Integer
        Private MaxHashStringLength As Integer
        Private MaxItemForEachItem As Integer
        Private IndexTableNode As List(Of ListPartStringEx(Of Byte))

        ''' <summary>
        ''' 已重载。构造函数。
        ''' </summary>
        ''' <param name="OriginalData">原始数据</param>
        ''' <param name="SlideWindowLength">滑动窗口大小</param>
        ''' <param name="MaxMatchLength">最大匹配长度</param>
        ''' <param name="MinMatchLength">最小匹配长度</param>
        ''' <param name="MaxHashStringLength">最大散列匹配长度</param>
        Public Sub New(ByVal OriginalData As Byte(), ByVal SlideWindowLength As Integer, ByVal MaxMatchLength As Integer, Optional ByVal MinMatchLength As Integer = 1, Optional ByVal MaxHashStringLength As Integer = 10)
            Me.New(OriginalData, SlideWindowLength, MaxMatchLength, MinMatchLength, MaxHashStringLength, MinMatchLength, SlideWindowLength)
        End Sub

        ''' <summary>
        ''' 已重载。构造函数。
        ''' </summary>
        ''' <param name="OriginalData">原始数据</param>
        ''' <param name="SlideWindowLength">滑动窗口大小</param>
        ''' <param name="MaxMatchLength">最大匹配长度</param>
        ''' <param name="MinMatchLength">最小匹配长度</param>
        ''' <param name="MaxHashStringLength">最大散列匹配长度</param>
        ''' <param name="MinHashStringLength">最小散列匹配长度</param>
        ''' <param name="MaxItemForEachItem">最大非散列匹配项数</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal OriginalData As Byte(), ByVal SlideWindowLength As Integer, ByVal MaxMatchLength As Integer, ByVal MinMatchLength As Integer, ByVal MaxHashStringLength As Integer, ByVal MinHashStringLength As Integer, ByVal MaxItemForEachItem As Integer)
            If OriginalData Is Nothing Then Throw New ArgumentNullException
            If SlideWindowLength <= 0 Then Throw New ArgumentOutOfRangeException
            If MinMatchLength <= 0 Then Throw New ArgumentOutOfRangeException
            If MaxHashStringLength <= 0 Then Throw New ArgumentOutOfRangeException
            If MinHashStringLength <= 0 Then Throw New ArgumentOutOfRangeException
            If MaxItemForEachItem <= 0 Then Throw New ArgumentOutOfRangeException
            If MaxMatchLength < MinMatchLength Then Throw New ArgumentException
            If MaxHashStringLength < MinHashStringLength Then Throw New ArgumentException
            If MaxMatchLength < MaxHashStringLength Then Throw New ArgumentException
            If MaxHashStringLength < MinMatchLength Then Throw New ArgumentException
            Data = OriginalData
            Me.SlideWindowLength = SlideWindowLength
            Me.MinMatchLength = MinMatchLength
            Me.MaxMatchLength = MaxMatchLength
            Me.MinHashStringLength = MinHashStringLength
            Me.MaxHashStringLength = MaxHashStringLength
            Me.MaxItemForEachItem = MaxItemForEachItem
            IndexTable = New Queue(Of List(Of ListPartStringEx(Of Byte)))(SlideWindowLength + MaxMatchLength)
            InvertTable = New Dictionary(Of ListPartStringEx(Of Byte), LinkedList(Of Integer))(SlideWindowLength * MaxMatchLength)
            Offset = 0
            LowerOffset = 0
            IndexTableNode = New List(Of ListPartStringEx(Of Byte))
        End Sub

        ''' <summary>原始数据</summary>
        Public ReadOnly Property OriginalData() As Byte()
            Get
                Return Data
            End Get
        End Property

        ''' <summary>位置</summary>
        Public ReadOnly Property Position() As Integer
            Get
                Return Offset
            End Get
        End Property

        ''' <summary>已重载。前进</summary>
        Public Sub Proceed(ByVal n As Integer)
            If n < 0 Then Throw New ArgumentOutOfRangeException
            For i = 0 To n - 1
                Proceed()
            Next
        End Sub

        ''' <summary>已重载。前进</summary>
        Public Sub Proceed()
            Update()
            IndexTable.Enqueue(IndexTableNode)
            IndexTableNode = New List(Of ListPartStringEx(Of Byte))
            Offset += 1
            Delete()
        End Sub

        ''' <summary>更新查找表</summary>
        Private Sub Update()
            If Offset >= Data.Length Then Throw New InvalidOperationException
            Dim s As ListPartStringEx(Of Byte) = Nothing
            For i = MinHashStringLength To Min(MaxHashStringLength, Data.Length - 1 - Offset)
                If i - MinHashStringLength < IndexTableNode.Count Then
                    s = IndexTableNode(i - MinHashStringLength)
                ElseIf s IsNot Nothing Then
                    s = New ListPartStringEx(Of Byte)(s, 1)
                    IndexTableNode.Add(s)
                Else
                    s = New ListPartStringEx(Of Byte)(Data, Offset, i)
                    IndexTableNode.Add(s)
                End If
                Dim Index As LinkedList(Of Integer) = Nothing
                If InvertTable.TryGetValue(s, Index) Then
                    Index.AddLast(Offset)
                Else
                    Index = New LinkedList(Of Integer)
                    Index.AddLast(Offset)
                    InvertTable.Add(s, Index)
                End If
            Next
        End Sub

        ''' <summary>去除窗口外内容</summary>
        Private Sub Delete()
            While Offset - LowerOffset > SlideWindowLength
                For Each s In IndexTable.Dequeue
                    Dim r = InvertTable(s)
                    System.Diagnostics.Debug.Assert(r.First.Value = LowerOffset)
                    r.RemoveFirst()
                    If r.Count = 0 Then InvertTable.Remove(s)
                Next
                LowerOffset += 1
            End While
        End Sub

        ''' <summary>匹配</summary>
        ''' <remarks>无副作用</remarks>
        Public Function Match() As LZPointer
            Dim Max = Min(MaxMatchLength, Data.Length - Offset)
            Dim PreviousMatches As LinkedList(Of Integer) = Nothing
            Dim PreviousMatch As LZPointer = Nothing
            Dim s As ListPartStringEx(Of Byte) = Nothing
            If Max < MinMatchLength Then Return Nothing
            For l = MinHashStringLength To Min(MinMatchLength - 1, MaxHashStringLength)
                If l - MinHashStringLength < IndexTableNode.Count Then
                    s = IndexTableNode(l - MinHashStringLength)
                ElseIf s IsNot Nothing Then
                    s = New ListPartStringEx(Of Byte)(s, 1)
                    IndexTableNode.Add(s)
                Else
                    s = New ListPartStringEx(Of Byte)(Data, Offset, l)
                    IndexTableNode.Add(s)
                End If
                If Not InvertTable.ContainsKey(s) Then
                    Return Nothing
                End If
                PreviousMatches = InvertTable(s)
            Next
            For l = MinMatchLength To Min(MaxHashStringLength, Max)
                If l - MinHashStringLength < IndexTableNode.Count Then
                    s = IndexTableNode(l - MinHashStringLength)
                ElseIf s IsNot Nothing Then
                    s = New ListPartStringEx(Of Byte)(s, 1)
                    IndexTableNode.Add(s)
                Else
                    s = New ListPartStringEx(Of Byte)(Data, Offset, l)
                    IndexTableNode.Add(s)
                End If
                If Not InvertTable.ContainsKey(s) Then
                    Return PreviousMatch
                End If
                PreviousMatches = InvertTable(s)
                PreviousMatch = New LZPointer(Offset - PreviousMatches.Last.Value, l)
            Next
            If Max <= MaxHashStringLength Then Return PreviousMatch

            If PreviousMatches.Count > MaxItemForEachItem Then
                Dim LastNode = PreviousMatches.Last
                PreviousMatches = New LinkedList(Of Integer)
                For n = 0 To MaxItemForEachItem - 1
                    PreviousMatches.AddFirst(LastNode.Value)
                    LastNode = LastNode.Previous
                Next
            End If

            Dim MaxLength = MaxHashStringLength
            Dim BestNode = PreviousMatches.Last
            Dim Node = PreviousMatches.Last
            While Node IsNot Nothing
                Dim l As Integer
                For l = MaxHashStringLength + 1 To Max
                    If Data(Node.Value + l - 1) <> Data(Offset + l - 1) Then
                        Dim CurrentLength = l - 1
                        If MaxLength < CurrentLength Then
                            MaxLength = CurrentLength
                            BestNode = Node
                        End If
                        Node = Node.Previous
                        Continue While
                    End If
                Next
                MaxLength = Max
                BestNode = Node
                Exit While
            End While
            Return New LZPointer(Offset - BestNode.Value, MaxLength)
        End Function

        ''' <summary>LZ匹配指针，表示一个LZ匹配</summary>
        Public Class LZPointer
            Implements Pointer

            ''' <summary>回退量</summary>
            Public ReadOnly NumBack As Integer
            Private ReadOnly LengthValue As Integer

            Public Sub New(ByVal NumBack As Integer, ByVal Length As Integer)
                Me.NumBack = NumBack
                Me.LengthValue = Length
            End Sub

            ''' <summary>长度</summary>
            Public ReadOnly Property Length() As Integer Implements Pointer.Length
                Get
                    Return LengthValue
                End Get
            End Property
        End Class
    End Class
End Namespace
