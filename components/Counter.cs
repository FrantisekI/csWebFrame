using csWebFrame;

namespace csWebFrame.components
{
    /// <summary>
    /// Example SessionVar for storing a counter value
    /// </summary>
    public class Counter : SessionVar<int>
    {
        public Counter(UserSession s, int val) : base(s, val)
        {
        }
    }
}
