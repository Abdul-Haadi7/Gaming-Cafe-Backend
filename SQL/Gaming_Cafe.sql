USE Gaming_Cafe


DROP DATABASE Gaming_Cafe
CREATE DATABASE Gaming_Cafe

CREATE TABLE Users (id INT PRIMARY KEY IDENTITY(1,1),
name NVARCHAR(100) NOT NULL,
email NVARCHAR(100) NOT NULL UNIQUE,
phone VARCHAR(50) NOT NULL,
isActive BIT NOT NULL DEFAULT 1);
UPDATE Users SET isActive = 1

DROP TABLE Users
SELECT r.name
            FROM Roles r
            JOIN UserRoles ur ON ur.roleId = r.id
            WHERE ur.userId = 3
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
INSERT INTO Permissions (name) VALUES ('CanEditOwnGameRequirements')
INSERT INTO Permissions (name) VALUES ('CanViewOwnGame')
INSERT INTO Permissions (name) VALUES ('CanRateGames')
INSERT INTO Permissions (name) VALUES ('CanViewOwnCart')
INSERT INTO Permissions (name) VALUES ('CanCheckOut')
INSERT INTO Permissions (name) VALUES ('CanViewOwnRequests')
INSERT INTO Permissions (name) VALUES ('CanViewAllCustomers')
INSERT INTO Permissions (name) VALUES ('CanViewAllDevelopers')
INSERT INTO Permissions (name) VALUES ('CanRejectGame')
INSERT INTO Permissions (name) VALUES ('CanViewHisWarnings')
INSERT INTO Permissions (name) VALUES ('CanRequestToEndWarning')
INSERT INTO Permissions (name) VALUES ('CanViewAllWarnings')
INSERT INTO Permissions (name) VALUES ('CanViewAllEndWarningReq')
INSERT INTO Permissions (name) VALUES ('CanViewHisEndWarningReqs')
INSERT INTO Permissions (name) VALUES ('CanDeleteAnyGame')
INSERT INTO Permissions (name) VALUES ('CanBlockCustomers')
INSERT INTO Permissions (name) VALUES ('CanBlockDevelopers')
INSERT INTO Permissions (name) VALUES ('CanBlockAdmins')

INSERT INTO Permissions (name) VALUES ('CanUnblockCustomers')
INSERT INTO Permissions (name) VALUES ('CanUnblockDevelopers')
INSERT INTO Permissions (name) VALUES ('CanUnblockAdmins')
INSERT INTO Permissions (name) VALUES ('CanViewAllAdmins')
SELECT * FROM Permissions




-- Out of all the permissions, each role can perform specific permissions of that role only
CREATE TABLE RolePermissions (roleId INT,
permissionId INT,
CONSTRAINT fk_roleId FOREIGN KEY (roleId) REFERENCES Roles(id) ON UPDATE CASCADE ON DELETE SET NULL,
CONSTRAINT fk_permissionId FOREIGN KEY (permissionId) REFERENCES Permissions(id)
 ON UPDATE CASCADE ON DELETE SET NULL)
DROP TABLE RolePermissions

--Insert the role permissions of Super Admin role
INSERT INTO RolePermissions VALUES (1,1)
INSERT INTO RolePermissions VALUES (1,2)
INSERT INTO RolePermissions VALUES (1,9)
INSERT INTO RolePermissions VALUES (1,10)
INSERT INTO RolePermissions VALUES (1,11)
INSERT INTO RolePermissions VALUES (1,20)
INSERT INTO RolePermissions VALUES (1,18)
INSERT INTO RolePermissions VALUES (1,19)
INSERT INTO RolePermissions VALUES (1,23)
INSERT INTO RolePermissions VALUES (1,24)
INSERT INTO RolePermissions VALUES (1,26)
INSERT INTO RolePermissions VALUES (1,27)
INSERT INTO RolePermissions VALUES (1,28)
INSERT INTO RolePermissions VALUES (1,29)
INSERT INTO RolePermissions VALUES (1,30)
INSERT INTO RolePermissions VALUES (1,31)
INSERT INTO RolePermissions VALUES (1,32)
INSERT INTO RolePermissions VALUES (1,33)

