'==========================================================================
'
'  File:        NumericStrings.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 数值字符串操作
'  Version:     2011.03.22.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Globalization
Imports System.Runtime.CompilerServices

Public Module NumericStrings
    Public Function InvariantParseUInt8(ByVal s As String) As Byte
        Return Byte.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseUInt16(ByVal s As String) As UInt16
        Return UInt16.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseUInt32(ByVal s As String) As UInt32
        Return UInt32.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseUInt64(ByVal s As String) As UInt64
        Return UInt64.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseInt8(ByVal s As String) As SByte
        Return SByte.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseInt16(ByVal s As String) As Int16
        Return Int16.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseInt32(ByVal s As String) As Int32
        Return Int32.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseInt64(ByVal s As String) As Int64
        Return Int64.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseFloat32(ByVal s As String) As Single
        Return Single.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseFloat64(ByVal s As String) As Double
        Return Double.Parse(s, CultureInfo.InvariantCulture)
    End Function
    Public Function InvariantParseBoolean(ByVal s As String) As Boolean
        Return Boolean.Parse(s)
    End Function
    Public Function InvariantParseDecimal(ByVal s As String) As Decimal
        Return Decimal.Parse(s, CultureInfo.InvariantCulture)
    End Function

    <Extension()> Public Function ToInvariantString(ByVal i As Byte) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As UInt16) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As UInt32) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As UInt64) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As SByte) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As Int16) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As Int32) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As Int64) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal f As Single) As String
        Return f.ToString("r", CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal f As Double) As String
        Return f.ToString("r", CultureInfo.InvariantCulture)
    End Function
    <Extension()> Public Function ToInvariantString(ByVal b As Boolean) As String
        Return b.ToString()
    End Function
    <Extension()> Public Function ToInvariantString(ByVal i As Decimal) As String
        Return i.ToString(CultureInfo.InvariantCulture)
    End Function
End Module
