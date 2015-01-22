USE [FinanceCrawler]
GO

/****** Object:  Table [dbo].[HotCopper_Posts]    Script Date: 19/12/2014 5:14:23 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HotCopper_Posts](
	[Identity] [bigint] IDENTITY(1,1) NOT NULL,
	[Stock] [nvarchar](max) NOT NULL,
	[Subject] [nvarchar](max) NOT NULL,
	[PageNum] [bigint] NOT NULL,
	[Content] [nvarchar](max) NULL,
	[NegWords] [bigint] NULL,
	[PosWords] [bigint] NULL,
	[Likes] [bigint] NULL,
	[Price_at_Posting] [numeric](18, 2) NULL,
	[Sentiment] [nvarchar](max) NULL,
	[Disclosure] [nvarchar](max) NULL,
	[Author] [nvarchar](max) NULL,
	[DateTime] [datetime] NULL,
	[Post_ID] [bigint] NULL,
	[IP] [nvarchar](max) NULL,
	[Length_of_Post] [bigint] NULL
PRIMARY KEY CLUSTERED 
(
	[Identity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


