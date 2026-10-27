
Imports System.Drawing.Printing
Imports Microsoft.Data.SqlClient
Imports System.Data
Imports System.Drawing

Public Class frmReports

    Private ReadOnly grid As New DataGridView()
    Private ReadOnly cmbReport As New ComboBox()
    Private ReadOnly cmbPurok As New ComboBox()
    Private ReadOnly dtpFrom As New DateTimePicker()
    Private ReadOnly dtpTo As New DateTimePicker()
    Private ReadOnly btnGenerate As New Button()
    Private ReadOnly btnPreview As New Button()
    Private ReadOnly btnPrint As New Button()
    Private ReadOnly btnClose As New Button()
    Private ReadOnly lblCount As New Label()

    Private ReadOnly printDoc As New PrintDocument()
    Private printRow As Integer = 0
    Private reportTitle As String = "BIMS Report"

    Public Sub New()
        InitializeComponent()
        BuildForm()
    End Sub

    Private Sub BuildForm()
        Text = "Reports"
        StartPosition = FormStartPosition.CenterParent
        WindowState = FormWindowState.Maximized
        MinimumSize = New Size(850, 550)

        Dim topPanel As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 150,
            .Padding = New Padding(12)
        }

        Dim lblReport As New Label With {
            .Text = "Report Type",
            .Location = New Point(15, 15),
            .AutoSize = True
        }

        cmbReport.DropDownStyle = ComboBoxStyle.DropDownList
        cmbReport.Items.AddRange({
            "Resident Records",
            "Pending Resident Registrations",
            "Certificate Requests",
            "Issued Certificates",
            "Complaints and Blotters"
        })
        cmbReport.SelectedIndex = 0
        cmbReport.SetBounds(15, 38, 260, 30)

        Dim lblPurok As New Label With {
            .Text = "Purok Filter",
            .Location = New Point(295, 15),
            .AutoSize = True
        }

        cmbPurok.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPurok.Items.AddRange({"All", "Purok 1", "Purok 2", "Purok 3"})
        cmbPurok.SelectedIndex = 0
        cmbPurok.SetBounds(295, 38, 160, 30)

        Dim lblFrom As New Label With {
            .Text = "From Date",
            .Location = New Point(475, 15),
            .AutoSize = True
        }

        dtpFrom.Format = DateTimePickerFormat.Short
        dtpFrom.Value = Date.Today.AddMonths(-1)
        dtpFrom.SetBounds(475, 38, 125, 30)

        Dim lblTo As New Label With {
            .Text = "To Date",
            .Location = New Point(620, 15),
            .AutoSize = True
        }

        dtpTo.Format = DateTimePickerFormat.Short
        dtpTo.Value = Date.Today
        dtpTo.SetBounds(620, 38, 125, 30)

        ConfigureButton(btnGenerate, "Generate Report", 15, 85, 145)
        ConfigureButton(btnPreview, "Print Preview", 170, 85, 130)
        ConfigureButton(btnPrint, "Print / Save PDF", 310, 85, 145)
        ConfigureButton(btnClose, "Close", 465, 85, 90)

        AddHandler btnGenerate.Click, AddressOf GenerateClicked
        AddHandler btnPreview.Click, AddressOf PreviewClicked
        AddHandler btnPrint.Click, AddressOf PrintClicked
        AddHandler btnClose.Click, Sub(sender, e) Close()

        AddHandler cmbReport.SelectedIndexChanged,
            AddressOf ReportSelectionChanged

        topPanel.Controls.AddRange({
            lblReport, cmbReport, lblPurok,
            cmbPurok, lblFrom, dtpFrom,
            lblTo, dtpTo, btnGenerate,
            btnPreview, btnPrint, btnClose
        })

        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect = False
        grid.RowHeadersVisible = False
        grid.BackgroundColor = Color.White

        lblCount.Dock = DockStyle.Bottom
        lblCount.Height = 30
        lblCount.Padding = New Padding(12, 6, 0, 0)
        lblCount.Text = "Records: 0"

        Controls.Add(grid)
        Controls.Add(lblCount)
        Controls.Add(topPanel)

        printDoc.DefaultPageSettings.Landscape = True
        AddHandler printDoc.BeginPrint, AddressOf PrintStarted
        AddHandler printDoc.PrintPage, AddressOf PrintPage

        AddHandler Load, AddressOf ReportsLoaded

        ReportSelectionChanged(Nothing, EventArgs.Empty)
    End Sub

    Private Sub ConfigureButton(
        button As Button,
        caption As String,
        x As Integer,
        y As Integer,
        width As Integer)

        button.Text = caption
        button.SetBounds(x, y, width, 32)
    End Sub

    Private Function HasReportAccess() As Boolean
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

    Private Sub ReportsLoaded(sender As Object, e As EventArgs)
        If Not HasReportAccess() Then
            MessageBox.Show(
                "Only active authorized staff can access reports.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Close()
            Return
        End If

        GenerateReport()
    End Sub

    Private Sub ReportSelectionChanged(
        sender As Object,
        e As EventArgs)

        Dim selectedReport As String =
            If(cmbReport.SelectedItem Is Nothing,
               "",
               cmbReport.SelectedItem.ToString())

        Dim usesPurok As Boolean =
            selectedReport = "Resident Records" OrElse
            selectedReport = "Pending Resident Registrations"

        cmbPurok.Enabled = usesPurok

        Dim usesDates As Boolean =
            selectedReport = "Issued Certificates" OrElse
            selectedReport = "Complaints and Blotters" OrElse
            selectedReport = "Certificate Requests"

        dtpFrom.Enabled = usesDates
        dtpTo.Enabled = usesDates
    End Sub

    Private Sub GenerateClicked(sender As Object, e As EventArgs)
        GenerateReport()
    End Sub

    Private Sub GenerateReport()
        If Not HasReportAccess() Then
            MessageBox.Show(
                "Your session is no longer authorized to generate reports.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            grid.DataSource = Nothing
            lblCount.Text = "Records: 0"
            Return
        End If

        If dtpFrom.Value.Date > dtpTo.Value.Date Then
            MessageBox.Show(
                "The From Date cannot be later than the To Date.")
            Return
        End If

        If cmbReport.SelectedItem Is Nothing Then Return

        reportTitle = cmbReport.SelectedItem.ToString()

        Try
            Dim table As DataTable = LoadReportData()

            grid.DataSource = table

            For Each column As DataGridViewColumn In grid.Columns
                column.MinimumWidth = 80
                column.SortMode =
                    DataGridViewColumnSortMode.NotSortable
            Next

            If grid.Columns.Contains("Narrative") Then
                grid.Columns("Narrative").FillWeight = 220
            End If

            If grid.Columns.Contains("Purpose") Then
                grid.Columns("Purpose").FillWeight = 180
            End If

            If grid.Columns.Contains("Resolution") Then
                grid.Columns("Resolution").FillWeight = 180
            End If

            If grid.Columns.Contains("Archive Status") Then
                grid.Columns("Archive Status").FillWeight = 100
            End If

            If grid.Columns.Contains("Narrative") Then
                grid.Columns("Narrative").AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            End If

            If grid.Columns.Contains("Purpose") Then
                grid.Columns("Purpose").AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            End If

            lblCount.Text = $"Records: {table.Rows.Count}"

        Catch ex As UnauthorizedAccessException
            grid.DataSource = Nothing
            lblCount.Text = "Records: 0"

            MessageBox.Show(
                "Access denied. Please sign in with an authorized active account.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Close()

        Catch ex As Exception
            MessageBox.Show(
                "Could not generate the report." &
                Environment.NewLine & ex.Message,
                "Report Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function LoadReportData() As DataTable
        If Not HasReportAccess() Then
            Throw New UnauthorizedAccessException(
                "Only active authorized staff can access reports.")
        End If

        Dim sql As String = ""
        Dim table As New DataTable()

        Using connection As SqlConnection =
            DatabaseConnection.CreateConnection()

            Using command As New SqlCommand()
                command.Connection = connection

                Select Case reportTitle

                    Case "Resident Records"
                        sql =
                            "SELECT r.id AS [Resident ID], " &
                            "r.first_name AS [First Name], " &
                            "r.middle_name AS [Middle Name], " &
                            "r.last_name AS [Last Name], " &
                            "h.purok AS Purok, " &
                            "r.approval_status AS [Approval Status], " &
                            "r.status AS [Resident Status], " &
                            "CASE WHEN r.is_archived = 1 " &
                            "THEN 'Archived' ELSE 'Active' END " &
                            "AS [Archive Status], " &
                            "r.created_at AS [Date Registered] " &
                            "FROM dbo.residents r " &
                            "LEFT JOIN dbo.households h " &
                            "ON h.id = r.household_id " &
                            "WHERE (@purok = 'All' OR h.purok = @purok) " &
                            "ORDER BY r.is_archived, r.last_name, r.first_name;"

                        command.Parameters.Add(
                            "@purok", SqlDbType.NVarChar, 50).Value =
                            cmbPurok.SelectedItem.ToString()

                    Case "Pending Resident Registrations"
                        sql =
                            "SELECT r.id AS [Resident ID], " &
                            "r.first_name AS [First Name], " &
                            "r.middle_name AS [Middle Name], " &
                            "r.last_name AS [Last Name], " &
                            "h.purok AS Purok, " &
                            "r.approval_status AS [Approval Status], " &
                            "r.created_at AS [Date Registered] " &
                            "FROM dbo.residents r " &
                            "LEFT JOIN dbo.households h " &
                            "ON h.id = r.household_id " &
                            "WHERE r.approval_status = 'Pending' " &
                            "AND r.is_archived = 0 " &
                            "AND (@purok = 'All' OR h.purok = @purok) " &
                            "ORDER BY r.created_at;"

                        command.Parameters.Add(
                            "@purok", SqlDbType.NVarChar, 50).Value =
                            cmbPurok.SelectedItem.ToString()

                    Case "Certificate Requests"
                        sql =
                            "SELECT cr.id AS [Request ID], " &
                            "r.first_name AS [First Name], " &
                            "r.last_name AS [Last Name], " &
                            "cr.cert_type AS [Certificate Type], " &
                            "cr.purpose AS Purpose, " &
                            "cr.request_date AS [Request Date], " &
                            "cr.request_status AS Status, " &
                            "CASE WHEN r.is_archived = 1 " &
                            "THEN 'Archived' ELSE 'Active' END " &
                            "AS [Archive Status], " &
                            "cr.rejection_reason AS [Rejection Reason] " &
                            "FROM dbo.certificate_requests cr " &
                            "INNER JOIN dbo.residents r " &
                            "ON r.id = cr.resident_id " &
                            "WHERE cr.request_date >= @fromDate " &
                            "AND cr.request_date < DATEADD(day, 1, @toDate) " &
                            "ORDER BY cr.request_date DESC;"

                        AddDateParameters(command)

                    Case "Issued Certificates"
                        sql =
                            "SELECT c.control_no AS [Control Number], " &
                            "r.first_name AS [First Name], " &
                            "r.last_name AS [Last Name], " &
                            "c.cert_type AS [Certificate Type], " &
                            "c.purpose AS Purpose, " &
                            "c.or_number AS [OR Number], " &
                            "c.fee AS Fee, " &
                            "c.date_issued AS [Date Issued], " &
                            "CASE WHEN r.is_archived = 1 " &
                            "THEN 'Archived' ELSE 'Active' END " &
                            "AS [Archive Status] " &
                            "FROM dbo.certificates c " &
                            "INNER JOIN dbo.residents r " &
                            "ON r.id = c.resident_id " &
                            "WHERE c.date_issued >= @fromDate " &
                            "AND c.date_issued < DATEADD(day, 1, @toDate) " &
                            "ORDER BY c.date_issued DESC;"

                        AddDateParameters(command)

                    Case "Complaints and Blotters"
                        sql =
                            "SELECT case_no AS [Case Number], " &
                            "record_type AS [Record Type], " &
                            "complainant_name AS Complainant, " &
                            "respondent_name AS Respondent, " &
                            "incident_date AS [Incident Date], " &
                            "incident_place AS [Incident Place], " &
                            "nature AS Nature, status AS Status, " &
                            "resolution AS Resolution " &
                            "FROM dbo.case_records " &
                            "WHERE incident_date >= @fromDate " &
                            "AND incident_date < DATEADD(day, 1, @toDate) " &
                            "ORDER BY incident_date DESC;"

                        AddDateParameters(command)

                    Case Else
                        Throw New InvalidOperationException(
                            "Please select a valid report type.")
                End Select

                command.CommandText = sql
                connection.Open()

                Using adapter As New SqlDataAdapter(command)
                    adapter.Fill(table)
                End Using
            End Using
        End Using

        Return table
    End Function

    Private Sub AddDateParameters(command As SqlCommand)
        command.Parameters.Add("@fromDate", SqlDbType.Date).Value =
            dtpFrom.Value.Date

        command.Parameters.Add("@toDate", SqlDbType.Date).Value =
            dtpTo.Value.Date
    End Sub

    Private Sub PreviewClicked(sender As Object, e As EventArgs)
        If Not HasReportAccess() Then
            MessageBox.Show(
                "Your session is no longer authorized to print reports.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If grid.DataSource Is Nothing OrElse grid.Rows.Count = 0 Then
            MessageBox.Show("Generate a report with records first.")
            Return
        End If

        Try
            Using preview As New PrintPreviewDialog()
                preview.Document = printDoc
                preview.Width = 1000
                preview.Height = 700
                preview.ShowDialog(Me)
            End Using
        Catch ex As Exception
            MessageBox.Show("Print preview failed: " & ex.Message)
        End Try
    End Sub

    Private Sub PrintClicked(sender As Object, e As EventArgs)
        If Not HasReportAccess() Then
            MessageBox.Show(
                "Your session is no longer authorized to print reports.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If grid.DataSource Is Nothing OrElse grid.Rows.Count = 0 Then
            MessageBox.Show("Generate a report with records first.")
            Return
        End If

        Using dialog As New PrintDialog()
            dialog.Document = printDoc
            dialog.AllowSomePages = False
            dialog.UseEXDialog = True

            If dialog.ShowDialog(Me) = DialogResult.OK Then
                Try
                    printDoc.Print()
                Catch ex As Exception
                    MessageBox.Show("Printing failed: " & ex.Message)
                End Try
            End If
        End Using
    End Sub

    Private Sub PrintStarted(sender As Object, e As PrintEventArgs)
        printRow = 0
    End Sub

    Private Sub PrintPage(sender As Object, e As PrintPageEventArgs)
        If Not HasReportAccess() Then
            e.HasMorePages = False
            Return
        End If

        Dim left As Single = e.MarginBounds.Left
        Dim top As Single = e.MarginBounds.Top
        Dim y As Single = top

        Using titleFont As New Font("Arial", 16, FontStyle.Bold),
              reportFont As New Font("Arial", 12, FontStyle.Bold),
              headerFont As New Font("Arial", 8, FontStyle.Bold),
              bodyFont As New Font("Arial", 8),
              linePen As New Pen(Color.Black)

            e.Graphics.DrawString(
                "BARANGAY INFORMATION MANAGEMENT SYSTEM",
                titleFont, Brushes.Black, left, y)

            y += 30

            e.Graphics.DrawString(
                reportTitle,
                reportFont,
                Brushes.Black, left, y)

            y += 25

            e.Graphics.DrawString(
                "Generated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm") &
                "    Prepared by: " & AppSession.CurrentUser.FullName,
                bodyFont, Brushes.Black, left, y)

            y += 25

            e.Graphics.DrawLine(
                linePen, left, y, e.MarginBounds.Right, y)

            y += 8

            Dim table As DataTable =
                TryCast(grid.DataSource, DataTable)

            If table Is Nothing Then
                e.HasMorePages = False
                Return
            End If

            Dim visibleColumns As New List(Of DataGridViewColumn)

            For Each column As DataGridViewColumn In grid.Columns
                If column.Visible Then
                    visibleColumns.Add(column)
                End If
            Next

            Dim columnWidth As Single =
                e.MarginBounds.Width / Math.Max(1, visibleColumns.Count)

            For i As Integer = 0 To visibleColumns.Count - 1
                Dim header As String = visibleColumns(i).HeaderText

                Dim rect As New RectangleF(
                    left + i * columnWidth, y,
                    columnWidth - 4, 35)

                e.Graphics.DrawString(
                    header, headerFont, Brushes.Black, rect)
            Next

            y += 38

            e.Graphics.DrawLine(
                linePen, left, y, e.MarginBounds.Right, y)

            y += 6

            While printRow < table.Rows.Count
                If y + 28 > e.MarginBounds.Bottom Then
                    e.HasMorePages = True
                    Return
                End If

                For i As Integer = 0 To visibleColumns.Count - 1
                    Dim columnName As String =
                        visibleColumns(i).DataPropertyName

                    If String.IsNullOrWhiteSpace(columnName) Then
                        columnName = visibleColumns(i).Name
                    End If

                    Dim value As String = ""

                    If table.Columns.Contains(columnName) Then
                        Dim raw As Object =
                            table.Rows(printRow)(columnName)

                        If raw IsNot DBNull.Value Then
                            If TypeOf raw Is DateTime Then
                                value = DirectCast(raw, DateTime).
                                    ToString("yyyy-MM-dd")
                            Else
                                value = Convert.ToString(raw)
                            End If
                        End If
                    End If

                    value = value.Replace(vbCr, " ").Replace(vbLf, " ")

                    Dim rect As New RectangleF(
                        left + i * columnWidth, y,
                        columnWidth - 4, 28)

                    e.Graphics.DrawString(
                        value, bodyFont, Brushes.Black, rect)
                Next

                y += 30
                printRow += 1
            End While

            e.Graphics.DrawLine(
                linePen, left, y, e.MarginBounds.Right, y)

            e.Graphics.DrawString(
                "Total records: " & table.Rows.Count.ToString(),
                bodyFont, Brushes.Black, left, y + 8)

            e.HasMorePages = False
        End Using
    End Sub

    Private Sub frmReports_Load(
        sender As Object,
        e As EventArgs) Handles MyBase.Load

        ' Authorization is handled by ReportsLoaded.
    End Sub

End Class
