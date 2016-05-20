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
            'Stack<ParenthesisType> 括号栈

            '初值
            'State <- 0
            'Tag TokenType <- 单行字面量
            'Stack<ParenthesisType> <- 空

            'State 0
            '    EndOfLine -> 返回空Token，RemainingChars为空
            '    Space -> 前进
            '    \f\t\v -> 失败
            '    " -> 标记符号开始，State 2，前进
            '    ( -> 标记符号开始，前进，返回LeftParenthesis
            '    ) -> 标记符号开始，前进，返回RightParenthesis
            '    // -> 标记符号开始，前进到底，返回SingleLineComment，RemainingChars为空
            '    / -> 失败
            '    < -> 压栈，加入Output，State 1，前进
            '    [ -> 压栈，加入Output，State 1，前进
            '    { -> 压栈，加入Output，State 1，前进
            '    $ -> 标记符号开始，Tag TokenType <- 预处理指令，State 1，前进
            '    # -> 标记符号开始，Tag TokenType <- 自定义指令，State 1，前进
            '    [!@%&;=?\^`|~] -> 失败
            '    Any -> 标记符号开始，加入Output，State 1，前进

            'State 1
            '    EndOfLine -> 检查栈，失败或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)，RemainingChars为空
            '    Space -> 检查栈，(加入Output，前进)或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)
            '    \f\t\v -> 失败
            '    " -> 检查栈，(加入Output，前进)或失败
            '    ( -> 检查栈，失败或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)
            '    ) -> 检查栈，失败或返回(Tag 0 => SingleLineLiteral | PreprocessDirective | FunctionDirective)
            '    < -> 压栈，加入Output，前进
            '    [ -> 压栈，加入Output，前进
            '    { -> 压栈，加入Output，前进
            '    > -> 前进，检查并退栈，加入Output，保持
            '    ] -> 前进，检查并退栈，加入Output，保持
            '    } -> 前进，检查并退栈，加入Output，保持
            '    Any -> 加入Output，前进

            'State 2
            '    EndOfLine -> 失败
            '    " -> State 21，前进
            '    Any -> 加入Output，State 22，前进

            'State 21
            '    EndOfLine -> 返回SingleLineLiteral，RemainingChars为空
            '    Space -> 返回SingleLineLiteral
            '    " -> 加入Output，State 22，前进
            '    \ -> State 31，前进
            '    Any -> 加入Output，State 3，前进

            'State 22
            '    EndOfLine -> 失败
            '    " -> State 23，前进
            '    Any -> 加入Output，前进

            'State 23
            '    EndOfLine -> 返回SingleLineLiteral，RemainingChars为空
            '    Space -> 返回SingleLineLiteral
            '    \f\t\v -> 失败
            '    " -> 加入Output，State 22，前进
            '    ( -> 返回SingleLineLiteral
            '    ) -> 返回SingleLineLiteral
            '    Any -> 失败

            'State 3
            '    EndOfLine -> 失败
            '    "" -> 前进，前进，返回SingleLineLiteral
            '    " -> 失败
            '    \ -> State 31，前进
            '    Any -> 加入Output，前进

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
            '    x -> 失败
            '    u -> 失败
            '    U -> 失败
            '    Any -> 加入Output，State 3，前进

            Dim s = Text.GetTextInLine(RangeInLine)
            Dim Index As Integer = 0

            Dim EndOfLine = Function() Index >= s.Length
            Dim Peek1 = Function() s(Index)
            Dim Peek = Function(n As Integer) s.Substring(Index, Math.Min(n, s.Length - Index))
            Dim Proceed = Sub() Index += 1
            Dim ProceedMultiple = Sub(n As Integer) Index += n

            Dim MakeRemainingChars = Function() CType(New TextRange With {.Start = Text.Calc(RangeInLine.Start, Index), .End = RangeInLine.End}, [Optional](Of TextRange))
            Dim NullRemainingChars = [Optional](Of TextRange).Empty
            Dim NullToken = [Optional](Of Token).Empty
            Dim MakeTokenRange = Function(TokenStart As Integer, TokenEnd As Integer) New TextRange With {.Start = Text.Calc(RangeInLine.Start, TokenStart), .End = Text.Calc(RangeInLine.Start, TokenEnd)}

            Dim State = 0
            Dim Tag = TokenType.SingleLineLiteral
            Dim Stack As New Stack(Of ParenthesisType)

            Dim StartIndex As Integer = 0
            Dim MakeCurrentErrorTokenException = Function(Message As String) New InvalidTokenException(Message, New FileTextRange With {.Text = Text, .Range = MakeTokenRange(StartIndex, Index)}, Text.GetTextInLine(MakeTokenRange(StartIndex, Index)))
            Dim MakeNextCharErrorTokenException = Function(Message As String) New InvalidTokenException(Message, MakeNextErrorTokenRange(1), Peek(1))
            Dim MarkStart = Sub() StartIndex = Index
            Dim Output As New List(Of Char)
            Dim Write = Sub(c As Char) Output.Add(c)
            Dim WriteString = Sub(cs As String) Output.AddRange(cs)
            Dim MarkToken =
                Function(t As Token) As Token
                    Dim Range = MakeTokenRange(StartIndex, Index)
                    Positions.Add(t, Range)
                    Return t
                End Function
            Dim MakeToken =
                Function() As [Optional](Of Token)
                    Select Case Tag
                        Case TokenType.SingleLineLiteral
                            Return MarkToken(Token.CreateSingleLineLiteral(New String(Output.ToArray())))
                        Case TokenType.PreprocessDirective
                            Return MarkToken(Token.CreatePreprocessDirective(New String(Output.ToArray())))
                        Case TokenType.FunctionDirective
                            Return MarkToken(Token.CreateFunctionDirective(New String(Output.ToArray())))
                        Case Else
                            Throw New InvalidOperationException
                    End Select
                End Function

            While True
                Select Case State
                    Case 0
                        If EndOfLine() Then Return New TreeFormatTokenParseResult With {.Token = NullToken, .RemainingChars = NullRemainingChars}
                        Dim c = Peek1()
                        If IsForbiddenWhitespaces(c) Then Throw MakeNextCharErrorTokenException("InvalidWhitespace")
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
                                Return New TreeFormatTokenParseResult With {.Token = MarkToken(Token.CreateLeftParenthesis()), .RemainingChars = MakeRemainingChars()}
                            Case ")"c
                                MarkStart()
                                Proceed()
                                Return New TreeFormatTokenParseResult With {.Token = MarkToken(Token.CreateRightParenthesis()), .RemainingChars = MakeRemainingChars()}
                            Case "/"c
                                If Peek(2) = "//" Then
                                    MarkStart()
                                    Proceed()
                                    Proceed()
                                    While Not EndOfLine()
                                        Write(Peek1())
                                        Proceed()
                                    End While
                                    Return New TreeFormatTokenParseResult With {.Token = MarkToken(Token.CreateSingleLineComment(New String(Output.ToArray()))), .RemainingChars = NullRemainingChars}
                                End If
                                Throw MakeNextCharErrorTokenException("InvalidChar")
                            Case "<"c
                                Stack.Push(ParenthesisType.Angle)
                                Write(c)
                                State = 1
                                Proceed()
                            Case "["c
                                Stack.Push(ParenthesisType.Bracket)
                                Write(c)
                                State = 1
                                Proceed()
                            Case "{"c
                                Stack.Push(ParenthesisType.Brace)
                                Write(c)
                                State = 1
                                Proceed()
                            Case "$"c
                                Tag = TokenType.PreprocessDirective
                                State = 1
                                Proceed()
                            Case "#"c
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
                            If Stack.Count <> 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                            Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = NullRemainingChars}
                        End If
                        Dim c = Peek1()
                        If IsForbiddenWhitespaces(c) Then Throw MakeNextCharErrorTokenException("InvalidWhitespace")
                        Select Case c
                            Case " "c
                                If Stack.Count = 0 Then Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                                Write(c)
                                Proceed()
                            Case """"c
                                If Stack.Count = 0 Then Throw MakeNextCharErrorTokenException("InvalidChar")
                                Write(c)
                                Proceed()
                            Case "("c, ")"c
                                If Stack.Count <> 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case "<"c
                                Stack.Push(ParenthesisType.Angle)
                                Write(c)
                                Proceed()
                            Case "["c
                                Stack.Push(ParenthesisType.Bracket)
                                Write(c)
                                Proceed()
                            Case "{"c
                                Stack.Push(ParenthesisType.Brace)
                                Write(c)
                                Proceed()
                            Case ">"c
                                Proceed()
                                If Stack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                If Stack.Peek <> ParenthesisType.Angle Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Stack.Pop()
                                Write(c)
                            Case "]"c
                                Proceed()
                                If Stack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                If Stack.Peek <> ParenthesisType.Bracket Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Stack.Pop()
                                Write(c)
                            Case "}"c
                                Proceed()
                                If Stack.Count = 0 Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                If Stack.Peek <> ParenthesisType.Brace Then Throw MakeCurrentErrorTokenException("InvalidParenthesis")
                                Stack.Pop()
                                Write(c)
                            Case Else
                                Write(c)
                                Proceed()
                        End Select
                    Case 2
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek1()
                        If c = """"c Then
                            State = 21
                            Proceed()
                        Else
                            Write(c)
                            State = 22
                            Proceed()
                        End If
                    Case 21
                        If EndOfLine() Then Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = NullRemainingChars}
                        Dim c = Peek1()
                        Select Case c
                            Case " "c
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case """"c
                                Write(c)
                                State = 22
                                Proceed()
                            Case "\"c
                                State = 31
                                Proceed()
                            Case Else
                                Write(c)
                                State = 3
                                Proceed()
                        End Select
                    Case 22
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek1()
                        If c = """"c Then
                            State = 23
                            Proceed()
                        Else
                            Write(c)
                            Proceed()
                        End If
                    Case 23
                        If EndOfLine() Then Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = NullRemainingChars}
                        Dim c = Peek1()
                        If IsForbiddenWhitespaces(c) Then Throw MakeNextCharErrorTokenException("InvalidWhitespace")
                        Select Case c
                            Case " "c
                                If Stack.Count <> 0 Then Throw New InvalidOperationException
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case """"c
                                Write(c)
                                State = 22
                                Proceed()
                            Case "("c, ")"c
                                If Stack.Count <> 0 Then Throw New InvalidOperationException
                                Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                            Case Else
                                Throw MakeNextCharErrorTokenException("InvalidChar")
                        End Select
                    Case 3
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek1()
                        Select Case c
                            Case """"c
                                If Peek(2) = """""" Then
                                    If Stack.Count <> 0 Then Throw New InvalidOperationException
                                    Proceed()
                                    Proceed()
                                    Return New TreeFormatTokenParseResult With {.Token = MakeToken(), .RemainingChars = MakeRemainingChars()}
                                End If
                                Throw MakeNextCharErrorTokenException("InvalidQuotationMarks")
                            Case "\"c
                                State = 31
                                Proceed()
                            Case Else
                                Write(c)
                                Proceed()
                        End Select
                    Case 31
                        If EndOfLine() Then Throw MakeCurrentErrorTokenException("InvalidQuotationMarks")
                        Dim c = Peek1()
                        Select Case c
                            Case "0"c
                                Write(ChrW(&H0))
                                State = 3
                                Proceed()
                            Case "a"c
                                Write(ChrW(&H7))
                                State = 3
                                Proceed()
                            Case "b"c
                                Write(ChrW(&H8))
                                State = 3
                                Proceed()
                            Case "f"c
                                Write(ChrW(&HC))
                                State = 3
                                Proceed()
                            Case "n"c
                                Write(ChrW(&HA))
                                State = 3
                                Proceed()
                            Case "r"c
                                Write(ChrW(&HD))
                                State = 3
                                Proceed()
                            Case "t"c
                                Write(ChrW(&H9))
                                State = 3
                                Proceed()
                            Case "v"c
                                Write(ChrW(&HB))
                                State = 3
                                Proceed()
                            Case "x"c
                                Proceed()
                                Dim Hex = Peek(2)
                                ProceedMultiple(Hex.Length)
                                If Not IsHex(Hex, 2) Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Write(ChrW(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)))
                                State = 3
                            Case "u"c
                                Proceed()
                                Dim Hex = Peek(4)
                                ProceedMultiple(Hex.Length)
                                If Not IsHex(Hex, 4) Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                Write(ChrW(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)))
                                State = 3
                            Case "U"c
                                Proceed()
                                Dim Hex = Peek(5)
                                ProceedMultiple(Hex.Length)
                                If Not IsHex(Hex, 5) Then
                                    Throw MakeCurrentErrorTokenException("InvalidEscapeSequence")
                                End If
                                WriteString(ChrQ(Integer.Parse(Hex, Globalization.NumberStyles.HexNumber)).ToString())
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
