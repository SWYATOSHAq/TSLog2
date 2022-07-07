/*
    Страница отчёта по производительности по работникам (за период).
    Автор - Буров Михаил.
*/

// Страница отчёта по производительности по работникам (за период)
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
            viewUtils.showError(result.message);
        }
    }

    clearReport() {
        const elementsTbody = document.getElementById("report-tbody");
        elementsTbody.replaceChildren();
    }

    showReport() {
        const h3 = document.getElementById("report-header")
        h3.textContent = "Отчёт по производительности по работникам" +
            " с " + utils.getUrlParameter("from_date") +
            " по " + utils.getUrlParameter("to_date") + 
            " (включая!)";

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

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        this.showShiftInfo();
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"), utils.getUrlParameter("to_date"));
        this.showReport()
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
        await this.createReport();

        viewUtils.sortTable();
        const workerTh = document.getElementById("worker-th");
        workerTh.click();
    }
}

const workersPerfomanceReportPage = new WorkersPerfomanceReportPage();

document.addEventListener('DOMContentLoaded', workersPerfomanceReportPage.show.bind(workersPerfomanceReportPage));