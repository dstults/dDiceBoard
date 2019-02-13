Public Class FrmMain

#Region "Shared Variables"
    Private moveX As Integer
    Private moveY As Integer
    Private MoveVector As Point
    Private LastMouseLoc As Point
    Private LastClickTime As Double = DateTime.Now.Ticks
#End Region

    Private Sub FrmMain_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles Me.MouseDown
        LastMouseLoc = e.Location
        LastMouseLoc.Y -= Sys_TopMenuBuffer
        AssignSysPiecesAtMouseLoc()
    End Sub

    Private Sub FrmMain_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles Me.MouseUp
        'Touch screen compatibility: <= 1/4 of a second should be a double click
        Dim DoubleClickForceRightClick As Boolean
        Dim timeSpan As Double = DateTime.Now.Ticks - LastClickTime
        If timeSpan < 2500000 Then DoubleClickForceRightClick = True
        LastClickTime = DateTime.Now.Ticks
        If Sys_CurrentPiece IsNot Nothing Then
            If DoubleClickForceRightClick Then
                Sys_CurrentPiece.R_Click()
            Else
                Select Case e.Button
                    Case MouseButtons.Left
                        Sys_CurrentPiece.L_Click()
                    Case MouseButtons.Right, MouseButtons.XButton1
                        Sys_CurrentPiece.R_Click()
                    Case MouseButtons.Middle, MouseButtons.XButton2
                        Sys_CurrentPiece.M_Click()
                    Case Else
                        MsgBox(e.Button)
                End Select
            End If
        End If
        DoPieceClearing()
    End Sub

    Private Sub FrmMain_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles Me.MouseMove
        'Touch screen compatibility: <= 1/4 of a second should be a double click
        Dim timeSpan As Double = DateTime.Now.Ticks - LastClickTime
        If timeSpan >= 2500000 Then
            If Sys_MovePiece IsNot Nothing Then
                moveX = (e.X - LastMouseLoc.X)
                moveY = (e.Y - Sys_TopMenuBuffer - LastMouseLoc.Y)
                LastMouseLoc = e.Location
                LastMouseLoc.Y -= Sys_TopMenuBuffer
                Sys_CurrentPiece = Nothing
                Sys_MovePiece.Drag(e.Button = MouseButtons.Right, moveX, moveY, e.Location)
                Me.Invalidate()
            End If
        End If
    End Sub

    Private Sub FrmMain_Paint(ByVal sender As Object, ByVal e As PaintEventArgs) Handles Me.Paint
        bbGfx.Clear(Color.Transparent)
        bbGfx.DrawImage(bgImg, 1, 1, bgImg.Width, bgImg.Height)
        'GamePieces.Sort(Function(a, b) a.SizeLoc.Bottom.CompareTo(b.SizeLoc.Bottom))
        Dim SortedPieces As IOrderedEnumerable(Of ClsPiece) = GamePieces.OrderBy(Function(p) p.Layer).ThenBy(Function(p) p.SizeLoc.Bottom)
        For Each iPiece As ClsPiece In SortedPieces
            iPiece.Render()
        Next
        bbGfx.DrawEllipse(Pens.Red, LastMouseLoc.X, LastMouseLoc.Y, 3, 3)
        e.Graphics.DrawImage(BackBuffer, 0, Sys_TopMenuBuffer)
    End Sub

    Private Sub FrmMain_FormClosed() Handles Me.FormClosed
        Application.Exit()
    End Sub

    ' Assigns sys_movepiece and _currentpiece
    Private Sub AssignSysPiecesAtMouseLoc()
        ' Ensure movepiece is nothing to avoid checking when multiple
        ' buttons are pressed at a time.
        If Sys_MovePiece Is Nothing Then
            ' Scan each piece in reverse, gets only the piece on top as that should
            ' be the foremost one that the player thinks s/he is clicking on.
            For intA As Integer = GamePieces.Count - 1 To 0 Step -1
                Dim iPiece As ClsPiece = GamePieces(intA)
                ' Research transforming click here:
                'https://www.codeproject.com/Questions/265952/More-GDIplus-fun-moving-a-scaled-rotated-image
                'Dim ptByPiece() As Point = {LastMouseLoc}
                'iPiece.ClickMatrix.TransformPoints(ptByPiece)
                'MsgBox("X1: " & LastMouseLoc.X & " Y1: " & LastMouseLoc.Y & vbNewLine & " X2: " & ptByPiece(0).X & " Y2: " & ptByPiece(0).Y)
                If iPiece.Contains(LastMouseLoc) Then
                    Sys_MovePiece = iPiece.LowestMovableUnit
                    Sys_CurrentPiece = iPiece
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub DoPieceClearing()
        Sys_CurrentPiece = Nothing
        Sys_MovePiece = Nothing
        If Sys_DeletePiece IsNot Nothing Then
            Sys_DeletePiece.Imagery.DisposeAll()
            GamePieces.Remove(Sys_DeletePiece)
            Sys_DeletePiece = Nothing
        End If
    End Sub

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Width = 1024 + 12
        Me.Height = 768 + 36 + Sys_TopMenuBuffer
        'Me.Width = 2048
        'Me.Height = 1536
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Dim Args() As String = {"Load a Game File", "Game Data", "Game.txt"}
        LoadFile_Prompt(Args)
        If Me.MyOpenFileDialog.FileName <> "" Then
            ResetGame()
            LoadFile_Run()
            RefreshScreen()
        End If
    End Sub

End Class
