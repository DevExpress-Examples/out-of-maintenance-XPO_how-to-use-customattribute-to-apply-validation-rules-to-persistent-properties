Imports Microsoft.VisualBasic
Imports System
Imports DXSample
Imports DevExpress.Xpo
Imports DevExpress.XtraGrid
Imports System.Windows.Forms
Imports System.Collections.Generic
Imports DevExpress.XtraGrid.Columns
Imports DevExpress.XtraGrid.Views.Base
Imports DevExpress.XtraGrid.Views.Grid
Imports DevExpress.XtraEditors.Controls

Namespace Q296004
	Partial Public Class Form1
		Inherits Form
		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub gridView1_ValidatingEditor(ByVal sender As Object, ByVal e As BaseContainerValidateEditorEventArgs) Handles gridView1.ValidatingEditor
			Dim view As GridView = CType(sender, GridView)
			Dim row As XPBaseObject = CType(view.GetFocusedRow(), XPBaseObject)
			Dim customValues As New Dictionary(Of String, Object)()
			customValues.Add(view.FocusedColumn.FieldName, e.Value)
			e.ErrorText = New Validator(row, customValues).Validate(view.FocusedColumn.FieldName)
			e.Valid = String.IsNullOrEmpty(e.ErrorText)
		End Sub

		Private Sub gridView1_ValidateRow(ByVal sender As Object, ByVal e As ValidateRowEventArgs) Handles gridView1.ValidateRow
			If e.RowHandle <> GridControl.NewItemRowHandle Then
				Return
			End If
			Dim customValues As New Dictionary(Of String, Object)()
			Dim view As GridView = CType(sender, GridView)
			For Each col As GridColumn In view.Columns
				customValues.Add(col.FieldName, view.GetRowCellValue(e.RowHandle, col))
			Next col
			Dim validator As New Validator(CType(e.Row, XPBaseObject), customValues)
			For Each col As GridColumn In (CType(sender, GridView)).Columns
				e.ErrorText = validator.Validate(col.FieldName)
				If (Not String.IsNullOrEmpty(e.ErrorText)) Then
					e.Valid = False
					Return
				End If
			Next col
		End Sub

		Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing
			unitOfWork1.CommitChanges()
		End Sub
	End Class
End Namespace