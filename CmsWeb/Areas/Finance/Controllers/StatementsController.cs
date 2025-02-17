using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using CmsData;
using CmsWeb.Areas.Finance.Models.Report;
using UtilityExtensions;
using Task = System.Threading.Tasks.Task;

namespace CmsWeb.Areas.Finance.Controllers
{
    [Authorize(Roles = "Finance")]
    [RouteArea("Finance", AreaPrefix = "Statements"), Route("{action}")]
    public class StatementsController : CmsController
    {
        [Route("~/Statements")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost, Route("Start")]
        public ActionResult ContributionStatements(DateTime? fromDate, DateTime? endDate, string startswith, string sort, int? tagid, bool excludeelectronic)
        {
            if (!fromDate.HasValue || !endDate.HasValue)
                return Content("<h3>Must have a Startdate and Enddate</h3>");
            var runningtotals = new ContributionsRun
            {
                Started = DateTime.Now,
                Count = 0,
                Processed = 0
            };
            if (!startswith.HasValue())
                startswith = null;
            DbUtil.Db.ContributionsRuns.InsertOnSubmit(runningtotals);
            DbUtil.Db.SubmitChanges();
            var cul = DbUtil.Db.Setting("Culture", "en-US");
            var host = Util.Host;

            var output = Output();
            if (tagid == 0)
                tagid = null;

            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cul);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cul);
                var m = new ContributionStatementsExtract(host, fromDate.Value, endDate.Value, output, startswith, sort, tagid, excludeelectronic);
                m.DoWork();
            });
            return Redirect("/Statements/Progress");
        }

        public ActionResult SomeTaskCompleted(string result)
        {
            return Content(result);
        }

        private static string Output()
        {
            var output = ConfigurationManager.AppSettings["SharedFolder"].Replace("%USERPROFILE%", Environment.GetEnvironmentVariable("USERPROFILE"));
            output = output + $"/Statements/contributions_{Util.Host}.pdf";
            return output;
        }

        [HttpPost]
        public JsonResult Progress2()
        {
            var r = DbUtil.Db.ContributionsRuns.OrderByDescending(mm => mm.Id).First();
            var html = new StringBuilder("<a href=\"/Statements/Download\">All Pages</a>");
            if (r.Sets.HasValue())
            {
                var sets = r.Sets.Split(',').Select(ss => ss.ToInt()).ToList();
                foreach (var set in sets)
                    html.Append($" | <a href=\"/Statements/Download/{set}\">Set {set}</a>");
            }
            return Json(new
            {
                r.Count,
                Error = r.Error ?? "",
                r.Processed,
                r.CurrSet,
                Completed = r.Completed.ToString(),
                r.Running,
                html = html.ToString()
            });
        }

        [HttpGet]
        public ActionResult Progress()
        {
            var r = DbUtil.Db.ContributionsRuns.OrderByDescending(mm => mm.Id).First();
            return View(r);
        }

        [HttpGet, Route("~/Statements/Download/{id:int?}")]
        public ActionResult Download(int? id)
        {
            var output = Output();
            var fn = output;
            if (id.HasValue)
                fn = ContributionStatementsExtract.Output(output, id.Value);
            if (!System.IO.File.Exists(fn))
                return Content("no pending download");
            return new ContributionStatementsResult(fn);
        }
    }
}
