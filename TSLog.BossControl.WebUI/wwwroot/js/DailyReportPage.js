/*
    Страница отчёта диспетчера (за период).
    Автор - Буров Михаил.
    Создан - 2022.07.14.
*/

// Страница отчёта диспетчера (за период).
class DailyReportPage {
    shortUrl;
    data = {};
    marks = {};
    markColumnWidth = 320;
    shiftColumnWidth = 112;

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
        const result = await api.createDailyReport(fromDate, toDate);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            alert(result.message);
        }
    }

    clearData() {
        const headerCells = document.getElementsByClassName("header-cell");
        while (headerCells.length > 0) {
            headerCells[0].remove();
        }
        const contentCells = document.getElementsByClassName("content-cell");
        while (contentCells.length > 0) {
            contentCells[0].remove();
        }
        this.data = [];
        this.marks = {};
    }

    async showData() {
        const headerTr = document.getElementById("header-tr");
        const shifts = this.data.shifts;
        let tableWidth = this.markColumnWidth;
        for (let shift of shifts) {
            const shiftTh = document.createElement("th");
            shiftTh.textContent = shift.caption;
            shiftTh.classList.add("header-cell");
            shiftTh.style.cssText = "width: 112px; text-align: center; vertical-align: middle";
            headerTr.append(shiftTh);
            tableWidth += this.shiftColumnWidth;
        }

        const reportTbody = document.getElementById("report-tbody");
        const marks = this.data.marks;
        for (let mark of marks) {
            this.marks[mark.key] = mark;
            const markTr = document.createElement("tr");

            const markTd = document.createElement("td");
            markTd.classList.add("header-cell");
            if (mark.key.includes("TOTAL")) {
                markTd.style.cssText = "font-weight: bold";
            }
            markTd.textContent = mark.name;
            markTr.append(markTd);

            for (let shift of shifts) {
                const valueTd = document.createElement("td");
                valueTd.classList.add("content-cell");
                valueTd.id = mark.key + "_" + shift.key;
                valueTd.textContent = "0";
                markTr.append(valueTd);
            }

            reportTbody.append(markTr);
        } 

        for (let element of this.data.elements) {
            let markKey = element.markKey;
            const dataType = this.marks[markKey].dataType;
            for (let value of element.values) {
                const tdId = markKey + "_" + value.shiftKey;
                const td = document.getElementById(tdId);                
                if (markKey.includes("TOTAL") || value.shiftKey.includes("TOTAL")) {
                    td.style.cssText = "font-weight: bold";
                }

                if (markKey.includes("PLAN_TIME") || markKey.includes("FACT_TIME") || markKey.includes("AUXILIARY_TIME")) {
                    value.value = value.value / 60 / 60;
                }
                let text = "";
                if (dataType === "INTEGER") {
                    text = utils.displayFloat(value.value);
                } else if (dataType === "DOUBLE") {
                    text = utils.displayFloat(value.value, 2);
                }
                td.textContent = text;
            }
        }
        const table = document.getElementById("report-table");
        table.style.cssText = "margin: auto; width: " + tableWidth + "px";
    }

    async onChangeFilter() {
        this.clearData();
        await this.getData();
        await this.showData();
    }

    onChangeParameters() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async createReport() {
        this.clearData();
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