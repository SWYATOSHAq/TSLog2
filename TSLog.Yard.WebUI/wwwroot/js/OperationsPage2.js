class OperationsPage {
    shortUrl;

    operations = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    getPartnersNames(operation) {
        let result = "";
        for (let partner of operation.Partners) {
            result += ((result.Length > 0) ? ", " : "") + partner.Name;
        }
        return result;
    }

    showOperationPage(id) {
        location.href = "OperationPage?id=" + id;
    }

    openOperation(id) {
        sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
        const url = "OperationPage?id=" + id;
        location.href = url;
    }

    createOpenOperation(id) {
        return function () {
            this.openOperation(id);
        }        
    }

    createOpenOperationFunction(id) {
        return function () {
            sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
            location.href = "/OperationPage?id=" + id;
        }
    }

    showOperation(operation, operationTr) {
        operation.startDate = new Date(operation.startDate);
        operation.planWorkStartDate = new Date(operation.planWorkStartDate);
        operation.factWorkStartDate = new Date(operation.factWorkStartDate);
        operation.planWorkFinishDate = new Date(operation.planWorkFinishDate);
        operation.factWorkFinishDate = new Date(operation.factWorkFinishDate);
        operation.finishDate = new Date(operation.finishDate);
        let contents = [];

        const openP = document.createElement("p");
        openP.classList.add("text-center");
        openP.classList.add("p-margin-0");
        openP.textContent = operation.id;
        contents = [openP];

        const openButton = document.createElement("button");
        openButton.classList.add("btn");
        openButton.classList.add("btn-outline-info");
        openButton.classList.add("m-3");
        openButton.classList.add("table-cell-button");
        openButton.style.cssText = "width: 100px !important";
        openButton.onclick = this.createOpenOperationFunction(operation.id);
        openButton.textContent = "Открыть";
        contents.push(openButton);

        viewUtils.createCell(operationTr, ["table-cell"], contents, "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.typeName], "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.partnersNames], "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.driverName], "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.driverPhoneNumber], "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.stateName], "box-shadow: none; background-color: " + operation.displayColor);



        let text = operation.startDate > utils.nullDate ?
            operation.startDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.startDate.toLocaleDateString() :
            "Ожидается";
        viewUtils.createCell(operationTr, ["table-cell"], [text], "box-shadow: none; background-color: " + operation.displayColor);

        contents = [operation.planWorkStartDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.planWorkStartDate.toLocaleDateString()];
        if (operation.factWorkStartDate > utils.nullDate) {
            contents.push(document.createElement("br"));
            contents.push("(" + operation.factWorkStartDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.factWorkStartDate.toLocaleDateString() + ")");
        }
        viewUtils.createCell(operationTr, ["table-cell"], contents, "width: 13% !important; box-shadow: none; background-color: " + operation.displayColor);

        contents = [operation.planWorkFinishDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.planWorkFinishDate.toLocaleDateString()];
        if (operation.factWorkFinishDate > utils.nullDate) {
            contents.push(document.createElement("br"));
            contents.push("(" + operation.factWorkFinishDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.factWorkFinishDate.toLocaleDateString() + ")");
        }
        viewUtils.createCell(operationTr, ["table-cell"], contents, "width: 13% !important; box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.finishDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.finishDate.toLocaleDateString()], "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.vehicleNumber], "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.vehicleKindShortName], "width: 10% !important; box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.note], "box-shadow: none; background-color: " + operation.displayColor);
    }

    createParameters() {
        const fromDatePicker = document.getElementById("from-date-input");
        let result = "?from_date=" + fromDatePicker.value;

        const toDatePicker = document.getElementById("to-date-input");
        result = result + "&to_date=" + toDatePicker.value;

        const gatesSelect = document.getElementById("gates-select");
        if (gatesSelect != undefined && gatesSelect.value != "") {
            result = result + "&gate_code=" + gatesSelect.value;
        }

        const partnersSelect = document.getElementById("partners-select");
        if (partnersSelect != undefined && partnersSelect.value != "") {
            result = result + "&partner_id=" + partnersSelect.value;
        }

        const operationTypesSelect = document.getElementById("operation-types-select");
        if (operationTypesSelect != undefined && operationTypesSelect.value != "") {
            result = result + "&operation_type_code=" + operationTypesSelect.value;
        }

        const vehicleKindsSelect = document.getElementById("vehicle-kinds-select");
        if (vehicleKindsSelect != undefined && vehicleKindsSelect.value != "") {
            result = result + "&vehicle_kind_code=" + vehicleKindsSelect.value;
        }

        const operationStatesSelect = document.getElementById("operation-states-select");
        if (operationStatesSelect != undefined && operationStatesSelect.value != "") {
            result = result + "&state_code=" + operationStatesSelect.value;
        }

        const vehicleNumberInput = document.getElementById("vehicle-number-input");
        if (vehicleNumberInput != undefined && vehicleNumberInput.value != "") {
            result = result + "&vehicle_number=" + vehicleNumberInput.value;
        }

        const driverNameInput = document.getElementById("driver-name-input");
        if (driverNameInput != undefined && driverNameInput.value != "") {
            result = result + "&driver_name=" + driverNameInput.value;
        }

        const workerNameInput = document.getElementById("worker-input");
        if (workerNameInput != undefined && workerNameInput.value != "") {
            result = result + "&worker=" + workerNameInput.value;
        }

        return result;
    }

    async getOperations() {        
        const parameters = this.createParameters();
        this.operations = await api.getOperations(parameters);
        const url = this.shortUrl + parameters;
        history.pushState({}, null, url);
    }

    clearOperations() {
        this.operations = [];
        const container = document.getElementById("operations-table-body");
        container.replaceChildren();
    }

    showOperations() {
        const operationsTBody = document.getElementById("operations-table-body");
        for (let operation of this.operations) {
            const operationTr = document.createElement("tr");
            this.showOperation(operation, operationTr);
            operationsTBody.appendChild(operationTr);
        }
    }

    async onChangeFilter() {
        this.clearOperations();
        await this.getOperations();
        this.showOperations();
    }

    getOperationTypeHint() {
        const select = document.getElementById("operation-types-select");
        if (select.selectedIndex >= 0) {
            select.title = select.options[select.selectedIndex].getAttribute("note");
        }        
    }

    async onChangeOperationType() {
        await this.onChangeFilter();
        this.getOperationTypeHint();        
    }

    async show() {
        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = fromDate;
        viewUtils.initDatePicker("from-date-input", fromDate, this.onChangeFilter.bind(this));
        viewUtils.initDatePicker("to-date-input", toDate, this.onChangeFilter.bind(this));

        viewUtils.initSelect("gates-select", utils.getUrlParameter("gate_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("partners-select", utils.getUrlParameter("partner_id"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("operation-types-select", utils.getUrlParameter("operation_type_code"), this.onChangeOperationType.bind(this));
        viewUtils.initSelect("vehicle-kinds-select", utils.getUrlParameter("vehicle_kind_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("operation-states-select", utils.getUrlParameter("state_code"), this.onChangeFilter.bind(this));

        viewUtils.initInput("vehicle-number-input", utils.getUrlParameter("vehicle_number"), this.onChangeFilter.bind(this));
        viewUtils.initInput("driver-name-input", utils.getUrlParameter("driver_name"), this.onChangeFilter.bind(this));
        viewUtils.initInput("partner-action-id-input", utils.getUrlParameter("partner_action_id"), this.onChangeFilter.bind(this));
        viewUtils.initInput("worker-input", utils.getUrlParameter("worker"), this.onChangeFilter.bind(this));

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        await this.getOperations();
        this.showOperations();

        this.getOperationTypeHint();
    }
}

const operationsPage = new OperationsPage();

document.addEventListener('DOMContentLoaded', operationsPage.show.bind(operationsPage));