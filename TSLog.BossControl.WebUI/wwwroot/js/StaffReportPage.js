/*
    Страница отчёта по персоналу.
    Автор - Буров Михаил.
    Создан - 2022.07.04.
*/

// Страница отчёта по персоналу
class StaffReportPage {
    shortUrl;

    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");

        const onlyLinearInput = document.getElementById("only-linear-input");
        const onlyLinear = onlyLinearInput.checked;
        result = utils.addParameterValue(result, "only_linear", onlyLinear ? "1" : "0");

        return result != "" ? "?" + result : "";
    }

    async getData(fromDate, toDate, onlyLinear) {
        const result = await api.createStaffReport(fromDate, toDate, onlyLinear);
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

    showPosition(position, elementsTbody) {
        const positionTr = document.createElement("tr");
        positionTr.classList.add("table-row");
        positionTr.classList.add("worker-table-row");

        const planTime = utils.displayFloat(position.planTime / 60 / 60, 2);
        const planCost = utils.displayFloat(position.planCost);
        const factTime = utils.displayFloat(position.factTime / 60 / 60, 2);
        const factCost = utils.displayFloat(position.factCost);
        const downTime = utils.displayFloat(position.downTime / 60 / 60, 2);
        const downCost = utils.displayFloat(position.downCost, 0);
        const downTimePercent = utils.displayFloat(position.downTime / position.planTime * 100);
        const downCostPercent = utils.displayFloat(position.downCost / position.planCost * 100);

        viewUtils.createCell(positionTr, ["table-cell"], [position.name], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [planTime], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [planCost], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [factTime], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [factCost], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [downTime], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [downCost], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [downTimePercent], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [downCostPercent], "font-weight: bold");

        elementsTbody.append(positionTr);
    }

    showWorker(worker, elementsTbody) {
        const workerTr = document.createElement("tr");
        workerTr.classList.add("table-row");
        workerTr.classList.add("worker-table-row");

        const planTime = utils.displayFloat(worker.planTime / 60 / 60, 2);
        const planCost = utils.displayFloat(worker.planCost);
        const factTime = utils.displayFloat(worker.factTime / 60 / 60, 2);
        const factCost = utils.displayFloat(worker.factCost);
        const downTime = utils.displayFloat(worker.downTime / 60 / 60, 2);
        const downCost = utils.displayFloat(worker.downCost, 0);
        const downTimePercent = utils.displayFloat(worker.downTime / worker.planTime * 100);
        const downCostPercent = utils.displayFloat(worker.downCost / worker.planCost * 100);

        viewUtils.createCell(workerTr, ["table-cell"], [worker.fullName]);
        viewUtils.createCell(workerTr, ["table-cell"], [planTime]);
        viewUtils.createCell(workerTr, ["table-cell"], [planCost]);
        viewUtils.createCell(workerTr, ["table-cell"], [factTime]);
        viewUtils.createCell(workerTr, ["table-cell"], [factCost]);
        viewUtils.createCell(workerTr, ["table-cell"], [downTime]);
        viewUtils.createCell(workerTr, ["table-cell"], [downCost]);
        viewUtils.createCell(workerTr, ["table-cell"], [downTimePercent]);
        viewUtils.createCell(workerTr, ["table-cell"], [downCostPercent]);

        elementsTbody.append(workerTr);
    }

    showReport() {
        let planTime = 0.0;
        let planCost = 0.0;
        let factTime = 0.0;
        let factCost = 0.0;
        let downTime = 0.0;
        let downCost = 0.0;

        const elementsTbody = document.getElementById("report-tbody");
        for (let position of this.data) {
            this.showPosition(position, elementsTbody);
            for (let worker of position.employees) {
                this.showWorker(worker, elementsTbody);

                planTime += worker.planTime;
                planCost += worker.planCost;
                factTime += worker.factTime;
                factCost += worker.factCost;
                downTime += worker.downTime;
                downCost += worker.downCost;
            }            
        }
        const downTimePercent = downTime / planTime * 100;
        const downCostPercent = downCost / planCost * 100;

        const totalTr = document.createElement("tr");
        totalTr.classList.add("table-row");
        totalTr.classList.add("worker-table-row");

        viewUtils.createCell(totalTr, ["table-cell"], ["Итого:"], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(planTime / 60 / 60, 2)], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(planCost)], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(factTime / 60 / 60, 2)], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(factCost)], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(downTime / 60 / 60, 2)], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(downCost)], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(downTimePercent)], "font-weight: bold");
        viewUtils.createCell(totalTr, ["table-cell"], [utils.displayFloat(downCostPercent)], "font-weight: bold");

        elementsTbody.append(totalTr);
    }

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);        
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"),
            utils.getUrlParameter("to_date"),
            utils.getUrlParameter("only_linear") === "1");
        this.showReport()
    }

    exportToExcel() {
        const reportTable = document.getElementById("report-table");

        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        const fileName = "staffReport_" + fromDate + "_" + toDate;
        const sheetName = fromDate + " - " + toDate;

        viewUtils.tableToExcel_v2(reportTable, fileName, "xlsx", sheetName);
    }

    async show() {        
        let fromDate = utils.getUrlParameter("from-from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        const onlyLinear = utils.getUrlParameter("only_linear") != "0";
        
        await this.getData(fromDate, toDate, onlyLinear);        

        viewUtils.initDatePicker("from-date-input", fromDate, this.onChangeParameters.bind(this));        
        viewUtils.initDatePicker("to-date-input", fromDate, this.onChangeParameters.bind(this));        
        viewUtils.initCheckbox("only-linear-input", onlyLinear, this.onChangeParameters.bind(this));        

        this.showReport();        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const staffReportPage = new StaffReportPage();

document.addEventListener('DOMContentLoaded', staffReportPage.show.bind(staffReportPage));