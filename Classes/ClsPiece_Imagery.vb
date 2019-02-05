Public Class ClsPiece_Imagery

    Public StaticImg As Image
    Public AnimState As String
    Public Anims As New List(Of ClsPiece_Imagery_AnimSet)

    Public Function Img() As Image
        Dim MyAnimSet As ClsPiece_Imagery_AnimSet = Anims.Find(Function(p) p.Name = AnimState)
        If MyAnimSet Is Nothing Then Return StaticImg Else Return MyAnimSet.Img
    End Function

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