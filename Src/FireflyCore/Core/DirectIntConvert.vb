'==========================================================================
'
'  File:        DirectIntConvert.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 直接整数转换
'  Version:     2009.03.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

''' <summary>
''' 直接整数转换
''' </summary>
''' <remarks></remarks>
Public Module DirectIntConvert

    ''' <summary>Int32->Int16</summary>
    Public Function CID(ByVal i As Int32) As Int16
        If CBool(i And &H8000I) Then
            Return CShort((i And &HFFFFI) Or &HFFFF0000I)
        Else
            Return CShort(i)
        End If
    End Function
    ''' <summary>Int64->Int32</summary>
    Public Function CID(ByVal i As Int64) As Int32
        If CBool(i And &H80000000L) Then
            Return CInt((i And &HFFFFFFFFL) Or &HFFFFFFFF00000000L)
        Else
            Return CInt(i)
        End If
    End Function
    ''' <summary>Int16->Int32</summary>
    Public Function EID(ByVal i As Int16) As Int32
        If CBool(i And &H8000S) Then
            Return CInt(i And &H7FFFS) Or &H8000I
        Else
            Return CInt(i)
        End If
    End Function
    ''' <summary>Int32->Int64</summary>
    Public Function EID(ByVal i As Int32) As Int64
        If CBool(i And &H80000000I) Then
            Return CLng(i And &H7FFFFFFFI) Or &H80000000L
        Else
            Return CLng(i)
        End If
    End Function
    ''' <summary>SByte->Byte</summary>
    Public Function CSU(ByVal i As SByte) As Byte
        If CBool(i And &H80I) Then
            Return CByte(i And &H7FI) Or CByte(&H80I)
        Else
            Return CByte(i)
        End If
    End Function
    ''' <summary>Int16->UInt16</summary>
    Public Function CSU(ByVal i As Int16) As UInt16
        If CBool(i And &H8000S) Then
            Return CUShort(i And &H7FFFS) Or &H8000US
        Else
            Return CUShort(i)
        End If
    End Function
    ''' <summary>Int32->UInt32</summary>
    Public Function CSU(ByVal i As Int32) As UInt32
        If CBool(i And &H80000000I) Then
            Return CUInt(i And &H7FFFFFFFI) Or &H80000000UI
        Else
            Return CUInt(i)
        End If
    End Function
    ''' <summary>Int64->UInt64</summary>
    Public Function CSU(ByVal i As Int64) As UInt64
        If CBool(i And &H8000000000000000L) Then
            Return CULng(i And &H7FFFFFFFFFFFFFFFL) Or &H8000000000000000UL
        Else
            Return CULng(i)
        End If
    End Function
    ''' <summary>Byte->SByte</summary>
    Public Function CUS(ByVal i As Byte) As SByte
        If CBool(i And &H80) Then
            Return CSByte(i And &H7FI) Or CSByte(&HFFFFFF80I)
        Else
            Return CSByte(i)
        End If
    End Function
    ''' <summary>UInt16->Int16</summary>
    Public Function CUS(ByVal i As UInt16) As Int16
        If CBool(i And &H8000US) Then
            Return CShort(i And &H7FFFUS) Or &H8000S
        Else
            Return CShort(i)
        End If
    End Function
    ''' <summary>UInt32->Int32</summary>
    Public Function CUS(ByVal i As UInt32) As Int32
        If CBool(i And &H80000000UI) Then
            Return CInt(i And &H7FFFFFFFUI) Or &H80000000I
        Else
            Return CInt(i)
        End If
    End Function
    ''' <summary>UInt64->Int64</summary>
    Public Function CUS(ByVal i As UInt64) As Int64
        If CBool(i And &H8000000000000000UL) Then
            Return CLng(i And &H7FFFFFFFFFFFFFFFUL) Or &H8000000000000000L
        Else
            Return CLng(i)
        End If
    End Function
End Module
