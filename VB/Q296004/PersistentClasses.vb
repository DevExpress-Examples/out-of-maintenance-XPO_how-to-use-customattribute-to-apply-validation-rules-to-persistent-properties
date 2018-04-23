Imports Microsoft.VisualBasic
Imports System
Imports DevExpress.Xpo

Namespace DXSample
	Public Class Person
		Inherits XPObject
		Public Sub New(ByVal session As Session)
			MyBase.New(session)
		End Sub

		Private fName As String
		<Custom("Validation", "not(Name is null or Name = '');Unique|Please provide the person name;The person with this name is already exists")> _
		Public Property Name() As String
			Get
				Return fName
			End Get
			Set(ByVal value As String)
				SetPropertyValue("Name", fName, value)
			End Set
		End Property

		Private fAge As Integer
		<Custom("Validation", "Age between (20, 40)|The age can be between 20 and 40 years old")> _
		Public Property Age() As Integer
			Get
				Return fAge
			End Get
			Set(ByVal value As Integer)
				SetPropertyValue("Age", fAge, value)
			End Set
		End Property
	End Class
End Namespace