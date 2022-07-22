/*
    Страница отчёта о трудозатратах за смену.
    Автор - Буров Михаил.
*/

// Страница отчёта о трудозатратах за смену
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
        const result = await api.createPositionsShiftReport(date, shift);
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

    collapsePositionsRows() {
        var positionsRows = document.getElementsByClassName("position-table-row");
        for (let positionRow of positionsRows) {
            positionRow.dataset.state = "C";
        }
    }

    collapseWorkersRows() {
        var workersRows = document.getElementsByClassName("worker-table-row");
        for (let workerRow of workersRows) {
            workerRow.dataset.state = "C";
        }
    }

    hideWorkersRows() {
        var workersRows = document.getElementsByClassName("worker-table-row");
        for (let workerRow of workersRows) {
            workerRow.classList.add("d-none");
        }
    }    

    hideWorksRows() {
        var worksRows = document.getElementsByClassName("work-table-row");
        for (let workRow of worksRows) {
            workRow.classList.add("d-none");
        }
    }

    onPositionRowClick(positionId) {
        const tr = document.getElementById("position-tr-" + positionId);
        const state = tr.dataset.state;
        this.hideWorksRows();
        this.collapseWorkersRows();
        this.hideWorkersRows();        
        this.collapsePositionsRows();
        if (state === "C") {
            const elementsTbody = document.getElementsByClassName("worker-table-row");
            for (var workerTr of elementsTbody) {
                if (workerTr.dataset.positionId == positionId) {
                    workerTr.classList.remove("d-none");
                }
            }
            tr.dataset.state = "E";
        }
    }

    onWorkerRowClick(workerId) {
        const tr = document.getElementById("worker-tr-" + workerId);
        const state = tr.dataset.state;
        this.hideWorksRows();
        this.collapseWorkersRows();
        if (state === "C") {
            const elementsTbody = document.getElementsByClassName("work-table-row");
            for (var workTr of elementsTbody) {
                if (workTr.dataset.workerId == workerId) {
                    workTr.classList.remove("d-none");
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
            workerTr.id = "worker-tr-" + worker.workerId;
            workerTr.dataset.workerId = worker.workerId;
            workerTr.dataset.workerFullName = worker.workerFullName;
            workerTr.dataset.state = "C";

            const restWork = worker.restWork === 1;
            viewUtils.createCell(workerTr, ["table-cell"], [worker.workerFullName],
                !restWork ? "text-align: left; padding-left: 2% !important" : "text-align: left; padding-left: 2% !important; color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.factLaborCost], !restWork ? "" : "color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.normLaborCost], !restWork ? "" : "color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.efficiency], !restWork ? "" : "color: red");
            viewUtils.createCell(workerTr, ["table-cell"], [worker.losses], !restWork ? "" : "color: red");

            workerTr.onclick = () => {
                this.onWorkerRowClick(worker.workerId);
            }  

            elementsTbody.append(workerTr);

            for (let work of worker.works) {
                let workTr = document.createElement("tr");
                workTr.classList.add("table-row");
                workTr.classList.add("work-table-row");

                workTr.dataset.workerId = worker.workerId;

                //work.startDate = utils.convertDate(work.startDate) + " " + utils.convertTime(work.startDate);
                //work.finishDate = utils.convertDate(work.finishDate) + " " + utils.convertTime(work.finishDate);
                work.startDate = utils.convertTimeHHMM(work.startDate);
                work.finishDate = utils.convertTimeHHMM(work.finishDate);

                const restWork = work.restWork === 1;
                const siteB = document.createElement("b");
                siteB.textContent = work.siteName;
                viewUtils.createCell(workTr, ["table-cell"],
                    [work.startDate + " - " + work.finishDate + " (" + work.duration + " мин.) ", siteB, " (" + work.partnerName + ")"],
                    !restWork ? "text-align: left; padding-left: 4% !important" : "text-align: left; padding-left: 4% !important; color: red");

                viewUtils.createCell(workTr, ["table-cell"], "");
                viewUtils.createCell(workTr, ["table-cell"], "");
                viewUtils.createCell(workTr, ["table-cell"], "");
                viewUtils.createCell(workTr, ["table-cell"], "");

                elementsTbody.append(workTr);
            }            
        }
    }

    showReport() {
        const elementsTbody = document.getElementById("report-tbody");
        for (let element of this.data) {
            this.showElement(element, elementsTbody);
        }    
        this.collapsePositionsRows();
        this.hideWorkersRows();
        this.hideWorksRows();
    }

    showShiftInfo() {
        const shiftDiv = document.getElementById("shift-info");
        if (document.getElementById("shift-select").value == "day") {
            const date = utils.getUrlParameter("date");
            shiftDiv.innerHTML = "Дневная смена с <b>9:00</b> (" + date + ") по <b>21:00</b> (" + date + ")";
        }
        else if (document.getElementById("shift-select").value == "night") {
            const fromDate = utils.stringToDate(utils.getUrlParameter("date"));
            let toDate = utils.stringToDate(utils.getUrlParameter("date"));
            toDate.setDate(toDate.getDate() + 1);
            shiftDiv.innerHTML = "Ночная смена с <b>21:00</b> (" + fromDate.toLocaleDateString() + ") по <b>9:00</b> (" + toDate.toLocaleDateString() + ")";
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

    printPage () {
        const printButton = document.querySelector(".print-button");
        printButton.onclick = () => {
            window.print();
        }
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
        this.printPage();
    }
}

const positionsShiftReport = new PositionsShiftReport();

document.addEventListener('DOMContentLoaded', positionsShiftReport.show.bind(positionsShiftReport));