
Public Module AppSession

    Public Property CurrentUser As User

    Public ReadOnly Property IsLoggedIn As Boolean
        Get
            Return CurrentUser IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property IsAdmin As Boolean
        Get
            Return CurrentUser IsNot Nothing AndAlso
                CurrentUser.Role = "Admin"
        End Get
    End Property

    Public Sub Clear()
        CurrentUser = Nothing
    End Sub

End Module
