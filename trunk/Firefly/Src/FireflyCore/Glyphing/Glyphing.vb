'==========================================================================
'
'  File:        Glyphing.vb
'  Location:    Firefly.Glyphing <Visual Basic .Net>
'  Description: 图形绘制相关函数
'  Version:     2010.08.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports Firefly
Imports Firefly.Imaging
Imports Firefly.TextEncoding
Imports Firefly.Texting

Namespace Glyphing
    Public Module Glyphing
        ''' <summary>测定字符串显示的宽度。</summary>
        <Extension()> Public Function MeasureStringWidth(ByVal g As Graphics, ByVal Text As String, ByVal f As Font) As Single
            If Text.Length = 0 Then Return 0
            Using format As StringFormat = New System.Drawing.StringFormat(StringFormatFlags.MeasureTrailingSpaces Or StringFormatFlags.NoClip Or StringFormatFlags.FitBlackBox)
                Dim rect As RectangleF = New System.Drawing.RectangleF(0, 0, 0, 0)
                Dim ranges As CharacterRange() = {New System.Drawing.CharacterRange(0, Text.Length)}
                format.SetMeasurableCharacterRanges(ranges)
                Dim regions As Region() = g.MeasureCharacterRanges(Text, f, rect, format)
                rect = regions(0).GetBounds(g)
                Return rect.Width
            End Using
        End Function
        ''' <summary>测定字符串显示的矩形。</summary>
        <Extension()> Public Function MeasureStringRectangle(ByVal g As Graphics, ByVal Text As String, ByVal f As Font) As RectangleF
            If Text.Length = 0 Then Return New RectangleF(0, 0, 0, 0)
            Using format As StringFormat = New System.Drawing.StringFormat(StringFormatFlags.MeasureTrailingSpaces Or StringFormatFlags.NoClip Or StringFormatFlags.FitBlackBox)
                Dim rect As RectangleF = New System.Drawing.RectangleF(0, 0, 0, 0)
                Dim ranges As CharacterRange() = {New System.Drawing.CharacterRange(0, Text.Length)}
                format.SetMeasurableCharacterRanges(ranges)
                Dim regions As Region() = g.MeasureCharacterRanges(Text, f, rect, format)
                rect = regions(0).GetBounds(g)
                Return rect
            End Using
        End Function
    End Module
End Namespace
