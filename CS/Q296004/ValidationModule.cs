using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using DevExpress.Xpo.Metadata.Helpers;
using DevExpress.Data.Filtering.Helpers;

namespace DXSample {
    public class Validator {
        XPBaseObject fValidatedObject;
        Dictionary<string, object> fCustomValues;

        public Validator(XPBaseObject validatedObject, Dictionary<string, object> customValues) {
            fValidatedObject = validatedObject;
            fCustomValues = customValues;
        }

        public string Validate(string property) {
            XPMemberInfo member = fValidatedObject.ClassInfo.FindMember(property);
            if (member == null) return string.Empty;
            CustomAttribute attribute = (CustomAttribute)member.FindAttributeInfo("Validation");
            if (attribute == null) return string.Empty;
            string[] rules = attribute.Value.Substring(0, attribute.Value.IndexOf('|')).Split(';');
            string[] messages = attribute.Value.Substring(attribute.Value.IndexOf('|') + 1).Split(';');
            for (int i = 0; i < rules.Length; i++)
                if (!ValidateRule(member, rules[i]))
                    return messages[i];
            return string.Empty;
        }

        bool ValidateUniqueRule(XPMemberInfo property) {
            object val = fCustomValues.ContainsKey(property.Name) ? fCustomValues[property.Name] : property.GetValue(fValidatedObject);
            return fValidatedObject.Session.FindObject(PersistentCriteriaEvaluationBehavior.InTransaction, fValidatedObject.ClassInfo, 
                !new BinaryOperator("This", fValidatedObject) &
                new BinaryOperator(property.Name, val)) == null;
        }

        bool ValidateExpression(string expression) {
            CustomValidationEvaluatorContextDescriptor contextDescriptor = new CustomValidationEvaluatorContextDescriptor(fValidatedObject.ClassInfo);
            contextDescriptor.CustomValue += contextDescriptor_CustomValue;
            bool result = new ExpressionEvaluator(contextDescriptor, CriteriaOperator.Parse(expression)).Fit(fValidatedObject);
            contextDescriptor.CustomValue -= contextDescriptor_CustomValue;
            return result;
        }

        void contextDescriptor_CustomValue(object sender, CustomValueEventArgs e) {
            if (fCustomValues.ContainsKey(e.PropertyName)) {
                e.Handled = true;
                e.CustomValue = fCustomValues[e.PropertyName];
            }
        }

        bool ValidateRule(XPMemberInfo property, string rule) {
            if (rule == "Unique") return ValidateUniqueRule(property);
            return ValidateExpression(rule);
        }
    }

    public class CustomValidationEvaluatorContextDescriptor :EvaluatorContextDescriptorXpo {
        public CustomValidationEvaluatorContextDescriptor(XPClassInfo owner) : base(owner) { }

        public override object GetPropertyValue(object source, EvaluatorProperty propertyPath) {
            CustomValueEventArgs customValue = GetCustomValue(propertyPath.PropertyPath);
            if (customValue.Handled) return customValue.CustomValue;
            return base.GetPropertyValue(source, propertyPath);
        }

        EventHandler<CustomValueEventArgs> fCustomValue;
        public event EventHandler<CustomValueEventArgs> CustomValue {
            add { fCustomValue += value; }
            remove { fCustomValue -= value; }
        }
        CustomValueEventArgs GetCustomValue(string propertyName) {
            CustomValueEventArgs args = new CustomValueEventArgs(propertyName);
            if (fCustomValue != null) fCustomValue(this, args);
            return args;
        }
    }

    public class CustomValueEventArgs :EventArgs {
        public CustomValueEventArgs(string propertyName) {
            fPropertyName = propertyName;
        }

        private object fCustomValue;
        public object CustomValue {
            get { return fCustomValue; }
            set { fCustomValue = value; }
        }

        private string fPropertyName;
        public string PropertyName { get { return fPropertyName; } }

        private bool fHandled;
        public bool Handled {
            get { return fHandled; }
            set { fHandled = value; }
        }
    }
}