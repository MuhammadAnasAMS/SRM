
USE [PIA_SRM_DB]
GO
/****** Object:  Table [dbo].[Complaints]    Script Date: 22/07/2025 12:08:20 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Complaints](
	[complaint_id] [int] IDENTITY(1,1) NOT NULL,
	[title] [varchar](200) NULL,
	[description] [text] NULL,
	[status] [varchar](20) NOT NULL,
	[priority] [varchar](20) NOT NULL,
	[requester_id] [int] NOT NULL,
	[assigned_to_id] [int] NULL,
	[department_id] [int] NOT NULL,
	[created_at] [datetime] NULL,
	[updated_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[complaint_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Department_Heads]    Script Date: 22/07/2025 12:08:20 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Department_Heads](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[department_id] [int] NOT NULL,
	[user_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Departments]    Script Date: 22/07/2025 12:08:20 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departments](
	[department_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](100) NOT NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[department_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Logs]    Script Date: 22/07/2025 12:08:20 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logs](
	[log_id] [int] IDENTITY(1,1) NOT NULL,
	[action_by] [int] NOT NULL,
	[role_performed_as] [varchar](50) NULL,
	[action_description] [text] NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[log_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OTP_Verifications]    Script Date: 22/07/2025 12:08:20 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OTP_Verifications](
	[otp_id] [int] IDENTITY(1,1) NOT NULL,
	[employee_id] [varchar](50) NULL,
	[otp_code] [varchar](10) NULL,
	[expires_at] [datetime] NULL,
	[verified] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[otp_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 22/07/2025 12:08:20 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[employee_id] [varchar](50) NOT NULL,
	[name] [varchar](100) NULL,
	[email] [varchar](100) NULL,
	[mobile] [varchar](50) NULL,
	[role] [varchar](20) NOT NULL,
	[department_id] [int] NULL,
	[password_hash] [varchar](255) NULL,
	[is_first_login] [bit] NULL,
	[created_at] [datetime] NULL,
	[designation] [varchar](100) NULL,
	[status] [varchar](10) NULL,
	[employee_type] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Complaints] ADD  DEFAULT ('Open') FOR [status]
GO
ALTER TABLE [dbo].[Complaints] ADD  DEFAULT ('Medium') FOR [priority]
GO
ALTER TABLE [dbo].[Complaints] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Complaints] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[Departments] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Logs] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[OTP_Verifications] ADD  DEFAULT ((0)) FOR [verified]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [is_first_login]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Complaints]  WITH CHECK ADD FOREIGN KEY([assigned_to_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Complaints]  WITH CHECK ADD FOREIGN KEY([department_id])
REFERENCES [dbo].[Departments] ([department_id])
GO
ALTER TABLE [dbo].[Complaints]  WITH CHECK ADD FOREIGN KEY([requester_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Department_Heads]  WITH CHECK ADD FOREIGN KEY([department_id])
REFERENCES [dbo].[Departments] ([department_id])
GO
ALTER TABLE [dbo].[Department_Heads]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Logs]  WITH CHECK ADD FOREIGN KEY([action_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD FOREIGN KEY([department_id])
REFERENCES [dbo].[Departments] ([department_id])
GO
ALTER TABLE [dbo].[Complaints]  WITH CHECK ADD CHECK  (([priority]='Low' OR [priority]='Medium' OR [priority]='High' OR [priority]='Urgent'))
GO
ALTER TABLE [dbo].[Complaints]  WITH CHECK ADD CHECK  (([status]='Closed' OR [status]='Resolved' OR [status]='In Progress' OR [status]='Open'))
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD CHECK  (([role]='Requester' OR [role]='Employee' OR [role]='DepartmentAdmin' OR [role]='SuperUser'))
GO
