'==========================================================================
'
'  File:        Char32.vb
'  Location:    Firefly.TextEncoding <Visual Basic .Net>
'  Description: UTF-32 字符
'  Version:     2009.03.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace TextEncoding
    Public Module ControlChars
        ''' <summary>回车符。</summary>
        Public ReadOnly Cr As Char32 = &HD
        ''' <summary>换行符。</summary>
        Public ReadOnly Lf As Char32 = &HA
        ''' <summary>回车换行符。</summary>
        Public ReadOnly CrLf As String = (New Char32() {&HD, &HA}).ToUTF16B
        ''' <summary>空字符。</summary>
        Public ReadOnly Nul As Char32 = &H0
        ''' <summary>双引号。</summary>
        Public ReadOnly Quote As Char32 = &H22
    End Module
End Namespace
