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
        /// 檢測資料
        /// </summary>
        /// <param name="mySqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="searchNames"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool CheckData(MySqlConnection mySqlConnection, string tableName, string[] searchNames, string[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"SELECT * FROM {tableName} WHERE {searchNames[0]} = @searchName0");
            for (int i = 1; i < searchNames.Length; i++)
            {
                sb.Append($" AND {searchNames[i]} = @searchName{i}");
            }

            using (MySqlCommand cmd = new MySqlCommand(sb.ToString(), mySqlConnection))
            {
                for (int i = 0; i < values.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@searchName{i}", values[i]);
                }

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.HasRows;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
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

            using (MySqlCommand cmd = new MySqlCommand(sb.ToString(), mySqlConnection))
            {
                for (int i = 0; i < values.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@inserNames{i}", values[i]);
                }

                cmd.ExecuteNonQuery();
                return  true;
            }
        }

        /// <summary>
        /// 獲取資料
        /// </summary>
        /// <param name="mySqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="searchName"></param>
        /// <param name="searchNameValue"></param>
        /// <param name="dataNames"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetData(MySqlConnection mySqlConnection, string tableName, string searchName, string searchNameValue, string[] dataNames)
        {
            string sql = $"SELECT * FROM {tableName} WHERE {searchName} = @{searchName}";
            MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);

            cmd.Parameters.AddWithValue($"@{searchName}", searchNameValue);

            // 使用 MySqlDataReader 來獲取完整的結果集
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    for (int i = 0; i < dataNames.Length; i++)
                    {
                        dic.Add(dataNames[i], reader[dataNames[i]].ToString());
                    }
                    return dic;
                }
                else
                {
                    Console.WriteLine($"獲取資料錯誤!!!");
                    return null;
                }
            }
        }

        /// <summary>
        /// 修改資料
        /// </summary>
        /// <param name="mySqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="searchName"></param>
        /// <param name="searchNameValue"></param>
        /// <param name="reviseName"></param>
        /// <param name="reviseValue"></param>
        /// <returns></returns>
        public bool ReviseData(MySqlConnection mySqlConnection, string tableName, string searchName, string searchNameValue, string[] reviseName, string[] reviseValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{reviseName[0]} = @{reviseName[0]}");
            for (int i = 1; i < reviseName.Length; i++)
            {
                sb.Append($", {reviseName[i]} = @{reviseName[i]}");
            }

            string sql = $"UPDATE {tableName} SET {sb} WHERE {searchName} = @{searchName}";

            using (MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection))
            {
                cmd.Parameters.AddWithValue($"@{searchName}", searchNameValue);
                for (int i = 0; i < reviseValue.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@{reviseName[i]}", reviseValue[i]);
                }

                int reviseCount = cmd.ExecuteNonQuery();
                if (reviseCount != 0)
                {
                    Console.WriteLine("修改資料完成!");
                    return true;
                }
                else
                {
                    Console.WriteLine("修改資料錯誤!");
                    return false;
                }
            }
        }

    }
}
