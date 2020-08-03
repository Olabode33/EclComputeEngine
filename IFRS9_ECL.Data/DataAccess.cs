using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace IFRS9_ECL.Data
{
    public class DataAccess
    {
        public static readonly DataAccess i = new DataAccess();
            private static string sqlConnection
            {
                get
                {
                    return AppSettings.ConnectionString;
                }
            }
            public int ExecuteQuery(string qry)
            {
            try
            {

                var con = new SqlConnection(sqlConnection);
                var com = new SqlCommand(qry, con);

                con.Open();
                var i = com.ExecuteNonQuery();
                con.Close();
                return 0;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                return -1;
            }

            }


        public int ExecuteBulkCopy(DataTable dt, string tablename)
        {
            
            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                // make sure to enable triggers
                // more on triggers in next post
                SqlBulkCopy bulkCopy = new SqlBulkCopy(
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                // set the destination table name
                bulkCopy.DestinationTableName = tablename;
                bulkCopy.BatchSize = 10000;

                foreach(DataColumn dc in dt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }

                connection.Open();

                // write the data in the "dataTable"
                bulkCopy.WriteToServer(dt);
                connection.Close();
            }
            // reset
            dt.Clear();

            return 1;
        }


        public int getCount(string qry)
            {
            try
            {

                var con = new SqlConnection(sqlConnection);
                var com = new SqlCommand(qry, con);

                con.Open();
                var i = com.ExecuteScalar();
                con.Close();
                try
                {
                    return int.Parse(i.ToString());
                }
                catch { return 0; }
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                return -1;
            }

            }
            public DataTable GetData(string qry)
            {
                var dt = new DataTable();
            try
            {

                var con = new SqlConnection(sqlConnection);
                var da = new SqlDataAdapter(qry, con);

                con.Open();
                da.Fill(dt);
                con.Close();
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                Log4Net.Log.Info(qry);
                ////Console.ReadKey();
            }
                return dt;
            }
        //public T ParseDataToObject<T>(T t, DataRow dr)
        //{

        //    Type myObjOriginalType = t.GetType();
        //    PropertyInfo[] myProps = myObjOriginalType.GetProperties();

        //    for (int i = 0; i < myProps.Length; i++)
        //    {

                
        //        if (myProps[i].PropertyType.FullName == "System.Int32")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (int)dr[i] : 0, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName == "System.Int64")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (long)dr[i] : 0, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName == "System.Decimal")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (decimal)dr[i] : 0M, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName == "System.Double")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? double.Parse(dr[i].ToString()) : 0, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName == "System.Boolean")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (bool)dr[i] : false, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName == "System.DateTime")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (DateTime?)dr[i] : null, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName == "System.String")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (string)dr[i] : null, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName == "System.Guid")
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (Guid)dr[i] : Guid.NewGuid(), null);
        //        }
        //        else if (myProps[i].PropertyType.FullName.StartsWith("System.Nullable`1[[System.Int32"))
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (int?)dr[i] : 0, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName.StartsWith("System.Nullable`1[[System.Int64"))
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (long?)dr[i] : 0, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName.StartsWith("System.Nullable`1[[System.Decimal"))
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (decimal?)dr[i] : 0, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName.StartsWith("System.Nullable`1[[System.Double"))
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (double?)dr[i] : 0, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName.StartsWith("System.Nullable`1[[System.DateTime"))
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (DateTime?)dr[i] : null, null);
        //        }
        //        else if (myProps[i].PropertyType.FullName.StartsWith("System.Nullable`1[[System.Guid"))
        //        {
        //            myProps[i].SetValue(t, dr[i] != DBNull.Value ? (Guid?)dr[i] : null, null);
        //        }
        //        else
        //        {
        //            var dum = myProps[i].PropertyType.FullName;
        //        }

        //    }

        //    return t;
        //}



        // function that creates an object from the given data row
        public T ParseDataToObject<T>(T t, DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }

    }
}
