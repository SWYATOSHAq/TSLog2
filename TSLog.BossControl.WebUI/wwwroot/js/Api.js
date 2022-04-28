class Api {

    async enterSite(workerId, siteId, partnerId, note) {
        const url = "./api/v1/BossControl/EnterSite";
        const data = {};
        data.workerId = workerId;
        data.siteId = siteId;
        data.partnerId = partnerId;
        data.note = note;
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ workerId: workerId, })
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            const worker = await response.json();
            return worker;
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
        }
    }

    async leaveSite(workerId) {
        const url = "./api/v1/BossControl/LeaveSite";
        const data = {};
        data.workerId = workerId;
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            const worker = await response.json();
            return worker;
        }
        else {
            const message = await response.text();
            console.log(message);
            alert(message);
        }
    }

    async getBusyWorkers() {
        const url = "./api/v1/BossControl/BusyWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const sitesWorkers = await response.json();
            return sitesWorkers;
        }
        else {
            const message = await response.text();
            console.log(message);
            return [];
        }
    }

    async getFreeWorkers() {
        const url = "./api/v1/BossControl/FreeWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const workers = await response.json();
            return workers;
        }
        else {
            const message = await response.text();
            console.log(message);
            return [];
        }
    }

    async getFreeShiftWorkers() {
        const url = "./api/v1/BossControl/FreeShiftWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            const sitesWorkers = await response.json();
            return sitesWorkers;
        }
        else {
            const message = await response.text();
            console.log(message);
            return [];
        }
    }
}

const api = new Api();