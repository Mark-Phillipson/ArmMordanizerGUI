

/****** Object:  Table [dbo].[MapperConfiguration]    Script Date: 4/9/2022 3:34:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MapperConfiguration](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SourceTable] [nvarchar](max) NOT NULL,
	[DestinationTable] [nvarchar](max) NOT NULL,
	[SQL] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [date] NOT NULL,
 CONSTRAINT [PK_MapperConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


