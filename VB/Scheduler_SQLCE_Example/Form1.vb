Imports Microsoft.VisualBasic
Imports DevExpress.XtraScheduler
Imports System
Imports System.Data
Imports System.Data.SqlServerCe
Imports System.Windows.Forms

Namespace Scheduler_SQLCE_Example
	Partial Public Class Form1
		Inherits Form
		Public Sub New()
			InitializeComponent()
			Me.schedulerStorage1.Appointments.ResourceSharing = True
			Me.schedulerStorage1.Appointments.CommitIdToDataSource = False
			AddHandler Me.schedulerStorage1.AppointmentsInserted, AddressOf schedulerStorage1_AppointmentsInserted
			AddHandler appointmentsTableAdapter.Adapter.RowUpdated, AddressOf appointmentsTableAdapter_RowUpdated
		End Sub
		Private Sub appointmentsTableAdapter_RowUpdated(ByVal sender As Object, ByVal e As SqlCeRowUpdatedEventArgs)
			' Gets ID for a new appointment from the SQL CE data source
			If e.Status = UpdateStatus.Continue AndAlso e.StatementType = StatementType.Insert Then
				Dim id As Integer = 0
				Using cmd As New SqlCeCommand("SELECT @@IDENTITY", appointmentsTableAdapter.Connection)
					id = Convert.ToInt32(cmd.ExecuteScalar())
				End Using
				e.Row("UniqueID") = id
			End If
		End Sub

		Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
			' TODO: This line of code loads data into the 'dXDBDataSet2.Resources' table. You can move, or remove it, as needed.
			Me.resourcesTableAdapter.Fill(Me.dXDBDataSet2.Resources)
			' TODO: This line of code loads data into the 'dXDBDataSet2.Appointments' table. You can move, or remove it, as needed.
			Me.appointmentsTableAdapter.Fill(Me.dXDBDataSet2.Appointments)
		End Sub

		Private Sub schedulerStorage1_AppointmentChanging(ByVal sender As Object, ByVal e As DevExpress.XtraScheduler.PersistentObjectCancelEventArgs) Handles schedulerStorage1.AppointmentChanging
			Dim apt_id As String = String.Empty
			If (CType(e.Object, Appointment)).Id Is Nothing Then
				apt_id = "null"
			Else
				apt_id = (CType(e.Object, Appointment)).Id.ToString()
			End If
			MessageBox.Show("AppointmentChanging event for " & apt_id)
		End Sub

		Private Sub OnApptChangedInsertedDeleted(ByVal sender As Object, ByVal e As PersistentObjectsEventArgs) Handles schedulerStorage1.AppointmentsChanged, schedulerStorage1.AppointmentsDeleted
			UpdateSource()
		End Sub

		Private Sub schedulerStorage1_AppointmentsInserted(ByVal sender As Object, ByVal e As PersistentObjectsEventArgs)
			UpdateSource()
			' Synchronize appointment IDs in the storage with the SQL CE data source
			Dim apts As AppointmentBaseCollection = TryCast(e.Objects, AppointmentBaseCollection)
			For Each apt As Appointment In apts
				Dim dataRow As DataRowView = TryCast(apt.GetSourceObject(Me.schedulerStorage1), DataRowView)
				Me.schedulerStorage1.SetAppointmentId(apt, dataRow.Row("UniqueID"))
			Next apt
		End Sub

		Private Sub UpdateSource()
			appointmentsTableAdapter.Update(dXDBDataSet2)
			dXDBDataSet2.AcceptChanges()
		End Sub
	End Class
End Namespace
