'==========================================================================
'
'  File:        BitStream.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 位流
'  Version:     2010.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace Streaming
    ''' <summary>
    ''' 位流。字节内部的位的顺序，低位在前。字节间，低位在前。返回的数据的内部，低位在前。
    ''' </summary>
    ''' <remarks>
    ''' 请显式调用Close或Dispose来关闭流。
    ''' </remarks>
    Public NotInheritable Class BitStream
        Implements IDisposable
        Private BaseArray As Byte()
        Private PositionValue As Integer
        Private LengthValue As Integer

        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal Length As Integer)
            If Length < 0 Then Throw New ArgumentOutOfRangeException
            BaseArray = New Byte(Length - 1) {}
            PositionValue = 0
            LengthValue = Length
        End Sub
        ''' <summary>已重载。初始化新实例。</summary>
        Public Sub New(ByVal BaseArray As Byte())
            If BaseArray Is Nothing Then Throw New ArgumentNullException
            Me.BaseArray = BaseArray
            Me.PositionValue = 0
            Me.LengthValue = BaseArray.Length * 8
        End Sub

        ''' <summary>读取到Byte。</summary>
        Public Function ReadToByte(ByVal i As Integer) As Byte
            If i < 0 Then Throw New ArgumentOutOfRangeException
            If i = 0 Then Return 0

            Dim n = PositionValue \ 8
            Dim p = PositionValue Mod 8
            Dim r = 8 - p
            Dim v As Byte = BaseArray(n).Bits(7, p)
            If r < i Then
                n += 1
                v = CByte(ConcatBits(BaseArray(n), 8, v, r).Bits(i - 1, 0))
                r += 8
            Else
                v = v.Bits(i - 1, 0)
            End If
            PositionValue += i
            Return v
        End Function
        ''' <summary>从Byte写入。</summary>
        Public Sub WriteFromByte(ByVal v As Byte, ByVal i As Integer)
            If i < 0 Then Throw New ArgumentOutOfRangeException
            If i = 0 Then Return

            v = v.Bits(i - 1, 0)
            Dim n = PositionValue \ 8
            Dim p = PositionValue Mod 8
            Dim r = 8 - p
            If r >= i Then
                BaseArray(n) = CByte(ConcatBits(BaseArray(n).Bits(7, p + i), 8 - p - i, CByte(v), i, BaseArray(n), p))
                PositionValue += i
                Return
            Else
                BaseArray(n) = CByte(ConcatBits(v, r, BaseArray(n), p))
            End If
            If r < i Then
                n += 1
                BaseArray(n) = CByte(ConcatBits(BaseArray(n), 8 - i + r, v.Bits(i - 1, r), i - r))
            End If
            PositionValue += i
        End Sub

        ''' <summary>读取到Int32。</summary>
        Public Function ReadToInt32(ByVal i As Integer) As Int32
            If i < 0 Then Throw New ArgumentOutOfRangeException
            If i = 0 Then Return 0

            Dim n = PositionValue \ 8
            Dim p = PositionValue Mod 8
            Dim r = 8 - p
            Dim v As Int32 = BaseArray(n).Bits(7, p)
            While r < i
                n += 1
                v = ConcatBits(BaseArray(n), 8, v, r)
                r += 8
            End While
            v = v.Bits(i - 1, 0)
            PositionValue += i
            Return v
        End Function
        ''' <summary>从Int32写入。</summary>
        Public Sub WriteFromInt32(ByVal v As Int32, ByVal i As Integer)
            If i < 0 Then Throw New ArgumentOutOfRangeException
            If i = 0 Then Return

            v = v.Bits(i - 1, 0)
            Dim n = PositionValue \ 8
            Dim p = PositionValue Mod 8
            Dim r = 8 - p
            If r >= i Then
                BaseArray(n) = CByte(ConcatBits(BaseArray(n).Bits(7, p + i), 8 - p - i, CByte(v), i, BaseArray(n), p))
                PositionValue += i
                Return
            Else
                BaseArray(n) = CByte(ConcatBits(v, r, BaseArray(n), p))
            End If
            While r + 8 <= i
                n += 1
                BaseArray(n) = CByte(v.Bits(r + 8 - 1, r))
                r += 8
            End While
            If r < i Then
                n += 1
                BaseArray(n) = CByte(ConcatBits(BaseArray(n), 8 - i + r, v.Bits(i - 1, r), i - r))
            End If
            PositionValue += i
        End Sub

        ''' <summary>强制同步缓冲数据。</summary>
        Public Sub Flush()
        End Sub
        ''' <summary>用字节表示的流的长度。</summary>
        Public ReadOnly Property Length() As Int64
            Get
                Return LengthValue
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Property Position() As Integer
            Get
                Return PositionValue
            End Get
            Set(ByVal Value As Integer)
                PositionValue = Value
            End Set
        End Property

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            BaseArray = Nothing
        End Sub
    End Class
End Namespace
