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
        this.data = await api.createShiftLaborReport3(date, shift);
    }

    clearReport() {
        const elementsTbody = document.getElementById("elements-tbody");
        elementsTbody.replaceChildren();
    }

    showElement(operation, reportTbody) {
        
    }

    createBlockClick(id) {
        return function () {
            let blocks = document.getElementsByClassName("plan-selected-block");
            while (blocks.length > 0) {
                blocks[0].classList.remove("plan-selected-block");
            }
            blocks = document.getElementsByClassName("fact-selected-block");
            while (blocks.length > 0) {
                blocks[0].classList.remove("fact-selected-block");
            }

            let block = document.getElementById("plan-" + id);
            if (block != undefined) {
                block.classList.add("plan-selected-block");
            }            
            block = document.getElementById("fact-" + id);
            if (block != undefined) {
                block.classList.add("fact-selected-block");
            }
        }
    }

    showReport() {
        let operations = { };
        let intervalsCount = 12;
        for (let operation of this.data) {
            if (operations[operation.gateCode] == undefined) {
                operations[operation.gateCode] = [];
            }
            operations[operation.gateCode].push(operation);
        }
        const gatesCodes = Object.keys(operations);
        gatesCodes.sort();

        // Готовим шаблон заголовка
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
        let columnHeader = document.createElement("th");
        columnHeader.classList.add("table-header");
        columnHeader.textContent = "Интервал";
        headerRow.append(columnHeader);
        for (let gateCode of gatesCodes) {
            columnHeader = document.createElement("th");
            columnHeader.classList.add("table-header");
            columnHeader.textContent = gates[gateCode];
            headerRow.append(columnHeader);
        }        

        // Готовим шалон самой таблицы
        const tbody = document.getElementById("elements-tbody");
        tbody.replaceChildren();
        const gatesCount = gatesCodes.length;
        const shift = document.getElementById("shift-select").value;         
        let startHour = 9;
        let finishHour = 10;
        if (shift === "night") {
            startHour = 21;
            finishHour = 22;
        }
        for (let rowIndex = 0; rowIndex < intervalsCount; rowIndex++) {
            const row = document.createElement("tr");
            row.style.cssText = "height: 120px";

            const cell = document.createElement("td");
            cell.classList.add("row-header");
            startHour = startHour > 24 ? startHour - 24 : startHour;
            finishHour = finishHour > 24 ? finishHour - 24 : finishHour;
            cell.textContent = startHour.toString() + ":00-" + finishHour.toString() + ":00";
            startHour++;
            finishHour++;
            row.append(cell);

            for (let columnIndex = 0; columnIndex < gatesCount; columnIndex++) {
                const cell = document.createElement("td");
                cell.classList.add("table-cell");
                cell.textContent = "";
                cell.id = gatesCodes[columnIndex] + "-" + (rowIndex + 1).toString();
                row.append(cell);
            }
            tbody.append(row);
        }
        const row = document.createElement("tr");
        row.style.cssText = "height: 120px";
        const cell = document.createElement("td");
        cell.classList.add("row-header");
        cell.textContent = "Итого:";
        row.append(cell);
        for (let columnIndex = 0; columnIndex < gatesCount; columnIndex++) {
            const cell = document.createElement("td");
            cell.classList.add("table-cell");
            cell.textContent = "";
            cell.id = gatesCodes[columnIndex] + "-total";
            row.append(cell);
        }
        tbody.append(row);


        // Выводим непосредствено данные
        for (let gateCode in operations) {
            const id = gateCode + "-1";
            const cell = document.getElementById(id);

            for (let operation of operations[gateCode]) {
                if (Number(operation.factDuration) > 0) {
                    const factBlock = document.createElement("div");
                    factBlock.id = "fact-" + operation.id;
                    factBlock.onclick = this.createBlockClick(operation.id);
                    factBlock.style.cssText = "height: " + Math.round(operation.factDuration * 2).toString() + "px; " +
                        "width: 100px; " +
                        "background-color: dodgerblue; " +
                        "opacity: 50%; " +
                        "position: absolute; " +
                        "margin-top: " + Math.round(operation.factStart * 2).toString() + "px; " + 
                        "border: 2px solid";
                    cell.append(factBlock);
                }                

                if (Number(operation.planDuration) > 0) {
                    const planBlock = document.createElement("div");
                    planBlock.id = "plan-" + operation.id;
                    planBlock.onclick = this.createBlockClick(operation.id);
                    planBlock.style.cssText = "height: " + Math.round(operation.planDuration * 2).toString() + "px; " +
                        "width: 100px; " +
                        "background-color: coral; " +
                        "opacity: 50%; " +
                        "position: absolute; " +
                        "margin-top: " + Math.round(operation.planStart * 2).toString() + "px; " +
                        "border: 2px solid";
                    cell.append(planBlock);
                }                
            }

            // Выводим среднее значение по воротам
            //const average = Math.round(total / operations[gateCode].length);
            //const id = gateCode + "-" + (maxOperationCount + 1).toString();
            //const cell = document.getElementById(id);
            //cell.textContent = average.toString() + " %"
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