namespace MyChatLib
{
    public enum MessageType : byte
    {
        Unknown,
        InformationMessege,
        Menu,
        PublicChat,
        PrivateChat,
        ChangeNameSucced,
    }
    public enum MessegeClientInfo : byte
    {
        Allert,
        Clear,
        Succed,
        Information,
        MenuTrue,
        MenuFalse,
    }
}