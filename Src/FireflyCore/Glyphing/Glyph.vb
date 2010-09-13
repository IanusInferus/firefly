'==========================================================================
'
'  File:        Glyph.vb
'  Location:    Firefly.Glyphing <Visual Basic .Net>
'  Description: 字形信息
'  Version:     2009.11.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Drawing
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Imaging

Namespace Glyphing
    ''' <summary>字形描述信息</summary>
    Public NotInheritable Class GlyphDescriptor
        ''' <summary>字符信息</summary>
        Public c As StringCode

        ''' <summary>物理包围盒，字符的图片信息在此包围盒内。</summary>
        Public PhysicalBox As Rectangle

        ''' <summary>虚拟包围盒，字符的显示相对位置为此包围盒。</summary>
        Public VirtualBox As Rectangle
    End Class

    ''' <summary>字形信息接口</summary>
    Public Interface IGlyph
        ''' <summary>字符信息</summary>
        ReadOnly Property c() As StringCode

        ''' <summary>字符的32位颜色数据。</summary>
        ReadOnly Property Block() As Int32(,)

        ''' <summary>字符的宽度。</summary>
        ReadOnly Property PhysicalWidth() As Integer

        ''' <summary>字符的高度。</summary>
        ReadOnly Property PhysicalHeight() As Integer

        ''' <summary>虚拟包围盒，字符的显示相对位置为此包围盒。</summary>
        ReadOnly Property VirtualBox() As Rectangle
    End Interface

    ''' <summary>字形提供器接口</summary>
    Public Interface IGlyphProvider
        Inherits IDisposable

        ''' <summary>物理宽度</summary>
        ReadOnly Property PhysicalWidth() As Integer

        ''' <summary>物理高度</summary>
        ReadOnly Property PhysicalHeight() As Integer

        ''' <summary>获取字形</summary>
        Function GetGlyph(ByVal c As StringCode) As IGlyph
    End Interface

    ''' <summary>字形信息</summary>
    Public NotInheritable Class Glyph
        Implements IGlyph

        ''' <summary>字符信息</summary>
        Public c As StringCode

        ''' <summary>字符的32位颜色数据。</summary>
        Public Block As Int32(,)

        ''' <summary>字符的宽度。</summary>
        Public ReadOnly Property PhysicalWidth() As Integer Implements IGlyph.PhysicalWidth
            Get
                Return Block.GetLength(0)
            End Get
        End Property

        ''' <summary>字符的高度。</summary>
        Public ReadOnly Property PhysicalHeight() As Integer Implements IGlyph.PhysicalHeight
            Get
                Return Block.GetLength(1)
            End Get
        End Property

        ''' <summary>虚拟包围盒，字符的显示相对位置为此包围盒。</summary>
        Public VirtualBox As Rectangle

        Private ReadOnly Property cI() As TextEncoding.StringCode Implements IGlyph.c
            Get
                Return c
            End Get
        End Property

        Private ReadOnly Property BlockI() As Integer(,) Implements IGlyph.Block
            Get
                Return Block
            End Get
        End Property

        Private ReadOnly Property VirtualBoxI() As System.Drawing.Rectangle Implements IGlyph.VirtualBox
            Get
                Return VirtualBox
            End Get
        End Property
    End Class

    ''' <summary>位于位图中的字形信息</summary>
    Public NotInheritable Class BmpGlyph
        Implements IGlyph

        ''' <summary>位图。</summary>
        Public Bmp As Bmp

        ''' <summary>字形描述信息</summary>
        Public GlyphDescriptor As GlyphDescriptor

        ''' <summary>字符信息</summary>
        Public ReadOnly Property c() As TextEncoding.StringCode Implements IGlyph.c
            Get
                Return GlyphDescriptor.c
            End Get
        End Property

        ''' <summary>字符的32位颜色数据。</summary>
        Private ReadOnly Property Block() As Integer(,) Implements IGlyph.Block
            Get
                Dim pb = GlyphDescriptor.PhysicalBox
                Return Bmp.GetRectangleAsARGB(pb.X, pb.Y, pb.Width, pb.Height)
            End Get
        End Property

        ''' <summary>字符的宽度。</summary>
        Public ReadOnly Property PhysicalWidth() As Integer Implements IGlyph.PhysicalWidth
            Get
                Return GlyphDescriptor.PhysicalBox.Width
            End Get
        End Property

        ''' <summary>字符的高度。</summary>
        Public ReadOnly Property PhysicalHeight() As Integer Implements IGlyph.PhysicalHeight
            Get
                Return GlyphDescriptor.PhysicalBox.Height
            End Get
        End Property

        ''' <summary>虚拟包围盒，字符的显示相对位置为此包围盒。</summary>
        Private ReadOnly Property VirtualBoxI() As System.Drawing.Rectangle Implements IGlyph.VirtualBox
            Get
                Return GlyphDescriptor.VirtualBox
            End Get
        End Property
    End Class
End Namespace
