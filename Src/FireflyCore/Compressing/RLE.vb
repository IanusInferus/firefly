'==========================================================================
'
'  File:        RLE.vb
'  Location:    Firefly.Compressing <Visual Basic .Net>
'  Description: RLE算法类
'  Version:     2010.06.23.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On

Imports System
Imports System.Math
Imports System.Collections.Generic

Namespace Compressing
    ''' <summary>
    ''' RLE算法类
    ''' 完成一个完整压缩的时间复杂度为O(n)，空间复杂度为O(1)
    ''' </summary>
    Public Class RLE
        Private Data As Byte()
        Private Offset As Integer
        Private MinMatchLength As UInt16
        Private MaxMatchLength As UInt16

        Public Sub New(ByVal OriginalData As Byte(), ByVal MaxMatchLength As UInt16, Optional ByVal MinMatchLength As UInt16 = 1)
            If OriginalData Is Nothing Then Throw New ArgumentNullException
            If MinMatchLength <= 0 Then Throw New ArgumentOutOfRangeException
            If MaxMatchLength < MinMatchLength Then Throw New ArgumentException
            Data = OriginalData
            Me.MinMatchLength = MinMatchLength
            Me.MaxMatchLength = MaxMatchLength
            Offset = 0
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
            Offset += 1
        End Sub

        ''' <summary>匹配</summary>
        ''' <remarks>无副作用</remarks>
        Public Function Match() As RLEPointer
            Dim d = Data(Offset)
            Dim Max As UInt16 = CUShort(Min(MaxMatchLength, Data.Length - Offset))
            Dim Count = Max
            For l As UInt16 = 1 To Max - 1US
                If Data(Offset + l) <> d Then
                    Count = l
                    Exit For
                End If
            Next
            If Count < MinMatchLength Then Return Nothing
            Return New RLEPointer(d, Count)
        End Function

        ''' <summary>RLE匹配指针，表示一个RLE匹配</summary>
        Public Class RLEPointer
            Implements Pointer

            ''' <summary>重复值</summary>
            Public ReadOnly Value As Byte
            Private ReadOnly Count As UInt16

            Public Sub New(ByVal Value As Byte, ByVal Count As UInt16)
                Me.Value = Value
                Me.Count = Count
            End Sub

            ''' <summary>长度</summary>
            Public ReadOnly Property Length() As Integer Implements Pointer.Length
                Get
                    Return Count
                End Get
            End Property
        End Class
    End Class
End Namespace
