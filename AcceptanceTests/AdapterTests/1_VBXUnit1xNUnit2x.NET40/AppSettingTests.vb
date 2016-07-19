Imports NUnit.Framework
Imports Xunit

<TestFixture()>
Public Class AppSettingTests

    Private mAppSetting As AppSetting
    Private idval As String = "test234567"
    Private nameval As String = "test"

    <SetUp()>
    Public Sub SetUp()
        mAppSetting = New AppSetting(idval, nameval)
    End Sub

    <TearDown()>
    Public Sub TearDown()

        'nothing to do here
    End Sub

    <Test()>
    Public Sub id()
        NUnit.Framework.Assert.AreSame(mAppSetting.id, idval, "ID value loaded was " & mAppSetting.id & " and not the expected value of : " & idval)
    End Sub

    <Test()>
    Public Sub name()
        NUnit.Framework.Assert.AreSame(mAppSetting.name, nameval)
    End Sub

    <Fact()>
    Public Sub name2()
        Xunit.Assert.Equal(New AppSetting(idval, nameval).name, nameval)
    End Sub
End Class
