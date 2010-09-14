'==========================================================================
'
'  File:        ArrayStream.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 数组流
'  Version:     2010.09.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

''' <summary>
''' 数组流
''' </summary>
''' <remarks>
''' 请显式调用Close或Dispose来关闭流。
''' </remarks>
Public Class ArrayStream(Of T)
    Implements IDisposable
    Private BaseArray As T()
    Private BasePositionValue As Integer
    Private PositionValue As Integer
    Private LengthValue As Integer

    ''' <summary>已重载。初始化新实例。</summary>
    Sub New(ByVal Length As Integer)
        If Length < 0 Then Throw New ArgumentOutOfRangeException
        BaseArray = New T(Length - 1) {}
        BasePositionValue = 0
        PositionValue = 0
        LengthValue = Length
    End Sub
    ''' <summary>已重载。初始化新实例。</summary>
    Sub New(ByVal BaseArray As T(), Optional ByVal BasePosition As Integer = 0)
        If BaseArray Is Nothing Then Throw New ArgumentNullException
        If BasePosition < 0 OrElse BasePosition > BaseArray.Length Then Throw New ArgumentOutOfRangeException
        Me.BaseArray = BaseArray
        Me.BasePositionValue = BasePosition
        Me.PositionValue = BasePosition
        Me.LengthValue = BaseArray.Length - BasePosition
    End Sub

    ''' <summary>已重载。初始化新实例。</summary>
    Sub New(ByVal BaseArray As T(), ByVal BasePosition As Integer, ByVal Length As Integer)
        If BaseArray Is Nothing Then Throw New ArgumentNullException
        If Length < 0 Then Throw New ArgumentOutOfRangeException
        If BasePosition < 0 OrElse BasePosition + Length > BaseArray.Length Then Throw New ArgumentOutOfRangeException
        Me.BaseArray = BaseArray
        Me.BasePositionValue = BasePosition
        Me.PositionValue = BasePosition
        Me.LengthValue = Length
    End Sub

    ''' <summary>读取元素。</summary>
    Overridable Function ReadElement() As T
        Dim t As T = BaseArray(PositionValue)
        PositionValue += 1
        Return t
    End Function
    ''' <summary>写入元素。</summary>
    Overridable Sub WriteElement(ByVal b As T)
        BaseArray(PositionValue) = b
        PositionValue += 1
    End Sub

    ''' <summary>强制同步缓冲数据。</summary>
    Overridable Sub Flush()
    End Sub
    ''' <summary>关闭流。</summary>
    ''' <remarks>对继承者的说明：该方法调用Dispose()，不要覆盖该方法，而应覆盖Dispose(Boolean)</remarks>
    Overridable Sub Close()
        Static Closed As Boolean = False
        If Closed Then Throw New InvalidOperationException
        Dispose()
        Closed = True
    End Sub
    ''' <summary>用字节表示的流的长度。</summary>
    Overridable ReadOnly Property Length() As Int64
        Get
            Return LengthValue
        End Get
    End Property
    ''' <summary>流的当前位置。</summary>
    Overridable Property Position() As Integer
        Get
            Return PositionValue - BasePositionValue
        End Get
        Set(ByVal Value As Integer)
            PositionValue = BasePositionValue + Value
        End Set
    End Property
    ''' <summary>已重载。读取到元素数组。</summary>
    ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
    Overridable Sub Read(ByVal Buffer As T(), ByVal Offset As Integer, ByVal Count As Integer)
        If Count < 0 OrElse PositionValue + Count > BasePositionValue + LengthValue Then Throw New ArgumentOutOfRangeException
        Array.Copy(BaseArray, PositionValue, Buffer, Offset, Count)
        PositionValue += Count
    End Sub
    ''' <summary>已重载。读取到元素数组。</summary>
    Sub Read(ByVal Buffer() As T)
        Read(Buffer, 0, Buffer.Length)
    End Sub
    ''' <summary>已重载。读取元素数组。</summary>
    Function Read(ByVal Count As Integer) As T()
        Dim d As T() = New T(Count - 1) {}
        Read(d, 0, Count)
        Return d
    End Function
    ''' <summary>已重载。写入元素数组。</summary>
    ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
    Overridable Sub Write(ByVal Buffer As T(), ByVal Offset As Integer, ByVal Count As Integer)
        If Count < 0 OrElse PositionValue + Count > BasePositionValue + LengthValue Then Throw New ArgumentOutOfRangeException
        Array.Copy(Buffer, Offset, BaseArray, PositionValue, Count)
        PositionValue += Count
    End Sub
    ''' <summary>已重载。写入元素数组。</summary>
    Sub Write(ByVal Buffer As T())
        Write(Buffer, 0, Buffer.Length)
    End Sub

    ''' <summary>读取到外部流。</summary>
    Sub ReadToStream(ByVal s As ArrayStream(Of T), ByVal Count As Integer)
        If Count <= 0 Then Return
        Dim Buffer As T() = New T(CInt(Min(Count, 4 * (1 << 10)) - 1)) {}
        For n As Integer = 0 To Count - Buffer.Length Step Buffer.Length
            Read(Buffer)
            s.Write(Buffer)
        Next
        Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
        Read(Buffer, 0, LeftLength)
        s.Write(Buffer, 0, LeftLength)
    End Sub
    ''' <summary>从外部流写入。</summary>
    Sub WriteFromStream(ByVal s As ArrayStream(Of T), ByVal Count As Integer)
        If Count <= 0 Then Return
        Dim Buffer As T() = New T(CInt(Min(Count, 4 * (1 << 10)) - 1)) {}
        For n As Integer = 0 To Count - Buffer.Length Step Buffer.Length
            s.Read(Buffer)
            Write(Buffer)
        Next
        Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
        s.Read(Buffer, 0, LeftLength)
        Write(Buffer, 0, LeftLength)
    End Sub

#Region " IDisposable 支持 "
    ''' <summary>释放托管对象或间接非托管对象(Stream等)。可在这里将大型字段设置为 null。</summary>
    Protected Overridable Sub DisposeManagedResource()
        BaseArray = Nothing
    End Sub

    ''' <summary>释放直接非托管对象(Handle等)。可在这里将大型字段设置为 null。</summary>
    Protected Overridable Sub DisposeUnmanagedResource()
    End Sub

    '检测冗余的调用
    Private DisposedValue As Boolean = False
    ''' <summary>释放流的资源。请优先覆盖DisposeManagedResource、DisposeUnmanagedResource、DisposeNullify方法。如果你直接保存非托管对象(Handle等)，请覆盖Finalize方法，并在其中调用Dispose(False)。</summary>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If DisposedValue Then Return
        DisposedValue = True
        If disposing Then
            DisposeManagedResource()
        End If
        DisposeUnmanagedResource()
    End Sub

    ''' <summary>释放流的资源。</summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>析构。</summary>
    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub
#End Region

End Class
