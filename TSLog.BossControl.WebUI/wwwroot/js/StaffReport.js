/*
    Страница отчёта по персоналу.
    Автор - Буров Михаил.
    Создан - 2022.07.04.
*/

class StaffReport {
    shortUrl;

    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");
        return result != "" ? "?" + result : "";
    }

    async getData(fromDate, toDate) {
        const result = await api.createStaffReport(fromDate, toDate);
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

        const planTime = utils.displayFloat(position.planWorkTime / 60 / 60, 2);
        const planCost = utils.displayFloat(position.planCost);
        const factTime = utils.displayFloat(position.factWorkTime / 60 / 60, 2);
        const factCost = utils.displayFloat(position.factCost);
        const lostTime = utils.displayFloat(position.planWorkTime / 60 / 60 - position.factWorkTime / 60 / 60, 2);
        const lostCost = utils.displayFloat(position.planCost - position.factCost, 0);

        viewUtils.createCell(positionTr, ["table-cell"], [position.name], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [planTime], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [planCost], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [factTime], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [factCost], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [lostTime], "font-weight: bold");
        viewUtils.createCell(positionTr, ["table-cell"], [lostCost], "font-weight: bold");

        elementsTbody.append(positionTr);
    }

    showWorker(worker, elementsTbody) {
        const workerTr = document.createElement("tr");
        workerTr.classList.add("table-row");
        workerTr.classList.add("worker-table-row");

        const planTime = utils.displayFloat(worker.planWorkTime / 60 / 60, 2);
        const planCost = utils.displayFloat(worker.planCost);
        const factTime = utils.displayFloat(worker.factWorkTime / 60 / 60, 2);
        const factCost = utils.displayFloat(worker.factCost);
        const lostTime = utils.displayFloat(worker.planWorkTime / 60 / 60 - worker.factWorkTime / 60 / 60, 2);
        const lostCost = utils.displayFloat(worker.planCost - worker.factCost, 0);

        viewUtils.createCell(workerTr, ["table-cell"], [worker.fullName]);
        viewUtils.createCell(workerTr, ["table-cell"], [planTime]);
        viewUtils.createCell(workerTr, ["table-cell"], [planCost]);
        viewUtils.createCell(workerTr, ["table-cell"], [factTime]);
        viewUtils.createCell(workerTr, ["table-cell"], [factCost]);
        viewUtils.createCell(workerTr, ["table-cell"], [lostTime]);
        viewUtils.createCell(workerTr, ["table-cell"], [lostCost]);

        elementsTbody.append(workerTr);
    }

    showReport() {
        const elementsTbody = document.getElementById("report-tbody");
        for (let position of this.data) {
            this.showPosition(position, elementsTbody);
            for (let worker of position.employees) {
                this.showWorker(worker, elementsTbody);
            }            
        }    
    }

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);        
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"), utils.getUrlParameter("to_date"));
        this.showReport()
    }

    async show() {        
        let fromDate = utils.getUrlParameter("from-from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
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

const staffReport = new StaffReport();

document.addEventListener('DOMContentLoaded', staffReport.show.bind(staffReport));