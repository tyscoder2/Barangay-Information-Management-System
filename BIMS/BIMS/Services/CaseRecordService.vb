
Imports System.Data
Imports System.Linq

Public Class CaseRecordService

    Private ReadOnly _repository As New CaseRecordRepository()

    Public Function GetAll(search As String,
                           recordType As String,
                           caseStatus As String) As DataTable

        EnsureActiveStaff()

        Return _repository.GetAll(search, recordType, caseStatus)
    End Function

    Public Sub Create(item As CaseRecord)

        EnsureActiveStaff()

        If item Is Nothing Then
            Throw New ArgumentNullException(NameOf(item))
        End If

        If String.IsNullOrWhiteSpace(item.ComplainantName) Then
            Throw New ArgumentException("Enter the complainant's name.")
        End If

        If String.IsNullOrWhiteSpace(item.RespondentName) Then
            Throw New ArgumentException("Enter the respondent's name.")
        End If

        If String.IsNullOrWhiteSpace(item.IncidentPlace) Then
            Throw New ArgumentException("Enter the incident location.")
        End If

        If String.IsNullOrWhiteSpace(item.Nature) Then
            Throw New ArgumentException("Enter the nature of the case.")
        End If

        If String.IsNullOrWhiteSpace(item.Narrative) Then
            Throw New ArgumentException("Enter the incident narrative.")
        End If

        If item.IncidentDate.Date > Date.Today Then
            Throw New ArgumentException(
                "The incident date cannot be in the future.")
        End If

        If item.RecordType <> "Complaint" AndAlso
           item.RecordType <> "Blotter" Then
            Throw New ArgumentException(
                "Select Complaint or Blotter.")
        End If

        item.CaseNo =
            "CASE-" & DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")

        item.FiledBy = AppSession.CurrentUser.Id

        _repository.Create(item)
    End Sub

    Public Sub UpdateStatus(caseId As Integer,
                            newStatus As String,
                            resolution As String)

        EnsureActiveStaff()

        Dim allowedStatuses As String() = {
            "Filed",
            "Mediation",
            "Settled",
            "Escalated",
            "Dismissed"
        }

        If Not allowedStatuses.Contains(newStatus) Then
            Throw New ArgumentException(
                "Select a valid case status.")
        End If

        If newStatus = "Settled" AndAlso
           String.IsNullOrWhiteSpace(resolution) Then

            Throw New ArgumentException(
                "Enter the resolution before marking a case as Settled.")
        End If

        _repository.UpdateStatus(
            caseId,
            newStatus,
            resolution)
    End Sub

    Public Sub SubmitResidentCase(item As CaseRecord)

        If item Is Nothing Then
            Throw New ArgumentNullException(NameOf(item))
        End If

        If String.IsNullOrWhiteSpace(item.ComplainantName) Then
            Throw New ArgumentException("Enter your name.")
        End If

        If String.IsNullOrWhiteSpace(item.RespondentName) Then
            Throw New ArgumentException(
                "Enter the respondent's name.")
        End If

        If String.IsNullOrWhiteSpace(item.IncidentPlace) Then
            Throw New ArgumentException(
                "Enter the incident location.")
        End If

        If String.IsNullOrWhiteSpace(item.Nature) Then
            Throw New ArgumentException(
                "Enter the nature of the incident.")
        End If

        If String.IsNullOrWhiteSpace(item.Narrative) Then
            Throw New ArgumentException(
                "Describe what happened.")
        End If

        If item.IncidentDate.Date > Date.Today Then
            Throw New ArgumentException(
                "The incident date cannot be in the future.")
        End If

        If item.RecordType <> "Complaint" AndAlso
           item.RecordType <> "Blotter" Then
            Throw New ArgumentException(
                "Select Complaint or Blotter.")
        End If

        item.CaseNo =
            "CASE-" & DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")

        _repository.CreateResidentSubmission(item)
    End Sub

    Private Shared Sub EnsureActiveStaff()

        If AppSession.CurrentUser Is Nothing OrElse
           Not AppSession.CurrentUser.IsActive OrElse
           (AppSession.CurrentUser.Role <> "Admin" AndAlso
            AppSession.CurrentUser.Role <> "Staff") Then

            Throw New UnauthorizedAccessException(
                "Only active, authorized barangay staff can manage case records.")
        End If

    End Sub

End Class
