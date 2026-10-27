
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmResidentPortal
    Inherits Form

    Public Sub New()
        Me.Text = "BIMS - Resident Services"
        Me.ClientSize = New Size(700, 490)
        Me.MinimumSize = New Size(600, 450)
        Me.StartPosition = FormStartPosition.CenterScreen

        BuildForm()
    End Sub

    Private Sub BuildForm()
        Dim heading As New Label With {
            .Text = "Barangay Resident Services",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .AutoSize = True
        }
        heading.SetBounds(35, 25, 620, 45)
        Me.Controls.Add(heading)

        Dim description As New Label With {
            .Text = "Register or submit a request to your barangay.",
            .Font = New Font("Segoe UI", 10),
            .AutoSize = True
        }
        description.SetBounds(38, 78, 620, 25)
        Me.Controls.Add(description)

        AddMenuButton(
            "Register as a Resident",
            "Submit your resident information for approval.",
            35, 125,
            Sub() OpenChild(New frmResidentRegistration()))

        AddMenuButton(
            "Submit a Complaint / Blotter",
            "Submit an incident for barangay staff to review.",
            35, 220,
            Sub() OpenChild(New frmResidentCaseSubmission()))

        AddMenuButton(
            "Request a Certificate",
            "Submit a request for an eligible barangay certificate.",
            35, 315,
            Sub() OpenChild(New frmResidentCertificateRequest()))

        Dim backButton As New Button With {
            .Text = "Back to Welcome Screen",
            .Font = New Font("Segoe UI", 9),
            .Width = 200,
            .Height = 35
        }
        backButton.SetBounds(35, 420, 200, 35)
        AddHandler backButton.Click, Sub(sender, e) Me.Close()
        Me.Controls.Add(backButton)
    End Sub

    Private Sub AddMenuButton(caption As String,
                              description As String,
                              x As Integer,
                              y As Integer,
                              action As Action)

        Dim btn As New Button With {
            .Text = caption & Environment.NewLine & description,
            .Font = New Font("Segoe UI", 10),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Width = 620,
            .Height = 75
        }

        btn.SetBounds(x, y, 620, 75)
        AddHandler btn.Click, Sub(sender, e) action()
        Me.Controls.Add(btn)
    End Sub

    Private Sub OpenChild(childForm As Form)
        Using childForm
            childForm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub frmResidentPortal_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
