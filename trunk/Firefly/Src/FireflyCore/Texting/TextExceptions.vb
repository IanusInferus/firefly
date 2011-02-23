'==========================================================================
'
'  File:        TextExceptions.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 文本异常
'  Version:     2011.02.23.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace Texting
    Public Class InvalidTextFormatException
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal Path As String, ByVal Message As String)
            MyBase.New(GetMessage(Path, "", Message))
            PathValue = Path
        End Sub

        Public Sub New(ByVal LineNumber As Integer, ByVal Message As String)
            MyBase.New(GetMessage("", CStr(LineNumber), Message))
            LineNumberValue = LineNumber
        End Sub

        Public Sub New(ByVal Path As String, ByVal LineNumber As Integer, ByVal Message As String)
            MyBase.New(GetMessage(Path, CStr(LineNumber), Message))
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

        Private Shared Function GetMessage(ByVal Path As String, ByVal LineNumber As String, ByVal Message As String) As String
            Dim l As New List(Of String)
            If Path <> "" Then l.Add(Path)
            If LineNumber <> "" Then l.Add("({0})".Formats(LineNumber))
            If Message <> "" Then l.Add(" : {0}".Formats(Message))
            Return String.Join("", l.ToArray())
        End Function
    End Class

    Public Class InvalidTextFormatOrEncodingException
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal Path As String, ByVal Message As String)
            MyBase.New(GetMessage(Path, "", Message))
            PathValue = Path
        End Sub

        Public Sub New(ByVal LineNumber As Integer, ByVal Message As String)
            MyBase.New(GetMessage("", CStr(LineNumber), Message))
            LineNumberValue = LineNumber
        End Sub

        Public Sub New(ByVal Path As String, ByVal LineNumber As Integer, ByVal Message As String)
            MyBase.New(GetMessage(Path, CStr(LineNumber), Message))
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

        Private Shared Function GetMessage(ByVal Path As String, ByVal LineNumber As String, ByVal Message As String) As String
            Dim l As New List(Of String)
            If Path <> "" Then l.Add(Path)
            If LineNumber <> "" Then l.Add("({0})".Formats(LineNumber))
            If Message <> "" Then l.Add(" : {0}".Formats(Message))
            Return String.Join("", l.ToArray())
        End Function
    End Class
End Namespace
