using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.ProtocolBuffers;
using com.gexin.rp.sdk.dto;
using com.igetui.api.openservice;
using com.igetui.api.openservice.igetui;
using com.igetui.api.openservice.igetui.template;

namespace GameServer.PushMessage
{
    /// <summary>
    /// (个推)消息推送类
    /// </summary>
    public static class GetuiServerApiSDK
    {
        //参数设置 <-----参数需要重新设置----->
        private static string APPID = "kJCXNOHa3L7d1iCNwE4Nr9";                     //您应用的AppId
        private static string APPKEY = "NGtVs04iwz9yA7MEJ7eON4";                    //您应用的AppKey
        private static string MASTERSECRET = "YgcHKoEwAw7hoMzt9jtwv3";              //您应用的MasterSecret 
        private static string HOST = "http://sdk.open.api.igexin.com/apiex.htm";    //HOST：OpenService接口地址

        //PushMessageToSingle接口测试代码
        public static string PushMessageToSingle(string cid, string title, string messageText)
        {
            // 推送主类
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);

            /*消息模版：
                1.TransmissionTemplate:透传模板
                2.LinkTemplate:通知链接模板
                3.NotificationTemplate：通知透传模板
                4.NotyPopLoadTemplate：通知弹框下载模板
             */

            //TransmissionTemplate template =  TransmissionTemplateDemo();
            NotificationTemplate template = NotificationTemplateDemo(title, messageText);
            //LinkTemplate template = LinkTemplateDemo();
            //NotyPopLoadTemplate template = NotyPopLoadTemplateDemo();


            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = false;                         // 用户当前不在线时，是否离线存储,可选
            message.OfflineExpireTime = 1000 * 3600 * 12;            // 离线有效时间，单位为毫秒，可选
            message.Data = template;

            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = cid;

            string pushResult = push.pushMessageToSingle(message, target);
            return pushResult;
        }

        //PushMessageToList接口测试代码
        public static string PushMessageToList(List<string> cidList, string title, string messageText)
        {
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);

            ListMessage message = new ListMessage();
            /*消息模版：
                 1.TransmissionTemplate:透传功能模板
                 2.LinkTemplate:通知打开链接功能模板
                 3.NotificationTemplate：通知透传功能模板
                 4.NotyPopLoadTemplate：通知弹框下载功能模板
             */

            //TransmissionTemplate template =  TransmissionTemplateDemo();
            NotificationTemplate template = NotificationTemplateDemo(title, messageText); ;
            //LinkTemplate template = LinkTemplateDemo();
            //NotyPopLoadTemplate template = NotyPopLoadTemplateDemo();

            message.IsOffline = false;                         // 用户当前不在线时，是否离线存储,可选
            message.OfflineExpireTime = 1000 * 3600 * 12;            // 离线有效时间，单位为毫秒，可选
            message.Data = template;

            //设置接收者
            List<com.igetui.api.openservice.igetui.Target> targetList = new List<com.igetui.api.openservice.igetui.Target>();

            for (int i = 0; i < cidList.Count; i++)
            {
                com.igetui.api.openservice.igetui.Target target1 = new com.igetui.api.openservice.igetui.Target();
                target1.appId = APPID;
                target1.clientId = cidList[i];

                // 如需要，可以设置多个接收者
                //com.igetui.api.openservice.igetui.Target target2 = new com.igetui.api.openservice.igetui.Target();
                //target2.AppId = APPID;
                //target2.ClientId = "ddf730f6cabfa02ebabf06e0c7fc8da0";

                targetList.Add(target1);
                //targetList.Add(target2);
            }

