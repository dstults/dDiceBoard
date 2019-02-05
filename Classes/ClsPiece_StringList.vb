Public Class ClsPiece_StringList
    Public Name As String
    Public Texts As New List(Of String)

    Public Function RndText() As String
        Return Texts(CInt(Int(Rnd() * Texts.Count)))
    End Function
End Class
