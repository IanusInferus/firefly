'==========================================================================
'
'  File:        SyntaxWriter.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文本输出类
'  Version:     2013.05.03.
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
Imports Firefly.Texting.TreeFormat.Syntax

Namespace Texting.TreeFormat
    Public Class TreeFormatSyntaxWriter
        Private sw As StreamWriter
        Public Sub New(ByVal sw As StreamWriter)
            Me.sw = sw
        End Sub

        Private Shared ReadOnly Comment As String = "$Comment"
        Private Shared ReadOnly Empty As String = "$Empty"
        Private Shared ReadOnly StringDirective As String = "$String"
        Private Shared ReadOnly EndDirective As String = "$End"
        Private Shared ReadOnly List As String = "$List"
        Private Shared ReadOnly Table As String = "$Table"

        Public Sub Write(ByVal Forest As Forest)
            For Each mn In Forest.MultiNodesList
                WriteMultiNodes(0, mn)
            Next
        End Sub

        Private Sub WriteMultiNodes(ByVal IndentLevel As Integer, ByVal mn As MultiNodes)
            Select Case mn._Tag
                Case MultiNodesTag.Node
                    WriteNode(IndentLevel, mn.Node)
                Case MultiNodesTag.ListNodes
                    WriteListNodes(IndentLevel, mn.ListNodes)
                Case MultiNodesTag.TableNodes
                    WriteTableNodes(IndentLevel, mn.TableNodes)
                Case MultiNodesTag.FunctionNodes
                    WriteFunctionNodes(IndentLevel, mn.FunctionNodes)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Sub

        Private Sub WriteNode(ByVal IndentLevel As Integer, ByVal n As Node)
            Select Case n._Tag
                Case NodeTag.SingleLineNodeLine
                    WriteSingleLineNodeLine(IndentLevel, n.SingleLineNodeLine)
                Case NodeTag.MultiLineLiteral
                    WriteMultiLineLiteral(IndentLevel, n.MultiLineLiteral)
                Case NodeTag.SingleLineComment
                    WriteSingleLineComment(IndentLevel, n.SingleLineComment)
                Case NodeTag.MultiLineComment
                    WriteMultiLineComment(IndentLevel, n.MultiLineComment)
                Case NodeTag.MultiLineNode
                    WriteMultiLineNode(IndentLevel, n.MultiLineNode)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Sub
        Private Sub WriteListNodes(ByVal IndentLevel As Integer, ByVal ln As ListNodes)
            Dim ChildHeadLiteral = GetLiteral(ln.ChildHead.Text, True).SingleLine
            If ln.SingleLineComment.HasValue Then
                Dim slc = GetSingleLineComment(ln.SingleLineComment.Value)
                WriteRaw(IndentLevel, List, ChildHeadLiteral, slc)
            Else
                WriteRaw(IndentLevel, List, ChildHeadLiteral)
            End If
            For Each mn In ln.Children
                WriteMultiNodes(IndentLevel + 1, mn)
            Next
            WriteEndDirective(IndentLevel, ln.EndDirective)
        End Sub
        Private Sub WriteTableNodes(ByVal IndentLevel As Integer, ByVal tn As TableNodes)
            Dim ChildHeadLiteral = GetLiteral(tn.ChildHead.Text, True).SingleLine
            Dim ChildFields = Combine(tn.ChildFields.Select(Function(f) GetSingleLineLiteral(f)).ToArray())
            If tn.SingleLineComment.HasValue Then
                Dim slc = GetSingleLineComment(tn.SingleLineComment.Value)
                WriteRaw(IndentLevel, Table, ChildHeadLiteral, ChildFields, slc)
            Else
                WriteRaw(IndentLevel, Table, ChildHeadLiteral, ChildFields)
            End If
            Dim NumColumn = (New Integer() {0}).Concat(tn.Children.Select(Function(c) c.Nodes.Length)).Max() + 1
            Dim DataTable As New List(Of String())
            For Each tl In tn.Children
                Dim l As New List(Of String)
                For Each n In tl.Nodes
                    l.Add(GetTableLineNode(n))
                Next
                If tl.SingleLineComment.HasValue Then
                    l.Add(GetSingleLineComment(tl.SingleLineComment.Value))
                End If
                While l.Count < NumColumn
                    l.Add("")
                End While
                DataTable.Add(l.ToArray())
            Next
            Dim DataLines = GetDataLines(DataTable.ToArray())
            For Each dl In DataLines
                WriteRaw(IndentLevel + 1, dl)
            Next
            WriteEndDirective(IndentLevel, tn.EndDirective)
        End Sub
        Private Function GetDataLines(ByVal DataTable As String()()) As String()
            If DataTable.Length = 0 Then Return New String() {}

            Dim Table = DataTable.Select(Function(Row) Row.Select(Function(Column) Escape(Column)).ToArray()).ToArray()

            Dim NumColumn = Table.Select(Function(Row) Row.Length).Distinct().Single()
            Dim ColumnLength = Enumerable.Range(0, NumColumn).Select(Function(i) ((New Integer() {0}).Concat(Table.Select(Function(Row) CalculateCharWidth(Row(i)))).Max() + 1).CeilToMultipleOf(4) + 4).ToArray()

            Dim NodeLines = New List(Of String)()
            For Each Row In Table
                NodeLines.Add(String.Join("", Row.Zip(ColumnLength, Function(v, l) v + New String(" "c, l - CalculateCharWidth(v))).ToArray()))
            Next
            Return NodeLines.Select(Function(Line) Line.TrimEnd(" "c)).ToArray()
        End Function
        Private Function CalculateCharWidth(ByVal s As String) As Integer
            Return s.ToUTF32().Select(Function(c) If(c.IsHalfWidth, 1, 2)).Sum()
        End Function
        Private Sub WriteFunctionNodes(ByVal IndentLevel As Integer, ByVal fn As FunctionNodes)
            Dim Tokens = CombineTokens(fn.Parameters.Select(Function(t) GetToken(t)).ToArray())
            If fn.SingleLineComment.HasValue Then
                Dim slc = GetSingleLineComment(fn.SingleLineComment.Value)
                WriteRaw(IndentLevel, GetFunctionDirective(fn.FunctionDirective), Tokens, slc)
            Else
                WriteRaw(IndentLevel, GetFunctionDirective(fn.FunctionDirective), Tokens)
            End If
            Dim ContentIndentLevel = fn.Content.IndentLevel
            For Each l In fn.Content.Lines
                If l.Text.Take(ContentIndentLevel * 4).Where(Function(c) c <> " ").Count > 0 Then
                    Throw New ArgumentException(l.Text)
                End If
                WriteRaw(IndentLevel + 1, New String(l.Text.Skip(ContentIndentLevel * 4).ToArray()))
            Next
            WriteEndDirective(IndentLevel, fn.EndDirective)
        End Sub

        Private Sub WriteSingleLineNodeLine(ByVal IndentLevel As Integer, ByVal slnl As SingleLineNodeLine)
            Dim sln = GetSingleLineNode(slnl.SingleLineNode)
            If slnl.SingleLineComment.HasValue Then
                Dim slc = GetSingleLineComment(slnl.SingleLineComment.Value)
                WriteRaw(IndentLevel, sln, slc)
            Else
                WriteRaw(IndentLevel, sln)
            End If
        End Sub
        Private Sub WriteMultiLineLiteral(ByVal IndentLevel As Integer, ByVal mll As MultiLineLiteral)
            If mll.SingleLineComment.HasValue Then
                Dim slc = GetSingleLineComment(mll.SingleLineComment.Value)
                WriteRaw(IndentLevel, StringDirective, slc)
            Else
                WriteRaw(IndentLevel, StringDirective)
            End If
            Dim MultiLine = GetLiteral(mll.Content.Text, False, True).MultiLine
            For Each Line In MultiLine
                WriteRaw(IndentLevel + 1, Line)
            Next
            WriteEndDirective(IndentLevel, mll.EndDirective)
        End Sub
        Private Sub WriteSingleLineComment(ByVal IndentLevel As Integer, ByVal slc As SingleLineComment)
            WriteRaw(IndentLevel, GetSingleLineComment(slc))
        End Sub
        Private Sub WriteMultiLineComment(ByVal IndentLevel As Integer, ByVal mlc As MultiLineComment)
            If mlc.SingleLineComment.HasValue Then
                Dim slc = GetSingleLineComment(mlc.SingleLineComment.Value)
                WriteRaw(IndentLevel, Comment, slc)
            Else
                WriteRaw(IndentLevel, Comment)
            End If
            Dim MultiLine = GetLiteral(mlc.Content.Text, False, True).MultiLine
            For Each Line In MultiLine
                WriteRaw(IndentLevel + 1, Line)
            Next
            WriteEndDirective(IndentLevel, mlc.EndDirective)
        End Sub
        Private Sub WriteMultiLineNode(ByVal IndentLevel As Integer, ByVal mln As MultiLineNode)
            Dim HeadLiteral = GetLiteral(mln.Head.Text, True).SingleLine
            If mln.SingleLineComment.HasValue Then
                Dim slc = GetSingleLineComment(mln.SingleLineComment.Value)
                WriteRaw(IndentLevel, HeadLiteral, slc)
            Else
                WriteRaw(IndentLevel, HeadLiteral)
            End If
            For Each mn In mln.Children
                WriteMultiNodes(IndentLevel + 1, mn)
            Next
            WriteEndDirective(IndentLevel, mln.EndDirective)
        End Sub

        Private Function GetSingleLineNode(ByVal sln As SingleLineNode) As String
            Select Case sln._Tag
                Case SingleLineNodeTag.EmptyNode
                    Return GetEmptyNode(sln.EmptyNode)
                Case SingleLineNodeTag.SingleLineFunctionNode
                    Return GetSingleLineFunctionNode(sln.SingleLineFunctionNode)
                Case SingleLineNodeTag.SingleLineLiteral
                    Return GetSingleLineLiteral(sln.SingleLineLiteral)
                Case SingleLineNodeTag.ParenthesesNode
                    Return GetParenthesesNode(sln.ParenthesesNode)
                Case SingleLineNodeTag.SingleLineNodeWithParameters
                    Return GetSingleLineNodeWithParameters(sln.SingleLineNodeWithParameters)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Private Function GetTableLineNode(ByVal tln As TableLineNode) As String
            Select Case tln._Tag
                Case TableLineNodeTag.EmptyNode
                    Return GetEmptyNode(tln.EmptyNode)
                Case TableLineNodeTag.SingleLineFunctionNode
                    Return GetSingleLineFunctionNode(tln.SingleLineFunctionNode)
                Case TableLineNodeTag.SingleLineLiteral
                    Return GetSingleLineLiteral(tln.SingleLineLiteral)
                Case TableLineNodeTag.ParenthesesNode
                    Return GetParenthesesNode(tln.ParenthesesNode)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

        Private Function GetEmptyNode(ByVal en As EmptyNode) As String
            Return Empty
        End Function
        Private Function GetSingleLineFunctionNode(ByVal slfn As SingleLineFunctionNode) As String
            Dim Tokens = CombineTokens(slfn.Parameters.Select(Function(t) GetToken(t)).ToArray())
            Return Combine(GetFunctionDirective(slfn.FunctionDirective), Tokens)
        End Function
        Private Function GetParenthesesNode(ByVal pn As ParenthesesNode) As String
            Return "(" & GetSingleLineNode(pn.SingleLineNode) & ")"
        End Function
        Private Function GetSingleLineNodeWithParameters(ByVal slnp As SingleLineNodeWithParameters) As String
            Dim l As New List(Of String)
            For Each c In slnp.Children
                l.Add(GetParenthesesNode(c))
            Next
            If slnp.LastChild.HasValue Then
                l.Add(GetSingleLineNode(slnp.LastChild.Value))
            End If
            Return Combine(GetSingleLineLiteral(slnp.Head), Combine(l.ToArray()))
        End Function

        Private Shared Function GetToken(ByVal t As Token) As String
            Select Case t._Tag
                Case TokenTag.SingleLineLiteral
                    Return GetSingleLineLiteral(t.SingleLineLiteral)
                Case TokenTag.LeftParentheses
                    Return "("
                Case TokenTag.RightParentheses
                    Return ")"
                Case TokenTag.PreprocessDirective
                    Return GetPreprocessDirective(t.PreprocessDirective)
                Case TokenTag.FunctionDirective
                    Return GetFunctionDirective(t.FunctionDirective)
                Case TokenTag.SingleLineComment
                    Return GetSingleLineComment(t.SingleLineComment)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function

        Private Shared Function GetSingleLineLiteral(ByVal sll As String) As String
            Return GetLiteral(sll, True).SingleLine
        End Function
        Private Shared Function GetSingleLineLiteral(ByVal sll As SingleLineLiteral) As String
            Return GetSingleLineLiteral(sll.Text)
        End Function

        Private Shared Function GetPreprocessDirective(ByVal pd As String) As String
            Dim s = GetLiteral(pd, True).SingleLine
            If s.Contains("""") Then Throw New ArgumentException(pd)
            Return "$" & s
        End Function

        Private Shared Function GetFunctionDirective(ByVal fd As String) As String
            Dim s = GetLiteral(fd, True).SingleLine
            If s.Contains("""") Then Throw New ArgumentException(fd)
            Return "#" & s
        End Function
        Private Shared Function GetFunctionDirective(ByVal fd As FunctionDirective) As String
            Return GetFunctionDirective(fd.Text)
        End Function

        Private Shared Function GetSingleLineComment(ByVal slc As String) As String
            If slc.Contains(Cr) OrElse slc.Contains(Lf) Then
                Throw New InvalidOperationException
            End If
            Return "//" & slc
        End Function
        Private Shared Function GetSingleLineComment(ByVal slc As SingleLineComment) As String
            Return GetSingleLineComment(slc.Content.Text)
        End Function

        Private Sub WriteEndDirective(ByVal IndentLevel As Integer, ByVal ed As Opt(Of EndDirective))
            If ed.HasValue Then
                If ed.Value.EndSingleLineComment.HasValue Then
                    Dim eslc = GetSingleLineComment(ed.Value.EndSingleLineComment.Value)
                    WriteRaw(IndentLevel, EndDirective, eslc)
                Else
                    WriteRaw(IndentLevel, EndDirective)
                End If
            End If
        End Sub

        Private Function CombineTokens(ByVal ParamArray Values As String()) As String
            '左括号的右边或右括号的左边没有空格

            If Values.Length = 0 Then Return ""

            Dim l As New List(Of String)
            For k = 0 To Values.Length - 2
                Dim a = Values(k)
                Dim b = Values(k + 1)
                l.Add(a)
                If a = "(" OrElse b = ")" Then Continue For
                l.Add(" ")
            Next
            l.Add(Values(Values.Length - 1))

            Return String.Join("", l.ToArray())
        End Function
        Private Function Combine(ByVal ParamArray Values As String()) As String
            Return String.Join(" ", Values.Where(Function(v) v <> "").ToArray())
        End Function
        Private Sub WriteRaw(ByVal IndentLevel As Integer, ByVal Value1 As String, ByVal Value2 As String, ByVal ParamArray Values As String())
            WriteRaw(IndentLevel, Combine((New String() {Value1, Value2}).Concat(Values).ToArray()))
        End Sub
        Private Sub WriteRaw(ByVal IndentLevel As Integer, ByVal Value As String)
            Dim si = New String(" "c, 4 * IndentLevel)
            sw.WriteLine(si & Value)
        End Sub

        Private Shared Function GetLiteral(ByVal Value As String, ByVal MustSingleLine As Boolean, ByVal Optional MustMultiLine As Boolean = False) As Literal
            Return TreeFormatLiteralWriter.GetLiteral(Value, MustSingleLine, MustMultiLine)
        End Function
    End Class
End Namespace
