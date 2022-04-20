class FreeWorkersPage {
    shortUrl;

    workers = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    clearFreeWorkers() {
        const tbody = document.getElementById("sites-workers-tbody");
        tbody.replaceChildren();
    }

    showWorker(worker) {
        const tr = document.createElement("tr");

        const workerTd = document.createElement("td");
        workerTd.classList.add("table-cell");
        workerTd.textContent = worker.surname + " " + worker.firstName;
        tr.appendChild(workerTd);

        const tbody = document.getElementById("sites-workers-tbody");
        tbody.appendChild(tr);
    }

    showFreeWorkers() {
        for (let siteWorker of this.workers) {
            this.showWorker(siteWorker);
        }
    }

    async getFreeWorkers() {
        this.workers = await api.getFreeWorkers();
    }

    async show() {
        this.clearFreeWorkers();
        await this.getFreeWorkers();
        this.showFreeWorkers();
    }
}

const freeWorkersPage = new FreeWorkersPage();

$(document).ready(freeWorkersPage.show.bind(freeWorkersPage));