'==========================================================================
'
'  File:        GimTran.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: GIM/MIG操作实例
'  Version:     2013.02.26.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.Imaging

Public Module GimTran

    Public Function Main() As Integer
        If System.Diagnostics.Debugger.IsAttached Then
            Return MainInner()
        Else
            Try
                Return MainInner()
            Catch ex As Exception
                Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex))
                Return -1
            End Try
        End If
    End Function

    Public Function MainInner() As Integer
        Dim argv = CommandLine.GetCmdLine.Arguments

        For Each f In argv
            Dim FileName = GetFileName(f)
            Dim FileDir = GetFileDirectory(f)

            If IsMatchFileMask(FileName, "*.mig") OrElse IsMatchFileMask(FileName, "*.gim") Then
                Dim Data As Byte()
                Using s = Streams.OpenReadable(f)
                    Data = s.Read(s.Length)
                End Using

                Dim m As New GIM(Data)
                Dim Images = m.Images.ToArray
                For n = 0 To Images.Length - 1
                    Dim Image As GIM.ImageBlock = Images(n)
                    Dim Bitmaps = Image.Bitmap
                    Dim Palettes = Image.Palette
                    If Palettes Is Nothing AndAlso Bitmaps.NeedPalette Then Throw New InvalidDataException
                    Dim Width = Bitmaps.Width
                    Dim Height = Bitmaps.Height
                    If Bitmaps.Indices.Length > 1 Then
                        Height = ((Height + 7) \ 8) * 8
                    End If
                    If Bitmaps.NeedPalette Then
                        Dim BitsPerPixel = CUS(Bitmaps.BitsPerPixel)
                        If BitsPerPixel > 8 Then
                            BitsPerPixel = Ceiling(Log(Enumerable.Range(0, Bitmaps.Indices.Length).Select(Function(i) Palettes.PaletteData(Palettes.Indices(i)).First.Length).Max, 2))
                        End If
                        If BitsPerPixel > 8 Then Throw New InvalidDataException
                        Using bmp As New Bmp(Width, Height, BitsPerPixel)
                            Using png As New Bitmap(bmp.Width, bmp.Height * Bitmaps.Indices.Length, PixelFormat.Format32bppArgb)
                                For i = 0 To Bitmaps.Indices.Length - 1
                                    Dim Rectangle = Bitmaps.BitmapData(Bitmaps.Indices(i)).First
                                    Dim Palette = Palettes.PaletteData(Palettes.Indices(i)).First
                                    Dim p = New Int32((1 << BitsPerPixel) - 1) {}
                                    Array.Copy(Palette, p, Min(Palette.Length, p.Length))
                                    bmp.Palette = p
                                    bmp.SetRectangle(0, 0, Rectangle)
                                    png.SetRectangle(0, Height * i, bmp.GetRectangleAsARGB(0, 0, bmp.Width, bmp.Height))
                                    png.Save(GetPath(FileDir, "{0}.{1}.png".Formats(FileName, n)))
                                Next
                            End Using
                        End Using
                    Else
                        Using png As New Bitmap(Width, Height, PixelFormat.Format32bppArgb)
                            For i = 0 To Bitmaps.Indices.Length - 1
                                Dim Rectangle = Bitmaps.BitmapData(Bitmaps.Indices(i)).First
                                png.SetRectangle(0, Height * i, Rectangle)
                                png.Save(GetPath(FileDir, "{0}.{1}.png".Formats(FileName, n)))
                            Next
                        End Using
                    End If
                Next
            ElseIf IsMatchFileMask(FileName, "*.mig.*.png") OrElse IsMatchFileMask(FileName, "*.gim.*.png") Then
                Dim GimName = GetMainFileName(GetMainFileName(f))
                Dim GimPath = GetPath(GetFileDirectory(f), GimName)
                Dim Data As Byte()
                Using s = Streams.OpenReadable(GimPath)
                    Data = s.Read(s.Length)
                End Using

                Dim m As New GIM(Data)
                Dim Images = m.Images.ToArray

                Dim n = GetExtendedFileName(GetMainFileName(f))

                Dim Image As GIM.ImageBlock = Images(n)
                Dim Bitmaps = Image.Bitmap
                Dim Palettes = Image.Palette
                If Palettes Is Nothing AndAlso Bitmaps.NeedPalette Then Throw New InvalidDataException
                Dim Width = Bitmaps.Width
                Dim Height = Bitmaps.Height
                If Bitmaps.Indices.Length > 1 Then
                    Height = ((Height + 7) \ 8) * 8
                End If
                Dim ab As AbstractBitmap(Of Int32)
                Using png As New Bitmap(f)
                    ab = New AbstractBitmap(Of Int32)(png.Width, png.Height)
                    If png.PixelFormat = PixelFormat.Format32bppArgb Then
                        ab.SetRectangle2(0, 0, png.GetRectangle(0, 0, png.Width, png.Height))
                    Else
                        Using b As New Bitmap(png.Width, png.Height, PixelFormat.Format32bppArgb)
                            Using g = Graphics.FromImage(b)
                                g.DrawImage(png, 0, 0)
                                ab.SetRectangle2(0, 0, png.GetRectangle(0, 0, png.Width, png.Height))
                            End Using
                        End Using
                    End If
                End Using
                Dim GuassianKernel4x4 = New Integer() {1, 3, 3, 1, 3, 9, 9, 3, 3, 9, 9, 3, 1, 3, 3, 1}
                For i = 0 To Bitmaps.Indices.Length - 1
                    Dim RectangleInMipmap = ab.GetRectangle2(0, Height * i, Width, Height)
                    Dim WidthInMipmap As Integer = Width
                    Dim HeightInMipmap As Integer = Height
                    For k = 0 To Bitmaps.NumMipmap - 1
                        Dim r = New Int32(WidthInMipmap - 1, HeightInMipmap - 1) {}
                        If Bitmaps.NeedPalette Then
                            Dim Palette = Palettes.PaletteData(Palettes.Indices(i))(k)
                            Dim Fun =
                                Sub(xy)
                                    Dim x = xy Mod WidthInMipmap
                                    Dim y = xy \ WidthInMipmap
                                    r(x, y) = Quantizer.QuantizeOnPalette(RectangleInMipmap(x, y), Palette, AddressOf ColourDistanceARGB)
                                End Sub
                            Enumerable.Range(0, WidthInMipmap * HeightInMipmap).AsParallel().ForAll(Fun)
                        Else
                            For y = 0 To HeightInMipmap - 1
                                For x = 0 To WidthInMipmap - 1
                                    r(x, y) = RectangleInMipmap(x, y)
                                Next
                            Next
                        End If
                        Bitmaps.BitmapData(Bitmaps.Indices(i))(k) = r
                        If k < Bitmaps.NumMipmap - 1 Then
                            WidthInMipmap = (WidthInMipmap + 1) \ 2
                            HeightInMipmap = (HeightInMipmap + 1) \ 2
                            Dim Small = New Int32(WidthInMipmap - 1, HeightInMipmap - 1) {}
                            Dim Fun =
                                Sub(xy)
                                    Dim x = xy Mod WidthInMipmap
                                    Dim y = xy \ WidthInMipmap
                                    Dim Pixels = New List(Of KeyValuePair(Of Int32, Integer))()
                                    Dim Index = 0
                                    For yy = y * 2 - 1 To y * 2 + 2
                                        For xx = x * 2 - 1 To x * 2 + 2
                                            If xx >= 0 AndAlso xx < RectangleInMipmap.GetLength(0) AndAlso yy >= 0 AndAlso yy < RectangleInMipmap.GetLength(1) Then
                                                Pixels.Add(CreatePair(RectangleInMipmap(xx, yy), GuassianKernel4x4(Index)))
                                            End If
                                            Index += 1
                                        Next
                                    Next
                                    Dim SumWeight = Pixels.Select(Function(c) c.Value).Sum
                                    Dim ColorAddOffset = (SumWeight - 1) \ 2
                                    Dim cA = (Pixels.Select(Function(c) c.Key.Bits(31, 24) * c.Value).Sum() + ColorAddOffset) \ SumWeight
                                    Dim cR = (Pixels.Select(Function(c) c.Key.Bits(23, 16) * c.Value).Sum() + ColorAddOffset) \ SumWeight
                                    Dim cG = (Pixels.Select(Function(c) c.Key.Bits(15, 8) * c.Value).Sum() + ColorAddOffset) \ SumWeight
                                    Dim cB = (Pixels.Select(Function(c) c.Key.Bits(7, 0) * c.Value).Sum() + ColorAddOffset) \ SumWeight
                                    Small(x, y) = cA.ConcatBits(cR, 8).ConcatBits(cG, 8).ConcatBits(cB, 8)
                                End Sub
                            Enumerable.Range(0, WidthInMipmap * HeightInMipmap).AsParallel().ForAll(Fun)
                            RectangleInMipmap = Small
                        End If
                    Next
                Next
                Data = m.ToBytes

                Using s = Streams.CreateResizable(GimPath)
                    s.Write(Data)
                End Using
            End If
        Next

        Return 0
    End Function
End Module
