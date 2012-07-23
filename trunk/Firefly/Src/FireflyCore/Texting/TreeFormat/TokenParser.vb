'==========================================================================
'
'  File:        TokenParser.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 词法解析器
'  Version:     2012.07.23.
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
        Public Token As Opt(Of Token)
        Public RemainingChars As Opt(Of TextRange)
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
            Dim IndentChars = t.Distinct
            If IndentChars.Count > 1 OrElse (IndentChars.Count = 1 AndAlso IndentChars.Single <> " "c) Then Return False
            Return True
        End Function
        Public Function IsExactFitIndentLevel(ByVal Line As TextLine, ByVal IndentLevel As Integer) As Boolean
            Dim t = Line.Text
            Dim IndentCount = IndentLevel * 4
            If t.Length < IndentCount Then Return False
            Dim IndentChars = t.Substring(0, IndentCount).Distinct
            If IndentChars.Count > 1 OrElse (IndentChars.Count = 1 AndAlso IndentChars.Single <> " "c) Then Return False
            If t.Length = IndentCount Then Return True
            If t(IndentCount) = " "c Then Return False
            Return True
        End Function
        Public Function IsFitIndentLevel(ByVal Line As TextLine, ByVal IndentLevel As Integer) As Boolean
            Dim t = Line.Text
            Dim IndentCount = IndentLevel * 4
            If t.Length < IndentCount Then Return False
            Dim IndentChars = t.Substring(0, IndentCount).Distinct
            If IndentChars.Count > 1 OrElse (IndentChars.Count = 1 AndAlso IndentChars.Single <> " "c) Then Return False
            Return True
        End Function

        Private Enum ParentheseType
            Angle
            Bracket
            Brace
        End Enum
        Private Enum TokenType
            SingleLineLiteral
            PreprocessDirective
            FunctionDirective
        End Enum
        Private rForbiddenWhitespaces As New Regex("^[\f\t\v]$", RegexOptions.ExplicitCapture)
        Private rForbiddenHeadChars As New Regex("^[!%&;=?\^`|~]$", RegexOptions.ExplicitCapture)
        Private rHex2 As New Regex("^[0-9A-Fa-f]{2}$", RegexOptions.ExplicitCapture)
        Private rHex4 As New Regex("^[0-9A-Fa-f]{4}$", RegexOptions.ExplicitCapture)
        Private rHex5 As New Regex("^[0-9A-Fa-f]{5}$", RegexOptions.ExplicitCapture)
        Public Function ReadToken(ByVal RangeInLine As TextRange) As TreeFormatTokenParseResult
            'State 0    Whitespace空格
            'State 1    普通Token
            'State 1-n  普通Token<中
            'State 1-n  普通Token[中
            'State 1-n  普通Token{中
            'State 2    双引号开始
            'State 21   双双引号开始
            'State 22   普通双引号Token确定
            'State 23   普通双引号Token结束双引号/转义双引号
            'State 3    转义双双引号Token
            'State 31   转义双双引号Token转义符
            'Tag TokenType 单行字面量、预处理指令、自定义指令
            'Stack<ParentheseType> 括号栈

            '初值
            'State <- 0
            'Tag TokenType <- 单行字面量
            'Stack<Integer> <- 空

            'State 0
            '    EndOfLine -> 返回空Token，RemainingChars为空
            '    空格 -> 保持，前进
            '    \f\t\v -> 失败
            '    " -> 标记符号开始，State 2，前进
            '    ( -> 标记符号开始，前进，返回LeftParenthesesToken
            '    ) -> 标记符号开始，前进，返回RightParentheses
            '    // -> 标记符号开始，前进到底，返回SingleLineComment，RemainingChars为空
            '    / -> 失败
            '    < -> 压栈，加入Output，State 1，前进
            '    [ -> 压栈，加入Output，State 1，前进
            '    { -> 压栈，加入Output，State 1，前进
            '    $ -> 标记符号开始，Tag TokenType <- 预处理指令，State 1，前进
            '    # -> 标记符号开始，Tag TokenType <- 自定义指令，State 1，前进
            '    [!@%&;=?\^`|~] -> 失败
            '    . -> 标记符号开始，加入Output，State 1，前进

            'State 1
            '    EndOfLine -> 检查栈，失败或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)，RemainingChars为空
            '    空格 -> 检查栈，(加入Output，保持，前进)或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)
            '    \f\t\v -> 失败
            '    " -> 检查栈，(加入Output，保持，前进)或失败
            '    ( -> 检查栈，失败或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)
            '    ) -> 检查栈，失败或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)
            '    < -> 压栈，加入Output，保持，前进
            '    [ -> 压栈，加入Output，保持，前进
            '    { -> 压栈，加入Output，保持，前进
            '    > -> 前进，检查并退栈，加入Output，保持
            '    ] -> 前进，检查并退栈，加入Output，保持
            '    } -> 前进，检查并退栈，加入Output，保持
            '    . -> 加入Output，保持，前进

            'State 2
            '    EndOfLine -> 失败
            '    " -> State 21，前进
            '    . -> 加入Output，State 22，前进

            'State 21
            '    EndOfLine -> 返回SingleLineLiteral，RemainingChars为空
            '    空格 -> 返回SingleLineLiteral
            '    " -> 加入Output，State 22，前进
            '    \ -> State 31，前进
            '    . -> 加入Output，State 3，前进

            'State 22
            '    EndOfLine -> 失败
            '    " -> State 23，前进
            '    . -> 加入Output，保持，前进

            'State 23
            '    EndOfLine -> 返回SingleLineLiteral，RemainingChars为空
            '    空格 -> 返回SingleLineLiteral
            '    \f\t\v -> 失败
            '    " -> 加入Output，State 22，前进
            '    ( -> 返回SingleLineLiteral
            '    ) -> 返回SingleLineLiteral
            '    . -> 失败

            'State 3
            '    EndOfLine -> 失败
            '    "" -> 前进，前进，返回SingleLineLiteral
            '    " -> 失败
            '    \ -> State 31，前进
            '    . -> 加入Output，保持，前进

            'State 31
            '    EndOfLine -> 失败
            '    0 -> 加入U+0000到Output，State 3，前进
            '    a -> 加入U+0007到Output，State 3，前进
            '    b -> 加入U+0008到Output，State 3，前进
            '    f -> 加入U+000C到Output，State 3，前进
            '    n -> 加入U+000A到Output，State 3，前进
            '    r -> 加入U+000D到Output，State 3，前进
            '    t -> 加入U+0009到Output，State 3，前进
            '    v -> 加入U+000B到Output，State 3，前进
            '    x[0-9A-Fa-f]{2} -> 加入U+00..到Output，State 3，前进3
            '    u[0-9A-Fa-f]{4} -> 加入U+....到Output，State 3，前进5
            '    U[0-9A-Fa-f]{5} -> 加入U+.....到Output，State 3，前进6
            '    . -> 加入Output，State 3，前进

            Dim s = Text.GetTextInLine(RangeInLine)
            Dim Index As Integer = 0

            Dim EndOfLine = Function() Index >= s.Length
            Dim Peek = Function(n As Integer) s.Substring(Index, Math.Min(n, s.Length - Index))
            Dim Proceed = Sub() Index += 1
            Dim ProceedMultiple = Sub(n As Integer) Index += n

            Dim MakeRemainingChars = Function() CType(New TextRange With {.Start = Text.Calc(RangeInLine.Start, Index), .End = RangeInLine.End}, Opt(Of TextRange))
            Dim NullRemainingChars = Opt(Of TextRange).Empty
            Dim NullToken = Opt(Of Token).Empty
            Dim MakeTokenRange = Function(TokenStart As Integer, TokenEnd As Integer) New TextRange With {.Start = Text.Calc(RangeInLine.Start, TokenStart), .End = Text.Calc(RangeInLine.Start, TokenEnd)}
            Dim MakeNextErrorTokenRange = Function(n As Integer) New FileTextRange With {.Text = Text, .Range = MakeTokenRange(Index, n)}

            Dim State = 0
            Dim Tag = TokenType.SingleLineLiteral
            Dim ParentheseStack As New Stack(Of ParentheseType)

            Dim StartIndex As Integer = 0
            Dim MakeCurrentErrorTokenException = Function(Message As String) New InvalidTokenException(Message, New FileTextRange With {.Text = Text, .Range = MakeTokenRange(StartIndex, Index)}, Text.GetTextInLine(MakeTokenRange(StartIndex, Index)))
            Dim MakeNextCharErrorTokenException = Function(Message As String) New InvalidTokenException(Message, MakeNextErrorTokenRange(1), Peek(1))
            Dim MarkStart = Sub() StartIndex = Index
            Dim Output As New List(Of Char)
            Dim Write = Sub(cs As String) Output.AddRange(cs)
            Dim MakeToken =
                Function() As Opt(Of Token)
                    Dim Range = MakeTokenRange(StartIndex, Index)
                    Select Case Tag
                        Case TokenType.SingleLineLiteral
                            Dim t = Token.CreateSingleLineLiteral(New String(Output.ToArray()))
                            Positions.Add(t, Range)
                            Return t
                        Case TokenType.PreprocessDirective
                            Dim t = Token.CreatePreprocessDirective(New String(Output.ToArray()))
                            Positions.Add(t, Range)
                            Return t
                        Case TokenType.FunctionDirective
                            Dim t = Token.CreateFunctionDirective(New String(Output.ToArray()))
                            Positions.Add(t, Range)
                            Return t
                        Case Else
                            Throw New InvalidOperationException
                    End Select
                End Function
            Dim MakeLeftParenthesesToken =
                Function() As Opt(Of Token)
                    Dim Range = MakeTokenRange(StartIndex, Index)
                    Dim t = Token.CreateLeftParentheses()
                    Positions.Add(t, Range)
                    Return t
                End Function
            Dim MakeRightParenthesesToken =
                Function() As Opt(Of Token)
                    Dim Range = MakeTokenRange(StartIndex, Index)
                    Dim t = Token.CreateRightParentheses()
                    Positions.Add(t, Range)
                    Return t
                End Function
            Dim MakeSingleLineCommentToken =
                Function() As Opt(Of Token)
                    Dim Range = MakeTokenRange(StartIndex, Index)
                    Dim t = Token.CreateSingleLineComment(New String(Output.ToArray()))
                    Positions.Add(t, Range)
                    Return t
                End Function

            While True
                Select Case State
                    Case 0
                        If EndOfLine() Then Return New TreeFormatTokenParseResult With {.Token = NullToken, .RemainingChars = NullRemainingChars}
                        Dim c = Peek(1)
                        If rForbiddenWhitespaces.Match(c).Success Then Throw MakeNextCharErrorTokenException("InvalidWhitespace")
                        If rForbiddenHeadChars.Match(c).Success Then Throw MakeNextCharErrorTokenException("InvalidHeadChar")
                        Select Case c
                            Case " "
                                Proceed()
                            Case """"
                                State = 2
                                Proceed()
                            Case "("
                                MarkStart()
                                Proceed()
                                Return New TreeFormatTokenParseResult With {.Token = MakeLeftParenthesesToken(), .RemainingChars = MakeRemainingChars()}
                            Case ")"
                                MarkStart()
                                Proceed()
                                Return New TreeFormatTokenParseResult With {.Token = MakeRightParenthesesToken(), .RemainingChars = MakeRemainingChars()}
                            Case "/"
                                If Peek(2) = "//" Then
                                    MarkStart()
                                    Proceed()
                                    Proceed()
                                    While Not EndOfLine()
                                        Write(Peek(1))
                                        Proceed()
                                    End While
                                    Return New TreeFormatTokenParseResult With {.Token = MakeSingleLineCommentToken(), .RemainingChars = NullRemainingChars}
                                End If
                                Throw MakeNextCharErrorTokenException("InvalidChar")
                            Case "<"
                                ParentheseStack.Push(ParentheseType.Angle)
                                Write(c)
                                State = 1
                                Proceed()
                            Case "["
                                ParentheseStack.Push(ParentheseType.Bracket)
                                Write(c)
                                State = 1
                                Proceed()
                            Case "{"
                                ParentheseStack.Push(ParentheseType.Brace)
                                Write(c)
                                State = 1
                                Proceed()
                            Case "$"
                                Tag = TokenType.PreprocessDirective
                                State = 1
                                Proceed()
                            Case "#"
                                Tag = TokenType.FunctionDirective
                                State = 1
                                Proceed()
                            Case Else
                                Write(c)
                                State = 1
                                Proceed()
                        End Select
                    Case 1
                        If EndOfLine() Then
                            If ParentheseStack.Count <> 0 Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                            Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = NullRemainingChars}
                        End If
                        Dim c = Peek(1)
                        If rForbiddenWhitespaces.Match(c).Success Then Throw MakeNextCharErrorTokenException("InvalidWhitespace")
                        Select Case c
                            Case " "
                                If ParentheseStack.Count = 0 Then Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                                Write(c)
                                Proceed()
                            Case """"
                                If ParentheseStack.Count = 0 Then Throw MakeNextCharErrorTokenException("InvalidChar")
                                Write(c)
                                Proceed()
                            Case "(", ")"
                                If ParentheseStack.Count <> 0 Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case "<"
                                ParentheseStack.Push(ParentheseType.Angle)
                                Write(c)
                                Proceed()
                            Case "["
                                ParentheseStack.Push(ParentheseType.Bracket)
                                Write(c)
                                Proceed()
                            Case "{"
                                ParentheseStack.Push(ParentheseType.Brace)
                                Write(c)
                                Proceed()
                            Case ">"
                                Proceed()
                                If ParentheseStack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                                If ParentheseStack.Peek <> ParentheseType.Angle Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                                ParentheseStack.Pop()
                                Write(c)
                            Case "]"
                                Proceed()
                                If ParentheseStack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                                If ParentheseStack.Peek <> ParentheseType.Bracket Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                                ParentheseStack.Pop()
                                Write(c)
                            Case "}"
                                Proceed()
                                If ParentheseStack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                                If ParentheseStack.Peek <> ParentheseType.Brace Then Throw MakeCurrentErrorTokenException("InvalidParentheses")
                                ParentheseStack.Pop()
                                Write(c)
                            Case Else
                                Write(c)
                                Proceed()
                        End Select
                    Case 2
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek(1)
                        If c = """" Then
                            State = 21
                            Proceed()
                        Else
                            Write(c)
                            State = 22
                            Proceed()
                        End If
                    Case 21
                        If EndOfLine() Then Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = NullRemainingChars}
                        Dim c = Peek(1)
                        Select Case c
                            Case " "
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case """"
                                Write(c)
                                State = 22
                                Proceed()
                            Case "\"
                                State = 31
                                Proceed()
                            Case Else
                                Write(c)
                                State = 3
                                Proceed()
                        End Select
                    Case 22
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek(1)
                        If c = """" Then
                            State = 23
                            Proceed()
                        Else
                            Write(c)
                            Proceed()
                        End If
                    Case 23
                        If EndOfLine() Then Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = NullRemainingChars}
                        Dim c = Peek(1)
                        If rForbiddenWhitespaces.Match(c).Success Then Throw MakeNextCharErrorTokenException("InvalidWhitespace")
                        Select Case c
                            Case " "
                                If ParentheseStack.Count <> 0 Then Throw New InvalidOperationException
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case """"
                                Write(c)
                                State = 22
                                Proceed()
                            Case "(", ")"
                                If ParentheseStack.Count <> 0 Then Throw New InvalidOperationException
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case Else
                                Throw MakeNextCharErrorTokenException("InvalidChar")
                        End Select
                    Case 3
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek(1)
                        Select Case c
                            Case """"
                                If Peek(2) = """""" Then
                                    If ParentheseStack.Count <> 0 Then Throw New InvalidOperationException
                                    Proceed()
                                    Proceed()
                                    Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                                End If
                                Throw MakeNextCharErrorTokenException("InvalidQuotationMarks")
                            Case "\"
                                State = 31
                                Proceed()
                            Case Else
                                Write(c)
                                Proceed()
                        End Select
                    Case 31
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek(1)
                        Select Case c
                            Case "0"
                                Write(ChrW(&H0))
                                State = 3
                                Proceed()
                            Case "a"
                                Write(ChrW(&H7))
                                State = 3
                                Proceed()
                            Case "b"
                                Write(ChrW(&H8))
                                State = 3
                                Proceed()
                            Case "f"
                                Write(ChrW(&HC))
                                State = 3
                                Proceed()
                            Case "n"
                                Write(ChrW(&HA))
                                State = 3
                                Proceed()
                            Case "r"
                                Write(ChrW(&HD))
                                State = 3
                                Proceed()
                            Case "t"
                                Write(ChrW(&H9))
                                State = 3
                                Proceed()
                            Case "v"
                                Write(ChrW(&HB))
                                State = 3
                                Proceed()
                            Case "x"
                                Proceed()
                                Dim Hex = Peek(2)
                                ProceedMultiple(Hex.Length)
                                If Not rHex2.Match(Hex).Success Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Write(ChrW(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)))
                                State = 3
                            Case "u"
                                Proceed()
                                Dim Hex = Peek(4)
                                ProceedMultiple(Hex.Length)
                                If Not rHex4.Match(Hex).Success Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Write(ChrW(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)))
                                State = 3
                            Case "U"
                                Proceed()
                                Dim Hex = Peek(5)
                                ProceedMultiple(Hex.Length)
                                If Not rHex5.Match(Hex).Success Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Write(ChrQ(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)).ToString().ToCharArray())
                                State = 3
                            Case Else
                                Write(c)
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
