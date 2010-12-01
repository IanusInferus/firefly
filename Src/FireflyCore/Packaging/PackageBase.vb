'==========================================================================
'
'  File:        PackageBase.vb
'  Location:    Firefly.Packaging <Visual Basic .Net>
'  Description: 文件包基类
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Option Compare Text
Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Firefly.TextEncoding
Imports Firefly.Streaming

Namespace Packaging
    ''' <summary>
    ''' 文件包基类
    ''' 
    ''' 
    ''' 给继承者的说明：
    ''' 
    ''' 如果文件包支持写入，应
    ''' (1)在加入一个FileDB时，调用PushFile方法，使得它被加入到FileList、IndexOfFile中，以及PushFileToDir到根目录FileDB中，若根目录FileDB不存在，则空的根目录会自动创建
    ''' 
    ''' 请使用PackageRegister来注册文件包类型。
    ''' 应提供一个返回"ISO(*.ISO)|*.ISO"形式字符串的Filter属性，
    ''' 并按照PackageRegister中的委托类型提供一个Open函数、一个Create函数(如果支持创建)。
    ''' </summary>
    Public MustInherit Class PackageBase
        Implements IDisposable
        Protected Readable As IReadableSeekableStream
        Protected Writable As IStream

        ''' <summary>全部文件的集合。</summary>
        Protected FileList As New List(Of FileDB)
        ''' <summary>文件到文件编号的映射。</summary>
        Protected IndexOfFile As New Dictionary(Of FileDB, Integer)

        ''' <summary>已重载。默认构照函数。请手动初始化BaseStream。</summary>
        Protected Sub New()
        End Sub
        ''' <summary>已重载。打开文件包。</summary>
        Public Sub New(ByVal sp As NewReadingStreamPasser)
            Readable = sp.GetStream
        End Sub
        ''' <summary>已重载。打开或创建文件包。</summary>
        Public Sub New(ByVal sp As NewReadingWritingStreamPasser)
            Readable = sp.GetStream
            Writable = sp.GetStream
        End Sub

        Protected RootValue As FileDB = New FileDB("", FileDB.FileType.Directory, -1, 0)
        ''' <summary>文件根。</summary>
        Public ReadOnly Property Root() As FileDB
            Get
                Return RootValue
            End Get
        End Property

        ''' <summary>全部文件的集合。</summary>
        Public ReadOnly Property Files() As IEnumerable(Of FileDB)
            Get
                Return FileList
            End Get
        End Property

        ''' <summary>把文件FileDB放入根目录FileDB。若根目录FileDB不存在，则空的根目录会自动创建。在加入一个FileDB时，调用该方法，使得它被加入到FileList、IndexOfFile中，以及PushFileToDir到根目录FileDB中。</summary>
        Protected Overloads Sub PushFile(ByVal f As FileDB)
            PushFile(f, RootValue)
        End Sub
        ''' <summary>将文件FileDB放入文件夹的FileDB。在加入一个FileDB时，调用该方法，使得它被加入到FileList、IndexOfFile中，以及PushFileToDir到文件夹FileDB中。</summary>
        Protected Overridable Overloads Sub PushFile(ByVal f As FileDB, ByVal Directory As FileDB)
            PushFileToDir(f, Directory)

            Dim n = FileList.Count
            FileList.Add(f)
            IndexOfFile.Add(f, n)
        End Sub

        ''' <summary>将文件FileDB放入文件夹的FileDB。请只在PushFile中调用。</summary>
        Protected Sub PushFileToDir(ByVal File As FileDB, ByVal Directory As FileDB)
            Dim Dir As String = ""
            If File.Name.Contains("\") OrElse File.Name.Contains("/") Then Dir = PopFirstDir(File.Name)
            If Dir = "" Then
                Directory.SubFile.Add(File)
                Directory.SubFileNameRef.Add(File.Name, File)
                File.ParentFileDB = Directory
            Else
                If Not Directory.SubFileNameRef.ContainsKey(Dir) Then
                    Dim DirDB As FileDB = FileDB.CreateDirectory(Dir)
                    Directory.SubFile.Add(DirDB)
                    Directory.SubFileNameRef.Add(DirDB.Name, DirDB)
                    DirDB.ParentFileDB = Directory
                End If
                PushFileToDir(File, Directory.SubFileNameRef(Dir))
            End If
        End Sub

        ''' <summary>尝试按路径取得FileDB。</summary>
        Public Function TryGetFileDB(ByVal Path As String) As FileDB
            Dim p As String = Path
            Dim ret As FileDB = Root
            Dim d As String = ""
            If p = "" Then Return Root
            If Root.Name <> "" Then
                d = PopFirstDir(p)
                If d <> Root.Name Then p = Path
            End If
            While ret IsNot Nothing
                d = PopFirstDir(p)
                If d = "" Then Return ret
                If ret.SubFileNameRef.ContainsKey(d) Then
                    ret = ret.SubFileNameRef(d)
                Else
                    Return Nothing
                End If
            End While
            Return Nothing
        End Function

        ''' <summary>从文件头标识符猜测文件扩展名。</summary>
        Public Shared Function GuessExtensionFromMagicIdentifier(ByVal MagicIdentifier As Int32, ByVal DefaultExt As String) As String
            Dim Ext As String = (New Char32() {ChrQ(MagicIdentifier And &HFF), ChrQ((MagicIdentifier >> 8) And &HFF), ChrQ((MagicIdentifier >> 16) And &HFF), ChrQ((MagicIdentifier >> 24) And &HFF)}).ToUTF16B
            Dim Match = Regex.Match(Ext, "[A-Za-z][0-9A-Za-z]{2,}")
            If Match.Success Then Return Match.Value
            Match = Regex.Match(Ext.Substring(0, 2), "[A-Za-z]{2}")
            If Match.Success Then Return Match.Value
            Return DefaultExt
        End Function

        ''' <summary>获得一个FileDB的子文件。</summary>
        Public Shared Function GetFiles(ByVal File As FileDB, Optional ByVal Mask As String = "*") As IEnumerable(Of FileDB)
            Dim l As New List(Of FileDB)
            Select Case File.Type
                Case FileDB.FileType.File
                    If IsMatchFileMask(File.Name, Mask) Then
                        l.Add(File)
                    End If
                Case FileDB.FileType.Directory
                    For Each f As FileDB In File.SubFile
                        l.AddRange(GetFiles(f, Mask))
                    Next
                Case Else
                    Throw New InvalidOperationException
            End Select
            Return l
        End Function

        ''' <summary>获得一个FileDB的子文件。</summary>
        Public Shared Function GetFilesAndPaths(ByVal File As FileDB, ByVal Path As String, ByVal MustExist As Boolean, Optional ByVal Mask As String = "*") As IEnumerable(Of KeyValuePair(Of FileDB, String))
            Dim l As New List(Of KeyValuePair(Of FileDB, String))
            Select Case File.Type
                Case FileDB.FileType.File
                    If IsMatchFileMask(File.Name, Mask) Then
                        If Not MustExist OrElse IO.File.Exists(Path) Then
                            l.Add(New KeyValuePair(Of FileDB, String)(File, Path))
                        End If
                    End If
                Case FileDB.FileType.Directory
                    If Not MustExist OrElse IO.Directory.Exists(Path) Then
                        For Each f As FileDB In File.SubFile
                            l.AddRange(GetFilesAndPaths(f, GetPath(Path, f.Name), MustExist, Mask))
                        Next
                    End If
                Case Else
                    Throw New InvalidOperationException
            End Select
            Return l
        End Function

        ''' <summary>从包中解出一个文件。</summary>
        Protected Overridable Sub ExtractSingleInner(ByVal File As FileDB, ByVal sp As NewWritingStreamPasser)
            Dim s = sp.GetStream
            With File
                Using f = Writable.Partialize(.Address, .Length)
                    s.WriteFromStream(f, f.Length)
                End Using
            End With
        End Sub
        ''' <summary>从包中解出一个文件。应优先考虑覆盖ExtractSingleInner。默认实现调用ExtractSingleInner。</summary>
        Protected Overridable Sub ExtractSingleInnerPath(ByVal File As FileDB, ByVal Path As String)
            Using t = StreamEx.Create(Path, FileMode.Create)
                ExtractSingleInner(File, t.AsNewWriting)
            End Using
        End Sub
        ''' <summary>从包中解出多个文件。应优先考虑覆盖ExtractSingleInner。默认实现调用ExtractSingleInner。</summary>
        Protected Overridable Sub ExtractMultipleInner(ByVal Files As FileDB(), ByVal StreamPassers As NewWritingStreamPasser())
            For n As Integer = 0 To Files.Length - 1
                ExtractSingleInner(Files(n), StreamPassers(n))
            Next
        End Sub
        ''' <summary>从包中解出多个文件。应优先考虑覆盖ExtractMultipleInner。默认实现调用ExtractMultipleInner。</summary>
        Protected Overridable Sub ExtractMultipleInnerPath(ByVal Files As FileDB(), ByVal Paths As String())
            Dim Streams As New List(Of IStream)
            Dim StreamPassers As New List(Of NewWritingStreamPasser)
            Try
                For n As Integer = 0 To Files.Length - 1
                    Dim s = StreamEx.Create(Paths(n), FileMode.Create)
                    Streams.Add(s)
                    StreamPassers.Add(s.AsNewWriting)
                Next
                ExtractMultipleInner(Files, StreamPassers.ToArray)
            Finally
                For Each s In Streams
                    s.Dispose()
                Next
            End Try
        End Sub

        ''' <summary>已重载。从包中解出一个文件。该函数不再可覆盖，请覆盖ExtractSingleInner。调用ExtractSingleInner。</summary>
        Public Sub ExtractSingle(ByVal File As FileDB, ByVal sp As NewWritingStreamPasser)
            If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            ExtractSingleInner(File, sp)
        End Sub
        ''' <summary>已重载。从包中解出一个文件。默认实现调用ExtractSingleInnerPath。</summary>
        Public Sub ExtractSingle(ByVal File As FileDB, ByVal Path As String)
            If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            Dim Dir As String = GetFileDirectory(Path)
            If Dir <> "" AndAlso Not IO.Directory.Exists(Dir) Then IO.Directory.CreateDirectory(Dir)
            ExtractSingleInnerPath(File, Path)
        End Sub
        ''' <summary>已重载。从包中解出多个文件。调用ExtractMultipleInner。</summary>
        Public Sub ExtractMultiple(ByVal Files As IEnumerable(Of FileDB), ByVal StreamPassers As IEnumerable(Of NewWritingStreamPasser))
            If Files.Count <> StreamPassers.Count Then Throw New ArgumentException
            For Each File In Files
                If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            Next
            ExtractMultipleInner(Files.ToArray, StreamPassers.ToArray)
        End Sub
        ''' <summary>已重载。从包中解出多个文件。调用ExtractMultipleInnerPath。</summary>
        Public Sub ExtractMultiple(ByVal Files As IEnumerable(Of FileDB), ByVal Paths As IEnumerable(Of String))
            If Files.Count <> Paths.Count Then Throw New ArgumentException
            For Each File In Files
                If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            Next
            For Each Path In Paths
                Dim Dir As String = GetFileDirectory(Path)
                If Dir <> "" AndAlso Not IO.Directory.Exists(Dir) Then IO.Directory.CreateDirectory(Dir)
            Next
            ExtractMultipleInnerPath(Files.ToArray, Paths.ToArray)
        End Sub
        ''' <summary>已重载。从包中解出一个文件或文件夹。调用ExtractSingle和ExtractMultiple。</summary>
        Public Sub Extract(ByVal File As FileDB, ByVal Path As String, Optional ByVal Mask As String = "*")
            Extract(New FileDB() {File}, New String() {Path}, Mask)
        End Sub
        ''' <summary>已重载。从包中解出多个文件或文件夹。调用ExtractSingle和ExtractMultiple。</summary>
        Public Sub Extract(ByVal Files As IEnumerable(Of FileDB), ByVal Paths As IEnumerable(Of String), Optional ByVal Mask As String = "*")
            Dim lFiles As New List(Of FileDB)
            Dim lPaths As New List(Of String)

            For Each Pair In Files.Zip(Paths, Function(f, p) New With {.File = f, .Path = p})
                Dim File = Pair.File
                Dim Path = Pair.Path

                Select Case File.Type
                    Case FileDB.FileType.File
                        If IsMatchFileMask(File.Name, Mask) Then
                            lFiles.Add(File)
                            lPaths.Add(Path)
                        End If
                    Case FileDB.FileType.Directory
                        Dim DirectoryPath = File.Path
                        For Each p In GetFilesAndPaths(File, DirectoryPath, False, Mask)
                            lFiles.Add(p.Key)
                            lPaths.Add(p.Value)
                        Next
                    Case Else
                        Throw New InvalidOperationException
                End Select
            Next

            If lFiles.Count = 0 Then Return
            If lFiles.Count = 1 Then
                ExtractSingle(lFiles(0), lPaths(0))
                Return
            End If

            ExtractMultiple(lFiles.ToArray, lPaths.ToArray)
        End Sub

        ''' <summary>替换包中的一个文件。</summary>
        Protected MustOverride Sub ReplaceSingleInner(ByVal File As FileDB, ByVal sp As NewReadingStreamPasser)
        ''' <summary>替换包中的一个文件。应优先考虑覆盖ReplaceSingleInner。默认实现调用ReplaceSingleInner。</summary>
        Protected Overridable Sub ReplaceSingleInnerPath(ByVal File As FileDB, ByVal Path As String)
            Using t = StreamEx.CreateReadable(Path, FileMode.Open)
                ReplaceSingleInner(File, t.AsNewReading)
            End Using
        End Sub
        ''' <summary>替换包中的多个文件。应优先考虑覆盖ReplaceSingleInner。默认实现调用ExtractSingleInner。</summary>
        Protected Overridable Sub ReplaceMultipleInner(ByVal Files As FileDB(), ByVal StreamPassers As NewReadingStreamPasser())
            For n As Integer = 0 To Files.Length - 1
                ReplaceSingleInner(Files(n), StreamPassers(n))
            Next
        End Sub
        ''' <summary>替换包中的多个文件。应优先考虑覆盖ExtractMultipleInner。默认实现调用ExtractMultipleInner。</summary>
        Protected Overridable Sub ReplaceMultipleInnerPath(ByVal Files As FileDB(), ByVal Paths As String())
            Dim Streams As New List(Of IReadableSeekableStream)
            Dim StreamPassers As New List(Of NewReadingStreamPasser)
            Try
                For n As Integer = 0 To Files.Length - 1
                    Dim s = StreamEx.CreateReadable(Paths(n), FileMode.Open)
                    Streams.Add(s)
                    StreamPassers.Add(s.AsNewReading)
                Next
                ReplaceMultipleInner(Files, StreamPassers.ToArray)
            Finally
                For Each s In Streams
                    s.Dispose()
                Next
            End Try
        End Sub

        ''' <summary>已重载。替换包中的一个文件。调用ReplaceSingleInner。</summary>
        Public Sub ReplaceSingle(ByVal File As FileDB, ByVal sp As NewReadingStreamPasser)
            If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            ReplaceSingleInner(File, sp)
        End Sub
        ''' <summary>已重载。替换包中的一个文件。调用ReplaceSingleInnerPath。</summary>
        Public Sub ReplaceSingle(ByVal File As FileDB, ByVal Path As String)
            If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            ReplaceSingleInnerPath(File, Path)
        End Sub
        ''' <summary>已重载。替换包中的多个文件。调用ReplaceMultipleInner。</summary>
        Public Sub ReplaceMultiple(ByVal Files As IEnumerable(Of FileDB), ByVal StreamPassers As IEnumerable(Of NewReadingStreamPasser))
            If Files.Count <> StreamPassers.Count Then Throw New ArgumentException
            For Each File In Files
                If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            Next
            ReplaceMultipleInner(Files.ToArray, StreamPassers.ToArray)
        End Sub
        ''' <summary>已重载。替换包中的多个文件。调用ReplaceMultipleInnerPath。</summary>
        Public Sub ReplaceMultiple(ByVal Files As IEnumerable(Of FileDB), ByVal Paths As IEnumerable(Of String))
            If Files.Count <> Paths.Count Then Throw New ArgumentException
            For Each File In Files
                If File.Type <> FileDB.FileType.File Then Throw New ArgumentException
            Next
            ReplaceMultipleInnerPath(Files.ToArray, Paths.ToArray)
        End Sub
        ''' <summary>已重载。替换包中的一个文件或文件夹。调用ReplaceSingle和ReplaceMultiple。</summary>
        Public Sub Replace(ByVal File As FileDB, ByVal Path As String, Optional ByVal Mask As String = "*")
            Replace(New FileDB() {File}, New String() {Path}, Mask)
        End Sub
        ''' <summary>已重载。替换包中的多个文件或文件夹。调用ReplaceSingle和ReplaceMultiple。</summary>
        Public Sub Replace(ByVal Files As IEnumerable(Of FileDB), ByVal Paths As IEnumerable(Of String), Optional ByVal Mask As String = "*")
            Dim lFiles As New List(Of FileDB)
            Dim lPaths As New List(Of String)

            For Each Pair In Files.Zip(Paths, Function(f, p) New With {.File = f, .Path = p})
                Dim File = Pair.File
                Dim Path = Pair.Path

                Select Case File.Type
                    Case FileDB.FileType.File
                        If IsMatchFileMask(File.Name, Mask) AndAlso IO.File.Exists(Path) Then
                            lFiles.Add(File)
                            lPaths.Add(Path)
                        End If
                    Case FileDB.FileType.Directory
                        Dim DirectoryPath = File.Path
                        If IO.Directory.Exists(DirectoryPath) Then
                            For Each p In GetFilesAndPaths(File, DirectoryPath, True, Mask)
                                lFiles.Add(p.Key)
                                lPaths.Add(p.Value)
                            Next
                        End If
                    Case Else
                        Throw New InvalidOperationException
                End Select
            Next

            If lFiles.Count = 0 Then Return
            If lFiles.Count = 1 Then
                ReplaceSingle(lFiles(0), lPaths(0))
                Return
            End If

            ReplaceMultiple(lFiles.ToArray, lPaths.ToArray)
        End Sub

        ''' <summary>关闭包。</summary>
        Public Sub Close()
            Dispose(True)
        End Sub

