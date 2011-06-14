'==========================================================================
'
'  File:        TreeFile.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: Tree文件(Xml等价)读写
'  Version:     2011.06.14.
'  Copyright:   F.R.C.
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
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Mapping.MetaSchema

Public NotInheritable Class TreeFile
    Public Shared Function ReadFile(ByVal Path As String) As XElement
        Return ReadFile(Path, TextEncoding.Default)
    End Function
    Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding) As XElement
        Using sr = Txt.CreateTextReader(Path, Encoding)
            Return ReadFile(sr)
        End Using
    End Function
    Public Shared Function ReadFile(ByVal Reader As StreamReader) As XElement
        Dim Lines As String() = Txt.ReadFile(Reader).UnifyNewLineToLf.Split(CChar(Lf))

        Dim r As New Reader With {.s = New ArrayStream(Of String)(Lines)}
        Dim Root As TreeElement
        Try
            Root = r.ReadRoot()
        Catch ex As Exception When Not TypeOf ex Is InvalidTextFormatException
            Throw New InvalidTextFormatException("", New FileLocationInformation With {.LineNumber = r.s.Position + 1}, ex)
        End Try
        Dim t As New ReaderTranslator With {.LineInformation = r.LineInformation, .ValueLineInformation = r.ValueLineInformation}
        Return t.GetRoot(Root)
    End Function

    Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As XElement)
        WriteFile(Path, TextEncoding.WritingDefault, Value)
    End Sub
    Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As XElement)
        Using sw = Txt.CreateTextWriter(Path, Encoding)
            WriteFile(sw, Value)
        End Using
    End Sub
    Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As XElement)
        Dim w As New Writer With {.sw = Writer}
        w.WriteNode(0, Value)
    End Sub

    Private Shared ReadOnly Empty As String = "$Empty"
    Private Shared ReadOnly List As String = "$List"
    Private Shared ReadOnly StringLiteral As String = "$String"
    Private Shared ReadOnly Comment As String = "$Comment"
    Private Shared ReadOnly Table As String = "$Table"

    Private Class ReaderTranslator
        Public LineInformation As New Dictionary(Of TreeElement, FileLocationInformation)
        Public ValueLineInformation As New Dictionary(Of TreeValue, FileLocationInformation)

        Public Function GetRoot(ByVal Root As TreeElement) As XElement
            Dim n = TryGetXName(Root.Name, Nothing)
            Dim i As FileLocationInformation = Nothing
            If LineInformation.ContainsKey(Root) Then
                i = LineInformation(Root)
            End If
            If n Is Nothing Then
                If i Is Nothing Then i = New FileLocationInformation
                Throw New InvalidTextFormatException("NamingError", i)
            End If
            Dim x As XElementEx
            If Root.Value IsNot Nothing Then
                x = New XElementEx(n, Root.Value.Value)
            Else
                x = New XElementEx(n, Nothing)
            End If
            If i IsNot Nothing Then x.SetLineInfo(i)
            FillElement(Root, x)
            Return x
        End Function

        Public Sub FillElement(ByVal t As TreeElement, ByVal x As XElement)
            For Each a In t.Attributes
                Dim xa = GetAttribute(a, x)
                x.SetAttributeValue(xa.Name, xa.Value)
            Next
            For Each e In t.Elements
                Dim n = TryGetXName(e.Name, x)
                Dim i As FileLocationInformation = Nothing
                If LineInformation.ContainsKey(e) Then
                    i = LineInformation(e)
                End If
                If n Is Nothing Then
                    If i Is Nothing Then i = New FileLocationInformation
                    Throw New InvalidTextFormatException("NamingError", i)
                End If
                Dim xe As XElementEx
                If e.Value IsNot Nothing Then
                    xe = New XElementEx(n, e.Value.Value)
                Else
                    xe = New XElementEx(n, Nothing)
                End If
                If i IsNot Nothing Then xe.SetLineInfo(i)
                x.Add(xe)
                FillElement(e, xe)
            Next
        End Sub

        Public Function GetAttribute(ByVal t As TreeAttribute, ByVal Parent As XElement) As XAttribute
            Return New XAttribute(TryGetXName(t.Name, Parent), t.Value)
        End Function

        Public Function TryGetXName(ByVal Name As String, ByVal Node As XElement) As XName
            Dim NameParts = Name.Split(":"c)
            If NameParts.Length > 2 Then Return Nothing
            If NameParts.Length <= 1 Then
                If Node Is Nothing Then
                    Return Name
                Else
                    Return Node.GetDefaultNamespace().GetName(Name)
                End If
            Else
                Dim NamespacePrefix = NameParts(0)
                Dim LocalName = NameParts(1)
                If Node Is Nothing Then
                    Select Case NamespacePrefix
                        Case "xml"
                            Return XNamespace.Xml.GetName(LocalName)
                        Case "xmlns"
                            Return XNamespace.Xmlns.GetName(LocalName)
                        Case Else
                            Return Nothing
                    End Select
                Else
                    Return Node.GetNamespaceOfPrefix(NamespacePrefix).GetName(LocalName)
                End If
            End If
        End Function
    End Class

    Private Class Reader
        Public s As ArrayStream(Of String)
        Public LineInformation As New Dictionary(Of TreeElement, FileLocationInformation)
        Public ValueLineInformation As New Dictionary(Of TreeValue, FileLocationInformation)

        Public Function ReadRoot() As TreeElement
            Dim l = ReadNodes(0)
            If l.Length <> 1 Then Throw New InvalidTextFormatException("UseOnlyOneRoot", New FileLocationInformation With {.LineNumber = 1})
            Dim x = l.Single
            If x._Tag <> NodeResultTag.Element Then Throw New InvalidTextFormatException("RootMustBeElement", New FileLocationInformation With {.LineNumber = 1})
            Return x.Element
        End Function

        Public Function TryReadNode(ByVal IndentLevel As Integer) As NodeResult
            Dim LineNumber = s.Position + 1

            Dim RawResult As RawResult = Nothing
            Dim Tokens As String() = Nothing
            While True
                LineNumber = s.Position + 1
                RawResult = TryReadRaw(IndentLevel)
                Select Case RawResult._Tag
                    Case RawResultTag.Normal
                        Dim Text = RawResult.Normal
                        If Text.StartsWith(" ") Then Throw New InvalidTextFormatException("IndentError", New FileLocationInformation With {.LineNumber = LineNumber})
                        Tokens = GetTokens(IndentLevel, Text)
                        If Tokens.Length = 0 Then Continue While
                        Exit While
                    Case RawResultTag.EmptyLineWithIndentedSpaces, RawResultTag.EmptyLineWithoutIndentedSpaces
                        Continue While
                    Case RawResultTag.EndOfBranch, RawResultTag.EndOfStream
                        Return New NodeResult With {._Tag = NodeResultTag.EndOfBranch}
                    Case Else
                        Throw New InvalidOperationException
                End Select
            End While

            Dim HeadToken = Tokens.First

            Dim HeadChar = HeadToken.ToUTF32()(0)
            If Not "!@#$%&/;=?\^`|~".ToUTF32.Any(Function(c) c = HeadChar) Then
                Dim Nodes = ReadNodes(IndentLevel + 1)
                If Tokens.Length > 1 AndAlso Nodes.Length > 0 Then Throw New InvalidTextFormatException("IndentError", New FileLocationInformation With {.LineNumber = LineNumber})
                If Tokens.Length = 1 AndAlso Nodes.Length = 0 Then
                    Dim v As New TreeValue With {.Value = Tokens(0)}
                    ValueLineInformation.Add(v, New FileLocationInformation With {.LineNumber = LineNumber})
                    Return New NodeResult With {._Tag = NodeResultTag.Value, .Value = v}
                End If
                Dim Node As New TreeElement With {.Name = HeadToken}
                LineInformation.Add(Node, New FileLocationInformation With {.LineNumber = LineNumber, .ColumnNumber = 4 * IndentLevel + 1})
                Dim PreventMultiple As Boolean = False
                For Each n In Nodes
                    Select Case n._Tag
                        Case NodeResultTag.Attribute
                            Dim a = n.Attribute
                            Node.Attributes.Add(New TreeAttribute With {.Name = a.Name, .Value = a.Value})
                        Case NodeResultTag.Element
                            Node.Elements.Add(n.Element)
                            PreventMultiple = True
                        Case NodeResultTag.Elements
                            Node.Elements.AddRange(n.Elements)
                            PreventMultiple = True
                        Case NodeResultTag.Value
                            If PreventMultiple Then Throw New InvalidTextFormatException("ValueSyntaxError", ValueLineInformation(n.Value))
                            Node.Value = n.Value
                            PreventMultiple = True
                        Case Else
                            Throw New InvalidOperationException
                    End Select
                Next
                If Nodes.Length = 0 Then
                    Dim p As TreeElement
                    Dim n = Node
                    For Each Token In Tokens.Skip(1)
                        If Token Is Tokens.Last Then
                            Dim v As New TreeValue With {.Value = Token}
                            ValueLineInformation.Add(v, New FileLocationInformation With {.LineNumber = LineNumber})
                            n.Value = v
                            Exit For
                        End If
                        p = n
                        n = New TreeElement With {.Name = Token}
                        LineInformation.Add(n, New FileLocationInformation With {.LineNumber = LineNumber})
                        p.Elements.Add(n)
                    Next
                End If
                Return New NodeResult With {._Tag = NodeResultTag.Element, .Element = Node}
            ElseIf HeadChar = "@" Then
                If Tokens.Length > 2 Then Throw New InvalidTextFormatException("TokenError", New FileLocationInformation With {.LineNumber = LineNumber})
                Dim Nodes = ReadNodes(IndentLevel + 1)
                If Nodes.Length > 0 Then Throw New InvalidTextFormatException("IndentError", New FileLocationInformation With {.LineNumber = LineNumber + 1})
                Dim AttributeName = HeadToken.ToUTF32.Skip(1).ToUTF16B
                Return New NodeResult With {._Tag = NodeResultTag.Attribute, .Attribute = New TreeAttribute With {.Name = AttributeName, .Value = Tokens(1)}}
            ElseIf HeadChar = "$" Then
                Select Case HeadToken
                    Case Empty
                        Return New NodeResult With {._Tag = NodeResultTag.Value, .Value = Nothing}
                    Case List
                        If Tokens.Length <> 2 Then Throw New InvalidTextFormatException("ErrorProcessorDirectiveParam", New FileLocationInformation With {.LineNumber = LineNumber})
                        Dim Name = Tokens(1)
                        Dim Nodes = ReadNodes(IndentLevel + 1)
                        Dim l As New List(Of TreeElement)
                        For Each n In Nodes
                            Select Case n._Tag
                                Case NodeResultTag.Attribute
                                    Throw New InvalidTextFormatException("ListMustNotHaveAttribute", New FileLocationInformation With {.LineNumber = LineNumber})
                                Case NodeResultTag.Element
                                    Dim Node = New TreeElement With {.Name = Name}
                                    LineInformation.Add(Node, LineInformation(n.Element))
                                    Node.Elements.Add(n.Element)
                                    l.Add(Node)
                                Case NodeResultTag.Elements
                                    For Each e In n.Elements
                                        Dim Node = New TreeElement With {.Name = Name}
                                        LineInformation.Add(Node, LineInformation(e))
                                        Node.Elements.Add(e)
                                        l.Add(Node)
                                    Next
                                Case NodeResultTag.Value
                                    Dim Node = New TreeElement With {.Name = Name}
                                    LineInformation.Add(Node, ValueLineInformation(n.Value))
                                    Node.Value = n.Value
                                    l.Add(Node)
                                Case Else
                                    Throw New InvalidOperationException
                            End Select
                        Next
                        Return New NodeResult With {._Tag = NodeResultTag.Elements, .Elements = l}
                    Case Table
                        If Tokens.Length < 2 Then Throw New InvalidTextFormatException("ErrorProcessorDirectiveParam", New FileLocationInformation With {.LineNumber = LineNumber})
                        Dim Name = Tokens(1)
                        Dim Fields = Tokens.Skip(2).ToArray()
                        Dim l As New List(Of TreeElement)
                        Dim LineIndex = 1
                        While True
                            Dim Line = TryReadRaw(IndentLevel + 1)
                            Select Case Line._Tag
                                Case RawResultTag.Normal
                                    Dim LineTokens = GetTokens(0, Line.Normal)
                                    Dim li As New FileLocationInformation With {.LineNumber = LineNumber + LineIndex}
                                    If LineTokens.Length <> Fields.Length Then Throw New InvalidTextFormatException("TableFieldNameNotMatch", li)

                                    Dim Node As New TreeElement With {.Name = Name}
                                    LineInformation.Add(Node, li)

                                    For Each t In Fields.ZipStrict(LineTokens, Function(f, lt) New With {.Field = f, .Token = lt})
                                        Dim v As New TreeValue With {.Value = t.Token}
                                        ValueLineInformation.Add(v, li)
                                        Dim fe As New TreeElement With {.Name = t.Field, .Value = v}
                                        LineInformation.Add(fe, li)
                                        Node.Elements.Add(fe)
                                    Next

                                    l.Add(Node)
                                Case RawResultTag.EmptyLineWithIndentedSpaces, RawResultTag.EmptyLineWithoutIndentedSpaces

                                Case RawResultTag.EndOfBranch, RawResultTag.EndOfStream
                                    Exit While
                                Case Else
                                    Throw New InvalidOperationException
                            End Select
                            LineIndex += 1
                        End While
                        Return New NodeResult With {._Tag = NodeResultTag.Elements, .Elements = l}
                    Case StringLiteral, Comment
                        Dim l As New List(Of String)
                        While True
                            Dim Line = TryReadRaw(IndentLevel + 1)
                            Select Case Line._Tag
                                Case RawResultTag.Normal
                                    l.Add(Line.Normal)
                                Case RawResultTag.EmptyLineWithIndentedSpaces
                                    l.Add(Line.EmptyLineWithIndentedSpaces)
                                Case RawResultTag.EmptyLineWithoutIndentedSpaces
                                    l.Add(Nothing)
                                Case RawResultTag.EndOfBranch, RawResultTag.EndOfStream
                                    Exit While
                                Case Else
                                    Throw New InvalidOperationException
                            End Select
                        End While
                        Dim NonTailNothingCount As Integer = l.Count
                        While NonTailNothingCount > 0
                            If l(NonTailNothingCount - 1) Is Nothing Then
                                NonTailNothingCount -= 1
                            Else
                                Exit While
                            End If
                        End While
                        l = l.Take(NonTailNothingCount).Select(Function(s) If(s, "")).ToList
                        Select Case HeadToken
                            Case StringLiteral
                                Dim v As New TreeValue With {.Value = String.Join(CrLf, l.ToArray())}
                                ValueLineInformation.Add(v, New FileLocationInformation With {.LineNumber = LineNumber})
                                Return New NodeResult With {._Tag = NodeResultTag.Value, .Value = v}
                            Case Comment
                                Return New NodeResult With {._Tag = NodeResultTag.Comment, .Comment = String.Join(CrLf, l.ToArray())}
                            Case Else
                                Throw New InvalidOperationException
                        End Select
                    Case Else
                        Throw New InvalidTextFormatException("UndefinedProcessorDirective {0}".Formats(HeadToken), New FileLocationInformation With {.LineNumber = LineNumber})
                End Select
            Else
                Throw New InvalidTextFormatException("UndefinedProcessorDirective", New FileLocationInformation With {.LineNumber = LineNumber})
            End If
        End Function

        Public Function ReadNodes(ByVal IndentLevel As Integer) As NodeResult()
            Dim l As New List(Of NodeResult)
            While True
                Dim Node = TryReadNode(IndentLevel)
                Select Case Node._Tag
                    Case NodeResultTag.Comment
                        Continue While
                    Case NodeResultTag.EndOfBranch
                        Exit While
                    Case Else
                        Exit Select
                End Select
                l.Add(Node)
            End While
            Return l.ToArray()
        End Function

        Public Function TryReadRaw(ByVal IndentLevel As Integer) As RawResult
            If s.Position >= s.Length Then Return New RawResult With {._Tag = RawResultTag.EndOfStream}
            Dim Head = s.PeekElement
            Dim Line32 = Head.ToUTF32
            Dim Chars = Line32.Distinct.ToArray()
            If Not Head.StartsWith(New String(" "c, 4 * IndentLevel)) Then
                If Chars.Length = 0 OrElse (Chars.Length = 1 AndAlso Chars.Single = " ") Then
                    s.ReadElement()
                    Return New RawResult With {._Tag = RawResultTag.EmptyLineWithoutIndentedSpaces}
                Else
                    Return New RawResult With {._Tag = RawResultTag.EndOfBranch}
                End If
            Else
                Dim Str = Line32.Skip(4 * IndentLevel).ToUTF16B
                If Chars.Length = 0 OrElse (Chars.Length = 1 AndAlso Chars.Single = " ") Then
                    s.ReadElement()
                    Return New RawResult With {._Tag = RawResultTag.EmptyLineWithIndentedSpaces, .EmptyLineWithIndentedSpaces = Str}
                Else
                    s.ReadElement()
                    Return New RawResult With {._Tag = RawResultTag.Normal, .Normal = Str}
                End If
            End If
        End Function

        Private Shared Tab As String = "\t".Descape
        Public Function GetTokens(ByVal IndentLevel As Integer, ByVal Line As String) As String()
            '双引号(")中的字符串是一个token，且两个双引号表示一个双引号
            '双斜杠(//)及之后的字符表示注释
            '其余的字符在空格处分隔，不得有其他的空白字符

            'State 0 空白状态
            '    EndOfLine -> 结束
            '    空格 -> 保持
            '    \s -> throw
            '    " -> State 1
            '    // -> 结束
            '    _ -> inner token put, State 2

            'State 1 token状态
            '    EndOfLine -> throw
            '    "" -> inner token put ("), 保持
            '    " -> token put, State 0
            '    _ -> inner token put, 保持

            'State 2 无引号token状态
            '    EndOfLine -> token put, 结束
            '    空格 -> token put, State 0
            '    \s -> throw
            '    _ -> inner token put, 保持

            Dim ColumeIndex = IndentLevel * 4
            Static rWhitespace As New Regex("^\s$", RegexOptions.ExplicitCapture)

            Using ls As New ArrayStream(Of Char32)(Line.ToUTF32())
                Dim Tokens As New List(Of String)
                Dim InnerTokens As New List(Of Char32)

                Dim State = 0
                While True
                    Select Case State
                        Case 0
                            If ls.Position >= ls.Length Then Exit While

                            Dim c = ls.ReadElement()
                            If c = Tab Then Throw New InvalidTextFormatException("UseOnlySpaceAsWhitespace", New FileLocationInformation With {.LineNumber = s.Position + 1, .ColumnNumber = ColumeIndex + 1})
                            ColumeIndex += 1
                            If c = " " Then Continue While
                            If rWhitespace.Match(c).Success Then Throw New InvalidTextFormatException("UseOnlySpaceAsWhitespace", New FileLocationInformation With {.LineNumber = s.Position + 1, .ColumnNumber = ColumeIndex + 1 - 1})

                            Select Case c
                                Case """"
                                    State = 1
                                    Continue While
                                Case "/"
                                    If ls.Position < ls.Length AndAlso ls.PeekElement = "/" Then
                                        ls.ReadElement()
                                        ColumeIndex += 1
                                        Exit While
                                    End If
                            End Select
                            State = 2
                            InnerTokens.Add(c)
                        Case 1
                            If ls.Position >= ls.Length Then Throw New InvalidTextFormatException("", New FileLocationInformation With {.LineNumber = s.Position + 1, .ColumnNumber = ColumeIndex + 1})

                            Dim c = ls.ReadElement()
                            If c = Tab Then
                                ColumeIndex += 4
                            Else
                                ColumeIndex += 1
                            End If

                            Select Case c
                                Case """"
                                    If ls.Position < ls.Length AndAlso ls.PeekElement = """" Then
                                        ls.ReadElement()
                                        InnerTokens.Add(Char32.FromString(""""))
                                        ColumeIndex += 1
                                    Else
                                        Tokens.Add(InnerTokens.ToArray.ToUTF16B)
                                        InnerTokens.Clear()
                                        State = 0
                                    End If
                                Case Else
                                    InnerTokens.Add(c)
                            End Select
                        Case 2
                            If ls.Position >= ls.Length Then
                                Tokens.Add(InnerTokens.ToArray.ToUTF16B)
                                InnerTokens.Clear()
                                Exit While
                            End If

                            Dim c = ls.ReadElement()
                            If c = Tab Then Throw New InvalidTextFormatException("UseOnlySpaceAsWhitespace", New FileLocationInformation With {.LineNumber = s.Position + 1, .ColumnNumber = ColumeIndex + 1})
                            ColumeIndex += 1
                            If c = " " Then
                                Tokens.Add(InnerTokens.ToArray.ToUTF16B)
                                InnerTokens.Clear()
                                State = 0
                                Continue While
                            End If
                            If rWhitespace.Match(c).Success Then Throw New InvalidTextFormatException("UseOnlySpaceAsWhitespace", New FileLocationInformation With {.LineNumber = s.Position + 1, .ColumnNumber = ColumeIndex + 1 - 1})

                            InnerTokens.Add(c)
                        Case Else
                            Throw New InvalidOperationException
                    End Select
                End While

                Return Tokens.ToArray
            End Using
        End Function
    End Class

    Private Class Writer
        Public sw As StreamWriter

        Public Sub WriteNode(ByVal IndentLevel As Integer, ByVal Node As XElement)
            With Nothing
                Dim s = TryGetNodeAsSingleLineString(Node)
                If s IsNot Nothing Then
                    WriteRaw(IndentLevel, s)
                    Return
                End If
            End With

            With Nothing
                Dim s = TryGetListNodeString(Node)
                If s IsNot Nothing Then
                    WriteRaw(IndentLevel, GetNameString(Node))
                    Dim SubIndentLevel = IndentLevel + 1
                    For Each a In Node.Attributes
                        WriteAttribute(SubIndentLevel, a, Node)
                    Next
                    WriteRaw(IndentLevel + 1, List & " " & s)
                    Dim SubIndentLevel2 = IndentLevel + 2
                    For Each n In Node.Elements
                        If n.HasElements Then
                            WriteNode(SubIndentLevel2, n.Elements.Single)
                        Else
                            WriteValue(SubIndentLevel2, n.Value)
                        End If
                    Next
                    Return
                End If
            End With

            With Nothing
                If Not Node.HasElements Then
                    WriteRaw(IndentLevel, GetNameString(Node))
                    Dim SubIndentLevel = IndentLevel + 1
                    For Each a In Node.Attributes
                        WriteAttribute(SubIndentLevel, a, Node)
                    Next
                    WriteValue(SubIndentLevel, Node.Value)
                    Return
                End If
            End With

            With Nothing
                WriteRaw(IndentLevel, GetNameString(Node))
                Dim SubIndentLevel = IndentLevel + 1
                For Each a In Node.Attributes
                    WriteAttribute(SubIndentLevel, a, Node)
                Next
                For Each n In Node.Elements
                    WriteNode(SubIndentLevel, n)
                Next
            End With
        End Sub

        Public Sub WriteValue(ByVal IndentLevel As Integer, ByVal Value As String)
            Dim s = TryGetValueAsSingleLineString(Value)
            If s IsNot Nothing Then
                WriteRaw(IndentLevel, s)
                Return
            End If

            Dim Lines = Regex.Split(Value, "\r\n|\n")
            WriteRaw(IndentLevel, StringLiteral)
            Dim SubIndentLevel = IndentLevel + 1
            For Each Line In Lines
                If Line.Contains(Cr) Then Throw New InvalidDataException(Line)
                WriteRaw(SubIndentLevel, Line)
            Next
        End Sub

        Public Sub WriteAttribute(ByVal IndentLevel As Integer, ByVal Attribute As XAttribute, ByVal Node As XElement)
            WriteRaw(IndentLevel, "@" & GetNameString(Attribute.Name, Node) & " " & GetValueAsSingleLineString(Attribute.Value))
        End Sub

        Public Sub WriteRaw(ByVal IndentLevel As Integer, ByVal Value As String)
            Dim si = New String(" "c, 4 * IndentLevel)
            sw.WriteLine(si & Value)
        End Sub

        Public Shared Function TryGetNodeAsSingleLineString(ByVal Node As XElement) As String
            Dim l As New List(Of XElement)

            Dim n = Node
            While True
                l.Add(n)

                If n.HasAttributes Then Return Nothing

                If Not n.HasElements Then
                    Dim s = TryGetNodeValueAsSingleLineString(n)
                    If s Is Nothing Then Return Nothing
                    Return String.Join(" ", l.Select(Function(k) GetNameString(k)).ToArray) & " " & s
                End If

                Dim Elements = n.Elements.ToArray()
                If Elements.Length > 1 Then Return Nothing

                n = Elements.Single
            End While

            Throw New InvalidOperationException
        End Function

        Public Shared Function TryGetNodeValueAsSingleLineString(ByVal Node As XElement) As String
            If Node.IsEmpty Then Return Empty
            Return TryGetValueAsSingleLineString(Node.Value)
        End Function
        Public Shared Function TryGetValueAsSingleLineString(ByVal Value As String) As String
            If Value Is Nothing Then Return Empty
            If Value = "" Then Return """"""
            If Value.Contains(Cr) Then Return Nothing
            If Value.Contains(Lf) Then Return Nothing

            Static rWholeSpecial As New Regex(String.Join("|", "s,:<>{}[]()'""".ToCharArray().Select(Function(c) "\" & c).ToArray()), RegexOptions.ExplicitCapture)
            If rWholeSpecial.Match(Value).Success Then Return """" & Value.Replace("""", """""") & """"

            Static rHeadSpecial As New Regex("^" & String.Join("|", "!@#$%&/;=?\^`|~".ToCharArray().Select(Function(c) "\" & c).ToArray()), RegexOptions.ExplicitCapture)
            If rHeadSpecial.Match(Value).Success Then Return """" & Value.Replace("""", """""") & """"

            Return Value
        End Function
        Public Shared Function GetValueAsSingleLineString(ByVal Value As String) As String
            Dim s = TryGetValueAsSingleLineString(Value)
            If s Is Nothing Then Throw New InvalidOperationException
            Return s
        End Function

        Public Shared Function GetNameString(ByVal Name As XName, ByVal Node As XElement) As String
            Dim NamespacePrefix = Node.GetPrefixOfNamespace(Name.Namespace)
            If NamespacePrefix Is Nothing Then Return Name.LocalName
            Return NamespacePrefix & ":" & Name.LocalName
        End Function
        Public Shared Function GetNameString(ByVal Node As XElement) As String
            Return GetNameString(Node.Name, Node)
        End Function

        Public Shared Function TryGetListNodeString(ByVal Node As XElement) As String
            If Not Node.HasElements Then Return Nothing
            If Node.Elements.Count = 1 Then Return Nothing
            If Not (Node.Elements.All(Function(n) (Not n.HasElements) OrElse n.Elements.Count = 1)) Then Return Nothing

            Dim Heads = Node.Elements.Select(Function(n) GetNameString(n)).Distinct.ToArray()
            If Heads.Length <> 1 Then Return Nothing

            Return Heads.Single
        End Function
    End Class

    Private Structure NullObject
    End Structure
    Private Enum RawResultTag
        Normal
        EmptyLineWithIndentedSpaces
        EmptyLineWithoutIndentedSpaces
        EndOfBranch
        EndOfStream
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Private Class RawResult
        <Tag()> Public _Tag As RawResultTag
        Public Normal As String
        Public EmptyLineWithIndentedSpaces As String
        Public EmptyLineWithoutIndentedSpaces As NullObject
        Public EndOfBranch As NullObject
        Public EndOfStream As NullObject

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Private Enum NodeResultTag
        Element
        Elements
        Attribute
        Value
        Comment
        EndOfBranch
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Private Class NodeResult
        <Tag()> Public _Tag As NodeResultTag
        Public Element As TreeElement
        Public Elements As List(Of TreeElement)
        Public Attribute As TreeAttribute
        Public Value As TreeValue
        Public Comment As String
        Public EndOfBranch As NullObject

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
    <Record(), DebuggerDisplay("{ToString()}")>
    Private Class TreeElement
        Public Name As String
        Public Attributes As New List(Of TreeAttribute)
        Public Elements As New List(Of TreeElement)
        Public Value As TreeValue

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
    <Record(), DebuggerDisplay("{ToString()}")>
    Private Class TreeAttribute
        Public Name As String
        Public Value As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
    <[Alias](), DebuggerDisplay("{ToString()}")>
    Private Class TreeValue
        Public Value As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Private Class XElementEx
        Inherits XElement
        Implements IXmlLineInfo

        Public Sub New(ByVal Name As XName)
            MyBase.New(Name)
        End Sub
        Public Sub New(ByVal Name As XName, ByVal Content As Object)
            MyBase.New(Name, Content)
        End Sub

        Private Shadows Function HasLineInfo() As Boolean Implements IXmlLineInfo.HasLineInfo
            Return (Not Me.Annotation(Of FileLocationInformation)() Is Nothing)
        End Function

        Private ReadOnly Property LineNumber As Integer Implements IXmlLineInfo.LineNumber
            Get
                Dim annotation = Me.Annotation(Of FileLocationInformation)()
                If (Not annotation Is Nothing) Then
                    Return annotation.LineNumber
                End If
                Return 0
            End Get
        End Property

        Private ReadOnly Property LinePosition As Integer Implements IXmlLineInfo.LinePosition
            Get
                Dim annotation = Me.Annotation(Of FileLocationInformation)()
                If (Not annotation Is Nothing) Then
                    Return annotation.ColumnNumber
                End If
                Return 0
            End Get
        End Property

        Public Sub SetLineInfo(ByVal i As FileLocationInformation)
            Me.AddAnnotation(i)
        End Sub
    End Class
End Class
