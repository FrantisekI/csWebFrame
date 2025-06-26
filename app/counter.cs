using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app
{
    // Session variable to store the counter value
    public class CounterValue : SessionVar<int>
    {
        public CounterValue(UserSession session) : base(session, 0)
        {
        }
    }

    // Button to increment the counter
    public class IncrementButton : Button
    {
        private CounterValue counterValue;

        public IncrementButton(CounterValue counter)
        {
            Name = "Increment";
            counterValue = counter;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            counterValue.Set(counterValue.Get() + 1);
        }
    }

    // Button to decrement the counter
    public class DecrementButton : Button
    {
        private CounterValue counterValue;

        public DecrementButton(CounterValue counter)
        {
            Name = "Decrement";
            counterValue = counter;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            counterValue.Set(counterValue.Get() - 1);
        }
    }

    // Button to reset the counter
    public class ResetButton : Button
    {
        private CounterValue counterValue;

        public ResetButton(CounterValue counter)
        {
            Name = "Reset";
            counterValue = counter;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            counterValue.Set(0);
        }
    }

    public class Counter : DefaultPage
    {
        private CounterValue counterValue;
        private IncrementButton incrementButton;
        private DecrementButton decrementButton;
        private ResetButton resetButton;
        private NavigationComponent navigation;

        public Counter(UserSession session) : base(session)
        {
            counterValue = new CounterValue(session);
            incrementButton = new IncrementButton(counterValue);
            decrementButton = new DecrementButton(counterValue);
            resetButton = new ResetButton(counterValue);
            navigation = new NavigationComponent("counter");
        }

        public override Dictionary<string, object> Render()
        {
            return new Dictionary<string, object>
            {
                ["title"] = "Counter Application",
                ["counterValue"] = counterValue.Get(),
                ["incrementButton"] = incrementButton,
                ["decrementButton"] = decrementButton,
                ["resetButton"] = resetButton,
                ["navigation"] = navigation
            };
        }
    }
}