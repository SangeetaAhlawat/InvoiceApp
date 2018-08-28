using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using System.Web;
using Xero.Api.Core;
using Xero.Api.Core.Model;
using Xero.Api.Example.Applications.Private;
using Xero.Api.Infrastructure.OAuth;
using Xero.Api.Serialization;


namespace InvoiceApp.Models
{
    public class InvoiceService
    {
        private SqlConnection con;
        private string consumerKey = System.Configuration.ConfigurationManager.AppSettings["consumerKey"];
        private string consumerSecret = System.Configuration.ConfigurationManager.AppSettings["consumerSecret"];
        private string privatePublicKey = System.Configuration.ConfigurationManager.AppSettings["privatePublicKey"];
        private string privatePublicFile = System.Configuration.ConfigurationManager.AppSettings["privatePublicFile"];

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
                    throw ex; //ToDo: Can log this error in log file instead of throw.
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

            using (TransactionScope scope = new TransactionScope())
            {
                using (connection())
                {
                    SqlCommand com = new SqlCommand("InvoiceAndItemDetails", con);
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("@custName", model.custName);
                    com.Parameters.AddWithValue("@custAddress", model.custAddress);
                    //Set the Paid status in DB based on paid checkbox value
                    if (model.invoicePaid == "True")
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
                        throw ex; //ToDo: Can log this error in log file instead of throw.
                    }
                    finally
                    {
                        con.Close();
                        con.Dispose();
                    }
                }
                scope.Complete();
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
                        throw ex; //ToDo: Can log this error in log file instead of throw.
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
                    throw ex; //ToDo: Can log this error in log file instead of throw.
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

