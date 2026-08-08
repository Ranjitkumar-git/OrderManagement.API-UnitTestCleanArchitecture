using System;
using CalculatorApp;
using Xunit;

namespace CalculatorApp.Tests
{
    public class CalculatorServiceTests
    {
        private readonly CalculatorService _calculator;

        public CalculatorServiceTests()
        {
            // xUnit creates a new instance of the test class
            // for each test case that it executes.
            _calculator = new CalculatorService();
        }

        [Fact]
        public void Add_WhenCalledWith2And3_Returns5()
        {
            // Arrange
            const int firstNumber = 2;
            const int secondNumber = 3;
            const int expectedResult = 5;

            // Act
            var actualResult = _calculator.Add(firstNumber, secondNumber);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void Subtract_WhenCalledWith5And3_Returns2()
        {
            // Arrange
            const int firstNumber = 5;
            const int secondNumber = 3;
            const int expectedResult = 2;

            // Act
            var actualResult = _calculator.Subtract(firstNumber, secondNumber);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }



        [Theory]
        [InlineData(2, 3, 6)]
        [InlineData(-2, 3, -6)]
        [InlineData(0, 5, 0)]
        public void Multiply_WhenCalled_ReturnsExpectedResult(
     int firstNumber,
     int secondNumber,
     int expectedResult)
        {
            // Arrange is provided through InlineData.

            // Act
            var actualResult = _calculator.Multiply(firstNumber, secondNumber);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void Divide_WhenCalledWith6And3_Returns2()
        {
            // Arrange
            const int dividend = 6;
            const int divisor = 3;
            const int expectedResult = 2;

            // Act
            var actualResult = _calculator.Divide(dividend, divisor);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void Divide_WhenDividingByZero_ThrowsDivideByZeroException()
        {
            // Arrange
            const int dividend = 10;
            const int divisor = 0;

            // Act
            Action action = () => _calculator.Divide(dividend, divisor);

            // Assert
            // This assertion verifies that the correct exception type is thrown. 
            var exception =
                Assert.Throws<DivideByZeroException>(action);

            // This assertion verifies the exception message.
            // Checking the message is optional and is useful only when
            // the exact message is part of the required behaviour.
            Assert.Equal(
                "Division by zero is not allowed.",
                exception.Message);
        }





    }

}
