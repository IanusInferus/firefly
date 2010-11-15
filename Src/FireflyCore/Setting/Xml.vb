﻿'==========================================================================
'
'  File:        Xml.vb
'  Location:    Firefly.Setting <Visual Basic .Net>
'  Description: Xml读写
'  Version:     2010.11.15.
'  Copyright:   F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Xml
Imports System.Xml.Linq
Imports System.Text
Imports Firefly
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
    '''           | Boolean
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
            Dim Root As XElement
            Using r = XmlReader.Create(Reader)
                Root = XElement.Load(r)
            End Using
            Dim xs As New XmlSerializer(ExternalTypes)
            For Each m In Mappers
                Dim SourceType = m.SourceType
                Dim TargetType = m.TargetType
                Dim DummyMethod = DirectCast(AddressOf PutReaderTranslator(Of DummyType, DummyType2), Action(Of XmlSerializer, IMapper))
                Dim f = DummyMethod.MakeDelegateMethodFromDummy(
                    Function(Type) As Type
                        Select Case Type
                            Case GetType(DummyType)
                                Return SourceType
                            Case GetType(DummyType2)
                                Return TargetType
                            Case Else
                                Return Type
                        End Select
                    End Function
                )
                DirectCast(f, Action(Of XmlSerializer, IMapper))(xs, m)
            Next
            Return xs.Read(Of T)(Root)
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
            Dim xs As New XmlSerializer(ExternalTypes)
            For Each m In Mappers
                Dim SourceType = m.SourceType
                Dim TargetType = m.TargetType
                Dim DummyMethod = DirectCast(AddressOf PutWriterTranslator(Of DummyType, DummyType2), Action(Of XmlSerializer, IMapper))
                Dim f = DummyMethod.MakeDelegateMethodFromDummy(
                    Function(Type) As Type
                        Select Case Type
                            Case GetType(DummyType)
                                Return SourceType
                            Case GetType(DummyType2)
                                Return TargetType
                            Case Else
                                Return Type
                        End Select
                    End Function
                )
                DirectCast(f, Action(Of XmlSerializer, IMapper))(xs, m)
            Next
            Dim Root = xs.Write(Of T)(Value)
            Dim Setting = New XmlWriterSettings With {.Encoding = Writer.Encoding, .Indent = True, .OmitXmlDeclaration = False}
            Using w = XmlWriter.Create(Writer, Setting)
                Root.Save(w)
            End Using
        End Sub

        Private Class DummyType2
        End Class
        Private Shared Sub PutReaderTranslator(Of D, R)(ByVal xs As XmlSerializer, ByVal m As IMapper)
            Dim a = TryCast(m, Mapper(Of D, R))
            If a Is Nothing Then a = New IMapperToMapperAdapter(Of D, R) With {.Mapper = m}
            Dim t = New MapperToIProjectorToProjectorRangeTranslatorAdapter(Of D, R) With {.Mapper = a}
            xs.PutReaderTranslator(Of D, R)(t)
        End Sub
        Private Shared Sub PutWriterTranslator(Of D, R)(ByVal xs As XmlSerializer, ByVal m As IMapper)
            Dim a = TryCast(m, Mapper(Of D, R))
            If a Is Nothing Then a = New IMapperToMapperAdapter(Of D, R) With {.Mapper = m}
            Dim t = New MapperToIProjectorToProjectorDomainTranslatorAdapter(Of D, R) With {.Mapper = a}
            xs.PutWriterTranslator(Of D, R)(t)
        End Sub

        Public Interface IMapper
            ReadOnly Property SourceType() As Type
            ReadOnly Property TargetType() As Type
            Function GetMappedObject(ByVal o As Object) As Object
            Function GetInverseMappedObject(ByVal o As Object) As Object
        End Interface

        Public MustInherit Class Mapper(Of D, R)
            Implements IMapper

            Public ReadOnly Property SourceType() As Type Implements IMapper.SourceType
                Get
                    Return GetType(D)
                End Get
            End Property
            Public ReadOnly Property TargetType() As Type Implements IMapper.TargetType
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

        Private Class IMapperToMapperAdapter(Of D, R)
            Inherits Mapper(Of D, R)
            Public Mapper As IMapper
            Public Overloads Overrides Function GetInverseMappedObject(ByVal o As R) As D
                Return DirectCast(Mapper.GetInverseMappedObject(o), D)
            End Function
            Public Overloads Overrides Function GetMappedObject(ByVal o As D) As R
                Return DirectCast(Mapper.GetMappedObject(o), R)
            End Function
        End Class

        Private Class MapperToIProjectorToProjectorRangeTranslatorAdapter(Of R, M)
            Implements IProjectorToProjectorRangeTranslator(Of R, M)
            Public Mapper As Mapper(Of R, M)
            Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, M)) As Func(Of D, R) Implements IProjectorToProjectorRangeTranslator(Of R, M).TranslateProjectorToProjectorRange
                Return Function(k) Mapper.GetInverseMappedObject(Projector(k))
            End Function
        End Class
        Private Class MapperToIProjectorToProjectorDomainTranslatorAdapter(Of D, M)
            Implements IProjectorToProjectorDomainTranslator(Of D, M)
            Public Mapper As Mapper(Of D, M)
            Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of M, R)) As Func(Of D, R) Implements IProjectorToProjectorDomainTranslator(Of D, M).TranslateProjectorToProjectorDomain
                Return Function(k) Projector(Mapper.GetMappedObject(k))
            End Function
        End Class
    End Class
End Namespace
