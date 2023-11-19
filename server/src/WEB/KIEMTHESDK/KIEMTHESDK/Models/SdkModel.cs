using ProtoBuf;
using System;
using System.Collections.Generic;

namespace KIEMTHESDK.Models
{
    public class IOSResp
    {
        public string status { get; set; }
        public int error { get; set; }
        public string msg { get; set; }
        public int pack { get; set; }
    }

    public class Item
    {
        public string itemRefId
        {
            get;
            set;
        }

        public int count
        {
            get;
            set;
        }
    }

    public class Data
    {
        public List<Item> listitem
        {
            get;
            set;
        }
    }

    public class TraVe
    {
        public string description { get; set; }
        public int code { get; set; }
    }

    public class InApp
    {
        public string quantity { get; set; }
        public string product_id { get; set; }
        public string transaction_id { get; set; }
        public string original_transaction_id { get; set; }
        public string purchase_date { get; set; }
        public string purchase_date_ms { get; set; }
        public string purchase_date_pst { get; set; }
        public string original_purchase_date { get; set; }
        public string original_purchase_date_ms { get; set; }
        public string original_purchase_date_pst { get; set; }
        public string is_trial_period { get; set; }
    }

    public class Receipt
    {
        public string receipt_type { get; set; }
        public UInt64 adam_id { get; set; }
        public UInt64 app_item_id { get; set; }
        public string bundle_id { get; set; }
        public string application_version { get; set; }
        public UInt64 download_id { get; set; }
        public UInt64 version_external_identifier { get; set; }
        public string receipt_creation_date { get; set; }
        public string receipt_creation_date_ms { get; set; }
        public string receipt_creation_date_pst { get; set; }
        public string request_date { get; set; }
        public string request_date_ms { get; set; }
        public string request_date_pst { get; set; }
        public string original_purchase_date { get; set; }
        public string original_purchase_date_ms { get; set; }
        public string original_purchase_date_pst { get; set; }
        public string original_application_version { get; set; }
        public List<InApp> in_app { get; set; }
    }

    public class PayRoot
    {
        public String Payload;

        public String Store;

        public String TransactionID;
    }

    public class IOSPAYREP
    {
        public int status { get; set; }
        public string environment { get; set; }
        public Receipt receipt { get; set; }
    }

    public class IOSREQUEST
    {
        public string jsonbanlen { get; set; }
        public string access_token { get; set; }
        public string server_id { get; set; }
        public string role_id { get; set; }
        public string app_id { get; set; }
        public string product_id { get; set; }
    }

    public class InstallRequest
    {
        public string device_info { get; set; }

        public string device_token { get; set; }

        public string userId { get; set; }
    }

    public class AppInfoResp
    {
        public string facebook_link { get; set; }
        public string news_link { get; set; }
        public string gifcode_link { get; set; }
        public string vkl_link { get; set; }
        public string game_name { get; set; }
        public string home_page { get; set; }
        public string icon_game { get; set; }
        public int status { get; set; }
        public Update update { get; set; }
        public string image_age { get; set; }
        public string warning_time_message { get; set; }
    }

    public class Update
    {
        public int status { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string link { get; set; }
        public int force { get; set; }
    }

    public class TmpLoginDic
    {
        public string LoginName { get; set; }

        public string DeviceID { get; set; }

        public int CountFail { get; set; }

        public DateTime LockTime { get; set; }
    }

    public class CardRep
    {
        public string CardTransID { get; set; }

        public int Amount { get; set; }

        public int NeedCallBack { get; set; }

        public int Status { get; set; }
    }

    public class DauRequest
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string LoginType { get; set; }
    }

    public class GetAppsRequest
    {
        public string bundleid { get; set; }

        public string app_id { get; set; }

        public string gver { get; set; }

        public string sdkver { get; set; }

        public string device_id_vcc { get; set; }
    }

