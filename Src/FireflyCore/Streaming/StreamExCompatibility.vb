'==========================================================================
'
'  File:        StreamExCompatibility.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 扩展流类 - 兼容支持
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace Streaming
    Partial Class StreamEx
        ''' <summary>读取单精度浮点数。</summary>
        Public Function ReadSingle(ByVal This As IReadableStream) As Single
            Return This.ReadFloat32()
        End Function
        ''' <summary>读取双精度浮点数。</summary>
        Public Function ReadDouble(ByVal This As IReadableStream) As Double
            Return This.ReadFloat64()
        End Function
        ''' <summary>写入单精度浮点数。</summary>
        Public Sub WriteSingle(ByVal This As IWritableStream, ByVal f As Single)
            This.WriteFloat32(f)
        End Sub
        ''' <summary>写入双精度浮点数。</summary>
        Public Sub WriteDouble(ByVal This As IWritableStream, ByVal f As Double)
            This.WriteFloat64(f)
        End Sub

        ''' <summary>读取Int32数组。</summary>
        Public Function ReadInt32Array(ByVal Count As Integer) As Int32()
            Dim d As Int32() = New Int32(Count - 1) {}
            For n As Integer = 0 To Count - 1
                d(n) = ReadInt32()
            Next
            Return d
        End Function
        ''' <summary>写入Int32数组。</summary>
        Public Sub WriteInt32Array(ByVal Buffer As Int32())
            For Each i In Buffer
                WriteInt32(i)
            Next
        End Sub

        ''' <summary>保存到文件。</summary>
        Public Sub SaveAs(ByVal Path As String)
            Using s As New StreamEx(Path, FileMode.Create, FileAccess.ReadWrite)
                Dim Current As Int64 = Position
                Position = 0
                ReadToStream(s, Length)
                Position = Current
            End Using
        End Sub
    End Class
End Namespace
