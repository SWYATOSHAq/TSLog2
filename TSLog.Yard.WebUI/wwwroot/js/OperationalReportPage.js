/*
    Страница оперативного отчёта (за смену).
    Автор - Буров Михаил.
    Создан - 2022.07.14.
*/
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

        result = utils.addParameter(result, "gate_code", "gates-select");
        result = utils.addParameter(result, "partner_id", "partners-select");
        result = utils.addParameter(result, "operation_type_code", "operation-types-select");
        result = utils.addParameter(result, "vehicle_kind_code", "vehicle-kinds-select");
        result = utils.addParameter(result, "state_code", "operation-states-select");

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

    async showOperations() {
        const reportTBody = document.getElementById("report-tbody");
        for (let element of this.data) {
            element.startDate = utils.stringToDate(element.startDate);
            element.planWorkStartDate = utils.stringToDate(element.planWorkStartDate);
            element.factWorkStartDate = utils.stringToDate(element.factWorkStartDate);
            element.planWorkFinishDate = utils.stringToDate(element.planWorkFinishDate);
            element.factWorkFinishDate = utils.stringToDate(element.factWorkFinishDate);
            element.finishDate = utils.stringToDate(element.finishDate);

            const isBad = element.factWorkFinishDate > utils.nullDate && element.planWorkFinishDate < element.factWorkFinishDate ||
                element.factDuration > 0 && element.planDuration < element.factDuration ||
                element.factLaborCost > 0 && element.planLinearLaborCost < element.factLaborCost;

            const tr = document.createElement("tr");
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
            if (element.factWorkStartDate > utils.nullDate) {
                content += " (" + utils.timeToStringHHMM(element.factWorkFinishDate) + ")"
            }
            viewUtils.createCell(tr, ["table-cell"], [content]);

            viewUtils.createCell(tr, ["table-cell"],
                [element.finishDate > utils.nullDate ? utils.timeToStringHHMM(element.finishDate) : ""]);

            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.presenceTime / 60)]);

            content = utils.displayFloat(element.planDuration / 60);
            if (element.factDuration > 0) {
                content += " (" + utils.displayFloat(element.factDuration / 60) + ")"
            }
            viewUtils.createCell(tr, ["table-cell"], [content]);

            content = utils.displayFloat(element.planLinearLaborCost / 60);
            if (element.factLaborCost > 0) {
                content += " (" + utils.displayFloat(element.factLaborCost / 60) + ")"
            }
            viewUtils.createCell(tr, ["table-cell"], [content]);

            viewUtils.createCell(tr, ["table-cell"], [element.workersCount]);
            viewUtils.createCell(tr, ["table-cell"], [element.warehouseActionsCount]);
                        
            //viewUtils.createCell(tr, ["table-cell"], [element.netWeight]);
            viewUtils.createCell(tr, ["table-cell"], [element.grossWeight]);
            viewUtils.createCell(tr, ["table-cell"], [element.volume]);

            viewUtils.createCell(tr, ["table-cell"], [""]);

            reportTBody.append(tr);
        }
    }

    async onChangeFilter() {
        this.clearOperations();
        await this.getData();
        this.sortOperations(this.sortCompare.bind(this));
        await this.showOperations();
    }

    getOperationTypeHint() {
        //const select = document.getElementById("operation-types-select");
        //if (select.selectedIndex >= 0) {
        //    select.title = select.options[select.selectedIndex].getAttribute("note");
        //}        
    }

    async onChangeOperationType() {
        await this.onChangeFilter();
        this.getOperationTypeHint();        
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
        this.showShiftInfo();

        viewUtils.initSelect("gates-select", utils.getUrlParameter("gate_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("partners-select", utils.getUrlParameter("partner_id"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("operation-types-select", utils.getUrlParameter("operation_type_code"), this.onChangeOperationType.bind(this));
        viewUtils.initSelect("vehicle-kinds-select", utils.getUrlParameter("vehicle_kind_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("operation-states-select", utils.getUrlParameter("state_code"), this.onChangeFilter.bind(this));
        viewUtils.initInput("partner-action-id-input", utils.getUrlParameter("partner_action_id"), this.onChangeFilter.bind(this));
        viewUtils.initInput("worker-input", utils.getUrlParameter("worker"), this.onChangeFilter.bind(this));
               
        this.getOperationTypeHint();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        viewUtils.sortTable();

        await this.createReport();
    }
}

const operationalReportPage = new OperationalReportPage();

document.addEventListener('DOMContentLoaded', operationalReportPage.show.bind(operationalReportPage));