Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmCaseRecords
    Inherits Form

    Private ReadOnly _service As New CaseRecordService()

    Private ReadOnly txtSearch As New TextBox()
    Private ReadOnly cmbFilterType As New ComboBox()
    Private ReadOnly cmbFilterStatus As New ComboBox()
    Private ReadOnly grid As New DataGridView()

    Private ReadOnly cmbType As New ComboBox()
    Private ReadOnly txtComplainant As New TextBox()
    Private ReadOnly txtRespondent As New TextBox()
    Private ReadOnly dtpIncident As New DateTimePicker()
    Private ReadOnly txtPlace As New TextBox()
    Private ReadOnly txtNature As New TextBox()
    Private ReadOnly txtNarrative As New TextBox()
    Private ReadOnly cmbStatus As New ComboBox()
    Private ReadOnly txtResolution As New TextBox()

    Private ReadOnly btnSave As New Button()
    Private ReadOnly btnUpdate As New Button()
    Private ReadOnly btnRefresh As New Button()

    Public Sub New()
        Me.Text = "BIMS - Complaints and Blotters"
        Me.ClientSize = New Size(1120, 760)
        Me.MinimumSize = New Size(900, 650)
        Me.StartPosition = FormStartPosition.CenterParent

        If Not AppSession.IsLoggedIn Then
            Throw New UnauthorizedAccessException("Please log in first.")
        End If

        BuildForm()
        LoadCases()
    End Sub

    Private Sub BuildForm()
        Dim heading As New Label With {
            .Text = "Complaints and Blotters",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .AutoSize = True
        }
        heading.SetBounds(20, 15, 500, 35)
        Me.Controls.Add(heading)

        AddLabel("Search", 20, 60, 70)
        txtSearch.SetBounds(90, 57, 240, 28)
        Me.Controls.Add(txtSearch)

        AddLabel("Type", 350, 60, 45)
        ConfigureCombo(cmbFilterType, {"All", "Complaint", "Blotter"})
        cmbFilterType.SetBounds(400, 57, 140, 28)
        Me.Controls.Add(cmbFilterType)

        AddLabel("Status", 555, 60, 55)
        ConfigureCombo(cmbFilterStatus, {
            "All", "Filed", "Mediation", "Settled", "Escalated", "Dismissed"
        })
        cmbFilterStatus.SetBounds(615, 57, 145, 28)
        Me.Controls.Add(cmbFilterStatus)

        btnRefresh.Text = "Refresh"
        btnRefresh.SetBounds(775, 55, 100, 32)
        Me.Controls.Add(btnRefresh)

        grid.SetBounds(20, 100, 1065, 250)
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
        grid.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or
                      AnchorStyles.Right
        Me.Controls.Add(grid)

        Dim entryHeading As New Label With {
            .Text = "File a New Case",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .AutoSize = True
        }
        entryHeading.SetBounds(20, 365, 300, 25)
        Me.Controls.Add(entryHeading)

        AddLabel("Record Type", 20, 405, 100)
        ConfigureCombo(cmbType, {"Complaint", "Blotter"})
        cmbType.SetBounds(125, 402, 180, 28)
        Me.Controls.Add(cmbType)

        AddLabel("Incident Date", 330, 405, 100)
        dtpIncident.Format = DateTimePickerFormat.Short
        dtpIncident.MaxDate = Date.Today
        dtpIncident.SetBounds(435, 402, 180, 28)
        Me.Controls.Add(dtpIncident)

        AddLabel("Complainant", 20, 445, 100)
        txtComplainant.SetBounds(125, 442, 250, 28)
        Me.Controls.Add(txtComplainant)

        AddLabel("Respondent", 390, 445, 100)
        txtRespondent.SetBounds(495, 442, 250, 28)
        Me.Controls.Add(txtRespondent)

        AddLabel("Incident Place", 20, 485, 100)
        txtPlace.SetBounds(125, 482, 620, 28)
        Me.Controls.Add(txtPlace)

        AddLabel("Nature", 20, 525, 100)
        txtNature.SetBounds(125, 522, 620, 28)
        Me.Controls.Add(txtNature)

        AddLabel("Narrative", 20, 565, 100)
        txtNarrative.Multiline = True
        txtNarrative.ScrollBars = ScrollBars.Vertical
        txtNarrative.SetBounds(125, 562, 620, 70)
        Me.Controls.Add(txtNarrative)

        btnSave.Text = "File Case"
        btnSave.SetBounds(125, 642, 150, 38)
        Me.Controls.Add(btnSave)

        Dim updateHeading As New Label With {
            .Text = "Update Selected Case",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .AutoSize = True
        }
        updateHeading.SetBounds(785, 365, 300, 25)
        Me.Controls.Add(updateHeading)

        AddLabel("New Status", 785, 405, 90)
        ConfigureCombo(cmbStatus, {
            "Filed", "Mediation", "Settled", "Escalated", "Dismissed"
        })
        cmbStatus.SetBounds(880, 402, 190, 28)
        Me.Controls.Add(cmbStatus)

        AddLabel("Resolution", 785, 445, 90)
        txtResolution.Multiline = True
        txtResolution.ScrollBars = ScrollBars.Vertical
        txtResolution.SetBounds(880, 442, 190, 100)
        Me.Controls.Add(txtResolution)

        btnUpdate.Text = "Update Case"
        btnUpdate.SetBounds(880, 555, 190, 38)
        Me.Controls.Add(btnUpdate)

        AddHandler txtSearch.TextChanged, Sub(sender, e) LoadCases()
        AddHandler cmbFilterType.SelectedIndexChanged, Sub(sender, e) LoadCases()
        AddHandler cmbFilterStatus.SelectedIndexChanged, Sub(sender, e) LoadCases()
        AddHandler btnRefresh.Click, Sub(sender, e) LoadCases()
        AddHandler btnSave.Click, AddressOf SaveCase
        AddHandler btnUpdate.Click, AddressOf UpdateCase
        AddHandler grid.SelectionChanged, AddressOf CaseSelected
    End Sub

    Private Sub AddLabel(caption As String, x As Integer, y As Integer, width As Integer)
        Dim lbl As New Label With {
            .Text = caption,
            .AutoSize = False,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        lbl.SetBounds(x, y, width, 28)
        Me.Controls.Add(lbl)
    End Sub

    Private Sub ConfigureCombo(combo As ComboBox, items As String())
        combo.DropDownStyle = ComboBoxStyle.DropDownList
        combo.Items.AddRange(items)
        combo.SelectedIndex = 0
    End Sub

    Private Sub LoadCases()
        Try
            Dim filterType As String =
                If(cmbFilterType.SelectedItem Is Nothing, "All",
                   cmbFilterType.SelectedItem.ToString())

            Dim filterStatus As String =
                If(cmbFilterStatus.SelectedItem Is Nothing, "All",
                   cmbFilterStatus.SelectedItem.ToString())

            grid.DataSource = _service.GetAll(
                txtSearch.Text, filterType, filterStatus)

            If grid.Columns.Contains("CaseID") Then
                grid.Columns("CaseID").Visible = False
            End If

            If grid.Columns.Contains("Incident Date") Then
                grid.Columns("Incident Date").DefaultCellStyle.Format = "yyyy-MM-dd"
            End If

            If grid.Columns.Contains("Narrative") Then
                grid.Columns("Narrative").AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            End If

            If grid.Columns.Contains("Resolution") Then
                grid.Columns("Resolution").AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            End If

        Catch ex As Exception
            MessageBox.Show("Unable to load case records." &
                            Environment.NewLine & ex.Message,
                            "BIMS", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SaveCase(sender As Object, e As EventArgs)
        Try
            Dim item As New CaseRecord With {
                .RecordType = cmbType.Text,
                .ComplainantName = txtComplainant.Text.Trim(),
                .RespondentName = txtRespondent.Text.Trim(),
                .IncidentDate = dtpIncident.Value.Date,
                .IncidentPlace = txtPlace.Text.Trim(),
                .Nature = txtNature.Text.Trim(),
                .Narrative = txtNarrative.Text.Trim(),
                .Resolution = ""
            }

            _service.Create(item)

            MessageBox.Show("Case filed successfully." &
                            Environment.NewLine & "Case number: " & item.CaseNo,
                            "BIMS", MessageBoxButtons.OK, MessageBoxIcon.Information)

            txtComplainant.Clear()
            txtRespondent.Clear()
            txtPlace.Clear()
            txtNature.Clear()
            txtNarrative.Clear()
            dtpIncident.Value = Date.Today

            LoadCases()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Unable to File Case",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CaseSelected(sender As Object, e As EventArgs)
        If grid.CurrentRow Is Nothing Then Return
        If Not grid.Columns.Contains("Status") Then Return

        Dim currentStatus As String =
            Convert.ToString(grid.CurrentRow.Cells("Status").Value)

        If cmbStatus.Items.Contains(currentStatus) Then
            cmbStatus.SelectedItem = currentStatus
        End If

        txtResolution.Text =
            Convert.ToString(grid.CurrentRow.Cells("Resolution").Value)
    End Sub

    Private Sub UpdateCase(sender As Object, e As EventArgs)
        If grid.CurrentRow Is Nothing OrElse
           Not grid.Columns.Contains("CaseID") Then

            MessageBox.Show("Select a case first.", "BIMS",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim caseId As Integer =
                Convert.ToInt32(grid.CurrentRow.Cells("CaseID").Value)

            _service.UpdateStatus(caseId, cmbStatus.Text, txtResolution.Text)

            MessageBox.Show("Case updated successfully.", "BIMS",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)

            LoadCases()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Unable to Update Case",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

End Class