'==========================================================================
'
'  File:        ImageInterface.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: 图片调用接口和实现
'  Version:     2010.10.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace Imaging
    Public Interface IImageReader
        Inherits IDisposable

        Sub Load()
        Function GetRectangleAsARGB(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As Int32(,)
    End Interface

    Public Interface IImageWriter
        Inherits IDisposable

        Sub Create(ByVal w As Integer, ByVal h As Integer)
        Sub SetRectangleFromARGB(ByVal x As Integer, ByVal y As Integer, ByVal a As Int32(,))
    End Interface
End Namespace
