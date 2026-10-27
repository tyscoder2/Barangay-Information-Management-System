
Imports Microsoft.Data.SqlClient
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmResidentRegistration
    Inherits Form

    Private ReadOnly fields As New Dictionary(Of String, TextBox)
    Private ReadOnly cmbPurok As New ComboBox()
    Private ReadOnly cmbSex As New ComboBox()
    Private ReadOnly cmbCivil As New ComboBox()
    Private ReadOnly dtBirthdate As New DateTimePicker()

    Public Sub New()
        Me.Text = "BIMS - Resident Registration"
        Me.ClientSize = New Size(500, 650)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.AutoScroll = True

        Dim heading As New Label With {
            .Text = "Resident Registration",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold)
        }
        heading.SetBounds(20, 15, 440, 35)
        Me.Controls.Add(heading)

        AddTextField("First name", 65)
        AddTextField("Middle name", 115)
        AddTextField("Last name", 165)
        AddTextField("Contact number", 215)
        AddTextField("Occupation", 265)
        AddTextField("Household number", 315)
        AddTextField("Street", 365)

        AddLabel("Birthdate", 415)
        dtBirthdate.SetBounds(200, 415, 250, 28)
        dtBirthdate.Format = DateTimePickerFormat.Short
        dtBirthdate.MaxDate = Date.Today
        dtBirthdate.Value = Date.Today.AddYears(-18)
        Me.Controls.Add(dtBirthdate)

        AddLabel("Sex", 455)
        cmbSex.SetBounds(200, 455, 250, 28)
        cmbSex.DropDownStyle = ComboBoxStyle.DropDownList
        cmbSex.Items.AddRange({"Male", "Female"})
        Me.Controls.Add(cmbSex)

        AddLabel("Civil status", 495)
        cmbCivil.SetBounds(200, 495, 250, 28)
        cmbCivil.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCivil.Items.AddRange({
            "Single", "Married", "Widowed",
            "Separated", "Annulled"
        })
        Me.Controls.Add(cmbCivil)

        AddLabel("Purok", 535)
        cmbPurok.SetBounds(200, 535, 250, 28)
        cmbPurok.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPurok.Items.AddRange({
            "Purok 1", "Purok 2", "Purok 3"
        })
        Me.Controls.Add(cmbPurok)

        Dim btnSubmit As New Button With {.Text = "Submit Registration"}
        btnSubmit.SetBounds(200, 580, 155, 35)
        AddHandler btnSubmit.Click, AddressOf SubmitClicked
        Me.Controls.Add(btnSubmit)
    End Sub

    Private Sub AddLabel(caption As String, top As Integer)
        Dim lbl As New Label With {
            .Text = caption,
            .AutoSize = True
        }
        lbl.SetBounds(20, top + 5, 170, 25)
        Me.Controls.Add(lbl)
    End Sub

    Private Sub AddTextField(caption As String, top As Integer)
        AddLabel(caption, top)

        Dim txt As New TextBox()
        txt.SetBounds(200, top, 250, 28)
        fields(caption) = txt
        Me.Controls.Add(txt)
    End Sub

    Private Function Field(caption As String) As String
        Return fields(caption).Text.Trim()
    End Function

    Private Sub SubmitClicked(sender As Object, e As EventArgs)
        If Field("First name") = "" OrElse
           Field("Last name") = "" OrElse
           Field("Household number") = "" OrElse
           cmbPurok.SelectedIndex < 0 OrElse
           cmbSex.SelectedIndex < 0 OrElse
           cmbCivil.SelectedIndex < 0 Then

            MessageBox.Show(
                "Complete the required fields and selections."
            )
            Return
        End If

        If Field("Household number").Length > 20 Then
            MessageBox.Show("Household number must be 20 characters or fewer.")
            Return
        End If

        If dtBirthdate.Value.Date >= Date.Today Then
            MessageBox.Show("Enter a valid birthdate.")
            Return
        End If

        Try
            Using con = DatabaseConnection.CreateConnection()
                con.Open()

                Using tx = con.BeginTransaction()
                    Try
                        Dim householdId As Integer
                        Dim householdPurok As String = Nothing

                        Const findHousehold As String =
                            "SELECT id, purok FROM households " &
                            "WHERE household_no = @number;"

                        Using cmd As New SqlCommand(
                            findHousehold, con, tx)

                            cmd.Parameters.AddWithValue(
                                "@number", Field("Household number"))

                            Using reader = cmd.ExecuteReader()
                                If reader.Read() Then
                                    householdId = CInt(reader("id"))
                                    householdPurok = CStr(reader("purok"))
                                End If
                            End Using
                        End Using

                        If householdId = 0 Then
                            Const insertHousehold As String =
                                "INSERT INTO households " &
                                "(household_no, purok, street) " &
                                "OUTPUT INSERTED.id " &
                                "VALUES (@number, @purok, @street);"

                            Using cmd As New SqlCommand(
                                insertHousehold, con, tx)

                                cmd.Parameters.AddWithValue(
                                    "@number", Field("Household number"))
                                cmd.Parameters.AddWithValue(
                                    "@purok", cmbPurok.Text)
                                cmd.Parameters.AddWithValue(
                                    "@street",
                                    If(Field("Street") = "",
                                       CType(DBNull.Value, Object),
                                       Field("Street")))

                                householdId = CInt(cmd.ExecuteScalar())
                            End Using
                        ElseIf householdPurok <> cmbPurok.Text Then
                            Throw New InvalidOperationException(
                                "That household number belongs to " &
                                householdPurok & ". Select the correct Purok.")
                        End If

                        Const insertResident As String =
                            "INSERT INTO residents " &
                            "(first_name, middle_name, last_name, " &
                            "birthdate, sex, civil_status, occupation, " &
                            "contact_no, household_id, approval_status, status) " &
                            "VALUES (@first, @middle, @last, @birth, " &
                            "@sex, @civil, @occupation, @contact, " &
                            "@household, 'Pending', 'Active');"

                        Using cmd As New SqlCommand(
                            insertResident, con, tx)

                            cmd.Parameters.AddWithValue(
                                "@first", Field("First name"))
                            cmd.Parameters.AddWithValue(
                                "@last", Field("Last name"))
                            cmd.Parameters.AddWithValue(
                                "@birth", dtBirthdate.Value.Date)
                            cmd.Parameters.AddWithValue(
                                "@sex", cmbSex.Text)
                            cmd.Parameters.AddWithValue(
                                "@civil", cmbCivil.Text)
                            cmd.Parameters.AddWithValue(
                                "@household", householdId)

                            cmd.Parameters.AddWithValue(
                                "@middle",
                                If(Field("Middle name") = "",
                                   CType(DBNull.Value, Object),
                                   Field("Middle name")))
                            cmd.Parameters.AddWithValue(
                                "@occupation",
                                If(Field("Occupation") = "",
                                   CType(DBNull.Value, Object),
                                   Field("Occupation")))
                            cmd.Parameters.AddWithValue(
                                "@contact",
                                If(Field("Contact number") = "",
                                   CType(DBNull.Value, Object),
                                   Field("Contact number")))

                            cmd.ExecuteNonQuery()
                        End Using

                        tx.Commit()

                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            MessageBox.Show(
                "Registration submitted successfully." &
                Environment.NewLine &
                "Your application is Pending staff approval.",
                "BIMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            Me.Close()

        Catch ex As Exception
            MessageBox.Show(
                ex.Message,
                "Registration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try
    End Sub

    Private Sub frmResidentRegistration_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
