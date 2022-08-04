/*
    Страница отчёта диспетчера (за период).
    Автор - Буров Михаил.
    Создан - 2022.07.14.
*/

// Страница отчёта диспетчера (за период) - отчёта по количеству операций двора с теми или иными призаками, по дням.
class DispatcherReportPage {
    shortUrl;
    data = [];
    markColumnWidth = 240;
    valueColumnWidth = 80;

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

        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");

        return result != "" ? "?" + result : "";
    }

    async getData() {
        const fromDate = utils.getUrlParameter("from_date");
        const toDate = utils.getUrlParameter("to_date");
        const result = await api.createDispatcherReport(fromDate, toDate);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            this.data = [];
            alert(result.message);
        }
    }

    clearOperations() {
        const headerCells = document.getElementsByClassName("header-cell");
        while (headerCells.length > 0) {
            headerCells[0].remove();
        }
        const contentCells = document.getElementsByClassName("content-cell");
        while (contentCells.length > 0) {
            contentCells[0].remove();
        }
        this.data = [];
    }

    async showReport() {
        if (this.data.length <= 0) {
            return;
        }
        const headerTr = document.getElementById("header-tr");

        const nextDayOperationsTr = document.getElementById("next-day-operations-tr");
        const dayToDayOperationsTr = document.getElementById("day-to-day-operations-tr");
        const dayOperationsTr = document.getElementById("day-operations-tr");
        const dayToDayOperationsPercentTr = document.getElementById("day-to-day-operations-percent-tr");
        const deletedOperationsTr = document.getElementById("deleted-operations-tr");

        const idleOperationsTr = document.getElementById("idle-operations-tr");
        const overPlanIncomesTr = document.getElementById("over-plan-incomes-tr");
        const overPlanOutgoesTr = document.getElementById("over-plan-outgoes-tr");

        const palletIncomesTr = document.getElementById("pallet-incomes-tr");
        const boxIncomesTr = document.getElementById("box-incomes-tr");
        const totalIncomesTr = document.getElementById("total-incomes-tr");

        const palletOutgouesTr = document.getElementById("pallet-outgoes-tr");
        const boxOutgoesTr = document.getElementById("box-outgoes-tr");
        const totalOutgoesTr = document.getElementById("total-outgoes-tr");

        const gates1_6operationsTr = document.getElementById("gates-1-6-operations-tr");
        const gate7operationsTr = document.getElementById("gate-7-operations-tr");
        const gates8_16operationsTr = document.getElementById("gates-8-16-operations-tr");

        const totalOperationsTr = document.getElementById("total-operations-tr");

        let width = this.markColumnWidth;
        for (let element of this.data) {
            const totalIncomes = element.palletIncomes + element.boxIncomes;
            const totalOutgoes = element.palletOutgoues + element.boxOutgoes;
            const overPlanCellOnclick = function () { location.href = "OperationalReportPage?date=" + element.stringDate + "&shift=day"; };

            //const cellStyle = element.isHoliday ? "background-color: bisque" : "";
            const cellStyle = "";

            width += this.valueColumnWidth;
            const br = document.createElement("br");
            const dayOwWeekB = document.createElement("b");
            dayOwWeekB.textContent = element.dayOfWeek;
            viewUtils.createCell(headerTr, ["header-cell"], [element.day, br, dayOwWeekB], "width: 80px !important; " + cellStyle);

            viewUtils.createCell(nextDayOperationsTr, ["content-cell"], [element.nextDayOperations], cellStyle);
            viewUtils.createCell(dayToDayOperationsTr, ["content-cell"], [element.dayToDayOperations], cellStyle);
            viewUtils.createCell(dayOperationsTr, ["content-cell"], [element.dayOperations], cellStyle);
            viewUtils.createCell(dayToDayOperationsPercentTr, ["content-cell"], [utils.displayFloat(element.dayToDayOperationsPercent)], cellStyle);
            viewUtils.createCell(deletedOperationsTr, ["content-cell"], [element.deletedOperations], cellStyle);

            viewUtils.createCell(idleOperationsTr, ["content-cell"], [element.idleOperations], cellStyle);
            const overPlanIncomesTd =
                viewUtils.createCell(overPlanIncomesTr, ["content-cell"], [utils.displayFloat(100 * element.overPlanIncomes / totalIncomes)], cellStyle);
            overPlanIncomesTd.onclick = overPlanCellOnclick; 
            const overPlanOutgoesTd =
                viewUtils.createCell(overPlanOutgoesTr, ["content-cell"], [utils.displayFloat(100 * element.overPlanOutgoes / totalOutgoes)], cellStyle);
            overPlanOutgoesTd.onclick = overPlanCellOnclick;

            viewUtils.createCell(palletIncomesTr, ["content-cell"], [element.palletIncomes], cellStyle);
            viewUtils.createCell(boxIncomesTr, ["content-cell"], [element.boxIncomes], cellStyle);
            viewUtils.createCell(totalIncomesTr, ["content-cell"], [totalIncomes], cellStyle);

            viewUtils.createCell(palletOutgouesTr, ["content-cell"], [element.palletOutgoues], cellStyle);
            viewUtils.createCell(boxOutgoesTr, ["content-cell"], [element.boxOutgoes], cellStyle);
            viewUtils.createCell(totalOutgoesTr, ["content-cell"], [totalOutgoes], cellStyle);

            viewUtils.createCell(gates1_6operationsTr, ["content-cell"], [utils.displayFloat(element.gates1_6Operations / 24 / 60 / 60 / 6 * 100)], cellStyle);
            viewUtils.createCell(gate7operationsTr, ["content-cell"], [utils.displayFloat(element.gate7Operations / 24 / 60 / 60 * 100)], cellStyle);
            viewUtils.createCell(gates8_16operationsTr, ["content-cell"], [utils.displayFloat(element.gates8_16Operations / 24 / 60 / 60 / 8 * 100)], cellStyle);

            viewUtils.createCell(totalOperationsTr, ["content-cell"],
                [element.palletIncomes + element.boxIncomes + element.palletOutgoues + element.boxOutgoes], cellStyle);
        }
        document.getElementById("report-table").style.cssText = "width: " + width + "px; margin: 0; margin-top: 16px";
    }

    showReportTitle() {
        const reportHeaderB = document.getElementById("report-header-b");
        const fromDate = utils.getUrlParameter("from_date");
        const toDate = utils.getUrlParameter("to_date");
        reportHeaderB.textContent = "Отчёт диспетчера с " + fromDate + " по " + toDate + " (ВКЛЮЧАЯ!)";
    }

    async onChangeFilter() {
        this.clearOperations();
        await this.getData();
        await this.showReport();
    }

    onChangeParameters() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async createReport() {
        this.clearOperations();
        await this.getData();
        await this.showReport();
        this.showReportTitle();
    }

    exportToExcel() {
        const reportTable = document.getElementById("report-table");

        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        const fileName = "dispatcherReport_" + fromDate + "_" + toDate;
        const sheetName = fromDate + " - " + toDate;

        viewUtils.tableToExcel_v2(reportTable, fileName, "xlsx", sheetName);
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
        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        viewUtils.initDatePicker("from-date-input", fromDate, this.onChangeParameters.bind(this));
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        viewUtils.initDatePicker("to-date-input", toDate, this.onChangeParameters.bind(this));        

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        await this.createReport();
    }
}

const dispatcherReportPage = new DispatcherReportPage();

document.addEventListener('DOMContentLoaded', dispatcherReportPage.show.bind(dispatcherReportPage));