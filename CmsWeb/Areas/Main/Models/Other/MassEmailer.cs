using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using CmsData;
using CmsWeb.Code;
using MoreLinq;
using UtilityExtensions;
using System.IO;
using System.Web.Mvc;
using System.Net.Mail;
using System.Text.RegularExpressions;
using CmsWeb.Models;

namespace CmsWeb.Areas.Main.Models
{
    [Serializable]
    public class MassEmailer
    {
        public int Count { get; set; }

        public int TagId { get; set; }
        public int? OrgId { get; set; }
        public bool wantParents { get; set; }
        public bool noDuplicates { get; set; }
        public bool CcParents { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime? Schedule { get; set; }
        public bool PublicViewable { get; set; }
        public IEnumerable<string> Recipients { get; set; } 

        public string Host { get; set; }

        public MassEmailer()
        {
        }

        public MassEmailer(int tagid, string host, string subject, bool? parents = false)
        {
            wantParents = parents ?? false;
            var tag = DbUtil.Db.TagById(tagid);
            TagId = tag.Id;
            var people = tag.People(DbUtil.Db);
            Recipients = people.Select(p => p.ToString()).ToList();
            Count = people.Count();

        }
        public MassEmailer(Guid id, bool? parents = null, bool? ccparents = null, bool? nodups = null, int? orgid = null)
        {
            wantParents = parents ?? false;
            noDuplicates = nodups ?? false;
            CcParents = ccparents ?? false;
            OrgId = orgid;
            IQueryable<Person> q = null;
            if (OrgId.HasValue)
                q = from p in DbUtil.Db.PeopleQuery(DbUtil.Db.QueryInCurrentOrg().QueryId)
                    where p.EmailAddress != null
                    where p.EmailAddress != ""
                    where (p.SendEmailAddress1 ?? true) || (p.SendEmailAddress2 ?? false)
                    select p;
            else
            {
                q = DbUtil.Db.PeopleQuery(id);
                var c = DbUtil.Db.LoadQueryById2(id);
                var cc = c.ToClause();

                if (!cc.PlusParentsOf && !cc.ParentsOf && wantParents)
                    q = DbUtil.Db.PersonQueryParents(q);

                if (CcParents)
                    q = from p in q
                        where (p.EmailAddress ?? "") != ""
                              || (p.Family.HeadOfHousehold.EmailAddress ?? "") != ""
                              || (p.Family.HeadOfHouseholdSpouse.EmailAddress ?? "") != ""
                        where (p.SendEmailAddress1 ?? true)
                              || (p.SendEmailAddress2 ?? false)
                              || (p.Family.HeadOfHousehold.SendEmailAddress1 ?? false)
                              || (p.Family.HeadOfHousehold.SendEmailAddress2 ?? false)
                              || (p.Family.HeadOfHouseholdSpouse.SendEmailAddress1 ?? false)
                              || (p.Family.HeadOfHouseholdSpouse.SendEmailAddress2 ?? false)
                        select p;
                else
                    q = from p in q
                        where p.EmailAddress != null
                        where p.EmailAddress != ""
                        where (p.SendEmailAddress1 ?? true) || (p.SendEmailAddress2 ?? false)
                        select p;
            }
            var tag = DbUtil.Db.PopulateSpecialTag(q, DbUtil.TagTypeId_Emailer);
            TagId = tag.Id;
            if (noDuplicates)
                DbUtil.Db.NoEmailDupsInTag(TagId);

            var people = tag.People(DbUtil.Db);
            Recipients = people.Select(p => p.ToString()).ToList();
            Count = people.Count();
        }

        public EmailQueue CreateQueueForOrg()
        {
            var From = new MailAddress(FromAddress, FromName);
            DbUtil.Db.CopySession();

            if (!Schedule.HasValue)
            {
                var tag = DbUtil.Db.TagById(TagId);
                var q = tag.People(DbUtil.Db);
                Count = q.Count();
                if (Count >= 300)
                    Schedule = Util.Now.AddSeconds(10); // some time for emailqueue to be ready to go
            }

            if (!OrgId.HasValue)
                throw new Exception("no org to email from");
            var emailqueue = DbUtil.Db.CreateQueueForOrg(From, Subject, Body, Schedule, OrgId.Value, PublicViewable);
            if (emailqueue == null)
                return null;
            emailqueue.NoReplacements = noDuplicates;
            DbUtil.Db.SubmitChanges();
            return emailqueue;
        }
        public EmailQueue CreateQueue(bool transactional = false)
        {
            if(OrgId.HasValue)
                return CreateQueueForOrg();
            var From = new MailAddress(FromAddress, FromName);
            DbUtil.Db.CopySession();

            if (!Schedule.HasValue)
            {
                var tag = DbUtil.Db.TagById(TagId);
                if(tag == null)
                    throw new Exception("email tag is missing");
                var q = tag.People(DbUtil.Db);
                Count = q.Count();
                if (Count >= 300)
                    Schedule = Util.Now.AddSeconds(10); // some time for emailqueue to be ready to go
            }

            var emailqueue = DbUtil.Db.CreateQueue(From, Subject, Body, Schedule, TagId, PublicViewable, CcParents);
            if (emailqueue == null)
                return null;
            emailqueue.NoReplacements = noDuplicates;
            emailqueue.Transactional = transactional;
            DbUtil.Db.SubmitChanges();
            return emailqueue;
        }

        public IEnumerable<SelectListItem> EmailFroms()
        {
            return new SelectList(new CodeValueModel().PeopleToEmailFor(), "Code", "Value");
        }
    }
}