/*
    Страница отчёта по производительности по работникам (за период).
    Автор - Буров Михаил (m.burov@tslog.ru).
*/

// Страница отчёта по производительности по работникам (за период, с указанием основных должностей).
class WorkersPerfomanceReportPage {
    shortUrl;

    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");
        //result = utils.addParameter(result, "partner_id", "partners-select");
        return result != "" ? "?" + result : "";
    }

    async getData(fromDate, toDate) {
        // const result = await api.createWorkersPerfomanceReport2(fromDate, toDate);
        const result = await api.createWorkersPerfomanceReport2(fromDate, toDate);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            this.data = [];
            viewUtils.showError(result.message);
        }
    }

    clearReport() {
        const elementsTbody = document.getElementById("report-tbody");
        elementsTbody.replaceChildren();
    }

    showReport() {
        const tbody = document.getElementById("report-tbody");
        for (let element of this.data) {
            const tr = document.createElement("tr");
            viewUtils.createCell(tr, ["table-cell"], [element.workerFullName]);
            viewUtils.createCell(tr, ["table-cell"], [element.workerPositionName]);
            //viewUtils.createCell(tr, ["table-cell"], [Math.round(element.perfomance).toString() + " / " + Math.round(element.perfomance2).toString()]);
            viewUtils.createCell(tr, ["table-cell"], [Math.round(element.perfomance).toString()]);
            tbody.append(tr);
        }
    }

    showReportTitle() {
        const reportHeaderB = document.getElementById("report-header-b")
        const fromDate = utils.getUrlParameter("from_date");
        const toDate = utils.getUrlParameter("to_date");
        reportHeaderB.textContent = "Отчёт по производительности работников с " + fromDate + " по " + toDate + " (ВКЛЮЧАЯ!)";
    }

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        this.showShiftInfo();
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"), utils.getUrlParameter("to_date"));
        this.showReport();
        this.showReportTitle();

        const workerTh = document.getElementById("worker-th");
        workerTh.click();
    }

    exportToExcel() {
        const reportTable = document.getElementById("report-table");

        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        const fileName = "workersPerfomanceReport_" + fromDate + "_" + toDate;
        const sheetName = fromDate + " - " + toDate;

        viewUtils.tableToExcel_v2(reportTable, fileName, "xlsx", sheetName);
    }

    print() {
        window.print();
    }

    showHideDescription() {
        const showHideDescriptionButton = document.getElementById("show-hide-description-button");
        const descriptionDiv = document.getElementById("description-div");
        if (descriptionDiv.classList.contains("d-none")) {
            showHideDescriptionButton.textContent = "Порядок рассчёта и комментарии к отчёту (скрыть)";
            showHideDescriptionButton.title = "Скрыть порядок рассчёта и комментарии к отчёту";
            descriptionDiv.classList.remove("d-none");
        } else {
            showHideDescriptionButton.textContent = "Порядок рассчёта и комментарии к отчёту (показать)";
            showHideDescriptionButton.title = "Показать порядок рассчёта и комментарии к отчёту";
            descriptionDiv.classList.add("d-none");
        }
    }

    onChangeDate() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async show() {        
        const fromDatePicker = $("#from-date-input");
        const toDatePicker = $("#to-date-input");
        fromDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        toDatePicker.datepicker({
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
        fromDatePicker.datepicker("setDate", fromDate);
        fromDatePicker.on("changeDate", this.onChangeDate.bind(this));
        if (toDate == "") {
            toDate = fromDate;
        }
        toDatePicker.datepicker("setDate", toDate);
        toDatePicker.on("changeDate", this.onChangeDate.bind(this));
        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        viewUtils.sortTable();
        await this.createReport();                
    }
}

const workersPerfomanceReportPage = new WorkersPerfomanceReportPage();

document.addEventListener('DOMContentLoaded', workersPerfomanceReportPage.show.bind(workersPerfomanceReportPage));