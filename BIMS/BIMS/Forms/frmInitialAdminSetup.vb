
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmInitialAdminSetup
    Inherits Form

    Private ReadOnly _auth As New AuthenticationService()

    Private ReadOnly txtUsername As New TextBox()
    Private ReadOnly txtFullName As New TextBox()
    Private ReadOnly txtPassword As New TextBox()
    Private ReadOnly txtConfirmPassword As New TextBox()

    Public Sub New()
        Me.Text = "BIMS - Initial Admin Setup"
        Me.ClientSize = New Size(440, 390)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ControlBox = False

        Dim heading As New Label With {
            .Text = "Create the First Admin Account",
            .Font = New Font("Segoe UI", 15, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter
        }
        heading.SetBounds(20, 20, 400, 40)

        Dim explanation As New Label With {
            .Text = "No user accounts exist. Create an Admin account to begin.",
            .TextAlign = ContentAlignment.MiddleCenter
        }
        explanation.SetBounds(25, 65, 390, 38)

        AddField("Username", txtUsername, 30, 115)
        AddField("Full Name", txtFullName, 225, 115)
        AddField("Password (minimum 8 characters)", txtPassword, 30, 195)
        AddField("Confirm Password", txtConfirmPassword, 225, 195)

        txtPassword.UseSystemPasswordChar = True
        txtConfirmPassword.UseSystemPasswordChar = True

        Dim btnCreate As New Button With {
            .Text = "Create Admin Account",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold)
        }
        btnCreate.SetBounds(30, 295, 240, 40)

        Dim btnExit As New Button With {
            .Text = "Exit"
        }
        btnExit.SetBounds(285, 295, 125, 40)

        AddHandler btnCreate.Click, AddressOf CreateAdminClicked
        AddHandler btnExit.Click,
            Sub(sender, e)
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
            End Sub

        Me.Controls.AddRange({
            heading, explanation, btnCreate, btnExit
        })

        Me.AcceptButton = btnCreate
        Me.CancelButton = btnExit
    End Sub

    Private Sub AddField(
        caption As String,
        input As TextBox,
        x As Integer,
        y As Integer
    )
        Dim label As New Label With {
            .Text = caption,
            .AutoSize = True
        }
        label.SetBounds(x, y, 185, 20)
        input.SetBounds(x, y + 23, 185, 27)

        Me.Controls.Add(label)
        Me.Controls.Add(input)
    End Sub

    Private Sub CreateAdminClicked(
        sender As Object,
        e As EventArgs
    )
        Try
            If txtPassword.Text <> txtConfirmPassword.Text Then
                MessageBox.Show(
                    "Passwords do not match.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
                txtConfirmPassword.Focus()
                Return
            End If

            _auth.CreateFirstAdmin(
                txtUsername.Text,
                txtPassword.Text,
                txtFullName.Text)

            MessageBox.Show(
                "The first Admin account has been created successfully.",
                "Setup Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to create the Admin account: " & ex.Message,
                "Setup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

End Class
