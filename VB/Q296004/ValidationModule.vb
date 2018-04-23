Imports Microsoft.VisualBasic
Imports System
Imports DevExpress.Xpo
Imports DevExpress.Xpo.Metadata
Imports DevExpress.Data.Filtering
Imports System.Collections.Generic
Imports DevExpress.Xpo.Metadata.Helpers
Imports DevExpress.Data.Filtering.Helpers

Namespace DXSample
	Public Class Validator
		Private fValidatedObject As XPBaseObject
		Private fCustomValues As Dictionary(Of String, Object)

		Public Sub New(ByVal validatedObject As XPBaseObject, ByVal customValues As Dictionary(Of String, Object))
			fValidatedObject = validatedObject
			fCustomValues = customValues
		End Sub

		Public Function Validate(ByVal [property] As String) As String
			Dim member As XPMemberInfo = fValidatedObject.ClassInfo.FindMember([property])
			If member Is Nothing Then
				Return String.Empty
			End If
			Dim attribute As CustomAttribute = CType(member.FindAttributeInfo("Validation"), CustomAttribute)
			If attribute Is Nothing Then
				Return String.Empty
			End If
			Dim rules() As String = attribute.Value.Substring(0, attribute.Value.IndexOf("|"c)).Split(";"c)
			Dim messages() As String = attribute.Value.Substring(attribute.Value.IndexOf("|"c) + 1).Split(";"c)
			For i As Integer = 0 To rules.Length - 1
				If (Not ValidateRule(member, rules(i))) Then
					Return messages(i)
				End If
			Next i
			Return String.Empty
		End Function

		Private Function ValidateUniqueRule(ByVal [property] As XPMemberInfo) As Boolean
			Dim val As Object
			If fCustomValues.ContainsKey([property].Name) Then
				val = fCustomValues([property].Name)
			Else
				val = [property].GetValue(fValidatedObject)
			End If
			Return fValidatedObject.Session.FindObject(PersistentCriteriaEvaluationBehavior.InTransaction, fValidatedObject.ClassInfo, (Not New BinaryOperator("This", fValidatedObject)) And New BinaryOperator([property].Name, val)) Is Nothing
		End Function

		Private Function ValidateExpression(ByVal expression As String) As Boolean
			Dim contextDescriptor As New CustomValidationEvaluatorContextDescriptor(fValidatedObject.ClassInfo)
			AddHandler contextDescriptor.CustomValue, AddressOf contextDescriptor_CustomValue
			Dim result As Boolean = New ExpressionEvaluator(contextDescriptor, CriteriaOperator.Parse(expression)).Fit(fValidatedObject)
			RemoveHandler contextDescriptor.CustomValue, AddressOf contextDescriptor_CustomValue
			Return result
		End Function

		Private Sub contextDescriptor_CustomValue(ByVal sender As Object, ByVal e As CustomValueEventArgs)
			If fCustomValues.ContainsKey(e.PropertyName) Then
				e.Handled = True
				e.CustomValue = fCustomValues(e.PropertyName)
			End If
		End Sub

		Private Function ValidateRule(ByVal [property] As XPMemberInfo, ByVal rule As String) As Boolean
			If rule = "Unique" Then
				Return ValidateUniqueRule([property])
			End If
			Return ValidateExpression(rule)
		End Function
	End Class

	Public Class CustomValidationEvaluatorContextDescriptor
		Inherits EvaluatorContextDescriptorXpo
		Public Sub New(ByVal owner As XPClassInfo)
			MyBase.New(owner)
		End Sub

		Public Overrides Function GetPropertyValue(ByVal source As Object, ByVal propertyPath As EvaluatorProperty) As Object
			Dim customValue As CustomValueEventArgs = GetCustomValue(propertyPath.PropertyPath)
			If customValue.Handled Then
				Return customValue.CustomValue
			End If
			Return MyBase.GetPropertyValue(source, propertyPath)
		End Function

		Private Event fCustomValue As EventHandler(Of CustomValueEventArgs)
		Public Custom Event CustomValue As EventHandler(Of CustomValueEventArgs)
			AddHandler(ByVal value As EventHandler(Of CustomValueEventArgs))
				AddHandler fCustomValue, value
			End AddHandler
			RemoveHandler(ByVal value As EventHandler(Of CustomValueEventArgs))
				RemoveHandler fCustomValue, value
			End RemoveHandler
            RaiseEvent(ByVal sender As System.Object, ByVal e As CustomValueEventArgs)
            End RaiseEvent
		End Event
		Private Function GetCustomValue(ByVal propertyName As String) As CustomValueEventArgs
			Dim args As New CustomValueEventArgs(propertyName)
			RaiseEvent fCustomValue(Me, args)
			Return args
		End Function
	End Class

	Public Class CustomValueEventArgs
		Inherits EventArgs
		Public Sub New(ByVal propertyName As String)
			fPropertyName = propertyName
		End Sub

		Private fCustomValue As Object
		Public Property CustomValue() As Object
			Get
				Return fCustomValue
			End Get
			Set(ByVal value As Object)
				fCustomValue = value
			End Set
		End Property

		Private fPropertyName As String
		Public ReadOnly Property PropertyName() As String
			Get
				Return fPropertyName
			End Get
		End Property

		Private fHandled As Boolean
		Public Property Handled() As Boolean
			Get
				Return fHandled
			End Get
			Set(ByVal value As Boolean)
				fHandled = value
			End Set
		End Property
	End Class
End Namespace