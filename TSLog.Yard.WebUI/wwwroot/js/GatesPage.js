/*
    Страница отчёта по воротам
    Автор - Буров Михаил (m.burov@tslog.ru).
*/
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
        let date = utils.getUrlParameter("date");
        if (date == "") {
            date = utils.getNowDate();
        }
        viewUtils.initDatePicker("operation-date-picker", date, this.onChangeDate.bind(this));
        const url = this.shortUrl + "?date=" + date;
        history.pushState({}, null, url);
    }

    showGatesPage() {
        const operationDatePicker = document.getElementById("operation-date-picker");
        location.href = this.shortUrl + "?date=" + operationDatePicker.value;
    }

    printBlock(cellId) {
        // Hide tables
        $(".statistics-table").addClass("d-none");
        // Hide all intervals but selected
        $(".interval-print").addClass("d-none");
        $(".interval-print-" + cellId).removeClass("d-none");

        window.print();
        // Unhide all
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

document.addEventListener('DOMContentLoaded', gatesPage.show.bind(gatesPage));