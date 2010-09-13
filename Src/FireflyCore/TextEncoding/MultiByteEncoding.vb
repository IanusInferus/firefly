'==========================================================================
'
'  File:        MultiByteEncoding.vb
'  Location:    Firefly.TextEncoding <Visual Basic .Net>
'  Description: 多字节编码
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Namespace TextEncoding
    Public NotInheritable Class MultiByteEncoding
        Inherits Encoding

        Private BytesToChars As Dictionary(Of StringEx(Of Byte), StringEx(Of Char))
        Private CharsToBytes As Dictionary(Of StringEx(Of Char), StringEx(Of Byte))
        Private CharTokenizer As Tokenizer(Of Byte, Char)
        Private ByteTokenizer As Tokenizer(Of Char, Byte)

        Public Sub New(ByVal l As IEnumerable(Of StringCode))
            BytesToChars = New Dictionary(Of StringEx(Of Byte), StringEx(Of Char))
            CharsToBytes = New Dictionary(Of StringEx(Of Char), StringEx(Of Byte))
            Dim ReplacementString = ""
            For Each sc In l
                If Not sc.HasUnicodes Then Continue For
                If Not sc.HasCodes Then Continue For
                If sc.UnicodeString = "?" Then ReplacementString = "?"

                Dim bl As StringEx(Of Byte) = sc.Codes
                Dim cl = New StringEx(Of Char)(sc.UnicodeString.ToCharArray)

                If Not BytesToChars.ContainsKey(bl) Then BytesToChars.Add(bl, cl)
                If Not CharsToBytes.ContainsKey(cl) Then CharsToBytes.Add(cl, bl)
            Next

            CharTokenizer = New Tokenizer(Of Byte, Char)(BytesToChars)
            ByteTokenizer = New Tokenizer(Of Char, Byte)(CharsToBytes)

            Dim fi = GetType(Encoding).GetField("m_isReadOnly", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.SetField)
            fi.SetValue(Me, False)

            EncoderFallback = New EncoderReplacementFallback(ReplacementString)
            DecoderFallback = New DecoderReplacementFallback(ReplacementString)
        End Sub

        Public Overrides Function GetMaxByteCount(ByVal charCount As Integer) As Integer
            Return charCount * ByteTokenizer.MaxTokenPerSymbol
        End Function

        Public Overrides Function GetMaxCharCount(ByVal byteCount As Integer) As Integer
            Return byteCount * CharTokenizer.MaxTokenPerSymbol
        End Function

        Public Overrides Function GetEncoder() As System.Text.Encoder
            Return New InternalEncoder(Me)
        End Function

        Public Overrides Function GetDecoder() As System.Text.Decoder
            Return New InternalDecoder(Me)
        End Function

        Private Class Counter(Of T)
            Public Tick As Integer = 0
            Public Sub Count(ByVal Value As T)
                Tick += 1
            End Sub
        End Class

        Private Function DoEncoderCountFallback(ByVal CharUnknown As Char, ByVal Index As Integer, ByVal Counter As Counter(Of Byte)) As Boolean
            Dim Buffer = EncoderFallback.CreateFallbackBuffer()
            Dim ret = Buffer.Fallback(CharUnknown, Index)
            If Buffer.Remaining > 0 Then
                Counter.Tick += Buffer.Remaining
            End If
            Return ret
        End Function

        Public Overrides Function GetByteCount(ByVal chars() As Char, ByVal index As Integer, ByVal count As Integer) As Integer
            Dim State = ByteTokenizer.GetDefaultState
            Dim outputCounter As New Counter(Of Byte) With {.Tick = 0}
            Using input As New ArrayStream(Of Char)(chars, index, count)
                While input.Position < input.Length
                    State = ByteTokenizer.Feed(State, input.ReadElement)
                    State = ByteTokenizer.Transit(State, AddressOf outputCounter.Count, Function(c, Offset) DoEncoderCountFallback(c, input.Position + Offset, outputCounter))
                End While
                State = ByteTokenizer.Finish(State, AddressOf outputCounter.Count, Function(c, Offset) DoEncoderCountFallback(c, input.Position + Offset, outputCounter))
            End Using
            Return outputCounter.Tick
        End Function

        Private Function DoEncoderFallback(ByVal CharUnknown As Char, ByVal Index As Integer, ByVal WriteOutput As Action(Of Byte)) As Boolean
            Dim Buffer = EncoderFallback.CreateFallbackBuffer()
            Dim ret = Buffer.Fallback(CharUnknown, Index)
            Dim l As New List(Of Char)
            While Buffer.Remaining > 0
                l.Add(Buffer.GetNextChar)
            End While
            For Each v In GetBytes(l.ToArray)
                WriteOutput(v)
            Next
            Return ret
        End Function

        Public Overrides Function GetBytes(ByVal chars() As Char, ByVal charIndex As Integer, ByVal charCount As Integer, ByVal bytes() As Byte, ByVal byteIndex As Integer) As Integer
            Dim State = ByteTokenizer.GetDefaultState
            Using output As New ArrayStream(Of Byte)(bytes, byteIndex)
                Using input As New ArrayStream(Of Char)(chars, charIndex, charCount)
                    While input.Position < input.Length
                        State = ByteTokenizer.Feed(State, input.ReadElement)
                        State = ByteTokenizer.Transit(State, AddressOf output.WriteElement, Function(c, Offset) DoEncoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                    End While
                    State = ByteTokenizer.Finish(State, AddressOf output.WriteElement, Function(c, Offset) DoEncoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                End Using
                Return output.Position
            End Using
        End Function

        Private Function DoDecoderCountFallback(ByVal ByteUnknown As Byte, ByVal Index As Integer, ByVal Counter As Counter(Of Char)) As Boolean
            Dim Buffer = DecoderFallback.CreateFallbackBuffer()
            Dim ret = Buffer.Fallback(New Byte() {ByteUnknown}, Index)
            If Buffer.Remaining > 0 Then
                Counter.Tick += Buffer.Remaining
            End If
            Return ret
        End Function

        Public Overrides Function GetCharCount(ByVal bytes() As Byte, ByVal index As Integer, ByVal count As Integer) As Integer
            Dim State = CharTokenizer.GetDefaultState
            Dim outputCounter As New Counter(Of Char) With {.Tick = 0}
            Using input As New ArrayStream(Of Byte)(bytes, index, count)
                While input.Position < input.Length
                    State = CharTokenizer.Feed(State, input.ReadElement)
                    State = CharTokenizer.Transit(State, AddressOf outputCounter.Count, Function(c, Offset) DoDecoderCountFallback(c, input.Position + Offset, outputCounter))
                End While
                State = CharTokenizer.Finish(State, AddressOf outputCounter.Count, Function(c, Offset) DoDecoderCountFallback(c, input.Position + Offset, outputCounter))
            End Using
            Return outputCounter.Tick
        End Function

        Private Function DoDecoderFallback(ByVal ByteUnknown As Byte, ByVal Index As Integer, ByVal WriteOutput As Action(Of Char)) As Boolean
            Dim Buffer = DecoderFallback.CreateFallbackBuffer()
            Dim ret = Buffer.Fallback(New Byte() {ByteUnknown}, Index)
            While Buffer.Remaining > 0
                WriteOutput(Buffer.GetNextChar)
            End While
            Return ret
        End Function

        Public Overrides Function GetChars(ByVal bytes() As Byte, ByVal byteIndex As Integer, ByVal byteCount As Integer, ByVal chars() As Char, ByVal charIndex As Integer) As Integer
            Dim State = CharTokenizer.GetDefaultState
            Using output As New ArrayStream(Of Char)(chars, charIndex)
                Using input As New ArrayStream(Of Byte)(bytes, byteIndex, byteCount)
                    While input.Position < input.Length
                        State = CharTokenizer.Feed(State, input.ReadElement)
                        State = CharTokenizer.Transit(State, AddressOf output.WriteElement, Function(c, Offset) DoDecoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                    End While
                    State = CharTokenizer.Finish(State, AddressOf output.WriteElement, Function(c, Offset) DoDecoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                End Using
                Return output.Position
            End Using
        End Function

        ''' <summary>辅助类，仅仅是为了是使用跨缓冲区多次GetBytes的.Net内部的类正常。</summary>
        Private Class InternalEncoder
            Inherits Encoder

            Public Encoding As MultiByteEncoding
            Public State As TokenizerState(Of Byte, Char)

            Public Sub New(ByVal Encoding As MultiByteEncoding)
                Me.Encoding = Encoding
                State = Encoding.ByteTokenizer.GetDefaultState
            End Sub

            Public Overrides Sub Convert(ByVal chars() As Char, ByVal charIndex As Integer, ByVal charCount As Integer, ByVal bytes() As Byte, ByVal byteIndex As Integer, ByVal byteCount As Integer, ByVal flush As Boolean, ByRef charsUsed As Integer, ByRef bytesUsed As Integer, ByRef completed As Boolean)
                Using output As New ArrayStream(Of Byte)(bytes, byteIndex, byteCount)
                    Using input As New ArrayStream(Of Char)(chars, charIndex, charCount)
                        While input.Position < input.Length AndAlso output.Position < output.Length
                            State = Encoding.ByteTokenizer.Feed(State, input.ReadElement)
                            State = Encoding.ByteTokenizer.Transit(State, AddressOf output.WriteElement, Function(c, Offset) Encoding.DoEncoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                        End While
                        If flush Then
                            If output.Position < output.Length Then
                                State = Encoding.ByteTokenizer.Finish(State, AddressOf output.WriteElement, Function(c, Offset) Encoding.DoEncoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                            End If
                        End If
                        charsUsed = input.Position
                        bytesUsed = output.Position
                        completed = (input.Position = input.Length) AndAlso Encoding.ByteTokenizer.IsStateFinished(State)
                    End Using
                End Using
            End Sub

            Public Overrides Function GetByteCount(ByVal chars() As Char, ByVal index As Integer, ByVal count As Integer, ByVal flush As Boolean) As Integer
                Dim outputCounter As New Counter(Of Byte) With {.Tick = 0}
                Using input As New ArrayStream(Of Char)(chars, index, count)
                    While input.Position < input.Length
                        State = Encoding.ByteTokenizer.Feed(State, input.ReadElement)
                        State = Encoding.ByteTokenizer.Transit(State, AddressOf outputCounter.Count, Function(c, Offset) Encoding.DoEncoderCountFallback(c, input.Position + Offset, outputCounter))
                    End While
                    If flush Then State = Encoding.ByteTokenizer.Finish(State, AddressOf outputCounter.Count, Function(c, Offset) Encoding.DoEncoderCountFallback(c, input.Position + Offset, outputCounter))
                End Using
                Return outputCounter.Tick
            End Function

            Public Overrides Function GetBytes(ByVal chars() As Char, ByVal charIndex As Integer, ByVal charCount As Integer, ByVal bytes() As Byte, ByVal byteIndex As Integer, ByVal flush As Boolean) As Integer
                Dim byteCount = bytes.Length - byteIndex
                Dim bytesUsed = 0
                Dim charsUsed = 0
                Dim completed = False
                Convert(chars, charIndex, charCount, bytes, byteIndex, byteCount, flush, charsUsed, bytesUsed, completed)
                Return bytesUsed
            End Function

            Public Overrides Sub Reset()
                State = Encoding.ByteTokenizer.GetDefaultState
            End Sub
        End Class

        ''' <summary>辅助类，仅仅是为了是使用跨缓冲区多次GetChars的.Net内部的类正常。</summary>
        Private Class InternalDecoder
            Inherits Decoder

            Public Encoding As MultiByteEncoding
            Public State As TokenizerState(Of Char, Byte)

            Public Sub New(ByVal Encoding As MultiByteEncoding)
                Me.Encoding = Encoding
                State = Encoding.CharTokenizer.GetDefaultState
            End Sub

            Public Overrides Sub Convert(ByVal bytes() As Byte, ByVal byteIndex As Integer, ByVal byteCount As Integer, ByVal chars() As Char, ByVal charIndex As Integer, ByVal charCount As Integer, ByVal flush As Boolean, ByRef bytesUsed As Integer, ByRef charsUsed As Integer, ByRef completed As Boolean)
                Using output As New ArrayStream(Of Char)(chars, charIndex, charCount)
                    Using input As New ArrayStream(Of Byte)(bytes, byteIndex, byteCount)
                        While input.Position < input.Length AndAlso output.Position < output.Length
                            State = Encoding.CharTokenizer.Feed(State, input.ReadElement)
                            State = Encoding.CharTokenizer.Transit(State, AddressOf output.WriteElement, Function(c, Offset) Encoding.DoDecoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                        End While
                        If flush Then
                            If output.Position < output.Length Then
                                State = Encoding.CharTokenizer.Finish(State, AddressOf output.WriteElement, Function(c, Offset) Encoding.DoDecoderFallback(c, input.Position + Offset, AddressOf output.WriteElement))
                            End If
                        End If
                        bytesUsed = input.Position
                        charsUsed = output.Position
                        completed = (input.Position = input.Length) AndAlso Encoding.CharTokenizer.IsStateFinished(State)
                    End Using
                End Using
            End Sub

            Public Overrides Function GetCharCount(ByVal bytes() As Byte, ByVal index As Integer, ByVal count As Integer, ByVal flush As Boolean) As Integer
                Dim outputCounter As New Counter(Of Char) With {.Tick = 0}
                Using input As New ArrayStream(Of Byte)(bytes, index, count)
                    While input.Position < input.Length
                        State = Encoding.CharTokenizer.Feed(State, input.ReadElement)
                        State = Encoding.CharTokenizer.Transit(State, AddressOf outputCounter.Count, Function(c, Offset) Encoding.DoDecoderCountFallback(c, input.Position + Offset, outputCounter))
                    End While
                    If flush Then State = Encoding.CharTokenizer.Finish(State, AddressOf outputCounter.Count, Function(c, Offset) Encoding.DoDecoderCountFallback(c, input.Position + Offset, outputCounter))
                End Using
                Return outputCounter.Tick
            End Function

            Public Overrides Function GetChars(ByVal bytes() As Byte, ByVal byteIndex As Integer, ByVal byteCount As Integer, ByVal chars() As Char, ByVal charIndex As Integer, ByVal flush As Boolean) As Integer
                Dim charCount = chars.Length - charIndex
                Dim bytesUsed = 0
                Dim charsUsed = 0
                Dim completed = False
                Convert(bytes, byteIndex, byteCount, chars, charIndex, charCount, flush, bytesUsed, charsUsed, completed)
                Return charsUsed
            End Function

            Public Overrides Function GetCharCount(ByVal bytes() As Byte, ByVal index As Integer, ByVal count As Integer) As Integer
                Return GetCharCount(bytes, index, count, False)
            End Function

            Public Overrides Function GetChars(ByVal bytes() As Byte, ByVal byteIndex As Integer, ByVal byteCount As Integer, ByVal chars() As Char, ByVal charIndex As Integer) As Integer
                Return GetChars(bytes, byteIndex, byteCount, chars, charIndex, False)
            End Function

            Public Overrides Sub Reset()
                State = Encoding.CharTokenizer.GetDefaultState
            End Sub
        End Class
    End Class
End Namespace
