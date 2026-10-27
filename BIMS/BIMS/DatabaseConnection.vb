
Imports Microsoft.Data.SqlClient

Public Module DatabaseConnection

    Private ReadOnly ConnectionString As String =
        "Data Source=(localdb)\MSSQLLocalDB;" &
        "Initial Catalog=BaranggayInformationSystem;" &
        "Integrated Security=True;" &
        "TrustServerCertificate=True;"

    Public Function CreateConnection() As SqlConnection
        Return New SqlConnection(ConnectionString)
    End Function

    Public Function TestConnection() As Boolean
        Using connection As SqlConnection = CreateConnection()
            connection.Open()
            Return connection.State = ConnectionState.Open
        End Using
    End Function

End Module
