'==========================================================================
'
'  File:        MetaSchemaFsCodeGenerator.vb
'  Location:    Firefly.MetaSchemaManipulator <Visual Basic .Net>
'  Description: 元类型结构F#代码生成器
'  Version:     2011.08.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Linq
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.Mapping
Imports Firefly.Mapping.MetaSchema
Imports Firefly.Mapping.XmlText
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Tuple = Firefly.Mapping.MetaSchema.Tuple
Imports Firefly.Texting.TreeFormat

Public Module MetaSchemaFsCodeGenerator
    <Extension()> Public Function CompileToFS(ByVal Schema As Schema, ByVal NamespaceName As String) As String
        Dim w As New Writer With {.Schema = Schema, .NamespaceName = NamespaceName}
        Dim a = w.GetSchema()
        Return String.Join(CrLf, a)
    End Function

    Private Class MetaSchemaFsTemplateInfo
        Public Keywords As HashSet(Of String)
        Public PrimitiveMappings As Dictionary(Of String, PrimitiveMapping)
        Public Templates As Dictionary(Of String, Template)

        Public Sub New(ByVal Template As MetaSchemaTemplate)
            Keywords = New HashSet(Of String)(Template.Keywords, StringComparer.Ordinal)
            PrimitiveMappings = Template.PrimitiveMappings.ToDictionary(Function(m) m.Name, StringComparer.OrdinalIgnoreCase)
            Templates = Template.Templates.ToDictionary(Function(t) t.Name, StringComparer.OrdinalIgnoreCase)
        End Sub
    End Class

    Private Class Writer
        Private Shared TemplateInfo As MetaSchemaFsTemplateInfo

        Public Schema As Schema
        Public NamespaceName As String

        Shared Sub New()
            Dim b = My.Resources.MetaSchemaFsTemplate
            Dim x As XElement
            Using s As New ByteArrayStream(b)
                Using sr = Txt.CreateTextReader(s.AsNewReading, TextEncoding.Default, True)
                    x = TreeFile.ReadFile(sr)
                End Using
            End Using
            Dim xs As New XmlSerializer
            Dim t = xs.Read(Of MetaSchemaTemplate)(x)
            TemplateInfo = New MetaSchemaFsTemplateInfo(t)
        End Sub

        Public Function GetSchema() As String()
            Dim Concepts = GetConcepts(Schema)

            Return EvaluateEscapedIdentifiers(GetTemplate("Main").Substitute("NamespaceName", NamespaceName).Substitute("Concepts", Concepts))
        End Function

        Public Function GetPrimitive(ByVal Name As String, ByVal PlatformName As String) As String()
            Return GetTemplate("Primitive").Substitute("Name", Name).Substitute("PlatformName", PlatformName)
        End Function

        Public Function GetTypeString(ByVal Type As ConceptSpec) As String
            Select Case Type._Tag
                Case ConceptSpecTag.ConceptRef
                    Return Type.ConceptRef.Value
                Case ConceptSpecTag.List
                    Return GetEscapedIdentifier(GetTypeString(Type.List.ElementType)) & " list"
                Case ConceptSpecTag.Tuple
                    Dim F =
                        Function(s As String) As String
                            If s.Contains("*") Then
                                Return "(" & s & ")"
                            Else
                                Return s
                            End If
                        End Function
                    Return String.Join(" * ", Type.Tuple.Types.Select(Function(t) F(GetEscapedIdentifier(GetTypeString(t)))).ToArray())
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Public Function GetAlias(ByVal a As [Alias]) As String()
            Return GetTemplate("Alias").Substitute("Name", a.Name).Substitute("Type", GetTypeString(a.Type))
        End Function
        Public Function GetField(ByVal f As Field) As String()
            Return GetTemplate("Field").Substitute("Name", f.Name).Substitute("Type", GetTypeString(f.Type))
        End Function
        Public Function GetFields(ByVal Fields As Field()) As String()
            Dim l As New List(Of String)
            For Each f In Fields
                l.AddRange(GetField(f))
            Next
            Return l.ToArray
        End Function
        Public Function GetRecord(ByVal r As Record) As String()
            Dim Fields = GetFields(r.Fields)
            Return GetTemplate("Record").Substitute("Name", r.Name).Substitute("Fields", Fields)
        End Function
        Public Function GetAlternative(ByVal a As Alternative) As String()
            Return GetTemplate("Alternative").Substitute("Name", a.Name).Substitute("Type", GetTypeString(a.Type))
        End Function
        Public Function GetAlternatives(ByVal Alternatives As Alternative()) As String()
            Dim l As New List(Of String)
            For Each a In Alternatives
                l.AddRange(GetAlternative(a))
            Next
            Return l.ToArray
        End Function
        Public Function GetTaggedUnion(ByVal tu As TaggedUnion) As String()
            Dim Alternatives = GetAlternatives(tu.Alternatives)
            Return GetTemplate("TaggedUnion").Substitute("Name", tu.Name).Substitute("Alternatives", Alternatives)
        End Function
        Public Function GetConcepts(ByVal Schema As Schema) As String()
            Dim l As New List(Of String)

            Dim IsFirst = True

            For Each c In Schema.Concepts
                Select Case c._Tag
                    Case ConceptDefTag.Primitive
                        Dim p = c.Primitive
                        If TemplateInfo.PrimitiveMappings.ContainsKey(p.Value) Then
                            l.AddRange(ReplaceFirstLineFirstTypeWithAnd(GetPrimitive(p.Value, TemplateInfo.PrimitiveMappings(p.Value).PlatformName), IsFirst))
                        Else
                            Throw New ArgumentException("PrimitiveNotExist: {0}".Formats(p.Value))
                        End If
                    Case ConceptDefTag.Alias
                        l.AddRange(ReplaceFirstLineFirstTypeWithAnd(GetAlias(c.Alias), IsFirst))
                    Case ConceptDefTag.Record
                        l.AddRange(ReplaceFirstLineFirstTypeWithAnd(GetRecord(c.Record), IsFirst))
                    Case ConceptDefTag.TaggedUnion
                        l.AddRange(ReplaceFirstLineFirstTypeWithAnd(GetTaggedUnion(c.TaggedUnion), IsFirst))
                    Case Else
                        Throw New InvalidOperationException
                End Select
                IsFirst = False
                l.Add("")
            Next

            If l.Count > 0 Then l = l.Take(l.Count - 1).ToList

            Return l.ToArray()
        End Function

        Public Function ReplaceFirstLineFirstTypeWithAnd(ByVal Lines As String(), ByVal IsFirst As Boolean) As String()
            If IsFirst Then Return Lines
            If Lines.Length = 0 Then Return Lines
            Dim FirstLine = Lines.First
            If Not FirstLine.StartsWith("type") Then Return Lines
            Return (New String() {"and" & New String(FirstLine.Skip(4).ToArray())}).Concat(Lines.Skip(1)).ToArray()
        End Function

        Public Function GetTemplate(ByVal Name As String) As String()
            Return GetLines(TemplateInfo.Templates(Name).Value)
        End Function
        Public Function GetLines(ByVal Value As String) As String()
            Return Value.UnifyNewLineToLf.Split(Lf)
        End Function
        Public Function GetEscapedIdentifier(ByVal Identifier As String) As String
            Dim l As New List(Of String)
            For Each IdentifierPart In Identifier.Split("."c)
                If TemplateInfo.Keywords.Contains(IdentifierPart) Then
                    l.Add("``" & IdentifierPart & "``")
                Else
                    l.Add(IdentifierPart)
                End If
            Next
            Return String.Join(".", l.ToArray())
        End Function
        Private rIdentifier As New Regex("(?<!\[\[)\[\[(?<Identifier>.*?)\]\](?!\]\])", RegexOptions.ExplicitCapture)
        Private Function EvaluateEscapedIdentifiers(ByVal Lines As String()) As String()
            Return Lines.Select(Function(Line) rIdentifier.Replace(Line, Function(s) GetEscapedIdentifier(s.Result("${Identifier}"))).Replace("[[[[", "[[").Replace("]]]]", "]]")).ToArray()
        End Function
    End Class

    <Extension()> Private Function Substitute(ByVal Lines As String(), ByVal Parameter As String, ByVal Value As String) As String()
        Dim l As New List(Of String)
        For Each Line In Lines
            Dim ParameterString = "${" & Parameter & "}"
            If Line.Contains(ParameterString) Then
                l.Add(Line.Replace(ParameterString, Value))
            Else
                l.Add(Line)
            End If
        Next
        Return l.ToArray()
    End Function
    <Extension()> Private Function Substitute(ByVal Lines As String(), ByVal Parameter As String, ByVal Value As String()) As String()
        Dim l As New List(Of String)
        For Each Line In Lines
            Dim ParameterString = "${" & Parameter & "}"
            If Line.Contains(ParameterString) Then
                For Each vLine In Value
                    l.Add(Line.Replace(ParameterString, vLine))
                Next
            Else
                l.Add(Line)
            End If
        Next
        Return l.ToArray()
    End Function
End Module