--Insert the role permissions of Admin role
INSERT INTO RolePermissions VALUES (2,9)
INSERT INTO RolePermissions VALUES (2,10)
INSERT INTO RolePermissions VALUES (2,11)
INSERT INTO RolePermissions VALUES (2,18)
INSERT INTO RolePermissions VALUES (2,19)
INSERT INTO RolePermissions VALUES (2,20)
INSERT INTO RolePermissions VALUES (2,23)
INSERT INTO RolePermissions VALUES (2,24)

--Insert the role permissions of Customer role
INSERT INTO RolePermissions VALUES (3,2)
INSERT INTO RolePermissions VALUES (3,4)
INSERT INTO RolePermissions VALUES (3,5)
INSERT INTO RolePermissions VALUES (3,6)
INSERT INTO RolePermissions VALUES (3,14)
INSERT INTO RolePermissions VALUES (3,15)
INSERT INTO RolePermissions VALUES (3,16)

--Insert the role permissions of Developer role
INSERT INTO RolePermissions VALUES (4,3)
INSERT INTO RolePermissions VALUES (4,7)
INSERT INTO RolePermissions VALUES (4,8)
INSERT INTO RolePermissions VALUES (4,12)
INSERT INTO RolePermissions VALUES (4,13)
INSERT INTO RolePermissions VALUES (4,17)
INSERT INTO RolePermissions VALUES (4,21)
INSERT INTO RolePermissions VALUES (4,22)
INSERT INTO RolePermissions VALUES (4,25)

SELECT * FROM Permissions

SELECT permissionId FROM RolePermissions WHERE roleId = 2
SELECT permissionId FROM RolePermissions WHERE roleId = 1

SELECT * FROM RolePermissions
TRUNCATE TABLE RolePermissions

