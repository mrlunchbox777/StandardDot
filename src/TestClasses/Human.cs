using System.Collections.Generic;

namespace StandardDot.TestClasses
{
    public enum Gender
    {
        Male = 0,
        Female = 1
    }

    public class Human
    {
        private string _name;
        private Gender _gender;
        private List<Human> _children;

        public string Name { get { return _name; } set { _name = value; } }
        public Gender Gender { get { return _gender; } set { _gender = value; } }
        public List<Human> Children { get { return _children; } set { _children = value; } }
    }
}
