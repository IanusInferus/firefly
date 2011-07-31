'==========================================================================
'
'  File:        Writer.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文本输出类
'  Version:     2011.07.31.
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
    Public Class TreeFormatWriter
        Private sw As StreamWriter
        Public Sub New(ByVal sw As StreamWriter)
            Me.sw = sw
        End Sub

        Private Shared ReadOnly Empty As String = "$Empty"
        Private Shared ReadOnly List As String = "$List"
        Private Shared ReadOnly StringDirective As String = "$String"
        Private Shared ReadOnly EndDirective As String = "$End"

        Public Sub Write(ByVal Forest As Forest)
            For Each n In Forest.Nodes
                WriteNode(0, n)
            Next
        End Sub

        Private Sub WriteNode(ByVal IndentLevel As Integer, ByVal Node As Node)
            Dim s = TryGetNodeAsSingleLineString(Node)
            If s.HasValue Then
                WriteRaw(IndentLevel, s.Value)
                Return
            End If

            Select Case Node._Tag
                Case NodeTag.Empty
                    Throw New InvalidOperationException
                Case NodeTag.Leaf
                    WriteValue(IndentLevel, Node.Leaf)
                Case NodeTag.Stem
                    Dim ns = Node.Stem
                    Dim Name = GetLiteral(ns.Name, True).SingleLine
                    WriteRaw(IndentLevel, Name)

                    If ns.Children.Length = 0 Then
                        WriteRaw(IndentLevel, EndDirective)
                    Else
                        If ns.Children.Length > 1 AndAlso ns.Children.All(Function(c) c.OnStem AndAlso c.Stem.Children.Length = 1) Then
                            Dim ChildNames = ns.Children.Select(Function(c) c.Stem.Name).Distinct.ToArray()
                            If ChildNames.Length = 1 Then
                                Dim ChildName = GetLiteral(ChildNames.Single(), True).SingleLine
                                WriteRaw(IndentLevel + 1, List & " " & ChildName)
                                For Each c In ns.Children
                                    WriteNode(IndentLevel + 2, c.Stem.Children.Single())
                                Next
                                Return
                            End If
                        End If

                        For Each c In ns.Children
                            WriteNode(IndentLevel + 1, c)
                        Next
                    End If
                Case Else
                    Throw New ArgumentException
            End Select
        End Sub

        Private Shared Function TryGetNodeAsSingleLineString(ByVal Node As Node) As Opt(Of String)
            Dim l As New List(Of String)

            Dim n = Node
            While True
                Select Case n._Tag
                    Case NodeTag.Empty
                        l.Add(Empty)
                        Exit While
                    Case NodeTag.Leaf
                        Dim s = GetLiteral(n.Leaf, False)
                        If s.OnMultiLine Then Return Opt(Of String).Empty
                        If Not s.OnSingleLine Then Throw New InvalidOperationException
                        l.Add(s.SingleLine)
                        Exit While
                    Case NodeTag.Stem
                        Dim ns = n.Stem
                        If ns.Children.Length <> 1 Then Return Opt(Of String).Empty
                        Dim s = GetLiteral(ns.Name, False)
                        If s.OnMultiLine Then Return Opt(Of String).Empty
                        If Not s.OnSingleLine Then Throw New InvalidOperationException
                        l.Add(s.SingleLine)
                        n = ns.Children.Single()
                    Case Else
                        Throw New InvalidOperationException
                End Select
            End While

            Return String.Join(" ", l.ToArray)
        End Function

        Private Sub WriteValue(ByVal IndentLevel As Integer, ByVal Value As String)
            Dim s = GetLiteral(Value, False)

            Select Case s._Tag
                Case LiteralTag.SingleLine
                    WriteRaw(IndentLevel, s.SingleLine)
                Case LiteralTag.MultiLine
                    WriteRaw(IndentLevel, StringDirective)
                    For Each Line In s.MultiLine
                        WriteRaw(IndentLevel + 1, Line)
                    Next
                    If s.MultiLine.Length > 0 Then
                        Dim LastLine = s.MultiLine.Last()
                        Dim Chars = LastLine.Distinct.ToArray()
                        If Chars.Length = 0 OrElse (Chars.Length = 1 AndAlso Chars.Single = " ") Then
                            WriteRaw(IndentLevel, EndDirective)
                        End If
                    End If
            End Select
        End Sub

        Private Sub WriteRaw(ByVal IndentLevel As Integer, ByVal Value As String)
            Dim si = New String(" "c, 4 * IndentLevel)
            sw.WriteLine(si & Value)
        End Sub


        Private Enum LiteralTag
            SingleLine
            MultiLine
        End Enum
        <TaggedUnion(), DebuggerDisplay("{ToString()}")>
        Private Class Literal
            <Tag()> Public Property _Tag As LiteralTag
            Public Property SingleLine As String
            Public Property MultiLine As String()

            Public Shared Function CreateSingleLine(ByVal Value As String) As Literal
                Return New Literal With {._Tag = LiteralTag.SingleLine, .SingleLine = Value}
            End Function
            Public Shared Function CreateMultiLine(ByVal Value As String()) As Literal
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

        Private Enum ParentheseType
            Angle
            Bracket
            Brace
        End Enum
        Private Shared rCrLf As New Regex("\r\n", RegexOptions.ExplicitCapture)
        Private Shared rWildCrOrLf As New Regex("\r(?!\n)|(?<!\r)\n", RegexOptions.ExplicitCapture)
        Private Shared rForbiddenChars As New Regex("[()\f\t\v]", RegexOptions.ExplicitCapture)
        Private Shared rForbiddenHeadChars As New Regex("^[!%&;=?\^`|~]", RegexOptions.ExplicitCapture)
        Private Shared Function GetLiteral(ByVal Value As String, ByVal MustSingleLine As Boolean) As Literal
            If Value Is Nothing Then Return Literal.CreateSingleLine(Empty)
            If Value = "" Then Return Literal.CreateSingleLine("""""")
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
            If cm Then Return Literal.CreateMultiLine(rCrLf.Split(Value))

            Dim CreateQuotationLiteral = Function() Literal.CreateSingleLine("""" & Value.Replace("""", """""") & """")

            If rForbiddenHeadChars.Match(Value).Success OrElse rForbiddenChars.Match(Value).Success Then
                Return CreateQuotationLiteral()
            End If

            Dim ParentheseStack As New Stack(Of ParentheseType)
            For Each c In Value
                Dim cs = New String(c, 1)
                Select Case cs
                    Case """", " "
                        Return CreateQuotationLiteral()
                    Case "<"
                        ParentheseStack.Push(ParentheseType.Angle)
                    Case "["
                        ParentheseStack.Push(ParentheseType.Bracket)
                    Case "{"
                        ParentheseStack.Push(ParentheseType.Brace)
                    Case ">"
                        If ParentheseStack.Count = 0 OrElse ParentheseStack.Peek <> ParentheseType.Angle Then
                            Return CreateQuotationLiteral()
                        End If
                        ParentheseStack.Pop()
                    Case "]"
                        If ParentheseStack.Count = 0 OrElse ParentheseStack.Peek <> ParentheseType.Bracket Then
                            Return CreateQuotationLiteral()
                        End If
                        ParentheseStack.Pop()
                    Case "}"
                        If ParentheseStack.Count = 0 OrElse ParentheseStack.Peek <> ParentheseType.Brace Then
                            Return CreateQuotationLiteral()
                        End If
                        ParentheseStack.Pop()
                    Case Else

                End Select
            Next
            If ParentheseStack.Count <> 0 Then Return CreateQuotationLiteral()

            Return Literal.CreateSingleLine(Value)
        End Function
    End Class
End Namespace
