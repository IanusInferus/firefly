'==========================================================================
'
'  File:        ProgressDialog.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 进度显示框
'  Version:     2008.11.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel

Public Class ProgressDialog
    <Category("Appearance")> _
    Public Property LabelText() As String
        Get
            Return Label.Text
        End Get
        Set(ByVal Value As String)
            Label.Text = Value
        End Set
    End Property

    <Category("Appearance")> _
    Public Property Minimum() As Integer
        Get
            Return ProgressBar.Minimum
        End Get
        Set(ByVal Value As Integer)
            ProgressBar.Minimum = Value
        End Set
    End Property

    <Category("Appearance")> _
    Public Property Maximum() As Integer
        Get
            Return ProgressBar.Maximum
        End Get
        Set(ByVal Value As Integer)
            ProgressBar.Maximum = Value
        End Set
    End Property

    <Category("Appearance")> _
    Public Property Value() As Integer
        Get
            Return ProgressBar.Value
        End Get
        Set(ByVal Value As Integer)
            ProgressBar.Value = Value
        End Set
    End Property
End Class