class ClientReportPage {
    shortUrl;

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createUrl() {
        const fromOperationDatePicker = document.getElementById("from-operation-date-picker");
        const toOperationDatePicker = document.getElementById("to-operation-date-picker");        
        const partnersDropdown = document.getElementById("partners-select");
        let url = this.shortUrl + "?from_date=" + fromOperationDatePicker.value + "&to_date=" + toOperationDatePicker.value;
        if (partnersDropdown.value != "") {
            url = url + "&partner=" + partnersDropdown.value;
        }
        return url;
    }

    onChangeDate() {
        history.pushState({}, null, this.createUrl());
    }

    show() {
        const fromOperationDatePicker = $("#from-operation-date-picker");
        const toOperationDatePicker = $("#to-operation-date-picker");
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

        let fromDate = utils.getUrlParameter("from_date");
        let toDate = utils.getUrlParameter("to_date");
        if (fromDate == "") {
            fromDate = utils.getNowDate();
        }
        fromOperationDatePicker.datepicker("setDate", fromDate);
        fromOperationDatePicker.on("changeDate", this.onChangeDate.bind(this));
        if (toDate == "") {
            toDate = fromDate;
        }
        toOperationDatePicker.datepicker("setDate", toDate);
        toOperationDatePicker.on("changeDate", this.onChangeDate.bind(this));

        const partner = utils.getUrlParameter("partner");
        const partnersDropdown = document.getElementById("partners-dropdown");
        if (partner != "") {
            partnersDropdown.value = partner;
        }
        window.history.replaceState({}, "", this.createUrl());
    }

    showClientReportPage() {
        const fromOperationDatePicker = document.getElementById("from-operation-date-picker");
        const toOperationDatePicker = document.getElementById("to-operation-date-picker");
        if (fromOperationDatePicker.value <= toOperationDatePicker.value) {
            location.href = this.createUrl();
        } else {
            alert("Параметры отчета заполнены неверно!");
        }
    }

    printTable() {
        window.print();
    }
}

const clientReportPage = new ClientReportPage();

$(document).ready(clientReportPage.show.bind(clientReportPage));
