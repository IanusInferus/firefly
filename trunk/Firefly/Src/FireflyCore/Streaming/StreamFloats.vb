'==========================================================================
'
'  File:        StreamFloats.vb
'  Location:    Firefly.Streaming <Visual Basic .Net>
'  Description: 流 - 浮点数
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Runtime.CompilerServices

Namespace Streaming
    Public Module ReadableStreamFloats
        <System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)>
        Private Structure SingleInt32
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Float32Value As Single
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Int32Value As Int32
        End Structure
        <System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)>
        Private Structure DoubleInt64
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Float64Value As Double
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Int64Value As Int64
        End Structure

        ''' <summary>读取单精度浮点数。</summary>
        <Extension()> Public Function ReadFloat32(ByVal This As IReadableStream) As Single
            Dim a As SingleInt32
            a.Int32Value = This.ReadInt32()
            Return a.Float32Value
        End Function
        ''' <summary>读取双精度浮点数。</summary>
        <Extension()> Public Function ReadFloat64(ByVal This As IReadableStream) As Double
            Dim a As DoubleInt64
            a.Int64Value = This.ReadInt64()
            Return a.Float64Value
        End Function
        ''' <summary>读取单精度浮点数，高位优先字节序。</summary>
        <Extension()> Public Function ReadFloat32B(ByVal This As IReadableStream) As Single
            Dim a As SingleInt32
            a.Int32Value = This.ReadInt32B()
            Return a.Float32Value
        End Function
        ''' <summary>读取双精度浮点数，高位优先字节序。</summary>
        <Extension()> Public Function ReadFloat64B(ByVal This As IReadableStream) As Double
            Dim a As DoubleInt64
            a.Int64Value = This.ReadInt64B()
            Return a.Float64Value
        End Function
    End Module

    Public Module WritableStreamFloats
        <System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)>
        Private Structure SingleInt32
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Float32Value As Single
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Int32Value As Int32
        End Structure
        <System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)>
        Private Structure DoubleInt64
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Float64Value As Double
            <System.Runtime.InteropServices.FieldOffset(0)>
            Public Int64Value As Int64
        End Structure

        ''' <summary>写入单精度浮点数。</summary>
        <Extension()> Public Sub WriteFloat32(ByVal This As IWritableStream, ByVal f As Single)
            Dim a As SingleInt32
            a.Float32Value = f
            This.WriteInt32(a.Int32Value)
        End Sub
        ''' <summary>写入双精度浮点数。</summary>
        <Extension()> Public Sub WriteFloat64(ByVal This As IWritableStream, ByVal f As Double)
            Dim a As DoubleInt64
            a.Float64Value = f
            This.WriteInt64(a.Int64Value)
        End Sub
        ''' <summary>写入单精度浮点数，高位优先字节序。</summary>
        <Extension()> Public Sub WriteFloat32B(ByVal This As IWritableStream, ByVal f As Single)
            Dim a As SingleInt32
            a.Float32Value = f
            This.WriteInt32B(a.Int32Value)
        End Sub
        ''' <summary>写入双精度浮点数，高位优先字节序。</summary>
        <Extension()> Public Sub WriteFloat64B(ByVal This As IWritableStream, ByVal f As Double)
            Dim a As DoubleInt64
            a.Float64Value = f
            This.WriteInt64B(a.Int64Value)
        End Sub
    End Module
End Namespace
