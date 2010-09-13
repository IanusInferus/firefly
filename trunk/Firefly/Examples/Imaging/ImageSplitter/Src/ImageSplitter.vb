'==========================================================================
'
'  File:        ImageSplitter.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 图像通道分离器 分离A与RGB
'  Version:     2009.05.10.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports Firefly
Imports Firefly.Imaging

Public Module ImageSplitter

    Public Sub Main(ByVal argv As String())
#If Not DEBUG Then
        Try
#End If
        For Each f In argv
            Dim FileName = GetFileName(f)
            Dim ExtName = GetExtendedFileName(FileName)

            If IsMatchFileMask(FileName, "*.A.*") OrElse IsMatchFileMask(FileName, "*.RGB.*") Then
                Dim rgbFile = GetPath(GetFileDirectory(f), ChangeExtension(GetMainFileName(FileName), "RGB.{0}".Formats(ExtName)))
                Dim aFile = GetPath(GetFileDirectory(f), ChangeExtension(GetMainFileName(FileName), "A.{0}".Formats(ExtName)))
                Dim File = GetPath(GetFileDirectory(f), ChangeExtension(GetMainFileName(FileName), "{0}".Formats(ExtName)))

                Dim Rect As Int32(,)
                Dim RawFormat As ImageFormat
                Using b As New Bitmap(rgbFile)
                    Rect = b.GetRectangle(0, 0, b.Width, b.Height)
                    RawFormat = b.RawFormat
                End Using

                Dim RectA As Int32(,)
                Using b As New Bitmap(aFile)
                    RectA = b.GetRectangle(0, 0, b.Width, b.Height)
                End Using

                Dim Width = Rect.GetLength(0)
                Dim Height = Rect.GetLength(1)
                For y = 0 To Height - 1
                    For x = 0 To Width - 1
                        Dim rgb = Rect(x, y)
                        Dim a = (RectA(x, y).Bits(23, 16) + RectA(x, y).Bits(15, 8) + RectA(x, y).Bits(7, 0)) \ 3
                        Rect(x, y) = ComposeBits(a, 7, 0, rgb, 23, 0)
                    Next
                Next

                Using b As New Bitmap(Width, Height, PixelFormat.Format32bppArgb)
                    b.SetRectangle(0, 0, Rect)
                    b.Save(File, RawFormat)
                End Using
            Else
                Dim rgbFile = ChangeExtension(f, "RGB.{0}".Formats(ExtName))
                Dim aFile = ChangeExtension(f, "A.{0}".Formats(ExtName))
                Dim File = f

                Using b As New Bitmap(File)
                    If b.PixelFormat <> PixelFormat.Format32bppArgb Then
                        Throw New NotSupportedException("OnlySupport PixelFormat.Format32bppArgb")
                    End If

                    Dim Rect = b.GetRectangle(0, 0, b.Width, b.Height)
                    Dim RectA = b.GetRectangle(0, 0, b.Width, b.Height)
                    For y = 0 To b.Height - 1
                        For x = 0 To b.Width - 1
                            Dim argb = Rect(x, y)
                            Dim rgb = argb.Bits(23, 0)
                            Dim a = argb.Bits(31, 24)
                            Rect(x, y) = ComposeBits(&HFF, 7, 0, rgb, 23, 0)
                            RectA(x, y) = ComposeBits(&HFF, 7, 0, a, 7, 0, a, 7, 0, a, 7, 0)
                        Next
                    Next
                    b.SetRectangle(0, 0, Rect)
                    b.Save(rgbFile, b.RawFormat)

                    b.SetRectangle(0, 0, RectA)
                    b.Save(aFile, b.RawFormat)
                End Using
            End If
        Next
#If Not DEBUG Then
        Catch ex As Exception
            ExceptionHandler.PopupException(ex)
        End Try
#End If
    End Sub
End Module
