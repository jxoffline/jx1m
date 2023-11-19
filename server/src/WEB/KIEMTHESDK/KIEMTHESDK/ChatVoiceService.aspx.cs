using KIEMTHESDK.Database;
using KIEMTHESDK.Models.Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KIEMTHESDK
{
    public partial class ChatVoiceService : System.Web.UI.Page
    {
        public static string CreateMD5(string input)
        {
            string result;
            using (MD5 md = MD5.Create())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(input);
                byte[] array = md.ComputeHash(bytes);
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    stringBuilder.Append(array[i].ToString("X2"));
                }
                result = stringBuilder.ToString();
            }
            return result;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            VoiceChatResponse _Rep = new VoiceChatResponse();

            try
            {

              //  LogManager.WriteLog(LogTypes.Error, "Revice Void Data....");

                byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
                int length = array.Length;


               // LogManager.WriteLog(LogTypes.Error, "Revice Void Data....LENG :" + length);

                ChatVoiceData AudioData = DataHelper.BytesToObject<ChatVoiceData>(array, 0, length);

                if (AudioData != null)
                {
                   // LogManager.WriteLog(LogTypes.Error, "Revice Void Data....FROMROLE :" + AudioData.FromRoleName);

                    string FileeName = CreateMD5(AudioData.FromRoleName + DateTime.Now.ToString());

                    string SavePath = Server.MapPath(@"~/AudioFile/");

                    byte[] AUDIODATA = AudioData.VoiceData.arrAudioChat;

                    File.WriteAllBytes(SavePath + FileeName + ".data", AUDIODATA);


                    LogManager.WriteLog(LogTypes.Error, "SAVE FILE....FROMROLE :" + SavePath + FileeName + ".data");

                    using (var db = new KiemTheDbEntities())
                    {
                        ChatData _Chat = new ChatData();
                        _Chat.Channel = AudioData.Channel;
                        _Chat.ChatID = AudioData.ChatID;
                        _Chat.ToRoleName = AudioData.ToRoleName;
                        _Chat.FromRoleName = AudioData.FromRoleName;

                        _Chat.ChatTime = DateTime.Now;

                        _Chat.FileName = FileeName;




                        db.ChatDatas.Add(_Chat);
                        db.SaveChanges();
                    }

                    _Rep.Status = 1;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "Bug ChatVoiceService::" + ex.ToString());
                _Rep.Status = -1;
            }

            byte[] array2 = DataHelper.ObjectToBytes<VoiceChatResponse>(_Rep);
            base.Response.OutputStream.Write(array2, 0, array2.Length);
            base.Response.OutputStream.Flush();
        }
    }
}