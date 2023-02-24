/*
    Страница оперативного отчёта (за смену).
    Автор - Буров Михаил (m.burov@tslog.ru).
    Создан - 2022.07.14.
*/

// Страница оперативного отчёта (за смену) - отчёта по состояниям, корректности, объёмам товара операций двора за смену.
class OperationalReportPage {
    shortUrl;
    data = [];

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
        const reportTFoot = document.getElementById("report-tfoot");
        reportTFoot.replaceChildren();
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
        if (durationExcess || laborCostExcess) {
            style = "background-color: peachpuff";
        } else if (isLate) {
            style = "background-color: lemonchiffon";
        }
        tr.style.cssText = style;

        const openButton = document.createElement("button");
        openButton.classList.add("btn");
        openButton.classList.add("btn-outline-info");
        openButton.classList.add("m-3");
        openButton.classList.add("table-cell-button");
        openButton.onclick = this.createOpenOperationFunction(element.id);
        openButton.textContent = "Открыть";
        openButton.title = "Открыть карточку операции двора";
        let content = [openButton, element.id];

        viewUtils.createCell(tr, ["table-cell"], content);
        viewUtils.createCell(tr, ["table-cell"], [element.partnersNames]);
        viewUtils.createCell(tr, ["table-cell"], [element.typeShortName]);
        viewUtils.createCell(tr, ["table-cell"], [element.vehicleKindShortName]);
        viewUtils.createCell(tr, ["table-cell"], [element.stateShortName]);

        content = "Ожидается"
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
        content = utils.displayFloat(element.planGrossWeight / 1000 / 1000);
        if (element.stateCode === "FINISHED") {
            content += " (" + utils.displayFloat(element.factGrossWeight / 1000 / 1000) + ")";
        }
        viewUtils.createCell(tr, ["table-cell"], [content]);

        content = utils.displayFloat(element.planVolume / 1000 / 1000 / 1000, 2);
        if (element.stateCode === "FINISHED") {
            content += " (" + utils.displayFloat(element.factVolume / 1000 / 1000 / 1000, 2) + ")";
        }
        viewUtils.createCell(tr, ["table-cell"], [content]);

        /*
        content = utils.displayFloat(element.planPalletsCount);
        if (element.stateCode === "FINISHED") {
            content += " (" + utils.displayFloat(element.factPalletsCount) + ")"
        }
        viewUtils.createCell(tr, ["table-cell"], [content]);

        content = utils.displayFloat(element.planBoxesCount);
        if (element.stateCode === "FINISHED") {
            content += " (" + utils.displayFloat(element.factBoxesCount) + ")"
        }
        viewUtils.createCell(tr, ["table-cell"], [content]);
        */
        content = utils.displayFloat(element.tusCount);
        viewUtils.createCell(tr, ["table-cell"], [content]);

        if (durationExcess || laborCostExcess) {
            viewUtils.createCell(tr, ["table-cell"], ["ПП"]);
        } else if (isLate) {
            viewUtils.createCell(tr, ["table-cell"], ["О"]);
        } else {
            viewUtils.createCell(tr, ["table-cell"], [""]);
        }        
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
        let totalPlanGrossWeight = 0.0;
        let totalFactGrossWeight = 0.0;
        let totalPlanVolume = 0.0;
        let totalFactVolume = 0.0;
        let totalPlanPalletsCount = 0.0;
        let totalFactPalletsCount = 0.0;
        let totalPlanBoxesCount = 0.0;
        let totalFactBoxesCount = 0.0;
        let totalTusCount = 0;

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

            totalPlanGrossWeight += element.planGrossWeight;            
            totalPlanVolume += element.planVolume;
            totalPlanPalletsCount += element.planPalletsCount;
            totalPlanBoxesCount += element.planBoxesCount;
            if (element.stateCode === "FINISHED") {
                totalFactGrossWeight += element.factGrossWeight;
                totalFactVolume += element.factVolume;
                totalFactPalletsCount += element.factPalletsCount;
                totalFactBoxesCount += element.factBoxesCount;
            }
            totalTusCount += element.tusCount;
        }        

        const reportTFoot = document.getElementById("report-tfoot");
        const summaryTr = document.createElement("tr");
        let contents = "";
        viewUtils.createBoldCell(summaryTr, ["table-cell"], ["Всего:"]);
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
        viewUtils.createBoldCell(summaryTr, ["table-cell"],
            [utils.displayFloat(totalPlanGrossWeight / 1000 / 1000) + " (" + utils.displayFloat(totalFactGrossWeight / 1000 / 1000) + ")"]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"],
            [utils.displayFloat(totalPlanVolume / 1000 / 1000 / 1000, 2) + " (" + utils.displayFloat(totalFactVolume / 1000 / 1000 / 1000, 2) + ")"]);

        /*
        viewUtils.createBoldCell(summaryTr, ["table-cell"],
            [utils.displayFloat(totalPlanPalletsCount) + " (" + utils.displayFloat(totalFactPalletsCount) + ")"]);
        viewUtils.createBoldCell(summaryTr, ["table-cell"],
            [utils.displayFloat(totalPlanBoxesCount) + " (" + utils.displayFloat(totalFactBoxesCount) + ")"]);
        */
        viewUtils.createBoldCell(summaryTr, ["table-cell"], [utils.displayFloat(totalTusCount)]);

        viewUtils.createBoldCell(summaryTr, ["table-cell"], [""]);

        reportTFoot.append(summaryTr);
    }

    showReportTitle() {
        const reportHeaderB = document.getElementById("report-header-b");
        const date = utils.getUrlParameter("date");
        const shiftCode = utils.getUrlParameter("shift");
        const shift = shiftCode === "day" ? "ДНЕВНУЮ" : "НОЧНУЮ";
        reportHeaderB.textContent = "Оперативный отчёт за " + date + " за " + shift + " смену (всего заявок - " + this.data.length + ")";
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
        this.clearOperations();
        await this.getData();
        await this.showOperations();
        this.showReportTitle();

        const operationIdTh = document.getElementById("operation-id-th");
        operationIdTh.classList.remove("th-sort-asc", "th-sort-desc")
        operationIdTh.click();
    }

    exportToExcel() {
        const reportTable = document.getElementById("report-table");

        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        const fileName = "operationalReport_" + fromDate + "_" + toDate;
        const sheetName = fromDate + " - " + toDate;

        const openButtons = document.getElementsByClassName("table-cell-button");
        function setButtonsText(buttons, text = "") {
            for (let button of buttons) {
                button.textContent = text;
            }
        }

        setButtonsText(openButtons);        
        viewUtils.tableToExcel_v2(reportTable, fileName, "xlsx", sheetName);
        setButtonsText(openButtons, "Открыть");
    }

    print() {
        window.print();
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