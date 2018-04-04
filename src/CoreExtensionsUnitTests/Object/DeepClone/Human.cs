using System.Collections.Generic;

namespace StandardDot.CoreExtensions.UnitTests.Object.DeepClone
{
    internal enum Gender
    {
        Male = 0,
        Female = 1
    }

    internal class Human
    {
        private string _name;
        private Gender _gender;
        private List<Human> _children;

        public string Name { get { return _name; } set { _name = value; } }
        public Gender Gender { get { return _gender; } set { _gender = value; } }
        public List<Human> Children { get { return _children; } set { _children = value; } }
    }
}
