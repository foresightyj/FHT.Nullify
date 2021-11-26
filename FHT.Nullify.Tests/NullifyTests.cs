using NUnit.Framework;

namespace FHT.Nullify.Tests
{
    public class NullifyTests
    {
        public enum Gender
        {
            Male,
            Female,
        }
        public class Person
        {
            public int Age { get; set; }
            public int BirthYear;
            public string Name { get; set; }
            public Gender Gender { get; set; }
            public Person Spouse { get; set; }
        }

        [Test]
        public void AllPropsDefaultWillReturnNull()
        {
            var p = new Person();
            var res = p.NullifyIfAllDefault();
            Assert.AreEqual(res, null, "all properties are default, so we should get null");
        }

        [Test]
        public void NotAllPropsDefaultWillReturnOriginal()
        {
            var p = new Person { Name = "Yuan Jian" };
            var res = p.NullifyIfAllDefault();
            Assert.AreEqual(res, res, "at least one prop is not default, we get back original value");
        }
        [Test]
        public void NotAllPropsDefaultWillReturnOriginal_Enum()
        {
            var p = new Person { Gender = Gender.Female };
            var res = p.NullifyIfAllDefault();
            Assert.AreEqual(res, res, "at least one prop is not default, we get back original value");
        }

        [Test]
        public void NotAllPropsDefaultWillReturnOriginal_Field()
        {
            var p = new Person { BirthYear = 1985 };
            var res = p.NullifyIfAllDefault();
            Assert.AreEqual(res, res, "at least one field is not default, we get back original value");
        }
    }
}