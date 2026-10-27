Imports System
Imports System.Data
Imports System.Linq

Public Class CertificateRequestService

    Private ReadOnly _repository As New CertificateRequestRepository()

    Private Shared ReadOnly AllowedTypes As String() = {
        "Barangay Clearance",
        "Certificate of Residency",
        "Certificate of Indigency",
        "Business Clearance"
    }

    Public Sub SubmitRequest(
        firstName As String,
        lastName As String,
        birthdate As Date,
        certType As String,
        purpose As String)

        If String.IsNullOrWhiteSpace(firstName) Then
            Throw New ArgumentException("Enter your first name.")
        End If

        If String.IsNullOrWhiteSpace(lastName) Then
            Throw New ArgumentException("Enter your last name.")
        End If

        If birthdate.Date >= Date.Today Then
            Throw New ArgumentException("Enter a valid birthdate.")
        End If

        If Not AllowedTypes.Contains(certType) Then
            Throw New ArgumentException("Select a valid certificate type.")
        End If

        If String.IsNullOrWhiteSpace(purpose) Then
            Throw New ArgumentException("Enter the purpose of the certificate.")
        End If

        Dim residentId As Integer? =
            _repository.FindApprovedResidentId(
                firstName, lastName, birthdate)

        If Not residentId.HasValue Then
            Throw New InvalidOperationException(
                "We could not find an active, approved resident matching those details. " &
                "If you recently registered, please wait for barangay staff to approve your registration.")
        End If

        _repository.CreateRequest(residentId.Value, certType, purpose)
    End Sub

    Public Function GetAllRequests(
    requestStatus As String,
    search As String) As DataTable

        EnsureStaff()
        Return _repository.GetAllRequests(requestStatus, search)
    End Function

    Public Sub ReviewRequest(
        requestId As Integer,
        newStatus As String,
        rejectionReason As String)

        EnsureStaff()

        If newStatus = "Rejected" AndAlso
           String.IsNullOrWhiteSpace(rejectionReason) Then
            Throw New ArgumentException(
                "Enter a reason when rejecting a certificate request.")
        End If

        _repository.ReviewRequest(
            requestId,
            newStatus,
            AppSession.CurrentUser.Id,
            rejectionReason)
    End Sub

    Public Function IssueCertificate(
        requestId As Integer,
        orNumber As String,
        fee As Decimal) As String

        EnsureStaff()

        If String.IsNullOrWhiteSpace(orNumber) Then
            Throw New ArgumentException("Enter the official receipt number.")
        End If

        If fee < 0D Then
            Throw New ArgumentException("The fee cannot be negative.")
        End If

        Return _repository.IssueCertificate(
            requestId,
            orNumber,
            fee,
            AppSession.CurrentUser.Id)
    End Function

    Private Shared Sub EnsureStaff()
        If AppSession.CurrentUser Is Nothing OrElse
           Not AppSession.CurrentUser.IsActive OrElse
           (AppSession.CurrentUser.Role <> "Admin" AndAlso
            AppSession.CurrentUser.Role <> "Staff") Then

            Throw New UnauthorizedAccessException(
                "Only active, authorized barangay staff can process certificate requests.")
        End If
    End Sub


End Class
