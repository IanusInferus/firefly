'==========================================================================
'
'  File:        CompressorSelector.vb
'  Location:    Firefly.Compressing <Visual Basic .Net>
'  Description: 压缩方法选择器
'  Version:     2008.11.08.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On

Imports System
Imports System.Math
Imports System.Collections.Generic

Namespace Compressing
    ''' <summary>
    ''' 压缩委托
    ''' 本接口仅用于短数据，即所有数据均可放于内存中。
    ''' </summary>
    Public Delegate Function Compress(ByVal Data As Byte()) As Byte()

    ''' <summary>
    ''' 解压委托
    ''' 本接口仅用于短数据，即所有数据均可放于内存中。
    ''' </summary>
    Public Delegate Function Decompress(ByVal Data As Byte()) As Byte()

    ''' <summary>
    ''' 压缩方法选择器
    ''' 能够对数据尝试输入的一组压缩方法，返回最小的或第一个小于指定大小的压缩数据。
    ''' </summary>
    Public Class CompressorSelector
        Protected Compressors As Compress()

        ''' <summary>
        ''' 靠前的压缩方法会被优先使用。
        ''' </summary>
        Public Sub New(ByVal CompressMethods As Compress())
            If CompressMethods Is Nothing OrElse CompressMethods.Length = 0 Then Throw New ArgumentNullException
            Compressors = CompressMethods
        End Sub

        ''' <summary>
        ''' 逐次尝试，选取最佳压缩率的压缩方法。
        ''' </summary>
        Public Function Compress(ByVal Data As Byte(), Optional ByRef Method As Integer = -1) As Byte()
            Dim BestCompressedData As Byte() = Nothing
            Dim BestMethodLength As Integer = Integer.MaxValue
            Dim BestMethod As Integer = -1
            For n = 0 To Compressors.Length - 1
                Dim CompressedData = Compressors(n)(Data)
                If CompressedData.Length < BestMethodLength Then
                    BestCompressedData = CompressedData
                    BestMethodLength = CompressedData.Length
                    BestMethod = n
                End If
            Next
            Method = BestMethod
            Return BestCompressedData
        End Function

        ''' <summary>
        ''' 逐次尝试，选取第一个能压缩到指定大小的压缩方法。
        ''' </summary>
        Public Function CompressAndFitIn(ByVal Data As Byte(), ByVal Size As Int32, Optional ByRef Method As Integer = -1, Optional ByVal OutputNothingIfNoFit As Boolean = False) As Byte()
            If OutputNothingIfNoFit Then
                Dim BestCompressedData As Byte() = Nothing
                Dim BestMethodLength As Integer = Integer.MaxValue
                Dim BestMethod As Integer = -1
                For n = 0 To Compressors.Length - 1
                    Dim CompressedData = Compressors(n)(Data)
                    If CompressedData.Length <= Size Then
                        Method = n
                        Return CompressedData
                    ElseIf CompressedData.Length < BestMethodLength Then
                        BestCompressedData = CompressedData
                        BestMethodLength = CompressedData.Length
                        BestMethod = n
                    End If
                Next
                Method = BestMethod
                Return BestCompressedData
            Else
                For n = 0 To Compressors.Length - 1
                    Dim CompressedData = Compressors(n)(Data)
                    If CompressedData.Length <= Size Then
                        Method = n
                        Return CompressedData
                    End If
                Next
                Return Nothing
            End If
        End Function
    End Class
End Namespace
