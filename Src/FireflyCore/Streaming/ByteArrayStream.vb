'==========================================================================
'
'  File:        ByteArrayStream.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 字节数组流
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace Streaming
    ''' <summary>
    ''' 字节数组流
    ''' </summary>
    ''' <remarks>
    ''' 请显式调用Close或Dispose来关闭流。
    ''' </remarks>
    Public NotInheritable Class ByteArrayStream
        Implements IReadableWritableSeekableStream
        Private BaseArray As Byte()
        Private BasePositionValue As Integer
        Private PositionValue As Integer
        Private LengthValue As Integer

        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal Length As Integer)
            If Length < 0 Then Throw New ArgumentOutOfRangeException
            BaseArray = New Byte(Length - 1) {}
            BasePositionValue = 0
            PositionValue = 0
            LengthValue = Length
        End Sub
        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal BaseArray As Byte(), Optional ByVal BasePosition As Integer = 0)
            If BaseArray Is Nothing Then Throw New ArgumentNullException
            If BasePosition < 0 OrElse BasePosition > BaseArray.Length Then Throw New ArgumentOutOfRangeException
            Me.BaseArray = BaseArray
            Me.BasePositionValue = BasePosition
            Me.PositionValue = BasePosition
            Me.LengthValue = BaseArray.Length - BasePosition
        End Sub

        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal BaseArray As Byte(), ByVal BasePosition As Integer, ByVal Length As Integer)
            If BaseArray Is Nothing Then Throw New ArgumentNullException
            If Length < 0 Then Throw New ArgumentOutOfRangeException
            If BasePosition < 0 OrElse BasePosition + Length > BaseArray.LongLength Then Throw New ArgumentOutOfRangeException
            Me.BaseArray = BaseArray
            Me.BasePositionValue = BasePosition
            Me.PositionValue = BasePosition
            Me.LengthValue = Length
        End Sub

        ''' <summary>读取元素。</summary>
        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            Dim t As Byte = BaseArray(PositionValue)
            PositionValue += 1
            Return t
        End Function
        ''' <summary>写入元素。</summary>
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            BaseArray(PositionValue) = b
            PositionValue += 1
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IFlushable.Flush
        End Sub
        ''' <summary>用字节表示的流的长度。</summary>
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return LengthValue
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return PositionValue - BasePositionValue
            End Get
            Set(ByVal Value As Int64)
                PositionValue = CInt(BasePositionValue + Value)
            End Set
        End Property
        ''' <summary>已重载。读取到元素数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            If Count < 0 OrElse PositionValue + Count > BasePositionValue + LengthValue Then Throw New ArgumentOutOfRangeException
            Array.Copy(BaseArray, PositionValue, Buffer, Offset, Count)
            PositionValue += Count
        End Sub
        ''' <summary>已重载。写入元素数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            If Count < 0 OrElse PositionValue + Count > BasePositionValue + LengthValue Then Throw New ArgumentOutOfRangeException
            Array.Copy(Buffer, Offset, BaseArray, PositionValue, Count)
            PositionValue += Count
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            BaseArray = Nothing
        End Sub
    End Class
End Namespace
