'==========================================================================
'
'  File:        Exceptions.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 异常定义
'  Version:     2016.05.23.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic

Namespace Texting.TreeFormat.Syntax
    Public Class FileTextRange
        Public Text As Text
        Public Range As [Optional](Of Syntax.TextRange)
    End Class

    Public Class InvalidSyntaxException
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal Message As String)
            MyBase.New(Message)
        End Sub
        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
            MyBase.New(Message, InnerException)
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange))
            MyBase.New(GetMessage(Message, Range))
            RangeValue = Range
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal InnerException As Exception)
            MyBase.New(GetMessage(Message, Range), InnerException)
            RangeValue = Range
        End Sub

        Private RangeValue As [Optional](Of FileTextRange)

        Public ReadOnly Property Range As [Optional](Of FileTextRange)
            Get
                Return RangeValue
            End Get
        End Property

        Protected Shared Function GetMessage(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange)) As String
            Dim l As New List(Of String)
            If Range.OnHasValue Then
                Dim RangeValue = Range.Value
                If RangeValue.Text IsNot Nothing AndAlso RangeValue.Text.Path <> "" Then l.Add(RangeValue.Text.Path)
                If RangeValue.Range.OnHasValue Then
                    l.Add(RangeValue.Range.Value.ToString())
                End If
                If Message <> "" Then
                    If l.Count > 0 Then
                        l.Add(" : {0}".Formats(Message))
                    Else
                        l.Add(Message)
                    End If
                End If
            End If
            Return String.Join("", l.ToArray())
        End Function
    End Class

    Public Class InvalidTokenException
        Inherits InvalidSyntaxException

        Public Sub New()
        End Sub

        Public Sub New(ByVal Message As String)
            MyBase.New(Message)
        End Sub
        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
            MyBase.New(Message, InnerException)
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal Token As String)
            MyBase.New(GetTokenMessage(Message, Range, Token))
            TokenValue = Token
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal Token As String, ByVal InnerException As Exception)
            MyBase.New(GetTokenMessage(Message, Range, Token), InnerException)
            TokenValue = Token
        End Sub

        Private TokenValue As String

        Public ReadOnly Property Token As String
            Get
                Return TokenValue
            End Get
        End Property

        Private Shared Function GetTokenMessage(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal Token As String) As String
            If Message = "" Then
                Return GetMessage("'{0}' : InvalidToken".Formats(Token), Range)
            End If
            Return GetMessage("'{0}' : {1}".Formats(Token, Message), Range)
        End Function
    End Class

    Public Class InvalidSyntaxRuleException
        Inherits InvalidSyntaxException

        Public Sub New()
        End Sub

        Public Sub New(ByVal Message As String)
            MyBase.New(Message)
        End Sub
        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
            MyBase.New(Message, InnerException)
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal Token As Token)
            MyBase.New(GetTokenMessage(Message, Range, Token))
            TokenValue = Token
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal Token As Token, ByVal InnerException As Exception)
            MyBase.New(GetTokenMessage(Message, Range, Token), InnerException)
            TokenValue = Token
        End Sub

        Private TokenValue As Token

        Public ReadOnly Property Token As Token
            Get
                Return TokenValue
            End Get
        End Property

        Private Shared Function GetTokenMessage(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal Token As Token) As String
            If Message = "" Then
                Return GetMessage("'{0}' : InvalidSyntaxAtToken".Formats(Token.ToString()), Range)
            End If
            Return GetMessage("'{0}' : {1}".Formats(Token.ToString(), Message), Range)
        End Function
    End Class

    Public Class InvalidEvaluationException
        Inherits InvalidSyntaxException

        Public Sub New()
        End Sub

        Public Sub New(ByVal Message As String)
            MyBase.New(Message)
        End Sub
        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
            MyBase.New(Message, InnerException)
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal SyntaxRule As Object)
            MyBase.New(GetTokenMessage(Message, Range))
            SyntaxRuleValue = SyntaxRule
        End Sub
        Public Sub New(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange), ByVal SyntaxRule As Object, ByVal InnerException As Exception)
            MyBase.New(GetTokenMessage(Message, Range), InnerException)
            SyntaxRuleValue = SyntaxRule
        End Sub

        Private SyntaxRuleValue As Object

        Public ReadOnly Property SyntaxRule As Object
            Get
                Return SyntaxRuleValue
            End Get
        End Property

        Private Shared Function GetTokenMessage(ByVal Message As String, ByVal Range As [Optional](Of FileTextRange)) As String
            If Message = "" Then
                Return GetMessage("InvalidSyntaxRule", Range)
            End If
            Return GetMessage(Message, Range)
        End Function
    End Class
End Namespace
