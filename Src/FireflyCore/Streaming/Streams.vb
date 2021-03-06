﻿'==========================================================================
'
'  File:        Streams.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 扩展流类
'  Version:     2018.09.09.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO

Namespace Streaming
    ''' <summary>
    ''' 扩展流类
    ''' </summary>
    ''' <remarks>
    ''' 请显式调用Close或Dispose来关闭流。
    ''' 如果调用了ToStream或转换到了Stream，并放弃了StreamEx，StreamEx也不会消失，因为使用了一个继承自Stream的Adapter来持有StreamEx的引用。
    ''' 本类与System.IO.StreamReader等类不兼容。这些类使用了ReadByte返回的结束标志-1等。本类会在位置超过文件长度时读取会抛出异常。
    ''' 本类主要用于封装System.IO.MemoryStream和System.IO.FileStream，对其他流可能抛出无法预期的异常。
    ''' 一切的异常都由调用者来处理。
    ''' </remarks>
    Partial Public Class Streams
        Private Sub New()
        End Sub

        ''' <summary>初始化新实例。</summary>
        Public Shared Function OpenReadable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IReadableSeekableStream
            Return SafeWrap(New FileStream(Path, FileMode.Open, FileAccess.Read, Share), Function(fs) New IReadableSeekableStreamAdapter(fs))
        End Function
        ''' <summary>初始化新实例。</summary>
        Public Shared Function CreateWritable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IWritableSeekableStream
            Return SafeWrap(New FileStream(Path, FileMode.Create, FileAccess.Write, Share), Function(fs) New IWritableSeekableStreamAdapter(fs))
        End Function
        ''' <summary>初始化新实例。</summary>
        Public Shared Function CreateNewWritable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IWritableSeekableStream
            Return SafeWrap(New FileStream(Path, FileMode.CreateNew, FileAccess.Write, Share), Function(fs) New IWritableSeekableStreamAdapter(fs))
        End Function
        ''' <summary>初始化新实例。</summary>
        Public Shared Function CreateReadableWritable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IReadableWritableSeekableStream
            Return SafeWrap(New FileStream(Path, FileMode.Create, FileAccess.ReadWrite, Share), Function(fs) New IReadableWritableSeekableStreamAdapter(fs))
        End Function
        ''' <summary>初始化新实例。</summary>
        Public Shared Function OpenReadableWritable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IReadableWritableSeekableStream
            Return SafeWrap(New FileStream(Path, FileMode.Open, FileAccess.ReadWrite, Share), Function(fs) New IReadableWritableSeekableStreamAdapter(fs))
        End Function
        ''' <summary>初始化新实例。</summary>
        Public Shared Function OpenOrCreateReadableWritable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IReadableWritableSeekableStream
            Return SafeWrap(New FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, Share), Function(fs) New IReadableWritableSeekableStreamAdapter(fs))
        End Function
        ''' <summary>已重载。初始化新实例。</summary>
        Public Shared Function CreateMemoryStream() As IStream
            Return SafeWrap(New MemoryStream, Function(fs) New IStreamAdapter(fs))
        End Function
        ''' <summary>已重载。初始化新实例。</summary>
        Public Shared Function CreateResizable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IStream
            Return SafeWrap(New FileStream(Path, FileMode.Create, FileAccess.ReadWrite, Share), Function(fs) New IStreamAdapter(fs))
        End Function
        ''' <summary>已重载。初始化新实例。</summary>
        Public Shared Function OpenResizable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IStream
            Return SafeWrap(New FileStream(Path, FileMode.Open, FileAccess.ReadWrite, Share), Function(fs) New IStreamAdapter(fs))
        End Function
        ''' <summary>已重载。初始化新实例。</summary>
        Public Shared Function OpenOrCreateResizable(ByVal Path As String, Optional ByVal Share As FileShare = FileShare.Read) As IStream
            Return SafeWrap(New FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, Share), Function(fs) New IStreamAdapter(fs))
        End Function

        Private Shared Function SafeWrap(Of T)(ByVal Stream As Stream, ByVal Factory As Func(Of Stream, T)) As T
            Dim Success = False
            Try
                Dim a = Factory(Stream)
                Success = True
                Return a
            Finally
                If Not Success Then
                    Stream.Dispose()
                End If
            End Try
        End Function
    End Class
End Namespace
