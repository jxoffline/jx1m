namespace GameServer.KiemThe.Core.Activity.CardMonth
{

    public enum YueKaError
    {
        YK_Success,
        YK_CannotAward_HasNotYueKa,
        YK_CannotAward_DayHasPassed,
        YK_CannotAward_AlreadyAward,
        YK_CannotAward_TimeNotReach,
        YK_CannotAward_BagNotEnough,
        YK_CannotAward_ParamInvalid,
        YK_CannotAward_ConfigError,
        YK_CannotAward_DBError,
    }
}