
Public Class UserManagementService

    Private ReadOnly _users As New UserRepository()

    Private Sub RequireAdmin()
        If AppSession.CurrentUser Is Nothing OrElse
           Not AppSession.CurrentUser.IsActive OrElse
           Not String.Equals(
               AppSession.CurrentUser.Role,
               "Admin",
               StringComparison.OrdinalIgnoreCase) Then

            Throw New UnauthorizedAccessException(
                "Only active Admin accounts can manage user accounts.")
        End If
    End Sub

    Public Function GetAllAccounts() As DataTable
        RequireAdmin()
        Return _users.GetAllAccounts()
    End Function

    Public Sub CreateStaff(
        username As String,
        fullName As String,
        password As String,
        confirmPassword As String
    )
        RequireAdmin()

        username = If(username, "").Trim()
        fullName = If(fullName, "").Trim()

        If username = "" OrElse fullName = "" OrElse
           String.IsNullOrEmpty(password) Then
            Throw New ArgumentException(
                "Username, full name, and password are required.")
        End If

        If username.Length > 50 OrElse fullName.Length > 100 Then
            Throw New ArgumentException(
                "Username cannot exceed 50 characters and full name cannot exceed 100.")
        End If

        If password.Length < 8 Then
            Throw New ArgumentException(
                "Password must contain at least 8 characters.")
        End If

        If password <> confirmPassword Then
            Throw New ArgumentException("Passwords do not match.")
        End If

        _users.CreateStaff(username, password, fullName)
    End Sub

    Public Sub SetAccountActive(userId As Integer, isActive As Boolean)
        RequireAdmin()

        If userId <= 0 Then
            Throw New ArgumentException("Select a valid account.")
        End If

        If AppSession.CurrentUser.Id = userId AndAlso Not isActive Then
            Throw New InvalidOperationException(
                "You cannot deactivate your own account while logged in.")
        End If

        _users.SetAccountActive(userId, isActive)
    End Sub

    Public Sub ResetPassword(
        userId As Integer,
        newPassword As String,
        confirmPassword As String
    )
        RequireAdmin()

        If userId <= 0 Then
            Throw New ArgumentException("Select a valid account.")
        End If

        If String.IsNullOrEmpty(newPassword) OrElse
           newPassword.Length < 8 Then
            Throw New ArgumentException(
                "The new password must contain at least 8 characters.")
        End If

        If newPassword <> confirmPassword Then
            Throw New ArgumentException("Passwords do not match.")
        End If

        _users.ResetPassword(userId, newPassword)
    End Sub

End Class
