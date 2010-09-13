'==========================================================================
'
'  File:        GlyphGenerator.vb
'  Location:    Firefly.Glyphing <Visual Basic .Net>
'  Description: 字形生成器
'  Version:     2010.09.10.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Linq
Imports System.Drawing
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Imaging

Namespace Glyphing

    ''' <summary>通道类型</summary>
    Public Enum ChannelPattern
        Zero
        Draw
        One
    End Enum

    ''' <summary>字形生成器</summary>
    Public Class GlyphGenerator
        Implements IGlyphProvider

        Private PhysicalWidthValue As Integer
        Private PhysicalHeightValue As Integer
        Public ReadOnly Property PhysicalWidth() As Integer Implements IGlyphProvider.PhysicalWidth
            Get
                Return PhysicalWidthValue
            End Get
        End Property
        Public ReadOnly Property PhysicalHeight() As Integer Implements IGlyphProvider.PhysicalHeight
            Get
                Return PhysicalHeightValue
            End Get
        End Property
        Private DrawOffsetX As Integer
        Private DrawOffsetY As Integer
        Private VirtualOffsetX As Integer
        Private VirtualOffsetY As Integer
        Private VirtualDeltaWidth As Integer
        Private VirtualDeltaHeight As Integer
        Private AnchorLeft As Boolean
        Private ChannelPatterns As ChannelPattern()

        Private Font As Font
        Private GlyphPiece As Bitmap
        Private g As Graphics

        Public Sub New(ByVal FontName As String, ByVal FontStyle As FontStyle, ByVal FontSize As Integer, ByVal PhysicalWidth As Integer, ByVal PhysicalHeight As Integer, ByVal DrawOffsetX As Integer, ByVal DrawOffsetY As Integer, ByVal VirtualOffsetX As Integer, ByVal VirtualOffsetY As Integer, ByVal VirtualDeltaWidth As Integer, ByVal VirtualDeltaHeight As Integer, ByVal AnchorLeft As Boolean, ByVal ChannelPatterns As ChannelPattern())
            If FontSize <= 0 Then Throw New ArgumentOutOfRangeException
            If PhysicalWidth <= 0 Then Throw New ArgumentOutOfRangeException
            If PhysicalHeight <= 0 Then Throw New ArgumentOutOfRangeException
            If ChannelPatterns.Length <> 4 Then Throw New ArgumentException

            Me.PhysicalWidthValue = PhysicalWidth
            Me.PhysicalHeightValue = PhysicalHeight
            Me.DrawOffsetX = DrawOffsetX
            Me.DrawOffsetY = DrawOffsetY
            Me.VirtualOffsetX = VirtualOffsetX
            Me.VirtualOffsetY = VirtualOffsetY
            Me.VirtualDeltaWidth = VirtualDeltaWidth
            Me.VirtualDeltaHeight = VirtualDeltaHeight
            Me.AnchorLeft = AnchorLeft
            Me.ChannelPatterns = ChannelPatterns

            Font = New Font(FontName, FontSize, FontStyle, GraphicsUnit.Pixel)
            GlyphPiece = New Bitmap(PhysicalWidth, PhysicalHeight, Drawing.Imaging.PixelFormat.Format32bppArgb)
            g = Graphics.FromImage(GlyphPiece)
        End Sub

        Public Function GetGlyph(ByVal c As StringCode) As IGlyph Implements IGlyphProvider.GetGlyph
            Dim Block = New Integer(PhysicalWidthValue - 1, PhysicalHeightValue - 1) {}
            If Not c.HasUnicodes Then
                For ry = 0 To PhysicalHeightValue - 1
                    For rx = 0 To PhysicalWidthValue - 1
                        Block(rx, ry) = 0
                    Next
                Next
                Return New Glyph With {.c = c, .Block = Block, .VirtualBox = New Rectangle(0, 0, PhysicalWidthValue, PhysicalHeightValue)}
            End If

            Dim GetGray = Function(ARGB) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)

            Dim DrawedRectangle = g.MeasureStringRectangle(c.UnicodeString, Font)
            Dim X As Integer = Round(DrawedRectangle.Left)
            Dim Y As Integer = Round(DrawedRectangle.Top)
            Dim X2 As Integer = Round(DrawedRectangle.Right)
            Dim Y2 As Integer = Round(DrawedRectangle.Bottom)
            Dim Width As Integer = X2 - X
            Dim Height As Integer = Y2 - Y
            Dim ox As Integer = (PhysicalWidthValue - Width) \ 2
            Dim oy As Integer = (PhysicalHeightValue - Height) \ 2
            If AnchorLeft Then ox = 0

            g.Clear(Color.Black)
            g.DrawString(c.UnicodeString, Font, Brushes.White, ox - X + DrawOffsetX, oy - Y + DrawOffsetY)
            Dim r = GlyphPiece.GetRectangle(0, 0, PhysicalWidthValue, PhysicalHeightValue)
            For ry = 0 To PhysicalHeightValue - 1
                For rx = 0 To PhysicalWidthValue - 1
                    Dim L = GetGray(r(rx, ry))
                    Dim ARGB = ConcatBits(GetChannel(ChannelPatterns(0), L), 8, GetChannel(ChannelPatterns(1), L), 8, GetChannel(ChannelPatterns(2), L), 8, GetChannel(ChannelPatterns(3), L), 8)
                    Block(rx, ry) = ARGB
                Next
            Next

            Dim VirtualRectangleX As Integer = ox - VirtualDeltaWidth \ 2 + VirtualOffsetX
            Dim VirtualRectangleY As Integer = oy - VirtualDeltaHeight \ 2 + VirtualOffsetY
            Dim VirtualRectangleX2 As Integer = VirtualRectangleX + Width + VirtualDeltaWidth
            Dim VirtualRectangleY2 As Integer = VirtualRectangleY + Height + VirtualDeltaHeight
            If VirtualRectangleX < 0 Then VirtualRectangleX = 0
            If VirtualRectangleY < 0 Then VirtualRectangleY = 0
            If VirtualRectangleX2 > PhysicalWidthValue Then VirtualRectangleX2 = PhysicalWidthValue
            If VirtualRectangleY2 > PhysicalHeightValue Then VirtualRectangleY2 = PhysicalHeightValue
            Return New Glyph With {.c = c, .Block = Block, .VirtualBox = New Rectangle(VirtualRectangleX, VirtualRectangleY, VirtualRectangleX2 - VirtualRectangleX, VirtualRectangleY2 - VirtualRectangleY)}
        End Function

        Private Shared Function GetChannel(ByVal Pattern As ChannelPattern, ByVal L As Byte) As Byte
            Select Case Pattern
                Case ChannelPattern.Zero
                    Return 0
                Case ChannelPattern.Draw
                    Return L
                Case ChannelPattern.One
                    Return &HFF
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

