using KIEMTHESDK.Database;
using KIEMTHESDK.Models.Server.Data;
using Server.Tools;
using System;
using System.IO;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class GetVoiceChat : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ServerGetAudioChatData _ChatVoiceSendToCLient = new ServerGetAudioChatData();

            try
            {
                LogManager.WriteLog(LogTypes.Error, "Revice GETVOID");

                byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
                int length = array.Length;

                LogManager.WriteLog(LogTypes.Error, "Revice GETVOID LENG :" + length);

                GetVoiceFile AudioData = DataHelper.BytesToObject<GetVoiceFile>(array, 0, length);

                if (AudioData != null)
                {
                    LogManager.WriteLog(LogTypes.Error, "Revice GETVOID CHATID :" + AudioData.ChatID);

                    using (var db = new KiemTheDbEntities())
                    {
                        var find = db.ChatDatas.Where(x => x.ChatID == AudioData.ChatID).FirstOrDefault();

                        if (find != null)
                        {
                            string SavePath = Server.MapPath(@"~/AudioFile/") + find.FileName + ".data";

                            byte[] ByteArray = File.ReadAllBytes(SavePath);

                            _ChatVoiceSendToCLient.arrAudioChat = ByteArray;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "GetVoiceChat BUG :" + ex.ToString());
            }

            byte[] array2 = DataHelper.ObjectToBytes<ServerGetAudioChatData>(_ChatVoiceSendToCLient);

            LogManager.WriteLog(LogTypes.Error, "ChatVoiceData BUG :" + array2.ToString());

            base.Response.OutputStream.Write(array2, 0, array2.Length);
            base.Response.OutputStream.Flush();
        }
    }
}