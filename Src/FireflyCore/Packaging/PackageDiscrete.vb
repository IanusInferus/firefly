'==========================================================================
'
'  File:        PackageDiscrete.vb
'  Location:    Firefly.Packaging <Visual Basic .Net>
'  Description: 离散数据文件包
'  Version:     2010.11.30.
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
    ''' 离散数据文件包，通常用于有完整地址索引和长度索引的文件包，能够在文件末尾和文件空白区扩充文件数据
    ''' 若需要数据顺序和索引一致，请使用PackageContinuous，虽然这通常意味着要增加修改包所需的时间
    ''' 
    ''' 
    ''' 给继承者的说明：
    ''' 
    ''' 文件包支持写入，应
    ''' (1)重写FileAddressInPhysicalFileDB、FileLengthInPhysicalFileDB、GetSpace方法
    ''' (2)在加入一个FileDB时，调用PushFile方法，使得它被加入到FileList、IndexOfFile、FileSetAddressSorted中，以及PushFileToDir到根目录FileDB中，若根目录FileDB不存在，则空的根目录会自动创建
    ''' (3)最后执行ScanHoles，扫描出可以放置文件数据的洞
    ''' 
    ''' 请使用PackageRegister来注册文件包类型。
    ''' 应提供一个返回"ISO(*.ISO)|*.ISO"形式字符串的Filter属性，
    ''' 并按照PackageRegister中的委托类型提供一个Open函数、一个Create函数(如果支持创建)。
    ''' </summary>
    Public MustInherit Class PackageDiscrete
        Inherits PackageBase

        ''' <summary>文件包中的数据块(地址->文件数)。</summary>
        Private Blocks As New SortedList(Of Int64, Integer)
        ''' <summary>文件包中的空洞。</summary>
        Private Holes As New SortedList(Of Hole, Integer)(HoleComparer.Default)
        ''' <summary>文件包中的空洞。</summary>
        Private HoleMap As New Dictionary(Of Int64, Hole)

        ''' <summary>从文件包读取FileDB文件地址和写入文件地址到文件包。用于替换文件包时使用。</summary>
        Public MustOverride Property FileAddressInPhysicalFileDB(ByVal File As FileDB) As Int64
        ''' <summary>从文件包读取FileDB文件长度和写入文件长度到文件包。用于替换文件包时使用。</summary>
        Public MustOverride Property FileLengthInPhysicalFileDB(ByVal File As FileDB) As Int64

        ''' <summary>默认的文件数据起始地址</summary>
        Private DataStart As Int64 = 0
        ''' <summary>默认的文件对齐大小</summary>
        Private AlignmentBlockSize As Int64 = 0

        Private Shared Function GCD(ByVal a As Int64, ByVal b As Int64) As Int64
            If a < 0 Then a = -a
            If b < 0 Then b = -b
            If a = 0 Then Return b
            If b = 0 Then Return a

            If a < b Then Exchange(a, b)

            While True
                a = a Mod b
                Exchange(a, b)
                If b = 0 Then Return a
            End While
            Throw New InvalidOperationException
        End Function

        ''' <summary>
        ''' 返回一个地址的对齐位置，相对于开始位置。默认通过GetSpace与ScanHoles传入的DataStart计算。
        ''' </summary>
        Protected Overridable Function GetAlignedAddress(ByVal Address As Int64) As Int64
            Return GetSpace(Address - DataStart) + DataStart
        End Function

        ''' <summary>
        ''' 返回一个长度的文件所占的空间，通常用于对齐。
        ''' 比如800h对齐的文件，应该返回((Length + 800h - 1) \ 800h) * 800h
        ''' </summary>
        Protected Overridable Function GetSpace(ByVal Length As Int64) As Int64
            If AlignmentBlockSize <= 1 Then Return Length
            Return ((Length + AlignmentBlockSize - 1) \ AlignmentBlockSize) * AlignmentBlockSize
        End Function


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

        Private Sub SetBlockFileCount(ByVal Address As Int64, ByVal FileCount As Integer)
            Dim OldFileCount As Integer = Blocks(Address)
            If FileCount < 0 Then Throw New ArgumentOutOfRangeException
            If OldFileCount < 0 Then Throw New InvalidOperationException
            If OldFileCount = 0 AndAlso FileCount > 0 Then
                If HoleMap.ContainsKey(Address) Then
                    Dim Hole = HoleMap(Address)
                    Holes.Remove(Hole)
                    HoleMap.Remove(Address)
                End If
            ElseIf OldFileCount > 0 AndAlso FileCount = 0 Then
                Dim EndAddress = GetAlignedAddress(Writable.Length)
                Dim i = Blocks.IndexOfKey(Address)
                If i + 1 < Blocks.Count Then EndAddress = Blocks.Keys(i + 1)
                If EndAddress > Address Then
                    Dim Hole As New Hole With {.Address = Address, .Length = EndAddress - Address}
                    Holes.Add(Hole, 0)
                    HoleMap.Add(Address, Hole)
                End If
            End If
            Blocks(Address) = FileCount
        End Sub

        Private Sub SplitBlockAt(ByVal Address As Int64)
            Blocks.Add(Address, 0)
            Dim i = Blocks.IndexOfKey(Address)
            If i - 1 < 0 Then Throw New InvalidOperationException
            Dim PreviousAddress = Blocks.Keys(i - 1)
            Dim FileCount = Blocks(PreviousAddress)
            Blocks(Address) = FileCount
            If FileCount < 0 Then Throw New InvalidOperationException
            If FileCount = 0 Then
                If HoleMap.ContainsKey(PreviousAddress) Then
                    Dim PreviousHole = HoleMap(PreviousAddress)
                    Holes.Remove(PreviousHole)
                    HoleMap.Remove(PreviousAddress)
                End If
                Dim NewPreviousHole As New Hole With {.Address = PreviousAddress, .Length = Address - PreviousAddress}
                Holes.Add(NewPreviousHole, 0)
                HoleMap.Add(NewPreviousHole.Address, NewPreviousHole)
                Dim NewHole As New Hole With {.Address = Address, .Length = GetAlignedAddress(Writable.Length) - Address}
                Holes.Add(NewHole, 0)
                HoleMap.Add(NewHole.Address, NewHole)
            End If
        End Sub

        Private Sub CombineHoleAt(ByVal Address As Int64)
            Dim FileCount = Blocks(Address)
            If FileCount > 0 Then Return
            If FileCount < 0 Then Throw New InvalidOperationException
            Dim i = Blocks.IndexOfKey(Address)
            If i - 1 < 0 Then Return
            Dim PreviousAddress = Blocks.Keys(i - 1)
            Dim PreviousFileCount = Blocks(PreviousAddress)
            If PreviousFileCount > 0 Then Return
            If PreviousFileCount < 0 Then Throw New InvalidOperationException

            Dim PreviousHole = HoleMap(PreviousAddress)
            Holes.Remove(PreviousHole)
            HoleMap.Remove(PreviousAddress)
            If i < Blocks.Count - 1 Then
                Dim Hole = HoleMap(Address)
                Holes.Remove(Hole)
                HoleMap.Remove(Address)
                Dim NewHole As New Hole With {.Address = PreviousAddress, .Length = PreviousHole.Length + Hole.Length}
                Holes.Add(NewHole, 0)
                HoleMap.Add(PreviousAddress, NewHole)
            End If
            Blocks.Remove(Address)
        End Sub

        ''' <summary>在增加文件时用于更新文件包占用数据块信息。</summary>
        Protected Sub AddFileToBlocks(ByVal File As FileDB)
            Dim EndAddress = File.Address + GetSpace(File.Length)
            If File.Address >= EndAddress Then Return
            If Not Blocks.ContainsKey(File.Address) Then
                SplitBlockAt(File.Address)
            End If
            Dim i As Integer
            For i = Blocks.IndexOfKey(File.Address) To Blocks.Count - 1
                Dim CurrentBlockAddress = Blocks.Keys(i)
                If EndAddress <= CurrentBlockAddress Then Exit For
                SetBlockFileCount(CurrentBlockAddress, Blocks(CurrentBlockAddress) + 1)
            Next
            If i >= Blocks.Count OrElse EndAddress <> Blocks.Keys(i) Then
                SplitBlockAt(EndAddress)
                SetBlockFileCount(Blocks.Keys(i), Blocks(Blocks.Keys(i)) - 1)
            End If
        End Sub

        ''' <summary>在去除文件时用于更新文件包占用数据块信息。</summary>
        Protected Sub RemoveFileFromBlocks(ByVal File As FileDB)
            Dim EndAddress = File.Address + GetSpace(File.Length)
            If File.Address >= EndAddress Then Return
            Dim i As Integer
            For i = Blocks.IndexOfKey(File.Address) To Blocks.Count - 1
                Dim CurrentBlockAddress = Blocks.Keys(i)
                If EndAddress <= CurrentBlockAddress Then Exit For
                SetBlockFileCount(CurrentBlockAddress, Blocks(CurrentBlockAddress) - 1)
                If Blocks(CurrentBlockAddress) = 0 Then
                    '清空原始数据
                    Dim Offset = CurrentBlockAddress - File.Address
                    Using s = Writable.Partialize(Blocks.Keys(i), Min(GetSpace(File.Length) - Offset, Writable.Length - CurrentBlockAddress))
                        s.Position = 0
                        For n = 0 To File.Length - 1
                            s.WriteByte(0)
                        Next
                    End Using
                End If
            Next
            If i >= Blocks.Count OrElse EndAddress <> Blocks.Keys(i) Then Throw New InvalidOperationException
            CombineHoleAt(File.Address)
            CombineHoleAt(EndAddress)
        End Sub

        ''' <summary>扫描洞。</summary>
        Protected Sub ScanHoles(ByVal DataStart As Int64)
            Me.DataStart = DataStart
            Blocks.Add(DataStart, 0)
            Dim InitialHole As New Hole With {.Address = DataStart, .Length = GetAlignedAddress(Readable.Length) - DataStart}
            Holes.Add(InitialHole, 0)
            HoleMap.Add(InitialHole.Address, InitialHole)
            For Each f In FileList
                AlignmentBlockSize = GCD(f.Address, AlignmentBlockSize - DataStart)
            Next
            For Each f In FileList
                If f.Address < DataStart Then
                    If f.Address + f.Length > DataStart Then Throw New InvalidOperationException
                    Continue For
                End If
                AddFileToBlocks(f)
            Next
        End Sub

        ''' <summary>已重载。替换包中的一个文件。</summary>
        Protected Overrides Sub ReplaceSingleInner(ByVal File As FileDB, ByVal sp As NewReadingStreamPasser)
            Dim s = sp.GetStream

            If File.Address <> FileAddressInPhysicalFileDB(File) Then Throw New ArgumentException("PhysicalFileAddressErrorPointing")
            If File.Length <> FileLengthInPhysicalFileDB(File) Then Throw New ArgumentException("PhysicalFileLengthErrorPointing")

            Dim EndAddress = File.Address + GetSpace(File.Length)
            Dim MaxSize As Int64 = 0
            Dim IndexOfAddress = Blocks.IndexOfKey(File.Address)
            If IndexOfAddress >= 0 Then
                Dim i As Integer
                For i = IndexOfAddress To Blocks.Count - 2
                    Dim CurrentBlockAddress = Blocks.Keys(i)
                    If EndAddress <= CurrentBlockAddress Then Exit For
                    Dim FileCount = Blocks(CurrentBlockAddress)
                    If FileCount = 1 Then MaxSize = Blocks.Keys(i + 1) - File.Address
                Next
                If i >= Blocks.Count - 1 Then MaxSize = Int64.MaxValue
            End If

            Dim Hole As Hole = Nothing

            '如果可能，则原位导入
            If s.Length <= MaxSize Then Hole = New Hole With {.Address = File.Address, .Length = GetSpace(s.Length)}

            '如果不能原位导入，则寻找洞
            If Hole Is Nothing Then
                For Each h In Holes
                    If s.Length <= h.Key.Length Then
                        Hole = h.Key
                        Exit For
                    End If
                Next
            End If

            '如果需要超过长度，则扩展空间
            Dim Address = Blocks.Keys(Blocks.Count - 1)
            If Blocks(Address) <> 0 Then Throw New InvalidOperationException
            If Hole IsNot Nothing Then
                If Hole.Address + Hole.Length >= Address Then
                    Writable.SetLength(Hole.Address + Hole.Length)
                End If
            Else
                Dim Length = GetSpace(s.Length)
                Writable.SetLength(Address + Length)
                Hole = New Hole With {.Address = Address, .Length = Length}
            End If

            '此时空间足够，改变文件中存储的结构
            FileLengthInPhysicalFileDB(File) = s.Length
            If Hole.Address <> File.Address Then FileAddressInPhysicalFileDB(File) = Hole.Address

            '更新空洞信息，改变文件信息
            RemoveFileFromBlocks(File)
            File.Address = Hole.Address
            File.Length = s.Length
            AddFileToBlocks(File)

            '改变文件数据
            Using f = Writable.Partialize(Hole.Address, Hole.Length)
                f.Position = 0
                f.WriteFromStream(s, s.Length)
                For n = s.Length To f.Length - 1
                    f.WriteByte(0)
                Next
            End Using
        End Sub
    End Class

    ''' <summary>洞</summary>
    Public Class Hole
        Public Address As Int64
        Public Length As Int64
    End Class

    ''' <summary>洞地址比较器</summary>
    Public Class HoleComparer
        Implements IComparer(Of Hole)
        Public Shared ReadOnly Property [Default]() As HoleComparer
            Get
                Static c As New HoleComparer
                Return c
            End Get
        End Property
        Public Function Compare(ByVal x As Hole, ByVal y As Hole) As Integer Implements IComparer(Of Hole).Compare
            If x.Length < y.Length Then Return -1
            If x.Length > y.Length Then Return 1
            If x.Address < y.Address Then Return -1
            If x.Address > y.Address Then Return 1
            Return 0
        End Function
    End Class
End Namespace
