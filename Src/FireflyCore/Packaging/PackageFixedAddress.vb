'==========================================================================
'
'  File:        PackageFixedAddress.vb
'  Location:    Firefly.Packaging <Visual Basic .Net>
'  Description: 固定地址文件包
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
    ''' 固定地址文件包，通常用于无需改变文件地址索引的文件包
    ''' 若需要数据顺序和索引一致，请使用PackageContinuous
    ''' 若需要改变文件地址索引，请使用PackageDiscrete
    ''' 若不需要改变文件地址索引，请使用PackageFixed
    ''' 
    ''' 
    ''' 给继承者的说明：
    ''' 
    ''' 如果文件包支持写入，应
    ''' (1)重写FileLengthInPhysicalFileDB方法
    ''' (2)在加入一个FileDB时，调用PushFile方法，使得它被加入到FileList、IndexOfFile、FileSetAddressSorted中，以及PushFileToDir到根目录FileDB中，若根目录FileDB不存在，则空的根目录会自动创建
    ''' 
    ''' 请使用PackageRegister来注册文件包类型。
    ''' 应提供一个返回"ISO(*.ISO)|*.ISO"形式字符串的Filter属性，
    ''' 并按照PackageRegister中的委托类型提供一个Open函数、一个Create函数(如果支持创建)。
    ''' </summary>
    Public MustInherit Class PackageFixedAddress
        Inherits PackageBase

        ''' <summary>按照地址排序的文件集。</summary>
        Protected FileSetAddressSorted As New SortedList(Of FileDB, Int64)(New FileDBAddressComparer(IndexOfFile))

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
        Protected Overrides Sub PushFile(ByVal f As FileDB, ByVal Directory As FileDB)
            MyBase.PushFile(f, Directory)
            FileSetAddressSorted.Add(f, f.Address)
        End Sub

        ''' <summary>从文件包读取FileDB文件长度和写入文件长度到文件包。用于替换文件包时使用。</summary>
        Public MustOverride Property FileLengthInPhysicalFileDB(ByVal File As FileDB) As Int64

        ''' <summary>已重载。替换包中的一个文件。</summary>
        Protected Overrides Sub ReplaceSingleInner(ByVal File As FileDB, ByVal sp As NewReadingStreamPasser)
            Dim s = sp.GetStream

            If FileSetAddressSorted.Count = 0 Then Throw New InvalidOperationException("NullFileSetAddressSorted")

            s.Position = 0
            Dim MaxSize As Int64 = Writable.Length - File.Address
            Dim NextIndex As Integer = FileSetAddressSorted.IndexOfKey(File) + 1
            If NextIndex < FileSetAddressSorted.Count Then
                MaxSize = FileSetAddressSorted.Keys(NextIndex).Address - File.Address
            End If
            If MaxSize < 0 Then Throw New IOException(String.Format("NotEnoughSpace: {0}", File.Name))
            If s.Length > MaxSize Then Throw New IOException(String.Format("NotEnoughSpace: {0}", File.Name))

            If FileLengthInPhysicalFileDB(File) <> File.Length Then Throw New InvalidOperationException(String.Format("OriginalFileLenghtNotMatch: {0}", File.Name))

            Using f = Writable.Partialize(File.Address, MaxSize)
                f.Position = 0
                f.WriteFromStream(s, s.Length)
            End Using

            FileLengthInPhysicalFileDB(File) = s.Length
            File.Length = s.Length
        End Sub
    End Class
End Namespace
