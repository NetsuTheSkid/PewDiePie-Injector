Public Class Form2
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim oForm As New Form1()

        Me.Hide()
        oForm.ShowDialog()

        Me.Close()

    End Sub
End Class