﻿namespace ExplorerClient.Core.Network
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
        GetPrivateFileList,
        DeleteCommonFile,
        DeletePrivateFile,
        ChangeFileKey,
        CheckFile,
        RecalcHash,
        SendFile,
        CreateShareKey,
        GetNewFileList,
        GetReportList,
        ReciveNewFile,
        ShareFile,
        DeleteNewFile,
        GetUserList,
        Logout,
        CreateUser,
        MustChangeAllPass
    }
}