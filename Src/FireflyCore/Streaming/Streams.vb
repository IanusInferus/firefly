'==========================================================================
'
'  File:        Streams.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace Streaming
    Public Interface IFlushable
        ''' <summary>强制同步缓冲数据。</summary>
        Sub Flush()
    End Interface

    Public Interface IBasicStream
        Inherits IFlushable
        Inherits IDisposable
    End Interface

    Public Interface IReadableStream
        Inherits IBasicStream

        ''' <summary>读取字节。</summary>
        Function ReadByte() As Byte

        ''' <summary>读取到字节数组。</summary>
        Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
    End Interface

    Public Interface IWritableStream
        Inherits IBasicStream

        ''' <summary>写入字节。</summary>
        Sub WriteByte(ByVal b As Byte)

        ''' <summary>写入字节数组。</summary>
        Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
    End Interface

    Public Interface ISeekableStream
        Inherits IBasicStream

        ''' <summary>流的当前位置。</summary>
        Property Position() As Int64

        ''' <summary>用字节表示的流的长度。</summary>
        ReadOnly Property Length() As Int64
    End Interface

    Public Interface IResizableStream
        Inherits IBasicStream

        ''' <summary>设置流的长度。</summary>
        Sub SetLength(ByVal Value As Int64)
    End Interface

    Public Interface IReadableSeekableStream
        Inherits IReadableStream
        Inherits ISeekableStream
    End Interface

    Public Interface IWritableSeekableStream
        Inherits IWritableStream
        Inherits ISeekableStream
    End Interface

    Public Interface IReadableWritableSeekableStream
        Inherits IReadableSeekableStream
        Inherits IWritableSeekableStream
    End Interface

    Public Interface IStream
        Inherits IReadableWritableSeekableStream
        Inherits IResizableStream
    End Interface
End Namespace
