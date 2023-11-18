using GameDBServer.Core;
using GameDBServer.DB;
using Server.Tools;
using System;
using System.Collections.Concurrent;

namespace GameDBServer.Logic.KT_ItemManager
{
    public class DelayUpdateManager
    {
        private static DelayUpdateManager instance = new DelayUpdateManager();

        public static DelayUpdateManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// Thực hiện add vào hàng đợi
        /// </summary>
        /// <param name="SqlQuerry"></param>
        public void AddItemProsecc(string SqlQuerry)
        {
            CardProsecc.Add(SqlQuerry);
        }

        public BlockingCollection<string> CardProsecc = new BlockingCollection<string>();

        public bool IsItemProseccSing = false;

        // Cứ 2s tick 1 lần
        private const long MaxDBRoleParamCmdSlot = (5 * 1000);

        public long LastUpdateItem = 0;

        public void DoUpdateSql()
        {
            long Now = TimeUtil.NOW();

            // Nếu đang xử lý dở thì thôi đợi lần sau
            if (IsItemProseccSing)
            {
                return;
            }

            if (Now - LastUpdateItem > MaxDBRoleParamCmdSlot)
            {
                LastUpdateItem = Now;

                IsItemProseccSing = true;

                //  Console.WriteLine("DEALY MYSQL UDPATE  ITEM ==>TOTALCOUNT :" + CardProsecc.Count);

                while (CardProsecc.Count > 0)
                {
                    var ItemGet = CardProsecc.Take();

                    try
                    {
                        using (MySqlUnity conn = new MySqlUnity())
                        {
                            // Console.WriteLine("DEALY MYSQL UDPATE  ITEM ==>TOTALCOUNT111111");
                            conn.ExecuteNonQueryBool(ItemGet);
                            //Console.WriteLine("DEALY MYSQL UDPATE  ITEM ==>TOTALCOUNT3333333");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.SQL, "[BUG]Exception=" + ex.ToString());
                    }
                }

                // Console.WriteLine("DEALY MYSQL UDPATE  ITEM ==>DONEEEE");
                //TODO THỰC HIỆN UPDATE Ở ĐÂY

                IsItemProseccSing = false;
                // THỰC HIỆN CÁC TRUY VẤN Ở ĐÂY ĐỂ FILL RA BẢNG
            }
        }
    }
}