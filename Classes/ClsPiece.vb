Public Class ClsPiece

    ' Base Data
    Public Name As String
    Public CanMouseMove As Boolean = True
    Public CanMouseRotate As Boolean = False
    Public Function LowestMovableUnit() As ClsPiece
        If Me.CanMouseMove Then Return Me
        If Parent IsNot Nothing Then Return Parent.LowestMovableUnit
        Return Nothing
    End Function
    Public Layer As Integer
    Public SizeLoc As New Rectangle
    Public ClickMatrix As Drawing2D.Matrix
    Public Rotation As Single
    Public L_Click_Script As New List(Of String)
    Public R_Click_Script As New List(Of String)
    Public M_Click_Script As New List(Of String)

    ' Visuals
    Public Imagery As New ClsPiece_Imagery
    Public TextBoxes As New HashSet(Of ClsPiece_TextBox)

    ' Linked Pieces
    Public Parent As ClsPiece
    Public AngFromParentCenter As Single
    Public DistFromParentCenter As Single
    Public Children As New HashSet(Of ClsPiece)

    ' Roller-Only
    Public DieSides As Integer
    Public RollLocked As Boolean
    Public LastRoll As Integer
    Public Function Roll() As Boolean
        If IsDie() And Not RollLocked Then
            My.Computer.Audio.Play(GetSndFileNamePath("dice-throw_2.wav"))
            LastRoll = CInt(Int(Rnd() * DieSides + 1))
            Return True
        End If
        Return False
    End Function
    Public Function IsDie() As Boolean
        Return DieSides > 0
    End Function

    ' Deck-Only
    Public Stringlists As New List(Of ClsPiece_StringList)

    Public Sub New()
        Imagery.Parent = Me
    End Sub

    Public Function GetRndTextFromList(listName As String) As String
        Return Stringlists.Find(Function(p) p.Name = listName).RndText
    End Function

    Public Sub L_Click()
        RunScript(L_Click_Script)
    End Sub

    Public Sub R_Click()
        RunScript(R_Click_Script)
    End Sub

    Public Sub M_Click()
        RunScript(M_Click_Script)
    End Sub

    Public Sub Drag(R_Button As Boolean, ByVal moveX As Integer, ByVal moveY As Integer, ByVal mousePt As Point)
        If moveX <> 0 Or moveY <> 0 Then
            If R_Button And Me.CanMouseRotate Then
                'Me.Rotate(moveX - moveY)
                Me.SetRotation(GetAngle(Me.SizeLoc.X + CInt(Int(Me.SizeLoc.Width / 2)), Me.SizeLoc.Y + CInt(Int(Me.SizeLoc.Height / 2)), mousePt.X, mousePt.Y) + Imagery.TopAngleModifier)
                RefreshDisplay = True
            ElseIf Me.CanMouseMove Then
                Me.SizeLoc.X += moveX
                Me.SizeLoc.Y += moveY
                If Me.SizeLoc.Left < 4 Then Me.SizeLoc.X = 4
                If Me.SizeLoc.Top < 4 Then Me.SizeLoc.Y = 4
                If Me.SizeLoc.Right > BackBuffer.Width - 2 Then Me.SizeLoc.X = BackBuffer.Width - Me.SizeLoc.Width - 2
                If Me.SizeLoc.Bottom > BackBuffer.Height - 3 Then Me.SizeLoc.Y = BackBuffer.Height - Me.SizeLoc.Height - 3
                If Children.Count > 0 Then Me.MoveChildren()
                RefreshDisplay = True
            End If
        End If
    End Sub
    Public Sub MoveChildren()
        Dim parentCenter As Point = New Point(Me.SizeLoc.X + CInt(Int(Me.SizeLoc.Width / 2)), Me.SizeLoc.Y + CInt(Int(Me.SizeLoc.Height / 2)))
        For Each iChild As ClsPiece In Children
            Dim angToChild As Single = CSng((iChild.AngFromParentCenter + Me.Rotation) / 180 * Math.PI)

            Dim dX = CInt(Int(iChild.DistFromParentCenter * Math.Cos(angToChild)))
            Dim dY = CInt(Int(iChild.DistFromParentCenter * Math.Sin(angToChild)))
            'MsgBox("Angle " & ang1 & " has changed to " & ang2 & " and as such the new dX is " & dX & " and dY is " & dY & ".")

            Dim childCenter As Point
            childCenter.X = parentCenter.X + dX
            childCenter.Y = parentCenter.Y + dY
            iChild.SizeLoc.X = childCenter.X - CInt(Int(iChild.SizeLoc.Width / 2))
            iChild.SizeLoc.Y = childCenter.Y - CInt(Int(iChild.SizeLoc.Width / 2))

            If iChild.Children.Count > 0 Then iChild.MoveChildren()
        Next
    End Sub

    Public Sub SetRotation(nAng As Single)
        Dim dAng As Single = nAng - Rotation
        Rotation = nAng
        If Children.Count > 0 Then Me.RotateChildren(dAng)
        CheckRotation()
    End Sub
    Public Sub Rotate(dAng As Single)
        Rotation += dAng
        If Children.Count > 0 Then Me.RotateChildren(dAng)
        CheckRotation()
    End Sub
    Private Sub CheckRotation()
        If Rotation > 180 Then
            Do Until Rotation <= 180
                Rotation -= 360
            Loop
        End If
        If Rotation <= -180 Then
            Do Until Rotation > -180
                Rotation += 360
            Loop
        End If
    End Sub
    Public Sub RotateChildren(dAng As Single)
        For Each iChild As ClsPiece In Children
            iChild.Rotate(dAng)
            If iChild.Children.Count > 0 Then iChild.RotateChildren(dAng)
        Next
        MoveChildren()
    End Sub

    Public Sub DrawImg()
        Me.Imagery.DrawImg()
    End Sub

    Public Overridable Sub Render()
        bbGfx.ResetTransform()
        Me.DrawImg
        bbGfx.ResetTransform()
        If IsDie() Then
            If RollLocked Then
                bbGfx.DrawString("[" & Format(LastRoll, "0") & "]", DiceFont, Brushes.Black, SizeLoc.X - 4, SizeLoc.Y - 1)
            Else
                bbGfx.DrawString(CStr(LastRoll), DiceFont, Brushes.Black, SizeLoc.X + 8, SizeLoc.Y - 1)
            End If
        End If
        For Each iTB As ClsPiece_TextBox In TextBoxes
            Dim FrameRect As New Rectangle With
                {.X = Me.SizeLoc.X + iTB.SizeLoc.X, .Y = Me.SizeLoc.Y + iTB.SizeLoc.Y,
                .Width = iTB.SizeLoc.Width, .Height = iTB.SizeLoc.Height}
            Dim ShadowRect As New Rectangle With
                {.X = FrameRect.X + 4, .Y = FrameRect.Y + 4,
                .Width = FrameRect.Width, .Height = FrameRect.Height}
            If iTB.DrawFrame Then
                bbGfx.FillRectangle(ShadowBrush, ShadowRect)
                bbGfx.FillRectangle(Brushes.LightYellow, FrameRect)
                bbGfx.DrawRectangle(FramePen, FrameRect)
            End If
            DrawOutlinedString(iTB.Text, DefFont, FrameRect, iTB.DrawFrame)
        Next
    End Sub
    Private Sub DrawOutlinedString(aText As String, aFont As Font, aRect As Rectangle, addFrameBuffer As Boolean)

        Dim drawTextRect As New Rectangle
        drawTextRect = aRect
        If addFrameBuffer Then
            drawTextRect.X += 4
            drawTextRect.Y += 1
            drawTextRect.Width -= 6
            drawTextRect.Height -= 1
        End If

        Dim outlinePath As New Drawing2D.GraphicsPath
        outlinePath.AddString(aText, aFont.FontFamily, aFont.Style, aFont.Size, drawTextRect, StringFormat.GenericTypographic)

        'bbGfx.FillPath(Brushes.LightGray, outlinePath) ' Shadow?
        bbGfx.DrawPath(OutlinePen, outlinePath)
        bbGfx.FillPath(Brushes.Goldenrod, outlinePath)

        outlinePath.Dispose()
    End Sub

    Public Function Contains(ByVal thisPoint As Point) As Boolean
        ' Returns  My_X <= This_X <= My_X+_W  &&  My_Y <= This_Y <= My_Y+_H
        bbGfx.DrawRectangle(Pens.Red, Me.SizeLoc)
        If Me.SizeLoc.X <= thisPoint.X And thisPoint.X <= Me.SizeLoc.X + Me.SizeLoc.Width And Me.SizeLoc.Y <= thisPoint.Y And thisPoint.Y <= Me.SizeLoc.Y + Me.SizeLoc.Height Then Return True
        For Each iTB As ClsPiece_TextBox In Me.TextBoxes
            If Me.SizeLoc.X + iTB.SizeLoc.X <= thisPoint.X And thisPoint.X <= Me.SizeLoc.X + iTB.SizeLoc.X + iTB.SizeLoc.Width And Me.SizeLoc.Y + iTB.SizeLoc.Y <= thisPoint.Y And thisPoint.Y <= Me.SizeLoc.Y + iTB.SizeLoc.Y + iTB.SizeLoc.Height Then Return True
        Next
        Return False
    End Function

    Public Sub SetChildren(uniqueNames As String)
        Dim childStringList() As String = Split(uniqueNames, "|")
        For Each iChild As String In childStringList
            Dim newChild As ClsPiece = GamePieces.Find(Function(p) p.Name = iChild)
            If newChild IsNot Nothing Then
                Me.Children.Add(newChild)
            Else
                MsgBox("Child object '" & iChild & "' not found for parent object '" & Me.Name & "'!")
            End If
        Next
    End Sub

    Public Sub AttachTextBox(aText As String, x As Integer, y As Integer, width As Integer, height As Integer)
        Me.TextBoxes.Add(New ClsPiece_TextBox With {.Text = aText})
        Me.TextBoxes.Last.SizeLoc.X = x
        Me.TextBoxes.Last.SizeLoc.Y = y
        Me.TextBoxes.Last.SizeLoc.Width = width
        Me.TextBoxes.Last.SizeLoc.Height = height
    End Sub

End Class
