class PresentVehiclesPage {
    shortUrl;

    operations = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    async getOperations() {
        this.operations = await api.getActiveOperations();
    }

    clearOperations() {
        const operationsTbody = document.getElementById("active-operations-tbody");
        this.operations = [];
        operationsTbody.replaceChildren();
    }

    createOpenOperationFunction(id) {
        return function () {
            location.href = "/OperationPage?id=" + id;
        }
    }

    showOperation(index, operation, operationTr) {
        operationTr.classList.add("table-row");

        const serialNumberTh = document.createElement("th");
        serialNumberTh.classList.add("table-cell");
        serialNumberTh.textContent = index.toString();
        operationTr.append(serialNumberTh);

        const vehicleNumberTd = document.createElement("td");
        vehicleNumberTd.classList.add("table-cell");
        vehicleNumberTd.textContent = operation.vehicleNumber;
        operationTr.append(vehicleNumberTd);

        const vehicleBrandTd = document.createElement("td");
        vehicleBrandTd.classList.add("table-cell");
        vehicleBrandTd.textContent = operation.vehicleBrand;
        operationTr.append(vehicleBrandTd);

        const driverNameTd = document.createElement("td");
        driverNameTd.classList.add("table-cell");
        driverNameTd.textContent = operation.driverName;
        operationTr.append(driverNameTd);

        const partnersNamesTd = document.createElement("td");
        partnersNamesTd.classList.add("table-cell");
        partnersNamesTd.textContent = operation.partnersNames;
        operationTr.append(partnersNamesTd);

        const typeTd = document.createElement("td");
        typeTd.classList.add("table-cell");
        typeTd.textContent = operation.typeDisplayName;
        operationTr.append(typeTd);

        const partnerActionIdTd = document.createElement("td");
        partnerActionIdTd.classList.add("table-cell");
        partnerActionIdTd.textContent = operation.partnerActionId;
        operationTr.append(partnerActionIdTd);

        const noteTd = document.createElement("td");
        noteTd.classList.add("table-cell");
        noteTd.textContent = operation.note;
        operationTr.append(noteTd);

        let date = new Date(operation.startDate);
        const startDate = date.toLocaleDateString();
        const startTime = date.toLocaleTimeString();

        const startDateTd = document.createElement("td");
        startDateTd.classList.add("table-cell");
        const startTimeB = document.createElement("b");
        startTimeB.textContent = startTime;
        startDateTd.append(startTimeB);
        const startDateBr = document.createElement("br");
        startDateTd.append(startDateBr);
        startDateTd.append(startDate);
        operationTr.append(startDateTd);


        /*        
        <td class="table-cell">
            <b>@planWorkStartTime</b> - @planWorkStartDate
            @if (!string.IsNullOrWhiteSpace(factWorkStartDateTime))
            {
                <br />
                @factWorkStartDateTime
            }
        </td>
        */       

        const workStartDateTd = document.createElement("td");
        workStartDateTd.classList.add("table-cell");

        date = new Date(operation.planWorkStartDate);
        const planWorkStartTimeB = document.createElement("b");
        planWorkStartTimeB.textContent = date.toLocaleTimeString();
        workStartDateTd.append(planWorkStartTimeB);
        workStartDateTd.append(" - " + date.toLocaleDateString());

        date = new Date(operation.factWorkStartDate);
        if (date >= utils.nullDate) {
            const factWorkStartDateTimeBr = document.createElement("br");
            workStartDateTd.append(factWorkStartDateTimeBr);
            workStartDateTd.append("(" + date.toLocaleTimeString() + " - " + date.toLocaleDateString() + ")");
        }

        operationTr.append(workStartDateTd);

        const responsibleOfficerTd = document.createElement("td");
        responsibleOfficerTd.classList.add("table-cell");
        responsibleOfficerTd.textContent = operation.responsibleOfficer;
        operationTr.append(responsibleOfficerTd);

        const gateTd = document.createElement("td");
        gateTd.classList.add("table-cell");
        gateTd.textContent = operation.gateShortName;
        operationTr.append(gateTd);

        const openTd = document.createElement("td");
        openTd.classList.add("table-cell");
        openTd.style.cssText = "background-color: " + operation.displayColor + " !important; box-shadow:none";

        const openButton = document.createElement("button");
        openButton.classList.add("btn");
        openButton.classList.add("btn-outline-info");
        openButton.classList.add("m-3");
        openButton.classList.add("table-cell-button");
        openButton.onclick = this.createOpenOperationFunction(operation.id);
        openButton.textContent = "Открыть";
        openTd.append(openButton);

        const openP = document.createElement("p");
        openP.classList.add("text-center");
        openP.classList.add("p-margin-0");
        openP.textContent = operation.stateDisplayName;
        openTd.append(openP);

        operationTr.append(openTd);
    }

    showOperations() {
        const operationsTbody = document.getElementById("active-operations-tbody");
        let operationTr = null;
        let index = 0;
        for (let operation of this.operations) {
            operationTr = document.createElement("tr");
            index++;
            this.showOperation(index, operation, operationTr);
            operationsTbody.append(operationTr);
        }        
    }

    onChangeFilter() {
        //
    }

    async show() {
        await this.getOperations();
        this.showOperations();
    }
}

const presentVehiclesPage = new PresentVehiclesPage();

$(document).ready(presentVehiclesPage.show.bind(presentVehiclesPage));