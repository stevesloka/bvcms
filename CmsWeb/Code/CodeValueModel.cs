/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license
 */

using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsData;
using CmsData.Classes.ProtectMyMinistry;
using CmsData.Codes;
using UtilityExtensions;

namespace CmsWeb.Code
{
    public class CodeValueModel
    {
        private static readonly CodeValueItem[] top =
        {
            new CodeValueItem
            {
                Id = 0,
                Value = "(not specified)",
                Code = "0"
            }
        };

        public static List<CodeValueItem> GetStateList()
        {
            var q = from s in DbUtil.Db.StateLookups
                    orderby s.StateCode
                    select new CodeValueItem
                    {
                        Code = s.StateCode,
                        Value = s.StateCode + " - " + s.StateName
                    };
            var list = q.ToList();
            list.Insert(0, new CodeValueItem {Code = "", Value = "(not specified)"});
            return list;
        }

        public static IEnumerable<CodeValueItem> GetCountryList()
        {
            return from c in DbUtil.Db.Countries
                   select new CodeValueItem
                   {
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public List<CodeValueItem> GetStateListUnknown()
        {
            var list = GetStateList().ToList();
            list.Insert(1, new CodeValueItem {Code = "na", Value = "(Unknown)"});
            return list;
        }

        public List<CodeValueItem> LetterStatusCodes()
        {
            var q = from ms in DbUtil.Db.MemberLetterStatuses
                    orderby ms.Description
                    select new CodeValueItem
                    {
                        Id = ms.Id,
                        Code = ms.Code,
                        Value = ms.Description
                    };
            return q.ToList();
        }
        public List<CodeValueItem> Activities()
        {
            var q = from a in DbUtil.Db.CheckInActivities
                    group a.Activity by a.Activity
                    into g
                    select new CodeValueItem
                    {
                        Code = g.Key,
                        Value = g.Key
                    };
            var list = q.ToList();
            return list;
        }

        public static IEnumerable<CodeValueItem> AttendCommitmentCodes()
        {
            yield return new CodeValueItem {Id = AttendCommitmentCode.Attending, Code = "AT", Value = "Attending"};
            yield return new CodeValueItem {Id = AttendCommitmentCode.FindSub, Code = "FS", Value = "Find Sub"};
            yield return new CodeValueItem {Id = AttendCommitmentCode.SubFound, Code = "SF", Value = "Sub Found"};
            yield return new CodeValueItem {Id = AttendCommitmentCode.Substitute, Code = "SB", Value = "Substitute"};
            yield return new CodeValueItem {Id = AttendCommitmentCode.Regrets, Code = "RG", Value = "Regrets"};
        }

        public static IEnumerable<CodeValueItem> AttendCredits()
        {
            return from ms in DbUtil.Db.AttendCredits
                   orderby ms.Id
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> EnvelopeOptionList()
        {
            return from ms in DbUtil.Db.EnvelopeOptions
                   orderby ms.Description
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> ContributionOptionsList()
        {
            return EnvelopeOptionList();
        }

        public IEnumerable<CodeValueItem> EnvelopeOptionsList()
        {
            return EnvelopeOptionList();
        }

        public IEnumerable<CodeValueItem> JoinTypeList()
        {
            return from ms in DbUtil.Db.JoinTypes
                   orderby ms.Description
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> VolApplicationStatusCodes()
        {
            var q = from sc in DbUtil.Db.VolApplicationStatuses
                    orderby sc.Description
                    select new CodeValueItem
                    {
                        Id = sc.Id,
                        Code = sc.Code,
                        Value = sc.Description
                    };
            return q.AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> DropTypeList()
        {
            return from ms in DbUtil.Db.DropTypes
                   orderby ms.Description
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> GenderCodes()
        {
            return from ms in DbUtil.Db.Genders
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> BundleStatusTypes()
        {
            return from ms in DbUtil.Db.BundleStatusTypes
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> BundleHeaderTypes()
        {
            return from ms in DbUtil.Db.BundleHeaderTypes
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> BundleHeaderTypes0()
        {
            return BundleHeaderTypes().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> ContributionStatuses()
        {
            return from ms in DbUtil.Db.ContributionStatuses
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> ContributionStatuses99()
        {
            return ContributionStatuses().AddNotSpecified(99);
        }

        public IEnumerable<CodeValueItem> ContributionTypes()
        {
            return from ms in DbUtil.Db.ContributionTypes
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> ContributionTypes0()
        {
            return ContributionTypes().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> Funds()
        {
            var q = from f in DbUtil.Db.ContributionFunds
                    where f.FundStatusId == 1
                    orderby f.FundId
                    select new CodeValueItem
                    {
                        Id = f.FundId,
                        Value = f.FundName
                    };
            var list = q.ToList();
            list.Insert(0, new CodeValueItem {Id = 0, Value = "(not specified)"});
            return list;
        }

        public List<CodeValueItem> GenderCodesWithUnspecified()
        {
            var u = new CodeValueItem {Id = 99, Code = "99", Value = "(not specified)"};
            var list = GenderCodes().ToList();
            list.Insert(0, u);
            return list;
        }

        public IEnumerable<CodeValueItem> NewMemberClassStatusList()
        {
            return from c in DbUtil.Db.NewMemberClassStatuses
                   select new CodeValueItem
                   {
                       Id = c.Id,
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public IEnumerable<CodeValueItem> EntryPoints()
        {
            return from ms in DbUtil.Db.EntryPoints
                   orderby ms.Description
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> OrganizationTypes()
        {
            return from ms in DbUtil.Db.OrganizationTypes
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> OrganizationTypes0()
        {
            return OrganizationTypes().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> Origins()
        {
            return from ms in DbUtil.Db.Origins
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> InterestPoints()
        {
            return from ms in DbUtil.Db.InterestPoints
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> BaptismTypeList()
        {
            return from ms in DbUtil.Db.BaptismTypes
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> BaptismStatusList()
        {
            return from ms in DbUtil.Db.BaptismStatuses
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> DecisionTypeList()
        {
            return from ms in DbUtil.Db.DecisionTypes
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> FamilyPositionCodes()
        {
            return from ms in DbUtil.Db.FamilyPositions
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> Ministries()
        {
            return from m in DbUtil.Db.Ministries
                   orderby m.MinistryName
                   select new CodeValueItem
                   {
                       Id = m.MinistryId,
                       Code = m.MinistryName,
                       Value = m.MinistryName
                   };
        }

        public IEnumerable<CodeValueItem> ContactReasonCodes()
        {
            return from c in DbUtil.Db.ContactReasons
                   orderby c.Description.StartsWith("-") ? "Z" + c.Description : c.Description
                   select new CodeValueItem
                   {
                       Id = c.Id,
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public IEnumerable<CodeValueItem> ContactTypeCodes()
        {
            return from c in DbUtil.Db.ContactTypes
                   orderby c.Description.StartsWith("-") ? "Z" + c.Description : c.Description
                   select new CodeValueItem
                   {
                       Id = c.Id,
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public IEnumerable<CodeValueItem> CampusList()
        {
            return AllCampusesNo();
        }

        public IEnumerable<CodeValueItem> FundList()
        {
            return Funds();
        }

        public IEnumerable<CodeValueItem> Campus0List()
        {
            return AllCampuses0();
        }

        public IEnumerable<CodeValueItem> ContactReasonList()
        {
            return ContactReasonCodes().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> ContactTypeList()
        {
            return ContactTypeCodes().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> CountryList()
        {
            return GetCountryList();
        }

        public IEnumerable<CodeValueItem> EntryPointList()
        {
            return EntryPoints();
        }

        public IEnumerable<CodeValueItem> OriginList()
        {
            return Origins();
        }

        public IEnumerable<CodeValueItem> GenderList()
        {
            return GenderCodesWithUnspecified();
        }

        public IEnumerable<CodeValueItem> MaritalStatusList()
        {
            return MaritalStatusCodes();
        }

        public IEnumerable<CodeValueItem> MemberTypeList()
        {
            return MemberTypeCodes0();
        }

        public IEnumerable<CodeValueItem> MinistryList()
        {
            return Ministries().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> PositionInFamilyList()
        {
            return FamilyPositionCodes();
        }

        public IEnumerable<CodeValueItem> ResCodeList()
        {
            return ResidentCodesWithZero();
        }

        public IEnumerable<CodeValueItem> StateList()
        {
            return GetStateList();
        }

        public IEnumerable<CodeValueItem> LetterStatusList()
        {
            return LetterStatusCodes();
        }

        public IEnumerable<CodeValueItem> OrganizationStatusList()
        {
            return OrganizationStatusCodes();
        }

        public IEnumerable<CodeValueItem> SecurityTypeList()
        {
            return SecurityTypeCodes();
        }

        public IEnumerable<CodeValueItem> OrganizationTypeList()
        {
            return OrganizationTypes0();
        }

        public IEnumerable<CodeValueItem> LeaderMemberTypeList()
        {
            return MemberTypeCodes0().Select(c => new CodeValueItem {Code = c.Code, Id = c.Id, Value = c.Value});
        }

        public IEnumerable<CodeValueItem> AttendCreditList()
        {
            return AttendCredits();
        }

        public IEnumerable<CodeValueItem> RegistrationTypeList()
        {
            return RegistrationTypes();
        }

        public SelectList SchedDayList()
        {
            return DaysOfWeek();
        }

        public static SelectList DaysOfWeek()
        {
            return new SelectList(new[]
            {
                new {Text = "Sunday", Value = "0"},
                new {Text = "Monday", Value = "1"},
                new {Text = "Tuesday", Value = "2"},
                new {Text = "Wednesday", Value = "3"},
                new {Text = "Thursday", Value = "4"},
                new {Text = "Friday", Value = "5"},
                new {Text = "Saturday", Value = "6"},
                new {Text = "Any Day", Value = "10"}
            }, "Value", "Text");
        }

        public SelectList PublishDirectoryList()
        {
            return new SelectList(new[]
            {
                new {Value = "0", Text = "No Directory"},
                new {Value = "1", Text = "Yes Publish Directory"},
                new {Value = "2", Text = "Yes, Publish Family Directory"}
            }, "Value", "Text");
        }

        public SelectList MinistrySelectList()
        {
            return MinistryList().ToSelect();
        }

        public SelectList ContactReasonSelectList()
        {
            return ContactReasonCodes().ToSelect();
        }

        public SelectList ContactTypeSelectList()
        {
            return ContactTypeCodes().ToSelect();
        }

        public SelectList ExtraValueTypeList()
        {
            return ExtraValueTypeCodes().ToSelect("Code");
        }

        public SelectList AdhocExtraValueTypeList()
        {
            return AdhocExtraValueTypeCodes().ToSelect("Code");
        }

        public IEnumerable<CodeValueItem> ContactResultList()
        {
            return new List<CodeValueItem>
            {
                new CodeValueItem {Value = "(not selected)"},
                new CodeValueItem {Value = "Attempted/Not Available"},
                new CodeValueItem {Value = "Left Note Card"},
                new CodeValueItem {Value = "Left Message"},
                new CodeValueItem {Value = "Contact Made"},
                new CodeValueItem {Value = "Gospel Shared"},
                new CodeValueItem {Value = "Prayer Request Received"},
                new CodeValueItem {Value = "Gift Bag Given"}
            };
        }

        public static IEnumerable<CodeValueItem> YesNoAll()
        {
            return new List<CodeValueItem>
            {
                new CodeValueItem {Value = "All"},
                new CodeValueItem {Value = "Yes"},
                new CodeValueItem {Value = "No"}
            };
        }

        public SelectList TagList()
        {
            var tg = UserTags(Util.UserPeopleId).ToList();
            if (HttpContext.Current.User.IsInRole("Edit"))
                tg.Insert(0, new CodeValueItem {Id = -1, Value = "(last query)"});
            tg.Insert(0, new CodeValueItem {Id = 0, Value = "(not specified)"});
            return tg.ToSelect();
        }

        public IEnumerable<CodeValueItem> ExtraValueTypeCodes()
        {
            yield return new CodeValueItem {Code = "Header", Value = "Header"};
            yield return new CodeValueItem {Code = "Link", Value = "Link"};
            yield return new CodeValueItem {Code = "Text", Value = "Text (single line)"};
            yield return new CodeValueItem {Code = "Text2", Value = "Text (multi line)"};
            yield return new CodeValueItem {Code = "Code", Value = "Dropdown"};
            yield return new CodeValueItem {Code = "Bit", Value = "Checkbox"};
            yield return new CodeValueItem {Code = "Bits", Value = "Checkboxes"};
            yield return new CodeValueItem {Code = "Int", Value = "Integer"};
            yield return new CodeValueItem {Code = "Date", Value = "Date"};
        }

        public IEnumerable<CodeValueItem> AdhocExtraValueTypeCodes()
        {
            yield return new CodeValueItem {Code = "Text", Value = "Text (multi line)"};
            yield return new CodeValueItem {Code = "Code", Value = "Code"};
            yield return new CodeValueItem {Code = "Bit", Value = "Checkbox"};
            yield return new CodeValueItem {Code = "Int", Value = "Integer"};
            yield return new CodeValueItem {Code = "Date", Value = "Date"};
        }

        public SelectList PositionInFamilySelectList()
        {
            return PositionInFamilyList().ToSelect();
        }

        public static List<CodeValueItem> PmmLabels()
        {
            var list = (from lab in DbUtil.Db.BackgroundCheckLabels
                        select new CodeValueItem
                        {
                            Id = lab.Id,
                            Code = lab.Code,
                            Value = lab.Description
                        }).ToList();
            list.Insert(0, new CodeValueItem {Id = 0, Code = "LB", Value = "(not specified)"});
            return list;
        }

        public List<CodeValueItem> BackgroundStatuses()
        {
            var list = new List<CodeValueItem>();
            for (var i = 0; i < ProtectMyMinistryHelper.STATUSES.Length; i++)
                list.Add(new CodeValueItem
                {
                    Id = i,
                    Code = i.ToString(),
                    Value = ProtectMyMinistryHelper.STATUSES[i]
                });
            list.Insert(0, new CodeValueItem {Id = 99, Code = "99", Value = "(not specified)"});
            return list;
        }

        public static List<CodeValueItem> UserTags()
        {
            var tid = DbUtil.Db.TagCurrent().Id;
            return new CodeValueModel().UserTags(Util.UserPeopleId).Where(tt => tt.Id != tid).ToList();
        }

        public static List<CodeValueItem> UserTagsAll()
        {
            return new CodeValueModel().UserTags(Util.UserPeopleId).ToList();
        }

        public List<CodeValueItem> UserTags(int? UserPeopleId)
        {
            if (UserPeopleId == Util.UserPeopleId)
                DbUtil.Db.TagCurrent(); // make sure the current tag exists

            var q1 = from t in DbUtil.Db.Tags
                     where t.PeopleId == UserPeopleId
                     where t.TypeId == DbUtil.TagTypeId_Personal
                     orderby t.Name.StartsWith(".") ? "z" : "", t.Name
                     select new CodeValueItem
                     {
                         Id = t.Id,
                         Code = $"{t.Id},{t.PeopleId}!{t.Name}",
                         Value = t.Name
                     };
            var q2 = from t in DbUtil.Db.Tags
                     where t.PeopleId != UserPeopleId
                     where t.TagShares.Any(ts => ts.PeopleId == UserPeopleId)
                     where t.TypeId == DbUtil.TagTypeId_Personal
                     orderby t.PersonOwner.Name2, t.Name.StartsWith(".") ? "z" : "", t.Name
                     let op = DbUtil.Db.People.SingleOrDefault(p => p.PeopleId == t.PeopleId)
                     select new CodeValueItem
                     {
                         Id = t.Id,
                         Code = $"{t.Id},{t.PeopleId}!{t.Name}",
                         Value = op.Name + "!" + t.Name
                     };
            var list = q1.ToList();
            list.AddRange(q2);
            return list;
        }

        public IEnumerable<CodeValueItem> UserTagsWithUnspecified()
        {
            var list = UserTags(Util.UserPeopleId).ToList();
            list.Insert(0, top[0]);
            return list;
        }

        public List<CodeValueItem> PeopleToEmailFor()
        {
            var p = DbUtil.Db.LoadPersonById(Util.UserPeopleId ?? 0);

            var q = from cf in DbUtil.Db.PeopleCanEmailFors
                    where cf.CanEmail == p.PeopleId
                    select new CodeValueItem
                    {
                        Id = cf.OnBehalfOf,
                        Code = cf.OnBehalfOfPerson.EmailAddress,
                        Value = cf.OnBehalfOfPerson.Name
                    };
            var list = q.ToList();
            list.Insert(0, new CodeValueItem
            {
                Id = p.PeopleId,
                Code = p.EmailAddress,
                Value = p.Name
            });
            return list;
        }

        public IEnumerable<CodeValueItem> MaritalStatusCodes()
        {
            return from ms in DbUtil.Db.MaritalStatuses
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> MaritalStatusCodes99()
        {
            return MaritalStatusCodes().AddNotSpecified(99);
        }

        public IEnumerable<CodeValueItem> QueryBuilderFields(string category)
        {
            var n = 1;
            return from f in FieldClass.Fields.Values
                   where f.Category == category
                   select new CodeValueItem
                   {
                       Id = n++,
                       Value = f.Title,
                       Code = f.Name
                   };
        }

        public List<string> QueryBuilderCategories()
        {
            return (from f in CategoryClass.Categories
                    select f.Title).ToList();
        }

        //public List<CodeValueItem> MeetingStatusCodes()
        //{
        //    var list = HttpRuntime.Cache[DbUtil.Db.Host + NAME] as List<CodeValueItem>;
        //    if (list == null)
        //    {
        //        return from ms in DbUtil.Db.MeetingStatuses
        //                select new CodeValueItem
        //                {
        //                    Id = ms.Id,
        //                    Code = ms.Code,
        //                    Value = ms.Description
        //                };
        //        list = q.ToList();
        //        HttpRuntime.Cache[DbUtil.Db.Host + NAME] = list;
        //    }
        //    return list;
        //}
        public List<CodeValueItem> DateFields()
        {
            return new List<CodeValueItem>
            {
                new CodeValueItem {Id = 1, Value = "Joined", Code = "JoinDate"},
                new CodeValueItem {Id = 2, Value = "Dropped", Code = "DropDate"},
                new CodeValueItem {Id = 3, Value = "Decision", Code = "DecisionDate"},
                new CodeValueItem {Id = 4, Value = "Baptism", Code = "BaptismDate"},
                new CodeValueItem {Id = 5, Value = "Wedding", Code = "WeddingDate"},
                new CodeValueItem {Id = 6, Value = "New Member Class", Code = "NewMemberClassDate"}
            };
        }

        public IEnumerable<CodeValueItem> AllCampuses()
        {
            return from c in DbUtil.Db.Campus
                   orderby c.Description
                   select new CodeValueItem
                   {
                       Id = c.Id,
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public IEnumerable<CodeValueItem> AllCampuses0()
        {
            return AllCampuses().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> AllCampusesNo()
        {
            return AllCampuses().AddNotSpecified("No Campus");
        }

        public IEnumerable<CodeValueItem> OrganizationStatusCodes()
        {
            return from c in DbUtil.Db.OrganizationStatuses
                   select new CodeValueItem
                   {
                       Id = c.Id,
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public IEnumerable<CodeValueItem> OrganizationStatusCodes0()
        {
            return OrganizationStatusCodes().AddNotSpecified();
        }

        public static List<CodeValueItem> ResidentCodesWithZero()
        {
            var list = ResidentCodes().ToList();
            list.Insert(0, top[0]);
            return list;
        }

        public static IEnumerable<CodeValueItem> ResidentCodes()
        {
            return from c in DbUtil.Db.ResidentCodes
                   select new CodeValueItem
                   {
                       Id = c.Id,
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public IEnumerable<CodeValueItem> RegistrationTypes()
        {
            var q = RegistrationTypeCode.GetCodePairs();
            if (!HttpContext.Current.User.IsInRole("Developer"))
                q = q.Where(pp => pp.Key != RegistrationTypeCode.RegisterLinkMaster);
            return from i in q
                   select new CodeValueItem
                   {
                       Id = i.Key,
                       Code = i.Key.ToString(),
                       Value = i.Value
                   };
        }

        public IEnumerable<CodeValueItem> RegistrationTypes99()
        {
            var list = (from i in RegistrationTypeCode.GetCodePairs()
                        select new CodeValueItem
                        {
                            Id = i.Key,
                            Code = i.Key.ToString(),
                            Value = i.Value
                        }).ToList();
            list.Insert(0, new CodeValueItem {Id = 99, Code = "99", Value = "(not specified)"});
            return list;
        }

        public List<CodeValueItem> SecurityTypeCodes()
        {
            return new List<CodeValueItem>
            {
                new CodeValueItem {Id = 0, Value = "None", Code = "N"},
                new CodeValueItem {Id = 2, Value = "LeadersOnly", Code = "U"},
                new CodeValueItem {Id = 3, Value = "UnShared", Code = "U"}
            };
        }

        public List<CodeValueItem> BadETCodes()
        {
            return new List<CodeValueItem>
            {
                new CodeValueItem {Id = 11, Value = "Enroll-Enroll", Code = "N"},
                new CodeValueItem {Id = 55, Value = "Drop-Drop", Code = "C"},
                new CodeValueItem {Id = 15, Value = "Same Time", Code = "C"},
                new CodeValueItem {Id = 10, Value = "Missing Drop", Code = "B"}
            };
        }

        public IEnumerable<CodeValueItem> MemberStatusCodes()
        {
            return from ms in DbUtil.Db.MemberStatuses
                   select new CodeValueItem
                   {
                       Id = ms.Id,
                       Code = ms.Code,
                       Value = ms.Description
                   };
        }

        public IEnumerable<CodeValueItem> MemberStatusCodes0()
        {
            return MemberStatusCodes().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> MemberStatusList()
        {
            return MemberStatusCodes();
        }

        public static IEnumerable<CodeValueItem> StatusFlags()
        {
            var sf = from ms in DbUtil.Db.ViewStatusFlagLists.ToList()
                     where ms.RoleName == null || HttpContext.Current.User.IsInRole(ms.RoleName)
                     select new CodeValueItem
                     {
                         Code = ms.Flag,
                         Value = ms.Name
                     };
            return sf.OrderBy(ss => ss.Value);
        }

        public IEnumerable<CodeValueItem> Schedules()
        {
            return from o in DbUtil.Db.Organizations
                   let sc = o.OrgSchedules.FirstOrDefault() // SCHED
                   where sc != null
                   group o by new {sc.ScheduleId, sc.MeetingTime}
                   into g
                   orderby g.Key.ScheduleId
                   where g.Key.ScheduleId != null
                   select new CodeValueItem
                   {
                       Id = (g.Key.ScheduleId.Value),
                       Code = g.Key.ScheduleId.ToString(),
                       Value = DbUtil.Db.GetScheduleDesc(g.Key.MeetingTime)
                   };
        }

        public IEnumerable<CodeValueItem> Schedules0()
        {
            return Schedules().AddNotSpecified();
        }

        public IEnumerable<CodeValueItem> UserRoles()
        {
            var q = from s in DbUtil.Db.Roles
                    orderby s.RoleId
                    select new CodeValueItem
                    {
                        Id = s.RoleId,
                        Code = s.RoleName,
                        Value = s.RoleName
                    };
            var list = q.ToList();
            list.Insert(0, new CodeValueItem {Value = "(not specified)", Id = 0});
            return list;
        }

        public IEnumerable<CodeValueItem> UserRolesMyData()
        {
            var q = from s in DbUtil.Db.Roles
                    orderby s.RoleId
                    select new CodeValueItem
                    {
                        Id = s.RoleId,
                        Code = s.RoleName,
                        Value = s.RoleName
                    };
            var list = q.ToList();
            list.Insert(0, new CodeValueItem {Value = "(mydata, no role)", Id = 0});
            return list;
        }

        public IEnumerable<CodeValueItem> MemberTypeCodesByFreq()
        {
            var q = from mt in DbUtil.Db.OrganizationMembers
                    group mt by mt.MemberTypeId
                    into g
                    orderby g.Count()
                    select new {g.Key, count = g.Count()};

            var q2 = from mt in DbUtil.Db.MemberTypes
                     join g in q on mt.Id equals g.Key
                     orderby g.count descending
                     select new CodeValueItem
                     {
                         Id = mt.Id,
                         Code = mt.Code,
                         Value = mt.Description
                     };
            return q2;
        }

        public static IEnumerable<MemberTypeItem> MemberTypeCodes2()
        {
            return from mt in DbUtil.Db.MemberTypes
                   where mt.Id != MemberTypeCode.Visitor
                   where mt.Id != MemberTypeCode.VisitingMember
                   orderby mt.Description
                   select new MemberTypeItem
                   {
                       Id = mt.Id,
                       Code = mt.Code,
                       Value = mt.Description,
                       AttendanceTypeId = mt.AttendanceTypeId
                   };
        }

        public static List<MemberTypeItem> MemberTypeCodes0()
        {
            var list = MemberTypeCodes2().ToList();
            list.Insert(0, new MemberTypeItem {Id = 0, Value = "(not specified)"});
            return list;
        }

        public static IEnumerable<CodeValueItem> MemberTypeCodes()
        {
            var list = MemberTypeCodes2();
            return list.Select(c => new CodeValueItem {Code = c.Code, Id = c.Id, Value = c.Value});
        }

        public IEnumerable<CodeValueItem> AttendanceTypeCodes()
        {
            return from c in DbUtil.Db.AttendTypes
                   select new CodeValueItem
                   {
                       Id = c.Id,
                       Code = c.Code,
                       Value = c.Description
                   };
        }

        public IEnumerable<CodeValueItem> AddressTypeCodes()
        {
            return from at in DbUtil.Db.AddressTypes
                   select new CodeValueItem
                   {
                       Id = at.Id,
                       Code = at.Code,
                       Value = at.Description
                   };
        }

        public IEnumerable<CodeValueItem> Schools()
        {
            return from p in DbUtil.Db.People
                   group p by p.SchoolOther
                   into g
                   orderby g.Key
                   select new CodeValueItem
                   {
                       Value = g.Key,
                       Code = g.Key
                   };
        }

        public IEnumerable<CodeValueItem> Employers()
        {
            return from p in DbUtil.Db.People
                   group p by p.EmployerOther
                   into g
                   orderby g.Key
                   select new CodeValueItem
                   {
                       Value = g.Key
                   };
        }

        public IEnumerable<CodeValueItem> Occupations()
        {
            return from p in DbUtil.Db.People
                   group p by p.OccupationOther
                   into g
                   orderby g.Key
                   select new CodeValueItem
                   {
                       Value = g.Key
                   };
        }

        public IEnumerable<CodeValueItem> VolunteerCodes()
        {
            return from vc in DbUtil.Db.VolunteerCodes
                   select new CodeValueItem
                   {
                       Id = vc.Id,
                       Code = vc.Code,
                       Value = vc.Description
                   };
        }

        public IEnumerable<CodeValueItem> TaskStatusCodes()
        {
            return from vc in DbUtil.Db.TaskStatuses
                   orderby vc.Description
                   select new CodeValueItem
                   {
                       Id = vc.Id,
                       Code = vc.Code,
                       Value = vc.Description
                   };
        }

        public static IEnumerable<string> VolunteerOpportunities()
        {
            return from c in DbUtil.Db.Contents
                   where c.Name.StartsWith("Volunteer-")
                   where c.Name.EndsWith(".view")
                   orderby c.Name
                   select c.Name.Substring(10, c.Name.Length - 15);
        }

        //--------------------------------------------------
        //--------------Organizations---------------------

        public IEnumerable<CodeValueItem> GetOrganizationList(int DivId)
        {
            return from ot in DbUtil.Db.DivOrgs
                   where ot.DivId == DivId
                         && (SqlMethods.DateDiffMonth(ot.Organization.OrganizationClosedDate, Util.Now) < 14
                             || ot.Organization.OrganizationStatusId == 30)
                   orderby ot.Organization.OrganizationStatusId, ot.Organization.OrganizationName
                   select new CodeValueItem
                   {
                       Id = ot.OrgId,
                       Value = Organization.FormatOrgName(ot.Organization.OrganizationName,
                           ot.Organization.LeaderName, ot.Organization.Location)
                   };
        }

        public IEnumerable<CodeValueItem> OrgDivTags()
        {
            return from t in DbUtil.Db.Programs
                   orderby t.Name
                   select new CodeValueItem
                   {
                       Id = t.Id,
                       Value = t.Name
                   };
        }

        public IEnumerable<CodeValueItem> OrgSubDivTags(int ProgId)
        {
            var q = from div in DbUtil.Db.Divisions
                    where div.ProgId == ProgId
                    orderby div.Name
                    select new CodeValueItem
                    {
                        Id = div.Id,
                        Value = div.Name
                    };
            return top.Union(q);
        }

        public IEnumerable<string> OrgSubDivTags2(int ProgId)
        {
            return from program in DbUtil.Db.Programs
                   from div in program.Divisions
                   where (program.Id == ProgId || ProgId == 0)
                   orderby program.Name, div.Name
                   select (ProgId > 0 ? program.Name + "." : "") + div.Name;
        }

        public IEnumerable<CodeValueItem> AllOrgDivTags()
        {
            var q = from program in DbUtil.Db.Programs
                    from div in program.Divisions
                    orderby program.Name, div.Name
                    select new CodeValueItem
                    {
                        Id = div.Id,
                        Value = $"{program.Name}: {div.Name}"
                    };
            return top.Union(q);
        }

        public IEnumerable<DropDownItem> AllOrgDivTags2()
        {
            var q = from program in DbUtil.Db.Programs
                    from div in program.Divisions
                    orderby program.Name, div.Name
                    select new DropDownItem
                    {
                        Value = $"{program.Id}:{div.Id}",
                        Text = $"{program.Name}: {div.Name}"
                    };
            return (new[]
            {
                new DropDownItem
                {
                    Text = "(not specified)",
                    Value = "0"
                }
            }).Union(q);
        }

        public IEnumerable<CodeValueItem> Organizations(int SubDivId)
        {
            return top.Union(GetOrganizationList(SubDivId));
        }

        public static IEnumerable<SelectListItem> Tags()
        {
            var cv = new CodeValueModel();
            var tg = ConvertToSelect(cv.UserTags(Util.UserPeopleId), "Id").ToList();
            if (HttpContext.Current.User.IsInRole("Edit"))
                tg.Insert(0, new SelectListItem {Value = "-1", Text = "(last query)"});
            tg.Insert(0, new SelectListItem {Value = "0", Text = "(not specified)"});
            return tg;
        }

        public static IEnumerable<SelectListItem> StatusIds()
        {
            var q = from s in DbUtil.Db.OrganizationStatuses
                    select new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Description
                    };
            var list = q.ToList();
            list.Insert(0, new SelectListItem {Value = "0", Text = "(not specified)"});
            return list;
        }

        public static IEnumerable<SelectListItem> OrgTypes()
        {
            var q = from t in DbUtil.Db.OrganizationTypes
                    orderby t.Code
                    select new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Description
                    };
            var list = q.ToList();
            list.Insert(0, new SelectListItem {Text = "Suspended Checkin", Value = OrgType.SuspendedCheckin.ToString()});
            list.Insert(0, new SelectListItem {Text = "Main Fellowship", Value = OrgType.MainFellowship.ToString()});
            list.Insert(0, new SelectListItem {Text = "Not Main Fellowship", Value = OrgType.NotMainFellowship.ToString()});
            list.Insert(0, new SelectListItem {Text = "Parent Org", Value = OrgType.ParentOrg.ToString()});
            list.Insert(0, new SelectListItem {Text = "Child Org", Value = OrgType.ChildOrg.ToString()});
            list.Insert(0, new SelectListItem {Text = "Orgs Without Type", Value = OrgType.NoOrgType.ToString()});
            list.Insert(0, new SelectListItem {Text = "Orgs With Fees", Value = OrgType.Fees.ToString()});
            list.Insert(0, new SelectListItem {Text = "Orgs Without Fees", Value = OrgType.NoFees.ToString()});
            list.Insert(0, new SelectListItem {Text = "(not specified)", Value = "0"});
            return list;
        }

        public static IEnumerable<SelectListItem> RegistrationTypeIds()
        {
            var q = from o in RegistrationTypeCode.GetCodePairs()
                    select new SelectListItem
                    {
                        Value = o.Key.ToString(),
                        Text = o.Value
                    };
            var list = q.ToList();
            list.Insert(0, new SelectListItem
            {
                Value = RegistrationClassification.AnyOnlineRegOnApp94.ToString(),
                Text = "(any reg on app)"
            });
            list.Insert(0, new SelectListItem
            {
                Value = RegistrationClassification.AnyOnlineRegNotOnApp95.ToString(),
                Text = "(active reg not on app)"
            });
            list.Insert(0, new SelectListItem
            {
                Value = RegistrationClassification.AnyOnlineRegActive96.ToString(),
                Text = "(active registration)"
            });
            list.Insert(0, new SelectListItem
            {
                Value = RegistrationClassification.AnyOnlineRegNonPicklist97.ToString(),
                Text = "(registration, no picklist)"
            });
            list.Insert(0, new SelectListItem
            {
                Value = RegistrationClassification.AnyOnlineReg99.ToString(),
                Text = "(any registration)"
            });
            list.Insert(0, new SelectListItem
            {
                Value = "-1",
                Text = "(not specified)"
            });
            list.Add(new SelectListItem
            {
                Value = RegistrationClassification.AnyOnlineRegMissionTrip98.ToString(),
                Text = "Mission Trip"
            });
            return list;
        }

        public static List<SelectListItem> ConvertToSelect(object items, string valuefield)
        {
            var list = items as IEnumerable<CodeValueItem>;
            if (list == null)
                throw new Exception("items are null in ConvertToSelect");
            List<SelectListItem> list2;
            switch (valuefield)
            {
                case "IdCode":
                    list2 = list.Select(c => new SelectListItem {Text = c.Value, Value = c.IdCode}).ToList();
                    break;
                case "Id":
                    list2 = list.Select(c => new SelectListItem {Text = c.Value, Value = c.Id.ToString()}).ToList();
                    break;
                case "Code":
                    list2 = list.Select(c => new SelectListItem {Text = c.Value, Value = c.Code}).ToList();
                    break;
                default:
                    list2 = list.Select(c => new SelectListItem {Text = c.Value, Value = c.Value}).ToList();
                    break;
            }
            if (list2.Count > 0)
                list2[0].Selected = true;
            return list2;
        }

        public class DropDownItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        public class OrgType
        {
            public const int NoFees = -8;
            public const int Fees = -7;
            public const int ChildOrg = -6;
            public const int ParentOrg = -5;
            public const int SuspendedCheckin = -4;
            public const int MainFellowship = -3;
            public const int NotMainFellowship = -2;
            public const int NoOrgType = -1;
        }

        public class RegistrationClassification
        {
            public const int NotSpecified = -1;
            public const int AnyOnlineReg99 = 99;
            public const int AnyOnlineRegMissionTrip98 = 98;
            public const int AnyOnlineRegNonPicklist97 = 97;
            public const int AnyOnlineRegActive96 = 96;
            public const int AnyOnlineRegNotOnApp95 = 95;
            public const int AnyOnlineRegOnApp94 = 94;
        }
    }
}
