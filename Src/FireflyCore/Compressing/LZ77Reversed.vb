'==========================================================================
'
'  File:        LZ77Reversed.vb
'  Location:    Firefly.Compressing <Visual Basic .Net>
'  Description: 从后向前的LZ77算法类
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
    ''' 从后向前的LZ77算法类
    ''' 完成一个完整压缩的时间复杂度为O(n * MaxHashStringLength)，空间复杂度为O(n)
    ''' 主要用于完成完全LZ和RLE下的绝对最优压缩率的压缩。内存占用很大。
    ''' 压缩时，逐次调用Match获得当前匹配，调用Proceed向左移动数据指针。
    ''' </summary>
    ''' <remarks>
    ''' 本类不用于较长数据的压缩，如需进行较长数据的压缩，需要对数据进行分段处理。
    ''' </remarks>
    Public Class LZ77Reversed
        Private Data As Byte()
        Private Offset As Integer
        Private LowerOffset As Integer
        Private IndexTable As Queue(Of List(Of ListPartStringEx(Of Byte)))
        Private InvertTable As Dictionary(Of ListPartStringEx(Of Byte), LinkedList(Of Integer))
        Private SlideWindowLength As UInt16
        Private MinMatchLength As UInt16
        Private MaxMatchLength As UInt16
        Private MinHashStringLength As UInt16
        Private MaxHashStringLength As UInt16
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
        Public Sub New(ByVal OriginalData As Byte(), ByVal SlideWindowLength As UInt16, ByVal MaxMatchLength As UInt16, Optional ByVal MinMatchLength As UInt16 = 1, Optional ByVal MaxHashStringLength As UInt16 = 10)
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
        Public Sub New(ByVal OriginalData As Byte(), ByVal SlideWindowLength As UInt16, ByVal MaxMatchLength As UInt16, ByVal MinMatchLength As UInt16, ByVal MaxHashStringLength As UInt16, ByVal MinHashStringLength As UInt16, ByVal MaxItemForEachItem As Integer)
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
            IndexTable = New Queue(Of List(Of ListPartStringEx(Of Byte)))
            InvertTable = New Dictionary(Of ListPartStringEx(Of Byte), LinkedList(Of Integer))(CInt(SlideWindowLength) * CInt(MaxMatchLength))
            Offset = OriginalData.Length - 1
            LowerOffset = OriginalData.Length - 1
            IndexTableNode = New List(Of ListPartStringEx(Of Byte))
            Update()
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

        ''' <summary>已重载。向左移动</summary>
        Public Sub Proceed(ByVal n As Integer)
            If n < 0 Then Throw New ArgumentOutOfRangeException
            For i = 0 To n - 1
                Proceed()
            Next
        End Sub

        ''' <summary>已重载。向左移动</summary>
        Public Sub Proceed()
            Offset -= 1
            Delete()
            Update()
        End Sub

        ''' <summary>更新查找表</summary>
        ''' <remarks>无副作用</remarks>
        Private Sub Update()
            If Offset < -1 Then Throw New InvalidOperationException
            While Offset - LowerOffset < SlideWindowLength AndAlso LowerOffset > 0
                LowerOffset -= 1
                Dim LowerIndexTableNode As New List(Of ListPartStringEx(Of Byte))
                Dim s As ListPartStringEx(Of Byte) = Nothing
                For i = MinHashStringLength To Min(MaxHashStringLength, Data.Length - LowerOffset)
                    If s IsNot Nothing Then
                        s = New ListPartStringEx(Of Byte)(s, 1)
                    Else
                        s = New ListPartStringEx(Of Byte)(Data, LowerOffset, i)
                    End If
                    LowerIndexTableNode.Add(s)
                    Dim Index As LinkedList(Of Integer) = Nothing
                    If InvertTable.TryGetValue(s, Index) Then
                        Index.AddLast(LowerOffset)
                    Else
                        Index = New LinkedList(Of Integer)
                        Index.AddLast(LowerOffset)
                        InvertTable.Add(s, Index)
                    End If
                Next
                IndexTable.Enqueue(LowerIndexTableNode)
            End While
        End Sub

        ''' <summary>去除窗口外内容</summary>
        Private Sub Delete()
            If IndexTable.Count > 0 Then
                IndexTableNode = IndexTable.Dequeue
                For Each s In IndexTableNode
                    Dim r = InvertTable(s)
                    System.Diagnostics.Debug.Assert(r.First.Value = Offset)
                    r.RemoveFirst()
                    If r.Count = 0 Then InvertTable.Remove(s)
                Next
            Else
                IndexTableNode = New List(Of ListPartStringEx(Of Byte))
            End If
        End Sub

        ''' <summary>匹配</summary>
        ''' <remarks>无副作用</remarks>
        Public Function Match(ByVal StatesAccLength As LinkedList(Of AccPointer)) As LZPointer
            If StatesAccLength.Count <> Data.Length - Offset - 1 Then Throw New ArgumentException
            Dim Max As UInt16 = CUShort(Min(MaxMatchLength, Data.Length - Offset))
            Dim PreviousMatches As LinkedList(Of Integer) = Nothing
            Dim BestAccLength As Integer = 0
            Dim BestMatch As LZPointer = Nothing
            Dim AccLength = StatesAccLength.First
            If Max < MinHashStringLength Then Return Nothing
            For l = 1 To MinHashStringLength - 1
                AccLength = AccLength.Next
                If AccLength Is Nothing Then Return Nothing
            Next
            Dim s As ListPartStringEx(Of Byte) = Nothing
            If Max < MinMatchLength Then Return Nothing
            For l = MinHashStringLength To Min(MinMatchLength - 1US, MaxHashStringLength)
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
                AccLength = AccLength.Next
                If AccLength Is Nothing Then Return Nothing
            Next
            For l = MinMatchLength To Min(Max, MaxHashStringLength)
                If l - MinHashStringLength < IndexTableNode.Count Then
                    s = IndexTableNode(l - MinHashStringLength)
                ElseIf s IsNot Nothing Then
                    s = New ListPartStringEx(Of Byte)(s, 1)
                    IndexTableNode.Add(s)
                Else
                    s = New ListPartStringEx(Of Byte)(Data, Offset, l)
                    IndexTableNode.Add(s)
                End If
                If Not InvertTable.ContainsKey(s) Then Return BestMatch
                PreviousMatches = InvertTable(s)
                UpdateBestMatch(BestAccLength, BestMatch, l, AccLength, CUShort(Offset - PreviousMatches.Last.Value))
                AccLength = AccLength.Next
                If AccLength Is Nothing Then Return BestMatch
            Next
            If Max <= MaxHashStringLength Then Return BestMatch

            If PreviousMatches.Count > MaxItemForEachItem Then
                Dim LastNode = PreviousMatches.Last
                PreviousMatches = New LinkedList(Of Integer)
                For n = 0 To MaxItemForEachItem - 1
                    PreviousMatches.AddFirst(LastNode.Value)
                    LastNode = LastNode.Previous
                Next
            End If

            Dim AccLengthInitial = AccLength
            Dim Node = PreviousMatches.Last
            While Node IsNot Nothing
                AccLength = AccLengthInitial
                For l = MaxHashStringLength + 1US To Max
                    If Data(Node.Value + l - 1) <> Data(Offset + l - 1) Then
                        Dim CurrentLength = l - 1US
                        UpdateBestMatch(BestAccLength, BestMatch, CurrentLength, AccLength.Previous, CUShort(Offset - Node.Value))
                        Node = Node.Previous
                        Continue While
                    End If
                    If l = Max OrElse AccLength.Next Is Nothing Then
                        UpdateBestMatch(BestAccLength, BestMatch, Max, AccLength, CUShort(Offset - Node.Value))
                        Exit While
                    End If
                    AccLength = AccLength.Next
                Next
            End While
            Return BestMatch
        End Function

        Private Shared Sub UpdateBestMatch(ByRef BestAccLength As Integer, ByRef BestMatch As LZPointer, ByVal CurLength As UInt16, ByVal CurTailAccLengthNode As LinkedListNode(Of AccPointer), ByVal NumBack As UInt16)
            If BestMatch Is Nothing Then
                If CurTailAccLengthNode IsNot Nothing Then
                    BestAccLength = CurTailAccLengthNode.Value.AccLength
                Else
                    BestAccLength = 0
                End If
                BestMatch = New LZPointer(NumBack, CurLength, BestAccLength)
            Else
                Dim CurAccLength As Integer
                If CurTailAccLengthNode IsNot Nothing Then
                    CurAccLength = CurTailAccLengthNode.Value.AccLength
                Else
                    CurAccLength = 0
                End If
                If CurAccLength < BestAccLength Then
                    BestAccLength = CurAccLength
                    BestMatch = New LZPointer(NumBack, CurLength, CurAccLength)
                End If
            End If
        End Sub

        ''' <summary>LZ匹配指针，表示一个LZ匹配</summary>
        Public Class LZPointer
            Implements AccPointer

            ''' <summary>回退量</summary>
            Public ReadOnly NumBack As UInt16
            Private ReadOnly LengthValue As UInt16
            Private AccLengthValue As Integer

            Public Sub New(ByVal NumBack As UInt16, ByVal Length As UInt16, ByVal AccLength As Integer)
                Me.NumBack = NumBack
                Me.LengthValue = Length
                Me.AccLengthValue = AccLength
            End Sub

            ''' <summary>长度</summary>
            Public ReadOnly Property Length() As Integer Implements Pointer.Length
                Get
                    Return LengthValue
                End Get
            End Property

            ''' <summary>后缀最优压缩长度</summary>
            Public Property AccLength() As Integer
                Get
                    Return AccLengthValue
                End Get
                Set(ByVal Value As Integer)
                    AccLengthValue = Value
                End Set
            End Property

            ''' <summary>后缀最优压缩长度</summary>
            Public ReadOnly Property AccLengthReadOnly() As Integer Implements AccPointer.AccLength
                Get
                    Return AccLengthValue
                End Get
            End Property
        End Class

        ''' <summary>指针，表示一个匹配</summary>
        Public Interface AccPointer
            Inherits Pointer

            ''' <summary>后缀最优压缩长度</summary>
            ReadOnly Property AccLength() As Integer
        End Interface

        ''' <summary>字面量指针，表示一个字面量匹配</summary>
        Public Class Literal
            Implements AccPointer

            Private ReadOnly AccLengthValue As Integer

            Public Sub New(ByVal AccLength As Integer)
                Me.AccLengthValue = AccLength
            End Sub

            ''' <summary>长度</summary>
            Public ReadOnly Property Length() As Integer Implements Pointer.Length
                Get
                    Return 1
                End Get
            End Property

            ''' <summary>后缀最优压缩长度</summary>
            Public ReadOnly Property AccLength() As Integer Implements AccPointer.AccLength
                Get
                    Return AccLengthValue
                End Get
            End Property
        End Class
    End Class
End Namespace
