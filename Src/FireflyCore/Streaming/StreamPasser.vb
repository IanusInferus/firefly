'==========================================================================
'
'  File:        StreamPasser.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流传递器，用于显式确定函数传参时的流是否包含长度位置信息。
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Runtime.CompilerServices
Imports System.IO

Namespace Streaming
    Public Module StreamPasser
        <Extension()> Public Function AsNewReading(ByVal This As IReadableSeekableStream) As NewReadingStreamPasser
            Return New NewReadingStreamPasser(This)
        End Function
        <Extension()> Public Function AsNewWriting(ByVal This As IStream) As NewWritingStreamPasser
            Return New NewWritingStreamPasser(This)
        End Function
        <Extension()> Public Function AsNewReadingWriting(ByVal This As IStream) As NewReadingWritingStreamPasser
            Return New NewReadingWritingStreamPasser(This)
        End Function

        <Extension()> Public Function AsReadable(ByVal This As Stream) As IReadableStream
            Return New IReadableStreamAdapter(This)
        End Function
        <Extension()> Public Function AsWritable(ByVal This As Stream) As IWritableStream
            Return New IWritableStreamAdapter(This)
        End Function
        <Extension()> Public Function AsReadableSeekable(ByVal This As Stream) As IReadableSeekableStream
            Return New IReadableSeekableStreamAdapter(This)
        End Function
        <Extension()> Public Function AsWritableSeekable(ByVal This As Stream) As IWritableSeekableStream
            Return New IWritableSeekableStreamAdapter(This)
        End Function
        <Extension()> Public Function AsReadableWritableSeekable(ByVal This As Stream) As IReadableWritableSeekableStream
            Return New IReadableWritableSeekableStreamAdapter(This)
        End Function
        <Extension()> Public Function AsIStream(ByVal This As Stream) As IStream
            Return New IStreamAdapter(This)
        End Function

        <Extension()> Public Function ToStream(ByVal This As IBasicStream) As Stream
            Return New StreamAdapter(This)
        End Function
        <Extension()> Public Function ToUnsafeStream(ByVal This As IBasicStream) As Stream
            Return New UnsafeStreamAdapter(This)
        End Function
    End Module

    ''' <summary>新读取流传递器。保证在函数传参时传递零位置的流。</summary>
    Public Class NewReadingStreamPasser
        Private BaseStream As IReadableSeekableStream

        Public Sub New(ByVal s As IReadableSeekableStream)
            If s.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            BaseStream = s
        End Sub

        Public Function GetStream() As IReadableSeekableStream
            If BaseStream.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            Return BaseStream
        End Function
    End Class

    ''' <summary>新写入流传递器。保证在函数传参时传递零长度零位置的流。</summary>
    Public Class NewWritingStreamPasser
        Private BaseStream As IStream

        Public Sub New(ByVal s As IStream)
            If s.Length <> 0 Then Throw New ArgumentException("LengthNotZero")
            If s.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            BaseStream = s
        End Sub

        Public Function GetStream() As IStream
            If BaseStream.Length <> 0 Then Throw New ArgumentException("LengthNotZero")
            If BaseStream.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            Return BaseStream
        End Function
    End Class

    ''' <summary>新读写流传递器。保证在函数传参时传递零位置的流。</summary>
    Public Class NewReadingWritingStreamPasser
        Private BaseStream As IStream

        Public Sub New(ByVal s As IStream)
            If s.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            BaseStream = s
        End Sub

        Public Function GetStream() As IStream
            If BaseStream.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            Return BaseStream
        End Function
    End Class
End Namespace
