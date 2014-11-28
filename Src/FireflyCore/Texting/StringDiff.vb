'==========================================================================
'
'  File:        StringDiff.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 串比较
'  Version:     2014.11.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq

Namespace Texting
    Public Class TranslatePart
        Public SourceIndex As Integer
        Public SourceLength As Integer
        Public TargetIndex As Integer
        Public TargetLength As Integer
    End Class

    Public NotInheritable Class StringDiff
        Private Sub New()
        End Sub

        Private Class ComparerBreadthFirst(Of T)
            Private Comparer As IEqualityComparer(Of T)
            Private Source As IList(Of T)
            Private Target As IList(Of T)
            Private N As Integer
            Private M As Integer

            Public Sub New(ByVal Source As IList(Of T), ByVal Target As IList(Of T), ByVal Comparer As IEqualityComparer(Of T))
                Me.Comparer = Comparer
                Me.Source = Source
                Me.Target = Target
                N = Source.Count
                M = Target.Count
            End Sub

            Private Class ListNode
                Public x As Integer
                Public y As Integer
                Public Previous As ListNode
            End Class

            Public Function GetDifference() As TranslatePart()
                Dim Success As Boolean = False
                Dim Route As ListNode = Nothing
                Dim Even As Boolean = True

                Dim xRoot As Integer = 0
                Dim yRoot As Integer = 0
                Dim hMaxRoot As Integer = 0
                While xRoot < N AndAlso yRoot < M
                    If Comparer.Equals(Source(xRoot), Target(yRoot)) Then
                        xRoot += 1
                        yRoot += 1
                    Else
                        Exit While
                    End If
                End While

                Dim MinDeterminedSolutionCost = (N - xRoot) + (M - yRoot)
                Dim kMinDeterminedSolution = 0
                Dim k2Lx As New Dictionary(Of Integer, ListNode)
                k2Lx.Add(0, New ListNode With {.x = xRoot, .y = yRoot, .Previous = Nothing})

                If xRoot = N AndAlso yRoot = M Then
                    Success = True
                    Route = k2Lx(0)
                End If

                For D = 1 To N + M
                    If Success Then Exit For
                    Even = (D Mod 2 = 0)
                    For k = -D To D Step 2
                        Dim IsRemoveReachable = k2Lx.ContainsKey(k - 1)
                        Dim IsAddReachable = k2Lx.ContainsKey(k + 1)

                        Dim Previous As ListNode
                        Dim x As Integer
                        Dim y As Integer
                        Dim hMax As Integer
                        If IsRemoveReachable AndAlso IsAddReachable Then
                            Dim RemoveReachedPrevious = k2Lx(k - 1)
                            Dim xRemove = RemoveReachedPrevious.x + 1
                            Dim yRemove = xRemove - k
                            Dim hMaxRemove = (N - xRemove) + (M - yRemove)
                            Dim AddReachedPrevious = k2Lx(k + 1)
                            Dim xAdd = AddReachedPrevious.x
                            Dim yAdd = xAdd - k
                            Dim hMaxAdd = (N - xAdd) + (M - yAdd)
                            If hMaxRemove < hMaxAdd Then
                                Previous = RemoveReachedPrevious
                                x = xRemove
                                y = yRemove
                                hMax = hMaxRemove
                            Else
                                Previous = AddReachedPrevious
                                x = xAdd
                                y = yAdd
                                hMax = hMaxAdd
                            End If
                        ElseIf IsRemoveReachable Then
                            Previous = k2Lx(k - 1)
                            x = Previous.x + 1
                            y = x - k
                            hMax = (N - x) + (M - y)
                        ElseIf IsAddReachable Then
                            Previous = k2Lx(k + 1)
                            x = Previous.x
                            y = x - k
                            hMax = (N - x) + (M - y)
                        Else
                            Continue For
                        End If

                        If x > N OrElse y > M Then
                            If k2Lx.ContainsKey(k) Then k2Lx.Remove(k)
                            Continue For
                        End If

                        Dim hMin = 1
                        If D + hMin > MinDeterminedSolutionCost AndAlso Abs(k - kMinDeterminedSolution) > 1 Then
                            If k2Lx.ContainsKey(k) Then k2Lx.Remove(k)
                            Continue For
                        End If

                        While x < N AndAlso y < M
                            If Comparer.Equals(Source(x), Target(y)) Then
                                x += 1
                                y += 1
                            Else
                                Exit While
                            End If
                        End While

                        hMax = (N - x) + (M - y)
                        If D + hMax <= MinDeterminedSolutionCost Then
                            MinDeterminedSolutionCost = D + hMax
                            kMinDeterminedSolution = k
                        End If

                        If k2Lx.ContainsKey(k) Then
                            k2Lx(k) = New ListNode With {.x = x, .y = y, .Previous = Previous}
                        Else
                            k2Lx.Add(k, New ListNode With {.x = x, .y = y, .Previous = Previous})
                        End If

                        If x = N AndAlso y = M Then
                            Success = True
                            Route = k2Lx(k)
                            Exit For
                        End If
                    Next
                Next
                If Not Success Then Throw New InvalidOperationException

                Dim CurrentRouteReversed As New List(Of TranslatePart)
                Dim CurrentPart As New TranslatePart With {.SourceIndex = N, .SourceLength = 0, .TargetIndex = M, .TargetLength = 0}
                Dim CurrentNode = Route
                While CurrentNode IsNot Nothing
                    Dim xDifference = CurrentPart.SourceIndex - CurrentNode.x
                    Dim yDifference = CurrentPart.TargetIndex - CurrentNode.y
                    Dim SnakeDifference = Min(xDifference, yDifference)
                    If SnakeDifference > 0 Then
                        If CurrentPart.SourceLength <> 0 OrElse CurrentPart.TargetLength <> 0 Then
                            CurrentRouteReversed.Add(CurrentPart)
                        End If
                        Dim xSnake = CurrentPart.SourceIndex - SnakeDifference
                        Dim ySnake = CurrentPart.TargetIndex - SnakeDifference
                        CurrentRouteReversed.Add(New TranslatePart With {.SourceIndex = xSnake, .SourceLength = SnakeDifference, .TargetIndex = ySnake, .TargetLength = SnakeDifference})
                        CurrentPart = New TranslatePart With {.SourceIndex = CurrentNode.x, .SourceLength = xSnake - CurrentNode.x, .TargetIndex = CurrentNode.y, .TargetLength = ySnake - CurrentNode.y}
                    Else
                        Dim DotProduct = xDifference * CurrentPart.TargetLength - yDifference * CurrentPart.SourceLength
                        If DotProduct <> 0 Then
                            CurrentRouteReversed.Add(CurrentPart)
                            CurrentPart = New TranslatePart With {.SourceIndex = CurrentNode.x, .SourceLength = xDifference, .TargetIndex = CurrentNode.y, .TargetLength = yDifference}
                        Else
                            CurrentPart = New TranslatePart With {.SourceIndex = CurrentNode.x, .SourceLength = CurrentPart.SourceLength + xDifference, .TargetIndex = CurrentNode.y, .TargetLength = CurrentPart.TargetLength + yDifference}
                        End If
                    End If
                    CurrentNode = CurrentNode.Previous
                End While
                If CurrentPart.SourceLength <> 0 OrElse CurrentPart.TargetLength <> 0 Then
                    CurrentRouteReversed.Add(CurrentPart)
                End If

                If CurrentPart.SourceIndex <> 0 OrElse CurrentPart.TargetIndex <> 0 Then
                    CurrentRouteReversed.Add(New TranslatePart With {.SourceIndex = 0, .SourceLength = CurrentPart.SourceIndex, .TargetIndex = 0, .TargetLength = CurrentPart.TargetIndex})
                End If

                Return Enumerable.Range(0, CurrentRouteReversed.Count).Select(Function(i) CurrentRouteReversed(CurrentRouteReversed.Count - 1 - i)).ToArray
            End Function
        End Class

        Public Shared Function Compare(Of T As IEquatable(Of T))(ByVal Source As IList(Of T), ByVal Target As IList(Of T)) As TranslatePart()
            Return (New ComparerBreadthFirst(Of T)(Source, Target, EqualityComparer(Of T).Default)).GetDifference()
        End Function
        Public Shared Function Compare(Of T)(ByVal Source As IList(Of T), ByVal Target As IList(Of T), ByVal Comparer As IEqualityComparer(Of T)) As TranslatePart()
            Return (New ComparerBreadthFirst(Of T)(Source, Target, Comparer)).GetDifference()
        End Function
    End Class
End Namespace