            string contentId = push.getContentId(message);
            string pushResult = push.pushMessageToList(contentId, targetList);
            //System.Console.WriteLine("-----------------------------------------------");
            //System.Console.WriteLine("服务端返回结果:" + pushResult);
            return pushResult;
        }

        //pushMessageToApp接口测试代码
        public static string PushMessageToApp(string title, string messageText)
        {
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);

            AppMessage message = new AppMessage();
            /*消息模版：
                1.TransmissionTemplate:透传模板
                2.LinkTemplate:通知链接模板
                3.NotificationTemplate：通知透传模板
                4.NotyPopLoadTemplate：通知弹框下载模板
             */

            //TransmissionTemplate template =  TransmissionTemplateDemo();
            NotificationTemplate template = NotificationTemplateDemo(title, messageText);
            //LinkTemplate template = LinkTemplateDemo();
            //NotyPopLoadTemplate template = NotyPopLoadTemplateDemo();

            message.IsOffline = false;                         // 用户当前不在线时，是否离线存储,可选
            message.OfflineExpireTime = 1000 * 3600 * 12;            // 离线有效时间，单位为毫秒，可选
            message.Data = template;

            List<string> appIdList = new List<string>();
            appIdList.Add(APPID);

            List<string> phoneTypeList = new List<string>();    //通知接收者的手机操作系统类型
            //phoneTypeList.Add("ANDROID");
            //phoneTypeList.Add("IOS");

            List<string> provinceList = new List<string>();     //通知接收者所在省份
            //provinceList.Add("浙江");
            //provinceList.Add("上海");
            //provinceList.Add("北京");

            List<string> tagList = new List<string>();
            //tagList.Add("开心");

            message.AppIdList = appIdList;
            message.PhoneTypeList = phoneTypeList;
            message.ProvinceList = provinceList;
            message.TagList = tagList;


            string pushResult = push.pushMessageToApp(message);
            //System.Console.WriteLine("-----------------------------------------------");
            //System.Console.WriteLine("服务端返回结果：" + pushResult);
            return pushResult;
        }

        /*
         * 
         * 所有推送接口均支持四个消息模板，依次为透传模板，通知透传模板，通知链接模板，通知弹框下载模板
         * 注：IOS离线推送需通过APN进行转发，需填写pushInfo字段，目前仅不支持通知弹框下载功能
         *
         */
        //透传模板动作内容
        public static TransmissionTemplate TransmissionTemplateDemo(string messageText)
        {
            TransmissionTemplate template = new TransmissionTemplate();
            template.AppId = APPID;
            template.AppKey = APPKEY;
            template.TransmissionType = "1";            //应用启动类型，1：强制应用启动 2：等待应用启动
            template.TransmissionContent = messageText;  //透传内容
            //iOS推送需要的pushInfo字段
            //template.setPushInfo(actionLocKey, badge, message, sound, payload, locKey, locArgs, launchImage);
            //template.setPushInfo("", 4, "", "", "", "", "", "");
            return template;
        }

        //通知透传模板动作内容
        public static NotificationTemplate NotificationTemplateDemo(string title, string messageText)
        {
            NotificationTemplate template = new NotificationTemplate();
            template.AppId = APPID;
            template.AppKey = APPKEY;
            template.Title = title;         //通知栏标题
            template.Text = messageText;          //通知栏内容
            template.Logo = "";               //通知栏显示本地图片
            template.LogoURL = "";                    //通知栏显示网络图标

            template.TransmissionType = "1";          //应用启动类型，1：强制应用启动  2：等待应用启动
            template.TransmissionContent = "";   //透传内容
            //iOS推送需要的pushInfo字段
            //template.setPushInfo(actionLocKey, badge, message, sound, payload, locKey, locArgs, launchImage);

            template.IsRing = false;                //接收到消息是否响铃，true：响铃 false：不响铃
            template.IsVibrate = false;               //接收到消息是否震动，true：震动 false：不震动
            template.IsClearable = true;             //接收到消息是否可清除，true：可清除 false：不可清除
            return template;
        }

        //通知链接动作内容
        public static LinkTemplate LinkTemplateDemo(string title, string messageText, string url)
        {
            LinkTemplate template = new LinkTemplate();
            template.AppId = APPID;
            template.AppKey = APPKEY;
            template.Title = title;         //通知栏标题
            template.Text = messageText;          //通知栏内容
            template.Logo = "";               //通知栏显示本地图片
            template.LogoURL = "";  //通知栏显示网络图标，如无法读取，则显示本地默认图标，可为空
            template.Url = url;      //打开的链接地址

            //iOS推送需要的pushInfo字段
            //template.setPushInfo(actionLocKey, badge, message, sound, payload, locKey, locArgs, launchImage);

            template.IsRing = true;                 //接收到消息是否响铃，true：响铃 false：不响铃
            template.IsVibrate = true;               //接收到消息是否震动，true：震动 false：不震动
            template.IsClearable = true;             //接收到消息是否可清除，true：可清除 false：不可清除

            return template;
        }

        //通知弹框下载模板动作内容
        public static NotyPopLoadTemplate NotyPopLoadTemplateDemo()
        {
            NotyPopLoadTemplate template = new NotyPopLoadTemplate();
            template.AppId = APPID;
            template.AppKey = APPKEY;
            template.NotyTitle = "请填写通知标题";     //通知栏标题
            template.NotyContent = "请填写通知内容";   //通知栏内容
            template.NotyIcon = "icon.png";           //通知栏显示本地图片
            template.LogoURL = "http://www-igexin.qiniudn.com/wp-content/uploads/2013/08/logo_getui1.png";                    //通知栏显示网络图标

            template.PopTitle = "弹框标题";             //弹框显示标题
            template.PopContent = "弹框内容";           //弹框显示内容
            template.PopImage = "";                     //弹框显示图片
            template.PopButton1 = "下载";               //弹框左边按钮显示文本
            template.PopButton2 = "取消";               //弹框右边按钮显示文本

            template.LoadTitle = "下载标题";            //通知栏显示下载标题
            template.LoadIcon = "file://push.png";      //通知栏显示下载图标,可为空
            template.LoadUrl = "http://www.appchina.com/market/d/425201/cop.baidu_0/com.gexin.im.apk";//下载地址，不可为空

            template.IsActived = true;                  //应用安装完成后，是否自动启动
            template.IsAutoInstall = true;              //下载应用完成后，是否弹出安装界面，true：弹出安装界面，false：手动点击弹出安装界面

            template.IsBelled = true;                 //接收到消息是否响铃，true：响铃 false：不响铃
            template.IsVibrationed = true;               //接收到消息是否震动，true：震动 false：不震动
            template.IsCleared = true;             //接收到消息是否可清除，true：可清除 false：不可清除
            return template;
        }
    }
}
