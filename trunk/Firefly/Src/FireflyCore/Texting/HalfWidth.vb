'==========================================================================
'
'  File:        HalfWidth.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 半角字符判定
'  Version:     2011.08.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports Firefly.TextEncoding

Namespace Texting
    ''' <summary>
    ''' 本模块用于判定半角字符。
    ''' </summary>
    ''' <remarks>
    ''' 参见
    ''' Unicode Technical Report #11
    ''' Unicode Character Property "East Asian Width"
    ''' http://unicode.org/reports/tr11-2/
    ''' </remarks>
    Public Module HalfWidth
        Private Ranges As Indexer
        Sub New()
            Dim l As New List(Of Range)

            'H - Halfwidth
            l.Add(New Range(&H20A9, &H20A9))
            l.Add(New Range(&HFF61, &HFF64))
            'N - Narrow
            l.Add(New Range(&H20, &H7E))
            l.Add(New Range(&HA2, &HA3))
            l.Add(New Range(&HA5, &HA6))
            l.Add(New Range(&HAC, &HAC))
            l.Add(New Range(&HAF, &HAF))
            'N - Not-East Asian Neutral
            l.Add(New Range(&H0, &H1F))
            l.Add(New Range(&H7F, &HA0))
            l.Add(New Range(&HA9, &HA9))
            l.Add(New Range(&HAB, &HAB))
            l.Add(New Range(&HAE, &HAE))
            l.Add(New Range(&HB5, &HB5))
            l.Add(New Range(&HBB, &HBB))
            l.Add(New Range(&HC0, &HC5))
            l.Add(New Range(&HC7, &HCF))
            l.Add(New Range(&HD1, &HD6))
            l.Add(New Range(&HD9, &HDD))
            l.Add(New Range(&HE2, &HE5))
            l.Add(New Range(&HE7, &HE7))
            l.Add(New Range(&HEB, &HEB))
            l.Add(New Range(&HEE, &HEF))
            l.Add(New Range(&HF1, &HF1))
            l.Add(New Range(&HF4, &HF6))
            l.Add(New Range(&HFB, &HFB))
            l.Add(New Range(&HFD, &HFD))
            l.Add(New Range(&HFF, &H100))
            l.Add(New Range(&H102, &H110))
            l.Add(New Range(&H112, &H112))
            l.Add(New Range(&H114, &H11A))
            l.Add(New Range(&H11C, &H125))
            l.Add(New Range(&H128, &H12A))
            l.Add(New Range(&H12C, &H130))
            l.Add(New Range(&H134, &H137))
            l.Add(New Range(&H139, &H13E))
            l.Add(New Range(&H143, &H143))
            l.Add(New Range(&H145, &H147))
            l.Add(New Range(&H14C, &H14C))
            l.Add(New Range(&H14E, &H151))
            l.Add(New Range(&H154, &H165))
            l.Add(New Range(&H168, &H16A))
            l.Add(New Range(&H16C, &H1CD))
            l.Add(New Range(&H1CF, &H1CF))
            l.Add(New Range(&H1D1, &H1D1))
            l.Add(New Range(&H1D3, &H1D3))
            l.Add(New Range(&H1D5, &H1D5))
            l.Add(New Range(&H1D7, &H1D7))
            l.Add(New Range(&H1D9, &H1D9))
            l.Add(New Range(&H1DB, &H1DB))
            l.Add(New Range(&H1DD, &H250))
            l.Add(New Range(&H252, &H260))
            l.Add(New Range(&H262, &H2A8))
            l.Add(New Range(&H2B0, &H2C6))
            l.Add(New Range(&H2C8, &H2C8))
            l.Add(New Range(&H2CC, &H2CC))
            l.Add(New Range(&H2CE, &H2CF))
            l.Add(New Range(&H2D1, &H2D7))
            l.Add(New Range(&H2DC, &H2DC))
            l.Add(New Range(&H2DE, &H2DE))
            l.Add(New Range(&H2E0, &H2E9))
            l.Add(New Range(&H374, &H390))
            l.Add(New Range(&H3AA, &H3B0))
            l.Add(New Range(&H3C2, &H3C2))
            l.Add(New Range(&H3CA, &H3EF))
            l.Add(New Range(&H400, &H400))
            l.Add(New Range(&H402, &H40F))
            l.Add(New Range(&H450, &H450))
            l.Add(New Range(&H452, &H486))
            l.Add(New Range(&H490, &H4F9))
            l.Add(New Range(&H531, &H556))
            l.Add(New Range(&H559, &H55F))
            l.Add(New Range(&H561, &H587))
            l.Add(New Range(&H589, &H589))
            l.Add(New Range(&H591, &H5F4))
            l.Add(New Range(&H60C, &H6F9))
            l.Add(New Range(&H901, &H970))
            l.Add(New Range(&H981, &H9FA))
            l.Add(New Range(&HA02, &HA74))
            l.Add(New Range(&HA81, &HAEF))
            l.Add(New Range(&HB01, &HB70))
            l.Add(New Range(&HB82, &HBF2))
            l.Add(New Range(&HC01, &HC6F))
            l.Add(New Range(&HC82, &HCEF))
            l.Add(New Range(&HD02, &HD6F))
            l.Add(New Range(&HE01, &HE5B))
            l.Add(New Range(&HE81, &HEDD))
            l.Add(New Range(&HF00, &HFB9))
            l.Add(New Range(&H10A0, &H10F6))
            l.Add(New Range(&H10FB, &H10FB))
            l.Add(New Range(&H1E00, &H1EF9))
            l.Add(New Range(&H1F00, &H1FFE))
            l.Add(New Range(&H2000, &H200F))
            l.Add(New Range(&H2011, &H2012))
            l.Add(New Range(&H2017, &H2017))
            l.Add(New Range(&H201A, &H201B))
            l.Add(New Range(&H201E, &H201F))
            l.Add(New Range(&H2022, &H2024))
            l.Add(New Range(&H2028, &H202E))

            Ranges = New Indexer(l)
        End Sub

        <Extension()> Public Function IsHalfWidth(ByVal c As Char32) As Boolean
            Return Ranges.Contain(c.Value)
        End Function
    End Module
End Namespace
