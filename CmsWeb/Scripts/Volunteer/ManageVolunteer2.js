﻿$(function () {
    $('#sortweek').click(function (ev) {
        ev.preventDefault();
        $('table.grid2 > tbody > tr').sortElements(function (a, b) {
            return $(a).find("td.week").attr("jweek") > $(b).find("td.week").attr("jweek") ? 1 : -1;
        });
    });
    $('#sortday').click(function (ev) {
        ev.preventDefault();
        $('table.grid2 > tbody > tr').sortElements(function (a, b) {
            return $(a).find("td.day").attr("jday") > $(b).find("td.day").attr("jday") ? 1 : -1;
        });
    });
    if (jQuery().transpose) {
        $(".wrapper .item").transpose();
    }
    $("#SelectAll").click(function () {
        if ($(this).attr("checked"))
            $("input[name='pids']").attr('checked', true);
        else
            $("input[name='pids']").removeAttr('checked');
    });
    $("form").submit(function () {
        $("input[name='Commit']").removeAttr("disabled");
    });
    $("#volunteer").change(function () {
        window.location = "/OnlineReg/ManageVolunteer/" + $("#OrgId").val() + "/" + $(this).val();
    });
    var lastChecked = null;
    $("[name='Commit']").click(function (e) {
        var col = $(this).attr("col");
        if (!lastChecked) {
            lastChecked = this;
            return;
        }
        if (e.shiftKey) {
            var colsel = "[name='Commit'][col='" + col + "']";
            var start = $(colsel).index(this);
            var end = $(colsel).index(lastChecked);
            $(colsel).slice(Math.min(start, end), Math.max(start, end) + 1).attr('checked', lastChecked.checked);
        }
        lastChecked = this;
    });
});