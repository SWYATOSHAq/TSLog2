class Api {
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