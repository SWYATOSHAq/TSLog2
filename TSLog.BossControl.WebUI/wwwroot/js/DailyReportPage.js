/*
    Страница отчёта диспетчера (за период).
    Автор - Буров Михаил.
    Создан - 2022.07.14.
*/

// Страница отчёта диспетчера (за период).
class DailyReportPage {
    shortUrl;
    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createOpenOperationFunction(id) {
        return function () {
            sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
            location.href = "/OperationPage?id=" + id + "&action=VIEW";
        }
    }

    createParameters() {
        let result = "";

        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");

        return result != "" ? "?" + result : "";
    }

    async getData() {
        const fromDate = utils.getUrlParameter("from_date");
        const toDate = utils.getUrlParameter("to_date");
        const result = await api.createDispatcherReport(fromDate, toDate);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            alert(result.message);
        }
    }

    clearOperations() {
        const headerCells = document.getElementsByClassName("header-cell");
        while (headerCells.length > 0) {
            headerCells[0].remove();
        }
        const contentCells = document.getElementsByClassName("content-cell");
        while (contentCells.length > 0) {
            contentCells[0].remove();
        }
        this.data = [];
    }

    async showData() {
        const headerTr = document.getElementById("header-tr");
    }

    async onChangeFilter() {
        this.clearOperations();
        await this.getData();
        await this.showData();
    }

    onChangeParameters() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async createReport() {
        this.clearOperations();
        await this.getData();
        await this.showData();
    }

    exportToExcel() {
        const reportTable = document.getElementById("report-table");

        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        const fileName = "dailyReport_" + fromDate + "_" + toDate;
        const sheetName = fromDate + " - " + toDate;

        viewUtils.tableToExcel_v2(reportTable, fileName, "xlsx", sheetName);
    }

    async show() {
        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        viewUtils.initDatePicker("from-date-input", fromDate, this.onChangeParameters.bind(this));
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        viewUtils.initDatePicker("to-date-input", toDate, this.onChangeParameters.bind(this));        

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        await this.createReport();
    }
}

const dailyReportPage = new DailyReportPage();

document.addEventListener('DOMContentLoaded', dailyReportPage.show.bind(dailyReportPage));