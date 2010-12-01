'==========================================================================
'
'  File:        StreamAdapters.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流适配器类
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO

Namespace Streaming
    Friend NotInheritable Class IReadableStreamAdapter
        Implements IReadableStream
        Private BaseStream As Stream

        ''' <summary>初始化新实例。</summary>
        Public Sub New(ByVal BaseStream As Stream)
            If Not BaseStream.CanRead Then Throw New ArgumentException
            Me.BaseStream = BaseStream
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub

        ''' <summary>读取Byte。</summary>
        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            Dim b As Integer = BaseStream.ReadByte
            If b = -1 Then Throw New EndOfStreamException
            Return CByte(b)
        End Function
        ''' <summary>已重载。读取到字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            Dim c As Integer = BaseStream.Read(Buffer, Offset, Count)
            If c <> Count Then Throw New EndOfStreamException
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend NotInheritable Class IWritableStreamAdapter
        Implements IWritableStream
        Private BaseStream As Stream

        ''' <summary>初始化新实例。</summary>
        Public Sub New(ByVal BaseStream As Stream)
            If Not BaseStream.CanWrite Then Throw New ArgumentException
            Me.BaseStream = BaseStream
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub

        ''' <summary>写入Byte。</summary>
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            BaseStream.WriteByte(b)
        End Sub
        ''' <summary>已重载。写入字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            BaseStream.Write(Buffer, Offset, Count)
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend NotInheritable Class IReadableSeekableStreamAdapter
        Implements IReadableSeekableStream
        Private BaseStream As Stream

        ''' <summary>初始化新实例。</summary>
        Public Sub New(ByVal BaseStream As Stream)
            If Not (BaseStream.CanRead AndAlso BaseStream.CanSeek) Then Throw New ArgumentException
            Me.BaseStream = BaseStream
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub

        ''' <summary>用字节表示的流的长度。</summary>
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return BaseStream.Length
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = Value
            End Set
        End Property

        ''' <summary>读取Byte。</summary>
        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            Dim b As Integer = BaseStream.ReadByte
            If b = -1 Then Throw New EndOfStreamException
            Return CByte(b)
        End Function
        ''' <summary>已重载。读取到字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            Dim c As Integer = BaseStream.Read(Buffer, Offset, Count)
            If c <> Count Then Throw New EndOfStreamException
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend NotInheritable Class IWritableSeekableStreamAdapter
        Implements IWritableSeekableStream
        Private BaseStream As Stream

        ''' <summary>初始化新实例。</summary>
        Public Sub New(ByVal BaseStream As Stream)
            If Not (BaseStream.CanWrite AndAlso BaseStream.CanSeek) Then Throw New ArgumentException
            Me.BaseStream = BaseStream
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub

        ''' <summary>用字节表示的流的长度。</summary>
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return BaseStream.Length
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = Value
            End Set
        End Property

        ''' <summary>写入Byte。</summary>
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            BaseStream.WriteByte(b)
        End Sub
        ''' <summary>已重载。写入字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            BaseStream.Write(Buffer, Offset, Count)
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend NotInheritable Class IReadableWritableSeekableStreamAdapter
        Implements IReadableWritableSeekableStream
        Private BaseStream As Stream

        ''' <summary>初始化新实例。</summary>
        Public Sub New(ByVal BaseStream As Stream)
            If Not (BaseStream.CanRead AndAlso BaseStream.CanWrite AndAlso BaseStream.CanSeek) Then Throw New ArgumentException
            Me.BaseStream = BaseStream
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub

        ''' <summary>用字节表示的流的长度。</summary>
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return BaseStream.Length
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = Value
            End Set
        End Property

        ''' <summary>读取Byte。</summary>
        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            Dim b As Integer = BaseStream.ReadByte
            If b = -1 Then Throw New EndOfStreamException
            Return CByte(b)
        End Function
        ''' <summary>已重载。读取到字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            Dim c As Integer = BaseStream.Read(Buffer, Offset, Count)
            If c <> Count Then Throw New EndOfStreamException
        End Sub

        ''' <summary>写入Byte。</summary>
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            BaseStream.WriteByte(b)
        End Sub
        ''' <summary>已重载。写入字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            BaseStream.Write(Buffer, Offset, Count)
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend NotInheritable Class IStreamAdapter
        Implements IStream
        Private BaseStream As Stream

        ''' <summary>初始化新实例。</summary>
        Public Sub New(ByVal BaseStream As Stream)
            If Not (BaseStream.CanRead AndAlso BaseStream.CanWrite AndAlso BaseStream.CanSeek) Then Throw New ArgumentException
            Me.BaseStream = BaseStream
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub

        ''' <summary>用字节表示的流的长度。</summary>
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return BaseStream.Length
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = Value
            End Set
        End Property
        ''' <summary>设置流的长度。</summary>
        Public Sub SetLength(ByVal Value As Int64) Implements IResizableStream.SetLength
            BaseStream.SetLength(Value)
        End Sub

        ''' <summary>读取Byte。</summary>
        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            Dim b As Integer = BaseStream.ReadByte
            If b = -1 Then Throw New EndOfStreamException
            Return CByte(b)
        End Function
        ''' <summary>已重载。读取到字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            Dim c As Integer = BaseStream.Read(Buffer, Offset, Count)
            If c <> Count Then Throw New EndOfStreamException
        End Sub

        ''' <summary>写入Byte。</summary>
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            BaseStream.WriteByte(b)
        End Sub
        ''' <summary>已重载。写入字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            BaseStream.Write(Buffer, Offset, Count)
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
        End Sub
    End Class

    ''' <summary>流适配器类</summary>
    ''' <remarks>用于安全保存IStream的Stream形式。</remarks>
    Friend NotInheritable Class StreamAdapter
        Inherits Stream
        Private BaseStream As IBasicStream

        Private Readable As IReadableStream
        Private Writable As IWritableStream
        Private Seekable As ISeekableStream
        Private Resizable As IResizableStream

        Public Sub New(ByVal s As IBasicStream)
            BaseStream = s
            Readable = TryCast(s, IReadableStream)
            Writable = TryCast(s, IWritableStream)
            Seekable = TryCast(s, ISeekableStream)
            Resizable = TryCast(s, IResizableStream)
        End Sub
        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return Readable IsNot Nothing
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return Seekable IsNot Nothing
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return Writable IsNot Nothing
            End Get
        End Property
        Public Overrides Sub Flush()
            BaseStream.Flush()
        End Sub
        Public Overrides ReadOnly Property Length() As Int64
            Get
                Return Seekable.Length
            End Get
        End Property
        Public Overrides Property Position() As Int64
            Get
                Return Seekable.Position
            End Get
            Set(ByVal Value As Int64)
                Seekable.Position = Value
            End Set
        End Property
        Public Overrides Function Seek(ByVal Offset As Int64, ByVal Origin As System.IO.SeekOrigin) As Int64
            Select Case Origin
                Case SeekOrigin.Begin
                    Position = Offset
                Case SeekOrigin.Current
                    Position += Offset
                Case SeekOrigin.End
                    Position = Length - Offset
            End Select
            Return Position
        End Function
        Public Overrides Sub SetLength(ByVal Value As Int64)
            Resizable.SetLength(Value)
        End Sub
        Public Overrides Function ReadByte() As Integer
            Return Readable.ReadByte()
        End Function
        Public Overrides Sub WriteByte(ByVal Value As Byte)
            Writable.WriteByte(Value)
        End Sub
        Public Overrides Function Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As Integer
            Readable.Read(Buffer, Offset, Count)
            Return Count
        End Function
        Public Overrides Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
            Writable.Write(Buffer, Offset, Count)
        End Sub
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

    ''' <summary>流适配器类-适配非安全流</summary>
    ''' <remarks>用于非安全保存IStream的Stream形式。</remarks>
    Friend NotInheritable Class UnsafeStreamAdapter
        Inherits Stream
        Private BaseStream As IBasicStream

        Private Readable As IReadableStream
        Private Writable As IWritableStream
        Private Seekable As ISeekableStream
        Private Resizable As IResizableStream

        Public Sub New(ByVal s As IBasicStream)
            BaseStream = s
            Readable = TryCast(s, IReadableStream)
            Writable = TryCast(s, IWritableStream)
            Seekable = TryCast(s, ISeekableStream)
            Resizable = TryCast(s, IResizableStream)
        End Sub
        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return Readable IsNot Nothing
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return Seekable IsNot Nothing
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return Writable IsNot Nothing
            End Get
        End Property
        Public Overrides Sub Flush()
            BaseStream.Flush()
        End Sub
        Public Overrides ReadOnly Property Length() As Int64
            Get
                Return Seekable.Length
            End Get
        End Property
        Public Overrides Property Position() As Int64
            Get
                Return Seekable.Position
            End Get
            Set(ByVal Value As Int64)
                Seekable.Position = Value
            End Set
        End Property
        Public Overrides Function Seek(ByVal Offset As Int64, ByVal Origin As System.IO.SeekOrigin) As Int64
            Select Case Origin
                Case SeekOrigin.Begin
                    Position = Offset
                Case SeekOrigin.Current
                    Position += Offset
                Case SeekOrigin.End
                    Position = Length - Offset
            End Select
            Return Position
        End Function
        Public Overrides Sub SetLength(ByVal Value As Int64)
            Resizable.SetLength(Value)
        End Sub
        Public Overrides Function ReadByte() As Integer
            Try
                Return Readable.ReadByte()
            Catch ex As EndOfStreamException
                Return -1
            End Try
        End Function
        Public Overrides Sub WriteByte(ByVal Value As Byte)
            Writable.WriteByte(Value)
        End Sub
        Public Overrides Function Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As Integer
            If Seekable.Position >= Seekable.Length Then
                Return 0
            ElseIf Seekable.Position + Count > Seekable.Length Then
                Dim NewCount = CInt(Seekable.Length - Seekable.Position)
                Readable.Read(Buffer, Offset, NewCount)
                Return NewCount
            Else
                Readable.Read(Buffer, Offset, Count)
                Return Count
            End If
        End Function
        Public Overrides Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
            Writable.Write(Buffer, Offset, Count)
        End Sub
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
