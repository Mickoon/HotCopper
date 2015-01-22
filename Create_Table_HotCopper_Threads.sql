USE [FinanceCrawler]
GO

/****** Object:  Table [dbo].[HotCopper_Threads]    Script Date: 19/12/2014 5:14:27 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HotCopper_Threads](
	[Identity] [bigint] IDENTITY(1,1) NOT NULL,
	[Stock] [nvarchar](max) NOT NULL,
	[Tags] [nvarchar](max) NOT NULL,
	[Subject] [nvarchar](max) NOT NULL,
	[First_Poster] [nvarchar](max) NULL,
	[Num_of_Views] [bigint] NULL,
	[Begin_Date] [datetime] NULL,
	[Last_Poster] [nvarchar](max) NULL,
	[Last_Post] [nvarchar](max) NULL,
	[Num_of_Posts] [bigint] NULL
PRIMARY KEY CLUSTERED 
(
	[Identity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


