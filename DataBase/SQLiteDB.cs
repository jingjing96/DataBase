using System;
using System.Data;
using System.Data.SQLite;

namespace DataBase
{
    public class SQLiteDB
    {
        #region 字段

        /// <summary>
        /// 实现连接对象的单例模式
        /// </summary>
        private static SQLiteConnection conn;

        #endregion

        #region 方法

        /// <summary>
        /// 获取可用连接对象（单例模式）
        /// </summary>
        /// <param name="DataFilePath">数据库文件的位置</param>
        /// <returns></returns>
        private static SQLiteConnection GetConnection(string DataFilePath)
        {
            try
            {
                SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder();
                csb.DataSource = DataFilePath;
                string conn_str = csb.ToString();
                if (conn == null)
                    conn = new SQLiteConnection(conn_str);
                return conn;
            }
            catch (Exception ex)
            {
                //conn.Dispose();
                conn = null;
                throw ex;
            }
        }

        /// <summary>
        /// 根据给定的查询语句，返回结果数据表
        /// </summary>
        /// <param name="DataFilePath">数据库文件的位置</param>
        /// <param name="SqlString">给定的查询语句</param>
        /// <param name="SqliteParameters">SQL语句的参数数组</param>
        /// <returns>查询结果数据表</returns>
        public static DataTable SelectDataTable(string DataFilePath, string SqlString, SQLiteParameter[] SqliteParameters)
        {
            conn = GetConnection(DataFilePath);
            try
            {
                conn.Open();
                SQLiteCommand SqlCmd = new SQLiteCommand(SqlString, conn);
                if (SqliteParameters != null)
                    foreach (SQLiteParameter para in SqliteParameters)
                        SqlCmd.Parameters.Add(para);
                DataTable dt = new DataTable();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(SqlCmd);
                adapter.Fill(dt);
                adapter.Dispose();
                SqlCmd.Parameters.Clear();
                SqlCmd.Dispose();
                conn.Close();
                //conn.Dispose();
                return dt;
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 将数据表更新到外部数据源 
        /// </summary>
        /// <param name="dt">回写的数据表内容</param>
        /// <param name="DataFilePath">数据库文件的位置</param>
        /// <param name="SqlString">用于生成Update语句的Select语句</param>
        /// <param name="SqliteParameters">SQL语句的参数数组</param>
        public static void UpdateDataTable(DataTable dt, string DataFilePath, string SqlString, SQLiteParameter[] SqliteParameters)
        {
            conn = GetConnection(DataFilePath);
            try
            {
                conn.Open();
                SQLiteCommand SqlCmd = new SQLiteCommand(SqlString, conn);
                if (SqliteParameters != null)
                    foreach (SQLiteParameter para in SqliteParameters)
                        SqlCmd.Parameters.Add(para);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(SqlCmd);
                SQLiteCommandBuilder cb = new SQLiteCommandBuilder(adapter);
                adapter.UpdateCommand = cb.GetUpdateCommand();
                adapter.Update(dt);
                cb.Dispose();
                adapter.Dispose();
                SqlCmd.Parameters.Clear();
                SqlCmd.Dispose();
                conn.Close();
                //conn.Dispose();
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 执行非查询语句，返回整型结果（数据表中受影响的行数）
        /// </summary>
        /// <param name="DataFilePath">数据库文件的位置</param>
        /// <param name="SqlString">非查询语句</param>
        /// <param name="SqliteParameters">SQL语句的参数数组</param>
        /// <returns>数据表中受影响的行数</returns>
        public static int ExecuteNonQuery(string DataFilePath, string SqlString, SQLiteParameter[] SqliteParameters)
        {
            conn = GetConnection(DataFilePath);
            try
            {
                conn.Open();
                SQLiteCommand SqlCmd = new SQLiteCommand(SqlString, conn);
                if (SqliteParameters != null)
                    foreach (SQLiteParameter para in SqliteParameters)
                        SqlCmd.Parameters.Add(para);
                int i = SqlCmd.ExecuteNonQuery();
                SqlCmd.Parameters.Clear();
                SqlCmd.Dispose();
                conn.Close();
                //conn.Dispose();
                return i;
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 执行统计查询，返回 Object 类型的结果
        /// <para>
        /// 注意：此方法的执行结果为 Object 类型，因此其返回值在使用时要进行类型转换
        /// </para>
        /// </summary>
        /// <param name="DataFilePath">数据库文件的位置</param>
        /// <param name="SqlString">统计查询语句</param>
        /// <param name="SqliteParameters">SQL语句的参数数组</param>
        /// <returns>Object类型的查询结果</returns>
        public static object ExecuteScaler(string DataFilePath, string SqlString, SQLiteParameter[] SqliteParameters)
        {
            conn = GetConnection(DataFilePath);
            try
            {
                conn.Open();
                SQLiteCommand SqlCmd = new SQLiteCommand(SqlString, conn);
                if (SqliteParameters != null)
                    foreach (SQLiteParameter para in SqliteParameters)
                        SqlCmd.Parameters.Add(para);
                object i = SqlCmd.ExecuteScalar();
                SqlCmd.Parameters.Clear();
                SqlCmd.Dispose();
                conn.Close();
                //conn.Dispose();
                return i;
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="DataFilePath">数据库文件的位置</param>
        /// <param name="SqlStrings">用于传递事务指令集的字符串数组</param>
        /// <param name="SqliteParameters">事务指令集的参数数组</param>
        public static void ExecuteSqliteTransaction(string DataFilePath, string[] SqlStrings, SQLiteParameter[][] SqliteParameters)
        {
            conn = GetConnection(DataFilePath);
            try
            {
                conn.Open();
                SQLiteCommand SqlCmd = conn.CreateCommand();
                SQLiteTransaction SqlTran = conn.BeginTransaction();
                SqlCmd.Transaction = SqlTran;
                try
                {
                    for (int i = 0; i < SqlStrings.Length; i++)
                    {
                        SqlCmd.CommandText = SqlStrings[i];
                        if (SqliteParameters[i] != null)
                            foreach (SQLiteParameter para in SqliteParameters[i])
                                SqlCmd.Parameters.Add(para);
                        SqlCmd.ExecuteNonQuery();
                    }
                    SqlTran.Commit();
                }
                catch (Exception ex)
                {
                    SqlTran.Rollback();
                    throw new Exception(ex.Message);
                }
                finally
                {
                    SqlTran.Dispose();
                    SqlCmd.Parameters.Clear();
                    SqlCmd.Dispose();
                    conn.Close();
                    //conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
