'==========================================================================
'
'  File:        Syntax.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文法对象定义
'  Version:     2011.07.31.
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

        Public Shared Function CreateNode(ByVal Value As Node) As MultiNodes
            Return New MultiNodes With {._Tag = MultiNodesTag.Node, .Node = Value}
        End Function
        Public Shared Function CreateListNodes(ByVal Value As ListNodes) As MultiNodes
            Return New MultiNodes With {._Tag = MultiNodesTag.ListNodes, .ListNodes = Value}
        End Function
        Public Shared Function CreateTableNodes(ByVal Value As TableNodes) As MultiNodes
            Return New MultiNodes With {._Tag = MultiNodesTag.TableNodes, .TableNodes = Value}
        End Function
        Public Shared Function CreateFunctionNodes(ByVal Value As FunctionNodes) As MultiNodes
            Return New MultiNodes With {._Tag = MultiNodesTag.FunctionNodes, .FunctionNodes = Value}
        End Function

        Public ReadOnly Property OnNode() As Boolean
            Get
                Return _Tag = MultiNodesTag.Node
            End Get
        End Property
        Public ReadOnly Property OnListNodes() As Boolean
            Get
                Return _Tag = MultiNodesTag.ListNodes
            End Get
        End Property
        Public ReadOnly Property OnTableNodes() As Boolean
            Get
                Return _Tag = MultiNodesTag.TableNodes
            End Get
        End Property
        Public ReadOnly Property OnFunctionNodes() As Boolean
            Get
                Return _Tag = MultiNodesTag.FunctionNodes
            End Get
        End Property

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

        Public Shared Function CreateSingleLineNodeLine(ByVal Value As SingleLineNodeLine) As Node
            Return New Node With {._Tag = NodeTag.SingleLineNodeLine, .SingleLineNodeLine = Value}
        End Function
        Public Shared Function CreateMultiLineLiteral(ByVal Value As MultiLineLiteral) As Node
            Return New Node With {._Tag = NodeTag.MultiLineLiteral, .MultiLineLiteral = Value}
        End Function
        Public Shared Function CreateSingleLineComment(ByVal Value As SingleLineComment) As Node
            Return New Node With {._Tag = NodeTag.SingleLineComment, .SingleLineComment = Value}
        End Function
        Public Shared Function CreateMultiLineComment(ByVal Value As MultiLineComment) As Node
            Return New Node With {._Tag = NodeTag.MultiLineComment, .MultiLineComment = Value}
        End Function
        Public Shared Function CreateMultiLineNode(ByVal Value As MultiLineNode) As Node
            Return New Node With {._Tag = NodeTag.MultiLineNode, .MultiLineNode = Value}
        End Function

        Public ReadOnly Property OnSingleLineNodeLine() As Boolean
            Get
                Return _Tag = NodeTag.SingleLineNodeLine
            End Get
        End Property
        Public ReadOnly Property OnMultiLineLiteral() As Boolean
            Get
                Return _Tag = NodeTag.MultiLineLiteral
            End Get
        End Property
        Public ReadOnly Property OnSingleLineComment() As Boolean
            Get
                Return _Tag = NodeTag.SingleLineComment
            End Get
        End Property
        Public ReadOnly Property OnMultiLineComment() As Boolean
            Get
                Return _Tag = NodeTag.MultiLineComment
            End Get
        End Property
        Public ReadOnly Property OnMultiLineNode() As Boolean
            Get
                Return _Tag = NodeTag.MultiLineNode
            End Get
        End Property

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

        Public Shared Function CreateEmptyNode(ByVal Value As EmptyNode) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.EmptyNode, .EmptyNode = Value}
        End Function
        Public Shared Function CreateSingleLineFunctionNode(ByVal Value As SingleLineFunctionNode) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.SingleLineFunctionNode, .SingleLineFunctionNode = Value}
        End Function
        Public Shared Function CreateSingleLineLiteral(ByVal Value As SingleLineLiteral) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.SingleLineLiteral, .SingleLineLiteral = Value}
        End Function
        Public Shared Function CreateParenthesesNode(ByVal Value As ParenthesesNode) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.ParenthesesNode, .ParenthesesNode = Value}
        End Function
        Public Shared Function CreateSingleLineNodeWithParameters(ByVal Value As SingleLineNodeWithParameters) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.SingleLineNodeWithParameters, .SingleLineNodeWithParameters = Value}
        End Function

        Public ReadOnly Property OnEmptyNode() As Boolean
            Get
                Return _Tag = SingleLineNodeTag.EmptyNode
            End Get
        End Property
        Public ReadOnly Property OnSingleLineFunctionNode() As Boolean
            Get
                Return _Tag = SingleLineNodeTag.SingleLineFunctionNode
            End Get
        End Property
        Public ReadOnly Property OnSingleLineLiteral() As Boolean
            Get
                Return _Tag = SingleLineNodeTag.SingleLineLiteral
            End Get
        End Property
        Public ReadOnly Property OnParenthesesNode() As Boolean
            Get
                Return _Tag = SingleLineNodeTag.ParenthesesNode
            End Get
        End Property
        Public ReadOnly Property OnSingleLineNodeWithParameters() As Boolean
            Get
                Return _Tag = SingleLineNodeTag.SingleLineNodeWithParameters
            End Get
        End Property

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

        Public Shared Function CreateEmptyNode(ByVal Value As EmptyNode) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.EmptyNode, .EmptyNode = Value}
        End Function
        Public Shared Function CreateSingleLineFunctionNode(ByVal Value As SingleLineFunctionNode) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.SingleLineFunctionNode, .SingleLineFunctionNode = Value}
        End Function
        Public Shared Function CreateSingleLineLiteral(ByVal Value As SingleLineLiteral) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.SingleLineLiteral, .SingleLineLiteral = Value}
        End Function
        Public Shared Function CreateParenthesesNode(ByVal Value As ParenthesesNode) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.ParenthesesNode, .ParenthesesNode = Value}
        End Function

        Public ReadOnly Property OnEmptyNode() As Boolean
            Get
                Return _Tag = TableLineNodeTag.EmptyNode
            End Get
        End Property
        Public ReadOnly Property OnSingleLineFunctionNode() As Boolean
            Get
                Return _Tag = TableLineNodeTag.SingleLineFunctionNode
            End Get
        End Property
        Public ReadOnly Property OnSingleLineLiteral() As Boolean
            Get
                Return _Tag = TableLineNodeTag.SingleLineLiteral
            End Get
        End Property
        Public ReadOnly Property OnParenthesesNode() As Boolean
            Get
                Return _Tag = TableLineNodeTag.ParenthesesNode
            End Get
        End Property

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

        Public Shared Function CreateSingleLineLiteral(ByVal Value As String) As Token
            Return New Token With {._Tag = TokenTag.SingleLineLiteral, .SingleLineLiteral = Value}
        End Function
        Public Shared Function CreateLeftParentheses() As Token
            Return New Token With {._Tag = TokenTag.LeftParentheses, .LeftParentheses = New Unit()}
        End Function
        Public Shared Function CreateRightParentheses() As Token
            Return New Token With {._Tag = TokenTag.RightParentheses, .RightParentheses = New Unit()}
        End Function
        Public Shared Function CreatePreprocessDirective(ByVal Value As String) As Token
            Return New Token With {._Tag = TokenTag.PreprocessDirective, .PreprocessDirective = Value}
        End Function
        Public Shared Function CreateFunctionDirective(ByVal Value As String) As Token
            Return New Token With {._Tag = TokenTag.FunctionDirective, .FunctionDirective = Value}
        End Function
        Public Shared Function CreateSingleLineComment(ByVal Value As String) As Token
            Return New Token With {._Tag = TokenTag.SingleLineComment, .SingleLineComment = Value}
        End Function

        Public ReadOnly Property OnSingleLineLiteral() As Boolean
            Get
                Return _Tag = TokenTag.SingleLineLiteral
            End Get
        End Property
        Public ReadOnly Property OnLeftParentheses() As Boolean
            Get
                Return _Tag = TokenTag.LeftParentheses
            End Get
        End Property
        Public ReadOnly Property OnRightParentheses() As Boolean
            Get
                Return _Tag = TokenTag.RightParentheses
            End Get
        End Property
        Public ReadOnly Property OnPreprocessDirective() As Boolean
            Get
                Return _Tag = TokenTag.PreprocessDirective
            End Get
        End Property
        Public ReadOnly Property OnFunctionDirective() As Boolean
            Get
                Return _Tag = TokenTag.FunctionDirective
            End Get
        End Property
        Public ReadOnly Property OnSingleLineComment() As Boolean
            Get
                Return _Tag = TokenTag.SingleLineComment
            End Get
        End Property

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

        Public Shared Function CreateTokenParameters(ByVal Value As Token()) As RawFunctionCallParameters
            Return New RawFunctionCallParameters With {._Tag = RawFunctionCallParametersTag.TokenParameters, .TokenParameters = Value}
        End Function
        Public Shared Function CreateTreeParameter(ByVal Value As Opt(Of SingleLineNode)) As RawFunctionCallParameters
            Return New RawFunctionCallParameters With {._Tag = RawFunctionCallParametersTag.TreeParameter, .TreeParameter = Value}
        End Function
        Public Shared Function CreateTableParameters(ByVal Value As TableLineNode()) As RawFunctionCallParameters
            Return New RawFunctionCallParameters With {._Tag = RawFunctionCallParametersTag.TableParameters, .TableParameters = Value}
        End Function

        Public ReadOnly Property OnTokenParameters() As Boolean
            Get
                Return _Tag = RawFunctionCallParametersTag.TokenParameters
            End Get
        End Property
        Public ReadOnly Property OnTreeParameter() As Boolean
            Get
                Return _Tag = RawFunctionCallParametersTag.TreeParameter
            End Get
        End Property
        Public ReadOnly Property OnTableParameters() As Boolean
            Get
                Return _Tag = RawFunctionCallParametersTag.TableParameters
            End Get
        End Property

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

        Public Shared Function CreateLineContent(ByVal Value As FunctionContent) As RawFunctionCallContent
            Return New RawFunctionCallContent With {._Tag = RawFunctionCallContentTag.LineContent, .LineContent = Value}
        End Function
        Public Shared Function CreateTreeContent(ByVal Value As MultiNodes()) As RawFunctionCallContent
            Return New RawFunctionCallContent With {._Tag = RawFunctionCallContentTag.TreeContent, .TreeContent = Value}
        End Function
        Public Shared Function CreateTableContent(ByVal Value As TableLine()) As RawFunctionCallContent
            Return New RawFunctionCallContent With {._Tag = RawFunctionCallContentTag.TableContent, .TableContent = Value}
        End Function

        Public ReadOnly Property OnLineContent() As Boolean
            Get
                Return _Tag = RawFunctionCallContentTag.LineContent
            End Get
        End Property
        Public ReadOnly Property OnTreeContent() As Boolean
            Get
                Return _Tag = RawFunctionCallContentTag.TreeContent
            End Get
        End Property
        Public ReadOnly Property OnTableContent() As Boolean
            Get
                Return _Tag = RawFunctionCallContentTag.TableContent
            End Get
        End Property

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

        Public Shared Function CreateLineContent(ByVal Value As FunctionContent) As FunctionCallContent
            Return New FunctionCallContent With {._Tag = FunctionCallContentTag.LineContent, .LineContent = Value}
        End Function
        Public Shared Function CreateTreeContent(ByVal Value As Semantics.Node()) As FunctionCallContent
            Return New FunctionCallContent With {._Tag = FunctionCallContentTag.TreeContent, .TreeContent = Value}
        End Function
        Public Shared Function CreateTableContent(ByVal Value As FunctionCallTableLine()) As FunctionCallContent
            Return New FunctionCallContent With {._Tag = FunctionCallContentTag.TableContent, .TableContent = Value}
        End Function

        Public ReadOnly Property OnLineContent() As Boolean
            Get
                Return _Tag = FunctionCallContentTag.LineContent
            End Get
        End Property
        Public ReadOnly Property OnTreeContent() As Boolean
            Get
                Return _Tag = FunctionCallContentTag.TreeContent
            End Get
        End Property
        Public ReadOnly Property OnTableContent() As Boolean
            Get
                Return _Tag = FunctionCallContentTag.TableContent
            End Get
        End Property

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