    public class LoginRequest
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string device_id_vcc { get; set; }
    }

    public class CardType
    {
        public string value { get; set; }
        public string display { get; set; }
    }

    public class PayPack
    {
        public string order_info { get; set; }
        public string image { get; set; }
        public string price { get; set; }
        public string point { get; set; }
        public string old_point { get; set; }
        public string old_price { get; set; }
        public int? no_enough { get; set; }
        public int? buy_scoin { get; set; }
    }

    public class PayResquest
    {
        public string gver { get; set; }
        public string sdkver { get; set; }
        public string access_token { get; set; }
        public string device_id_vcc { get; set; }
    }

    public class PayList
    {
        public string title { get; set; }
        public string pay_type { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string bonus { get; set; }
        public string bonus_message { get; set; }

        public List<CardType> card_values { get; set; }
        public List<CardType> card_types { get; set; }
        public List<PayPack> list { get; set; }
    }

    public class DeviceInfo
    {
        public string app_version_code { get; set; }
        public string app_version_name { get; set; }
        public string phone_model { get; set; }
        public string brand { get; set; }
        public string product { get; set; }
        public string android_version { get; set; }
        public string total_mem_size { get; set; }
        public string available_mem_size { get; set; }
        public string total_ram_size { get; set; }
        public string available_ram_size { get; set; }
        public string resolution { get; set; }
        public string device_id { get; set; }
        public string mac_address { get; set; }
        public string imei { get; set; }
    }

    public class SidResponse
    {
        public string errmsg { get; set; }
        public int code { get; set; }
        public int serverId { get; set; }
        public string identityId { get; set; }
        public string identityName { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public string tstamp { get; set; }
        public string sign { get; set; }
    }

    public class RecoveryRequest
    {
        public string RecoveryAccount { get; set; }

        public string device_id_vcc { get; set; }
    }

    public class RegisterRequest
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string device_id_vcc { get; set; }
    }

    public class KQNAP
    {
        public int ErrorCode { get; set; }

        public string Msg { get; set; }
    }

    public class PayReponse
    {
        public string status { get; set; }
        public int error_code { get; set; }
        public string message { get; set; }
    }

    public class PayPacket
    {
        public string order_info { get; set; }
        public string md5 { get; set; }
        public int money { get; set; }
        public string app_id { get; set; }
        public string areaid { get; set; }
        public string roleid { get; set; }
        public string time { get; set; }
        public string tokenpay { get; set; }
        public string access_token { get; set; }
        public string extras_id { get; set; }

        public string server_id { get; set; }
    }

    public class CardRechageRequest
    {
        public string gver { get; set; }
        public string sdkver { get; set; }
        public string access_token { get; set; }
        public string device_id_vcc { get; set; }

        public string CardType { get; set; }

        public int CardValue { get; set; }

        public string CardSeri { get; set; }

        public string CardCode { get; set; }
    }

    public class PayPacketRequest
    {
        public string gver { get; set; }
        public string sdkver { get; set; }
        public string access_token { get; set; }
        public string device_id_vcc { get; set; }

        public string OderInfo { get; set; }
    }

    public class PayRequestRep
    {
        public string status { get; set; }

        public string errorcode { get; set; }

        public string Trans_ID { get; set; }

        public string time { get; set; }
        public string msg { get; set; }
        public string tokensing { get; set; }

        public string order_info { get; set; }
        public string data { get; set; }
    }

    public class GoogleVerifyData
    {
        public string order_data { get; set; }
        public string signature { get; set; }
    }

    public class PurcharsData
    {
        public string orderId { get; set; }
        public string packageName { get; set; }
        public string productId { get; set; }
        public long purchaseTime { get; set; }
        public int purchaseState { get; set; }
        public string developerPayload { get; set; }
        public string purchaseToken { get; set; }
    }

    [ProtoContract]
    public class RecipeVerify
    {
        /// <summary>
        /// Tên thằng mua gói
        /// </summary>
        ///
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tài khoản thằng mua gói
        /// </summary>
        ///
        [ProtoMember(2)]
        public string UserToken { get; set; }

        /// <summary>
        /// Mã giao dịch của gói mua
        /// <summary>
        /// Mã của gói mua lúc đầu
        /// </summary>
        ///
        [ProtoMember(3)]
        public string TransID { get; set; }

        /// <summary>
        ///Toàn bộ chuỗi json của google đẩy lên đây
        /// </summary>
        ///
        [ProtoMember(4)]
        public string PurchaseData { get; set; }

        [ProtoMember(5)]
        public int ServerID { get; set; }
    }

    [ProtoContract]
    public class PaymentVerifyRep
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public string ProductBuy { get; set; }
    }

    [ProtoContract]
    public class GiftCodeRep
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public string GiftItem { get; set; }
    }

    [ProtoContract]
    public class KTCoinRequest
    {
        [ProtoMember(1)]
        public int Type { get; set; }

        [ProtoMember(2)]
        public int UserID { get; set; }

        [ProtoMember(3)]
        public int Value { get; set; }

        [ProtoMember(4)]
        public int RoleID { get; set; }

        [ProtoMember(5)]
        public string RoleName { get; set; }

        [ProtoMember(6)]
        public int SeverID { get; set; }
    }

    [ProtoContract]
    public class KTCoinResponse
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public int Value { get; set; }
    }



    [ProtoContract]
    public class GiftCodeRequest
    {
        [ProtoMember(1)]
        public int RoleActive { get; set; }

        [ProtoMember(2)]
        public string CodeActive { get; set; }

        [ProtoMember(3)]
        public int ServerID { get; set; }
    }

    [ProtoContract]
    public class PaymentRequestRep
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public string ProductBuy { get; set; }

        [ProtoMember(4)]
        public string TransID { get; set; }
    }

    [ProtoContract]
    public class UpdateServerModel
    {
        /// <summary>
        /// Status này trả về trạng thái của gameserver
        /// 1 : Bảo trì
        /// 2 : Đang Đầy
        /// 3 : Gần Đầy
        /// 4 : Tốt
        /// </summary>
        [ProtoMember(1)]
        public int Status { get; set; }

        /// <summary>
        /// Trạng thái Update 
        /// </summary>
        /// 
        [ProtoMember(2)]
        public string NotifyUpdate { get; set; }


        /// <summary>
        /// Đây là máy chủ nào
        /// </summary>
        [ProtoMember(3)]
        public int SeverID { get; set; }

    }

    [ProtoContract]
    public class PaymentRequest
    {
        /// <summary>
        /// Tên thằng mua gói
        /// </summary>
        ///
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tài khoản thằng mua gói
        /// </summary>
        ///

        [ProtoMember(2)]
        public string UserToken { get; set; }

        /// <summary>
        /// Mã giao dịch của gói mua
        /// </summary>
        ///
        [ProtoMember(3)]
        public string TransID { get; set; }

        /// <summary>
        /// Tên gói mua
        /// </summary>
        ///
        [ProtoMember(4)]
        public string PackageName { get; set; }

        /// <summary>
        /// ServerID để biết server nào mà còn gọi SERVICE ADD ĐỒNG
        /// </summary>
        ///
        [ProtoMember(5)]
        public int ServerID { get; set; }

        /// <summary>
        /// DEVICE ID Của thiết bị
        /// </summary>
        ///
        [ProtoMember(6)]
        public string DeviceID { get; set; }

        [ProtoMember(7)]
        public string PlatForm { get; set; }
    }
}