'==========================================================================
'
'  File:        GimTran.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: GIM/MIG操作实例
'  Version:     2011.02.23.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Math
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
                            BitsPerPixel = Ceiling(Log((From i In Enumerable.Range(0, Bitmaps.Indices.Length) Select Palettes.PaletteData(Palettes.Indices(i)).Length).Max, 2))
                        End If
                        If BitsPerPixel > 8 Then Throw New InvalidDataException
                        Using bmp As New Bmp(Width, Height, BitsPerPixel)
                            Using png As New Bitmap(bmp.Width, bmp.Height * Bitmaps.Indices.Length, PixelFormat.Format32bppArgb)
                                For i = 0 To Bitmaps.Indices.Length - 1
                                    Dim Rectangle = Bitmaps.BitmapData(Bitmaps.Indices(i))
                                    Dim Palette = Palettes.PaletteData(Palettes.Indices(i))
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
                                Dim Rectangle = Bitmaps.BitmapData(Bitmaps.Indices(i))
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
                If Bitmaps.NeedPalette Then
                    Using png As New Bitmap(f)
                        If png.PixelFormat = PixelFormat.Format32bppArgb Then
                            For i = 0 To Bitmaps.Indices.Length - 1
                                Dim Rectangle = png.GetRectangle(0, Height * i, Width, Height)
                                Dim Palette = Palettes.PaletteData(Palettes.Indices(i))
                                For y = 0 To png.Height - 1
                                    For x = 0 To png.Width - 1
                                        Rectangle(x, y) = Quantizer.QuantizeOnPalette(Rectangle(x, y), Palette, AddressOf ColourDistanceARGB)
                                    Next
                                Next
                                Bitmaps.BitmapData(Bitmaps.Indices(i)) = Rectangle
                            Next
                        Else
                            Using b As New Bitmap(png.Width, png.Height, PixelFormat.Format32bppArgb)
                                Using g = Graphics.FromImage(b)
                                    g.DrawImage(png, 0, 0)
                                End Using
                                For i = 0 To Bitmaps.Indices.Length - 1
                                    Dim Rectangle = b.GetRectangle(0, Height * i, Width, Height)
                                    Dim Palette = Palettes.PaletteData(Palettes.Indices(i))
                                    For y = 0 To b.Height - 1
                                        For x = 0 To b.Width - 1
                                            Rectangle(x, y) = Quantizer.QuantizeOnPalette(Rectangle(x, y), Palette, AddressOf ColourDistanceARGB)
                                        Next
                                    Next
                                    Bitmaps.BitmapData(Bitmaps.Indices(i)) = Rectangle
                                Next
                            End Using
                        End If
                    End Using
                Else
                    Using png As New Bitmap(f)
                        If png.PixelFormat = PixelFormat.Format32bppArgb Then
                            For i = 0 To Bitmaps.Indices.Length - 1
                                Dim Rectangle = png.GetRectangle(0, Height * i, Width, Height)
                                Bitmaps.BitmapData(Bitmaps.Indices(i)) = Rectangle
                            Next
                        Else
                            Using b As New Bitmap(png.Width, png.Height, PixelFormat.Format32bppArgb)
                                Using g = Graphics.FromImage(b)
                                    g.DrawImage(png, 0, 0)
                                End Using
                                For i = 0 To Bitmaps.Indices.Length - 1
                                    Dim Rectangle = b.GetRectangle(0, Height * i, Width, Height)
                                    Bitmaps.BitmapData(Bitmaps.Indices(i)) = Rectangle
                                Next
                            End Using
                        End If
                    End Using
                End If
                Data = m.ToBytes

                Using s = Streams.CreateResizable(GimPath)
                    s.Write(Data)
                End Using
            End If
        Next

        Return 0
    End Function
End Module
