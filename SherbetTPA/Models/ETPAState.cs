namespace SherbetTPA.Models
{
    public enum ETPAState
    {
        Waiting_Accept,
        Waiting_Teleport,
        Aborted_PlayerDisconnect,
        Aborted_PlayerMoved,
        Aborted_TargetDenied,
        Aborted_SenderAborted,
        Accepted,
        Finished,
        Teleport,
        TimedOut
    }
}