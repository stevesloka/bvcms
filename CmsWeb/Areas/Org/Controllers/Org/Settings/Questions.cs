using System;
using System.Linq;
using System.Web.Mvc;
using CmsData;
using CmsData.Registration;
using UtilityExtensions;

namespace CmsWeb.Areas.Org.Controllers
{
    public partial class OrgController
    {
        [HttpPost]
        public ActionResult Questions(int id)
        {
            return View("Registration/Questions", DbUtil.Db.CreateRegistrationSettings(id));
        }

        [HttpPost]
        public ActionResult QuestionsHelpToggle(int id)
        {
            DbUtil.Db.ToggleUserPreference("ShowQuestionsHelp");
            return PartialView("Registration/Questions", DbUtil.Db.CreateRegistrationSettings(id));
        }

        [HttpPost]
        [Authorize(Roles = "Edit")]
        public ActionResult QuestionsEdit(int id)
        {
            return PartialView("Registration/QuestionsEdit", DbUtil.Db.CreateRegistrationSettings(id));
        }

        [HttpPost]
        public ActionResult QuestionsUpdate(int id)
        {
            var m = DbUtil.Db.CreateRegistrationSettings(id);
            DbUtil.LogActivity($"Update SettingsQuestions {m.org.OrganizationName}");
            m.AskItems.Clear();
            m.TimeSlots.list.Clear();
            try
            {
                if (!TryUpdateModel(m))
                {
                    var q = from e in ModelState.Values
                            where e.Errors.Count > 0
                            select e.Errors[0].ErrorMessage;
                    throw new Exception(q.First());
                }
                var s = m.ToString();
                m = DbUtil.Db.CreateRegistrationSettings(s, id);
                m.org.UpdateRegSetting(m);
                DbUtil.Db.SubmitChanges();
                if (!m.org.NotifyIds.HasValue())
                    ModelState.AddModelError("Form", needNotify);
                return PartialView("Registration/Questions", m);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Form", ex.Message);
                return Content("error:" + ex.Message);
            }
        }

        [HttpPost]
        public ActionResult NewMenuItem(string id)
        {
            return PartialView("EditorTemplates/MenuItem", new AskMenu.MenuItem {Name = id});
        }

        [HttpPost]
        public ActionResult NewDropdownItem(string id)
        {
            return PartialView("EditorTemplates/DropdownItem", new AskDropdown.DropdownItem {Name = id});
        }

        [HttpPost]
        public ActionResult NewCheckbox(string id)
        {
            return PartialView("EditorTemplates/CheckboxItem", new AskCheckboxes.CheckboxItem {Name = id});
        }

        [HttpPost]
        public ActionResult NewGradeOption(string id)
        {
            return PartialView("EditorTemplates/GradeOption", new AskGradeOptions.GradeOption {Name = id});
        }

        [HttpPost]
        public ActionResult NewYesNoQuestion(string id)
        {
            return PartialView("EditorTemplates/YesNoQuestion", new AskYesNoQuestions.YesNoQuestion {Name = id});
        }

        [HttpPost]
        public ActionResult NewSize(string id)
        {
            return PartialView("EditorTemplates/Size", new AskSize.Size {Name = id});
        }

        [HttpPost]
        public ActionResult NewExtraQuestion(string id)
        {
            return PartialView("EditorTemplates/ExtraQuestion", new AskExtraQuestions.ExtraQuestion {Name = id});
        }

        [HttpPost]
        public ActionResult NewText(string id)
        {
            return PartialView("EditorTemplates/Text", new AskExtraQuestions.ExtraQuestion {Name = id});
        }

        [HttpPost]
        public ActionResult NewTimeSlot(string id)
        {
            return PartialView("EditorTemplates/TimeSlot", new TimeSlots.TimeSlot {Name = id});
        }

        [HttpPost]
        public ActionResult NewAsk(string id, string type)
        {
            ViewBag.ShowHelp = DbUtil.Db.UserPreference("ShowQuestionsHelp") == "true";
            var template = "EditorTemplates/" + type;
            switch (type)
            {
                case "AnswersNotRequired":
                case "AskSMS":
                case "AskEmContact":
                case "AskInsurance":
                case "AskDoctor":
                case "AskAllergies":
                case "AskTylenolEtc":
                case "AskParents":
                case "AskCoaching":
                case "AskChurch":
                    return PartialView("EditorTemplates/Ask", new Ask(type) {Name = id});
                case "AskCheckboxes":
                    return PartialView(template, new AskCheckboxes {Name = id});
                case "AskDropdown":
                    return PartialView(template, new AskDropdown {Name = id});
                case "AskMenu":
                    return PartialView(template, new AskMenu {Name = id});
                case "AskSuggestedFee":
                    return PartialView(template, new AskSuggestedFee {Name = id});
                case "AskSize":
                    return PartialView(template, new AskSize {Name = id});
                case "AskRequest":
                    return PartialView(template, new AskRequest {Name = id});
                case "AskHeader":
                    return PartialView(template, new AskHeader {Name = id});
                case "AskInstruction":
                    return PartialView(template, new AskInstruction {Name = id});
                case "AskTickets":
                    return PartialView(template, new AskTickets {Name = id});
                case "AskYesNoQuestions":
                    return PartialView(template, new AskYesNoQuestions {Name = id});
                case "AskExtraQuestions":
                    return PartialView(template, new AskExtraQuestions {Name = id});
                case "AskText":
                    return PartialView(template, new AskText {Name = id});
                case "AskGradeOptions":
                    return PartialView(template, new AskGradeOptions {Name = id});
            }
            return Content("unexpected type " + type);
        }
    }
}
