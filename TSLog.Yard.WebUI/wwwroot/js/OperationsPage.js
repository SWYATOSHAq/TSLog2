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

        const filters = document.getElementById("filters");
        const gatesDropdown = filters.querySelector("#gates-dropdown");
        if (gatesDropdown != undefined && gatesDropdown.value != "") {
            result = result + "&gate_code=" + gatesDropdown.value;
        }

        const partnersDropdown = filters.querySelector("#partners-dropdown");
        if (partnersDropdown != undefined && partnersDropdown.value != "") {
            result = result + "&partner_id=" + partnersDropdown.value;
        }

        const operationTypesDropdown = filters.querySelector("#operation-types-dropdown");
        if (operationTypesDropdown != undefined && operationTypesDropdown.value != "") {
            result = result + "&operation_type_code=" + operationTypesDropdown.value;
        }

        const vehicleKindsDropdown = filters.querySelector("#vehicle-kinds-dropdown");
        if (vehicleKindsDropdown != undefined && vehicleKindsDropdown.value != "") {
            result = result + "&vehicle_kind_code=" + vehicleKindsDropdown.value;
        }

        const operationStatesDropdown = filters.querySelector("#operation-states-dropdown");
        if (operationStatesDropdown != undefined && operationStatesDropdown.value != "") {
            result = result + "&state_code=" + operationStatesDropdown.value;
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
        url = window.location.href.split('?')[0] + parameters;
        history.pushState({}, null, url);
    }

    clearOperations() {
        const container = document.getElementById("operations-container");
        container.replaceChildren();
    }

    onChangeFilter() {
        this.getOperations();
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

        const gatesDropdown = filters.querySelector("#gates-dropdown");
        if (gatesDropdown != undefined)
            gatesDropdown.onchange = this.onChangeFilter.bind(this);

        const partnersDropdown = filters.querySelector("#partners-dropdown");
        if (partnersDropdown != undefined)
            partnersDropdown.onchange = this.onChangeFilter.bind(this);

        const operationTypesDropdown = filters.querySelector("#operation-types-dropdown");
        if (operationTypesDropdown != undefined)
            operationTypesDropdown.onchange = this.onChangeFilter.bind(this);

        const vehicleKindsDropdown = filters.querySelector("#vehicle-kinds-dropdown");
        if (vehicleKindsDropdown != undefined)
            vehicleKindsDropdown.onchange = this.onChangeFilter.bind(this);

        const operationStatesDropdown = filters.querySelector("#operation-states-dropdown");
        if (operationStatesDropdown != undefined)
            operationStatesDropdown.onchange = this.onChangeFilter.bind(this);

        const vehicleNumberInput = filters.querySelector("#vehicle-number-input");
        if (vehicleNumberInput != undefined)
            vehicleNumberInput.onchange = this.onChangeFilter.bind(this);

        const driverNameInput = filters.querySelector("#driver-name-input");
        if (driverNameInput != undefined)
            driverNameInput.onchange = this.onChangeFilter.bind(this);

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const operationsPage = new OperationsPage();

$(document).ready(operationsPage.show.bind(operationsPage));