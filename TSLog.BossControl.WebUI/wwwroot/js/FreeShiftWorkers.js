class FreeShiftWorkersPage {
    shortUrl;

    workers = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    clearFreeShiftWorkers() {
        const tbody = document.getElementById("sites-workers-tbody");
        tbody.replaceChildren();
    }

    showSiteWorker(siteWorker) {
        const tr = document.createElement("tr");

        const workerTd = document.createElement("td");
        workerTd.classList.add("table-cell");
        workerTd.textContent = siteWorker.worker.surname + " " + siteWorker.worker.firstName;
        tr.appendChild(workerTd);

        const siteTd = document.createElement("td");
        siteTd.classList.add("table-cell");
        siteTd.textContent = siteWorker.site.name;
        tr.appendChild(siteTd);

        const partnerTd = document.createElement("td");
        partnerTd.classList.add("table-cell");
        partnerTd.textContent = siteWorker.partner.name;
        tr.appendChild(partnerTd);

        const startDateTd = document.createElement("td");
        startDateTd.classList.add("table-cell");
        const startDateTdBold = document.createElement("b");
        startDateTdBold.textContent = siteWorker.startTime;
        startDateTd.appendChild(startDateTdBold);
        const text = document.createTextNode(" (" + siteWorker.startDay + ")");
        startDateTd.appendChild(text);
        tr.appendChild(startDateTd);

        const tbody = document.getElementById("sites-workers-tbody");
        tbody.appendChild(tr);
    }

    showFreeShiftWorkers() {
        for (let siteWorker of this.workers) {
            this.showSiteWorker(siteWorker);
        }
    }

    async getFreehiftWorkers() {
        this.workers = await api.getFreeShiftWorkers();
    }

    async show() {
        this.clearFreeShiftWorkers();
        await this.getFreehiftWorkers();
        this.showFreeShiftWorkers();
    }
}

const freeShiftWorkersPage = new FreeShiftWorkersPage();

$(document).ready(freeShiftWorkersPage.show.bind(freeShiftWorkersPage));