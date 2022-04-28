class GuardPage {
    shortUrl;

    operations = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";

        result = utils.addParameter(result, "state_code", "operation-states-select");
        result = utils.addParameter(result, "partner_id", "partners-select");
        result = utils.addParameter(result, "vehicle_number", "vehicle-number-input");
        result = utils.addParameter(result, "driver_name", "driver-name-input");

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
        const startDate = date > utils.nullDate ? date.toLocaleTimeString() + " - " + date.toLocaleDateString(): "Ожидается";
        viewUtils.createCell(operationTr, ["table-cell"], [startDate]);

        date = new Date(operation.planWorkStartDate);
        let contents = [date.toLocaleTimeString() + " - " + date.toLocaleDateString()];
        date = new Date(operation.factWorkStartDate);
        if (date > utils.nullDate) {
            contents.push(document.createElement("br"));
            const startTimeB = document.createElement("b");
            startTimeB.textContent = "(" + date.toLocaleTimeString() + " - " + date.toLocaleDateString() + ")";
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

    showOperations() {
        const operationsTbody = document.getElementById("guard-operations-tbody");
        let operationTr = null;

        const vehicleNumberInput = document.getElementById("vehicle-number-input");
        const vehicleNumber = vehicleNumberInput != undefined ? vehicleNumberInput.value.toUpperCase() : "";
        const driverNameInput = document.getElementById("driver-name-input");
        const driverName = driverNameInput != undefined ? driverNameInput.value.toUpperCase() : "";

        for (let operation of this.operations) {
            const statesSelect = document.getElementById("operation-states-select");
            if (statesSelect != undefined && statesSelect.value != "" && operation.stateCode != statesSelect.value) {
                continue;
            }
            const partnersSelect = document.getElementById("partners-select");
            if (partnersSelect != undefined && partnersSelect.value != "" && operation.partnerId != partnersSelect.value) {
                continue;
            }

            if (!operation.vehicleNumber.toUpperCase().includes(vehicleNumber)) {
                continue;
            }
            if (!operation.driverName.toUpperCase().includes(driverName)) {
                continue;
            }
            operationTr = document.createElement("tr");
            this.showOperation(operation, operationTr);
            operationsTbody.append(operationTr);
        }
    }

    onChangeFilter() {
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

        this.showOperations();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    showGuardPage() {
        location.href = this.shortUrl + this.createParameters();
    }
}

const guardPage = new GuardPage();

$(document).ready(guardPage.show.bind(guardPage));