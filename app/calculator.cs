using csWebFrame;

namespace csWebFrame.app
{
    // Session variables for calculator
    public class FirstNumber : SessionVar<int>
    {
        public FirstNumber(UserSession session) : base(session, 0)
        {
        }
    }

    public class SecondNumber : SessionVar<int>
    {
        public SecondNumber(UserSession session) : base(session, 0)
        {
        }
    }

    public class CalculatorResult : SessionVar<int>
    {
        public CalculatorResult(UserSession session) : base(session, 0)
        {
        }
    }

    public class CurrentOperation : SessionVar<string>
    {
        public CurrentOperation(UserSession session) : base(session, "")
        {
        }
    }

    public class ErrorMessage : SessionVar<string>
    {
        public ErrorMessage(UserSession session) : base(session, "")
        {
        }
    }

    // Calculator operation buttons (no form elements, just buttons)
    public class AddButton : Button
    {
        private FirstNumber firstNumber;
        private SecondNumber secondNumber;
        private CalculatorResult result;
        private CurrentOperation operation;
        private ErrorMessage errorMessage;

        public AddButton(FirstNumber first, SecondNumber second, CalculatorResult res, CurrentOperation op, ErrorMessage err)
        {
            Name = "Add (+)";
            firstNumber = first;
            secondNumber = second;
            result = res;
            operation = op;
            errorMessage = err;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            try
            {
                // Get input values from the main form
                if (data.ContainsKey("firstNum"))
                    firstNumber.Set(int.Parse(data["firstNum"]));
                if (data.ContainsKey("secondNum"))
                    secondNumber.Set(int.Parse(data["secondNum"]));

                int calcResult = firstNumber.Get() + secondNumber.Get();
                result.Set(calcResult);
                operation.Set($"{firstNumber.Get()} + {secondNumber.Get()} = {calcResult}");
                errorMessage.Set("");
            }
            catch (Exception ex)
            {
                errorMessage.Set("Error: Please enter valid whole numbers");
                operation.Set("");
            }
        }
    }

    public class SubtractButton : Button
    {
        private FirstNumber firstNumber;
        private SecondNumber secondNumber;
        private CalculatorResult result;
        private CurrentOperation operation;
        private ErrorMessage errorMessage;

        public SubtractButton(FirstNumber first, SecondNumber second, CalculatorResult res, CurrentOperation op, ErrorMessage err)
        {
            Name = "Subtract (-)";
            firstNumber = first;
            secondNumber = second;
            result = res;
            operation = op;
            errorMessage = err;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            try
            {
                if (data.ContainsKey("firstNum"))
                    firstNumber.Set(int.Parse(data["firstNum"]));
                if (data.ContainsKey("secondNum"))
                    secondNumber.Set(int.Parse(data["secondNum"]));

                int calcResult = firstNumber.Get() - secondNumber.Get();
                result.Set(calcResult);
                operation.Set($"{firstNumber.Get()} - {secondNumber.Get()} = {calcResult}");
                errorMessage.Set("");
            }
            catch (Exception ex)
            {
                errorMessage.Set("Error: Please enter valid whole numbers");
                operation.Set("");
            }
        }
    }

    public class MultiplyButton : Button
    {
        private FirstNumber firstNumber;
        private SecondNumber secondNumber;
        private CalculatorResult result;
        private CurrentOperation operation;
        private ErrorMessage errorMessage;

        public MultiplyButton(FirstNumber first, SecondNumber second, CalculatorResult res, CurrentOperation op, ErrorMessage err)
        {
            Name = "Multiply (×)";
            firstNumber = first;
            secondNumber = second;
            result = res;
            operation = op;
            errorMessage = err;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            try
            {
                if (data.ContainsKey("firstNum"))
                    firstNumber.Set(int.Parse(data["firstNum"]));
                if (data.ContainsKey("secondNum"))
                    secondNumber.Set(int.Parse(data["secondNum"]));

                int calcResult = firstNumber.Get() * secondNumber.Get();
                result.Set(calcResult);
                operation.Set($"{firstNumber.Get()} × {secondNumber.Get()} = {calcResult}");
                errorMessage.Set("");
            }
            catch (Exception ex)
            {
                errorMessage.Set("Error: Please enter valid whole numbers");
                operation.Set("");
            }
        }
    }

    public class DivideButton : Button
    {
        private FirstNumber firstNumber;
        private SecondNumber secondNumber;
        private CalculatorResult result;
        private CurrentOperation operation;
        private ErrorMessage errorMessage;

