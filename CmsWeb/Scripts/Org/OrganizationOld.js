﻿onload = function () {
    var e = document.getElementById("refreshed");
    if (e.value == "no")
        e.value = "yes";
    else {
        e.value = "no";
        location.reload();
    }
};
$(function () {
    $("#Settings-tab").tabs();
    $("li.pending-list").hide();
    $("li.current-list").show();
    $("a.trigger-dropdown").dropdown2();
    $("#main-tab").tabs({
        activate: function (event, ui) {
            var qid = "";

            switch ($(ui.newTab[0]).text()) {
                case "Members":
                    qid = $("#currentQid").val();
                    $("#bluetoolbarstop li > a.qid").parent().removeClass("hidy");
                    $("li.current-list").show();
                    break;
                case "Previous":
                    qid = $("#previousQid").val();
                    $("#bluetoolbarstop li > a.qid").parent().removeClass("hidy");
                    $("li.orgcontext").hide();
                    break;
                case "Pending":
                    qid = $("#pendingQid").val();
                    $("#bluetoolbarstop li > a.qid").parent().removeClass("hidy");
                    $("li.orgcontext").hide();;
                    $("li.pending-list").show();
                    break;
                case "Inactive":
                case "Senders":
                    qid = $("#inactiveQid").val();
                    $("#bluetoolbarstop li > a.qid").parent().removeClass("hidy");
                    $("li.orgcontext").hide();
                    break;
                case "Prospects":
                    qid = $("#prospectsQid").val();
                    $("#bluetoolbarstop li > a.qid").parent().removeClass("hidy");
                    $("li.orgcontext").hide();
                    break;
                case "Guests":
                    qid = $("#visitedQid").val();
                    $("#bluetoolbarstop li > a.qid").parent().removeClass("hidy");
                    $("li.orgcontext").hide();
                    break;
                case "Settings":
                case "Meetings":
                    $("#bluetoolbarstop li > a.qid").parent().addClass("hidy");
                    break;
            }
            if (qid) {
                $("#bluetoolbarstop a.qid").each(function () {
                    $(this).attr("href", this.href.replace(/(.*\/)([^\/?]*)(\?[^?]*)?$/mg, "$1" + qid + "$3"));
                });
            }
        }
    });
    $("#main-tab").show();
    $('#deleteorg').click(function (ev) {
        ev.preventDefault();
        var href = $(this).attr("href");
        if (confirm('Are you sure you want to delete?')) {
            $.block("deleting org");
            $.post(href, null, function (ret) {
                if (ret != "ok") {
                    window.location = ret;
                }
                else {
                    $.block("org deleted");
                    $('.blockOverlay').attr('title', 'Click to unblock').click(function () {
                        $.unblock();
                        window.location = "/";
                    });
                }
            });
        }
        return false;
    });
    $('#sendreminders').click(function (ev) {
        ev.preventDefault();
        var href = $(this).attr("href");
        if (confirm('Are you sure you want to send reminders?')) {
            $.block("sending reminders");
            $.post(href, null, function (ret) {
                if (ret != "ok") {
                    $.unblock();
                    $.growlUI("error", ret);
                }
                else {
                    $.unblock();
                    $.growlUI("Email", "Reminders Sent");
                }
            });
        }
    });
    $('#reminderemails').click(function (ev) {
        ev.preventDefault();
        var href = $(this).attr("href");
        if (confirm('Are you sure you want to send reminders?')) {
            $.block("sending reminders");
            $.post(href, null, function (ret) {
                if (ret != "ok") {
                    $.block(ret);
                    $('.blockOverlay').attr('title', 'Click to unblock').click($.unblock);
                }
                else {
                    $.block("org deleted");
                    $('.blockOverlay').attr('title', 'Click to unblock').click(function () {
                        $.unblock();
                        window.location = "/";
                    });
                }
            });
        }
        return false;
    });
    $(".bt").button();
    $(".datepicker").datetimepicker({
        autoclose: true,
        orientation: "auto",
        minView: 2,
        forceParse: false,
        format: $.dtoptions.format
    });


    $(".CreateAndGo").click(function (ev) {
        ev.preventDefault();
        if (confirm($(this).attr("confirm")))
            $.post($(this).attr("href"), null, function (ret) {
                window.location = ret;
            });
        return false;
    });
    $('#memberDialog').dialog({
        title: 'Add Members Dialog',
        bgiframe: true,
        autoOpen: false,
        zindex: 9999,
        width: 750,
        height: 700,
        modal: true,
        overlay: {
            opacity: 0.5,
            background: "black"
        }, close: function () {
            $('iframe', this).attr("src", "");
        }
    });
    $('#AddFromTag').dialog({
        title: 'Add From Tag',
        bgiframe: true,
        autoOpen: false,
        width: 750,
        height: 650,
        modal: true,
        overlay: {
            opacity: 0.5,
            background: "black"
        }, close: function () {
            $('iframe', this).attr("src", "");
            RebindMemberGrids();
        }
    });
    $('a.addfromtag').live("click", function (e) {
        e.preventDefault();
        var d = $('#AddFromTag');
        $('iframe', d).attr("src", this.href);
        d.dialog("option", "title", "Add Members From Tag");
        d.dialog("open");
    });

    $('a.addmembers').live("click", function (e) {
        e.preventDefault();
        var d = $('#memberDialog');
        $('iframe', d).attr("src", this.href);
        d.dialog("option", "title", "Add Members");
        d.dialog("open");
    });
    $('a.memberdialog').live("click", function (e) {
        e.preventDefault();
        var title;
        var d = $('#memberDialog');
        $('iframe', d).attr("src", this.href);
        d.dialog("option", "title", this.title || 'Edit Member Dialog');
        d.dialog("open");
    });

    $("a.membertype").live("click", function (ev) {
        ev.preventDefault();
        $("<div />").load(this.href, {}, function () {
            var d = $(this);
            var f = d.find("form");
            f.modal("show");
            f.on('hidden', function () {
                d.remove();
                f.remove();
                RebindMemberGrids();
            });
        });
    });

    $("#inactive-link").click(function () {
        $.showTable($('#Inactive-tab form'));
    });
    $("#prospects-link").click(function () {
        $.showTable($('#Prospects-tab form'));
    });
    $("#pending-link").click(function () {
        $.showTable($('#Pending-tab form'));
    });
    $("#priors-link").click(function () {
        $.showTable($('#Priors-tab form'));
    });
    $("#visitors-link").click(function () {
        $.showTable($('#Visitors-tab form'));
    });
    $("#meetings-link").click(function () {
        $.showTable($('#Meetings-tab form'));
    });
    $.maxZIndex = $.fn.maxZIndex = function (opt) {
        var def = { inc: 10, group: "*" };
        $.extend(def, opt);
        var zmax = 0;
        $(def.group).each(function () {
            var cur = parseInt($(this).css('z-index'));
            zmax = cur > zmax ? cur : zmax;
        });
        if (!this.jquery)
            return zmax;

        return this.each(function () {
            zmax += def.inc;
            $(this).css("z-index", zmax);
        });
    };

    $.InitFunctions.ReloadMeetings = function (f) {
        $("#Meetings-tab form").load("/Organization/MeetingGrid", {
            id: $("#OrganizationId").val(),
            future: $("#future").is(':checked')
        });
    }
    $.initDatePicker = function (f) {
        $("ul.edit .datepicker").datetimepicker({
            autoclose: true,
            orientation: "auto",
            minView: 2,
            forceParse: false,
            format: $.dtoptions.format
        });
        $("ul.edit .datetimepicker").datetimepicker({
            format: $.dtoptions.format + " H:ii P",
            showMeridian: true,
            autoclose: true,
            todayBtn: false,
            pickerPosition: "bottom-left"
        });
        $(".timepicker").datetimepicker({
            format: "H:ii P",
            showMeridian: true,
            autoclose: true,
            todayBtn: false,
            pickerPosition: "bottom-left",
            startView: 1,
            minView: 0,
            maxView: 1
        });
        $(".datetimepicker-hours table thead, .datetimepicker-minutes table thead").attr('style', 'display:block; overflow:hidden; height:0;');
    };
    $.showHideRegTypes = function (f) {
        $("#Settings-tab").tabs('option', 'disabled', []);
        $("#QuestionList li").show();
        $(".yes6").hide();
        switch ($("#org_RegistrationTypeId").val()) {
            case "0":
                $("#Settings-tab").tabs('option', 'disabled', [3, 4, 5]);
                break;
            case "6":
                $("#QuestionList > li").hide();
                $(".yes6").show();
                break;
        }
    };
    $("#org_RegistrationTypeId").live("change", $.showHideRegTypes);
    $.showHideRegTypes();
    $("a.displayedit,a.displayedit2").live('click', function (ev) {
        ev.preventDefault();
        var f = $(this).closest('form');
        $.post($(this).attr('href'), null, function (ret) {
            $(f).html(ret).ready(function () {
                $.initDatePicker(f);
                $(".submitbutton,.bt", f).button();
                $(".roundbox select", f).css("width", "100%");
                $("#schedules", f).sortable({ stop: $.renumberListItems });
                $("#editor", f);
                $.regsettingeditclick(f);
                $.showHideRegTypes();
                $.updateQuestionList();
                $("#selectquestions").dialog({
                    title: "Add Question",
                    autoOpen: false,
                    width: 550,
                    height: 250,
                    modal: true
                });
                $('a.AddQuestion').click(function (ev) {
                    var d = $('#selectquestions');
                    d.dialog("open");
                    ev.preventDefault();
                    return false;
                });
                $(".helptip").tooltip({ showBody: "|" });
            });
        });
        return false;
    });
    $('#selectquestions a').live("click", function (ev) {
        ev.preventDefault();
        $.post('/Organization/NewAsk/', { id: 'AskItems', type: $(this).attr("type") }, function (ret) {
            $('#selectquestions').dialog("close");
            $('html, body').animate({ scrollTop: $("body").height() }, 800);
            var newli = $("#QuestionList").append(ret);
            $("#QuestionList > li:last").effect("highlight", {}, 3000);
            $(".tip", newli).tooltip({ opacity: 0, showBody: "|" });
            $.updateQuestionList();
        });
        return false;
    });
    $("ul.enablesort a.del").live("click", function (ev) {
        ev.preventDefault();
        if (!$(this).attr("href"))
            return false;
        $(this).parent().parent().parent().remove();
        return false;
    });
    $("ul.enablesort a.delt").live("click", function (ev) {
        ev.preventDefault();
        if (!$(this).attr("href"))
            return false;
        if (confirm("are you sure?")) {
            $(this).parent().parent().remove();
            $.updateQuestionList();
        }
        return false;
    });
    $.exceptions = [
        "AskDropdown",
        "AskCheckboxes",
        "AskExtraQuestions",
        "AskHeader",
        "AskInstruction",
        "AskMenu",
        "AskText"
    ];
    $.updateQuestionList = function () {
        $("#selectquestions li").each(function () {
            var type = this.className;
            var text = $(this).text();
            if (!text)
                text = type;
            if ($.inArray(type, $.exceptions) >= 0 || $("li.type-" + type).length == 0)
                $(this).html("<a href='#' type='" + type + "'>" + text + "</a>");
            else
                $(this).html("<span>" + text + "</span>");
        });
    };
    $(".helptip").tooltip({ showBody: "|", blocked: true });
    $("form.DisplayEdit a.submitbutton").live('click', function (ev) {
        ev.preventDefault();
        var f = $(this).closest('form');
        if (!$(f).valid())
            return false;
        var q = f.serialize();
        $.post($(this).attr('href'), q, function (ret) {
            if (ret.startsWith("error:")) {
                $("div.formerror", f).html(ret.substring(6));
            } else {
                $(f).html(ret).ready(function () {
                    $(".submitbutton,.bt").button();
                    $.regsettingeditclick(f);
                    $.showHideRegTypes();
                });
            }
        });
        return false;
    });
    $("#future").live('click', function () {
        var f = $(this).closest('form');
        var q = f.serialize();
        $.post($(f).attr("action"), q, function (ret) {
            $(f).html(ret);
            $(".bt", f).button();
        });
    });
    $("#ShowProspects").live('click', function () {
        var f = $(this).closest('form');
        var q = f.serialize();
        $.post($(f).attr("action"), q, function (ret) {
            $(f).html(ret);
            $(".bt", f).button();
        });
    });
    $("form.DisplayEdit").submit(function () {
        if (!$("#submitit").val())
            return false;
        return true;
    });
    $('a.taguntag').live("click", function (ev) {
        ev.preventDefault();
        $.post('/Organization/ToggleTag/' + $(this).attr('pid'), null, function (ret) {
            $(ev.target).text(ret);
        });
        return false;
    });
    $.validator.addMethod("time", function (value, element) {
        return this.optional(element) || /^\d{1,2}:\d{2}\s(?:AM|am|PM|pm)/.test(value);
    }, "time format h:mm AM/PM");
    $.validator.setDefaults({
        highlight: function (input) {
            $(input).addClass("ui-state-highlight");
        },
        unhighlight: function (input) {
            $(input).removeClass("ui-state-highlight");
        }
    });
    $("#orginfoform").validate({
        rules: {
            "org.OrganizationName": { required: true, maxlength: 100 }
        }
    });
    // validate signup form on keyup and submit
    $("#settingsForm").validate({
        rules: {
            "org.SchedTime": { time: true },
            "org.OnLineCatalogSort": { digits: true },
            "org.Limit": { digits: true },
            "org.NumCheckInLabels": { digits: true },
            "org.NumWorkerCheckInLabels": { digits: true },
            "org.FirstMeetingDate": { date: true },
            "org.LastMeetingDate": { date: true },
            "org.RollSheetVisitorWks": { digits: true },
            "org.GradeAgeStart": { digits: true },
            "org.GradeAgeEnd": { digits: true },
            "org.Fee": { number: true },
            "org.Deposit": { number: true },
            "org.ExtraFee": { number: true },
            "org.ShirtFee": { number: true },
            "org.ExtraOptionsLabel": { maxlength: 50 },
            "org.OptionsLabel": { maxlength: 50 },
            "org.NumItemsLabel": { maxlength: 50 },
            "org.GroupToJoin": { digits: true },
            "org.RequestLabel": { maxlength: 50 },
            "org.DonationFundId": { number: true }
        }
    });

    $.getTable = function (f) {
        var q = q || f.serialize();
        var ff = $("#FilterGroups form");
        q = q + '&' + ff.serialize();
        $.post(f.attr('action'), q, function (ret) {
            $(f).html(ret).ready(function () {
                $('.bt').button();
                $(".datepicker").datetimepicker({
                    autoclose: true,
                    orientation: "auto",
                    minView: 2,
                    forceParse: false,
                    format: $.dtoptions.format
                });
            });
        });
        return false;
    };
    $("a.filtergroupslink").live("click", function (ev) {
        ev.preventDefault();
        var f = $(this).closest("form");
        $("#FilterGroups").dialog({
            title: "Filter by Name, Sub-Groups",
            width: "300px",
            buttons: [{
                "text": 'Cancel',
                "class": 'bt',
                "click": function () {
                    $("#FilterGroups").dialog("close");
                }
            }, {
                "text": 'Clear',
                "class": 'bt green',
                "click": function () {
                    $("#namefilter").val('');
                    $("#sgprefix").val('');
                    $("#smallgrouplist").val(null);
                    $.getTable(f);
                    $("#FilterGroups").dialog("close");
                }
            }, {
                "text": 'Ok',
                "class": 'blue bt',
                "click": function () {
                    $.getTable(f);
                    $("#FilterGroups").dialog("close");
                }
            }
            ]
        });
        return false;
    });
    $("#namefilter").keypress(function (e) {
        if ((e.keyCode || e.which) == 13) {
            e.preventDefault();
            var d = $("#FilterGroups").dialog();
            buttons = d.dialog('option', 'buttons');
            buttons[2].click();
        }
        return true;
    });
    $("#addsch").live("click", function (ev) {
        ev.preventDefault();
        var href = $(this).attr("href");
        if (href) {
            var f = $(this).closest('form');
            $.post(href, null, function (ret) {
                $("#schedules", f).append(ret).ready(function () {
                    $.renumberListItems();
                    $.initDatePicker(f);
                });
            });
        }
        return false;
    });
    $("a.deleteschedule").live("click", function (ev) {
        ev.preventDefault();
        var href = $(this).attr("href");
        if (href) {
            $(this).parent().remove();
            $.renumberListItems();
        }
    });
    $.renumberListItems = function () {
        var i = 1;
        $(".renumberMe").each(function () {
            $(this).val(i);
            i++;
        });
    };
    $("#NewMeetingDialog").dialog({
        autoOpen: false,
        width: 560,
        height: 550,
        modal: true
    });
    $("#Schedule_Value").live("change", function () {
        var ss = $(this).val().split(',');
        $(".modal #MeetingDate").val(ss[0]);
        $(".modal #AttendCredit_Value").val(ss[1]);
    });
    $('#RollsheetLink').live("click", function (ev) {
        ev.preventDefault();
        $('#grouplabel').text("By Group");
        $("tr.forMeeting").hide();
        $("tr.forRollsheet").show();
        var d = $("#NewMeetingDialog");
        d.dialog("option", "buttons", {
            "Ok": function () {
                var dt = $.GetNextMeetingDateTime();
                if (!dt.valid)
                    return false;
                var args = "?org=curr&dt=" + dt.date + " " + dt.time;
                if ($('#altnames').is(":checked"))
                    args += "&altnames=true";
                if ($('#group').is(":checked"))
                    args += "&bygroup=1";
                if ($("#highlightsg").val())
                    args += "&highlight=" + $("#highlightsg").val();
                if ($("#sgprefixrs").val())
                    args += "&sgprefix=" + $("#sgprefixrs").val();
                window.open("/Reports/Rollsheet/" + args);
                $(this).dialog("close");
            }
        });
        d.dialog('open');
    });
    $('#RallyRollsheetLink').live("click", function (ev) {
        ev.preventDefault();
        $('#grouplabel').text("By Group");
        $("tr.forMeeting").hide();
        $("tr.forRollsheet").show();
        var d = $("#NewMeetingDialog");
        d.dialog("option", "buttons", {
            "Ok": function () {
                var dt = $.GetNextMeetingDateTime();
                if (!dt.valid)
                    return false;
                var args = "?org=curr&dt=" + dt.date + " " + dt.time;
                if ($('#altnames').is(":checked"))
                    args += "&altnames=true";
                if ($('#group').is(":checked"))
                    args += "&bygroup=1&sgprefix=";
                if ($("#highlightsg").val())
                    args += "&highlight=" + $("#highlightsg").val();
                if ($("#sgprefix").val())
                    args += "&sgprefix=" + $("#sgprefix").val();
                window.open("/Reports/RallyRollsheet/" + args);
                $(this).dialog("close");
            }
        });
        d.dialog('open');
    });
    $('#NewMeeting').live("click", function (ev) {
        ev.preventDefault();
        $('#grouplabel').text("Group Meeting");
        $("tr.forMeeting").show();
        $("tr.forRollsheet").hide();
        var d = $("#NewMeetingDialog");

        var sch = $("#ScheduleListPrev").val();
        if (sch) {
            var a = sch.split(',');
            $("#PrevMeetingDate").val(a[0]);
            $("#NewMeetingTime").val(a[1]);
            $("#AttendCreditList").val(a[2]);
        }

        d.dialog("option", "buttons", {
            "Ok": function () {
                var dt = $.GetPrevMeetingDateTime();
                if (!dt.valid)
                    return false;
                var url = "?d=" + dt.date + "&t=" + dt.time +
                "&group=" + ($('#group').is(":checked") ? "true" : "false");
                $.post("/Organization/NewMeeting", { d: dt.date, t: dt.time, AttendCredit: $("#AttendCreditList").val(), group: $('#group').is(":checked") }, function (ret) {
                    if (!ret.startsWith("error"))
                        window.location = ret;
                });
                $(this).dialog("close");
            }
        });
        d.dialog('open');
        return false;
    });
    $("#ScheduleListPrev").change(function () {
        var a = $(this).val().split(",");
        $("#PrevMeetingDate").val(a[0]);
        $("#NewMeetingTime").val(a[1]);
        $("#AttendCreditList").val(a[2]);
    });
    $("#ScheduleListNext").change(function () {
        var a = $(this).val().split(",");
        $("#NextMeetingDate").val(a[0]);
        $("#NewMeetingTime").val(a[1]);
        $("#AttendCreditList").val(a[2]);
    });
    $.GetPrevMeetingDateTime = function () {
        var d = $('#PrevMeetingDate').val();
        return $.GetMeetingDateTime(d);
    };
    $.GetNextMeetingDateTime = function () {
        var d = $('#NextMeetingDate').val();
        return $.GetMeetingDateTime(d);
    };
    $.GetMeetingDateTime = function (d) {
        var reTime = /^ *(\d{1,2}):[0-5][0-9] *((a|p|A|P)(m|M)){0,1} *$/;
        var t = $('#NewMeetingTime').val();
        var v = true;
        if (!reTime.test(t)) {
            $.growlUI("error", "enter valid time");
            v = false;
        }
        if (!$.DateValid(d)) {
            $.growlUI("error", "enter valid date");
            v = false;
        }
        return { date: d, time: t, valid: v };
    };
    $('a.joinlink').live('click', function (ev) {
        ev.preventDefault();
        $.post($(this)[0].href,
            function (ret) {
                if (ret == "ok")
                    RebindMemberGrids();
                else
                    alert(ret);
            });
        return false;
    });
    $("#divisionlist").live("click", function (ev) {
        ev.preventDefault();
        var a = $(this);
        $("<div />").load(a.attr("href"), {}, function () {
            var d = $(this);
            var f = d.find("form");
            f.modal("show");
            f.on('hidden', function () {
                a.load(a.data("refresh"), {});
                d.remove();
                f.remove();
            });
            f.on("change", "input:checkbox", function () {
                $("input[name='TargetDivision']", f).val($(this).val());
                $("input[name='Adding']", f).val($(this).is(":checked"));
                $.formAjaxClick($(this), "/SearchDivisions/AddRemoveDiv");
            });
            f.on("click", "a.move", function () {
                $("input[name='TargetDivision']", f).val($(this).data("moveid"));
                $.formAjaxClick($(this), "/SearchDivisions/MoveToTop");
            });
        });
    });
    $('#orgsDialog').dialog({
        title: 'Select Orgs Dialog',
        bgiframe: true,
        autoOpen: false,
        width: 690,
        height: 650,
        modal: true,
        overlay: {
            opacity: 0.5,
            background: "black"
        }, close: function () {
            $('iframe', this).attr("src", "");
        }
    });
    $('#orgpicklist').live("click", function (e) {
        e.preventDefault();
        $("<div />").load(this.href, {}, function () {
            var d = $(this);
            var f = d.find("form");
            f.modal("show");
            $.initializeSelectOrgsDialog(f);

            f.on('hidden', function () {
                d.remove();
                f.remove();
            });
        });
    });
    function UpdateSelectedOrgs(list, f) {
        $.post("/Organization/UpdateOrgIds", { id: $("#OrganizationId").val(), list: list }, function (ret) {
            $("#orgpickdiv").html(ret);
            f.modal("hide");
        });
    }
    $.initializeSelectOrgsDialog = function (f) {
        $("#select-orgs").on("click", "#UpdateSelected", function (ev) {
            ev.preventDefault();
            var list = $('#select-orgs input[type=checkbox]:checked').map(function () {
                return $(this).val();
            }).get().join(',');

            UpdateSelectedOrgs(list, f);
            return false;
        });
        $("#select-orgs").on('keydown', "#name", function (ev) {
            if (ev.keyCode === 13) {
                ev.preventDefault();
                $('#select-orgs #search').click();
                return false;
            }
            return true;
        });
        $.SaveOrgIds = function (ev) {
            var list = $('#select-orgs input[type=checkbox]:checked').map(function () {
                return $(this).val();
            }).get().join(',');
            $.post("/SearchOrgs/SaveOrgIds/" + $("#select-orgs #id").val(), { oids: list });
        };
        $('body').on('change', '#select-orgs input:checkbox', $.SaveOrgIds);
    };

    $.extraEditable = function () {
        $('.editarea').editable('/Organization/EditExtra/', {
            type: 'textarea',
            submit: 'OK',
            rows: 5,
            width: 200,
            indicator: '<img src="/Content/images/loading.gif">',
            tooltip: 'Click to edit...'
        });
        $(".editline").editable("/Organization/EditExtra/", {
            indicator: "<img src='/Content/images/loading.gif'>",
            tooltip: "Click to edit...",
            style: 'display: inline',
            width: 200,
            height: 25,
            submit: 'OK'
        });
    };
    $.extraEditable();
    $("#newvalueform").dialog({
        autoOpen: false,
        buttons: {
            "Ok": function () {
                var ck = $("#multiline").is(':checked');
                var fn = $("#fieldname").val();
                var v = $("#fieldvalue").val();
                if (fn)
                    $.post("/Organization/NewExtraValue/" + $("#OrganizationId").val(), { field: fn, value: v, multiline: ck }, function (ret) {
                        if (ret.startsWith("error"))
                            alert(ret);
                        else {
                            $("#extras > tbody").html(ret);
                            $.extraEditable();
                        }
                        $("#fieldname").val("");
                    });
                $(this).dialog("close");
            }
        }
    });
    $("#newextravalue").live("click", function (ev) {
        ev.preventDefault();
        var d = $('#newvalueform');
        d.dialog("open");
    });
    $("a.deleteextra").live("click", function (ev) {
        ev.preventDefault();
        if (confirm("are you sure?"))
            $.post("/Organization/DeleteExtra/" + $("#OrganizationId").val(), { field: $(this).attr("field") }, function (ret) {
                if (ret.startsWith("error"))
                    alert(ret);
                else {
                    $("#extras > tbody").html(ret);
                    $.extraEditable();
                }
            });
        return false;
    });
    // Add for ministrEspace
    var submitDialog = $("#dialogHolder").dialog({ modal: true, width: 'auto', title: 'Select ministrEspace Event', autoOpen: false });
    $("#addMESEvent").click(function (ev) {
        ev.preventDefault();
        var id = $(this).attr("orgid");
        submitDialog.html("<div style='text-align:center; margin-top:20px;'>Loading...</div>");
        submitDialog.dialog('open');
        $.post("/Organization/DialogAdd/" + id + "?type=MES", null, function (data) {
            submitDialog.html(data);
            submitDialog.dialog({ position: { my: "center", at: "center" } });
            $(".bt").button();
        });
    });
    $("#closeSubmitDialog").live("click", null, function (ev) {
        ev.preventDefault();
        $(submitDialog).dialog("close");
    });
    $.InitFunctions.ReloadPeople = function () {
        RebindMemberGrids();
    };
});
function RebindMemberGrids(from) {
    $.updateTable($('#Members-tab form'));
    $.updateTable($('#Inactive-tab form'));
    $.updateTable($('#Pending-tab form'));
    $.updateTable($('#Priors-tab form'));
    $.updateTable($('#Prospects-tab form'));
    $.updateTable($('#Visitors-tab form'));
    $("#memberDialog").dialog("close");
}
function AddSelected() {
    RebindMemberGrids();
}
function CloseAddDialog(from) {
    $("#memberDialog").dialog("close");
}
function UpdateSelectedUsers(topid) {
}
function UpdateSelectedOrgs(list) {
    $.post("/Organization/UpdateOrgIds", { id: $("#OrganizationId").val(), list: list }, function (ret) {
        $("#orgpickdiv").html(ret);
        $("#orgsDialog").dialog("close");
    });
}
