/********** 1. Table***********/
USE InvoiceDB
GO


/****** Object:  Table [dbo].[InvoiceDetails]    Script Date: 22/8/2018 5:55:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InvoiceDetails](
	[InvoiceID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](max) NULL,
	[Status] [nvarchar](10) NULL,
	[CustomerAddress] [nvarchar](max) NULL,
	[CompanyName] [nvarchar](50) NULL,
	[CompanyAddress] [nvarchar](max) NULL,
	[TotalCost] [float] NULL,
	[Paid] [bit] NULL,
 CONSTRAINT [PK_InvoiceDetails] PRIMARY KEY CLUSTERED 
(
	[InvoiceID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


/********** 2. Table***********/
USE InvoiceDB
GO

/****** Object:  Table [dbo].[InvoiceItem]    Script Date: 22/8/2018 5:58:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InvoiceItem](
	[ItemID] [int] IDENTITY(1,1) NOT NULL,
	[ItemDescription] [nvarchar](max) NULL,
	[ItemQuantity] [float] NULL,
	[ItemRate] [float] NULL,
	[ItemCost] [float] NULL,
	[InvoiceID] [int] NOT NULL,
 CONSTRAINT [PK_InvoiceItem] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceItem_InvoiceDetails] FOREIGN KEY([InvoiceID])
REFERENCES [dbo].[InvoiceDetails] ([InvoiceID])
GO

ALTER TABLE [dbo].[InvoiceItem] CHECK CONSTRAINT [FK_InvoiceItem_InvoiceDetails]
GO


/********** 1. Stored Procedure***********/
USE InvoiceDB
GO

/****** Object:  StoredProcedure [dbo].[InsertItemDetails]    Script Date: 22/8/2018 6:00:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create procedure [dbo].[InsertItemDetails]  
(  
   @InvoiceId int, 
   @des nvarchar(500),
   @qty float,
   @rate float,
   @cost float
)  
as  
begin      
   Insert into dbo.InvoiceItem(ItemDescription,ItemQuantity, ItemRate, ItemCost, InvoiceID) values(@des, @qty, @rate, @cost, @InvoiceId)  
End 
GO


/********** 2. Stored Procedure***********/
USE InvoiceDB
GO

/****** Object:  StoredProcedure [dbo].[InvoiceAndItemDetails]    Script Date: 22/8/2018 6:00:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[InvoiceAndItemDetails]  
(  
   @custName nvarchar (50),  
   @custAddress nvarchar (500), 
   @Status nvarchar(10),
   @compName varchar (50),
   @compAddress nvarchar (500),
   @totalcost float,
   @invoicePaid bit,  
   @id int output
)  
as  
begin  
  SET NOCOUNT ON;

    Insert into dbo.InvoiceDetails(CustomerName, CustomerAddress, [Status], CompanyName, CompanyAddress, TotalCost, Paid) values(@custName, @custAddress, @Status, @compName, @compAddress,@totalcost,@invoicePaid)  
   
   SET @id=SCOPE_IDENTITY()
   RETURN  @id   
End 
GO


/********** 3. Stored Procedure***********/
USE InvoiceDB
GO

/****** Object:  StoredProcedure [dbo].[GetInvoiceDetails]    Script Date: 22/8/2018 6:02:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create procedure [dbo].[GetInvoiceDetails]  
AS 
begin  
   
   Select InvoiceId, CustomerName, [Status]  From dbo.InvoiceDetails
  
End 
GO


/********** 4. Stored Procedure***********/
USE InvoiceDB
GO
/****** Object:  StoredProcedure [dbo].[UpdateInvoice]    Script Date: 22/8/2018 6:02:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[UpdateInvoice]  
(  
   @InvoiceID nvarchar(MAX)
)  
as  
begin
    
    Update InvoiceDetails SET  [Status]='Paid' WHERE InvoiceID IN  (SELECT Data FROM dbo.Split (@InvoiceID,','))

end 
GO


/********** 1. Function***********/
USE InvoiceDB
GO

/****** Object:  UserDefinedFunction [dbo].[Split]    Script Date: 22/8/2018 6:03:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[Split]
(
	@RowData nvarchar(2000),
	@SplitOn nvarchar(5)
)
RETURNS @RtnValue table
(
	Id int identity(1,1),
	Data nvarchar(100)
)
AS
BEGIN
	Declare @Cnt int
	Set @Cnt = 1

	While (Charindex(@SplitOn,@RowData)>0)
	Begin
		Insert Into @RtnValue (data)
		Select
			Data = ltrim(rtrim(Substring(@RowData,1,Charindex(@SplitOn,@RowData)-1)))

		Set @RowData = Substring(@RowData,Charindex(@SplitOn,@RowData)+1,len(@RowData))
		Set @Cnt = @Cnt + 1
	End
	
	Insert Into @RtnValue (data)
	Select Data = ltrim(rtrim(@RowData))

	Return
END
GO




