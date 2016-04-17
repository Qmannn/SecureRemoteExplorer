namespace ExplorerServer.Core.Network
{
    public enum Commands
    {
        Ok,
        Error,
        Login,
        GetUserState,
        ChangePass,
        SetShareStatus,
        PutCommonFile,
        GetCommonFile,
        PutPrivateFile,
        GetPrivateFile,
        GetCommonFileList,
        GetPrivateFileList
    }
}