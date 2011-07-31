'==========================================================================
'
'  File:        SyntaxParser.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文法解析器 - 用于从符号转到文法树
'  Version:     2011.07.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports Firefly.TextEncoding
Imports Firefly.Texting.TreeFormat.Syntax

Namespace Texting.TreeFormat
    Public Class TreeFormatParseSetting
        Public IsTreeParameterFunction As Func(Of String, Boolean) = Function(Name) False
        Public IsTableParameterFunction As Func(Of String, Boolean) = Function(Name) False
        Public IsTreeContentFunction As Func(Of String, Boolean) = Function(Name) False
        Public IsTableContentFunction As Func(Of String, Boolean) = Function(Name) False
    End Class

    Public Class TreeFormatParseResult
        Public Value As Forest
        Public Path As String

        ''' <summary>Token | SyntaxRule => Range</summary>
        Public Positions As Dictionary(Of Object, TextRange)

        ''' <summary>SingleLineFunctionNode | FunctionNodes => RawFunctionCall</summary>
        Public RawFunctionCalls As Dictionary(Of Object, RawFunctionCall)
    End Class

    Public Class TreeFormatSyntaxParser
        Private RawText As String
        Private Path As String = ""
        Private Text As Text
        Private IsTreeParameterFunction As Func(Of String, Boolean) = Function(f) False
        Private IsTableParameterFunction As Func(Of String, Boolean) = Function(f) False
        Private IsTreeContentFunction As Func(Of String, Boolean) = Function(f) False
        Private IsTableContentFunction As Func(Of String, Boolean) = Function(f) False

        Private TokenParser As TreeFormatTokenParser

        Private Positions As New Dictionary(Of Object, TextRange)
        Private FilePositions As New Dictionary(Of Object, FileTextRange)
        Private RawFunctionCalls As New Dictionary(Of Object, RawFunctionCall)

        Public Sub New(ByVal Text As String, Optional ByVal Path As String = "")
            Me.New(New TreeFormatParseSetting, Text, Path)
        End Sub
        Public Sub New(ByVal Setting As TreeFormatParseSetting, ByVal Text As String, Optional ByVal Path As String = "")
            Me.RawText = Text
            Me.Path = Path
            Me.IsTreeParameterFunction = Setting.IsTreeParameterFunction
            Me.IsTableParameterFunction = Setting.IsTableParameterFunction
            Me.IsTreeContentFunction = Setting.IsTreeContentFunction
            Me.IsTableContentFunction = Setting.IsTableContentFunction
        End Sub

        Public Function Parse() As TreeFormatParseResult
            Text = New Text With {.Path = Path, .Lines = GetLines(RawText)}
            TokenParser = New TreeFormatTokenParser(Path, Text, Positions)
            Dim Lines = New TextLineRange With {.StartRow = 1, .EndRow = Text.Lines.Length + 1}
            Dim MultiNodesList = ParseMultiNodesList(Lines, 0)
            Dim Forest = Mark(New Forest With {.MultiNodesList = MultiNodesList}, Lines)
            Return New TreeFormatParseResult With {.Value = Forest, .Path = Path, .Positions = Positions, .RawFunctionCalls = RawFunctionCalls}
        End Function

        Private Function GetRange(ByVal Obj As Object) As TextRange
            Return Positions(Obj)
        End Function
        Private Function GetFileRange(ByVal Obj As Object) As FileTextRange
            If FilePositions.ContainsKey(Obj) Then Return FilePositions(Obj)
            Dim fp = New FileTextRange With {.Path = Path, .Range = Positions(Obj)}
            FilePositions.Add(Obj, fp)
            Return fp
        End Function
        Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As TextRange) As T
            Positions.Add(Obj, Range)
            Return Obj
        End Function
        Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As TextLineRange) As T
            Dim Start = Text.GetTextLine(Range.StartRow).Range.Start
            Dim [End] As TextPosition
            If Range.EndRow >= Text.Lines.Length Then
                [End] = Text.GetTextLine(Range.EndRow - 1).Range.End
            Else
                [End] = Text.GetTextLine(Range.EndRow).Range.Start
            End If
            Return Mark(Obj, New TextRange With {.Start = Start, .End = [End]})
        End Function

        Private Shared rLineSeparator As New Regex("\r\n|\n", RegexOptions.ExplicitCapture)
        Private Shared rLineSeparators As New Regex("\r|\n", RegexOptions.ExplicitCapture)
        Private Function GetLines(ByVal Text As String) As TextLine()
            Dim l As New List(Of TextLine)

            Dim CurrentRow = 1
            Dim CurrentIndex = 0
            For Each m As Match In rLineSeparator.Matches(Text & "\r\n".Descape())
                Dim t = Text.Substring(CurrentIndex, m.Index - CurrentIndex)
                If rLineSeparators.Match(t).Success Then Throw New ArgumentException("IllegalLineSeparator")
                Dim Start As New TextPosition With {.CharIndex = CurrentIndex, .Row = CurrentRow, .Column = 1}
                Dim [End] As New TextPosition With {.CharIndex = m.Index, .Row = CurrentRow, .Column = Start.Column + t.Length}
                Dim r = New TextRange With {.Start = Start, .End = [End]}
                l.Add(New TextLine With {.Text = t, .Range = r})
                CurrentIndex = m.Index + m.Length
                CurrentRow += 1
            Next

            Return l.ToArray()
        End Function

        Private Function ParseMultiNodesList(ByVal Lines As TextLineRange, ByVal IndentLevel As Integer) As MultiNodes()
            Dim l As New List(Of MultiNodes)
            Dim RemainingLines As Opt(Of TextLineRange) = Lines
            While RemainingLines.HasValue
                Dim Result = ReadMultiNodes(RemainingLines.Value, IndentLevel)
                If Result.MultiNodes.HasValue Then l.Add(Result.MultiNodes.Value)
                RemainingLines = Result.RemainingLines
            End While
            Return l.ToArray()
        End Function

        Private Class MultiNodesParseResult
            Public MultiNodes As Opt(Of MultiNodes)
            Public RemainingLines As Opt(Of TextLineRange)
        End Class
        Private Function ReadMultiNodes(ByVal Lines As TextLineRange, ByVal IndentLevel As Integer) As MultiNodesParseResult
            Dim NullNode = Opt(Of MultiNodes).Empty
            Dim NullRemainingLines = Opt(Of TextLineRange).Empty

            Dim FirstLineIndex = Lines.StartRow
            While True
                If FirstLineIndex >= Lines.EndRow Then Return New MultiNodesParseResult With {.MultiNodes = NullNode, .RemainingLines = NullRemainingLines}
                If Not TokenParser.IsBlankLine(Text.GetTextLine(FirstLineIndex)) Then Exit While
                FirstLineIndex += 1
            End While

            Dim FirstLine = Text.GetTextLine(FirstLineIndex)
            If Not TokenParser.IsExactFitIndentLevel(FirstLine, IndentLevel) Then Throw New InvalidTokenException("InvaildIndentLevel", New FileTextRange With {.Path = Path, .Range = FirstLine.Range}, FirstLine.Text)

            Dim ChildLines As Opt(Of TextLineRange) = Nothing
            Dim ChildStartLineIndex = FirstLineIndex + 1
            Dim ChildEndLineIndex = ChildStartLineIndex
            While True
                If ChildEndLineIndex >= Lines.EndRow Then
                    If ChildEndLineIndex > ChildStartLineIndex Then
                        ChildLines = New TextLineRange With {.StartRow = ChildStartLineIndex, .EndRow = ChildEndLineIndex}
                    Else
                        ChildLines = New TextLineRange With {.StartRow = ChildStartLineIndex, .EndRow = ChildStartLineIndex}
                    End If
                    Exit While
                End If
                Dim ChildCurrentLine = Text.GetTextLine(ChildEndLineIndex)
                If Not TokenParser.IsBlankLine(ChildCurrentLine) Then
                    If TokenParser.IsExactFitIndentLevel(ChildCurrentLine, IndentLevel) Then
                        ChildLines = New TextLineRange With {.StartRow = ChildStartLineIndex, .EndRow = ChildEndLineIndex}
                        Exit While
                    End If
                    If Not TokenParser.IsFitIndentLevel(ChildCurrentLine, IndentLevel + 1) Then
                        Throw New InvalidTokenException("InvaildIndentLevel", New FileTextRange With {.Path = Path, .Range = ChildCurrentLine.Range}, ChildCurrentLine.Text)
                    End If
                End If
                ChildEndLineIndex += 1
            End While
            If Not ChildLines.HasValue Then Throw New InvalidOperationException

            Dim EndLineIndex = ChildEndLineIndex
            Dim EndLine = Opt(Of TextLine).Empty

            '如果最后有$End预处理指令，则将其包含到
            If ChildEndLineIndex < Lines.EndRow Then
                Dim CurrentLine = Text.GetTextLine(ChildEndLineIndex)
                Dim FirstToken = TokenParser.ReadToken(CurrentLine.Range)
                If FirstToken.Token.HasValue Then
                    If FirstToken.Token.Value.OnPreprocessDirective Then
                        If FirstToken.Token.Value.PreprocessDirective = "End" Then
                            EndLineIndex += 1
                            EndLine = CurrentLine
                        End If
                    End If
                End If
            End If

            '获得剩余行数
            Dim RemainingLines As Opt(Of TextLineRange)
            If EndLineIndex >= Lines.EndRow Then
                RemainingLines = NullRemainingLines
            Else
                RemainingLines = New TextLineRange With {.StartRow = EndLineIndex, .EndRow = Lines.EndRow}
            End If

            Dim MultiNodesLines As New TextLineRange With {.StartRow = FirstLineIndex, .EndRow = EndLineIndex}
            Dim MultiNodes = ParseMultiNodes(MultiNodesLines, FirstLine, ChildLines.Value, EndLine, IndentLevel)
            Return New MultiNodesParseResult With {.MultiNodes = MultiNodes, .RemainingLines = RemainingLines}
        End Function
        Private Function ParseMultiNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer) As MultiNodes
            Dim FirstTokenResult = TokenParser.ReadToken(FirstLine.Range)
            If Not FirstTokenResult.Token.HasValue Then Throw New InvalidOperationException
            Dim FirstToken = FirstTokenResult.Token.Value
            Dim RemainingChars = FirstTokenResult.RemainingChars

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral, TokenTag.LeftParentheses, TokenTag.RightParentheses, TokenTag.SingleLineComment
                    Dim Node = ParseNode(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)
                    Return Mark(MultiNodes.CreateNode(Node), Lines)
                Case TokenTag.PreprocessDirective
                    Select Case FirstToken.PreprocessDirective
                        Case "List"
                            Dim ListNodes = ParseListNodes(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)
                            Return Mark(MultiNodes.CreateListNodes(ListNodes), Lines)
                        Case "Table"
                            Dim TableNodes = ParseTableNodes(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)
                            Return Mark(MultiNodes.CreateTableNodes(TableNodes), Lines)
                        Case Else
                            Dim Node = ParseNode(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)
                            Return Mark(MultiNodes.CreateNode(Node), Lines)
                    End Select
                Case TokenTag.FunctionDirective
                    Dim FunctionNodes = ParseFunctionNodes(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)
                    Return Mark(MultiNodes.CreateFunctionNodes(FunctionNodes), Lines)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Private Function ParseNode(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As Node
            Dim FirstTokenResult = TokenParser.ReadToken(FirstLine.Range)
            If Not FirstTokenResult.Token.HasValue Then Throw New InvalidOperationException
            Dim FirstTokenRange = GetRange(FirstToken)

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral
                    If Not EndLine.HasValue AndAlso (ChildLines.StartRow >= ChildLines.EndRow OrElse Text.GetLines(ChildLines).All(Function(Line) TokenParser.IsBlankLine(Line))) Then
                        Return Mark(Node.CreateSingleLineNodeLine(ParseSingleLineNodeLine(FirstLine, FirstToken, RemainingChars)), FirstLine.Range)
                    End If
                    Return Mark(Node.CreateMultiLineNode(ParseMultiLineNode(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)), Lines)
                Case TokenTag.LeftParentheses, TokenTag.RightParentheses
                    Return Mark(Node.CreateSingleLineNodeLine(ParseSingleLineNodeLine(FirstLine, FirstToken, RemainingChars)), FirstLine.Range)
                Case TokenTag.PreprocessDirective
                    Select Case FirstToken.PreprocessDirective
                        Case "String"
                            Return Mark(Node.CreateMultiLineLiteral(ParseMultiLineLiteral(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)), Lines)
                        Case "Comment"
                            Return Mark(Node.CreateMultiLineComment(ParseMultiLineComment(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)), Lines)
                        Case Else
                            Throw New InvalidSynaxRuleException("InvalidPreprocessDirective", GetFileRange(FirstToken), FirstToken)
                    End Select
                Case TokenTag.FunctionDirective
                    Throw New InvalidOperationException
                Case TokenTag.SingleLineComment
                    If FirstTokenResult.RemainingChars.HasValue Then Throw New InvalidOperationException
                    Dim FreeContent = Mark(New FreeContent With {.Text = FirstToken.SingleLineComment}, FirstTokenRange)
                    Dim SingleLineComment = Mark(New SingleLineComment With {.Content = FreeContent}, FirstTokenRange)
                    Return Mark(Node.CreateSingleLineComment(SingleLineComment), FirstTokenRange)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Private Function ParseListNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As ListNodes
            If Not RemainingChars.HasValue Then Throw New InvalidSynaxRuleException("ListChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondTokenResult = TokenParser.ReadToken(RemainingChars.Value)
            If Not SecondTokenResult.Token.HasValue Then Throw New InvalidSynaxRuleException("ListChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondToken = SecondTokenResult.Token.Value
            If Not SecondToken.OnSingleLineLiteral Then Throw New InvalidSynaxRuleException("ListChildHeadExpected", GetFileRange(SecondToken), SecondToken)
            Dim ChildHead = Mark(New SingleLineLiteral With {.Text = SecondToken.SingleLineLiteral}, GetRange(SecondToken))
            Dim SingleLineComment = ParseSingleLineComment(SecondTokenResult.RemainingChars)
            Dim Children = ParseMultiNodesList(ChildLines, IndentLevel + 1)
            Dim EndDirective = ParseEndDirective(EndLine)
            Return Mark(New ListNodes With {.ChildHead = ChildHead, .SingleLineComment = SingleLineComment, .Children = Children, .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseTableNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As TableNodes
            If Not RemainingChars.HasValue Then Throw New InvalidSynaxRuleException("TableChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondTokenResult = TokenParser.ReadToken(RemainingChars.Value)
            If Not SecondTokenResult.Token.HasValue Then Throw New InvalidSynaxRuleException("TableChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondToken = SecondTokenResult.Token.Value
            If Not SecondToken.OnSingleLineLiteral Then Throw New InvalidSynaxRuleException("TableChildHeadExpected", GetFileRange(SecondToken), SecondToken)
            Dim ChildHead = Mark(New SingleLineLiteral With {.Text = SecondToken.SingleLineLiteral}, GetRange(SecondToken))
            Dim ChildFields As New List(Of SingleLineLiteral)
            Dim CurrentRemainingChars As Opt(Of TextRange) = SecondTokenResult.RemainingChars
            Dim l As New List(Of SingleLineLiteral)
            While CurrentRemainingChars.HasValue
                Dim ChildHeadResult = TokenParser.ReadToken(CurrentRemainingChars.Value)
                If Not ChildHeadResult.Token.HasValue Then
                    CurrentRemainingChars = ChildHeadResult.RemainingChars
                    Exit While
                End If
                Dim ChildHeadToken = ChildHeadResult.Token.Value
                Select Case ChildHeadToken._Tag
                    Case TokenTag.SingleLineLiteral
                        Dim FieldHead = Mark(New SingleLineLiteral With {.Text = ChildHeadToken.SingleLineLiteral}, GetRange(ChildHeadToken))
                        ChildFields.Add(FieldHead)
                        CurrentRemainingChars = ChildHeadResult.RemainingChars
                    Case TokenTag.SingleLineComment
                        Exit While
                    Case Else
                        Throw New InvalidSynaxRuleException("SingleLineLiteralOrSingleLineCommentExpected", GetFileRange(ChildHeadToken), ChildHeadToken)
                End Select
            End While
            Dim SingleLineComment = ParseSingleLineComment(CurrentRemainingChars)
            Dim Children As New List(Of TableLine)
            For Each Line In Text.GetLines(ChildLines)
                Dim OptTableLine = ParseTableLine(Line)
                If OptTableLine.HasValue Then
                    Children.Add(OptTableLine.Value)
                End If
            Next
            Dim EndDirective = ParseEndDirective(EndLine)
            Return Mark(New TableNodes With {.ChildHead = ChildHead, .ChildFields = ChildFields.ToArray(), .SingleLineComment = SingleLineComment, .Children = Children.ToArray(), .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseFunctionNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As FunctionNodes
            Dim FunctionDirective = Mark(New FunctionDirective With {.Text = FirstToken.FunctionDirective}, GetRange(FirstToken))
            Dim l As New List(Of Token)
            Dim ParametersStart As TextPosition
            If RemainingChars.HasValue Then
                ParametersStart = RemainingChars.Value.Start
            Else
                ParametersStart = GetRange(FirstToken).End
            End If
            Dim ParameterEnd = ParametersStart
            Dim CurrentRemainingChars As Opt(Of TextRange) = RemainingChars
            Dim Level = 0
            While CurrentRemainingChars.HasValue
                Dim TokenResult = TokenParser.ReadToken(CurrentRemainingChars.Value)
                If Not TokenResult.Token.HasValue Then
                    CurrentRemainingChars = TokenResult.RemainingChars
                    Exit While
                End If
                Dim Token = TokenResult.Token.Value
                Select Case Token._Tag
                    Case TokenTag.SingleLineLiteral, TokenTag.PreprocessDirective, TokenTag.FunctionDirective
                        l.Add(Token)
                        ParameterEnd = GetRange(Token).End
                    Case TokenTag.LeftParentheses
                        Level += 1
                        l.Add(Token)
                        ParameterEnd = GetRange(Token).End
                    Case TokenTag.RightParentheses
                        If Level = 0 Then Exit While
                        Level -= 1
                        l.Add(Token)
                        ParameterEnd = GetRange(Token).End
                    Case TokenTag.SingleLineComment
                        Exit While
                    Case Else
                        Throw New InvalidOperationException
                End Select
                CurrentRemainingChars = TokenResult.RemainingChars
            End While
            Dim ParameterRange As New TextRange With {.Start = ParametersStart, .End = ParameterEnd}
            Dim SingleLineComment = ParseSingleLineComment(CurrentRemainingChars)
            Dim Content = Mark(New FunctionContent With {.Lines = Text.GetLines(ChildLines).ToArray(), .IndentLevel = IndentLevel + 1}, ChildLines)
            Dim EndDirective = ParseEndDirective(EndLine)
            Dim F = Mark(New FunctionNodes With {.FunctionDirective = FunctionDirective, .Parameters = l.ToArray(), .SingleLineComment = SingleLineComment, .Content = Content, .EndDirective = EndDirective}, Lines)

            Dim RawFunctionCallParameters As RawFunctionCallParameters
            If IsTreeParameterFunction(FunctionDirective.Text) Then
                If Not RemainingChars.HasValue Then
                    RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(Opt(Of SingleLineNode).Empty), ParameterRange)
                Else
                    Dim SecondTokenResult = TokenParser.ReadToken(RemainingChars.Value)
                    If Not SecondTokenResult.Token.HasValue Then
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(Opt(Of SingleLineNode).Empty), ParameterRange)
                    Else
                        Dim SecondToken = SecondTokenResult.Token.Value
                        Dim SingleLineNodeResult = ParseSingleLineNode(SecondToken, SecondTokenResult.RemainingChars)
                        Dim SingleLineNode = SingleLineNodeResult.Value
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(SingleLineNode), ParameterRange)
                    End If
                End If
            ElseIf IsTableParameterFunction(FunctionDirective.Text) Then
                Dim Nodes As New List(Of TableLineNode)
                Dim CurrentRemainingCharsInTable As Opt(Of TextRange) = RemainingChars
                While CurrentRemainingCharsInTable.HasValue
                    Dim TokenResult = TokenParser.ReadToken(CurrentRemainingCharsInTable.Value)
                    If Not TokenResult.Token.HasValue Then Exit While
                    Dim Token = TokenResult.Token.Value
                    If Token.OnSingleLineComment Then
                        Exit While
                    End If
                    Dim TableLineNodeResult = ParseTableLineNode(Token, TokenResult.RemainingChars)
                    Nodes.Add(TableLineNodeResult.Value)
                    CurrentRemainingCharsInTable = TableLineNodeResult.RemainingChars
                End While
                RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTableParameters(Nodes.ToArray()), ParameterRange)
            Else
                RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTokenParameters(F.Parameters), ParameterRange)
            End If

            Dim RawFunctionCallContent As RawFunctionCallContent
            If IsTreeContentFunction(FunctionDirective.Text) Then
                Dim MultiNodesList = ParseMultiNodesList(ChildLines, IndentLevel + 1)
                RawFunctionCallContent = Mark(RawFunctionCallContent.CreateTreeContent(MultiNodesList), ChildLines)
            ElseIf IsTableContentFunction(FunctionDirective.Text) Then
                Dim Children As New List(Of TableLine)
                For Each Line In Text.GetLines(ChildLines)
                    Dim OptTableLine = ParseTableLine(Line)
                    If OptTableLine.HasValue Then
                        Children.Add(OptTableLine.Value)
                    End If
                Next
                RawFunctionCallContent = Mark(RawFunctionCallContent.CreateTableContent(Children.ToArray()), ChildLines)
            Else
                RawFunctionCallContent = Mark(RawFunctionCallContent.CreateLineContent(Content), ChildLines)
            End If

            RawFunctionCalls.Add(F, Mark(New RawFunctionCall With {.Name = FunctionDirective, .ReturnValueMode = FunctionCallReturnValueMode.MultipleNodes, .Parameters = RawFunctionCallParameters, .Content = RawFunctionCallContent}, Lines))

            Return F
        End Function

        Private Function ParseMultiLineNode(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As MultiLineNode
            Dim Head = Mark(New SingleLineLiteral With {.Text = FirstToken.SingleLineLiteral}, GetRange(FirstToken))
            Dim SingleLineComment = ParseSingleLineComment(RemainingChars)
            Dim Children = ParseMultiNodesList(ChildLines, IndentLevel + 1)
            Dim EndDirective = ParseEndDirective(EndLine)
            Return Mark(New MultiLineNode With {.Head = Head, .SingleLineComment = SingleLineComment, .Children = Children, .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseMultiLineLiteral(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As MultiLineLiteral
            Dim SingleLineComment = ParseSingleLineComment(RemainingChars)
            Dim ContentLines = Text.GetLines(ChildLines).Select(Function(cl) New String(cl.Text.Skip((IndentLevel + 1) * 4).ToArray()))
            Dim EndDirective = ParseEndDirective(EndLine)
            Dim ContentString As String
            If EndDirective.HasValue Then
                ContentString = String.Join(CrLf, ContentLines.ToArray())
            Else
                ContentString = String.Join(CrLf, ContentLines.Reverse.SkipWhile(Function(Line) Line = "").Reverse.ToArray())
            End If
            Dim Content = Mark(New FreeContent With {.Text = ContentString}, ChildLines)
            Return Mark(New MultiLineLiteral With {.SingleLineComment = SingleLineComment, .Content = Content, .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseMultiLineComment(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As Opt(Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As MultiLineComment
            Dim SingleLineComment = ParseSingleLineComment(RemainingChars)
            Dim ContentLines = Text.GetLines(ChildLines).Select(Function(cl) New String(cl.Text.Skip((IndentLevel + 1) * 4).ToArray()))
            Dim EndDirective = ParseEndDirective(EndLine)
            Dim ContentString As String
            If EndDirective.HasValue Then
                ContentString = String.Join(CrLf, ContentLines.ToArray())
            Else
                ContentString = String.Join(CrLf, ContentLines.Reverse.SkipWhile(Function(Line) Line = "").Reverse.ToArray())
            End If
            Dim Content = Mark(New FreeContent With {.Text = ContentString}, ChildLines)
            Return Mark(New MultiLineComment With {.SingleLineComment = SingleLineComment, .Content = Content, .EndDirective = EndDirective}, Lines)
        End Function

        Private Function ParseTableLine(ByVal Line As TextLine) As Opt(Of TableLine)
            If TokenParser.IsBlankLine(Line) Then Return Opt(Of TableLine).Empty
            Dim Nodes As New List(Of TableLineNode)
            Dim SingleLineComment = Opt(Of SingleLineComment).Empty
            Dim CurrentRemainingChars As Opt(Of TextRange) = Line.Range
            While CurrentRemainingChars.HasValue
                Dim TokenResult = TokenParser.ReadToken(CurrentRemainingChars.Value)
                If Not TokenResult.Token.HasValue Then Exit While
                Dim Token = TokenResult.Token.Value
                If Token.OnSingleLineComment Then
                    SingleLineComment = ParseSingleLineComment(CurrentRemainingChars)
                    Exit While
                End If
                Dim TableLineNodeResult = ParseTableLineNode(Token, TokenResult.RemainingChars)
                Nodes.Add(TableLineNodeResult.Value)
                CurrentRemainingChars = TableLineNodeResult.RemainingChars
            End While
            Return Mark(New TableLine With {.Nodes = Nodes.ToArray(), .SingleLineComment = SingleLineComment}, Line.Range)
        End Function

        Private Function ParseSingleLineNodeLine(ByVal Line As TextLine, ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As SingleLineNodeLine
            Dim SingleLineNodeResult = ParseSingleLineNode(FirstToken, RemainingChars)
            Dim SingleLineNode = SingleLineNodeResult.Value
            Dim SingleLineComment = ParseSingleLineComment(SingleLineNodeResult.RemainingChars)
            Return Mark(New SingleLineNodeLine With {.SingleLineNode = SingleLineNode, .SingleLineComment = SingleLineComment}, Line.Range)
        End Function

        Private Class SyntaxParseResult(Of T)
            Public Value As T
            Public RemainingChars As Opt(Of TextRange)
        End Class
        Private Function ParseSingleLineNode(ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As SyntaxParseResult(Of SingleLineNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStart = FirstTokenRange.Start
            Dim NodeEnd = FirstTokenRange.End
            Dim CreateRange = Function() New TextRange With {.Start = NodeStart, .End = NodeEnd}

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral
                    Dim Head = Mark(New SingleLineLiteral With {.Text = FirstToken.SingleLineLiteral}, FirstTokenRange)
                    If Not RemainingChars.HasValue Then
                        Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = RemainingChars}
                    End If
                    Dim CurrentRemainingChars As Opt(Of TextRange) = RemainingChars
                    Dim l As New List(Of ParenthesesNode)
                    While True
                        If Not CurrentRemainingChars.HasValue Then
                            If l.Count = 0 Then
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = CurrentRemainingChars}
                            Else
                                Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l.ToArray(), .LastChild = Opt(Of SingleLineNode).Empty}, CreateRange())
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = CurrentRemainingChars}
                            End If
                        End If
                        Dim FollowingTokenResult = TokenParser.ReadToken(CurrentRemainingChars.Value)
                        If Not FollowingTokenResult.Token.HasValue Then
                            If l.Count = 0 Then
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = FollowingTokenResult.RemainingChars}
                            Else
                                Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l.ToArray(), .LastChild = Opt(Of SingleLineNode).Empty}, CreateRange())
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = FollowingTokenResult.RemainingChars}
                            End If
                        End If
                        Dim FollowingToken = FollowingTokenResult.Token.Value
                        Select Case FollowingToken._Tag
                            Case TokenTag.SingleLineLiteral, TokenTag.PreprocessDirective, TokenTag.FunctionDirective
                                Dim ChildResult = ParseSingleLineNode(FollowingToken, FollowingTokenResult.RemainingChars)
                                Dim Child = ChildResult.Value
                                NodeEnd = GetRange(FollowingToken).End
                                Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l.ToArray(), .LastChild = Child}, CreateRange())
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = ChildResult.RemainingChars}
                            Case TokenTag.LeftParentheses
                                Dim ChildResult = ParseParenthesesNode(FollowingToken, FollowingTokenResult.RemainingChars)
                                Dim Child = ChildResult.Value
                                l.Add(Child)
                                CurrentRemainingChars = ChildResult.RemainingChars
                                NodeEnd = GetRange(FollowingToken).End
                            Case TokenTag.RightParentheses, TokenTag.SingleLineComment
                                If l.Count = 0 Then
                                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = CurrentRemainingChars}
                                Else
                                    Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l.ToArray(), .LastChild = Opt(Of SingleLineNode).Empty}, CreateRange())
                                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = CurrentRemainingChars}
                                End If
                            Case Else
                                Throw New InvalidOperationException
                        End Select
                    End While
                    Throw New InvalidOperationException
                Case TokenTag.LeftParentheses
                    Dim ParenthesesNodeResult = ParseParenthesesNode(FirstToken, RemainingChars)
                    Dim ParenthesesNode = ParenthesesNodeResult.Value
                    NodeEnd = GetRange(ParenthesesNode).End
                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateParenthesesNode(ParenthesesNode), CreateRange()), .RemainingChars = ParenthesesNodeResult.RemainingChars}
                Case TokenTag.RightParentheses
                    Throw New InvalidSynaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.PreprocessDirective
                    If FirstToken.PreprocessDirective = "Empty" Then
                        Dim EmptyNode = Mark(New EmptyNode, FirstTokenRange)
                        Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateEmptyNode(EmptyNode), FirstTokenRange), .RemainingChars = RemainingChars}
                    End If
                    Throw New InvalidSynaxRuleException("InvalidPreprocessDirective", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.FunctionDirective
                    Dim SingleLineFunctionNodeResult = ParseSingleLineFunctionNode(FirstToken, RemainingChars)
                    Dim SingleLineFunctionNode = SingleLineFunctionNodeResult.Value
                    NodeEnd = GetRange(SingleLineFunctionNode).End
                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineFunctionNode(SingleLineFunctionNode), CreateRange()), .RemainingChars = SingleLineFunctionNodeResult.RemainingChars}
                Case TokenTag.SingleLineComment
                    Throw New InvalidSynaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

        Private Function ParseTableLineNode(ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As SyntaxParseResult(Of TableLineNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStart = FirstTokenRange.Start
            Dim NodeEnd = FirstTokenRange.End
            Dim CreateRange = Function() New TextRange With {.Start = NodeStart, .End = NodeEnd}

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral
                    Dim Head = Mark(New SingleLineLiteral With {.Text = FirstToken.SingleLineLiteral}, GetRange(FirstToken))
                    Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = RemainingChars}
                Case TokenTag.LeftParentheses
                    Dim ParenthesesNodeResult = ParseParenthesesNode(FirstToken, RemainingChars)
                    Dim ParenthesesNode = ParenthesesNodeResult.Value
                    Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateParenthesesNode(ParenthesesNode), CreateRange()), .RemainingChars = ParenthesesNodeResult.RemainingChars}
                Case TokenTag.RightParentheses
                    Throw New InvalidSynaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.PreprocessDirective
                    If FirstToken.PreprocessDirective = "Empty" Then
                        Dim EmptyNode = Mark(New EmptyNode, FirstTokenRange)
                        Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateEmptyNode(EmptyNode), FirstTokenRange), .RemainingChars = RemainingChars}
                    End If
                    Throw New InvalidSynaxRuleException("InvalidPreprocessDirective", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.FunctionDirective
                    Dim SingleLineFunctionNodeResult = ParseSingleLineFunctionNode(FirstToken, RemainingChars)
                    Dim SingleLineFunctionNode = SingleLineFunctionNodeResult.Value
                    Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateSingleLineFunctionNode(SingleLineFunctionNode), CreateRange()), .RemainingChars = SingleLineFunctionNodeResult.RemainingChars}
                Case TokenTag.SingleLineComment
                    Throw New InvalidSynaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

        Private Function ParseSingleLineFunctionNode(ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As SyntaxParseResult(Of SingleLineFunctionNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStart = FirstTokenRange.Start
            Dim NodeEnd = FirstTokenRange.End
            Dim CreateRange = Function() New TextRange With {.Start = NodeStart, .End = NodeEnd}

            Dim FunctionDirective = Mark(New FunctionDirective With {.Text = FirstToken.FunctionDirective}, GetRange(FirstToken))
            Dim l As New List(Of Token)
            Dim ParametersStart As TextPosition
            If RemainingChars.HasValue Then
                ParametersStart = RemainingChars.Value.Start
            Else
                ParametersStart = GetRange(FirstToken).End
            End If
            Dim ParameterEnd = ParametersStart
            Dim CurrentRemainingChars As Opt(Of TextRange) = RemainingChars
            Dim Level = 0
            While CurrentRemainingChars.HasValue
                Dim TokenResult = TokenParser.ReadToken(CurrentRemainingChars.Value)
                If Not TokenResult.Token.HasValue Then
                    CurrentRemainingChars = TokenResult.RemainingChars
                    Exit While
                End If
                Dim Token = TokenResult.Token.Value
                Select Case Token._Tag
                    Case TokenTag.SingleLineLiteral, TokenTag.PreprocessDirective, TokenTag.FunctionDirective
                        l.Add(Token)
                        ParameterEnd = GetRange(Token).End
                    Case TokenTag.LeftParentheses
                        Level += 1
                        l.Add(Token)
                        ParameterEnd = GetRange(Token).End
                    Case TokenTag.RightParentheses
                        If Level = 0 Then Exit While
                        Level -= 1
                        l.Add(Token)
                        ParameterEnd = GetRange(Token).End
                    Case TokenTag.SingleLineComment
                        Exit While
                    Case Else
                        Throw New InvalidOperationException
                End Select
                CurrentRemainingChars = TokenResult.RemainingChars
            End While
            Dim ParameterRange As New TextRange With {.Start = ParametersStart, .End = ParameterEnd}
            Dim FunctionRange = CreateRange()
            Dim F = Mark(New SingleLineFunctionNode With {.FunctionDirective = FunctionDirective, .Parameters = l.ToArray()}, FunctionRange)

            Dim RawFunctionCallParameters As RawFunctionCallParameters
            If IsTreeParameterFunction(FunctionDirective.Text) Then
                If Not RemainingChars.HasValue Then
                    RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(Opt(Of SingleLineNode).Empty), ParameterRange)
                Else
                    Dim SecondTokenResult = TokenParser.ReadToken(RemainingChars.Value)
                    If Not SecondTokenResult.Token.HasValue Then
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(Opt(Of SingleLineNode).Empty), ParameterRange)
                    Else
                        Dim SecondToken = SecondTokenResult.Token.Value
                        Dim SingleLineNodeResult = ParseSingleLineNode(SecondToken, SecondTokenResult.RemainingChars)
                        Dim SingleLineNode = SingleLineNodeResult.Value
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(SingleLineNode), ParameterRange)
                    End If
                End If
            ElseIf IsTableParameterFunction(FunctionDirective.Text) Then
                Dim Nodes As New List(Of TableLineNode)
                Dim CurrentRemainingCharsInTable As Opt(Of TextRange) = RemainingChars
                While CurrentRemainingCharsInTable.HasValue
                    Dim TokenResult = TokenParser.ReadToken(CurrentRemainingCharsInTable.Value)
                    If Not TokenResult.Token.HasValue Then Exit While
                    Dim Token = TokenResult.Token.Value
                    If Token.OnSingleLineComment Then
                        Exit While
                    End If
                    Dim TableLineNodeResult = ParseTableLineNode(Token, TokenResult.RemainingChars)
                    Nodes.Add(TableLineNodeResult.Value)
                    CurrentRemainingCharsInTable = TableLineNodeResult.RemainingChars
                End While
                RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTableParameters(Nodes.ToArray()), ParameterRange)
            Else
                RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTokenParameters(F.Parameters), ParameterRange)
            End If

            RawFunctionCalls.Add(F, Mark(New RawFunctionCall With {.Name = FunctionDirective, .ReturnValueMode = FunctionCallReturnValueMode.SingleNode, .Parameters = RawFunctionCallParameters, .Content = Opt(Of RawFunctionCallContent).Empty}, FunctionRange))

            Return New SyntaxParseResult(Of SingleLineFunctionNode) With {.Value = F, .RemainingChars = CurrentRemainingChars}
        End Function

        Private Function ParseParenthesesNode(ByVal FirstToken As Token, ByVal RemainingChars As Opt(Of TextRange)) As SyntaxParseResult(Of ParenthesesNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStart = FirstTokenRange.Start
            Dim NodeEnd = FirstTokenRange.End
            Dim CreateRange = Function() New TextRange With {.Start = NodeStart, .End = NodeEnd}

            If Not RemainingChars.HasValue Then Throw New InvalidSynaxRuleException("ParenthesesNotMatched", GetFileRange(FirstToken), FirstToken)
            Dim SecondTokenResult = TokenParser.ReadToken(RemainingChars.Value)
            If Not SecondTokenResult.Token.HasValue Then Throw New InvalidSynaxRuleException("ParenthesesNotMatched", GetFileRange(FirstToken), FirstToken)
            Dim SecondToken = SecondTokenResult.Token.Value
            NodeEnd = GetRange(SecondToken).End
            Dim SingleLineNodeResult = ParseSingleLineNode(SecondToken, SecondTokenResult.RemainingChars)
            If Not SingleLineNodeResult.RemainingChars.HasValue Then Throw New InvalidSynaxRuleException("ParenthesesNotMatched", New FileTextRange With {.Path = Path, .Range = CreateRange()}, FirstToken)
            Dim SingleLineNode = SingleLineNodeResult.Value
            NodeEnd = GetRange(SingleLineNode).End
            Dim EndTokenResult = TokenParser.ReadToken(SingleLineNodeResult.RemainingChars.Value)
            If Not EndTokenResult.Token.HasValue Then Throw New InvalidSynaxRuleException("ParenthesesNotMatched", New FileTextRange With {.Path = Path, .Range = CreateRange()}, FirstToken)
            Dim EndToken = EndTokenResult.Token.Value
            If Not EndToken.OnRightParentheses Then Throw New InvalidSynaxRuleException("ParenthesesNotMatched", GetFileRange(EndToken), EndToken)
            NodeEnd = GetRange(EndToken).End
            Return New SyntaxParseResult(Of ParenthesesNode) With {.Value = Mark(New ParenthesesNode With {.SingleLineNode = SingleLineNode}, CreateRange()), .RemainingChars = EndTokenResult.RemainingChars}
        End Function

        Private Function ParseSingleLineComment(ByVal RemainingChars As Opt(Of TextRange)) As Opt(Of SingleLineComment)
            If Not RemainingChars.HasValue Then Return Opt(Of SingleLineComment).Empty

            Dim TokenResult = TokenParser.ReadToken(RemainingChars.Value)
            If Not TokenResult.Token.HasValue Then Return Opt(Of SingleLineComment).Empty

            If TokenResult.RemainingChars.HasValue Then Throw New InvalidOperationException

            Dim Token = TokenResult.Token.Value
            If Not Token.OnSingleLineComment Then Throw New InvalidSynaxRuleException("UnexpectedToken", GetFileRange(Token), Token)
            Dim Content = Mark(New FreeContent With {.Text = Token.SingleLineComment}, RemainingChars.Value)
            Return Mark(New SingleLineComment With {.Content = Content}, RemainingChars.Value)
        End Function
        Private Function ParseEndDirective(ByVal Line As Opt(Of TextLine)) As Opt(Of EndDirective)
            If Not Line.HasValue Then Return Opt(Of EndDirective).Empty
            Dim LineValue = Line.Value
            Dim EndTokenResult = TokenParser.ReadToken(LineValue.Range)
            If Not EndTokenResult.Token.HasValue Then Throw New InvalidOperationException
            If Not EndTokenResult.Token.Value.OnPreprocessDirective Then Throw New InvalidOperationException
            If EndTokenResult.Token.Value.PreprocessDirective <> "End" Then Throw New InvalidOperationException
            Dim SingleLineComment = ParseSingleLineComment(EndTokenResult.RemainingChars)
            Return Mark(New EndDirective With {.EndSingleLineComment = SingleLineComment}, LineValue.Range)
        End Function
    End Class
End Namespace

Namespace Texting.TreeFormat.Syntax
    Friend Module Collections
        <Extension()>
        Public Function BinarySearch(Of T)(ByVal l As IList(Of T), ByVal CompareWithKey As Func(Of T, Integer)) As T
            Dim Min = 0
            Dim Max = l.Count
            Dim Index As Integer = 0
            While Min < Max
                Dim Mid As Integer = (Max + Min) \ 2
                Dim MidItem As T = l(Mid)
                Dim Comp As Integer = CompareWithKey(MidItem)
                If Comp < 0 Then
                    Min = Mid + 1
                ElseIf Comp > 0 Then
                    Max = Mid - 1
                Else
                    Return MidItem
                End If
            End While
            If Min = Max AndAlso CompareWithKey(l(Min)) = 0 Then
                Return l(Min)
            End If
            Throw New InvalidOperationException
        End Function

        <Extension()>
        Public Function BinarySearch(Of T, TKey As IComparable(Of TKey))(ByVal l As IList(Of T), ByVal KeySelector As Func(Of T, TKey), ByVal Key As TKey) As T
            Return BinarySearch(l, Function(Left) KeySelector(Left).CompareTo(Key))
        End Function
    End Module

    Partial Public Class Text
        Public Function GetTextLine(ByVal Row As Integer) As TextLine
            Return Lines(Row - 1)
        End Function
        Public Function GetPosition(ByVal CharIndex As Integer) As TextPosition
            Dim Line = Lines.BinarySearch(
                Function(l As TextLine)
                    If l.Range.End.CharIndex < CharIndex Then Return -1
                    If l.Range.Start.CharIndex > CharIndex Then Return 1
                    Return 0
                End Function
            )
            Dim ColumnIndex = CharIndex - Line.Range.Start.CharIndex
            Return New TextPosition With {.CharIndex = CharIndex, .Row = Line.Range.Start.Row, .Column = Line.Range.Start.Column + ColumnIndex}
        End Function
        Public Function GetTextInLine(ByVal Range As TextRange) As String
            If Range.Start.Row <> Range.End.Row Then Throw New ArgumentException
            Dim Line = Lines(Range.Start.Row - 1)
            Return Line.Text.Substring(Range.Start.Column - Line.Range.Start.Column, Range.End.Column - Range.Start.Column)
        End Function
        Public Function Calc(ByVal p As TextPosition, ByVal Offset As Integer) As TextPosition
            Return GetPosition(p.CharIndex + Offset)
        End Function
        Public Function GetLines(ByVal Range As TextLineRange) As IEnumerable(Of TextLine)
            Return Lines.Skip(Range.StartRow - 1).Take(Range.EndRow - Range.StartRow)
        End Function
    End Class
End Namespace
