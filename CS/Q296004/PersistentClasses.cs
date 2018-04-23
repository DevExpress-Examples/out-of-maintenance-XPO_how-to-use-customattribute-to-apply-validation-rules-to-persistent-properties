using DevExpress.Xpo;

namespace DXSample {
    public class Person :XPObject {
        public Person(Session session) : base(session) { }

        private string fName;
        [Custom("Validation", "not(Name is null or Name = '');Unique|Please provide the person name;The person with this name is already exists")]
        public string Name {
            get {
                return fName;
            }
            set {
                SetPropertyValue("Name", ref fName, value);
            }
        }

        private int fAge;
        [Custom("Validation", "Age between (20, 40)|The age can be between 20 and 40 years old")]
        public int Age {
            get {
                return fAge;
            }
            set {
                SetPropertyValue("Age", ref fAge, value);
            }
        }
    }
}