'==========================================================================
'
'  File:        Xml.vb
'  Location:    Firefly.Setting <Visual Basic .Net>
'  Description: Xml读写
'  Version:     2010.11.13.
'  Copyright:   F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Mapping

Namespace Setting
    ''' <summary>
    ''' Xml
    ''' 
    ''' 用于将对象格式化到Xml文件及从Xml文件恢复数据
    ''' 简单类型能够直接格式化
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
    ''' 对于类对象，允许出现null。
    ''' 允许使用继承，但所有不直接出现在根类型的类型声明的类型树中的类型必须添加到ExternalTypes中
    ''' ExternalTypes中不应有命名冲突
    ''' 
    ''' 如果不能满足这些条件，可以使用类型替代
    ''' 需要注意的是，不要在替代器中返回替代类的子类，比如，不能使用List(Of Int32)来做某个类->ICollection(Of Int32)的替代，这时应明确为某个类->List(Of Int32)的替代
    ''' </summary>
    Public NotInheritable Class Xml
        Private Sub New()
        End Sub

        Public Shared Function ReadFile(Of T)(ByVal Path As String) As T
            Return ReadFile(Of T)(Path, New Type() {}, New IMapper() {})
        End Function

        Public Shared Function ReadFile(Of T)(ByVal Reader As StreamReader) As T
            Return ReadFile(Of T)(Reader, New Type() {}, New IMapper() {})
        End Function

        Public Shared Function ReadFile(Of T)(ByVal Path As String, ByVal ExternalTypes As IEnumerable(Of Type)) As T
            Return ReadFile(Of T)(Path, ExternalTypes, New IMapper() {})
        End Function

        Public Shared Function ReadFile(Of T)(ByVal Path As String, ByVal Mappers As IEnumerable(Of IMapper)) As T
            Return ReadFile(Of T)(Path, New Type() {}, Mappers)
        End Function

        Public Shared Function ReadFile(Of T)(ByVal Path As String, ByVal ExternalTypes As IEnumerable(Of Type), ByVal Mappers As IEnumerable(Of IMapper)) As T
            Using s As New FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using sr As New StreamReader(s)
                    Return ReadFile(Of T)(sr, ExternalTypes, Mappers)
                End Using
            End Using
        End Function

        Public Shared Function ReadFile(Of T)(ByVal Reader As StreamReader, ByVal ExternalTypes As IEnumerable(Of Type), ByVal Mappers As IEnumerable(Of IMapper)) As T
            Dim s = Reader
            Using r = XmlReader.Create(s)
                Dim Root = XElement.Load(r)
                Return DirectCast(InvMapElement("", Root, GetType(T), ExternalTypes.ToDictionary(Function(type) GetTypeFriendlyName(type), StringComparer.OrdinalIgnoreCase), Mappers.ToDictionary(Function(m) m.SourceType)), T)
            End Using
        End Function

        Public Shared Sub WriteFile(Of T)(ByVal Path As String, ByVal Value As T)
            WriteFile(Path, TextEncoding.WritingDefault, Value, New Type() {}, New IMapper() {})
        End Sub

        Public Shared Sub WriteFile(Of T)(ByVal Writer As StreamWriter, ByVal Value As T)
            WriteFile(Writer, Value, New Type() {}, New IMapper() {})
        End Sub

        Public Shared Sub WriteFile(Of T)(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As T)
            WriteFile(Path, Encoding, Value, New Type() {}, New IMapper() {})
        End Sub

        Public Shared Sub WriteFile(Of T)(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As T, ByVal ExternalTypes As IEnumerable(Of Type))
            WriteFile(Path, Encoding, Value, ExternalTypes, New IMapper() {})
        End Sub

        Public Shared Sub WriteFile(Of T)(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As T, ByVal Mappers As IEnumerable(Of IMapper))
            WriteFile(Path, Encoding, Value, New Type() {}, Mappers)
        End Sub

        Public Shared Sub WriteFile(Of T)(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As T, ByVal ExternalTypes As IEnumerable(Of Type), ByVal Mappers As IEnumerable(Of IMapper))
            Using tw = Txt.CreateTextWriter(Path, Encoding)
                WriteFile(tw, Value, ExternalTypes, Mappers)
            End Using
        End Sub

        Public Shared Sub WriteFile(Of T)(ByVal Writer As StreamWriter, ByVal Value As T, ByVal ExternalTypes As IEnumerable(Of Type), ByVal Mappers As IEnumerable(Of IMapper))
            Dim Root = MapElement("", Value, GetType(T), ExternalTypes.ToDictionary(Function(type) GetTypeFriendlyName(type), StringComparer.OrdinalIgnoreCase), Mappers.ToDictionary(Function(m) m.SourceType))
            Dim Setting = New XmlWriterSettings With {.Encoding = Writer.Encoding, .Indent = True, .OmitXmlDeclaration = False}
            Dim tw = Writer
            Using w = XmlWriter.Create(tw, Setting)
                Root.Save(w)
            End Using
        End Sub

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

        Private Shared Function MapElement(ByVal Name As String, ByVal Value As Object, ByVal ConstraintType As Type, ByVal ExternalTypeDict As Dictionary(Of String, Type), ByVal MapperDict As Dictionary(Of Type, IMapper)) As XElement
            Dim Element As XElement

            Dim PhysicalType = ConstraintType
            If Value IsNot Nothing Then PhysicalType = Value.GetType

            If Name = "" Then
                Element = New XElement(GetTypeFriendlyName(PhysicalType))
            Else
                Element = New XElement(Name)
            End If

            If PhysicalType IsNot ConstraintType Then
                If Not ExternalTypeDict.ContainsKey(GetTypeFriendlyName(PhysicalType)) Then Throw New InvalidOperationException("TypeNotInExternalTypes: {0}".Formats(GetTypeFriendlyName(PhysicalType)))
                Element.@<Type> = GetTypeFriendlyName(PhysicalType)
            End If

            '注意：即使有转换，也保存转换之前的类型名称

            If MapperDict.ContainsKey(PhysicalType) Then
                Dim m = MapperDict(PhysicalType)
                Value = m.GetMappedObject(Value)
                PhysicalType = Value.GetType
                ConstraintType = m.TargetType
                If PhysicalType IsNot ConstraintType Then Throw New NotSupportedException("MapperTypeDismatch: {0}->{1}, {2}".Formats(GetTypeFriendlyName(m.SourceType), GetTypeFriendlyName(m.TargetType), PhysicalType))
            ElseIf MapperDict.ContainsKey(ConstraintType) Then
                Dim m = MapperDict(ConstraintType)
                Value = m.GetMappedObject(Value)
                PhysicalType = Value.GetType
                ConstraintType = m.TargetType
                If PhysicalType IsNot ConstraintType Then Throw New NotSupportedException("MapperTypeDismatch: {0}->{1}, {2}".Formats(GetTypeFriendlyName(m.SourceType), GetTypeFriendlyName(m.TargetType), PhysicalType))
            End If

            If Value Is Nothing Then Return Element

            If PhysicalType Is GetType(String) Then
                Element.Value = DirectCast(Value, String)
                Return Element
            End If

            If PhysicalType Is GetType(Decimal) Then
                Element.Value = DirectCast(Value, Decimal).ToString(Globalization.CultureInfo.InvariantCulture)
                Return Element
            End If

            If PhysicalType.IsPrimitive Then
                If PhysicalType Is GetType(Single) Then
                    Element.Value = DirectCast(Value, Single).ToString("r", Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(Double) Then
                    Element.Value = DirectCast(Value, Double).ToString("r", Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(Boolean) Then
                    Element.Value = DirectCast(Value, Boolean).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(Byte) Then
                    Element.Value = DirectCast(Value, Byte).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(SByte) Then
                    Element.Value = DirectCast(Value, SByte).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(Int16) Then
                    Element.Value = DirectCast(Value, Int16).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(UInt16) Then
                    Element.Value = DirectCast(Value, UInt16).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(Int32) Then
                    Element.Value = DirectCast(Value, Int32).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(UInt32) Then
                    Element.Value = DirectCast(Value, UInt32).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(Int64) Then
                    Element.Value = DirectCast(Value, Int64).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(UInt64) Then
                    Element.Value = DirectCast(Value, UInt64).ToString(Globalization.CultureInfo.InvariantCulture)
                    Return Element
                End If

                If PhysicalType Is GetType(IntPtr) Then
                    Throw New NotSupportedException("IntPtrNotSerializable")
                End If

                If PhysicalType Is GetType(Char) Then
                    Throw New NotSupportedException("Char16ShallNotBeSerialized")
                End If

                Throw New NotSupportedException("UnexpectedPrimitive {0}".Formats(GetTypeFriendlyName(PhysicalType)))
            End If

            If PhysicalType.IsEnum Then
                Element.Value = Value.ToString()
                Return Element
            End If

            If PhysicalType.IsArray Then
                Dim n = PhysicalType.GetArrayRank
                If n <> 1 Then Throw New NotSupportedException("MultidimesionArrayNotSupported: {0}".Formats(GetTypeFriendlyName(PhysicalType)))
                Dim ObjectType = PhysicalType.GetElementType

                Dim EmptyFlag = True
                For Each o In CType(Value, Array)
                    Element.Add(MapElement("", o, ObjectType, ExternalTypeDict, MapperDict))
                    EmptyFlag = False
                Next
                If EmptyFlag Then Element.Value = ""
                Return Element
            End If

            If PhysicalType.IsCollectionType() Then
                Dim enu = DirectCast(Value, IEnumerable)
                Dim ObjectType = GetCollectionElementType(PhysicalType)

                Dim EmptyFlag = True
                For Each o In enu
                    Element.Add(MapElement("", o, ObjectType, ExternalTypeDict, MapperDict))
                    EmptyFlag = False
                Next
                If EmptyFlag Then Element.Value = ""
                Return Element
            End If

            If PhysicalType.IsClass Then
                Dim c = PhysicalType.GetConstructor(New Type() {})
                If c Is Nothing OrElse Not c.IsPublic Then Throw New NotSupportedException("NoPublicDefaultConstructor: {0}".Formats(GetTypeFriendlyName(PhysicalType)))
                Dim EmptyFlag = True
                For Each f In PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance)
                    If Not f.IsInitOnly Then
                        Element.Add(MapElement(f.Name, f.GetValue(Value), f.FieldType, ExternalTypeDict, MapperDict))
                        EmptyFlag = False
                    End If
                Next
                For Each f In PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                    If f.CanRead AndAlso f.CanWrite AndAlso f.GetIndexParameters.Length = 0 Then
                        Element.Add(MapElement(f.Name, f.GetValue(Value, Nothing), f.PropertyType, ExternalTypeDict, MapperDict))
                        EmptyFlag = False
                    End If
                Next
                If EmptyFlag Then Element.Value = ""
                Return Element
            End If

            If PhysicalType.IsValueType Then
                Dim HasData As Boolean = False
                For Each f In PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance)
                    If Not f.IsInitOnly Then
                        HasData = True
                        Element.Add(MapElement(f.Name, f.GetValue(Value), f.FieldType, ExternalTypeDict, MapperDict))
                    End If
                Next
                For Each f In PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                    If f.CanRead AndAlso f.CanWrite AndAlso f.GetIndexParameters.Length = 0 Then
                        HasData = True
                        Element.Add(MapElement(f.Name, f.GetValue(Value, Nothing), f.PropertyType, ExternalTypeDict, MapperDict))
                    End If
                Next
                If Not HasData Then Throw New InvalidOperationException("NonPublicValueType: {0}".Formats(GetTypeFriendlyName(PhysicalType)))
                Return Element
            End If

            Throw New NotSupportedException
        End Function

        Private Shared Function InvMapElement(ByVal Name As String, ByVal Element As XElement, ByVal ConstraintType As Type, ByVal ExternalTypeDict As Dictionary(Of String, Type), ByVal MapperDict As Dictionary(Of Type, IMapper)) As Object
            Dim PhysicalType = ConstraintType
            If Element.@<Type> <> "" Then
                If ExternalTypeDict.ContainsKey(Element.@<Type>) Then
                    PhysicalType = ExternalTypeDict(Element.@<Type>)
                Else
                    Throw New InvalidOperationException("TypeNotResovled: {0}".Formats(Element.@<Type>))
                End If
            End If

            Dim m As IMapper = Nothing
            If MapperDict.ContainsKey(PhysicalType) Then
                m = MapperDict(PhysicalType)
                PhysicalType = m.TargetType
                ConstraintType = m.TargetType
            ElseIf MapperDict.ContainsKey(ConstraintType) Then
                m = MapperDict(ConstraintType)
                PhysicalType = m.TargetType
                ConstraintType = m.TargetType
            End If

            Dim InvM As Func(Of Object, Object)
            If m Is Nothing Then
                InvM = Function(o As Object) o
            Else
                InvM = Function(o As Object) m.GetInverseMappedObject(o)
            End If

            If Element.IsEmpty Then Return InvM(Nothing)

            If PhysicalType Is GetType(String) Then
                Return InvM(Element.Value)
            End If

            If PhysicalType Is GetType(Decimal) Then
                Return InvM(Decimal.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
            End If

            If PhysicalType.IsPrimitive Then
                If PhysicalType Is GetType(Single) Then
                    Return InvM(Single.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(Double) Then
                    Return InvM(Double.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(Boolean) Then
                    Return InvM(Boolean.Parse(Element.Value))
                End If

                If PhysicalType Is GetType(Byte) Then
                    Return InvM(Byte.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(SByte) Then
                    Return InvM(SByte.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(Int16) Then
                    Return InvM(Int16.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(UInt16) Then
                    Return InvM(UInt16.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(Int32) Then
                    Return InvM(Int32.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(UInt32) Then
                    Return InvM(UInt32.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(Int64) Then
                    Return InvM(Int64.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(UInt64) Then
                    Return InvM(UInt64.Parse(Element.Value, Globalization.CultureInfo.InvariantCulture))
                End If

                If PhysicalType Is GetType(IntPtr) Then
                    Throw New NotSupportedException("IntPtrNotSerializable")
                End If

                If PhysicalType Is GetType(Char) Then
                    Throw New NotSupportedException("Char16ShallNotBeSerialized")
                End If

                Throw New NotSupportedException("UnexpectedPrimitive {0}".Formats(GetTypeFriendlyName(PhysicalType)))
            End If

            If PhysicalType.IsEnum Then
                Return InvM(System.Enum.Parse(PhysicalType, Element.Value, True))
            End If

            If PhysicalType.IsArray Then
                Dim n = PhysicalType.GetArrayRank
                If n <> 1 Then Throw New NotSupportedException("MultidimesionArrayNotSupported: {0}".Formats(GetTypeFriendlyName(PhysicalType)))
                Dim ObjectType = PhysicalType.GetElementType

                Dim SubElements = Element.Elements.ToArray
                Dim arr = Array.CreateInstance(ObjectType, SubElements.Count)
                For n = 0 To arr.Length - 1
                    arr.SetValue(InvMapElement("", SubElements(n), ObjectType, ExternalTypeDict, MapperDict), n)
                Next
                Return InvM(arr)
            End If

            If PhysicalType.IsCollectionType() Then
                Dim ObjectType = GetCollectionElementType(PhysicalType)
                Dim AddDelegate As Action(Of ICollection(Of DummyType), DummyType) = AddressOf ListAdd(Of DummyType, ICollection(Of DummyType))
                Dim Add = AddDelegate.MakeDelegateMethodFromDummy(ObjectType)

                Dim SubElements = Element.Elements.ToArray
                Dim enu = DirectCast(Activator.CreateInstance(PhysicalType), IEnumerable)
                For n = 0 To SubElements.Length - 1
                    Add.DynamicInvoke(New Object() {enu, InvMapElement("", SubElements(n), ObjectType, ExternalTypeDict, MapperDict)})
                Next
                Return InvM(enu)
            End If

            If PhysicalType.IsClass Then
                Dim c = PhysicalType.GetConstructor(New Type() {})
                If c Is Nothing OrElse Not c.IsPublic Then Throw New NotSupportedException("NoPublicDefaultConstructor: {0}".Formats(GetTypeFriendlyName(PhysicalType)))

                Dim obj = Activator.CreateInstance(PhysicalType)

                Dim SubElementDict = Element.Elements.ToDictionary(Function(e) e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                For Each f In PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance)
                    If Not f.IsInitOnly Then
                        If SubElementDict.ContainsKey(f.Name) Then
                            f.SetValue(obj, InvMapElement(f.Name, SubElementDict(f.Name), f.FieldType, ExternalTypeDict, MapperDict))
                        End If
                    End If
                Next
                For Each f In PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                    If f.CanRead AndAlso f.CanWrite AndAlso f.GetIndexParameters.Length = 0 Then
                        If SubElementDict.ContainsKey(f.Name) Then
                            f.SetValue(obj, InvMapElement(f.Name, SubElementDict(f.Name), f.PropertyType, ExternalTypeDict, MapperDict), Nothing)
                        End If
                    End If
                Next
                Return InvM(obj)
            End If

            If PhysicalType.IsValueType Then
                '必须定义为ValueType，否则无法正常设置值
                Dim obj = DirectCast(Activator.CreateInstance(PhysicalType), ValueType)

                Dim SubElementDict = Element.Elements.ToDictionary(Function(e) e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                For Each f In PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance)
                    If Not f.IsInitOnly Then
                        If SubElementDict.ContainsKey(f.Name) Then
                            f.SetValue(obj, InvMapElement(f.Name, SubElementDict(f.Name), f.FieldType, ExternalTypeDict, MapperDict))
                        End If
                    End If
                Next
                For Each f In PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                    If f.CanRead AndAlso f.CanWrite AndAlso f.GetIndexParameters.Length = 0 Then
                        If SubElementDict.ContainsKey(f.Name) Then
                            f.SetValue(obj, InvMapElement(f.Name, SubElementDict(f.Name), f.PropertyType, ExternalTypeDict, MapperDict), Nothing)
                        End If
                    End If
                Next
                Return InvM(obj)
            End If

            Throw New NotSupportedException
        End Function
        Private Shared Sub ListAdd(Of T, TList As ICollection(Of T))(ByVal l As TList, ByVal v As T)
            l.Add(v)
        End Sub

        Public Interface IMapper
            ReadOnly Property SourceType() As Type
            ReadOnly Property TargetType() As Type
            Function GetMappedObject(ByVal o As Object) As Object
            Function GetInverseMappedObject(ByVal o As Object) As Object
        End Interface

        Public MustInherit Class Mapper(Of D, R)
            Implements IMapper

            Public ReadOnly Property SourceType() As System.Type Implements IMapper.SourceType
                Get
                    Return GetType(D)
                End Get
            End Property

            Public ReadOnly Property TargetType() As System.Type Implements IMapper.TargetType
                Get
                    Return GetType(R)
                End Get
            End Property

            Public MustOverride Function GetMappedObject(ByVal o As D) As R

            Public MustOverride Function GetInverseMappedObject(ByVal o As R) As D

            Public Function GetMappedObject(ByVal o As Object) As Object Implements IMapper.GetMappedObject
                Dim d = DirectCast(o, D)
                Return GetMappedObject(d)
            End Function

            Public Function GetInverseMappedObject(ByVal o As Object) As Object Implements IMapper.GetInverseMappedObject
                Dim r = DirectCast(o, R)
                Return GetInverseMappedObject(r)
            End Function
        End Class
    End Class
End Namespace
