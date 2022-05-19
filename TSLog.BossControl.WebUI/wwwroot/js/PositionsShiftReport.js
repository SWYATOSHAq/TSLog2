class PositionsShiftReport {
    shortUrl;

    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        result = utils.addParameter(result, "date", "date-input");
        result = utils.addParameter(result, "shift", "shift-select");
        return result != "" ? "?" + result : "";
    }

    async getData(date, shift) {
        this.data = await api.createPositionsShiftReport(date, shift);
    }

    clearReport() {
        const elementsTbody = document.getElementById("report-tbody");
        elementsTbody.replaceChildren();
    }

    hideWorkersRows() {
        var workersRows = document.getElementsByClassName("worker-table-row");
        for (let workerRow of workersRows) {
            workerRow.classList.add("d-none");
        }
    }

    hidePositionsRows() {
        var positionsRows = document.getElementsByClassName("position-table-row");
        for (let positionRow of positionsRows) {
            positionRow.dataset.state = "C";
        }
    }

    onPositionRowClick(trId) {
        const tr = document.getElementById("position-tr-" + trId);
        const positionId = tr.dataset.positionId;
        const state = tr.dataset.state;
        this.hideWorkersRows();        
        this.hidePositionsRows();
        if (state === "C") {
            const elementsTbody = document.getElementsByClassName("table-row");
            for (var workerTr of elementsTbody) {
                if (workerTr.dataset.positionId == positionId) {
                    workerTr.classList.remove("d-none");
                }
            }
            tr.dataset.state = "E";
        }
    }

    showElement(element, elementsTbody) {
        const positionTr = document.createElement("tr");
        positionTr.classList.add("table-row");
        positionTr.classList.add("position-table-row");

        positionTr.id = "position-tr-" + element.positionId;
        positionTr.dataset.positionId = element.positionId;
        positionTr.dataset.positionName = element.positionName;
        positionTr.dataset.state = "C";
        viewUtils.createCell(positionTr, ["table-cell"], [element.positionName], "text-align: left; font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [element.factLaborCost], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [element.normLaborCost], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [element.efficiency], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [element.losses], "font-weight: bold");        
        positionTr.onclick = () => {
            this.onPositionRowClick(element.positionId);
        }    
        elementsTbody.append(positionTr);        

        for (let worker of element.workers) {
            let workerTr = document.createElement("tr");
            workerTr.classList.add("table-row");
            workerTr.classList.add("worker-table-row");

            workerTr.dataset.positionId = worker.positionId;
            const restWork = worker.restWork === 1;
            viewUtils.createCell(workerTr, ["table-cell"], [worker.workerFullName],
                !restWork ? "text-align: left; padding-left: 4% !important" : "text-align: left; padding-left: 4% !important; color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.factLaborCost], !restWork ? "" : "color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.normLaborCost], !restWork ? "" : "color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.efficiency], !restWork ? "" : "color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.losses], !restWork ? "" : "color: red");

            elementsTbody.append(workerTr);
        }
    }

    showReport() {
        const elementsTbody = document.getElementById("report-tbody");
        for (let element of this.data) {
            this.showElement(element, elementsTbody);
        }    
        this.hidePositionsRows();
        this.hideWorkersRows();
    }

    showShiftInfo() {
        const shiftDiv = document.getElementById("shift-info");
        if (document.getElementById("shift-select").value == "day") {
            const date = utils.getUrlParameter("date");
            shiftDiv.textContent = "Дневная смена с " + date + " 9:00 по " + date + " 21:00";
        }
        else if (document.getElementById("shift-select").value == "night") {
            const fromDate = utils.stringToDate(utils.getUrlParameter("date"));
            let toDate = utils.stringToDate(utils.getUrlParameter("date"));
            toDate.setDate(toDate.getDate() + 1);
            shiftDiv.textContent = "Ночная смена с " + fromDate.toLocaleDateString() + " 21:00 по " + toDate.toLocaleDateString() + " 9:00";
        }
        else {
            shiftDiv.textContent = "Смена не определена!";
        }
    }

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        this.showShiftInfo();
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("date"), utils.getUrlParameter("shift"));
        this.showReport()
    }

    async show() {        
        let date = utils.getUrlParameter("date");
        if (date == "")
            date = utils.getNowDate();
        let shift = utils.getUrlParameter("shift");
        if (shift == "")
            shift = "day";
        await this.getData(date, shift);        
        viewUtils.initDatePicker("date-input", date, this.onChangeParameters.bind(this));
        viewUtils.initSelect("shift-select", shift, this.onChangeParameters.bind(this));        

        this.showReport();        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        this.showShiftInfo();
    }
}

const positionsShiftReport = new PositionsShiftReport();

document.addEventListener('DOMContentLoaded', positionsShiftReport.show.bind(positionsShiftReport));