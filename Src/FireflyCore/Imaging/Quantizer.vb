'==========================================================================
'
'  File:        Quantizer.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: 量化器
'  Version:     2009.01.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict Off
Imports System
Imports System.Collections.Generic

Namespace Imaging

    ''' <summary>量化</summary>
    Public Module Quantizer

        ''' <summary>按调色板量化，使用自定义颜色距离函数。</summary>
        Public Function QuantizeOnPalette(ByVal Color As Int32, ByVal Palette As Int32(), ByVal ColorDistance As ColorDistance) As Byte
            If Palette.Length > 256 Then Throw New NotSupportedException
            Dim Index As Byte
            Dim d As Integer = &H7FFFFFFF
            For n As Integer = 0 To Palette.Length - 1
                Dim cd As Integer = ColorDistance(Color, Palette(n))
                If cd < d Then
                    d = cd
                    Index = CByte(n)
                End If
            Next
            Return Index
        End Function

        ''' <summary>按调色板量化ARGB颜色，使用内置颜色距离函数。</summary>
        Public Function QuantizeOnPalette(ByVal ARGB As Int32, ByVal Palette As Int32()) As Byte
            Return QuantizeOnPalette(ARGB, Palette, AddressOf ColourDistanceARGB)
        End Function

    End Module

    Public Class QuantizerCache
        Private q As Func(Of Int32, Byte)

        Public Sub New(ByVal Quantizer As Func(Of Int32, Byte))
            q = Quantizer
        End Sub

        Private h As New Dictionary(Of Int32, Byte)
        Public Function Quantize(ByVal Color As Int32) As Byte
            If h.ContainsKey(Color) Then Return h(Color)
            Dim qc = q(Color)
            h.Add(Color, qc)
            Return qc
        End Function
    End Class
End Namespace
