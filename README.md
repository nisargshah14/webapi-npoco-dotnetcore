# webapi-npoco-dotnetcore

Project Architecture is designed to using the ASP.NET Web API 5.2.7, NPoco ORMs, and can able to work with multiple databases. Also, added a custom token-based Authentication using the action filter attribute and handling the exceptions.


**1. Architecture.Web** - Architecture.WebApi holds the routing and configuration of the projects and API controllers and controllers.

**2. Architecture.Core** - Architecture.Core holds business logic of the system and mapped to the Entity to ViewModel and sends the result to the Controllers to source and vice versa. Currently, the Core project holds Attributes, IDataPRovider, and DataProvider. You can add Attributes under the Core projects which is for the business logic and create Data Provider and Interface Data Provider for every controller.

**3. Architecture.Data** - Architecture.Data holds the database logic and created the multiple common functions into the BaseDataProvider.cs which can be inherited from the PetaPoco methods and easy to use in multiple scenarios. Also, added Entity and Attributes which depend on the database logics. The data project is the mediator of the database and business logic.

**4. Architecture.Common** - Architecture.Generic holds all view models, common functions, helpers, enumerations, extensions, configs, constants, and resources.

### Database Steps!

```sh
  USE [master]  
GO  
  
/****** Object:  Database [DbCustomers]    Script Date: 2/25/2018 7:45:46 AM ******/  
CREATE DATABASE [DbCustomers]  
 CONTAINMENT = NONE  
 ON  PRIMARY   
( NAME = N'DbCustomers', FILENAME = N'c:\Program Files (x86)\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\DbCustomers.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )  
 LOG ON   
( NAME = N'DbCustomers_log', FILENAME = N'c:\Program Files (x86)\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\DbCustomers_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)  
GO  
  
ALTER DATABASE [DbCustomers] SET COMPATIBILITY_LEVEL = 110  
GO  
  
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))  
begin  
EXEC [DbCustomers].[dbo].[sp_fulltext_database] @action = 'enable'  
end  
GO  
  
ALTER DATABASE [DbCustomers] SET ANSI_NULL_DEFAULT OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET ANSI_NULLS OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET ANSI_PADDING OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET ANSI_WARNINGS OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET ARITHABORT OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET AUTO_CLOSE OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET AUTO_CREATE_STATISTICS ON   
GO  
  
ALTER DATABASE [DbCustomers] SET AUTO_SHRINK OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET AUTO_UPDATE_STATISTICS ON   
GO  
  
ALTER DATABASE [DbCustomers] SET CURSOR_CLOSE_ON_COMMIT OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET CURSOR_DEFAULT  GLOBAL   
GO  
  
ALTER DATABASE [DbCustomers] SET CONCAT_NULL_YIELDS_NULL OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET NUMERIC_ROUNDABORT OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET QUOTED_IDENTIFIER OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET RECURSIVE_TRIGGERS OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET  DISABLE_BROKER   
GO  
  
ALTER DATABASE [DbCustomers] SET AUTO_UPDATE_STATISTICS_ASYNC OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET DATE_CORRELATION_OPTIMIZATION OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET TRUSTWORTHY OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET ALLOW_SNAPSHOT_ISOLATION OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET PARAMETERIZATION SIMPLE   
GO  
  
ALTER DATABASE [DbCustomers] SET READ_COMMITTED_SNAPSHOT OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET HONOR_BROKER_PRIORITY OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET RECOVERY SIMPLE   
GO  
  
ALTER DATABASE [DbCustomers] SET  MULTI_USER   
GO  
  
ALTER DATABASE [DbCustomers] SET PAGE_VERIFY CHECKSUM    
GO  
  
ALTER DATABASE [DbCustomers] SET DB_CHAINING OFF   
GO  
  
ALTER DATABASE [DbCustomers] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )   
GO  
  
ALTER DATABASE [DbCustomers] SET TARGET_RECOVERY_TIME = 0 SECONDS   
GO  
  
ALTER DATABASE [DbCustomers] SET  READ_WRITE   
GO  

USE [DbCustomers]  
GO  
  
/****** Object:  Table [dbo].[Customers]    Script Date: 2/25/2018 7:46:32 AM ******/  
SET ANSI_NULLS ON  
GO  
  
SET QUOTED_IDENTIFIER ON  
GO  
  
SET ANSI_PADDING ON  
GO  
  
CREATE TABLE [dbo].[Customers](  
    [CustomerID] [int] IDENTITY(1,1) NOT NULL,  
    [CustomerName] [varchar](50) NULL,  
    [CustomerEmail] [varchar](50) NULL,  
    [CustomerCountry] [varchar](50) NULL,  
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED   
(  
    [CustomerID] ASC  
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]  
) ON [PRIMARY]  
  
GO  
  
SET ANSI_PADDING OFF  
GO              

```