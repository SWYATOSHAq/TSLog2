class OperationPrintPage {
    id = -1;
    stateCode = "";
    stateName = "";
    startDate = utils.nullDate;
    planWorkStartDate = utils.nullDate;
    factWorkStartDate = utils.nullDate;
    planWorkFinishDate = utils.nullDate;
    factWorkFinishDate = utils.nullDate;
    planDuration = 0;
    factDuration = 0;
    planLaborCost = 0;
    planLinearLaborCost = 0;
    factLaborCost = 0;
    factBcLaborCost = 0;
    finishDate = utils.nullDate;

    selectedWarehouseActions = {};
    selectedWorkersIds = new Set([]);

    getKind() {
        const operationTypesSelect = document.getElementById("operation-types-select");
        return operationTypesSelect.options[operationTypesSelect.selectedIndex].getAttribute("direction");
    }

    showDate(planDate, factDate, input) {
        if (planDate > utils.nullDate) {
            input.value = utils.dateTimeToString(planDate);
            if (factDate > utils.nullDate) {
                input.value = input.value + " (факт. " + utils.dateTimeToString(factDate) + ")";
            }
        } else {
            input.value = "<Не указано>";
        }
    }

    showWorkStartFinish() {
        this.showDate(this.planWorkStartDate, this.factWorkStartDate, document.getElementById("work-start-input"))
        this.showDate(this.planWorkFinishDate, this.factWorkFinishDate, document.getElementById("work-finish-input"))
    }    

    showStartFinish() {
        const startFinishInput = document.getElementById("start-finish-input");
        startFinishInput.value = "";
        if (this.startDate > utils.nullDate) {
            startFinishInput.value = "Прибыла - " + utils.dateTimeToString(this.startDate);
        }
        if (this.finishDate > utils.nullDate) {
            startFinishInput.value = startFinishInput.value + " Убыла - " + utils.dateTimeToString(this.finishDate);
        }
    }

    showDuration() {
        const durationInput = document.getElementById("duration-input");
        if (this.planDuration <= 0)
            durationInput.value = "<Не определено>";
        else {
            durationInput.value = utils.secondsToString(this.planDuration);
            if (this.factDuration > 0) {
                durationInput.value = durationInput.value + " (факт. " + utils.secondsToString(this.factDuration) + ")"
            }
        }
    }

    async showLaborCost() {
        const laborCostInput = document.getElementById("labor-cost-input");
        if (this.planLaborCost <= 0)
            laborCostInput.value = "<Не определено>";
        else {
            laborCostInput.value = "План - " + (this.planLaborCost / 60).toFixed();
            if (this.planLinearLaborCost > 0) {
                laborCostInput.value = laborCostInput.value + ", только линейные - " + (this.planLinearLaborCost / 60).toFixed();
            }
            if (this.factLaborCost > 0) {
                laborCostInput.value = laborCostInput.value + ", факт - " + (this.factLaborCost / 60).toFixed();
            }
            if (this.factBcLaborCost > 0) {
                laborCostInput.value = laborCostInput.value + ", факт БК - " + (this.factBcLaborCost / 60).toFixed();
            }            
        }
    }

    // #region Warehouse actions
    async getSelectedWarehouseActions() {
        const partnersSelect = document.getElementById("partners-select");
        partnersSelect.value = "";
        const result = await api.getSelectedWarehouseActions(this.id);
        if (result.code === 0) {
            for (let action of result.data) {
                this.selectedWarehouseActions[action.id] = action;
                if (partnersSelect.value === "") {
                    partnersSelect.value = action.partnerId;
                }
            }
        }
        else {
            this.selectedWarehouseActions = {};
            viewUtils.showError(result.message);
        }
    }

    showWarehouseAction(action, button, direction) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left !important; width: 100%";
        button.dataset.actionId = action.id;
        button.dataset.actionPartnerId = action.partnerId;
        button.dataset.actionPartnerName = action.partnerName;
        button.dataset.actionPlanActionDate = action.planActionDate;

        const workerFullNameB = document.createElement("b");
        workerFullNameB.textContent = action.partnerActionId;
        button.append(workerFullNameB);

        button.append(" (" + utils.dateToString(utils.stringToDate(action.planActionDate)) + ") ");

        button.onclick = () => {
            if (direction === "ADD") {
                this.onWarehouseActionClick(button);
            } else if (direction === "REMOVE") {
                this.onSelectedWarehouseActionClick(button);
            } else {
                alert("Unknown direction " + direction);
            }
        }
    }

    showSelectedWarehouseActions() {
        const selectedActionsDiv = document.getElementById("selected-actions-div");
        selectedActionsDiv.replaceChildren();
        for (let id in this.selectedWarehouseActions) {
            const button = document.createElement("button");
            this.showWarehouseAction(this.selectedWarehouseActions[id], button, "REMOVE");
            selectedActionsDiv.append(button);
        }
    }

    showWarehouseActionsDivs() {
        let actionsDiv = document.getElementById("all-actions-div");
        if (actionsDiv != undefined) {
            if (actionsDiv.childElementCount < 5) {
                actionsDiv.style.cssText = "width: 49%; height: fit-content; max-height: 180px; padding: 0; margin: 0; margin-right: 2%";
            } else {
                actionsDiv.style.cssText = "width: 49%; height: fit-content; max-height: 180px; overflow-y: scroll; padding: 0; margin: 0; margin-right: 2%";
            }
        }

        actionsDiv = document.getElementById("selected-actions-div");
        if (actionsDiv != undefined) {
            if (actionsDiv.childElementCount < 5) {
                actionsDiv.style.cssText = "width: 49%; height: fit-content; max-height: 180px; padding: 0; margin: 0";
            } else {
                actionsDiv.style.cssText = "width: 49%; height: fit-content; max-height: 180px; overflow-y: scroll; padding: 0; margin: 0";
            }
        }
    }
    // #endregion

    // #region Workers
    showWorkersDivs() {
        let workersDiv = document.getElementById("linear-workers-div");
        if (workersDiv.childElementCount < 8) {
            workersDiv.style.cssText = "width: 49%; height: fit-content; max-height: 320px; padding: 0; margin: 0; margin-right: 2%";
        } else {
            workersDiv.style.cssText = "width: 49%; height: fit-content; max-height: 320px; overflow-y: scroll; padding: 0; margin: 0; margin-right: 2%";
        }

        workersDiv = document.getElementById("selected-workers-div");
        if (workersDiv.childElementCount < 8) {
            workersDiv.style.cssText = "width: 49%; height: fit-content; max-height: 320px; padding: 0; margin: 0";
        } else {
            workersDiv.style.cssText = "width: 49%; height: fit-content; max-height: 320px; overflow-y: scroll; padding: 0; margin: 0";
        }
    }

    async getSelectedWorkers() {
        const result = await api.getOperationWorkers(this.id);
        this.selectedWorkersIds.clear();
        if (result.code === 0) {
            this.selectedWorkers = result.data;
            for (let worker of this.selectedWorkers) {
                this.selectedWorkersIds.add(worker.id);
            }
        }
        else {
            this.selectedWorkers = [];
            viewUtils.showError(result.message);
        }
    }

    showSelectedWorker(worker, button) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left";
        button.dataset.workerId = worker.id;
        button.dataset.positionCode = worker.positionCode;
        button.append("[" + worker.positionShortName + "] ");
        const workerFullNameB = document.createElement("b");
        workerFullNameB.textContent = worker.fullName;
        button.append(workerFullNameB);
        button.onclick = () => {
            this.onSelectedWorkerClick(button);
        }
    }

    showSelectedWorkers() {
        const selectedWorkersDiv = document.getElementById("selected-workers-div");
        selectedWorkersDiv.replaceChildren();
        for (let operationWorker of this.selectedWorkers) {
            const operationWorkerButton = document.createElement("button");
            this.showSelectedWorker(operationWorker, operationWorkerButton);
            selectedWorkersDiv.append(operationWorkerButton);
        }
    }
    // #endregion

    showOperationsPage() {
        let operationsPageUrl = sessionStorage.getItem("OPERATIONS_PAGE_URL");
        if (operationsPageUrl == null || operationsPageUrl == "")
            operationsPageUrl = "/OperationsPage2"
        location.href = operationsPageUrl;
    }

    closeOperation() {
        this.showOperationsPage();
    }

    showWorkersDivs() {
        const workersDiv = document.getElementById("selected-workers-div");
        if (workersDiv.childElementCount < 8) {
            workersDiv.style.cssText = "width: 49%; height: fit-content; max-height: 320px; padding: 0; margin: 0";
        } else {
            workersDiv.style.cssText = "width: 49%; height: fit-content; max-height: 320px; overflow-y: scroll; padding: 0; margin: 0";
        }
    }

    async showManager() {
        const managerInput = document.getElementById("manager-input");
        managerInput.value = "<Не определено>"
        const partnersSelect = document.getElementById("partners-select");
        const partnerId = partnersSelect.value;
        if (partnerId === "")
            return;

        const result = await api.getPartnerManager(partnerId);
        if (result.code === 0) {
            managerInput.value = result.manager.FullName;
        } else {
            viewUtils.showError(result.message);
        }
    }

    async show() {
        const id = utils.getUrlParameter("id");
        let operation = null;
        if (id != "") {
            const result = await api.getOperation(Number(id));
            if (result.code === 0) {
                operation = result.operation;
            } else {
                viewUtils.showError(result.message);
            }
        }

        // Id
        this.id = operation != null ? operation.id : -1;

        // State
        this.stateCode = operation.stateCode;
        this.stateName = operation.stateName;

        // Header
        const h1 = document.getElementById("header");
        h1.textContent = "#" + this.id + " (" + this.stateName + ")";

        // Operation type
        const operationTypesSelect = document.getElementById("operation-types-select");
        operationTypesSelect.value = operation.typeCode;
        this.kind = this.getKind();

        // Vehicle kind
        const vehiclekindsSelect = document.getElementById("vehicle-kinds-select");
        vehiclekindsSelect.value = operation.vehicleKindCode;

        // Work start and finish (plan and fact)        
        this.planWorkStartDate = new Date(operation.planWorkStartDate);
        this.factWorkStartDate = new Date(operation.factWorkStartDate);
        this.planWorkFinishDate = new Date(operation.planWorkFinishDate);
        this.factWorkFinishDate = new Date(operation.factWorkFinishDate);
        this.showWorkStartFinish();

        // Duration
        this.planDuration = Number(operation.planDuration);
        this.factDuration = Number(operation.factDuration);
        this.showDuration();        

        // Start and finish
        this.startDate = new Date(operation.startDate);
        this.finishDate = new Date(operation.finishDate);
        this.showStartFinish();

        // Labor cost
        this.planLaborCost = Number(operation.planLaborCost);
        this.planLinearLaborCost = Number(operation.planLinearLaborCost);
        this.factLaborCost = Number(operation.factLaborCost);
        await this.showLaborCost();

        // Warehouse actions (and partner)
        const warehouseActionsB = document.getElementById("warehouse-actions-b");
        if (this.kind === "O") {
            warehouseActionsB.textContent = "Заказы:";
        } else {
            warehouseActionsB.textContent = "Приходы:";
        }

        await this.getSelectedWarehouseActions();
        this.showSelectedWarehouseActions();

        // Manager        
        await this.showManager();

        // Responsible officer
        document.getElementById("responsible-officer-input").value = operation.responsibleOfficer != "" ? operation.responsibleOfficer : "<Не определено>";

        // Gates
        const gatesSelect = document.getElementById("gates-select");
        if (gatesSelect != undefined) {
            gatesSelect.value = operation.gateCode;            
        }

        // Additional attributes
        document.getElementById("vehicle-brand-input").value = operation.vehicleBrand;
        document.getElementById("vehicle-model-input").value = operation.vehicleModel;
        document.getElementById("vehicle-number-input").value = operation.vehicleNumber;
        document.getElementById("trailer-number-input").value = operation.trailerNumber;
        document.getElementById("driver-name-input").value = operation.driverName;
        document.getElementById("driver-phone-number-input").value = operation.driverPhoneNumber;
        document.getElementById("partner-manager-name-input").value = operation.partnerManagerName;
        document.getElementById("partner-manager-phone-number-input").value = operation.partnerManagerPhoneNumber;
        document.getElementById("partner-manager-email-input").value = operation.partnerManagerEmail;
        document.getElementById("note-textarea").value = operation.note;

        // Workers
        const workersDiv = document.getElementById("workers-div");
        if (workersDiv != null && workersDiv != undefined) {
            const linearWorkersDiv = document.getElementById("linear-workers-div");
            linearWorkersDiv.remove();
            await this.getSelectedWorkers();
            this.showSelectedWorkers();
            const workersFieldset = document.getElementById("workers-fieldset");                                                
            workersFieldset.disabled = true;
            this.showWorkersDivs();
        } else {
            workersDiv.classList.add("d-none");
        }

        const operationTypeNoteButton = document.getElementById("operation-type-note-button");
        operationTypeNoteButton.remove();
        const operationTypeNoteDiv = document.getElementById("operation-type-note-div");
        operationTypeNoteDiv.remove();

        const workersFilterDiv = document.getElementById("workers-filter-div");
        workersFilterDiv.remove();
        this.showWorkersDivs();

        const closeButton = document.getElementById("close-button");
        closeButton.onclick = this.closeOperation.bind(this);

        window.onafterprint = window.close;
        window.print();        
    }
}

const operationPrintPage = new OperationPrintPage();

document.addEventListener('DOMContentLoaded', operationPrintPage.show.bind(operationPrintPage));