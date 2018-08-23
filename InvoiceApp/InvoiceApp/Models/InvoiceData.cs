
using System.Collections.Generic;
namespace InvoiceApp.Models
{
    public class InvoiceData
    {
        public string custName { get; set; }
        public string custAddress { get; set; }
        public string compName { get; set; }
        public string compAddress { get; set; }
        public string totalcost { get; set; }
        public string invoicePaid { get; set; }

        public List<GridItems> gridItems {get; set;}

        public string invoiceIdList { get; set; }
        public string invoiceNameList { get; set; }
    }
}