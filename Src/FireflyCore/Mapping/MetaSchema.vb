﻿'==========================================================================
'
'  File:        MetaSchema.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 元类型结构
'  Version:     2011.03.18.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Diagnostics

Namespace Mapping.MetaSchema
    ''' <summary>记录类型标记</summary>
    ''' <remarks>
    ''' 该标记可选，只要符合记录的语法，序列化器应默认识别为记录。
    ''' 记录，即有若干字段的数据结构。
    ''' 在VB中，记录可定义如下
    ''' 记录 ::= 
    '''     ( 类或结构(构造函数(参数(简单类型)*), 公共只读字段(简单类型)*, 公共可写属性{0}) AND (参数(简单类型)* = 公共只读字段(简单类型)*)
    '''     | 类或结构(构造函数(参数(简单类型)*), 公共可写字段{0}, 公共只读属性(简单类型)*) AND (参数(简单类型)* = 公共只读属性(简单类型)*)
    '''     | 类或结构(无参构造函数, 公共可读写字段(简单类型)*, 公共可写属性{0})
    '''     | 类或结构(无参构造函数, 公共可写字段{0}, 公共可读写属性(简单类型)*)
    '''     ) AND 数据为树状
    ''' </remarks>
    Public Class RecordAttribute
        Inherits Attribute
    End Class

    ''' <summary>别名类型标记</summary>
    ''' <remarks>
    ''' 别名，即有且只有一个字段的记录。字段的名称没有意义。
    ''' </remarks>
    Public Class AliasAttribute
        Inherits Attribute
    End Class

    ''' <summary>标签联合类型标记</summary>
    ''' <remarks>
    ''' 标签联合，即一个这样的记录，它有其仅有一个枚举字段标记其余字段中哪一个是有效的。该枚举字段应用TagAttribute标记。
    ''' </remarks>
    Public Class TaggedUnionAttribute
        Inherits Attribute
    End Class

    ''' <summary>元组类型标记</summary>
    ''' <remarks>
    ''' 元组，即一个这样的记录，它的字段的顺序和类型有意义，而名称没有意义。
    ''' </remarks>
    Public Class TupleAttribute
        Inherits Attribute
    End Class

    ''' <summary>标签字段标记</summary>
    ''' <remarks>
    ''' 用于标记标签联合的标签字段。
    ''' </remarks>
    Public Class TagAttribute
        Inherits Attribute
    End Class


    Public Enum ConceptDefTag
        Primitive
        [Alias]
        Record
        TaggedUnion
    End Enum

    Public Enum ConceptSpecTag
        ConceptRef
        Tuple
        List
    End Enum

    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class ConceptDef
        <Tag()> Public _Tag As ConceptDefTag
        Public Primitive As Primitive
        Public [Alias] As [Alias]
        Public Record As Record
        Public TaggedUnion As TaggedUnion

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <[Alias](), DebuggerDisplay("{ToString()}")>
    Public Class ConceptRef
        Public Value As String

        Public Shared Widening Operator CType(ByVal o As String) As ConceptRef
            Return New ConceptRef With {.Value = o}
        End Operator
        Public Shared Widening Operator CType(ByVal c As ConceptRef) As String
            Return c.Value
        End Operator

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <TaggedUnion(), DebuggerDisplay("{ToString()}")>
    Public Class ConceptSpec
        <Tag()> Public _Tag As ConceptSpecTag
        Public ConceptRef As ConceptRef
        Public Tuple As Tuple
        Public List As List

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Unit
        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <[Alias](), DebuggerDisplay("{ToString()}")>
    Public Class Primitive
        Public Value As String

        Public Shared Widening Operator CType(ByVal o As String) As Primitive
            Return New Primitive With {.Value = o}
        End Operator
        Public Shared Widening Operator CType(ByVal c As Primitive) As String
            Return c.Value
        End Operator

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class [Alias]
        Public Name As String
        Public Type As ConceptSpec

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Tuple
        Public Types As ConceptSpec()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class List
        Public ElementType As ConceptSpec

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Field
        Public Name As String
        Public Type As ConceptSpec

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Record
        Public Name As String
        Public Fields As Field()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Alternative
        Public Name As String
        Public Type As ConceptSpec

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class TaggedUnion
        Public Name As String
        Public Alternatives As Alternative()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class

    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Schema
        Public Concepts As ConceptDef()

        Public Overrides Function ToString() As String
            Return DebuggerDisplayer.ConvertToString(Me)
        End Function
    End Class
End Namespace
