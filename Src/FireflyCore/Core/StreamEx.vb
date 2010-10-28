'==========================================================================
'
'  File:        StreamEx.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 扩展流类
'  Version:     2010.10.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.IO
Imports Firefly.TextEncoding

''' <summary>
''' 扩展流类
''' </summary>
''' <remarks>
''' 请显式调用Close或Dispose来关闭流。
''' 如果调用了ToStream或转换到了Stream，并放弃了StreamEx，StreamEx也不会消失，因为使用了一个继承自Stream的Adapter来持有StreamEx的引用。
''' 本类与System.IO.StreamReader等类不兼容。这些类使用了ReadByte返回的结束标志-1等。本类会在位置超过文件长度时读取会抛出异常。
''' 本类主要用于封装System.IO.MemoryStream和System.IO.FileStream，对其他流可能抛出无法预期的异常。
''' 一切的异常都由调用者来处理。
''' </remarks>
Public Class StreamEx
    Implements IDisposable
    Protected BaseStream As Stream

    ''' <summary>已重载。初始化新实例。</summary>
    Sub New()
        BaseStream = New MemoryStream
    End Sub
    ''' <summary>已重载。初始化新实例。</summary>
    Sub New(ByVal Path As String, ByVal Mode As FileMode, ByVal Access As FileAccess, ByVal Share As FileShare)
        BaseStream = New FileStream(Path, Mode, Access, Share)
    End Sub
    ''' <summary>已重载。初始化新实例。</summary>
    Sub New(ByVal Path As String, ByVal Mode As FileMode, Optional ByVal Access As FileAccess = FileAccess.ReadWrite)
        BaseStream = New FileStream(Path, Mode, Access, FileShare.Read)
    End Sub
    ''' <summary>已重载。初始化新实例。</summary>
    Sub New(ByVal BaseStream As Stream)
        Me.BaseStream = BaseStream
    End Sub
    Shared Widening Operator CType(ByVal s As Stream) As StreamEx
        Dim sa = TryCast(s, StreamAdapter)
        If sa IsNot Nothing Then Return sa.BaseStream
        Return New StreamEx(s)
    End Operator
    Shared Widening Operator CType(ByVal s As StreamEx) As Stream
        Return New StreamAdapter(s)
    End Operator
    Function ToStream() As Stream
        Return New StreamAdapter(Me)
    End Function
    Function ToUnsafeStream() As Stream
        Return New UnsafeStreamAdapter(Me)
    End Function

    ''' <summary>读取Byte。</summary>
    Overridable Function ReadByte() As Byte
        Dim b As Integer = BaseStream.ReadByte
        If b = -1 Then Throw New EndOfStreamException
        Return CByte(b)
    End Function
    ''' <summary>写入Byte。</summary>
    Overridable Sub WriteByte(ByVal b As Byte)
        BaseStream.WriteByte(b)
    End Sub

    ''' <summary>读取Int16。</summary>
    Function ReadInt16() As Int16
        Dim o As Int16
        o = CShort(ReadByte())
        o = o Or (CShort(ReadByte()) << 8)
        Return o
    End Function
    ''' <summary>读取Int32。</summary>
    Function ReadInt32() As Int32
        Dim o As Int32
        o = ReadByte()
        o = o Or (CInt(ReadByte()) << 8)
        o = o Or (CInt(ReadByte()) << 16)
        o = o Or (CInt(ReadByte()) << 24)
        Return o
    End Function
    ''' <summary>读取Int64。</summary>
    Function ReadInt64() As Int64
        Dim o As Int64
        o = ReadByte()
        o = o Or (CLng(ReadByte()) << 8)
        o = o Or (CLng(ReadByte()) << 16)
        o = o Or (CLng(ReadByte()) << 24)
        o = o Or (CLng(ReadByte()) << 32)
        o = o Or (CLng(ReadByte()) << 40)
        o = o Or (CLng(ReadByte()) << 48)
        o = o Or (CLng(ReadByte()) << 56)
        Return o
    End Function

    ''' <summary>写入Int16。</summary>
    Sub WriteInt16(ByVal i As Int16)
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
    End Sub
    ''' <summary>写入Int32。</summary>
    Sub WriteInt32(ByVal i As Int32)
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
    End Sub
    ''' <summary>写入Int64。</summary>
    Sub WriteInt64(ByVal i As Int64)
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
        i = i >> 8
        WriteByte(CByte(i And &HFF))
    End Sub

    ''' <summary>读取Int16，高位优先字节序。</summary>
    Function ReadInt16B() As Int16
        Dim o As Int16
        o = CShort(ReadByte()) << 8
        o = o Or CShort(ReadByte())
        Return o
    End Function
    ''' <summary>读取Int32，高位优先字节序。</summary>
    Function ReadInt32B() As Int32
        Dim o As Int32
        o = CInt(ReadByte()) << 24
        o = o Or (CInt(ReadByte()) << 16)
        o = o Or (CInt(ReadByte()) << 8)
        o = o Or CInt(ReadByte())
        Return o
    End Function
    ''' <summary>读取Int64，高位优先字节序。</summary>
    Function ReadInt64B() As Int64
        Dim o As Int64
        o = CLng(ReadByte()) << 56
        o = o Or (CLng(ReadByte()) << 48)
        o = o Or (CLng(ReadByte()) << 40)
        o = o Or (CLng(ReadByte()) << 32)
        o = o Or (CLng(ReadByte()) << 24)
        o = o Or (CLng(ReadByte()) << 16)
        o = o Or (CLng(ReadByte()) << 8)
        o = o Or CLng(ReadByte())
        Return o
    End Function

    ''' <summary>写入Int16，高位优先字节序。</summary>
    Sub WriteInt16B(ByVal i As Int16)
        WriteByte(CByte(CSU(i) >> 8 And &HFF))
        WriteByte(CByte(i And &HFF))
    End Sub
    ''' <summary>写入Int32，高位优先字节序。</summary>
    Sub WriteInt32B(ByVal i As Int32)
        WriteByte(CByte((CSU(i) >> 24) And &HFF))
        WriteByte(CByte((CSU(i) >> 16) And &HFF))
        WriteByte(CByte((CSU(i) >> 8) And &HFF))
        WriteByte(CByte(i And &HFF))
    End Sub
    ''' <summary>写入Int64，高位优先字节序。</summary>
    Sub WriteInt64B(ByVal i As Int64)
        WriteByte(CByte(CLng(CSU(i) >> 56) And &HFF))
        WriteByte(CByte(CLng(CSU(i) >> 48) And &HFF))
        WriteByte(CByte(CLng(CSU(i) >> 40) And &HFF))
        WriteByte(CByte(CLng(CSU(i) >> 32) And &HFF))
        WriteByte(CByte(CLng(CSU(i) >> 24) And &HFF))
        WriteByte(CByte(CLng(CSU(i) >> 16) And &HFF))
        WriteByte(CByte(CLng(CSU(i) >> 8) And &HFF))
        WriteByte(CByte(i And &HFF))
    End Sub

    ''' <summary>读取Int8。</summary>
    Function ReadInt8() As SByte
        Return CUS(ReadByte())
    End Function
    ''' <summary>写入Int8。</summary>
    Sub WriteInt8(ByVal i As SByte)
        WriteByte(CSU(i))
    End Sub
    ''' <summary>读取UInt8。</summary>
    Function ReadUInt8() As Byte
        Return ReadByte()
    End Function
    ''' <summary>写入UInt8。</summary>
    Sub WriteUInt8(ByVal b As Byte)
        WriteByte(b)
    End Sub

    ''' <summary>读取UInt16。</summary>
    Function ReadUInt16() As UInt16
        Return CSU(ReadInt16)
    End Function
    ''' <summary>写入UInt16。</summary>
    Sub WriteUInt16(ByVal i As UInt16)
        WriteInt16(CUS(i))
    End Sub
    ''' <summary>读取UInt16，高位优先字节序。</summary>
    Function ReadUInt16B() As UInt16
        Return CSU(ReadInt16B)
    End Function
    ''' <summary>写入UInt16，高位优先字节序。</summary>
    Sub WriteUInt16B(ByVal i As UInt16)
        WriteInt16B(CUS(i))
    End Sub

    ''' <summary>读取UInt32。</summary>
    Function ReadUInt32() As UInt32
        Return CSU(ReadInt32)
    End Function
    ''' <summary>写入UInt32。</summary>
    Sub WriteUInt32(ByVal i As UInt32)
        WriteInt32(CUS(i))
    End Sub
    ''' <summary>读取UInt32，高位优先字节序。</summary>
    Function ReadUInt32B() As UInt32
        Return CSU(ReadInt32B)
    End Function
    ''' <summary>写入UInt32，高位优先字节序。</summary>
    Sub WriteUInt32B(ByVal i As UInt32)
        WriteInt32B(CUS(i))
    End Sub

    ''' <summary>读取UInt64。</summary>
    Function ReadUInt64() As UInt64
        Return CSU(ReadInt64)
    End Function
    ''' <summary>写入UInt64。</summary>
    Sub WriteUInt64(ByVal i As UInt64)
        WriteInt64(CUS(i))
    End Sub
    ''' <summary>读取UInt64，高位优先字节序。</summary>
    Function ReadUInt64B() As UInt64
        Return CSU(ReadInt64B)
    End Function
    ''' <summary>写入UInt64，高位优先字节序。</summary>
    Sub WriteUInt64B(ByVal i As UInt64)
        WriteInt64B(CUS(i))
    End Sub

    ''' <summary>读取\0字节结尾的字符串(UTF-16等不适用)。</summary>
    Function ReadString(ByVal Count As Integer, ByVal Encoding As System.Text.Encoding) As String
        Dim Bytes As New List(Of Byte)
        For n = 0 To Count - 1
            Dim b = ReadByte()
            If b = Nul Then
                Position += Count - 1 - n
                Exit For
            Else
                Bytes.Add(b)
            End If
        Next
        Return Encoding.GetChars(Bytes.ToArray)
    End Function
    ''' <summary>写入\0字节结尾的字符串(UTF-16等不适用)。</summary>
    Sub WriteString(ByVal s As String, ByVal Count As Integer, ByVal Encoding As System.Text.Encoding)
        If s = "" Then
            For n = 0 To Count - 1
                WriteByte(0)
            Next
        Else
            Dim Bytes = Encoding.GetBytes(s)
            If Bytes.Length > Count Then Throw New InvalidDataException
            Write(Bytes)
            For n = Bytes.Length To Count - 1
                WriteByte(0)
            Next
        End If
    End Sub
    ''' <summary>读取包括\0字节的字符串(如UTF-16)。</summary>
    Function ReadStringWithNull(ByVal Count As Integer, ByVal Encoding As System.Text.Encoding) As String
        Return Encoding.GetChars(Read(Count))
    End Function

    ''' <summary>读取ASCII字符串。</summary>
    Function ReadSimpleString(ByVal Count As Integer) As String
        Return ReadString(Count, ASCII)
    End Function
    ''' <summary>写入ASCII字符串。</summary>
    Sub WriteSimpleString(ByVal s As String, ByVal Count As Integer)
        WriteString(s, Count, ASCII)
    End Sub
    ''' <summary>写入ASCII字符串。</summary>
    Sub WriteSimpleString(ByVal s As String)
        WriteSimpleString(s, s.Length)
    End Sub
    ''' <summary>读取ASCII字符串(包括\0)。</summary>
    Function ReadSimpleStringWithNull(ByVal Count As Integer) As String
        Return ReadStringWithNull(Count, ASCII)
    End Function

    <System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)> Private Structure SingleInt32
        <System.Runtime.InteropServices.FieldOffset(0)> Public SingleValue As Single
        <System.Runtime.InteropServices.FieldOffset(0)> Public Int32Value As Int32
    End Structure
    ''' <summary>读取单精度浮点数。</summary>
    Function ReadSingle() As Single
        Dim a As SingleInt32
        a.Int32Value = ReadInt32()
        Return a.SingleValue
    End Function
    ''' <summary>写入单精度浮点数。</summary>
    Sub WriteSingle(ByVal f As Single)
        Dim a As SingleInt32
        a.SingleValue = f
        WriteInt32(a.Int32Value)
    End Sub
    ''' <summary>读取单精度浮点数。</summary>
    Function ReadFloat32() As Single
        Dim a As SingleInt32
        a.Int32Value = ReadInt32()
        Return a.SingleValue
    End Function
    ''' <summary>写入单精度浮点数。</summary>
    Sub WriteFloat32(ByVal f As Single)
        Dim a As SingleInt32
        a.SingleValue = f
        WriteInt32(a.Int32Value)
    End Sub

    <System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)> Private Structure DoubleInt64
        <System.Runtime.InteropServices.FieldOffset(0)> Public DoubleValue As Double
        <System.Runtime.InteropServices.FieldOffset(0)> Public Int64Value As Int64
    End Structure
    ''' <summary>读取双精度浮点数。</summary>
    Function ReadDouble() As Double
        Dim a As DoubleInt64
        a.Int64Value = ReadInt64()
        Return a.DoubleValue
    End Function
    ''' <summary>写入双精度浮点数。</summary>
    Sub WriteDouble(ByVal f As Double)
        Dim a As DoubleInt64
        a.DoubleValue = f
        WriteInt64(a.Int64Value)
    End Sub
    ''' <summary>读取双精度浮点数。</summary>
    Function ReadFloat64() As Double
        Dim a As DoubleInt64
        a.Int64Value = ReadInt64()
        Return a.DoubleValue
    End Function
    ''' <summary>写入双精度浮点数。</summary>
    Sub WriteFloat64(ByVal f As Double)
        Dim a As DoubleInt64
        a.DoubleValue = f
        WriteInt64(a.Int64Value)
    End Sub

    ''' <summary>指示当前流是否支持读取。</summary>
    Overridable ReadOnly Property CanRead() As Boolean
        Get
            Return BaseStream.CanRead
        End Get
    End Property
    ''' <summary>指示当前流是否支持定位。</summary>
    Overridable ReadOnly Property CanSeek() As Boolean
        Get
            Return BaseStream.CanSeek
        End Get
    End Property
    ''' <summary>指示当前流是否支持写入。</summary>
    Overridable ReadOnly Property CanWrite() As Boolean
        Get
            Return BaseStream.CanWrite
        End Get
    End Property
    ''' <summary>强制同步缓冲数据。</summary>
    Overridable Sub Flush()
        BaseStream.Flush()
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
            Return BaseStream.Length
        End Get
    End Property
    ''' <summary>流的当前位置。</summary>
    Overridable Property Position() As Int64
        Get
            Return BaseStream.Position
        End Get
        Set(ByVal Value As Int64)
            BaseStream.Position = Value
        End Set
    End Property
    ''' <summary>设置流的当前位置。</summary>
    Function Seek(ByVal Offset As Int64, ByVal Origin As System.IO.SeekOrigin) As Int64
        Select Case Origin
            Case SeekOrigin.Begin
                Position = Offset
            Case SeekOrigin.Current
                Position += Offset
            Case SeekOrigin.End
                Position = Length - Offset
        End Select
        Return Position
    End Function
    ''' <summary>设置流的长度。</summary>
    Overridable Sub SetLength(ByVal Value As Int64)
        BaseStream.SetLength(Value)
    End Sub
    ''' <summary>已重载。读取到字节数组。</summary>
    ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
    Overridable Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
        Dim c As Integer = BaseStream.Read(Buffer, Offset, Count)
        If c <> Count Then Throw New EndOfStreamException
    End Sub
    ''' <summary>已重载。读取到字节数组。</summary>
    Sub Read(ByVal Buffer() As Byte)
        Read(Buffer, 0, Buffer.Length)
    End Sub
    ''' <summary>已重载。读取字节数组。</summary>
    Function Read(ByVal Count As Integer) As Byte()
        Dim d As Byte() = New Byte(Count - 1) {}
        Read(d, 0, Count)
        Return d
    End Function
    ''' <summary>已重载。写入字节数组。</summary>
    ''' <param name="Offset">Buffer 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
    Overridable Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
        BaseStream.Write(Buffer, Offset, Count)
    End Sub
    ''' <summary>已重载。写入字节数组。</summary>
    Sub Write(ByVal Buffer As Byte())
        Write(Buffer, 0, Buffer.Length)
    End Sub

    ''' <summary>读取Int32数组。</summary>
    Function ReadInt32Array(ByVal Count As Integer) As Int32()
        Dim d As Int32() = New Int32(Count - 1) {}
        For n As Integer = 0 To Count - 1
            d(n) = ReadInt32()
        Next
        Return d
    End Function
    ''' <summary>写入Int32数组。</summary>
    Sub WriteInt32Array(ByVal Buffer As Int32())
        For Each i In Buffer
            WriteInt32(i)
        Next
    End Sub

    ''' <summary>读取到外部流。</summary>
    Sub ReadToStream(ByVal s As StreamEx, ByVal Count As Int64)
        If Count <= 0 Then Return
        Dim Buffer As Byte() = New Byte(CInt(Min(Count, 4 * (1 << 20)) - 1)) {}
        For n As Int64 = 0 To Count - Buffer.Length Step Buffer.Length
            Read(Buffer)
            s.Write(Buffer)
        Next
        Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
        Read(Buffer, 0, LeftLength)
        s.Write(Buffer, 0, LeftLength)
    End Sub
    ''' <summary>从外部流写入。</summary>
    Sub WriteFromStream(ByVal s As StreamEx, ByVal Count As Int64)
        If Count <= 0 Then Return
        Dim Buffer As Byte() = New Byte(CInt(Min(Count, 4 * (1 << 20)) - 1)) {}
        For n As Int64 = 0 To Count - Buffer.Length Step Buffer.Length
            s.Read(Buffer)
            Write(Buffer)
        Next
        Dim LeftLength As Int32 = CInt(Count Mod Buffer.Length)
        s.Read(Buffer, 0, LeftLength)
        Write(Buffer, 0, LeftLength)
    End Sub
    ''' <summary>保存到文件。</summary>
    Sub SaveAs(ByVal Path As String)
        Using s As New StreamEx(Path, FileMode.Create, FileAccess.ReadWrite)
            Dim Current As Int64 = Position
            Position = 0
            ReadToStream(s, Length)
            Position = Current
        End Using
    End Sub

#Region " IDisposable 支持 "
    ''' <summary>释放托管对象或间接非托管对象(Stream等)。可在这里将大型字段设置为 null。</summary>
    Protected Overridable Sub DisposeManagedResource()
        If BaseStream IsNot Nothing Then
            BaseStream.Dispose()
            BaseStream = Nothing
        End If
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

