'==========================================================================
'
'  File:        GIM.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: PSP GIM图像格式
'  Version:     2010.12.01.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports Firefly
Imports Firefly.Streaming

Public Class GIM
    Public Const Identifier As String = "MIG.00.1PSP"

    Public Shared Function GetSpace(ByVal Length As Int64) As Int64
        Return ((Length + 3) \ 4) * 4
    End Function
    Public Shared Function GetSpace2(ByVal Length As Int64) As Int64
        Return ((Length + 15) \ 16) * 16
    End Function

    Protected Root As RootBlock
    Public ReadOnly Property Images() As IEnumerable(Of ImageBlock)
        Get
            Dim Img As New List(Of ImageBlock)
            For Each i In Root.SubBlocks
                If TypeOf i Is ImageBlock Then
                    Img.Add(i)
                End If
            Next
            Return Img
        End Get
    End Property

    Public Sub New(ByVal Data As Byte())
        Using s = StreamEx.Create
            s.Write(Data)
            s.Position = 0

            If s.ReadSimpleString(16) <> Identifier Then Throw New InvalidDataException
            Root = Block.Read(s)
        End Using
    End Sub

    Public Function ToBytes() As Byte()
        Using s = StreamEx.Create
            s.WriteSimpleString(Identifier, 16)
            Root.Write(s)

            s.Position = 0
            Return s.Read(s.Length)
        End Using
    End Function

    Public Enum BlockTypes As UInt16
        Root = 2
        Image = 3
        Bitmap = 4
        Palette = 5
        Comment = 255
    End Enum

    Public Enum BitmapType As UInt16
        B5G6R5 = 0
        A1B5G5R5 = 1
        A4B4G4R4 = 2
        A8B8G8R8 = 3
        Index4 = 4
        Index8 = 5
        Index16 = 6
        Index32 = 7
        DXT1 = 8
        DXT3 = 9
        DXT5 = 10
        DXT1Ext = 264
        DXT3Ext = 265
        DXT5Ext = 266
    End Enum

    Public Enum PaletteTypes As UInt16
        B5G6R5 = 0
        A1B5G5R5 = 1
        A4B4G4R4 = 2
        A8B8G8R8 = 3
    End Enum

    Public Class Block
        Protected BlockTypeValue As BlockTypes
        Protected Unknown As UInt16
        Public ReadOnly Property BlockType() As BlockTypes
            Get
                Return BlockTypeValue
            End Get
        End Property
        Protected Data As Byte()
        Protected SubBlocks As New List(Of Block)

        Public Shared Function Read(ByVal s As IReadableSeekableStream) As Block
            Dim HoldPosition = s.Position
            Dim BlockType As BlockTypes = s.ReadUInt16
            Dim Unknown = s.ReadUInt16
            Dim BlockLength = s.ReadInt32
            Dim ContentLength = s.ReadInt32
            Dim BaseAddressForData = s.ReadInt32
            s.Position = HoldPosition + BaseAddressForData
            Dim b As Block
            Select Case BlockType
                Case BlockTypes.Root
                    b = New RootBlock
                Case BlockTypes.Image
                    b = New ImageBlock
                Case BlockTypes.Bitmap
                    b = New BitmapBlock
                Case BlockTypes.Palette
                    b = New PaletteBlock
                Case BlockTypes.Comment
                    b = New CommentBlock
                Case Else
                    b = New Block
            End Select
            b.BlockTypeValue = BlockType
            b.Unknown = Unknown
            b.Data = s.Read(ContentLength - BaseAddressForData)
            b.ReadData()
            While s.Position < HoldPosition + BlockLength
                b.SubBlocks.Add(Read(s))
            End While
            Return b
        End Function
        Public Overridable Sub ReadData()
        End Sub

        Public Sub Write(ByVal s As IWritableSeekableStream)
            WriteData()
            Dim HoldPosition = s.Position
            s.Position += 16
            If Data IsNot Nothing Then
                s.Write(Data)
                s.Position = GetSpace(s.Position)
            End If
            Dim ContentLength = s.Position - HoldPosition
            For Each sb In SubBlocks
                sb.Write(s)
            Next
            Dim EndPosition = s.Position
            s.Position = HoldPosition
            s.WriteUInt16(BlockType)
            s.WriteUInt16(Unknown)
            s.WriteInt32(EndPosition - HoldPosition)
            s.WriteInt32(ContentLength)
            s.WriteInt32(16)
            s.Position = s.Length
            While s.Position < EndPosition
                s.WriteByte(0)
            End While
        End Sub
        Public Overridable Sub WriteData()
        End Sub
    End Class

    Public Class RootBlock
        Inherits Block

        Public Overrides Sub ReadData()
        End Sub

        Public Overrides Sub WriteData()
        End Sub

        Public Shadows ReadOnly Property SubBlocks() As List(Of Block)
            Get
                Return MyBase.SubBlocks
            End Get
        End Property
    End Class

    Public Class ImageBlock
        Inherits Block

        Public ReadOnly Property Bitmap() As BitmapBlock
            Get
                For Each b In SubBlocks
                    If TypeOf b Is BitmapBlock Then Return b
                Next
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Palette() As PaletteBlock
            Get
                For Each b In SubBlocks
                    If TypeOf b Is PaletteBlock Then Return b
                Next
                Return Nothing
            End Get
        End Property
    End Class

    Public Class BitmapBlock
        Inherits Block

        Public Indices As Integer()
        Public BitmapData As New List(Of Int32(,))

        Public Type As BitmapType
        Public Width As UInt16
        Public Height As UInt16
        Public BitsPerPixel As UInt16
        Public RectangleByteWidth As UInt16
        Public RectangleHeight As UInt16

        Private AddressStart As Int32

        Public ReadOnly Property NumFrame() As Integer
            Get
                If Indices Is Nothing Then Return 0
                Return Indices.Length
            End Get
        End Property

        Public ReadOnly Property NeedPalette() As Boolean
            Get
                Select Case Type
                    Case BitmapType.Index4, BitmapType.Index8, BitmapType.Index16, BitmapType.Index32
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        Public Overrides Sub ReadData()
            Using s = StreamEx.Create()
                s.Write(Me.Data)
                s.Position = 0

                s.Position = 4
                Type = s.ReadUInt16

                s.Position = 8
                Width = s.ReadUInt16
                Height = s.ReadUInt16
                BitsPerPixel = s.ReadUInt16
                RectangleByteWidth = s.ReadUInt16
                RectangleHeight = s.ReadUInt16

                s.Position = &H18
                AddressStart = s.ReadInt32
                Dim BitmapStart = s.ReadInt32
                Dim BitmapEnd = s.ReadInt32

                s.Position = &H2E
                Dim NumFrame = s.ReadUInt16

                Dim RectanglePixelWidth = (RectangleByteWidth * 8) \ BitsPerPixel
                Dim NumWidthBlock = (Width + RectanglePixelWidth - 1) \ RectanglePixelWidth
                Dim NumHeightBlock = (Height + RectangleHeight - 1) \ RectangleHeight
                Dim FrameSize = CInt(RectangleByteWidth) * CInt(RectangleHeight) * CInt(NumWidthBlock) * CInt(NumHeightBlock)

                s.Position = AddressStart
                Indices = New Int32(NumFrame - 1) {}
                For n = 0 To NumFrame - 1
                    Dim BitmapAddress = s.ReadInt32
                    Dim Index = (BitmapAddress - BitmapStart) \ FrameSize
                    If Index * FrameSize <> BitmapAddress - BitmapStart Then
                        Throw New InvalidDataException
                    End If
                    Indices(n) = Index
                Next

                s.Position = BitmapStart
                For n = 0 To (BitmapEnd - BitmapStart) \ FrameSize - 1
                    Dim Rectangle = New Int32(Width - 1, Height - 1) {}
                    Select Case Type
                        Case BitmapType.Index4
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        If i * RectanglePixelWidth + RectanglePixelWidth <= Width Then
                                            For x = i * RectanglePixelWidth To i * RectanglePixelWidth + RectanglePixelWidth - 1 Step 2
                                                Dim b = s.ReadByte()
                                                Rectangle(x, y) = b.Bits(3, 0)
                                                Rectangle(x + 1, y) = b.Bits(7, 4)
                                            Next
                                        Else
                                            For x = i * RectanglePixelWidth To Width - 2 Step 2
                                                Dim b = s.ReadByte()
                                                Rectangle(x, y) = b.Bits(3, 0)
                                                Rectangle(x + 1, y) = b.Bits(7, 4)
                                            Next
                                            If (Width And 1) <> 0 Then
                                                Dim b = s.ReadByte()
                                                Rectangle(Width - 1, y) = b.Bits(3, 0)
                                            End If
                                            For x = ((Width + 1) \ 2) * 2 To i * RectanglePixelWidth + RectanglePixelWidth - 1 Step 2
                                                s.ReadByte()
                                            Next
                                        End If
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case BitmapType.Index8
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Rectangle(x, y) = s.ReadByte()
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.ReadByte()
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case BitmapType.Index16
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Rectangle(x, y) = s.ReadUInt16()
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.ReadUInt16()
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case BitmapType.Index32
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Rectangle(x, y) = s.ReadInt32()
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.ReadInt32()
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case BitmapType.B5G6R5
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ABGR = s.ReadUInt16()
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(B, 5, G, 6, R, 5, ABGR)
                                            B = ConcatBits(B, 5, B >> 2, 3)
                                            G = ConcatBits(G, 6, G >> 4, 2)
                                            R = ConcatBits(R, 5, R >> 2, 3)
                                            Rectangle(x, y) = ConcatBits(&HFF, 8, R, 8, G, 8, B, 8)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.ReadUInt16()
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case BitmapType.A1B5G5R5
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ABGR = s.ReadUInt16()
                                            Dim A As Byte
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(A, 1, B, 5, G, 5, R, 5, ABGR)
                                            A = ConcatBits(A, 1, A, 1)
                                            A = ConcatBits(A, 2, A, 2)
                                            A = ConcatBits(A, 4, A, 4)
                                            B = ConcatBits(B, 5, B >> 2, 3)
                                            G = ConcatBits(G, 5, G >> 2, 3)
                                            R = ConcatBits(R, 5, R >> 2, 3)
                                            Rectangle(x, y) = ConcatBits(A, 8, R, 8, G, 8, B, 8)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.ReadUInt16()
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case BitmapType.A4B4G4R4
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ABGR = s.ReadUInt16()
                                            Dim A As Byte
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(A, 4, B, 4, G, 4, R, 4, ABGR)
                                            A = ConcatBits(A, 4, A, 4)
                                            B = ConcatBits(B, 4, B, 4)
                                            G = ConcatBits(G, 4, G, 4)
                                            R = ConcatBits(R, 4, R, 4)
                                            Rectangle(x, y) = ConcatBits(A, 8, R, 8, G, 8, B, 8)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.ReadUInt16()
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case BitmapType.A8B8G8R8
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ABGR = s.ReadInt32()
                                            Dim A As Byte
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(A, 8, B, 8, G, 8, R, 8, ABGR)
                                            Rectangle(x, y) = ConcatBits(A, 8, R, 8, G, 8, B, 8)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.ReadInt32()
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Read(RectangleByteWidth)
                                    Next
                                Next
                            Next
                        Case Else
                            Throw New NotSupportedException("NotSupportedBitmapType:{0}".Formats(Type))
                    End Select
                    BitmapData.Add(Rectangle)
                Next
            End Using
        End Sub

        Public Overrides Sub WriteData()
            Using s = StreamEx.Create()
                s.Write(Me.Data)
                s.Position = 0

                s.Position = 4
                s.WriteUInt16(Type)

                s.Position = 8
                s.WriteUInt16(Width)
                s.WriteUInt16(Height)
                s.WriteUInt16(BitsPerPixel)
                s.WriteUInt16(RectangleByteWidth)
                s.WriteUInt16(RectangleHeight)

                Dim NumFrame As UInt16 = Indices.Length
                Dim RectanglePixelWidth = (RectangleByteWidth * 8) \ BitsPerPixel
                Dim NumWidthBlock = (Width + RectanglePixelWidth - 1) \ RectanglePixelWidth
                Dim NumHeightBlock = (Height + RectangleHeight - 1) \ RectangleHeight
                Dim FrameSize = CInt(RectangleByteWidth) * CInt(RectangleHeight) * CInt(NumWidthBlock) * CInt(NumHeightBlock)

                Dim BitmapStart = AddressStart + GetSpace2(NumFrame * 4)
                Dim BitmapEnd = BitmapStart + FrameSize * BitmapData.Count

                s.Position = &H18
                s.WriteInt32(AddressStart)
                s.WriteInt32(BitmapStart)
                s.WriteInt32(BitmapEnd)

                s.Position = &H2E
                s.WriteUInt16(NumFrame)

                s.Position = AddressStart
                For n = 0 To NumFrame - 1
                    s.WriteInt32(BitmapStart + FrameSize * Indices(n))
                Next

                s.Position = BitmapStart
                For n = 0 To BitmapData.Count - 1
                    Dim Rectangle = BitmapData(n)
                    Select Case Type
                        Case BitmapType.Index4
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        If i * RectanglePixelWidth + RectanglePixelWidth <= Width Then
                                            For x = i * RectanglePixelWidth To i * RectanglePixelWidth + RectanglePixelWidth - 1 Step 2
                                                s.WriteByte(ConcatBits(Rectangle(x + 1, y), 4, Rectangle(x, y), 4))
                                            Next
                                        Else
                                            For x = i * RectanglePixelWidth To Width - 2 Step 2
                                                s.WriteByte(ConcatBits(Rectangle(x + 1, y), 4, Rectangle(x, y), 4))
                                            Next
                                            If (Width And 1) <> 0 Then
                                                s.WriteByte(ConcatBits(0, 4, Rectangle(Width - 1, y), 4))
                                            End If
                                            For x = ((Width + 1) \ 2) * 2 To i * RectanglePixelWidth + RectanglePixelWidth - 1 Step 2
                                                s.WriteByte(0)
                                            Next
                                        End If
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case BitmapType.Index8
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            s.WriteByte(Rectangle(x, y))
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.WriteByte(0)
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case BitmapType.Index16
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            s.WriteUInt16(Rectangle(x, y))
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.WriteUInt16(0)
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case BitmapType.Index32
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            s.WriteInt32(Rectangle(x, y))
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.WriteInt32(0)
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case BitmapType.B5G6R5
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ARGB = Rectangle(x, y)
                                            Dim A As Byte
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                            Dim ABGR = ConcatBits(B >> 3, 5, G >> 2, 6, R >> 3, 5)
                                            s.WriteUInt16(ABGR)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.WriteUInt16(0)
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case BitmapType.A1B5G5R5
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ARGB = Rectangle(x, y)
                                            Dim A As Byte
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                            Dim ABGR = ConcatBits(A >> 7, 1, B >> 3, 5, G >> 3, 5, R >> 3, 5)
                                            s.WriteUInt16(ABGR)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.WriteUInt16(0)
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case BitmapType.A4B4G4R4
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ARGB = Rectangle(x, y)
                                            Dim A As Byte
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                            Dim ABGR = ConcatBits(A >> 4, 4, B >> 4, 4, G >> 4, 4, R >> 4, 4)
                                            s.WriteUInt16(ABGR)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.WriteUInt16(0)
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case BitmapType.A8B8G8R8
                            For j = 0 To NumHeightBlock - 1
                                For i = 0 To NumWidthBlock - 1
                                    For y = j * RectangleHeight To Min(j * RectangleHeight + RectangleHeight - 1, Height - 1)
                                        For x = i * RectanglePixelWidth To Min(i * RectanglePixelWidth + RectanglePixelWidth - 1, Width - 1)
                                            Dim ARGB = Rectangle(x, y)
                                            Dim A As Byte
                                            Dim B As Byte
                                            Dim G As Byte
                                            Dim R As Byte
                                            SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                            Dim ABGR = ConcatBits(A, 8, B, 8, G, 8, R, 8)
                                            s.WriteInt32(ABGR)
                                        Next
                                        For x = Width To i * RectanglePixelWidth + RectanglePixelWidth - 1
                                            s.WriteInt32(0)
                                        Next
                                    Next
                                    For y = Height To j * RectangleHeight + RectangleHeight - 1
                                        s.Write(New Byte(RectangleByteWidth - 1) {})
                                    Next
                                Next
                            Next
                        Case Else
                            Throw New NotSupportedException
                    End Select
                Next

                s.Position = 0
                Me.Data = s.Read(s.Length)
            End Using
        End Sub
    End Class

    Public Class PaletteBlock
        Inherits Block

        Public Indices As Integer()
        Public PaletteData As New List(Of Int32())

        Public Type As PaletteTypes
        Public NumColor As UInt16

        Private AddressStart As Int32

        Public ReadOnly Property NumFrame() As Integer
            Get
                If Indices Is Nothing Then Return 0
                Return Indices.Length
            End Get
        End Property


        Public Overrides Sub ReadData()
            Using s = StreamEx.Create()
                s.Write(Me.Data)
                s.Position = 0

                s.Position = 4
                Type = s.ReadUInt16

                s.Position = 8
                NumColor = s.ReadUInt16

                s.Position = &H18
                AddressStart = s.ReadInt32
                Dim PaletteStart = s.ReadInt32
                Dim PaletteEnd = s.ReadInt32

                s.Position = &H2E
                Dim NumFrame = s.ReadUInt16

                s.Position = AddressStart

                Select Case Type
                    Case PaletteTypes.B5G6R5
                        Dim FrameSize = NumColor * 2

                        Indices = New Int32(NumFrame - 1) {}
                        For n = 0 To NumFrame - 1
                            Dim PaletteAddress = s.ReadInt32
                            Dim Index = (PaletteAddress - PaletteStart) \ FrameSize
                            If Index * FrameSize <> PaletteAddress - PaletteStart Then
                                Throw New InvalidDataException
                            End If
                            Indices(n) = Index
                        Next

                        s.Position = PaletteStart
                        For n = 0 To (PaletteEnd - PaletteStart) \ FrameSize - 1
                            Dim Palette = New Int32(NumColor - 1) {}
                            For i = 0 To NumColor - 1
                                Dim ABGR = s.ReadInt16
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(B, 5, G, 6, R, 5, ABGR)
                                B = ConcatBits(B, 5, B >> 2, 3)
                                G = ConcatBits(G, 6, G >> 4, 2)
                                R = ConcatBits(R, 5, R >> 2, 3)
                                Palette(i) = ConcatBits(&HFF, 8, R, 8, G, 8, B, 8)
                            Next
                            PaletteData.Add(Palette)
                        Next
                    Case PaletteTypes.A1B5G5R5
                        Dim FrameSize = NumColor * 2

                        Indices = New Int32(NumFrame - 1) {}
                        For n = 0 To NumFrame - 1
                            Dim PaletteAddress = s.ReadInt32
                            Dim Index = (PaletteAddress - PaletteStart) \ FrameSize
                            If Index * FrameSize <> PaletteAddress - PaletteStart Then
                                Throw New InvalidDataException
                            End If
                            Indices(n) = Index
                        Next

                        s.Position = PaletteStart
                        For n = 0 To (PaletteEnd - PaletteStart) \ FrameSize - 1
                            Dim Palette = New Int32(NumColor - 1) {}
                            For i = 0 To NumColor - 1
                                Dim ABGR = s.ReadInt16
                                Dim A As Byte
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(A, 1, B, 5, G, 5, R, 5, ABGR)
                                A = ConcatBits(A, 1, A, 1)
                                A = ConcatBits(A, 2, A, 2)
                                A = ConcatBits(A, 4, A, 4)
                                B = ConcatBits(B, 5, B >> 2, 3)
                                G = ConcatBits(G, 5, G >> 2, 3)
                                R = ConcatBits(R, 5, R >> 2, 3)
                                Palette(i) = ConcatBits(A, 8, R, 8, G, 8, B, 8)
                            Next
                            PaletteData.Add(Palette)
                        Next
                    Case PaletteTypes.A4B4G4R4
                        Dim FrameSize = NumColor * 2

                        Indices = New Int32(NumFrame - 1) {}
                        For n = 0 To NumFrame - 1
                            Dim PaletteAddress = s.ReadInt32
                            Dim Index = (PaletteAddress - PaletteStart) \ FrameSize
                            If Index * FrameSize <> PaletteAddress - PaletteStart Then
                                Throw New InvalidDataException
                            End If
                            Indices(n) = Index
                        Next

                        s.Position = PaletteStart
                        For n = 0 To (PaletteEnd - PaletteStart) \ FrameSize - 1
                            Dim Palette = New Int32(NumColor - 1) {}
                            For i = 0 To NumColor - 1
                                Dim ABGR = s.ReadInt16
                                Dim A As Byte
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(A, 4, B, 4, G, 4, R, 4, ABGR)
                                A = ConcatBits(A, 4, A, 4)
                                B = ConcatBits(B, 4, B, 4)
                                G = ConcatBits(G, 4, G, 4)
                                R = ConcatBits(R, 4, R, 4)
                                Palette(i) = ConcatBits(A, 8, R, 8, G, 8, B, 8)
                            Next
                            PaletteData.Add(Palette)
                        Next
                    Case PaletteTypes.A8B8G8R8
                        Dim FrameSize = NumColor * 4

                        Indices = New Int32(NumFrame - 1) {}
                        For n = 0 To NumFrame - 1
                            Dim PaletteAddress = s.ReadInt32
                            Dim Index = (PaletteAddress - PaletteStart) \ FrameSize
                            If Index * FrameSize <> PaletteAddress - PaletteStart Then
                                Throw New InvalidDataException
                            End If
                            Indices(n) = Index
                        Next

                        s.Position = PaletteStart
                        For n = 0 To (PaletteEnd - PaletteStart) \ FrameSize - 1
                            Dim Palette = New Int32(NumColor - 1) {}
                            For i = 0 To NumColor - 1
                                Dim ABGR = s.ReadInt32
                                Dim A As Byte
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(A, 8, B, 8, G, 8, R, 8, ABGR)
                                Palette(i) = ConcatBits(A, 8, R, 8, G, 8, B, 8)
                            Next
                            PaletteData.Add(Palette)
                        Next
                    Case Else
                        Throw New NotSupportedException
                End Select
            End Using
        End Sub

        Public Overrides Sub WriteData()
            Using s = StreamEx.Create()
                s.Write(Me.Data)
                s.Position = 0

                s.Position = 4
                s.WriteUInt16(Type)

                s.Position = 8
                s.WriteUInt16(NumColor)

                Dim NumFrame As UInt16 = Indices.Length
                Dim FrameSize As Integer
                Select Case Type
                    Case PaletteTypes.B5G6R5, PaletteTypes.A1B5G5R5, PaletteTypes.A4B4G4R4
                        FrameSize = NumColor * 2
                    Case PaletteTypes.A8B8G8R8
                        FrameSize = NumColor * 4
                    Case Else
                        Throw New NotSupportedException
                End Select

                Dim PaletteStart = AddressStart + GetSpace2(NumFrame * 4)
                Dim PaletteEnd = PaletteStart + FrameSize * NumFrame

                s.Position = &H18
                s.WriteInt32(AddressStart)
                s.WriteInt32(PaletteStart)
                s.WriteInt32(PaletteEnd)

                s.Position = &H2E
                s.WriteUInt16(NumFrame)

                s.Position = AddressStart
                For n = 0 To NumFrame - 1
                    s.WriteInt32(PaletteStart + FrameSize * Indices(n))
                Next

                s.Position = PaletteStart

                Select Case Type
                    Case PaletteTypes.B5G6R5
                        For n = 0 To PaletteData.Count - 1
                            Dim Palette = PaletteData(n)
                            For i = 0 To NumColor - 1
                                Dim ARGB = Palette(i)
                                Dim A As Byte
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                Dim BGR = CID(ConcatBits(B >> 3, 5, G >> 2, 6, R >> 3, 5))
                                s.WriteInt16(BGR)
                            Next
                        Next
                    Case PaletteTypes.A1B5G5R5
                        For n = 0 To PaletteData.Count - 1
                            Dim Palette = PaletteData(n)
                            For i = 0 To NumColor - 1
                                Dim ARGB = Palette(i)
                                Dim A As Byte
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                Dim ABGR = CID(ConcatBits(A >> 7, 1, B >> 3, 5, G >> 3, 5, R >> 3, 5))
                                s.WriteInt16(ABGR)
                            Next
                        Next
                    Case PaletteTypes.A4B4G4R4
                        For n = 0 To PaletteData.Count - 1
                            Dim Palette = PaletteData(n)
                            For i = 0 To NumColor - 1
                                Dim ARGB = Palette(i)
                                Dim A As Byte
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                Dim ABGR = CID(ConcatBits(A >> 4, 4, B >> 4, 4, G >> 4, 4, R >> 4, 4))
                                s.WriteInt16(ABGR)
                            Next
                        Next
                    Case PaletteTypes.A8B8G8R8
                        For n = 0 To PaletteData.Count - 1
                            Dim Palette = PaletteData(n)
                            For i = 0 To NumColor - 1
                                Dim ARGB = Palette(i)
                                Dim A As Byte
                                Dim B As Byte
                                Dim G As Byte
                                Dim R As Byte
                                SplitBits(A, 8, R, 8, G, 8, B, 8, ARGB)
                                Dim ABGR = ConcatBits(A, 8, B, 8, G, 8, R, 8)
                                s.WriteInt32(ABGR)
                            Next
                        Next
                    Case Else
                        Throw New NotSupportedException
                End Select

                s.Position = 0
                Me.Data = s.Read(s.Length)
            End Using
        End Sub
    End Class

    Public Class CommentBlock
        Inherits Block

        ''' <summary>Shift-JIS注释信息</summary>
        Public Property CommentData() As Byte()
            Get
                Return Me.Data
            End Get
            Set(ByVal Value As Byte())
                Me.Data = Value
            End Set
        End Property
    End Class

    Public Class Image
        Public Palette As Int32()
        Public Rectangle As Byte(,)
        Public Address As Int32
        Public Length As Int32
        Public PicNumWidthBlock As Integer
        Public PicNumHeightBlock As Integer
        Public PicWidth As Integer
        Public PicHeight As Integer
    End Class
End Class
