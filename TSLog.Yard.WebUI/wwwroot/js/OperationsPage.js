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

    showOperation(block) {
        const operationOuterDiv = document.createElement("div");
        $(operationOuterDiv).addClass("col operation-outer-div");

        const operationDiv = document.createElement("div");
        $(operationDiv).addClass("col operation-div");

        const cardHeader = document.createElement("div");
        $(cardHeader).addClass("card-header");
        $(cardHeader).css("background-color", block.DisplayColor);
        $(cardHeader).text(utils.convertTime(block.PlanWorkStartDate) + " - " + utils.convertTime(block.PlanWorkFinishDate) + " (" + utils.convertDate(block.PlanWorkFinishDate) + ")");

        const cardBody = document.createElement("div");
        $(cardBody).addClass("card-body");

        const cardBodyMndnr = document.createElement("h5");
        $(cardBodyMndnr).addClass("card-title");
        $(cardBodyMndnr).text(this.getPartnersNames(block));

        const cardBodyInputType = document.createElement("p");
        $(cardBodyInputType).addClass("card-text block-size-2");
        $(cardBodyInputType).text(block.Type.DisplayName);

        const cardBodyGate = document.createElement("p");
        $(cardBodyGate).addClass("card-text  text-primary font-1-3");
        $(cardBodyGate).text((block.Gate.DisplayName == "NULL" ? "не проставлены" : block.Gate.DisplayName));

        const cardBodyState = document.createElement("p");
        $(cardBodyState).addClass("card-text");
        $(cardBodyState).text(block.State.DisplayName);

        const cardBodyVehicleKind = document.createElement("p");
        $(cardBodyVehicleKind).addClass("card-text block-size-2");
        $(cardBodyVehicleKind).text(block.VehicleKind.DisplayName);

        const cardBodyVehicleNumber = document.createElement("p");
        $(cardBodyVehicleNumber).addClass("card-text");
        $(cardBodyVehicleNumber).text(block.VehicleNumber);

        const cardBodyDriverName = document.createElement("p");
        $(cardBodyDriverName).addClass("card-text block-size-2");
        $(cardBodyDriverName).text(block.DriverName);

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
        $(cardBodyButton).click(this.createOpenOperation(block.Id).bind(this));

        operationDiv.append(cardHeader);
        operationDiv.append(cardBody);
        operationDiv.append(cardBodyButton);

        operationOuterDiv.append(operationDiv);

        const container = document.getElementById("operations-container");
        container.appendChild(operationOuterDiv);
    }

    createParameters() {
        let result = "";

        result = utils.addParameter(result, "from_date", "from-date-picker");
        result = utils.addParameter(result, "to_date", "to-date-picker");
        result = utils.addParameter(result, "operation_type_code", "operation-types-select");
        result = utils.addParameter(result, "vehicle_kind_code", "vehicle-kinds-select");
        result = utils.addParameter(result, "gate_code", "gates-select");
        result = utils.addParameter(result, "state_code", "operation-states-select");
        result = utils.addParameter(result, "partner_id", "partners-select");
        result = utils.addParameter(result, "vehicle_number", "vehicle-number-input");
        result = utils.addParameter(result, "driver_name", "driver-name-input");
        result = utils.addParameter(result, "partner_action_id", "partner-action-id-input");

        return result != "" ? "?" + result : "";
    }

    async getOperations() {
        const url = "./api/v1/yard/Operations" + this.createParameters();
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
    }

    clearOperations() {
        const container = document.getElementById("operations-container");
        container.replaceChildren();
    }

    async onChangeFilter() {
        await this.getOperations();
        const url = this.shortUrl + this.createParameters();;
        history.pushState({}, null, url);
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
        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = fromDate;
        viewUtils.initDatePicker("from-date-picker", fromDate, this.onChangeFilter.bind(this));
        viewUtils.initDatePicker("to-date-picker", toDate, this.onChangeFilter.bind(this));

        viewUtils.initSelect("operation-types-select", utils.getUrlParameter("operation_type_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("vehicle-kinds-select", utils.getUrlParameter("vehicle_kind_code"), this.onChangeOperationType.bind(this));
        viewUtils.initSelect("gates-select", utils.getUrlParameter("gate_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("operation-states-select", utils.getUrlParameter("state_code"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("partners-select", utils.getUrlParameter("partner_id"), this.onChangeFilter.bind(this));
        viewUtils.initInput("vehicle-number-input", utils.getUrlParameter("vehicle_number"), this.onChangeFilter.bind(this));
        viewUtils.initInput("driver-name-input", utils.getUrlParameter("driver_name"), this.onChangeFilter.bind(this));
        viewUtils.initInput("partner-action-id-input", utils.getUrlParameter("partner_action_id"), this.onChangeFilter.bind(this));

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

        this.getOperationTypeHint();
    }
}

const operationsPage = new OperationsPage();

document.addEventListener('DOMContentLoaded', operationsPage.show.bind(operationsPage));