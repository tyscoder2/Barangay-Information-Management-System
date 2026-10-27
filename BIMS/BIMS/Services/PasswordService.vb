
Imports System.Security.Cryptography

Public Module PasswordService

    Public Function CreateSalt() As String
        Dim salt(15) As Byte
        RandomNumberGenerator.Fill(salt)
        Return Convert.ToBase64String(salt)
    End Function

    Public Function HashPassword(
        password As String,
        saltText As String
    ) As String

        Dim salt As Byte() = Convert.FromBase64String(saltText)

        Dim hash As Byte() =
            Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                600000,
                HashAlgorithmName.SHA256,
                32
            )

        Return Convert.ToBase64String(hash)
    End Function

    Public Function VerifyPassword(
        password As String,
        saltText As String,
        storedHash As String
    ) As Boolean

        Try
            Dim actual As Byte() =
                Convert.FromBase64String(
                    HashPassword(password, saltText)
                )

            Dim expected As Byte() =
                Convert.FromBase64String(storedHash)

            Return CryptographicOperations.FixedTimeEquals(
                actual, expected
            )

        Catch ex As FormatException
            Return False
        End Try
    End Function

End Module
