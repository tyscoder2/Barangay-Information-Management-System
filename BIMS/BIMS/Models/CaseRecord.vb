Imports System

Public Class CaseRecord
    Public Property Id As Integer
    Public Property CaseNo As String = ""
    Public Property RecordType As String = "Complaint"
    Public Property ComplainantName As String = ""
    Public Property RespondentName As String = ""
    Public Property IncidentDate As Date = Date.Today
    Public Property IncidentPlace As String = ""
    Public Property Nature As String = ""
    Public Property Narrative As String = ""
    Public Property Status As String = "Filed"
    Public Property Resolution As String = ""
    Public Property FiledBy As Integer
End Class