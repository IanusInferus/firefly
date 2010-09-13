'==========================================================================
'
'  File:        Pointer.vb
'  Location:    Firefly.Compressing <Visual Basic .Net>
'  Description: 压缩匹配指针
'  Version:     2009.01.20.
'  Copyright(C) F.R.C.
'
'==========================================================================

Namespace Compressing
    ''' <summary>压缩匹配指针</summary>
    Public Interface Pointer
        ReadOnly Property Length() As Integer
    End Interface

    ''' <summary>字面量</summary>
    Public Class Literal
        Implements Pointer

        Public Sub New()
        End Sub

        ''' <summary>长度</summary>
        Public ReadOnly Property Length() As Integer Implements Pointer.Length
            Get
                Return 1
            End Get
        End Property
    End Class
End Namespace
