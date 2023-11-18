using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameDBServer.MySqlHelpLib
{
    /// <summary>
    /// Điều khiển maping từ 1 dòng trong DB sang 1 T object
    /// </summary>
    public class DBMapper
    {
        //<columnName, FieldInfo or PropertyInfo>
        private Dictionary<string, MemberInfo> memberMappings = new Dictionary<string, MemberInfo>();

        public DBMapper(Type type)
        {
            MemberInfo[] members = type.GetMembers();

            foreach (MemberInfo member in members)
            {
                if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                    continue;

                Object[] attributes = member.GetCustomAttributes(typeof(DBMappingAttribute), false);

                if (null == attributes)
                    continue;

                DBMappingAttribute[] mappingAttrs = (DBMappingAttribute[])attributes;

                foreach (DBMappingAttribute mappingAttr in mappingAttrs)
                {
                    if (null == mappingAttr.ColumnName || "".Equals(mappingAttr.ColumnName))
                        continue;

                    memberMappings.Add(mappingAttr.ColumnName, member);
                }
            }
        }

        public MemberInfo getMemberInfo(string columnName)
        {
            MemberInfo member = null;
            memberMappings.TryGetValue(columnName, out member);
            return member;
        }
    }


}