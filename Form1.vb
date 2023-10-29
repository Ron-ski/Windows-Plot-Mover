Option Strict On
Option Explicit On


Imports System.ComponentModel
Imports System.IO
Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Windows.Forms.VisualStyles
Imports Windows_Plot_Mover.My

Public Class Form1

    Dim StopMoving As Boolean = True
    Dim ActiveMoves As Decimal = 0

    Dim DriveSlots As Integer = 10

    Dim DriveBusy(DriveSlots) As Boolean
    Dim FileMoving(DriveSlots) As String
    Dim Destination(DriveSlots) As String
    Dim DriveDisabled(DriveSlots) As Boolean
    Dim Delete_Plots(DriveSlots) As Boolean
    Dim Nickname(DriveSlots) As String
    Dim DeleteLocation(DriveSlots) As String
    Dim DeletePlotsSize(DriveSlots) As Decimal
    Dim FreeDriveSpace(DriveSlots) As Decimal
    Dim ReservedSpace(DriveSlots) As Long
    Dim PlotterPaused As Boolean = False
    Dim LastDriveMovedTo As Integer = 0
    Dim Startup As Boolean = True           'Used in the form1 load to stop multiple calls to drivelocations routine when check boxes change
    Dim PlotType As String = "plot"
    Dim PlotDeleteType As String = "plot"
    Dim MyAppPath As String


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'Check if PlotMove exists

        MyAppPath = My.Application.Info.DirectoryPath

        If Not My.Computer.FileSystem.FileExists(MyAppPath & "\PlotMove.exe") Then
            MessageBox.Show("The PlotMove program is missing, it should be in the same folder as this program, please download it from Github. ", "PlotMove program missing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Me.Close()
        End If

        'This checks if its a new version, and moves across the settings
        'https://stackoverflow.com/questions/534261/how-do-you-keep-user-config-settings-across-different-assembly-versions-in-net

        If My.Settings.UpgradeRequired Then
            My.Settings.Upgrade()
            My.Settings.UpgradeRequired = False
            My.Settings.Save()
        End If


        txtDrive1Dest.Text = My.Settings.Drive1Dest
        txtDrive2Dest.Text = My.Settings.Drive2Dest
        txtDrive3Dest.Text = My.Settings.Drive3Dest
        txtDrive4Dest.Text = My.Settings.Drive4Dest
        txtDrive5Dest.Text = My.Settings.Drive5Dest
        txtDrive6Dest.Text = My.Settings.Drive6Dest
        txtDrive7Dest.Text = My.Settings.Drive7Dest
        txtDrive8Dest.Text = My.Settings.Drive8Dest
        txtDrive9Dest.Text = My.Settings.Drive9Dest
        txtDrive10Dest.Text = My.Settings.Drive10Dest

        txtDrive1DelPlotsFolder.Text = My.Settings.Drive1DelFolder
        txtDrive2DelPlotsFolder.Text = My.Settings.Drive2DelFolder
        txtDrive3DelPlotsFolder.Text = My.Settings.Drive3DelFolder
        txtDrive4DelPlotsFolder.Text = My.Settings.Drive4DelFolder
        txtDrive5DelPlotsFolder.Text = My.Settings.Drive5DelFolder
        txtDrive6DelPlotsFolder.Text = My.Settings.Drive6DelFolder
        txtDrive7DelPlotsFolder.Text = My.Settings.Drive7DelFolder
        txtDrive8DelPlotsFolder.Text = My.Settings.Drive8DelFolder
        txtDrive9DelPlotsFolder.Text = My.Settings.Drive9DelFolder
        txtDrive10DelPlotsFolder.Text = My.Settings.Drive10DelFolder

        'Changing the checked values triggers the change checked state code to run

        chkDisbleDrive1.Checked = My.Settings.Drive1CheckBox
        chkDisbleDrive2.Checked = My.Settings.Drive2CheckBox
        chkDisbleDrive3.Checked = My.Settings.Drive3CheckBox
        chkDisbleDrive4.Checked = My.Settings.Drive4CheckBox
        chkDisbleDrive5.Checked = My.Settings.Drive5CheckBox
        chkDisbleDrive6.Checked = My.Settings.Drive6CheckBox
        chkDisbleDrive7.Checked = My.Settings.Drive7CheckBox
        chkDisbleDrive8.Checked = My.Settings.Drive8CheckBox
        chkDisbleDrive9.Checked = My.Settings.Drive9CheckBox
        chkDisbleDrive10.Checked = My.Settings.Drive10CheckBox

        ChkDel_Plots1.Checked = My.Settings.Drive1Del
        ChkDel_Plots2.Checked = My.Settings.Drive2Del
        ChkDel_Plots3.Checked = My.Settings.Drive3Del
        ChkDel_Plots4.Checked = My.Settings.Drive4Del
        ChkDel_Plots5.Checked = My.Settings.Drive5Del
        ChkDel_Plots6.Checked = My.Settings.Drive6Del
        ChkDel_Plots7.Checked = My.Settings.Drive7Del
        ChkDel_Plots8.Checked = My.Settings.Drive8Del
        ChkDel_Plots9.Checked = My.Settings.Drive9Del
        ChkDel_Plots10.Checked = My.Settings.Drive10Del

        txtNickname1.Text = My.Settings.Drive1NickName
        txtNickname2.Text = My.Settings.Drive2NickName
        txtNickname3.Text = My.Settings.Drive3NickName
        txtNickname4.Text = My.Settings.Drive4NickName
        txtNickname5.Text = My.Settings.Drive5NickName
        txtNickname6.Text = My.Settings.Drive6NickName
        txtNickname7.Text = My.Settings.Drive7NickName
        txtNickname8.Text = My.Settings.Drive8NickName
        txtNickname9.Text = My.Settings.Drive9NickName
        txtNickname10.Text = My.Settings.Drive10NickName

        TxtReserved1.Text = My.Settings.ReservedSpace1.ToString
        TxtReserved2.Text = My.Settings.ReservedSpace2.ToString
        TxtReserved3.Text = My.Settings.ReservedSpace3.ToString
        TxtReserved4.Text = My.Settings.ReservedSpace4.ToString
        TxtReserved5.Text = My.Settings.ReservedSpace5.ToString
        TxtReserved6.Text = My.Settings.ReservedSpace6.ToString
        TxtReserved7.Text = My.Settings.ReservedSpace7.ToString
        TxtReserved8.Text = My.Settings.ReservedSpace8.ToString
        TxtReserved9.Text = My.Settings.ReservedSpace9.ToString
        TxtReserved10.Text = My.Settings.ReservedSpace10.ToString
        ComBoxPLotType.Text = My.Settings.PlotType.ToString
        ComBoxPlotDeleteType.Text = My.Settings.PlotDeleteType.ToString


        NumUpDownMaxMove.Value = My.Settings.MaxConsecMoves

        txtPlotSourceFolder.Text = My.Settings.PlotSourceFolder
        Me.Text = Me.Text & " (v " & Me.GetType.Assembly.GetName.Version.ToString() & ")"

        Startup = False

        UpdateArrays()
        UpdateNicknames()


        ListBox_PlotsBeingMoved.Items.Clear()
        ListBox_SizeofDeletePlots.Items.Clear()
        ListBox_SpaceofDrives.Items.Clear()


        For a = 1 To DriveSlots

            DriveBusy(a) = False
            FileMoving(a) = ""
            ListBox_PlotsBeingMoved.Items.Add("Drive " & a.ToString & " (" & Nickname(a) & ") not busy")
            ListBox_SpaceofDrives.Items.Add("0 GB of 0 GB")
            ListBox_SizeofDeletePlots.Items.Add("0 GB")

        Next

        Call SetallDriveSpace()
        Call DriveLocations()
        Call UpdateMovesList()
        Call UpdateReservedSpace()

        Dim MyFolder = txtPlotSourceFolder.Text

        If System.IO.Directory.Exists(MyFolder) Then

            LblMonitoring.Text = "Monitoring " & MyFolder & " for plots."
            GetListofPlots()                                                        'build the list of plots
            Timer_CheckandMove.Enabled = True                                       'start checking to see if moves have completed, update the plot list etc.
        Else

            If MyFolder = "" Then
                LblMonitoring.Text = "Not monitoring for plots - plots folder is not set up."
            Else
                LblMonitoring.Text = "Not monitoring for plots - " & MyFolder & " does not exist."
            End If

        End If

    End Sub

    Private Sub SetallDriveSpace()

        'Update the drive space lists and labels

        Dim PlotTmpFreeSpace As Long = CLng(DriveFreeSpace(txtPlotSourceFolder.Text))
        lblPlotterFreeSpace.Text = ForHumans(PlotTmpFreeSpace)

        SetDrive1Space()
        SetDrive2Space()
        SetDrive3Space()
        SetDrive4Space()
        SetDrive5Space()
        SetDrive6Space()
        SetDrive7Space()
        SetDrive8Space()
        SetDrive9Space()
        SetDrive10Space()

        Dim Reserved As String

        For a = 1 To DriveSlots

            If ReservedSpace(a) > 0 Then

                Reserved = ListBox_SpaceofDrives.Items(a - 1).ToString & " (" & ForHumans(ReservedSpace(a)) & " reserved)"
                ListBox_SpaceofDrives.Items.RemoveAt(a - 1)
                ListBox_SpaceofDrives.Items.Insert(a - 1, Reserved)

            End If
        Next
    End Sub

    Private Sub BtnPlotSourceFolder_Click(sender As Object, e As EventArgs) Handles btnPlotSourceFolder.Click

        'Sub for choosing the plot source folder and validating it

        Dim ChosenDirs As String = GetDriveLetters(0)
        Dim Folder = ChooseFolder("Please select the temporary plot storage folder.", "My PC")

        If Folder.Length > 0 Then
            If ChosenDirs.Contains(Folder.Substring(0, 1)) Then
                MessageBox.Show("Please select a diffrent plot source drive, drive already selected as a destination.", "Incorrect drive", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Folder = txtPlotSourceFolder.Text
            End If
        Else
            Folder = txtPlotSourceFolder.Text
        End If

        txtPlotSourceFolder.Text = Folder

    End Sub


    Private Function ChooseDelFolder(ByVal CurrentDir As String, ByVal MyDirectory As String) As String

        'Function for choosing dlete folder which should be on the same destination drive.

        Dim Folder As String = ChooseFolder("Please select a folder on the same drive as the destination.", MyDirectory)

        If Folder <> "" Then
            If Folder.Substring(0, 1) <> MyDirectory.Substring(0, 1) Then
                MessageBox.Show("Please select a folder on the same drive as the destination.", "Incorrect drive.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Folder = CurrentDir
            End If

        End If

        Return Folder

    End Function

    Private Function ChooseDestFolder(ByVal MyDirectory As String, ByVal ChosenDirs As String) As String

        'Function for selecting destination folder, and validating

        Dim Folder As String = ChooseFolder("Please select a local drive or folder", "This PC")

        If Folder = "" Then
            Folder = MyDirectory
        ElseIf Folder.Substring(0, 1) = ChosenDirs.Substring(0, 1) Then
            MessageBox.Show("Please select a diffrent destination drive, drive is selected as plot source drive.", "Incorrect drive", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Folder = MyDirectory
        ElseIf ChosenDirs.Contains(Folder.Substring(0, 1)) Then
            MessageBox.Show("Please select a diffrent destination drive, drive already selected", "Incorrect drive", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Folder = MyDirectory
        End If

        Return Folder

    End Function

    Private Function GetDriveLetters(ByVal DriveNumber As Integer) As String


        'Creates a string containing all the drive letters which have been added, except the one currently being added
        'The calling routing then checks to see if the selected drive has already been selected.

        Dim Letters As String = "!"

        If DriveNumber <> 0 Then
            If txtPlotSourceFolder.TextLength > 0 Then
                Letters += txtPlotSourceFolder.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 1 Then
            If txtDrive1Dest.TextLength > 0 Then
                Letters += txtDrive1Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 2 Then
            If txtDrive2Dest.TextLength > 0 Then
                Letters += txtDrive2Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 3 Then
            If txtDrive3Dest.TextLength > 0 Then
                Letters += txtDrive3Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 4 Then
            If txtDrive4Dest.TextLength > 0 Then
                Letters += txtDrive4Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 5 Then
            If txtDrive5Dest.TextLength > 0 Then
                Letters += txtDrive5Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 6 Then
            If txtDrive6Dest.TextLength > 0 Then
                Letters += txtDrive6Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 7 Then
            If txtDrive7Dest.TextLength > 0 Then
                Letters += txtDrive7Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 8 Then
            If txtDrive8Dest.TextLength > 0 Then
                Letters += txtDrive8Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 9 Then
            If txtDrive9Dest.TextLength > 0 Then
                Letters += txtDrive9Dest.Text.Substring(0, 1)
            End If
        End If

        If DriveNumber <> 10 Then
            If txtDrive10Dest.TextLength > 0 Then
                Letters += txtDrive10Dest.Text.Substring(0, 1)
            End If
        End If

        Return Letters

    End Function


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse01.Click

        Dim btn As String = CType(sender, Button).Name
        Dim DriveNumber = Val(btn.Substring(btn.Length - 2, 2))

        Dim ChosenDirs As String = GetDriveLetters(1)
        Dim MyText As String

        txtDrive1Dest.Text = ChooseDestFolder(txtDrive1Dest.Text, ChosenDirs)
        SetDrive1Space()
        Call UpdateDestinations()

        MyText = "Drive 1 (" & Nickname(0) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(0)
        ListBox_PlotsBeingMoved.Items.Insert(0, MyText)

        If txtDrive1DelPlotsFolder.TextLength > 0 Then
            If txtDrive1Dest.Text.Substring(0, 1) <> txtDrive1DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive1DelPlotsFolder.Text = ""
            End If
        End If

    End Sub


    Private Sub SetDrive1Space()


        FreeDriveSpace(1) = DriveFreeSpace(txtDrive1Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(1)) & " free of " & ForHumans(DriveSize(txtDrive1Dest.Text))

        lblDrive1_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(0)
        ListBox_SpaceofDrives.Items.Insert(0, MyText)

    End Sub

    Private Sub BtnDrive1Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive1Del_Browse.Click

        txtDrive1DelPlotsFolder.Text = ChooseDelFolder(txtDrive1DelPlotsFolder.Text, txtDrive1Dest.Text)
        Call UpdateDeletePlots()

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse02.Click

        Dim ChosenDirs As String = GetDriveLetters(2)
        Dim Mytext As String

        txtDrive2Dest.Text = ChooseDestFolder(txtDrive2Dest.Text, ChosenDirs)

        Call UpdateDestinations()
        SetDrive2Space()

        Mytext = "Drive 2 (" & Nickname(1) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(1)
        ListBox_PlotsBeingMoved.Items.Insert(1, Mytext)



        If txtDrive2DelPlotsFolder.TextLength > 0 Then
            If txtDrive2Dest.Text.Substring(0, 1) <> txtDrive2DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive2DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive2Space()

        FreeDriveSpace(2) = DriveFreeSpace(txtDrive2Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(2)) & " free of " & ForHumans(DriveSize(txtDrive2Dest.Text))

        lblDrive2_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(1)
        ListBox_SpaceofDrives.Items.Insert(1, MyText)


    End Sub

    Private Sub BtnDrive2Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive2Del_Browse.Click

        txtDrive2DelPlotsFolder.Text = ChooseDelFolder(txtDrive2DelPlotsFolder.Text, txtDrive2Dest.Text)
        Call UpdateDeletePlots()

    End Sub



    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse03.Click

        Dim ChosenDirs As String = GetDriveLetters(3)
        Dim MyText As String

        txtDrive3Dest.Text = ChooseDestFolder(txtDrive3Dest.Text, ChosenDirs)

        MyText = "Drive 3 (" & Nickname(2) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(2)
        ListBox_PlotsBeingMoved.Items.Insert(2, MyText)

        Call UpdateDestinations()
        Call SetDrive3Space()

        If txtDrive3DelPlotsFolder.TextLength > 0 Then
            If txtDrive3Dest.Text.Substring(0, 1) <> txtDrive3DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive3DelPlotsFolder.Text = ""
            End If
        End If


    End Sub

    Private Sub SetDrive3Space()

        FreeDriveSpace(3) = DriveFreeSpace(txtDrive3Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(3)) & " free of " & ForHumans(DriveSize(txtDrive3Dest.Text))

        lblDrive3_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(2)
        ListBox_SpaceofDrives.Items.Insert(2, MyText)

    End Sub

    Private Sub BtnDrive3Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive3Del_Browse.Click

        txtDrive3DelPlotsFolder.Text = ChooseDelFolder(txtDrive3DelPlotsFolder.Text, txtDrive3Dest.Text)
        Call UpdateDeletePlots()

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse04.Click

        Dim ChosenDirs As String = GetDriveLetters(4)
        Dim MyText As String

        txtDrive4Dest.Text = ChooseDestFolder(txtDrive4Dest.Text, ChosenDirs)

        MyText = "Drive 4 (" & Nickname(3) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(3)
        ListBox_PlotsBeingMoved.Items.Insert(3, MyText)

        Call UpdateDestinations()
        SetDrive4Space()

        If txtDrive4DelPlotsFolder.TextLength > 0 Then
            If txtDrive4Dest.Text.Substring(0, 1) <> txtDrive4DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive4DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive4Space()

        FreeDriveSpace(4) = DriveFreeSpace(txtDrive4Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(4)) & " free of " & ForHumans(DriveSize(txtDrive4Dest.Text))

        lblDrive4_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(3)
        ListBox_SpaceofDrives.Items.Insert(3, MyText)

    End Sub

    Private Sub BtnDrive4Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive4Del_Browse.Click

        txtDrive4DelPlotsFolder.Text = ChooseDelFolder(txtDrive4DelPlotsFolder.Text, txtDrive4Dest.Text)
        Call UpdateDeletePlots()

    End Sub


    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse05.Click

        Dim ChosenDirs As String = GetDriveLetters(5)
        Dim MyText As String

        txtDrive5Dest.Text = ChooseDestFolder(txtDrive5Dest.Text, ChosenDirs)

        MyText = "Drive 5 (" & Nickname(4) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(4)
        ListBox_PlotsBeingMoved.Items.Insert(4, MyText)

        Call UpdateDestinations()
        SetDrive5Space()

        If txtDrive5DelPlotsFolder.TextLength > 0 Then
            If txtDrive5Dest.Text.Substring(0, 1) <> txtDrive5DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive5DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive5Space()

        FreeDriveSpace(5) = DriveFreeSpace(txtDrive5Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(5)) & " free of " & ForHumans(DriveSize(txtDrive5Dest.Text))

        lblDrive5_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(4)
        ListBox_SpaceofDrives.Items.Insert(4, MyText)

    End Sub

    Private Sub BtnDrive5Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive5Del_Browse.Click

        txtDrive5DelPlotsFolder.Text = ChooseDelFolder(txtDrive5DelPlotsFolder.Text, txtDrive5Dest.Text)
        Call UpdateDeletePlots()

    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse06.Click

        Dim Drive = 6
        Dim ChosenDirs As String = GetDriveLetters(6)
        Dim MyText As String

        txtDrive6Dest.Text = ChooseDestFolder(txtDrive6Dest.Text, ChosenDirs)

        MyText = "Drive " & Drive.ToString & " (" & Nickname(Drive) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(Drive - 1)
        ListBox_PlotsBeingMoved.Items.Insert(Drive - 1, MyText)

        Call UpdateDestinations()
        SetDrive6Space()

        If txtDrive6DelPlotsFolder.TextLength > 0 Then
            If txtDrive6Dest.Text.Substring(0, 1) <> txtDrive6DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive6DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive6Space()

        FreeDriveSpace(6) = DriveFreeSpace(txtDrive6Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(6)) & " free of " & ForHumans(DriveSize(txtDrive6Dest.Text))

        lblDrive6_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(5)
        ListBox_SpaceofDrives.Items.Insert(5, MyText)

    End Sub

    Private Sub BtnDrive6Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive6Del_Browse.Click

        txtDrive6DelPlotsFolder.Text = ChooseDelFolder(txtDrive6DelPlotsFolder.Text, txtDrive6Dest.Text)
        Call UpdateDeletePlots()

    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse07.Click

        Dim Drive = 7
        Dim ChosenDirs As String = GetDriveLetters(7)
        Dim MyText As String

        txtDrive7Dest.Text = ChooseDestFolder(txtDrive7Dest.Text, ChosenDirs)

        MyText = "Drive " & Drive.ToString & " (" & Nickname(Drive) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(Drive - 1)
        ListBox_PlotsBeingMoved.Items.Insert(Drive - 1, MyText)

        Call UpdateDestinations()
        SetDrive7Space()

        If txtDrive7DelPlotsFolder.TextLength > 0 Then
            If txtDrive7Dest.Text.Substring(0, 1) <> txtDrive7DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive7DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive7Space()

        FreeDriveSpace(7) = DriveFreeSpace(txtDrive7Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(7)) & " free of " & ForHumans(DriveSize(txtDrive7Dest.Text))

        lblDrive7_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(6)
        ListBox_SpaceofDrives.Items.Insert(6, MyText)

    End Sub

    Private Sub BtnDrive7Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive7Del_Browse.Click

        txtDrive7DelPlotsFolder.Text = ChooseDelFolder(txtDrive7DelPlotsFolder.Text, txtDrive7Dest.Text)
        Call UpdateDeletePlots()

    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse08.Click

        Dim Drive = 8
        Dim ChosenDirs As String = GetDriveLetters(8)
        Dim MyText As String

        txtDrive8Dest.Text = ChooseDestFolder(txtDrive8Dest.Text, ChosenDirs)

        MyText = "Drive " & Drive.ToString & " (" & Nickname(Drive) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(Drive - 1)
        ListBox_PlotsBeingMoved.Items.Insert(Drive - 1, MyText)

        Call UpdateDestinations()
        SetDrive8Space()

        If txtDrive8DelPlotsFolder.TextLength > 0 Then
            If txtDrive8Dest.Text.Substring(0, 1) <> txtDrive8DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive8DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive8Space()

        FreeDriveSpace(8) = DriveFreeSpace(txtDrive8Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(8)) & " free of " & ForHumans(DriveSize(txtDrive8Dest.Text))

        lblDrive8_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(7)
        ListBox_SpaceofDrives.Items.Insert(7, MyText)

    End Sub

    Private Sub BtnDrive8Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive8Del_Browse.Click

        txtDrive8DelPlotsFolder.Text = ChooseDelFolder(txtDrive8DelPlotsFolder.Text, txtDrive8Dest.Text)
        Call UpdateDeletePlots()

    End Sub


    Private Sub Drive1CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles chkDisbleDrive1.CheckedChanged, chkDisbleDrive2.CheckedChanged, chkDisbleDrive3.CheckedChanged, chkDisbleDrive4.CheckedChanged, chkDisbleDrive5.CheckedChanged, chkDisbleDrive6.CheckedChanged, chkDisbleDrive7.CheckedChanged, chkDisbleDrive8.CheckedChanged, chkDisbleDrive9.CheckedChanged, chkDisbleDrive10.CheckedChanged

        Call DriveLocations()
        Call UpdateDriveDisabled()

    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse09.Click

        Dim Drive = 9
        Dim ChosenDirs As String = GetDriveLetters(9)
        Dim MyText As String

        txtDrive9Dest.Text = ChooseDestFolder(txtDrive9Dest.Text, ChosenDirs)

        MyText = "Drive " & Drive.ToString & " (" & Nickname(Drive) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(Drive - 1)
        ListBox_PlotsBeingMoved.Items.Insert(Drive - 1, MyText)

        Call UpdateDestinations()
        SetDrive9Space()

        If txtDrive9DelPlotsFolder.TextLength > 0 Then
            If txtDrive9Dest.Text.Substring(0, 1) <> txtDrive9DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive9DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive9Space()

        FreeDriveSpace(9) = DriveFreeSpace(txtDrive9Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(9)) & " free of " & ForHumans(DriveSize(txtDrive9Dest.Text))

        lblDrive9_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(8)
        ListBox_SpaceofDrives.Items.Insert(8, MyText)

    End Sub

    Private Sub BtnDrive9Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive9Del_Browse.Click

        txtDrive9DelPlotsFolder.Text = ChooseDelFolder(txtDrive9DelPlotsFolder.Text, txtDrive9Dest.Text)
        Call UpdateDeletePlots()

    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles DriveDest_Browse10.Click

        Dim Drive = 10
        Dim ChosenDirs As String = GetDriveLetters(10)
        Dim MyText As String

        txtDrive10Dest.Text = ChooseDestFolder(txtDrive10Dest.Text, ChosenDirs)

        MyText = "Drive " & Drive.ToString & " (" & Nickname(Drive) & ") not busy"
        ListBox_PlotsBeingMoved.Items.RemoveAt(Drive - 1)
        ListBox_PlotsBeingMoved.Items.Insert(Drive - 1, MyText)

        Call UpdateDestinations()
        SetDrive10Space()

        If txtDrive10DelPlotsFolder.TextLength > 0 Then
            If txtDrive10Dest.Text.Substring(0, 1) <> txtDrive10DelPlotsFolder.Text.Substring(0, 1) Then
                txtDrive10DelPlotsFolder.Text = ""
            End If
        End If

    End Sub

    Private Sub SetDrive10Space()

        FreeDriveSpace(10) = DriveFreeSpace(txtDrive10Dest.Text)
        Dim MyText = ForHumans(FreeDriveSpace(10)) & " free of " & ForHumans(DriveSize(txtDrive10Dest.Text))

        lblDrive10_Space.Text = MyText
        ListBox_SpaceofDrives.Items.RemoveAt(9)
        ListBox_SpaceofDrives.Items.Insert(9, MyText)

    End Sub

    Private Sub BtnDrive10Del_Browse_Click(sender As Object, e As EventArgs) Handles btnDrive10Del_Browse.Click

        txtDrive10DelPlotsFolder.Text = ChooseDelFolder(txtDrive10DelPlotsFolder.Text, txtDrive10Dest.Text)
        Call UpdateDeletePlots()

    End Sub

    Private Function ChooseFolder(ByVal Description As String, ByVal Root As String) As String

        'Validates and folder picker

        If Root = "" Then
            MessageBox.Show("Please select a destination folder first", "No destination set", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return ""
        End If

        Dim SelectedFolder As String
        FolderBrowserDialog1.Description = Description
        FolderBrowserDialog1.SelectedPath = Root
        Dim result As DialogResult = FolderBrowserDialog1.ShowDialog()
        SelectedFolder = FolderBrowserDialog1.SelectedPath

        If (result = DialogResult.Cancel) Then
            Return ""                      'Returns empty string for cancel
        End If

        If SelectedFolder.Substring(0, 2) = "\\" Then

            MessageBox.Show("Please select a local folder path only", "Network path selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            SelectedFolder = ""

        End If

        Return SelectedFolder

    End Function


    Private Sub DriveLocations()

        If Startup Then Return          'Form load sets the check boxes, and due to them changing this code gets called multiple times

        'Clears the settings if a drive is disabled.

        If chkDisbleDrive1.Checked Then


            DriveDest_Browse01.Enabled = False
            txtDrive1Dest.Enabled = False
            txtDrive1DelPlotsFolder.Enabled = False
            btnDrive1Del_Browse.Enabled = False
            ChkDel_Plots1.Enabled = False
            txtDrive1Dest.Text = ""
            txtDrive1DelPlotsFolder.Text = ""
            txtNickname1.Text = ""
            lblDrive1_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved1.Text = "0"

        Else

            DriveDest_Browse01.Enabled = True
            txtDrive1Dest.Enabled = True
            txtDrive1DelPlotsFolder.Enabled = True
            btnDrive1Del_Browse.Enabled = True
            ChkDel_Plots1.Enabled = True

        End If

        If chkDisbleDrive2.Checked Then

            DriveDest_Browse02.Enabled = False
            txtDrive2Dest.Enabled = False
            txtDrive2DelPlotsFolder.Enabled = False
            btnDrive2Del_Browse.Enabled = False
            ChkDel_Plots2.Enabled = False
            txtDrive2Dest.Text = ""
            txtDrive2DelPlotsFolder.Text = ""
            txtNickname2.Text = ""
            lblDrive2_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved2.Text = "0"

        Else

            DriveDest_Browse02.Enabled = True
            txtDrive2Dest.Enabled = True
            txtDrive2DelPlotsFolder.Enabled = True
            btnDrive2Del_Browse.Enabled = True
            ChkDel_Plots2.Enabled = True

        End If

        If chkDisbleDrive3.Checked Then

            DriveDest_Browse03.Enabled = False
            txtDrive3Dest.Enabled = False
            txtDrive3DelPlotsFolder.Enabled = False
            btnDrive3Del_Browse.Enabled = False
            ChkDel_Plots3.Enabled = False
            txtDrive3Dest.Text = ""
            txtDrive3DelPlotsFolder.Text = ""
            txtNickname3.Text = ""
            lblDrive3_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved3.Text = "0"

        Else

            DriveDest_Browse03.Enabled = True
            txtDrive3Dest.Enabled = True
            txtDrive3DelPlotsFolder.Enabled = True
            btnDrive3Del_Browse.Enabled = True
            ChkDel_Plots3.Enabled = True

        End If

        If chkDisbleDrive4.Checked Then

            DriveDest_Browse04.Enabled = False
            txtDrive4Dest.Enabled = False
            txtDrive4DelPlotsFolder.Enabled = False
            btnDrive4Del_Browse.Enabled = False
            ChkDel_Plots4.Enabled = False
            txtDrive4Dest.Text = ""
            txtDrive4DelPlotsFolder.Text = ""
            txtNickname4.Text = ""
            lblDrive4_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved4.Text = "0"

        Else

            DriveDest_Browse04.Enabled = True
            txtDrive4Dest.Enabled = True
            txtDrive4DelPlotsFolder.Enabled = True
            btnDrive4Del_Browse.Enabled = True
            ChkDel_Plots4.Enabled = True

        End If

        If chkDisbleDrive5.Checked Then

            DriveDest_Browse05.Enabled = False
            txtDrive5Dest.Enabled = False
            txtDrive5DelPlotsFolder.Enabled = False
            btnDrive5Del_Browse.Enabled = False
            ChkDel_Plots5.Enabled = False
            txtDrive5Dest.Text = ""
            txtDrive5DelPlotsFolder.Text = ""
            txtNickname5.Text = ""
            lblDrive5_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved5.Text = "0"

        Else

            DriveDest_Browse05.Enabled = True
            txtDrive5Dest.Enabled = True
            txtDrive5DelPlotsFolder.Enabled = True
            btnDrive5Del_Browse.Enabled = True
            ChkDel_Plots5.Enabled = True

        End If

        If chkDisbleDrive6.Checked Then


            DriveDest_Browse06.Enabled = False
            txtDrive6Dest.Enabled = False
            txtDrive6DelPlotsFolder.Enabled = False
            btnDrive6Del_Browse.Enabled = False
            ChkDel_Plots6.Enabled = False
            txtDrive6Dest.Text = ""
            txtDrive6DelPlotsFolder.Text = ""
            txtNickname6.Text = ""
            lblDrive6_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved6.Text = "0"

        Else

            DriveDest_Browse06.Enabled = True
            txtDrive6Dest.Enabled = True
            txtDrive6DelPlotsFolder.Enabled = True
            btnDrive6Del_Browse.Enabled = True
            ChkDel_Plots6.Enabled = True

        End If

        If chkDisbleDrive7.Checked Then

            DriveDest_Browse07.Enabled = False
            txtDrive7Dest.Enabled = False
            txtDrive7DelPlotsFolder.Enabled = False
            btnDrive7Del_Browse.Enabled = False
            ChkDel_Plots7.Enabled = False
            txtDrive7Dest.Text = ""
            txtDrive7DelPlotsFolder.Text = ""
            txtNickname7.Text = ""
            lblDrive7_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved7.Text = "0"

        Else

            DriveDest_Browse07.Enabled = True
            txtDrive7Dest.Enabled = True
            txtDrive7DelPlotsFolder.Enabled = True
            btnDrive7Del_Browse.Enabled = True
            ChkDel_Plots7.Enabled = True

        End If

        If chkDisbleDrive8.Checked Then

            DriveDest_Browse08.Enabled = False
            txtDrive8Dest.Enabled = False
            txtDrive8DelPlotsFolder.Enabled = False
            btnDrive8Del_Browse.Enabled = False
            ChkDel_Plots8.Enabled = False
            txtDrive8Dest.Text = ""
            txtDrive8DelPlotsFolder.Text = ""
            txtNickname8.Text = ""
            lblDrive8_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved8.Text = "0"

        Else

            DriveDest_Browse08.Enabled = True
            txtDrive8Dest.Enabled = True
            txtDrive8DelPlotsFolder.Enabled = True
            btnDrive8Del_Browse.Enabled = True
            ChkDel_Plots8.Enabled = True

        End If

        If chkDisbleDrive9.Checked Then

            DriveDest_Browse09.Enabled = False
            txtDrive9Dest.Enabled = False
            txtDrive9DelPlotsFolder.Enabled = False
            btnDrive9Del_Browse.Enabled = False
            ChkDel_Plots9.Enabled = False
            txtDrive9Dest.Text = ""
            txtDrive9DelPlotsFolder.Text = ""
            txtNickname9.Text = ""
            lblDrive9_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved9.Text = "0"

        Else

            DriveDest_Browse09.Enabled = True
            txtDrive9Dest.Enabled = True
            txtDrive9DelPlotsFolder.Enabled = True
            btnDrive9Del_Browse.Enabled = True
            ChkDel_Plots9.Enabled = True

        End If

        If chkDisbleDrive10.Checked Then

            DriveDest_Browse10.Enabled = False
            txtDrive10Dest.Enabled = False
            txtDrive10DelPlotsFolder.Enabled = False
            btnDrive10Del_Browse.Enabled = False
            ChkDel_Plots10.Enabled = False
            txtDrive10Dest.Text = ""
            txtDrive10DelPlotsFolder.Text = ""
            txtNickname10.Text = ""
            lblDrive10_Space.Text = "0 bytes free of 0 bytes"
            TxtReserved10.Text = "0"

        Else

            DriveDest_Browse10.Enabled = True
            txtDrive10Dest.Enabled = True
            txtDrive10DelPlotsFolder.Enabled = True
            btnDrive10Del_Browse.Enabled = True
            ChkDel_Plots10.Enabled = True

        End If

    End Sub

    Private Function DriveFreeSpace(ByVal DrivePath As String) As Decimal

        'Returns the freespace on the disk.
        Try

            If DrivePath = "" Then
                Return 0
            End If

            If Not System.IO.Directory.Exists(DrivePath) Then
                Return 0
            End If

            Return ((New DriveInfo(DrivePath).AvailableFreeSpace))

        Catch ex As Exception

            'MessageBox.Show(String.Format("Error in DriveFreeSpace: {0}", ex.Message))
            ExceptionThrown("DriveFreeSpace", String.Format("{0}" & ex.Message))

            Return 0
        End Try

    End Function

    Private Function DriveSize(ByVal DrivePath As String) As Decimal

        'Returned the size of the disk

        If DrivePath = "" Then
            Return 0
        End If

        If Not System.IO.Directory.Exists(DrivePath) Then
            Return 0
        End If

        Return ((New DriveInfo(DrivePath).TotalSize))


    End Function

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles BtnStartMovingPlots.Click

        'Start moving plots button, validate the plots source directory

        If txtPlotSourceFolder.Text = "" Then
            MessageBox.Show("Please setup a plot source drive.", "No plot source drive set", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        ElseIf Not System.IO.Directory.Exists(txtPlotSourceFolder.Text) Then
            MessageBox.Show("Please select a valid plot source drive.", "Plot source drive invalid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        StopMoving = False

        BtnStopMovingPlots.Enabled = True
        BtnStartMovingPlots.Enabled = False
        Timer_CheckandMove.Interval = 250

    End Sub
    Private Sub GetListofPlots()

        'This refreshes the list of plots to be moved.

        Try

            If txtPlotSourceFolder.Text = "" Then
                Return
            ElseIf Not System.IO.Directory.Exists(txtPlotSourceFolder.Text) Then
                Return
            End If

            Dim strFileSize As String
            Dim di As New IO.DirectoryInfo(txtPlotSourceFolder.Text)
            Dim aryFi As IO.FileInfo() = di.GetFiles("*." & PlotType)
            Dim fi As IO.FileInfo


            ListBox_Plots2Move.BeginUpdate()        'Stops the list being redrawn whilst updated
            ListBox_SizeOfPlots.BeginUpdate()

            ListBox_Plots2Move.Items.Clear()
            ListBox_SizeOfPlots.Items.Clear()

            For Each fi In aryFi

                strFileSize = (Math.Round(fi.Length / 1024 / 1024 / 1024, 2)).ToString()
                ListBox_Plots2Move.Items.Add(fi.FullName)
                ListBox_SizeOfPlots.Items.Add(strFileSize)

            Next

            ListBox_Plots2Move.EndUpdate()      'Redraws the list
            ListBox_SizeOfPlots.EndUpdate()

        Catch ex As Exception

            'MessageBox.Show(String.Format("Error in GetListofPlots: {0}", ex.Message))
            ExceptionThrown("GetListofPlots", String.Format("{0}" & ex.Message))

        End Try

    End Sub


    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles BtnStopMovingPlots.Click

        StopMoving = True
        Call UpdateStatus()
        BtnStopMovingPlots.Enabled = False
        BtnStartMovingPlots.Enabled = True

    End Sub



    Private Function DeletePlots(PlotFullPathName As String, Destination As String, DriveNumber As Integer) As Integer

        'We pass the name of the plot we are going to move, and the drive number for the destination

        'First we need to check if we are deleting plots

        Dim DriveFull As Boolean

        If Delete_Plots(DriveNumber) = False Then

            ListBox_PlotsBeingMoved.Items.RemoveAt(DriveNumber - 1)
            ListBox_PlotsBeingMoved.Items.Insert(DriveNumber - 1, "Drive " & DriveNumber.ToString & " (" & Nickname(DriveNumber) & ")" & " is full")
            Return 0

        End If


        'So we are deleting plots on the drive

        'Get the size of the plot we are planning on moving

        Dim myFile As New FileInfo(PlotFullPathName)
        Dim PlotSizeInBytes As Long = CLng(myFile.Length + ReservedSpace(DriveNumber))

        'Get the list of files which can be deleted

        'Check delete folder exits

        If Not System.IO.Directory.Exists(DeleteLocation(DriveNumber)) Then
            ListBox_PlotsBeingMoved.Items.RemoveAt(DriveNumber - 1)
            ListBox_PlotsBeingMoved.Items.Insert(DriveNumber - 1, "Drive " & DriveNumber.ToString & " (" & Nickname(DriveNumber) & ") is full - delete folder does not exist")
            Return 0
        End If

        Dim di As New IO.DirectoryInfo(DeleteLocation(DriveNumber))
        Dim aryFi As IO.FileInfo() = di.GetFiles("*." & PlotDeleteType)
        Dim fi As IO.FileInfo

        DriveFull = True 'if we make it all the way through the for next loop the drive is full


        For Each fi In aryFi

            If System.IO.File.Exists(fi.FullName) = True Then
                Try
                    System.IO.File.Delete(fi.FullName)              'we crash here if the plot is locked
                Catch ex As Exception

                    ExceptionThrown("DeletePlots", ex.Message)

                End Try


            End If

            'Now we need to check if we have enough free space
            'Get the free space on the destination drive

            Dim FreeSpace = New DriveInfo(Destination).AvailableFreeSpace

            If FreeSpace > PlotSizeInBytes Then
                DriveFull = False
                Exit For
            End If

        Next

        Call SetallDriveSpace()

        If DriveFull = True Then
            ListBox_PlotsBeingMoved.Items.RemoveAt(DriveNumber - 1)
            ListBox_PlotsBeingMoved.Items.Insert(DriveNumber - 1, "Drive " & DriveNumber.ToString & " (" & Nickname(DriveNumber) & ") is full")
            Return 0
        End If

        Return 1 ' We have enough space

    End Function

    Private Sub ExceptionThrown(Mysub As String, MyException As String)

        Dim Gridrow As Integer = 0
        Dim Exists As Boolean = False

        For Each Row As DataGridViewRow In ErrorGridView.Rows
            Dim cell As DataGridViewCell = (ErrorGridView.Rows(Gridrow).Cells(2))
            If cell.Value.ToString = MyException Then
                Exists = True
                Exit For
            End If
            Gridrow += 1

        Next

        If Exists Then
            Dim count As Int32
            count = CInt(ErrorGridView.Rows(Gridrow).Cells(0).Value) + 1
            ErrorGridView.Rows(Gridrow).Cells(0).Value = count
        Else
            ErrorGridView.Rows.Add("1", Mysub, MyException)
        End If

        TabCtrl.TabPages(2).Text = "Error Exceptions (" & ErrorGridView.RowCount.ToString & ")"


    End Sub

    Private Sub Btn_ClearErrors_Click(sender As Object, e As EventArgs) Handles Btn_ClearErrors.Click

        ErrorGridView.Rows.Clear()
        ErrorGridView.Columns.Clear()
        TabCtrl.TabPages(2).Text = "Error Exceptions (0)"

    End Sub

    Private Sub ListBox_Errors_DoubleClick(sender As Object, e As EventArgs)

        'For x = 0 To ListBox_Errors.Items.Count - 1
        'If ListBox_Errors.SelectedItem Then
        ' Next

        ' Dim SelectedError As String = ListBox_Errors.SelectedItems.ToString
        ' My.Computer.Clipboard.SetText(clip)

        ' MsgBox()

    End Sub




    Private Sub UpdateMovesList()

        'This section checks to see if moves have been completed, and updates things if they have

        Dim MyText As String

        For a = 1 To DriveSlots

            If DriveBusy(a) Then

                'If the file being moved no longer exists then the move has completed or been cancelled.

                If Not (System.IO.File.Exists(FileMoving(a))) Then
                    DriveBusy(a) = False
                    FileMoving(a) = ""
                    MyText = "Drive " & a.ToString & " (" & Nickname(a) & ") not busy"
                    ListBox_PlotsBeingMoved.Items.RemoveAt(a - 1)
                    ListBox_PlotsBeingMoved.Items.Insert(a - 1, MyText)

                End If

            End If

            If Destination(a) = "" Then
                MyText = "Drive " & a.ToString & " location not setup"
                ListBox_PlotsBeingMoved.Items.RemoveAt(a - 1)
                ListBox_PlotsBeingMoved.Items.Insert(a - 1, MyText)
            ElseIf Not System.IO.Directory.Exists(Destination(a)) Then
                MyText = "Drive " & a.ToString & " (" & Nickname(a) & ") location not valid"
                ListBox_PlotsBeingMoved.Items.RemoveAt(a - 1)
                ListBox_PlotsBeingMoved.Items.Insert(a - 1, MyText)
            End If

        Next

    End Sub
    Private Sub Timer_CheckMovesCompleted_Tick(sender As Object, e As EventArgs) Handles Timer_CheckandMove.Tick

        Timer_CheckandMove.Enabled = False
        Timer_CheckandMove.Interval = 5000

        'This section checks to see if the file has been moved

        UpdateArrays()               'Updates destination, drive disabled arrays etc. bit not Nicknames, as they are updated when changed.
        UpdateSizeofDeletePlots()    'Update list of size of delete plots and deleteplots size array
        UpdateMovesList()            'Update the list of moves
        GetListofPlots()             'Refresh the list of plots
        SetallDriveSpace()           'Update drive space

        'This section updates the list to show whether a drive is setup, available, full or not, otherwise its not done until we try and move a plot to that drive

        Dim MyText As String = ""

        For a = 1 To DriveSlots

            If ListBox_Plots2Move.Items.Count > 0 Then

                If DriveBusy(a) = False Then   'if the drive is busy we can ignore it

                    If Destination(a) = "" Then 'is the drive destination blank

                        If Nickname(a) = "" Then
                            MyText = "Drive " & a.ToString & " Location is not setup"
                        Else
                            MyText = "Drive " & a.ToString & " (" & Nickname(a) & ")" & " is not setup"
                        End If


                    ElseIf Not System.IO.Directory.Exists(Destination(a)) Then 'is the drive accessible

                        MyText = "Drive " & a.ToString & " (" & Nickname(a) & ")" & " location not valid"

                    Else

                        Dim PlotFullPathName As String = ListBox_Plots2Move.Items(0).ToString

                        If My.Computer.FileSystem.FileExists(PlotFullPathName) Then

                            Dim SpaceRequired As Long = CLng(FileLen(PlotFullPathName)) 'get the size of the plot

                            If FreeDriveSpace(a) + DeletePlotsSize(a) - ReservedSpace(a) - SpaceRequired < 0 Then  'The drive doesn't have enough space

                                MyText = "Drive " & a.ToString & " (" & Nickname(a) & ")" & " is full"

                            Else ' the drive isn't busy, it exists and it isn't full

                                MyText = "Drive " & a.ToString & " (" & Nickname(a) & ")" & " is not busy"

                            End If
                        End If


                    End If

                    ListBox_PlotsBeingMoved.Items.RemoveAt(a - 1)
                    ListBox_PlotsBeingMoved.Items.Insert(a - 1, MyText)

                End If
            End If

        Next


        If StopMoving = False Then                          'if true then don't move anymore plots

            'DriveNumber(a) is set to order the drives so plots are moved evenly across the available drives.

            Dim DriveNumber(DriveSlots) As Integer
            Dim DriveCounter As Integer
            DriveCounter = LastDriveMovedTo

            For a = 1 To DriveSlots

                DriveCounter += 1
                If DriveCounter > DriveSlots Then DriveCounter = 1
                DriveNumber(a) = DriveCounter

            Next a

            'Now we'll organise moving some plots

            For a = 1 To DriveSlots
                If DriveBusy(DriveNumber(a)) = False Then
                    If DriveDisabled(DriveNumber(a)) = False Then
                        If ListBox_Plots2Move.Items.Count > 0 Then
                            MoveNextPlot(DriveNumber(a))                    'we can move a plot to drive A as long as there is enough space
                        End If

                    Else
                        ListBox_PlotsBeingMoved.Items.RemoveAt(DriveNumber(a) - 1)
                        ListBox_PlotsBeingMoved.Items.Insert(DriveNumber(a) - 1, "Drive " & DriveNumber(a).ToString & " disabled")
                    End If
                End If
            Next a

        End If

        Call UpdateStatus()

        Timer_CheckandMove.Enabled = True


    End Sub

    Private Sub MoveNextPlot(ByVal DriveNumber As Integer)

        'Another part of the program will decide the drive number to use - supplied in DriveNumber
        'And also will ensure there is a plot ready move
        'And there is suffcient space on the drive
        'This section just triggers the move, sets the drive as busy and updates the label etc.

        Dim PlotFullPathName As String = ListBox_Plots2Move.Items(0).ToString

        'We need to check if the drive is valid

        If Destination(DriveNumber) = "" Then
            ListBox_PlotsBeingMoved.Items.RemoveAt(DriveNumber - 1)
            ListBox_PlotsBeingMoved.Items.Insert(DriveNumber - 1, "Drive " & DriveNumber.ToString & " Location is not setup")
            Return
        End If

        If Not System.IO.Directory.Exists(Destination(DriveNumber)) Then
            ListBox_PlotsBeingMoved.Items.RemoveAt(DriveNumber - 1)
            ListBox_PlotsBeingMoved.Items.Insert(DriveNumber - 1, "Drive " & DriveNumber.ToString & " location not valid")
            Return
        End If

        'Check if we have the maximum consecutive moves

        Call UpdateStatus()

        If ActiveMoves >= NumUpDownMaxMove.Value Then Return

        'Check if PlotMove.exe exists - can't move plots without it!
        'This is also checked at startup so unlikely to be missing when we get here!

        If Not My.Computer.FileSystem.FileExists(MyAppPath & "\PlotMove.exe") Then
            MessageBox.Show("The PlotMove program is missing, it should be in the same folder as this program, please download it from Github. This program will close.", "PlotMove program missing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Me.Close()
        End If


        'We need to check if there is enough space.

        If My.Computer.FileSystem.FileExists(PlotFullPathName) Then

            Dim SpaceRequired As Long = CLng(FileLen(PlotFullPathName) + ReservedSpace(DriveNumber))
            Dim FreeSpace As Long = New DriveInfo(Destination(DriveNumber)).AvailableFreeSpace

            If FreeSpace - SpaceRequired < 0 Then
                'there is not enough space - should we make space or disable the drive, and mark it full?

                Dim MyResults = DeletePlots(PlotFullPathName, Destination(DriveNumber), DriveNumber)
                If MyResults = 0 Then Return 'We don't have enough freespace

            End If

            ListBox_PlotsBeingMoved.Items.RemoveAt(DriveNumber - 1)
            ListBox_PlotsBeingMoved.Items.Insert(DriveNumber - 1, "Drive " & DriveNumber.ToString & " (" & Nickname(DriveNumber) & ") busy moving " & PlotFullPathName & " to " & Destination(DriveNumber))
            DriveBusy(DriveNumber) = True

            'Now we move the plot

            Dim NewPlotName As String
            Dim PlotName As String = System.IO.Path.GetFileName(PlotFullPathName)
            Dim DriveNumberText As String

            'Convert the drive number to a two digit text string

            If DriveNumber < 10 Then
                DriveNumberText = "0" & DriveNumber.ToString
            Else
                DriveNumberText = DriveNumber.ToString
            End If

            NewPlotName = PlotName & ".mov" & DriveNumberText

            My.Computer.FileSystem.RenameFile(PlotFullPathName, NewPlotName)

            NewPlotName = PlotFullPathName & ".mov" & DriveNumberText


            FileMoving(DriveNumber) = NewPlotName 'This will be monitored to know when the move has finished - deleted

            Process.Start(My.Application.Info.DirectoryPath + "\PlotMove.exe", Destination(DriveNumber) & "/" & Nickname(DriveNumber) & "/" & NewPlotName)

            LastDriveMovedTo = DriveNumber

        End If

        Call GetListofPlots() 'Refresh the list of plots

    End Sub


    Private Sub UpdateStatus()

        'Updates the stauts text at the top of the process plots tab.
        Try


            Dim PlotSourceFolder As String = txtPlotSourceFolder.Text

            If System.IO.Directory.Exists(PlotSourceFolder) Then

                LblMonitoring.Text = "Monitoring " & PlotSourceFolder & " for plots."

            Else

                LblMonitoring.Text = "Not monitoring for plots - " & PlotSourceFolder & " does not exist."
                ListBox_Plots2Move.Items.Clear()
                ListBox_SizeOfPlots.Items.Clear()
                lblStatus.Text = "There are no plots being moved - plot folder does not exist or is not accessible."

                Return

            End If

            Dim di As New IO.DirectoryInfo(PlotSourceFolder)
            Dim MoveFilesAry As IO.FileInfo() = di.GetFiles("*.mov*")
            Dim PlotFilesAry As IO.FileInfo() = di.GetFiles("*." & PlotType)

            ActiveMoves = MoveFilesAry.Count
            Dim PlotsWaitng = PlotFilesAry.Count

            Dim MyStatus As String
            Dim PlotsWaitingStr As String


            If StopMoving = True Then
                If ActiveMoves > 0 Then
                    If ActiveMoves > 1 Then
                        MyStatus = "Stopping, there are " & ActiveMoves.ToString & " plots still being moved."
                    Else
                        MyStatus = "Stopping, there is " & ActiveMoves.ToString & " plot still being moved."
                    End If

                Else
                    MyStatus = "Stopped moving plots."
                End If
            Else
                If ActiveMoves = 0 Then

                    MyStatus = "Waiting for more plots."
                    Dim Mytext As String
                    Dim count As Integer = 0

                    'we check if all drives are full and or not valid, thus no where to move plots to

                    For a = 0 To 9

                        Mytext = ListBox_PlotsBeingMoved.Items(a).ToString
                        If Mytext.Contains("full") Then count += 1
                        If Mytext.Contains("location") Then count += 1

                    Next

                    If count = 10 Then MyStatus = "Drives are either full or not setup."

                ElseIf ActiveMoves > 1 Then
                    MyStatus = "There are " & ActiveMoves.ToString & " plots being moved."
                Else
                    MyStatus = "There is " & ActiveMoves.ToString & " plot being moved."
                End If
            End If

            If PlotsWaitng = 1 Then
                PlotsWaitingStr = "There is 1 plot waiting to be moved."
            Else
                PlotsWaitingStr = "There are " & PlotsWaitng.ToString & " to be moved."

            End If

            lblStatus.Text = MyStatus & PlotsWaitingStr

        Catch ex As Exception

            'MessageBox.Show(String.Format("Error in UpdateStatus: {0}", ex.Message))
            ExceptionThrown("UpdateStatus", String.Format("{0}" & ex.Message))

        End Try

    End Sub

    Private Sub UpdateArrays()

        'Update all applicable arrays

        Call UpdateDestinations()
        Call UpdateDriveDisabled()
        Call UpdateNicknames()
        Call UpdateDeletePlots()
        Call UpdateReservedSpace()

    End Sub

    Private Sub UpdateDestinations()

        'Updates the destination array.

        If Startup Then Return

        Destination(1) = txtDrive1Dest.Text
        Destination(2) = txtDrive2Dest.Text
        Destination(3) = txtDrive3Dest.Text
        Destination(4) = txtDrive4Dest.Text
        Destination(5) = txtDrive5Dest.Text
        Destination(6) = txtDrive6Dest.Text
        Destination(7) = txtDrive7Dest.Text
        Destination(8) = txtDrive8Dest.Text
        Destination(9) = txtDrive9Dest.Text
        Destination(10) = txtDrive10Dest.Text

    End Sub
    Private Sub UpdateDriveDisabled()

        'Updates the drive disbale array

        If Startup Then Return

        DriveDisabled(1) = chkDisbleDrive1.Checked
        DriveDisabled(2) = chkDisbleDrive2.Checked
        DriveDisabled(3) = chkDisbleDrive3.Checked
        DriveDisabled(4) = chkDisbleDrive4.Checked
        DriveDisabled(5) = chkDisbleDrive5.Checked
        DriveDisabled(6) = chkDisbleDrive6.Checked
        DriveDisabled(7) = chkDisbleDrive7.Checked
        DriveDisabled(8) = chkDisbleDrive8.Checked
        DriveDisabled(9) = chkDisbleDrive9.Checked
        DriveDisabled(10) = chkDisbleDrive10.Checked

    End Sub
    Private Sub UpdateNicknames()

        'Updates the nickname array

        If Startup Then Return

        Nickname(1) = txtNickname1.Text
        Nickname(2) = txtNickname2.Text
        Nickname(3) = txtNickname3.Text
        Nickname(4) = txtNickname4.Text
        Nickname(5) = txtNickname5.Text
        Nickname(6) = txtNickname6.Text
        Nickname(7) = txtNickname7.Text
        Nickname(8) = txtNickname8.Text
        Nickname(9) = txtNickname9.Text
        Nickname(10) = txtNickname10.Text

    End Sub
    Private Sub UpdateDeletePlots()

        'Updates the delete plot locations array
        'Updates the delete plot check box array

        If Startup Then Return

        DeleteLocation(1) = txtDrive1DelPlotsFolder.Text
        DeleteLocation(2) = txtDrive2DelPlotsFolder.Text
        DeleteLocation(3) = txtDrive3DelPlotsFolder.Text
        DeleteLocation(4) = txtDrive4DelPlotsFolder.Text
        DeleteLocation(5) = txtDrive5DelPlotsFolder.Text
        DeleteLocation(6) = txtDrive6DelPlotsFolder.Text
        DeleteLocation(7) = txtDrive7DelPlotsFolder.Text
        DeleteLocation(8) = txtDrive8DelPlotsFolder.Text
        DeleteLocation(9) = txtDrive9DelPlotsFolder.Text
        DeleteLocation(10) = txtDrive10DelPlotsFolder.Text

        Delete_Plots(1) = ChkDel_Plots1.Checked
        Delete_Plots(2) = ChkDel_Plots2.Checked
        Delete_Plots(3) = ChkDel_Plots3.Checked
        Delete_Plots(4) = ChkDel_Plots4.Checked
        Delete_Plots(5) = ChkDel_Plots5.Checked
        Delete_Plots(6) = ChkDel_Plots6.Checked
        Delete_Plots(7) = ChkDel_Plots7.Checked
        Delete_Plots(8) = ChkDel_Plots8.Checked
        Delete_Plots(9) = ChkDel_Plots9.Checked
        Delete_Plots(10) = ChkDel_Plots10.Checked

    End Sub
    Private Sub UpdateReservedSpace()

        'For some reason when doing the calc in one line I got Arithmetic operation resulted in an overflow 
        'Updates the reserved space, and converts it to bytes

        If Startup Then Return

        Dim Val1 As Long = CInt(TxtReserved1.Text)
        Dim Val2 As Long = CInt(TxtReserved2.Text)
        Dim Val3 As Long = CInt(TxtReserved3.Text)
        Dim Val4 As Long = CInt(TxtReserved4.Text)
        Dim Val5 As Long = CInt(TxtReserved5.Text)
        Dim Val6 As Long = CInt(TxtReserved6.Text)
        Dim Val7 As Long = CInt(TxtReserved7.Text)
        Dim Val8 As Long = CInt(TxtReserved8.Text)
        Dim Val9 As Long = CInt(TxtReserved9.Text)
        Dim Val10 As Long = CInt(TxtReserved10.Text)

        ReservedSpace(1) = 1073741824 * Val1  '1073741824 = 1GB
        ReservedSpace(2) = 1073741824 * Val2
        ReservedSpace(3) = 1073741824 * Val3
        ReservedSpace(4) = 1073741824 * Val4
        ReservedSpace(5) = 1073741824 * Val5
        ReservedSpace(6) = 1073741824 * Val6
        ReservedSpace(7) = 1073741824 * Val7
        ReservedSpace(8) = 1073741824 * Val8
        ReservedSpace(9) = 1073741824 * Val9
        ReservedSpace(10) = 1073741824 * Val10

    End Sub

    Public Function GetFolderSize(ByVal folderPath As String, ByVal plotextension As String) As Decimal

        'Gets the size of all the plot files in the directory

        Dim size As Decimal = 0

        If System.IO.Directory.Exists(folderPath) Then

            Try
                Dim files As String() = Directory.GetFiles(folderPath, "*." & plotextension, SearchOption.AllDirectories)
                For Each file As String In files
                    Dim fileInfo As New FileInfo(file)
                    size += fileInfo.Length
                Next
            Catch ex As Exception
                ' Handle any exceptions that may occur, or not as the case may be!
                'MessageBox.Show(String.Format("Error in GetFolderSize: {0}", ex.Message))
                ExceptionThrown("GetFolderSize", String.Format("{0}" & ex.Message))
            End Try

        End If

        Return size


    End Function

    Public Function ForHumans(ByteSize As Decimal) As String

        'Takes the size in bytes and convertes it to GB,TB etc.

        Dim OneKB As Decimal = 1024
        Dim OneMB As Decimal = 1048576
        Dim OneGB As Decimal = 1073741824
        Dim OneTB As Decimal = 1099511600000
        Dim Sum As Decimal

        If ByteSize / OneTB > 1 Then
            Sum = ByteSize / OneTB
            Return Math.Round(Sum, 2) & " TB"
        ElseIf ByteSize / OneGB > 1 Then
            Sum = ByteSize / OneGB
            Return Math.Round(Sum, 1) & " GB"
        ElseIf ByteSize / OneMB > 1 Then
            Sum = ByteSize / OneMB
            Return Math.Round(Sum, 0) & " MB"
        ElseIf ByteSize / OneKB > 1 Then
            Sum = ByteSize / OneKB
            Return Math.Round(Sum, 0) & " KB"
        Else
            Return Math.Round(ByteSize, 0) & " bytes"

        End If

    End Function


    Private Sub UpdateSizeofDeletePlots()

        'Updates the size of the delete plots list

        Dim Size As Decimal

        For a = 1 To DriveSlots

            Size = GetFolderSize(DeleteLocation(a), PlotDeleteType)
            DeletePlotsSize(a) = Size
            ListBox_SizeofDeletePlots.Items.RemoveAt(a - 1)
            ListBox_SizeofDeletePlots.Items.Insert(a - 1, ForHumans(Size))

        Next

    End Sub


    Private Sub TxtReserved_TextChanged(sender As Object, e As EventArgs) Handles TxtReserved1.TextChanged, TxtReserved2.TextChanged, TxtReserved3.TextChanged, TxtReserved4.TextChanged, TxtReserved5.TextChanged, TxtReserved6.TextChanged, TxtReserved7.TextChanged, TxtReserved8.TextChanged, TxtReserved9.TextChanged, TxtReserved10.TextChanged

        If Startup Then Return

        'Allows ony digits to be used in the resevered space text box

        Dim digitsOnly As Regex = New Regex("[^\d]")
        Dim c As Control = CType(sender, Control)
        c.Text = digitsOnly.Replace(c.Text, "")

    End Sub
    Private Sub TxtNickname_Leave(sender As Object, e As EventArgs) Handles txtNickname1.Leave, txtNickname2.Leave, txtNickname3.Leave, txtNickname4.Leave, txtNickname5.Leave, txtNickname6.Leave, txtNickname7.Leave, txtNickname8.Leave, txtNickname9.Leave, txtNickname10.Leave

        'Any drives that are not busy will have their nicknames updated
        'Fires whenever we leave a nickname text box.

        Dim OldNickNames(DriveSlots) As String
        Array.Copy(Nickname, OldNickNames, 11)

        Call UpdateNicknames()

        Dim Mytext As String
        Dim Newtext As String
        Dim Oldtext As String

        ListBox_PlotsBeingMoved.BeginUpdate()       'Stops the list refreshing whilst we update it.

        For a = 1 To DriveSlots

            Mytext = ListBox_PlotsBeingMoved.Items(a - 1).ToString()

            If Not Mytext.Contains("busy moving") Then

                Newtext = "(" & Nickname(a)
                Oldtext = "(" & OldNickNames(a)
                ListBox_PlotsBeingMoved.Items.RemoveAt(a - 1)
                ListBox_PlotsBeingMoved.Items.Insert(a - 1, Mytext.Replace(Oldtext, Newtext))

            End If

        Next

        ListBox_PlotsBeingMoved.EndUpdate()

    End Sub

    Private Sub BtnViewLogs_Click(sender As Object, e As EventArgs) Handles BtnViewLogs.Click

        'Opens the logs folder

        Process.Start(MyAppPath & "\Logs")

    End Sub

    Private Sub TabPage2_Leave(sender As Object, e As EventArgs) Handles TabPage2.Leave

        'Starts the event timer when we leave the drive locations tab

        Timer_CheckandMove.Interval = 250 'So changes appear immediate, rather than wait 5 seconds
        Timer_CheckandMove.Enabled = True

    End Sub

    Private Sub TabPage2_Enter(sender As Object, e As EventArgs) Handles TabPage2.Enter

        'Stops the event timer when we switch to the drive locations tab

        Timer_CheckandMove.Enabled = False

    End Sub

    Private Sub TxtReserved1_Leave(sender As Object, e As EventArgs) Handles TxtReserved1.Leave, TxtReserved2.Leave, TxtReserved3.Leave, TxtReserved4.Leave, TxtReserved5.Leave, TxtReserved6.Leave, TxtReserved7.Leave, TxtReserved8.Leave, TxtReserved9.Leave, TxtReserved10.Leave

        If TxtReserved1.Text = "" Then TxtReserved1.Text = "0"
        If TxtReserved2.Text = "" Then TxtReserved2.Text = "0"
        If TxtReserved3.Text = "" Then TxtReserved3.Text = "0"
        If TxtReserved4.Text = "" Then TxtReserved4.Text = "0"
        If TxtReserved5.Text = "" Then TxtReserved5.Text = "0"
        If TxtReserved6.Text = "" Then TxtReserved6.Text = "0"
        If TxtReserved7.Text = "" Then TxtReserved7.Text = "0"
        If TxtReserved8.Text = "" Then TxtReserved8.Text = "0"
        If TxtReserved9.Text = "" Then TxtReserved9.Text = "0"
        If TxtReserved10.Text = "" Then TxtReserved10.Text = "0"

    End Sub

    Private Sub Button1_Click_2(sender As Object, e As EventArgs) Handles Button1.Click

        Clipboard.SetText(txtXCH_address.Text)

    End Sub

    Private Sub ComBoxPLotType_TextChanged(sender As Object, e As EventArgs) Handles ComBoxPLotType.TextChanged

        PlotType = ComBoxPLotType.Text
        Timer_CheckandMove.Interval = 250

    End Sub

    Private Sub ComBoxPlotDeleteType_TextChanged(sender As Object, e As EventArgs) Handles ComBoxPlotDeleteType.TextChanged

        PlotDeleteType = ComBoxPlotDeleteType.Text
        Timer_CheckandMove.Interval = 250

    End Sub


    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        'Writes the setting to disk

        My.Settings.Drive1Dest = txtDrive1Dest.Text
        My.Settings.Drive2Dest = txtDrive2Dest.Text
        My.Settings.Drive3Dest = txtDrive3Dest.Text
        My.Settings.Drive4Dest = txtDrive4Dest.Text
        My.Settings.Drive5Dest = txtDrive5Dest.Text
        My.Settings.Drive6Dest = txtDrive6Dest.Text
        My.Settings.Drive7Dest = txtDrive7Dest.Text
        My.Settings.Drive8Dest = txtDrive8Dest.Text
        My.Settings.Drive9Dest = txtDrive9Dest.Text
        My.Settings.Drive10Dest = txtDrive10Dest.Text

        My.Settings.Drive1DelFolder = txtDrive1DelPlotsFolder.Text
        My.Settings.Drive2DelFolder = txtDrive2DelPlotsFolder.Text
        My.Settings.Drive3DelFolder = txtDrive3DelPlotsFolder.Text
        My.Settings.Drive4DelFolder = txtDrive4DelPlotsFolder.Text
        My.Settings.Drive5DelFolder = txtDrive5DelPlotsFolder.Text
        My.Settings.Drive6DelFolder = txtDrive6DelPlotsFolder.Text
        My.Settings.Drive7DelFolder = txtDrive7DelPlotsFolder.Text
        My.Settings.Drive8DelFolder = txtDrive8DelPlotsFolder.Text
        My.Settings.Drive9DelFolder = txtDrive9DelPlotsFolder.Text
        My.Settings.Drive10DelFolder = txtDrive10DelPlotsFolder.Text

        My.Settings.Drive1CheckBox = chkDisbleDrive1.Checked
        My.Settings.Drive2CheckBox = chkDisbleDrive2.Checked
        My.Settings.Drive3CheckBox = chkDisbleDrive3.Checked
        My.Settings.Drive4CheckBox = chkDisbleDrive4.Checked
        My.Settings.Drive5CheckBox = chkDisbleDrive5.Checked
        My.Settings.Drive6CheckBox = chkDisbleDrive6.Checked
        My.Settings.Drive7CheckBox = chkDisbleDrive7.Checked
        My.Settings.Drive8CheckBox = chkDisbleDrive8.Checked
        My.Settings.Drive9CheckBox = chkDisbleDrive9.Checked
        My.Settings.Drive10CheckBox = chkDisbleDrive10.Checked

        My.Settings.Drive1Del = ChkDel_Plots1.Checked
        My.Settings.Drive2Del = ChkDel_Plots2.Checked
        My.Settings.Drive3Del = ChkDel_Plots3.Checked
        My.Settings.Drive4Del = ChkDel_Plots4.Checked
        My.Settings.Drive5Del = ChkDel_Plots5.Checked
        My.Settings.Drive6Del = ChkDel_Plots6.Checked
        My.Settings.Drive7Del = ChkDel_Plots7.Checked
        My.Settings.Drive8Del = ChkDel_Plots8.Checked
        My.Settings.Drive9Del = ChkDel_Plots9.Checked
        My.Settings.Drive10Del = ChkDel_Plots10.Checked

        My.Settings.PlotSourceFolder = txtPlotSourceFolder.Text

        My.Settings.Drive1NickName = txtNickname1.Text
        My.Settings.Drive2NickName = txtNickname2.Text
        My.Settings.Drive3NickName = txtNickname3.Text
        My.Settings.Drive4NickName = txtNickname4.Text
        My.Settings.Drive5NickName = txtNickname5.Text
        My.Settings.Drive6NickName = txtNickname6.Text
        My.Settings.Drive7NickName = txtNickname7.Text
        My.Settings.Drive8NickName = txtNickname8.Text
        My.Settings.Drive9NickName = txtNickname9.Text
        My.Settings.Drive10NickName = txtNickname10.Text

        My.Settings.MaxConsecMoves = NumUpDownMaxMove.Value

        My.Settings.ReservedSpace1 = CInt(TxtReserved1.Text)
        My.Settings.ReservedSpace2 = CInt(TxtReserved2.Text)
        My.Settings.ReservedSpace3 = CInt(TxtReserved3.Text)
        My.Settings.ReservedSpace4 = CInt(TxtReserved4.Text)
        My.Settings.ReservedSpace5 = CInt(TxtReserved5.Text)
        My.Settings.ReservedSpace6 = CInt(TxtReserved6.Text)
        My.Settings.ReservedSpace7 = CInt(TxtReserved7.Text)
        My.Settings.ReservedSpace8 = CInt(TxtReserved8.Text)
        My.Settings.ReservedSpace9 = CInt(TxtReserved9.Text)
        My.Settings.ReservedSpace10 = CInt(TxtReserved10.Text)
        My.Settings.PlotType = ComBoxPLotType.Text
        My.Settings.PlotDeleteType = ComBoxPlotDeleteType.Text


        My.Settings.Save()

    End Sub


End Class
