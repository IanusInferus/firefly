'==========================================================================
'
'  File:        PCK.vb
'  Location:    Firefly.Packaging <Visual Basic .Net>
'  Description: PCK文件流类(一个标准的文件包)
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Compare Text
Imports System
Imports System.Math
Imports System.IO
Imports System.Collections.Generic
Imports Firefly.Streaming

Namespace Packaging
    ''' <summary>PCK文件流类</summary>
    ''' <remarks>
    ''' 用于打开和创建盟军2的PCK文件
    ''' </remarks>
    Public Class PCK
        Inherits PackageFixedAddress
        Public Sub New(ByVal sp As NewReadingStreamPasser)
            MyBase.New(sp)
            Initializer()
        End Sub
        Public Sub New(ByVal sp As NewReadingWritingStreamPasser)
            MyBase.New(sp)
            Initializer()
        End Sub

        Private Sub Initializer()
            Readable.Position = 0
            RootValue = OpenFileDB()
        End Sub

        Protected Function OpenFileDB() As FileDB
            Dim ret As New FileDB
            PhysicalPosition.Add(ret, Readable.Position)
            With ret
                Dim s = Readable
                .Name = s.ReadSimpleString(36)
                .Type = s.ReadInt32
                .Length = s.ReadInt32
                .Address = s.ReadInt32
                If .Type = FileDB.FileType.Directory Then
                    s.Position = .Address
                    While True
                        Dim f = OpenFileDB()
                        If f.Type = 255 Then Exit While
                        If f.Type = 1 Then
                            f.ParentFileDB = ret
                            .SubFile.Add(f)
                            .SubFileNameRef.Add(f.Name, f)
                        Else
                            PushFile(f, ret)
                        End If
                    End While
                End If
            End With
            Return ret
        End Function

        Protected Const DBLength As Integer = 48
        Public Sub WriteFileDB(ByVal File As FileDB)
            PhysicalPosition.Add(File, Writable.Position)
            With File
                Dim s = Writable
                s.WriteSimpleString(.Name, 36)
                s.WriteInt32(.Type)
                s.WriteInt32(.Length)
                s.WriteInt32(.Address)
            End With
        End Sub

        Public Sub New(ByVal sp As NewWritingStreamPasser, ByVal Directory As String)
            Dim s = sp.GetStream
            Readable = s
            Writable = s

            Dim FileQueue As New Queue(Of FileDB)
            Dim FileLengthAddressPointerQueue As New Queue(Of Integer)
            Dim FilePathQueue As New Queue(Of String)
            Dim FileLengthQueue As New Queue(Of Integer)
            Dim FileAddressQueue As New Queue(Of Integer)

            Dim RootName As String = GetFileName(Directory)
            If RootName.Length > 36 Then Throw New InvalidDataException(Directory)

            s.SetLength(16777216)
            Dim cFileDB As FileDB = CreateDirectory(RootName, DBLength)
            RootValue = cFileDB
            WriteFileDB(cFileDB)
            ImportDirectory(GetFileDirectory(Directory), cFileDB, FileQueue, FileLengthAddressPointerQueue, FilePathQueue)
            WriteFileDB(CreateDirectoryEnd())

            For Each f As String In FilePathQueue
                Using File = StreamEx.CreateReadable(f, FileMode.Open)
                    GotoNextFilePoint()
                    FileLengthQueue.Enqueue(File.Length)
                    FileAddressQueue.Enqueue(s.Position)
                    If s.Length - s.Position < File.Length Then
                        s.SetLength(s.Length + Max(16777216, Ceiling(File.Length / 16777216) * 16777216))
                    End If
                    s.WriteFromStream(File, File.Length)
                End Using
            Next
            GotoNextFilePoint()
            s.SetLength(s.Position)

            Dim fn As FileDB
            Dim pl As Integer
            Dim pa As Integer
            For Each p As Integer In FileLengthAddressPointerQueue
                s.Position = p
                fn = FileQueue.Dequeue
                pl = FileLengthQueue.Dequeue
                pa = FileAddressQueue.Dequeue
                fn.Length = pl
                fn.Address = pa
                s.WriteInt32(pl)
                s.WriteInt32(pa)
            Next
            s.Position = 0
        End Sub
        Private Sub ImportDirectory(ByVal Dir As String, ByVal DirDB As FileDB, ByVal FileQueue As Queue(Of FileDB), ByVal FileLengthAddressPointerQueue As Queue(Of Integer), ByVal FilePathQueue As Queue(Of String))
            Dim s = Writable
            Dim cFileDB As FileDB
            Dim Name As String
            For Each f As String In Directory.GetFiles(GetPath(Dir, DirDB.Name))
                Name = GetFileName(f)
                If Name.Length > 36 Then Throw New InvalidDataException(f)
                cFileDB = FileDB.CreateFile(Name, -1, -1)
                WriteFileDB(cFileDB)
                cFileDB.ParentFileDB = DirDB
                DirDB.SubFile.Add(cFileDB)
                DirDB.SubFileNameRef.Add(cFileDB.Name, cFileDB)
                FileQueue.Enqueue(cFileDB)
                FileLengthAddressPointerQueue.Enqueue(s.Position - 8)
                FilePathQueue.Enqueue(f)
            Next
            For Each d As String In Directory.GetDirectories(GetPath(Dir, DirDB.Name))
                Name = GetFileName(d)
                If Name.Length > 36 Then Throw New InvalidDataException(d)
                cFileDB = CreateDirectory(Name, s.Position + DBLength)
                WriteFileDB(cFileDB)
                cFileDB.ParentFileDB = DirDB
                DirDB.SubFile.Add(cFileDB)
                DirDB.SubFileNameRef.Add(cFileDB.Name, cFileDB)
                ImportDirectory(GetFileDirectory(d), cFileDB, FileQueue, FileLengthAddressPointerQueue, FilePathQueue)
                WriteFileDB(CreateDirectoryEnd())
            Next
        End Sub
        Sub GotoNextFilePoint()
            Dim NewPosition = (Writable.Position \ &H800 + 1) * &H800
            While Writable.Position < NewPosition
                Writable.WriteByte(0)
            End While
        End Sub
        Public Shared Function CreateDirectory(ByVal Name As String, ByVal Address As Int32) As FileDB
            Return New FileDB(Name, 1, &HFFFFFFFF, Address)
        End Function
        Public Shared Function CreateDirectoryEnd() As FileDB
            Return New FileDB(Nothing, 255, &HFFFFFFFF, &HFFFFFFFF)
        End Function

        Protected PhysicalPosition As New Dictionary(Of FileDB, Int32)
        Public Overrides Property FileLengthInPhysicalFileDB(ByVal File As FileDB) As Int64
            Get
                Readable.Position = PhysicalPosition(File) + 40
                Return Readable.ReadInt32
            End Get
            Set(ByVal Value As Int64)
                Writable.Position = PhysicalPosition(File) + 40
                Writable.WriteInt32(Value)
            End Set
        End Property

        Public Shared ReadOnly Property Filter() As String
            Get
                Return "PCK(*.PCK)|*.PCK"
            End Get
        End Property
        Public Shared Function OpenRead(ByVal sp As NewReadingStreamPasser) As PackageBase
            Return New PCK(sp)
        End Function
        Public Shared Function OpenReadWrite(ByVal sp As NewReadingWritingStreamPasser) As PackageBase
            Return New PCK(sp)
        End Function
        Public Shared Function Create(ByVal sp As NewWritingStreamPasser, ByVal Directory As String) As PackageBase
            Return New PCK(sp, Directory)
        End Function
    End Class
End Namespace
