class OperationsPage {
    shortUrl;
    operations = [];
    sortedOperations = [];
    sortField = "driverName";
    sortDirection = "forward";

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    openOperation(id) {
        sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
        const url = "OperationPage?id=" + id;
        location.href = url;
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
        viewUtils.createCell(operationTr, ["table-cell"], [operation.typeShortName], "box-shadow: none; background-color: " + operation.displayColor);
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
        viewUtils.createCell(operationTr, ["table-cell"], [operation.gateShortName], "box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.vehicleKindShortName], "width: 10% !important; box-shadow: none; background-color: " + operation.displayColor);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.note], "box-shadow: none; background-color: " + operation.displayColor);
    }

    sortCompare(operation1, operation2) {
        switch (this.sortField) {
            case "driverName":
                return operation1.driverName.localeCompare(operation2.driverName);
            case "id":
                return operation1.id - operation2.id;
            case "partnerId":
                return operation1.partnerId.localeCompare(operation2.partnerId);
            case "gateCode":
                return operation1.gateCode.localeCompare(operation2.gateCode);
            case "vehicleNumber":
                return operation1.vehicleNumber.localeCompare(operation2.vehicleNumber);
            case "startDate":
                return (new Date(operation1.startDate)) - (new Date(operation2.startDate));
            case "directionName":
                return operation1.directionName.localeCompare(operation2.directionName);
            case "planWorkStartDate":
                return (new Date(operation1.planWorkStartDate)) - (new Date(operation2.planWorkStartDate));
            case "factWorkStartDate":
                return (new Date(operation1.factWorkStartDate)) - (new Date(operation2.factWorkStartDate));
            case "finishDate":
                return (new Date(operation1.finishDate)) - (new Date(operation2.finishDate));
            case "stateId":
                return operation1.stateDisplayName.localeCompare(operation2.stateDisplayName);
        }
    }

    sortOperations(compare) {
        this.sortedOperations = [...this.operations];
        this.sortedOperations.sort(compare);
    }

    createParameters() {
        let result = "";

        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");
        result = utils.addParameter(result, "gate_code", "gates-select");
        result = utils.addParameter(result, "partner_id", "partners-select");
        result = utils.addParameter(result, "operation_type_code", "operation-types-select");
        result = utils.addParameter(result, "vehicle_kind_code", "vehicle-kinds-select");
        result = utils.addParameter(result, "state_code", "operation-states-select");
        result = utils.addParameter(result, "id", "id-number-input");
        result = utils.addParameter(result, "vehicle_number", "vehicle-number-input");
        result = utils.addParameter(result, "driver_name", "driver-name-input");
        result = utils.addParameter(result, "partner_action_id", "partner-action-id-input");
        result = utils.addParameter(result, "worker", "worker-input");
        result = utils.addParameter(result, "sort_field", "sort-field-select");
        result = utils.addParameterValue(result, "sort_direction", this.sortDirection);

        return result != "" ? "?" + result : "";
    }

    async getOperations() {        
        const parameters = this.createParameters();
        this.operations = await api.getOperations(parameters);
        const url = this.shortUrl + parameters;
        history.pushState({}, null, url);
    }

    clearOperations() {
        const container = document.getElementById("operations-table-body");
        container.replaceChildren();
    }

    showOperations() {
        const operationsTbody = document.getElementById("operations-table-body");
        let operationTr = null;
        if (this.sortDirection === "forward") {
            for (let operation of this.sortedOperations) {
                operationTr = document.createElement("tr");
                this.showOperation(operation, operationTr);
                operationsTbody.append(operationTr);
            }
        } else if (this.sortDirection === "reverse") {
            for (let i = this.sortedOperations.length - 1; i >= 0; i--) {
                operationTr = document.createElement("tr");
                this.showOperation(this.sortedOperations[i], operationTr);
                operationsTbody.append(operationTr);
            }
        }
    }

    async onChangeFilter() {
        this.clearOperations();
        await this.getOperations();
        this.sortOperations(this.sortCompare.bind(this));
        this.showOperations();
    }

    onChangeSortField() {
        this.sortField = document.querySelector(".select_sort").value;
        this.clearOperations();
        this.sortOperations(this.sortCompare.bind(this));
        this.showOperations();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    onChangeSortDirection() {
        const sortDirectionInputs = document.querySelectorAll("input[name='sort-input']:checked")
        if (sortDirectionInputs[0].id === "forward-sort-input") {
            this.sortDirection = "forward";
        } else if (sortDirectionInputs[0].id === "reverse-sort-input") {
            this.sortDirection = "reverse";
        }
        this.clearOperations();
        this.showOperations();

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
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
        viewUtils.initInput("id-number-input", utils.getUrlParameter("id"), this.onChangeFilter.bind(this));
        viewUtils.initInput("vehicle-number-input", utils.getUrlParameter("vehicle_number"), this.onChangeFilter.bind(this));
        viewUtils.initInput("driver-name-input", utils.getUrlParameter("driver_name"), this.onChangeFilter.bind(this));
        viewUtils.initInput("partner-action-id-input", utils.getUrlParameter("partner_action_id"), this.onChangeFilter.bind(this));
        viewUtils.initInput("worker-input", utils.getUrlParameter("worker"), this.onChangeFilter.bind(this));

        this.sortField = utils.getUrlParameter("sort_field");
        if (this.sortField === "")
            this.sortField = "driverName";
        viewUtils.initSelect("sort-field-select", this.sortField, this.onChangeSortField.bind(this));

        const sortDirectionInputs = document.querySelectorAll("input[name='sort-input']")
        for (const sortDirectionInput of sortDirectionInputs) {
            sortDirectionInput.onchange = this.onChangeSortDirection.bind(this);
        }

        this.sortDirection = utils.getUrlParameter("sort_direction");
        if (this.sortDirection != "reverse")
            this.sortDirection = "forward";
        if (this.sortDirection === "forward")
            document.getElementById("forward-sort-input").checked = true;
        else
            document.getElementById("reverse-sort-input").checked = true;
        
        await this.getOperations();
        this.sortOperations(this.sortCompare.bind(this));
        this.showOperations();

        this.getOperationTypeHint();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const operationsPage = new OperationsPage();

document.addEventListener('DOMContentLoaded', operationsPage.show.bind(operationsPage));