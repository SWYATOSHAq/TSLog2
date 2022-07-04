/*
    Страница отчёта по персоналу.
    Автор - Буров Михаил.
*/

class StaffReport {
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
                    [work.startDate + " - " + work.finishDate + " (" + work.duration + " мин.) ", siteB],
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
        let fromDate = utils.getUrlParameter("from-date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to-date");
        if (toDate == "")
            toDate = utils.getNowDate();
        
        await this.getData(fromDate, toDate);        

        viewUtils.initDatePicker("from-date-input", fromDate, this.onChangeParameters.bind(this));        
        viewUtils.initDatePicker("to-date-input", fromDate, this.onChangeParameters.bind(this));        

        this.showReport();        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const positionsShiftReport = new PositionsShiftReport();

document.addEventListener('DOMContentLoaded', positionsShiftReport.show.bind(positionsShiftReport));