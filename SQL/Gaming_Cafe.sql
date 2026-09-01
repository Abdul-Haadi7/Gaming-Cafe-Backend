USE Gaming_Cafe

CREATE TABLE Users (id INT PRIMARY KEY IDENTITY(1,1),
name NVARCHAR(100) NOT NULL,
email NVARCHAR(100) NOT NULL UNIQUE,
phone VARCHAR(50) NOT NULL,
dateOfBirth DATE NOT NULL,
isActive BIT NOT NULL DEFAULT 1);
DROP TABLE Users

DROP TABLE Users
SELECT * FROM Users

CREATE TABLE Auth 
(
    userId INT,
    passwordSalt VARBINARY(16) NOT NULL,
    passwordHash VARBINARY(64) NOT NULL,
    isActive BIT NOT NULL DEFAULT 1

    CONSTRAINT fk_userID FOREIGN KEY (userId) REFERENCES Users(id)
    ON UPDATE CASCADE ON DELETE SET NULL
);

DROP TABLE Auth
SELECT * FROM Auth

-- All the roles that exists in our system
CREATE TABLE Roles (id int PRIMARY KEY IDENTITY (1,1),
name VARCHAR(100))
DROP TABLE Roles

-- Insert all roles in DB
INSERT INTO Roles (name) VALUES ('Super Admin'),('Admin'),('Customer'),('Developer');
SELECT * FROM Roles

-- Each user has one role only
CREATE TABLE UserRoles (userId INT UNIQUE,
roleId INT,
CONSTRAINT fk_userId_userRoles FOREIGN KEY (userId) REFERENCES Users(id) ON UPDATE CASCADE ON DELETE SET NULL,
CONSTRAINT fk_roleId_userRoles FOREIGN KEY (roleId) REFERENCES Roles(id) ON UPDATE CASCADE ON DELETE SET NULL)
DROP TABLE UserRoles
SELECT * FROM UserRoles
SELECT * FROM Users
SELECT * FROM Auth

-- All the permissions that exists in the whole system 
CREATE TABLE Permissions (id int PRIMARY KEY IDENTITY (1,1),
name VARCHAR(100))
DROP TABLE Permissions

-- Insert all permissions of the system
INSERT INTO Permissions (name) VALUES('CanAddAdmin')
INSERT INTO Permissions (name) VALUES ('CanViewAllGames'),('CanUploadGames')
INSERT INTO Permissions (name) VALUES ('CanViewGamesDetails'),('CanAddGameToCart'),('CanRemoveGameFromCart'),
('EditOwnGames'),('DeleteOwnGame')
INSERT INTO Permissions (name) VALUES ('CanApproveGame'),('CanSendWarning'),('CanEndWarning');

SELECT * FROM Permissions

-- Out of all the permissions, each role can perform specific permissions of that role only
CREATE TABLE RolePermissions (roleId INT,
permissionId INT,
CONSTRAINT fk_roleId FOREIGN KEY (roleId) REFERENCES Roles(id) ON UPDATE CASCADE ON DELETE SET NULL,
CONSTRAINT fk_permissionId FOREIGN KEY (permissionId) REFERENCES Permissions(id) ON UPDATE CASCADE ON DELETE SET NULL)
DROP TABLE RolePermissions

--Insert the role permissions of Super Admin role
INSERT INTO RolePermissions VALUES (1,1)
INSERT INTO RolePermissions VALUES (1,9)
INSERT INTO RolePermissions VALUES (1,10)
INSERT INTO RolePermissions VALUES (1,11)

--Insert the role permissions of Admin role
INSERT INTO RolePermissions VALUES (2,9)
INSERT INTO RolePermissions VALUES (2,10)
INSERT INTO RolePermissions VALUES (2,11)

--Insert the role permissions of Customer role
INSERT INTO RolePermissions VALUES (3,2)
INSERT INTO RolePermissions VALUES (3,4)
INSERT INTO RolePermissions VALUES (3,5)
INSERT INTO RolePermissions VALUES (3,6)

--Insert the role permissions of Developer role
INSERT INTO RolePermissions VALUES (4,3)
INSERT INTO RolePermissions VALUES (4,7)
INSERT INTO RolePermissions VALUES (4,8)

SELECT * FROM RolePermissions

SELECT * FROM RolePermissions
TRUNCATE TABLE RolePermissions

-- All users have specific permissions from their role`s permissions
CREATE TABLE UserPermissions (userId INT,
permissionId INT,
CONSTRAINT fk_roleId_userPermissions FOREIGN KEY (userId) REFERENCES Users(id) ON UPDATE CASCADE ON DELETE SET NULL,
CONSTRAINT fk_permissionId_userPermissions FOREIGN KEY (permissionId) REFERENCES Permissions(id) ON UPDATE CASCADE ON DELETE SET NULL)
DROP TABLE UserPermissions
SELECT * FROM UserPermissions


SELECT * FROM Users
SELECT * FROM Auth
SELECT * FROM UserRoles
SELECT * FROM UserPermissions
select * from roles


TRUNCATE TABLE Auth
TRUNCATE TABLE UserRoles
TRUNCATE TABLE UserPermissions
TRUNCATE TABLE Users