        #region Xero Methods
        public bool CreateInvoice(InvoiceData model)
        {
            bool isSaved = false;
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                X509Certificate2 cert = new X509Certificate2(privatePublicFile, privatePublicKey);
                var private_app_api = new XeroCoreApi("https://api.xero.com/api.xro/2.0/invoices", new PrivateAuthenticator(cert),
                                         new Consumer(consumerKey, consumerSecret), null,
                                         new DefaultMapper(), new DefaultMapper());

                var inv = private_app_api.Invoices;

                Contact newContact = new Contact();
                newContact.Name = model.custName; 

                Invoice newInvoice = new Invoice();
                newInvoice.Contact = new Contact();
                newInvoice.Contact = newContact;
                newInvoice.Date = System.DateTime.Now;
                newInvoice.Reference = "TestDemoNZ";
                newInvoice.DueDate = System.DateTime.Now.AddMonths(1);
                newInvoice.Type = Xero.Api.Core.Model.Types.InvoiceType.AccountsReceivable;
                newInvoice.LineAmountTypes = Xero.Api.Core.Model.Types.LineAmountType.Exclusive;
                List<LineItem> lines = new List<LineItem>();
                LineItem li = new LineItem();
                foreach (var item in model.gridItems)
                {
                    li.Quantity = Convert.ToDecimal(item.qty);
                    li.Description = item.des;
                    li.AccountCode = "200";  //Need to analyse from Xero invoice for which this column is being used. Time being hard coded the value.
                    li.UnitAmount = Convert.ToDecimal(item.rate);
                    lines.Add(li);
                }
                newInvoice.LineItems = lines;

                if (model.invoicePaid == "True")
                {
                    var result = inv.Create(newInvoice);
                    IEnumerable<Invoice> invF = inv.Find().Where(invt => invt.Contact.Name == model.custName).ToList();
                    if (invF.Count() > 0)
                    {
                        foreach (Invoice item in invF)
                        {
                            item.AmountPaid = item.AmountDue;
                            item.Status = Xero.Api.Core.Model.Status.InvoiceStatus.Authorised;
                            inv.Update(item);


                            var pItem = private_app_api.Payments;
                            Payment pm = new Payment();

                            Account ac = new Account();
                            ac.Code = "091";
                            Invoice iv = new Invoice();
                            iv.Id = item.Id;
                            pm.Account = ac;
                            pm.Invoice = iv;
                            pm.Amount = item.AmountDue;
                            pm.Date = System.DateTime.Now;
                            pm.Status = Xero.Api.Core.Model.Status.PaymentStatus.Authorised;
                            pItem.Create(pm);

                            isSaved = true;
                        }
                    }
                }
                else
                {
                    // call the API to create the Invoice
                    var result = inv.Create(newInvoice);
                    isSaved = true;
                }                    

                // To Create Contact if required
                //var cItem = private_app_api.Contacts;
                //IEnumerable<Contact> invC = cItem.Find().Where(invtc => invtc.Name == "Sangi NZ").ToList();
                //if (invC.Count() == 0)
                //{
                //    var contacts = CreateContacts("Sangi", "NZ").ToList();
                //    cItem.Create(contacts);
                //}  
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isSaved;
        }
        public bool GetAndUpdateInvoice(string names)
        {
            bool isUpdated = false;
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                X509Certificate2 cert = new X509Certificate2(privatePublicFile, privatePublicKey);
                var private_app_api = new XeroCoreApi("https://api.xero.com/api.xro/2.0/invoices", new PrivateAuthenticator(cert),
                                         new Consumer(consumerKey, consumerSecret), null,
                                         new DefaultMapper(), new DefaultMapper());

                var pvtInv = private_app_api.Invoices;

                if (names.Contains(','))
                {
                    string[] strNames = names.Split(',');
                    foreach (string str in strNames)
                    {
                        //IEnumerable<Invoice> inv = pvtInv.Find().Where(invt => invt.Contact.Name == str).ToList();
                        IEnumerable<Invoice> inv = pvtInv.Find().Where(invt => invt.Number == str).ToList();

                        if (inv.Count() > 0)
                        {
                            foreach (Invoice item in inv)
                            {
                                item.AmountPaid = item.AmountDue;
                                item.Status = Xero.Api.Core.Model.Status.InvoiceStatus.Authorised;
                                pvtInv.Update(item);

                                var pItem = private_app_api.Payments;
                                Payment pm = new Payment();

                                Account ac = new Account();
                                ac.Code = "091";
                                Invoice iv = new Invoice();
                                iv.Id = item.Id;
                                pm.Account = ac;
                                pm.Invoice = iv;
                                pm.Amount = item.AmountDue;
                                pm.Date = System.DateTime.Now;
                                pm.Status = Xero.Api.Core.Model.Status.PaymentStatus.Authorised;
                                pItem.Create(pm);

                                isUpdated = true;
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(names))
                {
                    //IEnumerable<Invoice> inv = pvtInv.Find().Where(invt => invt.Contact.Name == names).ToList();
                    IEnumerable<Invoice> inv = pvtInv.Find().Where(invt => invt.Number == names).ToList();
                    if (inv.Count() > 0)
                    {
                        foreach (Invoice item in inv)
                        {
                            item.AmountPaid = item.AmountDue;
                            item.Status = Xero.Api.Core.Model.Status.InvoiceStatus.Authorised;
                            pvtInv.Update(item);


                            var pItem = private_app_api.Payments;
                            Payment pm = new Payment();

                            Account ac = new Account();
                            ac.Code = "091";
                            Invoice iv = new Invoice();
                            iv.Id = item.Id;
                            pm.Account = ac;
                            pm.Invoice = iv;
                            pm.Amount = item.AmountDue;
                            pm.Date = System.DateTime.Now;
                            pm.Status = Xero.Api.Core.Model.Status.PaymentStatus.Authorised;
                            pItem.Create(pm);


                            isUpdated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isUpdated = false;
                throw ex;
            }
            return isUpdated;
        }
        private IEnumerable<Contact> CreateContacts(string fName, string lName)
        {
            var contacts = new List<Contact>();

            contacts.Add(new Contact
            {
                FirstName = fName,
                LastName = lName,
                Name = string.Format("{0} {1}", fName, lName)          
            });

            return contacts;
        }
        public DataTable GetInvoiceListFromXero()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            X509Certificate2 cert = new X509Certificate2(privatePublicFile, privatePublicKey);
            var private_app_api = new XeroCoreApi("https://api.xero.com/api.xro/2.0/invoices", new PrivateAuthenticator(cert),
                                     new Consumer(consumerKey, consumerSecret), null,
                                     new DefaultMapper(), new DefaultMapper());

            var pvtInv = private_app_api.Invoices;

            DataTable dt = new DataTable();
            IEnumerable<Invoice> inv = pvtInv.Find().Where(invt => invt.Reference == "TestDemoNZ").ToList();
            dt = ToDataTable(inv.ToList());
            return dt;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (prop.Name == "Number")
                {
                    tb.Columns.Add("InvoiceId");
                }
                if (prop.Name == "Contact")
                {
                    tb.Columns.Add("CustomerName");
                }
                if (prop.Name == "Status")
                {
                    tb.Columns.Add("Status");
                }
            }

            foreach (var item in items)
            {
                var values = new object[3];
                for (var i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        values[i] = props[1].GetValue(item);
                    }
                    else if (i == 1)
                    {
                        values[i] = ((Xero.Api.Core.Model.Contact)(props[2].GetValue(item, null))).Name;
                    }
                    else if (i == 2)
                    {
                        values[i] = props[4].GetValue(item);
                    }
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        #endregion
    }
}