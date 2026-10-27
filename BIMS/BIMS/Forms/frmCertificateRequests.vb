
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmCertificateRequests
    Inherits Form

    Private ReadOnly _service As New CertificateRequestService()

    Private ReadOnly txtSearch As New TextBox()
    Private ReadOnly cmbFilter As New ComboBox()
    Private ReadOnly grid As New DataGridView()
    Private ReadOnly txtReason As New TextBox()
    Private ReadOnly txtOrNumber As New TextBox()
    Private ReadOnly numFee As New NumericUpDown()

    Private ReadOnly btnRefresh As New Button()
    Private ReadOnly btnApprove As New Button()
    Private ReadOnly btnReject As New Button()
    Private ReadOnly btnIssue As New Button()

    Public Sub New()
        Me.Text = "BIMS - Certificate Requests and Issuance"
        Me.ClientSize = New Size(1100, 690)
        Me.MinimumSize = New Size(900, 600)
        Me.StartPosition = FormStartPosition.CenterParent

        If Not AppSession.IsLoggedIn OrElse
           (AppSession.CurrentUser.Role <> "Admin" AndAlso
            AppSession.CurrentUser.Role <> "Staff") Then

            Throw New UnauthorizedAccessException(
                "Only authorized barangay staff can access certificate requests.")
        End If

        BuildForm()
        LoadRequests()
    End Sub

    Private Sub BuildForm()
        Dim heading As New Label With {
            .Text = "Certificate Requests and Issuance",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .AutoSize = True
        }
        heading.SetBounds(20, 15, 700, 35)
        Me.Controls.Add(heading)

        AddLabel("Search", 20, 65, 65)
        txtSearch.SetBounds(85, 62, 230, 28)
        Me.Controls.Add(txtSearch)

        AddLabel("Status", 330, 65, 55)
        cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList
        cmbFilter.Items.AddRange({
            "All", "Pending", "Approved", "Rejected", "Issued"
        })
        cmbFilter.SelectedIndex = 0
        cmbFilter.SetBounds(385, 62, 150, 28)
        Me.Controls.Add(cmbFilter)

        btnRefresh.Text = "Refresh"
        btnRefresh.SetBounds(550, 60, 100, 32)
        Me.Controls.Add(btnRefresh)

        grid.SetBounds(20, 105, 1060, 300)
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or
                      AnchorStyles.Right
        Me.Controls.Add(grid)

        Dim actionHeading As New Label With {
            .Text = "Review Selected Request",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .AutoSize = True
        }
        actionHeading.SetBounds(20, 420, 400, 28)
        Me.Controls.Add(actionHeading)

        AddLabel("Rejection Reason", 20, 465, 125)
        txtReason.SetBounds(150, 462, 420, 55)
        txtReason.Multiline = True
        txtReason.ScrollBars = ScrollBars.Vertical
        Me.Controls.Add(txtReason)

        btnApprove.Text = "Approve Request"
        btnApprove.SetBounds(600, 455, 150, 38)
        Me.Controls.Add(btnApprove)

        btnReject.Text = "Reject Request"
        btnReject.SetBounds(770, 455, 150, 38)
        Me.Controls.Add(btnReject)

        Dim issueHeading As New Label With {
            .Text = "Issue Approved Certificate",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .AutoSize = True
        }
        issueHeading.SetBounds(20, 535, 400, 28)
        Me.Controls.Add(issueHeading)

        AddLabel("Official Receipt No.", 20, 575, 125)
        txtOrNumber.SetBounds(150, 572, 200, 28)
        Me.Controls.Add(txtOrNumber)

        AddLabel("Fee", 370, 575, 45)
        numFee.DecimalPlaces = 2
        numFee.Maximum = 1000000D
        numFee.Minimum = 0D
        numFee.ThousandsSeparator = True
        numFee.SetBounds(420, 572, 150, 28)
        Me.Controls.Add(numFee)

        btnIssue.Text = "Issue Certificate"
        btnIssue.SetBounds(600, 568, 180, 38)
        Me.Controls.Add(btnIssue)

        AddHandler txtSearch.TextChanged, Sub(sender, e) LoadRequests()
        AddHandler cmbFilter.SelectedIndexChanged, Sub(sender, e) LoadRequests()
        AddHandler btnRefresh.Click, Sub(sender, e) LoadRequests()
        AddHandler grid.SelectionChanged, AddressOf RequestSelected
        AddHandler btnApprove.Click, AddressOf ApproveRequest
        AddHandler btnReject.Click, AddressOf RejectRequest
        AddHandler btnIssue.Click, AddressOf IssueCertificate
    End Sub

    Private Sub AddLabel(caption As String,
                         x As Integer,
                         y As Integer,
                         width As Integer)

        Dim lbl As New Label With {
            .Text = caption,
            .AutoSize = False,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        lbl.SetBounds(x, y, width, 28)
        Me.Controls.Add(lbl)
    End Sub

    Private Sub LoadRequests()
        Try
            Dim status As String =
                If(cmbFilter.SelectedItem Is Nothing,
                   "All", cmbFilter.SelectedItem.ToString())

            grid.DataSource = _service.GetAllRequests(status, txtSearch.Text)


            If grid.Columns.Contains("First Name") Then
                grid.Columns("First Name").FillWeight = 90
            End If

            If grid.Columns.Contains("Last Name") Then
                grid.Columns("Last Name").FillWeight = 90
            End If

            If grid.Columns.Contains("Certificate Type") Then
                grid.Columns("Certificate Type").FillWeight = 140
            End If

            If grid.Columns.Contains("Purpose") Then
                grid.Columns("Purpose").FillWeight = 170
            End If

            If grid.Columns.Contains("Request Date") Then
                grid.Columns("Request Date").FillWeight = 110
            End If

            If grid.Columns.Contains("Status") Then
                grid.Columns("Status").FillWeight = 80
            End If

            If grid.Columns.Contains("Rejection Reason") Then
                grid.Columns("Rejection Reason").FillWeight = 130
            End If


            If grid.Columns.Contains("RequestID") Then
                grid.Columns("RequestID").Visible = False
            End If

            If grid.Columns.Contains("ResidentID") Then
                grid.Columns("ResidentID").Visible = False
            End If

            If grid.Columns.Contains("CertificateID") Then
                grid.Columns("CertificateID").Visible = False
            End If

            If grid.Columns.Contains("Birthdate") Then
                grid.Columns("Birthdate").DefaultCellStyle.Format = "yyyy-MM-dd"
            End If

            If grid.Columns.Contains("Request Date") Then
                grid.Columns("Request Date").DefaultCellStyle.Format =
                    "yyyy-MM-dd HH:mm"
            End If

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load certificate requests." &
                Environment.NewLine & ex.Message,
                "BIMS", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function SelectedRequestId() As Integer?
        If grid.CurrentRow Is Nothing OrElse
           Not grid.Columns.Contains("RequestID") Then
            Return Nothing
        End If

        Return Convert.ToInt32(grid.CurrentRow.Cells("RequestID").Value)
    End Function

    Private Function SelectedStatus() As String
        If grid.CurrentRow Is Nothing OrElse
           Not grid.Columns.Contains("Status") Then
            Return ""
        End If

        Return Convert.ToString(grid.CurrentRow.Cells("Status").Value)
    End Function

    Private Sub RequestSelected(sender As Object, e As EventArgs)
        Dim status As String = SelectedStatus()
        Dim isPending As Boolean = status = "Pending"
        Dim isApproved As Boolean = status = "Approved"

        btnApprove.Enabled = isPending
        btnReject.Enabled = isPending
        btnIssue.Enabled = isApproved

        If grid.CurrentRow IsNot Nothing AndAlso
           grid.Columns.Contains("Rejection Reason") Then
            txtReason.Text =
                Convert.ToString(grid.CurrentRow.Cells("Rejection Reason").Value)
        Else
            txtReason.Clear()
        End If
    End Sub

    Private Sub ApproveRequest(sender As Object, e As EventArgs)
        ReviewSelected("Approved")
    End Sub

    Private Sub RejectRequest(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtReason.Text) Then
            MessageBox.Show(
                "Enter a reason for rejecting the request.",
                "BIMS", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ReviewSelected("Rejected")
    End Sub

    Private Sub ReviewSelected(newStatus As String)
        Dim requestId As Integer? = SelectedRequestId()

        If Not requestId.HasValue Then
            MessageBox.Show("Select a request first.")
            Return
        End If

        If SelectedStatus() <> "Pending" Then
            MessageBox.Show("Only pending requests can be reviewed.")
            Return
        End If

        Try
            _service.ReviewRequest(
                requestId.Value, newStatus, txtReason.Text)

            MessageBox.Show(
                "Request " & newStatus.ToLowerInvariant() & " successfully.",
                "BIMS", MessageBoxButtons.OK, MessageBoxIcon.Information)

            LoadRequests()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Unable to Review Request",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub IssueCertificate(sender As Object, e As EventArgs)
        Dim requestId As Integer? = SelectedRequestId()

        If Not requestId.HasValue Then
            MessageBox.Show("Select an approved request first.")
            Return
        End If

        If SelectedStatus() <> "Approved" Then
            MessageBox.Show("Only approved requests can be issued.")
            Return
        End If

        If String.IsNullOrWhiteSpace(txtOrNumber.Text) Then
            MessageBox.Show("Enter the official receipt number.")
            Return
        End If

        Try
            Dim result As DialogResult = MessageBox.Show(
                "Issue the certificate for this request?",
                "Confirm Issuance",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

            If result <> DialogResult.Yes Then Return

            Dim controlNo As String =
                _service.IssueCertificate(
                    requestId.Value, txtOrNumber.Text, numFee.Value)

            MessageBox.Show(
                "Certificate issued successfully." &
                Environment.NewLine &
                "Control Number: " & controlNo,
                "Certificate Issued",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            txtOrNumber.Clear()
            numFee.Value = 0D
            LoadRequests()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Unable to Issue Certificate",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub frmCertificateRequests_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
