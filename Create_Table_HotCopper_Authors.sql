USE [FinanceCrawler]
GO

/****** Object:  Table [dbo].[HotCopper_Authors]    Script Date: 19/12/2014 5:14:15 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HotCopper_Authors](
	[Identity] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Num_of_Posts] [bigint] NULL,
	[Likes_Received] [bigint] NULL,
	[Following] [bigint] NULL,
	[Followers] [bigint] NULL,
	[Following_List] [nvarchar](max) NOT NULL,
	[Followers_List] [nvarchar](max) NULL,
	[Num_of_Posts_in_calendar_month] [bigint] NULL,
	[Following_Stocks] [nvarchar](max) NULL
PRIMARY KEY CLUSTERED 
(
	[Identity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


