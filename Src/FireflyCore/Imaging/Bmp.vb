'==========================================================================
'
'  File:        Bmp.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: 基本Bmp文件流类
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Drawing
Imports System.IO
Imports Firefly.TextEncoding

Namespace Imaging

    ''' <summary>基本Bmp文件流类</summary>
    ''' <remarks>不能使用压缩等无用功能</remarks>
    Public Class Bmp
        Implements IDisposable

        ''' <summary>标志符。</summary>
        Public Const Identifier As String = "BM"
        ''' <summary>文件大小。</summary>
        ReadOnly Property FileSize() As Int32
            Get
                Return BitmapDataOffset + BitmapDataSize
            End Get
        End Property
        Protected Const Reserved As Int32 = 0
        ''' <summary>位图数据偏移量。</summary>
        ReadOnly Property BitmapDataOffset() As Int32
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
        Protected Const BitmapHeaderSize As Int32 = &H28
        Protected PicWidth As Int32
        ''' <summary>宽度。</summary>
        Property Width() As Int32
            Get
                Return PicWidth
            End Get
            Set(ByVal value As Int32)
                If value >= 0 Then PicWidth = value Else Return
                CalcLineBitLength()
                BaseStream.SetLength(FileSize)
                BaseStream.Position = 18
                BaseStream.WriteInt32(PicWidth)
            End Set
        End Property
        Protected LineBitLength As Int32
        Protected Sub CalcLineBitLength()
            If (PicWidth * PicBitsPerPixel) Mod 32 <> 0 Then
                LineBitLength = (((PicWidth * PicBitsPerPixel) >> 5) + 1) << 5
            Else
                LineBitLength = PicWidth * PicBitsPerPixel
            End If
        End Sub
        Protected PicHeight As Int32
        ''' <summary>高度。</summary>
        Property Height() As Int32
            Get
                Return PicHeight
            End Get
            Set(ByVal value As Int32)
                If value >= 0 Then PicHeight = value Else Return
                BaseStream.SetLength(FileSize)
                BaseStream.Position = 22
                BaseStream.WriteInt32(PicHeight)
            End Set
        End Property
        Protected Const Planes As Int16 = 1
        Protected PicBitsPerPixel As Int16
        ''' <summary>位深度。</summary>
        ReadOnly Property BitsPerPixel() As Int16
            Get
                If PicBitsPerPixel = 16 AndAlso Not r5g6b5 Then Return 15
                Return PicBitsPerPixel
            End Get
        End Property
        Protected PicCompression As Int32
        ''' <summary>压缩方式。</summary>
        ReadOnly Property Compression() As Int32
            Get
                Return PicCompression
            End Get
        End Property
        ''' <summary>位图数据大小。</summary>
        ReadOnly Property BitmapDataSize() As Int32
            Get
                Return (LineBitLength * PicHeight) >> 3
            End Get
        End Property
        Protected Const HResolution As Int32 = 0 '不用
        Protected Const VResolution As Int32 = 0 '不用
        Protected Const Colors As Int32 = 0
        Protected Const ImportantColors As Int32 = 0
        Protected PicPalette As Int32()
        ''' <summary>调色板。</summary>
        Property Palette() As Int32()
            Get
                Dim Value As Int32()
                If (PicBitsPerPixel = 1) OrElse (PicBitsPerPixel = 4) OrElse (PicBitsPerPixel = 8) Then
                    Value = New Int32((1 << PicBitsPerPixel) - 1) {}
                    BaseStream.Position = &H36
                    For n As Integer = 0 To (1 << PicBitsPerPixel) - 1
                        Value(n) = BaseStream.ReadInt32
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
                    BaseStream.Position = &H36
                    For n As Integer = 0 To (1 << PicBitsPerPixel) - 1
                        BaseStream.WriteInt32(Value(n))
                    Next
                Else
                    Throw New InvalidDataException
                End If
                PicPalette = CType(Value.Clone, Int32())
            End Set
        End Property

        Protected BaseStream As StreamEx
        Protected r5g6b5 As Boolean
        Private Sub New()
        End Sub

        ''' <summary>新建内存流Bmp</summary>
        ''' <param name="BitsPerPixel">Bmp位数：可以取1、4、8、15、16、24、32</param>
        Sub New(ByVal Width As Int32, ByVal Height As Int32, Optional ByVal BitsPerPixel As Int16 = 24)
            BaseStream = New MemoryStream
            If Width < 0 OrElse Height < 0 Then
                BaseStream.Dispose()
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
                BaseStream.Dispose()
                Throw New NotSupportedException("PicBitsPerPixelNotSupported")
            End If
            CalcLineBitLength()
            BaseStream.SetLength(FileSize)

            BaseStream.Position = 0
            For n As Integer = 0 To Identifier.Length - 1
                BaseStream.WriteByte(CByte(AscQ(Identifier(n))))
            Next
            BaseStream.WriteInt32(FileSize)
            BaseStream.WriteInt32(Reserved)
            BaseStream.WriteInt32(BitmapDataOffset)
            BaseStream.WriteInt32(BitmapHeaderSize)
            BaseStream.WriteInt32(PicWidth)
            BaseStream.WriteInt32(PicHeight)
            BaseStream.WriteInt16(Planes)
            BaseStream.WriteInt16(PicBitsPerPixel)
            BaseStream.WriteInt32(PicCompression)
            BaseStream.WriteInt32(BitmapDataSize)
            BaseStream.WriteInt32(HResolution)
            BaseStream.WriteInt32(VResolution)
            BaseStream.WriteInt32(Colors)
            BaseStream.WriteInt32(ImportantColors)

            If (PicCompression = 3) AndAlso (PicBitsPerPixel = 16) Then
                If r5g6b5 Then
                    BaseStream.WriteInt32(&HF800)
                    BaseStream.WriteInt32(&H7E0)
                    BaseStream.WriteInt32(&H1F)
                    BaseStream.WriteInt32(&H0)
                Else
                    BaseStream.WriteInt32(&H7C00)
                    BaseStream.WriteInt32(&H3E0)
                    BaseStream.WriteInt32(&H1F)
                    BaseStream.WriteInt32(&H0)
                End If
            End If
        End Sub
        ''' <summary>新建文件流Bmp</summary>
        ''' <param name="BitsPerPixel">Bmp位数：可以取1、4、8、15、16、24、32</param>
        Sub New(ByVal Path As String, ByVal Width As Int32, ByVal Height As Int32, Optional ByVal BitsPerPixel As Int16 = 24)
            BaseStream = New FileStream(Path, FileMode.Create)
            If Width < 0 OrElse Height < 0 Then
                BaseStream.Dispose()
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
                BaseStream.Dispose()
                Throw New NotSupportedException("PicBitsPerPixelNotSupported")
            End If
            CalcLineBitLength()
            BaseStream.SetLength(FileSize)

            BaseStream.Position = 0
            For n As Integer = 0 To Identifier.Length - 1
                BaseStream.WriteByte(CByte(AscQ(Identifier(n))))
            Next
            BaseStream.WriteInt32(FileSize)
            BaseStream.WriteInt32(Reserved)
            BaseStream.WriteInt32(BitmapDataOffset)
            BaseStream.WriteInt32(BitmapHeaderSize)
            BaseStream.WriteInt32(PicWidth)
            BaseStream.WriteInt32(PicHeight)
            BaseStream.WriteInt16(Planes)
            BaseStream.WriteInt16(PicBitsPerPixel)
            BaseStream.WriteInt32(PicCompression)
            BaseStream.WriteInt32(BitmapDataSize)
            BaseStream.WriteInt32(HResolution)
            BaseStream.WriteInt32(VResolution)
            BaseStream.WriteInt32(Colors)
            BaseStream.WriteInt32(ImportantColors)

            If (PicCompression = 3) AndAlso (PicBitsPerPixel = 16) Then
                If r5g6b5 Then
                    BaseStream.WriteInt32(&HF800)
                    BaseStream.WriteInt32(&H7E0)
                    BaseStream.WriteInt32(&H1F)
                    BaseStream.WriteInt32(&H0)
                Else
                    BaseStream.WriteInt32(&H7C00)
                    BaseStream.WriteInt32(&H3E0)
                    BaseStream.WriteInt32(&H1F)
                    BaseStream.WriteInt32(&H0)
                End If
            End If
        End Sub

        ''' <summary>已重载。从流打开一个位图。</summary>
        Shared Function Open(ByVal sp As ZeroPositionStreamPasser) As Bmp
            Dim s = sp.GetStream
            Dim bf As New Bmp
            With bf
                .BaseStream = s
                .BaseStream.Position = 0
                For n As Integer = 0 To Identifier.Length - 1
                    If .BaseStream.ReadByte() <> AscQ(Identifier(n)) Then
                        bf.Dispose()
                        Throw New InvalidDataException
                    End If
                Next
                .BaseStream.ReadInt32() '跳过File Size
                .BaseStream.ReadInt32() '跳过Reserved
                .BaseStream.ReadInt32() '跳过Bitmap Data Offset
                .BaseStream.ReadInt32() '跳过Bitmap Header Size
                .PicWidth = .BaseStream.ReadInt32
                .PicHeight = .BaseStream.ReadInt32
                If .PicWidth < 0 OrElse .PicHeight < 0 Then
                    bf.Dispose()
                    Throw New InvalidDataException
                End If
                .BaseStream.ReadInt16() '跳过Planes
                .PicBitsPerPixel = .BaseStream.ReadInt16
                .PicCompression = .BaseStream.ReadInt32
                .BaseStream.ReadInt32() '跳过Bitmap Data Size
                .BaseStream.ReadInt32() '跳过HResolution
                .BaseStream.ReadInt32() '跳过VResolution
                .BaseStream.ReadInt32() '跳过Colors
                .BaseStream.ReadInt32() '跳过Important Colors

                If .PicCompression <> 0 Then
                    If (.PicCompression = 3) AndAlso (.PicBitsPerPixel = 16) Then
                        .r5g6b5 = CBool(.BaseStream.ReadInt32() And &H8000) '检验红色掩码是否从最高位开始
                        .BaseStream.ReadInt32() '跳过绿色掩码
                        .BaseStream.ReadInt32() '跳过蓝色掩码
                        .BaseStream.ReadInt32()
                    Else
                        bf.Dispose()
                        Throw New InvalidDataException
                    End If
                End If

                If (.PicBitsPerPixel = 1) OrElse (.PicBitsPerPixel = 4) OrElse (.PicBitsPerPixel = 8) Then
                    .PicPalette = New Int32((1 << .PicBitsPerPixel) - 1) {}
                    For n As Integer = 0 To (1 << .PicBitsPerPixel) - 1
                        .PicPalette(n) = .BaseStream.ReadInt32()
                    Next
                ElseIf (.PicBitsPerPixel = 16) OrElse (.PicBitsPerPixel = 24) OrElse (.PicBitsPerPixel = 32) Then
                Else
                    bf.Dispose()
                    Throw New NotSupportedException("PicBitsPerPixelNotSupported")
                End If

                .CalcLineBitLength()
            End With
            Return bf
        End Function
        ''' <summary>已重载。从文件打开一个位图。</summary>
        Shared Function Open(ByVal Path As String) As Bmp
            Dim s As New StreamEx(Path, FileMode.Open)
            Try
                Return Open(s)
            Catch
                s.Dispose()
                Throw
            End Try
        End Function
        ''' <summary>关闭。</summary>
        Sub Close()
            BaseStream.Close()
        End Sub
        ''' <summary>转换为System.Drawing.Bitmap。</summary>
        Function ToBitmap() As Bitmap
            BaseStream.Position = 0
            BaseStream.Flush()
            Return New Bitmap(BaseStream)
        End Function
        ''' <summary>保存到流。</summary>
        Sub SaveTo(ByVal sp As ZeroPositionStreamPasser)
            Dim s = sp.GetStream
            BaseStream.Position = 0
            s.WriteFromStream(BaseStream, BaseStream.Length)
        End Sub

        Protected ReadOnly Property Pos(ByVal x As Int32, ByVal y As Int32) As Integer
            Get
                Return (LineBitLength * (Height - 1 - y) + x * PicBitsPerPixel) >> 3
            End Get
        End Property
        ''' <summary>获得像素点。</summary>
        Function GetPixel(ByVal x As Int32, ByVal y As Int32) As Int32
            If x < 0 OrElse x > PicWidth - 1 OrElse y < 0 OrElse y > PicHeight - 1 Then Return 0
            BaseStream.Position = BitmapDataOffset + Pos(x, y)
            Select Case PicBitsPerPixel
                Case 1
                    Return (BaseStream.ReadByte >> (7 - x Mod 8)) And 1
                Case 4
                    Return (BaseStream.ReadByte >> (4 * (1 - x Mod 2))) And 15
                Case 8
                    Return BaseStream.ReadByte
                Case 16
                    Return BaseStream.ReadInt16
                Case 24
                    Return BaseStream.ReadInt32 And &HFFFFFF
                Case 32
                    Return BaseStream.ReadInt32
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        ''' <summary>设置像素点。</summary>
        Sub SetPixel(ByVal x As Int32, ByVal y As Int32, ByVal c As Int32)
            If x < 0 OrElse x > PicWidth - 1 OrElse y < 0 OrElse y > PicHeight - 1 Then Return
            BaseStream.Position = BitmapDataOffset + Pos(x, y)
            Select Case PicBitsPerPixel
                Case 1
                    Dim k As Byte = BaseStream.ReadByte
                    k = k And Not CByte(1 << (7 - x Mod 8)) Or CByte((CByte(c <> 0) And 1) << (7 - x Mod 8))
                    BaseStream.Position -= 1
                    BaseStream.WriteByte(k)
                Case 4
                    Dim k As Byte = BaseStream.ReadByte
                    k = k And Not CByte(15 << (4 * (1 - x Mod 2))) Or CByte((c And 15) << (4 * (1 - x Mod 2)))
                    BaseStream.Position -= 1
                    BaseStream.WriteByte(k)
                Case 8
                    BaseStream.WriteByte(CByte(c And &HFF))
                Case 16
                    BaseStream.WriteInt16(CID(c And &HFFFF))
                Case 24
                    BaseStream.WriteInt16(CID(c And &HFFFF))
                    BaseStream.WriteByte(CByte((c >> 16) And &HFF))
                Case 32S
                    BaseStream.WriteInt32(c)
            End Select
        End Sub
        ''' <summary>获取矩形。</summary>
        Function GetRectangle(ByVal x As Int32, ByVal y As Int32, ByVal w As Int32, ByVal h As Int32) As Int32(,)
            If w < 0 OrElse h < 0 Then Return Nothing
            Dim a As Int32(,) = New Int32(w - 1, h - 1) {}
            Dim ox, oy As Integer
            If y < 0 Then
                h = h + y
                oy = 0
            Else
                oy = y
            End If
            If oy + h > PicHeight Then
                h = PicHeight - oy
            End If
            If x < 0 Then
                ox = 0
            Else
                ox = x
            End If
            If ox + w > PicWidth Then
                w = PicWidth - ox
            End If
            Dim xl As Integer = ox - x
            Dim xu As Integer
            If x >= 0 Then
                xu = w + ox - x - 1
            Else
                xu = w - 1
            End If

            For m As Integer = oy + h - y - 1 To oy - y Step -1
                BaseStream.Position = BitmapDataOffset + Pos(ox, oy + m)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If (ox And 7) <> 0 Then
                            t = BaseStream.ReadByte
                            t = t << (ox And 7)
                        End If
                        For n As Integer = xl To xu
                            If (n And 7) = 0 Then
                                t = BaseStream.ReadByte
                            End If
                            a(n, m) = (t And 128) >> 7
                            t = t << 1
                        Next
                    Case 4
                        Dim t As Byte
                        If (ox And 1) = 1 Then
                            t = BaseStream.ReadByte
                        End If
                        For n As Integer = xl To xu
                            If (n And 1) = 0 Then
                                t = BaseStream.ReadByte
                                a(n, m) = t >> 4
                            Else
                                a(n, m) = t And 15
                            End If
                        Next
                    Case 8
                        For n As Integer = xl To xu
                            a(n, m) = BaseStream.ReadByte
                        Next
                    Case 16
                        For n As Integer = xl To xu
                            a(n, m) = EID(BaseStream.ReadInt16)
                        Next
                    Case 24
                        For n As Integer = xl To xu
                            a(n, m) = EID(BaseStream.ReadInt16)
                            a(n, m) = a(n, m) Or (CInt(BaseStream.ReadByte) << 16)
                            a(n, m) = &HFF000000 Or a(n, m)
                        Next
                    Case 32
                        For n As Integer = xl To xu
                            a(n, m) = BaseStream.ReadInt32
                        Next
                End Select
            Next
            Return a
        End Function
        ''' <summary>获取矩形。表示为字节。仅供8位及以下图片使用。</summary>
        Function GetRectangleBytes(ByVal x As Int32, ByVal y As Int32, ByVal w As Int32, ByVal h As Int32) As Byte(,)
            If w < 0 OrElse h < 0 Then Return Nothing
            Dim a As Byte(,) = New Byte(w - 1, h - 1) {}
            Dim ox, oy As Integer
            If y < 0 Then
                h = h + y
                oy = 0
            Else
                oy = y
            End If
            If oy + h > PicHeight Then
                h = PicHeight - oy
            End If
            If x < 0 Then
                ox = 0
            Else
                ox = x
            End If
            If ox + w > PicWidth Then
                w = PicWidth - ox
            End If
            Dim xl As Integer = ox - x
            Dim xu As Integer
            If x >= 0 Then
                xu = w + ox - x - 1
            Else
                xu = w - 1
            End If

            For m As Integer = oy + h - y - 1 To oy - y Step -1
                BaseStream.Position = BitmapDataOffset + Pos(ox, oy + m)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If (ox And 7) <> 0 Then
                            t = BaseStream.ReadByte
                            t = t << (ox And 7)
                        End If
                        For n As Integer = xl To xu
                            If (n And 7) = 0 Then
                                t = BaseStream.ReadByte
                            End If
                            a(n, m) = CByte((t And 128) >> 7)
                            t = t << 1
                        Next
                    Case 4
                        Dim t As Byte
                        If (ox And 1) = 1 Then
                            t = BaseStream.ReadByte
                        End If
                        For n As Integer = xl To xu
                            If (n And 1) = 0 Then
                                t = BaseStream.ReadByte
                                a(n, m) = t >> 4
                            Else
                                a(n, m) = CByte(t And 15)
                            End If
                        Next
                    Case 8
                        For n As Integer = xl To xu
                            a(n, m) = BaseStream.ReadByte
                        Next
                    Case 16, 24, 32
                        Throw New InvalidOperationException
                End Select
            Next
            Return a
        End Function
        ''' <summary>已重载。设置矩形。</summary>
        Sub SetRectangle(ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,))
            If a Is Nothing Then Return
            Dim w As Integer = a.GetLength(0)
            Dim h As Integer = a.GetLength(1)
            Dim ox, oy As Integer
            If y < 0 Then
                h = h + y
                oy = 0
            Else
                oy = y
            End If
            If oy + h > PicHeight Then
                h = PicHeight - oy
            End If
            If x < 0 Then
                ox = 0
            Else
                ox = x
            End If
            If ox + w > PicWidth Then
                w = PicWidth - ox
            End If
            Dim xl As Integer = ox - x
            Dim xu As Integer
            If x >= 0 Then
                xu = w + ox - x - 1
            Else
                xu = w - 1
            End If

            For m As Integer = oy + h - y - 1 To oy - y Step -1
                BaseStream.Position = BitmapDataOffset + Pos(ox, oy + m)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If (ox And 7) <> 0 Then
                            t = BaseStream.ReadByte >> (8 - ox And 7)
                            BaseStream.Position -= 1
                        End If
                        Dim n As Integer
                        For n = xl To xu
                            t = t << 1
                            If a(n, m) <> 0 Then t = t Or CByte(1)
                            If (n And 7) = 7 Then
                                BaseStream.WriteByte(t)
                            End If
                        Next
                        If (n And 7) <> 0 Then
                            t = t << (7 - n And 7)
                            BaseStream.WriteByte(t)
                        End If
                    Case 4
                        Dim t As Byte
                        If (ox And 1) <> 0 Then
                            t = BaseStream.ReadByte >> (4 * (2 - ox And 1))
                            BaseStream.Position -= 1
                        End If
                        Dim n As Integer
                        For n = xl To xu
                            t = t << 4
                            t = t Or CByte(a(n, m) And 15)
                            If (n And 1) = 1 Then
                                BaseStream.WriteByte(t)
                            End If
                        Next
                        If (n And 1) <> 0 Then
                            t = t << 4
                            BaseStream.WriteByte(t)
                        End If
                    Case 8
                        For n As Integer = xl To xu
                            BaseStream.WriteByte(CByte(a(n, m) And &HFF))
                        Next
                    Case 16
                        For n As Integer = xl To xu
                            BaseStream.WriteInt16(CID(a(n, m) And &HFFFF))
                        Next
                    Case 24
                        For n As Integer = xl To xu
                            BaseStream.WriteInt16(CID(a(n, m) And &HFFFF))
                            BaseStream.WriteByte(CByte((a(n, m) >> 16) And &HFF))
                        Next
                    Case 32
                        For n As Integer = xl To xu
                            BaseStream.WriteInt32(a(n, m))
                        Next
                End Select
            Next
        End Sub
        ''' <summary>已重载。设置矩形。</summary>
        Sub SetRectangle(ByVal x As Int32, ByVal y As Int32, ByVal a As Byte(,))
            If a Is Nothing Then Return
            Dim w As Integer = a.GetLength(0)
            Dim h As Integer = a.GetLength(1)
            Dim ox, oy As Integer
            If y < 0 Then
                h = h + y
                oy = 0
            Else
                oy = y
            End If
            If oy + h > PicHeight Then
                h = PicHeight - oy
            End If
            If x < 0 Then
                ox = 0
            Else
                ox = x
            End If
            If ox + w > PicWidth Then
                w = PicWidth - ox
            End If
            Dim xl As Integer = ox - x
            Dim xu As Integer
            If x >= 0 Then
                xu = w + ox - x - 1
            Else
                xu = w - 1
            End If

            For m As Integer = oy + h - y - 1 To oy - y Step -1
                BaseStream.Position = BitmapDataOffset + Pos(ox, oy + m)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If (ox And 7) <> 0 Then
                            t = BaseStream.ReadByte >> (8 - ox And 7)
                            BaseStream.Position -= 1
                        End If
                        Dim n As Integer
                        For n = xl To xu
                            t = t << 1
                            If a(n, m) <> 0 Then t = t Or CByte(1)
                            If (n And 7) = 7 Then
                                BaseStream.WriteByte(t)
                            End If
                        Next
                        If (n And 7) <> 0 Then
                            t = t << (7 - n And 7)
                            BaseStream.WriteByte(t)
                        End If
                    Case 4
                        Dim t As Byte
                        If (ox And 1) <> 0 Then
                            t = BaseStream.ReadByte >> (4 * (2 - ox And 1))
                            BaseStream.Position -= 1
                        End If
                        Dim n As Integer
                        For n = xl To xu
                            t = t << 4
                            t = t Or CByte(a(n, m) And 15)
                            If (n And 1) = 1 Then
                                BaseStream.WriteByte(t)
                            End If
                        Next
                        If (n And 1) <> 0 Then
                            t = t << 4
                            BaseStream.WriteByte(t)
                        End If
                    Case 8
                        For n As Integer = xl To xu
                            BaseStream.WriteByte(CByte(a(n, m) And &HFF))
                        Next
                    Case 16, 24, 32
                        Throw New InvalidOperationException
                End Select
            Next
        End Sub
        ''' <summary>获取矩形为ARGB整数。对非24、32位位图会进行转换。</summary>
        Function GetRectangleAsARGB(ByVal x As Int32, ByVal y As Int32, ByVal w As Int32, ByVal h As Int32) As Int32(,)
            Dim a As Int32(,) = GetRectangle(x, y, w, h)
            Select Case PicBitsPerPixel
                Case 1, 4, 8
                    For py As Integer = 0 To h - 1
                        For px As Integer = 0 To w - 1
                            a(px, py) = PicPalette(a(px, py))
                        Next
                    Next
                Case 16
                    If r5g6b5 Then
                        For py As Integer = 0 To h - 1
                            For px As Integer = 0 To w - 1
                                a(px, py) = ColorSpace.RGB16To32(CID(a(px, py)))
                            Next
                        Next
                    Else
                        For py As Integer = 0 To h - 1
                            For px As Integer = 0 To w - 1
                                a(px, py) = ColorSpace.RGB15To32(CID(a(px, py)))
                            Next
                        Next
                    End If
                Case 24, 32
            End Select
            Return a
        End Function
        ''' <summary>从ARGB整数设置矩形。对非24、32位位图会进行转换。使用自定义的量化器。</summary>
        Sub SetRectangleFromARGB(ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,), ByVal Quantize As Func(Of Int32, Byte))
            If a Is Nothing Then Return
            Dim w As Integer = a.GetLength(0)
            Dim h As Integer = a.GetLength(1)
            Dim ox, oy As Integer
            If y < 0 Then
                h = h + y
                oy = 0
            Else
                oy = y
            End If
            If oy + h > PicHeight Then
                h = PicHeight - oy
            End If
            If x < 0 Then
                ox = 0
            Else
                ox = x
            End If
            If ox + w > PicWidth Then
                w = PicWidth - ox
            End If
            Dim xl As Integer = ox - x
            Dim xu As Integer
            If x >= 0 Then
                xu = w + ox - x - 1
            Else
                xu = w - 1
            End If

            For m As Integer = oy + h - y - 1 To oy - y Step -1
                BaseStream.Position = BitmapDataOffset + Pos(ox, oy + m)
                Select Case PicBitsPerPixel
                    Case 1
                        Dim t As Byte
                        If (ox And 7) <> 0 Then
                            t = BaseStream.ReadByte >> (8 - ox And 7)
                            BaseStream.Position -= 1
                        End If
                        Dim n As Integer
                        For n = xl To xu
                            t = t << 1
                            If Quantize(a(n, m)) <> 0 Then t = t Or CByte(1)
                            If (n And 7) = 7 Then
                                BaseStream.WriteByte(t)
                            End If
                        Next
                        If (n And 7) <> 0 Then
                            t = t << (7 - n And 7)
                            BaseStream.WriteByte(t)
                        End If
                    Case 4
                        Dim t As Byte
                        If (ox And 1) <> 0 Then
                            t = BaseStream.ReadByte >> (4 * (2 - ox And 1))
                            BaseStream.Position -= 1
                        End If
                        Dim n As Integer
                        For n = xl To xu
                            t = t << 4
                            t = t Or CByte(Quantize(a(n, m)) And 15)
                            If (n And 1) = 1 Then
                                BaseStream.WriteByte(t)
                            End If
                        Next
                        If (n And 1) <> 0 Then
                            t = t << 4
                            BaseStream.WriteByte(t)
                        End If
                    Case 8
                        For n As Integer = xl To xu
                            BaseStream.WriteByte(CByte(Quantize(a(n, m)) And &HFF))
                        Next
                    Case 16
                        If r5g6b5 Then
                            For n As Integer = xl To xu
                                BaseStream.WriteInt16(CID(ColorSpace.RGB32To16(a(n, m)) And &HFFFF))
                            Next
                        Else
                            For n As Integer = xl To xu
                                BaseStream.WriteInt16(CID(ColorSpace.RGB32To15(a(n, m)) And &HFFFF))
                            Next
                        End If
                    Case 24
                        For n As Integer = xl To xu
                            BaseStream.WriteInt16(CID(a(n, m) And &HFFFF))
                            BaseStream.WriteByte(CByte((a(n, m) >> 16) And &HFF))
                        Next
                    Case 32
                        For n As Integer = xl To xu
                            BaseStream.WriteInt32(a(n, m))
                        Next
                End Select
            Next
        End Sub
        ''' <summary>从ARGB整数设置矩形。对非24、32位位图会进行转换。</summary>
        Sub SetRectangleFromARGB(ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,))
            Dim qc As New QuantizerCache(Function(c) QuantizeOnPalette(c, PicPalette))
            SetRectangleFromARGB(x, y, a, AddressOf qc.Quantize)
        End Sub

#Region " IDisposable 支持 "
        Private DisposedValue As Boolean = False '检测冗余的调用
        ''' <summary>释放资源。</summary>
        ''' <remarks>对继承者的说明：不要调用基类的Dispose()，而应调用Dispose(True)，否则会出现无限递归。</remarks>
        Protected Overridable Sub Dispose(ByVal Disposing As Boolean)
            If DisposedValue Then Return
            If Disposing Then
                '释放其他状态(托管对象)。
            End If

            '释放您自己的状态(非托管对象)。
            '将大型字段设置为 null。
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
            DisposedValue = True
        End Sub
        ''' <summary>释放资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean) 中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace
