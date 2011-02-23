'==========================================================================
'
'  File:        DAT.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: プリニ DAT格式
'  Version:     2011.02.23.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

'这里主要提供对《プリニ》这个游戏的文件包的支持。
'示例中已包含该包类型的样例SCRIPT.DAT。
'プリニ DAT格式如下。

'プリニ DAT格式

'Header 00h
'12      String          Identifier      "NISPACK"
'4       Int32           NumFile         07 00 00 00

'Index 10h
'32      String          Name            "debug.nsf"
'4       Int32           Address         00 08 00 00
'4       Int32           Length          F5 03 00 00
'4       Int32           ?               E8 03 47 39

'Data
'(
'*       Byte()          FileData
'800h对齐
')

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.Packaging

Public Class DAT
    Inherits PackageDiscrete '使用离散文件包接口，表示文件数据不一定非要连续，即通过位置和长度来确定，连续文件一般只有长度一个数值

    Public Sub New(ByVal sp As NewReadingStreamPasser)
        MyBase.New(sp)
        Initialize()
    End Sub
    Public Sub New(ByVal sp As NewReadingWritingStreamPasser)
        MyBase.New(sp)
        Initialize()
    End Sub

    '在构造函数中填入文件包读取的部分，对每个文件需要调用PushFile以构造路径信息和各种映射信息
    Public Sub Initialize()
        Dim s = Readable

        '判断文件头部是否正常
        If s.ReadSimpleString(12) <> "NISPACK" Then Throw New InvalidDataException

        Dim NumFile = s.ReadInt32

        For n = 0 To NumFile - 1

            '读取索引的各部分
            Dim Name = s.ReadSimpleString(32) '读取简单字符串
            Dim Address = s.ReadInt32
            Dim Length = s.ReadInt32
            Dim Unknown = s.ReadInt32

            '创建一个文件描述信息，包括文件名、文件大小、文件地址
            Dim f As New FileDB(Name, FileDB.FileType.File, Length, Address)

            '将文件描述信息传递到框架内部
            '框架内部能够自动创建文件树(将文件名中以'\'或者'/'表示的文件路径拆开)
            '框架内部能够自动创建IndexOfFile映射表，能够将文件描述信息映射到文件索引的出现顺序
            '框架内部还记录一些数据用于寻找能放下数据的空洞
            PushFile(f)
        Next

        '离散文件在打开的时候应该寻找空洞，以供导入文件使用
        '寻找的起始地址是从当前位置的下一个块开始的位置
        ScanHoles(GetSpace(s.Position))
    End Sub

    '提供格式在打开文件包窗口中的过滤器
    Public Shared ReadOnly Property Filter() As String
        Get
            Return "プリニ DAT格式(*.DAT)|*.DAT"
        End Get
    End Property

    '打开文件包的函数
    Public Shared Function Open(ByVal Path As String) As PackageBase
        Dim s As IStream = Nothing
        Dim sRead As IReadableSeekableStream = Nothing
        Try
            s = Streams.OpenResizable(Path)
        Catch
            sRead = Streams.OpenReadable(Path)
        End Try
        If s IsNot Nothing Then
            Return New DAT(s.AsNewReadingWriting)
        Else
            Return New DAT(sRead.AsNewReading)
        End If
    End Function

    '读取文件在索引中的地址信息，所有索引中的地址信息应该在这里更新
    Public Overrides Property FileAddressInPhysicalFileDB(ByVal File As FileDB) As Int64
        Get
            Readable.Position = 16 + 44 * IndexOfFile(File) + 32
            Return Readable.ReadInt32
        End Get
        Set(ByVal Value As Int64)
            Writable.Position = 16 + 44 * IndexOfFile(File) + 32
            Writable.WriteInt32(Value)
        End Set
    End Property

    '读取文件在索引中的长度信息，所有索引中的长度信息应该在这里更新
    Public Overrides Property FileLengthInPhysicalFileDB(ByVal File As FileDB) As Int64
        Get
            Readable.Position = 16 + 44 * IndexOfFile(File) + 36
            Return Readable.ReadInt32
        End Get
        Set(ByVal Value As Int64)
            Writable.Position = 16 + 44 * IndexOfFile(File) + 36
            Writable.WriteInt32(Value)
        End Set
    End Property

    '提供文件数据的对齐的计算函数
    Protected Overrides Function GetSpace(ByVal Length As Int64) As Int64
        Return ((Length + &H800 - 1) \ &H800) * &H800
    End Function
End Class

'在提供上述几个函数的基础上，一个简单的提供自动扩展的可读写文件包系统就可以通过PackageManager来打开了
