/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CmsData;
using CmsWeb.Code;

namespace CmsWeb.Areas.Main.Models.Other
{
	public class VolunteerModel
	{
		public int PeopleId
		{
			get { return peopleid; }
			set
			{
				peopleid = value;
				Person = DbUtil.Db.LoadPersonById(peopleid);
				var q = from v in DbUtil.Db.Volunteers
						  where v.PeopleId == peopleid
						  let approvals = from a in v.VoluteerApprovalIds select a
						  let nonapprovals = from n in DbUtil.Db.VolunteerCodes
													where !v.VoluteerApprovalIds.Select(vv => vv.ApprovalId).Contains(n.Id)
													where n.Id > 0
													select n
						  select new {
							  v,
							  Approvals = (from a in approvals
												select new Approval {
													Id = a.VolunteerCode.Id,
													Approved = true,
													Description = a.VolunteerCode.Description,
													VolApprovalId = a
												}).ToList(),
							  NonApprovals = (from na in nonapprovals
													select new Approval {
														Id = na.Id,
														Approved = false,
														Description = na.Description,
														VolApprovalId = null
													}).ToList()
						  };
				var i = q.SingleOrDefault();
				if (i == null) {
					Volunteer = new Volunteer { PeopleId = peopleid };
					DbUtil.Db.Volunteers.InsertOnSubmit(Volunteer);
					DbUtil.Db.SubmitChanges();
					ApprovalList = (from n in DbUtil.Db.VolunteerCodes
										 where n.Id > 0
										 orderby n.Id
										 select new Approval {
											 Id = n.Id,
											 Approved = false,
											 Description = n.Description
										 }).ToList();
				} else {
					ApprovalList = i.Approvals.Union(i.NonApprovals).OrderBy(aa => aa.Id).ToList();
					Volunteer = i.v;
				}
			}
		}
		private int peopleid;
		public Person Person;

		public VolunteerModel()
		{

		}

		public VolunteerModel(int id)
		{
			PeopleId = id;
		}

		public Volunteer Volunteer { get; set; }

		public class Approval
		{
			public int Id { get; set; }
			public bool Approved { get; set; }
			public string Description { get; set; }
			public VoluteerApprovalId VolApprovalId { get; set; }
		}

		public List<Approval> ApprovalList = new List<Approval>();

		internal void Update(DateTime? processDate, int statusId, string comments, List<int> approvals, DateTime? mvrDate, int mvrStatusId)
		{
			Volunteer.ProcessedDate = processDate;
			Volunteer.StatusId = statusId;
			Volunteer.Comments = comments;

			Volunteer.MVRProcessedDate = mvrDate;
			Volunteer.MVRStatusId = mvrStatusId;

			if (approvals == null)
				approvals = new List<int>();
			var adds = from a in approvals
						  join b in ApprovalList.Where(aa => aa.Approved) on a equals b.Id into j
						  from v in j.DefaultIfEmpty()
						  where v == null
						  select a;
			foreach (var a in adds)
				Volunteer.VoluteerApprovalIds.Add(new VoluteerApprovalId { ApprovalId = a });

			var removes = from b in ApprovalList.Where(aa => aa.Approved)
							  join a in approvals on b.Id equals a into j
							  from v in j.DefaultIfEmpty(-1)
							  where v == -1
							  select b.VolApprovalId;
			DbUtil.Db.VoluteerApprovalIds.DeleteAllOnSubmit(removes);

			DbUtil.Db.SubmitChanges();
		}
		public IEnumerable<string> VolOpportunities()
		{
			return CodeValueModel.VolunteerOpportunities();
			//      var q = (from c in DbUtil.Db.VolInterestInterestCodes
			//where c.PeopleId == vol.PeopleId
			//               group c by c.VolInterestCode.Org into g
			//               select g.Key);
		}
	}
}
