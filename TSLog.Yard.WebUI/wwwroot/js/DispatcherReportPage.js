/*
    Страница отчёта диспетчера (за период).
    Автор - Буров Михаил.
    Создан - 2022.07.14.
*/

// Страница отчёта диспетчера (за период).
class DispatcherReportPage {
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

    async showData() {
        const headerTr = document.getElementById("header-tr");

        const nextDayOperationsTr = document.getElementById("next-day-operations-tr");
        const dayToDayOperationsTr = document.getElementById("day-to-day-operations-tr");
        const deletedOperationsTr = document.getElementById("deleted-operations-tr");
        const idleOperationsTr = document.getElementById("idle-operations-tr");

        const palletIncomesTr = document.getElementById("pallet-incomes-tr");
        const boxIncomesTr = document.getElementById("box-incomes-tr");
        const totalIncomesTr = document.getElementById("total-incomes-tr");

        const palletOutgouesTr = document.getElementById("pallet-outgoues-tr");
        const boxOutgoesTr = document.getElementById("box-outgoes-tr");
        const totalOutgoesTr = document.getElementById("total-outgoes-tr");

        const totalOperationsTr = document.getElementById("total-operations-tr");

        let width = 200;
        for (let element of this.data) {
            //const cellStyle = element.isHoliday ? "background-color: bisque" : "";
            const cellStyle = "";

            width += 80;
            const br = document.createElement("br");
            const dayOwWeekB = document.createElement("b");
            dayOwWeekB.textContent = element.dayOfWeek;
            viewUtils.createCell(headerTr, ["header-cell"], [element.day, br, dayOwWeekB], "width: 80px !important; " + cellStyle);

            viewUtils.createCell(nextDayOperationsTr, ["content-cell"], [element.nextDayOperations], cellStyle);
            viewUtils.createCell(dayToDayOperationsTr, ["content-cell"], [element.dayToDayOperations], cellStyle);
            viewUtils.createCell(deletedOperationsTr, ["content-cell"], [element.deletedOperations], cellStyle);
            viewUtils.createCell(idleOperationsTr, ["content-cell"], [element.idleOperations], cellStyle);

            viewUtils.createCell(palletIncomesTr, ["content-cell"], [element.palletIncomes], cellStyle);
            viewUtils.createCell(boxIncomesTr, ["content-cell"], [element.boxIncomes], cellStyle);
            viewUtils.createCell(totalIncomesTr, ["content-cell"], [element.palletIncomes + element.boxIncomes], cellStyle);

            viewUtils.createCell(palletOutgouesTr, ["content-cell"], [element.palletOutgoues], cellStyle);
            viewUtils.createCell(boxOutgoesTr, ["content-cell"], [element.boxOutgoes], cellStyle);
            viewUtils.createCell(totalOutgoesTr, ["content-cell"], [element.palletOutgoues + element.boxOutgoes], cellStyle);

            viewUtils.createCell(totalOperationsTr, ["content-cell"],
                [element.palletIncomes + element.boxIncomes + element.palletOutgoues + element.boxOutgoes], cellStyle);
        }
        document.getElementById("report-table").style.cssText = "width: " + width + "px; margin: auto";
    }

    async onChangeFilter() {
        this.clearOperations();
        await this.getData();
        await this.showData();
    }

    onChangeParameters() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async createReport() {
        this.clearOperations();
        await this.getData();
        await this.showData();

        const operationIdTh = document.getElementById("operation-id-th");
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