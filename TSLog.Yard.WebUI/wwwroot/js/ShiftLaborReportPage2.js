class ShiftLaborReportPage2 {
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
        this.data = await api.createShiftLaborReport2(date, shift);
    }

    clearReport() {
        const elementsTbody = document.getElementById("elements-tbody");
        elementsTbody.replaceChildren();
    }

    showElement(operation, reportTbody) {
        
    }

    createOpenOperationFunction(id) {
        return function () {
            sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
            location.href = "/OperationPage?id=" + id;
        }
    }

    showReport() {
        let operations = { };
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
        const gatesCodes = Object.keys(operations);
        gatesCodes.sort();

        // Готовим шаблон заголовка
        //<th class="table-header">В. 1</th>

        const gates = { };
        gates["EIN01"] = "Ворота 1";
        gates["EIN02"] = "Ворота 2";
        gates["EIN03"] = "Ворота 3";
        gates["EIN04"] = "Ворота 4";
        gates["EIN05"] = "Ворота 5";
        gates["EIN06"] = "Ворота 6";
        gates["EIN07"] = "Ворота 7";
        gates["EIN08"] = "Ворота 8";
        gates["EIN09"] = "Ворота 9";
        gates["EIN10"] = "Ворота 10";
        gates["EIN11"] = "Ворота 11";
        gates["EIN12"] = "Ворота 12";
        gates["EIN13"] = "Ворота 13";
        gates["EIN14"] = "Ворота 14";
        gates["EIN15"] = "Ворота 15";
        gates["EIN16"] = "Ворота 16";

        const headerRow = document.getElementById("header-row");
        headerRow.replaceChildren();
        for (let gateCode of gatesCodes) {
            const columnHeader = document.createElement("th");
            columnHeader.classList.add("table-header");
            columnHeader.textContent = gates[gateCode];
            headerRow.append(columnHeader);
        }        

        // Готовим шалон самой таблицы
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

        // Выводим непосредствено данные
        for (let gateCode in operations) {
            let index = 0;
            let total = 0.0;
            for (let operation of operations[gateCode]) {
                index++;
                const id = gateCode + "-" + index.toString();
                const cell = document.getElementById(id);

                cell.append(operation.vehicleNumber);
                cell.append(document.createElement("br"));

                //cell.append(operation.planLaborCost + " / " + operation.factLaborCost + " (ч. * м.)");
                //cell.append(document.createElement("br"));

                const e = operation.factLaborCost != 0 ? Math.round(100 * operation.planLaborCost / operation.factLaborCost) : 0.0;
                total += e;
                cell.append(e + " %");
                cell.append(document.createElement("br"));

                const openButton = document.createElement("button");
                openButton.classList.add("btn");
                openButton.classList.add("btn-outline-info");
                openButton.classList.add("m-3");
                openButton.classList.add("table-cell-button");
                openButton.style.cssText = "width: 100px";
                openButton.onclick = this.createOpenOperationFunction(operation.id);
                //openButton.textContent = operation.id;
                openButton.textContent = "Открыть";
                cell.append(openButton);     

                cell.style.cssText = e >= 100 ? "box-shadow: none; background-color: palegreen" : "box-shadow: none; background-color: #ffcccc"
            }

            // Выводим среднее значение по воротам
            const average = Math.round(total / operations[gateCode].length);
            const id = gateCode + "-" + (maxOperationCount + 1).toString();
            const cell = document.getElementById(id);
            cell.textContent = average.toString() + " %"
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

const shiftLaborReportPage2 = new ShiftLaborReportPage2();

document.addEventListener('DOMContentLoaded', shiftLaborReportPage2.show.bind(shiftLaborReportPage2));