using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CmsData;
using CmsData.Codes;
using CmsWeb.Models;
using CmsWeb.Models.iPhone;
using UtilityExtensions;

namespace CmsWeb.Areas.Public.Controllers
{
    [ValidateInput(false)]
    public class APIiPhoneController : CmsController
    {
        public ActionResult FetchImage(int id)
        {
            if (!Authenticate("Access"))
                return Content("not authorized");
            Response.NoCache();
            var person = DbUtil.Db.People.Single(pp => pp.PeopleId == id);
            if (person.PictureId != null)
                return new ImageResult(person.Picture.MediumId ?? 0);
            return new ImageResult(0);
        }

        public ActionResult Search(string name, string comm, string addr)
        {
            if (!Authenticate(checkOrgLeadersOnly: true))
                return Content("not authorized");
            Response.NoCache();

            var m = new SearchModel(name, comm, addr);
            return new SearchResult0(m.PeopleList(), m.Count);
        }

        public ActionResult SearchResults(string name, string comm, string addr)
        {
            if (!Authenticate(checkOrgLeadersOnly: true))
                return Content("not authorized");
            if (!CMSRoleProvider.provider.IsUserInRole(AccountModel.UserName2, "Access"))
                return Content("not authorized");
            Response.NoCache();

            DbUtil.LogActivity($"iphone search '{name}'");
            var m = new SearchModel(name, comm, addr);
            return new SearchResult(m.PeopleList(), m.Count);
        }

        public ActionResult DetailResults(int id)
        {
            if (!Authenticate())
                return Content("not authorized");
            Response.NoCache();
            DbUtil.LogActivity($"iphone view ({id})");
            return new DetailResult(id);
        }

        public ActionResult Organizations()
        {
            if (!Authenticate(checkOrgLeadersOnly: true))
                return Content("not authorized");
            Response.NoCache();
            DbUtil.LogActivity("iPhone Organizations");
            if (!CMSRoleProvider.provider.IsUserInRole(AccountModel.UserName2, "Attendance"))
                return new OrgResult(null);
            return new OrgResult(Util.UserPeopleId);
        }

        [HttpPost]
        public ActionResult RollList(int id, DateTime datetime)
        {
            if (!Authenticate())
                return Content("not authorized");
            var u = DbUtil.Db.Users.Single(uu => uu.Username == AccountModel.UserName2);
            DbUtil.LogActivity($"iphone RollList {id} {datetime:g}");

            var mid = DbUtil.Db.CreateMeeting(id, datetime);
            var meeting = DbUtil.Db.LoadMeetingById(mid);
            return new RollListResult(meeting);
        }

