'==========================================================================
'
'  File:        Semantics.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 语义对象定义
'  Version:     2016.05.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports Firefly.Mapping.MetaSchema

Namespace Texting.TreeFormat.Semantics
    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Forest
        Public Nodes As List(Of Node)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    Public Enum NodeTag
        Empty
        Leaf
        Stem
    End Enum
    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class Node
        <Tag()> Public _Tag As NodeTag
        Public Empty As Unit
        Public Leaf As String
        Public Stem As Stem

        Public Shared Function CreateEmpty() As Node
            Return New Node With {._Tag = NodeTag.Empty, .Empty = New Unit()}
        End Function
        Public Shared Function CreateLeaf(ByVal Value As String) As Node
            Return New Node With {._Tag = NodeTag.Leaf, .Leaf = Value}
        End Function
        Public Shared Function CreateStem(ByVal Value As Stem) As Node
            Return New Node With {._Tag = NodeTag.Stem, .Stem = Value}
        End Function

        Public ReadOnly Property OnEmpty() As Boolean
            Get
                Return _Tag = NodeTag.Empty
            End Get
        End Property
        Public ReadOnly Property OnLeaf() As Boolean
            Get
                Return _Tag = NodeTag.Leaf
            End Get
        End Property
        Public ReadOnly Property OnStem() As Boolean
            Get
                Return _Tag = NodeTag.Stem
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Stem
        Public Name As String
        Public Children As List(Of Node)

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
End Namespace
