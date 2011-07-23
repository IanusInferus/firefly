'==========================================================================
'
'  File:        ISO.vb
'  Location:    Firefly.Packaging <Visual Basic .Net>
'  Description: ISO类
'  Version:     2011.07.24.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Diagnostics
Imports Firefly.TextEncoding
Imports Firefly.Streaming

Namespace Packaging
    Public Class ISO
        Inherits PackageDiscrete

        Protected LogicalBlockSize As Integer = &H800
        Protected PrimaryDescriptor As IsoPrimaryDescriptor

        Protected PhysicalAdressAddressOfFile As New Dictionary(Of FileDB, Int64)
        Protected PhysicalLengthAddressOfFile As New Dictionary(Of FileDB, Int64)

        Protected DataScanStart As Int64 = 0

        Public Sub New(ByVal sp As NewReadingStreamPasser)
            MyBase.New(sp)
            Initializer()
        End Sub
        Public Sub New(ByVal sp As NewReadingWritingStreamPasser)
            MyBase.New(sp)
            Initializer()
        End Sub

        Private Sub Initializer()
            For n As Integer = 16 To Integer.MaxValue
                Readable.Position = &H800 * n
                Dim Type As Byte = Readable.ReadByte
                Readable.Position -= 1
                Select Case Type
                    Case 1
                        If PrimaryDescriptor IsNot Nothing Then Throw New InvalidDataException
                        PrimaryDescriptor = New IsoPrimaryDescriptor(Readable)
                    Case 255
                        Exit For
                End Select
            Next
            If PrimaryDescriptor Is Nothing Then Throw New InvalidDataException
            If PrimaryDescriptor.VolumeSetSize <> 1 Then Throw New InvalidDataException
            LogicalBlockSize = PrimaryDescriptor.LogicalBlockSize
            DataScanStart = Readable.Position
            RootValue = ToFileDB(PrimaryDescriptor.RootDirectoryRecord, LogicalBlockSize)
            DataScanStart = GetSpace(DataScanStart)
            ScanHoles(DataScanStart)
        End Sub

        Public Shared ReadOnly Property Filter() As String
            Get
                Return "ISO(*.ISO)|*.ISO"
            End Get
        End Property

        Public Shared Function OpenRead(ByVal sp As NewReadingStreamPasser) As PackageBase
            Return New ISO(sp)
        End Function
        Public Shared Function OpenReadWrite(ByVal sp As NewReadingWritingStreamPasser) As PackageBase
            Return New ISO(sp)
        End Function

        Public Shared Function Open(ByVal Path As String) As PackageBase
            Dim s As IStream = Nothing
            Dim sRead As IReadableSeekableStream = Nothing
            Try
                s = Streams.OpenResizable(Path)
            Catch
                sRead = Streams.OpenReadable(Path)
            End Try
            If s IsNot Nothing Then
                Return New ISO(s.AsNewReadingWriting)
            Else
                Return New ISO(sRead.AsNewReading)
            End If
        End Function

        Public Overrides Property FileAddressInPhysicalFileDB(ByVal File As FileDB) As Int64
            Get
                Readable.Position = PhysicalAdressAddressOfFile(File)
                Return CLng(Readable.ReadUInt32) * CLng(LogicalBlockSize)
            End Get
            Set(ByVal Value As Int64)
                Writable.Position = PhysicalAdressAddressOfFile(File)
                Dim ExtentLocation = CUInt(Value \ LogicalBlockSize)
                Writable.WriteUInt32(ExtentLocation)
                Writable.WriteUInt32B(ExtentLocation)
            End Set
        End Property

        Public Overrides Property FileLengthInPhysicalFileDB(ByVal File As FileDB) As Int64
            Get
                Readable.Position = PhysicalLengthAddressOfFile(File)
                Return Readable.ReadUInt32
            End Get
            Set(ByVal Value As Int64)
                Writable.Position = PhysicalLengthAddressOfFile(File)
                Dim DataLength = CUInt(Value)
                Writable.WriteUInt32(DataLength)
                Writable.WriteUInt32B(DataLength)
            End Set
        End Property

        Protected NotOverridable Overrides Function GetSpace(ByVal Length As Int64) As Int64
            Return ((Length + LogicalBlockSize - 1) \ LogicalBlockSize) * LogicalBlockSize
        End Function

        Protected Function ToFileDB(ByVal dr As IsoDirectoryRecord, ByVal LogicalBlockSize As Integer) As FileDB
            Dim s = Readable
            Dim CurrentPosition As Int64 = s.Position
            Dim FileName As String = dr.FileId
            '处理含有revision号的文件名
            If dr.FileId.ToString.IndexOf(";") >= 0 Then FileName = dr.FileId.ToString.Substring(0, dr.FileId.ToString.IndexOf(";"))
            Dim ret As New FileDB(FileName, CType((dr.FileFlags And 2) >> 1, FileDB.FileType), dr.DataLength, CLng(dr.ExtentLocation) * CLng(LogicalBlockSize))

            PhysicalAdressAddressOfFile.Add(ret, dr.PhysicalAdressAddress)
            PhysicalLengthAddressOfFile.Add(ret, dr.PhysicalLengthAddress)

            ret.ParentFileDB = Nothing
            If dr.IsDirectory() Then
                s.Position = dr.ExtentLocation * LogicalBlockSize
                Dim CurrentDirectoryLength As Byte = s.ReadByte
                s.Position += CurrentDirectoryLength - 1
                Dim ParentDirectoryLength As Byte = s.ReadByte
                s.Position += ParentDirectoryLength - 1

                Dim Length As Byte = s.PeekByte
                While Length <> 0
                    Dim r As New IsoDirectoryRecord(s)
                    Dim f As FileDB = ToFileDB(r, LogicalBlockSize)
                    If f.Type = FileDB.FileType.File Then
                        PushFile(f, ret)
                    ElseIf f.Type = FileDB.FileType.Directory Then
                        PushFileToDir(f, ret)
                    End If
                    Length = s.PeekByte
                End While
                DataScanStart = Max(s.Position, DataScanStart)
                DataScanStart = Max(ret.Address + ret.Length, DataScanStart)
            End If
            s.Position = CurrentPosition
            Return ret
        End Function

        Protected Overrides Sub ReplaceMultipleInner(Files() As FileDB, StreamPassers() As Streaming.NewReadingStreamPasser)
            MyBase.ReplaceMultipleInner(Files, StreamPassers)
            UpdateVolumeSpaceSize()
        End Sub
        Protected Overrides Sub ReplaceSingleInner(File As FileDB, sp As Streaming.NewReadingStreamPasser)
            MyBase.ReplaceSingleInner(File, sp)
            UpdateVolumeSpaceSize()
        End Sub

        Private Sub UpdateVolumeSpaceSize()
            Dim NewVolumeSpaceSize = CUInt(Writable.Length \ LogicalBlockSize)
            If PrimaryDescriptor.VolumeSpaceSize <> NewVolumeSpaceSize Then
                Writable.Position = PrimaryDescriptor.VolumeSpaceSizeAddress
                Writable.WriteUInt32(NewVolumeSpaceSize)
                Writable.WriteUInt32B(NewVolumeSpaceSize)
                PrimaryDescriptor.VolumeSpaceSize = NewVolumeSpaceSize
            End If
        End Sub
    End Class

    Public Class IsoPrimaryDescriptor
        Public Type As Byte
        Public Id As IsoAnsiString
        Public Version As Byte

        Public SystemId As IsoAnsiString
        Public VolumeId As IsoAnsiString

        Public VolumeSpaceSize As UInt32

        Public VolumeSetSize As UInt16
        Public VolumeSequenceNumber As UInt16
        Public LogicalBlockSize As UInt16
        Public PathTableSize As UInt32
        Public TypeLPathTable As UInt16
        Public OptTypeLPathTable As UInt16
        Public TypeMPathTable As UInt16
        Public OptTypeMPathTable As UInt16
        Public RootDirectoryRecord As IsoDirectoryRecord
        Public VolumeSetId As IsoAnsiString
        Public PublisherId As IsoAnsiString
        Public PreparerId As IsoAnsiString
        Public ApplicationId As IsoAnsiString
        Public CopyrightFileId As IsoAnsiString
        Public AbstractFileId As IsoAnsiString
        Public BibliographicFileId As IsoAnsiString
        Public CreationDate As IsoAnsiString
        Public ModificationDate As IsoAnsiString
        Public ExptrationDate As IsoAnsiString
        Public EffectiveDate As IsoAnsiString
        Public FileStructureVersion As Byte

        Public ApplicationData As IsoAnsiString


        Public VolumeSpaceSizeAddress As Int64

        Public Sub New(ByVal s As IReadableSeekableStream)
            Type = s.ReadByte
            Id = s.Read(5)
            Version = s.ReadByte
            s.Position += 1
            SystemId = s.Read(32)
            VolumeId = s.Read(32)
            s.Position += 8
            VolumeSpaceSizeAddress = s.Position
            VolumeSpaceSize = s.ReadUInt32 : s.ReadUInt32B()
            s.Position += 32
            VolumeSetSize = s.ReadUInt16 : s.ReadUInt16B()
            VolumeSequenceNumber = s.ReadUInt16 : s.ReadUInt16B()
            LogicalBlockSize = s.ReadUInt16 : s.ReadUInt16B()
            PathTableSize = s.ReadUInt32 : s.ReadUInt32B()
            TypeLPathTable = s.ReadUInt16 : s.ReadUInt16B()
            OptTypeLPathTable = s.ReadUInt16 : s.ReadUInt16B()
            TypeMPathTable = s.ReadUInt16 : s.ReadUInt16B()
            OptTypeMPathTable = s.ReadUInt16 : s.ReadUInt16B()
            RootDirectoryRecord = New IsoDirectoryRecord(s)
            VolumeSetId = s.Read(128)
            PublisherId = s.Read(128)
            PreparerId = s.Read(128)
            ApplicationId = s.Read(128)
            CopyrightFileId = s.Read(37)
            AbstractFileId = s.Read(37)
            BibliographicFileId = s.Read(37)
            CreationDate = s.Read(17)
            ModificationDate = s.Read(17)
            ExptrationDate = s.Read(17)
            EffectiveDate = s.Read(17)
            FileStructureVersion = s.ReadByte
            s.Position += 1
            ApplicationData = s.Read(512)
            s.Position += 653
        End Sub
    End Class

    Public Class IsoDirectoryRecord
        Public Length As Byte
        Public ExtAttrLength As Byte
        Public ExtentLocation As UInt32
        Public DataLength As UInt32
        Public RecordingDateAndTime As IsoAnsiString
        Public FileFlags As Byte
        Public FileUnitSize As Byte
        Public InterleaveGapSize As Byte
        Public VolumeSequenceNumber As UInt16
        Public FileIdLen As Byte
        Public FileId As IsoAnsiString

        Public PhysicalAdressAddress As Int64
        Public PhysicalLengthAddress As Int64
        Public Sub New(ByVal s As IReadableSeekableStream)
            Dim CurrentPosition As Int64 = s.Position
            Length = s.ReadByte
            ExtAttrLength = s.ReadByte

            PhysicalAdressAddress = s.Position
            ExtentLocation = s.ReadUInt32 : s.ReadUInt32B()

            PhysicalLengthAddress = s.Position
            DataLength = s.ReadUInt32 : s.ReadUInt32B()

            RecordingDateAndTime = s.Read(7)
            FileFlags = s.ReadByte
            FileUnitSize = s.ReadByte
            InterleaveGapSize = s.ReadByte
            VolumeSequenceNumber = s.ReadUInt16 : s.ReadUInt16B()
            FileIdLen = s.ReadByte
            FileId = s.Read(FileIdLen)
            If (FileIdLen And 1) = 0 Then s.Position += 1
            s.Position = CurrentPosition + Length
        End Sub
        Public Function IsDirectory() As Boolean
            Return CBool(FileFlags And 2)
        End Function
    End Class

    <DebuggerDisplay("{ToString()}")> _
    Public Class IsoAnsiString
        Public Data As Byte()

        Public Sub New(ByVal Data As Byte())
            Me.Data = Data
        End Sub

        Public Overrides Function ToString() As String
            Dim d As New List(Of Byte)
            For Each b In Data
                If b = 0 Then Exit For
                d.Add(b)
            Next
            Return CStr(TextEncoding.Default.GetChars(d.ToArray)).TrimEnd(" "c)
        End Function

        Public Shared Widening Operator CType(ByVal s As IsoAnsiString) As String
            Return s.ToString()
        End Operator

        Public Shared Widening Operator CType(ByVal b As Byte()) As IsoAnsiString
            Return New IsoAnsiString(b)
        End Operator

        Public Shared Widening Operator CType(ByVal s As String) As IsoAnsiString
            Return New IsoAnsiString(TextEncoding.Default.GetBytes(s))
        End Operator
    End Class
End Namespace
