namespace MyChatLib
{
    public enum MessageType : byte
    {
        Unknown,
        /// <summary>
        /// For ( MessageType.InformationMessege ) you need use ( MessegeClientInfo enum )
        /// </summary>
        InformationMessege,
        Print,
        PublicChat,
        PrivateChat,
    }
    /// <summary>
    /// MessegeClientInfo enum is using with MessageType.InformationMessege enum
    /// </summary>
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