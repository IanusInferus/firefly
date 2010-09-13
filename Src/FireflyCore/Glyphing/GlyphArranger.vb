'==========================================================================
'
'  File:        GlyphArranger.vb
'  Location:    Firefly.Glyphing <Visual Basic .Net>
'  Description: 字形集合
'  Version:     2010.04.22.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports Firefly.Imaging

Namespace Glyphing
    Public Delegate Sub SetRectangleFromARGB(ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,))

    Public Interface IGlyphArranger
        Function GetLeastGlyphCount(ByVal PicWidth As Integer, ByVal PicHeight As Integer) As Integer
        Function GetPreferredSize(ByVal Glyphs As IEnumerable(Of IGlyph)) As Size
        Function GetPreferredHeight(ByVal Glyphs As IEnumerable(Of IGlyph), ByVal PicWidth As Integer) As Integer
        Function GetGlyphArrangement(ByVal Glyphs As IEnumerable(Of IGlyph), ByVal PicWidth As Integer, ByVal PicHeight As Integer) As IEnumerable(Of GlyphDescriptor)
    End Interface

    Public Class GlyphArranger
        Implements IGlyphArranger

        Private PhysicalWidth As Integer
        Private PhysicalHeight As Integer

        Public Sub New(ByVal PhysicalWidth As Integer, ByVal PhysicalHeight As Integer)
            If PhysicalWidth <= 0 Then Throw New ArgumentOutOfRangeException
            If PhysicalHeight <= 0 Then Throw New ArgumentOutOfRangeException
            Me.PhysicalWidth = PhysicalWidth
            Me.PhysicalHeight = PhysicalHeight
        End Sub

        Public Function GetLeastGlyphCount(ByVal PicWidth As Integer, ByVal PicHeight As Integer) As Integer Implements IGlyphArranger.GetLeastGlyphCount
            Dim NumGlyphInLine = PicWidth \ PhysicalWidth
            Dim NumGlyphOfPart = NumGlyphInLine * (PicHeight \ PhysicalHeight)
            Return NumGlyphOfPart
        End Function

        Public Function GetPreferredSize(ByVal Glyphs As IEnumerable(Of IGlyph)) As System.Drawing.Size Implements IGlyphArranger.GetPreferredSize
            Dim Count = Glyphs.Count
            Dim k As Integer = Ceiling(Log(Sqrt(Count * PhysicalWidth * PhysicalHeight), 2))
            While True
                Dim PicSize = 2 ^ k

                Dim NumGlyphInLine = PicSize \ PhysicalWidth
                Dim NumGlyphOfPart = NumGlyphInLine * (PicSize \ PhysicalHeight)

                If Count <= NumGlyphOfPart Then Return New Size(PicSize, PicSize)
                k += 1
            End While
        End Function

        Public Function GetPreferredHeight(ByVal Glyphs As IEnumerable(Of IGlyph), ByVal PicWidth As Integer) As Integer Implements IGlyphArranger.GetPreferredHeight
            Dim Count = Glyphs.Count
            Dim k As Integer = Ceiling(Log(Count * PhysicalWidth * PhysicalHeight / PicWidth))
            Dim NumGlyphInLine = PicWidth \ PhysicalWidth

            While True
                Dim PicHeight = 2 ^ k

                Dim NumGlyphOfPart = NumGlyphInLine * (PicHeight \ PhysicalHeight)

                If Count <= NumGlyphOfPart Then Return PicHeight
                k += 1
            End While
            Throw New InvalidOperationException
        End Function

        Public Function GetGlyphArrangement(ByVal Glyphs As System.Collections.Generic.IEnumerable(Of IGlyph), ByVal PicWidth As Integer, ByVal PicHeight As Integer) As System.Collections.Generic.IEnumerable(Of GlyphDescriptor) Implements IGlyphArranger.GetGlyphArrangement
            Dim NumGlyphInLine = PicWidth \ PhysicalWidth
            Dim NumGlyphOfPart = NumGlyphInLine * (PicHeight \ PhysicalHeight)

            Dim l As New List(Of GlyphDescriptor)
            Dim Count = Min(NumGlyphOfPart, Glyphs.Count)

            For GlyphIndex = 0 To Count - 1
                Dim g = Glyphs(GlyphIndex)
                If g.PhysicalWidth > PhysicalWidth Then Throw New InvalidOperationException("PhysicalWidthOverflow:{0}".Formats(g.c.ToString()))
                If g.PhysicalHeight > PhysicalHeight Then Throw New InvalidOperationException("PhysicalHeightOverflow:{0}".Formats(g.c.ToString()))
                Dim x As Integer = (GlyphIndex Mod NumGlyphInLine) * PhysicalWidth
                Dim y As Integer = (GlyphIndex \ NumGlyphInLine) * PhysicalHeight
                l.Add(New GlyphDescriptor With {.c = g.c, .PhysicalBox = New Rectangle(x, y, g.PhysicalWidth, g.PhysicalHeight), .VirtualBox = g.VirtualBox})
            Next

            Return l
        End Function
    End Class

    Public Class GlyphArrangerCompact
        Implements IGlyphArranger

        Private PhysicalWidth As Integer
        Private PhysicalHeight As Integer

        Public Sub New(ByVal PhysicalWidth As Integer, ByVal PhysicalHeight As Integer)
            If PhysicalWidth <= 0 Then Throw New ArgumentOutOfRangeException
            If PhysicalHeight <= 0 Then Throw New ArgumentOutOfRangeException
            Me.PhysicalWidth = PhysicalWidth
            Me.PhysicalHeight = PhysicalHeight
        End Sub

        Public Function GetLeastGlyphCount(ByVal PicWidth As Integer, ByVal PicHeight As Integer) As Integer Implements IGlyphArranger.GetLeastGlyphCount
            Dim NumGlyphInLine = PicWidth \ PhysicalWidth
            Dim NumGlyphOfPart = NumGlyphInLine * (PicHeight \ PhysicalHeight)
            Return NumGlyphOfPart
        End Function

        Public Function GetPreferredSize(ByVal Glyphs As IEnumerable(Of IGlyph)) As System.Drawing.Size Implements IGlyphArranger.GetPreferredSize
            Dim Count = Glyphs.Count
            Dim k As Integer = Ceiling(Log(Sqrt(Count * PhysicalWidth * PhysicalHeight), 2))
            While True
                Dim PicSize = 2 ^ k

                Dim NumGlyphInLine = PicSize \ PhysicalWidth
                Dim NumGlyphOfPart = NumGlyphInLine * (PicSize \ PhysicalHeight)

                If Count <= NumGlyphOfPart Then Return New Size(PicSize, PicSize)
                k += 1
            End While
        End Function

        Public Function GetPreferredHeight(ByVal Glyphs As IEnumerable(Of IGlyph), ByVal PicWidth As Integer) As Integer Implements IGlyphArranger.GetPreferredHeight
            Dim Count = Glyphs.Count
            Dim k As Integer = Ceiling(Log(Count * PhysicalWidth * PhysicalHeight / PicWidth))
            Dim NumGlyphInLine = PicWidth \ PhysicalWidth

            While True
                Dim PicHeight = 2 ^ k

                Dim NumGlyphOfPart = NumGlyphInLine * (PicHeight \ PhysicalHeight)

                If Count <= NumGlyphOfPart Then Return PicHeight
                k += 1
            End While
            Throw New InvalidOperationException
        End Function

        Public Function GetGlyphArrangement(ByVal Glyphs As System.Collections.Generic.IEnumerable(Of IGlyph), ByVal PicWidth As Integer, ByVal PicHeight As Integer) As System.Collections.Generic.IEnumerable(Of GlyphDescriptor) Implements IGlyphArranger.GetGlyphArrangement
            Dim l As New List(Of GlyphDescriptor)

            Dim x As Integer = 0
            Dim y As Integer = 0
            Dim h = 0
            Dim lLine As New List(Of GlyphDescriptor)
            For GlyphIndex = 0 To Glyphs.Count - 1
                Dim g = Glyphs(GlyphIndex)
                If g.PhysicalWidth > PhysicalWidth Then Throw New InvalidOperationException("PhysicalWidthOverflow:{0}".Formats(g.c.ToString()))
                If g.PhysicalHeight > PhysicalHeight Then Throw New InvalidOperationException("PhysicalHeightOverflow:{0}".Formats(g.c.ToString()))
                If x + g.PhysicalWidth > PicWidth Then
                    x = 0
                    If y + h > PicHeight Then
                        Exit For
                    Else
                        y += h
                        l.AddRange(lLine)
                        lLine.Clear()
                        h = 0
                    End If
                End If
                h = Max(h, g.PhysicalHeight)
                If y + h > PicHeight Then
                    Exit For
                End If
                lLine.Add(New GlyphDescriptor With {.c = g.c, .PhysicalBox = New Rectangle(x, y, g.PhysicalWidth, g.PhysicalHeight), .VirtualBox = g.VirtualBox})
                x += g.PhysicalWidth
            Next
            If lLine.Count > 0 Then
                l.AddRange(lLine)
                lLine.Clear()
            End If

            Return l
        End Function
    End Class
End Namespace
