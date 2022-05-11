class Api {
    async getOperation(id) {
        let response = await fetch("./api/v1/yard/Operation?id=" + id);
        if (response.status >= 200 && response.status < 300) {
            return await response.json();            
        }
        else {            
            const text = await response.text();
            console.log(text);
            return null;
        }
    }

    async getOperations(parameters) {
        let url = "./api/v1/yard/Operations" + parameters;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return await response.json();
        }
        else {            
            const message = await response.text();
            console.log(message);
            return [];
        }
    }

    async getActiveOperations() {
        let url = "./api/v1/yard/ActiveOperations";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const operations = await response.json();            
            return operations;
        }
        else {
            const message = await response.text();
            console.log(message);
            return [];
        }
    }

    async getGuardOperations() {
        let url = "./api/v1/yard/GuardOperations";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const operations = await response.json();
            return operations;
        }
        else {
            const message = await response.text();
            console.log(message);
            return [];
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
            const operation = await response.json();
            console.log(operation);
            location.href = location.href + "?id=" + operation.Id;
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
            location.href = location.href + "?id=" + operation.Id;
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
            location.reload();
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
            location.reload();
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
        }
    }

    async editOperationNote(data) {
        const url = "./api/v1/yard/SetOperationNote";
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            const operation = await response.json();
            console.log(operation);
            location.reload();
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
            window.location.reload();
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
            window.location.reload();
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
        }
    }

    async getOperationWorkers(operationId) {
        const url = "./api/v1/yard/OperationWorkers?operation_id=" + operationId;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return {
                code: 0,
                message: "Ok",
                data: await response.json()
            };
        }
        else {
            const message = await response.text();
            return {
                code: 9999,
                message: message
            };
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
            const operation = await response.json();
            console.log("Added.");
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log("Removed.");
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            return {
                code: 0,
                message: "Ok"
            };
        }
        else {
            const message = await response.text();
            return {
                code: 9999,
                message: message
            };
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
            const operation = await response.json();
            console.log(operation);
            window.location.reload();
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
            window.location.reload();
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            const operation = await response.json();
            console.log(operation);            
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
        }
    }

    async createLaborReport(fromDate, toDate) {
        let url = "./api/v1/yard/LaborReport?from_date=" + fromDate + "&to_date=" + toDate;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const data = await response.json();
            return data;
        }
        else {
            const message = await response.text();
            console.log(message);
            return [];
        }
    }

    async createShiftLaborReport(date, shift) {
        let url = "./api/v1/yard/ShiftLaborReport?date=" + date + "&shift=" + shift;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const data = await response.json();
            return data;
        }
        else {
            const message = await response.text();
            console.log(message);
            return [];
        }
    }
}

const api = new Api();