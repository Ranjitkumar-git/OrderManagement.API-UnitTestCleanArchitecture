using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorApp
{
    public class CalculatorService
    {
        // Adds two integers and returns the result.
        public int Add(int firstNumber, int secondNumber)
        {
            return firstNumber + secondNumber;
        }

        // Subtracts the second number from the first number.
        public int Subtract(int firstNumber, int secondNumber)
        {
            // We are intentionally adding a defect to the Subtract() method for demonstration purposes.
            // Later, we will see how a unit test detects this incorrect behaviour.
           // return firstNumber - secondNumber + 5; //1.(firsrtime open this comeent and reun test) first time ru test method test fails because of this defect. After fixing the defect, the test method passes. 
            return firstNumber - secondNumber; // 2.2nd time open this line and run test method, test method passes because defect is fixed.
        }

        // Multiplies two integers and returns the result.
        public int Multiply(int firstNumber, int secondNumber)
        {
            return firstNumber * secondNumber;
        }

        // Divides the first integer by the second integer.
        // The Divide method uses integers, so it performs integer division.
        // For example, 5 / 2 returns 2, not 2.5.
        // To preserve the fractional part, use decimal or double.
        public int Divide(int dividend, int divisor)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException(
                    "Division by zero is not allowed.");
            }

            return dividend / divisor;
        }
    }
}

