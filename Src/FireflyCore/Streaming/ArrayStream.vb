﻿'==========================================================================
'
'  File:        ArrayStream.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 数组流
'  Version:     2011.03.06.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace Streaming
    ''' <summary>
    ''' 数组流
    ''' </summary>
    ''' <remarks>
    ''' 请显式调用Close或Dispose来关闭流。
    ''' </remarks>
    Public NotInheritable Class ArrayStream(Of T)
        Implements IDisposable
        Private BaseArray As T()
        Private BasePositionValue As Integer
        Private PositionValue As Integer
        Private LengthValue As Integer

        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal Length As Integer)
            If Length < 0 Then Throw New ArgumentOutOfRangeException
            BaseArray = New T(Length - 1) {}
            BasePositionValue = 0
            PositionValue = 0
            LengthValue = Length
        End Sub
        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal BaseArray As T(), Optional ByVal BasePosition As Integer = 0)
            If BaseArray Is Nothing Then Throw New ArgumentNullException
            If BasePosition < 0 OrElse BasePosition > BaseArray.Length Then Throw New ArgumentOutOfRangeException
            Me.BaseArray = BaseArray
            Me.BasePositionValue = BasePosition
            Me.PositionValue = BasePosition
            Me.LengthValue = BaseArray.Length - BasePosition
        End Sub

        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal BaseArray As T(), ByVal BasePosition As Integer, ByVal Length As Integer)
            If BaseArray Is Nothing Then Throw New ArgumentNullException
            If Length < 0 Then Throw New ArgumentOutOfRangeException
            If BasePosition < 0 OrElse BasePosition + Length > BaseArray.Length Then Throw New ArgumentOutOfRangeException
            Me.BaseArray = BaseArray
            Me.BasePositionValue = BasePosition
            Me.PositionValue = BasePosition
            Me.LengthValue = Length
        End Sub

        ''' <summary>读取元素。</summary>
        Public Function ReadElement() As T
            Dim t As T = BaseArray(PositionValue)
            PositionValue += 1
            Return t
        End Function
        ''' <summary>写入元素。</summary>
        Public Sub WriteElement(ByVal b As T)
            BaseArray(PositionValue) = b
            PositionValue += 1
        End Sub
        ''' <summary>查看元素。</summary>
        Public Function PeekElement() As T
            Dim HoldPosition = PositionValue
            Try
                Return ReadElement()
            Finally
                PositionValue = HoldPosition
            End Try
        End Function

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush()
        End Sub
        ''' <summary>关闭流。</summary>
        ''' <remarks>对继承者的说明：该方法调用Dispose()，不要覆盖该方法，而应覆盖Dispose(Boolean)</remarks>
        Public Sub Close()
            Static Closed As Boolean = False
            If Closed Then Throw New InvalidOperationException
            Dispose()
            Closed = True
        End Sub
        ''' <summary>用字节表示的流的长度。</summary>
        ReadOnly Property Length() As Int64
            Get
                Return LengthValue
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Integer
            Get
                Return PositionValue - BasePositionValue
            End Get
            Set(ByVal Value As Integer)
                PositionValue = BasePositionValue + Value
            End Set
        End Property
        ''' <summary>已重载。读取到元素数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Sub Read(ByVal Buffer As T(), ByVal Offset As Integer, ByVal Count As Integer)
            If Count < 0 OrElse PositionValue + Count > BasePositionValue + LengthValue Then Throw New ArgumentOutOfRangeException
            Array.Copy(BaseArray, PositionValue, Buffer, Offset, Count)
            PositionValue += Count
        End Sub
        ''' <summary>已重载。读取到元素数组。</summary>
        Public Sub Read(ByVal Buffer() As T)
            Read(Buffer, 0, Buffer.Length)
        End Sub
        ''' <summary>已重载。读取元素数组。</summary>
        Public Function Read(ByVal Count As Integer) As T()
            Dim d As T() = New T(Count - 1) {}
            Read(d, 0, Count)
            Return d
        End Function
        ''' <summary>已重载。写入元素数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Sub Write(ByVal Buffer As T(), ByVal Offset As Integer, ByVal Count As Integer)
            If Count < 0 OrElse PositionValue + Count > BasePositionValue + LengthValue Then Throw New ArgumentOutOfRangeException
            Array.Copy(Buffer, Offset, BaseArray, PositionValue, Count)
            PositionValue += Count
        End Sub
        ''' <summary>已重载。写入元素数组。</summary>
        Public Sub Write(ByVal Buffer As T())
            Write(Buffer, 0, Buffer.Length)
        End Sub

        ''' <summary>读取到外部流。</summary>
        Public Sub ReadToStream(ByVal s As ArrayStream(Of T), ByVal Count As Integer)
            If Count <= 0 Then Return
            Dim Buffer As T() = New T(CInt(Min(Count, 4 * (1 << 10)) - 1)) {}
            For n As Integer = 0 To Count - Buffer.Length Step Buffer.Length
                Read(Buffer)
                s.Write(Buffer)
            Next
            Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
            Read(Buffer, 0, LeftLength)
            s.Write(Buffer, 0, LeftLength)
        End Sub
        ''' <summary>从外部流写入。</summary>
        Public Sub WriteFromStream(ByVal s As ArrayStream(Of T), ByVal Count As Integer)
            If Count <= 0 Then Return
            Dim Buffer As T() = New T(CInt(Min(Count, 4 * (1 << 10)) - 1)) {}
            For n As Integer = 0 To Count - Buffer.Length Step Buffer.Length
                s.Read(Buffer)
                Write(Buffer)
            Next
            Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
            s.Read(Buffer, 0, LeftLength)
            Write(Buffer, 0, LeftLength)
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            BaseArray = Nothing
        End Sub
    End Class
End Namespace
