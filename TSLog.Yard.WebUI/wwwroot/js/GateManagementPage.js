/*
    Страница по управлению воротами.
    Автор - Аввакумов Алексей (avvakumov@tslog.ru).
*/

// Страница по управлению воротами - 
// по конкретным операциям двора с распределением по воротам, 
// с графическим указанием плановой продолжительности операции двора.
class GateManagementPage {
    shortUrl;

    data = [];
    reportRequest = 0;
    shiftReport = null;

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        result = utils.addParameter(result, "date", "date-input");
        if (document.getElementById("shift-select").value)
            result = utils.addParameter(result, "shift", "shift-select");
        return result != "" ? "?" + result : "";
    }

    clearReport() {
        const elementsTbody = document.getElementById("elements-tbody");
        elementsTbody.replaceChildren();
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
            location.href = `/OperationPage?id=${id}&action=VIEW`;
          
          //block = document.getElementById("fact-" + id);
          //if (block != undefined) {
          //    block.classList.add("fact-selected-block");
          //}
        }
    }

    formatTime(hour, minutes) {
        const hourString = hour.toString().padStart(2, "0");
        const minuteString = minutes.toString().padStart(2, "0");
        return hourString + ":" + minuteString;
    }

    showReport(shift, shiftGates) {
        let operations = { };
        const intervalsCount = shift ? 48 : 96;
        for (let operation of this.data) {
            if (operations[operation.gateCode] == undefined) {
                operations[operation.gateCode] = [];
            }
            operations[operation.gateCode].push(operation);
        }
        const shiftGatesByCode = new Map(shiftGates.map(gate => [gate.code, gate]));
        const gatesCodes = shift ? [...shiftGatesByCode.keys()] : Object.keys(operations);
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
        gates["EIN17"] = "Ворота 17";
        const headerRow = document.getElementById("header-row");
        headerRow.replaceChildren();
        let columnHeader = document.createElement("th");
        columnHeader.classList.add("table-header");
        columnHeader.textContent = "Интервал";
        headerRow.append(columnHeader);
        for (let gateCode of gatesCodes) {
            columnHeader = document.createElement("th");
            columnHeader.classList.add("table-header");
            columnHeader.textContent = shift ? shiftGatesByCode.get(gateCode).name : gates[gateCode];
            headerRow.append(columnHeader);
        }        

        // Готовим шалон самой таблицы
        const tbody = document.getElementById("elements-tbody");
        tbody.replaceChildren();
        const gatesCount = gatesCodes.length;
        let startHour = shift === "night" ? 21 : shift === "day" ? 9 : 0;
        let finishHour = startHour;
        let startMinutes = 0;
        let finishMinutes = 15;
        for (let rowIndex = 0; rowIndex < intervalsCount; rowIndex++) {
            const row = document.createElement("tr");
            row.style.cssText = "height: 40px";

            const cell = document.createElement("td");
            cell.classList.add("row-header");
            startHour = startHour >= 24 ? startHour - 24 : startHour;
            finishHour = finishHour >= 24 ? finishHour - 24 : finishHour;
            //cell.textContent = startHour.toString() + ":00-" + finishHour.toString() + ":00";
            cell.textContent = this.formatTime(startHour, startMinutes) + "-" + this.formatTime(finishHour, finishMinutes);
            startMinutes += 15;
            finishMinutes += 15;
            if (finishMinutes === 60) {
                finishMinutes = 0;
                finishHour++;
            }
            if (startMinutes === 60) {
                startMinutes = 0;
                startHour++;
            }
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
        if (shift && shiftGates.length > 0) {
            const row = document.createElement("tr");
            const label = document.createElement("th");
            label.scope = "row";
            label.textContent = "Занятость, %";
            row.append(label);
            for (const gateCode of gatesCodes) {
                const cell = document.createElement("td");
                cell.textContent = shiftGatesByCode.get(gateCode).occupancyPercent.toLocaleString("ru-RU", {
                    maximumFractionDigits: 1
                });
                row.append(cell);
            }
            tbody.append(row);
        }

        // Выводим непосредствено данные
        for (let gateCode in operations) {
            const id = gateCode + "-1";
            const cell = document.getElementById(id);

            for (let operation of operations[gateCode]) {
                if (operation.planStart < 0) {
                    operation.planDuration += operation.planStart;
                    operation.planStart = 0;
                    operation.planDuration = operation.planDuration > 0 ? operation.planDuration : 0;
                }
                //if (operation.factStart < 0) {
                //    operation.factDuration += operation.factStart;
                //    operation.factStart = 0;
                //    operation.factDuration = operation.factDuration > 0 ? operation.factDuration : 0;
                //}

                //if (Number(operation.factDuration) > 0) {
                //    const factBlock = document.createElement("div");
                //    factBlock.id = "fact-" + operation.id;
                //    factBlock.onclick = this.createBlockClick(operation.id);
                //    factBlock.style.cssText = "height: " + Math.round(operation.factDuration * 2).toString() + "px; " +
                //        "width: 100px; " +
                //        "background-color: dodgerblue; " +
                //        "opacity: 50%; " +
                //        "position: absolute; " +
                //        "margin-top: " + Math.round(operation.factStart * 2).toString() + "px; " + 
                //        "border: 2px solid";
                //    cell.append(factBlock);
                //}                

                if (Number(operation.planDuration) > 0) {
                    const planBlock = document.createElement("div");
                    planBlock.id = "plan-" + operation.id;
                    planBlock.onmouseover = function (event) {
                        let target = event.target;
                        target.style.background = 'darkred';
                        target.style.cursor = "pointer";
                        //target.сlassList.add("plan-selected-block");
                        text.value += "mouseover " + target.tagName + "\n";
                        text.scrollTop = text.scrollHeight;
                    };
                    planBlock.onmouseout = function (event) {
                        let target = event.target;
                        //target.сlassList.remove("plan-selected-block");
                        target.style.cursor = "default";
                        target.style.background = 'coral';
                        text.value += "mouseout " + target.tagName + "\n";
                        text.scrollTop = text.scrollHeight;
                    };
                    planBlock.onclick = this.createBlockClick(operation.id);
                    planBlock.style.cssText = "height: " + Math.round(operation.planDuration * 2.67).toString() + "px; " +
                        "width: 100px; " +
                        "background-color: coral; " +
                        "opacity: 50%; " +
                        "position: absolute; " +
                        "margin-top: " + Math.round(operation.planStart * 2.665).toString() + "px; " +
                        "border: 2px solid";
                    cell.append(planBlock);
                }                
            }
        }
    }

    showReportTitle(date, shift) {
        const reportHeaderB = document.getElementById("report-header-b");
        const shiftTitle = shift === "night" ? " · ночная смена (21:00–09:00)" :
            shift === "day" ? " · дневная смена (09:00–21:00)" : "";
        reportHeaderB.textContent = "Управление воротами за " + (date || utils.getNowDate()) + shiftTitle;
    }

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async createReport() {
        const request = ++this.reportRequest;
        const date = utils.getUrlParameter("date");
        const shift = document.getElementById("shift-select").value;
        const exportButton = document.getElementById("export-gate-management-button");
        this.shiftReport = null;
        exportButton.disabled = true;
        this.clearReport();
        const result = await api.createGateManagement(date, shift);
        if (request !== this.reportRequest)
            return;

        this.data = [];
        let shiftGates = [];
        if (result.code === 0) {
            this.data = shift ? result.data.operations : result.data;
            if (shift) {
                shiftGates = result.data.gates;
                this.shiftReport = { date: date || utils.getNowDate(), shift, gates: shiftGates, operations: this.data };
                exportButton.disabled = false;
            }
        } else {
            viewUtils.showError(result.message);
        }
        this.showReport(shift, shiftGates);
        this.showReportTitle(date, shift);
    }

    exportToExcel() {
        if (!this.shiftReport)
            return;
        if (typeof XLSX === "undefined") {
            viewUtils.showError("Не удалось загрузить модуль выгрузки в Excel.");
            return;
        }

        const { date, shift, gates, operations } = this.shiftReport;
        const shiftTitle = shift === "night" ? "Ночная" : "Дневная";
        const gateNames = new Map(gates.map(gate => [gate.code, gate.name]));
        const summary = [
            ["Дата начала смены", "Смена", "Код ворот", "Ворота", "Занятость, %"],
            ...gates.map(gate => [
                date, shiftTitle, gate.code, gate.name, Math.round(gate.occupancyPercent * 10) / 10
            ])
        ];
        const details = [
            ["Ворота", "Заявка", "Автомобиль", "Клиенты", "Начало на диаграмме", "Конец на диаграмме",
                "Фактическое начало", "Фактическое окончание"],
            ...operations.map(operation => [
                gateNames.get(operation.gateCode), operation.id, operation.vehicleNumber, operation.partnerNames,
                this.formatExportDate(operation.planWorkStartDate), this.formatExportDate(operation.planWorkFinishDate),
                this.formatExportDate(operation.factWorkStartDate), this.formatExportDate(operation.factWorkFinishDate)
            ])
        ];
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, XLSX.utils.aoa_to_sheet(summary), "Занятость");
        XLSX.utils.book_append_sheet(workbook, XLSX.utils.aoa_to_sheet(details), "Операции");
        XLSX.writeFile(workbook, `Занятость ворот ${date.replaceAll("/", "-")} ${shiftTitle}.xlsx`);
    }

    formatExportDate(value) {
        return value && !value.startsWith("0001-") ? value.slice(0, 19).replace("T", " ") : "";
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
        const shiftSelect = document.getElementById("shift-select");
        const shift = utils.getUrlParameter("shift");
        shiftSelect.value = shift === "day" || shift === "night" ? shift : "";
        shiftSelect.addEventListener("change", this.onChangeParameters.bind(this));
        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        this.createReport();
    }
}

const gateManagementPage = new GateManagementPage();

document.addEventListener('DOMContentLoaded', gateManagementPage.show.bind(gateManagementPage));
