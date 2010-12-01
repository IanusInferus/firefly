'==========================================================================
'
'  File:        LXB.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: Kung Fu Panda lxb文件格式
'  Version:     2010.12.01.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.TextEncoding

Public NotInheritable Class LXB
    Private Sub New()
    End Sub

    Public Shared Function Read(ByVal Path As String) As KeyValuePair(Of Int32, String)()
        Using s = StreamEx.CreateReadable(Path, FileMode.Open)
            Return Read(s.AsNewReading)
        End Using
    End Function

    Public Shared Sub Write(ByVal Path As String, ByVal Text As IEnumerable(Of KeyValuePair(Of Int32, String)))
        Using s = StreamEx.Create(Path, FileMode.Create)
            Write(s.AsNewWriting, Text)
        End Using
    End Sub

    Public Shared Function Read(ByVal sp As NewReadingStreamPasser) As KeyValuePair(Of Int32, String)()
        Dim s = sp.GetStream

        s.Position = &H7C
        Dim Num As Int32 = s.ReadInt32
        Dim Key = New Int32(Num - 1) {}
        Dim Address = New Int32(Num - 1) {}
        For n = 0 To Num - 1
            Key(n) = s.ReadInt32
            Address(n) = s.Position
            Address(n) += s.ReadInt32
        Next

        Dim Encoding = UTF8
        Dim Text = New KeyValuePair(Of Int32, String)(Num - 1) {}
        For n = 0 To Num - 1
            Dim TextBytes As New List(Of Byte)
            s.Position = Address(n)
            While s.Position < s.Length
                Dim b = s.ReadByte
                If b = 0 Then Exit While
                TextBytes.Add(b)
            End While
            Text(n) = New KeyValuePair(Of Int32, String)(Key(n), Encoding.GetChars(TextBytes.ToArray))
        Next
        Return Text
    End Function

    Public Shared Sub Write(ByVal sp As NewWritingStreamPasser, ByVal Text As IEnumerable(Of KeyValuePair(Of Int32, String)))
        Dim s = sp.GetStream

        s.Position = 0
        Dim Header = New Byte() {&H5, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H1, &H0, &H0, &H0, &HC7, &HA7, &H8B, &H3B, &H18, &H0, &H0, &H0, &H78, &H0, &H0, &H0, &H9, &H0, &H0, &H0, &H8, &H0, &H0, &H0, &HC, &H0, &H0, &H0, &HC, &H0, &H0, &H0, &HCF, &H2C, &H42, &HEF, &HC, &H0, &H0, &H0, &H6, &H0, &H0, &H0, &H2, &H0, &H0, &H0, &HA9, &HAB, &H90, &H8A, &H20, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &HC7, &HA7, &H8B, &H3B, &H3C, &H0, &H0, &H0, &H4, &H0, &H0, &H0, &HC, &H0, &H0, &H0, &H5E, &HD3, &HAB, &HAF, &HC, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &HFF, &HFF, &HFF, &HFF, &H4, &H0, &H0, &H0, &H4, &H0, &H0, &H0, &HC7, &HA7, &H8B, &H3B, &HA8, &HFF, &HFF, &HFF, &H8, &H0, &H0, &H0}
        Dim Num As Int32 = Text.Count
        s.Write(Header)
        s.WriteInt32(Num)

        Dim Address = New Int32(Num - 1) {}
        s.Position = 128 + Num * 8
        For n = 0 To Num - 1
            Address(n) = s.Position
            s.Write(UTF8.GetBytes(Text(n).Value))
            s.WriteByte(0)
        Next

        Dim EndPosition = s.Position
        s.Position = 128
        For n = 0 To Num - 1
            s.WriteInt32(Text(n).Key)
            s.WriteInt32(Address(n) - s.Position)
        Next
    End Sub
End Class
