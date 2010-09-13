'==========================================================================
'
'  File:        BitOperations.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 位与32位整数转换
'  Version:     2009.10.11.
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
    ''' <param name="This">Int32</param>
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
    ''' <param name="This">Int32</param>
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
    ''' <param name="This">Byte</param>
    ''' <param name="U">高位索引(7-0)</param>
    ''' <param name="L">低位索引(7-0)</param>
    <Extension()> Public Function Bits(ByVal This As SByte, ByVal U As Integer, ByVal L As Integer) As SByte
        Return CUS(CSU(This).Bits(U, L))
    End Function

    ''' <summary>
    ''' 获得整数的特定位。
    ''' </summary>
    ''' <param name="This">UInt16</param>
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
    ''' <param name="This">Int32</param>
    ''' <param name="U">高位索引(63-0)</param>
    ''' <param name="L">低位索引(63-0)</param>
    <Extension()> Public Function Bits(ByVal This As Int64, ByVal U As Integer, ByVal L As Integer) As Int64
        Return CUS(CSU(This).Bits(U, L))
    End Function


    ''' <summary>
    ''' 已重载。从位构成整数。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SU">次字节高位索引(7-0)</param>
    ''' <param name="SL">次字节低位索引(7-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TU">第三字节高位索引(7-0)</param>
    ''' <param name="TL">第三字节低位索引(7-0)</param>
    ''' <param name="Q">第四字节</param>
    ''' <param name="QU">第四字节高位索引(7-0)</param>
    ''' <param name="QL">第四字节低位索引(7-0)</param>
    ''' <returns>由这些字节的这些位依次从高到低连接得到的整数。</returns>
    Public Function ComposeBits(ByVal H As Byte, ByVal HU As Integer, ByVal HL As Integer, ByVal S As Byte, ByVal SU As Integer, ByVal SL As Integer, ByVal T As Byte, ByVal TU As Integer, ByVal TL As Integer, ByVal Q As Byte, ByVal QU As Integer, ByVal QL As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HU, HL)
        Dim SPart As Int32 = S.Bits(SU, SL)
        Dim TPart As Int32 = T.Bits(TU, TL)
        Dim QPart As Int32 = Q.Bits(QU, QL)
        Return HPart.SAL(SU - SL + 1 + TU - TL + 1 + QU - QL + 1) Or SPart.SAL(TU - TL + 1 + QU - QL + 1) Or TPart.SAL(QU - QL + 1) Or QPart
    End Function
    ''' <summary>
    ''' 已重载。从位构成整数。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SU">次字节高位索引(7-0)</param>
    ''' <param name="SL">次字节低位索引(7-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TU">第三字节高位索引(7-0)</param>
    ''' <param name="TL">第三字节低位索引(7-0)</param>
    ''' <returns>由这些字节的这些位依次从高到低连接得到的整数。</returns>
    Public Function ComposeBits(ByVal H As Byte, ByVal HU As Integer, ByVal HL As Integer, ByVal S As Byte, ByVal SU As Integer, ByVal SL As Integer, ByVal T As Byte, ByVal TU As Integer, ByVal TL As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HU, HL)
        Dim SPart As Int32 = S.Bits(SU, SL)
        Dim TPart As Int32 = T.Bits(TU, TL)
        Return HPart.SAL(SU - SL + 1 + TU - TL + 1) Or SPart.SAL(TU - TL + 1) Or TPart
    End Function
    ''' <summary>
    ''' 已重载。从位构成整数。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SU">次字节高位索引(7-0)</param>
    ''' <param name="SL">次字节低位索引(7-0)</param>
    ''' <returns>由这些字节的这些位依次从高到低连接得到的整数。</returns>
    Public Function ComposeBits(ByVal H As Byte, ByVal HU As Integer, ByVal HL As Integer, ByVal S As Byte, ByVal SU As Integer, ByVal SL As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HU, HL)
        Dim SPart As Int32 = S.Bits(SU, SL)
        Return HPart.SAL(SU - SL + 1) Or SPart
    End Function
    ''' <summary>
    ''' 已重载。从位构成整数。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <returns>由这些字节的这些位依次从高到低连接得到的整数。</returns>
    Public Function ComposeBits(ByVal H As Byte, ByVal HU As Integer, ByVal HL As Integer) As Int32
        Return H.Bits(HU, HL)
    End Function
    ''' <summary>
    ''' 已重载。从位构成整数。
    ''' </summary>
    ''' <param name="H">首Int32</param>
    ''' <param name="HU">首Int32高位索引(31-0)</param>
    ''' <param name="HL">首Int32低位索引(31-0)</param>
    ''' <param name="S">次Int32</param>
    ''' <param name="SU">次Int32高位索引(31-0)</param>
    ''' <param name="SL">次Int32低位索引(31-0)</param>
    ''' <returns>由这些Int32的这些位依次从高到低连接得到的整数。</returns>
    Public Function ComposeBits(ByVal H As Int32, ByVal HU As Integer, ByVal HL As Integer, ByVal S As Int32, ByVal SU As Integer, ByVal SL As Integer) As Int32
        Dim HPart As Int32 = H.Bits(HU, HL)
        Dim SPart As Int32 = S.Bits(SU, SL)
        Return HPart.SAL(SU - SL + 1) Or SPart
    End Function
    ''' <summary>
    ''' 已重载。从位构成整数。
    ''' </summary>
    ''' <param name="H">首Int32</param>
    ''' <param name="HU">首Int32高位索引(31-0)</param>
    ''' <param name="HL">首Int32低位索引(31-0)</param>
    ''' <returns>由这些Int32的这些位依次从高到低连接得到的整数。</returns>
    Public Function ComposeBits(ByVal H As Int32, ByVal HU As Integer, ByVal HL As Integer) As Int32
        Return H.Bits(HU, HL)
    End Function

    ''' <summary>
    ''' 已重载。将整数分解到位。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SU">次字节高位索引(7-0)</param>
    ''' <param name="SL">次字节低位索引(7-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TU">第三字节高位索引(7-0)</param>
    ''' <param name="TL">第三字节低位索引(7-0)</param>
    ''' <param name="Q">第四字节</param>
    ''' <param name="QU">第四字节高位索引(7-0)</param>
    ''' <param name="QL">第四字节低位索引(7-0)</param>
    ''' <param name="Value">待分解的整数。</param>
    Public Sub DecomposeBits(ByRef H As Byte, ByVal HU As Integer, ByVal HL As Integer, ByRef S As Byte, ByVal SU As Integer, ByVal SL As Integer, ByRef T As Byte, ByVal TU As Integer, ByVal TL As Integer, ByRef Q As Byte, ByVal QU As Integer, ByVal QL As Integer, ByVal Value As Int32)
        Dim HPart As Int32 = Value.SAR(SU - SL + 1 + TU - TL + 1 + QU - QL + 1) And (1.SAL(HU - HL + 1) - 1)
        H = H And Not CByte((1.SAL(HU - HL + 1) - 1).SAL(HL))
        H = H Or CByte(HPart.SAL(HL))
        Dim SPart As Int32 = Value.SAR(TU - TL + 1 + QU - QL + 1) And (1.SAL(SU - SL + 1) - 1)
        S = S And Not CByte((1.SAL(SU - SL + 1) - 1).SAL(SL))
        S = S Or CByte(SPart.SAL(SL))
        Dim TPart As Int32 = Value.SAR(QU - QL + 1) And (1.SAL(TU - TL + 1) - 1)
        T = T And Not CByte((1.SAL(TU - TL + 1) - 1).SAL(TL))
        T = T Or CByte(TPart.SAL(TL))
        Dim QPart As Int32 = Value And (1.SAL(QU - QL + 1) - 1)
        Q = Q And Not CByte((1.SAL(QU - QL + 1) - 1).SAL(QL))
        Q = Q Or CByte(QPart.SAL(QL))
    End Sub
    ''' <summary>
    ''' 已重载。将整数分解到位。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SU">次字节高位索引(7-0)</param>
    ''' <param name="SL">次字节低位索引(7-0)</param>
    ''' <param name="T">第三字节</param>
    ''' <param name="TU">第三字节高位索引(7-0)</param>
    ''' <param name="TL">第三字节低位索引(7-0)</param>
    ''' <param name="Value">待分解的整数。</param>
    Public Sub DecomposeBits(ByRef H As Byte, ByVal HU As Integer, ByVal HL As Integer, ByRef S As Byte, ByVal SU As Integer, ByVal SL As Integer, ByRef T As Byte, ByVal TU As Integer, ByVal TL As Integer, ByVal Value As Int32)
        Dim HPart As Int32 = Value.SAR(SU - SL + 1 + TU - TL + 1) And (1.SAL(HU - HL + 1) - 1)
        H = H And Not CByte((1.SAL(HU - HL + 1) - 1).SAL(HL))
        H = H Or CByte(HPart.SAL(HL))
        Dim SPart As Int32 = Value.SAR(TU - TL + 1) And (1.SAL(SU - SL + 1) - 1)
        S = S And Not CByte((1.SAL(SU - SL + 1) - 1).SAL(SL))
        S = S Or CByte(SPart.SAL(SL))
        Dim TPart As Int32 = Value And (1.SAL(TU - TL + 1) - 1)
        T = T And Not CByte((1.SAL(TU - TL + 1) - 1).SAL(TL))
        T = T Or CByte(TPart.SAL(TL))
    End Sub
    ''' <summary>
    ''' 已重载。将整数分解到位。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <param name="S">次字节</param>
    ''' <param name="SU">次字节高位索引(7-0)</param>
    ''' <param name="SL">次字节低位索引(7-0)</param>
    ''' <param name="Value">待分解的整数。</param>
    Public Sub DecomposeBits(ByRef H As Byte, ByVal HU As Integer, ByVal HL As Integer, ByRef S As Byte, ByVal SU As Integer, ByVal SL As Integer, ByVal Value As Int32)
        Dim HPart As Int32 = Value.SAR(SU - SL + 1) And (1.SAL(HU - HL + 1) - 1)
        H = H And Not CByte((1.SAL(HU - HL + 1) - 1).SAL(HL))
        H = H Or CByte(HPart.SAL(HL))
        Dim SPart As Int32 = Value And (1.SAL(SU - SL + 1) - 1)
        S = S And Not CByte((1.SAL(SU - SL + 1) - 1).SAL(SL))
        S = S Or CByte(SPart.SAL(SL))
    End Sub
    ''' <summary>
    ''' 已重载。将整数分解到位。
    ''' </summary>
    ''' <param name="H">首字节</param>
    ''' <param name="HU">首字节高位索引(7-0)</param>
    ''' <param name="HL">首字节低位索引(7-0)</param>
    ''' <param name="Value">待分解的整数。</param>
    Public Sub DecomposeBits(ByRef H As Byte, ByVal HU As Integer, ByVal HL As Integer, ByVal Value As Int32)
        Dim HPart As Int32 = Value And (1.SAL(HU - HL + 1) - 1)
        H = H And Not CByte((1.SAL(HU - HL + 1) - 1).SAL(HL))
        H = H Or CByte(HPart.SAL(HL))
    End Sub
    ''' <summary>
    ''' 已重载。将整数分解到位。
    ''' </summary>
    ''' <param name="H">首Int32</param>
    ''' <param name="HU">首Int32高位索引(31-0)</param>
    ''' <param name="HL">首Int32低位索引(31-0)</param>
    ''' <param name="S">次Int32</param>
    ''' <param name="SU">次Int32高位索引(31-0)</param>
    ''' <param name="SL">次Int32低位索引(31-0)</param>
    ''' <param name="Value">待分解的整数。</param>
    Public Sub DecomposeBits(ByRef H As Int32, ByVal HU As Integer, ByVal HL As Integer, ByRef S As Int32, ByVal SU As Integer, ByVal SL As Integer, ByVal Value As Int32)
        Dim HPart As Int32 = (Value.SAR(SU - SL + 1)) And ((1.SAL(HU - HL + 1)) - 1)
        H = H And Not (1.SAL(HU - HL + 1) - 1).SAL(HL)
        H = H Or HPart.SAL(HL)
        Dim SPart As Int32 = Value And ((1.SAL(SU - SL + 1)) - 1)
        S = S And Not (1.SAL(SU - SL + 1) - 1).SAL(SL)
        S = S Or SPart.SAL(SL)
    End Sub
    ''' <summary>
    ''' 已重载。将整数分解到位。
    ''' </summary>
    ''' <param name="H">首Int32</param>
    ''' <param name="HU">首Int32高位索引(31-0)</param>
    ''' <param name="HL">首Int32低位索引(31-0)</param>
    ''' <param name="Value">待分解的整数。</param>
    Public Sub DecomposeBits(ByRef H As Int32, ByVal HU As Integer, ByVal HL As Integer, ByVal Value As Int32)
        Dim HPart As Int32 = Value And ((1.SAL(HU - HL + 1)) - 1)
        H = H And Not (1.SAL(HU - HL + 1) - 1).SAL(HL)
        H = H Or HPart.SAL(HL)
    End Sub


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
