'==========================================================================
'
'  File:        StreamAdapters.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 扩展流适配器类
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO

Namespace Streaming
    ''' <summary>扩展流适配器类</summary>
    ''' <remarks>用于安全保存StreamEx的Stream形式。</remarks>
    Public Class StreamAdapter
        Inherits Stream
        Public BaseStream As IBasicStream

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

    ''' <summary>扩展流适配器类-适配非安全流</summary>
    ''' <remarks>用于安全保存StreamEx的Stream形式。</remarks>
    Public Class UnsafeStreamAdapter
        Inherits Stream
        Public BaseStream As IBasicStream

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
