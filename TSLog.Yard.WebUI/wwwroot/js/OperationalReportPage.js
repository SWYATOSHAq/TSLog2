/*
    Страница оперативного отчёта (за смену).
    Автор - Буров Михаил.
    Создан - 2022.07.14.
*/

// Страница оперативного отчёта (за смену)
class OperationalReportPage {
    shortUrl;
    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createOpenOperationFunction(id) {
        return function () {
            sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
            location.href = "/OperationPage?id=" + id + "&action=" + (!isPartnerUser ? "VIEW" : "VIEW_BY_PARTNER");
        }
    }

    createParameters() {
        let result = "";

        result = utils.addParameter(result, "date", "date-input");
        result = utils.addParameter(result, "shift", "shift-select");

        return result != "" ? "?" + result : "";
    }

    async getData() {
        const date = utils.getUrlParameter("date");
        const shift = utils.getUrlParameter("shift");
        const result = await api.createOperationalReport(date, shift);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            alert(result.message);
        }
    }

    clearOperations() {
        const reportTBody = document.getElementById("report-tbody");
        reportTBody.replaceChildren();
        this.data = [];
    }

    showOperation(tr, element) {
        element.startDate = utils.stringToDate(element.startDate);
        element.planWorkStartDate = utils.stringToDate(element.planWorkStartDate);
        element.factWorkStartDate = utils.stringToDate(element.factWorkStartDate);
        element.planWorkFinishDate = utils.stringToDate(element.planWorkFinishDate);
        element.factWorkFinishDate = utils.stringToDate(element.factWorkFinishDate);
        element.finishDate = utils.stringToDate(element.finishDate);

        const isLate = element.factWorkFinishDate > utils.nullDate && element.planWorkFinishDate < element.factWorkFinishDate; 
        const durationExcess = element.factDuration > 0 && element.planDuration < element.factDuration;
        const laborCostExcess = element.factLaborCost > 0 && element.planLinearLaborCost < element.factLaborCost;
        const isBad = isLate || durationExcess || laborCostExcess;
        
        let style = "background-color: lightcyan";
        if (isBad) {
            style = "background-color: peachpuff";
        }
        tr.style.cssText = style;

        viewUtils.createCell(tr, ["table-cell"], [element.id]);
        viewUtils.createCell(tr, ["table-cell"], [element.partnersNames]);
        viewUtils.createCell(tr, ["table-cell"], [element.typeShortName]);
        viewUtils.createCell(tr, ["table-cell"], [element.vehicleKindShortName]);
        viewUtils.createCell(tr, ["table-cell"], [element.stateShortName]);

        let content = "Ожидается"
        if (element.startDate > utils.nullDate) {
            content = utils.timeToStringHHMM(element.startDate);
        }
        viewUtils.createCell(tr, ["table-cell"], [content]);

        content = utils.timeToStringHHMM(element.planWorkStartDate);
        if (element.factWorkStartDate > utils.nullDate) {
            content += " (" + utils.timeToStringHHMM(element.factWorkStartDate) + ")"
        }
        viewUtils.createCell(tr, ["table-cell"], [content]);

        content = utils.timeToStringHHMM(element.planWorkFinishDate);
        if (element.factWorkFinishDate > utils.nullDate) {
            content += " (" + utils.timeToStringHHMM(element.factWorkFinishDate) + ")"
        }
        if (isLate) {
            viewUtils.createBoldCell(tr, ["table-cell"], [content]);
        } else {
            viewUtils.createCell(tr, ["table-cell"], [content]);
        }

        viewUtils.createCell(tr, ["table-cell"],
            [element.finishDate > utils.nullDate ? utils.timeToStringHHMM(element.finishDate) : ""]);

        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.presenceTime / 60)]);

        content = utils.displayFloat(element.planDuration / 60);
        if (element.factDuration > 0) {
            content += " (" + utils.displayFloat(element.factDuration / 60) + ")"
        }
        if (durationExcess) {
            viewUtils.createBoldCell(tr, ["table-cell"], [content]);
        } else {
            viewUtils.createCell(tr, ["table-cell"], [content]);
        }        

        content = utils.displayFloat(element.planLinearLaborCost / 60);
        if (element.factLaborCost > 0) {
            content += " (" + utils.displayFloat(element.factLaborCost / 60) + ")"
        }
        if (laborCostExcess) {
            viewUtils.createBoldCell(tr, ["table-cell"], [content]);
        } else {
            viewUtils.createCell(tr, ["table-cell"], [content]);
        }        

        viewUtils.createCell(tr, ["table-cell"], [element.workersCount]);
        viewUtils.createCell(tr, ["table-cell"], [element.warehouseActionsCount]);

        //viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.netWeight / 1000 / 1000)]);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.grossWeight / 1000 / 1000)]);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.volume / 1000 / 1000 / 1000, 2)]);

        viewUtils.createCell(tr, ["table-cell"], [""]);
    }

    async showOperations() {
        const reportTBody = document.getElementById("report-tbody");
        let totalPresenceTime = 0.0;
        let totalPlanWorkTime = 0.0;
        let totalFactWorkTime = 0.0;
        let totalPlanLaborCost = 0.0;
        let totalFactLaborCost = 0.0;
        let totalWorkersCount = 0;
        let totalActionsCount = 0;
        let totalWeight = 0.0;
        let totalVolume = 0.0;
        for (let element of this.data) {
            const tr = document.createElement("tr");
            this.showOperation(tr, element);
            reportTBody.append(tr);

            totalPresenceTime += element.presenceTime;
            totalPlanWorkTime += element.planDuration;
            totalFactWorkTime += element.factDuration;
            totalPlanLaborCost += element.planLaborCost;
            totalFactLaborCost += element.factLaborCost;
            totalWorkersCount += element.workersCount;
            totalActionsCount += element.warehouseActionsCount;
            totalWeight += element.grossWeight;
            totalVolume += element.volume;
        }
        const elementsCountH5 = document.getElementById("elements-count-h5");
        elementsCountH5.textContent = "Всего заявок - " + this.data.length;

        const reportTFoot = document.getElementById("report-tfoot");
        const summaryTr = document.createElement("tr");
        let contents = "";
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [utils.displayFloat(totalPresenceTime / 60)]);
        contents = utils.displayFloat(totalPlanWorkTime / 60);
        if (totalFactWorkTime > 0.0) {
            contents += " (" + utils.displayFloat(totalFactWorkTime / 60) + ")";
        }
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [contents]);

        contents = utils.displayFloat(totalPlanLaborCost / 60);
        if (totalFactLaborCost > 0.0) {
            contents += " (" + utils.displayFloat(totalFactLaborCost / 60) + ")"
        }
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [contents]);

        viewUtils.createBoldCell(summaryTr, ["table-cell"], [totalWorkersCount]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [totalActionsCount]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [utils.displayFloat(totalWeight / 1000 / 1000)]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [utils.displayFloat(totalVolume / 1000 / 1000 / 1000, 2)]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);

        reportTFoot.append(summaryTr);
    }

    async onChangeFilter() {
        this.clearOperations();
        await this.getData();
        this.sortOperations(this.sortCompare.bind(this));
        await this.showOperations();
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
        this.clearOperations();
        await this.getData();
        await this.showOperations();

        const operationIdTh = document.getElementById("operation-id-th");
        operationIdTh.classList.remove("th-sort-asc", "th-sort-desc")
        operationIdTh.click();
    }

    async show() {
        let date = utils.getUrlParameter("date");
        if (date == "")
            date = utils.getNowDate();
        viewUtils.initDatePicker("date-input", date, this.onChangeParameters.bind(this));
        let shift = utils.getUrlParameter("shift");
        if (shift != "day" && shift != "night") {
            shift = "day";
        }
        viewUtils.initSelect("shift-select", shift, this.onChangeParameters.bind(this));        

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        this.showShiftInfo();
        viewUtils.sortTable();

        await this.createReport();
    }
}

const operationalReportPage = new OperationalReportPage();

document.addEventListener('DOMContentLoaded', operationalReportPage.show.bind(operationalReportPage));