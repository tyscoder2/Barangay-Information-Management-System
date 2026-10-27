
Imports Microsoft.Data.SqlClient

Public Class CertificateRequestRepository

    Public Function FindApprovedResidentId(
        firstName As String,
        lastName As String,
        birthdate As Date) As Integer?

        Const sql As String =
            "SELECT TOP 1 id FROM residents " &
            "WHERE first_name = @firstName " &
            "AND last_name = @lastName " &
            "AND CAST(birthdate AS date) = @birthdate " &
            "AND approval_status = 'Approved' " &
            "AND status = 'Active' " &
            "ORDER BY id;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@firstName", firstName.Trim())
                cmd.Parameters.AddWithValue("@lastName", lastName.Trim())
                cmd.Parameters.AddWithValue("@birthdate", birthdate.Date)

                con.Open()
                Dim result As Object = cmd.ExecuteScalar()

                If result Is Nothing OrElse result Is DBNull.Value Then
                    Return Nothing
                End If

                Return Convert.ToInt32(result)
            End Using
        End Using
    End Function

    Public Sub CreateRequest(
        residentId As Integer,
        certType As String,
        purpose As String)

        Const sql As String =
            "INSERT INTO certificate_requests " &
            "(resident_id, cert_type, purpose, request_status) " &
            "VALUES (@residentId, @certType, @purpose, 'Pending');"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@residentId", residentId)
                cmd.Parameters.AddWithValue("@certType", certType)
                cmd.Parameters.AddWithValue("@purpose", purpose.Trim())

                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Function GetAllRequests(
    Optional requestStatus As String = "All",
    Optional search As String = "") As DataTable

        Dim table As New DataTable()

        Const sql As String =
            "SELECT cr.id AS RequestID, cr.resident_id AS ResidentID, " &
            "r.first_name AS [First Name], r.last_name AS [Last Name], " &
            "r.birthdate AS Birthdate, cr.cert_type AS [Certificate Type], " &
            "cr.purpose AS Purpose, cr.request_date AS [Request Date], " &
            "cr.request_status AS Status, cr.rejection_reason AS [Rejection Reason], " &
            "cr.certificate_id AS CertificateID " &
            "FROM certificate_requests cr " &
            "INNER JOIN residents r ON r.id = cr.resident_id " &
            "WHERE (@status = 'All' OR cr.request_status = @status) " &
            "AND (@search = '' OR r.first_name LIKE @pattern " &
            "OR r.last_name LIKE @pattern OR cr.cert_type LIKE @pattern) " &
            "ORDER BY cr.request_date DESC, cr.id DESC;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@status", requestStatus)
                cmd.Parameters.AddWithValue("@search", search.Trim())
                cmd.Parameters.AddWithValue("@pattern", "%" & search.Trim() & "%")

                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(table)
                End Using
            End Using
        End Using

        Return table
    End Function

    Public Sub ReviewRequest(
    requestId As Integer,
    newStatus As String,
    reviewedBy As Integer,
    rejectionReason As String)

        If newStatus <> "Approved" AndAlso newStatus <> "Rejected" Then
            Throw New ArgumentException("Choose Approved or Rejected.")
        End If

        Const sql As String =
            "UPDATE certificate_requests " &
            "SET request_status = @status, reviewed_by = @reviewedBy, " &
            "reviewed_at = SYSDATETIME(), rejection_reason = @reason " &
            "WHERE id = @id AND request_status = 'Pending';"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@status", newStatus)
                cmd.Parameters.AddWithValue("@reviewedBy", reviewedBy)
                cmd.Parameters.AddWithValue("@reason",
                    If(String.IsNullOrWhiteSpace(rejectionReason),
                       CType(DBNull.Value, Object), rejectionReason.Trim()))
                cmd.Parameters.AddWithValue("@id", requestId)

                con.Open()

                If cmd.ExecuteNonQuery() = 0 Then
                    Throw New InvalidOperationException(
                        "This request is no longer pending. Refresh the list and try again.")
                End If
            End Using
        End Using
    End Sub

    Public Function IssueCertificate(
    requestId As Integer,
    orNumber As String,
    fee As Decimal,
    issuedBy As Integer) As String

        Dim controlNo As String =
            "CERT-" & DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")

        Using con = DatabaseConnection.CreateConnection()
            con.Open()

            Using tx = con.BeginTransaction()
                Try
                    Dim residentId As Integer
                    Dim certType As String
                    Dim purpose As String

                    Const selectSql As String =
                        "SELECT resident_id, cert_type, purpose " &
                        "FROM certificate_requests WITH (UPDLOCK, HOLDLOCK) " &
                        "WHERE id = @id AND request_status = 'Approved' " &
                        "AND certificate_id IS NULL;"

                    Using cmd As New SqlCommand(selectSql, con, tx)
                        cmd.Parameters.AddWithValue("@id", requestId)

                        Using reader = cmd.ExecuteReader()
                            If Not reader.Read() Then
                                Throw New InvalidOperationException(
                                    "Only approved, unissued requests can be issued.")
                            End If

                            residentId = Convert.ToInt32(reader("resident_id"))
                            certType = Convert.ToString(reader("cert_type"))
                            purpose = Convert.ToString(reader("purpose"))
                        End Using
                    End Using

                    Dim certificateId As Integer

                    Const insertSql As String =
                        "INSERT INTO certificates " &
                        "(control_no, resident_id, cert_type, purpose, or_number, " &
                        "fee, date_issued, issued_by) " &
                        "VALUES (@controlNo, @residentId, @certType, @purpose, " &
                        "@orNumber, @fee, SYSDATETIME(), @issuedBy); " &
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);"

                    Using cmd As New SqlCommand(insertSql, con, tx)
                        cmd.Parameters.AddWithValue("@controlNo", controlNo)
                        cmd.Parameters.AddWithValue("@residentId", residentId)
                        cmd.Parameters.AddWithValue("@certType", certType)
                        cmd.Parameters.AddWithValue("@purpose", purpose)
                        cmd.Parameters.AddWithValue("@orNumber", orNumber.Trim())
                        cmd.Parameters.AddWithValue("@fee", fee)
                        cmd.Parameters.AddWithValue("@issuedBy", issuedBy)

                        certificateId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    Const updateSql As String =
                        "UPDATE certificate_requests " &
                        "SET request_status = 'Issued', " &
                        "certificate_id = @certificateId, " &
                        "reviewed_by = @issuedBy, reviewed_at = SYSDATETIME() " &
                        "WHERE id = @id AND request_status = 'Approved' " &
                        "AND certificate_id IS NULL;"

                    Using cmd As New SqlCommand(updateSql, con, tx)
                        cmd.Parameters.AddWithValue("@certificateId", certificateId)
                        cmd.Parameters.AddWithValue("@issuedBy", issuedBy)
                        cmd.Parameters.AddWithValue("@id", requestId)

                        If cmd.ExecuteNonQuery() <> 1 Then
                            Throw New InvalidOperationException(
                                "The request could not be marked as issued.")
                        End If
                    End Using

                    tx.Commit()
                    Return controlNo

                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Function

End Class
