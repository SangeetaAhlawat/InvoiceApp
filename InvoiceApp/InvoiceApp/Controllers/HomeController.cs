using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InvoiceApp.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Newtonsoft.Json;


namespace InvoiceApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult submitInvoice([System.Web.Http.FromBody]InvoiceData model)
        {
            string JSONResult = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.gridItems != null)
                {
                    InvoiceService invoiceSvc = new InvoiceService();
                    bool isXeroSaved = invoiceSvc.CreateInvoice(model);
                    if (isXeroSaved)
                    {
                        bool isSaved = invoiceSvc.SaveInvoice(model);
                        if (isSaved)
                        {
                            string message = "Invoice has been created successfully!";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            return Json("Kindly add item details", JsonRequestBehavior.AllowGet); 
        }

        [HttpGet]
        public JsonResult GetInvoiceList()
        {
            string JSONResult = string.Empty;
            InvoiceService invoiceSvc = new InvoiceService();
            //DataTable dt = invoiceSvc.GetInvoiceList();   // List from DB
            //if (dt.Rows.Count > 0)   // From DB 
            DataTable dtXero = invoiceSvc.GetInvoiceListFromXero();
            foreach (DataRow dr in dtXero.Rows)
            {
                if (dr["Status"].ToString() == "Draft")
                {
                    dr["Status"] = "UnPaid";
                }
            }
            if (dtXero.Rows.Count > 0)  // From Xero
            {
                JSONResult = JsonConvert.SerializeObject(dtXero);
            }
            return Json(JSONResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult updateInvoice([System.Web.Http.FromBody]InvoiceData model)
        {
            if (ModelState.IsValid)
            {                 
                InvoiceService invoiceSvc = new InvoiceService();
                bool isUpdated = invoiceSvc.GetAndUpdateInvoice(model.invoiceIdList);    //(model.invoiceNameList);
                if (isUpdated)
                {
                    bool isSaved = invoiceSvc.UpdateInvoice(model.invoiceNameList);    //(model.invoiceIdList);
                    if (isSaved)
                    {
                        string message = "Invoice has been created successfully!";
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("", JsonRequestBehavior.AllowGet); 
        }

    }
}
