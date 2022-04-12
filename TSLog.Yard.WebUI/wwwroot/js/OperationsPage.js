class OperationsPage {
    shortUrl;

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

    showOperation(card) {
        const operationOuterDiv = document.createElement("div");
        $(operationOuterDiv).addClass("col operation-outer-div");

        const operationDiv = document.createElement("div");
        $(operationDiv).addClass("col operation-div");

        const cardHeader = document.createElement("div");
        $(cardHeader).addClass("card-header");
        $(cardHeader).css("background-color", card.DisplayColor);
        $(cardHeader).text(utils.convertTime(card.PlanWorkStartDate) + " - " + utils.convertTime(card.PlanWorkFinishDate) + " (" + utils.convertDate(card.PlanWorkFinishDate) + ")");

        const cardBody = document.createElement("div");
        $(cardBody).addClass("card-body");

        const cardBodyMndnr = document.createElement("h5");
        $(cardBodyMndnr).addClass("card-title");
        $(cardBodyMndnr).text(this.getPartnersNames(card));

        const cardBodyInputType = document.createElement("p");
        $(cardBodyInputType).addClass("card-text block-size-2");
        $(cardBodyInputType).text(card.Type.DisplayName);

        const cardBodyGate = document.createElement("p");
        $(cardBodyGate).addClass("card-text  text-primary font-1-3");
        $(cardBodyGate).text((card.Gate.DisplayName == "NULL" ? "не проставлены" : card.Gate.DisplayName));

        const cardBodyState = document.createElement("p");
        $(cardBodyState).addClass("card-text");
        $(cardBodyState).text(card.State.DisplayName);

        const cardBodyVehicleKind = document.createElement("p");
        $(cardBodyVehicleKind).addClass("card-text block-size-2");
        $(cardBodyVehicleKind).text(card.VehicleKind.DisplayName);

        const cardBodyVehicleNumber = document.createElement("p");
        $(cardBodyVehicleNumber).addClass("card-text");
        $(cardBodyVehicleNumber).text(card.VehicleNumber);

        const cardBodyDriverName = document.createElement("p");
        $(cardBodyDriverName).addClass("card-text block-size-2");
        $(cardBodyDriverName).text(card.DriverName);

        cardBody.append(cardBodyMndnr);
        cardBody.append(cardBodyInputType);
        cardBody.append(cardBodyGate);
        cardBody.append(cardBodyState);
        cardBody.append(cardBodyVehicleKind);
        cardBody.append(cardBodyVehicleNumber);
        cardBody.append(cardBodyDriverName);

        const cardBodyButton = document.createElement("h5");
        $(cardBodyButton).addClass("btn btn-outline-info m-3 open-operation-button");
        $(cardBodyButton).text("Открыть");
        $(cardBodyButton).click(function () {
            const url = "OperationPage?id=" + card.Id;
            location.href = url;
        });

        operationDiv.append(cardHeader);
        operationDiv.append(cardBody);
        operationDiv.append(cardBodyButton);

        operationOuterDiv.append(operationDiv);

        const container = document.getElementById("operations-container");
        container.appendChild(operationOuterDiv);
    }

    createParameters() {
        const fromDatePicker = document.getElementById("from-date-picker");
        let result = "?from_date=" + fromDatePicker.value;

        const toDatePicker = document.getElementById("to-date-picker");
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

        const vehicleNumberInput = filters.querySelector("#vehicle-number-input");
        if (vehicleNumberInput != undefined && vehicleNumberInput.value != "") {
            result = result + "&vehicle_number=" + vehicleNumberInput.value;
        }

        const driverNameInput = filters.querySelector("#driver-name-input");
        if (driverNameInput != undefined && driverNameInput.value != "") {
            result = result + "&driver_name=" + driverNameInput.value;
        }

        return result;
    }

    async getOperations() {
        const parameters = this.createParameters();
        let url = "./api/v1/yard/Operations" + parameters;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const operations = await response.json();
            this.clearOperations();
            for (let operation of operations) {
                console.log(operation);
                this.showOperation(operation);
            }
        }
        else {
            const message = await response.text();
            console.log(message);
        }
        url = this.shortUrl + parameters;
        history.pushState({}, null, url);
    }

    clearOperations() {
        const container = document.getElementById("operations-container");
        container.replaceChildren();
    }

    async onChangeFilter() {
        await this.getOperations();
    }

    getOperationTypeHint() {
        const select = document.getElementById("operation-types-select");
        if (select.selectedIndex >= 0) {
            select.title = select.options[select.selectedIndex].getAttribute("note");
        }        
    }

    async onChangeOperationType() {
        await this.getOperations();
        this.getOperationTypeHint();        
    }

    show() {
        const filters = document.getElementById("filters");
        const fromDatePicker = $("#from-date-picker");
        if (fromDatePicker != undefined) {
            fromDatePicker.datepicker({
                format: "dd.mm.yyyy",
                autoclose: true,
                language: "ru",
                todayBtn: true,
                todayHighlight: true,
            });
            const fromDate = utils.getUrlParameter("from_date");
            if (fromDate == "")
                fromDate = utils.getNowDate();
            fromDatePicker.datepicker("setDate", fromDate);
            fromDatePicker.on("changeDate", this.onChangeFilter.bind(this));
        }
        const toDatePicker = $("#to-date-picker");
        if (toDatePicker != undefined) {
            toDatePicker.datepicker({
                format: "dd.mm.yyyy",
                autoclose: true,
                language: "ru",
                todayBtn: true,
                todayHighlight: true,
            });
            const toDate = utils.getUrlParameter("to_date");
            if (toDate == "")
                toDate = utils.getNowDate();
            toDatePicker.datepicker("setDate", toDate);
            toDatePicker.on("changeDate", this.onChangeFilter.bind(this));
        }

        const gatesSelect = filters.querySelector("#gates-select");
        if (gatesSelect != undefined)
            gatesSelect.onchange = this.onChangeFilter.bind(this);

        const partnersSelect = filters.querySelector("#partners-select");
        if (partnersSelect != undefined)
            partnersSelect.onchange = this.onChangeFilter.bind(this);

        const operationTypesSelect = filters.querySelector("#operation-types-select");
        if (operationTypesSelect != undefined)
            operationTypesSelect.onchange = this.onChangeOperationType.bind(this);

        const vehicleKindsSelect = filters.querySelector("#vehicle-kinds-select");
        if (vehicleKindsSelect != undefined)
            vehicleKindsSelect.onchange = this.onChangeFilter.bind(this);

        const operationStatesSelect = filters.querySelector("#operation-states-select");
        if (operationStatesSelect != undefined)
            operationStatesSelect.onchange = this.onChangeFilter.bind(this);

        const vehicleNumberInput = filters.querySelector("#vehicle-number-input");
        if (vehicleNumberInput != undefined)
            vehicleNumberInput.onchange = this.onChangeFilter.bind(this);

        const driverNameInput = filters.querySelector("#driver-name-input");
        if (driverNameInput != undefined)
            driverNameInput.onchange = this.onChangeFilter.bind(this);

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        this.getOperationTypeHint();
    }
}

const operationsPage = new OperationsPage();

$(document).ready(operationsPage.show.bind(operationsPage));