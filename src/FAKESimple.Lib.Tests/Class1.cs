using Xunit;

namespace FAKESimple.Lib.Tests
{
    public class CalculatorTest
    {
        [Fact]
        public void AddTest()
        {
            var actual = Calculator.Add(1, 3);
            Assert.Equal(4, actual);
        }

        [Fact]
        public void SubFailTest()
        {
            var actual = Calculator.Sub(1, 3);
            Assert.Equal(-2, actual);
        }
    }
}
