'==========================================================================
'
'  File:        XmlSerializer.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Xml序列化类
'  Version:     2010.11.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

''' <remarks>
''' 对于非简单类型，应提供自定义序列化器
''' 简单类型 ::= 简单类型
'''           | Byte(UInt8) | UInt16 | UInt32 | UInt64 | Int8(SByte) | Int16 | Int32 | Int64 | Float32(Single) | Float64(Double)
'''           | String | Decimal
'''           | 枚举
'''           | 数组(简单类型)
'''           | ICollection(简单类型)
'''           | 简单类或结构
''' 简单类或结构 ::= 
'''               ( 类或结构(构造函数(参数(简单类型)*), 公共只读字段(简单类型)*, 公共可写属性{0}) AND (参数(简单类型)* = 公共只读字段(简单类型)*)
'''               | 类或结构(构造函数(参数(简单类型)*), 公共可写字段{0}, 公共只读属性(简单类型)*) AND (参数(简单类型)* = 公共只读属性(简单类型)*)
'''               | 类或结构(无参构造函数, 公共可读写字段(简单类型)*, 公共可写属性{0})
'''               | 类或结构(无参构造函数, 公共可写字段{0}, 公共可读写属性(简单类型)*)
'''               ) AND 类型结构为树状
''' </remarks>
Public Class XmlSerializer
    'Public Sub PutReader(Of T)(ByVal Reader As Func(Of XElement, T))
    '    ReaderMapperValue.PutMapper(Of T)(Reader)
    'End Sub
    'Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, XElement))
    '    WriterMapperValue.PutMapper(Of T)(Writer)
    'End Sub

    'Public Function GetReader(Of T)() As Func(Of XElement, T)
    '    Return ReaderMapperValue.GetMapper(Of T)()
    'End Function
    'Public Function GetWriter(Of T)() As Action(Of T, XElement)
    '    Return WriterMapperValue.GetMapper(Of T)()
    'End Function

    'Public Function Read(Of T)(ByVal s As XElement) As T
    '    Return GetReader(Of T)()(s)
    'End Function
    'Public Sub Write(Of T)(ByVal Value As T, ByVal s As XElement)
    '    GetWriter(Of T)()(Value, s)
    'End Sub

    Private Shared Function GetTypeFriendlyName(ByVal Type As Type) As String
        If Type.IsArray Then
            Dim n = Type.GetArrayRank
            Dim ElementTypeName = GetTypeFriendlyName(Type.GetElementType)
            If n = 1 Then
                Return "ArrayOf" & ElementTypeName
            End If
            Return "Array" & n & "Of" & ElementTypeName
        End If
        If Type.IsGenericType Then
            Dim Name = Regex.Match(Type.Name, "^(?<Name>.*?)`.*$", RegexOptions.ExplicitCapture).Result("${Name}")
            Return Name & "Of" & String.Join("And", (From t In Type.GetGenericArguments() Select GetTypeFriendlyName(t)).ToArray)
        End If
        Return Type.Name
    End Function

    'PutReader(Function(s As XElement) Byte.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) UInt16.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) UInt32.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) UInt64.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) SByte.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) Int16.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) Int32.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) Int64.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) Single.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) Double.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))
    'PutReader(Function(s As XElement) s.Value)
    'PutReader(Function(s As XElement) Decimal.Parse(s.Value, Globalization.CultureInfo.InvariantCulture))

    'PutWriter(Sub(b As Byte, s As XElement) s.Value = b.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(i As UInt16, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(i As UInt32, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(i As UInt64, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(i As SByte, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(i As Int16, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(i As Int32, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(i As Int64, s As XElement) s.Value = i.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(f As Single, s As XElement) s.Value = f.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(f As Double, s As XElement) s.Value = f.ToString(Globalization.CultureInfo.InvariantCulture))
    'PutWriter(Sub(str As String, s As XElement) s.Value = str)
    'PutWriter(Sub(d As Decimal, s As XElement) s.Value = d.ToString(Globalization.CultureInfo.InvariantCulture))
End Class
