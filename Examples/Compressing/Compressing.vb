'==========================================================================
'
'  File:        Compressing.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 文件压缩解压示例
'  Version:     2009.11.08.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics.Debug
Imports Firefly
Imports Firefly.Compressing

Public Module Test
    Public Sub TestStreamExStreamReadWrite()
        Dim a As New StreamEx
        For n = 0 To 15 * (1 << 20) - 1
            a.WriteByte((n * 7 + 13) And &HFF)
        Next
        a.Position = 0
        Dim b As New StreamEx
        b.WriteFromStream(a, 15 * (1 << 20))
        b.Position = 0
        For n = 0 To 15 * (1 << 20) - 1
            Assert(b.ReadByte = ((n * 7 + 13) And &HFF))
        Next
        b.Position = 0
        a.Position = 0
        b.ReadToStream(a, 15 * (1 << 20))
        a.Position = 0
        For n = 0 To 15 * (1 << 20) - 1
            Assert(a.ReadByte = ((n * 7 + 13) And &HFF))
        Next
        b.Close()
        a.Close()
    End Sub

    Public Sub TestStringEx()
        Dim a As New StringEx(Of Byte)(New Byte() {1, 2, 3, 4, 5, 6})
        Dim b As New StringEx(Of Byte)(New Byte() {1, 2, 4, 3, 5, 6})
        Dim c As New StringEx(Of Byte)(New Byte() {1, 2, 4, 3, 5, 6})
        Assert(a < b)
        Assert(b = c)
        Assert(b.GetHashCode = c.GetHashCode)
        Dim d As New StringEx(Of Byte)(New Byte() {4, 3, 5})
        Assert(b.IndexOf(d) = 2)
        Assert(b.LastIndexOf(d) = 2)
    End Sub

    Public Function ZeroArrayGen() As Byte()
        System.Diagnostics.Debug.WriteLine("零数据")
        Dim Data = New Byte(65535) {}
        Return Data
    End Function

    Public Function FixedArrayGen() As Byte()
        System.Diagnostics.Debug.WriteLine("0-255循环数据")
        Dim Data = New Byte(65535) {}
        For n = 0 To Data.Length - 1
            Data(n) = n And &HFF
        Next
        Return Data
    End Function

    Public Function RandomArrayGen() As Byte()
        System.Diagnostics.Debug.WriteLine("随机数据")
        Dim Data = New Byte(65535) {}
        Dim r As New Random
        r.NextBytes(Data)
        Return Data
    End Function

    Public Delegate Function Method() As Byte()

    Public Function Methods() As Method()
        Return New Method() {AddressOf ZeroArrayGen, AddressOf FixedArrayGen, AddressOf RandomArrayGen}
    End Function

    Public Sub TestLZ77(ByVal Data As Byte())
        Dim time = System.Environment.TickCount
        Dim lz As New LZ77(Data.Clone, 1024, 10, 2, 2)
        Dim MatchSeq As New Queue(Of Pointer)
        While lz.Position < Data.Length
            Dim m = lz.Match()
            If m Is Nothing OrElse m.Length <= 1 Then
                MatchSeq.Enqueue(New Literal())
                lz.Proceed()
            Else
                MatchSeq.Enqueue(m)
                lz.Proceed(m.Length)
            End If
        End While

        Dim NewData = New Byte(Data.Length - 1) {}
        Dim p = 0
        Dim CompressionLength = 0
        While MatchSeq.Count > 0
            Dim m = MatchSeq.Dequeue
            If TypeOf m Is Literal Then
                NewData(p) = Data(p)
                p += 1
                CompressionLength += 2
            Else
                For i = 0 To m.Length - 1
                    NewData(p) = NewData(p - CType(m, LZ77.LZPointer).NumBack)
                    p += 1
                Next
                CompressionLength += 3
            End If
        End While

        System.Diagnostics.Debug.WriteLine(CompressionLength)

        For n = 0 To Data.Length - 1
            Assert(Data(n) = NewData(n))
        Next
        System.Diagnostics.Debug.WriteLine(System.Environment.TickCount - time)
    End Sub

    Public Sub TestLZ77Reversed(ByVal Data As Byte())
        Dim time = System.Environment.TickCount
        Dim LZ As New LZ77Reversed(Data.Clone, 1024, 10, 2, 2)
        Dim States As New LinkedList(Of LZ77Reversed.AccPointer)
        Dim PreviousStateAccLength As Integer = 0
        While LZ.Position >= 0
            If LZ.Position <= 2000 Then Stop
            Dim m = LZ.Match(States)
            If m Is Nothing OrElse m.AccLength + 3 >= PreviousStateAccLength + 2 Then
                States.AddFirst(New LZ77Reversed.Literal(PreviousStateAccLength + 2))
            Else
                m.AccLength += 3
                States.AddFirst(m)
            End If
            PreviousStateAccLength = States.First.Value.AccLength

            LZ.Proceed()
        End While

        Dim MatchSeq As New Queue(Of LZ77Reversed.AccPointer)

        Dim Holds = 0
        For Each m In States
            If Holds = 0 Then
                MatchSeq.Enqueue(m)
                If Not TypeOf m Is LZ77Reversed.Literal Then
                    Holds = m.Length - 1
                End If
            Else
                Holds -= 1
            End If
        Next

        States = Nothing

        Dim NewData = New Byte(Data.Length - 1) {}
        Dim p = 0
        Dim CompressionLength = 0
        While MatchSeq.Count > 0
            Dim m = MatchSeq.Dequeue
            If TypeOf m Is LZ77Reversed.Literal Then
                NewData(p) = Data(p)
                p += 1
                CompressionLength += 2
            Else
                For i = 0 To m.Length - 1
                    NewData(p) = NewData(p - CType(m, LZ77Reversed.LZPointer).NumBack)
                    Assert(Data(p) = NewData(p))
                    p += 1
                Next
                CompressionLength += 3
            End If
        End While

        If CompressionLength > Data.Length * 2 Then Stop

        System.Diagnostics.Debug.WriteLine(CompressionLength)

        For n = 0 To Data.Length - 1
            Assert(Data(n) = NewData(n))
        Next
        System.Diagnostics.Debug.WriteLine(System.Environment.TickCount - time)
    End Sub

    Public Sub Main()
        'TestStreamExStreamReadWrite()
        TestStringEx()
        For Each Method In Methods()
            Dim Data = Method()
            If Data Is Nothing Then Continue For
            TestLZ77(Data)
            TestLZ77Reversed(Data)
        Next
    End Sub
End Module
