'==========================================================================
'
'  File:        FontGen.vb
'  Location:    Firefly.FontGen <Visual Basic .Net>
'  Description: 字库图片生成器
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports System.Diagnostics
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Glyphing
Imports Firefly.Imaging
Imports Firefly.GUI

Public Class FontGen
    Public Declare Function FreeConsole Lib "kernel32.dll" () As Boolean

    Public Shared Sub Application_ThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
        ExceptionHandler.PopupException(e.Exception, New StackTrace(4, True))
    End Sub

    Public Shared Function Main() As Integer
        Dim CmdLine = CommandLine.GetCmdLine()
        Dim argv = CmdLine.Arguments
        Dim opt = CmdLine.Options

        If argv.Length = 0 AndAlso opt.Length = 0 Then
            If Debugger.IsAttached Then
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException)
                Return MainWindow()
            Else
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
                Try
                    AddHandler Application.ThreadException, AddressOf Application_ThreadException
                    Return MainWindow()
                Catch ex As Exception
                    ExceptionHandler.PopupException(ex)
                    Return -1
                Finally
                    RemoveHandler Application.ThreadException, AddressOf Application_ThreadException
                End Try
            End If
        Else
            If System.Diagnostics.Debugger.IsAttached Then
                Return MainConsole()
            Else
                Try
                    Return MainConsole()
                Catch ex As Exception
                    Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex))
                    Return -1
                End Try
            End If
        End If
    End Function

    Public Shared Function MainWindow() As Integer
        FreeConsole()
        Application.EnableVisualStyles()
        Application.Run(New FontGen)
        Return 0
    End Function

    Public Shared Sub DisplayInfo()
        Console.WriteLine("字库图片生成器")
        Console.WriteLine("Firefly.FontGen，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("本生成器用于从tbl编码文件、fd字库描述文件或者字符文件生成字库图片和对应的fd字库描述文件。")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("FontGen <Source tbl/fd/CharFile> <Target fd> <FontName> <FontStyle> <FontSize> <PhysicalWidth> <PhysicalHeight> <DrawOffsetX> <DrawOffsetY> [<VirtualOffsetX> <VirtualOffsetY> <VirtualDeltaWidth> <VirtualDeltaHeight> [<PicWidth> <PicHeight>]] [/x2] [/left]")
        Console.WriteLine("Source tbl/fd/CharFile 输入tbl编码文件、fd字库描述文件或者字符文件路径。")
        Console.WriteLine("Target fd 输出fd字库描述文件路径。输出的字库图片(bmp)也使用该路径。")
        Console.WriteLine("FontName 字体名称。")
        Console.WriteLine("FontStyle 字体风格，加粗 1 斜体 2 下划线 4 删除线 8，可叠加。")
        Console.WriteLine("FontSize 字体大小。")
        Console.WriteLine("PhysicalWidth 物理宽度，字符格子宽度。")
        Console.WriteLine("PhysicalHeight 物理高度，字符格子高度。")
        Console.WriteLine("DrawOffsetX 绘制X偏移。")
        Console.WriteLine("DrawOffsetY 绘制Y偏移。")
        Console.WriteLine("VirtualOffsetX 虚拟X偏移，字符的显示部分的X偏移。")
        Console.WriteLine("VirtualOffsetY 虚拟Y偏移，字符的显示部分的Y偏移。")
        Console.WriteLine("VirtualDeltaWidth 虚拟宽度差，字符的显示部分的宽度相对于默认值的差。")
        Console.WriteLine("VirtualDeltaHeight 虚拟高度差，字符的显示部分的高度相对于默认值的差。")
        Console.WriteLine("PicWidth 图片宽度。")
        Console.WriteLine("PicHeight 图片高度。")
        Console.WriteLine("如果不指定图片宽度和高度，将会自动选择最小的能容纳所有字符的2的幂的宽和高。")
        Console.WriteLine("如果指定图片宽度和高度，将会生成多张图片。")
        Console.WriteLine("/x2 2x超采样。")
        Console.WriteLine("/left 左对齐。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("FontGen FakeShiftJIS.tbl FakeShiftJIS.fd 宋体 0 16 16 16 0 0")
        Console.WriteLine("FontGen FakeShiftJIS.tbl FakeShiftJIS.fd 宋体 0 16 16 16 0 0 0 0 0 0 1024 1024")
        Console.WriteLine("")
        Console.WriteLine("高级用法：")
        Console.WriteLine("FontGen (Add|AddNew|RemoveUnicode|RemoveCode|SortUnicode|SortCode|Save)*")
        Console.WriteLine("Add ::= [/x2] [/left] [/argb:<Pattern>=1xxx] /add:<Source tbl/fd/CharFile>[,<FontName>,<FontStyle>,<FontSize>,<PhysicalWidth>,<PhysicalHeight>,<DrawOffsetX>,<DrawOffsetY>[,<VirtualOffsetX>,<VirtualOffsetY>,<VirtualDeltaWidth>,<VirtualDeltaHeight>]]")
        Console.WriteLine("AddNew ::= [/x2] [/left] [/argb:<Pattern>=1xxx] /addnew:<Source tbl/fd/CharFile>[,<FontName>,<FontStyle>,<FontSize>,<PhysicalWidth>,<PhysicalHeight>,<DrawOffsetX>,<DrawOffsetY>[,<VirtualOffsetX>,<VirtualOffsetY>,<VirtualDeltaWidth>,<VirtualDeltaHeight>]]")
        Console.WriteLine("RemoveUnicode ::= /removeunicode:<Lower:Hex>,<Upper:Hex>")
        Console.WriteLine("RemoveCode ::= /removecode:<Lower:Hex>,<Upper:Hex>")
        Console.WriteLine("SortUnicode ::= /sortunicode")
        Console.WriteLine("SortCode ::= /sortcode")
        Console.WriteLine("Save ::= [/bpp:<BitPerPixel>=8] [/size:<PicWidth>,<PicHeight>] [/multiple] [/compact] /save:<Target fd>")
        Console.WriteLine("/argb 指定颜色的形式。")
        Console.WriteLine("Pattern Pattern由4位组成，分别对应A、R、G、B通道，每位可以是0、1或x，其中0表示为0，1表示为最大值，x表示为绘图值。")
        Console.WriteLine("/add 添加字形源，可以指定参数由本程序生成，或指定由fd文件加载。")
        Console.WriteLine("/addnew 添加字形源，但仅当字符不存在时才添加。")
        Console.WriteLine("/removeunicode 移除该Unicode范围内(包含两边界)字符的字形，Unicode的范围包括扩展平面。")
        Console.WriteLine("/removecode 移除该编码范围内(包含两边界)字符的字形。")
        Console.WriteLine("/sortunicode 按Unicode排序。")
        Console.WriteLine("/sortcode 按编码排序。")
        Console.WriteLine("/bpp 指定位深度。")
        Console.WriteLine("BitPerPixel 位深度：1、2、4、8、16、32。")
        Console.WriteLine("/size 指定图片大小。")
        Console.WriteLine("PicWidth 图片宽度。")
        Console.WriteLine("PicHeight 图片高度。")
        Console.WriteLine("/multiple 指定保存为多个文件。")
        Console.WriteLine("/compact 紧凑存储，列不对齐。")
        Console.WriteLine("/save 保存字形到fd文件。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("FontGen /add:Original.fd /removecode:100,10000 /x2 /left /addnew:FakeShiftJIS.tbl,宋体,0,16,16,16,0,0 /save:FakeShiftJIS.fd")
        Console.WriteLine("该例子表明：从Original.fd加载字库，删去0x100到0x10000的部分，然後将FakeShiftJIS.tbl生成字形，将其中新增的字形加入，并将结果保存到FakeShiftJIS.fd")
    End Sub

    Public Class GlyphComparer
        Inherits EqualityComparer(Of IGlyph)

        Public Overloads Overrides Function Equals(ByVal x As IGlyph, ByVal y As IGlyph) As Boolean
            If x.c.HasCodes AndAlso y.c.HasCodes Then Return x.c.Codes = y.c.Codes
            If x.c.HasUnicodes AndAlso y.c.HasUnicodes Then Return x.c.UnicodeString = y.c.UnicodeString
            Return x.c.Equals(y.c)
        End Function

        Public Overloads Overrides Function GetHashCode(ByVal obj As IGlyph) As Integer
            If obj.c.HasCodes Then Return obj.c.Codes.GetHashCode()
            If obj.c.HasUnicodes Then Return obj.c.Unicodes.GetHashCode()
            Return obj.c.GetHashCode()
        End Function
    End Class

    Public Shared Function MainConsole() As Integer
        Dim CmdLine = CommandLine.GetCmdLine()

        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
            End Select
        Next

        Select Case CmdLine.Arguments.Count
            Case 0
                Dim Glyphs As IEnumerable(Of IGlyph) = New Glyph() {}
                Dim ChannelPatterns As ChannelPattern() = {ChannelPattern.One, ChannelPattern.Draw, ChannelPattern.Draw, ChannelPattern.Draw}
                Dim EnableDoubleSample As Boolean = False
                Dim AnchorLeft As Boolean = False
                Dim BitPerPixel As Integer = 8
                Dim PicWidth As Integer = -1
                Dim PicHeight As Integer = -1
                Dim Multiple As Boolean = False
                Dim Compact As Boolean = False
                For Each opt In CmdLine.Options
                    Select Case opt.Name.ToLower
                        Case "argb"
                            Dim argv = opt.Arguments
                            Select Case argv.Length
                                Case 1
                                    Dim s = argv(0).ToLower.ToUTF32
                                    If s.Length <> 4 Then Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    For n = 0 To 3
                                        Select Case s(n)
                                            Case "0"
                                                ChannelPatterns(n) = ChannelPattern.Zero
                                            Case "x"
                                                ChannelPatterns(n) = ChannelPattern.Draw
                                            Case "1"
                                                ChannelPatterns(n) = ChannelPattern.One
                                            Case Else
                                                Throw New ArgumentException(String.Join(",", opt.Arguments))
                                        End Select
                                    Next
                                Case Else
                                    Throw New ArgumentException(String.Join(",", opt.Arguments))
                            End Select
                        Case "x2"
                            EnableDoubleSample = True
                        Case "left"
                            AnchorLeft = True
                        Case "bpp"
                            Dim argv = opt.Arguments
                            Select Case argv.Length
                                Case 1
                                    BitPerPixel = Integer.Parse(argv(0))
                                Case Else
                                    Throw New ArgumentException(String.Join(",", opt.Arguments))
                            End Select
                        Case "size"
                            Dim argv = opt.Arguments
                            Select Case argv.Length
                                Case 2
                                    PicWidth = Integer.Parse(argv(0))
                                    PicHeight = Integer.Parse(argv(1))
                                Case Else
                                    Throw New ArgumentException(String.Join(",", opt.Arguments))
                            End Select
                        Case "multiple"
                            Multiple = True
                        Case "compact"
                            Compact = True
                        Case Else
                            Dim argv = opt.Arguments
                            Select Case opt.Name.ToLower
                                Case "add"
                                    Select Case argv.Length
                                        Case 1
                                            Dim g = FdGlyphDescriptionFile.ReadFont(argv(0))
                                            Glyphs = Glyphs.Except(g, New GlyphComparer).Concat(g)
                                        Case 8
                                            Dim g = GenerateFont(argv(0), argv(1), argv(2), argv(3), argv(4), argv(5), argv(6), argv(7), 0, 0, 0, 0, EnableDoubleSample, AnchorLeft, ChannelPatterns)
                                            Glyphs = Glyphs.Except(g, New GlyphComparer).Concat(g)
                                        Case 12
                                            Dim g = GenerateFont(argv(0), argv(1), argv(2), argv(3), argv(4), argv(5), argv(6), argv(7), argv(8), argv(9), argv(10), argv(11), EnableDoubleSample, AnchorLeft, ChannelPatterns)
                                            Glyphs = Glyphs.Except(g, New GlyphComparer).Concat(g)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "addnew"
                                    Select Case argv.Length
                                        Case 1
                                            Dim g = FdGlyphDescriptionFile.ReadFont(argv(0))
                                            Glyphs = Glyphs.Concat(g.Except(Glyphs, New GlyphComparer))
                                        Case 8
                                            Dim g = GenerateFont(argv(0), argv(1), argv(2), argv(3), argv(4), argv(5), argv(6), argv(7), 0, 0, 0, 0, EnableDoubleSample, AnchorLeft, ChannelPatterns)
                                            Glyphs = Glyphs.Concat(g.Except(Glyphs, New GlyphComparer))
                                        Case 12
                                            Dim g = GenerateFont(argv(0), argv(1), argv(2), argv(3), argv(4), argv(5), argv(6), argv(7), argv(8), argv(9), argv(10), argv(11), EnableDoubleSample, AnchorLeft, ChannelPatterns)
                                            Glyphs = Glyphs.Concat(g.Except(Glyphs, New GlyphComparer))
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "removeunicode"
                                    Select Case argv.Length
                                        Case 2
                                            Dim l = Int32.Parse(argv(0), Globalization.NumberStyles.HexNumber)
                                            Dim u = Int32.Parse(argv(1), Globalization.NumberStyles.HexNumber)
                                            Glyphs = Glyphs.Where(Function(g) (Not g.c.HasUnicodes) OrElse Char32.FromString(g.c.UnicodeString) < l OrElse Char32.FromString(g.c.UnicodeString) > u)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "removecode"
                                    Select Case argv.Length
                                        Case 2
                                            Dim l = UInt64.Parse(argv(0), Globalization.NumberStyles.HexNumber)
                                            Dim u = UInt64.Parse(argv(1), Globalization.NumberStyles.HexNumber)
                                            Glyphs = Glyphs.Where(Function(g) (Not g.c.HasCodes) OrElse GetCode(g.c) < l OrElse GetCode(g.c) > u)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "sortunicode"
                                    Glyphs = Glyphs.OrderBy(Function(g) g.c.UnicodeString, StringComparer.Ordinal)
                                Case "sortcode"
                                    Glyphs = Glyphs.OrderBy(Function(g) g.c.CodeString)
                                Case "save"
                                    Select Case argv.Length
                                        Case 1
                                            SaveFont(Glyphs, argv(0), PicWidth, PicHeight, BitPerPixel, Multiple, Compact)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case Else
                                    Throw New ArgumentException(opt.Name)
                            End Select
                            ChannelPatterns = New ChannelPattern() {ChannelPattern.One, ChannelPattern.Draw, ChannelPattern.Draw, ChannelPattern.Draw}
                            EnableDoubleSample = False
                            AnchorLeft = False
                            BitPerPixel = 8
                            PicWidth = -1
                            PicHeight = -1
                            Multiple = False
                            Compact = False
                    End Select
                Next
            Case 9, 13, 15
                Dim argv = CmdLine.Arguments
                Dim ChannelPatterns As ChannelPattern() = {ChannelPattern.One, ChannelPattern.Draw, ChannelPattern.Draw, ChannelPattern.Draw}
                Dim EnableDoubleSample As Boolean = False
                Dim AnchorLeft As Boolean = False
                For Each opt In CmdLine.Options
                    Select Case opt.Name.ToLower
                        Case "x2"
                            EnableDoubleSample = True
                        Case "left"
                            AnchorLeft = True
                        Case Else
                            Throw New ArgumentException
                    End Select
                Next
                Select Case argv.Count
                    Case 9
                        SaveFont(GenerateFont(argv(0), argv(2), argv(3), argv(4), argv(5), argv(6), argv(7), argv(8), 0, 0, 0, 0, EnableDoubleSample, AnchorLeft, ChannelPatterns), argv(1), -1, -1, 8, False, False)
                    Case 13
                        SaveFont(GenerateFont(argv(0), argv(2), argv(3), argv(4), argv(5), argv(6), argv(7), argv(8), argv(9), argv(10), argv(11), argv(12), EnableDoubleSample, AnchorLeft, ChannelPatterns), argv(1), -1, -1, 8, False, False)
                    Case 15
                        SaveFont(GenerateFont(argv(0), argv(2), argv(3), argv(4), argv(5), argv(6), argv(7), argv(8), argv(9), argv(10), argv(11), argv(12), EnableDoubleSample, AnchorLeft, ChannelPatterns), argv(1), argv(13), argv(14), 8, True, False)
                End Select
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    Private Shared Function GetCode(ByVal This As StringCode) As UInt64
        If This.Codes.Count > 8 Then Throw New NotSupportedException

        Dim i As UInt64 = 0
        For Each b In This.Codes
            i = (i << 8) Or b
        Next
        Return i
    End Function

    Public Shared Function GenerateFont(ByVal SourcePath As String, ByVal FontName As String, ByVal FontStyle As FontStyle, ByVal FontSize As Integer, ByVal PhysicalWidth As Integer, ByVal PhysicalHeight As Integer, ByVal DrawOffsetX As Integer, ByVal DrawOffsetY As Integer, ByVal VirtualOffsetX As Integer, ByVal VirtualOffsetY As Integer, ByVal VirtualDeltaWidth As Integer, ByVal VirtualDeltaHeight As Integer, ByVal EnableDoubleSample As Boolean, ByVal AnchorLeft As Boolean, ByVal ChannelPatterns As ChannelPattern()) As IEnumerable(Of IGlyph)
        Dim StringCodes As StringCode()

        Dim Ext = GetExtendedFileName(SourcePath)
        If Ext.Equals("tbl", StringComparison.OrdinalIgnoreCase) Then
            StringCodes = TblCharMappingFile.ReadFile(SourcePath).ToArray
        ElseIf Ext.Equals("fd", StringComparison.OrdinalIgnoreCase) Then
            StringCodes = (From d In FdGlyphDescriptionFile.ReadFile(SourcePath) Select d.c).ToArray
        ElseIf Ext.Equals("txt", StringComparison.OrdinalIgnoreCase) Then
            StringCodes = (From c In Txt.ReadFile(SourcePath).ToUTF32 Select StringCode.FromUnicodeChar(c)).ToArray
        Else
            Throw New InvalidDataException
        End If

        Dim gg As IGlyphProvider
        If EnableDoubleSample Then
            gg = New GlyphGeneratorDoubleSample(FontName, FontStyle, FontSize, PhysicalWidth, PhysicalHeight, DrawOffsetX, DrawOffsetY, VirtualOffsetX, VirtualOffsetY, VirtualDeltaWidth, VirtualDeltaHeight, AnchorLeft, ChannelPatterns)
        Else
            gg = New GlyphGenerator(FontName, FontStyle, FontSize, PhysicalWidth, PhysicalHeight, DrawOffsetX, DrawOffsetY, VirtualOffsetX, VirtualOffsetY, VirtualDeltaWidth, VirtualDeltaHeight, AnchorLeft, ChannelPatterns)
        End If

        Using gg
            Return (From c In StringCodes Select gg.GetGlyph(c)).ToArray
        End Using
    End Function

    Public Shared Sub SaveFont(ByVal Glyphs As IEnumerable(Of IGlyph), ByVal TargetPath As String, ByVal PicWidth As Integer, ByVal PicHeight As Integer, ByVal BitPerPixel As Integer, ByVal Multiple As Boolean, ByVal Compact As Boolean)
        Dim gl = Glyphs.ToArray
        Dim PhysicalWidth As Integer = (From g In gl Select (g.PhysicalWidth)).Max
        Dim PhysicalHeight As Integer = (From g In gl Select (g.PhysicalHeight)).Max
        Dim ga As IGlyphArranger
        If Compact Then
            ga = New GlyphArrangerCompact(PhysicalWidth, PhysicalHeight)
        Else
            ga = New GlyphArranger(PhysicalWidth, PhysicalHeight)
        End If
        If PicWidth < 0 OrElse PicHeight < 0 Then
            Dim Size = ga.GetPreferredSize(gl)
            PicWidth = Size.Width
            PicHeight = Size.Height
        End If

        If Multiple Then
            Dim n = 0
            Dim GlyphIndex = 0
            While GlyphIndex < gl.Length
                Dim FdPath = ChangeExtension(TargetPath, "{0}.{1}".Formats(n, GetExtendedFileName(TargetPath)))
                Dim PartGlyphDescriptors = ga.GetGlyphArrangement(gl, PicWidth, PicHeight)
                Dim pgd = PartGlyphDescriptors.ToArray
                If pgd.Length = 0 Then Throw New InvalidDataException("PicSizeTooSmallForGlyphOfChar:{0}".Formats(gl(GlyphIndex).c.ToString()))
                Dim pgl = gl.SubArray(GlyphIndex, pgd.Length)
                Using ImageWriter As New BmpFontImageFileWriter(ChangeExtension(FdPath, "bmp"), BitPerPixel)
                    FdGlyphDescriptionFile.WriteFont(FdPath, TextEncoding.WritingDefault, pgl, pgd, ImageWriter, PicWidth, PicHeight)
                End Using
                GlyphIndex += pgd.Length
            End While
        Else
            Using ImageWriter As New BmpFontImageFileWriter(ChangeExtension(TargetPath, "bmp"), BitPerPixel)
                FdGlyphDescriptionFile.WriteFont(TargetPath, TextEncoding.WritingDefault, gl, ImageWriter, ga, PicWidth, PicHeight)
            End Using
        End If
    End Sub


    Private Initialized As Boolean = False
    Private Sub ReDraw()
        If Not Initialized Then Return
        'Try
        Dim Image = New Bitmap(PictureBox_Preview.Width, PictureBox_Preview.Height, Drawing.Imaging.PixelFormat.Format32bppArgb)
        Dim Image2x = New Bitmap(PictureBox_Preview2x.Width, PictureBox_Preview2x.Height, Drawing.Imaging.PixelFormat.Format32bppArgb)
        'Try
        Using g = Graphics.FromImage(Image)
            Using g2x = Graphics.FromImage(Image2x)
                g.Clear(Color.White)
                g2x.Clear(Color.LightGray)

                Dim Style As FontStyle = FontStyle.Regular
                If CheckBox_Bold.Checked Then Style = Style Or FontStyle.Bold
                If CheckBox_Italic.Checked Then Style = Style Or FontStyle.Italic
                If CheckBox_Underline.Checked Then Style = Style Or FontStyle.Underline
                If CheckBox_Strikeout.Checked Then Style = Style Or FontStyle.Strikeout
                Dim PhysicalWidth As Integer = NumericUpDown_PhysicalWidth.Value
                Dim PhysicalHeight As Integer = NumericUpDown_PhysicalHeight.Value
                Dim EnableDoubleSample As Boolean = CheckBox_DoubleSample.Checked
                Dim AnchorLeft As Boolean = CheckBox_AnchorLeft.Checked
                Dim ChannelPatterns As ChannelPattern() = {ChannelPattern.One, ChannelPattern.Draw, ChannelPattern.Draw, ChannelPattern.Draw}
                Dim gg As IGlyphProvider
                If EnableDoubleSample Then
                    gg = New GlyphGeneratorDoubleSample(ComboBox_FontName.Text, Style, NumericUpDown_Size.Value, PhysicalWidth, PhysicalHeight, NumericUpDown_DrawOffsetX.Value, NumericUpDown_DrawOffsetY.Value, NumericUpDown_VirtualOffsetX.Value, NumericUpDown_VirtualOffsetY.Value, NumericUpDown_VirtualDeltaWidth.Value, NumericUpDown_VirtualDeltaHeight.Value, AnchorLeft, ChannelPatterns)
                Else
                    gg = New GlyphGenerator(ComboBox_FontName.Text, Style, NumericUpDown_Size.Value, PhysicalWidth, PhysicalHeight, NumericUpDown_DrawOffsetX.Value, NumericUpDown_DrawOffsetY.Value, NumericUpDown_VirtualOffsetX.Value, NumericUpDown_VirtualOffsetY.Value, NumericUpDown_VirtualDeltaWidth.Value, NumericUpDown_VirtualDeltaHeight.Value, AnchorLeft, ChannelPatterns)
                End If

                Using gg
                    Dim TestStrings = New String() {"012 AaBbCc", "!""#$%&'()*+,-./:;<=>?@[\]^_`", "珍爱生命　远离汉化", "これはテストォーー！", "يادداشت هاي شخصي احمدي نژاد"}

                    Using b As New Bmp(PhysicalWidth, PhysicalHeight, 32)
                        Using b2x As New Bmp(PhysicalWidth * 2, PhysicalHeight * 2, 32)
                            Dim Block2x = New Int32(PhysicalWidth * 2 - 1, PhysicalHeight * 2 - 1) {}
                            Dim l = 0
                            For Each t In TestStrings
                                Dim k = 0
                                For Each c In t.ToUTF32
                                    Dim x = k * (PhysicalWidth + 4)
                                    Dim y = l * (PhysicalHeight + 4)
                                    Dim PhysicalRect As New Rectangle(x, y, PhysicalWidth, PhysicalHeight)
                                    Dim glyph = gg.GetGlyph(StringCode.FromUnicodeChar(c))
                                    Dim VirtualRect = glyph.VirtualBox
                                    Dim Block = glyph.Block
                                    For y0 As Integer = 0 To PhysicalHeight - 1
                                        For x0 As Integer = 0 To PhysicalWidth - 1
                                            Block(x0, y0) = Block(x0, y0) Xor &HFFFFFF
                                        Next
                                    Next
                                    b.SetRectangle(0, 0, Block)
                                    Using bb = b.ToBitmap
                                        g.DrawImage(bb, PhysicalRect)
                                    End Using

                                    Dim PhysicalRect2x As New Rectangle(x * 2, y * 2, PhysicalWidth * 2, PhysicalHeight * 2)
                                    For y0 As Integer = 0 To PhysicalHeight - 1
                                        For x0 As Integer = 0 To PhysicalWidth - 1
                                            Block2x(x0 * 2, y0 * 2) = Block(x0, y0)
                                            Block2x(x0 * 2 + 1, y0 * 2) = Block(x0, y0)
                                            Block2x(x0 * 2, y0 * 2 + 1) = Block(x0, y0)
                                            Block2x(x0 * 2 + 1, y0 * 2 + 1) = Block(x0, y0)
                                        Next
                                    Next
                                    b2x.SetRectangle(0, 0, Block2x)
                                    Using bb2x = b2x.ToBitmap
                                        g2x.DrawImage(bb2x, PhysicalRect2x)
                                    End Using

                                    g2x.DrawRectangle(Pens.Red, New Rectangle(x * 2 + VirtualRect.X * 2, y * 2 + VirtualRect.Y * 2, VirtualRect.Width * 2 - 1, VirtualRect.Height * 2 - 1))

                                    k += 1
                                Next
                                l += 1
                            Next
                        End Using
                    End Using
                End Using
            End Using
        End Using
        'Catch
        'End Try
        PictureBox_Preview.Image = Image
        PictureBox_Preview2x.Image = Image2x
        PictureBox_Preview.Invalidate()
        PictureBox_Preview2x.Invalidate()
        'Catch
        'End Try
    End Sub

    Private Sub FontGen_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Initialized = True
        ComboBox_FontName.Items.AddRange((From f In FontFamily.Families Select f.Name).ToArray)
        ReDraw()
    End Sub
    Private Sub ComboBox_FontName_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox_FontName.TextChanged
        ReDraw()
    End Sub
    Private Sub NumericUpDown_Size_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles NumericUpDown_Size.ValueChanged
        ReDraw()
    End Sub
    Private Sub CheckBox_Bold_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox_Bold.CheckedChanged
        ReDraw()
    End Sub
    Private Sub CheckBox_Italic_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox_Italic.CheckedChanged
        ReDraw()
    End Sub
    Private Sub CheckBox_Underline_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox_Underline.CheckedChanged
        ReDraw()
    End Sub
    Private Sub CheckBox_Strikeout_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox_Strikeout.CheckedChanged
        ReDraw()
    End Sub
    Private Sub CheckBox_DoubleSample_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox_DoubleSample.CheckedChanged, CheckBox_AnchorLeft.CheckedChanged
        ReDraw()
    End Sub
    Private Sub NumericUpDowns_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles NumericUpDown_PhysicalWidth.ValueChanged, NumericUpDown_PhysicalHeight.ValueChanged, NumericUpDown_DrawOffsetX.ValueChanged, NumericUpDown_DrawOffsetY.ValueChanged, NumericUpDown_VirtualOffsetX.ValueChanged, NumericUpDown_VirtualOffsetY.ValueChanged, NumericUpDown_VirtualDeltaWidth.ValueChanged, NumericUpDown_VirtualDeltaHeight.ValueChanged
        ReDraw()
    End Sub
    Private Sub FontGen_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged
        ReDraw()
    End Sub

    Private Function Esc(ByVal Parameter As String) As String
        If Parameter = "" Then Return """"""
        If Parameter.Contains(" ") Then Return """" & Parameter & """"
        If Parameter.Contains("　") Then Return """" & Parameter & """"
        Return Parameter
    End Function

    Private Function FormatEsc(ByVal Format As String, ByVal ParamArray args As Object()) As String
        Return Format.Formats((From arg In args Select Esc(arg.ToString())).ToArray)
    End Function

    Private Sub Button_CmdToClipboard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_CmdToClipboard.Click
        Dim Style As FontStyle = FontStyle.Regular
        If CheckBox_Bold.Checked Then Style = Style Or FontStyle.Bold
        If CheckBox_Italic.Checked Then Style = Style Or FontStyle.Italic
        If CheckBox_Underline.Checked Then Style = Style Or FontStyle.Underline
        If CheckBox_Strikeout.Checked Then Style = Style Or FontStyle.Strikeout
        Dim PhysicalWidth As Integer = NumericUpDown_PhysicalWidth.Value
        Dim PhysicalHeight As Integer = NumericUpDown_PhysicalHeight.Value
        Dim Options As New List(Of String)
        If CheckBox_DoubleSample.Checked Then Options.Add("/x2")
        If CheckBox_AnchorLeft.Checked Then Options.Add("/left")
        Dim AddParameters = New String() {GetFileName(FileSelectBox_File.Path), ComboBox_FontName.Text, CInt(Style), NumericUpDown_Size.Value, PhysicalWidth, PhysicalHeight, NumericUpDown_DrawOffsetX.Value, NumericUpDown_DrawOffsetY.Value, NumericUpDown_VirtualOffsetX.Value, NumericUpDown_VirtualOffsetY.Value, NumericUpDown_VirtualDeltaWidth.Value, NumericUpDown_VirtualDeltaHeight.Value}
        Options.Add("/add:" & String.Join(",", (From p In AddParameters Select Esc(p)).ToArray))
        Options.Add("/save:" & Esc(ChangeExtension(GetFileName(FileSelectBox_File.Path), "fd")))
        Dim Cmd = FormatEsc("FontGen " & String.Join(" ", Options.ToArray))
        My.Computer.Clipboard.SetText(Cmd)
        MessageBox.Show(Cmd, Me.Text)
    End Sub

    Private Sub Button_Generate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Generate.Click
        Dim Style As FontStyle = FontStyle.Regular
        If CheckBox_Bold.Checked Then Style = Style Or FontStyle.Bold
        If CheckBox_Italic.Checked Then Style = Style Or FontStyle.Italic
        If CheckBox_Underline.Checked Then Style = Style Or FontStyle.Underline
        If CheckBox_Strikeout.Checked Then Style = Style Or FontStyle.Strikeout
        Dim PhysicalWidth As Integer = NumericUpDown_PhysicalWidth.Value
        Dim PhysicalHeight As Integer = NumericUpDown_PhysicalHeight.Value
        Dim EnableDoubleSample As Boolean = CheckBox_DoubleSample.Checked
        Dim AnchorLeft As Boolean = CheckBox_AnchorLeft.Checked
        Dim ChannelPatterns As ChannelPattern() = {ChannelPattern.One, ChannelPattern.Draw, ChannelPattern.Draw, ChannelPattern.Draw}

        SaveFont(GenerateFont(FileSelectBox_File.Path, ComboBox_FontName.Text, Style, NumericUpDown_Size.Value, PhysicalWidth, PhysicalHeight, NumericUpDown_DrawOffsetX.Value, NumericUpDown_DrawOffsetY.Value, NumericUpDown_VirtualOffsetX.Value, NumericUpDown_VirtualOffsetY.Value, NumericUpDown_VirtualDeltaWidth.Value, NumericUpDown_VirtualDeltaHeight.Value, EnableDoubleSample, AnchorLeft, ChannelPatterns), ChangeExtension(FileSelectBox_File.Path, "fd"), -1, -1, 8, False, False)
        MessageBox.Show("生成完毕。", Me.Text)
    End Sub
End Class
