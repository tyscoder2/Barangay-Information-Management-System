
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmResidentCertificateRequest
    Inherits Form

    Private ReadOnly _service As New CertificateRequestService()

    Private ReadOnly txtFirstName As New TextBox()
    Private ReadOnly txtLastName As New TextBox()
    Private ReadOnly dtpBirthdate As New DateTimePicker()
    Private ReadOnly cmbCertificate As New ComboBox()
    Private ReadOnly txtPurpose As New TextBox()

    Public Sub New()
        Me.Text = "BIMS - Certificate Request"
        Me.ClientSize = New Size(650, 440)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(600, 400)

        BuildForm()
    End Sub

    Private Sub BuildForm()
        Dim heading As New Label With {
            .Text = "Request a Barangay Certificate",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .AutoSize = True
        }
        heading.SetBounds(22, 20, 600, 35)
        Me.Controls.Add(heading)

        Dim info As New Label With {
            .Text = "Requests are subject to verification and staff approval.",
            .AutoSize = True
        }
        info.SetBounds(24, 62, 600, 25)
        Me.Controls.Add(info)

        AddLabel("First Name", 25, 110)
        txtFirstName.SetBounds(175, 110, 420, 28)
        Me.Controls.Add(txtFirstName)

        AddLabel("Last Name", 25, 155)
        txtLastName.SetBounds(175, 155, 420, 28)
        Me.Controls.Add(txtLastName)

        AddLabel("Birthdate", 25, 200)
        dtpBirthdate.Format = DateTimePickerFormat.Short
        dtpBirthdate.MaxDate = Date.Today.AddDays(-1)
        dtpBirthdate.Value = Date.Today.AddYears(-18)
        dtpBirthdate.SetBounds(175, 200, 200, 28)
        Me.Controls.Add(dtpBirthdate)

        AddLabel("Certificate Type", 25, 245)
        cmbCertificate.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCertificate.Items.AddRange({
            "Barangay Clearance",
            "Certificate of Residency",
            "Certificate of Indigency",
            "Business Clearance"
        })
        cmbCertificate.SelectedIndex = 0
        cmbCertificate.SetBounds(175, 245, 420, 28)
        Me.Controls.Add(cmbCertificate)

        AddLabel("Purpose", 25, 290)
        txtPurpose.Multiline = True
        txtPurpose.ScrollBars = ScrollBars.Vertical
        txtPurpose.SetBounds(175, 290, 420, 65)
        Me.Controls.Add(txtPurpose)

        Dim submit As New Button With {
            .Text = "Submit Request",
            .Width = 170,
            .Height = 38
        }
        submit.SetBounds(175, 370, 170, 38)
        AddHandler submit.Click, AddressOf SubmitRequest
        Me.Controls.Add(submit)

        Dim cancel As New Button With {
            .Text = "Cancel",
            .Width = 120,
            .Height = 38
        }
        cancel.SetBounds(365, 370, 120, 38)
        AddHandler cancel.Click, Sub(sender, e) Me.Close()
        Me.Controls.Add(cancel)
    End Sub

    Private Sub AddLabel(caption As String, x As Integer, y As Integer)
        Dim lbl As New Label With {
            .Text = caption,
            .AutoSize = False,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        lbl.SetBounds(x, y, 140, 28)
        Me.Controls.Add(lbl)
    End Sub

    Private Sub SubmitRequest(sender As Object, e As EventArgs)
        Try
            _service.SubmitRequest(
                txtFirstName.Text,
                txtLastName.Text,
                dtpBirthdate.Value.Date,
                cmbCertificate.Text,
                txtPurpose.Text)

            MessageBox.Show(
                "Your certificate request has been submitted for staff review.",
                "Request Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            Me.Close()

        Catch ex As Exception
            MessageBox.Show(
                ex.Message,
                "Unable to Submit Request",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub frmResidentCertificateRequest_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
