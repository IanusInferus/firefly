'==========================================================================
'
'  File:        EncodingString.vb
'  Location:    Firefly.TextEncoding <Visual Basic .Net>
'  Description: 编码
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic

Namespace TextEncoding
    Public Module EncodingString
        ''' <summary>已重载。得到编码文本，按第一次出现的位置排序。</summary>
        Public Function GetEncodingStringFromText(ByVal Text As String, Optional ByVal Exclude As String = "") As String
            Return GetEncodingString32FromText(Text.ToUTF32, Exclude).ToUTF16B
        End Function
        ''' <summary>已重载。得到编码文本，按第一次出现的位置排序。</summary>
        Public Function GetEncodingStringFromText(ByVal Text As String(), Optional ByVal Exclude As String = "") As String
            Dim Char32Text = New Char32(Text.Length - 1)() {}
            For n = 0 To Text.Length - 1
                Char32Text(n) = Text(n).ToUTF32
            Next
            Return GetEncodingString32FromText(Char32Text, Exclude).ToUTF16B
        End Function
        ''' <summary>已重载。得到编码文本，按第一次出现的位置排序。</summary>
        Public Function GetEncodingString32FromText(ByVal Text As String, Optional ByVal Exclude As String = "") As Char32()
            Return GetEncodingString32FromText(Text.ToUTF32, Exclude)
        End Function
        ''' <summary>已重载。得到编码文本，按第一次出现的位置排序。</summary>
        Public Function GetEncodingString32FromText(ByVal Text As String(), Optional ByVal Exclude As String = "") As Char32()
            Dim Char32Text = New Char32(Text.Length - 1)() {}
            For n = 0 To Text.Length - 1
                Char32Text(n) = Text(n).ToUTF32
            Next
            Return GetEncodingString32FromText(Char32Text, Exclude)
        End Function
        ''' <summary>已重载。得到编码文本，按第一次出现的位置排序。</summary>
        Public Function GetEncodingString32FromText(ByVal Text As Char32(), Optional ByVal Exclude As String = "") As Char32()
            Dim s As New List(Of Char32)
            Dim d As New Dictionary(Of Char32, Int32)
            Dim dExclude As New Dictionary(Of Char32, Int32)
            For Each c In Exclude
                If Not dExclude.ContainsKey(c) Then dExclude.Add(c, 0)
            Next
            For Each c In Text
                If dExclude.ContainsKey(c) Then Continue For
                If Not d.ContainsKey(c) Then
                    d.Add(c, 0)
                    s.Add(c)
                End If
            Next
            Return s.ToArray
        End Function
        ''' <summary>已重载。得到编码文本，按第一次出现的位置排序。</summary>
        Public Function GetEncodingString32FromText(ByVal Text As Char32()(), Optional ByVal Exclude As String = "") As Char32()
            Dim s As New List(Of Char32)
            Dim d As New Dictionary(Of Char32, Int32)
            Dim dExclude As New Dictionary(Of Char32, Int32)
            For Each c In Exclude
                If Not dExclude.ContainsKey(c) Then dExclude.Add(c, 0)
            Next
            For Each t In Text
                For Each c In t
                    If dExclude.ContainsKey(c) Then Continue For
                    If Not d.ContainsKey(c) Then
                        d.Add(c, 0)
                        s.Add(c)
                    End If
                Next
            Next
            Return s.ToArray
        End Function

        ''' <summary>编码文本生成器</summary>
        Public Class EncodingStringGenerator
            Private l As New List(Of Int32)
            Private s As New List(Of Char32)
            Private d As New Dictionary(Of Char32, Int32)
            Private dExclude As New Dictionary(Of Char32, Int32)

            ''' <summary>已重载。创建新实例。</summary>
            Public Sub New()
            End Sub
            ''' <summary>已重载。用排除列表创建新实例。</summary>
            Public Sub New(ByVal Exclude As String)
                Me.New(Exclude.ToUTF32)
            End Sub
            ''' <summary>已重载。用排除列表创建新实例。</summary>
            Public Sub New(ByVal Exclude As Char32())
                For Each c In Exclude
                    If dExclude.ContainsKey(c) Then Continue For
                    dExclude.Add(c, 0)
                Next
            End Sub
            ''' <summary>已重载。添加排除的字符列表。</summary>
            Public Sub PushExclude(ByVal c As Char)
                PushExclude(CType(c, Char32))
            End Sub
            ''' <summary>已重载。添加排除的字符列表。</summary>
            Public Sub PushExclude(ByVal c As Char32)
                If dExclude.ContainsKey(c) Then Return
                dExclude.Add(c, 0)
                If d.ContainsKey(c) Then
                    l(d(c)) = 0
                    d.Remove(c)
                End If
            End Sub
            ''' <summary>已重载。添加排除的字符列表。</summary>
            Public Sub PushExclude(ByVal Exclude As String)
                PushExclude(Exclude.ToUTF32)
            End Sub
            ''' <summary>已重载。添加排除的字符列表。</summary>
            Public Sub PushExclude(ByVal Exclude As Char32())
                For Each c In Exclude
                    If dExclude.ContainsKey(c) Then Continue For
                    dExclude.Add(c, 0)
                    If d.ContainsKey(c) Then
                        l(d(c)) = 0
                        d.Remove(c)
                    End If
                Next
            End Sub
            ''' <summary>已重载。从文本添加字符。</summary>
            Public Sub PushText(ByVal Text As String)
                PushText(Text.ToUTF32)
            End Sub
            ''' <summary>已重载。从文本添加字符。</summary>
            Public Sub PushText(ByVal Text As Char32())
                For Each c In Text
                    If dExclude.ContainsKey(c) Then Continue For
                    If Not d.ContainsKey(c) Then
                        d.Add(c, l.Count)
                        s.Add(c)
                        l.Add(1)
                    Else
                        l(d(c)) += 1
                    End If
                Next
            End Sub
            ''' <summary>已重载。得到字库文字，频率高的在前。</summary>
            Public Function GetLibString() As String
                Return GetLibString32.ToUTF16B
            End Function
            ''' <summary>已重载。得到字库文字，频率高的在前。</summary>
            Public Function GetLibString32() As Char32()
                Dim ret = s.ToArray
                Dim retl = l.ToArray
                Array.Sort(retl, ret)
                Array.Reverse(ret)
                Array.Reverse(retl)
                For i = ret.Length - 1 To 0 Step -1
                    If retl(i) > 0 Then
                        Dim a = New Char32(i) {}
                        Array.Copy(ret, a, i + 1)
                        Return a
                    End If
                Next
                Return New Char32() {}
            End Function
            ''' <summary>清空。</summary>
            Public Sub Clear()
                l.Clear()
                s.Clear()
                d.Clear()
            End Sub
        End Class
    End Module
End Namespace
