Imports System.Drawing
Imports System.Windows.Forms

Public Class frmUserManagement

    Private ReadOnly _service As New UserManagementService()

    Private WithEvents grid As New DataGridView()
    Private WithEvents btnCreate As New Button()
    Private WithEvents btnToggle As New Button()
    Private WithEvents btnResetPassword As New Button()
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnClose As New Button()

    Private txtUsername As New TextBox()
    Private txtFullName As New TextBox()
    Private txtPassword As New TextBox()
    Private txtConfirmPassword As New TextBox()
    Private txtNewPassword As New TextBox()
    Private txtConfirmNewPassword As New TextBox()

    Public Sub New()
        InitializeComponent()
        BuildInterface()
    End Sub

    Private Sub BuildInterface()
        Me.Text = "User Account Management"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(1050, 700)
        Me.MinimumSize = New Size(850, 550)

        Dim topPanel As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 285,
            .Padding = New Padding(12)
        }

        Dim title As New Label With {
            .Text = "Staff Account Management",
            .Font = New Font("Segoe UI", 15, FontStyle.Bold),
            .Location = New Point(12, 10),
            .AutoSize = True
        }
        topPanel.Controls.Add(title)

        AddField(topPanel, "Username", txtUsername, 20, 55, 220)
        AddField(topPanel, "Full Name", txtFullName, 360, 55, 250)
        AddField(topPanel, "Initial Password", txtPassword, 20, 110, 220)
        AddField(topPanel, "Confirm Password", txtConfirmPassword, 360, 110, 250)

        txtPassword.UseSystemPasswordChar = True
        txtConfirmPassword.UseSystemPasswordChar = True

        btnCreate.Text = "Create Staff Account"
        btnCreate.SetBounds(650, 72, 180, 35)
        topPanel.Controls.Add(btnCreate)

        Dim resetLabel As New Label With {
            .Text = "Reset selected account password",
            .Location = New Point(20, 165),
            .AutoSize = True
        }
        topPanel.Controls.Add(resetLabel)

        AddField(topPanel, "New Password", txtNewPassword, 20, 190, 220)
        AddField(topPanel, "Confirm New Password", txtConfirmNewPassword, 360, 190, 250)

        txtNewPassword.UseSystemPasswordChar = True
        txtConfirmNewPassword.UseSystemPasswordChar = True

        btnResetPassword.Text = "Reset Password"
        btnResetPassword.SetBounds(650, 205, 180, 35)
        topPanel.Controls.Add(btnResetPassword)

        btnToggle.Text = "Activate / Deactivate"
        btnToggle.SetBounds(20, 245, 170, 30)
        topPanel.Controls.Add(btnToggle)

        btnRefresh.Text = "Refresh"
        btnRefresh.SetBounds(205, 245, 100, 30)
        topPanel.Controls.Add(btnRefresh)

        btnClose.Text = "Close"
        btnClose.SetBounds(320, 245, 100, 30)
        topPanel.Controls.Add(btnClose)

        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.RowHeadersVisible = False

        Me.Controls.Add(grid)
        Me.Controls.Add(topPanel)

        AddHandler Me.Load, AddressOf FormLoaded
    End Sub

    Private Sub AddField(
        parent As Control,
        caption As String,
        input As TextBox,
        x As Integer,
        y As Integer,
        width As Integer)

        Dim label As New Label With {
            .Text = caption,
            .Location = New Point(x, y),
            .AutoSize = True
        }

        input.SetBounds(x, y + 20, width, 25)
        parent.Controls.Add(label)
        parent.Controls.Add(input)
    End Sub

    Private Sub FormLoaded(sender As Object, e As EventArgs)
        Try
            If AppSession.CurrentUser Is Nothing OrElse
               Not AppSession.CurrentUser.IsActive OrElse
               Not String.Equals(
                   AppSession.CurrentUser.Role,
                   "Admin",
                   StringComparison.OrdinalIgnoreCase) Then

                MessageBox.Show(
                    "Only active administrators can manage accounts.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                Me.Close()
                Return
            End If

            LoadAccounts()

        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub LoadAccounts()
        grid.DataSource = _service.GetAllAccounts()

        If grid.Columns.Contains("Account ID") Then
            grid.Columns("Account ID").Visible = False
        End If

        If grid.Columns.Contains("Username") Then
            grid.Columns("Username").FillWeight = 90
        End If

        If grid.Columns.Contains("Full Name") Then
            grid.Columns("Full Name").FillWeight = 140
        End If

        If grid.Columns.Contains("Date Created") Then
            grid.Columns("Date Created").FillWeight = 130
        End If
    End Sub

    Private Function SelectedAccountId() As Integer
        If grid.CurrentRow Is Nothing Then
            Throw New InvalidOperationException(
                "Select an account from the list first.")
        End If

        Return CInt(grid.CurrentRow.Cells("Account ID").Value)
    End Function

    Private Function SelectedAccountStatus() As String
        If grid.CurrentRow Is Nothing Then
            Throw New InvalidOperationException(
                "Select an account from the list first.")
        End If

        Return CStr(grid.CurrentRow.Cells("Account Status").Value)
    End Function

    Private Sub btnCreate_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnCreate.Click

        Try
            _service.CreateStaff(
                txtUsername.Text,
                txtFullName.Text,
                txtPassword.Text,
                txtConfirmPassword.Text)

            MessageBox.Show(
                "Staff account created successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            txtUsername.Clear()
            txtFullName.Clear()
            txtPassword.Clear()
            txtConfirmPassword.Clear()

            LoadAccounts()

        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub btnToggle_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnToggle.Click

        Try
            Dim id As Integer = SelectedAccountId()
            Dim status As String = SelectedAccountStatus()
            Dim activate As Boolean = (status = "Inactive")

            Dim actionText As String =
                If(activate, "activate", "deactivate")

            If MessageBox.Show(
                "Are you sure you want to " & actionText &
                " this account?",
                "Confirm Account Change",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then

                Return
            End If

            _service.SetAccountActive(id, activate)
            LoadAccounts()

            MessageBox.Show(
                "Account status updated.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub btnResetPassword_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnResetPassword.Click

        Try
            Dim id As Integer = SelectedAccountId()

            If MessageBox.Show(
                "Reset the password for the selected account?",
                "Confirm Password Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then

                Return
            End If

            _service.ResetPassword(
                id,
                txtNewPassword.Text,
                txtConfirmNewPassword.Text)

            txtNewPassword.Clear()
            txtConfirmNewPassword.Clear()

            MessageBox.Show(
                "Password reset successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub btnRefresh_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnRefresh.Click

        Try
            LoadAccounts()
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub btnClose_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnClose.Click

        Me.Close()
    End Sub

    Private Sub ShowError(ex As Exception)
        MessageBox.Show(
            ex.Message,
            "Account Management",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)
    End Sub

    Private Sub frmUserManagement_Load(
        sender As Object,
        e As EventArgs) Handles MyBase.Load

        ' Authorization is handled by FormLoaded and UserManagementService.
    End Sub

End Class