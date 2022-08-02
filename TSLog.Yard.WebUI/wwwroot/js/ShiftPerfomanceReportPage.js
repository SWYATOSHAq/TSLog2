/*
    Страница отчёта по производительности за смену.
    Автор - Буров Михаил.
*/

// Страница отчёта по производительности за смену (по конкретным операциям двора с распределением по воротам).
class ShiftPerfomanceReportPage {
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
        const result = await api.createShiftPerfomanceReport(date, shift);
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

    createOpenOperationFunction(id) {
        return function () {
            sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
            location.href = "/OperationPage?id=" + id + "&action=VIEW";
        }
    }

    prepareData(operations) {
        let maxOperationCount = 0;
        for (let operation of this.data) {
            if (operations[operation.gateCode] == undefined) {
                operations[operation.gateCode] = [];
            }
            operations[operation.gateCode].push(operation);
            if (operations[operation.gateCode].length > maxOperationCount) {
                maxOperationCount = operations[operation.gateCode].length;
            }
        }
        return maxOperationCount;
    }

    createGatesCodes(operations) {
        const gatesCodes = Object.keys(operations);
        gatesCodes.sort();
        return gatesCodes;
    }

    createTableHeaderTemplate(gatesCodes) {
        const gatesNames = {};
        gatesNames["EIN01"] = "Ворота 1";
        gatesNames["EIN02"] = "Ворота 2";
        gatesNames["EIN03"] = "Ворота 3";
        gatesNames["EIN04"] = "Ворота 4";
        gatesNames["EIN05"] = "Ворота 5";
        gatesNames["EIN06"] = "Ворота 6";
        gatesNames["EIN07"] = "Ворота 7";
        gatesNames["EIN08"] = "Ворота 8";
        gatesNames["EIN09"] = "Ворота 9";
        gatesNames["EIN10"] = "Ворота 10";
        gatesNames["EIN11"] = "Ворота 11";
        gatesNames["EIN12"] = "Ворота 12";
        gatesNames["EIN13"] = "Ворота 13";
        gatesNames["EIN14"] = "Ворота 14";
        gatesNames["EIN15"] = "Ворота 15";
        gatesNames["EIN16"] = "Ворота 16";

        const headerRow = document.getElementById("header-row");
        headerRow.replaceChildren();
        for (let gateCode of gatesCodes) {
            const columnHeader = document.createElement("th");
            columnHeader.classList.add("table-header");
            columnHeader.textContent = gatesNames[gateCode];
            headerRow.append(columnHeader);
        }  
    }

    createTableBobyTemplate(maxOperationCount, gatesCodes) {
        const tbody = document.getElementById("elements-tbody");
        tbody.replaceChildren();
        const gatesCount = gatesCodes.length;
        for (let rowIndex = 0; rowIndex < maxOperationCount + 1; rowIndex++) {
            const row = document.createElement("tr");
            for (let columnIndex = 0; columnIndex < gatesCount; columnIndex++) {
                const cell = document.createElement("td");
                cell.classList.add("table-cell");
                cell.textContent = "";
                cell.id = gatesCodes[columnIndex] + "-" + (rowIndex + 1).toString();
                row.append(cell);
            }
            tbody.append(row);
        }
    }

    displayData(operations, maxOperationCount) {
        for (let gateCode in operations) {
            let index = 0;
            let total = 0.0;
            for (let operation of operations[gateCode]) {
                index++;
                const id = gateCode + "-" + index.toString();
                const cell = document.getElementById(id);

                cell.append(operation.partnerNames);
                cell.append(document.createElement("br"));

                cell.append(operation.vehicleNumber);
                cell.append(document.createElement("br"));

                const perfomance = operation.factLaborCost != 0 ? Math.round(100 * operation.planLaborCost / operation.factLaborCost) : 0.0;
                total += perfomance;
                cell.append(perfomance + " %");
                cell.append(document.createElement("br"));

                const openButton = document.createElement("button");
                openButton.classList.add("btn");
                openButton.classList.add("btn-outline-info");
                openButton.classList.add("table-cell-button");
                openButton.title = "Открыть карточку операции двора";
                openButton.style.cssText = "width: 100px";
                openButton.onclick = this.createOpenOperationFunction(operation.id);
                openButton.textContent = "Открыть";
                cell.append(openButton);

                cell.style.cssText = perfomance >= 100 ? "box-shadow: none; background-color: palegreen" : "box-shadow: none; background-color: #ffcccc"
            }

            // Выводим среднее значение по воротам
            const average = Math.round(total / operations[gateCode].length);
            const id = gateCode + "-" + (maxOperationCount + 1).toString();
            const cell = document.getElementById(id);
            cell.textContent = average.toString() + " %"
        }
    }

    showReport() {
        // Готовим данные
        let operations = {};
        const maxOperationCount = this.prepareData(operations);
        const gatesCodes = this.createGatesCodes(operations);        

        // Готовим шаблон заголовка
        this.createTableHeaderTemplate(gatesCodes);

        // Готовим шалон самой таблицы
        this.createTableBobyTemplate(maxOperationCount, gatesCodes);

        // Выводим непосредствено данные
        this.displayData(operations, maxOperationCount);
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

const shiftPerfomanceReportPage = new ShiftPerfomanceReportPage();

document.addEventListener('DOMContentLoaded', shiftPerfomanceReportPage.show.bind(shiftPerfomanceReportPage));