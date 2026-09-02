USE Gaming_Cafe
DROP DATABASE Gaming_Cafe
CREATE DATABASE Gaming_Cafe

CREATE TABLE Users (id INT PRIMARY KEY IDENTITY(1,1),
name NVARCHAR(100) NOT NULL,
email NVARCHAR(100) NOT NULL UNIQUE,
phone VARCHAR(50) NOT NULL,
isActive BIT NOT NULL DEFAULT 1);



DROP TABLE Users

DROP TABLE Users
SELECT * FROM Users

CREATE TABLE Auth 
(
    userId INT PRIMARY KEY,
    passwordSalt VARBINARY(16) NOT NULL,
    passwordHash VARBINARY(64) NOT NULL,
    isActive BIT NOT NULL DEFAULT 1

    CONSTRAINT fk_userID FOREIGN KEY (userId) REFERENCES Users(id)
    ON UPDATE CASCADE
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


CREATE TABLE Games (id INT PRIMARY KEY IDENTITY(1,1),
name NVARCHAR(100) NOT NULL,
price DECIMAL(10,2) NOT NULL,
intro NVARCHAR(100) NOT NULL,
description NVARCHAR(MAX) NOT NULL,
genre NVARCHAR(100) NOT NULL,
downloadLink NVARCHAR(100) NOT NULL,
imageLink NVARCHAR(100) NOT NULL,
discountPercentage DECIMAL(4,1) NOT NULL DEFAULT 0,
developerId INT,
isActive BIT NOT NULL DEFAULT 1,
approvedBy INT DEFAULT NULL,
rejectedBy INT DEFAULT NULL,
rejectionReason NVARCHAR(MAX) DEFAULT NULL,
CONSTRAINT fk_developerId FOREIGN KEY (developerId) REFERENCES Users(id) 
ON UPDATE CASCADE ON DELETE SET NULL,

CONSTRAINT fk_approvedBy FOREIGN KEY (approvedBy) REFERENCES Users(id),

CONSTRAINT fk_rejectedBy FOREIGN KEY (rejectedBy) REFERENCES Users(id)
);

SELECT * FROM Games
DROP TABLE Games

-- Users can rate evey game, all ratings given to evry game are sotred here and the average rating
--calculated and displayed in the game.
CREATE TABLE Game_Ratings
(
    userId INT NOT NULL,
    gameId INT NOT NULL,
    ratingGiven DECIMAL(3,1) NOT NULL,

    PRIMARY KEY (userId, gameId),
    CONSTRAINT FK_GameRatings_User
        FOREIGN KEY (userId)
        REFERENCES Users(id),
    CONSTRAINT FK_GameRatings_Game
        FOREIGN KEY (gameId)
        REFERENCES Games(id),

    CONSTRAINT checkRating
        CHECK (ratingGiven >= 0 AND ratingGiven <= 10)
);
DROP TABLE Game_Ratings

CREATE TABLE Game_Requirements
(
    gameId INT PRIMARY KEY,
    os NVARCHAR(100) NOT NULL,
    processor NVARCHAR(200) NOT NULL,
    ram NVARCHAR(100) NOT NULL,
    graphicsCard NVARCHAR(100) NOT NULL,
    storage NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_GameRequirements_Game
        FOREIGN KEY (gameId)
        REFERENCES Games(id) ON UPDATE CASCADE ON DELETE CASCADE
);

DROP TABLE Game_Requirements
SELECT * FROM Game_Requirements