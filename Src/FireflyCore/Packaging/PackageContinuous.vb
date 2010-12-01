'==========================================================================
'
'  File:        PackageContinuous.vb
'  Location:    Firefly.Packaging <Visual Basic .Net>
'  Description: 连续数据文件包
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Option Compare Text
Imports System
Imports System.IO
Imports System.Collections.Generic
Imports Firefly.Streaming

Namespace Packaging
    ''' <summary>
    ''' 连续数据文件包，通常用于需改变文件索引的文件包，且文件包的数据顺序需要保持和索引一致
    ''' 若无需数据顺序和索引一致，请考虑PackageDiscrete，这通常能够减少修改包所需的时间
    ''' 当前实现假设索引在文件包头部或外部，因此无需备份。如果不是这样，则需要修改某些行为
    ''' 
    ''' 
    ''' 给继承者的说明：
    ''' 
    ''' 文件包支持写入，应
    ''' (1)重写FileLengthInPhysicalFileDB、GetSpace方法
    ''' (2)在加入一个FileDB时，调用PushFile方法，使得它被加入到FileList、IndexOfFile、FileSetAddressSorted中，以及PushFileToDir到根目录FileDB中，若根目录FileDB不存在，则空的根目录会自动创建
    ''' 
    ''' 如果需要启用进度通知功能，请设置委托函数NotifyProgress，参数值在[0, 1]上。
    ''' 
    ''' 请使用PackageRegister来注册文件包类型。
    ''' 应提供一个返回"ISO(*.ISO)|*.ISO"形式字符串的Filter属性，
    ''' 并按照PackageRegister中的委托类型提供一个Open函数、一个Create函数(如果支持创建)。
    ''' </summary>
    Public MustInherit Class PackageContinuous
        Inherits PackageBase

        ''' <summary>按照地址排序的文件集。</summary>
        Protected FileSetAddressSorted As New SortedList(Of FileDB, Int64)(New FileDBAddressComparer(IndexOfFile))
        ''' <summary>设置该委托函数，可以启用进度通知功能，参数值在[0, 1]上。</summary>
        Public NotifyProgress As Action(Of Double)

        ''' <summary>从文件包读取FileDB文件长度和写入文件长度到文件包。用于替换文件包时使用。</summary>
        Public MustOverride Property FileLengthInPhysicalFileDB(ByVal File As FileDB) As Int64

        ''' <summary>
        ''' 返回一个长度的文件所占的空间，通常用于对齐。
        ''' 比如800h对齐的文件，应该返回((Length + 800h - 1) \ 800h) * 800h
        ''' </summary>
        Protected MustOverride Function GetSpace(ByVal Length As Int64) As Int64


        ''' <summary>已重载。默认构照函数。请手动初始化BaseStream。</summary>
        Protected Sub New()
            MyBase.New()
        End Sub
        ''' <summary>已重载。打开文件包。</summary>
        Public Sub New(ByVal sp As NewReadingStreamPasser)
            MyBase.New(sp)
        End Sub
        ''' <summary>已重载。打开或创建文件包。</summary>
        Public Sub New(ByVal sp As NewReadingWritingStreamPasser)
            MyBase.New(sp)
        End Sub

        ''' <summary>把文件FileDB放入根目录FileDB。若根目录FileDB不存在，则空的根目录会自动创建。在加入一个FileDB时，调用该方法，使得它被加入到FileList、IndexOfFile、FileSetAddressSorted中，以及PushFileToDir到根目录FileDB中。</summary>
        Protected Overloads Overrides Sub PushFile(ByVal f As FileDB, ByVal Directory As FileDB)
            MyBase.PushFile(f, Directory)
            FileSetAddressSorted.Add(f, f.Address)
        End Sub

        ''' <summary>已重载。替换包中的一个文件。</summary>
        Protected Overrides Sub ReplaceSingleInner(ByVal File As FileDB, ByVal sp As NewReadingStreamPasser)
            ReplaceMultipleInner(New FileDB() {File}, New NewReadingStreamPasser() {sp})
        End Sub

        Protected Overrides Sub ReplaceMultipleInner(ByVal Files As FileDB(), ByVal StreamPassers As NewReadingStreamPasser())
            Dim FileIndex As New List(Of Integer)
            For Each File As FileDB In Files
                If File.Length <> FileLengthInPhysicalFileDB(File) Then Throw New ArgumentException("PhysicalFileLengthErrorPointing")
                FileIndex.Add(FileSetAddressSorted.IndexOfKey(File))
            Next

            '将数据分成头段、替换段、尾段
            '头段不变，尾段移动，替换段替换
            '寻找替换段起始与结束
            Dim Min As Integer = FileSetAddressSorted.Count - 1
            Dim Max As Integer = 0
            Dim ReplaceBlockLengthList As New SortedList(Of Integer, Int64)
            Dim TotalLengthDiff As Int64 = 0
            For k = 0 To FileIndex.Count - 1
                Dim n = FileIndex(k)
                If n < Min Then Min = n
                If n > Max Then Max = n
                Dim f As FileDB = FileSetAddressSorted.Keys(n)
                Dim fs = StreamPassers(k).GetStream
                ReplaceBlockLengthList.Add(n, fs.Length)
                TotalLengthDiff = TotalLengthDiff - GetSpace(f.Length) + GetSpace(fs.Length)
            Next

            '构建替换文件表与替换段保留文件表
            Dim ReplaceList As New SortedList(Of Integer, FileDB)
            Dim ReplacePathList As New SortedList(Of Integer, NewReadingStreamPasser)
            For k = 0 To FileIndex.Count - 1
                Dim n = FileIndex(k)
                ReplaceList.Add(n, FileSetAddressSorted.Keys(n))
                ReplacePathList.Add(n, StreamPassers(k))
            Next
            Dim PreserveList As New SortedList(Of Integer, FileDB)
            For i = Min To Max
                If ReplaceList.ContainsKey(i) Then Continue For
                ReplaceBlockLengthList.Add(i, FileSetAddressSorted.Keys(i).Length)
                PreserveList.Add(i, FileSetAddressSorted.Keys(i))
            Next

            '构建替换后地址表
            Dim ReplaceBlockStart As Int64 = FileSetAddressSorted.Keys(Min).Address
            Dim TailBlockStart As Int64
            If Max = FileSetAddressSorted.Count - 1 Then
                TailBlockStart = Writable.Length
            Else
                TailBlockStart = FileSetAddressSorted.Keys(Max + 1).Address
            End If
            Dim TailBlockLength As Int64 = Writable.Length - TailBlockStart
            'Dim ReplaceBlockLength As Int64 = TailBlockStart - ReplaceBlockStart

            Dim ReplaceBlockSpaceList As New List(Of Int64)
            For Each v In ReplaceBlockLengthList.Values
                ReplaceBlockSpaceList.Add(GetSpace(v))
            Next
            Dim ReplaceBlockAddressArray = GetSummation(ReplaceBlockStart, ReplaceBlockSpaceList.ToArray)

            '生成保留文件临时文件
            Dim TempFileName = Path.GetTempFileName
            Using TempFile = StreamEx.Create(TempFileName, FileMode.Create)
                For Each Pair In PreserveList
                    With Pair.Value
                        Using f = Readable.Partialize(.Address, .Length)
                            TempFile.WriteFromStream(f, .Length)
                        End Using
                    End With
                Next

                '移动尾段
                If TotalLengthDiff > 0 Then
                    Writable.SetLength(Writable.Length + TotalLengthDiff)
                End If
                MoveData(TailBlockStart, TailBlockLength, TailBlockStart + TotalLengthDiff)
                If TotalLengthDiff < 0 Then
                    Writable.SetLength(Writable.Length + TotalLengthDiff)
                End If

                '更改尾段数据地址
                For i = Max + 1 To FileSetAddressSorted.Count - 1
                    Dim f As FileDB = FileSetAddressSorted.Keys(i)
                    f.Address += TotalLengthDiff
                Next

                '填入保留文件数据
                TempFile.Position = 0
                For Each Pair In PreserveList
                    With Pair.Value
                        Using f = Writable.Partialize(ReplaceBlockAddressArray(Pair.Key - Min), GetSpace(.Length))
                            TempFile.ReadToStream(f, .Length)
                            While f.Position < f.Length
                                f.WriteByte(0)
                            End While
                        End Using
                    End With
                Next
            End Using

            '填入替换文件数据
            For Each Pair In ReplaceList
                Dim p = ReplacePathList(Pair.Key)
                Using f = Writable.Partialize(ReplaceBlockAddressArray(Pair.Key - Min), GetSpace(ReplaceBlockLengthList(Pair.Key)))
                    Dim fs = p.GetStream
                    fs.ReadToStream(f, ReplaceBlockLengthList(Pair.Key))
                    While f.Position < f.Length
                        f.WriteByte(0)
                    End While
                End Using
            Next

            '更改替换段文件数据地址，更改Index中替换文件数据长度
            For i = Min To Max
                Dim f As FileDB = FileSetAddressSorted.Keys(i)
                f.Address = ReplaceBlockAddressArray(i - Min)
                f.Length = ReplaceBlockLengthList(i)
                FileLengthInPhysicalFileDB(f) = f.Length
            Next

            '删除临时文件
            File.Delete(TempFileName)
        End Sub

        ''' <summary>物理移动数据。请勿轻易直接调用。</summary>
        Protected Sub MoveData(ByVal Address As Int64, ByVal Length As Int64, ByVal NewAddress As Int64)
            If NotifyProgress IsNot Nothing Then NotifyProgress(0)

            Dim Diff = NewAddress - Address
            If Diff = 0 Then Return
            If Length = 0 Then Return
            If Length < 0 Then Throw New ArgumentException()

            Dim BufferSize = 4 * (1 << 20)
            Dim Buffer = New Byte(BufferSize - 1) {}

            Dim FirstAddress = Address
            Dim SecondAddress = ((FirstAddress + BufferSize - 1) \ BufferSize) * BufferSize
            Dim FirstLength As Int32 = CInt(SecondAddress - FirstAddress)
            Dim LastLength As Int32 = CInt((Address + Length) Mod BufferSize)
            Dim LastAddress = Address + Length - LastLength

            If FirstAddress >= LastAddress Then
                FirstLength = CInt(Length)
                LastLength = 0
            End If

            If Diff > 0 Then
                If LastLength > 0 Then
                    Writable.Position = LastAddress
                    Writable.Read(Buffer, 0, LastLength)
                    Writable.Position = LastAddress + Diff
                    Writable.Write(Buffer, 0, LastLength)
                End If

                For PieceAddress As Int64 = LastAddress - BufferSize To SecondAddress Step -BufferSize
                    If NotifyProgress IsNot Nothing Then NotifyProgress((LastAddress - PieceAddress) / (LastAddress - SecondAddress))
                    Writable.Position = PieceAddress
                    Writable.Read(Buffer, 0, BufferSize)
                    Writable.Position = PieceAddress + Diff
                    Writable.Write(Buffer, 0, BufferSize)
                Next

                If FirstLength > 0 Then
                    Writable.Position = FirstAddress
                    Writable.Read(Buffer, 0, FirstLength)
                    Writable.Position = FirstAddress + Diff
                    Writable.Write(Buffer, 0, FirstLength)
                End If
            Else
                If FirstLength > 0 Then
                    Writable.Position = FirstAddress
                    Writable.Read(Buffer, 0, FirstLength)
                    Writable.Position = FirstAddress + Diff
                    Writable.Write(Buffer, 0, FirstLength)
                End If

                For PieceAddress As Int64 = SecondAddress To LastAddress - BufferSize Step BufferSize
                    If NotifyProgress IsNot Nothing Then NotifyProgress((PieceAddress - SecondAddress) / (LastAddress - SecondAddress))
                    Writable.Position = PieceAddress
                    Writable.Read(Buffer, 0, BufferSize)
                    Writable.Position = PieceAddress + Diff
                    Writable.Write(Buffer, 0, BufferSize)
                Next

                If LastLength > 0 Then
                    Writable.Position = LastAddress
                    Writable.Read(Buffer, 0, LastLength)
                    Writable.Position = LastAddress + Diff
                    Writable.Write(Buffer, 0, LastLength)
                End If
            End If

            If NotifyProgress IsNot Nothing Then NotifyProgress(1)
        End Sub
    End Class
End Namespace
