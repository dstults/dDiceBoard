Module ModInit

#Region "Shared Fonts, Colors, Pens, and Brushes"
    Public BigFont As New Font("Calibri", 48, FontStyle.Bold)
    Public MedBigFont As New Font("Calibri", 32, FontStyle.Bold)
    Public DefFont As New Font("Calibri", 22, FontStyle.bold)
    Public DiceFont As New Font("Calibri", 28, FontStyle.Bold)
    'Public DefPen As New Pen(Color.Black)
    Public OutlinePen As New Pen(Color.FromArgb(212, Color.Black)) With {.Width = 2}
    Public FramePen As New Pen(Color.Black) With {.Width = 3}
#End Region

#Region "Shared Data"
    Public GamePieces As New List(Of ClsPiece)
    Public Sys_DeletePiece As ClsPiece
    Public RefreshDisplay As Boolean
    Public Sys_CurrentPiece As ClsPiece
    Public Sys_MovePiece As ClsPiece
    Public PackFolder As String = Application.StartupPath & "\gamepacks\"
#End Region

#Region "Shared Graphics"
    Public Sys_TopMenuBuffer As Integer = 24
    Public BackBuffer As New Bitmap(1024, 768, Imaging.PixelFormat.Format32bppArgb)
    Public bbGfx As Graphics = Graphics.FromImage(BackBuffer)
    Public bgImg As New Bitmap(1024, 768, Imaging.PixelFormat.Format32bppArgb)
    Public bgGfx As Graphics = Graphics.FromImage(bgImg)
#End Region

    Public Sub Main()
        Randomize()

        ' Set graphics to antialias:
        bbGfx.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
        bbGfx.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        CreatePackLoader()
        'loadFile("ToD.txt")
        FrmMain.Show()
        Application.Run()
    End Sub

    Private Sub CreatePackLoader()
        GamePieces.Add(New ClsPiece)
        GamePieces.Last.R_Click_Script.Add("Prompt Open File := Load a Game File|Game Data|Game.txt")
        GamePieces.Last.SizeLoc.X = 32
        GamePieces.Last.SizeLoc.Y = 28
        GamePieces.Last.Imagery.StaticImg = New Bitmap(GetPicFileNamePath("folderBig.png"))
        GamePieces.Last.SizeLoc.Width = GamePieces.Last.Img.Width
        GamePieces.Last.SizeLoc.Height = GamePieces.Last.Img.Height
        GamePieces.Last.AttachTextBox("Right click me to load a game data pack!", 67, 0, 300, 64)
    End Sub

    Public Function GetFileNamePath(fileName As String) As String
        Dim fileFound As String = ""

        fileFound = PackFolder & "\" & fileName
        If Dir(fileFound) <> "" Then Return fileFound
        fileFound = Application.StartupPath & "\default\" & fileName
        If Dir(fileFound) <> "" Then Return fileFound

        Return fileFound
    End Function

    Public Function GetPicFileNamePath(fileName As String) As String
        Dim fileFound As String = ""

        fileFound = PackFolder & "\art\" & fileName
        If Dir(fileFound) <> "" Then Return fileFound
        fileFound = Application.StartupPath & "\default\art\" & fileName
        If Dir(fileFound) <> "" Then Return fileFound

        Return fileFound
    End Function

    Public Function GetSndFileNamePath(fileName As String) As String
        Dim fileFound As String = ""

        fileFound = PackFolder & "\snd\" & fileName
        If Dir(fileFound) <> "" Then Return fileFound
        fileFound = Application.StartupPath & "\default\snd\" & fileName
        If Dir(fileFound) <> "" Then Return fileFound

        Return fileFound
    End Function

    Public Sub RefreshScreen()
        FrmMain.Invalidate()
    End Sub

End Module
