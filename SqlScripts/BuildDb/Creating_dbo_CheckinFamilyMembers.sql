CREATE FUNCTION [dbo].[CheckinFamilyMembers] ( @familyid INT, @campus INT, @thisday INT )
RETURNS 
@t TABLE 
(
	Id INT, 
	[Position] INT,
	MemberVisitor CHAR,
	[Name] VARCHAR(150),
	[First] VARCHAR(50),
	PreferredName VARCHAR(50),
	[Last] VARCHAR(100)
	,BYear INT
	,BMon INT
	,BDay INT
	,Class VARCHAR(100)
	,Leader VARCHAR(100)
	,OrgId INT
	,Location VARCHAR(200)
	,Age INT
	,Gender VARCHAR(10)
	,NumLabels INT
	,[hour] DATETIME
	,CheckedIn BIT
	,goesby VARCHAR(50)
	,email VARCHAR(150)
	,addr VARCHAR(100)
	,zip VARCHAR(15)
	,home VARCHAR(20)
	,cell VARCHAR(20)
	,marital INT
	,genderid INT
	,CampusId INT
	,allergies VARCHAR(1000)
	,emfriend VARCHAR(100)
	,emphone VARCHAR(100)
	,activeother BIT
	,parent VARCHAR(100)
	,grade INT
	,HasPicture BIT
	,Custody BIT
	,Transport BIT
	,RequiresSecurityLabel BIT
	,church VARCHAR(130)
)
AS
BEGIN
	----------------
	--Family Members
	----------------
	DECLARE @pids TABLE (pid INT)
	INSERT INTO @pids
	SELECT PeopleId FROM dbo.People p
	WHERE p.DeceasedDate IS NULL
	AND (p.FamilyId = @familyid)
		OR (EXISTS( SELECT NULL 
					FROM dbo.PeopleExtra e 
					JOIN dbo.People hh ON e.PeopleId = p.PeopleId AND e.Field = 'Parent' 
					WHERE hh.FamilyId = @familyid
					AND e.IntValue = hh.PeopleId)
			AND EXISTS(SELECT NULL FROM dbo.Setting WHERE Id = 'ShowChildInExtraValueParentFamily' AND Setting = 'true'))

	DECLARE @disallowinactive BIT = CASE WHEN EXISTS(SELECT NULL
					FROM dbo.Setting 
					WHERE Id = 'DisallowInactiveCheckin' 
					AND Setting = 'true') THEN 1 ELSE 0 END

	---------
	--Members
	---------
	INSERT @t SELECT
		om.PeopleId
		,p.PositionInFamilyId 
		,'M'
		,p.Name
		,p.FirstName
		,p.PreferredName
		,p.LastName
		,p.BirthYear
		,p.BirthMonth
		,p.BirthDay
		,o.OrganizationName
		,o.LeaderName
		,om.OrganizationId
		,o.Location
		,ISNULL(p.Age, 0)
		,g.Code
		,CASE WHEN om.MemberTypeId IN (220, 230) 
				THEN ISNULL(o.NumCheckInLabels, 1) 
				ELSE ISNULL(o.NumWorkerCheckInLabels, 0) 
		 END
		,mh.[hour]
		,CASE WHEN EXISTS(SELECT NULL 
							FROM dbo.Attend aa
							JOIN dbo.Meetings mm ON aa.MeetingId = mm.MeetingId
							WHERE aa.PeopleId = om.PeopleId
							AND mm.OrganizationId = om.OrganizationId
							AND mm.MeetingDate = mh.[hour]
							AND aa.AttendanceFlag = 1) 
			   THEN 1 ELSE 0 
		 END
		,p.NickName
		,p.EmailAddress
		,f.AddressLineOne
		,f.ZipCode
		,f.HomePhone
		,p.CellPhone
		,p.MaritalStatusId 
		,p.GenderId
		,p.CampusId
		,rr.MedicalDescription
		,rr.emcontact
		,rr.emphone
		,ISNULL(rr.ActiveInAnotherChurch, 0)
		,ISNULL(rr.mname, rr.fname)
		,p.Grade
		,CASE WHEN p.PictureId IS NULL THEN 0 ELSE 1 END
		,ISNULL(p.CustodyIssue, 0)
		,ISNULL(p.OkTransport, 0)
		,CASE WHEN om.MemberTypeId IN (220, 230) 
				AND ISNULL(p.Age,0) < 18 
				AND ISNULL(o.NoSecurityLabel, 0) = 0 
			THEN 1 ELSE 0 
		 END
		,p.OtherPreviousChurch
		 
	FROM dbo.OrganizationMembers om
	JOIN dbo.Organizations o ON om.OrganizationId = o.OrganizationId
	JOIN dbo.People p ON om.PeopleId = p.PeopleId AND om.OrganizationId = o.OrganizationId
	JOIN @pids ON pid = p.PeopleId
	JOIN dbo.Families f ON f.FamilyId = p.FamilyId
	LEFT JOIN dbo.RecReg rr ON p.PeopleId = rr.PeopleId
	JOIN lookup.Gender g ON p.GenderId = g.Id
	JOIN dbo.GetTodaysMeetingHours3(@thisday) mh ON mh.OrganizationId = o.OrganizationId
	WHERE ISNULL(o.CanSelfCheckin, 0) = 1
	AND isnull(om.Pending, 0) = 0 -- not pending
	AND om.MemberTypeId <> 311 -- not a prospect
	AND (om.MemberTypeId <> 230 -- not inactive
		OR @disallowinactive = 0 -- or allow it because it is not disallowed
	)
	AND (o.CampusId = @campus OR @campus IS NULL OR @campus = 0)
	AND o.OrganizationStatusId = 30
			
	-----------------
	--Recent Visitors
	-----------------
	INSERT @t SELECT 
		 p.PeopleId 
		,p.PositionInFamilyId
		,'V'
		,p.Name
		,p.FirstName 
		,p.PreferredName
		,p.LastName
		,p.BirthYear
		,p.BirthMonth
		,p.BirthDay
		,o.OrganizationName
		,o.LeaderName
		,o.OrganizationId
		,o.Location
		,ISNULL(p.Age, 0)
		,g.Code
		, ISNULL(o.NumCheckInLabels, 1)
		,mh.[hour]
		,CASE WHEN EXISTS(SELECT NULL 
							FROM dbo.Attend aa
							JOIN dbo.Meetings mm ON mm.MeetingId = aa.MeetingId
							WHERE aa.PeopleId = p.PeopleId
							AND mm.OrganizationId = tt.OrganizationId
							AND aa.MeetingDate = mh.[hour]
							AND aa.AttendanceFlag = 1) 
			   THEN 1 ELSE 0 
		 END
		,p.NickName
		,p.EmailAddress
		,f.AddressLineOne
		,f.ZipCode
		,f.HomePhone
		,p.CellPhone
		,p.MaritalStatusId
		,p.GenderId
		,p.CampusId
		,rr.MedicalDescription
		,rr.emcontact
		,rr.emphone
		,ISNULL(rr.ActiveInAnotherChurch, 0)
		,ISNULL(rr.mname, rr.fname)
		,p.Grade
		,CASE WHEN p.PictureId IS NULL THEN 0 ELSE 1 END
		,ISNULL(p.CustodyIssue, 0)
		,ISNULL(p.OkTransport, 0)
		,CASE WHEN ISNULL(p.Age,0) < 18 AND ISNULL(o.NoSecurityLabel, 0) = 0 
			THEN 1 ELSE 0 
		 END
		,p.OtherPreviousChurch
	 
	FROM
	(
		SELECT
			a.PeopleId, m.OrganizationId
		FROM dbo.Attend a
		JOIN dbo.People p ON a.PeopleId = p.PeopleId
		JOIN @pids ON pid = p.PeopleId
		JOIN dbo.Meetings m ON m.MeetingId = a.MeetingId
		JOIN dbo.Organizations o ON m.OrganizationId = o.OrganizationId
		WHERE ISNULL(o.CanSelfCheckin, 0) = 1
		AND (o.AllowNonCampusCheckIn = 1 OR o.CampusId = @campus OR @campus IS NULL OR @campus = 0)
		AND o.OrganizationStatusId = 30
		AND a.AttendanceFlag = 1 
		AND (a.MeetingDate >= o.FirstMeetingDate OR o.FirstMeetingDate IS NULL)
		AND a.MeetingDate >= o.VisitorDate
		AND a.AttendanceTypeId IN (40,50,60)
		AND NOT EXISTS(SELECT NULL FROM dbo.OrganizationMembers om 
						WHERE om.PeopleId = a.PeopleId 
						AND om.OrganizationId = m.OrganizationId 
						AND om.MemberTypeId <> 311) --NOT a member, ok if Prospect
		GROUP BY a.PeopleId, m.OrganizationId
	) tt
	JOIN dbo.GetTodaysMeetingHours3(@thisday) mh ON tt.OrganizationId = mh.OrganizationId
	JOIN dbo.People p ON tt.PeopleId = p.PeopleId
	JOIN dbo.Families f ON p.FamilyId = f.FamilyId
	JOIN dbo.Organizations o ON tt.OrganizationId = o.OrganizationId
	JOIN lookup.Gender g ON p.GenderId = g.Id
	LEFT JOIN dbo.RecReg rr ON rr.PeopleId = p.PeopleId
	
	
	-------------------------------
	-- AllowNonCampusCheckin Orgs
	-------------------------------

	DECLARE @visitorgs TABLE
	(
		VisitorOrgName VARCHAR(80),
		VisitorOrgId INT,
		VisitorOrgHour DATETIME
	)
	INSERT INTO @visitorgs 
	SELECT o.OrganizationName, o.OrganizationId, mh.[hour]
	FROM dbo.Organizations o
	JOIN dbo.GetTodaysMeetingHours3(@thisday) mh ON o.OrganizationId = mh.OrganizationId
	WHERE o.OrganizationStatusId = 30
	AND (o.CampusId = @campus OR @campus IS NULL OR @campus = 1)
	AND o.CanSelfCheckin = 1
	AND o.AllowNonCampusCheckIn = 1
	

	INSERT @t SELECT
		 p.PeopleId 
		,p.PositionInFamilyId
		,NULL --neither Member nor Visitor
		,p.Name
		,p.FirstName 
		,p.PreferredName
		,p.LastName
		,p.BirthYear
		,p.BirthMonth
		,p.BirthDay
		,vo.VisitorOrgName -- class
		,'' -- leader
		,vo.VisitorOrgId -- orgid
		,'' -- location
		,ISNULL(p.Age, 0)
		,g.Code -- Gender
		,0 -- number of checkin labels
		,NULL -- hour
		,0 -- CheckedIn
		,p.NickName
		,p.EmailAddress
		,f.AddressLineOne
		,f.ZipCode
		,f.HomePhone
		,p.CellPhone
		,p.MaritalStatusId
		,p.GenderId
		,p.CampusId
		,rr.MedicalDescription
		,rr.emcontact
		,rr.emphone
		,ISNULL(rr.ActiveInAnotherChurch, 0)
		,ISNULL(rr.mname, rr.fname)
		,p.Grade
		,CASE WHEN p.PictureId IS NULL THEN 0 ELSE 1 END
		,ISNULL(p.CustodyIssue, 0)
		,ISNULL(p.OkTransport, 0)
		,0 -- no security label
		,p.OtherPreviousChurch
	FROM dbo.People p
	JOIN @pids ON pid = p.PeopleId
	JOIN dbo.Families f ON p.FamilyId = f.FamilyId
	JOIN lookup.Gender g ON g.Id = p.GenderId
	LEFT JOIN dbo.RecReg rr ON p.PeopleId = rr.PeopleId
	JOIN @visitorgs vo ON (p.CampusId != @campus OR p.CampusId IS NULL) AND p.Age > 12
	WHERE NOT EXISTS(SELECT NULL FROM @t WHERE Id = p.PeopleId)		
	
	-----------------------------------------------
	-- Rest of Family that cannot checkin anywhere
	------------------------------------------------
	
	INSERT INTO @t SELECT
		 p.PeopleId 
		,p.PositionInFamilyId
		,NULL --neither Member nor Visitor
		,p.Name
		,p.FirstName 
		,p.PreferredName
		,p.LastName
		,p.BirthYear
		,p.BirthMonth
		,p.BirthDay
		,'No self check-in meetings available' --class
		,'' -- leader
		,0 -- orgid
		,'' -- location
		,ISNULL(p.Age, 0)
		,g.Code -- Gender
		,0 -- number of checkin labels
		,NULL -- hour
		,0 -- CheckedIn
		,p.NickName
		,p.EmailAddress
		,f.AddressLineOne
		,f.ZipCode
		,f.HomePhone
		,p.CellPhone
		,p.MaritalStatusId
		,p.GenderId
		,p.CampusId
		,rr.MedicalDescription
		,rr.emcontact
		,rr.emphone
		,ISNULL(rr.ActiveInAnotherChurch, 0)
		,ISNULL(rr.mname, rr.fname)
		,p.Grade
		,CASE WHEN p.PictureId IS NULL THEN 0 ELSE 1 END
		,ISNULL(p.CustodyIssue, 0)
		,ISNULL(p.OkTransport, 0)
		,0 -- no security label
		,p.OtherPreviousChurch
	FROM dbo.People p
	JOIN @pids ON pid = p.PeopleId
	JOIN dbo.Families f ON p.FamilyId = f.FamilyId
	JOIN lookup.Gender g ON g.Id = p.GenderId
	LEFT JOIN dbo.RecReg rr ON p.PeopleId = rr.PeopleId
	WHERE NOT EXISTS(SELECT NULL FROM @t WHERE Id = p.PeopleId)		
	
	RETURN 
END
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
