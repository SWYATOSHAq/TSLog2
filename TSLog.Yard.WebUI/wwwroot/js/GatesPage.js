class GatesPage {
    Show() {
        var operationDatePicker = $("#operation-date-picker");
        operationDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });

        var date = GetUrlParameter("date");
        if (date == "") {
            date = (new Date()).toLocaleString("ru-RU", { year: 'numeric', month: 'numeric', day: 'numeric' });
            var url = window.location.href.split('?')[0] + "?date=" + date;
            history.pushState({}, null, url);
        }
        operationDatePicker.val(date);
    }

    ShowGatesPage() {
        var operationDatePicker = document.getElementById("operation-date-picker");
        location.href = "/GatesPage?date=" + operationDatePicker.value;
    }

    PrintTable() {
        var intervals = document.getElementById("intervals");
        intervals.classList.add("d-none");
        window.print();
        intervals.classList.remove("d-none");
    }
}

var gatesPage = new GatesPage();

function ShowPage() {
    gatesPage.Show();
}

$(document).ready(ShowPage);