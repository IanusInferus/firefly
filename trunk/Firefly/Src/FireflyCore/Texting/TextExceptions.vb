'==========================================================================
'
'  File:        TextExceptions.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 文本异常
'  Version:     2012.07.25.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace Texting
    Public Class FileLocationInformation
        Public Path As String = ""
        Public LineNumber As Integer = 0
        Public ColumnNumber As Integer = 0
    End Class

    Public Interface IFileLocationInformationProvider
        ReadOnly Property FileLocationInformation As FileLocationInformation
    End Interface

    Public Class InvalidTextFormatException
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal Message As String, ByVal i As FileLocationInformation)
            MyBase.New(GetMessage(Message, i))
            FileLocationInformationValue = i
        End Sub
        Public Sub New(ByVal Message As String, ByVal i As FileLocationInformation, ByVal InnerException As Exception)
            MyBase.New(GetMessage(Message, i), InnerException)
            FileLocationInformationValue = i
        End Sub

        Private FileLocationInformationValue As FileLocationInformation

        Public ReadOnly Property FileLocationInformation As FileLocationInformation
            Get
                Return FileLocationInformationValue
            End Get
        End Property

        Private Shared Function GetMessage(ByVal Message As String, ByVal i As FileLocationInformation) As String
            Dim l As New List(Of String)
            If i.Path <> "" Then l.Add(i.Path)
            If i.LineNumber <> 0 AndAlso i.ColumnNumber <> 0 Then
                l.Add("({0}, {1})".Formats(i.LineNumber, i.ColumnNumber))
            Else
                If i.LineNumber <> 0 Then l.Add("({0})".Formats(i.LineNumber))
                If i.ColumnNumber <> 0 Then l.Add("({0})".Formats(i.ColumnNumber))
            End If
            If Message <> "" Then
                If l.Count > 0 Then
                    l.Add(" : {0}".Formats(Message))
                Else
                    l.Add(Message)
                End If
            End If
            Return String.Join("", l.ToArray())
        End Function
    End Class

    Public Class InvalidTextFormatOrEncodingException
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal Message As String, ByVal i As FileLocationInformation)
            MyBase.New(GetMessage(Message, i))
            FileLocationInformationValue = i
        End Sub
        Public Sub New(ByVal Message As String, ByVal i As FileLocationInformation, ByVal InnerException As Exception)
            MyBase.New(GetMessage(Message, i), InnerException)
            FileLocationInformationValue = i
        End Sub

        Private FileLocationInformationValue As FileLocationInformation

        Public ReadOnly Property FileLocationInformation As FileLocationInformation
            Get
                Return FileLocationInformationValue
            End Get
        End Property

        Private Shared Function GetMessage(ByVal Message As String, ByVal i As FileLocationInformation) As String
            Dim l As New List(Of String)
            If i.Path <> "" Then l.Add(i.Path)
            If i.LineNumber <> 0 AndAlso i.ColumnNumber <> 0 Then
                l.Add("({0}, {1})".Formats(i.LineNumber, i.ColumnNumber))
            Else
                If i.LineNumber <> 0 Then l.Add("({0})".Formats(i.LineNumber))
                If i.ColumnNumber <> 0 Then l.Add("({0})".Formats(i.ColumnNumber))
            End If
            If Message <> "" Then
                If l.Count > 0 Then
                    l.Add(" : {0}".Formats(Message))
                Else
                    l.Add(Message)
                End If
            End If
            Return String.Join("", l.ToArray())
        End Function
    End Class
End Namespace
