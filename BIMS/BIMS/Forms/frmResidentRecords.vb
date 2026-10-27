
Imports Microsoft.Data.SqlClient
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmResidentRecords
    Inherits Form

    Private ReadOnly _purok As String

    Private ReadOnly grid As New DataGridView()
    Private ReadOnly txtSearch As New TextBox()
    Private ReadOnly cmbStatus As New ComboBox()
    Private ReadOnly btnSave As New Button()
    Private ReadOnly btnRefresh As New Button()
    Private ReadOnly btnArchive As New Button()
    Private ReadOnly btnRestore As New Button()
    Private ReadOnly chkShowArchived As New CheckBox()
    Private ReadOnly lblStatus As New Label()

    Private residents As New DataTable()

    Public Sub New(Optional purok As String = Nothing)
        If Not HasStaffAccess() Then
            Throw New UnauthorizedAccessException(
                "Only active, authorized barangay staff can access resident records.")
        End If

        _purok = purok

        Me.Text = If(purok Is Nothing,
                     "BIMS - Resident Approvals",
                     "BIMS - " & purok & " Residents")

        Me.ClientSize = New Size(1050, 620)
        Me.MinimumSize = New Size(850, 550)
        Me.StartPosition = FormStartPosition.CenterParent

        Dim lblSearch As New Label With {
            .Text = "Search name:",
            .AutoSize = True
        }
        lblSearch.SetBounds(15, 18, 100, 25)
        txtSearch.SetBounds(115, 15, 260, 28)

        btnRefresh.Text = "Refresh"
        btnRefresh.SetBounds(390, 13, 100, 32)

        chkShowArchived.Text = "Show archived"
        chkShowArchived.AutoSize = True
        chkShowArchived.SetBounds(510, 18, 150, 25)

        grid.SetBounds(15, 55, 1020, 395)
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill
        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect = False
        grid.RowHeadersVisible = False

        lblStatus.Text = "Approval status:"
        lblStatus.SetBounds(15, 465, 125, 25)

        cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList
        cmbStatus.Items.AddRange({"Pending", "Approved", "Rejected"})
        cmbStatus.SetBounds(150, 462, 180, 30)

        btnSave.Text = "Save Status"
        btnSave.SetBounds(350, 458, 140, 38)

        btnArchive.Text = "Archive Resident"
        btnArchive.SetBounds(15, 515, 160, 38)

        btnRestore.Text = "Restore Resident"
        btnRestore.SetBounds(185, 515, 160, 38)

        AddHandler txtSearch.TextChanged, AddressOf SearchChanged
        AddHandler btnRefresh.Click, AddressOf RefreshClicked
        AddHandler btnSave.Click, AddressOf SaveStatus
        AddHandler btnArchive.Click, AddressOf ArchiveResident
        AddHandler btnRestore.Click, AddressOf RestoreResident
        AddHandler chkShowArchived.CheckedChanged,
            AddressOf ArchivedFilterChanged
        AddHandler grid.SelectionChanged, AddressOf RowSelected

        Me.Controls.AddRange({
            lblSearch,
            txtSearch,
            btnRefresh,
            chkShowArchived,
            grid,
            lblStatus,
            cmbStatus,
            btnSave,
            btnArchive,
            btnRestore
        })

        UpdateActionButtons()
        LoadResidents()
    End Sub

    Private Function HasStaffAccess() As Boolean
        Return AppSession.CurrentUser IsNot Nothing AndAlso
               AppSession.CurrentUser.IsActive AndAlso
               (String.Equals(
                    AppSession.CurrentUser.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(
                    AppSession.CurrentUser.Role,
                    "Staff",
                    StringComparison.OrdinalIgnoreCase))
    End Function

    Private Function HasSelectedResident() As Boolean
        Return grid.CurrentRow IsNot Nothing AndAlso
               grid.CurrentRow.Cells("id").Value IsNot Nothing AndAlso
               grid.CurrentRow.Cells("id").Value IsNot DBNull.Value
    End Function

    Private Function SelectedResidentId() As Integer
        If Not HasSelectedResident() Then
            Throw New InvalidOperationException(
                "Select a resident record first.")
        End If

        Return Convert.ToInt32(grid.CurrentRow.Cells("id").Value)
    End Function

    Private Function SelectedResidentIsArchived() As Boolean
        If Not HasSelectedResident() Then Return False

        Return Convert.ToBoolean(
            grid.CurrentRow.Cells("Is Archived").Value)
    End Function

    Private Sub LoadResidents()
        If Not HasStaffAccess() Then
            MessageBox.Show(
                "Your account is no longer authorized to access resident records.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Close()
            Return
        End If

        Try
            Const sql As String =
                "SELECT r.id, " &
                "r.first_name AS [First Name], " &
                "r.middle_name AS [Middle Name], " &
                "r.last_name AS [Last Name], " &
                "r.birthdate AS Birthdate, " &
                "r.sex AS Sex, " &
                "h.purok AS Purok, " &
                "r.approval_status AS [Approval Status], " &
                "r.status AS [Resident Status], " &
                "r.is_archived AS [Is Archived] " &
                "FROM dbo.residents r " &
                "LEFT JOIN dbo.households h ON h.id = r.household_id " &
                "WHERE (@purok IS NULL OR h.purok = @purok) " &
                "AND (@search = '' OR r.first_name LIKE @pattern " &
                "OR r.last_name LIKE @pattern) " &
                "AND r.is_archived = @isArchived " &
                "ORDER BY r.last_name, r.first_name;"

            residents = New DataTable()

            Using con = DatabaseConnection.CreateConnection()
                Using cmd As New SqlCommand(sql, con)

                    cmd.Parameters.Add(
                        "@purok", SqlDbType.NVarChar, 50).Value =
                        If(_purok Is Nothing,
                           CType(DBNull.Value, Object),
                           _purok)

                    cmd.Parameters.Add(
                        "@search", SqlDbType.NVarChar, 100).Value =
                        txtSearch.Text.Trim()

                    cmd.Parameters.Add(
                        "@pattern", SqlDbType.NVarChar, 102).Value =
                        "%" & txtSearch.Text.Trim() & "%"

                    cmd.Parameters.Add(
                        "@isArchived", SqlDbType.Bit).Value =
                        chkShowArchived.Checked

                    Using adapter As New SqlDataAdapter(cmd)
                        adapter.Fill(residents)
                    End Using
                End Using
            End Using

            grid.DataSource = residents

            grid.Columns("id").Visible = False
            grid.Columns("Is Archived").Visible = False

            grid.Columns("Birthdate").DefaultCellStyle.Format =
                "yyyy-MM-dd"

            cmbStatus.SelectedIndex = -1

            UpdateActionButtons()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load residents: " & ex.Message,
                "Resident Records",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SearchChanged(sender As Object, e As EventArgs)
        LoadResidents()
    End Sub

    Private Sub RefreshClicked(sender As Object, e As EventArgs)
        LoadResidents()
    End Sub

    Private Sub ArchivedFilterChanged(
        sender As Object,
        e As EventArgs)

        LoadResidents()
    End Sub

    Private Sub RowSelected(sender As Object, e As EventArgs)
        If Not HasSelectedResident() Then
            cmbStatus.SelectedIndex = -1
            UpdateActionButtons()
            Return
        End If

        If SelectedResidentIsArchived() Then
            cmbStatus.SelectedIndex = -1
        Else
            Dim value =
                grid.CurrentRow.Cells("Approval Status").Value

            If value IsNot Nothing AndAlso
               value IsNot DBNull.Value AndAlso
               cmbStatus.Items.Contains(value.ToString()) Then

                cmbStatus.SelectedItem = value.ToString()
            Else
                cmbStatus.SelectedIndex = -1
            End If
        End If

        UpdateActionButtons()
    End Sub

    Private Sub UpdateActionButtons()
        Dim hasRow As Boolean = HasSelectedResident()
        Dim archived As Boolean =
            hasRow AndAlso SelectedResidentIsArchived()

        btnArchive.Enabled =
            HasStaffAccess() AndAlso hasRow AndAlso Not archived

        btnRestore.Enabled =
            HasStaffAccess() AndAlso hasRow AndAlso archived

        btnSave.Enabled =
            HasStaffAccess() AndAlso hasRow AndAlso Not archived

        cmbStatus.Enabled =
            HasStaffAccess() AndAlso hasRow AndAlso Not archived
    End Sub

    Private Sub SaveStatus(sender As Object, e As EventArgs)
        If Not HasStaffAccess() Then
            MessageBox.Show(
                "You are not authorized to update resident approval statuses.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If Not HasSelectedResident() OrElse
           cmbStatus.SelectedIndex < 0 Then

            MessageBox.Show(
                "Select an unarchived resident and an approval status.")
            Return
        End If

        If SelectedResidentIsArchived() Then
            MessageBox.Show(
                "Archived residents cannot have their approval status changed. Restore the record first.")
            Return
        End If

        Dim residentId As Integer = SelectedResidentId()
        Dim newStatus As String = cmbStatus.Text

        Try
            Using con = DatabaseConnection.CreateConnection()
                con.Open()

                Const sql As String =
                    "UPDATE dbo.residents SET " &
                    "approval_status = @status, " &
                    "approved_by = CASE WHEN @status = 'Approved' " &
                    "THEN @userId ELSE NULL END, " &
                    "approved_at = CASE WHEN @status = 'Approved' " &
                    "THEN SYSDATETIME() ELSE NULL END " &
                    "WHERE id = @id AND is_archived = 0;"

                Using cmd As New SqlCommand(sql, con)
                    cmd.Parameters.Add(
                        "@status", SqlDbType.NVarChar, 20).Value =
                        newStatus

                    cmd.Parameters.Add(
                        "@userId", SqlDbType.Int).Value =
                        AppSession.CurrentUser.Id

                    cmd.Parameters.Add(
                        "@id", SqlDbType.Int).Value = residentId

                    If cmd.ExecuteNonQuery() = 1 Then
                        MessageBox.Show("Approval status updated.")
                    Else
                        MessageBox.Show(
                            "Resident record not found or already archived.")
                    End If
                End Using
            End Using

            LoadResidents()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to save status: " & ex.Message,
                "Resident Records",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ArchiveResident(sender As Object, e As EventArgs)
        If Not HasStaffAccess() Then
            MessageBox.Show(
                "You are not authorized to archive resident records.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If Not HasSelectedResident() OrElse
           SelectedResidentIsArchived() Then

            MessageBox.Show("Select an unarchived resident to archive.")
            Return
        End If

        Dim residentId As Integer = SelectedResidentId()
        Dim residentName As String =
            Convert.ToString(
                grid.CurrentRow.Cells("First Name").Value) & " " &
            Convert.ToString(
                grid.CurrentRow.Cells("Last Name").Value)

        If MessageBox.Show(
            "Archive the record for " & residentName & "?" &
            Environment.NewLine & Environment.NewLine &
            "The resident's data will be retained and can be restored later.",
            "Confirm Archive",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then

            Return
        End If

        Try
            Using con = DatabaseConnection.CreateConnection()
                con.Open()

                Const sql As String =
                    "UPDATE dbo.residents " &
                    "SET is_archived = 1 " &
                    "WHERE id = @id AND is_archived = 0;"

                Using cmd As New SqlCommand(sql, con)
                    cmd.Parameters.Add(
                        "@id", SqlDbType.Int).Value = residentId

                    If cmd.ExecuteNonQuery() = 1 Then
                        MessageBox.Show(
                            "Resident archived successfully.",
                            "Archive Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
                    Else
                        MessageBox.Show(
                            "The resident was not found or is already archived.")
                    End If
                End Using
            End Using

            LoadResidents()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to archive resident: " & ex.Message,
                "Archive Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RestoreResident(sender As Object, e As EventArgs)
        If Not HasStaffAccess() Then
            MessageBox.Show(
                "You are not authorized to restore resident records.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If Not HasSelectedResident() OrElse
           Not SelectedResidentIsArchived() Then

            MessageBox.Show("Select an archived resident to restore.")
            Return
        End If

        Dim residentId As Integer = SelectedResidentId()
        Dim residentName As String =
            Convert.ToString(
                grid.CurrentRow.Cells("First Name").Value) & " " &
            Convert.ToString(
                grid.CurrentRow.Cells("Last Name").Value)

        If MessageBox.Show(
            "Restore the record for " & residentName & "?" &
            Environment.NewLine & Environment.NewLine &
            "The resident will appear in the normal records list again.",
            "Confirm Restore",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then

            Return
        End If

        Try
            Using con = DatabaseConnection.CreateConnection()
                con.Open()

                Const sql As String =
                    "UPDATE dbo.residents " &
                    "SET is_archived = 0 " &
                    "WHERE id = @id AND is_archived = 1;"

                Using cmd As New SqlCommand(sql, con)
                    cmd.Parameters.Add(
                        "@id", SqlDbType.Int).Value = residentId

                    If cmd.ExecuteNonQuery() = 1 Then
                        MessageBox.Show(
                            "Resident restored successfully.",
                            "Restore Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
                    Else
                        MessageBox.Show(
                            "The resident was not found or is no longer archived.")
                    End If
                End Using
            End Using

            LoadResidents()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to restore resident: " & ex.Message,
                "Restore Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub frmResidentRecords_Load(
        sender As Object,
        e As EventArgs) Handles MyBase.Load

        ' Initial authorization and loading are handled by the constructor.
    End Sub

End Class
