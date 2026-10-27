
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmAdminDashboard
    Inherits Form

    Public Sub New()
        Me.Text = "BIMS - Administration"
        Me.ClientSize = New Size(720, 640)
        Me.StartPosition = FormStartPosition.CenterScreen

        If Not AppSession.IsLoggedIn Then
            Throw New UnauthorizedAccessException(
                "Please log in first.")
        End If

        Dim heading As New Label With {
            .Text = "Barangay Information Management System",
            .Font = New Font("Segoe UI", 17, FontStyle.Bold),
            .AutoSize = True
        }
        heading.SetBounds(25, 20, 660, 40)
        Me.Controls.Add(heading)

        Dim welcome As New Label With {
            .Text = "Welcome, " & AppSession.CurrentUser.FullName &
                    " (" & AppSession.CurrentUser.Role & ")",
            .AutoSize = True
        }
        welcome.SetBounds(25, 65, 650, 25)
        Me.Controls.Add(welcome)

        AddMenuButton("Resident Approvals / All Records",
                      25, 115,
                      Sub() OpenResidents(Nothing))

        AddMenuButton("Purok 1", 25, 175,
                      Sub() OpenResidents("Purok 1"))

        AddMenuButton("Purok 2", 250, 175,
                      Sub() OpenResidents("Purok 2"))

        AddMenuButton("Purok 3", 475, 175,
                      Sub() OpenResidents("Purok 3"))

        AddMenuButton("Complaints and Blotters",
                      25, 250,
                      Sub() OpenChild(New frmCaseRecords()))

        AddMenuButton("Certificate Issuance",
                      250, 250,
                      Sub() OpenChild(New frmCertificateRequests()))

        AddMenuButton("Reports",
                      475, 250,
                      Sub() OpenChild(New frmReports()))

        ' Account management is available only to Admin users.
        If String.Equals(
            AppSession.CurrentUser.Role,
            "Admin",
            StringComparison.OrdinalIgnoreCase) Then

            AddMenuButton("User Account Management",
                          25, 325,
                          Sub() OpenChild(New frmUserManagement()))
        End If

        Dim logout As New Button With {
            .Text = "Logout",
            .Font = New Font("Segoe UI", 10)
        }
        logout.SetBounds(25, 405, 180, 42)

        AddHandler logout.Click,
            Sub(sender, e)
                Me.Close()
            End Sub

        Me.Controls.Add(logout)
    End Sub

    Private Sub AddMenuButton(
        caption As String,
        x As Integer,
        y As Integer,
        action As Action
    )
        Dim btn As New Button With {
            .Text = caption,
            .Font = New Font("Segoe UI", 10),
            .Width = 200,
            .Height = 48
        }

        btn.SetBounds(x, y, 200, 48)

        AddHandler btn.Click,
            Sub(sender, e)
                action()
            End Sub

        Me.Controls.Add(btn)
    End Sub

    Private Sub OpenResidents(purok As String)
        Using form As New frmResidentRecords(purok)
            form.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenChild(form As Form)
        Using form
            form.ShowDialog(Me)
        End Using
    End Sub

    Private Sub frmAdminDashboard_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

    End Sub

End Class
