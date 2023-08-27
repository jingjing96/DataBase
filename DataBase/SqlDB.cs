using System;
using System.Data;
using System.Data.SqlClient;

namespace DataBase
{
    public class SqlDB
    {
        #region 字段

        /// <summary>
        /// 实现连接对象的单例模式
        /// </summary>
        private static SqlConnection conn;

        #endregion

        #region 方法

        /// <summary>
        /// 获取可用连接对象（单例模式）
        /// </summary>
        /// <param name="ConnectionString">数据库的连接字符串</param>
        /// <returns>连接对象</returns>
        private static SqlConnection GetConnection(string ConnectionString)
        {
            try
            {
                if (conn == null)
                    conn = new SqlConnection(ConnectionString);
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
        /// <param name="ConnectionString">连接字符串</param>
        /// <param name="SqlString">给定的查询语句</param>
        /// <param name="SqlParameters">SQL语句的参数数组</param>
        /// <returns>查询结果数据表</returns>
        public static DataTable SelectDataTable(string ConnectionString, string SqlString, SqlParameter[] SqlParameters)
        {
            conn = GetConnection(ConnectionString);
            try
            {
                conn.Open();
                SqlCommand SqlCmd = new SqlCommand(SqlString, conn);
                if (SqlParameters != null)
                    foreach (SqlParameter para in SqlParameters)
                        SqlCmd.Parameters.Add(para);
                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(SqlCmd);
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
        /// <param name="ConnectionString">连接字符串</param>
        /// <param name="SqlString">用于生成Update语句的Select语句</param>
        /// <param name="SqlParameters">SQL语句的参数数组</param>
        public static void UpdateDataTable(DataTable dt, string ConnectionString, string SqlString, SqlParameter[] SqlParameters)
        {
            conn = GetConnection(ConnectionString);
            try
            {
                conn.Open();
                SqlCommand SqlCmd = new SqlCommand(SqlString, conn);
                if (SqlParameters != null)
                    foreach (SqlParameter para in SqlParameters)
                        SqlCmd.Parameters.Add(para);
                SqlDataAdapter adapter = new SqlDataAdapter(SqlCmd);
                SqlCommandBuilder cb = new SqlCommandBuilder(adapter);
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
        /// <param name="ConnectionString">连接字符串</param>
        /// <param name="SqlString">非查询语句</param>
        /// <param name="SqlParameters">SQL语句的参数数组</param>
        /// <returns>数据表中受影响的行数</returns>
        public static int ExecuteNonQuery(string ConnectionString, string SqlString, SqlParameter[] SqlParameters)
        {
            conn = GetConnection(ConnectionString);
            try
            {
                conn.Open();
                SqlCommand SqlCmd = new SqlCommand(SqlString, conn);
                if (SqlParameters != null)
                    foreach (SqlParameter para in SqlParameters)
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
        /// <param name="ConnectionString">连接字符串</param>
        /// <param name="SqlString">统计查询语句</param>
        /// <param name="SqlParameters">SQL语句的参数数组</param>
        /// <returns>Object类型的查询结果</returns>
        public static object ExecuteScaler(string ConnectionString, string SqlString, SqlParameter[] SqlParameters)
        {
            conn = GetConnection(ConnectionString);
            try
            {
                conn.Open();
                SqlCommand SqlCmd = new SqlCommand(SqlString, conn);
                if (SqlParameters != null)
                    foreach (SqlParameter para in SqlParameters)
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
        /// <param name="ConnectionString">连接字符串</param>
        /// <param name="SqlStrings">用于传递事务指令集的字符串数组</param>
        /// <param name="SqlParameters">事务指令集的参数数组</param>
        public static void ExecuteSqlTransaction(string ConnectionString, string[] SqlStrings, SqlParameter[][] SqlParameters)
        {
            conn = GetConnection(ConnectionString);
            try
            {
                conn.Open();
                SqlCommand SqlCmd = conn.CreateCommand();
                SqlTransaction SqlTran = conn.BeginTransaction();
                SqlCmd.Connection = conn;
                SqlCmd.Transaction = SqlTran;
                try
                {
                    for (int i = 0; i < SqlStrings.Length; i++)
                    {
                        SqlCmd.CommandText = SqlStrings[i];
                        if (SqlParameters[i] != null)
                            foreach (SqlParameter para in SqlParameters[i])
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