function ShowPage() {
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

function ShowStaffPage() {
    var operationDatePicker = document.getElementById("operation-date-picker");
    location.href = "/StaffPage?date=" + operationDatePicker.value;
}

function PrintTable() {
    var intervals = document.getElementById("intervals");
    intervals.classList.add("d-none");
    window.print();
    intervals.classList.remove("d-none");
}

$(document).ready(ShowPage);
