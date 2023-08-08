namespace MyChatLib
{
    public enum MessageType : byte
    {
        Unknown,
        InformationMessege,
        Menu,
        PublicChat,
        PrivateChat,
    }
    public enum MessegeClientInfo : byte
    {
        Unknown,
        Allert,
        Clear,
        Succed,
        Information,
        MenuTrue,
        MenuFalse,
        ChangeName,
    }
}