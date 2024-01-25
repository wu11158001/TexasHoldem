using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TexasHoldemServer.SqlData
{
    class MySqlManager
    {
        /// <summary>
        /// 搜索資料庫
        /// </summary>
        /// <param name="mySqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="searchNames"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool SearchData(MySqlConnection mySqlConnection, string tableName, string[] searchNames, string[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"SELECT * FROM {tableName} WHERE {searchNames[0]} = @searchName0");
            for (int i = 1; i < searchNames.Length; i++)
            {
                sb.Append($" AND {searchNames[i]} = @searchName{i}");
            }
            MySqlCommand cmd = new MySqlCommand(sb.ToString(), mySqlConnection);

            for (int i = 0; i < values.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@searchName{i}", values[i]);
            }

            MySqlDataReader read = cmd.ExecuteReader();

            bool result = read.HasRows;
            read.Close();

            return result;
        }

        /// <summary>
        /// 插入資料
        /// </summary>
        /// <param name="mySqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="inserNames"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool InsertData(MySqlConnection mySqlConnection, string tableName, string[] inserNames, string[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"INSERT INTO holdem.{tableName} ({inserNames[0]}");
            for (int i = 1; i < inserNames.Length; i++)
            {
                sb.Append($",{inserNames[i]}");
            }
            sb.Append(") VALUES (@inserNames0");
            for (int i = 1; i < inserNames.Length; i++)
            {
                sb.Append($",@inserNames{i}");
            }
            sb.Append(")");

            try
            {
                MySqlCommand comd = new MySqlCommand(sb.ToString(), mySqlConnection);

                for (int i = 0; i < values.Length; i++)
                {
                    comd.Parameters.AddWithValue($"@inserNames{i}", values[i]);
                }

                comd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
