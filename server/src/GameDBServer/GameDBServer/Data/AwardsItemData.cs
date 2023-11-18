using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class AwardsItemData
    {

        [ProtoMember(1)]
        public int Occupation = 0;

        [ProtoMember(2)]
        public int GoodsID = 0;


        [ProtoMember(3)]
        public int GoodsNum = 0;


        [ProtoMember(4)]
        public int Binding = 0;

        [ProtoMember(5)]
        public int Level = 0;


        [ProtoMember(6)]
        public int Quality = 0;

        [ProtoMember(7)]
        public string EndTime = "";

        [ProtoMember(8)]
        public int BornIndex = 0;


        [ProtoMember(9)]
        public int RoleSex = 0;
    }
}