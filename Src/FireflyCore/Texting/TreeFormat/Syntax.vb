'==========================================================================
'
'  File:        Syntax.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 文法对象定义
'  Version:     2016.05.26.
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
        Public CharIndex As Integer
        Public Row As Integer
        Public Column As Integer

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure TextRange
        Public Start As TextPosition
        Public [End] As TextPosition

        Public Overrides Function ToString() As String
            Return String.Format("({0}, {1})-({2}, {3})".Formats(Start.Row, Start.Column, [End].Row, [End].Column))
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure TextLine
        Public Text As String
        Public Range As TextRange

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure TextLineRange
        Public StartRow As Integer
        Public EndRow As Integer

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Structure

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Text
        Public Path As String
        Public Lines As List(Of TextLine)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Forest
        Public MultiNodesList As List(Of MultiNodes)

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
        <Tag()> Public _Tag As MultiNodesTag
        Public Node As Node
        Public ListNodes As ListNodes
        Public TableNodes As TableNodes
        Public FunctionNodes As FunctionNodes

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
        <Tag()> Public _Tag As NodeTag
        Public SingleLineNodeLine As SingleLineNodeLine
        Public MultiLineLiteral As MultiLineLiteral
        Public SingleLineComment As SingleLineComment
        Public MultiLineComment As MultiLineComment
        Public MultiLineNode As MultiLineNode

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
        Public SingleLineNode As SingleLineNode
        Public SingleLineComment As [Optional](Of SingleLineComment)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum SingleLineNodeTag
        EmptyNode
        SingleLineFunctionNode
        SingleLineLiteral
        ParenthesisNode
        SingleLineNodeWithParameters
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineNode
        <Tag()> Public _Tag As SingleLineNodeTag
        Public EmptyNode As EmptyNode
        Public SingleLineFunctionNode As SingleLineFunctionNode
        Public SingleLineLiteral As SingleLineLiteral
        Public ParenthesisNode As ParenthesisNode
        Public SingleLineNodeWithParameters As SingleLineNodeWithParameters

        Public Shared Function CreateEmptyNode(ByVal Value As EmptyNode) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.EmptyNode, .EmptyNode = Value}
        End Function
        Public Shared Function CreateSingleLineFunctionNode(ByVal Value As SingleLineFunctionNode) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.SingleLineFunctionNode, .SingleLineFunctionNode = Value}
        End Function
        Public Shared Function CreateSingleLineLiteral(ByVal Value As SingleLineLiteral) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.SingleLineLiteral, .SingleLineLiteral = Value}
        End Function
        Public Shared Function CreateParenthesisNode(ByVal Value As ParenthesisNode) As SingleLineNode
            Return New SingleLineNode With {._Tag = SingleLineNodeTag.ParenthesisNode, .ParenthesisNode = Value}
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
        Public ReadOnly Property OnParenthesisNode() As Boolean
            Get
                Return _Tag = SingleLineNodeTag.ParenthesisNode
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
        Public Head As SingleLineLiteral
        Public Children As List(Of ParenthesisNode)
        Public LastChild As [Optional](Of SingleLineNode)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class MultiLineNode
        Public Head As SingleLineLiteral
        Public SingleLineComment As [Optional](Of SingleLineComment)
        Public Children As List(Of MultiNodes)
        Public EndDirective As [Optional](Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class ParenthesisNode
        Public SingleLineNode As SingleLineNode

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class SingleLineComment
        Public Content As FreeContent

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class MultiLineComment
        Public SingleLineComment As [Optional](Of SingleLineComment)
        Public Content As FreeContent
        Public EndDirective As [Optional](Of EndDirective)

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
        Public FunctionDirective As FunctionDirective
        Public Parameters As List(Of Token)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class MultiLineLiteral
        Public SingleLineComment As [Optional](Of SingleLineComment)
        Public Content As FreeContent
        Public EndDirective As [Optional](Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class ListNodes
        Public ChildHead As SingleLineLiteral
        Public SingleLineComment As [Optional](Of SingleLineComment)
        Public Children As List(Of MultiNodes)
        Public EndDirective As [Optional](Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class TableNodes
        Public ChildHead As SingleLineLiteral
        Public ChildFields As List(Of SingleLineLiteral)
        Public SingleLineComment As [Optional](Of SingleLineComment)
        Public Children As List(Of TableLine)
        Public EndDirective As [Optional](Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class TableLine
        Public Nodes As List(Of TableLineNode)
        Public SingleLineComment As [Optional](Of SingleLineComment)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum TableLineNodeTag
        EmptyNode
        SingleLineFunctionNode
        SingleLineLiteral
        ParenthesisNode
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class TableLineNode
        <Tag()> Public _Tag As TableLineNodeTag
        Public EmptyNode As EmptyNode
        Public SingleLineFunctionNode As SingleLineFunctionNode
        Public SingleLineLiteral As SingleLineLiteral
        Public ParenthesisNode As ParenthesisNode

        Public Shared Function CreateEmptyNode(ByVal Value As EmptyNode) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.EmptyNode, .EmptyNode = Value}
        End Function
        Public Shared Function CreateSingleLineFunctionNode(ByVal Value As SingleLineFunctionNode) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.SingleLineFunctionNode, .SingleLineFunctionNode = Value}
        End Function
        Public Shared Function CreateSingleLineLiteral(ByVal Value As SingleLineLiteral) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.SingleLineLiteral, .SingleLineLiteral = Value}
        End Function
        Public Shared Function CreateParenthesisNode(ByVal Value As ParenthesisNode) As TableLineNode
            Return New TableLineNode With {._Tag = TableLineNodeTag.ParenthesisNode, .ParenthesisNode = Value}
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
        Public ReadOnly Property OnParenthesisNode() As Boolean
            Get
                Return _Tag = TableLineNodeTag.ParenthesisNode
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionNodes
        Public FunctionDirective As FunctionDirective
        Public Parameters As List(Of Token)
        Public SingleLineComment As [Optional](Of SingleLineComment)
        Public Content As FunctionContent
        Public EndDirective As [Optional](Of EndDirective)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class EndDirective
        Public EndSingleLineComment As [Optional](Of SingleLineComment)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionDirective
        Public Text As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum TokenTag
        SingleLineLiteral
        LeftParenthesis
        RightParenthesis
        PreprocessDirective
        FunctionDirective
        SingleLineComment
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class Token
        <Tag()> Public _Tag As TokenTag
        Public SingleLineLiteral As String
        Public LeftParenthesis As Unit
        Public RightParenthesis As Unit
        Public PreprocessDirective As String
        Public FunctionDirective As String
        Public SingleLineComment As String

        Public Shared Function CreateSingleLineLiteral(ByVal Value As String) As Token
            Return New Token With {._Tag = TokenTag.SingleLineLiteral, .SingleLineLiteral = Value}
        End Function
        Public Shared Function CreateLeftParenthesis() As Token
            Return New Token With {._Tag = TokenTag.LeftParenthesis, .LeftParenthesis = New Unit()}
        End Function
        Public Shared Function CreateRightParenthesis() As Token
            Return New Token With {._Tag = TokenTag.RightParenthesis, .RightParenthesis = New Unit()}
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
        Public ReadOnly Property OnLeftParenthesis() As Boolean
            Get
                Return _Tag = TokenTag.LeftParenthesis
            End Get
        End Property
        Public ReadOnly Property OnRightParenthesis() As Boolean
            Get
                Return _Tag = TokenTag.RightParenthesis
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
                Case TokenTag.LeftParenthesis
                    Return "("
                Case TokenTag.RightParenthesis
                    Return ")"
                Case TokenTag.PreprocessDirective
                    Return "$" & PreprocessDirective
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
        Public Text As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FreeContent
        Public Text As String

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class FunctionContent
        Public Lines As List(Of TextLine)
        Public IndentLevel As Integer

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class RawFunctionCall
        Public Name As FunctionDirective
        Public ReturnValueMode As FunctionCallReturnValueMode
        Public Parameters As RawFunctionCallParameters
        Public Content As [Optional](Of RawFunctionCallContent)

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
        <Tag()> Public _Tag As RawFunctionCallParametersTag
        Public TokenParameters As List(Of Token)
        Public TreeParameter As [Optional](Of SingleLineNode)
        Public TableParameters As List(Of TableLineNode)

        Public Shared Function CreateTokenParameters(ByVal Value As List(Of Token)) As RawFunctionCallParameters
            Return New RawFunctionCallParameters With {._Tag = RawFunctionCallParametersTag.TokenParameters, .TokenParameters = Value}
        End Function
        Public Shared Function CreateTreeParameter(ByVal Value As [Optional](Of SingleLineNode)) As RawFunctionCallParameters
            Return New RawFunctionCallParameters With {._Tag = RawFunctionCallParametersTag.TreeParameter, .TreeParameter = Value}
        End Function
        Public Shared Function CreateTableParameters(ByVal Value As List(Of TableLineNode)) As RawFunctionCallParameters
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
        <Tag()> Public _Tag As RawFunctionCallContentTag
        Public LineContent As FunctionContent
        Public TreeContent As List(Of MultiNodes)
        Public TableContent As List(Of TableLine)

        Public Shared Function CreateLineContent(ByVal Value As FunctionContent) As RawFunctionCallContent
            Return New RawFunctionCallContent With {._Tag = RawFunctionCallContentTag.LineContent, .LineContent = Value}
        End Function
        Public Shared Function CreateTreeContent(ByVal Value As List(Of MultiNodes)) As RawFunctionCallContent
            Return New RawFunctionCallContent With {._Tag = RawFunctionCallContentTag.TreeContent, .TreeContent = Value}
        End Function
        Public Shared Function CreateTableContent(ByVal Value As List(Of TableLine)) As RawFunctionCallContent
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
        Public Name As FunctionDirective
        Public ReturnValueMode As FunctionCallReturnValueMode
        Public Parameters As List(Of Semantics.Node)
        Public Content As [Optional](Of FunctionCallContent)

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
        <Tag()> Public _Tag As FunctionCallContentTag
        Public LineContent As FunctionContent
        Public TreeContent As List(Of Semantics.Node)
        Public TableContent As List(Of FunctionCallTableLine)

        Public Shared Function CreateLineContent(ByVal Value As FunctionContent) As FunctionCallContent
            Return New FunctionCallContent With {._Tag = FunctionCallContentTag.LineContent, .LineContent = Value}
        End Function
        Public Shared Function CreateTreeContent(ByVal Value As List(Of Semantics.Node)) As FunctionCallContent
            Return New FunctionCallContent With {._Tag = FunctionCallContentTag.TreeContent, .TreeContent = Value}
        End Function
        Public Shared Function CreateTableContent(ByVal Value As List(Of FunctionCallTableLine)) As FunctionCallContent
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
        Public Nodes As List(Of Semantics.Node)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
End Namespace
