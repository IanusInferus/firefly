'==========================================================================
'
'  File:        SyntaxParser.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文法解析器 - 用于从符号转到文法树
'  Version:     2019.04.28.
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
        Public Text As Text

        ''' <summary>Token | SyntaxRule => Range</summary>
        Public Positions As Dictionary(Of Object, TextRange)

        ''' <summary>SingleLineFunctionNode | FunctionNodes => RawFunctionCall</summary>
        Public RawFunctionCalls As Dictionary(Of Object, RawFunctionCall)
    End Class

    Public Class TreeFormatSyntaxParser
        Public Property Text As Text

        Private IsTreeParameterFunction As Func(Of String, Boolean) = Function(f) False
        Private IsTableParameterFunction As Func(Of String, Boolean) = Function(f) False
        Private IsTreeContentFunction As Func(Of String, Boolean) = Function(f) False
        Private IsTableContentFunction As Func(Of String, Boolean) = Function(f) False

        Private Positions As New Dictionary(Of Object, TextRange)
        Private FilePositions As New Dictionary(Of Object, FileTextRange)
        Private RawFunctionCalls As New Dictionary(Of Object, RawFunctionCall)

        Public Sub New(ByVal Text As Text)
            Me.New(New TreeFormatParseSetting, Text)
        End Sub
        Public Sub New(ByVal Setting As TreeFormatParseSetting, ByVal Text As Text)
            Me.Text = Text
            Me.IsTreeParameterFunction = Setting.IsTreeParameterFunction
            Me.IsTableParameterFunction = Setting.IsTableParameterFunction
            Me.IsTreeContentFunction = Setting.IsTreeContentFunction
            Me.IsTableContentFunction = Setting.IsTableContentFunction
        End Sub

        Public Function Parse() As TreeFormatParseResult
            Dim Lines = New TextLineRange With {.StartRow = 1, .EndRow = Text.Lines.Count + 1}
            Dim MultiNodesList = ParseMultiNodesList(Lines, 0)
            Dim Forest = Mark(New Forest With {.MultiNodesList = MultiNodesList}, Lines)
            Return New TreeFormatParseResult With {.Value = Forest, .Text = Text, .Positions = Positions, .RawFunctionCalls = RawFunctionCalls}
        End Function

        Private Function GetRange(ByVal Obj As Object) As [Optional](Of TextRange)
            If Not Positions.ContainsKey(Obj) Then Return [Optional](Of TextRange).Empty
            Return Positions(Obj)
        End Function
        Private Function GetFileRange(ByVal Obj As Object) As [Optional](Of FileTextRange)
            If FilePositions.ContainsKey(Obj) Then Return FilePositions(Obj)
            If Not Positions.ContainsKey(Obj) Then Return [Optional](Of FileTextRange).Empty
            Dim fp = New FileTextRange With {.Text = Text, .Range = Positions(Obj)}
            FilePositions.Add(Obj, fp)
            Return fp
        End Function
        Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As [Optional](Of TextRange)) As T
            If Range.OnSome Then
                Positions.Add(Obj, Range.Value)
            End If
            Return Obj
        End Function
        Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As TextLineRange) As T
            Dim Start = Text.GetTextLine(Range.StartRow).Range.Start
            Dim [End] As TextPosition
            If Range.EndRow >= Text.Lines.Count Then
                [End] = Text.GetTextLine(Range.EndRow - 1).Range.End
            Else
                [End] = Text.GetTextLine(Range.EndRow).Range.Start
            End If
            Return Mark(Obj, New TextRange With {.Start = Start, .End = [End]})
        End Function

        Private Function ParseMultiNodesList(ByVal Lines As TextLineRange, ByVal IndentLevel As Integer) As List(Of MultiNodes)
            Dim l As New List(Of MultiNodes)
            Dim RemainingLines As [Optional](Of TextLineRange) = Lines
            While RemainingLines.OnSome
                Dim Result = ReadMultiNodes(RemainingLines.Value, IndentLevel)
                If Result.MultiNodes.OnSome Then l.Add(Result.MultiNodes.Value)
                RemainingLines = Result.RemainingLines
            End While
            Return l
        End Function

        Private Class MultiNodesParseResult
            Public MultiNodes As [Optional](Of MultiNodes)
            Public RemainingLines As [Optional](Of TextLineRange)
        End Class
        Private Function ReadMultiNodes(ByVal Lines As TextLineRange, ByVal IndentLevel As Integer) As MultiNodesParseResult
            Dim NullNode = [Optional](Of MultiNodes).Empty
            Dim NullRemainingLines = [Optional](Of TextLineRange).Empty

            Dim FirstLineIndex = Lines.StartRow
            While True
                If FirstLineIndex >= Lines.EndRow Then Return New MultiNodesParseResult With {.MultiNodes = NullNode, .RemainingLines = NullRemainingLines}
                If Not TreeFormatTokenParser.IsBlankLine(Text.GetTextLine(FirstLineIndex)) Then Exit While
                FirstLineIndex += 1
            End While

            Dim FirstLine = Text.GetTextLine(FirstLineIndex)
            If Not TreeFormatTokenParser.IsExactFitIndentLevel(FirstLine, IndentLevel) Then Throw New InvalidTokenException("InvaildIndentLevel", New FileTextRange With {.Text = Text, .Range = FirstLine.Range}, FirstLine.Text)

            Dim ChildLines As [Optional](Of TextLineRange) = Nothing
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
                If Not TreeFormatTokenParser.IsBlankLine(ChildCurrentLine) Then
                    If TreeFormatTokenParser.IsExactFitIndentLevel(ChildCurrentLine, IndentLevel) Then
                        ChildLines = New TextLineRange With {.StartRow = ChildStartLineIndex, .EndRow = ChildEndLineIndex}
                        Exit While
                    End If
                    If Not TreeFormatTokenParser.IsFitIndentLevel(ChildCurrentLine, IndentLevel + 1) Then
                        Throw New InvalidTokenException("InvaildIndentLevel", New FileTextRange With {.Text = Text, .Range = ChildCurrentLine.Range}, ChildCurrentLine.Text)
                    End If
                End If
                ChildEndLineIndex += 1
            End While
            If Not ChildLines.OnSome Then Throw New InvalidOperationException

            Dim EndLineIndex = ChildEndLineIndex
            Dim EndLine = [Optional](Of TextLine).Empty

            '如果最后有$End预处理指令，则将其包含到
            If ChildEndLineIndex < Lines.EndRow Then
                Dim CurrentLine = Text.GetTextLine(ChildEndLineIndex)
                Dim FirstToken = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentLine.Range)
                If FirstToken.OnSome Then
                    If FirstToken.Value.Token.OnPreprocessDirective Then
                        If FirstToken.Value.Token.PreprocessDirective = "End" Then
                            EndLineIndex += 1
                            EndLine = CurrentLine
                        End If
                    End If
                End If
            End If

            '获得剩余行数
            Dim RemainingLines As [Optional](Of TextLineRange)
            If EndLineIndex >= Lines.EndRow Then
                RemainingLines = NullRemainingLines
            Else
                RemainingLines = New TextLineRange With {.StartRow = EndLineIndex, .EndRow = Lines.EndRow}
            End If

            Dim MultiNodesLines As New TextLineRange With {.StartRow = FirstLineIndex, .EndRow = EndLineIndex}
            Dim MultiNodes = ParseMultiNodes(MultiNodesLines, FirstLine, ChildLines.Value, EndLine, IndentLevel)
            Return New MultiNodesParseResult With {.MultiNodes = MultiNodes, .RemainingLines = RemainingLines}
        End Function
        Private Function ParseMultiNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer) As MultiNodes
            Dim FirstTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, FirstLine.Range)
            If Not FirstTokenResult.OnSome Then Throw New InvalidOperationException
            Dim FirstToken = FirstTokenResult.Value.Token
            Dim RemainingChars = FirstTokenResult.Value.RemainingChars

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral, TokenTag.LeftParenthesis, TokenTag.RightParenthesis, TokenTag.SingleLineComment
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
        Private Function ParseNode(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As Node
            Dim FirstTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, FirstLine.Range)
            If Not FirstTokenResult.OnSome Then Throw New InvalidOperationException
            Dim FirstTokenRange = GetRange(FirstToken)

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral
                    If Not EndLine.OnSome AndAlso (ChildLines.StartRow >= ChildLines.EndRow OrElse Text.GetLines(ChildLines).All(Function(Line) TreeFormatTokenParser.IsBlankLine(Line))) Then
                        Return Mark(Node.CreateSingleLineNodeLine(ParseSingleLineNodeLine(FirstLine, FirstToken, RemainingChars)), FirstLine.Range)
                    End If
                    Return Mark(Node.CreateMultiLineNode(ParseMultiLineNode(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)), Lines)
                Case TokenTag.LeftParenthesis, TokenTag.RightParenthesis
                    Return Mark(Node.CreateSingleLineNodeLine(ParseSingleLineNodeLine(FirstLine, FirstToken, RemainingChars)), FirstLine.Range)
                Case TokenTag.PreprocessDirective
                    Select Case FirstToken.PreprocessDirective
                        Case "String"
                            Return Mark(Node.CreateMultiLineLiteral(ParseMultiLineLiteral(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)), Lines)
                        Case "Comment"
                            Return Mark(Node.CreateMultiLineComment(ParseMultiLineComment(Lines, FirstLine, ChildLines, EndLine, IndentLevel, FirstToken, RemainingChars)), Lines)
                        Case "Empty"
                            If EndLine.OnSome OrElse (ChildLines.StartRow < ChildLines.EndRow AndAlso Text.GetLines(ChildLines).Any(Function(Line) Not TreeFormatTokenParser.IsBlankLine(Line))) Then
                                Throw New InvalidSyntaxRuleException("InvalidEmptyDirective", GetFileRange(FirstToken), FirstToken)
                            End If
                            Return Mark(Node.CreateSingleLineNodeLine(ParseSingleLineNodeLine(FirstLine, FirstToken, RemainingChars)), FirstLine.Range)
                        Case Else
                            Throw New InvalidSyntaxRuleException("InvalidPreprocessDirective", GetFileRange(FirstToken), FirstToken)
                    End Select
                Case TokenTag.FunctionDirective
                    Throw New InvalidOperationException
                Case TokenTag.SingleLineComment
                    If FirstTokenResult.Value.RemainingChars.OnSome Then Throw New InvalidOperationException
                    Dim FreeContent = Mark(New FreeContent With {.Text = FirstToken.SingleLineComment}, FirstTokenRange)
                    Dim SingleLineComment = Mark(New SingleLineComment With {.Content = FreeContent}, FirstTokenRange)
                    Return Mark(Node.CreateSingleLineComment(SingleLineComment), FirstTokenRange)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Private Function ParseListNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As ListNodes
            If Not RemainingChars.OnSome Then Throw New InvalidSyntaxRuleException("ListChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, RemainingChars.Value)
            If Not SecondTokenResult.OnSome Then Throw New InvalidSyntaxRuleException("ListChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondToken = SecondTokenResult.Value.Token
            If Not SecondToken.OnSingleLineLiteral Then Throw New InvalidSyntaxRuleException("ListChildHeadExpected", GetFileRange(SecondToken), SecondToken)
            Dim ChildHead = Mark(New SingleLineLiteral With {.Text = SecondToken.SingleLineLiteral}, GetRange(SecondToken))
            Dim SingleLineComment = ParseSingleLineComment(SecondTokenResult.Value.RemainingChars)
            Dim Children = ParseMultiNodesList(ChildLines, IndentLevel + 1)
            Dim EndDirective = ParseEndDirective(EndLine)
            Return Mark(New ListNodes With {.ChildHead = ChildHead, .SingleLineComment = SingleLineComment, .Children = Children, .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseTableNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As TableNodes
            If Not RemainingChars.OnSome Then Throw New InvalidSyntaxRuleException("TableChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, RemainingChars.Value)
            If Not SecondTokenResult.OnSome Then Throw New InvalidSyntaxRuleException("TableChildHeadNotExist", GetFileRange(FirstToken), FirstToken)
            Dim SecondToken = SecondTokenResult.Value.Token
            If Not SecondToken.OnSingleLineLiteral Then Throw New InvalidSyntaxRuleException("TableChildHeadExpected", GetFileRange(SecondToken), SecondToken)
            Dim ChildHead = Mark(New SingleLineLiteral With {.Text = SecondToken.SingleLineLiteral}, GetRange(SecondToken))
            Dim ChildFields As New List(Of SingleLineLiteral)
            Dim CurrentRemainingChars As [Optional](Of TextRange) = SecondTokenResult.Value.RemainingChars
            Dim l As New List(Of SingleLineLiteral)
            While CurrentRemainingChars.OnSome
                Dim ChildHeadResult = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentRemainingChars.Value)
                If Not ChildHeadResult.OnSome Then
                    CurrentRemainingChars = ChildHeadResult.Value.RemainingChars
                    Exit While
                End If
                Dim ChildHeadToken = ChildHeadResult.Value.Token
                Select Case ChildHeadToken._Tag
                    Case TokenTag.SingleLineLiteral
                        Dim FieldHead = Mark(New SingleLineLiteral With {.Text = ChildHeadToken.SingleLineLiteral}, GetRange(ChildHeadToken))
                        ChildFields.Add(FieldHead)
                        CurrentRemainingChars = ChildHeadResult.Value.RemainingChars
                    Case TokenTag.SingleLineComment
                        Exit While
                    Case Else
                        Throw New InvalidSyntaxRuleException("SingleLineLiteralOrSingleLineCommentExpected", GetFileRange(ChildHeadToken), ChildHeadToken)
                End Select
            End While
            Dim SingleLineComment = ParseSingleLineComment(CurrentRemainingChars)
            Dim Children As New List(Of TableLine)
            For Each Line In Text.GetLines(ChildLines)
                Dim OptTableLine = ParseTableLine(Line)
                If OptTableLine.OnSome Then
                    Children.Add(OptTableLine.Value)
                End If
            Next
            Dim EndDirective = ParseEndDirective(EndLine)
            Return Mark(New TableNodes With {.ChildHead = ChildHead, .ChildFields = ChildFields, .SingleLineComment = SingleLineComment, .Children = Children, .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseFunctionNodes(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As FunctionNodes
            Dim FunctionDirective = Mark(New FunctionDirective With {.Text = FirstToken.FunctionDirective}, GetRange(FirstToken))
            Dim l As New List(Of Token)
            Dim ParametersStart = [Optional](Of TextPosition).Empty
            If RemainingChars.OnSome Then
                ParametersStart = RemainingChars.Value.Start
            Else
                Dim TokenRange = GetRange(FirstToken)
                If TokenRange.OnSome Then
                    ParametersStart = TokenRange.Value.End
                End If
            End If
            Dim ParameterEnd = ParametersStart
            Dim CurrentRemainingChars As [Optional](Of TextRange) = RemainingChars
            Dim Level = 0
            While CurrentRemainingChars.OnSome
                Dim TokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentRemainingChars.Value)
                If Not TokenResult.OnSome Then
                    CurrentRemainingChars = [Optional](Of TextRange).Empty
                    Exit While
                End If
                Dim Token = TokenResult.Value.Token
                Select Case Token._Tag
                    Case TokenTag.SingleLineLiteral, TokenTag.PreprocessDirective, TokenTag.FunctionDirective
                        l.Add(Token)
                        Dim TokenRange = GetRange(Token)
                        If TokenRange.OnSome Then
                            ParametersStart = TokenRange.Value.End
                        End If
                    Case TokenTag.LeftParenthesis
                        Level += 1
                        l.Add(Token)
                        Dim TokenRange = GetRange(Token)
                        If TokenRange.OnSome Then
                            ParametersStart = TokenRange.Value.End
                        End If
                    Case TokenTag.RightParenthesis
                        If Level = 0 Then Exit While
                        Level -= 1
                        l.Add(Token)
                        Dim TokenRange = GetRange(Token)
                        If TokenRange.OnSome Then
                            ParametersStart = TokenRange.Value.End
                        End If
                    Case TokenTag.SingleLineComment
                        Exit While
                    Case Else
                        Throw New InvalidOperationException
                End Select
                CurrentRemainingChars = TokenResult.Value.RemainingChars
            End While
            Dim ParameterRange = [Optional](Of TextRange).Empty
            If ParametersStart.OnSome AndAlso ParameterEnd.OnSome Then
                ParameterRange = New TextRange With {.Start = ParametersStart.Value, .End = ParameterEnd.Value}
            End If
            Dim SingleLineComment = ParseSingleLineComment(CurrentRemainingChars)
            Dim EndDirective = ParseEndDirective(EndLine)
            Dim Content As FunctionContent
            If EndDirective.OnSome Then
                Content = Mark(New FunctionContent With {.Lines = Text.GetLines(ChildLines).ToList(), .IndentLevel = IndentLevel + 1}, ChildLines)
            Else
                Dim StartRow = ChildLines.StartRow
                Dim EndRow = ChildLines.EndRow
                While EndRow > StartRow AndAlso Text.Lines(EndRow - 2).Text.Where(Function(c) c <> " ").Count = 0
                    EndRow -= 1
                End While
                Dim cl = New TextLineRange With {.StartRow = StartRow, .EndRow = EndRow}
                Content = Mark(New FunctionContent With {.Lines = Text.GetLines(cl).ToList(), .IndentLevel = IndentLevel + 1}, cl)
            End If
            Dim F = Mark(New FunctionNodes With {.FunctionDirective = FunctionDirective, .Parameters = l, .SingleLineComment = SingleLineComment, .Content = Content, .EndDirective = EndDirective}, Lines)

            Dim RawFunctionCallParameters As RawFunctionCallParameters
            If IsTreeParameterFunction(FunctionDirective.Text) Then
                If Not RemainingChars.OnSome Then
                    RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter([Optional](Of SingleLineNode).Empty), ParameterRange)
                Else
                    Dim SecondTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, RemainingChars.Value)
                    If Not SecondTokenResult.OnSome Then
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter([Optional](Of SingleLineNode).Empty), ParameterRange)
                    Else
                        Dim SecondToken = SecondTokenResult.Value.Token
                        Dim SingleLineNodeResult = ParseSingleLineNode(SecondToken, SecondTokenResult.Value.RemainingChars)
                        Dim SingleLineNode = SingleLineNodeResult.Value
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(SingleLineNode), ParameterRange)
                    End If
                End If
            ElseIf IsTableParameterFunction(FunctionDirective.Text) Then
                Dim Nodes As New List(Of TableLineNode)
                Dim CurrentRemainingCharsInTable As [Optional](Of TextRange) = RemainingChars
                While CurrentRemainingCharsInTable.OnSome
                    Dim TokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentRemainingCharsInTable.Value)
                    If Not TokenResult.OnSome Then Exit While
                    Dim Token = TokenResult.Value.Token
                    If Token.OnSingleLineComment Then
                        Exit While
                    End If
                    Dim TableLineNodeResult = ParseTableLineNode(Token, TokenResult.Value.RemainingChars)
                    Nodes.Add(TableLineNodeResult.Value)
                    CurrentRemainingCharsInTable = TableLineNodeResult.RemainingChars
                End While
                RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTableParameters(Nodes), ParameterRange)
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
                    If OptTableLine.OnSome Then
                        Children.Add(OptTableLine.Value)
                    End If
                Next
                RawFunctionCallContent = Mark(RawFunctionCallContent.CreateTableContent(Children), ChildLines)
            Else
                RawFunctionCallContent = Mark(RawFunctionCallContent.CreateLineContent(Content), ChildLines)
            End If

            RawFunctionCalls.Add(F, Mark(New RawFunctionCall With {.Name = FunctionDirective, .ReturnValueMode = FunctionCallReturnValueMode.MultipleNodes, .Parameters = RawFunctionCallParameters, .Content = RawFunctionCallContent}, Lines))

            Return F
        End Function

        Private Function ParseMultiLineNode(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As MultiLineNode
            Dim Head = Mark(New SingleLineLiteral With {.Text = FirstToken.SingleLineLiteral}, GetRange(FirstToken))
            Dim SingleLineComment = ParseSingleLineComment(RemainingChars)
            Dim Children = ParseMultiNodesList(ChildLines, IndentLevel + 1)
            Dim EndDirective = ParseEndDirective(EndLine)
            Return Mark(New MultiLineNode With {.Head = Head, .SingleLineComment = SingleLineComment, .Children = Children, .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseMultiLineLiteral(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As MultiLineLiteral
            Dim SingleLineComment = ParseSingleLineComment(RemainingChars)
            Dim ContentLines = Text.GetLines(ChildLines).Select(Function(cl) New String(cl.Text.Skip((IndentLevel + 1) * 4).ToArray()))
            Dim EndDirective = ParseEndDirective(EndLine)
            Dim ContentString As String
            If EndDirective.OnSome Then
                ContentString = String.Join(CrLf, ContentLines)
            Else
                ContentString = String.Join(CrLf, ContentLines.Reverse().SkipWhile(Function(Line) Line = "").Reverse())
            End If
            Dim Content = Mark(New FreeContent With {.Text = ContentString}, ChildLines)
            Return Mark(New MultiLineLiteral With {.SingleLineComment = SingleLineComment, .Content = Content, .EndDirective = EndDirective}, Lines)
        End Function
        Private Function ParseMultiLineComment(ByVal Lines As TextLineRange, ByVal FirstLine As TextLine, ByVal ChildLines As TextLineRange, ByVal EndLine As [Optional](Of TextLine), ByVal IndentLevel As Integer, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As MultiLineComment
            Dim SingleLineComment = ParseSingleLineComment(RemainingChars)
            Dim ContentLines = Text.GetLines(ChildLines).Select(Function(cl) New String(cl.Text.Skip((IndentLevel + 1) * 4).ToArray()))
            Dim EndDirective = ParseEndDirective(EndLine)
            Dim ContentString As String
            If EndDirective.OnSome Then
                ContentString = String.Join(CrLf, ContentLines)
            Else
                ContentString = String.Join(CrLf, ContentLines.Reverse().SkipWhile(Function(Line) Line = "").Reverse())
            End If
            Dim Content = Mark(New FreeContent With {.Text = ContentString}, ChildLines)
            Return Mark(New MultiLineComment With {.SingleLineComment = SingleLineComment, .Content = Content, .EndDirective = EndDirective}, Lines)
        End Function

        Private Function ParseTableLine(ByVal Line As TextLine) As [Optional](Of TableLine)
            If TreeFormatTokenParser.IsBlankLine(Line) Then Return [Optional](Of TableLine).Empty
            Dim Nodes As New List(Of TableLineNode)
            Dim SingleLineComment = [Optional](Of SingleLineComment).Empty
            Dim CurrentRemainingChars As [Optional](Of TextRange) = Line.Range
            While CurrentRemainingChars.OnSome
                Dim TokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentRemainingChars.Value)
                If Not TokenResult.OnSome Then Exit While
                Dim Token = TokenResult.Value.Token
                If Token.OnSingleLineComment Then
                    SingleLineComment = ParseSingleLineComment(CurrentRemainingChars)
                    Exit While
                End If
                Dim TableLineNodeResult = ParseTableLineNode(Token, TokenResult.Value.RemainingChars)
                Nodes.Add(TableLineNodeResult.Value)
                CurrentRemainingChars = TableLineNodeResult.RemainingChars
            End While
            Return Mark(New TableLine With {.Nodes = Nodes, .SingleLineComment = SingleLineComment}, Line.Range)
        End Function

        Private Function ParseSingleLineNodeLine(ByVal Line As TextLine, ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As SingleLineNodeLine
            Dim SingleLineNodeResult = ParseSingleLineNode(FirstToken, RemainingChars)
            Dim SingleLineNode = SingleLineNodeResult.Value
            Dim SingleLineComment = ParseSingleLineComment(SingleLineNodeResult.RemainingChars)
            Return Mark(New SingleLineNodeLine With {.SingleLineNode = SingleLineNode, .SingleLineComment = SingleLineComment}, Line.Range)
        End Function

        Private Class SyntaxParseResult(Of T)
            Public Value As T
            Public RemainingChars As [Optional](Of TextRange)
        End Class
        Private Function ParseSingleLineNode(ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As SyntaxParseResult(Of SingleLineNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStartRange = FirstTokenRange
            Dim NodeEndRange = FirstTokenRange
            Dim CreateRange = Function() If(NodeStartRange.OnSome AndAlso NodeEndRange.OnSome, New TextRange With {.Start = NodeStartRange.Value.Start, .End = NodeEndRange.Value.End}, [Optional](Of TextRange).Empty)

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral
                    Dim Head = Mark(New SingleLineLiteral With {.Text = FirstToken.SingleLineLiteral}, FirstTokenRange)
                    If Not RemainingChars.OnSome Then
                        Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = RemainingChars}
                    End If
                    Dim CurrentRemainingChars As [Optional](Of TextRange) = RemainingChars
                    Dim l As New List(Of ParenthesisNode)
                    While True
                        If Not CurrentRemainingChars.OnSome Then
                            If l.Count = 0 Then
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = CurrentRemainingChars}
                            Else
                                Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l, .LastChild = [Optional](Of SingleLineNode).Empty}, CreateRange())
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = CurrentRemainingChars}
                            End If
                        End If
                        Dim FollowingTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentRemainingChars.Value)
                        If Not FollowingTokenResult.OnSome Then
                            If l.Count = 0 Then
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = [Optional](Of TextRange).Empty}
                            Else
                                Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l, .LastChild = [Optional](Of SingleLineNode).Empty}, CreateRange())
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = [Optional](Of TextRange).Empty}
                            End If
                        End If
                        Dim FollowingToken = FollowingTokenResult.Value.Token
                        Select Case FollowingToken._Tag
                            Case TokenTag.SingleLineLiteral, TokenTag.PreprocessDirective, TokenTag.FunctionDirective
                                Dim ChildResult = ParseSingleLineNode(FollowingToken, FollowingTokenResult.Value.RemainingChars)
                                Dim Child = ChildResult.Value
                                NodeEndRange = GetRange(FollowingToken)
                                Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l, .LastChild = Child}, CreateRange())
                                Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = ChildResult.RemainingChars}
                            Case TokenTag.LeftParenthesis
                                Dim ChildResult = ParseParenthesisNode(FollowingToken, FollowingTokenResult.Value.RemainingChars)
                                Dim Child = ChildResult.Value
                                l.Add(Child)
                                CurrentRemainingChars = ChildResult.RemainingChars
                                NodeEndRange = GetRange(FollowingToken)
                            Case TokenTag.RightParenthesis, TokenTag.SingleLineComment
                                If l.Count = 0 Then
                                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = CurrentRemainingChars}
                                Else
                                    Dim Node = Mark(New SingleLineNodeWithParameters With {.Head = Head, .Children = l, .LastChild = [Optional](Of SingleLineNode).Empty}, CreateRange())
                                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineNodeWithParameters(Node), CreateRange()), .RemainingChars = CurrentRemainingChars}
                                End If
                            Case Else
                                Throw New InvalidOperationException
                        End Select
                    End While
                    Throw New InvalidOperationException
                Case TokenTag.LeftParenthesis
                    Dim ParenthesisNodeResult = ParseParenthesisNode(FirstToken, RemainingChars)
                    Dim ParenthesisNode = ParenthesisNodeResult.Value
                    NodeEndRange = GetRange(ParenthesisNode)
                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateParenthesisNode(ParenthesisNode), CreateRange()), .RemainingChars = ParenthesisNodeResult.RemainingChars}
                Case TokenTag.RightParenthesis
                    Throw New InvalidSyntaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.PreprocessDirective
                    If FirstToken.PreprocessDirective = "Empty" Then
                        Dim EmptyNode = Mark(New EmptyNode, FirstTokenRange)
                        Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateEmptyNode(EmptyNode), FirstTokenRange), .RemainingChars = RemainingChars}
                    End If
                    Throw New InvalidSyntaxRuleException("InvalidPreprocessDirective", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.FunctionDirective
                    Dim SingleLineFunctionNodeResult = ParseSingleLineFunctionNode(FirstToken, RemainingChars)
                    Dim SingleLineFunctionNode = SingleLineFunctionNodeResult.Value
                    NodeEndRange = GetRange(SingleLineFunctionNode)
                    Return New SyntaxParseResult(Of SingleLineNode) With {.Value = Mark(SingleLineNode.CreateSingleLineFunctionNode(SingleLineFunctionNode), CreateRange()), .RemainingChars = SingleLineFunctionNodeResult.RemainingChars}
                Case TokenTag.SingleLineComment
                    Throw New InvalidSyntaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

        Private Function ParseTableLineNode(ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As SyntaxParseResult(Of TableLineNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStartRange = FirstTokenRange
            Dim NodeEndRange = FirstTokenRange
            Dim CreateRange = Function() If(NodeStartRange.OnSome AndAlso NodeEndRange.OnSome, New TextRange With {.Start = NodeStartRange.Value.Start, .End = NodeEndRange.Value.End}, [Optional](Of TextRange).Empty)

            Select Case FirstToken._Tag
                Case TokenTag.SingleLineLiteral
                    Dim Head = Mark(New SingleLineLiteral With {.Text = FirstToken.SingleLineLiteral}, GetRange(FirstToken))
                    Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateSingleLineLiteral(Head), CreateRange()), .RemainingChars = RemainingChars}
                Case TokenTag.LeftParenthesis
                    Dim ParenthesisNodeResult = ParseParenthesisNode(FirstToken, RemainingChars)
                    Dim ParenthesisNode = ParenthesisNodeResult.Value
                    Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateParenthesisNode(ParenthesisNode), CreateRange()), .RemainingChars = ParenthesisNodeResult.RemainingChars}
                Case TokenTag.RightParenthesis
                    Throw New InvalidSyntaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.PreprocessDirective
                    If FirstToken.PreprocessDirective = "Empty" Then
                        Dim EmptyNode = Mark(New EmptyNode, FirstTokenRange)
                        Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateEmptyNode(EmptyNode), FirstTokenRange), .RemainingChars = RemainingChars}
                    End If
                    Throw New InvalidSyntaxRuleException("InvalidPreprocessDirective", GetFileRange(FirstToken), FirstToken)
                Case TokenTag.FunctionDirective
                    Dim SingleLineFunctionNodeResult = ParseSingleLineFunctionNode(FirstToken, RemainingChars)
                    Dim SingleLineFunctionNode = SingleLineFunctionNodeResult.Value
                    Return New SyntaxParseResult(Of TableLineNode) With {.Value = Mark(TableLineNode.CreateSingleLineFunctionNode(SingleLineFunctionNode), CreateRange()), .RemainingChars = SingleLineFunctionNodeResult.RemainingChars}
                Case TokenTag.SingleLineComment
                    Throw New InvalidSyntaxRuleException("UnexpectedToken", GetFileRange(FirstToken), FirstToken)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

        Private Function ParseSingleLineFunctionNode(ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As SyntaxParseResult(Of SingleLineFunctionNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStartRange = FirstTokenRange
            Dim NodeEndRange = FirstTokenRange
            Dim CreateRange = Function() If(NodeStartRange.OnSome AndAlso NodeEndRange.OnSome, New TextRange With {.Start = NodeStartRange.Value.Start, .End = NodeEndRange.Value.End}, [Optional](Of TextRange).Empty)

            Dim FunctionDirective = Mark(New FunctionDirective With {.Text = FirstToken.FunctionDirective}, GetRange(FirstToken))
            Dim l As New List(Of Token)
            Dim ParametersStart = [Optional](Of TextPosition).Empty
            If RemainingChars.OnSome Then
                ParametersStart = RemainingChars.Value.Start
            Else
                Dim TokenRange = GetRange(FirstToken)
                If TokenRange.OnSome Then
                    ParametersStart = TokenRange.Value.End
                End If
            End If
            Dim ParameterEnd = ParametersStart
            Dim CurrentRemainingChars As [Optional](Of TextRange) = RemainingChars
            Dim Level = 0
            While CurrentRemainingChars.OnSome
                Dim TokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentRemainingChars.Value)
                If Not TokenResult.OnSome Then
                    CurrentRemainingChars = [Optional](Of TextRange).Empty
                    Exit While
                End If
                Dim Token = TokenResult.Value.Token
                Select Case Token._Tag
                    Case TokenTag.SingleLineLiteral, TokenTag.PreprocessDirective, TokenTag.FunctionDirective
                        l.Add(Token)
                        Dim TokenRange = GetRange(Token)
                        If TokenRange.OnSome Then
                            ParametersStart = TokenRange.Value.End
                        End If
                    Case TokenTag.LeftParenthesis
                        Level += 1
                        l.Add(Token)
                        Dim TokenRange = GetRange(Token)
                        If TokenRange.OnSome Then
                            ParametersStart = TokenRange.Value.End
                        End If
                    Case TokenTag.RightParenthesis
                        If Level = 0 Then Exit While
                        Level -= 1
                        l.Add(Token)
                        Dim TokenRange = GetRange(Token)
                        If TokenRange.OnSome Then
                            ParametersStart = TokenRange.Value.End
                        End If
                    Case TokenTag.SingleLineComment
                        Exit While
                    Case Else
                        Throw New InvalidOperationException
                End Select
                CurrentRemainingChars = TokenResult.Value.RemainingChars
            End While
            Dim ParameterRange = [Optional](Of TextRange).Empty
            If ParametersStart.OnSome AndAlso ParameterEnd.OnSome Then
                ParameterRange = New TextRange With {.Start = ParametersStart.Value, .End = ParameterEnd.Value}
            End If
            Dim FunctionRange = CreateRange()
            Dim F = Mark(New SingleLineFunctionNode With {.FunctionDirective = FunctionDirective, .Parameters = l}, FunctionRange)

            Dim RawFunctionCallParameters As RawFunctionCallParameters
            If IsTreeParameterFunction(FunctionDirective.Text) Then
                If Not RemainingChars.OnSome Then
                    RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter([Optional](Of SingleLineNode).Empty), ParameterRange)
                Else
                    Dim SecondTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, RemainingChars.Value)
                    If Not SecondTokenResult.OnSome Then
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter([Optional](Of SingleLineNode).Empty), ParameterRange)
                    Else
                        Dim SecondToken = SecondTokenResult.Value.Token
                        Dim SingleLineNodeResult = ParseSingleLineNode(SecondToken, SecondTokenResult.Value.RemainingChars)
                        Dim SingleLineNode = SingleLineNodeResult.Value
                        RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTreeParameter(SingleLineNode), ParameterRange)
                    End If
                End If
            ElseIf IsTableParameterFunction(FunctionDirective.Text) Then
                Dim Nodes As New List(Of TableLineNode)
                Dim CurrentRemainingCharsInTable As [Optional](Of TextRange) = RemainingChars
                While CurrentRemainingCharsInTable.OnSome
                    Dim TokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, CurrentRemainingCharsInTable.Value)
                    If Not TokenResult.OnSome Then Exit While
                    Dim Token = TokenResult.Value.Token
                    If Token.OnSingleLineComment Then
                        Exit While
                    End If
                    Dim TableLineNodeResult = ParseTableLineNode(Token, TokenResult.Value.RemainingChars)
                    Nodes.Add(TableLineNodeResult.Value)
                    CurrentRemainingCharsInTable = TableLineNodeResult.RemainingChars
                End While
                RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTableParameters(Nodes), ParameterRange)
            Else
                RawFunctionCallParameters = Mark(RawFunctionCallParameters.CreateTokenParameters(F.Parameters), ParameterRange)
            End If

            RawFunctionCalls.Add(F, Mark(New RawFunctionCall With {.Name = FunctionDirective, .ReturnValueMode = FunctionCallReturnValueMode.SingleNode, .Parameters = RawFunctionCallParameters, .Content = [Optional](Of RawFunctionCallContent).Empty}, FunctionRange))

            Return New SyntaxParseResult(Of SingleLineFunctionNode) With {.Value = F, .RemainingChars = CurrentRemainingChars}
        End Function

        Private Function ParseParenthesisNode(ByVal FirstToken As Token, ByVal RemainingChars As [Optional](Of TextRange)) As SyntaxParseResult(Of ParenthesisNode)
            Dim FirstTokenRange = GetRange(FirstToken)
            Dim NodeStartRange = FirstTokenRange
            Dim NodeEndRange = FirstTokenRange
            Dim CreateRange = Function() If(NodeStartRange.OnSome AndAlso NodeEndRange.OnSome, New TextRange With {.Start = NodeStartRange.Value.Start, .End = NodeEndRange.Value.End}, [Optional](Of TextRange).Empty)

            If Not RemainingChars.OnSome Then Throw New InvalidSyntaxRuleException("ParenthesisNotMatched", GetFileRange(FirstToken), FirstToken)
            Dim SecondTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, RemainingChars.Value)
            If Not SecondTokenResult.OnSome Then Throw New InvalidSyntaxRuleException("ParenthesisNotMatched", GetFileRange(FirstToken), FirstToken)
            Dim SecondToken = SecondTokenResult.Value.Token
            NodeEndRange = GetRange(SecondToken)
            Dim SingleLineNodeResult = ParseSingleLineNode(SecondToken, SecondTokenResult.Value.RemainingChars)
            If Not SingleLineNodeResult.RemainingChars.OnSome Then Throw New InvalidSyntaxRuleException("ParenthesisNotMatched", New FileTextRange With {.Text = Text, .Range = CreateRange()}, FirstToken)
            Dim SingleLineNode = SingleLineNodeResult.Value
            NodeEndRange = GetRange(SingleLineNode)
            Dim EndTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, SingleLineNodeResult.RemainingChars.Value)
            If Not EndTokenResult.OnSome Then Throw New InvalidSyntaxRuleException("ParenthesisNotMatched", New FileTextRange With {.Text = Text, .Range = CreateRange()}, FirstToken)
            Dim EndToken = EndTokenResult.Value.Token
            If Not EndToken.OnRightParenthesis Then Throw New InvalidSyntaxRuleException("ParenthesisNotMatched", GetFileRange(EndToken), EndToken)
            NodeEndRange = GetRange(EndToken)
            Return New SyntaxParseResult(Of ParenthesisNode) With {.Value = Mark(New ParenthesisNode With {.SingleLineNode = SingleLineNode}, CreateRange()), .RemainingChars = EndTokenResult.Value.RemainingChars}
        End Function

        Private Function ParseSingleLineComment(ByVal RemainingChars As [Optional](Of TextRange)) As [Optional](Of SingleLineComment)
            If Not RemainingChars.OnSome Then Return [Optional](Of SingleLineComment).Empty

            Dim TokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, RemainingChars.Value)
            If Not TokenResult.OnSome Then Return [Optional](Of SingleLineComment).Empty

            If TokenResult.Value.RemainingChars.OnSome Then Throw New InvalidTokenException("UnexpectedToken", New FileTextRange With {.Text = Text, .Range = TokenResult.Value.RemainingChars.Value}, Text.GetTextInLine(TokenResult.Value.RemainingChars.Value))

            Dim Token = TokenResult.Value.Token
            If Not Token.OnSingleLineComment Then Throw New InvalidSyntaxRuleException("UnexpectedToken", GetFileRange(Token), Token)
            Dim Content = Mark(New FreeContent With {.Text = Token.SingleLineComment}, RemainingChars.Value)
            Return Mark(New SingleLineComment With {.Content = Content}, RemainingChars.Value)
        End Function
        Private Function ParseEndDirective(ByVal Line As [Optional](Of TextLine)) As [Optional](Of EndDirective)
            If Not Line.OnSome Then Return [Optional](Of EndDirective).Empty
            Dim LineValue = Line.Value
            Dim EndTokenResult = TreeFormatTokenParser.ReadToken(Text, Positions, LineValue.Range)
            If Not EndTokenResult.OnSome Then Throw New InvalidOperationException
            If Not EndTokenResult.Value.Token.OnPreprocessDirective Then Throw New InvalidOperationException
            If EndTokenResult.Value.Token.PreprocessDirective <> "End" Then Throw New InvalidOperationException
            Dim SingleLineComment = ParseSingleLineComment(EndTokenResult.Value.RemainingChars)
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
            Dim Line = Lines(p.Row - 1)
            Dim CharIndex = p.CharIndex + Offset
            If CharIndex >= Line.Range.Start.CharIndex And CharIndex <= Line.Range.End.CharIndex Then
                Dim ColumnIndex = p.CharIndex + Offset - Line.Range.Start.CharIndex
                Return New TextPosition With {.CharIndex = CharIndex, .Row = Line.Range.Start.Row, .Column = Line.Range.Start.Column + ColumnIndex}
            End If
            Return GetPosition(p.CharIndex + Offset)
        End Function
        Public Function GetLines(ByVal Range As TextLineRange) As IEnumerable(Of TextLine)
            Return Lines.Skip(Range.StartRow - 1).Take(Range.EndRow - Range.StartRow)
        End Function
    End Class
End Namespace
