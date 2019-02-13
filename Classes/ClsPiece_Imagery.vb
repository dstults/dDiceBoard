Public Class ClsPiece_Imagery

    Public Parent As ClsPiece
    Public StaticImg As Image
    Public AnimState As String
    Public Anims As New List(Of ClsPiece_Imagery_AnimSet)
    Public hFlip As Boolean
    Public vFlip As Boolean
    Public TopAngleModifier As Single

    Public Sub DrawImg()
        ' Determine Current Picture/Frame
        Dim MyAnimSet As ClsPiece_Imagery_AnimSet = Anims.Find(Function(p) p.Name = AnimState)
        Dim ReferenceImage As Image = Nothing
        If MyAnimSet Is Nothing Then ReferenceImage = StaticImg Else ReferenceImage = MyAnimSet.Img

        ' Prepare temp bmp that can be modified with rotateflips, can't use the original image because
        ' that will be referencing the original ... image. Which will be flipping itself back and forth.
        Dim TempBMP As New Bitmap(ReferenceImage)
        If vFlip Then TempBMP.RotateFlip(RotateFlipType.RotateNoneFlipY)
        If hFlip Then TempBMP.RotateFlip(RotateFlipType.RotateNoneFlipX)

        ' As that bitmap's already 'drawn', now we need to make a blank canvas that can hold the shape
        ' after its been potentially flipped and can also be rotated, so it needs to be a little bigger
        ' so as to avoid cutting off corners.
        Dim xyTot As Integer = ReferenceImage.Width + ReferenceImage.Height
        ' (xyTot - Size.W) / 2 + Size.W/2 = xyTot / 2 -- Y likewise
        Dim TempBMP2 As New Bitmap(xyTot, xyTot, Imaging.PixelFormat.Format32bppArgb)
        Dim OffsetFromExpansion As New Point(CInt((xyTot - Parent.SizeLoc.Width) / 2), CInt((xyTot - Parent.SizeLoc.Height) / 2))

        Using g = Graphics.FromImage(TempBMP2)
            ' Unrotated Hitbox
            g.DrawRectangle(Pens.Black, OffsetFromExpansion.X, OffsetFromExpansion.Y, Parent.SizeLoc.Width - 1, Parent.SizeLoc.Height - 1)
            ' Needs to rotate around center point.
            g.TranslateTransform(CInt(Int(xyTot / 2)), CInt(Int(xyTot / 2)))
            ' Rotate.
            g.RotateTransform(Parent.Rotation)
            ' Restore to corner point.
            g.TranslateTransform(-CInt(Int(TempBMP.Width / 2)), -CInt(Int(TempBMP.Height / 2)))
            'g.TranslateTransform(OffsetFromExpansion.X, OffsetFromExpansion.Y)
            'For SOME REASON THIS DOESN'T WORK:
            'g.Transform.RotateAt(Rotation, New PointF(xyTot \ 2, xyTot \ 2))
            ' Rotated Hitbox
            g.DrawRectangle(Pens.Black, 0, 0, Parent.SizeLoc.Width - 1, Parent.SizeLoc.Height - 1)
            'https://www.codeproject.com/Questions/265952/More-GDIplus-fun-moving-a-scaled-rotated-image
            Parent.ClickMatrix = g.Transform
            Parent.ClickMatrix.Invert()

            ' Rotated Image
            g.DrawImage(TempBMP, 0, 0)
            bbGfx.DrawImage(TempBMP2, Parent.SizeLoc.X - OffsetFromExpansion.X, Parent.SizeLoc.Y - OffsetFromExpansion.Y, xyTot, xyTot)
        End Using
        'If Me.Parent.Rotation <> 0 Then
        'bbGfx.DrawString(Me.Parent.Rotation.ToString, DefFont, Brushes.Red, Me.Parent.SizeLoc.Location)
        'ElseIf Me.Parent.Parent IsNot Nothing Then
        'bbGfx.DrawString(Me.Parent.AngFromParentCenter.ToString, BigFont, Brushes.Yellow, Me.Parent.SizeLoc.Location)
        'End If
        TempBMP2.Dispose()
        TempBMP.Dispose()
    End Sub

    Public Sub DisposeAll()
        StaticImg.Dispose()
        For Each iAnim As ClsPiece_Imagery_AnimSet In Anims
            For Each iImg As Image In iAnim.Images
                iImg.Dispose()
            Next
        Next
    End Sub

End Class

Public Class ClsPiece_Imagery_AnimSet

    Public Name As String
    'Public RefreshRate As Integer
    Private CurrentFrame As Integer
    Public Images As New List(Of Image)

    Public Function Frames() As Integer
        Return Images.Count
    End Function

    Public Function Img() As Image
        CurrentFrame += 1
        If CurrentFrame >= Frames() Then CurrentFrame = 0
        Return Images(CurrentFrame)
    End Function

End Class