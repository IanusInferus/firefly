'==========================================================================
'
'  File:        StreamPasser.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流传递器，用于显式确定函数传参时的流是否包含长度位置信息。
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Runtime.CompilerServices
Imports System.IO

Namespace Streaming
    Public Module StreamPasser
        <Extension()> Public Function PassAsZeroLength(ByVal This As IStream) As ZeroLengthStreamPasser
            Return New ZeroLengthStreamPasser(This)
        End Function
        <Extension()> Public Function PassAsZeroPosition(ByVal This As IStream) As ZeroPositionStreamPasser
            Return New ZeroPositionStreamPasser(This)
        End Function
        <Extension()> Public Function PassAsPositioned(ByVal This As IStream) As PositionedStreamPasser
            Return New PositionedStreamPasser(This)
        End Function

        <Extension()> Public Function ToIStream(ByVal This As Stream) As IStream
            Return New StreamEx(This)
        End Function
        <Extension()> Public Function ToStream(ByVal This As IBasicStream) As Stream
            Return New StreamAdapter(This)
        End Function
        <Extension()> Public Function ToUnsafeStream(ByVal This As IBasicStream) As Stream
            Return New UnsafeStreamAdapter(This)
        End Function
    End Module

    ''' <summary>零长度零位置扩展流传递器。用于保证在函数传参时传递零长度零位置的流。</summary>
    Public Class ZeroLengthStreamPasser
        Private BaseStream As IStream

        Public Sub New(ByVal s As IStream)
            If s.Length <> 0 Then Throw New ArgumentException("LengthNotZero")
            If s.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            BaseStream = s
        End Sub

        Public Function GetStream() As IStream
            If BaseStream.Length <> 0 Then Throw New ArgumentException("LengthNotZero")
            If BaseStream.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            Return BaseStream
        End Function
    End Class

    ''' <summary>零位置扩展流传递器。用于保证在函数传参时传递零位置的流。</summary>
    Public Class ZeroPositionStreamPasser
        Private BaseStream As IStream

        Public Sub New(ByVal s As IStream)
            If s.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            BaseStream = s
        End Sub

        Public Function GetStream() As IStream
            If BaseStream.Position <> 0 Then Throw New ArgumentException("PositionNotZero")
            Return BaseStream
        End Function

        Public Shared Widening Operator CType(ByVal p As ZeroLengthStreamPasser) As ZeroPositionStreamPasser
            Return New ZeroPositionStreamPasser(p.GetStream())
        End Operator
    End Class

    ''' <summary>有位置的扩展流传递器。用于显式申明函数传参时传递的流有位置信息。</summary>
    Public Class PositionedStreamPasser
        Private BaseStream As IStream

        Public Sub New(ByVal s As IStream)
            BaseStream = s
        End Sub

        Public Function GetStream() As IStream
            Return BaseStream
        End Function
    End Class
End Namespace