#Region " IDisposable 支持 "
        Private DisposedValue As Boolean = False '检测冗余的调用
        ''' <summary>释放流的资源。</summary>
        ''' <remarks>对继承者的说明：不要调用基类的Dispose()，而应调用Dispose(True)，否则会出现无限递归。</remarks>
        Private Sub Dispose(ByVal Disposing As Boolean)
            If DisposedValue Then Return
            If Disposing Then
                '释放其他状态(托管对象)。
                If Font IsNot Nothing Then Font.Dispose()
                If GlyphPiece IsNot Nothing Then GlyphPiece.Dispose()
                If g IsNot Nothing Then g.Dispose()
            End If

            '释放您自己的状态(非托管对象)。
            '将大型字段设置为 null。
            DisposedValue = True
        End Sub
        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean) 中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class

    ''' <summary>两倍超采样字形生成器</summary>
    Public Class GlyphGeneratorDoubleSample
        Implements IGlyphProvider

        Private PhysicalWidthValue As Integer
        Private PhysicalHeightValue As Integer
        Public ReadOnly Property PhysicalWidth() As Integer Implements IGlyphProvider.PhysicalWidth
            Get
                Return PhysicalWidthValue
            End Get
        End Property
        Public ReadOnly Property PhysicalHeight() As Integer Implements IGlyphProvider.PhysicalHeight
            Get
                Return PhysicalHeightValue
            End Get
        End Property
        Private DrawOffsetX As Integer
        Private DrawOffsetY As Integer
        Private VirtualOffsetX As Integer
        Private VirtualOffsetY As Integer
        Private VirtualDeltaWidth As Integer
        Private VirtualDeltaHeight As Integer
        Private AnchorLeft As Boolean
        Private ChannelPatterns As ChannelPattern()

        Private Font As Font
        Private GlyphPiece As Bitmap
        Private g As Graphics

        Public Sub New(ByVal FontName As String, ByVal FontStyle As FontStyle, ByVal FontSize As Integer, ByVal PhysicalWidth As Integer, ByVal PhysicalHeight As Integer, ByVal DrawOffsetX As Integer, ByVal DrawOffsetY As Integer, ByVal VirtualOffsetX As Integer, ByVal VirtualOffsetY As Integer, ByVal VirtualDeltaWidth As Integer, ByVal VirtualDeltaHeight As Integer, ByVal AnchorLeft As Boolean, ByVal ChannelPatterns As ChannelPattern())
            If FontSize <= 0 Then Throw New ArgumentOutOfRangeException
            If PhysicalWidth <= 0 Then Throw New ArgumentOutOfRangeException
            If PhysicalHeight <= 0 Then Throw New ArgumentOutOfRangeException
            If ChannelPatterns.Length <> 4 Then Throw New ArgumentException

            Me.PhysicalWidthValue = PhysicalWidth
            Me.PhysicalHeightValue = PhysicalHeight
            Me.DrawOffsetX = DrawOffsetX
            Me.DrawOffsetY = DrawOffsetY
            Me.VirtualOffsetX = VirtualOffsetX
            Me.VirtualOffsetY = VirtualOffsetY
            Me.VirtualDeltaWidth = VirtualDeltaWidth
            Me.VirtualDeltaHeight = VirtualDeltaHeight
            Me.AnchorLeft = AnchorLeft
            Me.ChannelPatterns = ChannelPatterns

            Font = New Font(FontName, FontSize * 2, FontStyle, GraphicsUnit.Pixel)
            GlyphPiece = New Bitmap(PhysicalWidth * 2, PhysicalHeight * 2, Drawing.Imaging.PixelFormat.Format32bppArgb)
            g = Graphics.FromImage(GlyphPiece)
        End Sub

        Private Shared Function Mix(ByVal a1 As Integer, ByVal a2 As Integer, ByVal a3 As Integer, ByVal a4 As Integer) As Integer
            Dim s = (From a In New Integer() {a1, a2, a3, a4} Order By a).ToArray
            Return (s(0) * 1 + s(1) * 3 + s(2) * 5 + s(3) * 7 + 15) \ 16
        End Function

        Public Function GetGlyph(ByVal c As StringCode) As IGlyph Implements IGlyphProvider.GetGlyph
            Dim Block = New Integer(PhysicalWidthValue - 1, PhysicalHeightValue - 1) {}
            If Not c.HasUnicodes Then
                For ry = 0 To PhysicalHeightValue - 1
                    For rx = 0 To PhysicalWidthValue - 1
                        Block(rx, ry) = 0
                    Next
                Next
                Return New Glyph With {.c = c, .Block = Block, .VirtualBox = New Rectangle(0, 0, PhysicalWidthValue, PhysicalHeightValue)}
            End If

            Dim GetGray = Function(ARGB) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)

            Dim DrawedRectangle = g.MeasureStringRectangle(c.UnicodeString, Font)
            Dim X As Integer = Round(DrawedRectangle.Left)
            Dim Y As Integer = Round(DrawedRectangle.Top)
            Dim X2 As Integer = Round(DrawedRectangle.Right)
            Dim Y2 As Integer = Round(DrawedRectangle.Bottom)
            Dim Width As Integer = X2 - X
            Dim Height As Integer = Y2 - Y
            Dim ox As Integer = (PhysicalWidthValue * 2 - Width) \ 2
            Dim oy As Integer = (PhysicalHeightValue * 2 - Height) \ 2
            If AnchorLeft Then ox = ox Mod 2

            g.Clear(Color.Black)
            g.DrawString(c.UnicodeString, Font, Brushes.White, ox - X + DrawOffsetX * 2, oy - Y + DrawOffsetY * 2)
            Dim r = GlyphPiece.GetRectangle(0, 0, PhysicalWidthValue * 2, PhysicalHeightValue * 2)
            For ry = 0 To PhysicalHeightValue - 1
                For rx = 0 To PhysicalWidthValue - 1
                    Dim L1 = GetGray(r(rx * 2, ry * 2))
                    Dim L2 = GetGray(r(rx * 2 + 1, ry * 2))
                    Dim L3 = GetGray(r(rx * 2, ry * 2 + 1))
                    Dim L4 = GetGray(r(rx * 2 + 1, ry * 2 + 1))
                    Dim L As Integer = Mix(L1, L2, L3, L4)
                    If L < 0 Then L = 0
                    If L > 255 Then L = 255
                    Dim ARGB = ConcatBits(GetChannel(ChannelPatterns(0), L), 8, GetChannel(ChannelPatterns(1), L), 8, GetChannel(ChannelPatterns(2), L), 8, GetChannel(ChannelPatterns(3), L), 8)
                    Block(rx, ry) = ARGB
                Next
            Next

            Dim VirtualRectangleX As Integer = ox \ 2 - VirtualDeltaWidth \ 2 + VirtualOffsetX
            Dim VirtualRectangleY As Integer = oy \ 2 - VirtualDeltaHeight \ 2 + VirtualOffsetY
            Dim VirtualRectangleX2 As Integer = VirtualRectangleX + (Width + 1) \ 2 + VirtualDeltaWidth
            Dim VirtualRectangleY2 As Integer = VirtualRectangleY + (Height + 1) \ 2 + VirtualDeltaHeight
            If VirtualRectangleX < 0 Then VirtualRectangleX = 0
            If VirtualRectangleY < 0 Then VirtualRectangleY = 0
            If VirtualRectangleX2 > PhysicalWidthValue Then VirtualRectangleX2 = PhysicalWidthValue
            If VirtualRectangleY2 > PhysicalHeightValue Then VirtualRectangleY2 = PhysicalHeightValue
            Return New Glyph With {.c = c, .Block = Block, .VirtualBox = New Rectangle(VirtualRectangleX, VirtualRectangleY, VirtualRectangleX2 - VirtualRectangleX, VirtualRectangleY2 - VirtualRectangleY)}
        End Function

        Private Shared Function GetChannel(ByVal Pattern As ChannelPattern, ByVal L As Byte) As Byte
            Select Case Pattern
                Case ChannelPattern.Zero
                    Return 0
                Case ChannelPattern.Draw
                    Return L
                Case ChannelPattern.One
                    Return &HFF
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

#Region " IDisposable 支持 "
        Private DisposedValue As Boolean = False '检测冗余的调用
        ''' <summary>释放流的资源。</summary>
        ''' <remarks>对继承者的说明：不要调用基类的Dispose()，而应调用Dispose(True)，否则会出现无限递归。</remarks>
        Private Sub Dispose(ByVal Disposing As Boolean)
            If DisposedValue Then Return
            If Disposing Then
                '释放其他状态(托管对象)。
                If Font IsNot Nothing Then Font.Dispose()
                If GlyphPiece IsNot Nothing Then GlyphPiece.Dispose()
                If g IsNot Nothing Then g.Dispose()
            End If

            '释放您自己的状态(非托管对象)。
            '将大型字段设置为 null。
            DisposedValue = True
        End Sub
        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean) 中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
