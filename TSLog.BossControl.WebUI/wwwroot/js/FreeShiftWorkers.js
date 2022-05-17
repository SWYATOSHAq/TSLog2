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

    createEnter(workerId) {
        return async function () {
            alert("Не реализовано!");
        }
    }

    showSiteWorker(siteWorker) {
        const tr = document.createElement("tr");

        viewUtils.createCell(tr, ["table-cell"], [siteWorker.worker.surname + " " + siteWorker.worker.firstName]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.site.name]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.partner.name]);

        const startDateTdBold = document.createElement("b");
        startDateTdBold.textContent = siteWorker.startTime;
        viewUtils.createCell(tr, ["table-cell"], [startDateTdBold, " (" + siteWorker.startDay + ")"]);

        const enterButton = document.createElement("button");
        enterButton.classList.add("btn");
        enterButton.classList.add("btn-outline-info");
        enterButton.classList.add("m-3");
        enterButton.classList.add("table-cell-button");
        enterButton.style.cssText = "width: 160px";
        enterButton.textContent = "Назначить на уч.";
        enterButton.onclick = this.createEnter(siteWorker.worker.id);
        viewUtils.createCell(tr, ["table-cell"], [enterButton]);

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