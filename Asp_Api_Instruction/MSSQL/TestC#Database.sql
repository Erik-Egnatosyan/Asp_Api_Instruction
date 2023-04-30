create database TestC#Database
use TestC#Database
create table MyUsersTable
(
Id int NOT NULL identity primary key,
FirstName nvarchar(20) NOT NULL,
LastName nvarchar(20) NOT NULL,
Age tinyint NOT NULL,
Email nvarchar(50) NOT NULL,
PhoneNumber nvarchar(30)
)
