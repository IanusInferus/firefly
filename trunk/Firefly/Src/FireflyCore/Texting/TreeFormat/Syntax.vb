'==========================================================================
'
'  File:        Syntax.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文法对象定义
'  Version:     2011.06.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports Firefly.Mapping.MetaSchema

Namespace Texting.TreeFormat.Syntax
    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure TextPosition
        Public Property CharIndex As Integer
        Public Property Row As Integer
        Public Property Column As Integer

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure TextRange
        Public Property Start As TextPosition
        Public Property [End] As TextPosition

        Public Overrides Function ToString() As String
            Return String.Format("({0}, {1})-({2}, {3})".Formats(Start.Row, Start.Column, [End].Row, [End].Column))
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure TextLine
        Public Property Text As String
        Public Property Range As TextRange

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure TextLineRange
        Public Property StartRow As Integer
        Public Property EndRow As Integer

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Text
        Public Property Path As String
        Public Property Lines As TextLine()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Forest
        Public Property MultiNodesList As MultiNodes()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum MultiNodesTag
        Node
        ListNodes
        TableNodes
        FunctionNodes
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class MultiNodes
        <Tag()> Public Property _Tag As MultiNodesTag
        Public Property Node As Node
        Public Property ListNodes As ListNodes
        Public Property TableNodes As TableNodes
        Public Property FunctionNodes As FunctionNodes

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum NodeTag
        SingleLineNodeLine
        MultiLineLiteral
        SingleLineComment
        MultiLineComment
        MultiLineNode
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class Node
        <Tag()> Public Property _Tag As NodeTag
        Public Property SingleLineNodeLine As SingleLineNodeLine
        Public Property MultiLineLiteral As MultiLineLiteral
        Public Property SingleLineComment As SingleLineComment
        Public Property MultiLineComment As MultiLineComment
        Public Property MultiLineNode As MultiLineNode

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineNodeLine
        Public Property SingleLineNode As SingleLineNode
        Public Property SingleLineComment As Opt(Of SingleLineComment)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum SingleLineNodeTag
        EmptyNode
        SingleLineFunctionNode
        SingleLineLiteral
        ParenthesesNode
        SingleLineNodeWithParameters
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineNode
        <Tag()> Public Property _Tag As SingleLineNodeTag
        Public Property EmptyNode As EmptyNode
        Public Property SingleLineFunctionNode As SingleLineFunctionNode
        Public Property SingleLineLiteral As SingleLineLiteral
        Public Property ParenthesesNode As ParenthesesNode
        Public Property SingleLineNodeWithParameters As SingleLineNodeWithParameters

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineNodeWithParameters
        Public Property Head As SingleLineLiteral
        Public Property Children As ParenthesesNode()
        Public Property LastChild As Opt(Of SingleLineNode)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class MultiLineNode
        Public Property Head As SingleLineLiteral
        Public Property SingleLineComment As Opt(Of SingleLineComment)
        Public Property Children As MultiNodes()
        Public Property EndDirective As Opt(Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class ParenthesesNode
        Public Property SingleLineNode As SingleLineNode

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineComment
        Public Property Content As FreeContent

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class MultiLineComment
        Public Property SingleLineComment As Opt(Of SingleLineComment)
        Public Property Content As FreeContent
        Public Property EndDirective As Opt(Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class EmptyNode
        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineFunctionNode
        Public Property FunctionDirective As FunctionDirective
        Public Property Parameters As Token()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class MultiLineLiteral
        Public Property SingleLineComment As Opt(Of SingleLineComment)
        Public Property Content As FreeContent
        Public Property EndDirective As Opt(Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class ListNodes
        Public Property ChildHead As SingleLineLiteral
        Public Property SingleLineComment As Opt(Of SingleLineComment)
        Public Property Children As MultiNodes()
        Public Property EndDirective As Opt(Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class TableNodes
        Public Property ChildHead As SingleLineLiteral
        Public Property ChildFields As SingleLineLiteral()
        Public Property SingleLineComment As Opt(Of SingleLineComment)
        Public Property Children As TableLine()
        Public Property EndDirective As Opt(Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class TableLine
        Public Property Nodes As TableLineNode()
        Public Property SingleLineComment As Opt(Of SingleLineComment)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum TableLineNodeTag
        EmptyNode
        SingleLineFunctionNode
        SingleLineLiteral
        ParenthesesNode
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class TableLineNode
        <Tag()> Public Property _Tag As TableLineNodeTag
        Public Property EmptyNode As EmptyNode
        Public Property SingleLineFunctionNode As SingleLineFunctionNode
        Public Property SingleLineLiteral As SingleLineLiteral
        Public Property ParenthesesNode As ParenthesesNode

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionNodes
        Public Property FunctionDirective As FunctionDirective
        Public Property Parameters As Token()
        Public Property SingleLineComment As Opt(Of SingleLineComment)
        Public Property Content As FunctionContent
        Public Property EndDirective As Opt(Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class EndDirective
        Public Property EndSingleLineComment As Opt(Of SingleLineComment)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionDirective
        Public Property Text As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum TokenTag
        SingleLineLiteral
        LeftParentheses
        RightParentheses
        PreprocessDirective
        FunctionDirective
        SingleLineComment
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class Token
        <Tag()> Public Property _Tag As TokenTag
        Public Property SingleLineLiteral As String
        Public Property LeftParentheses As Unit
        Public Property RightParentheses As Unit
        Public Property PreprocessDirective As String
        Public Property FunctionDirective As String
        Public Property SingleLineComment As String

        Public Overrides Function ToString() As String
            Select Case _Tag
                Case TokenTag.SingleLineLiteral
                    Return SingleLineLiteral
                Case TokenTag.LeftParentheses
                    Return "("
                Case TokenTag.RightParentheses
                    Return ")"
                Case TokenTag.PreprocessDirective
                    Return PreprocessDirective
                Case TokenTag.FunctionDirective
                    Return "#" & FunctionDirective
                Case TokenTag.SingleLineComment
                    Return "//" & SingleLineComment
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineLiteral
        Public Property Text As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FreeContent
        Public Property Text As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionContent
        Public Property Lines As TextLine()
        Public Property IndentLevel As Integer

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class RawFunctionCall
        Public Property Name As FunctionDirective
        Public Property ReturnValueMode As FunctionCallReturnValueMode
        Public Property Parameters As RawFunctionCallParameters
        Public Property Content As Opt(Of RawFunctionCallContent)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum RawFunctionCallParametersTag
        TokenParameters
        TreeParameter
        TableParameters
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class RawFunctionCallParameters
        <Tag()> Public Property _Tag As RawFunctionCallParametersTag
        Public Property TokenParameters As Token()
        Public Property TreeParameter As Opt(Of SingleLineNode)
        Public Property TableParameters As TableLineNode()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum RawFunctionCallContentTag
        LineContent
        TreeContent
        TableContent
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class RawFunctionCallContent
        <Tag()> Public Property _Tag As RawFunctionCallContentTag
        Public Property LineContent As FunctionContent
        Public Property TreeContent As MultiNodes()
        Public Property TableContent As TableLine()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum FunctionCallReturnValueMode
        SingleNode
        MultipleNodes
    End Enum

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionCall
        Public Property Name As FunctionDirective
        Public Property ReturnValueMode As FunctionCallReturnValueMode
        Public Property Parameters As Semantics.Node()
        Public Property Content As Opt(Of FunctionCallContent)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum FunctionCallContentTag
        LineContent
        TreeContent
        TableContent
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionCallContent
        <Tag()> Public Property _Tag As FunctionCallContentTag
        Public Property LineContent As FunctionContent
        Public Property TreeContent As Semantics.Node()
        Public Property TableContent As FunctionCallTableLine()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionCallTableLine
        Public Property Nodes As Semantics.Node()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
End Namespace
