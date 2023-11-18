using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    /// <summary>
    /// 标准错误返回码
    /// </summary>
    public static class StdErrorCode
    {
        public const int Error_Success_No_Info = 0; //操作成功,无附加信息
        public const int Error_Success = 1; //操作成功完成
        public const int Error_Success_Bind = 2; //操作成功完成但结果为绑定物品
        public const int Error_Success_No_Change = 3; //操作成功完成但结果无变化(操作前已经是这个状态)
        public const int Error_Success_Wait = 4; //操作成功完成但需等待结果

        public const int Error_Invalid_DBID = -1; //无效的dbid
        public const int Error_Invalid_Index = -2; //无效的索引值
        public const int Error_Config_Fault = -3; //配置错误
        public const int Error_Data_Overdue = -4; //数据无效或已过期,请清除缓存重新操作
        public const int Error_Invalid_Operation = -5; //无效操作(洗炼无可洗练属性的装备或激活已激活洗炼的装备等)
        public const int Error_Goods_Not_Enough = -6; //物品不足
        public const int Error_Goods_Is_Using = -7; //物品在使用,不允许操作
        public const int Error_Goods_Not_Find = -8; //物品在使用,不允许操作
        public const int Error_JinBi_Not_Enough = -9; //金币不足
        public const int Error_ZuanShi_Not_Enough = -10; //钻石不足
        public const int Error_Operation_Faild = -11; //操作失败(传承失败等)
        public const int Error_Operation_Denied = -12; //拒绝操作(不符合操作条件)
        public const int Error_Type_Not_Match = -13; //拒绝操作(类型不匹配)
        public const int Error_MoneyType_Not_Select = -14; //未选择消耗钱类型
        public const int Error_DB_Faild = -15; //数据库操作失败
        public const int Error_No_Residue_Degree = -16; //无剩余次数
        public const int Error_BangZuan_Not_Enough = -17; //绑钻不足
        public const int Error_Invalid_Params = -18; //错误的参数
        public const int Error_Level_Limit = -19; //等级限制
        public const int Error_Not_Exist = -20; //不存在
        public const int Error_Denied_In_Current_Map = -21; //当前地图拒绝此操作
        public const int Error_Player_Count_Limit = -22; //参与者数量已满
        public const int Error_Level_Reach_Max_Level = -23;// 级别已达到最大值
        public const int Error_SpecJiFen_Not_Enough = -24; //专属活动充值积分不足
        public const int Error_Cannot_Have_Wish_Txt = -25; // 不可添加祝福寄语
        public const int Error_Wish_Txt_Length_Limit = -26; // 祝福寄语字数超出限制
        public const int Error_Cannot_Wish_Self = -27; // 不可祝福自己
        public const int Error_Wish_Player_Not_Exist = -28; // 被祝福玩家不存在
        public const int Error_Wish_Player_Not_Marry = -29; // 被祝福玩家未结婚
        public const int Error_Wish_Type_Is_In_CD = -30; // 祝福cd中
        public const int Error_Wish_In_Balance_Time = -31; // 赠送已结束，系统正在结算排名
        

        //登陆类和初始化错误
        public const int Error_10 = -10;//你已经被游戏管理员禁止登陆
        public const int Error_Invalid_In_KuaFu = -12; //拒绝操作(不符合操作条件)
        public const int Error_20 = -20;//系统检测您的角色使用加速
        public const int Error_30 = -30;//二级密码尚未验证
        public const int Error_40 = -40;//跨服时间同步中
        public const int Error_50 = -50;//系统检测您的角色使用外挂
        public const int Error_60 = -60;//返回登录界面
        public const int Error_70 = -70;//您的账号存在异常行为
        public const int Error_80 = -80;//系统检测您的角色交易异常

        //背包类
        public const int Error_BagNum_Not_Enough = -100; //包裹格子不足

        //奖励类
        public const int Error_Has_Get = -200; //已经领取
        public const int Error_Faild_No_Message = -201; //失败但不需提示

        //采集交付
        public const int Error_Has_Ownen_ShengBei = -300;
        public const int Error_Too_Far = -301;
        public const int Error_Other_Has_Get = -302;
        public const int Error_CaiJi_Break = -303; //采集中断

        //战盟类
        public const int Error_ZhanMeng_Not_In_ZhanMeng = -1000; //战盟不存在
        public const int Error_ZhanMeng_Not_Exist = -1001; //战盟不存在
        public const int Error_ZhanMeng_ShouLing_Only = -1002; //战盟首领才能进行此操作
        public const int Error_ZhanMeng_Is_Unqualified = -1003; //战盟没有(参战)资格
        public const int Error_ZhanMeng_Has_Bid_OtherSite = -1004; //战盟已经竞标了另一个名额
        public const int Error_ZhanMeng_ZhiWu_Not_Config = -1005; //角色的战盟职务未配置相应奖励（0,1,2,3,4）
        public const int Error_ZhanMeng_Not_Allowed_Change_LuoLanChengZhu = -1006; //罗兰城主持有期间不能委任其他成员为战盟首领
        public const int Error_ZhanMeng_Not_Allowed_Change_ShengYuChengZhu = -1007; //圣域城主持有期间不能委任其他成员为战盟首领

        public const int Error_ZhanMeng_Has_In_ZhanMeng = -1010; //已经在战盟中
        public const int Error_ZhanMeng_Duplicate_Name = -1011; //战盟名字已被占用

        //时间限制
        public const int Error_Not_In_valid_Time = -2001; //不在有效时间范围内
        public const int Error_Denied_In_Activity_Time = -2002; //活动时间禁止操作
        public const int Error_Time_Over = -2003; //可以操作的时间结束
        public const int Error_Time_Punish = -2004; //惩罚时间, 不可参加跨服副本
        public const int Error_Operate_Too_Fast = -2005; //您的操作太快，请稍候再试
        public const int Error_In_ZhanMeng_Time_Not_Enough = -2006; //加入战盟时间不足

        //时装类
        public const int Error_Is_Not_LuoLanChengZhu = -3001; //不是罗兰城主
        public const int Error_Is_Not_Married = -3002;        //没有结婚

        //跨服
        public const int Error_KuaFuFuBenNotExist = -4000; //跨服副本不存在(已结束)
        public const int Error_HasInQueue = -4001; //已经在匹配队列中
        public const int Error_HasInKuaFuFuBen = -4002; //已经在匹配中跨服副本
        public const int Error_Invalid_GameType = -4003; //跨服活动类型不正确
        public const int Error_Reach_Max_Level = -4004; //已经达到最高等级
        public const int Error_ZhanMeng_Has_SignUp = -4005; //战盟已经报名
        public const int Error_Game_Over = -4006; //战斗已结束


        //数据库错误
        public const int Error_DB_TimeOut = -10000; //数据库操作超时

        //服务器错误或连接错误
        public const int Error_Server_Busy = -11000; //服务器忙
        public const int Error_Server_Not_Registed = -11001; //服务器未注册
        public const int Error_Duplicate_Key_ServerId = -11002; //服务器ID重复
        public const int Error_Server_Internal_Error = -11003; //服务器内部错误
        public const int Error_Not_Implement = -11004; //此功能尚未实现(开放)
        public const int Error_Connection_Disabled = -11005; //连接被禁止
        public const int Error_Connection_Closing = -11006; //请重新连接这个服务器(等待上一个连接完全关闭)
        public const int Error_Redirect_Orignal_Server = -11007; //请尝试重新连接原来的服务器
        public const int Error_Token_Expired = -100; //登录Token已过期,请重新登录平台帐号(login_on2指令)
        public const int Error_Version_Not_Match = -2; //协议版本号不匹配,请更新游戏到最新版本(login_on2指令)
        public const int Error_Server_Connections_Limit = -100; //服务器连接数达到上限(login_on2指令),修改
        public const int Error_Version_Not_Match2 = -3; //协议版本号不匹配,请更新游戏到最新版本(login_on指令)
        public const int Error_Token_Expired2 = -1; //登录Token已过期,请重新登录平台帐号(login_on指令)
        public const int Error_Connection_Closing2 = -2; //请重新连接这个服务器(等待上一个连接完全关闭)

    }

}
