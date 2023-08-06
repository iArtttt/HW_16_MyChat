namespace MyChatServer
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class MenuActionAttribute : Attribute
    {
        public MenuActionAttribute(string title, int order)
        {
            Title = title;
            Order = order;
        }

        public string Title { get; }
        public int Order { get; }
    }
}