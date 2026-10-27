Public Class Form1

    Private Sub Form1_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        Try
            If DatabaseConnection.TestConnection() Then
                MessageBox.Show(
                    "Database connection successful!",
                    "BIMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )
            End If

        Catch ex As Exception
            MessageBox.Show(
                "Database connection failed:" &
                Environment.NewLine & ex.Message,
                "BIMS Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try

    End Sub

End Class
