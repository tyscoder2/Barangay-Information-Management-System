
Public Class AuthenticationService

    Private ReadOnly _users As New UserRepository()

    Public Function Login(
        username As String,
        password As String
    ) As User

        If String.IsNullOrWhiteSpace(username) OrElse
           String.IsNullOrEmpty(password) Then
            Return Nothing
        End If

        Dim row = _users.GetCredentials(username.Trim())

        If row Is Nothing Then Return Nothing
        If Not CBool(row("is_active")) Then Return Nothing

        If Not PasswordService.VerifyPassword(
            password,
            CStr(row("password_salt")),
            CStr(row("password_hash"))
        ) Then
            Return Nothing
        End If

        Return New User With {
            .Id = CInt(row("id")),
            .Username = CStr(row("username")),
            .FullName = CStr(row("full_name")),
            .Role = CStr(row("role")),
            .IsActive = CBool(row("is_active"))
        }
    End Function

    Public Sub CreateFirstAdmin(
        username As String,
        password As String,
        fullName As String
    )
        If String.IsNullOrWhiteSpace(username) OrElse
           String.IsNullOrWhiteSpace(fullName) OrElse
           String.IsNullOrEmpty(password) Then
            Throw New ArgumentException(
                "All fields are required."
            )
        End If

        If username.Trim().Length > 50 OrElse
           fullName.Trim().Length > 100 Then
            Throw New ArgumentException(
                "Username or full name is too long."
            )
        End If

        If password.Length < 8 Then
            Throw New ArgumentException(
                "Password must contain at least 8 characters."
            )
        End If

        _users.CreateFirstAdmin(
            username.Trim(),
            password,
            fullName.Trim()
        )
    End Sub

End Class
