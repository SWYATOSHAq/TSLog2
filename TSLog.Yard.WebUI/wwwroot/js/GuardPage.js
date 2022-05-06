class GuardPage {
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

        result = utils.addParameter(result, "state_code", "operation-states-select");
        result = utils.addParameter(result, "partner_id", "partners-select");
        result = utils.addParameter(result, "vehicle_number", "vehicle-number-input");
        result = utils.addParameter(result, "driver_name", "driver-name-input");
        result = utils.addParameter(result, "sort_field", "sort-field-select");
        result = utils.addParameterValue(result, "sort_direction", this.sortDirection);

        return result != "" ? "?" + result : "";
    }

    async getOperations() {
        this.operations = await api.getGuardOperations();
    }

    clearOperations() {
        const operationsTbody = document.getElementById("guard-operations-tbody");
        operationsTbody.replaceChildren();
    }

    showOperation(operation, operationTr) {
        operationTr.classList.add("table-row");

        viewUtils.createCell(operationTr, ["table-cell"], [operation.vehicleBrand]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.vehicleNumber]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.driverName]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.partnersNames]);
        viewUtils.createCell(operationTr, ["table-cell"], [operation.directionName]);

        let date = new Date(operation.startDate);
        const startDate = date > utils.nullDate ? date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + date.toLocaleDateString(): "Ожидается";
        viewUtils.createCell(operationTr, ["table-cell"], [startDate]);

        date = new Date(operation.planWorkStartDate);
        let contents = [date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + date.toLocaleDateString()];
        date = new Date(operation.factWorkStartDate);
        if (date > utils.nullDate) {
            contents.push(document.createElement("br"));
            const startTimeB = document.createElement("b");
            startTimeB.textContent = "(" + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + date.toLocaleDateString() + ")";
            contents.push(startTimeB)
        }
        viewUtils.createCell(operationTr, ["table-cell"], contents);

        viewUtils.createCell(operationTr, ["table-cell"], [operation.responsibleOfficer]);

        const passVehicleP = document.createElement("p");
        passVehicleP.classList.add("text-center");
        passVehicleP.classList.add("p-margin-0");
        passVehicleP.textContent = operation.stateDisplayName;

        const passVehicleButton = document.createElement("button");
        passVehicleButton.classList.add("btn");
        passVehicleButton.classList.add("btn-outline-info");
        passVehicleButton.classList.add("m-3");
        passVehicleButton.classList.add("table-cell-button");
        if (operation.stateCode == "CONFIRMED") {
            passVehicleButton.onclick = this.createLetVehicleIn(operation.id);
            passVehicleButton.textContent = "Впустить";
        } else if (operation.stateCode = "WORK_FINISHED") {
            passVehicleButton.onclick = this.createReleaseVehicle(operation.id);
            passVehicleButton.textContent = "Выпустить";
        } else {
            passVehicleButton.onclick = null;
            passVehicleButton.textContent = "?";
        }
        viewUtils.createCell(operationTr, ["table-cell"], [passVehicleP, passVehicleButton],
            "background-color: " + operation.displayColor + " !important; box-shadow:none");
    }

    filterOperations() {
        const statesSelect = document.getElementById("operation-states-select");
        const state = statesSelect != undefined ? statesSelect.value : "";

        const partnersSelect = document.getElementById("partners-select");
        const partner = partnersSelect != undefined ? partnersSelect.value : "";

        const vehicleNumberInput = document.getElementById("vehicle-number-input");
        const vehicleNumber = vehicleNumberInput != undefined ? vehicleNumberInput.value.toUpperCase() : "";

        const driverNameInput = document.getElementById("driver-name-input");
        const driverName = driverNameInput != undefined ? driverNameInput.value.toUpperCase() : "";

        this.filteredOperations = [];
        for (let operation of this.operations) {

            if (state != "" && operation.stateCode != state ||
                partner != "" && operation.partnerId != partner ||
                !operation.vehicleNumber.toUpperCase().includes(vehicleNumber) ||
                !operation.driverName.toUpperCase().includes(driverName))
            {
                continue;
            }            
            this.filteredOperations.push(operation);
        }
    }

    sortCompare(operation1, operation2) {
        switch (this.sortField) {
            case "driverName":
                return operation1.driverName.localeCompare(operation2.driverName);
            case "partnerId":
                return operation1.partnerId.localeCompare(operation2.partnerId);
            case "vehicleBrand":
                return operation1.vehicleBrand.localeCompare(operation2.vehicleBrand);
            case "vehicleNumber":
                return operation1.vehicleNumber.localeCompare(operation2.vehicleNumber);
            case "startDate":
                return (new Date(operation1.startDate)) - (new Date(operation2.startDate));
            case "planWorkStartDate":
                return (new Date(operation1.planWorkStartDate)) - (new Date(operation2.planWorkStartDate));
            case "factWorkStartDate":
                return (new Date(operation1.factWorkStartDate)) - (new Date(operation2.factWorkStartDate));
            case "responsibleOfficerName":
                return operation1.responsibleOfficer.localeCompare(operation2.responsibleOfficer);
        }
    }

    sortOperations(compare) {
        this.sortedOperations = [...this.filteredOperations];
        this.sortedOperations.sort(compare);
    }

    showOperations() {
        const operationsTbody = document.getElementById("guard-operations-tbody");
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

    // обработчик события при смене фильтрации
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

    passVehicle(id, action) {
        if (action == "START_OPERATION") {
            api.startOperation(id)
        }
        else if (action == "FINISH_OPERATION") {
            api.finishOperation(id);
        }
        this.showGuardPage();
    }

    createLetVehicleIn(id) {
        return function () {
            guardPage.passVehicle(id, "START_OPERATION");
        }        
    }

    createReleaseVehicle(id) {
        return function () {
            guardPage.passVehicle(id, "FINISH_OPERATION");
        }        
    }

    async show() {
        await this.getOperations();        

        viewUtils.initSelect("operation-states-select", utils.getUrlParameter("state_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("partners-select", utils.getUrlParameter("partner_id"), this.onChangeFilter.bind(this));
        viewUtils.initInput("vehicle-number-input", utils.getUrlParameter("vehicle_number"), this.onChangeFilter.bind(this));
        viewUtils.initInput("driver-name-input", utils.getUrlParameter("driver_name"), this.onChangeFilter.bind(this));

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
        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    showGuardPage() {
        location.href = this.shortUrl + this.createParameters();
    }
}

const guardPage = new GuardPage();

document.addEventListener('DOMContentLoaded', guardPage.show.bind(guardPage));