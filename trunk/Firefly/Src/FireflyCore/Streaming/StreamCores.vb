'==========================================================================
'
'  File:        StreamCores.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流的核
'  Version:     2010.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace Streaming
    Public Interface IReadableStreamCore
        Function ReadByte() As Byte
        Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
    End Interface

    Public Interface IWritableStreamCore
        Sub WriteByte(ByVal b As Byte)
        Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
    End Interface

    Public Interface ISeekableStreamCore
        Property Position() As Int64
        ReadOnly Property Length() As Int64
    End Interface

    Public Interface IResizableStreamCore
        Sub SetLength(ByVal Value As Int64)
    End Interface
End Namespace
