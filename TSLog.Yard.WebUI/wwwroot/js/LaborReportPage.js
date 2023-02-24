/*
    Страница отчёта по трудозатратам
    Автор - Буров Михаил (m.burov@tslog.ru).
*/
class LaborReportPage {
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

        const result = await api.createLaborReport(fromDate, toDate);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            viewUtils.showError(result.message);
        }        
    }

    clearReport() {
        const elementsTbody = document.getElementById("elements-tbody");
        elementsTbody.replaceChildren();
    }

    showElement(element, elementTr) {
        elementTr.classList.add("table-row");

        viewUtils.createCell(elementTr, ["table-cell"], [element.partnerName]);
        viewUtils.createCell(elementTr, ["table-cell"], [element.positionName]);

        const bcLaborCost = parseFloat(element.bcLaborCost).toFixed(2);
        const yaPlanLaborCost = parseFloat(element.yaPlanLaborCost).toFixed(2);
        const yaFactLaborCost = parseFloat(element.yaFactLaborCost).toFixed(2);

        const correctStyles = Number(element.bcWorkersCount) == Number(element.yaWorkersCount)
            ? ["table-cell", "correct-values"]
            : ["table-cell"];
        const incorrectStyles = bcLaborCost == 0.0 || yaPlanLaborCost == 0.0 || Math.abs(bcLaborCost - yaPlanLaborCost) / Math.max(bcLaborCost, yaPlanLaborCost) > 0.1
            ? ["table-cell", "incorrect-values"]
            : ["table-cell"];

        viewUtils.createCell(elementTr, correctStyles, [element.bcWorkersCount]);
        viewUtils.createCell(elementTr, incorrectStyles, [bcLaborCost]);
        viewUtils.createCell(elementTr, correctStyles, [element.yaWorkersCount]);
        viewUtils.createCell(elementTr, incorrectStyles, [yaPlanLaborCost]);
        viewUtils.createCell(elementTr, ["table-cell"], [yaFactLaborCost]);
    }

    showReport() {
        const elementsTbody = document.getElementById("elements-tbody");
        for (let element of this.data) {
            let elementTr = document.createElement("tr");
            this.showElement(element, elementTr);
            elementsTbody.append(elementTr);
        }                
    }

    onDateChange() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"), utils.getUrlParameter("to_date"));
        this.showReport()
    }

    async show() {        
        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = fromDate;
        await this.getData(fromDate, toDate);
        
        viewUtils.initDatePicker("from-date-input", fromDate, this.onDateChange.bind(this));
        viewUtils.initDatePicker("to-date-input", toDate, this.onDateChange.bind(this));

        this.showReport();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const laborReportPage = new LaborReportPage();

document.addEventListener('DOMContentLoaded', laborReportPage.show.bind(laborReportPage));