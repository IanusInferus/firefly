'==========================================================================
'
'  File:        PartialStreamEx.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 局部扩展流类
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Math
Imports System.IO

Namespace Streaming
    ''' <summary>
    ''' 局部扩展流类，用于表示一个流上的固定开始位置和长度的流，可以递归表示
    ''' </summary>
    ''' <remarks>注意：一切的异常都由你来处理。</remarks>
    Public NotInheritable Class PartialStreamEx
        Implements IStream
        Private BaseStream As IStream

        Private BasePositionValue As Int64
        Private BaseLengthValue As Int64
        Private BaseStreamClose As Boolean
        Private LengthValue As Int64

        ''' <summary>已重载。初始化新实例。</summary>
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        Public Sub New(ByVal BaseStream As IStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            Me.BaseStream = BaseStream
            Me.BasePositionValue = BasePosition
            BaseLengthValue = BaseLength
            LengthValue = BaseLength
            BaseStream.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        ''' <summary>已重载。初始化新实例。</summary>
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <param name="Length">初始大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        Public Sub New(ByVal BaseStream As StreamEx, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            Me.BaseStream = BaseStream
            Me.BasePositionValue = BasePosition
            If BaseLength < Length Then Throw New ArgumentOutOfRangeException
            BaseLengthValue = BaseLength
            LengthValue = Length
            BaseStream.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        ''' <summary>读取字节。</summary>
        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            Return BaseStream.ReadByte
        End Function
        ''' <summary>写入字节。</summary>
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            BaseStream.WriteByte(b)
        End Sub
        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IStream.Flush
            BaseStream.Flush()
        End Sub
        ''' <summary>用字节表示的流的长度。</summary>
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return LengthValue
            End Get
        End Property
        ''' <summary>用字节表示的流在基流中的的偏移位置。</summary>
        Public ReadOnly Property BasePosition() As Int64
            Get
                Return BasePositionValue
            End Get
        End Property
        ''' <summary>用字节表示的流的最大长度。</summary>
        Public ReadOnly Property BaseLength() As Int64
            Get
                Return BaseLengthValue
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position - BasePositionValue
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = BasePositionValue + Value
            End Set
        End Property
        ''' <summary>设置流的长度，不得大于最大大小。</summary>
        Public Sub SetLength(ByVal Value As Int64) Implements IResizableStream.SetLength
            If Value < 0 Then Throw New ArgumentOutOfRangeException
            If Value > BaseLength Then Throw New ArgumentOutOfRangeException
            If BasePositionValue + Value > BaseStream.Length Then BaseStream.SetLength(BasePositionValue + Value)
            LengthValue = Value
        End Sub
        ''' <summary>已重载。读取到字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            If Position + Count > Length Then Throw New EndOfStreamException
            BaseStream.Read(Buffer, Offset, Count)
        End Sub
        ''' <summary>已重载。写入字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            If Position + Count > BaseLength Then Throw New EndOfStreamException
            BaseStream.Write(Buffer, Offset, Count)
            If Position > Length Then LengthValue = Position
        End Sub

        Public Shared Widening Operator CType(ByVal s As PartialStreamEx) As ZeroLengthStreamPasser
            Return New ZeroLengthStreamPasser(s)
        End Operator
        Public Shared Widening Operator CType(ByVal s As PartialStreamEx) As ZeroPositionStreamPasser
            Return New ZeroPositionStreamPasser(s)
        End Operator
        Public Shared Widening Operator CType(ByVal s As PartialStreamEx) As PositionedStreamPasser
            Return New PositionedStreamPasser(s)
        End Operator
        Public Shared Widening Operator CType(ByVal s As PartialStreamEx) As Stream
            Return New StreamAdapter(s)
        End Operator

        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStreamClose Then
                BaseStream.Dispose()
            Else
                BaseStream.Flush()
            End If
            BaseStream = Nothing
        End Sub
    End Class
End Namespace
