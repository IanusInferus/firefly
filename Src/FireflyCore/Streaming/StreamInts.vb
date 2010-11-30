'==========================================================================
'
'  File:        StreamInts.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流 - 整数
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Runtime.CompilerServices

Namespace Streaming
    Public Module ReadableStreamInts
        ''' <summary>读取Int8。</summary>
        <Extension()> Public Function ReadInt8(ByVal This As IReadableStream) As SByte
            Return CUS(This.ReadByte())
        End Function
        ''' <summary>读取Int16。</summary>
        <Extension()> Public Function ReadInt16(ByVal This As IReadableStream) As Int16
            Dim o As Int16
            o = CShort(This.ReadByte())
            o = o Or (CShort(This.ReadByte()) << 8)
            Return o
        End Function
        ''' <summary>读取Int32。</summary>
        <Extension()> Public Function ReadInt32(ByVal This As IReadableStream) As Int32
            Dim o As Int32
            o = This.ReadByte()
            o = o Or (CInt(This.ReadByte()) << 8)
            o = o Or (CInt(This.ReadByte()) << 16)
            o = o Or (CInt(This.ReadByte()) << 24)
            Return o
        End Function
        ''' <summary>读取Int64。</summary>
        <Extension()> Public Function ReadInt64(ByVal This As IReadableStream) As Int64
            Dim o As Int64
            o = This.ReadByte()
            o = o Or (CLng(This.ReadByte()) << 8)
            o = o Or (CLng(This.ReadByte()) << 16)
            o = o Or (CLng(This.ReadByte()) << 24)
            o = o Or (CLng(This.ReadByte()) << 32)
            o = o Or (CLng(This.ReadByte()) << 40)
            o = o Or (CLng(This.ReadByte()) << 48)
            o = o Or (CLng(This.ReadByte()) << 56)
            Return o
        End Function
        ''' <summary>读取Int16，高位优先字节序。</summary>
        <Extension()> Public Function ReadInt16B(ByVal This As IReadableStream) As Int16
            Dim o As Int16
            o = CShort(This.ReadByte()) << 8
            o = o Or CShort(This.ReadByte())
            Return o
        End Function
        ''' <summary>读取Int32，高位优先字节序。</summary>
        <Extension()> Public Function ReadInt32B(ByVal This As IReadableStream) As Int32
            Dim o As Int32
            o = CInt(This.ReadByte()) << 24
            o = o Or (CInt(This.ReadByte()) << 16)
            o = o Or (CInt(This.ReadByte()) << 8)
            o = o Or CInt(This.ReadByte())
            Return o
        End Function
        ''' <summary>读取Int64，高位优先字节序。</summary>
        <Extension()> Public Function ReadInt64B(ByVal This As IReadableStream) As Int64
            Dim o As Int64
            o = CLng(This.ReadByte()) << 56
            o = o Or (CLng(This.ReadByte()) << 48)
            o = o Or (CLng(This.ReadByte()) << 40)
            o = o Or (CLng(This.ReadByte()) << 32)
            o = o Or (CLng(This.ReadByte()) << 24)
            o = o Or (CLng(This.ReadByte()) << 16)
            o = o Or (CLng(This.ReadByte()) << 8)
            o = o Or CLng(This.ReadByte())
            Return o
        End Function

        ''' <summary>读取UInt8。</summary>
        <Extension()> Public Function ReadUInt8(ByVal This As IReadableStream) As Byte
            Return This.ReadByte()
        End Function
        ''' <summary>读取UInt16。</summary>
        <Extension()> Public Function ReadUInt16(ByVal This As IReadableStream) As UInt16
            Return CSU(This.ReadInt16())
        End Function
        ''' <summary>读取UInt32。</summary>
        <Extension()> Public Function ReadUInt32(ByVal This As IReadableStream) As UInt32
            Return CSU(This.ReadInt32())
        End Function
        ''' <summary>读取UInt64。</summary>
        <Extension()> Public Function ReadUInt64(ByVal This As IReadableStream) As UInt64
            Return CSU(This.ReadInt64())
        End Function
        ''' <summary>读取UInt16，高位优先字节序。</summary>
        <Extension()> Public Function ReadUInt16B(ByVal This As IReadableStream) As UInt16
            Return CSU(This.ReadInt16B())
        End Function
        ''' <summary>读取UInt32，高位优先字节序。</summary>
        <Extension()> Public Function ReadUInt32B(ByVal This As IReadableStream) As UInt32
            Return CSU(This.ReadInt32B())
        End Function
        ''' <summary>读取UInt64，高位优先字节序。</summary>
        <Extension()> Public Function ReadUInt64B(ByVal This As IReadableStream) As UInt64
            Return CSU(This.ReadInt64B())
        End Function
    End Module

    Public Module WritableStreamInts
        ''' <summary>写入Int8。</summary>
        <Extension()> Public Sub WriteInt8(ByVal This As IWritableStream, ByVal i As SByte)
            This.WriteByte(CSU(i))
        End Sub
        ''' <summary>写入Int16。</summary>
        <Extension()> Public Sub WriteInt16(ByVal This As IWritableStream, ByVal i As Int16)
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
        End Sub
        ''' <summary>写入Int32。</summary>
        <Extension()> Public Sub WriteInt32(ByVal This As IWritableStream, ByVal i As Int32)
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
        End Sub
        ''' <summary>写入Int64。</summary>
        <Extension()> Public Sub WriteInt64(ByVal This As IWritableStream, ByVal i As Int64)
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
            i = i >> 8
            This.WriteByte(CByte(i And &HFF))
        End Sub
        ''' <summary>写入Int16，高位优先字节序。</summary>
        <Extension()> Public Sub WriteInt16B(ByVal This As IWritableStream, ByVal i As Int16)
            This.WriteByte(CByte(CSU(i) >> 8 And &HFF))
            This.WriteByte(CByte(i And &HFF))
        End Sub
        ''' <summary>写入Int32，高位优先字节序。</summary>
        <Extension()> Public Sub WriteInt32B(ByVal This As IWritableStream, ByVal i As Int32)
            This.WriteByte(CByte((CSU(i) >> 24) And &HFF))
            This.WriteByte(CByte((CSU(i) >> 16) And &HFF))
            This.WriteByte(CByte((CSU(i) >> 8) And &HFF))
            This.WriteByte(CByte(i And &HFF))
        End Sub
        ''' <summary>写入Int64，高位优先字节序。</summary>
        <Extension()> Public Sub WriteInt64B(ByVal This As IWritableStream, ByVal i As Int64)
            This.WriteByte(CByte(CLng(CSU(i) >> 56) And &HFF))
            This.WriteByte(CByte(CLng(CSU(i) >> 48) And &HFF))
            This.WriteByte(CByte(CLng(CSU(i) >> 40) And &HFF))
            This.WriteByte(CByte(CLng(CSU(i) >> 32) And &HFF))
            This.WriteByte(CByte(CLng(CSU(i) >> 24) And &HFF))
            This.WriteByte(CByte(CLng(CSU(i) >> 16) And &HFF))
            This.WriteByte(CByte(CLng(CSU(i) >> 8) And &HFF))
            This.WriteByte(CByte(i And &HFF))
        End Sub

        ''' <summary>写入UInt8。</summary>
        <Extension()> Public Sub WriteUInt8(ByVal This As IWritableStream, ByVal b As Byte)
            This.WriteByte(b)
        End Sub
        ''' <summary>写入UInt16。</summary>
        <Extension()> Public Sub WriteUInt16(ByVal This As IWritableStream, ByVal i As UInt16)
            This.WriteInt16(CUS(i))
        End Sub
        ''' <summary>写入UInt32。</summary>
        <Extension()> Public Sub WriteUInt32(ByVal This As IWritableStream, ByVal i As UInt32)
            This.WriteInt32(CUS(i))
        End Sub
        ''' <summary>写入UInt64。</summary>
        <Extension()> Public Sub WriteUInt64(ByVal This As IWritableStream, ByVal i As UInt64)
            This.WriteInt64(CUS(i))
        End Sub
        ''' <summary>写入UInt16，高位优先字节序。</summary>
        <Extension()> Public Sub WriteUInt16B(ByVal This As IWritableStream, ByVal i As UInt16)
            This.WriteInt16B(CUS(i))
        End Sub
        ''' <summary>写入UInt32，高位优先字节序。</summary>
        <Extension()> Public Sub WriteUInt32B(ByVal This As IWritableStream, ByVal i As UInt32)
            This.WriteInt32B(CUS(i))
        End Sub
        ''' <summary>写入UInt64，高位优先字节序。</summary>
        <Extension()> Public Sub WriteUInt64B(ByVal This As IWritableStream, ByVal i As UInt64)
            This.WriteInt64B(CUS(i))
        End Sub
    End Module

    Public Module ReadableSeekableStreamInts
        ''' <summary>查看Byte。</summary>
        <Extension()> Public Function PeekByte(ByVal This As IReadableSeekableStream) As Byte
            Dim HoldPosition = This.Position
            Try
                Return This.ReadByte()
            Finally
                This.Position = HoldPosition
            End Try
        End Function

        ''' <summary>查看Int8。</summary>
        <Extension()> Public Function PeekInt8(ByVal This As IReadableSeekableStream) As SByte
            Dim HoldPosition = This.Position
            Try
                Return This.ReadInt8()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看Int16。</summary>
        <Extension()> Public Function PeekInt16(ByVal This As IReadableSeekableStream) As Int16
            Dim HoldPosition = This.Position
            Try
                Return This.ReadInt16()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看Int32。</summary>
        <Extension()> Public Function PeekInt32(ByVal This As IReadableSeekableStream) As Int32
            Dim HoldPosition = This.Position
            Try
                Return This.ReadInt32()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看Int64。</summary>
        <Extension()> Public Function PeekInt64(ByVal This As IReadableSeekableStream) As Int64
            Dim HoldPosition = This.Position
            Try
                Return This.ReadInt64()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看Int16，高位优先字节序。</summary>
        <Extension()> Public Function PeekInt16B(ByVal This As IReadableSeekableStream) As Int16
            Dim HoldPosition = This.Position
            Try
                Return This.ReadInt16B()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看Int32，高位优先字节序。</summary>
        <Extension()> Public Function PeekInt32B(ByVal This As IReadableSeekableStream) As Int32
            Dim HoldPosition = This.Position
            Try
                Return This.ReadInt32B()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看Int64，高位优先字节序。</summary>
        <Extension()> Public Function PeekInt64B(ByVal This As IReadableSeekableStream) As Int64
            Dim HoldPosition = This.Position
            Try
                Return This.ReadInt64B()
            Finally
                This.Position = HoldPosition
            End Try
        End Function

        ''' <summary>查看UInt8。</summary>
        <Extension()> Public Function PeekUInt8(ByVal This As IReadableSeekableStream) As Byte
            Dim HoldPosition = This.Position
            Try
                Return This.ReadUInt8()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看UInt16。</summary>
        <Extension()> Public Function PeekUInt16(ByVal This As IReadableSeekableStream) As UInt16
            Dim HoldPosition = This.Position
            Try
                Return This.ReadUInt16()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看UInt32。</summary>
        <Extension()> Public Function PeekUInt32(ByVal This As IReadableSeekableStream) As UInt32
            Dim HoldPosition = This.Position
            Try
                Return This.ReadUInt32()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看UInt64。</summary>
        <Extension()> Public Function PeekUInt64(ByVal This As IReadableSeekableStream) As UInt64
            Dim HoldPosition = This.Position
            Try
                Return This.ReadUInt64()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看UInt16，高位优先字节序。</summary>
        <Extension()> Public Function PeekUInt16B(ByVal This As IReadableSeekableStream) As UInt16
            Dim HoldPosition = This.Position
            Try
                Return This.ReadUInt16B()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看UInt32，高位优先字节序。</summary>
        <Extension()> Public Function PeekUInt32B(ByVal This As IReadableSeekableStream) As UInt32
            Dim HoldPosition = This.Position
            Try
                Return This.ReadUInt32B()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
        ''' <summary>查看UInt64，高位优先字节序。</summary>
        <Extension()> Public Function PeekUInt64B(ByVal This As IReadableSeekableStream) As UInt64
            Dim HoldPosition = This.Position
            Try
                Return This.ReadUInt64B()
            Finally
                This.Position = HoldPosition
            End Try
        End Function
    End Module
End Namespace
