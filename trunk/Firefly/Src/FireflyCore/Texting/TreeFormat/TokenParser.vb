'==========================================================================
'
'  File:        TokenParser.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 词法解析器
'  Version:     2016.05.20.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Firefly.TextEncoding
Imports Firefly.Texting.TreeFormat.Syntax

Namespace Texting.TreeFormat
    Public Class TreeFormatTokenParseResult
        Public Token As [Optional](Of Token)
        Public RemainingChars As [Optional](Of TextRange)
    End Class

    Public Class TreeFormatTokenParser
        Private TextValue As Text
        Public Property Text As Text
            Get
                Return TextValue
            End Get
            Private Set(ByVal Value As Text)
                TextValue = Value
            End Set
        End Property
        Private Positions As New Dictionary(Of Object, TextRange)
        Public Sub New(ByVal Text As Text, ByVal Positions As Dictionary(Of Object, TextRange))
            Me.Text = Text
            Me.Positions = Positions
        End Sub

        Public Function IsBlankLine(ByVal Line As TextLine) As Boolean
            Dim t = Line.Text
            Return t.All(Function(c) c = " "c)
        End Function
        Public Function IsExactFitIndentLevel(ByVal Line As TextLine, ByVal IndentLevel As Integer) As Boolean
            Dim t = Line.Text
            Dim IndentCount = IndentLevel * 4
            If t.Length < IndentCount Then Return False
            If Not t.Take(IndentCount).All(Function(c) c = " "c) Then Return False
            If t.Length = IndentCount Then Return True
            If t(IndentCount) = " "c Then Return False
            Return True
        End Function
        Public Function IsFitIndentLevel(ByVal Line As TextLine, ByVal IndentLevel As Integer) As Boolean
            Dim t = Line.Text
            Dim IndentCount = IndentLevel * 4
            If t.Length < IndentCount Then Return False
            If Not t.Take(IndentCount).All(Function(c) c = " "c) Then Return False
            Return True
        End Function

        Private Enum ParenthesisType
            Angle
            Bracket
            Brace
        End Enum
        Private Enum TokenType
            SingleLineLiteral
            PreprocessDirective
            FunctionDirective
        End Enum
        Private Shared ForbiddenWhitespaces As Dictionary(Of Char, Integer) = "\f\t\v".Descape().ToCharArray().ToDictionary(Function(c) c, Function(c) 0)
        Private Function IsForbiddenWhitespaces(ByVal c As Char) As Boolean
            Return ForbiddenWhitespaces.ContainsKey(c)
        End Function
        Private Shared ForbiddenHeadChars As Dictionary(Of Char, Integer) = "!%&;=?^`|~".ToCharArray().ToDictionary(Function(c) c, Function(c) 0)
        Private Function IsForbiddenHeadChars(ByVal c As Char) As Boolean
            Return ForbiddenHeadChars.ContainsKey(c)
        End Function
        Private Shared HexChars As Dictionary(Of Char, Integer) = "0123456789ABCDEFabcdef".ToCharArray().ToDictionary(Function(c) c, Function(c) 0)
        Private Function IsHex(ByVal s As String, ByVal n As Integer) As Boolean
            Return s.Length = n AndAlso s.ToCharArray().All(Function(c) HexChars.ContainsKey(c))
        End Function
        Public Function ReadToken(ByVal RangeInLine As TextRange) As TreeFormatTokenParseResult
            Dim s = Text.GetTextInLine(RangeInLine)
            Dim Index As Integer = 0

            Dim EndOfLine = Function() Index >= s.Length
            Dim Peek1 = Function() s(Index)
            Dim Peek = Function(n As Integer) s.Substring(Index, Math.Min(n, s.Length - Index))
            Dim Proceed = Sub() Index += 1
            Dim ProceedMultiple = Sub(n As Integer) Index += n

            Dim MakeRemainingChars = Function() CType(New TextRange With {.Start = Text.Calc(RangeInLine.Start, Index), .End = RangeInLine.End}, [Optional](Of TextRange))
            Dim MakeTokenRange = Function(TokenStart As Integer, TokenEnd As Integer) New TextRange With {.Start = Text.Calc(RangeInLine.Start, TokenStart), .End = Text.Calc(RangeInLine.Start, TokenEnd)}
            Dim MakeNextErrorTokenRange = Function(n As Integer) New FileTextRange With {.Text = Text, .Range = MakeTokenRange(Index, Index + n)}

            Dim State = 0
            Dim Stack As New Stack(Of ParenthesisType)

            Dim StartIndex As Integer = 0
            Dim MakeCurrentErrorTokenException = Function(Message As String) New InvalidTokenException(Message, New FileTextRange With {.Text = Text, .Range = MakeTokenRange(StartIndex, Index)}, Text.GetTextInLine(MakeTokenRange(StartIndex, Index)))
            Dim MakeNextCharErrorTokenException = Function(Message As String) New InvalidTokenException(Message, MakeNextErrorTokenRange(1), Peek(1))
            Dim MarkStart = Sub() StartIndex = Index
            Dim Output As New List(Of Char)
            Dim MakeNullResult = Function() New TreeFormatTokenParseResult With {.Token = [Optional](Of Token).Empty, .RemainingChars = [Optional](Of TextRange).Empty}
            Dim MakeResult =
                Function(t As Token) As TreeFormatTokenParseResult
                    Dim Range = MakeTokenRange(StartIndex, Index)
                    Positions.Add(t, Range)
                    Dim RemainingChars = If(EndOfLine(), [Optional](Of TextRange).Empty, MakeRemainingChars())
                    Return New TreeFormatTokenParseResult With {.Token = t, .RemainingChars = RemainingChars}
                End Function
            Dim MakeResultChecked =
                Function() As TreeFormatTokenParseResult
                    Dim Range = MakeTokenRange(StartIndex, Index)
                    Dim OriginalText = Text.GetTextInLine(Range)
                    Dim t As Token
                    If OriginalText.StartsWith("$"c) Then
                        t = Token.CreatePreprocessDirective(OriginalText.Substring(1))
                    ElseIf OriginalText.StartsWith("#"c) Then
                        t = Token.CreateFunctionDirective(OriginalText.Substring(1))
                    Else
                        t = Token.CreateSingleLineLiteral(OriginalText)
                    End If
                    Positions.Add(t, Range)
                    Dim RemainingChars = If(EndOfLine(), [Optional](Of TextRange).Empty, MakeRemainingChars())
                    Return New TreeFormatTokenParseResult With {.Token = t, .RemainingChars = RemainingChars}
                End Function

            While True
                Select Case State
                    Case 0
                        If EndOfLine() Then Return MakeNullResult()
                        Dim c = Peek1()
                        If IsForbiddenWhitespaces(c) Then Throw MakeNextCharErrorTokenException("InvalidChar")
                        If IsForbiddenHeadChars(c) Then Throw MakeNextCharErrorTokenException("InvalidHeadChar")
                        Select Case c
                            Case " "c
                                Proceed()
                            Case """"c
                                MarkStart()
                                State = 2
                                Proceed()
                            Case "("c
                                MarkStart()
                                Proceed()
                                Return MakeResult(Token.CreateLeftParenthesis())
                            Case ")"c
                                MarkStart()
                                Proceed()
                                Return MakeResult(Token.CreateRightParenthesis())
                            Case "/"c
                                If Peek(2) = "//" Then
                                    MarkStart()
                                    Proceed()
                                    Proceed()
                                    While Not EndOfLine()
                                        Output.Add(Peek1())
                                        Proceed()
                                    End While
                                    Return MakeResult(Token.CreateSingleLineComment(New String(Output.ToArray())))
                                End If
                                Throw MakeNextCharErrorTokenException("InvalidChar")
                            Case "<"c
                                MarkStart()
                                Stack.Push(ParenthesisType.Angle)
                                Output.Add(c)
                                State = 1
                                Proceed()
                            Case "["c
                                MarkStart()
                                Stack.Push(ParenthesisType.Bracket)
                                State = 1
                                Proceed()
                            Case "{"c
                                MarkStart()
                                Stack.Push(ParenthesisType.Brace)
                                State = 1
                                Proceed()
                            Case Else
                                MarkStart()
                                State = 1
                                Proceed()
                        End Select
                    Case 1
                        If EndOfLine() Then
                            If Stack.Count <> 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                            Return MakeResultChecked()
                        End If
                        Dim c = Peek1()
                        If IsForbiddenWhitespaces(c) Then Throw MakeNextCharErrorTokenException("InvalidChar")
                        Select Case c
                            Case " "c
                                If Stack.Count = 0 Then Return MakeResultChecked()
                                Proceed()
                            Case """"c
                                If Stack.Count = 0 Then Throw MakeNextCharErrorTokenException("InvalidChar")
                                Proceed()
                            Case "("c, ")"c
                                If Stack.Count <> 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Return MakeResultChecked()
                            Case "<"c
                                Stack.Push(ParenthesisType.Angle)
                                Proceed()
                            Case "["c
                                Stack.Push(ParenthesisType.Bracket)
                                Proceed()
                            Case "{"c
                                Stack.Push(ParenthesisType.Brace)
                                Proceed()
                            Case ">"c
                                Proceed()
                                If Stack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                If Stack.Peek <> ParenthesisType.Angle Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Stack.Pop()
                            Case "]"c
                                Proceed()
                                If Stack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                If Stack.Peek <> ParenthesisType.Bracket Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Stack.Pop()
                            Case "}"c
                                Proceed()
                                If Stack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                If Stack.Peek <> ParenthesisType.Brace Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Stack.Pop()
                            Case Else
                                Proceed()
                        End Select
                    Case 2
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMark")
                        Dim c = Peek1()
                        If c = """"c Then
                            State = 21
                            Proceed()
                        Else
                            Output.Add(c)
                            State = 22
                            Proceed()
                        End If
                    Case 21
                        If EndOfLine() Then Return MakeResult(Token.CreateSingleLineLiteral(New String(Output.ToArray())))
                        Dim c = Peek1()
                        Select Case c
                            Case " "c
                                Return MakeResult(Token.CreateSingleLineLiteral(New String(Output.ToArray())))
                            Case """"c
                                Output.Add(c)
                                State = 22
                                Proceed()
                            Case "\"c
                                State = 31
                                Proceed()
                            Case Else
                                Output.Add(c)
                                State = 3
                                Proceed()
                        End Select
                    Case 22
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMark")
                        Dim c = Peek1()
                        If c = """"c Then
                            State = 23
                            Proceed()
                        Else
                            Output.Add(c)
                            Proceed()
                        End If
                    Case 23
                        If EndOfLine() Then Return MakeResult(Token.CreateSingleLineLiteral(New String(Output.ToArray())))
                        Dim c = Peek1()
                        If IsForbiddenWhitespaces(c) Then Throw MakeNextCharErrorTokenException("InvalidChar")
                        Select Case c
                            Case " "c
                                Return MakeResult(Token.CreateSingleLineLiteral(New String(Output.ToArray())))
                            Case """"c
                                Output.Add(c)
                                State = 22
                                Proceed()
                            Case "("c, ")"c
                                Return MakeResult(Token.CreateSingleLineLiteral(New String(Output.ToArray())))
                            Case Else
                                Throw MakeNextCharErrorTokenException("InvalidChar")
                        End Select
                    Case 3
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMark")
                        Dim c = Peek1()
                        Select Case c
                            Case """"c
                                If Peek(2) = """""" Then
                                    Proceed()
                                    Proceed()
                                    Return MakeResult(Token.CreateSingleLineLiteral(New String(Output.ToArray())))
                                End If
                                Throw MakeNextCharErrorTokenException("InvalidQuotationMark")
                            Case "\"c
                                State = 31
                                Proceed()
                            Case Else
                                Output.Add(c)
                                Proceed()
                        End Select
                    Case 31
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMark")
                        Dim c = Peek1()
                        Select Case c
                            Case "0"c
                                Output.Add(ChrW(&H0))
                                State = 3
                                Proceed()
                            Case "a"c
                                Output.Add(ChrW(&H7))
                                State = 3
                                Proceed()
                            Case "b"c
                                Output.Add(ChrW(&H8))
                                State = 3
                                Proceed()
                            Case "f"c
                                Output.Add(ChrW(&HC))
                                State = 3
                                Proceed()
                            Case "n"c
                                Output.Add(ChrW(&HA))
                                State = 3
                                Proceed()
                            Case "r"c
                                Output.Add(ChrW(&HD))
                                State = 3
                                Proceed()
                            Case "t"c
                                Output.Add(ChrW(&H9))
                                State = 3
                                Proceed()
                            Case "v"c
                                Output.Add(ChrW(&HB))
                                State = 3
                                Proceed()
                            Case "x"c
                                Proceed()
                                Dim Hex = Peek(2)
                                ProceedMultiple(Hex.Length)
                                If Not IsHex(Hex, 2) Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Output.Add(ChrW(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)))
                                State = 3
                            Case "u"c
                                Proceed()
                                Dim Hex = Peek(4)
                                ProceedMultiple(Hex.Length)
                                If Not IsHex(Hex, 4) Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Output.Add(ChrW(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)))
                                State = 3
                            Case "U"c
                                Proceed()
                                Dim Hex = Peek(5)
                                ProceedMultiple(Hex.Length)
                                If Not IsHex(Hex, 5) Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Output.AddRange(ChrQ(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)).ToString())
                                State = 3
                            Case Else
                                Output.Add(c)
                                State = 3
                                Proceed()
                        End Select
                    Case Else
                        Throw New InvalidOperationException
                End Select
            End While
            Throw New InvalidOperationException
        End Function
    End Class
End Namespace
