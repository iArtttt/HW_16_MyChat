namespace MyChatServer
{
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class MenuActionAttribute : Attribute
    {
        public MenuActionAttribute(string title, int order)
        {
            Title = title;
            Order = order;
        }
        public MenuActionAttribute(string title, int order, string description)
        {
            Title = title;
            Order = order;
            Description = description;
        }

        public string Title { get; }
        public int Order { get; }
        public string Description { get; }
    }
}