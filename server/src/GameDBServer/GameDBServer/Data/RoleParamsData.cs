using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class RoleParamsData
    {

        [ProtoMember(1)]
        public string ParamName = "";


        [ProtoMember(2)]
        public string ParamValue = "";

        public long UpdateFaildTicks;


        public RoleParamType ParamType;
    }


    public class RoleParamType
    {
        public enum ValueTypes
        {
            Normal,
            Char128,
            Long,
        }

        public readonly string VarName;
        public readonly string ParamName;
        public readonly string TableName;
        public readonly string IdxName;
        public readonly string ColumnName;
        public readonly int ParamIndex;
        public readonly int IdxKey;
        public readonly int Type;

        public readonly string KeyString;

        public RoleParamType(string varName, string paramName, string tableName, string idxName, string columnName, int idxKey, int paramIndex, int type)
        {
            VarName = varName;
            ParamName = paramName;
            TableName = tableName;
            IdxName = idxName;
            ColumnName = columnName;
            IdxKey = idxKey;
            ParamIndex = paramIndex;
            Type = type;
            if (Type > 0)
            {
                KeyString = IdxKey.ToString();
            }
            else
            {
                KeyString = "\'" + ParamName + '\'';
            }
        }
    }
}