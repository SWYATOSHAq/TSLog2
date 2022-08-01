/*
    Страница отчёта диспетчера (за период).
    Автор - Буров Михаил.
    Создан - 2022.07.14.
*/

// Страница отчёта по дням, по сменам (за период).
class DailyReportPage {
    shortUrl;
    data = null;
    marks = null;
    markColumnWidth = 420;
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

        const onlyLinearInput = document.getElementById("only-linear-input");
        const onlyLinear = onlyLinearInput.checked;
        result = utils.addParameterValue(result, "only_linear", onlyLinear ? "1" : "0");

        let shifts = "";
        const dayShiftsInput = document.getElementById("day-shifts-input");
        if (dayShiftsInput.checked) {
            shifts += "D";
        }
        const nightShiftsInput = document.getElementById("night-shifts-input");
        if (nightShiftsInput.checked) {
            shifts += "N";
        }
        result = utils.addParameterValue(result, "shifts", shifts);

        return result != "" ? "?" + result : "";
    }

    async getData() {
        const fromDate = utils.getUrlParameter("from_date");
        const toDate = utils.getUrlParameter("to_date");
        const onlyLinear = utils.getUrlParameter("only_linear") != "0";
        const shifts = utils.getUrlParameter("shifts");
        const result = await api.createDailyReport(fromDate, toDate, onlyLinear, shifts);        
        if (result.code === 0) {
            this.data = result.data;
        } else {
            this.data = null;
            this.marks = null;
            alert(result.message);
        }
    }

    clearData() {
        const headerCells = document.getElementsByClassName("header-cell");
        while (headerCells.length > 0) {
            headerCells[0].remove();
        }

        const reportTbody = document.getElementById("report-tbody");
        reportTbody.replaceChildren();

        /*
        const contentCells = document.getElementsByClassName("content-cell");
        while (contentCells.length > 0) {
            contentCells[0].remove();
        }
        */
        this.data = [];
        this.marks = {};
    }

    async showData() {
        if (this.data === null) {
            return;
        }

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
            const dataType = this.marks[markKey].dataType.toUpperCase();
            for (let value of element.values) {
                const tdId = markKey + "_" + value.shiftKey;
                const td = document.getElementById(tdId);                
                if (markKey.includes("TOTAL") || value.shiftKey.includes("TOTAL")) {
                    td.style.cssText = "font-weight: bold";
                }

                if (markKey.includes("PLAN_TIME") || markKey.includes("FACT_TIME") || markKey.includes("AUXILIARY_TIME") || markKey.includes("DOWN_TIME")) {
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
        table.style.cssText = "margin: auto; margin-top: 16px; width: " + tableWidth + "px";
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

    async show() {
        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        viewUtils.initDatePicker("from-date-input", fromDate, this.onChangeParameters.bind(this));
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        viewUtils.initDatePicker("to-date-input", toDate, this.onChangeParameters.bind(this));        

        const onlyLinear = utils.getUrlParameter("only_linear") != "0";
        viewUtils.initCheckbox("only-linear-input", onlyLinear, this.onChangeParameters.bind(this));

        let shifts = utils.getUrlParameter("shifts");
        shifts = shifts.trim();
        if (shifts === "") {
            shifts = "D";
        } else {
            shifts = shifts.toUpperCase();
        }        
        const dayShifts = shifts.includes("D");
        viewUtils.initCheckbox("day-shifts-input", dayShifts, this.onChangeParameters.bind(this));
        const nightShifts = shifts.includes("N");
        viewUtils.initCheckbox("night-shifts-input", nightShifts, this.onChangeParameters.bind(this));

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        await this.createReport();
    }
}

const dailyReportPage = new DailyReportPage();

document.addEventListener('DOMContentLoaded', dailyReportPage.show.bind(dailyReportPage));