''==========================================================================
''
''  File:        PartialStreams.vb
''  Location:    Firefly.Streaming <Visual Basic .Net>
''  Description: 局部流
''  Version:     2010.12.01.
''  Copyright(C) F.R.C.
''
''==========================================================================

Option Strict On
Imports System
Imports System.Runtime.CompilerServices
Imports System.IO

Namespace Streaming
    ''' <summary>
    ''' 局部扩展流类，用于表示一个流上的固定开始位置和长度的流，可以递归表示
    ''' </summary>
    Public Module PartialStreams
        ''' <param name="BaseLength">文件的大小</param>
        <Extension()> Public Function Partialize(ByVal This As IReadableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, Optional ByVal BaseStreamClose As Boolean = False) As IReadableSeekableStream
            Return New PartialReadableSeekableStream(This, BasePosition, BaseLength, BaseStreamClose)
        End Function
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        <Extension()> Public Function Partialize(ByVal This As IWritableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, Optional ByVal BaseStreamClose As Boolean = False) As IWritableSeekableStream
            Return New PartialWritableSeekableStream(This, BasePosition, BaseLength, BaseLength, BaseStreamClose)
        End Function
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        <Extension()> Public Function Partialize(ByVal This As IWritableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False) As IWritableSeekableStream
            Return New PartialWritableSeekableStream(This, BasePosition, BaseLength, Length, BaseStreamClose)
        End Function
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        <Extension()> Public Function Partialize(ByVal This As IReadableWritableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, Optional ByVal BaseStreamClose As Boolean = False) As IReadableWritableSeekableStream
            Return New PartialReadableWritableSeekableStream(This, BasePosition, BaseLength, BaseLength, BaseStreamClose)
        End Function
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        <Extension()> Public Function Partialize(ByVal This As IReadableWritableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False) As IReadableWritableSeekableStream
            Return New PartialReadableWritableSeekableStream(This, BasePosition, BaseLength, Length, BaseStreamClose)
        End Function
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        <Extension()> Public Function Partialize(ByVal This As IStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, Optional ByVal BaseStreamClose As Boolean = False) As IStream
            Return New PartialStream(This, BasePosition, BaseLength, BaseLength, BaseStreamClose)
        End Function
        ''' <param name="BaseLength">文件的最大大小</param>
        ''' <remarks>BaseLength不能小于Length。</remarks>
        <Extension()> Public Function Partialize(ByVal This As IStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False) As IStream
            Return New PartialStream(This, BasePosition, BaseLength, Length, BaseStreamClose)
        End Function
    End Module

    Friend Class PartialReadableSeekableStream
        Implements IReadableSeekableStream
        Private BaseStream As IReadableSeekableStream

        Private BasePosition As Int64
        Private BaseLength As Int64
        Private BaseStreamClose As Boolean
        Private LengthValue As Int64

        Public Sub New(ByVal s As IReadableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            BaseStream = s

            Me.BasePosition = BasePosition
            Me.BaseLength = BaseLength
            LengthValue = BaseLength
            BaseStream.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            Return BaseStream.ReadByte
        End Function

        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return LengthValue
            End Get
        End Property
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position - BasePosition
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = BasePosition + Value
            End Set
        End Property

        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            If Position + Count > Length Then Throw New EndOfStreamException
            BaseStream.Read(Buffer, Offset, Count)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                If BaseStreamClose Then
                    BaseStream.Dispose()
                Else
                    BaseStream.Flush()
                End If
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend Class PartialWritableSeekableStream
        Implements IWritableSeekableStream
        Private BaseStream As IWritableSeekableStream

        Private BasePosition As Int64
        Private BaseLength As Int64
        Private BaseStreamClose As Boolean
        Private LengthValue As Int64

        Public Sub New(ByVal s As IWritableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            BaseStream = s

            Me.BasePosition = BasePosition
            If BaseLength < Length Then Throw New ArgumentOutOfRangeException
            Me.BaseLength = BaseLength
            LengthValue = Length
            BaseStream.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            BaseStream.WriteByte(b)
        End Sub
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return LengthValue
            End Get
        End Property
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position - BasePosition
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = BasePosition + Value
            End Set
        End Property

        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            If Position + Count > BaseLength Then Throw New EndOfStreamException
            BaseStream.Write(Buffer, Offset, Count)
            If Position > Length Then LengthValue = Position
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                If BaseStreamClose Then
                    BaseStream.Dispose()
                Else
                    BaseStream.Flush()
                End If
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend Class PartialReadableWritableSeekableStream
        Implements IReadableWritableSeekableStream
        Private BaseStream As IReadableWritableSeekableStream

        Private BasePosition As Int64
        Private BaseLength As Int64
        Private BaseStreamClose As Boolean
        Private LengthValue As Int64

        Public Sub New(ByVal s As IReadableWritableSeekableStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            BaseStream = s

            Me.BasePosition = BasePosition
            If BaseLength < Length Then Throw New ArgumentOutOfRangeException
            Me.BaseLength = BaseLength
            LengthValue = Length
            BaseStream.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            Return BaseStream.ReadByte
        End Function
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            BaseStream.WriteByte(b)
        End Sub
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return LengthValue
            End Get
        End Property
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position - BasePosition
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = BasePosition + Value
            End Set
        End Property

        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            If Position + Count > Length Then Throw New EndOfStreamException
            BaseStream.Read(Buffer, Offset, Count)
        End Sub
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            If Position + Count > BaseLength Then Throw New EndOfStreamException
            BaseStream.Write(Buffer, Offset, Count)
            If Position > Length Then LengthValue = Position
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                If BaseStreamClose Then
                    BaseStream.Dispose()
                Else
                    BaseStream.Flush()
                End If
                BaseStream = Nothing
            End If
        End Sub
    End Class

    Friend Class PartialStream
        Implements IStream
        Private BaseStream As IStream

        Private BasePosition As Int64
        Private BaseLength As Int64
        Private BaseStreamClose As Boolean
        Private LengthValue As Int64

        Public Sub New(ByVal s As IStream, ByVal BasePosition As Int64, ByVal BaseLength As Int64, ByVal Length As Int64, Optional ByVal BaseStreamClose As Boolean = False)
            BaseStream = s

            Me.BasePosition = BasePosition
            If BaseLength < Length Then Throw New ArgumentOutOfRangeException
            Me.BaseLength = BaseLength
            LengthValue = Length
            BaseStream.Position = BasePosition
            Me.BaseStreamClose = BaseStreamClose
        End Sub

        Public Function ReadByte() As Byte Implements IReadableStream.ReadByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            Return BaseStream.ReadByte
        End Function
        Public Sub WriteByte(ByVal b As Byte) Implements IWritableStream.WriteByte
            If Position >= BaseLength Then Throw New EndOfStreamException
            BaseStream.WriteByte(b)
        End Sub
        Public Sub Flush() Implements IFlushable.Flush
            BaseStream.Flush()
        End Sub
        Public ReadOnly Property Length() As Int64 Implements ISeekableStream.Length
            Get
                Return LengthValue
            End Get
        End Property
        Public Property Position() As Int64 Implements ISeekableStream.Position
            Get
                Return BaseStream.Position - BasePosition
            End Get
            Set(ByVal Value As Int64)
                BaseStream.Position = BasePosition + Value
            End Set
        End Property
        Public Sub SetLength(ByVal Value As Int64) Implements IResizableStream.SetLength
            If Value < 0 Then Throw New ArgumentOutOfRangeException
            If Value > BaseLength Then Throw New ArgumentOutOfRangeException
            If BasePosition + Value > BaseStream.Length Then BaseStream.SetLength(BasePosition + Value)
            LengthValue = Value
        End Sub

        Public Sub Read(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IReadableStream.Read
            If Position + Count > Length Then Throw New EndOfStreamException
            BaseStream.Read(Buffer, Offset, Count)
        End Sub
        Public Sub Write(ByVal Buffer As Byte(), ByVal Offset As Integer, ByVal Count As Integer) Implements IWritableStream.Write
            If Position + Count > BaseLength Then Throw New EndOfStreamException
            BaseStream.Write(Buffer, Offset, Count)
            If Position > Length Then LengthValue = Position
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseStream IsNot Nothing Then
                If BaseStreamClose Then
                    BaseStream.Dispose()
                Else
                    BaseStream.Flush()
                End If
                BaseStream = Nothing
            End If
        End Sub
    End Class
End Namespace
