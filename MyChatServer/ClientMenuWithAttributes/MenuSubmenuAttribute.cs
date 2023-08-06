namespace MyChatServer
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class MenuSubmenuAttribute : Attribute
    {
        public MenuSubmenuAttribute(string title, int order)
        {
            Title = title;
            Order = order;
        }

        public string Title { get; }
        public int Order { get; }
    }
}