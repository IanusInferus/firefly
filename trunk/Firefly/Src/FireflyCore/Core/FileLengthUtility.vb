'==========================================================================
'
'  File:        FileLengthUtility.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 文件长度辅助函数库
'  Version:     2009.07.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO

Public Module FileLengthUtility
    ''' <summary>已重载。得到数组的差分，用Sum参数放在Value最后来凑齐</summary>
    Public Function GetDifference(ByVal Value As Int16(), ByVal Sum As Int16) As Int16()
        Dim ret As Int16() = New Int16(Value.Length - 1) {}
        Dim Upper As Int16 = Sum
        For n As Integer = Value.Length - 1 To 0 Step -1
            ret(n) = Upper - Value(n)
            Upper = Value(n)
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到数组的差分，用Sum参数放在Value最后来凑齐</summary>
    Public Function GetDifference(ByVal Value As UInt16(), ByVal Sum As UInt16) As UInt16()
        Dim ret As UInt16() = New UInt16(Value.Length - 1) {}
        Dim Upper As UInt16 = Sum
        For n As Integer = Value.Length - 1 To 0 Step -1
            ret(n) = Upper - Value(n)
            Upper = Value(n)
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到数组的差分，用Sum参数放在Value最后来凑齐</summary>
    Public Function GetDifference(ByVal Value As Int32(), ByVal Sum As Int32) As Int32()
        Dim ret As Int32() = New Int32(Value.Length - 1) {}
        Dim Upper As Int32 = Sum
        For n As Integer = Value.Length - 1 To 0 Step -1
            ret(n) = Upper - Value(n)
            Upper = Value(n)
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到数组的差分，用Sum参数放在Value最后来凑齐</summary>
    Public Function GetDifference(ByVal Value As Int64(), ByVal Sum As Int64) As Int64()
        Dim ret As Int64() = New Int64(Value.Length - 1) {}
        Dim Upper As Int64 = Sum
        For n As Integer = Value.Length - 1 To 0 Step -1
            ret(n) = Upper - Value(n)
            Upper = Value(n)
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到数组的求和，是GetDifference的逆运算</summary>
    Public Function GetSummation(ByVal Intial As Int16, ByVal Value As Int16()) As Int16()
        Dim ret As Int16() = New Int16(Value.Length - 1) {}
        Dim Address As Int16 = Intial
        For n As Integer = 0 To Value.Length - 1
            ret(n) = Address
            Address += Value(n)
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到数组的求和，是GetDifference的逆运算</summary>
    Public Function GetSummation(ByVal Intial As UInt16, ByVal Value As UInt16()) As UInt16()
        Dim ret As UInt16() = New UInt16(Value.Length - 1) {}
        Dim Address As UInt16 = Intial
        For n As Integer = 0 To Value.Length - 1
            ret(n) = Address
            Address += Value(n)
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到数组的求和，是GetDifference的逆运算</summary>
    Public Function GetSummation(ByVal Intial As Int32, ByVal Value As Int32()) As Int32()
        Dim ret As Int32() = New Int32(Value.Length - 1) {}
        Dim Address As Int32 = Intial
        For n As Integer = 0 To Value.Length - 1
            ret(n) = Address
            Address += Value(n)
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到数组的求和，是GetDifference的逆运算</summary>
    Public Function GetSummation(ByVal Intial As Int64, ByVal Value As Int64()) As Int64()
        Dim ret As Int64() = New Int64(Value.Length - 1) {}
        Dim Address As Int64 = Intial
        For n As Integer = 0 To Value.Length - 1
            ret(n) = Address
            Address += Value(n)
        Next
        Return ret
    End Function


    ''' <summary>已重载。得到地址列的差分，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifference(ByVal Address As Int16(), ByVal Length As Int16) As Int16()
        Dim ret As Int16() = New Int16(Address.Length - 1) {}
        Dim Upper As Int16 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的差分，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifference(ByVal Address As UInt16(), ByVal Length As UInt16) As UInt16()
        Dim ret As UInt16() = New UInt16(Address.Length - 1) {}
        Dim Upper As UInt16 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的差分，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifference(ByVal Address As Int32(), ByVal Length As Int32) As Int32()
        Dim ret As Int32() = New Int32(Address.Length - 1) {}
        Dim Upper As Int32 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的差分，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifference(ByVal Address As Int64(), ByVal Length As Int64) As Int64()
        Dim ret As Int64() = New Int64(Address.Length - 1) {}
        Dim Upper As Int64 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的求和，是GetAddressDifference的逆运算，长度为0则Address置为0</summary>
    Public Function GetAddressSummation(ByVal BaseAddress As Int16, ByVal Length As Int16()) As Int16()
        Dim ret As Int16() = New Int16(Length.Length - 1) {}
        Dim Address As Int16 = BaseAddress
        For n As Integer = 0 To Length.Length - 1
            If Length(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Address
                Address += Length(n)
            End If
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的求和，是GetAddressDifference的逆运算，长度为0则Address置为0</summary>
    Public Function GetAddressSummation(ByVal BaseAddress As UInt16, ByVal Length As UInt16()) As UInt16()
        Dim ret As UInt16() = New UInt16(Length.Length - 1) {}
        Dim Address As UInt16 = BaseAddress
        For n As Integer = 0 To Length.Length - 1
            If Length(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Address
                Address += Length(n)
            End If
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的求和，是GetAddressDifference的逆运算，长度为0则Address置为0</summary>
    Public Function GetAddressSummation(ByVal BaseAddress As Int32, ByVal Length As Int32()) As Int32()
        Dim ret As Int32() = New Int32(Length.Length - 1) {}
        Dim Address As Int32 = BaseAddress
        For n As Integer = 0 To Length.Length - 1
            If Length(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Address
                Address += Length(n)
            End If
        Next
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的求和，是GetAddressDifference的逆运算，长度为0则Address置为0</summary>
    Public Function GetAddressSummation(ByVal BaseAddress As Int64, ByVal Length As Int64()) As Int64()
        Dim ret As Int64() = New Int64(Length.Length - 1) {}
        Dim Address As Int64 = BaseAddress
        For n As Integer = 0 To Length.Length - 1
            If Length(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Address
                Address += Length(n)
            End If
        Next
        Return ret
    End Function

    ''' <summary>已重载。得到地址列的对应的长度，地址列可以乱序，但必须完备，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifferenceUnordered(ByVal Address As Int16(), ByVal Length As Int16) As Int16()
        Address = CType(Address.Clone, Int16())
        Dim ret = New Int16(Address.Length - 1) {}
        Dim Index = New Integer(Address.Length - 1) {}
        For n = 0 To Address.Length - 1
            Index(n) = n
        Next
        Array.Sort(Address, Index)
        Dim Upper As Int16 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Array.Sort(Index, ret)
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的对应的长度，地址列可以乱序，但必须完备，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifferenceUnordered(ByVal Address As UInt16(), ByVal Length As UInt16) As UInt16()
        Address = CType(Address.Clone, UInt16())
        Dim ret = New UInt16(Address.Length - 1) {}
        Dim Index = New Integer(Address.Length - 1) {}
        For n = 0 To Address.Length - 1
            Index(n) = n
        Next
        Array.Sort(Address, Index)
        Dim Upper As UInt16 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Array.Sort(Index, ret)
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的对应的长度，地址列可以乱序，但必须完备，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifferenceUnordered(ByVal Address As Int32(), ByVal Length As Int32) As Int32()
        Address = CType(Address.Clone, Int32())
        Dim ret = New Int32(Address.Length - 1) {}
        Dim Index = New Integer(Address.Length - 1) {}
        For n = 0 To Address.Length - 1
            Index(n) = n
        Next
        Array.Sort(Address, Index)
        Dim Upper As Int32 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Array.Sort(Index, ret)
        Return ret
    End Function
    ''' <summary>已重载。得到地址列的对应的长度，地址列可以乱序，但必须完备，用Length参数放在Address最后来凑齐，Address为0表示长度为0</summary>
    Public Function GetAddressDifferenceUnordered(ByVal Address As Int64(), ByVal Length As Int64) As Int64()
        Address = CType(Address.Clone, Int64())
        Dim ret = New Int64(Address.Length - 1) {}
        Dim Index = New Integer(Address.Length - 1) {}
        For n = 0 To Address.Length - 1
            Index(n) = n
        Next
        Array.Sort(Address, Index)
        Dim Upper As Int64 = Length
        For n As Integer = Address.Length - 1 To 0 Step -1
            If Address(n) = 0 Then
                ret(n) = 0
            Else
                ret(n) = Upper - Address(n)
                Upper = Address(n)
            End If
        Next
        Array.Sort(Index, ret)
        Return ret
    End Function
End Module
