function ShowPage() {
    var fromOperationDatePicker = $("#from-operation-date-picker");
    var toOperationDatePicker = $("#to-operation-date-picker");
    fromOperationDatePicker.datepicker({
        format: "dd.mm.yyyy",
        autoclose: true,
        language: "ru",
        todayBtn: true,
        todayHighlight: true,
    });
    toOperationDatePicker.datepicker({
        format: "dd.mm.yyyy",
        autoclose: true,
        language: "ru",
        todayBtn: true,
        todayHighlight: true,
    });

    var fromDate = GetUrlParameter("from_date");
    var toDate = GetUrlParameter("to_date");    
    if (fromDate == "") {
        fromDate = (new Date()).toLocaleString("ru-RU", { year: 'numeric', month: 'numeric', day: 'numeric' });
    }
    fromOperationDatePicker.val(fromDate);
    if (toDate == "") {
        toDate = fromDate;
    }    
    toOperationDatePicker.val(toDate);

    var url = window.location.href.split('?')[0] + "?" +
        "from_date=" + fromDate +
        "&to_date=" + toDate;

    var partner = GetUrlParameter("partner");
    var partnersDropdown = document.getElementById("partners-dropdown");
    if (partner != "") {
        partnersDropdown.value = partner;
        url = url + "&partner=" + partner;
    }
    window.history.replaceState({}, "", url);
    var a = 1;
}

function ShowClientReportPage() {
    var fromOperationDatePicker = document.getElementById("from-operation-date-picker");
    var toOperationDatePicker = document.getElementById("to-operation-date-picker");
    var fromDate = fromOperationDatePicker.value;
    var toDate = toOperationDatePicker.value;

    var partnersDropdown = document.getElementById("partners-dropdown");

    if (fromDate <= toDate) {
        var url = "/ClientReportPage?" +
            "from_date=" + fromDate +
            "&to_date=" + toDate;
        if (partnersDropdown.value != "")
            url = url + "&partner=" + partnersDropdown.value;
        location.href = url;            
    } else {
        alert("Параметры отчета заполнены неверно");
    }
}

function PrintTable() {
    var intervals = document.getElementById("intervals");
    intervals.classList.add("d-none");
    window.print();
    intervals.classList.remove("d-none");
}

$(document).ready(ShowPage);
