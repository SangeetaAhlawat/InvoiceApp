Use InvoiceDB
GO

/****** Object:  StoredProcedure [dbo].[UpdateInvoice]    Script Date: 28/8/2018 8:22:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[UpdateInvoice]  
(  
   @InvoiceID nvarchar(MAX)
)  
as  
begin
    /* Update InvoiceDetails SET  [Status]='Paid' WHERE InvoiceID IN  (SELECT Data FROM dbo.Split (@InvoiceID,',')) */

	Update InvoiceDetails SET  [Status]='Paid' WHERE CustomerName IN  (SELECT Data FROM dbo.Split (@InvoiceID,','))
end 



