using GameDBServer.Logic;
using GameDBServer.MySqlHelpLib;
using MySQLDriverCS;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace GameDBServer.DB
{
    /// <summary>
    /// Class thực hiện khởi tạo giao thức tương tác với mysql như thêm sửa xóa
    /// Tự động giải phóng và trả về kết nối cho pool
    /// Đẻ ra để hoạt động cùng using(xxx)
    /// </summary>
    public class MySqlUnity : IDisposable
    {
        public MySQLConnection DbConn = null;

        // Global value để tránh trường hợp quên close reader
        // Thì khi dispose sẽ tự close
        private MySQLDataReader _MySQLDataReader;

        public static bool LogSQLString = true;

        private bool m_disposed = false;

        /// <summary>
        /// Lấy ra 1 kết nối để thực hiện các truy vấn liên quan
        /// </summary>
        public MySqlUnity()
        {
            _MySQLDataReader = null;
            DbConn = DBManager.getInstance().DBConns.PopDBConnection();
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Khi không dùng nữa thì tự trả lại một kết nối
        /// Rất có tác dụng khi sử dụng cấu trúc using()
        /// Ngoài vòng using tự call tới dispose
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!m_disposed)
            {
                if (isDisposing)
                {
                    if (_MySQLDataReader != null && !_MySQLDataReader.IsClosed)
                    {
                        _MySQLDataReader.Close();
                        _MySQLDataReader = null;
                    }

                    DBManager.getInstance().DBConns.PushDBConnection(DbConn);
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// Logs ra cấu trúc mysql nếu thích
        /// </summary>
        /// <param name="sqlText"></param>
        private void LogSql(string sqlText)
        {
            if (LogSQLString)
            {
                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", sqlText), EventLevels.Important);
            }
        }

        /// <summary>
        /// Thực thi q querry trả về bool
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public bool ExecuteNonQueryBool(string sql, int commandTimeout = 0)
        {
            return ExecuteNonQuery(sql, commandTimeout) >= 0;
        }

        /// <summary>
        /// Thực thi 1 querry
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, int commandTimeout = 0)
        {
            int result = -1;

            try
            {
                using (MySQLCommand cmd = new MySQLCommand(sql, DbConn))
                {
                   
                    if (commandTimeout > 0)
                    {
                        cmd.CommandTimeout = commandTimeout;
                    }

                    result = cmd.ExecuteNonQuery();
                    LogSql(sql);
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("SQL BUGGGG: {0}\r\n{1}", sql, ex.ToString()));
                LogManager.WriteLog(LogTypes.Error, string.Format("BUG SQL: {0}", sql));
                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Lấy ra 1 giá trị kiểu int
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public int GetSingleInt(string sql, int commandTimeout = 0, params MySQLParameter[] cmdParms)
        {
            object obj = GetSingle(sql, commandTimeout, cmdParms);
            return Convert.ToInt32(obj.ToString());
        }

        /// <summary>
        /// Lấy ra 1 giá trị kiểu long
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public long GetSingleLong(string sql, int commandTimeout = 0, params MySQLParameter[] cmdParms)
        {
            object obj = GetSingle(sql, commandTimeout, cmdParms);
            return Convert.ToInt64(obj.ToString());
        }

        /// <summary>
        /// Đọc ra recore theo pram truyền vào
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public object GetSingle(string sql, int commandTimeout = 0, params MySQLParameter[] cmdParms)
        {
            try
            {
                using (MySQLCommand cmd = new MySQLCommand(sql, DbConn))
                {
                    if (commandTimeout > 0)
                    {
                        cmd.CommandTimeout = commandTimeout;
                    }

                    if (cmdParms.Length > 0)
                    {
                        PrepareCommand(cmd, DbConn, null, sql, cmdParms);
                    }

                    object obj = cmd.ExecuteScalar();
                    if (cmdParms.Length > 0)
                    {
                        cmd.Parameters.Clear();
                    }

                    LogSql(sql);
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("执行SQL异常: {0}\r\n{1}", sql, ex.ToString()));
                LogManager.WriteLog(LogTypes.Error, string.Format("写入数据库失败: {0}", sql));
            }

            return null;
        }

        /// <summary>
        /// Thực thi đọc dữ liệu theo querry
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public MySQLDataReader ExecuteReader(string sql, params MySQLParameter[] cmdParms)
        {
            try
            {
                using (MySQLCommand cmd = new MySQLCommand(sql, DbConn))
                {
                    if (cmdParms.Length > 0)
                    {
                        PrepareCommand(cmd, DbConn, null, sql, cmdParms);
                    }

                    MySQLDataReader myReader = cmd.ExecuteReaderEx();
                    if (cmdParms.Length > 0)
                    {
                        cmd.Parameters.Clear();
                    }

                    _MySQLDataReader = myReader;
                    LogSql(sql);

                    return myReader;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("SQL: {0}\r\n{1}", sql, ex.ToString()));
                LogManager.WriteLog(LogTypes.Error, string.Format("SQL: {0}", sql));
            }

            return null;
        }

        /// <summary>
        /// Call để buidl command
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        private static void PrepareCommand(MySQLCommand cmd, MySQLConnection conn, MySQLTransaction trans, string cmdText, MySQLParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (MySQLParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// Set value khi mapping object
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="obj"></param>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        private void setValue(DBMapper mapper, Object obj, string columnName, Object columnValue)
        {
            MemberInfo member = mapper.getMemberInfo(columnName);

            if (null == member)
                return;

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)member;

                    if (field.FieldType.Equals(typeof(System.Int64)) && columnValue.GetType().Equals(typeof(System.String)))
                        columnValue = Convert.ToInt64(columnValue);
                    if (field.FieldType.Equals(typeof(System.Byte)) && (columnValue.GetType().Equals(typeof(System.String)) || columnValue.GetType().Equals(typeof(System.Int16)) || columnValue.GetType().Equals(typeof(System.Int32)) || columnValue.GetType().Equals(typeof(System.Int64))))
                        columnValue = Convert.ToByte(columnValue);
                    if (field.FieldType.Equals(typeof(System.String)) && (columnValue.GetType().Equals(typeof(byte[])) || columnValue.GetType().Equals(typeof(System.Byte[]))))
                    {
                        byte[] _columnValue = (byte[])columnValue;
                        columnValue = Convert.ToString(new UTF8Encoding().GetString(_columnValue, 0, _columnValue.Length));
                    }

                    field.SetValue(obj, columnValue);
                    break;

                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)member;
                    if (property.PropertyType.Equals(typeof(System.Int64)) && columnValue.GetType().Equals(typeof(System.String)))
                        columnValue = Convert.ToInt64(columnValue);
                    if (property.PropertyType.Equals(typeof(System.Byte)) && (columnValue.GetType().Equals(typeof(System.String)) || columnValue.GetType().Equals(typeof(System.Int16)) || columnValue.GetType().Equals(typeof(System.Int32)) || columnValue.GetType().Equals(typeof(System.Int64))))
                        columnValue = Convert.ToByte(columnValue);
                    if (property.PropertyType.Equals(typeof(System.String)) && columnValue.GetType().Equals(typeof(System.Byte[])))
                        columnValue = Convert.ToString(columnValue);
                    property.SetValue(obj, columnValue, null);
                    break;
            }
        }

        /// <summary>
        /// Sử dụng để trả về 1 object T dựa trên database mapping thông qua câu lệnh truy vấn select
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public T ReadObject<T>(string sql)
        {
            T obj = default(T);

            //Mapper all database
            DBMapper mapper = new DBMapper(typeof(T));

            try
            {
                using (MySQLCommand cmd = new MySQLCommand(sql, DbConn))
                {
                    MySQLDataReader reader = cmd.ExecuteReaderEx();

                    int columnNum = reader.FieldCount;

                    while (reader.Read())
                    {
                        int index = 0;

                        for (int i = 0; i < columnNum; i++)
                        {
                            int _index = index++;
                            string columnName = reader.GetName(_index);
                            Object columnValue = reader.GetValue(_index);

                            if (null == obj)
                                obj = Activator.CreateInstance<T>();

                            setValue(mapper, obj, columnName, columnValue);
                        }

                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("Sql BUG: {0}\r\n{1}", sql, ex.ToString()));
                LogManager.WriteLog(LogTypes.Error, string.Format("SQL BUG : {0}", sql));
            }

            return obj;
        }

        /// <summary>
        /// Trả về 1 list object T dựa trên Datatable truyền vào
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public List<T> CreateListFromTable<T>(DataTable tbl) where T : new()
        {
            // define return list
            List<T> lst = new List<T>();

            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(CreateItemFromRow<T>(r));
            }

            // return the list
            return lst;
        }

        /// <summary>
        /// Tạo ra 1 object trên 1 dòng dữ liệu của dataROW
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T CreateItemFromRow<T>(DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return
            return item;
        }

        /// <summary>
        /// Set giá trị cho object trên 1 dòng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="row"></param>
        public static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);


                if (p.PropertyType.Equals(typeof(System.Int64)))
                {
                    p.SetValue(item, Int64.Parse(row[c].ToString()), null);
                }
                else if (p.PropertyType.Equals(typeof(System.Int32)))
                {
                    p.SetValue(item, Int32.Parse(row[c].ToString()), null);
                }
                else if (p.PropertyType.Equals(typeof(System.Byte)))
                {
                    p.SetValue(item, Convert.ToByte(row[c].ToString()), null);
                }
                else
                {
                    if (p != null && row[c] != DBNull.Value)
                    {
                        p.SetValue(item, row[c], null);
                    }
                }

                // if exists, set the value
            }
        }
        /// <summary>
        /// Trả về 1 data Table
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable ReadSqlToTable(string sql)
        {
            DataTable dt = new DataTable();

            try
            {
                using (MySQLCommand cmd = new MySQLCommand(sql, DbConn))
                {
                    MySQLDataAdapter da = new MySQLDataAdapter(cmd);

                    da.Fill(dt);

                    return dt;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("Sql BUG: {0}\r\n{1}", sql, ex.ToString()));
                LogManager.WriteLog(LogTypes.Error, string.Format("Sql BUG: {0}", sql));
            }

            return dt;
        }
    }
}