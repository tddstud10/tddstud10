using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CSXUnit1xNUnit3x
{
    public class StringTests3
    {
        [Xunit.Extensions.Theory, Xunit.Extensions.ClassData(typeof(IndexOfData))]
        public void IndexOf(string input, char letter, int expected)
        {
            var actual = input.IndexOf(letter);
            Xunit.Assert.Equal(expected, actual);
        }

        [NUnit.Framework.Datapoint]
        public double zero = 0;

        [NUnit.Framework.Datapoint]
        public double positive = 1;

        [NUnit.Framework.Theory]
        public void SquareRootDefinition(double num)
        {
            NUnit.Framework.Assume.That(num >= 0.0 && num < double.MaxValue);

            double sqrt = Math.Sqrt(num);

            NUnit.Framework.Assert.That(sqrt >= 0.0);
            NUnit.Framework.Assert.That(sqrt * sqrt, NUnit.Framework.Is.EqualTo(num).Within(0.000001));
        }
    }

    public class IndexOfData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
    {
        new object[] { "hello world", 'w', 6 },
        new object[] { "goodnight moon", 'w', -1 }
    };

        public IEnumerator<object[]> GetEnumerator()
        { return _data.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }
    }
}