#Region " Stream 兼容支持 "
    ''' <summary>扩展流适配器类</summary>
    ''' <remarks>用于安全保存StreamEx的Stream形式。</remarks>
    Protected Class StreamAdapter
        Inherits Stream
        Protected Friend BaseStream As StreamEx

        Public Sub New(ByVal s As StreamEx)
            BaseStream = s
        End Sub
        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return BaseStream.CanRead
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return BaseStream.CanSeek
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return BaseStream.CanWrite
            End Get
        End Property
        Public Overrides Sub Flush()
            BaseStream.Flush()
        End Sub
        Public Overrides ReadOnly Property Length() As Int64
            Get
                Return BaseStream.Length
            End Get
        End Property
        Public Overrides Property Position() As Int64
            Get
                Return BaseStream.Position
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = Value
            End Set
        End Property
        Public Overrides Function Seek(ByVal Offset As Int64, ByVal Origin As System.IO.SeekOrigin) As Int64
            Return BaseStream.Seek(Offset, Origin)
        End Function
        Public Overrides Sub SetLength(ByVal Value As Int64)
            BaseStream.SetLength(Value)
        End Sub
        Public Overrides Function ReadByte() As Integer
            Return BaseStream.ReadByte()
        End Function
        Public Overrides Sub WriteByte(ByVal Value As Byte)
            BaseStream.WriteByte(Value)
        End Sub
        Public Overrides Function Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As Integer
            BaseStream.Read(Buffer, Offset, Count)
            Return Count
        End Function
        Public Overrides Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
            BaseStream.Write(Buffer, Offset, Count)
        End Sub
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

    ''' <summary>扩展流适配器类-适配非安全流</summary>
    ''' <remarks>用于安全保存StreamEx的Stream形式。</remarks>
    Protected Class UnsafeStreamAdapter
        Inherits Stream
        Protected Friend BaseStream As StreamEx

        Public Sub New(ByVal s As StreamEx)
            BaseStream = s
        End Sub
        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return BaseStream.CanRead
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return BaseStream.CanSeek
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return BaseStream.CanWrite
            End Get
        End Property
        Public Overrides Sub Flush()
            BaseStream.Flush()
        End Sub
        Public Overrides ReadOnly Property Length() As Int64
            Get
                Return BaseStream.Length
            End Get
        End Property
        Public Overrides Property Position() As Int64
            Get
                Return BaseStream.Position
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = Value
            End Set
        End Property
        Public Overrides Function Seek(ByVal Offset As Int64, ByVal Origin As System.IO.SeekOrigin) As Int64
            Return BaseStream.Seek(Offset, Origin)
        End Function
        Public Overrides Sub SetLength(ByVal Value As Int64)
            BaseStream.SetLength(Value)
        End Sub
        Public Overrides Function ReadByte() As Integer
            Try
                Return BaseStream.ReadByte()
            Catch ex As EndOfStreamException
                Return -1
            End Try
        End Function
        Public Overrides Sub WriteByte(ByVal Value As Byte)
            BaseStream.WriteByte(Value)
        End Sub
        Public Overrides Function Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As Integer
            If BaseStream.Position >= BaseStream.Length Then
                Return 0
            ElseIf BaseStream.Position + Count > BaseStream.Length Then
                Dim NewCount = CInt(BaseStream.Length - BaseStream.Position)
                BaseStream.Read(Buffer, Offset, NewCount)
                Return NewCount
            Else
                BaseStream.Read(Buffer, Offset, Count)
                Return Count
            End If
        End Function
        Public Overrides Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer)
            BaseStream.Write(Buffer, Offset, Count)
        End Sub
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If BaseStream IsNot Nothing Then
                BaseStream.Dispose()
                BaseStream = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class
#End Region

End Class
