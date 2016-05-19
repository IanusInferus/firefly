'==========================================================================
'
'  File:        XmlInterop.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: XML互操作
'  Version:     2016.05.19.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Texting.TreeFormat.Semantics
Imports Syntax = Firefly.Texting.TreeFormat.Syntax

Namespace Texting.TreeFormat
    Public NotInheritable Class XmlInterop
        Private Sub New()
        End Sub

        Public Shared Function TreeToXml(ByVal EvaluateResult As TreeFormatResult) As XElement
            Dim txt As New TreeToXmlTranslator(EvaluateResult)
            Return txt.Translate()
        End Function
        Public Shared Function TreeToXml(ByVal Tree As Forest) As XElement
            Dim txt As New TreeToXmlTranslator(Tree)
            Return txt.Translate()
        End Function

        Public Shared Function XmlToTree(ByVal x As XElement) As TreeFormatResult
            Dim xtt As New XmlToTreeTranslator(x)
            Return xtt.Translate()
        End Function

        Public Shared Function XmlToTreeRaw(ByVal x As XElement) As TreeFormatParseResult
            Dim xtt As New XmlToTreeRawTranslator(x)
            Return xtt.Translate()
        End Function

        Private Class TreeToXmlTranslator
            Private Value As Forest
            Private Positions As Dictionary(Of Object, Syntax.FileTextRange)

            Public Sub New(ByVal EvaluateResult As TreeFormatResult)
                Value = EvaluateResult.Value
                Positions = EvaluateResult.Positions
            End Sub
            Public Sub New(ByVal Value As Forest)
                Me.Value = Value
                Positions = Nothing
            End Sub

            Public Function Translate() As XElement
                Dim i = GetFileLocationInformation(Value)

                If Value.Nodes.Length <> 1 Then Throw New InvalidTextFormatException("NotTree", If(i, New FileLocationInformation))
                Dim Root = Value.Nodes.Single()
                If Not Root.OnStem Then Throw New InvalidTextFormatException("NotTree", If(i, New FileLocationInformation))

                Dim ns = Root.Stem
                Dim n = TryGetXName(ns.Name, Nothing)
                If n Is Nothing Then Throw New InvalidTextFormatException("NamingError", If(i, New FileLocationInformation))
                Dim x As New XElementEx(n)
                If i IsNot Nothing Then x.SetLineInfo(i)
                FillElement(ns, x)
                Return x
            End Function

            Private Sub FillElement(ByVal t As Stem, ByVal x As XElement)
                Dim i = GetFileLocationInformation(t)

                Dim XmlnsAttributes As New List(Of Node)
                Dim Attributes As New List(Of Node)
                Dim Elements As New List(Of Node)
                Dim Values As New List(Of Node)
                For Each n In t.Children
                    Select Case n._Tag
                        Case NodeTag.Empty, NodeTag.Leaf
                            Values.Add(n)
                        Case NodeTag.Stem
                            Dim Name = n.Stem.Name
                            If Name.StartsWith("@xml:") OrElse Name.StartsWith("@xmlns:") Then
                                XmlnsAttributes.Add(n)
                                Continue For
                            End If
                            If Name.StartsWith("@") Then
                                Attributes.Add(n)
                                Continue For
                            End If
                            Elements.Add(n)
                        Case Else
                            Throw New ArgumentException
                    End Select
                Next
                If Elements.Count > 0 AndAlso Values.Count > 0 Then Throw New InvalidTextFormatException("NotTree", If(i, New FileLocationInformation))
                If Values.Count > 1 Then Throw New InvalidTextFormatException("NotTree", If(i, New FileLocationInformation))
                For Each a In XmlnsAttributes
                    Dim xa = GetAttribute(a, x)
                    x.SetAttributeValue(xa.Name, xa.Value)
                Next
                For Each a In Attributes
                    Dim xa = GetAttribute(a, x)
                    x.SetAttributeValue(xa.Name, xa.Value)
                Next
                For Each e In Elements
                    Dim ns = e.Stem
                    Dim ei = GetFileLocationInformation(ns)
                    Dim n = TryGetXName(ns.Name, Nothing)
                    If n Is Nothing Then Throw New InvalidTextFormatException("NamingError", If(ei, New FileLocationInformation))
                    Dim ex As New XElementEx(n)
                    If ei IsNot Nothing Then ex.SetLineInfo(ei)
                    x.Add(ex)
                    FillElement(ns, ex)
                Next
                If Values.Count = 1 Then
                    Dim c = Values.Single()
                    Select Case c._Tag
                        Case NodeTag.Empty

                        Case NodeTag.Leaf
                            x.Value = c.Leaf
                        Case Else
                            Throw New InvalidOperationException
                    End Select
                ElseIf Elements.Count = 0 AndAlso Values.Count = 0 Then
                    x.Value = ""
                End If
            End Sub

            Private Function GetAttribute(ByVal t As Node, ByVal Parent As XElement) As XAttribute
                Dim i = GetFileLocationInformation(t)
                If Not t.OnStem Then Throw New InvalidTextFormatException("NotAttribute", If(i, New FileLocationInformation))
                Dim ns = t.Stem
                If ns.Children.Length <> 1 Then Throw New InvalidTextFormatException("NotAttribute", If(i, New FileLocationInformation))
                Dim c = ns.Children.Single()
                If Not c.OnLeaf Then Throw New InvalidTextFormatException("NotAttribute", If(i, New FileLocationInformation))
                Dim xa As New XAttribute(TryGetXName(ns.Name.Substring(1), Parent), c.Leaf)
                Return xa
            End Function

            Private Shared Function TryGetXName(ByVal Name As String, ByVal Node As XElement) As XName
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

            Private Function GetFileLocationInformation(ByVal Obj As Object) As FileLocationInformation
                If Positions Is Nothing Then Return Nothing
                If Not Positions.ContainsKey(Obj) Then Return Nothing
                Dim r = Positions(Obj)
                If Not r.Range.HasValue Then Return New FileLocationInformation With {.Path = r.Text.Path}
                Return New FileLocationInformation With {.Path = r.Text.Path, .LineNumber = r.Range.Value.Start.Row, .ColumnNumber = r.Range.Value.Start.Column}
            End Function
        End Class

        Private Class XElementEx
            Inherits XElement
            Implements IXmlLineInfo
            Implements IFileLocationInformationProvider

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

            Private ReadOnly Property FileLocationInformation As FileLocationInformation Implements IFileLocationInformationProvider.FileLocationInformation
                Get
                    Dim annotation = Me.Annotation(Of FileLocationInformation)()
                    If (Not annotation Is Nothing) Then
                        Return annotation
                    End If
                    Return New FileLocationInformation
                End Get
            End Property
        End Class

        Private Class XmlToTreeTranslator
            Private Value As XElement
            Private Positions As Dictionary(Of Object, Syntax.FileTextRange)

            Public Sub New(ByVal Value As XElement)
                Me.Value = Value
                Positions = New Dictionary(Of Object, Syntax.FileTextRange)
            End Sub

            Public Function Translate() As TreeFormatResult
                Dim e = TranslateElement(Value)
                Return New TreeFormatResult With {.Value = New Forest With {.Nodes = {e}}, .Positions = Positions}
            End Function

            Private Function TranslateElement(ByVal xe As XElement) As Node
                Dim Attributes As New List(Of Node)
                Dim Elements As New List(Of Node)

                For Each a In xe.Attributes
                    Attributes.Add(TranslateAttribute(a, xe))
                Next

                For Each e In xe.Elements
                    Elements.Add(TranslateElement(e))
                Next

                If Elements.Count > 0 Then
                    Return MakeStemNode(GetNameString(xe), Attributes.Concat(Elements).ToArray(), xe)
                End If

                If Attributes.Count > 0 Then
                    If xe.IsEmpty Then
                        Return MakeStemNode(GetNameString(xe), Attributes.Concat({MakeEmptyNode(xe)}).ToArray(), xe)
                    Else
                        Return MakeStemNode(GetNameString(xe), Attributes.Concat({MakeLeafNode(xe.Value, xe)}).ToArray(), xe)
                    End If
                End If

                If xe.IsEmpty Then
                    Return MakeStemNode(GetNameString(xe), {MakeEmptyNode(xe)}, xe)
                Else
                    Return MakeStemNode(GetNameString(xe), {MakeLeafNode(xe.Value, xe)}.ToArray(), xe)
                End If
            End Function

            Private Function TranslateAttribute(ByVal xa As XAttribute, ByVal xe As XElement) As Node
                Return MakeStemNode("@" & GetNameString(xa.Name, xe), {MakeLeafNode(xa.Value, xa)}, xa)
            End Function

            Private Shared Function GetNameString(ByVal Name As XName, ByVal Node As XElement) As String
                Dim NamespacePrefix = Node.GetPrefixOfNamespace(Name.Namespace)
                If NamespacePrefix Is Nothing Then Return Name.LocalName
                Return NamespacePrefix & ":" & Name.LocalName
            End Function
            Private Shared Function GetNameString(ByVal Node As XElement) As String
                Return GetNameString(Node.Name, Node)
            End Function

            Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As [Optional](Of Syntax.FileTextRange)) As T
                If Range.HasValue Then Positions.Add(Obj, Range.Value)
                Return Obj
            End Function
            Private Function MakeEmptyNode(ByVal x As XObject) As Node
                Dim Range = GetFileTextRange(x)
                Dim n = Mark(Node.CreateEmpty(), Range)
                Return n
            End Function
            Private Function MakeLeafNode(ByVal Value As String, ByVal x As XObject) As Node
                Dim Range = GetFileTextRange(x)
                Dim n = Mark(Node.CreateLeaf(Value), Range)
                Return n
            End Function
            Private Function MakeStemNode(ByVal Name As String, ByVal Children As Node(), ByVal x As XObject) As Node
                Dim Range = GetFileTextRange(x)
                Dim s = Mark(New Stem With {.Name = Name, .Children = Children}, Range)
                Dim n = Mark(Node.CreateStem(s), Range)
                Return n
            End Function

            Private Function GetFileTextRange(ByVal x As XObject) As [Optional](Of Syntax.FileTextRange)
                Dim i As New FileLocationInformation
                Dim flip = TryCast(x, IFileLocationInformationProvider)
                If flip IsNot Nothing Then
                    i = flip.FileLocationInformation
                Else
                    Dim li = DirectCast(x, IXmlLineInfo)
                    If li.HasLineInfo() Then
                        i.LineNumber = li.LineNumber
                        i.ColumnNumber = li.LinePosition
                    End If
                End If
                Dim Start As New Syntax.TextPosition With {.CharIndex = 1, .Row = i.LineNumber, .Column = i.ColumnNumber}
                Dim Range As New Syntax.TextRange With {.Start = Start, .End = Start}
                Return New Syntax.FileTextRange With {.Text = New Syntax.Text With {.Path = "", .Lines = New Syntax.TextLine() {}}, .Range = Range}
            End Function
        End Class

        Private Class XmlToTreeRawTranslator
            Private Value As XElement
            Private Positions As Dictionary(Of Object, Syntax.TextRange)

            Public Sub New(ByVal Value As XElement)
                Me.Value = Value
                Positions = New Dictionary(Of Object, Syntax.TextRange)
            End Sub

            Public Function Translate() As TreeFormatParseResult
                Dim e = TranslateElement(Value)
                Dim Range = GetFileTextRange(Value)
                Return New TreeFormatParseResult With {.Value = New Syntax.Forest With {.MultiNodesList = {Mark(Syntax.MultiNodes.CreateNode(e), Range)}}, .Positions = Positions}
            End Function

            Private Function TranslateElement(ByVal xe As XElement) As Syntax.Node
                Dim Attributes As New List(Of Syntax.Node)
                Dim Elements As New List(Of Syntax.Node)
                Dim Children As New List(Of Syntax.Node)

                For Each a In xe.Attributes
                    Dim ta = TranslateAttribute(a, xe)
                    Attributes.Add(ta)
                    Children.Add(ta)
                Next

                If xe.Attributes.Count = 0 AndAlso (Not xe.Nodes.Any(Function(n) n.NodeType = XmlNodeType.Comment)) AndAlso xe.Elements.Count > 1 Then
                    If xe.Elements.All(Function(c) c.Attributes.Count = 0 AndAlso (Not c.Nodes.Any(Function(n) n.NodeType = XmlNodeType.Comment)) AndAlso (c.Elements.Count = 0 OrElse c.Elements.Count = 1)) Then
                        Dim ChildNames = xe.Elements.Select(Function(c) c.Name.LocalName).Distinct.ToArray()
                        If ChildNames.Length = 1 Then
                            Dim ChildElements As New List(Of Syntax.Node)
                            For Each n In xe.Nodes
                                If n.NodeType = XmlNodeType.Element Then
                                    Dim e = DirectCast(n, XElement)
                                    Dim tce As Syntax.Node
                                    If e.Elements.Count = 0 Then
                                        If e.IsEmpty Then
                                            tce = MakeEmptyNode(e)
                                        Else
                                            tce = MakeLeafNode(e.Value, e)
                                        End If
                                    Else
                                        tce = TranslateElement(e.Elements.Single())
                                    End If
                                    ChildElements.Add(tce)
                                End If
                            Next
                            Return MakeStemNodeOfList(GetNameString(xe), ChildNames.Single(), ChildElements.ToArray(), xe)
                        End If
                    End If
                End If

                For Each n In xe.Nodes
                    If n.NodeType = XmlNodeType.Element Then
                        Dim e = DirectCast(n, XElement)
                        Dim te = TranslateElement(e)
                        Elements.Add(te)
                        Children.Add(te)
                    ElseIf n.NodeType = XmlNodeType.Comment Then
                        Dim c = DirectCast(n, XComment)
                        Dim tc = TranslateComment(c)
                        Children.Add(tc)
                    End If
                Next

                If Elements.Count > 0 Then
                    Return MakeStemNode(GetNameString(xe), Children.ToArray(), xe)
                End If

                If Attributes.Count > 0 Then
                    If xe.IsEmpty Then
                        Return MakeStemNode(GetNameString(xe), Attributes.Concat({MakeEmptyNode(xe)}).ToArray(), xe)
                    Else
                        Return MakeStemNode(GetNameString(xe), Attributes.Concat({MakeLeafNode(xe.Value, xe)}).ToArray(), xe)
                    End If
                End If

                If xe.IsEmpty Then
                    Return MakeStemNode(GetNameString(xe), {MakeEmptyNode(xe)}, xe)
                Else
                    Return MakeStemNode(GetNameString(xe), {MakeLeafNode(xe.Value, xe)}.ToArray(), xe)
                End If
            End Function

            Private Function TranslateAttribute(ByVal xa As XAttribute, ByVal xe As XElement) As Syntax.Node
                Return MakeStemNode("@" & GetNameString(xa.Name, xe), {MakeLeafNode(xa.Value, xa)}, xa)
            End Function

            Private Function TranslateComment(ByVal xc As XComment) As Syntax.Node
                Dim Value = xc.Value
                Dim Range = GetFileTextRange(xc)
                Dim Literal = TreeFormatLiteralWriter.GetLiteral(Value, False, False)
                If Literal.OnSingleLine Then
                    Dim n = Mark(Syntax.Node.CreateSingleLineComment(Mark(New Syntax.SingleLineComment With {.Content = Mark(New Syntax.FreeContent With {.Text = Value}, Range)}, Range)), Range)
                    Return n
                ElseIf Literal.OnMultiLine Then
                    Dim n = Mark(Syntax.Node.CreateMultiLineComment(Mark(New Syntax.MultiLineComment With {.SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty, .Content = New Syntax.FreeContent With {.Text = Value}, .EndDirective = [Optional](Of Syntax.EndDirective).Empty}, Range)), Range)
                    Return n
                Else
                    Throw New InvalidOperationException
                End If
            End Function

            Private Shared Function GetNameString(ByVal Name As XName, ByVal Node As XElement) As String
                Dim NamespacePrefix = Node.GetPrefixOfNamespace(Name.Namespace)
                If NamespacePrefix Is Nothing Then Return Name.LocalName
                Return NamespacePrefix & ":" & Name.LocalName
            End Function
            Private Shared Function GetNameString(ByVal Node As XElement) As String
                Return GetNameString(Node.Name, Node)
            End Function

            Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As [Optional](Of Syntax.FileTextRange)) As T
                If Range.HasValue AndAlso Range.Value.Range.HasValue Then Positions.Add(Obj, Range.Value.Range.Value)
                Return Obj
            End Function
            Private Function MakeEmptyNode(ByVal x As XObject) As Syntax.Node
                Dim Range = GetFileTextRange(x)
                Dim EmptyNode = Mark(Syntax.SingleLineNode.CreateEmptyNode(Mark(New Syntax.EmptyNode(), Range)), Range)
                Dim n = Mark(Syntax.Node.CreateSingleLineNodeLine(Mark(New Syntax.SingleLineNodeLine With {.SingleLineNode = EmptyNode, .SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty}, Range)), Range)
                Return n
            End Function
            Private Function MakeLeafNode(ByVal Value As String, ByVal x As XObject) As Syntax.Node
                Dim Range = GetFileTextRange(x)
                Dim Literal = TreeFormatLiteralWriter.GetLiteral(Value, False, False)
                If Literal.OnSingleLine Then
                    Dim LeafNode = Mark(Syntax.SingleLineNode.CreateSingleLineLiteral(Mark(New Syntax.SingleLineLiteral With {.Text = Value}, Range)), Range)
                    Dim n = Mark(Syntax.Node.CreateSingleLineNodeLine(Mark(New Syntax.SingleLineNodeLine With {.SingleLineNode = LeafNode, .SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty}, Range)), Range)
                    Return n
                ElseIf Literal.OnMultiLine Then
                    Dim n = Mark(Syntax.Node.CreateMultiLineLiteral(Mark(New Syntax.MultiLineLiteral With {.SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty, .Content = New Syntax.FreeContent With {.Text = Value}, .EndDirective = [Optional](Of Syntax.EndDirective).Empty}, Range)), Range)
                    Return n
                Else
                    Throw New InvalidOperationException
                End If
            End Function
            Private Function MakeStemNode(ByVal Name As String, ByVal Children As Syntax.Node(), ByVal x As XObject) As Syntax.Node
                Dim Range = GetFileTextRange(x)
                Dim NameLiteral = Mark(New Syntax.SingleLineLiteral With {.Text = Name}, Range)
                If Children.Length = 0 Then
                    Dim n = Mark(Syntax.Node.CreateMultiLineNode(Mark(New Syntax.MultiLineNode With {.Head = NameLiteral, .SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty, .Children = New Syntax.MultiNodes() {}, .EndDirective = Mark(New Syntax.EndDirective With {.EndSingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty}, Range)}, Range)), Range)
                    Return n
                ElseIf Children.Length = 1 AndAlso Children.Single().OnSingleLineNodeLine Then
                    Dim SingleLineNode = Mark(Syntax.SingleLineNode.CreateSingleLineNodeWithParameters(Mark(New Syntax.SingleLineNodeWithParameters With {.Head = NameLiteral, .Children = New Syntax.ParenthesisNode() {}, .LastChild = Children.Single().SingleLineNodeLine.SingleLineNode}, Range)), Range)
                    Dim n = Mark(Syntax.Node.CreateSingleLineNodeLine(Mark(New Syntax.SingleLineNodeLine With {.SingleLineNode = SingleLineNode, .SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty}, Range)), Range)
                    Return n
                Else
                    Dim ChildrenNodes = Children.Select(Function(c) Mark(Syntax.MultiNodes.CreateNode(c), Range)).ToArray()
                    Dim n = Mark(Syntax.Node.CreateMultiLineNode(Mark(New Syntax.MultiLineNode With {.Head = NameLiteral, .SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty, .Children = ChildrenNodes, .EndDirective = [Optional](Of Syntax.EndDirective).Empty}, Range)), Range)
                    Return n
                End If
            End Function
            Private Function MakeStemNodeOfList(ByVal Name As String, ByVal ListName As String, ByVal Children As Syntax.Node(), ByVal x As XObject) As Syntax.Node
                Dim Range = GetFileTextRange(x)
                Dim NameLiteral = Mark(New Syntax.SingleLineLiteral With {.Text = Name}, Range)
                Dim ListNameLiteral = Mark(New Syntax.SingleLineLiteral With {.Text = ListName}, Range)
                Dim ChildrenNodes = Children.Select(Function(c) Mark(Syntax.MultiNodes.CreateNode(c), Range)).ToArray()
                Dim ListNode = Mark(Syntax.MultiNodes.CreateListNodes(Mark(New Syntax.ListNodes With {.ChildHead = ListNameLiteral, .SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty, .Children = ChildrenNodes, .EndDirective = [Optional](Of Syntax.EndDirective).Empty}, Range)), Range)
                Dim n = Mark(Syntax.Node.CreateMultiLineNode(Mark(New Syntax.MultiLineNode With {.Head = NameLiteral, .SingleLineComment = [Optional](Of Syntax.SingleLineComment).Empty, .Children = New Syntax.MultiNodes() {ListNode}, .EndDirective = [Optional](Of Syntax.EndDirective).Empty}, Range)), Range)
                Return n
            End Function

            Private Function GetFileTextRange(ByVal x As XObject) As [Optional](Of Syntax.FileTextRange)
                Dim i As New FileLocationInformation
                Dim flip = TryCast(x, IFileLocationInformationProvider)
                If flip IsNot Nothing Then
                    i = flip.FileLocationInformation
                Else
                    Dim li = DirectCast(x, IXmlLineInfo)
                    If li.HasLineInfo() Then
                        i.LineNumber = li.LineNumber
                        i.ColumnNumber = li.LinePosition
                    End If
                End If
                Dim Start As New Syntax.TextPosition With {.CharIndex = 1, .Row = i.LineNumber, .Column = i.ColumnNumber}
                Dim Range As New Syntax.TextRange With {.Start = Start, .End = Start}
                Return New Syntax.FileTextRange With {.Text = New Syntax.Text With {.Path = "", .Lines = New Syntax.TextLine() {}}, .Range = Range}
            End Function
        End Class
    End Class
End Namespace