#Region " IDisposable 支持 "
        ''' <summary>释放托管对象或间接非托管对象(Stream等)。可在这里将大型字段设置为 null。</summary>
        Protected Overridable Sub DisposeManagedResource()
            If Readable IsNot Nothing Then
                Readable.Dispose()
                Readable = Nothing
            End If
            If Writable IsNot Nothing Then
                Writable.Dispose()
                Writable = Nothing
            End If
        End Sub

        ''' <summary>释放直接非托管对象(Handle等)。可在这里将大型字段设置为 null。</summary>
        Protected Overridable Sub DisposeUnmanagedResource()
        End Sub

        '检测冗余的调用
        Private DisposedValue As Boolean = False
        ''' <summary>释放流的资源。请优先覆盖DisposeManagedResource、DisposeUnmanagedResource、DisposeNullify方法。如果你直接保存非托管对象(Handle等)，请覆盖Finalize方法，并在其中调用Dispose(False)。</summary>
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If DisposedValue Then Return
            DisposedValue = True
            If disposing Then
                DisposeManagedResource()
            End If
            DisposeUnmanagedResource()
        End Sub

        ''' <summary>释放流的资源。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>析构。</summary>
        Protected Overrides Sub Finalize()
            Dispose(False)
        End Sub
#End Region

    End Class

    ''' <summary>文件信息</summary>
    Public Class FileDB
        Protected NameValue As String
        Protected TypeValue As FileType
        Protected LengthValue As Int64
        Protected AddressValue As Int64
        Protected TitleNameValue As String

        ''' <summary>文件名</summary>
        Public Overridable Property Name() As String
            Get
                Return NameValue
            End Get
            Set(ByVal Value As String)
                NameValue = Value
            End Set
        End Property
        ''' <summary>文件类型：文件 文件夹</summary>
        Public Overridable Property Type() As FileType
            Get
                Return TypeValue
            End Get
            Set(ByVal Value As FileType)
                TypeValue = Value
            End Set
        End Property
        ''' <summary>文件长度</summary>
        Public Overridable Property Length() As Int64
            Get
                Return LengthValue
            End Get
            Set(ByVal Value As Int64)
                LengthValue = Value
            End Set
        End Property
        ''' <summary>文件地址</summary>
        Public Overridable Property Address() As Int64
            Get
                Return AddressValue
            End Get
            Set(ByVal Value As Int64)
                AddressValue = Value
            End Set
        End Property
        ''' <summary>显示用文件名，如果为空会返回Name</summary>
        Public Overridable Property TitleName() As String
            Get
                If TitleNameValue = "" Then Return Name
                Return TitleNameValue
            End Get
            Set(ByVal Value As String)
                TitleNameValue = Value
            End Set
        End Property

        Public ParentFileDB As FileDB
        Public SubFile As New List(Of FileDB)
        Public SubFileNameRef As New Dictionary(Of String, FileDB)(StringComparer.CurrentCultureIgnoreCase)
        Public Sub New(ByVal Name As String, ByVal Type As FileType, ByVal Length As Int64, ByVal Address As Int64, Optional ByVal TitleName As String = "")
            Me.Name = Name
            Me.Type = Type
            Me.Length = Length
            Me.Address = Address
            Me.TitleName = TitleName
        End Sub
        Public Sub New()
            Me.Type = FileType.File
        End Sub
        Public ReadOnly Property Path() As String
            Get
                Dim Ancestor As FileDB = ParentFileDB
                Dim ret As String = Name
                While Ancestor IsNot Nothing
                    ret = GetPath(Ancestor.Name, ret)
                    Ancestor = Ancestor.ParentFileDB
                End While
                Return ret
            End Get
        End Property
        Public Enum FileType As Integer
            File = 0
            Directory = 1
        End Enum
        Public Shared Function CreateFile(ByVal Name As String, ByVal Length As Int64, ByVal Address As Int64, Optional ByVal TitleName As String = "") As FileDB
            Return New FileDB(Name, FileType.File, Length, Address, TitleName)
        End Function
        Public Shared Function CreateDirectory(ByVal Name As String, Optional ByVal TitleName As String = "") As FileDB
            Return New FileDB(Name, FileType.Directory, 0, -1, TitleName)
        End Function
    End Class

    ''' <summary>文件信息开始地址比较器</summary>
    Public Class FileDBAddressComparer
        Implements IComparer(Of FileDB)

        Private IndexOfFile As IDictionary(Of FileDB, Integer)

        Public Sub New(ByVal IndexOfFile As IDictionary(Of FileDB, Integer))
            Me.IndexOfFile = IndexOfFile
        End Sub
        Public Function Compare(ByVal x As FileDB, ByVal y As FileDB) As Integer Implements IComparer(Of FileDB).Compare
            If x.Address < y.Address Then Return -1
            If x.Address > y.Address Then Return 1
            If IndexOfFile.ContainsKey(x) AndAlso IndexOfFile.ContainsKey(y) Then Return IndexOfFile(y) - IndexOfFile(x)
            If x.Length < y.Length Then Return -1
            If x.Length > y.Length Then Return 1
            Return String.Compare(x.Name, y.Name, StringComparison.Ordinal)
        End Function
    End Class

    ''' <summary>
    ''' 注册文件包类型
    ''' 至少应该包含打开
    ''' </summary>
    Public NotInheritable Class PackageRegister
        Private Sub New()
        End Sub

        ''' <summary>打开文件包。</summary>
        Public Delegate Function PackageOpenRead(ByVal sp As NewReadingStreamPasser) As PackageBase
        Public Delegate Function PackageOpenReadWrite(ByVal sp As NewReadingWritingStreamPasser) As PackageBase
        Public Delegate Function PackageOpenWithPath(ByVal Path As String) As PackageBase
        ''' <summary>创建新的文件包。</summary>
        Public Delegate Function PackageCreate(ByVal sp As NewWritingStreamPasser, ByVal Directory As String) As PackageBase
        Public Delegate Function PackageCreateWithPath(ByVal Path As String, ByVal Directory As String) As PackageBase

        Private Class PackageInfo
            Public Filter As String
            Public OpenRead As PackageOpenRead
            Public OpenReadWrite As PackageOpenReadWrite
            Public OpenWithPath As PackageOpenWithPath
            Public Create As PackageCreate
            Public CreateWithPath As PackageCreateWithPath
        End Class

        Private Shared Packages As New Dictionary(Of String, PackageInfo)
        Private Shared Openables As New List(Of PackageInfo)
        Private Shared Creatables As New List(Of PackageInfo)

        ''' <summary>
        ''' 注册一个包类型
        ''' </summary>
        ''' <param name="Filter">
        ''' 文件包文件名筛选器，应按照“Package File(*.Package)|*.Package”的格式书写。
        ''' </param>
        Public Shared Sub Register(ByVal Filter As String, ByVal OpenRead As PackageOpenRead, ByVal OpenReadWrite As PackageOpenReadWrite, Optional ByVal Create As PackageCreate = Nothing)
            Dim p As PackageInfo
            If Not Packages.ContainsKey(Filter) Then
                If OpenRead Is Nothing AndAlso OpenReadWrite Is Nothing Then Throw New ArgumentNullException("OpenFunctionNull")
                p = New PackageInfo With {.Filter = Filter, .OpenRead = OpenRead, .OpenReadWrite = OpenReadWrite, .Create = Create}
                Packages.Add(Filter, p)
            Else
                p = Packages(Filter)
                If OpenRead Is Nothing AndAlso OpenReadWrite Is Nothing AndAlso p.OpenRead Is Nothing AndAlso p.OpenReadWrite Is Nothing AndAlso p.OpenWithPath Is Nothing Then Throw New ArgumentNullException("OpenFunctionNull")
                p.OpenRead = OpenRead
                p.OpenReadWrite = OpenReadWrite
                If Create IsNot Nothing Then p.Create = Create
            End If
            If Not Openables.Contains(p) Then Openables.Add(p)
            If Create IsNot Nothing AndAlso Not Creatables.Contains(p) Then Creatables.Add(p)
        End Sub
        Public Shared Sub Register(ByVal Filter As String, ByVal Open As PackageOpenWithPath, Optional ByVal Create As PackageCreateWithPath = Nothing)
            Dim p As PackageInfo
            If Not Packages.ContainsKey(Filter) Then
                If Open Is Nothing Then Throw New ArgumentNullException("OpenFunctionNull")
                p = New PackageInfo With {.Filter = Filter, .OpenWithPath = Open, .CreateWithPath = Create}
                Packages.Add(Filter, p)
            Else
                p = Packages(Filter)
                If Open Is Nothing AndAlso p.OpenRead Is Nothing AndAlso p.OpenReadWrite Is Nothing AndAlso p.OpenWithPath Is Nothing Then Throw New ArgumentNullException("OpenFunctionNull")
                p.OpenWithPath = Open
                If Create IsNot Nothing Then p.CreateWithPath = Create
            End If
            If Not Openables.Contains(p) Then Openables.Add(p)
            If Create IsNot Nothing AndAlso Not Creatables.Contains(p) Then Creatables.Add(p)
        End Sub
        Public Shared Sub Unregister(ByVal Filter As String)
            Dim p As PackageInfo = Packages(Filter)
            Packages.Remove(Filter)
            Openables.Remove(p)
            If Creatables.Contains(p) Then Creatables.Remove(p)
        End Sub
        Public Shared Function Open(ByVal Index As Integer, ByVal sp As NewReadingStreamPasser) As PackageBase
            If Openables(Index).OpenRead IsNot Nothing Then
                Return Openables(Index).OpenRead(sp)
            Else
                Throw New InvalidOperationException("NoOpenReadWithStreamFunction")
            End If
        End Function
        Public Shared Function Open(ByVal Index As Integer, ByVal sp As NewReadingWritingStreamPasser) As PackageBase
            If Openables(Index).OpenRead IsNot Nothing Then
                Return Openables(Index).OpenReadWrite(sp)
            Else
                Throw New InvalidOperationException("NoOpenReadWriteWithStreamFunction")
            End If
        End Function
        Public Shared Function Open(ByVal Index As Integer, ByVal Path As String) As PackageBase
            If Openables(Index).OpenWithPath Is Nothing Then
                If CBool(IO.File.GetAttributes(Path) And IO.FileAttributes.ReadOnly) Then
                    Return Open(Index, StreamEx.CreateReadable(Path, FileMode.Open).AsNewReading)
                Else
                    Return Open(Index, StreamEx.Create(Path, FileMode.Open).AsNewReadingWriting)
                End If
            Else
                Return Openables(Index).OpenWithPath(Path)
            End If
        End Function
        Public Shared Function Create(ByVal WritableIndex As Integer, ByVal sp As NewWritingStreamPasser, ByVal Directory As String) As PackageBase
            If Creatables(WritableIndex).Create IsNot Nothing Then
                Return Creatables(WritableIndex).Create(sp, Directory)
            Else
                Throw New InvalidOperationException("NoCreateWithStreamFunction")
            End If
        End Function
        Public Shared Function Create(ByVal WritableIndex As Integer, ByVal Path As String, ByVal Directory As String) As PackageBase
            If Creatables(WritableIndex).OpenWithPath Is Nothing Then
                Return Create(WritableIndex, StreamEx.Create(Path, FileMode.Create).AsNewWriting, Directory)
            Else
                Return Creatables(WritableIndex).CreateWithPath(Path, Directory)
            End If
        End Function
        Public Shared Function GetFilter() As String
            Dim sb As New System.Text.StringBuilder
            For Each p In Openables
                sb.Append(p.Filter)
                sb.Append("|")
            Next
            Return sb.ToString.TrimEnd("|"c)
        End Function
        Public Shared Function GetWritableFilter() As String
            Dim sb As New System.Text.StringBuilder
            For Each p In Creatables
                sb.Append(p.Filter)
                sb.Append("|")
            Next
            Return sb.ToString.TrimEnd("|"c)
        End Function
        Public Shared ReadOnly Property PackageTypeCount() As Integer
            Get
                Return Packages.Count
            End Get
        End Property
    End Class
End Namespace
