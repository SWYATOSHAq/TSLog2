class ShiftLaborReportPage {
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
        this.data = await api.createShiftLaborReport(date, shift);
    }

    clearReport() {
        const elementsTbody = document.getElementById("elements-tbody");
        elementsTbody.replaceChildren();
    }

    showElement(element, elementTr) {
        elementTr.classList.add("table-row");

        viewUtils.createCell(elementTr, ["table-cell"], [element.positionName]);

        viewUtils.createCell(elementTr, ["table-cell", "table-cell-w5p"], [parseFloat(element.bcLaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell", "table-cell-w5p"], [parseFloat(element.bcStandardLaborCost).toFixed(2)]);

        const e = (parseFloat(element.bcEfficiency) * 100.0).toFixed(2);
        viewUtils.createCell(elementTr, ["table-cell", "table-cell-w5p"], [e.toString() + " %"]);

        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardLaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG01LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG02LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG03LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG04LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG05LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG06LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG07LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG08LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG09LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG10LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG11LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG12LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG13LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG14LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG15LaborCost).toFixed(2)]);
        viewUtils.createCell(elementTr, ["table-cell"], [parseFloat(element.yardG16LaborCost).toFixed(2)]);
    }

    showReport() {
        const elementsTbody = document.getElementById("elements-tbody");
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

const shiftLaborReportPage = new ShiftLaborReportPage();

document.addEventListener('DOMContentLoaded', shiftLaborReportPage.show.bind(shiftLaborReportPage));