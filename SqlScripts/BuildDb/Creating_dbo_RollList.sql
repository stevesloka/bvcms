

CREATE FUNCTION [dbo].[RollList]
(
	@mid INT
	,@meetingdt DATETIME
	,@oid INT
	,@current BIT
	,@FromMobile BIT
)
RETURNS @table TABLE 
(
	Section INT, -- Member = 1, Visitor = 2, Other = 3
	PeopleId INT, 
	Name NVARCHAR(100),
	Last NVARCHAR(100),
	FamilyId INT,
	First NVARCHAR(50),
	Email NVARCHAR(100),
	Attended BIT, 
	CommitmentId INT, 
	CurrMemberType NVARCHAR(100), 
	MemberType NVARCHAR(100), 
	AttendType NVARCHAR(100),
	OtherAttends INT,
	CurrMember BIT,
	Conflict BIT
)
AS
BEGIN
	DECLARE @AppRollListCommittedOnly BIT = 0

	IF @FromMobile = 1 AND 
		EXISTS(	SELECT NULL 
				FROM dbo.OrganizationExtra
				WHERE OrganizationId = @oid
				AND Field = 'AppRollListCommittedOnly'
				AND BitValue = 1)
		SET @AppRollListCommittedOnly = 1

	--People who attended the meeting or were committed to come
	DECLARE @attends TABLE 
	(
		PeopleId INT, 
		CommitmentId INT, 
		Attended BIT, 
		membertype INT, 
		attendtype INT,
		OtherAttends INT
	)
	--Members at the time of meeting
	DECLARE @members TABLE 
	(
		PeopleId INT, 
		joindt DATETIME, 
		membertype INT
	)
	--Recent Visitors
	DECLARE @visitors TABLE 
	(
		PeopleId INT
	)

	INSERT @attends
	SELECT a.PeopleId 
		   ,a.Commitment
		   ,a.AttendanceFlag
		   ,a.MemberTypeId
		   ,a.AttendanceTypeId
		   ,a.OtherAttends
	FROM dbo.Attend a
	WHERE MeetingId = @mid
	AND (a.EffAttendFlag IS NULL OR a.EffAttendFlag = 1 OR a.Commitment IS NOT NULL)

	DECLARE @dt DATETIME = @meetingdt
	IF(@current = 1)
		SET @dt = GETDATE()

	INSERT @members
	SELECT 
		PeopleId, 
		joined, 
		MemberTypeId 
	FROM dbo.OrgMembersAsOfDate(@oid, @dt)

	INSERT @visitors
	SELECT PeopleId 
	FROM dbo.OrgVisitorsAsOfDate(@oid, @meetingdt, 0)

	-- start building results, members first
	INSERT @table
	SELECT 1 -- Members
		   ,m.PeopleId
		   ,p.Name2
		   ,p.LastName
		   ,p.FamilyId
		   ,p.PreferredName
		   ,p.EmailAddress
		   ,a.Attended
		   ,a.CommitmentId
		   ,cmt.Description CurrMemberType
		   ,mt.Description MemberType
		   ,at.Description Attendtype
		   ,a.OtherAttends
		   ,CASE WHEN om.PeopleId IS NULL THEN 0 ELSE 1 END
		   ,CASE WHEN GETDATE() > @meetingdt OR mc.PeopleId IS NULL THEN 0 ELSE 1 END
	FROM @members m
	LEFT JOIN @attends a ON a.PeopleId = m.PeopleId
	JOIN dbo.People p ON p.PeopleId = m.PeopleId
	LEFT JOIN lookup.MemberType cmt ON cmt.Id = m.membertype
	LEFT JOIN lookup.MemberType mt ON mt.Id = a.membertype
	LEFT JOIN lookup.AttendType at ON at.Id = a.attendtype
	LEFT JOIN dbo.OrganizationMembers om ON om.PeopleId = p.PeopleId AND om.OrganizationId = @oid
	LEFT JOIN dbo.MeetingConflicts mc ON mc.PeopleId = a.PeopleId AND mc.MeetingDate = @meetingdt AND @oid IN (mc.OrgId1, mc.OrgId2)
	WHERE (@current = 1 OR @meetingdt > m.joindt OR (a.PeopleId IS NOT NULL AND attendtype NOT IN (40,50,60,110)))
	AND (@AppRollListCommittedOnly = 0 OR CommitmentId IN (1,4))

	-- recent visitors who have not become members as of the meeting date
	INSERT @table
	SELECT 2 -- Visitors
		   ,v.PeopleId
		   ,p.Name2
		   ,p.LastName
		   ,p.FamilyId
		   ,p.PreferredName
		   ,p.EmailAddress
		   ,a.Attended
		   ,a.CommitmentId
		   ,NULL
		   ,mt.Description MemberType
		   ,at.Description Attendtype
	       ,a.OtherAttends
		   ,CASE WHEN om.PeopleId IS NULL THEN 0 ELSE 1 END
		   ,0
	FROM @visitors v
	LEFT JOIN @attends a ON a.PeopleId = v.PeopleId
	JOIN dbo.People p ON p.PeopleId = v.PeopleId
	LEFT JOIN lookup.MemberType mt ON mt.Id = a.membertype
	LEFT JOIN lookup.AttendType at ON at.Id = a.attendtype
	LEFT JOIN dbo.OrganizationMembers om ON om.PeopleId = p.PeopleId AND om.OrganizationId = @oid
	WHERE NOT EXISTS(SELECT NULL FROM @members m WHERE m.PeopleId = v.PeopleId)
    
	-- now catch the odd ducks who slipped through the cracks, (should not be any if all is well)
	INSERT @table
	SELECT 3 -- Other
		   ,a.PeopleId
		   ,p.Name2
		   ,p.LastName
		   ,p.FamilyId
		   ,p.PreferredName
		   ,p.EmailAddress
		   ,a.Attended
		   ,a.CommitmentId
		   ,NULL
		   ,mt.Description MemberType
		   ,at.Description Attendtype
	       ,a.OtherAttends
		   ,CASE WHEN om.PeopleId IS NULL THEN 0 ELSE 1 END
		   ,0
	FROM @attends a
	LEFT JOIN @table t ON t.PeopleId = a.PeopleId 
	JOIN dbo.People p ON p.PeopleId = a.PeopleId
	LEFT JOIN lookup.MemberType mt ON mt.Id = a.membertype
	LEFT JOIN lookup.AttendType at ON at.Id = a.attendtype
	LEFT JOIN dbo.OrganizationMembers om ON om.PeopleId = p.PeopleId AND om.OrganizationId = @oid
	WHERE t.PeopleId IS NULL

	RETURN

END







GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
