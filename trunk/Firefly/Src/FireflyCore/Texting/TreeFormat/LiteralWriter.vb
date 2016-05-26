'==========================================================================
'
'  File:        LiteralWriter.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 字面量输出类
'  Version:     2016.05.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Diagnostics
Imports System.Text.RegularExpressions
Imports System.IO
Imports Firefly.Mapping.MetaSchema
Imports Firefly.TextEncoding
Imports Firefly.Texting.TreeFormat.Semantics

Namespace Texting.TreeFormat
    Public Enum LiteralTag
        SingleLine
        MultiLine
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class Literal
        <Tag()> Public _Tag As LiteralTag
        Public SingleLine As String
        Public MultiLine As List(Of String)

        Public Shared Function CreateSingleLine(ByVal Value As String) As Literal
            Return New Literal With {._Tag = LiteralTag.SingleLine, .SingleLine = Value}
        End Function
        Public Shared Function CreateMultiLine(ByVal Value As List(Of String)) As Literal
            Return New Literal With {._Tag = LiteralTag.MultiLine, .MultiLine = Value}
        End Function

        Public ReadOnly Property OnSingleLine() As Boolean
            Get
                Return _Tag = LiteralTag.SingleLine
            End Get
        End Property
        Public ReadOnly Property OnMultiLine() As Boolean
            Get
                Return _Tag = LiteralTag.MultiLine
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public NotInheritable Class TreeFormatLiteralWriter
        Private Sub New()
        End Sub

        Private Shared ReadOnly Empty As String = "$Empty"
        Private Enum ParenthesisType
            Angle
            Bracket
            Brace
        End Enum
        Private Shared rCrLf As New Regex("\r\n", RegexOptions.ExplicitCapture)
        Private Shared rWildCrOrLf As New Regex("\r(?!\n)|(?<!\r)\n", RegexOptions.ExplicitCapture)
        Private Shared rForbiddenChars As New Regex("[()\f\t\v]", RegexOptions.ExplicitCapture)
        Private Shared rForbiddenHeadChars As New Regex("^[!%&;=?\^`|~]", RegexOptions.ExplicitCapture)
        Public Shared Function GetLiteral(ByVal Value As String, ByVal MustSingleLine As Boolean, ByVal MustMultiLine As Boolean) As Literal
            If Value Is Nothing Then
                If MustMultiLine Then
                    Return Literal.CreateMultiLine(New List(Of String) From {})
                Else
                    Return Literal.CreateSingleLine(Empty)
                End If
            End If
            If Value = "" Then
                If MustMultiLine Then
                    Return Literal.CreateMultiLine(New List(Of String) From {""})
                Else
                    Return Literal.CreateSingleLine("""""")
                End If
            End If
            If MustMultiLine Then
                Return Literal.CreateMultiLine(rCrLf.Split(Value.UnifyNewLineToCrLf()).ToList())
            End If
            Dim wm = rWildCrOrLf.Match(Value).Success
            Dim cm = rCrLf.Match(Value).Success
            If wm OrElse (cm AndAlso MustSingleLine) Then
                Dim s = Value.Escape().Replace("""", "\""")
                If s.StartsWith(" ") Then
                    Return Literal.CreateSingleLine("""""\" & s & """""")
                Else
                    Return Literal.CreateSingleLine("""""" & s & """""")
                End If
            End If
            If cm Then Return Literal.CreateMultiLine(rCrLf.Split(Value).ToList())

            Dim CreateQuotationLiteral = Function() Literal.CreateSingleLine("""" & Value.Replace("""", """""") & """")

            If rForbiddenHeadChars.Match(Value).Success OrElse rForbiddenChars.Match(Value).Success Then
                Return CreateQuotationLiteral()
            End If

            Dim Stack As New Stack(Of ParenthesisType)
            For Each c In Value
                Dim cs = New String(c, 1)
                Select Case cs
                    Case """", " "
                        If Stack.Count = 0 Then
                            Return CreateQuotationLiteral()
                        End If
                    Case "<"
                        Stack.Push(ParenthesisType.Angle)
                    Case "["
                        Stack.Push(ParenthesisType.Bracket)
                    Case "{"
                        Stack.Push(ParenthesisType.Brace)
                    Case ">"
                        If Stack.Count = 0 OrElse Stack.Peek <> ParenthesisType.Angle Then
                            Return CreateQuotationLiteral()
                        End If
                        Stack.Pop()
                    Case "]"
                        If Stack.Count = 0 OrElse Stack.Peek <> ParenthesisType.Bracket Then
                            Return CreateQuotationLiteral()
                        End If
                        Stack.Pop()
                    Case "}"
                        If Stack.Count = 0 OrElse Stack.Peek <> ParenthesisType.Brace Then
                            Return CreateQuotationLiteral()
                        End If
                        Stack.Pop()
                    Case Else

                End Select
            Next
            If Stack.Count <> 0 Then Return CreateQuotationLiteral()

            Return Literal.CreateSingleLine(Value)
        End Function
    End Class
End Namespace
