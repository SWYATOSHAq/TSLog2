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

    showElement(element, elementTr) {
        elementTr.classList.add("table-row");

        viewUtils.createCell(elementTr, ["table-cell"], [element.positionName]);
        viewUtils.createCell(elementTr, ["table-cell"], [element.factLaborCost]);
        viewUtils.createCell(elementTr, ["table-cell"], [element.normLaborCost]);
        viewUtils.createCell(elementTr, ["table-cell"], [element.efficiency]);
        viewUtils.createCell(elementTr, ["table-cell"], [element.losses]);        
    }

    showReport() {
        const elementsTbody = document.getElementById("report-tbody");
        for (let element of this.data) {
            let elementTr = document.createElement("tr");
            this.showElement(element, elementTr);
            elementsTbody.append(elementTr);
        }                
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