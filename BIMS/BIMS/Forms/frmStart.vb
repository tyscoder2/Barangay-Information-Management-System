
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmStart
    Inherits Form

    Public Sub New()
        Me.Text = "BIMS - Welcome"
        Me.ClientSize = New Size(600, 380)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.BackColor = Color.WhiteSmoke

        Dim heading As New Label With {
            .Text = "BARANGAY INFORMATION" &
                    Environment.NewLine & "MANAGEMENT SYSTEM",
            .Font = New Font("Segoe UI", 19, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter
        }
        heading.SetBounds(40, 45, 520, 90)

        Dim subtitle As New Label With {
            .Text = "Welcome! Please select an option.",
            .Font = New Font("Segoe UI", 11),
            .TextAlign = ContentAlignment.MiddleCenter
        }
        subtitle.SetBounds(40, 145, 520, 30)

        Dim btnRegister As New Button With {
            .Text = "Register",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = Color.FromArgb(39, 120, 85),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        btnRegister.SetBounds(100, 215, 180, 55)

        Dim btnLogin As New Button With {
            .Text = "Admin / Staff Login",
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .BackColor = Color.FromArgb(45, 90, 145),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        btnLogin.SetBounds(320, 215, 180, 55)

        AddHandler btnRegister.Click, AddressOf RegisterClicked
        AddHandler btnLogin.Click, AddressOf LoginClicked

        Me.Controls.AddRange({
            heading, subtitle, btnRegister, btnLogin
        })
    End Sub

    Private Sub RegisterClicked(sender As Object, e As EventArgs)
        Using portal As New frmResidentPortal()
            portal.ShowDialog(Me)
        End Using
    End Sub

    Private Sub LoginClicked(sender As Object, e As EventArgs)
        Using form As New frmLogin()
            form.ShowDialog(Me)
        End Using
    End Sub


    Private Sub frmStart_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        Try
            Dim users As New UserRepository()

            If users.CountUsers() = 0 Then
                Using setup As New frmInitialAdminSetup()
                    If setup.ShowDialog(Me) <> DialogResult.OK Then
                        ' Initial setup was cancelled; do not continue
                        ' into the application without an Admin account.
                        Me.Close()
                        Return
                    End If
                End Using

                ' Confirm an account now exists before continuing.
                If users.CountUsers() = 0 Then
                    MessageBox.Show(
                        "No Admin account was created. The application will close.",
                        "Setup Incomplete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)

                    Me.Close()
                    Return
                End If
            End If

        Catch ex As Exception
            MessageBox.Show(
                "Unable to check user accounts: " & ex.Message,
                "BIMS Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Me.Close()
        End Try

    End Sub

End Class
