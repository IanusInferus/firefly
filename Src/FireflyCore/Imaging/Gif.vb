'==========================================================================
'
'  File:        Gif.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: 基本Gif文件类
'  Version:     2010.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict Off
Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.IO
Imports Firefly.TextEncoding
Imports Firefly.Streaming

Namespace Imaging
    ''' <summary>基本Gif文件类</summary>
    ''' <remarks>用于GIF89a，但忽略无用功能</remarks>
    Public Class Gif
        Public Const Identifier As String = "GIF89a"
        Private PicWidth As Int16
        Public ReadOnly Property Width() As Int16
            Get
                Return PicWidth
            End Get
        End Property
        Private PicHeight As Int16
        Public ReadOnly Property Height() As Int16
            Get
                Return PicHeight
            End Get
        End Property
        Public ReadOnly Property GlobalColorTableFlag() As Boolean
            Get
                Return PicPalette IsNot Nothing
            End Get
        End Property
        Private PicBitsPerPixel As Integer
        Public ReadOnly Property BitsPerPixel() As Integer
            Get
                Return PicBitsPerPixel
            End Get
        End Property
        Public ReadOnly Property GlobalColorTableSize() As Integer
            Get
                If PicPalette Is Nothing Then Return 0
                Return PicPalette.GetLength(0)
            End Get
        End Property
        Public GlobalBackgroundColor As Byte
        Private Const PixelAspectRadio As Byte = 0 '不用
        Private PicPalette As Int32()
        Public ReadOnly Property Palette() As Int32()
            Get
                Return PicPalette
            End Get
        End Property
        Public Flame As GifImageBlock()
        Private Const Trailer As Byte = &H3B

        Private Sub New()
        End Sub

        Public Sub New(ByVal SingleFlame As GifImageBlock, Optional ByVal Palette As Int32() = Nothing)
            If SingleFlame Is Nothing Then Throw New InvalidDataException
            With SingleFlame
                PicWidth = .Width
                PicHeight = .Height
                PicBitsPerPixel = Ceiling(Log(.Palette.GetLength(0)) / Log(2)) '色深
            End With
            If Palette IsNot Nothing Then Me.PicPalette = Palette.Clone
            Flame = New GifImageBlock() {SingleFlame}
        End Sub
        Public Sub New(ByVal Width As Int16, ByVal Height As Int16, ByVal BitsPerPixel As Byte, ByVal Flames As GifImageBlock(), Optional ByVal Palette As Int32() = Nothing)
            If Width < 0 OrElse PicHeight < 0 OrElse BitsPerPixel <= 0 Then Throw New InvalidDataException
            PicWidth = Width
            PicHeight = Height
            PicBitsPerPixel = BitsPerPixel '色深

            If Palette IsNot Nothing Then Me.PicPalette = Palette.Clone
            If Flames IsNot Nothing Then Flame = Flames.Clone
        End Sub

        Public Sub New(ByVal Path As String)
            Using gf As New StreamEx(Path, FileMode.Open)
                With Me
                    For n As Integer = 0 To 5
                        If gf.ReadByte <> AscW(Identifier(n)) Then
                            Throw New InvalidDataException
                        End If
                    Next
                    .PicWidth = gf.ReadInt16
                    .PicHeight = gf.ReadInt16
                    Dim b As Byte = gf.ReadByte
                    .PicBitsPerPixel = ((b And &H70) >> 4) + 1
                    Dim PicGlobalColorTableSize As Integer = 2 ^ ((b And 7) + 1)
                    .GlobalBackgroundColor = gf.ReadByte
                    gf.ReadByte()
                    If CBool(b And 128) Then
                        Dim c As Int32
                        .PicPalette = New Int32(PicGlobalColorTableSize - 1) {}
                        For n As Integer = 0 To PicGlobalColorTableSize - 1
                            c = gf.ReadByte
                            c = c << 8
                            c = c Or gf.ReadByte
                            c = c << 8
                            c = c Or gf.ReadByte
                            .PicPalette(n) = c
                        Next
                    End If
                    Dim Flame As New List(Of GifImageBlock)
                    Dim cur As GifImageBlock = GetNextImageBlock(gf)
                    While cur IsNot Nothing
                        Flame.Add(cur)
                        cur = GetNextImageBlock(gf)
                    End While
                    .Flame = Flame.ToArray
                End With
            End Using
        End Sub
        Private Shared Function GetNextImageBlock(ByVal sp As PositionedStreamPasser) As GifImageBlock
            Dim s = sp.GetStream
            Dim ret As GifImageBlock = Nothing
            Select Case s.ReadByte()
                Case GifImageBlock.ExtensionIntroducer
                    If s.ReadByte() <> GifImageBlock.ExtGraphicControlLabel Then
                        Dim Len As Integer = s.ReadByte()
                        While Len <> 0
                            s.Position += Len
                            Len = s.ReadByte()
                        End While
                        Return GetNextImageBlock(s)
                    End If
                    ret = New GifImageBlock
                    ReadExtendedImageBlock(s, ret)
                Case GifImageBlock.ImageDescriptorIntroducer
                    ret = New GifImageBlock
                    ReadNotExtendedImageBlock(s, ret)
                Case Trailer
                    Return Nothing
                Case Else
                    Throw New InvalidDataException
            End Select
            Return ret
        End Function
        Private Shared Sub ReadNotExtendedImageBlock(ByVal sp As PositionedStreamPasser, ByRef i As GifImageBlock)
            Dim s = sp.GetStream
            With i
                s.Position += 4
                .Width = s.ReadInt16
                .Height = s.ReadInt16
                Dim b As Byte = s.ReadByte
                .InterlaceFlag = CBool(b And 64)
                Dim LocalColorTableSize As Integer = 1 << ((b And 7) + 1)
                If CBool(b And 128) Then
                    Dim c As Int32
                    .Palette = New Int32(LocalColorTableSize - 1) {}
                    For n As Integer = 0 To LocalColorTableSize - 1
                        c = s.ReadByte
                        c = c << 8
                        c = c Or s.ReadByte
                        c = c << 8
                        c = c Or s.ReadByte
                        .Palette(n) = c
                    Next
                End If

                Dim CodeSize As Integer = s.ReadByte
                Dim TarBytes As New Queue(Of Byte)

                Dim Len As Integer = s.ReadByte
                While Len <> 0
                    For n As Integer = 0 To Len - 1
                        TarBytes.Enqueue(s.ReadByte)
                    Next
                    Len = s.ReadByte
                End While

                Dim LZW As New LZWCodec(CodeSize)
                Dim SrcBytes As Byte() = LZW.UnLZW(TarBytes.ToArray)

                .Rectangle = New Byte(.Width - 1, .Height - 1) {}
                If .InterlaceFlag Then
                    Dim YInBytes As Integer
                    For y As Integer = 0 To .Height - 1 Step 8
                        For x As Integer = 0 To .Width - 1
                            .Rectangle(x, y) = SrcBytes(x + YInBytes * .Width)
                        Next
                        YInBytes += 1
                    Next
                    For y As Integer = 4 To .Height - 1 Step 8
                        For x As Integer = 0 To .Width - 1
                            .Rectangle(x, y) = SrcBytes(x + YInBytes * .Width)
                        Next
                        YInBytes += 1
                    Next
                    For y As Integer = 2 To .Height - 1 Step 4
                        For x As Integer = 0 To .Width - 1
                            .Rectangle(x, y) = SrcBytes(x + YInBytes * .Width)
                        Next
                        YInBytes += 1
                    Next
                    For y As Integer = 1 To .Height - 1 Step 2
                        For x As Integer = 0 To .Width - 1
                            .Rectangle(x, y) = SrcBytes(x + YInBytes * .Width)
                        Next
                        YInBytes += 1
                    Next
                Else
                    For y As Integer = 0 To .Height - 1
                        For x As Integer = 0 To .Width - 1
                            .Rectangle(x, y) = SrcBytes(x + y * .Width)
                        Next
                    Next
                End If
            End With
        End Sub
        Private Shared Sub ReadExtendedImageBlock(ByVal sp As PositionedStreamPasser, ByRef i As GifImageBlock)
            Dim s = sp.GetStream
            With i
                .EnableControlExtension = True
                s.ReadByte()
                .TransparentColorFlag = CBool(s.ReadByte And 1)
                .DelayTime = s.ReadInt16
                .TransparentColorIndex = s.ReadByte
                s.ReadByte()
                If s.ReadByte <> GifImageBlock.ImageDescriptorIntroducer Then Throw New InvalidDataException
                ReadNotExtendedImageBlock(s, i)
            End With
        End Sub

        Public Sub WriteToFile(ByVal Path As String)
            Using gf As New StreamEx(Path, FileMode.Create)
                For n As Integer = 0 To Identifier.Length - 1
                    gf.WriteByte(CByte(AscW(Identifier(n))))
                Next
                gf.WriteInt16(PicWidth)
                gf.WriteInt16(PicHeight)
                Dim b As Byte
                Dim cr As Byte = PicBitsPerPixel - 1
                If cr >= 8 Then cr = 7
                b = b Or (cr << 4)
                If GlobalColorTableFlag Then
                    b = b Or 128
                    Dim pixel As Byte = Ceiling(Log(GlobalColorTableSize) / Log(2)) - 1
                    If pixel >= 8 Then pixel = 7
                    b = b Or pixel
                End If
                gf.WriteByte(b)
                gf.WriteByte(GlobalBackgroundColor)
                gf.WriteByte(PixelAspectRadio)
                If GlobalColorTableFlag Then
                    Dim c As Int32
                    For n As Integer = 0 To GlobalColorTableSize - 1
                        c = PicPalette(n)
                        gf.WriteByte((c >> 16) And 255)
                        gf.WriteByte((c >> 8) And 255)
                        gf.WriteByte(c And 255)
                    Next
                End If
                If Flame IsNot Nothing Then
                    For Each i As GifImageBlock In Flame
                        WriteImageBlock(gf, i)
                    Next
                End If
                gf.WriteByte(Trailer)
            End Using
        End Sub
        Private Sub WriteImageBlock(ByVal sp As PositionedStreamPasser, ByVal i As GifImageBlock)
            Dim s = sp.GetStream
            With i
                If .EnableControlExtension Then
                    s.WriteByte(GifImageBlock.ExtensionIntroducer)
                    s.WriteByte(GifImageBlock.ExtGraphicControlLabel)
                    s.WriteByte(4)
                    If .TransparentColorFlag Then
                        s.WriteByte(1 Or (GifImageBlock.DisposalMethod << 2))
                    Else
                        s.WriteByte(0 Or (GifImageBlock.DisposalMethod << 2))
                    End If
                    s.WriteInt16(.DelayTime)
                    s.WriteByte(.TransparentColorIndex)
                    s.WriteByte(GifImageBlock.BlockTerminator)
                End If
                s.WriteByte(GifImageBlock.ImageDescriptorIntroducer)
                s.WriteInt16(0)
                s.WriteInt16(0)
                s.WriteInt16(.Width)
                s.WriteInt16(.Height)
                Dim b As Byte
                If .InterlaceFlag Then b = b Or 64
                If .LocalColorTableFlag Then
                    b = b Or 128
                    Dim pixel As Byte = Ceiling(Log(.LocalColorTableSize) / Log(2)) - 1
                    If pixel >= 8 Then pixel = 7
                    b = b Or pixel
                End If
                s.WriteByte(b)
                If .LocalColorTableFlag Then
                    Dim c As Int32
                    For n As Integer = 0 To .LocalColorTableSize - 1
                        c = .Palette(n)
                        s.WriteByte((c >> 16) And 255)
                        s.WriteByte((c >> 8) And 255)
                        s.WriteByte(c And 255)
                    Next
                End If

                Dim CodeSize As Integer
                If PicBitsPerPixel <> 1 Then
                    CodeSize = PicBitsPerPixel
                Else
                    CodeSize = 2
                End If
                s.WriteByte(CodeSize)

                Dim SrcBytes As Byte() = New Byte(CInt(.Width) * CInt(.Height) - 1) {}

                If .InterlaceFlag Then
                    Dim YInBytes As Integer
                    For y As Integer = 0 To .Height - 1 Step 8
                        For x As Integer = 0 To .Width - 1
                            SrcBytes(x + YInBytes * .Width) = .Rectangle(x, y)
                        Next
                        YInBytes += 1
                    Next
                    For y As Integer = 4 To .Height - 1 Step 8
                        For x As Integer = 0 To .Width - 1
                            SrcBytes(x + YInBytes * .Width) = .Rectangle(x, y)
                        Next
                        YInBytes += 1
                    Next
                    For y As Integer = 2 To .Height - 1 Step 4
                        For x As Integer = 0 To .Width - 1
                            SrcBytes(x + YInBytes * .Width) = .Rectangle(x, y)
                        Next
                        YInBytes += 1
                    Next
                    For y As Integer = 1 To .Height - 1 Step 2
                        For x As Integer = 0 To .Width - 1
                            SrcBytes(x + YInBytes * .Width) = .Rectangle(x, y)
                        Next
                        YInBytes += 1
                    Next
                Else
                    For y As Integer = 0 To .Height - 1
                        For x As Integer = 0 To .Width - 1
                            SrcBytes(x + y * .Width) = .Rectangle(x, y)
                        Next
                    Next
                End If

                Dim LZW As New LZWCodec(CodeSize)
                Dim TarBytes As New Queue(Of Byte)(LZW.LZW(SrcBytes))
                While TarBytes.Count > 254
                    s.WriteByte(254)
                    For n As Integer = 0 To 253
                        s.WriteByte(TarBytes.Dequeue)
                    Next
                End While
                If TarBytes.Count > 0 Then
                    s.WriteByte(TarBytes.Count)
                    For n As Integer = 0 To TarBytes.Count - 1
                        s.WriteByte(TarBytes.Dequeue)
                    Next
                End If
                s.WriteByte(0)
            End With
        End Sub

        'End Of Class
        'Start Of SubClasses

        Public Class LZWCodec
            Private StartCodeSize As Integer
            Private CodeSize As Integer
            Private SrcBytes As List(Of Byte)
            Private SrcPos As Integer
            Private SrcReadEnd As Boolean
            Private TarBytes As List(Of Byte)
            Private TarPos As Integer
            Private TarReadEnd As Boolean
            Public Sub New(ByVal StartCodeSize As Integer)
                If StartCodeSize <= 0 Then Throw New InvalidDataException
                Me.StartCodeSize = StartCodeSize
            End Sub
            Private Function ReadSrc() As Byte
                '读取SrcBytes直到一字节也读不出来
                If SrcPos <= SrcBytes.Count - 1 Then
                    Dim ret As Byte = SrcBytes(SrcPos)
                    SrcPos += 1
                    Return ret
                Else
                    SrcPos += 1
                    SrcReadEnd = True
                    Return 0
                End If
            End Function
            Private Function ReadTar() As Int16
                '读取TarBytes直到一位也读不出来
                Dim r As Integer
                Dim d As Integer = DivRem(TarPos, 8, r)
                TarPos += CodeSize + 1
                If d > TarBytes.Count - 1 Then
                    TarReadEnd = True
                    Return 0
                End If
                Dim ret As Int32 = TarBytes(d)
                ret = ret >> r
                If r + (CodeSize + 1) <= 8 Then
                    Return ret And (2 ^ (CodeSize + 1) - 1)
                ElseIf d + 1 > TarBytes.Count - 1 Then
                    Return ret
                End If
                ret = ret Or (CInt(TarBytes(d + 1)) << (8 - r))
                If r + (CodeSize + 1) <= 16 Then
                    Return ret And (2 ^ (CodeSize + 1) - 1)
                ElseIf d + 2 > TarBytes.Count - 1 Then
                    Return ret
                End If
                ret = ret Or (CInt(TarBytes(d + 2)) << (16 - r))
                Return ret And (2 ^ (CodeSize + 1) - 1)
            End Function
            Private Sub WriteSrc(ByVal b As String)
                For n As Integer = 0 To b.Length - 1
                    SrcBytes.Add(AscW(b(n)))
                Next
                SrcPos += b.Length
            End Sub
            Private Sub WriteTar(ByVal i As Int16)
                Dim r As Integer
                Dim d As Integer = DivRem(TarPos, 8, r)
                TarPos += CodeSize + 1
                If d > TarBytes.Count - 1 Then TarBytes.Add(0)
                TarBytes(d) = TarBytes(d) Or ((i << r) And 255)
                If r + (CodeSize + 1) <= 8 Then Return
                If d + 1 > TarBytes.Count - 1 Then TarBytes.Add(0)
                i = i >> (8 - r)
                TarBytes(d + 1) = i And 255
                If r + (CodeSize + 1) <= 16 Then Return
                If d + 2 > TarBytes.Count - 1 Then TarBytes.Add(0)
                i = i >> 8
                TarBytes(d + 2) = i And 255
            End Sub
            Public Function LZW(ByVal SrcBytes As Byte()) As Byte()
                Me.SrcBytes = New List(Of Byte)(SrcBytes)
                Me.TarBytes = New List(Of Byte)()
                CodeSize = StartCodeSize
                SrcPos = 0
                TarPos = 0
                TarReadEnd = False
                Dim Table As New List(Of String)
                Dim RTable As New Dictionary(Of String, Integer)
                For n As Integer = 0 To (1 << (StartCodeSize)) - 1
                    Table.Add(ChrW(n))
                    RTable.Add(ChrW(n), n)
                Next
                Table.Add("CC")
                Table.Add("EC")
                Dim ClearCode As Int16 = 1 << (StartCodeSize)
                Dim OverflowCode As Int16 = 1 << (StartCodeSize + 1)

                Dim Prefix As String = ""
                Dim IndexOfPrefix As Int16
                Dim Root As Char
                WriteTar(1 << StartCodeSize)

                Dim CurStr As String

                While True
                    Root = ChrW(ReadSrc())
                    If SrcReadEnd Then Exit While
                    CurStr = Prefix & Root

                    If RTable.ContainsKey(CurStr) Then
                        IndexOfPrefix = RTable(CurStr)
                        Prefix = CurStr
                    Else
                        WriteTar(IndexOfPrefix)
                        If Table.Count = OverflowCode Then
                            If OverflowCode = 4096 Then
                                WriteTar(AscW(Root))
                                WriteTar(1 << StartCodeSize)
                                Table.RemoveRange((1 << (StartCodeSize)) + 2, Table.Count - (1 << (StartCodeSize)) - 2)
                                RTable.Clear()
                                For n As Integer = 0 To (1 << (StartCodeSize)) - 1
                                    RTable.Add(ChrW(n), n)
                                Next
                                CodeSize = StartCodeSize
                                OverflowCode = 1 << (StartCodeSize + 1)
                                Prefix = ""
                                Continue While
                            Else
                                CodeSize += 1
                                OverflowCode = OverflowCode << 1
                            End If
                        End If
                        Table.Add(CurStr)
                        RTable.Add(CurStr, Table.Count - 1)
                        Prefix = Root
                        IndexOfPrefix = AscW(Root)
                    End If
                End While
                WriteTar(IndexOfPrefix)
                WriteTar(ClearCode + 1)

                Return TarBytes.ToArray
            End Function
            Public Function UnLZW(ByVal TarBytes As Byte()) As Byte()
                Me.SrcBytes = New List(Of Byte)()
                Me.TarBytes = New List(Of Byte)(TarBytes)
                CodeSize = StartCodeSize
                SrcPos = 0
                TarPos = 0
                TarReadEnd = False
                Dim Table As New List(Of String)
                For n As Integer = 0 To (1 << (StartCodeSize)) - 1
                    Table.Add(ChrW(n))
                Next
                Table.Add("CC")
                Table.Add("EC")
                Dim ClearCode As Int16 = 1 << (StartCodeSize)
                Dim OverflowCode As Int16 = 1 << (StartCodeSize + 1)
                Dim CurStr As String
                Dim cur As Int16 = ReadTar()
                cur = ReadTar()
                WriteSrc(Table(cur))
                Dim old As Int16 = cur
                cur = ReadTar()
                While True
                    Select Case cur
                        Case ClearCode
                            Table.RemoveRange((1 << (StartCodeSize)) + 2, Table.Count - (1 << (StartCodeSize)) - 2)
                            CodeSize = StartCodeSize
                            OverflowCode = 1 << (StartCodeSize + 1)
                            cur = ReadTar()
                            WriteSrc(Table(cur))
                        Case ClearCode + 1
                            Exit While
                        Case Else
                            If cur <= Table.Count - 1 Then
                                WriteSrc(Table(cur))
                                Table.Add(Table(old) & Table(cur)(0))
                            Else
                                CurStr = Table(old) & Table(old)(0)
                                WriteSrc(CurStr)
                                Table.Add(CurStr)
                            End If
                            If Table.Count = OverflowCode AndAlso OverflowCode <> 4096 Then
                                CodeSize += 1
                                OverflowCode = OverflowCode << 1
                            End If
                    End Select
                    If TarReadEnd Then Exit While
                    old = cur
                    cur = ReadTar()
                End While
                Dim ret As Byte() = SrcBytes.ToArray
                Me.SrcBytes = Nothing
                Me.TarBytes = Nothing
                Return ret
            End Function
        End Class

        Public Class GifImageBlock
            Public EnableControlExtension As Boolean
            Public Const ExtensionIntroducer As Byte = &H21
            Public Const ExtGraphicControlLabel As Byte = &HF9
            Public Const ExtBlockSize As Byte = 4
            Public Const DisposalMethod As Byte = 2
            Public TransparentColorFlag As Boolean
            Public DelayTime As Int16 '/0.01s
            Public TransparentColorIndex As Byte
            Public Const BlockTerminator As Byte = 0

            Public Const ImageDescriptorIntroducer As Byte = &H2C
            Public Width As Int16
            Public Height As Int16
            Public ReadOnly Property LocalColorTableFlag() As Boolean
                Get
                    Return Palette IsNot Nothing
                End Get
            End Property
            Public InterlaceFlag As Boolean = True
            Public ReadOnly Property LocalColorTableSize() As Integer
                Get
                    If Palette Is Nothing Then Return 0
                    Return Palette.GetLength(0)
                End Get
            End Property
            Public Palette As Int32()
            Public Rectangle As Byte(,)
            Public Sub New()
            End Sub
            Public Sub New(ByVal Rectangle As Byte(,), Optional ByVal Palette As Int32() = Nothing)
                If Rectangle Is Nothing Then Throw New InvalidDataException
                Me.Rectangle = Rectangle
                Width = Rectangle.GetLength(0)
                Height = Rectangle.GetLength(1)
                If Palette IsNot Nothing Then Me.Palette = Palette.Clone
            End Sub
            ''' <param name="DelayTime">单位为0.01s</param>
            Public Sub SetControl(ByVal DelayTime As Int16)
                EnableControlExtension = True
                Me.DelayTime = DelayTime
            End Sub
            ''' <param name="DelayTime">单位为0.01s</param>
            Public Sub SetControl(ByVal DelayTime As Int16, ByVal TransparentColorIndex As Byte)
                EnableControlExtension = True
                TransparentColorFlag = True
                Me.DelayTime = DelayTime
                Me.TransparentColorIndex = TransparentColorIndex
            End Sub
        End Class
    End Class
End Namespace
