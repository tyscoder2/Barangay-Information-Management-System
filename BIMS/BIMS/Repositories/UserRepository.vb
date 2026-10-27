
Imports Microsoft.Data.SqlClient

Public Class UserRepository

    Public Function CountUsers() As Integer
        Const sql As String = "SELECT COUNT(*) FROM dbo.users;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                con.Open()
                Return CInt(cmd.ExecuteScalar())
            End Using
        End Using
    End Function

    Public Sub CreateFirstAdmin(
        username As String,
        password As String,
        fullName As String
    )
        Dim salt As String = PasswordService.CreateSalt()
        Dim hash As String =
            PasswordService.HashPassword(password, salt)

        Using con = DatabaseConnection.CreateConnection()
            con.Open()

            Using tx = con.BeginTransaction()
                Try
                    Const sql As String =
                        "IF EXISTS (SELECT 1 FROM dbo.users) " &
                        "THROW 50001, 'An account already exists.', 1; " &
                        "INSERT INTO dbo.users " &
                        "(username, password_hash, password_salt, " &
                        "full_name, role, is_active) " &
                        "VALUES (@username, @hash, @salt, @name, " &
                        "'Admin', 1);"

                    Using cmd As New SqlCommand(sql, con, tx)
                        cmd.Parameters.AddWithValue("@username", username)
                        cmd.Parameters.AddWithValue("@hash", hash)
                        cmd.Parameters.AddWithValue("@salt", salt)
                        cmd.Parameters.AddWithValue("@name", fullName)
                        cmd.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    Public Function GetCredentials(
        username As String
    ) As DataRow

        Dim table As New DataTable()

        Const sql As String =
            "SELECT id, username, password_hash, password_salt, " &
            "full_name, role, is_active " &
            "FROM dbo.users WHERE username = @username;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@username", username)

                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(table)
                End Using
            End Using
        End Using

        If table.Rows.Count = 0 Then Return Nothing
        Return table.Rows(0)
    End Function

    Public Function GetAllAccounts() As DataTable
        Dim table As New DataTable()

        Const sql As String =
            "SELECT id AS [Account ID], " &
            "username AS [Username], " &
            "full_name AS [Full Name], " &
            "role AS [Role], " &
            "CASE WHEN is_active = 1 " &
            "THEN 'Active' ELSE 'Inactive' END AS [Account Status], " &
            "created_at AS [Date Created] " &
            "FROM dbo.users " &
            "ORDER BY role, username;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(table)
                End Using
            End Using
        End Using

        Return table
    End Function

    Public Sub CreateStaff(
        username As String,
        password As String,
        fullName As String
    )
        Dim salt As String = PasswordService.CreateSalt()
        Dim hash As String =
            PasswordService.HashPassword(password, salt)

        Const sql As String =
            "INSERT INTO dbo.users " &
            "(username, password_hash, password_salt, " &
            "full_name, role, is_active) " &
            "VALUES (@username, @hash, @salt, @name, 'Staff', 1);"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@username", username)
                cmd.Parameters.AddWithValue("@hash", hash)
                cmd.Parameters.AddWithValue("@salt", salt)
                cmd.Parameters.AddWithValue("@name", fullName)

                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub SetAccountActive(
        userId As Integer,
        isActive As Boolean
    )
        Using con = DatabaseConnection.CreateConnection()
            con.Open()

            Using tx = con.BeginTransaction()
                Try
                    If Not isActive Then
                        Const checkSql As String =
                            "SELECT role, is_active " &
                            "FROM dbo.users WITH (UPDLOCK, HOLDLOCK) " &
                            "WHERE id = @id;"

                        Dim accountRole As String = Nothing
                        Dim currentlyActive As Boolean = False

                        Using checkCmd As New SqlCommand(
                            checkSql, con, tx)

                            checkCmd.Parameters.AddWithValue("@id", userId)

                            Using reader = checkCmd.ExecuteReader()
                                If Not reader.Read() Then
                                    Throw New ArgumentException(
                                        "Account not found.")
                                End If

                                accountRole = CStr(reader("role"))
                                currentlyActive = CBool(reader("is_active"))
                            End Using
                        End Using

                        If accountRole = "Admin" AndAlso currentlyActive Then
                            Const countSql As String =
                                "SELECT COUNT(*) FROM dbo.users " &
                                "WHERE role = 'Admin' AND is_active = 1;"

                            Using countCmd As New SqlCommand(
                                countSql, con, tx)

                                If CInt(countCmd.ExecuteScalar()) <= 1 Then
                                    Throw New InvalidOperationException(
                                        "The last active Admin account " &
                                        "cannot be deactivated.")
                                End If
                            End Using
                        End If
                    End If

                    Const updateSql As String =
                        "UPDATE dbo.users " &
                        "SET is_active = @active " &
                        "WHERE id = @id;"

                    Using updateCmd As New SqlCommand(
                        updateSql, con, tx)

                        updateCmd.Parameters.AddWithValue(
                            "@active", isActive)
                        updateCmd.Parameters.AddWithValue("@id", userId)

                        If updateCmd.ExecuteNonQuery() = 0 Then
                            Throw New ArgumentException(
                                "Account not found.")
                        End If
                    End Using

                    tx.Commit()
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    Public Sub ResetPassword(
        userId As Integer,
        newPassword As String
    )
        Dim salt As String = PasswordService.CreateSalt()
        Dim hash As String =
            PasswordService.HashPassword(newPassword, salt)

        Const sql As String =
            "UPDATE dbo.users " &
            "SET password_hash = @hash, password_salt = @salt " &
            "WHERE id = @id;"

        Using con = DatabaseConnection.CreateConnection()
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@hash", hash)
                cmd.Parameters.AddWithValue("@salt", salt)
                cmd.Parameters.AddWithValue("@id", userId)

                con.Open()

                If cmd.ExecuteNonQuery() = 0 Then
                    Throw New ArgumentException("Account not found.")
                End If
            End Using
        End Using
    End Sub

End Class
