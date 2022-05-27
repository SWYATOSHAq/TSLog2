/*
    Функционал для доступа к другим программным интерфейсам по http протоколу.
    Автор - Буров Михаил.
*/

// Класс для доступа к внешним программным интерфейсам по протоколу http
class Api {
    async getPlanDuration(operationTypeCode, vehicleKindCode) {
        let response = await fetch("./api/v1/yard/OperationDuration" +
            "?operation_type=" + operationTypeCode +
            "&vehicle_kind=" + vehicleKindCode);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("planDuration", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getPlanLaborCost(operationTypeCode, vehicleKindCode) {
        let response = await fetch("./api/v1/yard/OperationLaborCost?" +
            "operation_type=" + operationTypeCode +
            "&vehicle_kind=" + vehicleKindCode);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getAvalibleIntervals(date, operationTypeCode, vehicleKindCode) {
        let response = await fetch("./api/v1/yard/AvalibleIntervals?" +
            "date=" + date +
            "&operation_type=" + operationTypeCode +
            "&vehicle_kind=" + vehicleKindCode);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("avalibleIntervals", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getPartnerManager(partnerId) {
        let response = await fetch("./api/v1/yard/PartnerManager" +
            "?partner_id=" + partnerId);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("manager", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getOperation(id) {
        let response = await fetch("./api/v1/yard/Operation?id=" + id);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getOperations(parameters) {
        let url = "./api/v1/yard/Operations" + parameters;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operations", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getActiveOperations() {
        let url = "./api/v1/yard/ActiveOperations";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operations", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getGuardOperations() {
        let url = "./api/v1/yard/GuardOperations";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operations", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async createOperation(data) {
        const url = "./api/v1/yard/CreateOperation";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async createOperationByPartner(data) {
        const url = "./api/v1/yard/CreateOperationByPartner";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async editOperation(data) {
        const url = "./api/v1/yard/EditOperation";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async editOperationByPartner(data) {
        const url = "./api/v1/yard/EditOperationByPartner";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async editOperationNote(id, note) {
        const data = {
            id: id,
            note: note
        };
        const url = "./api/v1/yard/SetOperationNote";
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async setOperationGate(id, gateCode) {
        const url = "./api/v1/yard/SetOperationGate";
        const data = {
            id: id
        }
        if (gateCode != "")
            data.gateCode = gateCode;
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };

        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async startOperation(id) {
        const url = "./api/v1/yard/StartOperation";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getPositionWorkers(positionCode, pattern) {
        const url = "./api/v1/yard/PositionWorkers?" +
            "name_pattern=" + pattern +
            "&position_code=" + positionCode;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getLinearWorkers() {
        const url = "./api/v1/yard/LinearWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getLinearFreeWorkers() {
        const url = "./api/v1/yard/LinearFreeWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getAllWorkers() {
        const url = "./api/v1/yard/AllWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getOperationWorkers(operationId) {
        const url = "./api/v1/yard/OperationWorkers?operation_id=" + operationId;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async addOperationWorker(operationId, workerId, positionCode) {
        const url = "./api/v1/yard/AddOperationWorker";
        const data = {
            operationId: operationId,
            workerId: workerId,
            positionCode: positionCode,
        };
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async removeOperationWorker(operationId, workerId, positionCode) {
        const url = "./api/v1/yard/RemoveOperationWorker";
        const data = {
            operationId: operationId,
            workerId: workerId,
            positionCode: positionCode,
        };
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async allocateOperationWorkers(id) {
        const url = "./api/v1/yard/AllocateOperationEmployees";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async startOperationWork(id) {
        const url = "./api/v1/yard/StartOperationWork";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async finishOperationWork(id) {
        const url = "./api/v1/yard/FinishOperationWork";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async finishOperation(id) {
        const url = "./api/v1/yard/FinishOperation";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async cancelOperation(id) {
        const url = "./api/v1/yard/CancelOperation";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async deleteOperation(id) {
        const url = "./api/v1/yard/DeleteOperation";
        const data = {
            id: id
        }
        const options = {
            method: "DELETE",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async createLaborReport(fromDate, toDate) {
        let url = "./api/v1/yard/LaborReport?from_date=" + fromDate + "&to_date=" + toDate;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async createShiftLaborReport2(date, shift) {
        let url = "./api/v1/yard/ShiftLaborReport3?date=" + date + "&shift=" + shift;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async createShiftLaborReport3(date, shift) {
        let url = "./api/v1/yard/ShiftLaborReport3?date=" + date + "&shift=" + shift;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }
}

const api = new Api();