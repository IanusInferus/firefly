'==========================================================================
'
'  File:        Bmp.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: 基本Bmp文件流类
'  Version:     2018.09.09.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Drawing
Imports System.IO
Imports Firefly.TextEncoding
Imports Firefly.Streaming

Namespace Imaging

    ''' <summary>基本Bmp文件流类</summary>
    ''' <remarks>不能使用压缩等无用功能</remarks>
    Public NotInheritable Class Bmp
        Implements IDisposable

        ''' <summary>标志符。</summary>
        Public Const Identifier As String = "BM"
        ''' <summary>文件大小。</summary>
        ReadOnly Property FileSize() As Int32
            Get
                Return BitmapDataOffset + BitmapDataSize
            End Get
        End Property
        Private Const Reserved As Int32 = 0
        ''' <summary>位图数据偏移量。</summary>
        Public ReadOnly Property BitmapDataOffset() As Int32
            Get
                If (PicBitsPerPixel = 1) OrElse (PicBitsPerPixel = 4) OrElse (PicBitsPerPixel = 8) Then
                    Return 54 + (1 << PicBitsPerPixel) * 4
                ElseIf PicBitsPerPixel = 16 Then
                    Return 70
                Else
                    Return 54
                End If
            End Get
        End Property
        Private Const BitmapHeaderSize As Int32 = &H28
        Private PicWidth As Int32
        ''' <summary>宽度。</summary>
        Public Property Width() As Int32
            Get
                Return PicWidth
            End Get
            Set(ByVal value As Int32)
                If value >= 0 Then PicWidth = value Else Return
                CalcLineBitLength()
                Writable.SetLength(FileSize)
                Writable.Position = 18
                Writable.WriteInt32(PicWidth)
            End Set
        End Property
        Private LineBitLength As Int32
        Private Sub CalcLineBitLength()
            If (PicWidth * PicBitsPerPixel) Mod 32 <> 0 Then
                LineBitLength = (((PicWidth * PicBitsPerPixel) >> 5) + 1) << 5
            Else
                LineBitLength = PicWidth * PicBitsPerPixel
            End If
        End Sub
        Private PicHeight As Int32
        ''' <summary>高度。</summary>
        Public Property Height() As Int32
            Get
                Return PicHeight
            End Get
            Set(ByVal value As Int32)
                If value >= 0 Then PicHeight = value Else Return
                Writable.SetLength(FileSize)
                Writable.Position = 22
                Writable.WriteInt32(PicHeight)
            End Set
        End Property
        Private Const Planes As Int16 = 1
        Private PicBitsPerPixel As Int16
        ''' <summary>位深度。</summary>
        Public ReadOnly Property BitsPerPixel() As Int16
            Get
                If PicBitsPerPixel = 16 AndAlso Not r5g6b5 Then Return 15
                Return PicBitsPerPixel
            End Get
        End Property
        Private PicCompression As Int32
        ''' <summary>压缩方式。</summary>
        Public ReadOnly Property Compression() As Int32
            Get
                Return PicCompression
            End Get
        End Property
        ''' <summary>位图数据大小。</summary>
        Public ReadOnly Property BitmapDataSize() As Int32
            Get
                Return (LineBitLength * PicHeight) >> 3
            End Get
        End Property
        Private Const HResolution As Int32 = 0 '不用
        Private Const VResolution As Int32 = 0 '不用
        Private Const Colors As Int32 = 0
        Private Const ImportantColors As Int32 = 0
        Private PicPalette As Int32()
        ''' <summary>调色板。</summary>
        Public Property Palette() As Int32()
            Get
                Dim Value As Int32()
                If (PicBitsPerPixel = 1) OrElse (PicBitsPerPixel = 4) OrElse (PicBitsPerPixel = 8) Then
                    Value = New Int32((1 << PicBitsPerPixel) - 1) {}
                    Readable.Position = &H36
                    For n As Integer = 0 To (1 << PicBitsPerPixel) - 1
                        Value(n) = Readable.ReadInt32
                    Next
                Else
                    Throw New InvalidDataException
                End If
                PicPalette = Value
                Return Value
            End Get
            Set(ByVal Value As Int32())
                If (PicBitsPerPixel = 1) OrElse (PicBitsPerPixel = 4) OrElse (PicBitsPerPixel = 8) Then
                    If Value.Length <> 1 << PicBitsPerPixel Then Throw New InvalidDataException
                    Writable.Position = &H36
                    For n As Integer = 0 To (1 << PicBitsPerPixel) - 1
                        Writable.WriteInt32(Value(n))
                    Next
                Else
                    Throw New InvalidDataException
                End If
                PicPalette = CType(Value.Clone, Int32())
            End Set
        End Property

        Private Readable As IReadableSeekableStream
        Private Writable As IStream

        Private r5g6b5 As Boolean
        Private Sub New()
        End Sub

        ''' <summary>新建Bmp</summary>
        ''' <param name="BitsPerPixel">Bmp位数：可以取1、4、8、15、16、24、32</param>
        ''' <remarks>注意，流在Bmp关闭时会被关闭。</remarks>
        Public Sub New(ByVal sp As NewWritingStreamPasser, ByVal Width As Int32, ByVal Height As Int32, Optional ByVal BitsPerPixel As Int16 = 24)
            Dim s = sp.GetStream
            Readable = s
            Writable = s
            If Width < 0 OrElse Height < 0 Then
                Writable.Dispose()
                Throw New InvalidDataException
            End If
            PicWidth = Width
            PicHeight = Height
            If (BitsPerPixel = 1) OrElse (BitsPerPixel = 4) OrElse (BitsPerPixel = 8) Then
                PicBitsPerPixel = BitsPerPixel
                PicPalette = New Int32(CInt(2 ^ (PicBitsPerPixel)) - 1) {}
            ElseIf (BitsPerPixel = 15) OrElse (BitsPerPixel = 16) Then
                r5g6b5 = (BitsPerPixel = 16)
                PicBitsPerPixel = 16
                PicCompression = 3
            ElseIf (BitsPerPixel = 24) OrElse (BitsPerPixel = 32) Then
                PicBitsPerPixel = BitsPerPixel
            Else
                Writable.Dispose()
                Throw New NotSupportedException("PicBitsPerPixelNotSupported")
            End If
            CalcLineBitLength()
            Writable.SetLength(FileSize)

            Writable.Position = 0
            For n As Integer = 0 To Identifier.Length - 1
                Writable.WriteByte(CByte(AscQ(Identifier(n))))
            Next
            Writable.WriteInt32(FileSize)
            Writable.WriteInt32(Reserved)
            Writable.WriteInt32(BitmapDataOffset)
            Writable.WriteInt32(BitmapHeaderSize)
            Writable.WriteInt32(PicWidth)
            Writable.WriteInt32(PicHeight)
            Writable.WriteInt16(Planes)
            Writable.WriteInt16(PicBitsPerPixel)
            Writable.WriteInt32(PicCompression)
            Writable.WriteInt32(BitmapDataSize)
            Writable.WriteInt32(HResolution)
            Writable.WriteInt32(VResolution)
            Writable.WriteInt32(Colors)
            Writable.WriteInt32(ImportantColors)

            If (PicCompression = 3) AndAlso (PicBitsPerPixel = 16) Then
                If r5g6b5 Then
                    Writable.WriteInt32(&HF800)
                    Writable.WriteInt32(&H7E0)
                    Writable.WriteInt32(&H1F)
                    Writable.WriteInt32(&H0)
                Else
                    Writable.WriteInt32(&H7C00)
                    Writable.WriteInt32(&H3E0)
                    Writable.WriteInt32(&H1F)
                    Writable.WriteInt32(&H0)
                End If
            End If
        End Sub
        ''' <summary>新建内存流Bmp</summary>
        ''' <param name="BitsPerPixel">Bmp位数：可以取1、4、8、15、16、24、32</param>
        Public Sub New(ByVal Width As Int32, ByVal Height As Int32, Optional ByVal BitsPerPixel As Int16 = 24)
            Me.New(Streams.CreateMemoryStream.AsNewWriting, Width, Height, BitsPerPixel)
        End Sub
        ''' <summary>新建文件流Bmp</summary>
        ''' <param name="BitsPerPixel">Bmp位数：可以取1、4、8、15、16、24、32</param>
        Public Sub New(ByVal Path As String, ByVal Width As Int32, ByVal Height As Int32, Optional ByVal BitsPerPixel As Int16 = 24)
            Me.New(Streams.CreateResizable(Path).AsNewWriting, Width, Height, BitsPerPixel)
        End Sub

        ''' <summary>已重载。从流打开一个位图。</summary>
        Public Shared Function Open(ByVal sp As NewReadingStreamPasser) As Bmp
            Dim s = sp.GetStream
            Dim bf As New Bmp
            Dim Success = False
            Try
                With bf
                    .Readable = s
                    .Readable.Position = 0
                    For n As Integer = 0 To Identifier.Length - 1
                        If .Readable.ReadByte() <> AscQ(Identifier(n)) Then
                            Throw New InvalidDataException
                        End If
                    Next
                    .Readable.ReadInt32() '跳过File Size
                    .Readable.ReadInt32() '跳过Reserved
                    .Readable.ReadInt32() '跳过Bitmap Data Offset
                    .Readable.ReadInt32() '跳过Bitmap Header Size
                    .PicWidth = .Readable.ReadInt32
                    .PicHeight = .Readable.ReadInt32
                    If .PicWidth < 0 OrElse .PicHeight < 0 Then
                        Throw New InvalidDataException
                    End If
                    .Readable.ReadInt16() '跳过Planes
                    .PicBitsPerPixel = .Readable.ReadInt16
                    .PicCompression = .Readable.ReadInt32
                    .Readable.ReadInt32() '跳过Bitmap Data Size
                    .Readable.ReadInt32() '跳过HResolution
                    .Readable.ReadInt32() '跳过VResolution
                    .Readable.ReadInt32() '跳过Colors
                    .Readable.ReadInt32() '跳过Important Colors

                    If .PicCompression <> 0 Then
                        If (.PicCompression = 3) AndAlso (.PicBitsPerPixel = 16) Then
                            .r5g6b5 = CBool(.Readable.ReadInt32() And &H8000) '检验红色掩码是否从最高位开始
                            .Readable.ReadInt32() '跳过绿色掩码
                            .Readable.ReadInt32() '跳过蓝色掩码
                            .Readable.ReadInt32()
                        Else
                            Throw New InvalidDataException
                        End If
                    End If

                    If (.PicBitsPerPixel = 1) OrElse (.PicBitsPerPixel = 4) OrElse (.PicBitsPerPixel = 8) Then
                        .PicPalette = New Int32((1 << .PicBitsPerPixel) - 1) {}
                        For n As Integer = 0 To (1 << .PicBitsPerPixel) - 1
                            .PicPalette(n) = .Readable.ReadInt32()
                        Next
                    ElseIf (.PicBitsPerPixel = 16) OrElse (.PicBitsPerPixel = 24) OrElse (.PicBitsPerPixel = 32) Then
                    Else
                        Throw New NotSupportedException("PicBitsPerPixelNotSupported")
                    End If

                    .CalcLineBitLength()
                End With
                Success = True
                Return bf
            Finally
                If Not Success Then
                    bf.Dispose()
                End If
            End Try
        End Function
        ''' <summary>已重载。从流打开一个位图。</summary>
        Public Shared Function Open(ByVal sp As NewReadingWritingStreamPasser) As Bmp
            Dim s = sp.GetStream
            Dim bf = Open(s.AsNewReading)
            bf.Writable = s
            Return bf
        End Function
        ''' <summary>已重载。从文件打开一个位图。</summary>
        Public Shared Function Open(ByVal Path As String) As Bmp
            Try
                Dim s = Streams.OpenResizable(Path)
                Try
                    Return Open(s.AsNewReadingWriting)
                Catch
                    s.Dispose()
                    Throw
                End Try
            Catch
                Dim s = Streams.OpenReadable(Path)
                Try
                    Return Open(s.AsNewReading)
                Catch
                    s.Dispose()
                    Throw
                End Try
            End Try
        End Function
        ''' <summary>转换为System.Drawing.Bitmap。</summary>
        Public Function ToBitmap() As Bitmap
            Readable.Position = 0
            Readable.Flush()
            Return New Bitmap(Readable.ToStream)
        End Function
        ''' <summary>保存到流。</summary>
        Public Sub SaveTo(ByVal sp As NewWritingStreamPasser)
            Dim s = sp.GetStream
            Readable.Position = 0
            s.WriteFromStream(Readable, Readable.Length)
        End Sub

        Private ReadOnly Property Pos(ByVal x As Int32, ByVal y As Int32) As Integer
            Get
                Return (LineBitLength * (Height - 1 - y) + x * PicBitsPerPixel) >> 3
            End Get
        End Property
        ''' <summary>获得像素点。</summary>
        Public Function GetPixel(ByVal x As Int32, ByVal y As Int32) As Int32
            If x < 0 OrElse x > PicWidth - 1 OrElse y < 0 OrElse y > PicHeight - 1 Then Return 0
            Readable.Position = BitmapDataOffset + Pos(x, y)
            Select Case PicBitsPerPixel
                Case 1
                    Return (Readable.ReadByte >> (7 - x Mod 8)) And 1
                Case 4
                    Return (Readable.ReadByte >> (4 * (1 - x Mod 2))) And 15
                Case 8
                    Return Readable.ReadByte
                Case 16
                    Return Readable.ReadInt16
                Case 24
                    Return Readable.ReadInt32 And &HFFFFFF
                Case 32
                    Return Readable.ReadInt32
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        ''' <summary>设置像素点。</summary>
        Public Sub SetPixel(ByVal x As Int32, ByVal y As Int32, ByVal c As Int32)
            If x < 0 OrElse x > PicWidth - 1 OrElse y < 0 OrElse y > PicHeight - 1 Then Return
            Writable.Position = BitmapDataOffset + Pos(x, y)
            Select Case PicBitsPerPixel
                Case 1
                    Dim k As Byte = Writable.ReadByte
                    k = k And Not CByte(1 << (7 - x Mod 8)) Or CByte((CByte(c <> 0) And 1) << (7 - x Mod 8))
                    Writable.Position -= 1
                    Writable.WriteByte(k)
                Case 4
                    Dim k As Byte = Writable.ReadByte
                    k = k And Not CByte(15 << (4 * (1 - x Mod 2))) Or CByte((c And 15) << (4 * (1 - x Mod 2)))
                    Writable.Position -= 1
                    Writable.WriteByte(k)
                Case 8
                    Writable.WriteByte(CByte(c And &HFF))
                Case 16
                    Writable.WriteInt16(CID(c And &HFFFF))
                Case 24
                    Writable.WriteInt16(CID(c And &HFFFF))
                    Writable.WriteByte(CByte((c >> 16) And &HFF))
                Case 32S
                    Writable.WriteInt32(c)
            End Select
        End Sub
        ''' <summary>获取矩形。</summary>
        Public Function GetRectangle(ByVal x As Int32, ByVal y As Int32, ByVal w As Int32, ByVal h As Int32) As Int32(,)
            If w < 0 OrElse h < 0 Then Return Nothing
            Dim a = New Int32(w - 1, h - 1) {}
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, PicHeight) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, PicWidth) - x
            For j As Integer = je - 1 To jb Step -1
                Readable.Position = BitmapDataOffset + Pos(x + ib, y + j)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If ((x + ib) And 7) <> 0 Then
                            t = Readable.ReadByte
                            t = t << ((x + ib) And 7)
                        End If
                        For i = ib To ie - 1
                            If (i And 7) = 0 Then
                                t = Readable.ReadByte
                            End If
                            a(i, j) = (t And 128) >> 7
                            t = t << 1
                        Next
                    Case 4
                        Dim t As Byte
                        If ((x + ib) And 1) = 1 Then
                            t = Readable.ReadByte
                        End If
                        For i = ib To ie - 1
                            If (i And 1) = 0 Then
                                t = Readable.ReadByte
                                a(i, j) = t >> 4
                            Else
                                a(i, j) = t And 15
                            End If
                        Next
                    Case 8
                        For i = ib To ie - 1
                            a(i, j) = Readable.ReadByte
                        Next
                    Case 16
                        For i = ib To ie - 1
                            a(i, j) = EID(Readable.ReadInt16)
                        Next
                    Case 24
                        For i = ib To ie - 1
                            Dim c = EID(Readable.ReadInt16)
                            c = c Or (CInt(Readable.ReadByte) << 16)
                            c = &HFF000000 Or c
                            a(i, j) = c
                        Next
                    Case 32
                        For i = ib To ie - 1
                            a(i, j) = Readable.ReadInt32
                        Next
                End Select
            Next
            Return a
        End Function
        ''' <summary>获取矩形。表示为字节。仅供8位及以下图片使用。</summary>
        Public Function GetRectangleBytes(ByVal x As Int32, ByVal y As Int32, ByVal w As Int32, ByVal h As Int32) As Byte(,)
            If w < 0 OrElse h < 0 Then Return Nothing
            Dim a = New Byte(w - 1, h - 1) {}
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, PicHeight) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, PicWidth) - x
            For j As Integer = je - 1 To jb Step -1
                Readable.Position = BitmapDataOffset + Pos(x + ib, y + j)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If ((x + ib) And 7) <> 0 Then
                            t = Readable.ReadByte
                            t = t << ((x + ib) And 7)
                        End If
                        For i = ib To ie - 1
                            If (i And 7) = 0 Then
                                t = Readable.ReadByte
                            End If
                            a(i, j) = CByte((t And 128) >> 7)
                            t = t << 1
                        Next
                    Case 4
                        Dim t As Byte
                        If ((x + ib) And 1) = 1 Then
                            t = Readable.ReadByte
                        End If
                        For i = ib To ie - 1
                            If (i And 1) = 0 Then
                                t = Readable.ReadByte
                                a(i, j) = t >> 4
                            Else
                                a(i, j) = CByte(t And 15)
                            End If
                        Next
                    Case 8
                        For i = ib To ie - 1
                            a(i, j) = Readable.ReadByte
                        Next
                    Case 16, 24, 32
                        Throw New InvalidOperationException
                End Select
            Next
            Return a
        End Function
        ''' <summary>已重载。设置矩形。</summary>
        Public Sub SetRectangle(ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,))
            If a Is Nothing Then Return
            Dim w = a.GetLength(0)
            Dim h = a.GetLength(1)
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, PicHeight) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, PicWidth) - x
            For j As Integer = je - 1 To jb Step -1
                Writable.Position = BitmapDataOffset + Pos(x + ib, y + j)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If ((x + ib) And 7) <> 0 Then
                            t = Writable.ReadByte >> (8 - ((x + ib) And 7))
                            Writable.Position -= 1
                        End If
                        For i = ib To ie - 1
                            t = t << 1
                            If a(i, j) <> 0 Then t = t Or CByte(1)
                            If (i And 7) = 7 Then
                                Writable.WriteByte(t)
                            End If
                        Next
                        If ((x + ie - 1) And 7) <> 0 Then
                            t = t << (7 - ((x + ie - 1) And 7))
                            Writable.WriteByte(t)
                        End If
                    Case 4
                        Dim t As Byte
                        If ((x + ib) And 1) <> 0 Then
                            t = Writable.ReadByte >> (4 * (2 - ((x + ib) And 1)))
                            Writable.Position -= 1
                        End If
                        For i = ib To ie - 1
                            t = t << 4
                            t = t Or CByte(a(i, j) And 15)
                            If (i And 1) = 1 Then
                                Writable.WriteByte(t)
                            End If
                        Next
                        If ((x + ie - 1) And 1) <> 0 Then
                            t = t << 4
                            Writable.WriteByte(t)
                        End If
                    Case 8
                        For i = ib To ie - 1
                            Writable.WriteByte(CByte(a(i, j) And &HFF))
                        Next
                    Case 16
                        For i = ib To ie - 1
                            Writable.WriteInt16(CID(a(i, j) And &HFFFF))
                        Next
                    Case 24
                        For i = ib To ie - 1
                            Dim c = a(i, j)
                            Writable.WriteInt16(CID(c And &HFFFF))
                            Writable.WriteByte(CByte((c >> 16) And &HFF))
                        Next
                    Case 32
                        For i = ib To ie - 1
                            Writable.WriteInt32(a(i, j))
                        Next
                End Select
            Next
        End Sub
        ''' <summary>已重载。设置矩形。</summary>
        Public Sub SetRectangle(ByVal x As Int32, ByVal y As Int32, ByVal a As Byte(,))
            If a Is Nothing Then Return
            Dim w = a.GetLength(0)
            Dim h = a.GetLength(1)
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, PicHeight) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, PicWidth) - x
            For j As Integer = je - 1 To jb Step -1
                Writable.Position = BitmapDataOffset + Pos(x + ib, y + j)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If ((x + ib) And 7) <> 0 Then
                            t = Writable.ReadByte >> (8 - ((x + ib) And 7))
                            Writable.Position -= 1
                        End If
                        For i = ib To ie - 1
                            t = t << 1
                            If a(i, j) <> 0 Then t = t Or CByte(1)
                            If (i And 7) = 7 Then
                                Writable.WriteByte(t)
                            End If
                        Next
                        If ((x + ie - 1) And 7) <> 0 Then
                            t = t << (7 - ((x + ie - 1) And 7))
                            Writable.WriteByte(t)
                        End If
                    Case 4
                        Dim t As Byte
                        If ((x + ib) And 1) <> 0 Then
                            t = Writable.ReadByte >> (4 * (2 - ((x + ib) And 1)))
                            Writable.Position -= 1
                        End If
                        For i = ib To ie - 1
                            t = t << 4
                            t = t Or CByte(a(i, j) And 15)
                            If (i And 1) = 1 Then
                                Writable.WriteByte(t)
                            End If
                        Next
                        If ((x + ie - 1) And 1) <> 0 Then
                            t = t << 4
                            Writable.WriteByte(t)
                        End If
                    Case 8
                        For i = ib To ie - 1
                            Writable.WriteByte(CByte(a(i, j) And &HFF))
                        Next
                    Case 16, 24, 32
                        Throw New InvalidOperationException
                End Select
                Next
        End Sub
        ''' <summary>获取矩形为ARGB整数。对非24、32位位图会进行转换。</summary>
        Public Function GetRectangleAsARGB(ByVal x As Int32, ByVal y As Int32, ByVal w As Int32, ByVal h As Int32) As Int32(,)
            Dim a = GetRectangle(x, y, w, h)
            Select Case PicBitsPerPixel
                Case 1, 4, 8
                    For py = 0 To h - 1
                        For px = 0 To w - 1
                            a(px, py) = PicPalette(a(px, py))
                        Next
                    Next
                Case 16
                    If r5g6b5 Then
                        For py = 0 To h - 1
                            For px = 0 To w - 1
                                a(px, py) = ColorSpace.RGB16To32(CID(a(px, py)))
                            Next
                        Next
                    Else
                        For py = 0 To h - 1
                            For px = 0 To w - 1
                                a(px, py) = ColorSpace.RGB15To32(CID(a(px, py)))
                            Next
                        Next
                    End If
                Case 24, 32
            End Select
            Return a
        End Function
        ''' <summary>从ARGB整数设置矩形。对非24、32位位图会进行转换。使用自定义的量化器。</summary>
        Public Sub SetRectangleFromARGB(ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,), ByVal Quantize As Func(Of Int32, Byte))
            If a Is Nothing Then Return
            Dim w = a.GetLength(0)
            Dim h = a.GetLength(1)
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, PicHeight) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, PicWidth) - x
            For j As Integer = je - 1 To jb Step -1
                Writable.Position = BitmapDataOffset + Pos(x + ib, y + j)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If ((x + ib) And 7) <> 0 Then
                            t = Writable.ReadByte >> (8 - ((x + ib) And 7))
                            Writable.Position -= 1
                        End If
                        For i = ib To ie - 1
                            t = t << 1
                            If Quantize(a(i, j)) <> 0 Then t = t Or CByte(1)
                            If (i And 7) = 7 Then
                                Writable.WriteByte(t)
                            End If
                        Next
                        If ((x + ie - 1) And 7) <> 0 Then
                            t = t << (7 - ((x + ie - 1) And 7))
                            Writable.WriteByte(t)
                        End If
                    Case 4
                        Dim t As Byte
                        If ((x + ib) And 1) <> 0 Then
                            t = Writable.ReadByte >> (4 * (2 - ((x + ib) And 1)))
                            Writable.Position -= 1
                        End If
                        For i = ib To ie - 1
                            t = t << 4
                            t = t Or CByte(Quantize(a(i, j)) And 15)
                            If (i And 1) = 1 Then
                                Writable.WriteByte(t)
                            End If
                        Next
                        If ((x + ie - 1) And 1) <> 0 Then
                            t = t << 4
                            Writable.WriteByte(t)
                        End If
                    Case 8
                        For i = ib To ie - 1
                            Writable.WriteByte(CByte(Quantize(a(i, j)) And &HFF))
                        Next
                    Case 16
                        If r5g6b5 Then
                            For i = ib To ie - 1
                                Writable.WriteInt16(CID(ColorSpace.RGB32To16(a(i, j)) And &HFFFF))
                            Next
                        Else
                            For i = ib To ie - 1
                                Writable.WriteInt16(CID(ColorSpace.RGB32To15(a(i, j)) And &HFFFF))
                            Next
                        End If
                    Case 24
                        For i = ib To ie - 1
                            Dim c = a(i, j)
                            Writable.WriteInt16(CID(c And &HFFFF))
                            Writable.WriteByte(CByte((c >> 16) And &HFF))
                        Next
                    Case 32
                        For i = ib To ie - 1
                            Writable.WriteInt32(a(i, j))
                        Next
                End Select
            Next
        End Sub
        ''' <summary>从ARGB整数设置矩形。对非24、32位位图会进行转换。</summary>
        Public Sub SetRectangleFromARGB(ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,))
            Dim qc As New QuantizerCache(Function(c) QuantizeOnPalette(c, PicPalette))
            SetRectangleFromARGB(x, y, a, AddressOf qc.Quantize)
        End Sub

        ''' <summary>释放资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Readable IsNot Nothing Then
                Readable.Dispose()
                Readable = Nothing
            End If
            If Writable IsNot Nothing Then
                Writable.Dispose()
                Writable = Nothing
            End If
        End Sub
    End Class
End Namespace
