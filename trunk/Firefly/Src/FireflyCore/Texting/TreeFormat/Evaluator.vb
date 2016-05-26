'==========================================================================
'
'  File:        Evaluator.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 求值器 - 用于执行自定义函数，并将文法树转为语义树
'  Version:     2016.05.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Diagnostics
Imports Firefly.Texting.TreeFormat.Syntax

Namespace Texting.TreeFormat
    Public Interface ISyntaxMarker
        ReadOnly Property Text As Text
        Function GetRange(ByVal Obj As Object) As [Optional](Of TextRange)
        Function GetFileRange(ByVal Obj As Object) As [Optional](Of FileTextRange)
        Function Mark(Of T)(ByVal Obj As T, ByVal Range As [Optional](Of TextRange)) As T
        Function Mark(Of T)(ByVal Obj As T, ByVal SyntaxRule As Object) As T
    End Interface

    Public Interface ISemanticsNodeMaker
        ReadOnly Property Text As Text
        Function GetRange(ByVal Obj As Object) As [Optional](Of TextRange)
        Function GetFileRange(ByVal Obj As Object) As [Optional](Of FileTextRange)
        Function MakeEmptyNode(ByVal Range As [Optional](Of TextRange)) As Semantics.Node
        Function MakeLeafNode(ByVal Value As String, ByVal Range As [Optional](Of TextRange)) As Semantics.Node
        Function MakeStemNode(ByVal Name As String, ByVal Children As List(Of Semantics.Node), ByVal Range As [Optional](Of TextRange)) As Semantics.Node
        Function MakeEmptyNode(ByVal SyntaxRule As Object) As Semantics.Node
        Function MakeLeafNode(ByVal Value As String, ByVal SyntaxRule As Object) As Semantics.Node
        Function MakeStemNode(ByVal Name As String, ByVal Children As List(Of Semantics.Node), ByVal SyntaxRule As Object) As Semantics.Node
    End Interface

    Public Class TreeFormatEvaluateSetting
        Public TokenParameterEvaluator As Func(Of RawFunctionCall, ISyntaxMarker, List(Of Semantics.Node))
        Public FunctionCallEvaluator As Func(Of FunctionCall, ISemanticsNodeMaker, List(Of Semantics.Node))
    End Class

    Public Class TreeFormatEvaluator
        Implements ISyntaxMarker
        Implements ISemanticsNodeMaker

        Private pr As TreeFormatParseResult
        Private TokenParameterEvaluator As Func(Of RawFunctionCall, ISyntaxMarker, List(Of Semantics.Node))
        Private FunctionCallEvaluator As Func(Of FunctionCall, ISemanticsNodeMaker, List(Of Semantics.Node))

        Private Positions As Dictionary(Of Object, FileTextRange)

        Public Sub New(ByVal Setting As TreeFormatEvaluateSetting, ByVal pr As TreeFormatParseResult)
            Me.pr = pr
            Me.TokenParameterEvaluator = Setting.TokenParameterEvaluator
            Me.FunctionCallEvaluator = Setting.FunctionCallEvaluator
            Me.Positions = New Dictionary(Of Object, FileTextRange)(pr.Positions.ToDictionary(Function(p) p.Key, Function(p) New FileTextRange With {.Text = pr.Text, .Range = p.Value}))
        End Sub

        ReadOnly Property Text As Text Implements ISyntaxMarker.Text, ISemanticsNodeMaker.Text
            Get
                Return pr.Text
            End Get
        End Property

        Public Function Evaluate() As TreeFormatResult
            Dim Nodes = EvaluateMultiNodesList(pr.Value.MultiNodesList)
            Dim F = Mark(New Semantics.Forest With {.Nodes = Nodes}, pr.Value)
            Return New TreeFormatResult With {.Value = F, .Positions = Positions}
        End Function

        Private Function GetRange(ByVal Obj As Object) As [Optional](Of TextRange) Implements ISyntaxMarker.GetRange, ISemanticsNodeMaker.GetRange
            If Not Positions.ContainsKey(Obj) Then Return [Optional](Of TextRange).Empty
            Return Positions(Obj).Range.Value
        End Function
        Private Function GetFileRange(ByVal Obj As Object) As [Optional](Of FileTextRange) Implements ISyntaxMarker.GetFileRange, ISemanticsNodeMaker.GetFileRange
            If Not Positions.ContainsKey(Obj) Then Return [Optional](Of FileTextRange).Empty
            Return New FileTextRange With {.Text = pr.Text, .Range = GetRange(Obj)}
        End Function
        Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As [Optional](Of TextRange)) As T Implements ISyntaxMarker.Mark
            If Range.OnHasValue Then
                Positions.Add(Obj, New FileTextRange With {.Text = pr.Text, .Range = Range})
            End If
            Return Obj
        End Function
        Private Function Mark(Of T)(ByVal Obj As T, ByVal SyntaxRule As Object) As T Implements ISyntaxMarker.Mark
            Dim Range = GetRange(SyntaxRule)
            If Range.OnHasValue Then
                Positions.Add(Obj, New FileTextRange With {.Text = pr.Text, .Range = Range})
            End If
            Return Obj
        End Function
        Private Function MakeEmptyNode(ByVal Range As [Optional](Of TextRange)) As Semantics.Node Implements ISemanticsNodeMaker.MakeEmptyNode
            Dim n = Mark(Semantics.Node.CreateEmpty(), Range)
            Return n
        End Function
        Private Function MakeLeafNode(ByVal Value As String, ByVal Range As [Optional](Of TextRange)) As Semantics.Node Implements ISemanticsNodeMaker.MakeLeafNode
            Dim n = Mark(Semantics.Node.CreateLeaf(Value), Range)
            Return n
        End Function
        Private Function MakeStemNode(ByVal Name As String, ByVal Children As List(Of Semantics.Node), ByVal Range As [Optional](Of TextRange)) As Semantics.Node Implements ISemanticsNodeMaker.MakeStemNode
            Dim s = Mark(New Semantics.Stem With {.Name = Name, .Children = Children}, Range)
            Dim n = Mark(Semantics.Node.CreateStem(s), Range)
            Return n
        End Function
        Private Function MakeEmptyNode(ByVal SyntaxRule As Object) As Semantics.Node Implements ISemanticsNodeMaker.MakeEmptyNode
            Dim Range = GetRange(SyntaxRule)
            Dim n = Mark(Semantics.Node.CreateEmpty(), Range)
            Return n
        End Function
        Private Function MakeLeafNode(ByVal Value As String, ByVal SyntaxRule As Object) As Semantics.Node Implements ISemanticsNodeMaker.MakeLeafNode
            Dim Range = GetRange(SyntaxRule)
            Dim n = Mark(Semantics.Node.CreateLeaf(Value), Range)
            Return n
        End Function
        Private Function MakeStemNode(ByVal Name As String, ByVal Children As List(Of Semantics.Node), ByVal SyntaxRule As Object) As Semantics.Node Implements ISemanticsNodeMaker.MakeStemNode
            Dim Range = GetRange(SyntaxRule)
            Dim s = Mark(New Semantics.Stem With {.Name = Name, .Children = Children}, Range)
            Dim n = Mark(Semantics.Node.CreateStem(s), Range)
            Return n
        End Function

        Private Function EvaluateMultiNodesList(ByVal MultiNodesList As List(Of MultiNodes)) As List(Of Semantics.Node)
            Dim l As New List(Of Semantics.Node)
            For Each mn In MultiNodesList
                Select Case mn._Tag
                    Case MultiNodesTag.Node
                        Dim n = EvaluateNode(mn.Node)
                        If n.OnHasValue Then
                            l.Add(n.Value)
                        End If
                    Case MultiNodesTag.ListNodes
                        Dim ln = mn.ListNodes
                        Dim Children = EvaluateMultiNodesList(ln.Children)
                        For Each c In Children
                            Dim n = MakeStemNode(ln.ChildHead.Text, New List(Of Semantics.Node) From {c}, c)
                            l.Add(n)
                        Next
                    Case MultiNodesTag.TableNodes
                        Dim tn = mn.TableNodes
                        For Each c In tn.Children
                            If c.Nodes.Count = 0 Then Continue For
                            If c.Nodes.Count <> tn.ChildFields.Count Then
                                Throw New InvalidEvaluationException("TableLineNodeCountNotMatchHead", New FileTextRange With {.Text = pr.Text, .Range = GetRange(c)}, c)
                            End If
                            Dim MakeField = Function(TableLineNode As TableLineNode, Field As SingleLineLiteral) MakeStemNode(Field.Text, New List(Of Semantics.Node) From {EvaluateTableLineNode(TableLineNode)}, TableLineNode)
                            Dim Fields = c.Nodes.Zip(tn.ChildFields, MakeField).ToList()
                            Dim n = MakeStemNode(tn.ChildHead.Text, Fields, c)
                            l.Add(n)
                        Next
                    Case MultiNodesTag.FunctionNodes
                        Dim fn = mn.FunctionNodes
                        If Not pr.RawFunctionCalls.ContainsKey(fn) Then Throw New InvalidEvaluationException("FunctionCallNotFound", New FileTextRange With {.Text = pr.Text, .Range = GetRange(fn)}, fn)
                        Dim rfc = pr.RawFunctionCalls(fn)
                        If rfc.ReturnValueMode <> FunctionCallReturnValueMode.MultipleNodes Then Throw New InvalidEvaluationException("FunctionCallReturnValueModeUnexpected", New FileTextRange With {.Text = pr.Text, .Range = GetRange(fn)}, fn)
                        l.AddRange(EvaluateFunction(rfc))
                    Case Else
                        Throw New ArgumentException
                End Select
            Next
            Return l
        End Function

        Private Function EvaluateNode(ByVal Node As Node) As [Optional](Of Semantics.Node)
            Select Case Node._Tag
                Case NodeTag.SingleLineNodeLine
                    Return EvaluateSingleLineNode(Node.SingleLineNodeLine.SingleLineNode)
                Case NodeTag.MultiLineLiteral
                    Return MakeLeafNode(Node.MultiLineLiteral.Content.Text, Node)
                Case NodeTag.SingleLineComment
                    Return [Optional](Of Semantics.Node).Empty
                Case NodeTag.MultiLineComment
                    Return [Optional](Of Semantics.Node).Empty
                Case NodeTag.MultiLineNode
                    Dim mln = Node.MultiLineNode
                    Dim Children = EvaluateMultiNodesList(mln.Children)
                    Return MakeStemNode(mln.Head.Text, Children, mln)
                Case Else
                    Throw New ArgumentException
            End Select
        End Function

        Private Function EvaluateSingleLineNode(ByVal SingleLineNode As SingleLineNode) As Semantics.Node
            Select Case SingleLineNode._Tag
                Case SingleLineNodeTag.EmptyNode
                    Return MakeEmptyNode(SingleLineNode.EmptyNode)
                Case SingleLineNodeTag.SingleLineFunctionNode
                    Return EvaluateSingleLineFunctionNode(SingleLineNode.SingleLineFunctionNode)
                Case SingleLineNodeTag.SingleLineLiteral
                    Dim sll = SingleLineNode.SingleLineLiteral
                    Return MakeLeafNode(sll.Text, sll)
                Case SingleLineNodeTag.ParenthesisNode
                    Return EvaluateSingleLineNode(SingleLineNode.ParenthesisNode.SingleLineNode)
                Case SingleLineNodeTag.SingleLineNodeWithParameters
                    Dim slnwp = SingleLineNode.SingleLineNodeWithParameters
                    Dim Children As New List(Of Semantics.Node)
                    For Each c In slnwp.Children
                        Children.Add(EvaluateSingleLineNode(c.SingleLineNode))
                    Next
                    If slnwp.LastChild.OnHasValue Then
                        Dim lc = slnwp.LastChild.Value
                        Children.Add(EvaluateSingleLineNode(lc))
                    End If
                    Return MakeStemNode(slnwp.Head.Text, Children, slnwp)
                Case Else
                    Throw New ArgumentException
            End Select
        End Function

        Private Function EvaluateTableLineNode(ByVal TableLineNode As TableLineNode) As Semantics.Node
            Select Case TableLineNode._Tag
                Case TableLineNodeTag.EmptyNode
                    Return MakeEmptyNode(TableLineNode.EmptyNode)
                Case TableLineNodeTag.SingleLineFunctionNode
                    Return EvaluateSingleLineFunctionNode(TableLineNode.SingleLineFunctionNode)
                Case TableLineNodeTag.SingleLineLiteral
                    Dim sll = TableLineNode.SingleLineLiteral
                    Return MakeLeafNode(sll.Text, sll)
                Case TableLineNodeTag.ParenthesisNode
                    Return EvaluateSingleLineNode(TableLineNode.ParenthesisNode.SingleLineNode)
                Case Else
                    Throw New ArgumentException
            End Select
        End Function

        Private Function EvaluateSingleLineFunctionNode(ByVal SingleLineFunctionNode As SingleLineFunctionNode) As Semantics.Node
            Dim fn = SingleLineFunctionNode
            If Not pr.RawFunctionCalls.ContainsKey(fn) Then Throw New InvalidEvaluationException("FunctionCallNotFound", New FileTextRange With {.Text = pr.Text, .Range = GetRange(fn)}, fn)
            Dim rfc = pr.RawFunctionCalls(fn)
            If rfc.ReturnValueMode <> FunctionCallReturnValueMode.SingleNode Then Throw New InvalidEvaluationException("FunctionCallReturnValueModeUnexpected", New FileTextRange With {.Text = pr.Text, .Range = GetRange(fn)}, fn)
            Dim Nodes = EvaluateFunction(rfc)
            If Nodes.Count <> 1 Then Throw New InvalidEvaluationException("FunctionCallNotReturnSingleNode", New FileTextRange With {.Text = pr.Text, .Range = GetRange(fn)}, fn)
            Return Nodes.Single
        End Function

        Private Function EvaluateFunction(ByVal rfc As RawFunctionCall) As List(Of Semantics.Node)
            Dim Parameters As List(Of Semantics.Node)
            Select Case rfc.Parameters._Tag
                Case RawFunctionCallParametersTag.TokenParameters
                    If TokenParameterEvaluator Is Nothing Then Throw New InvalidEvaluationException("TokenParameterEvaluatorIsNull", New FileTextRange With {.Text = pr.Text, .Range = GetRange(rfc)}, rfc)
                    Parameters = TokenParameterEvaluator(rfc, Me)
                Case RawFunctionCallParametersTag.TreeParameter
                    Dim tp = rfc.Parameters.TreeParameter
                    If Not tp.OnHasValue Then
                        Parameters = New List(Of Semantics.Node) From {}
                    Else
                        Parameters = New List(Of Semantics.Node) From {EvaluateSingleLineNode(tp.Value)}
                    End If
                Case RawFunctionCallParametersTag.TableParameters
                    Dim tp = rfc.Parameters.TableParameters
                    Parameters = tp.Select(Function(p) EvaluateTableLineNode(p)).ToList()
                Case Else
                    Throw New ArgumentException
            End Select

            Dim Content = [Optional](Of FunctionCallContent).Empty
            If rfc.Content.OnHasValue Then
                Dim rfcc = rfc.Content.Value
                Select Case rfcc._Tag
                    Case RawFunctionCallContentTag.LineContent
                        Content = Mark(FunctionCallContent.CreateLineContent(rfcc.LineContent), rfcc)
                    Case RawFunctionCallContentTag.TreeContent
                        Dim TreeContent = EvaluateMultiNodesList(rfcc.TreeContent)
                        Content = Mark(FunctionCallContent.CreateTreeContent(TreeContent), rfcc)
                    Case RawFunctionCallContentTag.TableContent
                        Dim TableContent = rfcc.TableContent.Select(Function(Line) Mark(New FunctionCallTableLine With {.Nodes = Line.Nodes.Select(Function(LineNode) EvaluateTableLineNode(LineNode)).ToList()}, Line)).ToList()
                        Content = Mark(FunctionCallContent.CreateTableContent(TableContent), rfcc)
                    Case Else
                        Throw New ArgumentException
                End Select
            End If

            Dim F = Mark(New FunctionCall With {.Name = rfc.Name, .ReturnValueMode = rfc.ReturnValueMode, .Parameters = Parameters, .Content = Content}, rfc)

            If FunctionCallEvaluator Is Nothing Then Throw New InvalidEvaluationException("FunctionCallEvaluatorIsNull", New FileTextRange With {.Text = pr.Text, .Range = GetRange(rfc)}, rfc)
            Return FunctionCallEvaluator(F, Me)
        End Function
    End Class
End Namespace