        [HttpPost]
        public ActionResult RecordAttend(int id, int peopleId, bool present)
        {
            if (!Authenticate())
                return Content("not authorized");
            DbUtil.LogActivity($"iphone attend(mt:{id} person:{peopleId} {present})");
            Attend.RecordAttendance(peopleId, id, present);
            DbUtil.Db.UpdateMeetingCounters(id);
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult RecordVisit(int id, int peopleId)
        {
            if (!Authenticate())
                return Content("not authorized");
            Attend.RecordAttendance(peopleId, id, true);
            DbUtil.Db.UpdateMeetingCounters(id);
            var meeting = DbUtil.Db.Meetings.Single(mm => mm.MeetingId == id);
            return new RollListResult(meeting);
        }

        [HttpPost]
        public ActionResult AddPerson(int id, PersonInfo m)
        {
            if (!Authenticate())
                return Content("not authorized");

            var f = m.addtofamilyid > 0
                ? DbUtil.Db.Families.First(fam => fam.People.Any(pp => pp.PeopleId == m.addtofamilyid))
                : new Family();

            if (m.goesby == "(Null)")
                m.goesby = null;

            var position = DbUtil.Db.ComputePositionInFamily(m.dob.Age0(), m.marital == 20, f.FamilyId) ?? 10;

            var p = Person.Add(f, position,
                null, Trim(m.first), Trim(m.goesby), Trim(m.last), m.dob, false, m.gender,
                OriginCode.Visit, null);

            DbUtil.LogActivity($"iPhone AddPerson {p.PeopleId}");
            UpdatePerson(p, m);
            var meeting = DbUtil.Db.Meetings.Single(mm => mm.MeetingId == id);
            Attend.RecordAttendance(p.PeopleId, id, true);
            DbUtil.Db.UpdateMeetingCounters(id);
            return new RollListResult(meeting, p.PeopleId);
        }

        private void UpdatePerson(Person p, PersonInfo m)
        {
            var psb = new List<ChangeDetail>();
            var fsb = new List<ChangeDetail>();
            var z = DbUtil.Db.ZipCodes.SingleOrDefault(zc => zc.Zip == m.zip.Zip5());

            if (!m.home.HasValue() && m.cell.HasValue())
                m.home = m.cell;

            if (m.home.HasValue())
                p.Family.UpdateValue(fsb, "HomePhone", m.home.GetDigits());

            if (m.addr.HasValue())
                p.Family.UpdateValue(fsb, "AddressLineOne", m.addr);

            if (m.zip.HasValue())
            {
                p.Family.UpdateValue(fsb, "CityName", z != null ? z.City : null);
                p.Family.UpdateValue(fsb, "StateCode", z != null ? z.State : null);
                p.Family.UpdateValue(fsb, "ZipCode", m.zip);
                var rc = DbUtil.Db.FindResCode(m.zip, null);
                p.Family.UpdateValue(fsb, "ResCodeId", rc.ToString());
            }

            if (m.email.HasValue())
                p.UpdateValue(psb, "EmailAddress", Trim(m.email));

            if (m.cell.HasValue())
                p.UpdateValue(psb, "CellPhone", m.cell.GetDigits());

            p.UpdateValue(psb, "MaritalStatusId", m.marital);

            p.LogChanges(DbUtil.Db, psb);
            p.Family.LogChanges(DbUtil.Db, fsb, p.PeopleId, Util.UserPeopleId ?? 0);
            DbUtil.Db.SubmitChanges();
            if (!DbUtil.Db.Setting("NotifyCheckinChanges", "true").ToBool() || (psb.Count <= 0 && fsb.Count <= 0))
                return;
            var sb = new StringBuilder();
            foreach (var c in psb)
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>\n", c.Field, c.Before, c.After);
            foreach (var c in fsb)
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>\n", c.Field, c.Before, c.After);
            var np = DbUtil.Db.GetNewPeopleManagers();
            if (np != null)
                DbUtil.Db.EmailRedacted(p.FromEmail, np,
                    "Basic Person Info Changed during checkin on " + Util.Host,
                    $"{Util.UserName} changed the following information for {p.PreferredName} ({p.LastName}):<br />\n<table>{sb}</table>");
        }

        [HttpPost]
        public ActionResult JoinUnJoinOrg(int peopleId, int orgId, bool member)
        {
            if (!Authenticate())
                return Content("not authorized");
            var om = DbUtil.Db.OrganizationMembers.SingleOrDefault(m => m.PeopleId == peopleId && m.OrganizationId == orgId);
            if (om == null && member)
            {
                om = OrganizationMember.InsertOrgMembers(DbUtil.Db,
                    orgId, peopleId, MemberTypeCode.Member, DateTime.Now, null, false);
                DbUtil.LogActivity($"iphone join(org:{orgId} person:{peopleId})");
            }
            else if (om != null && !member)
            {
                om.Drop(DbUtil.Db);
                DbUtil.LogActivity($"iphone drop(org:{orgId} person:{peopleId})");
            }
            DbUtil.Db.SubmitChanges();
            return Content("OK");
        }

        private static string Trim(string s)
        {
            return s.HasValue() ? s.Trim() : s;
        }

        [HttpPost]
        public ActionResult RollList2(int id, DateTime datetime)
            // id = OrganizationId
        {
            if (!Authenticate())
                return Content("not authorized");
            return new RollListResult(id, datetime);
        }

        [HttpPost]
        public ActionResult RecordAttend2(int id, DateTime datetime, int peopleId, bool present)
            // id = OrganizationId
        {
            if (!Authenticate())
                return Content("not authorized");
            var u = DbUtil.Db.Users.Single(uu => uu.Username == AccountModel.UserName2);
            RecordAttend2Extracted(id, peopleId, present, datetime, u);
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult RecordVisit2(int id, DateTime datetime, int peopleId)
            // id = OrganizationId
        {
            if (!Authenticate())
                return Content("not authorized");
            var u = DbUtil.Db.Users.Single(uu => uu.Username == AccountModel.UserName2);

            var mid = RecordAttend2Extracted(id, peopleId, true, datetime, u);
            var meeting = DbUtil.Db.LoadMeetingById(mid);
            return new RollListResult(meeting, peopleId);
        }

        private static int RecordAttend2Extracted(int id, int peopleId, bool present, DateTime dt, User u)
        {
            var meetingId = DbUtil.Db.CreateMeeting(id, dt);
            Attend.RecordAttendance(peopleId, meetingId, present);
            DbUtil.Db.UpdateMeetingCounters(id);
            DbUtil.LogActivity($"Mobile RecAtt o:{id} p:{peopleId} u:{Util.UserPeopleId} a:{present}");
            return meetingId;
        }

        private static bool Authenticate(string role = null, bool checkOrgLeadersOnly = false)
        {
            return AccountModel.AuthenticateMobile(role, checkOrgLeadersOnly).IsValid;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public class PersonInfo
        {
            public int addtofamilyid { get; set; }
            public string addr { get; set; }
            public string zip { get; set; }
            public string first { get; set; }
            public string last { get; set; }
            public string goesby { get; set; }
            public string dob { get; set; }
            public string email { get; set; }
            public string cell { get; set; }
            public string home { get; set; }
            public int marital { get; set; }
            public int gender { get; set; }
        }
    }
}
