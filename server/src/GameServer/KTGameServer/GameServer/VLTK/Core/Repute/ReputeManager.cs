using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Repute
{
    public class ReputeManager
    {
        public static string KT_Repute_XML = "Config/KT_Repute/repute.xml";

        public static Repute _ReputeConfig = new Repute();

        /// <summary>
        /// Loading all Drop
        /// </summary>
        public static void Setup()
        {
            Console.WriteLine("Loading KT_Repute Profile..");

            string Files = KTGlobal.GetDataPath(KT_Repute_XML);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(Repute));
                _ReputeConfig = serializer.Deserialize(stream) as Repute;
            }
        }


        public static int GetTotalEXp(int CampInput,int ClassInput,int LevelInput)
        {
            int SUM = 0;


            Camp _Camp = _ReputeConfig.Camp.Where(x => x.Id == CampInput).FirstOrDefault();


            if(_Camp!=null)
            {
                Class _Class = _Camp.Class.Where(x => x.Id == ClassInput).FirstOrDefault();

                List<Level> TotalLevel = _Class.Level.Where(x => x.Id < LevelInput).ToList();

                 SUM = TotalLevel.Sum(x => x.LevelUp);
            }

            return SUM;

        }

     

    


    }
}