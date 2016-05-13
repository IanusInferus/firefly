'==========================================================================
'
'  File:        Writer.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文本输出类
'  Version:     2016.05.13.
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
Imports Firefly.Texting.TreeFormat.Semantics

Namespace Texting.TreeFormat
    Public Class TreeFormatWriter
        Private sw As StreamWriter
        Public Sub New(ByVal sw As StreamWriter)
            Me.sw = sw
        End Sub

        Private Shared ReadOnly Empty As String = "$Empty"
        Private Shared ReadOnly StringDirective As String = "$String"
        Private Shared ReadOnly EndDirective As String = "$End"
        Private Shared ReadOnly List As String = "$List"

        Public Sub Write(ByVal Forest As Forest)
            For Each n In Forest.Nodes
                WriteNode(0, n)
            Next
        End Sub

        Private Sub WriteNode(ByVal IndentLevel As Integer, ByVal Node As Node)
            Dim s = TryGetNodeAsSingleLineString(Node)
            If s.HasValue Then
                WriteRaw(IndentLevel, s.Value)
                Return
            End If

            Select Case Node._Tag
                Case NodeTag.Empty
                    Throw New InvalidOperationException
                Case NodeTag.Leaf
                    WriteValue(IndentLevel, Node.Leaf)
                Case NodeTag.Stem
                    Dim ns = Node.Stem
                    Dim Name = GetLiteral(ns.Name, True).SingleLine
                    WriteRaw(IndentLevel, Name)

                    If ns.Children.Length = 0 Then
                        WriteRaw(IndentLevel, EndDirective)
                    Else
                        If ns.Children.Length > 1 AndAlso ns.Children.All(Function(c) c.OnStem AndAlso c.Stem.Children.Length = 1) Then
                            Dim ChildNames = ns.Children.Select(Function(c) c.Stem.Name).Distinct.ToArray()
                            If ChildNames.Length = 1 Then
                                Dim ChildName = GetLiteral(ChildNames.Single(), True).SingleLine
                                WriteRaw(IndentLevel + 1, List & " " & ChildName)
                                For Each c In ns.Children
                                    WriteNode(IndentLevel + 2, c.Stem.Children.Single())
                                Next
                                Return
                            End If
                        End If

                        For Each c In ns.Children
                            WriteNode(IndentLevel + 1, c)
                        Next
                    End If
                Case Else
                    Throw New ArgumentException
            End Select
        End Sub

        Private Shared Function TryGetNodeAsSingleLineString(ByVal Node As Node) As [Optional](Of String)
            Dim l As New List(Of String)

            Dim n = Node
            While True
                Select Case n._Tag
                    Case NodeTag.Empty
                        l.Add(Empty)
                        Exit While
                    Case NodeTag.Leaf
                        Dim s = GetLiteral(n.Leaf, False)
                        If s.OnMultiLine Then Return [Optional](Of String).Empty
                        If Not s.OnSingleLine Then Throw New InvalidOperationException
                        l.Add(s.SingleLine)
                        Exit While
                    Case NodeTag.Stem
                        Dim ns = n.Stem
                        If ns.Children.Length <> 1 Then Return [Optional](Of String).Empty
                        Dim s = GetLiteral(ns.Name, False)
                        If s.OnMultiLine Then Return [Optional](Of String).Empty
                        If Not s.OnSingleLine Then Throw New InvalidOperationException
                        l.Add(s.SingleLine)
                        n = ns.Children.Single()
                    Case Else
                        Throw New InvalidOperationException
                End Select
            End While

            Return String.Join(" ", l.ToArray)
        End Function

        Private Sub WriteValue(ByVal IndentLevel As Integer, ByVal Value As String)
            Dim s = GetLiteral(Value, False)

            Select Case s._Tag
                Case LiteralTag.SingleLine
                    WriteRaw(IndentLevel, s.SingleLine)
                Case LiteralTag.MultiLine
                    WriteRaw(IndentLevel, StringDirective)
                    For Each Line In s.MultiLine
                        WriteRaw(IndentLevel + 1, Line)
                    Next
                    If s.MultiLine.Length > 0 Then
                        Dim LastLine = s.MultiLine.Last()
                        Dim Chars = LastLine.Distinct.ToArray()
                        If Chars.Length = 0 OrElse (Chars.Length = 1 AndAlso Chars.Single = " ") Then
                            WriteRaw(IndentLevel, EndDirective)
                        End If
                    End If
            End Select
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
