
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmLogin
    Inherits Form

    Private ReadOnly auth As New AuthenticationService()
    Private ReadOnly txtUsername As New TextBox()
    Private ReadOnly txtPassword As New TextBox()

    Public Sub New()
        Me.Text = "BIMS - Admin / Staff Login"
        Me.ClientSize = New Size(400, 290)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        Dim heading As New Label With {
            .Text = "Admin / Staff Login",
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter
        }
        heading.SetBounds(25, 20, 350, 40)

        Dim lblUsername As New Label With {.Text = "Username"}
        lblUsername.SetBounds(30, 80, 330, 22)
        txtUsername.SetBounds(30, 103, 340, 30)

        Dim lblPassword As New Label With {.Text = "Password"}
        lblPassword.SetBounds(30, 145, 330, 22)
        txtPassword.SetBounds(30, 168, 340, 30)
        txtPassword.UseSystemPasswordChar = True

        Dim btnLogin As New Button With {
            .Text = "Login",
            .Font = New Font("Segoe UI", 11, FontStyle.Bold)
        }
        btnLogin.SetBounds(30, 220, 210, 38)

        Dim btnCancel As New Button With {.Text = "Cancel"}
        btnCancel.SetBounds(250, 220, 120, 38)
        btnCancel.DialogResult = DialogResult.Cancel

        AddHandler btnLogin.Click, AddressOf LoginClicked

        Me.AcceptButton = btnLogin
        Me.CancelButton = btnCancel
        Me.Controls.AddRange({
            heading, lblUsername, txtUsername,
            lblPassword, txtPassword, btnLogin, btnCancel
        })
    End Sub

    Private Sub LoginClicked(sender As Object, e As EventArgs)
        Try
            Dim loggedIn As User =
                auth.Login(txtUsername.Text, txtPassword.Text)

            If loggedIn Is Nothing Then
                MessageBox.Show(
                    "Invalid credentials or inactive account.",
                    "Login Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                txtPassword.Clear()
                txtPassword.Focus()
                Return
            End If

            AppSession.CurrentUser = loggedIn

            Me.Hide()

            Try
                Using dashboard As New frmAdminDashboard()
                    dashboard.ShowDialog(Me)
                End Using
            Finally
                AppSession.Clear()
                txtPassword.Clear()
                Me.Show()
            End Try

        Catch ex As Exception
            MessageBox.Show(
                "Unable to log in: " & ex.Message,
                "BIMS Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try
    End Sub

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