-- All users have specific permissions from their role`s permissions
CREATE TABLE UserPermissions (userId INT,
permissionId INT,
CONSTRAINT fk_roleId_userPermissions FOREIGN KEY (userId) REFERENCES Users(id) ON UPDATE CASCADE ON DELETE SET NULL,
CONSTRAINT fk_permissionId_userPermissions FOREIGN KEY (permissionId) REFERENCES Permissions(id) ON UPDATE CASCADE ON DELETE SET NULL)
DROP TABLE UserPermissions
SELECT * FROM UserPermissions

SELECT * FROM UserPermissions WHERE userId = 1012


SELECT
    u.id,
    u.name,
    u.email,
    u.phone,
    u.isActive,
    STRING_AGG(p.name, ',') AS permissions
FROM Users u
INNER JOIN UserRoles ur
    ON u.id = ur.userId
INNER JOIN Roles r
    ON ur.roleId = r.id
LEFT JOIN UserPermissions up
    ON u.id = up.userId
LEFT JOIN Permissions p
    ON up.permissionId = p.id
WHERE r.name = 'Admin'
GROUP BY
    u.id,
    u.name,
    u.email,
    u.phone,
    u.isActive;

INSERT INTO UserPermissions VALUES (1,23)
INSERT INTO UserPermissions VALUES (1,24)
INSERT INTO UserPermissions VALUES (1,20)
INSERT INTO UserPermissions VALUES (1,18)
INSERT INTO UserPermissions VALUES (1,19)
INSERT INTO UserPermissions VALUES (1,26)
INSERT INTO UserPermissions VALUES (1,27)
INSERT INTO UserPermissions VALUES (1,28)
INSERT INTO UserPermissions VALUES (1,29)

INSERT INTO UserPermissions VALUES (1,30)
INSERT INTO UserPermissions VALUES (1,31)
INSERT INTO UserPermissions VALUES (1,32)
INSERT INTO UserPermissions VALUES (1,33)

SELECT * FROM UserPermissions WHERE userId = 1

SELECT * FROM UserPermissions 

SELECT * FROM Users
SELECT * FROM Auth
SELECT * FROM UserRoles
SELECT * FROM UserPermissions
select * from roles

SELECT p.name FROM Permissions p JOIN RolePermissions rp
        ON rp.permissionId = p.id WHERE rp.roleId IN
        (SELECT r.id FROM Roles r WHERE r.name = 'Customer')



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
downloadLink NVARCHAR(MAX) NOT NULL,
imageLink NVARCHAR(MAX) NOT NULL,
discountPercentage DECIMAL(4,1) NOT NULL DEFAULT 0,
developerId INT,
isActive BIT NOT NULL DEFAULT 0,
approvedBy INT DEFAULT NULL,
rejectedBy INT DEFAULT NULL,
rejectionReason NVARCHAR(MAX) DEFAULT NULL,
hasWarning BIT DEFAULT 0,
isPublic BIT DEFAULT 1,
isRejected BIT NOT NULL DEFAULT 0,
isApproved BIT NOT NULL DEFAULT 0,
requestResultViewed BIT NOT NULL DEFAULT 0,
CONSTRAINT fk_developerId FOREIGN KEY (developerId) REFERENCES Users(id) 
ON UPDATE CASCADE ON DELETE SET NULL,

CONSTRAINT fk_approvedBy FOREIGN KEY (approvedBy) REFERENCES Users(id),

CONSTRAINT fk_rejectedBy FOREIGN KEY (rejectedBy) REFERENCES Users(id)
);


ALTER TABLE Games
ALTER COLUMN downloadLink NVARCHAR(MAX);

SELECT * FROM Games
DROP TABLE Games
SELECT * FROM Games WHERE developerId = 7
SELECT * FROM Games WHERE developerId = 3

TRUNCATE TABLE Games

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
        CHECK (ratingGiven > 0 AND ratingGiven <= 10)
);
DROP TABLE Game_Ratings

INSERT INTO Game_Ratings values (1,4,10)
INSERT INTO Game_Ratings values (2,4,0.1)

CREATE TABLE Game_Requirements
(
    gameId INT PRIMARY KEY,
    os NVARCHAR(100) NOT NULL,
    processor NVARCHAR(200) NOT NULL,
    ram NVARCHAR(100) NOT NULL,
    graphicsCard NVARCHAR(100) NOT NULL,
    storage NVARCHAR(100) NOT NULL,
    isActive BIT DEFAULT 1,

    CONSTRAINT FK_GameRequirements_Game
        FOREIGN KEY (gameId)
        REFERENCES Games(id) ON UPDATE CASCADE ON DELETE CASCADE
);

DROP TABLE Game_Requirements
SELECT * FROM Games

UPDATE Games SET hasWarning = 0 WHERE id = 1

UPDATE Games SET isActive = 1 

SELECT * FROM Game_Requirements
SELECT * FROM Game_Ratings

SELECT COUNT(*) FROM Game_Ratings WHERE gameId =2

SELECT SUM(ratingGiven) FROM Game_Ratings WHERE gameId = 2

INSERT INTO Game_Ratings VALUES(1,2,9.6)
INSERT INTO Game_Ratings VALUES(2,2,8.2)

CREATE TABLE Sale_Records (gameId INT NOT NULL,
buyerId INT NOT NULL,
price DECIMAL(10,2),
PRIMARY KEY (gameId,buyerId),
CONSTRAINT FK_gameId
FOREIGN KEY (gameId)
REFERENCES Games(id),
CONSTRAINT FK_buyerId
FOREIGN KEY (buyerId)
REFERENCES Users(id))

DROP TABLE Sale_Records
SELECT * FROM Sale_Records
SELECT * FROM Sale_Records WHERE buyerId = 3

INSERT INTO Sale_Records VALUES(4,1,129.36)
INSERT INTO Sale_Records VALUES(2,2,839)

SELECT COUNT(*) FROM Sale_Records WHERE gameId = 2

SELECT SUM(price) FROM Sale_Records WHERE gameId = 2

DROP TABLE Sale_Records


CREATE TABLE Cart (buyerId INT NOT NULL,
gameId INT NOT NULL,
PRIMARY KEY (buyerId,gameId),
CONSTRAINT FK_buyerId_cart
FOREIGN KEY (buyerId)
REFERENCES Users(id),

CONSTRAINT FK_gameId_cart
FOREIGN KEY (gameId)
REFERENCES Games(id))

SELECT * FROM Cart
DROP TABLE Cart
DELETE FROM Cart WHERE buyerId = 1 AND gameId = 1


CREATE TABLE Warnings (id INT PRIMARY KEY IDENTITY (1,1),
gameId INT NOT NULL,
reason NVARCHAR (MAX) NOT NULL,
issuedBy INT NOT NULL,
issuedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
endedBy INT DEFAULT NULL,
endedAt DATETIME2 DEFAULT NULL,
requestedToEnd BIT DEFAULT 0,
isActive BIT DEFAULT 1,

CONSTRAINT FK_Warning_Game FOREIGN KEY (gameId) 
REFERENCES Games (Id),
CONSTRAINT FK_Warning_issuer FOREIGN KEY (issuedBy) 
REFERENCES Users (Id),
CONSTRAINT FK_Warning_ender FOREIGN KEY (endedBy) 
REFERENCES Users (Id))


CREATE TABLE End_Warning_Request (id INT PRIMARY KEY IDENTITY (1,1) NOT NULL,
warningId INT NOT NULL,
requestNote NVARCHAR(MAX),
isAccepted BIT DEFAULT 0,
isRejected BIT DEFAULT 0,
acceptedBy INT DEFAULT NULL,
rejectedBy INT DEFAULT NULL,
isActive BIT DEFAULT 1,
doNotShowAgain BIT DEFAULT 0 NOT NULL,

CONSTRAINT fk_WarningId FOREIGN KEY (warningId) REFERENCES Warnings(id),
CONSTRAINT fk_reqAcceptedBy FOREIGN KEY (acceptedBy) REFERENCES Users(id),
CONSTRAINT fk_reqRejectedBy FOREIGN KEY (rejectedBy) REFERENCES Users(id))


SELECT * FROM Warnings
SELECT * FROM Games
SELECT * FROM Games WHERE developerId = 2
SELECT * FROM End_Warning_Request
SELECT COUNT(*) FROM Games WHERE id = 7

DROP TABLE End_Warning_Request
DROP TABLE Warnings


SELECT COUNT(*) FROM Warnings WHERE id = 1
        AND gameId IN (SELECT id FROM Games WHERE developerId = 1005)

INSERT INTO End_Warning_Request (warningId,requestNote) VALUES (1,'dummy note')

INSERT INTO End_Warning_Request 
        (warningId, requestNote) VALUES (1,'')

UPDATE Games SET hasWarning = 0

UPDATE End_Warning_Request SET isAccepted = 1, acceptedBy = 1, is Active = 0 
            WHERE warningId = 1

SELECT * FROM Games WHERE developerId = 1005
UPDATE Warnings SET endedBy = 1, endedAt = GETDATE()
SELECT * FROM Warnings
UPDATE Games SET hasWarning = 0

SELECT * FROM End_Warning_Request
UPDATE End_Warning_Request SET doNotShowAgain = 0


UPDATE End_Warning_Request SET isAccepted = 1, acceptedBy = 1, isActive = 0 
            WHERE warningId = 1

SELECT reason FROM Warnings WHERE gameId = 7

SELECT COUNT(*) FROM Sale_Records WHERE buyerId = 3 AND gameId = 1

UPDATE Games SET imageLink = 'https://devimages-cdn.apple.com/wwdc-services/articles/images/3D5F5DD3-14F7-4384-94C0-798D15EE7CD7/2048.jpeg'


SELECT id,name,price,intro,description,genre,downloadLink,imageLink,
        discountPercentage FROM Games WHERE isActive = 1 AND isApproved = 1 AND isPublic = 1


SELECT id,name,price,intro,description,genre,downloadLink,imageLink,
        discountPercentage,hasWarning,isActive,isPublic FROM Games WHERE developerId = 1005 
        AND isActive = 0 

UPDATE Games SET requestResultViewed = 1 WHERE id = 1


SELECT 
            e.id,e.warningId,e.requestNote,e.isAccepted,e.isRejected,w.gameId,w.reason,
            g.name AS gameName,
            u.name AS developerName
        FROM End_Warning_Request e
        INNER JOIN Warnings w 
            ON e.warningId = w.id
        INNER JOIN Games g 
            ON w.gameId = g.id
        INNER JOIN Users u
            ON g.developerId = u.id
        WHERE e.doNotShowAgain = 0
        AND g.developerId = 2