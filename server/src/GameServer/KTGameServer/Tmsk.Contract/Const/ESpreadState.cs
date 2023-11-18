using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    public enum ESpreadState
    {
        ETelMore =  -36,//验证次数
        ETelCodeOutTime = -35,  //验证码过期
        ETelCodeWrong = -34,  //验证码错误
        ETelCodeGet = -33,  //验证码获取失败
        ETelBind = -32,  //手机已经绑定
        ETelWrong = -31,  //手机号错误
        ETelNull = -30,  //手机号为空

        ESpreadIsSign = -21,  //已注册推荐人
        ESpreadNo = -20,  //不是推广员

        EVerifyMore = -16,  //验证次数过多，禁止验证24小时
        EVerifySelf = -15,  //推荐人是自己
        EVerifyCodeWrong = -14,  //推荐码错误
        EVerifyOutTime = -13,  //超过推荐日期
        EVerifyCodeHave = -12,  //已经验证
        EVerifyCodeNull = -11,  //验证为空
        EVerifyNo = -10,  //未验证推荐人

        EServer = -5,   //服务器错误
        ENoAward = -4,   //没有奖励可以领取
        ENoBag = -3,   //背包已满
        ENoOpen = -2,   //功能未开启
        Fail = -1,   //失败
        Default = 0,    //默认
        Success = 1,    //成功
    }
}
