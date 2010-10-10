'==========================================================================
'
'  File:        BitOperations.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 位与32位整数转换
'  Version:     2010.10.10.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Runtime.CompilerServices

''' <summary>位操作</summary>
Public Module BitOperations

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模8，导致左移8位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHL(ByVal This As Byte, ByVal n As Integer) As Byte
        If n >= 8 Then Return 0
        If n < 0 Then Return SHR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模8，导致右移8位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHR(ByVal This As Byte, ByVal n As Integer) As Byte
        If n >= 8 Then Return 0
        If n < 0 Then Return SHL(This, -n)
        Return This >> n
    End Function

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模16，导致左移16位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHL(ByVal This As UInt16, ByVal n As Integer) As UInt16
        If n >= 16 Then Return 0
        If n < 0 Then Return SHR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模16，导致右移16位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHR(ByVal This As UInt16, ByVal n As Integer) As UInt16
        If n >= 16 Then Return 0
        If n < 0 Then Return SHL(This, -n)
        Return This >> n
    End Function

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模32，导致左移32位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHL(ByVal This As UInt32, ByVal n As Integer) As UInt32
        If n >= 32 Then Return 0
        If n < 0 Then Return SHR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模32，导致右移32位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHR(ByVal This As UInt32, ByVal n As Integer) As UInt32
        If n >= 32 Then Return 0
        If n < 0 Then Return SHL(This, -n)
        Return This >> n
    End Function

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模64，导致左移64位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHL(ByVal This As UInt64, ByVal n As Integer) As UInt64
        If n >= 64 Then Return 0
        If n < 0 Then Return SHR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模64，导致右移64位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SHR(ByVal This As UInt64, ByVal n As Integer) As UInt64
        If n >= 64 Then Return 0
        If n < 0 Then Return SHL(This, -n)
        Return This >> n
    End Function

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模8，导致左移8位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAL(ByVal This As SByte, ByVal n As Integer) As SByte
        If n >= 8 Then Return 0
        If n < 0 Then Return SAR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模8，导致右移8位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAR(ByVal This As SByte, ByVal n As Integer) As SByte
        If n >= 8 Then
            If CBool(This And &H80) Then
                Return -1
            Else
                Return 0
            End If
        End If
        If n < 0 Then Return SAL(This, -n)
        Return This >> n
    End Function

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模16，导致左移16位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAL(ByVal This As Int16, ByVal n As Integer) As Int16
        If n >= 16 Then Return 0
        If n < 0 Then Return SAR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模16，导致右移16位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAR(ByVal This As Int16, ByVal n As Integer) As Int16
        If n >= 16 Then
            If CBool(This And &H8000) Then
                Return -1
            Else
                Return 0
            End If
        End If
        If n < 0 Then Return SAL(This, -n)
        Return This >> n
    End Function

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模32，导致左移32位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAL(ByVal This As Int32, ByVal n As Integer) As Int32
        If n >= 32 Then Return 0
        If n < 0 Then Return SAR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模32，导致右移32位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAR(ByVal This As Int32, ByVal n As Integer) As Int32
        If n >= 32 Then
            If CBool(This And &H80000000) Then
                Return -1
            Else
                Return 0
            End If
        End If
        If n < 0 Then Return SAL(This, -n)
        Return This >> n
    End Function

    ''' <summary>安全的左移位操作，原来的操作由于Intel的原因，会自动对移的位数模64，导致左移64位错为左移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAL(ByVal This As Int64, ByVal n As Integer) As Int64
        If n >= 64 Then Return 0
        If n < 0 Then Return SAR(This, -n)
        Return This << n
    End Function

    ''' <summary>安全的右移位操作，原来的操作由于Intel的原因，会自动对移的位数模64，导致右移64位错为右移0位。</summary>
    ''' <remarks></remarks>
    <Extension()> Public Function SAR(ByVal This As Int64, ByVal n As Integer) As Int64
        If n >= 64 Then
            If CBool(This And &H8000000000000000L) Then
                Return -1
            Else
                Return 0
            End If
        End If
        If n < 0 Then Return SAL(This, -n)
        Return This >> n
    End Function


    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">Byte</param>
    ''' <param name="U">高位索引(7-0)</param>
    ''' <param name="L">低位索引(7-0)</param>
    <Extension()> Public Function Bits(ByVal This As Byte, ByVal U As Integer, ByVal L As Integer) As Byte
        Dim NumBits = U - L + 1
        Dim Mask As Byte
        If NumBits <= 0 Then
            Mask = 0
        ElseIf NumBits >= 8 Then
            Mask = Byte.MaxValue
        Else
            Mask = CByte(1).SHL(NumBits) - CByte(1)
        End If
        Return This.SHR(L) And Mask
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">UInt16</param>
    ''' <param name="U">高位索引(15-0)</param>
    ''' <param name="L">低位索引(15-0)</param>
    <Extension()> Public Function Bits(ByVal This As UInt16, ByVal U As Integer, ByVal L As Integer) As UInt16
        Dim NumBits = U - L + 1
        Dim Mask As UInt16
        If NumBits <= 0 Then
            Mask = 0
        ElseIf NumBits >= 16 Then
            Mask = UInt16.MaxValue
        Else
            Mask = 1US.SHL(NumBits) - 1US
        End If
        Return This.SHR(L) And Mask
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">UInt32</param>
    ''' <param name="U">高位索引(31-0)</param>
    ''' <param name="L">低位索引(31-0)</param>
    <Extension()> Public Function Bits(ByVal This As UInt32, ByVal U As Integer, ByVal L As Integer) As UInt32
        Dim NumBits = U - L + 1
        Dim Mask As UInt32
        If NumBits <= 0 Then
            Mask = 0
        ElseIf NumBits >= 32 Then
            Mask = UInt32.MaxValue
        Else
            Mask = 1UI.SHL(NumBits) - 1UI
        End If
        Return This.SHR(L) And Mask
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">UInt64</param>
    ''' <param name="U">高位索引(63-0)</param>
    ''' <param name="L">低位索引(63-0)</param>
    <Extension()> Public Function Bits(ByVal This As UInt64, ByVal U As Integer, ByVal L As Integer) As UInt64
        Dim NumBits = U - L + 1
        Dim Mask As UInt64
        If NumBits <= 0 Then
            Mask = 0
        ElseIf NumBits >= 64 Then
            Mask = UInt64.MaxValue
        Else
            Mask = 1UL.SHL(NumBits) - 1UL
        End If
        Return This.SHR(L) And Mask
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">SByte</param>
    ''' <param name="U">高位索引(7-0)</param>
    ''' <param name="L">低位索引(7-0)</param>
    <Extension()> Public Function Bits(ByVal This As SByte, ByVal U As Integer, ByVal L As Integer) As SByte
        Return CUS(CSU(This).Bits(U, L))
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">Int16</param>
    ''' <param name="U">高位索引(15-0)</param>
    ''' <param name="L">低位索引(15-0)</param>
    <Extension()> Public Function Bits(ByVal This As Int16, ByVal U As Integer, ByVal L As Integer) As Int16
        Return CUS(CSU(This).Bits(U, L))
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">Int32</param>
    ''' <param name="U">高位索引(31-0)</param>
    ''' <param name="L">低位索引(31-0)</param>
    <Extension()> Public Function Bits(ByVal This As Int32, ByVal U As Integer, ByVal L As Integer) As Int32
        Return CUS(CSU(This).Bits(U, L))
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">Int64</param>
    ''' <param name="U">高位索引(63-0)</param>
    ''' <param name="L">低位索引(63-0)</param>
    <Extension()> Public Function Bits(ByVal This As Int64, ByVal U As Integer, ByVal L As Integer) As Int64
        Return CUS(CSU(This).Bits(U, L))
    End Function


    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">Byte</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(8-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As Byte, ByVal Value As Byte, ByVal Width As Integer) As Byte
        Return This.SHL(Width) Or Value.Bits(Width - 1, 0)
    End Function

    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">UInt16</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(16-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As UInt16, ByVal Value As UInt16, ByVal Width As Integer) As UInt16
        Return This.SHL(Width) Or Value.Bits(Width - 1, 0)
    End Function

    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">UInt32</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(32-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As UInt32, ByVal Value As UInt32, ByVal Width As Integer) As UInt32
        Return This.SHL(Width) Or Value.Bits(Width - 1, 0)
    End Function

    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">UInt64</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(8-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As UInt64, ByVal Value As UInt64, ByVal Width As Integer) As UInt64
        Return This.SHL(Width) Or Value.Bits(Width - 1, 0)
    End Function

    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">SByte</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(8-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As SByte, ByVal Value As SByte, ByVal Width As Integer) As SByte
        Return This.SAL(Width) Or Value.Bits(Width - 1, 0)
    End Function

    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">Int16</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(16-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As Int16, ByVal Value As Int16, ByVal Width As Integer) As Int16
        Return This.SAL(Width) Or Value.Bits(Width - 1, 0)
    End Function

    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">Int32</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(32-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As Int32, ByVal Value As Int32, ByVal Width As Integer) As Int32
        Return This.SAL(Width) Or Value.Bits(Width - 1, 0)
    End Function

    ''' <summary>
    ''' 按位连接整数。
    ''' </summary>
    ''' <param name="This">Int64</param>
    ''' <param name="Value">欲连接的数</param>
    ''' <param name="Width">欲连接的数的位数(8-0)</param>
    <Extension()> Public Function ConcatBits(ByVal This As Int64, ByVal Value As Int64, ByVal Width As Integer) As Int64
        Return This.SAL(Width) Or Value.Bits(Width - 1, 0)
    End Function


    ''' <summary>
    ''' 已重载。从位连接整数。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HW">首字节宽度(8-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SW">次字节宽度(8-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TW">第三字节宽度(8-0)</param>
    ''' <param name="Q">第四字节</param>
    ''' <param name="QW">第四字节宽度(8-0)</param>
    ''' <returns>由这些字节的这些位依次从高到低连接得到的整数。</returns>
    Public Function ConcatBits(ByVal H As Byte, ByVal HW As Integer, ByVal S As Byte, ByVal SW As Integer, ByVal T As Byte, ByVal TW As Integer, ByVal Q As Byte, ByVal QW As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HW - 1, 0)
        Dim SPart As Int32 = S.Bits(SW - 1, 0)
        Dim TPart As Int32 = T.Bits(TW - 1, 0)
        Dim QPart As Int32 = Q.Bits(QW - 1, 0)
        Return HPart.SAL(SW + TW + QW) Or SPart.SAL(TW + QW) Or TPart.SAL(QW) Or QPart
    End Function
    ''' <summary>
    ''' 已重载。从位连接整数。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HW">首字节宽度(8-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SW">次字节宽度(8-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TW">第三字节宽度(8-0)</param>
    ''' <returns>由这些字节的这些位依次从高到低连接得到的整数。</returns>
    Public Function ConcatBits(ByVal H As Byte, ByVal HW As Integer, ByVal S As Byte, ByVal SW As Integer, ByVal T As Byte, ByVal TW As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HW - 1, 0)
        Dim SPart As Int32 = S.Bits(SW - 1, 0)
        Dim TPart As Int32 = T.Bits(TW - 1, 0)
        Return HPart.SAL(SW + TW) Or SPart.SAL(TW) Or TPart
    End Function
    ''' <summary>
    ''' 已重载。从位连接整数。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HW">首字节宽度(8-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SW">次字节宽度(8-0)</param>
    ''' <returns>由这些字节的这些位依次从高到低连接得到的整数。</returns>
    Public Function ConcatBits(ByVal H As Byte, ByVal HW As Integer, ByVal S As Byte, ByVal SW As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HW - 1, 0)
        Dim SPart As Int32 = S.Bits(SW - 1, 0)
        Return HPart.SAL(SW) Or SPart
    End Function
    ''' <summary>
    ''' 已重载。从位连接整数。
    ''' </summary>
    ''' <param name="H">首Int32</param>
    ''' <param name="HW">首Int32宽度(32-0)</param>
    ''' <param name="S">次Int32</param>
    ''' <param name="SW">次Int32宽度(32-0)</param>
    ''' <returns>由这些Int32的这些位依次从高到低连接得到的整数。</returns>
    Public Function ConcatBits(ByVal H As Int32, ByVal HW As Integer, ByVal S As Int32, ByVal SW As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HW - 1, 0)
        Dim SPart As Int32 = S.Bits(SW - 1, 0)
        Return HPart.SAL(SW) Or SPart
    End Function

    ''' <summary>
    ''' 已重载。将整数拆分到位。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HW">首字节宽度(8-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SW">次字节宽度(8-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TW">第三字节宽度(8-0)</param>
    ''' <param name="Q">第四字节</param>
    ''' <param name="QW">第四字节宽度(8-0)</param>
    ''' <param name="Value">待拆分的整数。</param>
    Public Sub SplitBits(ByRef H As Byte, ByVal HW As Integer, ByRef S As Byte, ByVal SW As Integer, ByRef T As Byte, ByVal TW As Integer, ByRef Q As Byte, ByVal QW As Integer, ByVal Value As Int32)
        H = CByte(Value.SAR(SW + TW + QW) And (1.SAL(HW) - 1))
        S = CByte(Value.SAR(TW + QW) And (1.SAL(SW) - 1))
        T = CByte(Value.SAR(QW) And (1.SAL(TW) - 1))
        Q = CByte(Value And (1.SAL(QW) - 1))
    End Sub
    ''' <summary>
    ''' 已重载。将整数拆分到位。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HW">首字节宽度(8-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SW">次字节宽度(8-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TW">第三字节宽度(8-0)</param>
    ''' <param name="Value">待拆分的整数。</param>
    Public Sub SplitBits(ByRef H As Byte, ByVal HW As Integer, ByRef S As Byte, ByVal SW As Integer, ByRef T As Byte, ByVal TW As Integer, ByVal Value As Int32)
        H = CByte(Value.SAR(SW + TW) And (1.SAL(HW) - 1))
        S = CByte(Value.SAR(TW) And (1.SAL(SW) - 1))
        T = CByte(Value And (1.SAL(TW) - 1))
    End Sub
    ''' <summary>
    ''' 已重载。将整数拆分到位。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HW">首字节宽度(8-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SW">次字节宽度(8-0)</param>
    ''' <param name="Value">待拆分的整数。</param>
    Public Sub SplitBits(ByRef H As Byte, ByVal HW As Integer, ByRef S As Byte, ByVal SW As Integer, ByVal Value As Int32)
        H = CByte(Value.SAR(SW) And (1.SAL(HW) - 1))
        S = CByte(Value And (1.SAL(SW) - 1))
    End Sub
    ''' <summary>
    ''' 已重载。将整数拆分到位。
    ''' </summary>
    ''' <param name="H">首Int32</param>
    ''' <param name="HW">首Int32宽度(32-0)</param>
    ''' <param name="S">次Int32</param>
    ''' <param name="SW">次Int32宽度(32-0)</param>
    ''' <param name="Value">待拆分的整数。</param>
    Public Sub SplitBits(ByRef H As Int32, ByVal HW As Integer, ByRef S As Int32, ByVal SW As Integer, ByVal Value As Int32)
        H = CInt(Value.SAR(SW) And (1.SAL(HW) - 1))
        S = CInt(Value And (1.SAL(SW) - 1))
    End Sub
End Module
