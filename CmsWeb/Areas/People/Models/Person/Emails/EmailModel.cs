﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsData;
using CmsWeb.Models;
using UtilityExtensions;

namespace CmsWeb.Areas.People.Models
{
    public abstract class EmailModel : PagedTableModel<EmailQueue, EmailRow>
    {
        public int? PeopleId { get; set; }
        public Person Person
        {
            get
            {
                if (_person == null && PeopleId.HasValue)
                    _person = DbUtil.Db.LoadPersonById(PeopleId.Value);
                return _person;
            }
        }
        private Person _person;

        protected EmailModel()
            : base("Sent", "desc", true)
        {}

        internal IQueryable<EmailQueue> FilterForUser(IQueryable<EmailQueue> q)
        {
            var user = HttpContext.Current.User;
            if(user.IsInRole("Admin") || user.IsInRole("ManageEmails"))
                return q;

            q = from e in q
                let p = DbUtil.Db.People.Single(pp => pp.PeopleId == Util.UserPeopleId)
                let isSender = e.QueuedBy == Util.UserPeopleId
                               || (e.FromAddr == p.EmailAddress && p.EmailAddress.Length > 0)
                               || (e.FromAddr == p.EmailAddress2 && p.EmailAddress2.Length > 0)
                let isReceiver = e.EmailQueueTos.Any(ee => ee.PeopleId == Util.UserPeopleId)
                where isSender || isReceiver
                select e;
            return q;
        }

        public override IEnumerable<EmailRow> DefineViewList(IQueryable<EmailQueue> q)
        {
            return from e in q
                   select new EmailRow
                   {
                       Id = e.Id,
                       Sent = e.Sent,
                       SendWhen = e.SendWhen,
                       Queued = e.Queued,
                       From = e.FromName,
                       FromAddr = e.FromAddr,
                       Count = e.EmailQueueTos.Count(),
                       Subject = e.Subject
                   };
        }
        public override IQueryable<EmailQueue> DefineModelSort(IQueryable<EmailQueue> q)
        {
            switch (SortExpression)
            {
                case "Sent":
                    return from e in q
                           orderby e.Sent
                           select e;
                case "Scheduled":
                    return from e in q
                           orderby e.SendWhen
                           select e;
                case "Sent desc":
                    return from e in q
                           orderby e.Sent descending
                           select e;
                case "Scheduled desc":
                    return from e in q
                           orderby e.SendWhen descending
                           select e;
                case "From":
                    return from e in q
                           orderby e.FromName
                           select e;
                case "From desc":
                    return from e in q
                           orderby e.FromName descending 
                           select e;
                case "Count":
                    return from e in q
                           orderby e.EmailQueueTos.Count(), e.Sent
                           select e;
                case "Count desc":
                    return from e in q
                           orderby e.EmailQueueTos.Count() descending, e.Sent
                           select e;
                case "Subject":
                    return from e in q
                           orderby e.Subject, e.Sent
                           select e;
                case "Subject desc":
                    return from e in q
                           orderby e.Subject descending, e.Sent
                           select e;
            }
            return q;
        }
    }
}