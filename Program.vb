Option Strict On
Option Infer On
Imports VbPixelGameEngine

Public NotInheritable Class SudokuGame
    Inherits PixelGameEngine

    Friend Const VIEWPORT_W As Integer = 792, VIEWPORT_H As Integer = 596
    Private ReadOnly sudokuBoard(8, 8) As Integer, selectables(8, 8) As Boolean
    Private selectedCell As Vi2d? = Nothing

    Public Sub New()
        AppName = "VBPGE Sudoku Game"
    End Sub

    Private ReadOnly Property IsSudokuComplete As Boolean
        Get
            ' Check rows and zeros.
            For row As Integer = 0 To 8 Step 1
                Dim rowNums As New HashSet(Of Integer)
                For col As Integer = 0 To 8 Step 1
                    If sudokuBoard(row, col) = 0 Then Return False
                    rowNums.Add(sudokuBoard(row, col))
                Next col
                If rowNums.Count <> 9 Then Return False
            Next row

            ' Check columns.
            For col As Integer = 0 To 8 Step 1
                Dim colNums As New HashSet(Of Integer)
                For row As Integer = 0 To 8 Step 1
                    colNums.Add(sudokuBoard(row, col))
                Next row
                If colNums.Count <> 9 Then Return False
            Next col

            ' Check subgrids.
            For i As Integer = 0 To 2 Step 1
                For j As Integer = 0 To 2 Step 1
                    Dim subgrid As New HashSet(Of Integer)
                    For x As Integer = 0 To 2 Step 1
                        For y As Integer = 0 To 2 Step 1
                            subgrid.Add(sudokuBoard(i * 3 + x, j * 3 + y))
                        Next y
                    Next x
                    If subgrid.Count <> 9 Then Return False
                Next j
            Next i

            Return True
        End Get
    End Property

    Private Function CreateSudokuBoard(numBlanks As Integer) As Integer()()
        If numBlanks < 30 OrElse numBlanks > 50 Then
            Throw New ArgumentException(Nothing, NameOf(numBlanks))
        End If

        Dim baseBoard As Integer()() = {
            New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9},
            New Integer() {4, 5, 6, 7, 8, 9, 1, 2, 3},
            New Integer() {7, 8, 9, 1, 2, 3, 4, 5, 6},
            New Integer() {2, 3, 4, 5, 6, 7, 8, 9, 1},
            New Integer() {5, 6, 7, 8, 9, 1, 2, 3, 4},
            New Integer() {8, 9, 1, 2, 3, 4, 5, 6, 7},
            New Integer() {3, 4, 5, 6, 7, 8, 9, 1, 2},
            New Integer() {6, 7, 8, 9, 1, 2, 3, 4, 5},
            New Integer() {9, 1, 2, 3, 4, 5, 6, 7, 8}
        }

        ' Shuffle rows within bands.
        For band As Integer = 0 To 6 Step 3
            Dim rows As New List(Of Integer()) From {
                baseBoard(band),
                baseBoard(band + 1),
                baseBoard(band + 2)
            }
            Shuffle(rows)
            For i As Integer = 0 To 2 Step 1
                baseBoard(band + i) = rows(i)
            Next i
        Next band

        ' Shuffle columns within stacks.
        For stk As Integer = 0 To 6 Step 3
            Dim cols As New List(Of Integer())
            For col As Integer = stk To stk + 2 Step 1
                Dim currCol(8) As Integer
                For row As Integer = 0 To 8 Step 1
                    currCol(row) = baseBoard(row)(col)
                Next row
                cols.Add(currCol)
            Next col
            Shuffle(cols)
            For i As Integer = 0 To 2 Step 1
                Dim currCol = stk + i
                For row As Integer = 0 To 8 Step 1
                    baseBoard(row)(currCol) = cols(i)(row)
                Next row
            Next i
        Next stk

        ' Create number permutation.
        Dim numbers As New List(Of Integer)(Enumerable.Range(1, 9))
        Shuffle(numbers)
        For row As Integer = 0 To 8 Step 1
            For col As Integer = 0 To 8 Step 1
                baseBoard(row)(col) = numbers(baseBoard(row)(col) - 1)
            Next col
        Next row

        ' Blank out cells.
        Dim blanks As New HashSet(Of (Integer, Integer))
        While blanks.Count < numBlanks
            blanks.Add((CInt(Rnd * 8), CInt(Rnd * 8)))
        End While
        For Each blank As (Integer, Integer) In blanks
            baseBoard(blank.Item1)(blank.Item2) = 0
        Next blank

        Return baseBoard
    End Function

    Private Sub Shuffle(Of T)(lst As List(Of T))
        For n As Integer = lst.Count - 1 To 2 Step -1
            Dim k = CInt(Rnd * n)
            Dim value = lst(k)
            lst(k) = lst(n)
            lst(n) = value
        Next n
    End Sub

    Protected Overrides Function OnUserCreate() As Boolean
        Try
            Dim userInput$ = InputBox("Enter the number of blanks to fill in the board
(ranging from 30 to 50):", Title:="Welcome to the Sudoku game!")
            Dim initialBoard = CreateSudokuBoard(CInt(userInput))
            For i As Integer = 0 To 8 Step 1
                For j As Integer = 0 To 8 Step 1
                    sudokuBoard(i, j) = initialBoard(i)(j)
                    selectables(i, j) = (initialBoard(i)(j) = 0)
                Next j
            Next i
            Return True
        Catch ex As Exception
            MsgBox("Please try again with a valid number input within range [30, 50].",
                   Title:="Oops! Sudoku board cannot be created.")
            Return False
        End Try
    End Function

    Protected Overrides Function OnUserUpdate(elapsedTime As Single) As Boolean
        Clear()

        ' Handle mouse input.
        If GetMouse(0).Pressed AndAlso Not IsSudokuComplete Then
            Dim col = GetMouseX \ (VIEWPORT_W \ 9)
            Dim row = GetMouseY \ (VIEWPORT_H \ 9)
            If col >= 0 AndAlso col < 9 AndAlso row >= 0 AndAlso row < 9 Then
                selectedCell = New Vi2d(col, row)
            End If
        End If

        ' Handle keyboard input.
        If selectedCell.HasValue AndAlso Not IsSudokuComplete Then
            Dim row = selectedCell.Value.y
            Dim col = selectedCell.Value.x
            If selectables(row, col) Then
                For k As Integer = Key.K1 To Key.K9
                    If GetKey(CType(k, Key)).Pressed Then
                        sudokuBoard(row, col) = k - Key.K1 + 1
                        selectedCell = Nothing
                        Exit For
                    End If
                Next k
            End If
        End If

        ' Draw grid lines.
        For i As Integer = 1 To 8 Step 1
            If i Mod 3 <> 0 Then Continue For
            DrawLine(New Vi2d(i * (VIEWPORT_W \ 9), 0),
                     New Vi2d(i * (VIEWPORT_W \ 9), VIEWPORT_H), Presets.White)
            DrawLine(New Vi2d(0, i * (VIEWPORT_H \ 9)),
                     New Vi2d(VIEWPORT_W, i * (VIEWPORT_H \ 9)), Presets.White)
        Next i

        ' Draw numbers and selection.
        For row As Integer = 0 To 8 Step 1
            For col As Integer = 0 To 8 Step 1
                Dim number = sudokuBoard(row, col)
                Dim cellColor = If(selectables(row, col), Presets.Cyan, Presets.White)
                If selectedCell.HasValue AndAlso selectedCell.Value.x = col AndAlso
                        selectedCell.Value.y = row Then
                    cellColor = If(selectables(row, col), Presets.Yellow, Presets.White)
                End If

                If number <> 0 Then
                    DrawString(New Vi2d(col * (VIEWPORT_W \ 9) + 20,
                                        row * (VIEWPORT_H \ 9) + 15),
                               number.ToString(), cellColor, 5)
                Else
                    FillCircle(New Vi2d(col * (VIEWPORT_W \ 9) + (VIEWPORT_W \ 18),
                                        row * (VIEWPORT_H \ 9) + (VIEWPORT_H \ 18)),
                               10, cellColor)
                End If
            Next col
        Next row

        ' Check win condition.
        If IsSudokuComplete Then
            Dim textPos As New Vi2d(100, VIEWPORT_H \ 2 - 20)
            Dim bgRectPos As New Vi2d(100, VIEWPORT_H \ 2 - 25)
            FillRect(bgRectPos, New Vi2d(600, 50), Presets.Black)
            DrawString(textPos, "Congratulations! You Win!", Presets.Yellow, 3)
        End If

        Return Not GetKey(Key.ESCAPE).Pressed
    End Function

    Friend Shared Sub Main()
        With New SudokuGame
            If .Construct(VIEWPORT_W, VIEWPORT_H, fullScreen:=True) Then .Start()
        End With
    End Sub
End Class