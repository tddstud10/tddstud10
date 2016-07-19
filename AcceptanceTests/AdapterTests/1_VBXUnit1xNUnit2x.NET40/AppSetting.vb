Public Class AppSetting
    Private displayName As String
    Private settingID As String
    Public Sub New(ByVal id As String, ByVal name As String)
        displayName = name
        settingID = id
    End Sub
    ReadOnly Property name() As String
        Get
            Return displayName
        End Get
    End Property
    ReadOnly Property id() As String
        Get
            Return settingID
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return displayName
    End Function
End Class
