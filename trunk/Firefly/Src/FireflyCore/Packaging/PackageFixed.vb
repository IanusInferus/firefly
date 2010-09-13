'==========================================================================
'
'  File:        PackageFixed.vb
'  Location:    Firefly.Packaging <Visual Basic .Net>
'  Description: 固定索引文件包
'  Version:     2010.05.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Option Compare Text
Imports System
Imports System.IO
Imports System.Collections.Generic

Namespace Packaging
    ''' <summary>
    ''' 固定索引文件包，通常用于无需改变文件索引的文件包
    ''' 若需要数据顺序和索引一致，请使用PackageContinuous
    ''' 若需要改变文件索引，请使用PackageDiscrete
    ''' 若需要改变文件地址索引，请使用PackageFixedAddress
    ''' 
    ''' 
    ''' 给继承者的说明：
    ''' 
    ''' 如果文件包支持写入，应
    ''' (1)在加入一个FileDB时，调用PushFile方法，使得它被加入到FileList、IndexOfFile中，以及PushFileToDir到根目录FileDB中，若根目录FileDB不存在，则空的根目录会自动创建
    ''' 
    ''' 请使用PackageRegister来注册文件包类型。
    ''' 应提供一个返回"ISO(*.ISO)|*.ISO"形式字符串的Filter属性，
    ''' 并按照PackageRegister中的委托类型提供一个Open函数、一个Create函数(如果支持创建)。
    ''' </summary>
    Public MustInherit Class PackageFixed
        Inherits PackageBase

        ''' <summary>已重载。默认构照函数。请手动初始化BaseStream。</summary>
        Protected Sub New()
            MyBase.New()
        End Sub
        ''' <summary>已重载。打开或创建文件包。</summary>
        Public Sub New(ByVal sp As ZeroPositionStreamPasser)
            MyBase.New(sp)
        End Sub

        ''' <summary>已重载。替换包中的一个文件。</summary>
        Protected Overrides Sub ReplaceSingleInner(ByVal File As FileDB, ByVal sp As ZeroPositionStreamPasser)
            Dim s = sp.GetStream
            If s.Length <> File.Length Then Throw New InvalidOperationException("LengthNotMatch")

            Using f As New PartialStreamEx(BaseStream, File.Address, File.Length)
                f.Position = 0
                f.WriteFromStream(s, s.Length)
            End Using
        End Sub
    End Class
End Namespace