        public DivideButton(FirstNumber first, SecondNumber second, CalculatorResult res, CurrentOperation op, ErrorMessage err)
        {
            Name = "Divide (÷)";
            firstNumber = first;
            secondNumber = second;
            result = res;
            operation = op;
            errorMessage = err;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            try
            {
                if (data.ContainsKey("firstNum"))
                    firstNumber.Set(int.Parse(data["firstNum"]));
                if (data.ContainsKey("secondNum"))
                    secondNumber.Set(int.Parse(data["secondNum"]));

                if (secondNumber.Get() == 0)
                {
                    errorMessage.Set("Error: Cannot divide by zero");
                    operation.Set("");
                    return;
                }

                int calcResult = firstNumber.Get() / secondNumber.Get();
                result.Set(calcResult);
                operation.Set($"{firstNumber.Get()} ÷ {secondNumber.Get()} = {calcResult}");
                errorMessage.Set("");
            }
            catch (Exception ex)
            {
                errorMessage.Set("Error: Please enter valid whole numbers");
                operation.Set("");
            }
        }
    }

    public class ClearButton : Button
    {
        private FirstNumber firstNumber;
        private SecondNumber secondNumber;
        private CalculatorResult result;
        private CurrentOperation operation;
        private ErrorMessage errorMessage;

        public ClearButton(FirstNumber first, SecondNumber second, CalculatorResult res, CurrentOperation op, ErrorMessage err)
        {
            Name = "Clear";
            firstNumber = first;
            secondNumber = second;
            result = res;
            operation = op;
            errorMessage = err;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            firstNumber.Set(0);
            secondNumber.Set(0);
            result.Set(0);
            operation.Set("");
            errorMessage.Set("");
        }
    }

    public class Calculator : DefaultPage
    {
        private FirstNumber firstNumber;
        private SecondNumber secondNumber;
        private CalculatorResult result;
        private CurrentOperation operation;
        private ErrorMessage errorMessage;
        private AddButton addButton;
        private SubtractButton subtractButton;
        private MultiplyButton multiplyButton;
        private DivideButton divideButton;
        private ClearButton clearButton;

        public Calculator(UserSession session) : base(session)
        {
            firstNumber = new FirstNumber(session);
            secondNumber = new SecondNumber(session);
            result = new CalculatorResult(session);
            operation = new CurrentOperation(session);
            errorMessage = new ErrorMessage(session);

            addButton = new AddButton(firstNumber, secondNumber, result, operation, errorMessage);
            subtractButton = new SubtractButton(firstNumber, secondNumber, result, operation, errorMessage);
            multiplyButton = new MultiplyButton(firstNumber, secondNumber, result, operation, errorMessage);
            divideButton = new DivideButton(firstNumber, secondNumber, result, operation, errorMessage);
            clearButton = new ClearButton(firstNumber, secondNumber, result, operation, errorMessage);

            // Add the input fields to each button so they're included in the form
            var firstNumInput = new Button.InputElementAtrributes(
                Button.InputElementAtrributes.PossibleAttributes.input, "number", "firstNum");
            firstNumInput.value = firstNumber.Get().ToString();
            firstNumInput.style = "display: none;"; // Hide the inputs since they're in the main form

            var secondNumInput = new Button.InputElementAtrributes(
                Button.InputElementAtrributes.PossibleAttributes.input, "number", "secondNum");
            secondNumInput.value = secondNumber.Get().ToString();
            secondNumInput.style = "display: none;"; // Hide the inputs since they're in the main form

            // Add hidden inputs to all operation buttons to capture the values
            addButton.AddFormElement(firstNumInput);
            addButton.AddFormElement(secondNumInput);
            
            subtractButton.AddFormElement(firstNumInput);
            subtractButton.AddFormElement(secondNumInput);
            
            multiplyButton.AddFormElement(firstNumInput);
            multiplyButton.AddFormElement(secondNumInput);
            
            divideButton.AddFormElement(firstNumInput);
            divideButton.AddFormElement(secondNumInput);
        }

        public override Dictionary<string, object> Render()
        {
            return new Dictionary<string, object>
            {
                ["title"] = "Calculator",
                ["firstNumber"] = firstNumber.Get().ToString(),
                ["secondNumber"] = secondNumber.Get().ToString(),
                ["result"] = result.Get().ToString(),
                ["operation"] = operation.Get(),
                ["errorMessage"] = errorMessage.Get(),
                ["addButton"] = addButton,
                ["subtractButton"] = subtractButton,
                ["multiplyButton"] = multiplyButton,
                ["divideButton"] = divideButton,
                ["clearButton"] = clearButton
            };
        }
    }
}