'==========================================================================
'
'  File:        PartialStreamEx.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 局部扩展流类
'  Version:     2010.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Math
Imports System.IO

Namespace Streaming
    ''' <summary>
    ''' 局部扩展流类，用于表示一个流上的固定开始位置和长度的流，可以递归表示
    ''' </summary>
    ''' <remarks>注意：一切的异常都由你来处理。</remarks>
    Public Class PartialStreamEx
        Inherits StreamEx

        Protected BasePositionValue As Int64
        Protected BaseLengthValue As Int64
        Protected BaseStreamClose As Boolean
        Protected LengthValue As Int64

        ''' <summary>已重载。初始化新实例。</summary>
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        Public Sub New(ByVal BaseStream As StreamEx, ByVal BasePosition As Int64, ByVal BaseLength As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            Me.BaseStream = BaseStream
            Me.BasePositionValue = BasePosition
            BaseLengthValue = BaseLength
            LengthValue = BaseLength
            MyBase.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        ''' <summary>已重载。初始化新实例。</summary>
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <param name="Length">初始大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        Public Sub New(ByVal BaseStream As StreamEx, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            Me.BaseStream = BaseStream
            Me.BasePositionValue = BasePosition
            If BaseLength < Length Then Throw New ArgumentOutOfRangeException
            BaseLengthValue = BaseLength
            LengthValue = Length
            MyBase.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        ''' <summary>读取字节。</summary>
        Public Overrides Function ReadByte() As Byte
            If Position >= BaseLength Then Throw New EndOfStreamException
            Return MyBase.ReadByte
        End Function
        ''' <summary>写入字节。</summary>
        Public Overrides Sub WriteByte(ByVal b As Byte)
            If Position >= BaseLength Then Throw New EndOfStreamException
            MyBase.WriteByte(b)
        End Sub
        ''' <summary>用字节表示的流的长度。</summary>
        Public Overrides ReadOnly Property Length() As Int64
            Get
                Return LengthValue
            End Get
        End Property
        ''' <summary>用字节表示的流在基流中的的偏移位置。</summary>
        Public Overridable ReadOnly Property BasePosition() As Int64
            Get
                Return BasePositionValue
            End Get
        End Property
        ''' <summary>用字节表示的流的最大长度。</summary>
        Public Overridable ReadOnly Property BaseLength() As Int64
            Get
                Return BaseLengthValue
            End Get
        End Property
        ''' <summary>流的当前位置。</summary>
        Public Overrides Property Position() As Int64
            Get
                Return MyBase.Position - BasePositionValue
            End Get
            Set(ByVal Value As Int64)
                MyBase.Position = BasePositionValue + Value
            End Set
        End Property
        ''' <summary>设置流的长度，不得大于最大大小。</summary>
        Public Overrides Sub SetLength(ByVal Value As Int64)
            If Value < 0 Then Throw New ArgumentOutOfRangeException
            If Value > BaseLength Then Throw New ArgumentOutOfRangeException
            If BasePositionValue + Value > MyBase.Length Then MyBase.SetLength(BasePositionValue + Value)
            LengthValue = Value
        End Sub
        ''' <summary>已重载。读取到字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
        Public Overrides Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
            If Position + Count > Length Then Throw New EndOfStreamException
            MyBase.Read(Buffer, Offset, Count)
        End Sub
        ''' <summary>已重载。写入字节数组。</summary>
        ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
        Public Overrides Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
            If Position + Count > BaseLength Then Throw New EndOfStreamException
            MyBase.Write(Buffer, Offset, Count)
            If Position > Length Then LengthValue = Position
        End Sub

        Protected Overrides Sub DisposeManagedResource()
            If BaseStreamClose Then
                BaseStream.Dispose()
            Else
                BaseStream.Flush()
            End If
            BaseStream = Nothing
            MyBase.DisposeManagedResource()
        End Sub
    End Class
End Namespace
