'==========================================================================
'
'  File:        BitmapEx.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: Bitmap扩展函数
'  Version:     2010.09.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices
Imports System.Security
Imports System.Security.Permissions

Namespace Imaging

    ''' <summary>
    ''' 用于扩展System.Drawing.Bitmap，使得可以将其中数据与Int32数组交换。
    ''' </summary>
    Public Module BitmapEx
        ''' <summary>
        ''' 从Bitmap中获得颜色数组。
        ''' 仅支持格式为PixelFormat.Format32bppArgb的Bitmap，且BitmapData.Stride必须为宽度的4倍。
        ''' </summary>
        <Extension()>
        Public Function GetRectangle(ByVal Bitmap As Bitmap, ByVal x As Int32, ByVal y As Int32, ByVal w As Int32, ByVal h As Int32) As Int32(,)
            If Bitmap.PixelFormat <> PixelFormat.Format32bppArgb Then Throw New NotSupportedException

            If w < 0 OrElse h < 0 Then Return Nothing
            Dim a As Int32(,) = New Int32(w - 1, h - 1) {}
            If w = 0 Then Return a
            If h = 0 Then Return a
            Dim ox, oy As Integer
            If y < 0 Then
                h = h + y
                oy = 0
            Else
                oy = y
            End If
            If oy + h > Bitmap.Height Then
                h = Bitmap.Height - oy
            End If
            If x < 0 Then
                ox = 0
            Else
                ox = x
            End If
            If ox + w > Bitmap.Width Then
                w = Bitmap.Width - ox
            End If
            Dim xl As Integer = ox - x
            Dim xu As Integer
            If x >= 0 Then
                xu = w + ox - x - 1
            Else
                xu = w - 1
            End If

            If h <= 0 Then Return a

            Dim Rect As New Rectangle(0, oy, Bitmap.Width, h)

            Dim BitmapPixelFormat = Bitmap.PixelFormat
            Dim BitmapWidth = Bitmap.Width

            Dim UnmanagedPermission = New SecurityPermission(PermissionState.Unrestricted)
            UnmanagedPermission.Assert()
            Dim BitmapData As System.Drawing.Imaging.BitmapData = Bitmap.LockBits(Rect, Drawing.Imaging.ImageLockMode.ReadOnly, BitmapPixelFormat)
            Dim Pixels As Int32()
            Try
                If BitmapData.Stride <> BitmapWidth * 4 Then Throw New NotSupportedException

                Dim Ptr As IntPtr = BitmapData.Scan0
                Dim NumPixels As Integer = (BitmapData.Stride * h) \ 4
                Pixels = New Int32(NumPixels - 1) {}
                Marshal.Copy(Ptr, Pixels, 0, NumPixels)
            Finally
                Bitmap.UnlockBits(BitmapData)
                CodeAccessPermission.RevertAssert()
            End Try

            Dim o = oy - y
            For m As Integer = 0 To h - 1
                For n As Integer = xl To xu
                    a(n, o + m) = Pixels(ox + n + m * BitmapWidth)
                Next
            Next

            Return a
        End Function

        ''' <summary>
        ''' 将颜色数组放入Bitmap。
        ''' 仅支持格式为PixelFormat.Format32bppArgb的Bitmap，且BitmapData.Stride必须为宽度的4倍。
        ''' </summary>
        <Extension()> Public Sub SetRectangle(ByVal Bitmap As Bitmap, ByVal x As Int32, ByVal y As Int32, ByVal a As Int32(,))
            If Bitmap.PixelFormat <> PixelFormat.Format32bppArgb Then Throw New NotSupportedException

            If a Is Nothing Then Return
            Dim w As Integer = a.GetLength(0)
            Dim h As Integer = a.GetLength(1)
            If w <= 0 Then Return
            If h <= 0 Then Return
            Dim ox, oy As Integer
            If y < 0 Then
                h = h + y
                oy = 0
            Else
                oy = y
            End If
            If oy + h > Bitmap.Height Then
                h = Bitmap.Height - oy
            End If
            If x < 0 Then
                ox = 0
            Else
                ox = x
            End If
            If ox + w > Bitmap.Width Then
                w = Bitmap.Width - ox
            End If
            Dim xl As Integer = ox - x
            Dim xu As Integer
            If x >= 0 Then
                xu = w + ox - x - 1
            Else
                xu = w - 1
            End If

            If h <= 0 Then Return

            Dim Rect As New Rectangle(0, oy, Bitmap.Width, h)

            Dim BitmapPixelFormat = Bitmap.PixelFormat
            Dim BitmapWidth = Bitmap.Width

            Dim UnmanagedPermission = New SecurityPermission(PermissionState.Unrestricted)
            UnmanagedPermission.Assert()
            Dim BitmapData As System.Drawing.Imaging.BitmapData = Bitmap.LockBits(Rect, Drawing.Imaging.ImageLockMode.ReadWrite, BitmapPixelFormat)
            Try
                If BitmapData.Stride <> BitmapWidth * 4 Then Throw New NotSupportedException

                Dim Ptr As IntPtr = BitmapData.Scan0
                Dim NumPixels As Integer = (BitmapData.Stride * h) \ 4
                Dim Pixels As Int32() = New Int32(NumPixels - 1) {}
                Marshal.Copy(Ptr, Pixels, 0, NumPixels)

                Dim o = oy - y
                For m As Integer = 0 To h - 1
                    For n As Integer = xl To xu
                        Pixels(ox + n + m * BitmapWidth) = a(n, o + m)
                    Next
                Next

                Marshal.Copy(Pixels, 0, Ptr, NumPixels)
            Finally
                Bitmap.UnlockBits(BitmapData)
                CodeAccessPermission.RevertAssert()
            End Try
        End Sub
    End Module
End Namespace
