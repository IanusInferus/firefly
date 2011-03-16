﻿'==========================================================================
'
'  File:        MetaSchemaVbCodeGenerator.vb
'  Location:    Firefly.MetaSchemaManipulator <Visual Basic .Net>
'  Description: 元类型结构VB代码生成器
'  Version:     2011.03.16.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Linq
Imports System.Runtime.CompilerServices
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.Mapping
Imports Firefly.Mapping.MetaSchema
Imports Firefly.Mapping.XmlText
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Tuple = Firefly.Mapping.MetaSchema.Tuple

Public Module MetaSchemaVbCodeGenerator
    <Extension()> Public Function CompileToVB(ByVal Schema As Schema, ByVal NamespaceName As String) As String
        Dim w As New Writer With {.Schema = Schema, .NamespaceName = NamespaceName}
        Dim a = w.GetSchema()
        Return String.Join(CrLf, a)
    End Function
    <Extension()> Public Function CompileForVB(ByVal Schema As Schema) As String
        Return CompileToVB(Schema, "")
    End Function

    Private Class MetaSchemaVbTemplateInfo
        Public Keywords As HashSet(Of String)
        Public PrimitiveMappings As Dictionary(Of String, PrimitiveMapping)
        Public Templates As Dictionary(Of String, Template)

        Public Sub New(Template As MetaSchemaVbTemplate)
            Keywords = New HashSet(Of String)(Template.Keywords, StringComparer.OrdinalIgnoreCase)
            PrimitiveMappings = Template.PrimitiveMappings.ToDictionary(Function(m) m.Name, StringComparer.OrdinalIgnoreCase)
            Templates = Template.Templates.ToDictionary(Function(t) t.Name, StringComparer.OrdinalIgnoreCase)
        End Sub
    End Class

    Private Class Writer
        Private Shared TemplateInfo As MetaSchemaVbTemplateInfo

        Public Schema As Schema
        Public NamespaceName As String

        Shared Sub New()
            Dim b = My.Resources.MetaSchemaVbTemplate
            Dim x As XElement
            Using s As New ByteArrayStream(b)
                Using sr = Txt.CreateTextReader(s.AsNewReading, TextEncoding.Default, True)
                    x = TreeFile.ReadFile(sr)
                End Using
            End Using
            Dim xs As New XmlSerializer
            Dim t = xs.Read(Of MetaSchemaVbTemplate)(x)
            TemplateInfo = New MetaSchemaVbTemplateInfo(t)
        End Sub

        Public Function GetSchema() As String()
            Dim Header = GetHeader()
            Dim Primitives = GetPrimitives()
            Dim ComplexConcepts = GetComplexConcepts(Schema)

            If NamespaceName <> "" Then
                Return GetTemplate("MainWithNamespace").Substitute("Header", Header).Substitute("NamespaceName", GetEscapedIdentifier(NamespaceName)).Substitute("Primitives", Primitives).Substitute("ComplexConcepts", ComplexConcepts)
            Else
                Return GetTemplate("MainWithoutNamespace").Substitute("Header", Header).Substitute("Primitives", Primitives).Substitute("ComplexConcepts", ComplexConcepts)
            End If
        End Function

        Public Function GetHeader() As String()
            Return GetTemplate("Header")
        End Function

        Public Function GetPrimitive(Name As String, PlatformName As String)
            Return GetTemplate("Primitive").Substitute("Name", GetEscapedIdentifier(Name)).Substitute("PlatformName", PlatformName)
        End Function
        Public Function GetPrimitives() As String()
            Dim l As New List(Of String)

            For Each p In Schema.Concepts.Where(Function(c) c._Tag = ConceptDefTag.Primitive).Select(Function(c) c.Primitive)
                If TemplateInfo.PrimitiveMappings.ContainsKey(p.Value) Then
                    l.AddRange(GetPrimitive(p.Value, TemplateInfo.PrimitiveMappings(p.Value).PlatformName))
                Else
                    Throw New ArgumentException("PrimitiveNotExist: {0}".Formats(p.Value))
                End If
            Next
            Return l.ToArray()
        End Function

        Private Tuples As New List(Of String)
        Private TupleDict As New Dictionary(Of String, Tuple)
        Public Function GetTypeFriendlyName(Type As ConceptSpec) As String
            Select Case Type._Tag
                Case ConceptSpecTag.ConceptRef
                    Return Type.ConceptRef.Value
                Case ConceptSpecTag.List
                    Return "ListOf" & GetTypeFriendlyName(Type.List.ElementType)
                Case ConceptSpecTag.Tuple
                    Dim tt = Type.Tuple
                    Dim Name = "TupleOf" & String.Join("And", tt.Types.Select(Function(t) GetTypeFriendlyName(t)).ToArray)
                    If Not TupleDict.ContainsKey(Name) Then
                        Tuples.Add(Name)
                        TupleDict.Add(Name, tt)
                    End If
                    Return Name
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Public Function GetTypeString(Type As ConceptSpec) As String
            Select Case Type._Tag
                Case ConceptSpecTag.List
                    Return GetEscapedIdentifier(GetTypeFriendlyName(Type.List.ElementType)) & "()"
                Case ConceptSpecTag.ConceptRef, ConceptSpecTag.Tuple
                    Return GetEscapedIdentifier(GetTypeFriendlyName(Type))
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Public Function GetAlias(a As [Alias]) As String()
            Return GetTemplate("Alias").Substitute("Name", GetEscapedIdentifier(a.Name)).Substitute("Type", GetTypeString(a.Type))
        End Function
        Public Function GetTupleElement(NameIndex As Int64, Type As ConceptSpec) As String()
            Return GetTemplate("TupleElement").Substitute("NameIndex", NameIndex.ToString(Globalization.CultureInfo.InvariantCulture)).Substitute("Type", GetTypeString(Type))
        End Function
        Public Function GetTupleElements(ByVal Types As ConceptSpec()) As String()
            Dim l As New List(Of String)
            Dim n = 0
            For Each e In Types
                l.AddRange(GetTupleElement(n, e))
                n += 1
            Next
            Return l.ToArray
        End Function
        Public Function GetTuple(Name As String, t As Tuple) As String()
            Dim TupleElements = GetTupleElements(t.Types)
            Return GetTemplate("Tuple").Substitute("Name", GetEscapedIdentifier(Name)).Substitute("TupleElements", TupleElements)
        End Function
        Public Function GetField(f As Field) As String()
            Return GetTemplate("Field").Substitute("Name", GetEscapedIdentifier(f.Name)).Substitute("Type", GetTypeString(f.Type))
        End Function
        Public Function GetFields(ByVal Fields As Field()) As String()
            Dim l As New List(Of String)
            For Each f In Fields
                l.AddRange(GetField(f))
            Next
            Return l.ToArray
        End Function
        Public Function GetRecord(r As Record) As String()
            Dim Fields = GetFields(r.Fields)
            Return GetTemplate("Record").Substitute("Name", GetEscapedIdentifier(r.Name)).Substitute("Fields", Fields)
        End Function
        Public Function GetAlternativeNames(ByVal Alternatives As Alternative()) As String()
            Dim l As New List(Of String)
            For Each a In Alternatives
                l.Add(GetEscapedIdentifier(a.Name))
            Next
            Return l.ToArray
        End Function
        Public Function GetAlternative(a As Alternative) As String()
            Return GetTemplate("Alternative").Substitute("Name", GetEscapedIdentifier(a.Name)).Substitute("Type", GetTypeString(a.Type))
        End Function
        Public Function GetAlternatives(ByVal Alternatives As Alternative()) As String()
            Dim l As New List(Of String)
            For Each a In Alternatives
                l.AddRange(GetAlternative(a))
            Next
            Return l.ToArray
        End Function
        Public Function GetTaggedUnion(tu As TaggedUnion) As String()
            Dim AlternativeNames = GetAlternativeNames(tu.Alternatives)
            Dim Alternatives = GetAlternatives(tu.Alternatives)
            Return GetTemplate("TaggedUnion").Substitute("Name", GetEscapedIdentifier(tu.Name)).Substitute("AlternativeNames", AlternativeNames).Substitute("Alternatives", Alternatives)
        End Function
        Public Function GetComplexConcepts(ByVal Schema As Schema) As String()
            Dim l As New List(Of String)

            For Each c In Schema.Concepts
                Select Case c._Tag
                    Case ConceptDefTag.Primitive
                        Continue For
                    Case ConceptDefTag.Alias
                        l.AddRange(GetAlias(c.Alias))
                    Case ConceptDefTag.Record
                        l.AddRange(GetRecord(c.Record))
                    Case ConceptDefTag.TaggedUnion
                        l.AddRange(GetTaggedUnion(c.TaggedUnion))
                    Case Else
                        Throw New InvalidOperationException
                End Select
                l.Add("")
            Next

            For Each t In Tuples
                l.AddRange(GetTuple(t, TupleDict(t)))
                l.Add("")
            Next

            If l.Count > 0 Then l = l.Take(l.Count - 1).ToList

            Return l.ToArray()
        End Function

        Public Function GetTemplate(Name As String) As String()
            Return GetLines(TemplateInfo.Templates(Name).Value)
        End Function
        Public Function GetLines(Value As String) As String()
            Return Value.UnifyNewLineToLf.Split(Lf)
        End Function
        Public Function GetEscapedIdentifier(Identifier As String) As String
            If TemplateInfo.Keywords.Contains(Identifier) Then
                Return "[" & Identifier & "]"
            Else
                Return Identifier
            End If
        End Function
    End Class

    <Extension()> Private Function Substitute(ByVal Lines As String(), Parameter As String, Value As String) As String()
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
    <Extension()> Private Function Substitute(ByVal Lines As String(), Parameter As String, Value As String()) As String()
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
