'==========================================================================
'
'  File:        Main.vb
'  Location:    Firefly.MetaSchemaManipulator <Visual Basic .Net>
'  Description: 元类型结构处理工具
'  Version:     2011.06.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports Firefly
Imports Firefly.Mapping
Imports Firefly.Mapping.XmlText
Imports Firefly.Texting
Imports Firefly.Texting.TreeFormat

Public Module Main
    Public Function Main() As Integer
        If System.Diagnostics.Debugger.IsAttached Then
            Return MainInner()
        Else
            Try
                Return MainInner()
            Catch ex As Exception
                Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex))
                Return -1
            End Try
        End If
    End Function

    Public Function MainInner() As Integer
        Dim CmdLine = CommandLine.GetCmdLine()
        Dim argv = CmdLine.Arguments

        If CmdLine.Arguments.Length <> 0 Then
            DisplayInfo()
            Return -1
        End If

        If CmdLine.Options.Length = 0 Then
            DisplayInfo()
            Return 0
        End If

        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case "t2vb"
                    Dim args = opt.Arguments
                    Select Case args.Length
                        Case 2
                            MetaSchemaToVbCode(args(0), args(1))
                        Case 3
                            MetaSchemaToVbCode(args(0), args(1), args(2))
                        Case Else
                            DisplayInfo()
                            Return -1
                    End Select
                Case "t2x", "x2t"
                    Dim args = opt.Arguments
                    If args.Length <> 2 Then
                        DisplayInfo()
                        Return -1
                    End If
                    Select Case opt.Name.ToLower
                        Case "t2x"
                            TreeToXml(args(0), args(1))
                        Case "x2t"
                            XmlToTree(args(0), args(1))
                        Case Else
                            Throw New InvalidOperationException
                    End Select
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("元类型结构处理工具")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("本工具用于从元类型结构生成代码。目前只支持VB.Net代码生成。")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("MetaSchemaManipulator /t2vb:<MetaSchemaPath>,<VbCodePath>[,<NamespaceName>]")
        Console.WriteLine("MetaSchemaManipulator /t2x:<TreeFile>,<XmlFile>")
        Console.WriteLine("MetaSchemaManipulator /x2t:<XmlFile>,<TreeFile>")
        Console.WriteLine("MetaSchemaPath 元类型结构Tree文件路径。")
        Console.WriteLine("VbCodePath VB代码文件路径。")
        Console.WriteLine("NamespaceName VB文件中的命名空间。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("MetaSchemaManipulator /t2vb:BinarySchema.tree,BinarySchema.vb,BinarySchema")
    End Sub

    Public Sub MetaSchemaToVbCode(ByVal MetaSchemaPath As String, ByVal VbCodePath As String, Optional ByVal NamespaceName As String = "")
        Dim x = TreeFile.ReadFile(MetaSchemaPath)
        Dim xs As New XmlSerializer
        Dim MetaSchema = xs.Read(Of MetaSchema.Schema)(x)

        Dim Compiled = MetaSchema.CompileToVB(NamespaceName)
        If IO.File.Exists(VbCodePath) Then
            Dim Original = Txt.ReadFile(VbCodePath)
            If String.Equals(Compiled, Original, StringComparison.Ordinal) Then
                Return
            End If
        End If
        Txt.WriteFile(VbCodePath, Compiled)
    End Sub

    Public Sub TreeToXml(ByVal TreePath As String, ByVal XmlPath As String)
        Dim x = TreeFile.ReadFile(TreePath)
        XmlFile.WriteFile(XmlPath, x)
    End Sub
    Public Sub XmlToTree(ByVal XmlPath As String, ByVal TreePath As String)
        Dim x = XmlFile.ReadFile(XmlPath)
        TreeFile.WriteFile(TreePath, x)
    End Sub
End Module
