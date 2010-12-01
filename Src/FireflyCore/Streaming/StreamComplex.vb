'==========================================================================
'
'  File:        StreamComplex.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流 - 复杂
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.IO
Imports System.Text
Imports Firefly.TextEncoding

Namespace Streaming
    Public Module ReadableStreamComplex
        ''' <summary>已重载。读取到字节数组。</summary>
        <Extension()> Public Sub Read(ByVal This As IReadableStream, ByVal Buffer() As Byte)
            This.Read(Buffer, 0, Buffer.Length)
        End Sub
        ''' <summary>已重载。读取字节数组。</summary>
        <Extension()> Public Function Read(ByVal This As IReadableStream, ByVal Count As Integer) As Byte()
            Dim d As Byte() = New Byte(Count - 1) {}
            This.Read(d, 0, Count)
            Return d
        End Function

        ''' <summary>读取到外部流。</summary>
        <Extension()> Public Sub ReadToStream(ByVal This As IReadableStream, ByVal s As IWritableStream, ByVal Count As Int64)
            If Count <= 0 Then Return
            Dim Buffer As Byte() = New Byte(CInt(Min(Count, 4 * (1 << 20)) - 1)) {}
            For n As Int64 = 0 To Count - Buffer.Length Step Buffer.Length
                This.Read(Buffer)
                s.Write(Buffer)
            Next
            Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
            This.Read(Buffer, 0, LeftLength)
            s.Write(Buffer, 0, LeftLength)
        End Sub

        ''' <summary>读取\0字节结尾的字符串(UTF-16等不适用)。</summary>
        <Extension()> Public Function ReadString(ByVal This As IReadableStream, ByVal Count As Integer, ByVal Encoding As Encoding) As String
            Dim Bytes As New List(Of Byte)
            For n = 0 To Count - 1
                Dim b = This.ReadByte()
                If b = Nul Then
                    For k = 0 To (Count - 1 - n) - 1
                        This.ReadByte()
                    Next
                    Exit For
                Else
                    Bytes.Add(b)
                End If
            Next
            Return Encoding.GetChars(Bytes.ToArray)
        End Function
        ''' <summary>读取包括\0字节的字符串(如UTF-16)。</summary>
        <Extension()> Public Function ReadStringWithNull(ByVal This As IReadableStream, ByVal Count As Integer, ByVal Encoding As Encoding) As String
            Return Encoding.GetChars(This.Read(Count))
        End Function
        ''' <summary>读取ASCII字符串。</summary>
        <Extension()> Public Function ReadSimpleString(ByVal This As IReadableStream, ByVal Count As Integer) As String
            Return This.ReadString(Count, ASCII)
        End Function
        ''' <summary>读取ASCII字符串(包括\0)。</summary>
        <Extension()> Public Function ReadSimpleStringWithNull(ByVal This As IReadableStream, ByVal Count As Integer) As String
            Return This.ReadStringWithNull(Count, ASCII)
        End Function
    End Module

    Public Module WritableStreamComplex
        ''' <summary>已重载。写入字节数组。</summary>
        <Extension()> Public Sub Write(ByVal This As IWritableStream, ByVal Buffer As Byte())
            This.Write(Buffer, 0, Buffer.Length)
        End Sub

        ''' <summary>从外部流写入。</summary>
        <Extension()> Public Sub WriteFromStream(ByVal This As IWritableStream, ByVal s As IReadableStream, ByVal Count As Int64)
            If Count <= 0 Then Return
            Dim Buffer As Byte() = New Byte(CInt(Min(Count, 4 * (1 << 20)) - 1)) {}
            For n As Int64 = 0 To Count - Buffer.Length Step Buffer.Length
                s.Read(Buffer)
                This.Write(Buffer)
            Next
            Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
            s.Read(Buffer, 0, LeftLength)
            This.Write(Buffer, 0, LeftLength)
        End Sub

        ''' <summary>写入\0字节结尾的字符串(UTF-16等不适用)。</summary>
        <Extension()> Public Sub WriteString(ByVal This As IWritableStream, ByVal s As String, ByVal Count As Integer, ByVal Encoding As Encoding)
            If s = "" Then
                For n = 0 To Count - 1
                    This.WriteByte(0)
                Next
            Else
                Dim Bytes = Encoding.GetBytes(s)
                If Bytes.Length > Count Then Throw New InvalidDataException
                This.Write(Bytes)
                For n = Bytes.Length To Count - 1
                    This.WriteByte(0)
                Next
            End If
        End Sub
        ''' <summary>写入ASCII字符串。</summary>
        <Extension()> Public Sub WriteSimpleString(ByVal This As IWritableStream, ByVal s As String, ByVal Count As Integer)
            This.WriteString(s, Count, ASCII)
        End Sub
        ''' <summary>写入ASCII字符串。</summary>
        <Extension()> Public Sub WriteSimpleString(ByVal This As IWritableStream, ByVal s As String)
            This.WriteSimpleString(s, s.Length)
        End Sub
    End Module

    Public Module ReadableSeekableStreamComplex
        ''' <summary>查看ASCII字符串。</summary>
        <Extension()> Public Function PeekSimpleString(ByVal This As IReadableSeekableStream, ByVal Count As Integer) As String
            Dim HoldPosition = This.Position
            Try
                Return This.ReadSimpleString(Count)
            Finally
                This.Position = HoldPosition
            End Try
        End Function
    End Module
End Namespace
