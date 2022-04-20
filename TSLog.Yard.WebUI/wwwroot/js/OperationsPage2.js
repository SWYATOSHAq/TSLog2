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

    showOperation(operation) {
        const operationOuterDiv = document.createElement("div");
        

        const container = document.getElementById("operations-container");
        container.appendChild(operationOuterDiv);

        const tableBody = document.getElementById("operations-table-body");
        tableBody.appendChild(row);
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
        const fromDatePicker = $("#from-date-picker");
        if (fromDatePicker != undefined) {
            fromDatePicker.datepicker({
                format: "dd.mm.yyyy",
                autoclose: true,
                language: "ru",
                todayBtn: true,
                todayHighlight: true,
            });
            let fromDate = utils.getUrlParameter("from_date");
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
            let toDate = utils.getUrlParameter("to_date");
            if (toDate == "")
                toDate = utils.getNowDate();
            toDatePicker.datepicker("setDate", toDate);
            toDatePicker.on("changeDate", this.onChangeFilter.bind(this));
        }

        const gatesSelect = document.getElementById("gates-select");
        if (gatesSelect != undefined)
            gatesSelect.onchange = this.onChangeFilter.bind(this);

        const partnersSelect = document.getElementById("partners-select");
        if (partnersSelect != undefined)
            partnersSelect.onchange = this.onChangeFilter.bind(this);

        const operationTypesSelect = document.getElementById("operation-types-select");
        if (operationTypesSelect != undefined)
            operationTypesSelect.onchange = this.onChangeOperationType.bind(this);

        const vehicleKindsSelect = document.getElementById("vehicle-kinds-select");
        if (vehicleKindsSelect != undefined)
            vehicleKindsSelect.onchange = this.onChangeFilter.bind(this);

        const operationStatesSelect = document.getElementById("operation-states-select");
        if (operationStatesSelect != undefined)
            operationStatesSelect.onchange = this.onChangeFilter.bind(this);

        const vehicleNumberInput = document.getElementById("vehicle-number-input");
        if (vehicleNumberInput != undefined)
            vehicleNumberInput.onchange = this.onChangeFilter.bind(this);

        const driverNameInput = document.getElementById("driver-name-input");
        if (driverNameInput != undefined)
            driverNameInput.onchange = this.onChangeFilter.bind(this);

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        // TODO: set filters
        await this.getOperations();

        this.getOperationTypeHint();
    }
}

const operationsPage = new OperationsPage();

$(document).ready(operationsPage.show.bind(operationsPage));