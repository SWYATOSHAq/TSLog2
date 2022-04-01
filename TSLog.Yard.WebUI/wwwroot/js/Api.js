class Api {
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
            window.location.reload();
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
            window.location.reload();
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
            console.log(operation);
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
            console.log(operation);
            console.log("Removed.");
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
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
            window.location.reload();
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
            window.location.href = "/OperationsPage";
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
            window.location.href = "/OperationsPage";
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
        }
    }
}

const api = new Api();