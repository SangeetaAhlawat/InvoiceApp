using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace InvoiceApp.Models
{
    public class InvoiceService
    {
        private SqlConnection con;

        #region Get DB Connection
        private SqlConnection connection()
        {
            string constr = ConfigurationManager.ConnectionStrings["invoiceDB_connection"].ToString();
            con = new SqlConnection(constr);
            return con;
        } 
        #endregion

        #region Get Invoice Details from DB to display on UI
        public DataTable GetInvoiceList()
        {
            DataTable dt = new DataTable();
            using (connection())
            {
                SqlCommand com = new SqlCommand("GetInvoiceDetails", con);
                com.CommandType = CommandType.StoredProcedure;                
                try
                {
                    con.Open();
                    SqlDataAdapter adp = new SqlDataAdapter(com);
                    adp.Fill(dt);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                }
            }
            return dt;
        } 
        #endregion
        
        #region Save/Insert Invoice Details in DB
        public bool SaveInvoice(InvoiceData model)
        {
            bool resultItems = false;
            using (connection())
            {
                SqlCommand com = new SqlCommand("InvoiceAndItemDetails", con);
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@custName", model.custName);
                com.Parameters.AddWithValue("@custAddress", model.custAddress);
                //Set the Paid status in DB based on paid checkbox value
                if (model.invoicePaid == "1")
                    com.Parameters.AddWithValue("@Status", "Paid");
                else
                    com.Parameters.AddWithValue("@Status", "UnPaid");
                com.Parameters.AddWithValue("@compName", model.compName);
                com.Parameters.AddWithValue("@compAddress", model.compAddress);
                com.Parameters.AddWithValue("@totalcost", Convert.ToDouble(model.totalcost));
                com.Parameters.AddWithValue("@invoicePaid", Convert.ToBoolean(model.invoicePaid));
                com.Parameters.Add("@id", SqlDbType.Int).Direction = ParameterDirection.Output; 
                
                try
                {
                    con.Open();
                    com.ExecuteNonQuery();
                    int invoiceID = Convert.ToInt32(com.Parameters["@id"].Value.ToString());
                    if (invoiceID != null)
                    {
                        resultItems = SaveInvoiceItems(model.gridItems, invoiceID);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                }
            }
            return resultItems;
        } 
        #endregion

        #region Save/Insert Invoice Items in DB
        public bool SaveInvoiceItems(List<GridItems> model, int invoiceID)
        {
            bool isSaved = false;
                       
            foreach (var item in model)
            {
                using (connection())
                {
                    SqlCommand com = new SqlCommand("InsertItemDetails", con);
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("@InvoiceId", invoiceID);
                    com.Parameters.AddWithValue("@des", item.des);
                    com.Parameters.AddWithValue("@qty", Convert.ToDouble(item.qty));
                    com.Parameters.AddWithValue("@rate", Convert.ToDouble(item.rate));
                    com.Parameters.AddWithValue("@cost", Convert.ToDouble(item.cost));

                    try
                    {
                        con.Open();
                        com.ExecuteNonQuery();
                        isSaved = true;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                    }
                }
            }
            return isSaved;            
        }
        #endregion

        #region Update Invoice Status 
        public bool UpdateInvoice(string IDs)
        {
            bool isUpdated = false;
            using (connection())
            {
                SqlCommand com = new SqlCommand("UpdateInvoice", con);
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@InvoiceID", IDs);

                try
                {
                    con.Open();
                    com.ExecuteNonQuery();
                    isUpdated = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                }
            }
            return isUpdated;
        }
	    #endregion
    }
}