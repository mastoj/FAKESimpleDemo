using Xunit;

namespace FAKESimple.Lib.Tests
{
    public class CalculatorTest
    {
        [Fact]
        public void AddTest()
        {
            var actual = Calculator.Add(1, 3);
            Assert.Equal(5, actual);
        }
    }
}
