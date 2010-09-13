'==========================================================================
'
'  File:        CRC32.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: CRC32计算
'  Version:     2008.10.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

''' <summary>CRC32类</summary>
''' <remarks>
''' 按照IEEE-802标准，参考RFC3385。
''' </remarks>
Public Class CRC32
    Private Table() As Int32
    Private Result As Int32

    Public Sub Reset()
        Result = &HFFFFFFFF
    End Sub
    Public Sub PushData(ByVal b As Byte)
        Dim iLookup As Integer = (Result And &HFF) Xor b
        Result = (Result >> 8) And &HFFFFFF
        Result = Result Xor Table(iLookup)
    End Sub
    Public Function GetCRC32() As Int32
        Return Not Result
    End Function

    Public Sub New()
        'g(x) = x^32 + x^26 + x^23 + x^22 + x^16 + x^12 + x^11 + x^10 + x^8 + x^7 + x^5 + x^4 + x^2 + x + 1
        '多项式系数的位数组表示104C11DB7
        Dim Coefficients As Int32 = &HEDB88320 '反向表示

        Table = New Int32(255) {}

        For i = 0 To 255
            Dim CRC As Int32 = i
            For j = 0 To 7
                If CBool(CRC And 1) Then
                    CRC = (CRC >> 1) And &H7FFFFFFF
                    CRC = CRC Xor Coefficients
                Else
                    CRC = (CRC >> 1) And &H7FFFFFFF
                End If
            Next
            Table(i) = CRC
        Next

        Reset()
    End Sub
End Class
