'==========================================================================
'
'  File:        TextExceptions.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 文本异常
'  Version:     2010.10.04.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO

Public Class InvalidTextFormatException
    Inherits Exception

    Public Sub New()
    End Sub

    Public Sub New(ByVal Path As String, ByVal Message As String)
        MyBase.New(Message)
        PathValue = Path
    End Sub

    Public Sub New(ByVal LineNumber As Integer, ByVal Message As String)
        MyBase.New(Message)
        LineNumberValue = LineNumber
    End Sub

    Public Sub New(ByVal Path As String, ByVal LineNumber As Integer, ByVal Message As String)
        MyBase.New(Message)
        PathValue = Path
        LineNumberValue = LineNumber
    End Sub

    Public Sub New(ByVal Path As String)
        Me.New(Path, "")
    End Sub

    Public Sub New(ByVal LineNumber As Integer)
        Me.New(LineNumber, "")
    End Sub

    Public Sub New(ByVal Path As String, ByVal LineNumber As Integer)
        Me.New(Path, LineNumber, "")
    End Sub

    Private PathValue As String
    Private LineNumberValue As Integer

    Public ReadOnly Property Path As String
        Get
            Return PathValue
        End Get
    End Property

    Public ReadOnly Property LineNumber As Integer
        Get
            Return LineNumberValue
        End Get
    End Property

    Public Overrides Function ToString() As String
        If Message = "" Then
            Return String.Format("{0}({1})", PathValue, LineNumberValue)
        Else
            Return String.Format("{0}({1}) : {2}", PathValue, LineNumberValue, Message)
        End If
    End Function
End Class

Public Class InvalidTextFormatOrEncodingException
    Inherits Exception

    Public Sub New()
    End Sub

    Public Sub New(ByVal Path As String, ByVal Message As String)
        MyBase.New(Message)
        PathValue = Path
    End Sub

    Public Sub New(ByVal LineNumber As Integer, ByVal Message As String)
        MyBase.New(Message)
        LineNumberValue = LineNumber
    End Sub

    Public Sub New(ByVal Path As String, ByVal LineNumber As Integer, ByVal Message As String)
        MyBase.New(Message)
        PathValue = Path
        LineNumberValue = LineNumber
    End Sub

    Public Sub New(ByVal Path As String)
        Me.New(Path, "")
    End Sub

    Public Sub New(ByVal LineNumber As Integer)
        Me.New(LineNumber, "")
    End Sub

    Public Sub New(ByVal Path As String, ByVal LineNumber As Integer)
        Me.New(Path, LineNumber, "")
    End Sub

    Private PathValue As String
    Private LineNumberValue As Integer

    Public ReadOnly Property Path As String
        Get
            Return PathValue
        End Get
    End Property

    Public ReadOnly Property LineNumber As Integer
        Get
            Return LineNumberValue
        End Get
    End Property

    Public Overrides Function ToString() As String
        If Message = "" Then
            Return String.Format("{0}({1})", PathValue, LineNumberValue)
        Else
            Return String.Format("{0}({1}) : {2}", PathValue, LineNumberValue, Message)
        End If
    End Function
End Class
