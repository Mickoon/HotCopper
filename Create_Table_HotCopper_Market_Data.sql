USE [FinanceCrawler]
GO

/****** Object:  Table [dbo].[HotCopper_Market_data]    Script Date: 19/12/2014 5:14:18 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HotCopper_Market_data](
	[Identity] [bigint] IDENTITY(1,1) NOT NULL,
	[Tag] [nvarchar](max) NOT NULL,
	[Date] [datetime] NOT NULL,
	[High] [numeric](18, 2) NULL,
	[Low] [numeric](18, 2) NULL,
	[Open] [numeric](18, 2) NULL,
	[Last] [numeric](18, 2) NULL,
	[Market_Price] [numeric](18, 2) NULL,
	[Volume_(Millions)] [numeric](18, 2) NULL,
	[Value_(Millions)] [numeric](18, 2) NULL,
	[Market_Cap_(Billions)] [numeric](18, 2) NULL
PRIMARY KEY CLUSTERED 
(
	[Identity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


