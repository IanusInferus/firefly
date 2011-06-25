'==========================================================================
'
'  File:        Semantics.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 语义对象定义
'  Version:     2011.06.26.
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
        Public Property Nodes As Node()

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
        <Tag()> Public Property _Tag As NodeTag
        Public Property Empty As Unit
        Public Property Leaf As String
        Public Property Stem As Stem

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Stem
        Public Property Name As String
        Public Property Children As Node()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
End Namespace
