'==========================================================================
'
'  File:        UniHanDatabase.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: UniHan数据库
'  Version:     2009.10.20.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Linq
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting

''' <summary>
''' 本类用于遍历UniHan数据库。
''' </summary>
''' <remarks>
''' UniHan数据库可从如下地址获取。
''' ftp://ftp.unicode.org/Public/UNIDATA/Unihan.zip
'''
''' Unicode委员会的技术报告可从如下地址获取。
''' Unicode Standard Annex #38
''' http://www.unicode.org/reports/tr38/
''' </remarks>
Public Class UniHanDatabase
    Private CharDict As New Dictionary(Of Char32, List(Of UniHanDouble))

    Public Sub New()
    End Sub

    Public Sub Load(ByVal Path As String, ByVal TriplePredicate As Predicate(Of UniHanTriple))
        Dim r As New Regex("^U\+(?<Unicode>[0-9A-F]{4,5})\t(?<FieldType>[0-9A-Za-z_]+)\t(?<Value>.*)$", RegexOptions.ExplicitCapture)
        Using sr = Txt.CreateTextReader(Path, UTF8)
            While Not sr.EndOfStream
                Dim Line As String = sr.ReadLine().TrimStart(" "c)
                If Line.StartsWith("#") Then Continue While
                If Line = "" Then Continue While

                Dim m = r.Match(Line)
                If Not m.Success Then Throw New InvalidDataException("{0}: {1}".Formats(Path, Line))

                Dim Unicode = New Char32(Integer.Parse(m.Result("${Unicode}"), Globalization.NumberStyles.HexNumber))
                Dim FieldType = m.Result("${FieldType}")
                Dim Value = m.Result("${Value}")

                Dim t = New UniHanTriple(Unicode, FieldType, Value)
                If TriplePredicate(t) Then
                    If CharDict.ContainsKey(Unicode) Then
                        CharDict(Unicode).Add(New UniHanDouble(FieldType, Value))
                    Else
                        CharDict.Add(Unicode, New List(Of UniHanDouble)(New UniHanDouble() {New UniHanDouble(FieldType, Value)}))
                    End If
                End If
            End While
        End Using
    End Sub
    Public Sub Load(ByVal Path As String, ByVal FirstFieldType As String, ByVal ParamArray FieldTypes As String())
        Dim r As New Regex("^U\+(?<Unicode>2?[0-9A-F]{4})\t(?<FieldType>[0-9A-Za-z_]+)\t(?<Value>.*)$", RegexOptions.ExplicitCapture)
        Dim ft As New HashSet(Of String)
        ft.Add(FirstFieldType)
        For Each f In FieldTypes
            ft.Add(f)
        Next
        Using sr = Txt.CreateTextReader(Path, UTF8)
            While Not sr.EndOfStream
                Dim Line As String = sr.ReadLine().TrimStart(" "c)
                If Line.StartsWith("#") Then Continue While
                If Line = "" Then Continue While

                Dim m = r.Match(Line)
                If Not m.Success Then Throw New InvalidDataException("{0}: {1}".Formats(Path, Line))

                Dim FieldType = m.Result("${FieldType}")
                If Not ft.Contains(FieldType) Then Continue While

                Dim Unicode = New Char32(Integer.Parse(m.Result("${Unicode}"), Globalization.NumberStyles.HexNumber))
                Dim Value = m.Result("${Value}")

                If CharDict.ContainsKey(Unicode) Then
                    Dim l = CharDict(Unicode)
                    l.Add(New UniHanDouble(FieldType, Value))
                Else
                    Dim l As New List(Of UniHanDouble)
                    l.Add(New UniHanDouble(FieldType, Value))
                    CharDict.Add(Unicode, l)
                End If
            End While
        End Using

    End Sub
    Public Sub LoadAll(ByVal Path As String)
        Dim r As New Regex("U+(?<Unicode>2?[0-9A-F]{4})\t(?<FieldType>[0-9A-Za-z]+)\t(?<Value>.*)", RegexOptions.ExplicitCapture)
        Using sr = Txt.CreateTextReader(Path, UTF8)
            While Not sr.EndOfStream
                Dim Line As String = sr.ReadLine().TrimStart(" "c)
                If Line.StartsWith("#") Then Continue While
                If Line = "" Then Continue While

                Dim m = r.Match(Line)
                If Not m.Success Then Throw New InvalidDataException("{0}: {1}".Formats(Path, Line))

                Dim Unicode = New Char32(Integer.Parse(m.Result("${Unicode}"), Globalization.NumberStyles.HexNumber))
                Dim FieldType = m.Result("${FieldType}")
                Dim Value = m.Result("${Value}")

                If CharDict.ContainsKey(Unicode) Then
                    Dim l = CharDict(Unicode)
                    l.Add(New UniHanDouble(FieldType, Value))
                Else
                    Dim l As New List(Of UniHanDouble)
                    l.Add(New UniHanDouble(FieldType, Value))
                    CharDict.Add(Unicode, l)
                End If
            End While
        End Using
    End Sub
    Public Function GetChars() As IEnumerable(Of UniHanChar)
        Return From p In CharDict Select New UniHanChar(p.Key, p.Value)
    End Function
    Public Function GetDoubleDict() As Dictionary(Of Char32, IEnumerable(Of UniHanDouble))
        Return (From p In CharDict Select Key = p.Key, Value = p.Value.AsEnumerable).ToDictionary(Function(p) p.Key, Function(p) p.Value)
    End Function
    Public Function GetTriples() As IEnumerable(Of UniHanTriple)
        Dim Triples As New List(Of UniHanTriple)
        For Each p In CharDict
            For Each v In p.Value
                Triples.Add(New UniHanTriple(p.Key, v.FieldType, v.Value))
            Next
        Next
        Return Triples
    End Function

    Public Structure UniHanDouble
        Private FieldTypeValue As String
        Private ValueValue As String
        Public ReadOnly Property FieldType() As String
            Get
                Return FieldTypeValue
            End Get
        End Property
        Public ReadOnly Property Value() As String
            Get
                Return ValueValue
            End Get
        End Property

        Public Sub New(ByVal FieldType As String, ByVal Value As String)
            FieldTypeValue = FieldType
            ValueValue = Value
        End Sub
    End Structure

    Public Structure UniHanTriple
        Private UnicodeValue As Char32
        Private FieldTypeValue As String
        Private ValueValue As String
        Public ReadOnly Property Unicode() As Char32
            Get
                Return UnicodeValue
            End Get
        End Property
        Public ReadOnly Property FieldType() As String
            Get
                Return FieldTypeValue
            End Get
        End Property
        Public ReadOnly Property Value() As String
            Get
                Return ValueValue
            End Get
        End Property

        Public Sub New(ByVal Unicode As Char32, ByVal FieldType As String, ByVal Value As String)
            UnicodeValue = Unicode
            FieldTypeValue = FieldType
            ValueValue = Value
        End Sub
    End Structure

    Public Class UniHanChar
        Private UnicodeValue As Char32
        Private Dict As New Dictionary(Of String, String)
        Public Sub New(ByVal Unicode As Char32, ByVal Fields As IEnumerable(Of UniHanDouble))
            UnicodeValue = Unicode
            Dict = Fields.ToDictionary(Function(d) d.FieldType, Function(d) d.Value)
        End Sub
        Public ReadOnly Property Unicode() As Char32
            Get
                Return UnicodeValue
            End Get
        End Property
        Default Public Property Field(ByVal FieldName As String) As String
            Get
                Return Dict(FieldName)
            End Get
            Set(ByVal Value As String)
                If Not Dict.ContainsKey(FieldName) Then
                    Dict.Add(FieldName, Value)
                Else
                    Dict(FieldName) = Value
                End If
            End Set
        End Property
        Public ReadOnly Property HasField(ByVal FieldName As String) As Boolean
            Get
                Return Dict.ContainsKey(FieldName)
            End Get
        End Property
        Public Sub AddField(ByVal FieldName As String, ByVal Value As String)
            Dict.Add(FieldName, Value)
        End Sub
        Public Sub RemoveField(ByVal FieldName As String)
            Dict.Remove(FieldName)
        End Sub
        Public Function GetFields() As IEnumerable(Of UniHanDouble)
            Return From p In Dict Select New UniHanDouble(p.Key, p.Value)
        End Function
    End Class
End Class
