class GatesPage {
    shortUrl;

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    onChangeDate() {
        const operationDatePicker = document.getElementById("operation-date-picker");
        const url = this.shortUrl + "?date=" + operationDatePicker.value;
        history.pushState({}, null, url);
    }

    show() {
        const operationDatePicker = $("#operation-date-picker");
        operationDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });

        let date = utils.getUrlParameter("date");
        if (date == "") {
            date = utils.getNowDate();
            const url = this.shortUrl + "?date=" + date;
            history.pushState({}, null, url);
        }
        operationDatePicker.datepicker("setDate", date);
        operationDatePicker.on("changeDate", this.onChangeDate.bind(this));
    }

    showGatesPage() {
        const operationDatePicker = document.getElementById("operation-date-picker");
        location.href = this.shortUrl + "?date=" + operationDatePicker.value;
    }

    printBlock(cellId) {
        // hide tables
        $(".statistics-table").addClass("d-none");
        // hide all intervals but selected
        $(".interval-print").addClass("d-none");
        $(".interval-print-" + cellId).removeClass("d-none");

        window.print();
        // unhide all
        $(".statistics-table").removeClass("d-none");
        $(".interval-print").removeClass("d-none");
    }

    printTable() {
        const intervals = document.getElementById("intervals");
        intervals.classList.add("d-none");
        window.print();
        intervals.classList.remove("d-none");
    }
}

const gatesPage = new GatesPage();

$(document).ready(gatesPage.show.bind(gatesPage));