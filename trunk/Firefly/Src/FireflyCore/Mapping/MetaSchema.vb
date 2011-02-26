'==========================================================================
'
'  File:        MetaSchema.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 元类型结构
'  Version:     2011.02.27.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System

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
End Namespace
