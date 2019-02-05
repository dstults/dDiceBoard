Module ModScripting

    Public Sub RunScriptFile(ByVal fileName As String)
        Dim FileDump As String = My.Computer.FileSystem.ReadAllText(fileName)
        Dim FileLines() As String = Split(FileDump, vbNewLine)
        Dim FileScript As New List(Of String)
        For intA As Integer = 0 To FileLines.Length - 1
            FileScript.Add(FileLines(intA))
        Next
        RunScript(FileScript)
    End Sub

    Public Sub RunScript(FullScript As List(Of String))
        For Each iLine As String In FullScript
            Dim TabFreeLine As String = Strings.Replace(iLine, vbTab, "")
            Dim phase1() As String = Strings.Split(TabFreeLine, ":=")
            For intA As Integer = 0 To phase1.Length - 1
                phase1(intA) = Trim(phase1(intA))
            Next
            If phase1(0) = "" Or Strings.Left(phase1(0), 2) = "--" Then GoTo SkipToNext
            Dim Left() As String = Nothing, RightSide As String = ""
            Dim Actor As ClsPiece = Nothing
            Dim Cmd As String = "", Args() As String = Nothing
            Left = Strings.Split(phase1(0), "|")
            For intA As Integer = 0 To Left.Length - 1
                Left(intA) = Trim(Left(intA))
            Next
            If Left.Length = 1 Then
                Cmd = LCase(Left(0))
                Actor = Sys_CurrentPiece
            ElseIf Left.Length = 2 Then
                Cmd = LCase(Left(1))
                Actor = GamePieces.Find(Function(p) p.Name = Left(0))
            End If
            If phase1.Length = 1 Then
                ' Do nothing
            ElseIf phase1.Length = 2 Then
                Args = Strings.Split(phase1(1), "|")
                For intA As Integer = 0 To Args.Length - 1
                    Args(intA) = Trim(Args(intA))
                Next
                RightSide = phase1(1)
            ElseIf phase1.Length > 2 Then
                ReDim Args(0)
                For intA As Integer = 1 To phase1.Length - 1
                    If intA > 1 Then Args(0) &= " := "
                    Args(0) &= Trim(phase1(intA))
                Next
                RightSide = Args(0)
            End If

            Select Case Cmd
                Case "reset"
                    ResetGame
                Case "board"
                    Select Case Args.Count
                        Case 1
                            DrawBoard(Args(0))
                        Case 3
                            DrawBoard(Args(0), CInt(Args(1)), CInt(Args(2)))
                        Case Else
                            ScriptError("board invalid arg count, should be 1 or 3", Actor, Cmd, TabFreeLine)
                    End Select
                Case "newpiece"
                    GamePieces.Add(New ClsPiece)
                    Sys_CurrentPiece = GamePieces.Last
                Case "delete"
                    Sys_DeletePiece = Actor
                Case "name"
                    Actor.Name = Args(0)
                Case "x"
                    Actor.SizeLoc.X = CInt(Args(0))
                    If Actor.Children.Count > 0 Then Actor.MoveChildren()
                Case "y"
                    Actor.SizeLoc.Y = CInt(Args(0)) '+ Sys_TopMenuBuffer
                    If Actor.Children.Count > 0 Then Actor.MoveChildren()
                Case "width"
                    Actor.SizeLoc.Width = CInt(Args(0))
                Case "height"
                    Actor.SizeLoc.Height = CInt(Args(0))
                Case "drag"
                    Actor.Drag(CInt(Args(0)), CInt(Args(1)))
                Case "layer"
                    Actor.Layer = CInt(Args(0))
                Case "img"
                    Actor.Imagery.StaticImg = New Bitmap(GetPicFileNamePath(Args(0)))
                    If Actor.SizeLoc.Width = 0 Then Actor.SizeLoc.Width = Actor.Imagery.StaticImg.Width
                    If Actor.SizeLoc.Height = 0 Then Actor.SizeLoc.Height = Actor.Imagery.StaticImg.Height
                Case "die"
                    Actor.DieSides = CInt(Args(0))
                Case "lclick"
                    If RightSide.Length > 0 Then
                        Actor.L_Click_Script.Add(RightSide)
                    Else
                        Actor.L_Click()
                    End If
                Case "rclick"
                    If RightSide.Length > 0 Then
                        Actor.R_Click_Script.Add(RightSide)
                    Else
                        Actor.R_Click()
                    End If
                Case "mclick"
                    If RightSide.Length > 0 Then
                        Actor.M_Click_Script.Add(RightSide)
                    Else
                        Actor.M_Click()
                    End If
                Case "play"
                    My.Computer.Audio.Play(GetSndFileNamePath(Args(0)))
                Case "prompt open file"
                    LoadFile_Prompt(Args)
                    If FrmMain.MyOpenFileDialog.FileName <> "" Then
                        LoadFile_Run()
                        Sys_DeletePiece = Actor
                        RefreshScreen()
                    End If
                Case "append file"
                    FrmMain.MyOpenFileDialog.FileName = GetFileNamePath(Args(0))
                    If FrmMain.MyOpenFileDialog.FileName <> "" Then
                        LoadFile_Run()
                    End If
                Case "parent"
                    Dim NewParent As ClsPiece = GamePieces.Find(Function(p) p.Name = Args(0))
                    If NewParent IsNot Nothing Then
                        Actor.Parent = NewParent
                        NewParent.Children.Add(Actor)
                        Actor.LocRelativeToParent.X = Actor.SizeLoc.X - NewParent.SizeLoc.X
                        Actor.LocRelativeToParent.Y = Actor.SizeLoc.Y - NewParent.SizeLoc.Y
                    Else
                        ScriptError("Piece not found", Actor, Cmd, TabFreeLine)
                    End If
                Case "child"
                    Dim NewChild As ClsPiece = GamePieces.Find(Function(p) p.Name = Args(0))
                    If NewChild IsNot Nothing Then
                        Actor.Children.Add(NewChild)
                        NewChild.Parent = Actor
                        Actor.LocRelativeToParent.X = NewChild.SizeLoc.X - Actor.SizeLoc.X
                        Actor.LocRelativeToParent.Y = NewChild.SizeLoc.Y - Actor.SizeLoc.Y
                    Else
                        ScriptError("Piece not found", Actor, Cmd, TabFreeLine)
                    End If
                Case "moveable"
                    Actor.CanMouseMove = CBool(Args(0))
                Case "roll"
                    Actor.Roll()
                Case "toggle lock"
                    Actor.RollLocked = Not Actor.RollLocked
                Case "text box"
                    Select Case Args.Count
                        Case 4
                            Actor.AttachTextBox("", CInt(Args(0)), CInt(Args(1)), CInt(Args(2)), CInt(Args(3)))
                        Case 5
                            Actor.AttachTextBox(Args(0), CInt(Args(1)), CInt(Args(2)), CInt(Args(3)), CInt(Args(4)))
                        Case Else
                            ScriptError("Text box wrong arg count, needs 4 or 5", Actor, Cmd, TabFreeLine)
                    End Select
                Case "text box text"
                    Actor.TextBoxes.Last.Text = Args(0)
                Case "text box text from list"
                    Select Case Args.Count
                        Case 1
                            Actor.TextBoxes.Last.Text = Actor.GetRndTextFromList(Args(0))
                        Case 2
                            Dim SourcePiece As ClsPiece = GamePieces.Find(Function(p) p.Name = Args(0))
                            Actor.TextBoxes.Last.Text = SourcePiece.GetRndTextFromList(Args(1))
                        Case Else
                            ScriptError("text box from list arg count mismatch, needs 1 or 2 args", Actor, Cmd, TabFreeLine)
                    End Select
                Case "text box frame"
                    Actor.TextBoxes.Last.DrawFrame = CBool(Args(0))
                Case "add stringlist"
                    Actor.Stringlists.Add(New ClsPiece_StringList With {.Name = Args(0)})
                Case "add string"
                    Actor.Stringlists.Last.Texts.Add(Args(0))
                Case Else
                    ScriptError("Cmd not found", Actor, Cmd, TabFreeLine)
            End Select
