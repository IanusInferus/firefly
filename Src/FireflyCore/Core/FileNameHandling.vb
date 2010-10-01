'==========================================================================
'
'  File:        FileNameHandling.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 文件名操作函数模块
'  Version:     2010.10.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions

Public Module FileNameHandling

    ''' <summary>获得文件名</summary>
    Public Function GetFileName(ByVal FilePath As String) As String
        If FilePath = "" Then Return ""
        Dim NameS As Integer
        Dim NameS2 As Integer = FilePath.Replace("/", "\").IndexOf("\"c, NameS)
        While NameS2 <> -1
            NameS = NameS2 + 1
            NameS2 = FilePath.Replace("/", "\").IndexOf("\"c, NameS)
        End While
        Return FilePath.Substring(NameS)
    End Function
    ''' <summary>获得主文件名</summary>
    Public Function GetMainFileName(ByVal FilePath As String) As String
        If FilePath = "" Then Return ""
        Dim NameS As Integer
        Dim NameS2 As Integer = FilePath.Replace("/", "\").IndexOf("\"c, NameS)
        While NameS2 <> -1
            NameS = NameS2 + 1
            NameS2 = FilePath.Replace("/", "\").IndexOf("\"c, NameS)
        End While
        Dim NameE As Integer = FilePath.Length - 1
        Dim NameE2 As Integer = FilePath.LastIndexOf("."c, NameE)
        If NameE2 <> -1 Then
            NameE = NameE2 - 1
        End If
        Return FilePath.Substring(NameS, NameE - NameS + 1)
    End Function
    ''' <summary>获得扩展文件名</summary>
    Public Function GetExtendedFileName(ByVal FilePath As String) As String
        If FilePath = "" Then Return ""
        If Not FilePath.Contains(".") Then Return ""
        Return FilePath.Substring(FilePath.LastIndexOf(".") + 1)
    End Function
    ''' <summary>获得文件路径中的文件夹部分</summary>
    Public Function GetFileDirectory(ByVal FilePath As String) As String
        If FilePath = "" Then Return ""
        Dim NameE As Integer
        Dim NameE2 As Integer
        While NameE2 <> -1
            NameE = NameE2 + 1
            NameE2 = FilePath.Replace("/", "\").IndexOf("\"c, NameE)
        End While
        Return FilePath.Substring(0, NameE - 1)
    End Function
    ''' <summary>获得相对路径</summary>
    Public Function GetRelativePath(ByVal FilePath As String, ByVal BaseDirectory As String) As String
        If FilePath = "" OrElse BaseDirectory = "" Then Return FilePath
        Dim a As String = FilePath.TrimEnd("\"c).TrimEnd("/"c)
        Dim b As String = BaseDirectory.TrimEnd("\"c).TrimEnd("/"c)
        Dim c As String
        Dim d As String

        c = PopFirstDir(a)
        d = PopFirstDir(b)
        If c <> d Then Return FilePath
        While c = d
            If c = "" Then Return "."
            c = PopFirstDir(a)
            d = PopFirstDir(b)
        End While

        a = (c & "\" & a).TrimEnd("\"c).TrimEnd("/"c)
        b = (d & "\" & b).TrimEnd("\"c).TrimEnd("/"c)

        While PopFirstDir(b) <> ""
            a = "..\" & a
        End While
        Return a.Replace("\", IO.Path.DirectorySeparatorChar)
    End Function
    ''' <summary>获得精简路径</summary>
    Public Function GetReducedPath(ByVal Path As String) As String
        Dim l As New Stack(Of String)
        If Path <> "" Then
            For Each d In Regex.Split(Path, "\\|/")
                If d = "." Then Continue For
                If d = ".." Then
                    If l.Count > 0 Then
                        Dim p = l.Pop()
                        If p = ".." Then
                            l.Push(p)
                            l.Push(d)
                        End If
                    Else
                        l.Push(d)
                    End If
                    Continue For
                End If
                If d.Contains(":") Then l.Clear()
                l.Push(d)
            Next
        End If
        Return String.Join(IO.Path.DirectorySeparatorChar, l.Reverse.ToArray)
    End Function
    ''' <summary>获得没有结尾分隔符的文件夹路径</summary>
    Public Function GetDirectoryPathWithoutTailingSeparator(ByVal DirectoryPath As String) As String
        If DirectoryPath = "" Then Return ""
        Return DirectoryPath.TrimEnd("\"c).TrimEnd("/"c)
    End Function
    ''' <summary>获得有结尾分隔符的文件夹路径，如果文件夹为空，则返回空</summary>
    Public Function GetDirectoryPathWithTailingSeparator(ByVal DirectoryPath As String) As String
        Dim d = GetDirectoryPathWithoutTailingSeparator(DirectoryPath)
        If d = "" Then Return ""
        Return d & IO.Path.DirectorySeparatorChar
    End Function
    ''' <summary>获得绝对路径</summary>
    Public Function GetAbsolutePath(ByVal FilePath As String, ByVal BaseDirectory As String) As String
        BaseDirectory = GetDirectoryPathWithoutTailingSeparator(BaseDirectory)
        If FilePath <> "" Then FilePath = FilePath.TrimStart("\"c).TrimStart("/"c)
        Dim s As New Stack(Of String)
        If BaseDirectory <> "" Then
            For Each d In Regex.Split(BaseDirectory, "\\|/")
                If d = "." Then Continue For
                If d = ".." Then
                    If s.Count > 0 Then
                        Dim p = s.Pop()
                        If p = ".." Then
                            s.Push(p)
                            s.Push(d)
                        End If
                    Else
                        s.Push(d)
                    End If
                    Continue For
                End If
                If d.Contains(":") Then s.Clear()
                s.Push(d)
            Next
        End If
        If FilePath <> "" Then
            For Each d In Regex.Split(FilePath, "\\|/")
                If d = "." Then Continue For
                If d = ".." Then
                    If s.Count > 0 Then
                        Dim p = s.Pop()
                        If p = ".." Then
                            s.Push(p)
                            s.Push(d)
                        End If
                    Else
                        s.Push(d)
                    End If
                    Continue For
                End If
                If d.Contains(":") Then s.Clear()
                s.Push(d)
            Next
        End If
        Return String.Join(IO.Path.DirectorySeparatorChar, s.Reverse.ToArray)
    End Function

    ''' <summary>取出路径的第一个文件夹名</summary>
    Public Function PopFirstDir(ByRef Path As String) As String
        Dim ret As String
        If Path = "" Then Return ""
        Dim NameS As Integer
        NameS = Path.Replace("/", "\").IndexOf("\"c, NameS)
        If NameS < 0 Then
            ret = Path
            Path = ""
            Return ret
        Else
            ret = Path.Substring(0, NameS)
            Path = Path.Substring(NameS + 1)
            Return ret
        End If
    End Function
    ''' <summary>构成路径</summary>
    Public Function GetPath(ByVal Directory As String, ByVal FileName As String) As String
        If Directory = "" Then Return FileName
        Directory = Directory.TrimEnd("\"c).TrimEnd("/"c)
        Return (Directory & "\" & FileName).Replace("\", IO.Path.DirectorySeparatorChar)
    End Function
    ''' <summary>更换扩展名</summary>
    Public Function ChangeExtension(ByVal FilePath As String, ByVal Extension As String) As String
        Return System.IO.Path.ChangeExtension(FilePath, Extension)
    End Function

    ''' <summary>判断文件名是否符合通配符</summary>
    Public Function IsMatchFileMask(ByVal FileName As String, ByVal Mask As String) As Boolean
        Dim Pattern = "^" & Regex.Escape(Mask).Replace("?", ".?").Replace("*", "*?") & "$"
        Dim r As New Regex(Pattern, RegexOptions.ExplicitCapture Or RegexOptions.IgnoreCase)
        Return r.Match(FileName).Success
    End Function
End Module
