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
        Sub Flush()
    End Interface

    Public Interface IBasicStream
        Inherits IFlushable
        Inherits IDisposable
    End Interface

    Public Interface IReadableStream
        Inherits IBasicStream

        Function ReadByte() As Byte
        Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
    End Interface

    Public Interface IWritableStream
        Inherits IBasicStream

        Sub WriteByte(ByVal b As Byte)
        Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
    End Interface

    Public Interface ISeekableStream
        Inherits IBasicStream

        Property Position() As Int64
        ReadOnly Property Length() As Int64
    End Interface

    Public Interface IResizableStream
        Inherits IBasicStream

        Sub SetLength(ByVal Value As Int64)
    End Interface

    Public Interface IReadableSeekableStream
        Inherits IReadableStream
        Inherits ISeekableStream
    End Interface

    Public Interface IReadableWritableSeekableStream
        Inherits IReadableSeekableStream
        Inherits IWritableStream
    End Interface

    Public Interface IStream
        Inherits IReadableWritableSeekableStream
        Inherits IResizableStream
    End Interface
End Namespace