SkipToNext:
        Next
        RefreshScreen()
    End Sub

    Private Sub ScriptError(errMsg As String, aPiece As ClsPiece, cmd As String, TabFreeLine As String)
        MsgBox(errMsg & " -- cmd: >" & cmd & "<" &
                           vbNewLine & " Actor: >" & aPiece.Name & "<" &
                           vbNewLine & " Script Line:" & vbNewLine & ">" & TabFreeLine & "<")
    End Sub

    Private Sub DrawBoard(PicFile As String, Optional arg1 As Integer = 0, Optional arg2 As Integer = 0)
        Dim newBoard As New Bitmap(GetPicFileNamePath(PicFile))

        If arg1 > 0 And arg2 > 0 Then
            'bgGfx.DrawImage(newBoard, 0, Sys_TopMenuBuffer, arg1, arg2)
            bgGfx.DrawImage(newBoard, 0, 0, arg1, arg2)
        Else
            'bgGfx.DrawImage(newBoard, 0, Sys_TopMenuBuffer)
            bgGfx.DrawImage(newBoard, 0, 0)
        End If
        newBoard.Dispose()
    End Sub

    Public Sub LoadFile_Prompt(args() As String)
        FrmMain.MyOpenFileDialog.Title = args(0)
        FrmMain.MyOpenFileDialog.Filter = args(1) & "|" & args(2)
        FrmMain.MyOpenFileDialog.InitialDirectory = PackFolder
        FrmMain.MyOpenFileDialog.ShowDialog()
        If FrmMain.MyOpenFileDialog.FileName <> "" Then PackFolder = IO.Path.GetDirectoryName(FrmMain.MyOpenFileDialog.FileName)
    End Sub

    Public Sub LoadFile_Run()
        RunScriptFile(FrmMain.MyOpenFileDialog.FileName)
        FrmMain.MyOpenFileDialog.FileName = ""
    End Sub

    Public Sub ResetGame()
        bgGfx.Clear(Color.Transparent)
        For Each iPiece In GamePieces
            iPiece.Imagery.DisposeAll()
        Next
        GamePieces.Clear()
    End Sub

End Module
