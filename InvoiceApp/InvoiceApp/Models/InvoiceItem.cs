//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InvoiceApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class InvoiceItem
    {
        public int ItemID { get; set; }
        public string ItemDescription { get; set; }
        public Nullable<double> ItemQuantity { get; set; }
        public Nullable<double> ItemRate { get; set; }
        public Nullable<double> ItemCost { get; set; }
        public Nullable<int> InvoiceID { get; set; }
    
        public virtual InvoiceDetail InvoiceDetail { get; set; }
    }
}
