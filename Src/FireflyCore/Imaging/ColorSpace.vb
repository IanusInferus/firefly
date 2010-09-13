'==========================================================================
'
'  File:        ColorSpace.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: 颜色空间变换
'  Version:     2009.10.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math

Namespace Imaging

    ''' <summary>
    ''' 各颜色空间中点的互相转换。
    ''' </summary>
    Public Module ColorSpace
        Public Function YCbCr2RGB(ByVal Y As Byte, ByVal Cb As Byte, ByVal Cr As Byte) As Int32
            Dim R, G, B As Int32

            R = Y + 1.402 * (Cr - 128)
            G = Y - 0.34414 * (Cb - 128) - 0.71414 * (Cr - 128)
            B = Y + 1.772 * (Cb - 128)

            'R = (Y * 256 + Cr * 358 - 45875) >> 8
            'G = (Y * 256 - Cb * 87 - Cr * 183 + 34667) >> 8
            'B = (Y * 256 + Cb * 454 - 58129) >> 8

            If R < 0 Then
                R = 0
            ElseIf R > &HFF Then
                R = &HFF
            End If
            If G < 0 Then
                G = 0
            ElseIf G > &HFF Then
                G = &HFF
            End If
            If B < 0 Then
                B = 0
            ElseIf B > &HFF Then
                B = &HFF
            End If

            Return &HFF000000 Or (R << 16) Or (G << 8) Or B
        End Function
        Public Sub YCbCr2RGB(ByVal Y As Byte, ByVal Cb As Byte, ByVal Cr As Byte, ByRef R As Byte, ByRef G As Byte, ByRef B As Byte)
            Dim tR, tG, tB As Int32

            tR = Y + 1.402 * (Cr - 128)
            tG = Y - 0.34414 * (Cb - 128) - 0.71414 * (Cr - 128)
            tB = Y + 1.772 * (Cb - 128)

            If tR < 0 Then
                tR = 0
            ElseIf tR > &HFF Then
                tR = &HFF
            End If
            If tG < 0 Then
                tG = 0
            ElseIf tG > &HFF Then
                tG = &HFF
            End If
            If tB < 0 Then
                tB = 0
            ElseIf tB > &HFF Then
                tB = &HFF
            End If

            R = tR
            G = tG
            B = tB
        End Sub
        Public Function RGB2YCbCr(ByVal R As Byte, ByVal G As Byte, ByVal B As Byte) As Int32
            Dim Y, Cb, Cr As Int32

            Y = 0.299 * R + 0.587 * G + 0.114 * B
            Cb = -0.1687 * R - 0.3313 * G + 0.5 * B + 128
            Cr = 0.5 * R - 0.4187 * G - 0.0813 * B + 128
            If Y < 0 Then
                Y = 0
            ElseIf Y > 255 Then
                Y = 255
            End If
            If Cb < 0 Then
                Cb = 0
            ElseIf Cb > 255 Then
                Cb = 255
            End If
            If Cr < 0 Then
                Cr = 0
            ElseIf Cr > 255 Then
                Cr = 255
            End If

            Return (Y << 16) Or (Cb << 8) Or Cr
        End Function
        Public Sub RGB2YCbCr(ByVal R As Byte, ByVal G As Byte, ByVal B As Byte, ByRef Y As Byte, ByRef Cb As Byte, ByRef Cr As Byte)
            Dim tY, tCb, tCr As Int32

            tY = 0.299 * R + 0.587 * G + 0.114 * B
            tCb = -0.1687 * R - 0.3313 * G + 0.5 * B + 128
            tCr = 0.5 * R - 0.4187 * G - 0.0813 * B + 128
            If tY < 0 Then
                tY = 0
            ElseIf tY > 255 Then
                tY = 255
            End If
            If tCb < 0 Then
                tCb = 0
            ElseIf tCb > 255 Then
                tCb = 255
            End If
            If tCr < 0 Then
                tCr = 0
            ElseIf tCr > 255 Then
                tCr = 255
            End If

            Y = tY
            Cb = tCb
            Cr = tCr
        End Sub
        Public Function RGB32To16(ByVal ARGB As Int32) As Int16
            Return CID(((ARGB And &HF80000) >> 8) Or ((ARGB And &HFC00) >> 5) Or ((ARGB And &HF8) >> 3))
        End Function
        Public Function RGB32To15(ByVal ARGB As Int32) As Int16
            Return CID(((ARGB And &HF80000) >> 9) Or ((ARGB And &HF800) >> 6) Or ((ARGB And &HF8) >> 3))
        End Function
        Public Function RGB16To32(ByVal RGB16 As Int16) As Int32
            Dim r As Int32 = (EID(RGB16) And &HF800) >> 8
            Dim g As Int32 = (EID(RGB16) And &H7E0) >> 3
            Dim b As Int32 = (EID(RGB16) And &H1F) << 3
            r = r Or (r >> 5)
            g = g Or (g >> 6)
            b = b Or (b >> 5)
            Return &HFF000000 Or (r << 16) Or (g << 8) Or b
        End Function
        Public Function RGB15To32(ByVal RGB15 As Int16) As Int32
            Dim r As Int32 = (EID(RGB15) And &H7C00) >> 7
            Dim g As Int32 = (EID(RGB15) And &H3E0) >> 2
            Dim b As Int32 = (EID(RGB15) And &H1F) << 3
            r = r Or (r >> 5)
            g = g Or (g >> 5)
            b = b Or (b >> 5)
            Return &HFF000000 Or (r << 16) Or (g << 8) Or b
        End Function
        Public Function RGB32ToL8(ByVal ARGB As Int32) As Byte
            Dim L As Int32 = 0.299 * ((ARGB And &HFF0000) >> 16) + 0.587 * ((ARGB And &HFF00) >> 8) + 0.114 * (ARGB And &HFF)
            If L < 0 Then
                L = 0
            ElseIf L > 255 Then
                L = 255
            End If
            Return L
        End Function
        Public Function L8ToRGB32(ByVal L As Byte) As Int32
            Dim g = CInt(L)
            Return &HFF000000 Or (g << 16) Or (g << 8) Or g
        End Function
        Public Sub HSL2RGB(ByVal H As Double, ByVal S As Double, ByVal L As Double, ByRef R As Byte, ByRef G As Byte, ByRef B As Byte)
            'H in [0,1)
            'S in [0,1)
            'L in [0,1)
            If S = 0 Then
                R = L * 255
                G = L * 255
                B = L * 255
            Else
                Dim temp1, temp2 As Double
                If L < 0.5 Then
                    temp2 = L * (1 + S)
                Else
                    temp2 = L + S - L * S
                End If
                temp1 = 2 * L - temp2

                R = 255 * Hue2RGB(temp1, temp2, H + 1 / 3)
                G = 255 * Hue2RGB(temp1, temp2, H)
                B = 255 * Hue2RGB(temp1, temp2, H - 1 / 3)
            End If
        End Sub
        Public Function Hue2RGB(ByVal v1 As Double, ByVal v2 As Double, ByVal vH As Double) As Double
            If vH < 0 Then vH += 1
            If vH > 1 Then vH -= 1
            If 6 * vH < 1 Then Return v1 + (v2 - v1) * 6 * vH
            If 2 * vH < 1 Then Return v2
            If 3 * vH < 2 Then Return v1 + (v2 - v1) * ((2 / 3) - vH) * 6
            Return v1
        End Function
        Public Sub RGB2HSL(ByVal R As Byte, ByVal G As Byte, ByVal B As Byte, ByRef H As Double, ByRef S As Double, ByRef L As Double)
            Dim var_R, var_G, var_B, var_Min, var_Max, del_Max, del_R, del_G, del_B As Double
            var_R = R / 255
            var_G = G / 255
            var_B = B / 255

            var_Min = Min(var_R, Min(var_G, var_B))
            var_Max = Max(var_R, Max(var_G, var_B))
            del_Max = var_Max - var_Min

            L = (var_Max + var_Min) / 2

            If del_Max = 0 Then
                H = 0
                S = 0
            Else
                If L < 0.5 Then
                    S = del_Max / (var_Max + var_Min)
                Else
                    S = del_Max / (2 - var_Max - var_Min)
                End If

                del_R = ((var_Max - var_R) / 6 + del_Max / 2) / del_Max
                del_G = ((var_Max - var_G) / 6 + del_Max / 2) / del_Max
                del_B = ((var_Max - var_B) / 6 + del_Max / 2) / del_Max

                If var_R = var_Max Then
                    H = del_B - del_G
                ElseIf var_G = var_Max Then
                    H = 1 / 3 + del_R - del_B
                ElseIf var_B = var_Max Then
                    H = 2 / 3 + del_G - del_R
                End If
                If H < 0 Then H += 1
                If H > 1 Then H -= 1
            End If
        End Sub
        Public Sub HSV2RGB(ByVal H As Double, ByVal S As Double, ByVal V As Double, ByRef R As Byte, ByRef G As Byte, ByRef B As Byte)
            'H in [0,1)
            'S in [0,1)
            'L in [0,1)
            If S = 0 Then
                R = V * 255
                G = V * 255
                B = V * 255
            Else
                Dim var_h, var_i, var_1, var_2, var_3, var_r, var_g, var_b As Double
                var_h = H * 6
                If var_h = 6 Then var_h = 0
                var_i = Floor(var_h)
                var_1 = V * (1 - S)
                var_2 = V * (1 - S * (var_h - var_i))
                var_3 = V * (1 - S * (1 - (var_h - var_i)))

                If var_i = 0 Then
                    var_r = V
                    var_g = var_3
                    var_b = var_1
                ElseIf var_i = 1 Then
                    var_r = var_2
                    var_g = V
                    var_b = var_1
                ElseIf var_i = 2 Then
                    var_r = var_1
                    var_g = V
                    var_b = var_3
                ElseIf var_i = 3 Then
                    var_r = var_1
                    var_g = var_2
                    var_b = V
                ElseIf var_i = 4 Then
                    var_r = var_3
                    var_g = var_1
                    var_b = V
                Else
                    var_r = V
                    var_g = var_1
                    var_b = var_2
                End If

                R = var_r * 255
                G = var_g * 255
                B = var_b * 255
            End If
        End Sub
        Public Sub RGB2HSV(ByVal R As Byte, ByVal G As Byte, ByVal B As Byte, ByRef H As Double, ByRef S As Double, ByRef V As Double)
            Dim var_R, var_G, var_B, var_Min, var_Max, del_Max, del_R, del_G, del_B As Double
            var_R = R / 255
            var_G = G / 255
            var_B = B / 255

            var_Min = Min(var_R, Min(var_G, var_B))
            var_Max = Max(var_R, Max(var_G, var_B))
            del_Max = var_Max - var_Min

            V = var_Max

            If del_Max = 0 Then
                H = 0
                S = 0
            Else
                S = del_Max / var_Max

                del_R = ((var_Max - var_R) / 6 + del_Max / 2) / del_Max
                del_G = ((var_Max - var_G) / 6 + del_Max / 2) / del_Max
                del_B = ((var_Max - var_B) / 6 + del_Max / 2) / del_Max

                If var_R = var_Max Then
                    H = del_B - del_G
                ElseIf var_G = var_Max Then
                    H = 1 / 3 + del_R - del_B
                ElseIf var_B = var_Max Then
                    H = 2 / 3 + del_G - del_R
                End If
                If H < 0 Then H += 1
                If H > 1 Then H -= 1
            End If
        End Sub

        Public Delegate Function ColorDistance(ByVal L As Int32, ByVal R As Int32) As Integer

        Public Function ColourDistanceRGB(ByVal x As Int32, ByVal y As Int32) As Integer
            Dim xr = x.Bits(23, 16)
            Dim xg = x.Bits(15, 8)
            Dim xb = x.Bits(7, 0)
            Dim yr = y.Bits(23, 16)
            Dim yg = y.Bits(15, 8)
            Dim yb = y.Bits(7, 0)

            Dim rmean As Integer = (xr + yr) \ 2
            Dim r As Integer = xr - yr
            Dim g As Integer = xg - yg
            Dim b As Integer = xb - yb
            Return (510 + rmean) * r * r + 1020 * g * g + (765 - rmean) * b * b
        End Function

        Public Function ColourDistanceARGB(ByVal x As Int32, ByVal y As Int32) As Integer
            Dim xa = x.Bits(31, 24)
            Dim xr = x.Bits(23, 16)
            Dim xg = x.Bits(15, 8)
            Dim xb = x.Bits(7, 0)
            Dim ya = y.Bits(31, 24)
            Dim yr = y.Bits(23, 16)
            Dim yg = y.Bits(15, 8)
            Dim yb = y.Bits(7, 0)

            Dim rmean As Integer = (xr * xa + yr * ya) \ 510
            Dim r As Integer = (xr * xa - yr * ya) \ 255
            Dim g As Integer = (xg * xa - yg * ya) \ 255
            Dim b As Integer = (xb * xa - yb * ya) \ 255
            Dim a As Integer = xa - ya
            Return (510 + rmean) * r * r + 1020 * g * g + (765 - rmean) * b * b + 1530 * a * a
        End Function
    End Module
End Namespace
