﻿Public Class Form3
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If TextBox1.Text = "Netsu" And TextBox2.Text = "Pass" Then

            Dim oForm As New Form1()

            Me.Hide()
            oForm.ShowDialog()

            Me.Close()

        Else

            MsgBox("Sorry, username or password not found", MsgBoxStyle.OkOnly, "Invalid")

        End If

    End Sub
End Class