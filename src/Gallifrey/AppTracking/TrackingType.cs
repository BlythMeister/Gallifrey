﻿namespace Gallifrey.AppTracking
{
    public enum TrackingType
    {
        AppLoad,
        ExportOccured,
        UpdateCheck,
        UpdateCheckManual,
        AppClose,
        DailyHearbeat,
        OptOut,
        TimerAdded,
        TimerDeleted,
        PayPalClick,
        GitHubClick,
        ContactClick,
        LockedTimerAdd,
        SearchLoad,
        SearchFilter,
        SearchText,
        InformationShown,
        JiraConnectCloudRest,
        JiraConnectCloudSoap,
        JiraConnectCloudRestWithTempo,
        JiraConnectSelfhostRestWithTempo,
        JiraConnectSelfhostRest,
        JiraConnectSelfhostSoap,
        AutoUpdateInstalled,
        ManualUpdateRestart,
        ExportAll,
        ShowRunningTimer,
        MultipleInstancesRunning,
        SettingsMissing,
        NoInternet,
        ConnectionError
    }
}
