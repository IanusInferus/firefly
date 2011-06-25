'==========================================================================
'
'  File:        XmlInterop.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: XML互操作
'  Version:     2011.06.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports Firefly.Texting
Imports Firefly.Texting.TreeFormat.Semantics

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
                If Root._Tag <> NodeTag.Stem Then Throw New InvalidTextFormatException("NotTree", If(i, New FileLocationInformation))

                Dim ns = Root.Stem
                Dim n = TryGetXName(ns.Name, Nothing)
                If n Is Nothing Then Throw New InvalidTextFormatException("NamingError", If(i, New FileLocationInformation))
                Dim x As New XElementEx(n)
                x.SetLineInfo(i)
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
                    ex.SetLineInfo(ei)
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
                If t._Tag <> NodeTag.Stem Then Throw New InvalidTextFormatException("NotAttribute", If(i, New FileLocationInformation))
                Dim ns = t.Stem
                If ns.Children.Length <> 1 Then Throw New InvalidTextFormatException("NotAttribute", If(i, New FileLocationInformation))
                Dim c = ns.Children.Single()
                If c._Tag <> NodeTag.Leaf Then Throw New InvalidTextFormatException("NotAttribute", If(i, New FileLocationInformation))
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
                If Not r.Range.HasValue Then Return New FileLocationInformation With {.Path = r.Path}
                Return New FileLocationInformation With {.Path = r.Path, .LineNumber = r.Range.Value.Start.Row, .ColumnNumber = r.Range.Value.Start.Column}
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

            Private Function Mark(Of T)(ByVal Obj As T, ByVal Range As Opt(Of Syntax.FileTextRange)) As T
                If Range.HasValue Then Positions.Add(Obj, Range.Value)
                Return Obj
            End Function
            Private Function MakeEmptyNode(ByVal x As XObject) As Node
                Dim Range = GetFileTextRange(x)
                Dim n = Mark(New Node With {._Tag = NodeTag.Empty}, Range)
                Return n
            End Function
            Private Function MakeLeafNode(ByVal Value As String, ByVal x As XObject) As Node
                Dim Range = GetFileTextRange(x)
                Dim n = Mark(New Node With {._Tag = NodeTag.Leaf, .Leaf = Value}, Range)
                Return n
            End Function
            Private Function MakeStemNode(ByVal Name As String, ByVal Children As Node(), ByVal x As XObject) As Node
                Dim Range = GetFileTextRange(x)
                Dim s = Mark(New Stem With {.Name = Name, .Children = Children}, Range)
                Dim n = Mark(New Node With {._Tag = NodeTag.Stem, .Stem = s}, Range)
                Return n
            End Function

            Private Function GetFileTextRange(ByVal x As XObject) As Opt(Of Syntax.FileTextRange)
                Dim i As IXmlLineInfo = x
                If Not i.HasLineInfo() Then Return Opt(Of Syntax.FileTextRange).Empty
                Dim Start As New Syntax.TextPosition With {.CharIndex = 1, .Row = i.LineNumber, .Column = i.LinePosition}
                Return New Syntax.FileTextRange With {.Path = "", .Range = New Syntax.TextRange With {.Start = Start, .End = Start}}
            End Function
        End Class
    End Class
End Namespace
