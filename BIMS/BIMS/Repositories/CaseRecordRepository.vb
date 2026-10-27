Imports Microsoft.Data.SqlClient

Public Class CaseRecordRepository

    Public Function GetAll(Optional search As String = "",
                           Optional recordType As String = "All",
                           Optional caseStatus As String = "All") As DataTable

        Dim table As New DataTable()

        Const sql As String =
            "SELECT id AS CaseID, case_no AS [Case Number], " &
            "record_type AS [Record Type], complainant_name AS Complainant, " &
            "respondent_name AS Respondent, incident_date AS [Incident Date], " &
            "incident_place AS [Incident Place], nature AS Nature, " &
            "narrative AS Narrative, status AS Status, resolution AS Resolution " &
            "FROM case_records " &
            "WHERE (@type = 'All' OR record_type = @type) " &
            "AND (@status = 'All' OR status = @status) " &
            "AND (@search = '' OR case_no LIKE @pattern " &
            "OR complainant_name LIKE @pattern " &
            "OR respondent_name LIKE @pattern " &
            "OR nature LIKE @pattern) " &
            "ORDER BY incident_date DESC, id DESC;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@type", recordType)
                cmd.Parameters.AddWithValue("@status", caseStatus)
                cmd.Parameters.AddWithValue("@search", search.Trim())
                cmd.Parameters.AddWithValue("@pattern", "%" & search.Trim() & "%")

                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(table)
                End Using
            End Using
        End Using

        Return table
    End Function

    Public Sub Create(item As CaseRecord)
        Const sql As String =
            "INSERT INTO case_records " &
            "(case_no, record_type, complainant_name, respondent_name, " &
            "incident_date, incident_place, nature, narrative, status, " &
            "resolution, filed_by) " &
            "VALUES (@caseNo, @type, @complainant, @respondent, " &
            "@incidentDate, @place, @nature, @narrative, 'Filed', NULLIF(@resolution, ''), @filedBy);"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@caseNo", item.CaseNo)
                cmd.Parameters.AddWithValue("@type", item.RecordType)
                cmd.Parameters.AddWithValue("@complainant", item.ComplainantName)
                cmd.Parameters.AddWithValue("@respondent", item.RespondentName)
                cmd.Parameters.AddWithValue("@incidentDate", item.IncidentDate.Date)
                cmd.Parameters.AddWithValue("@place", item.IncidentPlace)
                cmd.Parameters.AddWithValue("@nature", item.Nature)
                cmd.Parameters.AddWithValue("@narrative", item.Narrative)
                cmd.Parameters.AddWithValue("@resolution", item.Resolution)
                cmd.Parameters.AddWithValue("@filedBy", item.FiledBy)

                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub UpdateStatus(caseId As Integer, newStatus As String, resolution As String)
        Const sql As String =
            "UPDATE case_records " &
            "SET status = @status, resolution = NULLIF(@resolution, ''), " &
            "updated_at = SYSDATETIME() " &
            "WHERE id = @id;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@status", newStatus)
                cmd.Parameters.AddWithValue("@resolution", resolution.Trim())
                cmd.Parameters.AddWithValue("@id", caseId)

                con.Open()
                If cmd.ExecuteNonQuery() = 0 Then
                    Throw New InvalidOperationException("The selected case could not be found.")
                End If
            End Using
        End Using
    End Sub

    Public Sub CreateResidentSubmission(item As CaseRecord)
        Const sql As String =
            "INSERT INTO case_records " &
            "(case_no, record_type, complainant_name, respondent_name, " &
            "incident_date, incident_place, nature, narrative, status, " &
            "resolution, filed_by) " &
            "VALUES (@caseNo, @type, @complainant, @respondent, " &
            "@incidentDate, @place, @nature, @narrative, " &
            "'Filed', NULL, NULL);"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@caseNo", item.CaseNo)
                cmd.Parameters.AddWithValue("@type", item.RecordType)
                cmd.Parameters.AddWithValue("@complainant", item.ComplainantName)
                cmd.Parameters.AddWithValue("@respondent", item.RespondentName)
                cmd.Parameters.AddWithValue("@incidentDate", item.IncidentDate.Date)
                cmd.Parameters.AddWithValue("@place", item.IncidentPlace)
                cmd.Parameters.AddWithValue("@nature", item.Nature)
                cmd.Parameters.AddWithValue("@narrative", item.Narrative)

                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

End Class