class PresentVehiclesPage {
    shortUrl;
    operations = [];
    filteredOperations = [];
    sortedOperations = [];
    sortField = "driverName";
    sortDirection = "forward";

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";

        result = utils.addParameter(result, "operation_type_code", "operation-types-select");
        result = utils.addParameter(result, "gate_code", "gates-select");
        result = utils.addParameter(result, "state_code", "operation-states-select");
        result = utils.addParameter(result, "partner_id", "partners-select");
        result = utils.addParameter(result, "id_number", "id-number-input");
        result = utils.addParameter(result, "vehicle_number", "vehicle-number-input");
        result = utils.addParameter(result, "driver_name", "driver-name-input");
        result = utils.addParameter(result, "partner_action_id", "partner-action-id-input");
        result = utils.addParameter(result, "sort_field", "sort-field-select");
        result = utils.addParameterValue(result, "sort_direction", this.sortDirection);

        return result != "" ? "?" + result : "";
    }

    async getOperations() {
        this.operations = await api.getActiveOperations();
    }

    clearOperations() {
        const operationsTbody = document.getElementById("active-operations-tbody");
        operationsTbody.replaceChildren();
    }

    createOpenOperationFunction(id) {
        return function () {
            sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href);
            location.href = "/OperationPage?id=" + id;
        }
    }

    showOperation(operation, operationTr) {
        operationTr.classList.add("table-row");

        viewUtils.createRowHeader(operationTr, ["table-cell"], [operation.id]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.vehicleNumber]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.vehicleBrand]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.driverName]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.driverPhoneNumber]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.partnersNames]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.typeDisplayName]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.partnerActionId]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.note]);

        let date = new Date(operation.startDate);
        const startTimeB = document.createElement("b");
        startTimeB.textContent = date.toLocaleTimeString();
        let contents = [startTimeB];
        contents.push(document.createElement("br"));
        contents.push(date.toLocaleDateString());
        viewUtils.createCell(operationTr, ["table-cell"], contents);

        date = new Date(operation.planWorkStartDate);
        const planWorkStartTimeB = document.createElement("b");
        planWorkStartTimeB.textContent = date.toLocaleTimeString();
        contents = [planWorkStartTimeB];
        contents.push(" - " + date.toLocaleDateString());

        date = new Date(operation.factWorkStartDate);
        if (date >= utils.nullDate) {
            contents.push(document.createElement("br"));
            contents.push("(" + date.toLocaleTimeString() + " - " + date.toLocaleDateString() + ")");
        }
        viewUtils.createCell(operationTr, ["table-cell"], contents);

        viewUtils.createCell(operationTr, ["table-cell"], [operation.responsibleOfficer]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.gateShortName]);

        const openButton = document.createElement("button");
        openButton.classList.add("btn");
        openButton.classList.add("btn-outline-info");
        openButton.classList.add("m-3");
        openButton.classList.add("table-cell-button");
        openButton.onclick = this.createOpenOperationFunction(operation.id);
        openButton.textContent = "Открыть";
        contents = [openButton];

        const openP = document.createElement("p");
        openP.classList.add("text-center");
        openP.classList.add("p-margin-0");
        openP.textContent = operation.stateDisplayName;
        contents.push(openP);

        viewUtils.createCell(operationTr, ["table-cell"], contents,
            "background-color: " + operation.displayColor + " !important; box-shadow:none");
    }

    filterOperations() {
        const idNumberInput = document.getElementById("id-number-input");
        const idNumber = idNumberInput != undefined ? idNumberInput.value.toUpperCase() : "";

        const vehicleNumberInput = document.getElementById("vehicle-number-input");
        const vehicleNumber = vehicleNumberInput != undefined ? vehicleNumberInput.value.toUpperCase() : "";

        const driverNameInput = document.getElementById("driver-name-input");
        const driverName = driverNameInput != undefined ? driverNameInput.value.toUpperCase() : "";

        const actionSelect = document.getElementById("partner-action-id-input");
        const actionNumber = actionSelect != undefined ? actionSelect.value.toUpperCase() : "";

        this.filteredOperations = [];
        for (let operation of this.operations) {
            const operationTypeSelect = document.getElementById("operation-types-select");
            if (operationTypeSelect != undefined && operationTypeSelect.value != "" && operation.typeCode != operationTypeSelect.value) {
                continue;
            }
            const gatesSelect = document.getElementById("gates-select");
            if (gatesSelect != undefined && gatesSelect.value != "" && operation.gateCode != gatesSelect.value) {
                continue;
            }
            const statesSelect = document.getElementById("operation-states-select");
            if (statesSelect != undefined && statesSelect.value != "" && operation.stateCode != statesSelect.value) {
                continue;
            }
            const partnersSelect = document.getElementById("partners-select");
            if (partnersSelect != undefined && partnersSelect.value != "" && operation.partnerId != partnersSelect.value) {
                continue;
            }

            if (!operation.id.toString().includes(idNumber)) {
                continue;
            }

            if (!operation.vehicleNumber.toUpperCase().includes(vehicleNumber)) {
                continue;
            }
            if (!operation.driverName.toUpperCase().includes(driverName)) {
                continue;
            }
            if (!operation.partnerActionId.toUpperCase().includes(actionNumber)) {
                continue;
            }

            this.filteredOperations.push(operation);
        }
    }

    sortCompare(operation1, operation2) {
        switch (this.sortField) {
            case "driverName":
                return operation1.driverName.localeCompare(operation2.driverName);
            case "id":
                return operation1.id - operation2.id;
            case "partnerId":
                return operation1.partnerId.localeCompare(operation2.partnerId);
            case "vehicleBrand":
                return operation1.vehicleBrand.localeCompare(operation2.vehicleBrand);
            case "vehicleNumber":
                return operation1.vehicleNumber.localeCompare(operation2.vehicleNumber);
            case "gateCode":
                return operation1.gateCode.localeCompare(operation2.gateCode);
            case "planWorkStartDate":
                return (new Date(operation1.planWorkStartDate)) - (new Date(operation2.planWorkStartDate));
            case "factWorkStartDate":
                return (new Date(operation1.factWorkStartDate)) - (new Date(operation2.factWorkStartDate));
            case "responsibleOfficerName":
                return operation1.responsibleOfficer.localeCompare(operation2.responsibleOfficer);
            case "stateId":
                return operation1.stateId - operation2.stateId;
        }
    }

    sortOperations(compare) {
        this.sortedOperations = [...this.filteredOperations];
        this.sortedOperations.sort(compare);
    }

    showOperations() {
        const operationsTbody = document.getElementById("active-operations-tbody");
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

    onChangeFilter() {
        this.clearOperations();
        this.filterOperations();
        this.sortOperations(this.sortCompare.bind(this));
        this.showOperations();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    // обработчик события при смене поля сортировки
    onChangeSortField() {
        this.sortField = document.querySelector(".select_sort").value;
        this.clearOperations();
        this.sortOperations(this.sortCompare.bind(this));
        this.showOperations();

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    // обработчик события при смене "направления" сортировки
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
        this.onChangeFilter();
        this.getOperationTypeHint();
    }

    async show() {
        await this.getOperations();

        viewUtils.initSelect("operation-types-select", utils.getUrlParameter("operation_type_code"), this.onChangeOperationType.bind(this));
        viewUtils.initSelect("gates-select", utils.getUrlParameter("gate_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("operation-states-select", utils.getUrlParameter("state_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("partners-select", utils.getUrlParameter("partner_id"), this.onChangeFilter.bind(this));
        viewUtils.initInput("id-number-input", utils.getUrlParameter("id_number"), this.onChangeFilter.bind(this));
        viewUtils.initInput("vehicle-number-input", utils.getUrlParameter("vehicle_number"), this.onChangeFilter.bind(this));
        viewUtils.initInput("driver-name-input", utils.getUrlParameter("driver_name"), this.onChangeFilter.bind(this));
        viewUtils.initInput("partner-action-id-input", utils.getUrlParameter("partner_action_id"), this.onChangeFilter.bind(this));

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

        this.filterOperations();
        this.sortOperations(this.sortCompare.bind(this));
        this.showOperations();
        const header = document.getElementById("header1");
        header.textContent = header.textContent + " (всего машин - " + this.operations.length + ")";
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const presentVehiclesPage = new PresentVehiclesPage();

document.addEventListener('DOMContentLoaded', presentVehiclesPage.show.bind(presentVehiclesPage));