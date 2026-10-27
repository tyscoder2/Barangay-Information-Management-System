
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmResidentCaseSubmission
    Inherits Form

    Private ReadOnly _service As New CaseRecordService()

    Private ReadOnly cmbType As New ComboBox()
    Private ReadOnly txtName As New TextBox()
    Private ReadOnly txtRespondent As New TextBox()
    Private ReadOnly dtpIncident As New DateTimePicker()
    Private ReadOnly txtPlace As New TextBox()
    Private ReadOnly txtNature As New TextBox()
    Private ReadOnly txtNarrative As New TextBox()

    Public Sub New()
        Me.Text = "BIMS - Submit Complaint or Blotter"
        Me.ClientSize = New Size(650, 650)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(600, 600)

        BuildForm()
    End Sub

    Private Sub BuildForm()
        Dim heading As New Label With {
            .Text = "Submit a Complaint or Blotter",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .AutoSize = True
        }
        heading.SetBounds(20, 20, 600, 35)
        Me.Controls.Add(heading)

        Dim info As New Label With {
            .Text = "Complete the details below. Barangay staff will review your submission.",
            .AutoSize = False
        }
        info.SetBounds(22, 60, 600, 35)
        Me.Controls.Add(info)

        AddLabel("Submission Type", 22, 110)
        cmbType.DropDownStyle = ComboBoxStyle.DropDownList
        cmbType.Items.AddRange({"Complaint", "Blotter"})
        cmbType.SelectedIndex = 0
        cmbType.SetBounds(180, 110, 420, 28)
        Me.Controls.Add(cmbType)

        AddLabel("Your Full Name", 22, 155)
        txtName.SetBounds(180, 155, 420, 28)
        Me.Controls.Add(txtName)

        AddLabel("Respondent Name", 22, 200)
        txtRespondent.SetBounds(180, 200, 420, 28)
        Me.Controls.Add(txtRespondent)

        AddLabel("Incident Date", 22, 245)
        dtpIncident.Format = DateTimePickerFormat.Short
        dtpIncident.MaxDate = Date.Today
        dtpIncident.SetBounds(180, 245, 200, 28)
        Me.Controls.Add(dtpIncident)

        AddLabel("Incident Location", 22, 290)
        txtPlace.SetBounds(180, 290, 420, 28)
        Me.Controls.Add(txtPlace)

        AddLabel("Nature of Incident", 22, 335)
        txtNature.SetBounds(180, 335, 420, 28)
        Me.Controls.Add(txtNature)

        AddLabel("What Happened?", 22, 380)
        txtNarrative.Multiline = True
        txtNarrative.ScrollBars = ScrollBars.Vertical
        txtNarrative.SetBounds(180, 380, 420, 120)
        Me.Controls.Add(txtNarrative)

        Dim submit As New Button With {
            .Text = "Submit for Review",
            .Width = 190,
            .Height = 40
        }
        submit.SetBounds(180, 525, 190, 40)
        AddHandler submit.Click, AddressOf SubmitCase
        Me.Controls.Add(submit)

        Dim cancel As New Button With {
            .Text = "Cancel",
            .Width = 120,
            .Height = 40
        }
        cancel.SetBounds(390, 525, 120, 40)
        AddHandler cancel.Click, Sub(sender, e) Me.Close()
        Me.Controls.Add(cancel)
    End Sub

    Private Sub AddLabel(caption As String, x As Integer, y As Integer)
        Dim lbl As New Label With {
            .Text = caption,
            .AutoSize = False,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        lbl.SetBounds(x, y, 150, 28)
        Me.Controls.Add(lbl)
    End Sub

    Private Sub SubmitCase(sender As Object, e As EventArgs)
        Try
            Dim item As New CaseRecord With {
                .RecordType = cmbType.Text,
                .ComplainantName = txtName.Text.Trim(),
                .RespondentName = txtRespondent.Text.Trim(),
                .IncidentDate = dtpIncident.Value.Date,
                .IncidentPlace = txtPlace.Text.Trim(),
                .Nature = txtNature.Text.Trim(),
                .Narrative = txtNarrative.Text.Trim()
            }

            _service.SubmitResidentCase(item)

            MessageBox.Show(
                "Your submission has been recorded." &
                Environment.NewLine &
                "Reference number: " & item.CaseNo &
                Environment.NewLine &
                "Please keep this number for your records.",
                "Submission Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            Me.Close()

        Catch ex As Exception
            MessageBox.Show(
                ex.Message,
                "Unable to Submit",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub frmResidentCaseSubmission_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
